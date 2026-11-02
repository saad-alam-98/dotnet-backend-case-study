using SaudiPlus.Entities;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Repository
{
    public class ContactUsRepository : Repository<ContactUs, int>, IContactUsRepository
    {
        public ContactUsRepository(SaudiPlusDbContext dbContext) : base(dbContext)
        {
        }
    }
}
