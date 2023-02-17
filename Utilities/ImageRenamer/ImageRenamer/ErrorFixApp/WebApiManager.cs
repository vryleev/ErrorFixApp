using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ErrorDataLayer;
using ErrorFixApp.Properties;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace ErrorFixApp
{
    public class WebApiManager
    {
        private readonly HttpClient _client;
        
        private readonly Logger _log =
            new LoggerConfiguration().
                MinimumLevel.Debug().
                WriteTo.Console().
                WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day).CreateLogger();  

        public  WebApiManager()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            _client = new HttpClient();

            _client.Timeout = TimeSpan.FromSeconds(3);
            _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RemoteUrl"]);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

        }
        
        public async Task<string> AddError(ErrorEntity ee)
        {
            string resultContent = String.Empty;
            var json = JsonConvert.SerializeObject(ee);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage resp = await _client.PostAsync($"{_client.BaseAddress}Error", content);
                if (resp.IsSuccessStatusCode)
                {
                    resultContent = await resp.Content.ReadAsStringAsync();
                }
            }
            catch (TaskCanceledException e)
            {
                _log.Error(e.Message);
                var message = $"{Resources.ConnectionError}: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                MessageBox.Show(message);
            }

            return resultContent;
        }

        public async Task<ErrorEntity> GetError(int errorId, string baseName)
        {
            ErrorEntity error = null;
            try
            {
                HttpResponseMessage resp =
                    await _client.GetAsync($"{_client.BaseAddress}Error/OneError/{errorId}/{baseName}");
                if (resp.IsSuccessStatusCode)
                {
                    error = await resp.Content.ReadAsAsync<ErrorEntity>();
                }
            }
            catch (TaskCanceledException e)
            {
                _log.Error(e.Message);
                var message = $"{Resources.ConnectionError}: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                MessageBox.Show(message);
            }

            return error;
        }
        
        public async Task<List<ErrorEntity>> GetAllErrors(string baseName)
        {
            List<ErrorEntity> errorList = null;
            try
            {
                HttpResponseMessage resp =
                    await _client.GetAsync($"{_client.BaseAddress}Error/AllErrors/{Boolean.TrueString}/{baseName}");
                if (resp.IsSuccessStatusCode)
                {
                    errorList = await resp.Content.ReadAsAsync<List<ErrorEntity>>();
                }
            }
            catch (TaskCanceledException e)
            {
                _log.Error(e.Message);
                var message = $"{Resources.ConnectionError}: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                MessageBox.Show(message);
            }

            return errorList;
        }
        
        public async Task<List<string>> GetAvailableDb()
        {
            List<string> dbList = new List<string>();

            try
            {
                HttpResponseMessage resp = await _client.GetAsync($"{_client.BaseAddress}Error/AvailableDb");
                if (resp.IsSuccessStatusCode)
                {
                    dbList = await resp.Content.ReadAsAsync<List<string>>();
                }
            }
            catch (TaskCanceledException e)
            {
                _log.Error(e.Message);
                var message = $"{Resources.ConnectionError}: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                MessageBox.Show(message);
            }

            
            return dbList;
        }
        
        public async Task<string> GetDbToAdd()
        {
            string dbName = Resources.NameNotSet;

            try
            {
                HttpResponseMessage resp = await _client.GetAsync($"{_client.BaseAddress}Error/GetDbToAdd/{true}");
                if (resp.IsSuccessStatusCode)
                {
                    dbName = await resp.Content.ReadAsAsync<string>();
                }
            }
            catch (TaskCanceledException e)
            {
                _log.Error(e.Message);
                var message = $"{Resources.ConnectionError}: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                MessageBox.Show(message);
            }
            
            return dbName;
        }
        
        public async Task<int> GetErrorCount(string baseName)
        {
            int errorCount = -1;

            try
            {
                HttpResponseMessage resp = await _client.GetAsync($"{_client.BaseAddress}Error/ErrorCount/{baseName}");
                if (resp.IsSuccessStatusCode)
                {
                    errorCount = await resp.Content.ReadAsAsync<int>();
                }
            }
            catch (TaskCanceledException e)
            {
                _log.Error(e.Message);
            }

            
            return errorCount;
        }
    }
}