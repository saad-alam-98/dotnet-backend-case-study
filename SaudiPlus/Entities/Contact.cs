using System;
using System.Collections.Generic;

namespace SaudiPlus.Entities;

public partial class Contact
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Landline { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public string NameAr { get; set; } = null!;

    public string PositionAr { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }

    public bool IsDelete { get; set; }

    public Guid? CreatorId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public Guid? LastModifierId { get; set; }
}
