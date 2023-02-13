using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorDataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ErrorWebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorController : ControllerBase
    {
        private SqLiteManager _sqLiteManager = new SqLiteManager();

        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public List<ErrorEntity> Get()
        {
            return _sqLiteManager.LoadErrors(
                "D:/Yandex.Repository/GitRepository/Utilities/ImageRenamer/ImageRenamer/ErrorWebApplication/bin/Debug/net5.0/RouteErrors/26_01_23_RouteErrors.db3");

        }
    }
}