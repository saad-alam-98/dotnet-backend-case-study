using SaudiPlus.Enums;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Entities;

public partial class Invoice : Entity<int>
{
    public Guid UserId { get; set; }
    public Guid BusinessId { get; set; }
    public int BranchId { get; set; }
    public int OfferId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public double Amount { get; set; }
    public float Discount { get; set; }
    public bool IsVerified { get; set; }
    public double TotalAmount { get; set; }
    public DateTime IssueDate { get; set; }
    public virtual BusinessUser Business { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual IndividualUser User { get; set; } = null!;
    public virtual Offer Offer { get; set; } = null!;
}
