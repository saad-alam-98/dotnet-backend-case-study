using System;
using System.Collections.Generic;

namespace SaudiPlus.Entities;

public partial class AppSetting
{
    public int Id { get; set; }

    public int PolicyType { get; set; }

    public string? TermsAndConditionsAr { get; set; }

    public string? TermsAndConditionsEn { get; set; }

    public string? UsersPolicyAr { get; set; }

    public string? UsersPolicyEn { get; set; }

    public string? AboutAppAr { get; set; }

    public string? AboutAppEn { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }

    public bool IsDelete { get; set; }

    public Guid? CreatorId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public Guid? LastModifierId { get; set; }
}
