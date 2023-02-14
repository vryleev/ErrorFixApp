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
        private SqliteService _sqLiteMsService;

        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger, SqliteService sqliteService)
        {
            _logger = logger;
            _sqLiteMsService = sqliteService;
        }

        [HttpGet("AvailableDb")]
        public List<string> Get()
        {
            return _sqLiteMsService.DbManager.GetAvailableDb();
        
        }
        
        [HttpGet("GetDbToAdd/{param:bool}")]
        public string Get(bool param)
        {
            return _sqLiteMsService.DbManager.GetDbToAdd();
        
        }
        
        [HttpGet("ErrorCount/{baseName}")]
        public int Get(string baseName)
        {
            return _sqLiteMsService.DbManager.GetErrorCount(baseName);
        
        }
        
        [HttpGet("OneError/{id:int}/{baseName}")]
        public ErrorEntity Get(int id, string baseName)
        {
            return _sqLiteMsService.DbManager.LoadError(id, baseName);
        
        }
        
        [HttpGet("AllErrors/{fullLoad}/{basename}")]
        public List<ErrorEntity> Get(string basename, string fullLoad = "True")
        {
            return _sqLiteMsService.DbManager.LoadErrors(basename);

        }
        
        [HttpPost]
        public void Post([FromBody]ErrorEntity error)
        {
            _sqLiteMsService.DbManager.AddErrorToDb(error);

        }
    }
}