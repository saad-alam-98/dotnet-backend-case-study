using SaudiPlus.AutoMapper;

namespace SaudiPlus.Configurations
{
    public static class AutoMapperConfiguration
    {
        public static void ConfigureAutoMapperServies(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));
        }
    }
}
