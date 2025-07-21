using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

            var eventKey = req.Headers.TryGetValues("X-Event-Key", out var values)
                ? string.Join(",", values)
                : "unknown";

            _logger.LogInformation("Bitbucket Webhook Triggered");
            _logger.LogInformation("Event Key: {EventKey}", eventKey);
            _logger.LogInformation("Payload: {Payload}", requestBody);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Webhook received");
            return response;
        }
    }
}
