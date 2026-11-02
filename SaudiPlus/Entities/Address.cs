using System;
using System.Collections.Generic;

namespace SaudiPlus.Entities;

public partial class Address
{
    public int Id { get; set; }

    public string Street { get; set; } = null!;

    public string PoBox { get; set; } = null!;

    public string ZipCode { get; set; } = null!;

    public int CityId { get; set; }

    public int CountryId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }

    public bool IsDelete { get; set; }

    public Guid? CreatorId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public Guid? LastModifierId { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual Country Country { get; set; } = null!;
}
