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

        [HttpGet("AvailableDb")]
        public List<string> Get()
        {
            return _sqLiteManager.GetAvailableDb();
        
        }
        
        [HttpGet("GetDbToAdd/{param:bool}")]
        public string Get(bool param)
        {
            return _sqLiteManager.GetDbToAdd();
        
        }
        
        [HttpGet("ErrorCount/{baseName}")]
        public int Get(string baseName)
        {
            return _sqLiteManager.GetErrorCount(baseName);
        
        }
        
        [HttpGet("OneError/{id:int}/{baseName}")]
        public ErrorEntity Get(int id, string baseName)
        {
            return _sqLiteManager.LoadError(id, baseName);
        
        }
        
        [HttpGet("AllErrors/{fullLoad}/{basename}")]
        public List<ErrorEntity> Get(string basename, string fullLoad = "True")
        {
            return _sqLiteManager.LoadErrors(basename);

        }
        
        [HttpPost]
        public void Post([FromBody]ErrorEntity error)
        {
           _sqLiteManager.AddErrorToDb(error);

        }
    }
}