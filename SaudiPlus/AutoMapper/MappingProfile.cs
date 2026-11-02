using AutoMapper;
using SaudiPlus.Entities;
using SaudiPlus.Request.City;
using SaudiPlus.Response.City;
using SaudiPlus.Request.Offer;
using SaudiPlus.Response.User;
using SaudiPlus.Response.Offer;
using SaudiPlus.Request.Branch;
using SaudiPlus.Request.Banner;
using SaudiPlus.Request.Policy;
using SaudiPlus.Request.Status;
using SaudiPlus.Response.Policy;
using SaudiPlus.Response.Branch;
using SaudiPlus.Response.Banner;
using SaudiPlus.Request.Support;
using SaudiPlus.Response.Status;
using SaudiPlus.Response.Wallet;
using SaudiPlus.Response.Report;
using SaudiPlus.Request.Partner;
using SaudiPlus.Response.Support;
using SaudiPlus.Response.Partner;
using SaudiPlus.Request.SubAdmin;
using SaudiPlus.Request.Contract;
using SaudiPlus.Request.Category;
using SaudiPlus.Response.Account;
using SaudiPlus.Response.Invoice;
using SaudiPlus.Response.SubAdmin;
using SaudiPlus.Entities.Identity;
using SaudiPlus.Response.Contract;
using SaudiPlus.Response.Category;
using SaudiPlus.Request.BusinessUser;
using SaudiPlus.Request.Notification;
using SaudiPlus.Response.BusinessUser;
using SaudiPlus.Entities.Notification;
using SaudiPlus.Response.Notification;
using SaudiPlus.Request.IndividualUser;
using SaudiPlus.Request.BusinessRequest;
using SaudiPlus.Response.IndividualUser;
using SaudiPlus.Response.BusinessRequest;

