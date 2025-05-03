using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs;

public class DocumentUploadDto
{
    [Required]
    [StringLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [StringLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    public DateTime? ExpiryDate { get; set; }
}

public class DocumentUpdateDto
{
    [StringLength(50)]
    public string? DocumentType { get; set; }

    [StringLength(50)]
    public string? DocumentNumber { get; set; }

    public DateTime? ExpiryDate { get; set; }
}

public class DocumentVerificationDto
{
    [Required]
    public bool IsVerified { get; set; }

    public string? VerifiedBy { get; set; }
}

public class DocumentResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserProfileId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}