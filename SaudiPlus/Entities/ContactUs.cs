using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class ContactUs : Entity<int>
{
    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string WhatsApp { get; set; } = null!;

    public string? FacebookUrl { get; set; }
    public string? FacebookPage { get; set; }
    public string? InstagramUrl { get; set; }
    public string? InstagramPage { get; set; }
    public string? SnapchatUrl { get; set; }
    public string? SnapchatPage { get; set; }
    public string? TwitterUrl { get; set; }
    public string? TwitterPage { get; set; }
}
