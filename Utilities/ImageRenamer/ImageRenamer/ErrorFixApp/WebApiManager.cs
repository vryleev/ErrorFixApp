using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ErrorDataLayer;
using Newtonsoft.Json;

namespace ErrorFixApp
{
    public class WebApiManager
    {
        private HttpClient client;

        public  WebApiManager()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(3);
            client.BaseAddress = new Uri("http://localhost:7000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

        }
        
        public async Task<string> AddError(ErrorEntity ee)
        {
            string resultContent = String.Empty;
            var json = JsonConvert.SerializeObject(ee);
            var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage resp = await client.PostAsync($"{client.BaseAddress}Error", content);
                if (resp.IsSuccessStatusCode)
                {
                    resultContent = await resp.Content.ReadAsStringAsync();
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Соединение с {ConfigurationManager.AppSettings["RemoteUrl"]} не установлено");
            }

            return resultContent;
        }

        public async Task<ErrorEntity> GetError(int errorId, string baseName)
        {
            ErrorEntity error = null;
            try
            {
                HttpResponseMessage resp =
                    await client.GetAsync($"{client.BaseAddress}Error/OneError/{errorId}/{baseName}");
                if (resp.IsSuccessStatusCode)
                {
                    error = await resp.Content.ReadAsAsync<ErrorEntity>();
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Соединение с {ConfigurationManager.AppSettings["RemoteUrl"]} не установлено");
            }

            return error;
        }
        
        public async Task<List<ErrorEntity>> GetAllErrors(string baseName)
        {
            List<ErrorEntity> errorList = null;
            try
            {
                HttpResponseMessage resp =
                    await client.GetAsync($"{client.BaseAddress}Error/AllErrors/{Boolean.TrueString}/{baseName}");
                if (resp.IsSuccessStatusCode)
                {
                    errorList = await resp.Content.ReadAsAsync<List<ErrorEntity>>();
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Соединение с {ConfigurationManager.AppSettings["RemoteUrl"]} не установлено");
            }

            return errorList;
        }
        
        public async Task<List<string>> GetAvailableDb()
        {
            List<string> dbList = new List<string> {"Список БД пуст"};

            try
            {
                HttpResponseMessage resp = await client.GetAsync($"{client.BaseAddress}Error/AvailableDb");
                if (resp.IsSuccessStatusCode)
                {
                    dbList = await resp.Content.ReadAsAsync<List<string>>();
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Соединение с {ConfigurationManager.AppSettings["RemoteUrl"]} не установлено");
            }

            
            return dbList;
        }
        
        public async Task<string> GetDbToAdd()
        {
            string dbName = "Имя не задано";

            try
            {
                HttpResponseMessage resp = await client.GetAsync($"{client.BaseAddress}Error/GetDbToAdd/{true}");
                if (resp.IsSuccessStatusCode)
                {
                    dbName = await resp.Content.ReadAsAsync<string>();
                }
            }
            catch (TaskCanceledException e)
            {
                MessageBox.Show($"Соединение с {ConfigurationManager.AppSettings["RemoteUrl"]} не установлено");
            }
            
            return dbName;
        }
        
        public async Task<int> GetErrorCount(string baseName)
        {
            int errorCount = -1;

            try
            {
                HttpResponseMessage resp = await client.GetAsync($"{client.BaseAddress}Error/ErrorCount/{baseName}");
                if (resp.IsSuccessStatusCode)
                {
                    errorCount = await resp.Content.ReadAsAsync<int>();
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e);
            }

            
            return errorCount;
        }
    }
}