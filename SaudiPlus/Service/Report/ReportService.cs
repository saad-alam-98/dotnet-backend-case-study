using AutoMapper;
using SaudiPlus.Enums;
using FirebaseAdmin.Auth;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Report;
using SaudiPlus.Response.Contract;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Service.Report
{
    public class ReportService : BaseService<ReportService>, IReportService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IIndividualUserRepository _individualUserRepository;
        private readonly IUserOfferRedeemRepository _userOfferRedeemRepository;

        public ReportService(
            ILogger<ReportService> logger,
            IOfferRepository offerRepository,
            ILanguageService languageService,
            IBranchRepository branchRepository,
            IInvoiceRepository invoiceRepository,
            IBusinessRepository businessRepository,
            IContractRepository contractRepository,
            IIndividualUserRepository individualUserRepository,
            IUserOfferRedeemRepository userOfferRedeemRepository,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _offerRepository = offerRepository;
            _branchRepository = branchRepository;
            _invoiceRepository = invoiceRepository;
            _businessRepository = businessRepository;
            _contractRepository = contractRepository;
            _individualUserRepository = individualUserRepository;
            _userOfferRedeemRepository = userOfferRedeemRepository;
        }

        public async Task<GetBranchReportResponse> GetBranchReport(DateEntityFilter filter)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new GetBranchReportResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            if (filter.month.HasValue && !filter.year.HasValue)
            {
                return new GetBranchReportResponse(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Invalid filter: Month requires a corresponding year.",
                    payload: null
                );
            }

            try
            {
                var query = _businessRepository.GetBusinessForFilter();
                var totalItems = await query.CountAsync();
                var businesses = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;
                var payload = new BranchReportPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var business in businesses)
                {
                    var branches = await _branchRepository.GetBusinessBranches(business.Id);
                    var invoices = await _invoiceRepository.GetInvoiceByBusinessId(business.Id);

                    // Apply date filtering
                    if (filter.year.HasValue && filter.month.HasValue)
                    {
                        invoices = invoices.Where(
                            i => i.IssueDate.Year == filter.year.Value && 
                            i.IssueDate.Month == filter.month.Value
                        )
                        .ToList();
                    }
                    else if (filter.year.HasValue)
                    {
                        invoices = invoices.Where(i => i.IssueDate.Year == filter.year.Value).ToList();
                    }

                    var totalSales = invoices.Sum(i => i.TotalAmount);

                    var item = new BusinessReportItem()
                    {
                        Name = business.GetNameByLanguageCode(languageCode),
                        Branches = [],
                        TotalUsage = invoices.Count,
                        Sales = totalSales
                    };

                    foreach (var branch in branches)
                    {
                        var branchInvoices = invoices.Where(i => i.BranchId == branch.Id).ToList();
                        var totalSale = branchInvoices.Sum(i => i.TotalAmount);

                        item.Branches.Add(new BranchReportItem
                        {
                            Name = branch.Name,
                            TotalUsage = branchInvoices.Count,
                            Sales = totalSale
                        });
                    }

                    payload.Items.Add(item);
                }

                return new GetBranchReportResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Branch Report Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating reports");
                return new GetBranchReportResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetStoreReportResponse> GetStoreReport(DateEntityFilter filter)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new GetStoreReportResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }


            if (filter.month.HasValue && !filter.year.HasValue)
            {
                return new GetStoreReportResponse(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Invalid filter: Month requires a corresponding year.",
                    payload: null
                );
            }

            try
            {
                var query = _businessRepository.GetBusinessForFilter();

                var totalItems = await query.CountAsync();
                var businesses = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new StoreReportPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var business in businesses)
                {
                    var invoices = await _invoiceRepository.GetInvoiceByBusinessId(business.Id);

                    if (filter.year.HasValue && filter.month.HasValue)
                    {
                        invoices = invoices.Where(i => i.IssueDate.Year == filter.year.Value && i.IssueDate.Month == filter.month.Value).ToList();
                    }
                    else if (filter.year.HasValue)
                    {
                        invoices = invoices.Where(i => i.IssueDate.Year == filter.year.Value).ToList();
                    }

                    var totalSales = invoices.Sum(i => i.TotalAmount);
                    var offers = await _offerRepository.GetBusinessOffers(business.Id, false);

                    var item = new StoreReportItem()
                    {
                        Name = business.GetNameByLanguageCode(languageCode),
                        TotalOffers = offers.Count,
                        TotalUsage = invoices.Count,
                        Sales = totalSales
                    };

                    payload.Items.Add(item);
                }

                return new GetStoreReportResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Store Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating reports");
                return new GetStoreReportResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetOfferRedeemStateResponse> GetOfferUsage(int year)
        {
            try
            {
                var allInvoices = await _invoiceRepository.GetInvoicesByYear(year);
                
                // Get all user offer redeems for this year (now it can have multiple entries per offer)
                var redeems = await _userOfferRedeemRepository.GetRedeemsByYear(year);
                
                // Create a dictionary to store monthly offer redeem counts
                var monthlyOfferRedeemCounts = new Dictionary<(int, int), int>();
                
                // Initialize all months with zero redeems
                for (int month = 1; month <= 12; month++)
                {
                    monthlyOfferRedeemCounts[(year, month)] = 0;
                }
                
                // Group redeems by month and count
                foreach (var redeem in redeems)
                {
                    var redeemYear = redeem.CreationTime.Year;
                    var redeemMonth = redeem.CreationTime.Month;
                    if (redeemYear == year)
                    {
                        monthlyOfferRedeemCounts[(year, redeemMonth)]++;
                    }
                }

                var targetMonths = Enumerable.Range(1, 12)
                    .Select(month => new { Year = year, Month = month })
                    .ToList();

                var stats = targetMonths
                    .GroupJoin(
                        allInvoices,
                        target => new { target.Year, target.Month },
                        invoice => new { invoice.IssueDate.Year, invoice.IssueDate.Month },
                        (target, invoices) => 
                        {
                            var invoiceCount = invoices.Count();
                            return new OfferRedeemStat
                            {
                                Year = target.Year,
                                Month = target.Month,
                                TotalRedeemCount = invoiceCount + monthlyOfferRedeemCounts[(year, target.Month)],
                            };
                        })
                    .OrderBy(stat => stat.Month)
                    .ToList();

                return new GetOfferRedeemStateResponse(
                    StatusCodes.Status200OK,
                    "Offer Redeem Stats",
                    payload: new OfferRedeemStatePayload { Stats = stats }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting offer redeem stats");
                return new GetOfferRedeemStateResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetDashboardReportResponse> GetDashboardReport()
        {
            try
            {
                var auth = FirebaseAuth.DefaultInstance;
                int count = 0;

                var enumerator = auth.ListUsersAsync(null).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync())
                {
                    count++;
                }

                var businessUsers = _businessRepository.GetBusinessCount();
                var users = _individualUserRepository.GetUsersCount();
                var branches = _branchRepository.GetBranchCount();
                var activeOffers = _offerRepository.GetTotalActiveOffers();

                var payload = new GetDashboardReportPayload
                {
                    Downloads = count,
                    BusinessUsers = businessUsers,
                    IndividualUsers = users,
                    Branches = branches,
                    ActiveOffers = activeOffers,
                };

                return new GetDashboardReportResponse(
                   StatusCodes.Status200OK,
                   "Success",
                   payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting offer dashboard report");
                return new GetDashboardReportResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetBusinessDetailsForDashboardResponse> GetDashboardBusinessDetail(EntityFilter filter)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessDetailsForDashboardResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var query = _businessRepository.GetBusinessForFilter();

                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(business => business.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(business =>
                        business.NameEn.ToLower().Contains(searchKeyword) ||
                        business.Address.ToLower().Contains(searchKeyword) ||
                        business.Category.NameEn.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var businesses = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetBusinessDetailsForDashboardPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };
                foreach (var business in businesses)
                {
                    var name = business.GetNameByLanguageCode(languageCode);
                    var offerCount = _offerRepository.GetTotalActiveOffers(business.Id);
                    var contract = await _contractRepository.GetBusinessContract(business.Id);
                    GetContractPayload? contractPayload = null;
                    if (contract != null)
                    {
                        contractPayload = _mapper.Map<GetContractPayload>(contract);
                        contractPayload.ContractType = (int)contract.ContractType;
                        contractPayload.SubscriptionAmount = 0;
                        contractPayload.OfferAmount = 0;
                        contractPayload.InvoicePercentage = 0;

                        // Assign correct values based on contract type
                        switch (contract.ContractType)
                        {
                            case ContractType.MonthlySubscription:
                                contractPayload.SubscriptionAmount = contract.SubscriptionAmount ?? 0;
                                break;
                            case ContractType.FixedAmountPerOffer:
                                contractPayload.OfferAmount = contract.FixedAmountPerOffer ?? 0;
                                break;
                            case ContractType.PercentageOfInvoice:
                                contractPayload.InvoicePercentage = contract.PercentageOfInvoice ?? 0;
                                break;
                        }
                    }

                    var item = new BusinessDetailsForDashboardItem
                    {
                        Name = name,
                        OfferCount = offerCount,
                        PhoneNumber = business.MobileNumber,
                        Contract = contractPayload
                    };
                    payload.Items.Add(item);
                }

                return new GetBusinessDetailsForDashboardResponse(
                   StatusCodes.Status200OK,
                   "Success",
                   payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting offer dashboard report");
                return new GetBusinessDetailsForDashboardResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetBranchDetailsForDashboardResponse> GetDashboardBranchDetail()
        {
            try
            {
                var branches = await _branchRepository.GetAllBranches()
                     .ToListAsync();

                var payload = new GetBranchDetailsForDashboardPayload
                {
                    Items = []
                };

                foreach (var branch in branches)
                {
                    var item = _mapper.Map<BranchDetailsForDashboardItem>(branch);
                    item.LogoPath = branch.Business.LogoPath;
                    payload.Items.Add(item);
                }

                return new GetBranchDetailsForDashboardResponse(
                   StatusCodes.Status200OK,
                   "Success",
                   payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting branch details");
                return new GetBranchDetailsForDashboardResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetOfferUsageDetailsByMonthResponse> GetDashboardOfferUsageByMonth(int month, int year, bool isOnline=false)
        {
            try
            {
                if (isOnline) // For online (offers)
                {
                    var activeOffers = await _offerRepository.GetOffersByMonth(month, year);
                    var businessUsers = activeOffers.Select(x => x.BusinessId).Distinct();
                    
                    // Get actual redeem count from UserOfferRedeem table for this month
                    var redeems = await _userOfferRedeemRepository.GetRedeemsByMonth(year, month);
                    var totalRedeemCount = redeems.Count;
                    
                    // Get unique businesses that had redeems
                    var businessesWithRedeems = redeems
                        .Join(activeOffers, 
                              r => r.OfferId, 
                              o => o.Id, 
                              (r, o) => o.BusinessId)
                        .Distinct()
                        .Count();
                    
                    // Get unique offers that were redeemed
                    var offersWithRedeems = redeems
                        .Select(r => r.OfferId)
                        .Distinct()
                        .Count();
                    
                    var payload = new OfferUsageDetailsByMonthPayload()
                    {
                        TotalBusinessUsers = businessesWithRedeems,
                        TotalOffers = offersWithRedeems,
                        TotalUsage = totalRedeemCount
                    };

                    return new GetOfferUsageDetailsByMonthResponse(
                       StatusCodes.Status200OK,
                       "Success",
                       payload
                    );
                }
                else // For offline (invoices)
                {
                    var invoices = await _invoiceRepository.GetInvoiceByMonth(month, year).ToListAsync();
                    var businessUsers = invoices.Select(x => x.Business.Id).Distinct();
                    var offers = invoices.Select(x => x.Offer.Id).Distinct();

                    var payload = new OfferUsageDetailsByMonthPayload()
                    {
                        TotalBusinessUsers = businessUsers.Count(),
                        TotalOffers = offers.Count(),
                        TotalUsage = invoices.Count
                    };

                    return new GetOfferUsageDetailsByMonthResponse(
                       StatusCodes.Status200OK,
                       "Success",
                       payload
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting offer usage details");
                return new GetOfferUsageDetailsByMonthResponse(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetOfferDashboardDetailsByTypeRespose> GetDashboardOfferDetailsByType(string type, DateEntityFilter filter, bool isOnline = false)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetOfferDashboardDetailsByTypeRespose
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                type = type.ToLower().Trim();
                if (type != "business" && type != "offer" && type != "usage")
                {
                    return new GetOfferDashboardDetailsByTypeRespose(
                        StatusCodes.Status400BadRequest,
                        "Invalid type: User 'business', 'offer' or 'usage'",
                        null
                    );
                }

                if (isOnline)
                {
                    // Handle online (offers) data
                    var monthOffers = await _offerRepository.GetOffersByMonth(filter.month.Value, filter.year.Value);
                    
                    if (monthOffers.Count == 0)
                    {
                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "No data available",
                            new OfferDashboardDetailsByTypePayload
                            {
                                Items = [],
                                TotalItems = 0,
                                HasNextPage = false
                            }
                        );
                    }
                    
                    // Get actual redeem counts from the UserOfferRedeem table
                    var redeems = await _userOfferRedeemRepository.GetRedeemsByMonth(filter.year.Value, filter.month.Value);
                    
                    // Create a dictionary of offer ID to redeem count
                    var offerRedeemCounts = redeems
                        .GroupBy(r => r.OfferId)
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    // Total redeem count for reference in percentage calculations
                    int totalRedeemCount = redeems.Count;
                    
                    // For offers without any redeems in the tracked period, set count to 0
                    foreach (var offer in monthOffers)
                    {
                        if (!offerRedeemCounts.ContainsKey(offer.Id))
                        {
                            offerRedeemCounts[offer.Id] = 0;
                        }
                    }
                    
                    if (type == "business")
                    {
                        var businessRedeemCounts = redeems
                            .Join(monthOffers,
                                  r => r.OfferId,
                                  o => o.Id,
                                  (r, o) => new { BusinessId = o.BusinessId, Business = o.Business })
                            .GroupBy(item => item.Business)
                            .ToDictionary(g => g.Key, g => g.Count());
                        
                        var rankedItems = businessRedeemCounts
                            .Select(kvp => new
                            {
                                Business = kvp.Key,
                                RedeemCount = kvp.Value
                            })
                            .OrderByDescending(x => x.RedeemCount)
                            .ThenBy(x => x.Business.Id)
                            .Select((x, index) => new OfferDashboardDetailItem
                            {
                                Rank = index + 1,
                                Usage = x.RedeemCount,
                                Number = totalRedeemCount > 0 ? (int)Math.Round(((double)x.RedeemCount / totalRedeemCount) * 100) : 0,
                                BusinessTitle = x.Business.GetNameByLanguageCode(languageCode),
                                BusinessId = x.Business.Id,
                                OfferTitle = string.Empty,
                                OfferId = 0
                            })
                            .ToList();

                        var paginatedItems = rankedItems
                            .Skip((filter.pageNumber - 1) * filter.pageSize)
                            .Take(filter.pageSize)
                            .ToList();

                        var payload = new OfferDashboardDetailsByTypePayload
                        {
                            Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                            TotalItems = rankedItems.Count(),
                            HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count()
                        };

                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "Success",
                            payload
                        );
                    }
                    else if (type == "offer")
                    {
                        var rankedItems = monthOffers
                            .Select(o => new
                            {
                                Offer = o,
                                RedeemCount = offerRedeemCounts.ContainsKey(o.Id) ? offerRedeemCounts[o.Id] : 0
                            })
                            .OrderByDescending(x => x.RedeemCount)
                            .ThenBy(x => x.Offer.Title)
                            .Select((x, index) => new OfferDashboardDetailItem
                            {
                                Rank = index + 1,
                                Usage = x.RedeemCount,
                                Number = totalRedeemCount > 0 ? (int)Math.Round(((double)x.RedeemCount / totalRedeemCount) * 100) : 0,
                                BusinessTitle = x.Offer.Business.GetNameByLanguageCode(languageCode),
                                BusinessId = x.Offer.BusinessId,
                                OfferTitle = x.Offer.Title,
                                OfferId = x.Offer.Id
                            })
                            .ToList();

                        var paginatedItems = rankedItems
                          .Skip((filter.pageNumber - 1) * filter.pageSize)
                          .Take(filter.pageSize)
                          .ToList();

                        var payload = new OfferDashboardDetailsByTypePayload
                        {
                            Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                            TotalItems = rankedItems.Count(),
                            HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count()
                        };

                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "Success",
                            payload
                        );
                    }
                    else // type == usage
                    {
                        // For usage of online offers, we now have individual redemption records
                        var offerRedeemDetails = redeems
                            .Join(monthOffers,
                                  r => r.OfferId,
                                  o => o.Id,
                                  (r, o) => new { 
                                      Redeem = r, 
                                      Offer = o 
                                  })
                            .ToList();
                        
                        var rankedItems = new List<OfferDashboardUsageDetailItem>();
                        
                        foreach (var item in offerRedeemDetails)
                        {
                            var user = await _individualUserRepository.GetByIdAsync(item.Redeem.UserId);
                            
                            rankedItems.Add(new OfferDashboardUsageDetailItem
                            {
                                OfferTitle = item.Offer.Title,
                                OfferId = item.Offer.Id,
                                BusinessTitle = item.Offer.Business.GetNameByLanguageCode(languageCode),
                                BusinessId = item.Offer.BusinessId,
                                IssueDate = item.Redeem.CreationTime,
                                UserTitle = user?.DisplayName ?? "Unknown User",
                                UserId = item.Redeem.UserId,
                                InvoiceId = item.Redeem.Id,
                                InvoiceNumber = "RDM-" + item.Redeem.Id.ToString(), // Prefix with RDM for Redeem
                                Amount = 0,
                                Discount = 0,
                                TotalAmount = 0
                            });
                        }
                        
                        // Sort by creation time (newest first)
                        rankedItems = rankedItems.OrderByDescending(x => x.IssueDate).ToList();

                        var paginatedItems = rankedItems
                          .Skip((filter.pageNumber - 1) * filter.pageSize)
                          .Take(filter.pageSize)
                          .ToList();

                        var payload = new OfferDashboardDetailsByTypePayload
                        {
                            Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                            TotalItems = rankedItems.Count(),
                            HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count()
                        };

                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "Success",
                            payload
                        );
                    }
                }
                else
                {
                    // Handle offline (invoices) data - original implementation
                    var query = _invoiceRepository.GetInvoiceByMonth(filter.month.Value, filter.year.Value);

                    var invoiceList = await query.ToListAsync();
                    var totalUsage = invoiceList.Count;

                    if (totalUsage == 0)
                    {
                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "No data available",
                            new OfferDashboardDetailsByTypePayload
                            {
                                Items = [],
                                TotalItems = 0,
                                HasNextPage = false
                            }
                        );
                    }

                    if (type == "business")
                    {
                        var rankedItems = invoiceList
                            .GroupBy(i => new { i.Business })
                            .Select(g => new
                            {
                                g.Key.Business,
                                Usage = g.Count()
                            })
                            .OrderByDescending(x => x.Usage)
                            .ThenBy(x => x.Business.Id)
                            .Select((x, index) => new OfferDashboardDetailItem
                            {
                                Rank = index + 1,
                                Usage = x.Usage,
                                Number = (int)Math.Round(((double)x.Usage / totalUsage) * 100),
                                BusinessTitle = x.Business.GetNameByLanguageCode(languageCode),
                                BusinessId = x.Business.Id,
                                OfferTitle = string.Empty,
                                OfferId = 0
                            })
                            .ToList();

                        var paginatedItems = rankedItems
                            .Skip((filter.pageNumber - 1) * filter.pageSize)
                            .Take(filter.pageSize)
                            .ToList();

                        var payload = new OfferDashboardDetailsByTypePayload
                        {
                            Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                            TotalItems = rankedItems.Count(),
                            HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count
                        };

                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "Success",
                            payload
                        );
                    }
                    else if (type == "offer")
                    {
                        var rankedItems = invoiceList
                            .GroupBy(i => new { i.OfferId, i.Offer.Title, Business = i.Business })
                            .Select(g => new
                            {
                                g.Key.OfferId,
                                g.Key.Title,
                                g.Key.Business,
                                Usage = g.Count()
                            })
                            .OrderByDescending(x => x.Usage)
                            .ThenBy(x => x.Title)
                            .Select((x, index) => new OfferDashboardDetailItem
                            {
                                Rank = index + 1,
                                Usage = x.Usage,
                                Number = (int)Math.Round(((double)x.Usage / totalUsage) * 100),
                                BusinessTitle = x.Business.GetNameByLanguageCode(languageCode),
                                BusinessId = x.Business.Id,
                                OfferTitle = x.Title,
                                OfferId = x.OfferId
                            })
                            .ToList();

                        var paginatedItems = rankedItems
                          .Skip((filter.pageNumber - 1) * filter.pageSize)
                          .Take(filter.pageSize)
                          .ToList();

                        var payload = new OfferDashboardDetailsByTypePayload
                        {
                            Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                            TotalItems = rankedItems.Count(),
                            HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count
                        };

                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "Success",
                            payload
                        );
                    }
                    else // type == usage
                    {
                        var rankedItems = invoiceList
                            .Select(invoice => new OfferDashboardUsageDetailItem
                            {
                                OfferTitle = invoice.Offer.Title,
                                OfferId = invoice.Offer.Id,
                                BusinessTitle = invoice.Business.GetNameByLanguageCode(languageCode),
                                BusinessId = invoice.Business.Id,
                                InvoiceId = invoice.Id,
                                InvoiceNumber = invoice.InvoiceNumber,
                                Amount = invoice.Amount,
                                Discount = invoice.Discount,
                                TotalAmount = invoice.TotalAmount,
                                IssueDate = invoice.IssueDate,
                                UserTitle = invoice.User.DisplayName,
                                UserId = invoice.UserId,
                            })
                            .ToList();

                        var paginatedItems = rankedItems
                          .Skip((filter.pageNumber - 1) * filter.pageSize)
                          .Take(filter.pageSize)
                          .ToList();

                        var payload = new OfferDashboardDetailsByTypePayload
                        {
                            Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                            TotalItems = rankedItems.Count(),
                            HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count
                        };

                        return new GetOfferDashboardDetailsByTypeRespose(
                            StatusCodes.Status200OK,
                            "Success",
                            payload
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting dashboard offer details");
                return new GetOfferDashboardDetailsByTypeRespose(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetOfferDashboardDetailsByTypeRespose> GetDashboardUsageBusinessFilter(Guid businessId, DateEntityFilter filter)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetOfferDashboardDetailsByTypeRespose
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var query = _invoiceRepository.GetInvoiceByMonth(filter.month.Value, filter.year.Value);
                query = query.Where(i => i.BusinessId == businessId);

                var invoiceList = await query.ToListAsync();
                var totalUsage = invoiceList.Count;

                var rankedItems = invoiceList
                        .Select(invoice => new OfferDashboardUsageDetailItem
                        {
                            OfferTitle = invoice.Offer.Title,
                            OfferId = invoice.Offer.Id,
                            BusinessTitle = invoice.Business.GetNameByLanguageCode(languageCode),
                            BusinessId = invoice.Business.Id,
                            InvoiceId = invoice.Id,
                            InvoiceNumber = invoice.InvoiceNumber,
                            Amount = invoice.Amount,
                            Discount = invoice.Discount,
                            TotalAmount = invoice.TotalAmount,
                            IssueDate = invoice.IssueDate,
                            UserTitle = invoice.User.DisplayName,
                            UserId = invoice.UserId,
                        })
                        .ToList();

                var paginatedItems = rankedItems
                  .Skip((filter.pageNumber - 1) * filter.pageSize)
                  .Take(filter.pageSize)
                  .ToList();

                var payload = new OfferDashboardDetailsByTypePayload
                {
                    Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                    TotalItems = rankedItems.Count,
                    HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count
                };

                return new GetOfferDashboardDetailsByTypeRespose(
                    StatusCodes.Status200OK,
                    "Success",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting dashboard offer details");
                return new GetOfferDashboardDetailsByTypeRespose(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }

        public async Task<GetOfferDashboardDetailsByTypeRespose> GetDashboardUsageOfferFilter(int offerId, DateEntityFilter filter)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetOfferDashboardDetailsByTypeRespose
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var query = _invoiceRepository.GetInvoiceByMonth(filter.month.Value, filter.year.Value);
                query = query.Where(i => i.OfferId == offerId);

                var invoiceList = await query.ToListAsync();
                var totalUsage = invoiceList.Count;

                var rankedItems = invoiceList
                      .Select(invoice => new OfferDashboardUsageDetailItem
                      {
                          OfferTitle = invoice.Offer.Title,
                          OfferId = invoice.Offer.Id,
                          BusinessTitle = invoice.Business.GetNameByLanguageCode(languageCode),
                          BusinessId = invoice.Business.Id,
                          InvoiceId = invoice.Id,
                          InvoiceNumber = invoice.InvoiceNumber,
                          Amount = invoice.Amount,
                          Discount = invoice.Discount,
                          TotalAmount = invoice.TotalAmount,
                          IssueDate = invoice.IssueDate,
                          UserTitle = invoice.User.DisplayName,
                          UserId = invoice.UserId,
                      })
                      .ToList();

                var paginatedItems = rankedItems
                  .Skip((filter.pageNumber - 1) * filter.pageSize)
                  .Take(filter.pageSize)
                  .ToList();

                var payload = new OfferDashboardDetailsByTypePayload
                {
                    Items = paginatedItems.Cast<BaseOfferDashboardDetailItem>().ToList(),
                    TotalItems = rankedItems.Count,
                    HasNextPage = (filter.pageNumber * filter.pageSize) < rankedItems.Count
                };

                return new GetOfferDashboardDetailsByTypeRespose(
                    StatusCodes.Status200OK,
                    "Success",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting dashboard offer details");
                return new GetOfferDashboardDetailsByTypeRespose(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    null
                );
            }
        }
    }
}
