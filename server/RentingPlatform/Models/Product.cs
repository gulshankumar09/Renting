using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentingPlatform.Models
{
    public enum PropertyType
    {
        Hotel,
        House,
        PG
    }

    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public PropertyType Type { get; set; }

        [Required]
        public string Location { get; set; }

        public string Address { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public double Area { get; set; }

        public List<string> Amenities { get; set; } = new List<string>();

        public List<string> ImageUrls { get; set; } = new List<string>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int HostId { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}