namespace SaudiPlus.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap(typeof(User), typeof(RegisterUserDTO)).ReverseMap();
            CreateMap(typeof(User), typeof(CreateUserDTO)).ReverseMap();
            CreateMap(typeof(User), typeof(CreateSubAdminUserDTO)).ReverseMap();
            CreateMap(typeof(User), typeof(LoginData)).ReverseMap();

            CreateMap(typeof(CreateBusinessDTO), typeof(User)).ReverseMap();
            CreateMap(typeof(CreateBusinessDTO), typeof(BusinessUser)).ReverseMap();

            CreateMap(typeof(AdminCreateBusinessDTO), typeof(User)).ReverseMap();
            CreateMap(typeof(AdminCreateBusinessDTO), typeof(BusinessUser)).ReverseMap();

            CreateMap(typeof(BusinessInfoDetailed), typeof(BusinessUser)).ReverseMap();
            CreateMap(typeof(BusinessInfoLocalized), typeof(BusinessUser)).ReverseMap();
            CreateMap<BusinessUser, InvoiceBusinessData>();
            CreateMap<BusinessUser, FavouriteBusinessItem>();
            CreateMap(typeof(GetBusinessContactPayload), typeof(BusinessUser)).ReverseMap();
            CreateMap(typeof(BusinessLocationItem), typeof(BusinessUser)).ReverseMap();
            CreateMap<BusinessUser, GetBusinessDetailsPayload>()
             .ForMember(dest => dest.Offers, opt => opt.Ignore())
             .ForMember(dest => dest.Branches, opt => opt.Ignore())
             .ForMember(dest => dest.TermsAndConditions, opt => opt.Ignore())
             .ForMember(dest => dest.PrivacyPolicy, opt => opt.Ignore());

            CreateMap<EditBusinessDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition(
                        (src, dest, srcMember) => srcMember != null
                    )
                );

            CreateMap<BusinessContactDTO, User>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForAllMembers(opt =>
                   opt.Condition(
                       (src, dest, srcMember) => srcMember != null
                   )
               );

            CreateMap<EditBusinessDTO, BusinessUser>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForAllMembers(opt =>
                   opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<BusinessContactDTO, BusinessUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap(typeof(RegisterUserDTO), typeof(IndividualUser)).ReverseMap();
            CreateMap(typeof(CreateUserDTO), typeof(IndividualUser)).ReverseMap();
            CreateMap(typeof(GuestLoginDTO), typeof(IndividualUser)).ReverseMap();
            CreateMap(typeof(GetUserPointsPayload), typeof(IndividualUser)).ReverseMap();
            CreateMap(typeof(GetAvatarPayload), typeof(IndividualUser)).ReverseMap();
            CreateMap(typeof(IndividualUserData), typeof(IndividualUser)).ReverseMap();
            CreateMap(typeof(NotificationUserItem), typeof(IndividualUser)).ReverseMap();
            CreateMap<IndividualUser, InvoiceUserData>();
            CreateMap<EditUserDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );
            CreateMap<EditUserDTO, IndividualUser>()
            .ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );

            CreateMap(typeof(ForgetPassword), typeof(ForgetPasswordPayload)).ReverseMap();

            CreateMap(typeof(CategoryDTO), typeof(Category)).ReverseMap();
            CreateMap(typeof(CategoryInfoDetailed), typeof(Category)).ReverseMap();
            CreateMap(typeof(CategoryInfoLocalized), typeof(Category)).ReverseMap();
            CreateMap<EditCategoryDTO, Category>()
            .ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );

            CreateMap(typeof(CreateBannerDTO), typeof(Banner)).ReverseMap();
            CreateMap(typeof(BannerPayloadItem), typeof(Banner)).ReverseMap();
            CreateMap<EditBannerDTO, Banner>().ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );

            CreateMap<Banner, BannerPayloadItem>()
            .ForMember(dest => dest.BannerId, opt => opt.MapFrom(src => src.Id));

            CreateMap(typeof(AddCityDTO), typeof(City)).ReverseMap();
            CreateMap<City, CityInfoDetailed>().ReverseMap();
            CreateMap<City, CityInfoLocalized>().ReverseMap();
            CreateMap<EditCityDTO, City>().ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );

            CreateMap<CreateOfferDTO, Offer>()
                .ForMember(dest => dest.Logos, opt => opt.Ignore());
            CreateMap<AdminCreateOfferDTO, Offer>()
                .IncludeBase<CreateOfferDTO, Offer>();

            CreateMap(typeof(OrderOfferItem), typeof(Offer)).ReverseMap();
            CreateMap<Offer, OfferPayloadItem>().ReverseMap()
                .ForMember(dest => dest.Logos, opt => opt.Ignore());

            CreateMap<Offer, RedeemOfferPayload>().ReverseMap();
            CreateMap<Offer, GetBusinessOffersDetails>().ReverseMap();
            CreateMap<Offer, BusinessOfferItem>().ReverseMap()
                 .ForMember(dest => dest.Logos, opt => opt.Ignore());
            CreateMap<EditOfferDTO, Offer>()
             .ForMember(dest => dest.Logos, opt => opt.Ignore())
             .ForAllMembers(opt =>
                 opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<BusinessCreateBranchDTO, Branch>().ReverseMap();
            CreateMap<AdminCreateBranchDTO, Branch>().ReverseMap();
            CreateMap<ICreateBranchDTO, Branch>().ReverseMap();
            CreateMap<EditBranchDTO, Branch>().ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );
            CreateMap<BranchPayloadItem, Branch>().ReverseMap();
            CreateMap<BranchLocationItem, Branch>().ReverseMap();
            CreateMap<Branch, InvoiceBranchData>();
            CreateMap<Branch, InvoiceBranchDetail>();
            CreateMap<Branch, BranchSalesItem>();
            CreateMap<Branch, BranchDetailsForDashboardItem>();
            CreateMap<Branch, GetBusinessBranchDetails>().ReverseMap();
            CreateMap<GetBranchQRCodePayload, Branch>().ReverseMap();

            CreateMap<AddSupportTypeDTO, SupportType>().ReverseMap();
            CreateMap<SupportType, SupportTypeItemDetailed>().ReverseMap();
            CreateMap<SupportType, SupportTypeItemLocalized>().ReverseMap();
            CreateMap<EditSupportTypeDTO, SupportType>().ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );

            CreateMap<CreateSupportTicketDTO, SupportTicket>().ReverseMap();
            CreateMap<CreateSupportTicketDTO, SupportMessage>().ReverseMap();

            CreateMap<GetTicketPayload, SupportTicket>().ReverseMap();
            CreateMap<GetTicketItemPayload, SupportTicket>().ReverseMap()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => new TicketUserInfo
                {
                    DisplayName = src.User.DisplayName,
                    AvatarUrl = src.User.AvatarUrl
                }));
            CreateMap<TicketMessagePayload, SupportMessage>().ReverseMap();

            CreateMap<Policy, SetPolicyDTO>().ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.Ignore());
            CreateMap<Policy, PolicyDetailed>().ReverseMap();
            CreateMap<Policy, PolicyLocalized>().ReverseMap();

            CreateMap<BusinessPolicy, SetPolicyDTO>().ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.Ignore());
            CreateMap<BusinessPolicy, PolicyDetailed>().ReverseMap();
            CreateMap<BusinessPolicy, PolicyLocalized>().ReverseMap();

            CreateMap<ContactUs, SetContactDTO>().ReverseMap().ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );
            CreateMap<ContactUs, ContactUsPayload>().ReverseMap();

            CreateMap<Invoice, InvoiceDetailPayload>().ReverseMap();
            CreateMap<Invoice, GetUserOrdersItems>().ReverseMap();

            CreateMap<Partner, CreatePartnerDTO>().ReverseMap();
            CreateMap<Partner, BusinessUser>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<EditPartnerDTO, Partner>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryId, opt =>
                opt.Condition((src, dest, srcMember) => src.CategoryId.HasValue))
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null)
            );

            CreateMap<Partner, PartnerInfoDetailPayload>().ReverseMap();
            CreateMap<Partner, PartnerInfoLocalizedPayload>().ReverseMap();

            CreateMap<CreateContractDTO, Contract>();
            CreateMap<EditContractDTO, Contract>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null)
            );

            CreateMap<Contract, GetContractPayload>();

            CreateMap<BaseNotification, BaseNotificationItem>().ReverseMap();
            CreateMap<IndividualNotification, IndividualNotificationPayload>()
                .IncludeBase<BaseNotification, BaseNotificationItem>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => new NotificationUserItem
                {
                    UserId = src.UserId,
                    Email = src.User.Email,
                    DisplayName = src.User != null ? src.User.DisplayName : string.Empty,
                    AvatarUrl = src.User != null ? src.User.AvatarUrl : string.Empty,
                }));

            CreateMap<GroupNotification, GroupNotificationPayload>()
                .IncludeBase<BaseNotification, BaseNotificationItem>()
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src =>
                    src.Users.Select(u => new NotificationUserItem
                    {
                        UserId = u.Id,
                        Email = u.Email,
                        DisplayName = u.DisplayName,
                        AvatarUrl = u.AvatarUrl
                    }).ToArray()));

            CreateMap<GeneralNotification, GeneralNotificationPayload>()
                .IncludeBase<BaseNotification, BaseNotificationItem>();

            CreateMap<IndividualNotificationRequestDTO, IndividualNotification>();
            CreateMap<GroupUsersNotificationRequestDTO, GroupNotification>();
            CreateMap<GeneralNotificationRequestDTO, GeneralNotification>();

            CreateMap<CreateSubAdminUserDTO, SubAdminUser>()
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());
            CreateMap<EditSubAdminUserDTO, SubAdminUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForAllMembers(opt =>
                opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                )
            );
            CreateMap<SubAdminUser, GetSubAdminPayload>()
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());
            
            CreateMap<CreateBusinessRequestDTO, BusinessRequest>();
            CreateMap<BusinessRequest, BusinessRequestPayload>();

            CreateMap<CreateStatusDTO, Status>();
            CreateMap<EditStatusDTO, Status>();
            CreateMap<Status, GetStatusPayload>();
            CreateMap<Status, UserStatusPayload>();
        }
    }
}
