using FluentAssertions;
using Infrastructure.OCR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Infrastructure.Tests.OCR
{
    public class CustomMindeeOcrServiceTests
    {
        private readonly ILogger<CustomMindeeOcrService> _loggerMock = Mock.Of<ILogger<CustomMindeeOcrService>>();
        private readonly IConfiguration _config;

        public CustomMindeeOcrServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string>
        {
            {"Mindee:ApiKey", "fake-api-key"},
            {"Mindee:PassportEndpoint", "https://api.fake.com/passport"},
            {"Mindee:VehicleEndpoint", "https://api.fake.com/vehicle"}
        };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        [Fact]
        public async Task ExtractPassportDataAsync_Should_Return_Result_When_Successful()
        {
            // Arrange
            var mockedInitialResponse = new JObject
            {
                ["job"] = new JObject
                {
                    ["polling_url"] = "https://api.fake.com/polling"
                }
            }.ToString();

            var mockedPollingResponse = new JObject
            {
                ["job"] = new JObject
                {
                    ["status"] = "completed"
                }
            }.ToString();

            var handler = new DelegatingHandlerStub((request, cancellationToken) =>
            {
                if (request.RequestUri!.ToString().Contains("passport"))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)
                    {
                        Content = new StringContent(mockedInitialResponse, Encoding.UTF8, "application/json")
                    });
                }
                else if (request.RequestUri.ToString().Contains("polling"))
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(mockedPollingResponse, Encoding.UTF8, "application/json")
                    });
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            });

            var httpClient = new HttpClient(handler);
            var service = new CustomMindeeOcrService(httpClient, _config, _loggerMock);
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            // Act
            var result = await service.ExtractPassportDataAsync(stream, "file.jpg");

            // Assert
            result.Should().Contain("completed");
        }
    }

}
