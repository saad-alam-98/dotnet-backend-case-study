using SaudiPlus.Entities.Identity;
using System;
using System.Collections.Generic;

namespace SaudiPlus.Entities;

public partial class RefreshToken
{
    public Guid UserId { get; set; }

    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? RevokedOn { get; set; }

    public virtual User User { get; set; } = null!;
}
