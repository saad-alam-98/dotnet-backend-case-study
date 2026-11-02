using Microsoft.AspNetCore.Mvc;

namespace SaudiPlus.Controllers
{
    public class BaseController<T> : ControllerBase
    {
        protected ILogger<T> Logger { get; set; }

        public BaseController(ILogger<T> logger)
        {
            this.Logger = logger;
        }
    }
}
