using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Dtos.Gitlab;
using project_manage_system_backend.Models;
using project_manage_system_backend.RepoInfo;
using project_manage_system_backend.Shares;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace PMS_test.ServicesTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class GitlabRepoInfoTests
    {
        private readonly PMSContext _dbContext;
        private readonly HttpClient _client;
        private const string token = "access_token=nKswk3SkyZVyMR_q9KJ4";
        private const string repoId = "0"; 
        private readonly GitlabInfo _gitlabInfo;
        private readonly Repo _repo;
        public GitlabRepoInfoTests()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
               .UseSqlite(CreateInMemoryDatabase())
               .Options);
            _dbContext.Database.EnsureCreated();
            _client = CreateMockClient();
            _gitlabInfo = new GitlabInfo(token, _client);
            _repo = new Repo { RepoId = repoId };
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }

        private HttpClient CreateMockClient()
        {
            var mockHttp = new MockHttpMessageHandler();
            List<RequestCommitsDto> responseOfCommit = new List<RequestCommitsDto>();
            responseOfCommit.Add(new RequestCommitsDto
            {
                committed_date = new DateTime(2021, 4, 22),
                committer_email = "selab@gmail.com",
                committer_name = "selab",
                stats = new Stats
                {
                    additions = 52,
                    deletions = 46,
                    total = 98
                }
            });
            string commitsUrl = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repoId}/repository/commits?{token}&with_stats=true&per_page=100";
            string response = Newtonsoft.Json.JsonConvert.SerializeObject(responseOfCommit);
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-Total-Pages", "2");
            mockHttp.When(HttpMethod.Get, commitsUrl).Respond(headers, "application/json", response);
            mockHttp.When(HttpMethod.Get, commitsUrl+"&page=2").Respond("application/json", response);

            return mockHttp.ToHttpClient();
        }

        [Fact]
        public async void TestRequestCommits()
        {
            var response = await _gitlabInfo.RequestCommit(_repo);
            List<WeekTotalData> weekTotalDatas = new List<WeekTotalData>();
            weekTotalDatas.Add(new WeekTotalData { Total = 2, Week = new DateTime(2021, 4, 18).ToShortDateString()});
            Assert.Equal(weekTotalDatas[0].Total, response.WeekTotalData[0].Total);
            Assert.Equal(weekTotalDatas[0].Week, response.WeekTotalData[0].Week);

            List<DayOfWeekData> dayOfWeekDatas = new List<DayOfWeekData>();
            dayOfWeekDatas.Add(new DayOfWeekData 
            {
                Week = weekTotalDatas[0].Week,
                DetailDatas = new List<DayCommit> 
                { 
                    new DayCommit{ Day = "Sunday", Commit = 0},
                    new DayCommit{ Day = "Monday", Commit = 0},
                    new DayCommit{ Day = "Tuesday", Commit = 0},
                    new DayCommit{ Day = "Wednesday", Commit = 0},
                    new DayCommit{ Day = "Thursday", Commit = 2},
                    new DayCommit{ Day = "Friday", Commit = 0},
                    new DayCommit{ Day = "Saturday", Commit = 0}
                }
            });
            Assert.Equal(dayOfWeekDatas[0].Week, response.DayOfWeekData[0].Week);

            Assert.Equal(dayOfWeekDatas[0].DetailDatas[0].Day, response.DayOfWeekData[0].DetailDatas[0].Day);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[1].Day, response.DayOfWeekData[0].DetailDatas[1].Day);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[2].Day, response.DayOfWeekData[0].DetailDatas[2].Day);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[3].Day, response.DayOfWeekData[0].DetailDatas[3].Day);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[4].Day, response.DayOfWeekData[0].DetailDatas[4].Day);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[5].Day, response.DayOfWeekData[0].DetailDatas[5].Day);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[6].Day, response.DayOfWeekData[0].DetailDatas[6].Day);

            Assert.Equal(dayOfWeekDatas[0].DetailDatas[0].Commit, response.DayOfWeekData[0].DetailDatas[0].Commit);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[1].Commit, response.DayOfWeekData[0].DetailDatas[1].Commit);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[2].Commit, response.DayOfWeekData[0].DetailDatas[2].Commit);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[3].Commit, response.DayOfWeekData[0].DetailDatas[3].Commit);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[4].Commit, response.DayOfWeekData[0].DetailDatas[4].Commit);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[5].Commit, response.DayOfWeekData[0].DetailDatas[5].Commit);
            Assert.Equal(dayOfWeekDatas[0].DetailDatas[6].Commit, response.DayOfWeekData[0].DetailDatas[6].Commit);
        }

    }
}







