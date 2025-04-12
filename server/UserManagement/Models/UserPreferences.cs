using System.Collections.Generic;

namespace UserManagement.Models
{
    public class UserPreferences : BaseEntity
    {
        public string UserProfileId { get; set; }

        // Communication Preferences
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool SMSNotifications { get; set; }
        public bool MarketingEmails { get; set; }
        public bool NewsletterSubscription { get; set; }

        // Language and Regional Settings
        public string Language { get; set; }
        public string TimeZone { get; set; }
        public string Currency { get; set; }
        public string DateFormat { get; set; }

        // Property Preferences
        public List<string> PreferredPropertyTypes { get; set; }
        public decimal? MinPriceRange { get; set; }
        public decimal? MaxPriceRange { get; set; }
        public List<string> PreferredLocations { get; set; }
        public List<string> PreferredAmenities { get; set; }
        public int? PreferredBedrooms { get; set; }
        public int? PreferredBathrooms { get; set; }

        // Search and Filter Preferences
        public bool ShowVerifiedPropertiesOnly { get; set; }
        public bool ShowInstantBookPropertiesOnly { get; set; }
        public bool ShowSuperhostPropertiesOnly { get; set; }
        public string DefaultSortOrder { get; set; }
        public int DefaultResultsPerPage { get; set; }

        // Privacy Settings
        public bool ShowProfileToPublic { get; set; }
        public bool ShowEmailToPublic { get; set; }
        public bool ShowPhoneToPublic { get; set; }
        public bool ShowReviewsToPublic { get; set; }

        // Notification Preferences
        public bool NotifyOnBookingRequests { get; set; }
        public bool NotifyOnBookingConfirmations { get; set; }
        public bool NotifyOnMessages { get; set; }
        public bool NotifyOnReviews { get; set; }
        public bool NotifyOnPriceChanges { get; set; }
        public bool NotifyOnNewProperties { get; set; }

        // Navigation property
        public virtual UserProfile UserProfile { get; set; }
    }
}