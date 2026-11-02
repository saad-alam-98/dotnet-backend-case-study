using SaudiPlus.Enums;

namespace SaudiPlus.Interfaces
{
    public interface ILanguageService
    {
        string Language { get; set; }
        (Language code, Exception? error) GetLanguageCode();
    }
}
