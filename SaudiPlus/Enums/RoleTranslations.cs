namespace SaudiPlus.Enums
{
    public enum UserRole
    {
        Admin,
        Business,
        User,
        SubAdmin
    }

    public static class RoleTranslations
    {
        public static readonly Dictionary<UserRole, (string En, string Ar)> RoleNames = new()
        {
            { UserRole.Admin, ("Admin", "الادمن") },
            { UserRole.Business, ("Business", "الأعمال") },
            { UserRole.User, ("User", "مستخدم") },
            { UserRole.SubAdmin, ("SubAdmin", "مشرف فرعي") }
        };
    }
}
