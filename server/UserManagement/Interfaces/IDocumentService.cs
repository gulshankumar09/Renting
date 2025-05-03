using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using UserManagement.DTOs;
using UserManagement.Models.Entities;

namespace UserManagement.Interfaces;

public interface IDocumentService
{
    Task<DocumentResponseDto> UploadDocumentAsync(string userProfileId, DocumentUploadDto documentDto, IFormFile file);
    Task<DocumentResponseDto> GetDocumentByIdAsync(string documentId);
    Task<IEnumerable<DocumentResponseDto>> GetUserDocumentsAsync(string userProfileId);
    Task<DocumentResponseDto> UpdateDocumentAsync(string documentId, DocumentUpdateDto documentDto);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task<Stream> GetDocumentFileAsync(string documentId);
    Task<bool> VerifyDocumentAsync(string documentId, DocumentVerificationDto verificationDto);
}