using SaudiPlus.Entities;

namespace SaudiPlus.Interfaces.Repository
{
    public interface IForgetPasswordRepository : IRepository<ForgetPassword, int>
    {
        public Task<ForgetPassword?> FindByUserIdAsync(Guid userId);
    }
}
