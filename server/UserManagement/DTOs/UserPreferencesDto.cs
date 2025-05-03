using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs;

public class UserPreferencesDto
{
    // UI Preferences
    public bool DarkMode { get; set; }

    // Communication Preferences
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool SMSNotifications { get; set; }
    public bool MarketingEmails { get; set; }
    public bool NewsletterSubscription { get; set; }

    // Language and Regional Settings
    [StringLength(10)]
    public string Language { get; set; } = string.Empty;

    [StringLength(50)]
    public string TimeZone { get; set; } = string.Empty;

    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    [StringLength(20)]
    public string DateFormat { get; set; } = string.Empty;

    // Property Preferences
    public List<string> PreferredPropertyTypes { get; set; } = new();
    public decimal? MinPriceRange { get; set; }
    public decimal? MaxPriceRange { get; set; }
    public List<string> PreferredLocations { get; set; } = new();
    public List<string> PreferredAmenities { get; set; } = new();
    public int? PreferredBedrooms { get; set; }
    public int? PreferredBathrooms { get; set; }

    // Search and Filter Preferences
    public bool ShowVerifiedPropertiesOnly { get; set; }
    public bool ShowInstantBookPropertiesOnly { get; set; }
    public bool ShowSuperhostPropertiesOnly { get; set; }
    public string DefaultSortOrder { get; set; } = string.Empty;
    public int DefaultResultsPerPage { get; set; } = 20;

    // Privacy Settings
    public bool ShowProfileToPublic { get; set; }
    public bool ShowEmailToPublic { get; set; }
    public bool ShowPhoneToPublic { get; set; }
    public bool ShowReviewsToPublic { get; set; }

    // Notification Preferences
    public bool NotifyOnBookingRequests { get; set; } = true;
    public bool NotifyOnBookingConfirmations { get; set; } = true;
    public bool NotifyOnMessages { get; set; } = true;
    public bool NotifyOnReviews { get; set; } = true;
    public bool NotifyOnPriceChanges { get; set; }
    public bool NotifyOnNewProperties { get; set; }
}

public class UpdatePreferencesDto
{
    // UI Preferences
    public bool? DarkMode { get; set; }

    // Communication Preferences
    public bool? EmailNotifications { get; set; }
    public bool? PushNotifications { get; set; }
    public bool? SMSNotifications { get; set; }
    public bool? MarketingEmails { get; set; }
    public bool? NewsletterSubscription { get; set; }

    // Language and Regional Settings
    [StringLength(10)]
    public string? Language { get; set; }

    [StringLength(50)]
    public string? TimeZone { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [StringLength(20)]
    public string? DateFormat { get; set; }

    // Property Preferences
    public List<string>? PreferredPropertyTypes { get; set; }
    public decimal? MinPriceRange { get; set; }
    public decimal? MaxPriceRange { get; set; }
    public List<string>? PreferredLocations { get; set; }
    public List<string>? PreferredAmenities { get; set; }
    public int? PreferredBedrooms { get; set; }
    public int? PreferredBathrooms { get; set; }

    // Search and Filter Preferences
    public bool? ShowVerifiedPropertiesOnly { get; set; }
    public bool? ShowInstantBookPropertiesOnly { get; set; }
    public bool? ShowSuperhostPropertiesOnly { get; set; }
    public string? DefaultSortOrder { get; set; }
    public int? DefaultResultsPerPage { get; set; }

    // Privacy Settings
    public bool? ShowProfileToPublic { get; set; }
    public bool? ShowEmailToPublic { get; set; }
    public bool? ShowPhoneToPublic { get; set; }
    public bool? ShowReviewsToPublic { get; set; }

    // Notification Preferences
    public bool? NotifyOnBookingRequests { get; set; }
    public bool? NotifyOnBookingConfirmations { get; set; }
    public bool? NotifyOnMessages { get; set; }
    public bool? NotifyOnReviews { get; set; }
    public bool? NotifyOnPriceChanges { get; set; }
    public bool? NotifyOnNewProperties { get; set; }
}