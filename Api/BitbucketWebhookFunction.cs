using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BlazorApp.Shared;
using Newtonsoft.Json;

namespace Api
{
    public class BitbucketWebhookFunction
    {
        private readonly ILogger _logger;

        public BitbucketWebhookFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BitbucketWebhookFunction>();
        }

        [Function("BitbucketWebhook")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (requestBody != null)
            {
                dynamic payload = JsonConvert.DeserializeObject(requestBody);

                var eventKey = req.Headers.TryGetValues("X-Event-Key", out var values)
                    ? string.Join(",", values)
                    : "unknown";

                _logger.LogInformation("Bitbucket Webhook Triggered");
                _logger.LogInformation("Event Key: {EventKey}", eventKey);
                _logger.LogInformation("Payload: {Payload}", requestBody);

                WebhookLogStore.Add(
                    $"[{payload.commit_status.created_on}]\n" +
                    $"Status: {payload.commit_status.state}\n" +
                    $"Repo: {payload.repository.name}\n" +
                    $"Repo: {payload.repository.full_name}\n" +
                    $"Branch: {payload.commit_status.refname}\n" +
                    $"Creator: {payload.commit.author.raw}\n" +
                    $"Creator: {payload.commit.author.user.display_name}\n" +
                    $"Identifier: {payload.commit_status.name}\n"
                    );
            }

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync($"Webhook received");
            return response;
        }

        [Function("GetWebhookLogs")]
        public static async Task<HttpResponseData> GetLogs(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "webhook/logs")] HttpRequestData req)
        {
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            var logs = WebhookLogStore.GetAll();
            await response.WriteAsJsonAsync(logs);
            return response;
        }
    }
}
