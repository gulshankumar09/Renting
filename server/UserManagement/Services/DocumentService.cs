using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.DTOs;
using UserManagement.Interfaces;
using UserManagement.Models.Entities;

namespace UserManagement.Services;

public class DocumentService : IDocumentService
{
    private readonly UserManagementDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _basePath;
    private readonly string[] _allowedExtensions;
    private readonly long _maxFileSize;

    public DocumentService(
        UserManagementDbContext context,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;

        // Get configuration for file storage
        _basePath = _configuration["FileStorage:BasePath"] ?? "UserFiles";
        _allowedExtensions = (_configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ??
                            new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" })
                            .Select(ext => ext.ToLowerInvariant()).ToArray();
        _maxFileSize = _configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 10 * 1024 * 1024); // Default 10MB

        // Ensure directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<DocumentResponseDto> UploadDocumentAsync(string userProfileId, DocumentUploadDto documentDto, IFormFile file)
    {
        // Validate user profile exists
        var userProfile = await _context.UserProfiles.FindAsync(userProfileId)
            ?? throw new KeyNotFoundException($"User profile with ID {userProfileId} not found.");

        // Validate file
        ValidateFile(file);

        // Generate a unique file name to prevent collisions
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{fileExtension}";

        // Create user-specific directory if it doesn't exist
        var userDirectoryPath = Path.Combine(_basePath, userProfileId);
        if (!Directory.Exists(userDirectoryPath))
        {
            Directory.CreateDirectory(userDirectoryPath);
        }

        // Full path to store the file
        var filePath = Path.Combine(userDirectoryPath, fileName);

        // Create new document entity
        var document = new UserDocument
        {
            Id = Guid.NewGuid().ToString(),
            UserProfileId = userProfileId,
            DocumentType = documentDto.DocumentType,
            DocumentNumber = documentDto.DocumentNumber,
            FileName = fileName,
            FilePath = filePath,
            FileType = fileExtension.TrimStart('.'),
            FileSize = file.Length,
            ExpiryDate = documentDto.ExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save document to database
        _context.UserDocuments.Add(document);
        await _context.SaveChangesAsync();

        // Return response
        return MapToDocumentResponseDto(document);
    }

    public async Task<DocumentResponseDto> GetDocumentByIdAsync(string documentId)
    {
        var document = await _context.UserDocuments.FindAsync(documentId)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found.");

        return MapToDocumentResponseDto(document);
    }

    public async Task<IEnumerable<DocumentResponseDto>> GetUserDocumentsAsync(string userProfileId)
    {
        var documents = await _context.UserDocuments
            .Where(d => d.UserProfileId == userProfileId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDocumentResponseDto);
    }

    public async Task<DocumentResponseDto> UpdateDocumentAsync(string documentId, DocumentUpdateDto documentDto)
    {
        var document = await _context.UserDocuments.FindAsync(documentId)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found.");

        // Update properties if provided
        if (!string.IsNullOrEmpty(documentDto.DocumentType))
        {
            document.DocumentType = documentDto.DocumentType;
        }

        if (!string.IsNullOrEmpty(documentDto.DocumentNumber))
        {
            document.DocumentNumber = documentDto.DocumentNumber;
        }

        if (documentDto.ExpiryDate.HasValue)
        {
            document.ExpiryDate = documentDto.ExpiryDate;
        }

        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDocumentResponseDto(document);
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        var document = await _context.UserDocuments.FindAsync(documentId)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found.");

        // Try to delete the physical file
        try
        {
            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }
        }
        catch (Exception ex)
        {
            // Log the exception but continue - we still want to delete the DB record
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }

        // Remove from database
        _context.UserDocuments.Remove(document);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Stream> GetDocumentFileAsync(string documentId)
    {
        var document = await _context.UserDocuments.FindAsync(documentId)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found.");

        if (!File.Exists(document.FilePath))
        {
            throw new FileNotFoundException($"File for document {documentId} not found on disk.");
        }

        return new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
    }

    public async Task<bool> VerifyDocumentAsync(string documentId, DocumentVerificationDto verificationDto)
    {
        var document = await _context.UserDocuments.FindAsync(documentId)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found.");

        document.IsVerified = verificationDto.IsVerified;
        document.VerifiedAt = verificationDto.IsVerified ? DateTime.UtcNow : null;
        document.VerifiedBy = verificationDto.VerifiedBy;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or not provided.");
        }

        if (file.Length > _maxFileSize)
        {
            throw new ArgumentException($"File size exceeds the maximum allowed size of {_maxFileSize / (1024 * 1024)} MB.");
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }
    }

    private DocumentResponseDto MapToDocumentResponseDto(UserDocument document)
    {
        return new DocumentResponseDto
        {
            Id = document.Id,
            UserProfileId = document.UserProfileId,
            DocumentType = document.DocumentType,
            DocumentNumber = document.DocumentNumber,
            FileName = document.FileName,
            FileType = document.FileType,
            FileSize = document.FileSize,
            IsVerified = document.IsVerified,
            VerifiedAt = document.VerifiedAt,
            VerifiedBy = document.VerifiedBy,
            ExpiryDate = document.ExpiryDate,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}