using SaudiPlus.Service;
using SaudiPlus.Repository;
using SaudiPlus.Interfaces;
using SaudiPlus.Service.Auth;
using SaudiPlus.Service.City;
using SaudiPlus.Service.Offer;
using SaudiPlus.Service.Status;
using SaudiPlus.Service.Policy;
using SaudiPlus.Service.Wallet;
using SaudiPlus.Service.Branch;
using SaudiPlus.Service.Report;
using SaudiPlus.Service.Banner;
using SaudiPlus.Service.Partner;
using SaudiPlus.Service.Invoice;
using SaudiPlus.Service.Account;
using SaudiPlus.Service.Support;
using SaudiPlus.Service.Contract;
using SaudiPlus.Service.SubAdmin;
using SaudiPlus.Service.Category;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Service.BusinessUser;
using SaudiPlus.Service.Notification;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Service.IndividualUser;
using SaudiPlus.Service.BusinessRequest;

namespace SaudiPlus.Configurations
{
    public static class ApplicationConfiguration
    {
        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<INotificationCenterService, NotificationCenterService>();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IForgetPasswordRepository, ForgetPasswordRepository>();

            services.AddScoped<IIndividualUserRepository, IndividualUserRepository>();
            services.AddScoped<IIndividualUserService, IndividualUserService>();

            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<IBusinessUserService, BusinessUserService>();

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            services.AddScoped<IBannerService, BannerService>();
            services.AddScoped<IBannerRepository, BannerRepository>();

            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICityRepository, CityRepository>();

            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<IOfferLogoRepository, OfferLogoRepository>();
            services.AddScoped<IUserOfferRedeemRepository, UserOfferRedeemRepository>();

            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IBranchRepository, BranchRepository>();

            services.AddScoped<ISupportService, SupportService>();
            services.AddScoped<ISupportTypeRepository, SupportTypeRepository>();
            services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
            services.AddScoped<ISupportMessageRepository, SupportMessageRepository>();

            services.AddScoped<IPolicyService, PolicyService>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();
            services.AddScoped<IContactUsRepository, ContactUsRepository>();
            services.AddScoped<IBusinessPolicyRepository, BusinessPolicyRepository>();

            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();

            services.AddScoped<IPartnerService, PartnerService>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();

            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IContractRepository, ContractRepository>();

            services.AddScoped<IReportService, ReportService>();

            services.AddScoped<INotificationCenterService, NotificationCenterService>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<IUserOfferTokenRepository, UserOfferTokenRepository>();
            services.AddScoped<IFavouriteRepository, FavouriteRepository>();

            services.AddScoped<ITextMessageService, TextMessageService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletService, WalletService>();

            services.AddScoped<ISubAdminRepository, SubAdminRepository>();
            services.AddScoped<ISubAdminService, SubAdminService>();

            services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();

            services.AddScoped<IBusinessRequestService, BusinessRequestService>();
            services.AddScoped<IBusinessRequestRepository, BusinessRequestRepository>();

            services.AddScoped<IStatusRepository, StatusRepository>();
            services.AddScoped<IStatusService, StatusService>();

            services.AddScoped<IStatusInteractionRepository, StatusInteractionRepository>();
        }
    }
}
