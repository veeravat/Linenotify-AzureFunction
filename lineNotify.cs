using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;

namespace LineNotify
{
    public static class lineNotify
    {
        [FunctionName("lineNotify")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //Get request
            string message = req.Query["message"];
            string key = req.Query["key"];

            //Post request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            message = message ?? data?.message;
            key = key ?? data?.key;

            if (key != null && message != null)
            {
                //Debug log in Azure Portal
                log.LogInformation($"Notify with Key: {key}");
                log.LogInformation($"Notify message: {message}");

                //New Http Client
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
                Uri url = new Uri("https://notify-api.line.me/api/notify");

                //Set Request Data
                var dict = new Dictionary<string, string>();
                dict.Add("message", message.ToString());

                //Begin Request
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(dict) };
                var result = await client.SendAsync(request);

                return (ActionResult)new OkObjectResult($"Response: {result.StatusCode}");
            }
            else
            {
                return new BadRequestObjectResult("Please pass a API key and message on the query string or in the request body");
            }
        }
    }
}
