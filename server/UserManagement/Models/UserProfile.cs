using System;
using System.Collections.Generic;

namespace UserManagement.Models
{
    public class UserProfile : BaseEntity
    {
        public new string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsPhoneVerified { get; set; }
        public string ProfilePicture { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string PreferredLanguage { get; set; } = string.Empty;

        // Foreign key to ApplicationUser
        public string ApplicationUserId { get; set; } = string.Empty;
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;

        // Navigation properties
        public virtual UserPreferences? Preferences { get; set; }
        public virtual ICollection<UserDocument> Documents { get; set; } = new List<UserDocument>();
    }
}