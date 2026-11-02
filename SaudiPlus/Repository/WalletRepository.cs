using SaudiPlus.Entities;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class WalletRepository : Repository<Wallet, int>, IWalletRepository
    {
        public WalletRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }
    }
}
