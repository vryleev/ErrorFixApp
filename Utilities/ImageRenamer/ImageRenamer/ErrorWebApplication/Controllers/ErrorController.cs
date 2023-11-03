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
            //return _sqLiteMsService.DbManager.GetAvailableDb();
            return SqLiteManager.GetAvailableDb();
        
        }
        
        [HttpGet("GetDbToAdd/{param:bool}")]
        public string Get(bool param)
        {
            //return _sqLiteMsService.DbManager.GetDbToAdd();
            return SqLiteManager.GetDbToAdd();
        
        }
        
        [HttpGet("ErrorCount/{baseName}")]
        public int Get(string baseName)
        {
            //return _sqLiteMsService.DbManager.GetErrorCount(baseName);
            return SqLiteManager.GetErrorCount(baseName);
        
        }
        
        [HttpGet("OneError/{id:int}/{baseName}")]
        public ErrorEntity Get(int id, string baseName)
        {
            //return _sqLiteMsService.DbManager.LoadError(id, baseName);
            return SqLiteManager.LoadError(id, baseName);
        
        }
        
        [HttpGet("AllErrors/{fullLoad}/{basename}")]
        public List<ErrorEntity> Get(string basename, string fullLoad = "True")
        {
            //return _sqLiteMsService.DbManager.LoadErrors(basename);
            return SqLiteManager.LoadErrors(basename);

        }
        
        [HttpPost]
        public void Post([FromBody]ErrorEntity error)
        {
            //_sqLiteMsService.DbManager.AddErrorToDb(error);
            SqLiteManager.AddErrorToDb(error);

        }

        [HttpPut]
        public void Put([FromBody] int id, string baseName)
        {
            //_sqLiteMsService.DbManager.DeleteErrorFromDb(id, baseName);
            SqLiteManager.DeleteErrorFromDb(id, baseName);
        }
    }
}