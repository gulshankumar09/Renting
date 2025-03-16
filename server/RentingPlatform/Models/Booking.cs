using System;
using System.ComponentModel.DataAnnotations;

namespace RentingPlatform.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Canceled,
        Completed
    }

    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        public int GuestCount { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public string PaymentId { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}