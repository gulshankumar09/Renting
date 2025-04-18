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

    public enum Role
    {
        Admin,
        User,
        Manager
    }
}