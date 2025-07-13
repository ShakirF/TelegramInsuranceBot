using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Infrastructure.OCR
{
    public class CustomMindeeOcrService : ICustomOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly string _passportEndpoint;
        private readonly string _vehicleEndpoint;
        private readonly string _apiKey;
        private readonly ILogger<CustomMindeeOcrService> _logger;

        public CustomMindeeOcrService(HttpClient httpClient, IConfiguration config, ILogger<CustomMindeeOcrService> logger)
        {
            _httpClient = httpClient;
            _apiKey = config["Mindee:ApiKey"]!;
            _passportEndpoint = config["Mindee:PassportEndpoint"]!;
            _vehicleEndpoint = config["Mindee:VehicleEndpoint"]!;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", _apiKey);
        }

        public Task<string> ExtractPassportDataAsync(Stream stream, string fileName, CancellationToken ct = default)
            => SendToMindeeAsync(_passportEndpoint, stream, fileName, ct);

        public Task<string> ExtractVehicleDataAsync(Stream stream, string fileName, CancellationToken ct = default)
            => SendToMindeeAsync(_vehicleEndpoint, stream, fileName, ct);

        private async Task<string> SendToMindeeAsync(string url, Stream stream, string fileName, CancellationToken ct)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(fileContent, "document", fileName);

            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var initialContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("Raw initial response: {Body}", initialContent);


            var job = JObject.Parse(initialContent)["job"];
            var pollingUrl = job?["polling_url"]?.ToString();

            if (string.IsNullOrEmpty(pollingUrl))
                throw new Exception("Polling URL was not provided by Mindee API.");

            pollingUrl = pollingUrl
               .Replace("/passport_parser/1/", "/passport_parser/v1/")
               .Replace("/vehicle_parser/1/", "/vehicle_parser/v1/");

            string resultContent = string.Empty;
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1500, ct);

                var pollResponse = await _httpClient.GetAsync(pollingUrl, ct);
        
                if ((int)pollResponse.StatusCode >= 300 && (int)pollResponse.StatusCode < 400
                    && pollResponse.Headers.Location != null)
                {
                    var redirectUri = pollResponse.Headers.Location.IsAbsoluteUri
                        ? pollResponse.Headers.Location.ToString()
                        : new Uri(new Uri(pollingUrl), pollResponse.Headers.Location).ToString();

                    pollResponse = await _httpClient.GetAsync(redirectUri, ct);
                }

                pollResponse.EnsureSuccessStatusCode();


                resultContent = await pollResponse.Content.ReadAsStringAsync(ct);
                _logger.LogDebug("Pool initial response: {Body}", resultContent);
                var status = JObject.Parse(resultContent)["job"]?["status"]?.ToString();

                if (status == "completed")
                    return resultContent;
            }

            throw new TimeoutException("OCR processing did not complete in the expected time window.");
        }

        private string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }


}
