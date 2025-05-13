using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Keeper.Framework.Extensions.Http.Test
{
    public class HttpClientExtensionsTests
    {
        [Fact]
        public async Task Get_Positive()
        {
            // Arrange
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com")
            };

            IRestRequest request = new RestRequest
            {
                HttpMethod = HttpMethod.Get,
                Resource = @"/repos/symfony/symfony/contributors",
                Headers = { { "User-Agent", "C# console program" } },
                Encoding = Encoding.ASCII
            };

            // Act
            var resp = await httpClient.ExecuteAsync<List<Contributor>>(request);

            // Asserts
            AssertRestResponse(resp, request, httpClient, HttpStatusCode.OK);
        }

        [Fact]
        public void SerializeRequestProperly()
        {
            var person = new Person("Daniel", "Programer");
            var request = new RestRequest
            {
                HttpMethod = HttpMethod.Post,
                Resource = "post",
                StringContent = JsonConvert.SerializeObject(person)
            };

            request.AddAuthorizationHeader(new AuthenticationHeaderValue("Bearer", "faketoken"));
            request.Headers.Add("Header1", "Value1");
            request.Headers.Add("Header2", "Value2");

            var serialized = request.ToString();

            Assert.Equal(@"POST post

Header1: Value1
Header2: Value2
authorization: Bearer fake*****

{""Name"":""Daniel"",""Occupation"":""Programer""}", serialized);
        }

        [Fact]
        public void SerializeResponseProperly()
        {
            var response = new RestResponse
            {
                Headers = [],
                StatusCode = HttpStatusCode.OK
            };

            response.Headers.Add("Connection", "keep-alive");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");

            response.Content = @"{""MyContentKey"":""MyContent""}";

            var serialized = response.ToString();

            Assert.Equal(@"Status: OK

Connection: keep-alive
Access-Control-Allow-Origin: *
Access-Control-Allow-Credentials: true

{""MyContentKey"":""MyContent""}", serialized);

            response.ErrorException = new Exception("Root exception", new("ChildException"));

            serialized = response.ToString();

            Assert.Equal(@"Status: OK

Connection: keep-alive
Access-Control-Allow-Origin: *
Access-Control-Allow-Credentials: true

{""MyContentKey"":""MyContent""}

ErrorException: System.Exception: Root exception
 ---> System.Exception: ChildException
   --- End of inner exception stack trace ---", serialized);
        }

        [Fact]
        public async Task Post_Positive()
        {
            // Arrange
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://httpbin.org")
            };
            var person = new Person("Daniel", "Programer");
            IRestRequest request = new RestRequest
            {
                HttpMethod = HttpMethod.Post,
                Resource = "post",
                StringContent = JsonConvert.SerializeObject(person)
            };

            // Act
            var resp = await httpClient.ExecuteAsync<HttpbinRootobject>(request);

            // Asserts
            AssertRestResponse(resp, request, httpClient, HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_WithTBody_Positive()
        {
            // Arrange
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://httpbin.org")
            };
            var person = new Person("Daniel", "Programer");
            IRestRequest<Person> request = new RestRequest<Person>(person, "post");

            // Act
            var resp = await httpClient.ExecuteAsync<HttpbinRootobject, Person>(request);

            // Asserts
            AssertRestResponse(resp, request, httpClient, HttpStatusCode.OK);
        }

        private static void AssertRestResponse<T>(IRestResponse<T> resp, IRestRequest request, HttpClient httpClient, HttpStatusCode expectedStatusCode)
        {
            Assert.Equal(resp.StatusCode, expectedStatusCode);
            Assert.Equal(request.HttpMethod, resp.Request!.HttpMethod);
            Assert.Equal(new Uri(httpClient.BaseAddress!, request.Resource).AbsolutePath,
                resp.Request.RequestUri.AbsolutePath);
            Assert.True((int)expectedStatusCode >= 200 && (int)expectedStatusCode < 300 ? resp.IsSuccessful : !resp.IsSuccessful);
            if (resp.IsSuccessful)
            {
                Assert.NotNull(resp.Data);
                Assert.IsType<T>(resp.Data);
                Assert.Null(resp.ErrorException);
                Assert.Null(resp.ErrorMessage);
            }
            else
            {
                Assert.Null(resp.Data);
                Assert.NotNull(resp.ErrorException);
                Assert.NotNull(resp.ErrorMessage);
            }

            Assert.NotNull(resp.ContentType);
            Assert.NotNull(resp.Headers);
            Assert.True(resp.Headers.HasKeys());
            Assert.NotNull(resp.Request);
            Assert.NotNull(resp.StatusDescription);
            Assert.Equal(resp.Request.Content, request.Content);
            Assert.Equal(resp.Request.Encoding, request.Encoding);
            Assert.NotNull(resp.Headers);
            Assert.NotNull(resp.Request.Headers);
            Assert.Equal(request.HasHeaders, resp.Request.HasHeaders);
            Assert.Equal(request.Headers.AllKeys?.Length, resp.Request.Headers.AllKeys?.Length);
            foreach (var expectedKey in request.Headers.AllKeys!)
            {
                Assert.Contains(expectedKey, resp.Request.Headers.AllKeys!);
                Assert.Equal(request.Headers.GetValues(expectedKey)?.Length,
                    resp.Request.Headers.GetValues(expectedKey)?.Length);
                if (request.Headers.GetValues(expectedKey) != null)
                {
                    foreach (var headerVal in request.Headers.GetValues(expectedKey)!)
                    {
                        Assert.Contains(headerVal, resp.Request.Headers.GetValues(expectedKey)!);
                    }
                }
            }

            Assert.Equal(request.Content?.Length, resp.Request.Content?.Length);
            if (request.Content != null)
            {
                for (int i = 0; i < request.Content.Length; i++)
                {
                    Assert.Equal(request.Content[i], resp.Request.Content![i]);
                }
            }

            Assert.Equal(request.StringContent, resp.Request.StringContent);
        }
    }

    class Contributor
    {
        public string current_user_url { get; set; } = default!;
        public string current_user_authorizations_html_url { get; set; } = default!;
        public string authorizations_url { get; set; } = default!;
        public string code_search_url { get; set; } = default!;
        public string commit_search_url { get; set; } = default!;
        public string emails_url { get; set; } = default!;
        public string emojis_url { get; set; } = default!;
        public string events_url { get; set; } = default!;
        public string feeds_url { get; set; } = default!;
        public string followers_url { get; set; } = default!;
        public string following_url { get; set; } = default!;
        public string gists_url { get; set; } = default!;
        public string hub_url { get; set; } = default!;
        public string issue_search_url { get; set; } = default!;
        public string issues_url { get; set; } = default!;
        public string keys_url { get; set; } = default!;
        public string label_search_url { get; set; } = default!;
        public string notifications_url { get; set; } = default!;
        public string organization_url { get; set; } = default!;
        public string organization_repositories_url { get; set; } = default!;
        public string organization_teams_url { get; set; } = default!;
        public string public_gists_url { get; set; } = default!;
        public string rate_limit_url { get; set; } = default!;
        public string repository_url { get; set; } = default!;
        public string repository_search_url { get; set; } = default!;
        public string current_user_repositories_url { get; set; } = default!;
        public string starred_url { get; set; } = default!;
        public string starred_gists_url { get; set; } = default!;
        public string user_url { get; set; } = default!;
        public string user_organizations_url { get; set; } = default!;
        public string user_repositories_url { get; set; } = default!;
        public string user_search_url { get; set; } = default!;
    }

    class HttpbinRootobject
    {
        public Args args { get; set; } = default!;
        public string data { get; set; } = default!;
        public Files files { get; set; } = default!;
        public Form form { get; set; } = default!;
        public Headers headers { get; set; } = default!;
        public Person json { get; set; } = default!;
        public string origin { get; set; } = default!;
        public string url { get; set; } = default!;
    }

    class Args
    {
    }

    class Files
    {
    }

    class Form
    {
    }

    class Headers
    {
        public string ContentLength { get; set; } = default!;
        public string Host { get; set; } = default!;
        public string XAmznTraceId { get; set; } = default!;
    }

    class Person
    {
        public Person(string name, string occupation)
        {
            Name = name;
            Occupation = occupation;
        }

        public string Name { get; set; }

        public string Occupation { get; set; }
    }
}
