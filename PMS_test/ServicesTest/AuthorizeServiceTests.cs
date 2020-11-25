using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using project_manage_system_backend;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using RichardSzalay.MockHttp;
using System;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PMS_test
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class AuthorizeServiceTests
    {
        private readonly PMSContext _dbContext;
        private readonly HttpClient _client;
        private readonly AuthorizeService _authorizeService;
        private readonly UserService _userService;
        private readonly WebApplicationFactory<Startup> factory = new WebApplicationFactory<Startup>();

        public AuthorizeServiceTests()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
                                            .UseSqlite(CreateInMemoryDatabase())
                                            .Options);
            _dbContext.Database.EnsureCreated();

            var mockHttp = new MockHttpMessageHandler();

            var _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            string clientId = _configuration.GetValue<string>("githubConfig:client_id");
            string clientSecret = _configuration.GetValue<string>("githubConfig:client_secret");

            string content = "{\"client_id\":\"" + clientId + "\",\"client_secret\":\"" + clientSecret + "\",\"code\":\"testcode\"}";
            mockHttp.When(HttpMethod.Post, "https://github.com/login/oauth/access_token")
                    .WithContent(content)
                    .Respond("application/text", 
                    "access_token=token&scope=&token_type=bearer");

            mockHttp.When(HttpMethod.Get, "https://api.github.com/user")
                    .Respond("application/json",
                    "{\"login\":\"testuser\",\"avatar_url\":\"test\"}");

            _client = mockHttp.ToHttpClient();
            _authorizeService = new AuthorizeService(_dbContext, _configuration, new JwtHelper(_configuration), _client);
            _userService = new UserService(_dbContext);
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }

        [Fact]
        public async Task TestFirstLogin()
        {
            AuthorizeDto token = await _authorizeService.AuthenticateGithub(new RequestGithubLoginDto { Code = "testcode" });
            Assert.True(token != null);
            Assert.True(_userService.CheckUserExist("github_testuser"));
        }

        [Fact]
        public async Task TestSecondLogin()
        {
            AuthorizeDto token = await _authorizeService.AuthenticateGithub(new RequestGithubLoginDto { Code = "testcode" });
            Assert.True(token != null);
            Assert.True(_userService.CheckUserExist("github_testuser"));
        }
    }
}
