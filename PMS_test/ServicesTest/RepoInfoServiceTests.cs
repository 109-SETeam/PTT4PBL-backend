using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PMS_test.ControllersTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class RepoInfoServiceTests
    {
        private readonly PMSContext _dbContext;
        private readonly HttpClient _client;
        private readonly RepoInfoService _repoInfoService;

        private const string _owner = "WeedChen";
        private const string _name = "AutoPlaneCup";
        private const string _commitUrl = "https://api.github.com/repos/" + _owner + "/" + _name + "/stats/commit_activity";
        private const string _openIssueUrl = "https://api.github.com/repos/" + _owner + "/" + _name + "/issues?state=open&per_page=100&page=1&sort=created";
        private const string _closedIssueUrl = "https://api.github.com/repos/" + _owner + "/" + _name + "/issues?state=closed&per_page=100&page=1&sort=created";
        private const string _codebaseUrl = "https://api.github.com/repos/" + _owner + "/" + _name + "/stats/code_frequency";
        private const string _contributorsActivityUrl = "https://api.github.com/repos/" + _owner + "/" + _name + "/stats/contributors";

        public RepoInfoServiceTests()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
               .UseSqlite(CreateInMemoryDatabase()).Options);
            _dbContext.Database.EnsureCreated();
            _client = CreateMockClient();
            _repoInfoService = new RepoInfoService(_dbContext, _client);
            InitialDatabase();
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

            mockHttp.When(HttpMethod.Get, _commitUrl)
                    .Respond("application/json",
                    "[{\"total\":2,\"week\":1575158400,\"days\":[2,0,0,0,0,0,0]},{\"total\":7,\"week\":1575763200,\"days\":[0,2,0,2,3,0,0]}]");

            mockHttp.When(HttpMethod.Get, _codebaseUrl)
                    .Respond("application/json",
                    "[[1603584000, 17220, 0], [1604188800, 112, -193]]");

            //issue

            RepoIssuesDto dto = new RepoIssuesDto();
            DateTime closed = Convert.ToDateTime("2020/12/03");
            DateTime created = Convert.ToDateTime("2020/12/01");
            dto.averageDealWithIssueTime = TimeSpan.FromSeconds((created - closed).TotalSeconds).ToString(@"dd\.hh\:mm\:\:ss\.\.");
            dto.openIssues = JsonConvert.DeserializeObject<List<ResponseRepoIssuesDto>>(CreateFakeIssues("open"));
            dto.closeIssues = JsonConvert.DeserializeObject<List<ResponseRepoIssuesDto>>(CreateFakeIssues("closed"));

            mockHttp.When(HttpMethod.Get, _openIssueUrl)
                    .Respond("application/json", CreateFakeIssues("open"));
            mockHttp.When(HttpMethod.Get, _closedIssueUrl)
                    .Respond("application/json", CreateFakeIssues("closed"));

            /* contributors ActivityUrl */
            mockHttp.When(HttpMethod.Get, _contributorsActivityUrl)
                    .Respond("application/json", CreateFakeContributorsActivityData(false));

            return mockHttp.ToHttpClient();
        }

        private string CreateFakeIssues(string status)
        {
            DateTime closed = Convert.ToDateTime("2020-12-03");
            DateTime created = Convert.ToDateTime("2020-12-01");

            List<ResponseRepoIssuesDto> openList = new List<ResponseRepoIssuesDto>()
            {
                new ResponseRepoIssuesDto()
                { created_at=created.ToString("yyyy-MM-dd HH:mm:ss"),number =1,state="open",title="test",user = new user{html_url="",login="aaa" },closed_at="",html_url="" }
            };
            List<ResponseRepoIssuesDto> closedList = new List<ResponseRepoIssuesDto>()
            {
                new ResponseRepoIssuesDto()
                { created_at=created.ToString("yyyy-MM-dd HH:mm:ss"),closed_at=closed.ToString("yyyy-MM-dd HH:mm:ss"),number=2,state="closed",title="test",user = new user{html_url="",login="aaa" },html_url=""}
            };
            string openjson = JsonConvert.SerializeObject(openList);
            string closedjson = JsonConvert.SerializeObject(closedList);
            return status == "open" ? openjson : closedjson;
        }

        private void InitialDatabase()
        {
            _dbContext.Repositories.Add(new Repo
            {
                Name = _name,
                Owner = _owner,
                Url = "https://github.com/" + _owner + "/" + _name + ""
            });
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task TestRequestCommitInfo()
        {
            var commitInfo = await _repoInfoService.RequestCommit(1, null);

            List<WeekTotalData> weekTotalDatas = new List<WeekTotalData>()
            {
                new WeekTotalData(){Total=2,Week="12/01"},
                new WeekTotalData(){Total=7,Week="12/08"}
            };

            List<DayCommit> dayCommitsFirst = new List<DayCommit>()
            {
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(0),Commit=2},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(1),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(2),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(3),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(4),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(5),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(6),Commit=0}
            };

            List<DayCommit> dayCommitsSecond = new List<DayCommit>()
            {
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(0),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(1),Commit=2},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(2),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(3),Commit=2},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(4),Commit=3},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(5),Commit=0},
                new DayCommit(){Day=DateHandler.ConvertToDayOfWeek(6),Commit=0}
            };

            List<DayOfWeekData> dayOfWeekDatas = new List<DayOfWeekData>()
            {
                new DayOfWeekData(){DetailDatas=dayCommitsFirst,Week="12/01"},
                new DayOfWeekData(){DetailDatas=dayCommitsSecond,Week="12/08"}
            };

            var expected = new RequestCommitInfoDto
            {
                DayOfWeekData = dayOfWeekDatas,
                WeekTotalData = weekTotalDatas
            };

            var expectedStr = JsonConvert.SerializeObject(expected);
            var commitInfoStr = JsonConvert.SerializeObject(commitInfo);
            Assert.Equal(expectedStr, commitInfoStr);
        }

        [Fact]
        public async Task TestRequestCodebase()
        {
            List<ResponseCodebaseDto> codebaseDtos = new List<ResponseCodebaseDto>()
            {
                new ResponseCodebaseDto(){ date = "10/25", numberOfRowsAdded = 17220, numberOfRowsDeleted = 0, numberOfRows = 17220 },
                new ResponseCodebaseDto(){ date = "11/01", numberOfRowsAdded = 112, numberOfRowsDeleted = -193, numberOfRows = 17139 }
            };

            var codebaseInfo = await _repoInfoService.RequestCodebase(1, "todo");

            var expectedStr = JsonConvert.SerializeObject(codebaseDtos);
            var codebaseStr = JsonConvert.SerializeObject(codebaseInfo);
            Assert.Equal(expectedStr, codebaseStr);
        }

        [Fact]
        public async Task TestRequestIssueInfo()
        {
            RepoIssuesDto dto = new RepoIssuesDto();
            DateTime closed = Convert.ToDateTime("2020-12-03");
            DateTime created = Convert.ToDateTime("2020-12-01");
            dto.averageDealWithIssueTime = TimeSpan.FromSeconds((created - closed).TotalSeconds).ToString(@"dd\.hh\:mm\:\:ss\.\.").Replace("..", "Seconds").Replace(".", "Day(s) ").Replace("::", "Minute(s) ").Replace(":", "Hour(s) "); ;
            dto.openIssues = JsonConvert.DeserializeObject<List<ResponseRepoIssuesDto>>(CreateFakeIssues("open"));
            dto.closeIssues = JsonConvert.DeserializeObject<List<ResponseRepoIssuesDto>>(CreateFakeIssues("closed"));
            var excepted = JsonConvert.SerializeObject(dto);

            var issues = await _repoInfoService.RequestIssue(1, "KENFOnwogneorngIONefokwNGFIONROPGNro");
            string actual = JsonConvert.SerializeObject(issues);

            Assert.Equal(excepted, actual);
        }

        private string CreateFakeContributorsActivityData(bool isExcepted)
        {
            var weeks = new List<Week>
            {
                new Week { ws = isExcepted? "10/25":"", w = 1603584000, a = 0, d = 0, c = 0 },
                new Week { ws = isExcepted? "11/01":"", w = 1604188800, a = 0, d = 0, c = 0 },
                new Week { ws = isExcepted? "11/08":"", w = 1604793600, a = 0, d = 0, c = 0 },
                new Week { ws = isExcepted? "11/15":"", w = 1605398400, a = 778, d = 649, c = 32 },
                new Week { ws = isExcepted? "11/22":"", w = 1606003200, a = 289, d = 93, c = 13 },
                new Week { ws = isExcepted? "11/29":"", w = 1606608000, a = 8, d = 4, c = 3 }
            };

            ContributorsCommitActivityDto contributorsCommitActivityDto = new ContributorsCommitActivityDto
            {
                author = new Author
                {
                    avatar_url = "https://avatars2.githubusercontent.com/u/31059035?v=4",
                    html_url = "https://github.com/zxjte9411",
                    login = _owner
                },
                total = 48,
                weeks = weeks,
                totalAdditions = isExcepted ? 1075 : 0,
                totalDeletions = isExcepted ? 746 : 0,
                commitsHtmlUrl = $"https://github.com/{_owner}/{_name}/commits?author={_owner}"
            };
            List<ContributorsCommitActivityDto> contributorsCommitActivities = new List<ContributorsCommitActivityDto> { contributorsCommitActivityDto };
            contributorsCommitActivityDto.total = 50;
            contributorsCommitActivities.Add(contributorsCommitActivityDto);
            return JsonConvert.SerializeObject(contributorsCommitActivities);
        }

        [Fact]
        public async Task TestRequestContributorsActivity()
        {
            var contributorsActtivityInfo = await _repoInfoService.RequestContributorsActivity(1, "KENFOnwogneorngIONefokwNGFIONROPGNro");
            string actual = JsonConvert.SerializeObject(contributorsActtivityInfo);
            var excepted = CreateFakeContributorsActivityData(true);
            Assert.Equal(excepted, actual);
        }
    }
}
