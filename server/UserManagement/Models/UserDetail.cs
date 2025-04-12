using System;
using System.Collections.Generic;

namespace UserManagement.Models
{
    public enum Gender
    {
        Male,
        Female,
        Other,
        PreferNotToSay
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        Banned,
        PendingVerification
    }

    public class UserDetail : BaseEntity
    {
        public int UserId { get; set; } // This will be linked to Identity User Id
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPhoneVerified { get; set; }
        public string ProfilePicture { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string Bio { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string PreferredLanguage { get; set; }

        // Navigation properties
        public virtual UserPreferences Preferences { get; set; }
        public virtual ICollection<UserDocument> Documents { get; set; }
    }
} 