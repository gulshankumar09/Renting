using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.DTOs;
using UserManagement.Interfaces;

namespace UserManagement.Controllers;

[Authorize]
public class DocumentController : BaseApiController
{
    private readonly IDocumentService _documentService;
    private readonly IUserProfileService _userProfileService;

    public DocumentController(
        IDocumentService documentService,
        IUserProfileService userProfileService)
    {
        _documentService = documentService;
        _userProfileService = userProfileService;
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDto documentDto, IFormFile file)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        // Get current user's profile
        var userProfile = await _userProfileService.GetUserProfileAsync(userId);
        if (userProfile == null)
        {
            return NotFound(new { Error = "User profile not found." });
        }

        // Upload document
        var result = await _documentService.UploadDocumentAsync(userProfile.Id, documentDto, file);
        return CreatedAtAction(nameof(GetDocumentById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocumentById(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        var document = await _documentService.GetDocumentByIdAsync(id);

        // Check if user is authorized to access this document (either owns it or is admin)
        var isAdmin = User.IsInRole("Admin");
        var userProfile = await _userProfileService.GetUserProfileAsync(userId);

        if (!isAdmin && (userProfile == null || document.UserProfileId != userProfile.Id))
        {
            return Forbid();
        }

        return Ok(document);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserDocuments()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        var userProfile = await _userProfileService.GetUserProfileAsync(userId);
        if (userProfile == null)
        {
            return NotFound(new { Error = "User profile not found." });
        }

        var documents = await _documentService.GetUserDocumentsAsync(userProfile.Id);
        return Ok(documents);
    }

    [HttpGet("download/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadDocument(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        try
        {
            // Get document metadata
            var document = await _documentService.GetDocumentByIdAsync(id);

            // Check if user is authorized to access this document (either owns it or is admin)
            var isAdmin = User.IsInRole("Admin");
            var userProfile = await _userProfileService.GetUserProfileAsync(userId);

            if (!isAdmin && (userProfile == null || document.UserProfileId != userProfile.Id))
            {
                return Forbid();
            }

            // Get document file stream
            var fileStream = await _documentService.GetDocumentFileAsync(id);

            // Return file
            return File(fileStream, GetContentType(document.FileType), document.FileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "Document not found." });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { Error = "Document file not found." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DocumentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateDocument(string id, [FromBody] DocumentUpdateDto documentDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        try
        {
            // Get document
            var document = await _documentService.GetDocumentByIdAsync(id);

            // Check if user is authorized to modify this document (either owns it or is admin)
            var isAdmin = User.IsInRole("Admin");
            var userProfile = await _userProfileService.GetUserProfileAsync(userId);

            if (!isAdmin && (userProfile == null || document.UserProfileId != userProfile.Id))
            {
                return Forbid();
            }

            // Update document
            var updatedDocument = await _documentService.UpdateDocumentAsync(id, documentDto);
            return Ok(updatedDocument);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "Document not found." });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDocument(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        try
        {
            // Get document
            var document = await _documentService.GetDocumentByIdAsync(id);

            // Check if user is authorized to delete this document (either owns it or is admin)
            var isAdmin = User.IsInRole("Admin");
            var userProfile = await _userProfileService.GetUserProfileAsync(userId);

            if (!isAdmin && (userProfile == null || document.UserProfileId != userProfile.Id))
            {
                return Forbid();
            }

            // Delete document
            await _documentService.DeleteDocumentAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "Document not found." });
        }
    }

    [HttpPut("{id}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyDocument(string id, [FromBody] DocumentVerificationDto verificationDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Error = "User is not authenticated." });
        }

        try
        {
            // Set verified by if not provided
            if (string.IsNullOrEmpty(verificationDto.VerifiedBy))
            {
                verificationDto.VerifiedBy = userId;
            }

            // Verify document
            await _documentService.VerifyDocumentAsync(id, verificationDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Error = "Document not found." });
        }
    }

    private string GetContentType(string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}