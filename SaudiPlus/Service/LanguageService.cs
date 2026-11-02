using SaudiPlus.Enums;
using SaudiPlus.Interfaces;

namespace SaudiPlus.Service
{
    public class LanguageService : ILanguageService
    {
        public string Language { get; set; } = "ar";

        public (Language, Exception?) GetLanguageCode()
        {
            Exception? exception = null;
            if (!Enum.TryParse<Language>(Language, true, out var languageCode))
            {
                exception = new Exception
                (
                   "Invalid Accept-Language header value (use en or ar)"
                );
            }

            return (languageCode, exception);
        }
    }
}
