using SaudiPlus.Enums;

namespace SaudiPlus.Constants
{
    public static class Permissions
    {
        public static IEnumerable<PermissionGroup> All()
        {
            return new[]
            {
                new PermissionGroup("Terms And Conditions", "الشروط والأحكام", new[]
                {
                    new PermissionItem(TermsAndConditions.Create, "Create", "إنشاء"),
                    new PermissionItem(TermsAndConditions.View, "View", "عرض"),
                }),
                new PermissionGroup("Contact", "جهة الاتصال", new[]
                {
                    new PermissionItem(Contact.Create, "Create", "إنشاء"),
                    new PermissionItem(Contact.View, "View", "عرض"),
                }),
                new PermissionGroup("Reports", "التقرير", new[]
                {
                    new PermissionItem(Reports.View, "View", "عرض"),
                }),
                new PermissionGroup("Banner", "بانر", new[]
                {
                    new PermissionItem(Banner.Create, "Create", "إنشاء"),
                    new PermissionItem(Banner.Edit, "Edit", "تعديل"),
                    new PermissionItem(Banner.Delete, "Delete", "حذف"),
                    new PermissionItem(Banner.View, "View", "عرض"),
                }),
                new PermissionGroup("City", "مدينة", new[]
                {
                    new PermissionItem(City.Create, "Create", "إنشاء"),
                    new PermissionItem(City.Edit, "Edit", "تعديل"),
                    new PermissionItem(City.Delete, "Delete", "حذف"),
                    new PermissionItem(City.View, "View", "عرض"),
                }),
                new PermissionGroup("Category", "الفئة", new[]
                {
                    new PermissionItem(Category.Create, "Create", "إنشاء"),
                    new PermissionItem(Category.Edit, "Edit", "تعديل"),
                    new PermissionItem(Category.Delete, "Delete", "حذف"),
                    new PermissionItem(Category.View, "View", "عرض"),
                }),
                new PermissionGroup("Branch", "الفرع", new[]
                {
                    new PermissionItem(Branch.Create, "Create", "إنشاء"),
                    new PermissionItem(Branch.Edit, "Edit", "تعديل"),
                    new PermissionItem(Branch.Delete, "Delete", "حذف"),
                    new PermissionItem(Branch.View, "View", "عرض"),
                }),
                new PermissionGroup("Business", "النشاط التجاري", new[]
                {
                    new PermissionItem(Business.Create, "Create", "إنشاء"),
                    new PermissionItem(Business.Edit, "Edit", "تعديل"),
                    new PermissionItem(Business.Delete, "Delete", "حذف"),
                    new PermissionItem(Business.View, "View", "عرض"),
                }),
                new PermissionGroup("Offer", "العرض", new[]
                {
                    new PermissionItem(Offer.Create, "Create", "إنشاء"),
                    new PermissionItem(Offer.Edit, "Edit", "تعديل"),
                    new PermissionItem(Offer.Delete, "Delete", "حذف"),
                    new PermissionItem(Offer.View, "View", "عرض"),
                }),
                new PermissionGroup("Contract", "العقد", new[]
                {
                    new PermissionItem(Contract.Create, "Create", "إنشاء"),
                    new PermissionItem(Contract.Edit, "Edit", "تعديل"),
                    new PermissionItem(Contract.Delete, "Delete", "حذف"),
                    new PermissionItem(Contract.View, "View", "عرض"),
                }),
                new PermissionGroup("User", "المستخدم", new[]
                {
                    new PermissionItem(User.Create, "Create", "إنشاء"),
                    new PermissionItem(User.Edit, "Edit", "تعديل"),
                    new PermissionItem(User.Delete, "Delete", "حذف"),
                    new PermissionItem(User.View, "View", "عرض"),
                }),
                new PermissionGroup("Invoice", "الفاتورة", new[]
                {
                    new PermissionItem(Invoice.Create, "Create", "إنشاء"),
                    new PermissionItem(Invoice.View, "View", "عرض"),
                }),
                new PermissionGroup("Notification", "الإشعار", new[]
                {
                    new PermissionItem(Notification.Create, "Create", "إنشاء"),
                    new PermissionItem(Notification.View, "View", "عرض"),
                }),
                new PermissionGroup("Partner", "الشريك", new[]
                {
                    new PermissionItem(Partner.Create, "Create", "إنشاء"),
                    new PermissionItem(Partner.Edit, "Edit", "تعديل"),
                    new PermissionItem(Partner.Delete, "Delete", "حذف"),
                    new PermissionItem(Partner.View, "View", "عرض"),
                }),
                new PermissionGroup("Sub Admin", "مشرف فرعي", new[]
                {
                    new PermissionItem(SubAdmin.Create, "Create", "إنشاء"),
                    new PermissionItem(SubAdmin.Edit, "Edit", "تعديل"),
                    new PermissionItem(SubAdmin.Delete, "Delete", "حذف"),
                    new PermissionItem(SubAdmin.View, "View", "عرض"),
                }),
                new PermissionGroup("Support", "الدعم", new[]
                {
                    new PermissionItem(Support.Create, "Create", "إنشاء"),
                    new PermissionItem(Support.Edit, "Edit", "تعديل"),
                    new PermissionItem(Support.Delete, "Delete", "حذف"),
                    new PermissionItem(Support.View, "View", "عرض"),
                }),
                new PermissionGroup("Status", "حالة", new[]
                {
                    new PermissionItem(Support.Create, "Create", "إنشاء"),
                    new PermissionItem(Support.Edit, "Edit", "تعديل"),
                    new PermissionItem(Support.Delete, "Delete", "حذف"),
                    new PermissionItem(Support.View, "View", "عرض"),
                }),
            };
        }

