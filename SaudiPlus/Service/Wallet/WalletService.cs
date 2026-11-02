using AutoMapper;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Wallet;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;
using Newtonsoft.Json.Linq;

namespace SaudiPlus.Service.Wallet
{
    public class WalletService : BaseService<WalletService>, IWalletService
    {
        private IInvoiceRepository _invoiceRepository;
        private IBusinessRepository _businessRepository;
        private IContractRepository _contractRepository;

        public WalletService(
           IInvoiceRepository invoiceRepository,
           IBusinessRepository businessRepository,
           IContractRepository contractRepository,
           ILanguageService languageService,
           ILogger<WalletService> logger,
           IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _invoiceRepository = invoiceRepository;
            _businessRepository = businessRepository;
            _contractRepository = contractRepository;
        }

        public async Task<GetBusinessWalletResponse> GetBusinessWallet(Guid businessId, int year, int month, EntityFilter filter)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new GetBusinessWalletResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            try
            {
                var businessUser = await _businessRepository.GetByIdAsync(businessId);
                if (businessUser == null)
                {
                    return new GetBusinessWalletResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid Business",
                        payload: null
                    );
                }

                var contract = await _contractRepository.GetBusinessContract(businessId);
                if (contract == null)
                {
                    return new GetBusinessWalletResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "This business does not have any contract",
                        payload: null
                    );
                }

                var invoices = await (_invoiceRepository.GetInvoiceForFilter(businessId)
                        .Where(invoice =>
                            invoice.CreationTime.Year == year &&
                            invoice.CreationTime.Month == month
                        )
                    ).ToListAsync();

                var totalSales = invoices.Sum(i => i.TotalAmount);
                List<StoreSalesItem> items = [];

                foreach (var i in invoices)
                {
                    var branch = _mapper.Map<BranchSalesItem>(i.Branch);

                    items.Add(new StoreSalesItem
                    {
                        Name = i.Business.GetNameByLanguageCode(languageCode),
                        Branch = branch,
                        Price = i.Amount,
                        Discount = i.Discount,
                        FinalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate
                    });
                }

                var totalItems = items.Count;
                var sales = items
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize).ToList();
                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var Sales = new BusinessWalletSalesItem
                {
                    TotalSales = totalSales,
                    Items = sales,
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage,
                };

                var UnitCost = 1f;
                if (contract.ContractType == Enums.ContractType.MonthlySubscription &&
                    contract.SubscriptionAmount.HasValue
                )
                {
                    UnitCost = contract.SubscriptionAmount.Value;
                }
                else if (contract.ContractType == Enums.ContractType.FixedAmountPerOffer &&
                    contract.FixedAmountPerOffer.HasValue
                )
                {
                    UnitCost = contract.FixedAmountPerOffer.Value;
                }
                else if (contract.ContractType == Enums.ContractType.PercentageOfInvoice &&
                    contract.PercentageOfInvoice.HasValue
                )
                {
                    UnitCost = contract.PercentageOfInvoice.Value;
                }

                var invoiceDetail = new InvoiceDetails
                {
                    Description = contract.ContractType.ToString(),
                    Quantity = items.Count,
                    Sales = totalSales,
                    UnitCost = UnitCost,
                };

                double invoiceAmount = 0;
                if (contract.ContractType == Enums.ContractType.MonthlySubscription && contract.SubscriptionAmount.HasValue)
                {
                    invoiceAmount = contract.SubscriptionAmount.Value;
                }
                else if (contract.ContractType == Enums.ContractType.FixedAmountPerOffer && contract.FixedAmountPerOffer.HasValue)
                {
                    invoiceAmount = invoices.Count * contract.FixedAmountPerOffer.Value;
                }
                else if (contract.ContractType == Enums.ContractType.PercentageOfInvoice && contract.PercentageOfInvoice.HasValue)
                {
                    var percentage = contract.PercentageOfInvoice.Value;
                    invoiceAmount = invoices.Sum(x => (x.TotalAmount / 100) * percentage);
                }

                DateTime date = new(year, month, 1);
                var IssueDate = date.ToString("MMM yyyy");

                var random = new Random();
                int randomNumber = random.Next(10000, 99999);

                var invoice = new BusinessInvoiceItem
                {
                    InvoiceAmount = Math.Round(invoiceAmount, 2),
                    InvoiceNumber = $"INV-{randomNumber}",
                    IssueDate = IssueDate,
                    MobileNumber = businessUser.MobileNumber,
                    NameAr = businessUser.NameAr,
                    NameEn = businessUser.NameEn,
                    Details = invoiceDetail
                };

                var payload = new BusinessWalletPayload
                {
                    Sales = Sales,
                    Invoice = invoice
                };

                return new GetBusinessWalletResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Store Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating reports");
                return new GetBusinessWalletResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
    }
}
