using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using RichardSzalay.MockHttp;
using System.Data.Common;
using System.Net.Http;
using Xunit;

namespace PMS_test.ServicesTest
{

    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class SonarqubeServierTest
    {

        private const string _owner = "WeedChen";
        private const string _name = "AutoPlaneCup";
        private readonly PMSContext _dbContext;
        private readonly HttpClient _client;
        private readonly SonarqubeService _sonarqubeService;
        private readonly string _sonarqubeHostURL = "http://192.168.1.250:9000/";
        private readonly string apiURL = "api/measures/search?";
        private readonly string query = "&metricKeys=bugs,vulnerabilities,code_smells,duplicated_lines_density,coverage";
        private Repo _reop1;
        private Repo _repo2;

        public SonarqubeServierTest()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
               .UseSqlite(CreateInMemoryDatabase()).Options);
            _dbContext.Database.EnsureCreated();
            InitialDatabase();
            _client = CreateMockClient();
            _sonarqubeService = new SonarqubeService(_dbContext, _client);
        }

        private void InitialDatabase()
        {
            _reop1 = new Repo
            {
                Name = _name,
                Owner = _owner,
                Url = "https://github.com/" + _owner + "/" + _name + "",
                IsSonarqube = true,
                ProjectKey = "PMS_109",
                AccountColonPw = "109598028",
                SonarqubeUrl = _sonarqubeHostURL
            };

            _repo2 = new Repo
            {
                Name = _name,
                Owner = _owner,
                Url = "https://github.com/" + _owner + "/" + _name + "",
                IsSonarqube = false
            };
            _dbContext.Repositories.Add(_reop1);
            _dbContext.Repositories.Add(_repo2);
            _dbContext.SaveChanges();
        }

        private HttpClient CreateMockClient()
        {
            var mockHttp = new MockHttpMessageHandler();
            string responseData = "{\"measures\":" +
                "[{\"metric\":\"bugs\",\"value\":\"0\",\"component\":\"PMS_109\",\"bestValue\":true}," +
                "{\"metric\":\"code_smells\",\"value\":\"51\",\"component\":\"PMS_109\",\"bestValue\":false}," +
                "{\"metric\":\"coverage\",\"value\":\"88.5\",\"component\":\"PMS_109\",\"bestValue\":false}," +
                "{\"metric\":\"duplicated_lines_density\",\"value\":\"0.0\",\"component\":\"PMS_109\",\"bestValue\":true}," +
                "{\"metric\":\"vulnerabilities\",\"value\":\"0\",\"component\":\"PMS_109\",\"bestValue\":true}]}";

            mockHttp.When(HttpMethod.Get, $"{_sonarqubeHostURL}{apiURL}projectKeys={_reop1.ProjectKey}{query}").Respond("application/json", responseData);

            return mockHttp.ToHttpClient();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }

        [Fact]
        public async void TestGetSonarqubeInfoAsync()
        {
            var response = await _sonarqubeService.GetSonarqubeInfoAsync(_reop1.ID);
            Assert.Equal("PMS_109", response.projectName);
            Assert.Equal(5, response.measures.Count);

            Assert.Equal("bugs", response.measures[0].metric);
            Assert.True(response.measures[0].bestValue);
            Assert.Equal("PMS_109", response.measures[0].component);
            Assert.Equal("0", response.measures[0].value);

            Assert.Equal("code_smells", response.measures[1].metric);
            Assert.False(response.measures[1].bestValue);
            Assert.Equal("PMS_109", response.measures[1].component);
            Assert.Equal("51", response.measures[1].value);

            Assert.Equal("coverage", response.measures[2].metric);
            Assert.False(response.measures[2].bestValue);
            Assert.Equal("PMS_109", response.measures[2].component);
            Assert.Equal("88.5", response.measures[2].value);

            Assert.Equal("duplicated_lines_density", response.measures[3].metric);
            Assert.True(response.measures[3].bestValue);
            Assert.Equal("PMS_109", response.measures[3].component);
            Assert.Equal("0.0", response.measures[3].value);

            Assert.Equal("vulnerabilities", response.measures[4].metric);
            Assert.True(response.measures[4].bestValue);
            Assert.Equal("PMS_109", response.measures[4].component);
            Assert.Equal("0", response.measures[4].value);
        }

        [Fact]
        public async void TestIsHaveSonarqubeShouldReturnTrue()
        {
            Assert.True(await _sonarqubeService.IsHaveSonarqube(_reop1.ID));
        }

        [Fact]
        public async void TestIsHaveSonarqubeShouldReturnFalse()
        {
            Assert.False(await _sonarqubeService.IsHaveSonarqube(_repo2.ID));
        }
    }
}