        public static Dictionary<UserRole, string[]> RolePermissions()
        {
            return new Dictionary<UserRole, string[]>
            {
                [UserRole.Business] = new[]
                {
                    TermsAndConditions.Create,
                    TermsAndConditions.View,
                    Contact.Create,
                    Contact.View,
                    Reports.View,
                    Category.View,
                    City.View,
                    Branch.Create,
                    Branch.Edit,
                    Branch.View,
                    Branch.Delete,
                    Business.Edit,
                    Business.View,
                    Offer.Create,
                    Offer.Edit,
                    Offer.View,
                    Offer.Delete,
                    Invoice.Create,
                    Invoice.View,
                    Status.Create,
                    Status.Edit,
                    Status.View,
                    Status.Delete,
                },
                [UserRole.User] = new[]
                {
                    TermsAndConditions.View,
                    Contact.View,
                    Banner.View,
                    Category.View,
                    City.View,
                    Branch.View,
                    Business.View,
                    Offer.View,
                    User.View,
                    User.Delete,
                    User.Edit,
                    Notification.View,
                    Partner.View,
                    Support.Create,
                    Support.View,
                    Status.View,
                },
                [UserRole.Admin] = new[]
                {
                    TermsAndConditions.Create,
                    TermsAndConditions.View,
                    Contact.Create,
                    Contact.View,
                    Reports.View,
                    Banner.Create,
                    Banner.Edit,
                    Banner.View,
                    Banner.Delete,
                    Category.Create,
                    Category.Edit,
                    Category.View,
                    Category.Delete,
                    City.Create,
                    City.Edit,
                    City.View,
                    City.Delete,
                    Branch.Create,
                    Branch.Edit,
                    Branch.View,
                    Branch.Delete,
                    Business.Create,
                    Business.Edit,
                    Business.View,
                    Business.Delete,
                    Offer.Create,
                    Offer.Edit,
                    Offer.View,
                    Offer.Delete,
                    Contract.Create,
                    Contract.Edit,
                    Contract.View,
                    Contract.Delete,
                    User.Create,
                    User.Edit,
                    User.View,
                    User.Delete,
                    Invoice.Create,
                    Invoice.View, 
                    Notification.Create,
                    Notification.View,
                    Partner.Create,
                    Partner.Edit,
                    Partner.View,
                    Partner.Delete,
                    SubAdmin.Create,
                    SubAdmin.Edit,
                    SubAdmin.View,
                    SubAdmin.Delete,
                    Support.Create,
                    Support.Edit,
                    Support.View,
                    Support.Delete,
                    BusinessRequest.Edit,
                    BusinessRequest.View,
                    BusinessRequest.Delete,
                    Status.Create,
                    Status.Edit,
                    Status.View,
                    Status.Delete,
                }
            };
        }

        public static class TermsAndConditions
        {
            private const string Default = "TermsAndConditions";
            public const string Create = Default + ".Create";
            public const string View = Default + ".View";
        }

        public static class Contact
        {
            private const string Default = "Contact";
            public const string Create = Default + ".Create";
            public const string View = Default + ".View";
        }

        public static class Reports
        {
            private const string Default = "Reports";
            public const string View = Default + ".View";
        }

        public static class Banner
        {
            private const string Default = "Banner";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class City
        {
            private const string Default = "City";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Category
        {
            private const string Default = "Category";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Branch
        {
            private const string Default = "Branch";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Business
        {
            private const string Default = "Business";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Offer
        {
            private const string Default = "Offer";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Contract
        {
            private const string Default = "Contract";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class User
        {
            private const string Default = "User";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Invoice
        {
            private const string Default = "Invoice";
            public const string Create = Default + ".Create";
            public const string View = Default + ".View";
        }

        public static class Notification
        {
            private const string Default = "Notification";
            public const string Create = Default + ".Create";
            public const string View = Default + ".View";
        }

        public static class Partner
        {
            private const string Default = "Partner";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class SubAdmin
        {
            private const string Default = "SubAdmin";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Support
        {
            private const string Default = "Support";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class BusinessRequest
        {
            private const string Default = "BusinessRequest";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }

        public static class Status
        {
            private const string Default = "Status";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string View = Default + ".View";
            public const string Delete = Default + ".Delete";
        }
    }

    public record PermissionItem(string Id, string NameEn, string NameAr);
    public record PermissionGroup(string GroupEn, string GroupAr, IEnumerable<PermissionItem> Permissions);
}
