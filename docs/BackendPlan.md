# Backend Development Plan

## UserManagement Microservice

### Overview

The UserManagement microservice will handle all user-related operations including authentication, profile management, and user preferences. This service will be the foundation for user interactions across the platform.

### Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL 16
- **Authentication**: ASP.NET Identity + JWT
- **ORM**: Entity Framework Core
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Serilog
- **Caching**: Redis
- **Message Queue**: RabbitMQ (for async operations)

### Project Structure

```
server/
├── UserManagement/
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── Data/
│   ├── Middleware/
│   ├── Configurations/
│   └── Tests/
```

### API Endpoints

#### Authentication

```
POST /api/auth/register
- Register new user
- Request Body: { email, password, firstName, lastName, phoneNumber }
- Response: { userId, token }

POST /api/auth/login
- User login
- Request Body: { email, password }
- Response: { token, refreshToken, userProfile }

POST /api/auth/refresh-token
- Refresh JWT token
- Request Body: { refreshToken }
- Response: { token, refreshToken }

POST /api/auth/forgot-password
- Initiate password reset
- Request Body: { email }
- Response: { message }

POST /api/auth/reset-password
- Reset password with token
- Request Body: { token, newPassword }
- Response: { message }

POST /api/auth/verify-email
- Verify email address
- Request Body: { token }
- Response: { message }
```

#### User Profile Management

```
GET /api/users/profile
- Get current user profile
- Response: { userProfile }

PUT /api/users/profile
- Update user profile
- Request Body: { firstName, lastName, phoneNumber, profilePicture }
- Response: { userProfile }

GET /api/users/{userId}
- Get user by ID (admin only)
- Response: { userProfile }

PUT /api/users/{userId}/status
- Update user status (admin only)
- Request Body: { status }
- Response: { message }

GET /api/users/preferences
- Get user preferences
- Response: { preferences }

PUT /api/users/preferences
- Update user preferences
- Request Body: { preferences }
- Response: { preferences }
```

#### User Roles and Permissions

```
GET /api/users/roles
- Get all roles (admin only)
- Response: { roles }

POST /api/users/roles
- Create new role (admin only)
- Request Body: { name, permissions }
- Response: { role }

PUT /api/users/{userId}/roles
- Assign roles to user (admin only)
- Request Body: { roles }
- Response: { message }
```

#### Additional Required Endpoints

```
POST /api/users/verify-phone
- Verify phone number
- Request Body: { phoneNumber, code }
- Response: { message }

POST /api/users/request-phone-verification
- Request phone verification code
- Request Body: { phoneNumber }
- Response: { message }

GET /api/users/notifications
- Get user notifications
- Response: { notifications }

PUT /api/users/notifications/{notificationId}
- Mark notification as read
- Response: { message }

DELETE /api/users/notifications/{notificationId}
- Delete notification
- Response: { message }

POST /api/users/feedback
- Submit user feedback
- Request Body: { subject, message, type }
- Response: { message }
```

### Data Models

#### Base Entity

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedBy { get; set; }
    public bool IsActive { get; set; } = true;
}
```

#### User Detail Model

```csharp
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
```

#### User Preferences (Enhanced)

```csharp
public class UserPreferences : BaseEntity
{
    public int UserId { get; set; } // This will be linked to Identity User Id

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
    public virtual UserDetail UserDetail { get; set; }
}
```

#### User Document

```csharp
public class UserDocument : BaseEntity
{
    public int UserId { get; set; } // This will be linked to Identity User Id
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
    public virtual UserDetail UserDetail { get; set; }
}
```

### Security Considerations

1. Implement rate limiting for authentication endpoints
2. Use secure password hashing (Argon2)
3. Implement JWT token rotation
4. Add IP-based security measures
5. Implement audit logging for sensitive operations
6. Use HTTPS for all endpoints
7. Implement CORS policies
8. Add request validation and sanitization

### Performance Optimization

1. Implement caching for user profiles
2. Use database indexing for frequently queried fields
3. Implement connection pooling
4. Use async/await for all I/O operations
5. Implement response compression
6. Use pagination for list endpoints
7. Implement database query optimization

### Monitoring and Logging

1. Implement structured logging
2. Add performance metrics collection
3. Set up error tracking
4. Implement health checks
5. Add request tracing
6. Monitor authentication attempts
7. Track user activity patterns

### Testing Strategy

1. Unit tests for all services
2. Integration tests for API endpoints
3. Performance tests for critical paths
4. Security testing
5. Load testing
6. Database migration tests
7. Authentication flow tests

### Deployment Considerations

1. Containerization with Docker
2. Kubernetes deployment
3. Environment-specific configurations
4. Database migration strategy
5. Backup and recovery procedures
6. Monitoring and alerting setup
7. CI/CD pipeline configuration

### Future Enhancements

1. Social login integration
2. Two-factor authentication
3. User activity analytics
4. Advanced role-based access control
5. User segmentation
6. Integration with third-party services
7. Enhanced security features
