using System;
using System.Collections.Generic;

namespace SaudiPlus.Entities;

public partial class Representative
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Position { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }

    public bool IsDelete { get; set; }

    public Guid? CreatorId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public Guid? LastModifierId { get; set; }
}
