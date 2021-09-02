using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
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
        private const string COMPONENT_NAME = "PTT4PBL";
        private readonly string _sonarqubeHostURL = "http://192.168.1.250:9000/";
        private Repo _repo1;
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
            _repo1 = new Repo
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
            _dbContext.Repositories.Add(_repo1);
            _dbContext.Repositories.Add(_repo2);
            _dbContext.SaveChanges();
        }

        private string getResponseOfOverall()
        {
            const string COMPONENT = "PMS_109";
            List<Measure> measures = new List<Measure>()
            {
                new Measure
                {
                    metric = "bugs",
                    bestValue = true,
                    component = COMPONENT,
                    value = "0"
                },
                new Measure
                {
                    metric = "code_smells",
                    bestValue = false,
                    component = COMPONENT,
                    value = "51"
                },
                new Measure
                {
                    metric = "coverage",
                    bestValue = false,
                    component = COMPONENT,
                    value = "88.5"
                },
                new Measure
                {
                    metric = "duplicated_lines_density",
                    bestValue = true,
                    component = COMPONENT,
                    value = "0.0"
                },
                new Measure
                {
                    metric = "vulnerabilities",
                    bestValue = true,
                    component = COMPONENT,
                    value = "0"
                },
            };
            SonarqubeInfoDto sonarqubeInfoDto = new SonarqubeInfoDto
            {
                measures = measures,
                projectName = COMPONENT
            };

            return JsonConvert.SerializeObject(sonarqubeInfoDto);
        }

        private string getResponseOfCodeSmell()
        {
            const int TOTAL = 501;
            List<Issues> issues = new List<Issues>
            {
                new Issues
                {
                    key = "01fc972e-2a3c-433e-bcae-0bd7f88f5123",
                    component = COMPONENT_NAME,
                    line = 81,
                    message = "'System.Exception' should not be thrown by user code.",
                    severity = "MINOR"
                },
                new Issues
                {
                    key = "01fc972e-2a3c-433e-bcae-0bd7f88f5123",
                    component = COMPONENT_NAME,
                    line = 81,
                    message = "'System.Exception' should not be thrown by user code.",
                    severity = "MINOR"
                },
            };
            CodeSmellDataDto codeSmellDataDto = new CodeSmellDataDto
            {
                total = TOTAL,
                issues = issues
            };

            string responseData = JsonConvert.SerializeObject(codeSmellDataDto);

            return responseData;
        }

        private HttpClient CreateMockClient()
        {
            string apiURL = "api/measures/search?";
            string query = "&metricKeys=bugs,vulnerabilities,code_smells,duplicated_lines_density,coverage";
            var mockHttp = new MockHttpMessageHandler();
            string responseData = getResponseOfOverall();

            mockHttp.When(HttpMethod.Get, $"{_sonarqubeHostURL}{apiURL}projectKeys={_repo1.ProjectKey}{query}")
                .Respond("application/json", responseData);

            const int PAGE_SIZE = 500;
            apiURL = "api/issues/search?";
            query = $"&componentKeys={_repo1.ProjectKey}&s=FILE_LINE&resolved=false&ps={PAGE_SIZE}&organization=default-organization&facets=severities%2Ctypes&types=CODE_SMELL";
            responseData = getResponseOfCodeSmell();

            mockHttp.When(HttpMethod.Get, $"{_sonarqubeHostURL}{apiURL}projectKeys={_repo1.ProjectKey}{query}")
                .Respond("appication/json", responseData);
            mockHttp.When(HttpMethod.Get, $"{_sonarqubeHostURL}{apiURL}projectKeys={_repo1.ProjectKey}{query}&p=2")
                .Respond("appication/json", responseData);

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
            var response = await _sonarqubeService.GetSonarqubeInfoAsync(_repo1.ID);
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
            Assert.True(await _sonarqubeService.IsHaveSonarqube(_repo1.ID));
        }

        [Fact]
        public async void TestIsHaveSonarqubeShouldReturnFalse()
        {
            Assert.False(await _sonarqubeService.IsHaveSonarqube(_repo2.ID));
        }

        [Fact]
        public async void TestGetSonarqubeCodeSmellAsync()
        {
            var response = await _sonarqubeService.GetSonarqubeCodeSmellAsync(_repo1.ID);
            var issues = response[COMPONENT_NAME];

            Assert.Equal("01fc972e-2a3c-433e-bcae-0bd7f88f5123", issues[0].key);
            Assert.Equal("MINOR", issues[0].severity);
            Assert.Equal(COMPONENT_NAME, issues[0].component);
            Assert.Equal(81, issues[0].line);
            Assert.Equal("'System.Exception' should not be thrown by user code.", issues[0].message);

            Assert.Equal("01fc972e-2a3c-433e-bcae-0bd7f88f5123", issues[1].key);
            Assert.Equal("MINOR", issues[1].severity);
            Assert.Equal(COMPONENT_NAME, issues[1].component);
            Assert.Equal(81, issues[1].line);
            Assert.Equal("'System.Exception' should not be thrown by user code.", issues[1].message);
        }
    }
}
