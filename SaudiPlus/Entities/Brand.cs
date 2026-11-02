using System;
using System.Collections.Generic;

namespace SaudiPlus.Entities;

public partial class Brand
{
    public int Id { get; set; }

    public string AdvertisedBrandName { get; set; } = null!;

    public int ContractId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }

    public bool IsDelete { get; set; }

    public Guid? CreatorId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public Guid? LastModifierId { get; set; }

    public virtual Contract Contract { get; set; } = null!;
}
