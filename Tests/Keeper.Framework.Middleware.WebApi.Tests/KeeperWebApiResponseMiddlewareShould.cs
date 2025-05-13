using System.Net.Http.Headers;
using Keeper.Framework.Collections;
using Keeper.Framework.Validations;
using Keeper.Masking;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Keeper.Framework.Middleware.WebApi.Tests
{
    public class KeeperWebApiResponseMiddlewareShould
    {
        [Fact]
        public async Task MaskSensitiveDataOnResponse()
        {
            using var builder = new TestServer(
                new WebHostBuilder()
                   .UseStartup(context => new Startup(context))
            );

            using var client = builder.CreateClient();

            var result = await client.GetAsync("/api/test");

            var resultBody = await result.Content.ReadAsStringAsync();

            var mockLogger = (MockLogger)builder.Services.GetRequiredService<ILogger>();

            var responseLogLine = mockLogger.LogLines.First(x => x.StartsWith("Api Response:"));

            Assert.Contains("\"Result\": {\"notMasked\":\"This should not be masked\",\"ssn\":\"******555\"}", responseLogLine);
        }


        [Fact]
        public async Task MaskCardAndAccount()
        {
            using var builder = new TestServer(
                new WebHostBuilder()
                   .UseStartup(context => new Startup(context))
            );

            using var client = builder.CreateClient();

            var request = new TestRequestMaskAndAccount()
            {
                Card = "1111-1111-1111-1111",
                Account = "12345678901"
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(requestJson, new MediaTypeHeaderValue("application/json"));

            var result = await client.PostAsync("/api/test/test_card", stringContent);

            var resultBody = await result.Content.ReadAsStringAsync();

            var mockLogger = (MockLogger)builder.Services.GetRequiredService<ILogger>();

            var requestLogLine = mockLogger.LogLines.First(x => x.StartsWith("Api Request:"));
            var responseLogLine = mockLogger.LogLines.First(x => x.StartsWith("Api Response:"));

            Assert.Contains("RequestContentBody: {\"Card\":\"************1111\",\"Account\":\"*******8901\"}", requestLogLine);
            Assert.Contains("\"Result\": {\"notMasked\":\"This should not be masked\",\"ssn\":\"******555\"}", responseLogLine);
        }

        [Fact]
        public async Task MaskSensitiveDataOnRequest()
        {
            using var builder = new TestServer(
                new WebHostBuilder()
                   .UseStartup(context => new Startup(context))
            );

            using var client = builder.CreateClient();

            var request = new TestRequest()
            {
                NotMasked = "This should not be masked",
                SSN = "555-55-5555"
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(requestJson, new MediaTypeHeaderValue("application/json"));

            var result = await client.PostAsync("/api/test", stringContent);

            var resultBody = await result.Content.ReadAsStringAsync();

            var mockLogger = (MockLogger)builder.Services.GetRequiredService<ILogger>();

            var requestLogLine = mockLogger.LogLines.First(x => x.StartsWith("Api Request:"));
            var responseLogLine = mockLogger.LogLines.First(x => x.StartsWith("Api Response:"));

            Assert.Contains("RequestContentBody: {\"NotMasked\":\"This should not be masked\",\"SSN\":\"******555\"}", requestLogLine);
            Assert.Contains("\"Result\": {\"notMasked\":\"This should not be masked\",\"ssn\":\"******555\"}", responseLogLine);
        }

        [Fact]
        public async Task FilterRequestLogging()
        {
            using var builder = new TestServer(
                new WebHostBuilder()
                   .ConfigureAppConfiguration(builder => 
                        builder.AddInMemoryCollection(
                           new Dictionary<string, string?>
                           {
                               [$"{Startup.RequestFilter}:0"] = "api/test",
                               [$"{Startup.RequestFilter}:1"] = "api/fake_filter"
                           }
                        )
                    )
                   .UseStartup(options =>
                   {
                       return new Startup(options);
                   })
            );

            using var client = builder.CreateClient();

            var request = new TestRequest()
            {
                NotMasked = "This should not be masked",
                SSN = "555-55-5555"
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(requestJson, new MediaTypeHeaderValue("application/json"));

            var result = await client.PostAsync("/api/test", stringContent);

            var resultBody = await result.Content.ReadAsStringAsync();

            var mockLogger = (MockLogger)builder.Services.GetRequiredService<ILogger>();

            Assert.DoesNotContain(mockLogger.LogLines, x => x.StartsWith("Api Request:"));
            var responseLogLine = mockLogger.LogLines.First(x => x.StartsWith("Api Response:"));

            Assert.Contains("\"Result\": {\"notMasked\":\"This should not be masked\",\"ssn\":\"******555\"}", responseLogLine);
        }

        [Fact]
        public async Task FilterResponseLogging()
        {
            using var builder = new TestServer(
                new WebHostBuilder()
                    .ConfigureAppConfiguration(builder =>
                        builder.AddInMemoryCollection(
                           new Dictionary<string, string?>
                           {
                               [$"{Startup.ResponseFilter}:0"] = "api/test",
                               [$"{Startup.ResponseFilter}:1"] = "api/fake_filter"
                           }
                        )
                    )
                   .UseStartup(context => new Startup(context))
            );

            using var client = builder.CreateClient();

            var request = new TestRequest()
            {
                NotMasked = "This should not be masked",
                SSN = "555-55-5555"
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(requestJson, new MediaTypeHeaderValue("application/json"));

            var result = await client.PostAsync("/api/test", stringContent);

            var resultBody = await result.Content.ReadAsStringAsync();

            var mockLogger = (MockLogger)builder.Services.GetRequiredService<ILogger>();

            var requestLogLine = mockLogger.LogLines.First(x => x.StartsWith("Api Request:"));
            Assert.DoesNotContain(mockLogger.LogLines, x => x.StartsWith("Api Response:"));

            Assert.Contains("RequestContentBody: {\"NotMasked\":\"This should not be masked\",\"SSN\":\"******555\"}", requestLogLine);
        }

        [Fact]
        public async Task FilterRequestAndResponseLogging()
        {
            using var builder = new TestServer(
                new WebHostBuilder()
                    .ConfigureAppConfiguration(builder =>
                        builder.AddInMemoryCollection(
                           new Dictionary<string, string?>
                           {
                               [$"{Startup.RequestResponseFilter}:0"] = "api/test",
                               [$"{Startup.RequestResponseFilter}:1"] = "api/fake_filter"
                           }
                        )
                    )
                   .UseStartup(context => new Startup(context))
            );

            using var client = builder.CreateClient();

            var request = new TestRequest()
            {
                NotMasked = "This should not be masked",
                SSN = "555-55-5555"
            };

            var requestJson = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(requestJson, new MediaTypeHeaderValue("application/json"));

            var result = await client.PostAsync("/api/test", stringContent);

            var resultBody = await result.Content.ReadAsStringAsync();

            var mockLogger = (MockLogger)builder.Services.GetRequiredService<ILogger>();

            Assert.DoesNotContain(mockLogger.LogLines, x => x.StartsWith("Api Request:"));
            Assert.DoesNotContain(mockLogger.LogLines, x => x.StartsWith("Api Response:"));
        }
    }

    class MockLogger : ILogger
    {
        public SynchronizedList<string> LogLines = new();

        public IDisposable? BeginScope<TState>(TState state) 
            where TState : notnull =>
                NullLogger.Instance.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var line = formatter(state, exception);

            LogLines.Add(line);
        }
    }

    class MockLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public SynchronizedList<string> LogLines => ((MockLogger)_logger).LogLines;

        public MockLogger(ILogger logger)
        {
            _logger = logger;
        }

        public IDisposable? BeginScope<TState>(TState state) 
            where TState : notnull =>
                _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) =>
            _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public class Startup
    {
        public const string RequestFilter = "RequestFilter";

        public const string ResponseFilter = "ResponseFilter";

        public const string RequestResponseFilter = "RequestResponseFilter";

        public Startup(WebHostBuilderContext context)
        {
            Context = context;
        }

        public WebHostBuilderContext Context { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger, MockLogger>();
            services.AddSingleton(typeof(ILogger<>), typeof(MockLogger<>));

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseKeeperWebApiResponseMiddleware(x =>
            {
                x.LoggingPolicy = LoggingPolicy.All;
                x.EnableApplicationState = true;

                var requestFilter = Context.Configuration.GetSection(RequestFilter).Get<List<string>>();
                if (requestFilter?.Count > 0)
                    x.ApiLogRequestFilters.AddRange(requestFilter);

                var responseFilter = Context.Configuration.GetSection(ResponseFilter).Get<List<string>>();
                if (responseFilter?.Count > 0)
                    x.ApiLogResponseFilters.AddRange(responseFilter);

                var requestResponseFilter = Context.Configuration.GetSection(RequestResponseFilter).Get<List<string>>();
                if (requestResponseFilter?.Count > 0)
                    x.ApiLogRequestResponseFilters.AddRange(requestResponseFilter);
            });

            app.UseCors();

            app.UseEndpoints(x => x.MapControllers());
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public Task<ActionResult<TestResponse>> TestGet(CancellationToken ct)
        {
            return Task.FromResult((ActionResult<TestResponse>)Ok(new TestResponse
            {
                NotMasked = "This should not be masked",
                SSN = "555-55-5555"
            }));
        }

        [HttpPost]
        public Task<TestResponse> TestPost([FromBody] TestRequest request, CancellationToken ct)
        {
            return Task.FromResult(new TestResponse
            {
                NotMasked = "This should not be masked",
                SSN = "555-55-5555"
            });
        }

        [HttpPost("test_card")]
        public Task<TestResponse> TestPost([FromBody] TestRequestMaskAndAccount request, CancellationToken ct)
        {
            return Task.FromResult(new TestResponse
            {
                NotMasked = "This should not be masked",
                SSN = "555-55-5555"
            });
        }        
    }

    public class TestRequestMaskAndAccount
    {
        [MaskCard]
        public string? Card { get; set; }

        [MaskAccount]
        public string? Account { get; set; }
    }

    public class TestRequest
    {
        public string? NotMasked { get; set; }

        [MaskSSN]
        public string? SSN { get; set; }
    }

    public class TestResponse
    {
        public string? NotMasked { get; set; }

        [MaskSSN]
        public string? SSN { get; set; }
    }
}