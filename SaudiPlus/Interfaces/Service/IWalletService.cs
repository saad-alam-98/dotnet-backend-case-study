using SaudiPlus.Helper.Filter;
using SaudiPlus.Response.Wallet;

namespace SaudiPlus.Interfaces.Service
{
    public interface IWalletService
    {
        Task<GetBusinessWalletResponse> GetBusinessWallet(Guid businessId, int year, int month, EntityFilter filter);
    }
}
