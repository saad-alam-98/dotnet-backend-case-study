using AutoMapper;
using SaudiPlus.Interfaces;

namespace SaudiPlus.Service
{
    public class BaseService<T>
    {
        protected IMapper _mapper { get; set; }
        protected ILogger<T> _logger { get; set; }
        protected ILanguageService _languageService { get; set; }

        public BaseService(
            IMapper mapper,
            ILogger<T> logger,
            ILanguageService languageService
        )
        {
            _mapper = mapper;
            _logger = logger;
            _languageService = languageService;
        }
    }
}
