namespace SaudiPlus.Helper.Application
{
    public class AdminCredential
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
    }

    public class AdminCredentials
    {
        public List<AdminCredential> Admins { get; set; } = new();
    }
}
