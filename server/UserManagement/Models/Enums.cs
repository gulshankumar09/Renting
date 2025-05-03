namespace UserManagement.Models
{
    public enum Gender
    {
        NotSpecified,
        Male,
        Female,
        NonBinary,
        Other
    }

    public enum UserStatus
    {
        Pending,
        Active,
        Suspended,
        Deactivated,
        Locked
    }

    public enum Role
    {
        Admin,
        Owner,
        Manager,
        Tenant,
        Guest
    }

    public enum ActivityType
    {
        Login,
        Logout,
        FailedLogin,
        Registration,
        PasswordReset,
        EmailVerification,
        ProfileUpdate,
        PreferenceUpdate,
        DocumentUpload,
        DocumentDelete,
        AccountLocked,
        AccountUnlocked,
        RoleAssigned,
        RoleRemoved
    }
}