using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Report;
using SaudiPlus.Response.BusinessUser;

namespace SaudiPlus.Interfaces.Service
{
    public interface IReportService
    {
        Task<GetStoreReportResponse> GetStoreReport(DateEntityFilter filter);
        Task<GetBranchReportResponse> GetBranchReport(DateEntityFilter filter);
        Task<GetOfferRedeemStateResponse> GetOfferUsage(int year);
        Task<GetDashboardReportResponse> GetDashboardReport();
        Task<GetBusinessDetailsForDashboardResponse> GetDashboardBusinessDetail(EntityFilter filter);
        Task<GetBranchDetailsForDashboardResponse> GetDashboardBranchDetail();
        Task<GetOfferUsageDetailsByMonthResponse> GetDashboardOfferUsageByMonth(int month, int year, bool isOnline = false);
        Task<GetOfferDashboardDetailsByTypeRespose> GetDashboardOfferDetailsByType(string type, DateEntityFilter filter, bool isOnline = false);
        Task<GetOfferDashboardDetailsByTypeRespose> GetDashboardUsageBusinessFilter(Guid businessId, DateEntityFilter filter);
        Task<GetOfferDashboardDetailsByTypeRespose> GetDashboardUsageOfferFilter(int offerId, DateEntityFilter filter);
    }
}
