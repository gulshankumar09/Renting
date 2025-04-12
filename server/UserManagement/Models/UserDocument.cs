using System;

namespace UserManagement.Models
{
    public class UserDocument : BaseEntity
    {
        public string UserProfileId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Navigation property
        public virtual UserProfile UserProfile { get; set; }
    }
}