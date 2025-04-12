using System;
using System.ComponentModel.DataAnnotations;

namespace RentingPlatform.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty;

        public bool IsHost { get; set; } = false;

        public bool IsAdmin { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}