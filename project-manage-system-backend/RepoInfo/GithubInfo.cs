using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.RepoInfo
{
    public class GithubInfo : RepoInfoBase
    {
        public GithubInfo(string oauthToken, HttpClient httpClient = null) : base(oauthToken, httpClient)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", oauthToken);
        }

        public override async Task<List<ResponseCodebaseDto>> RequestCodebase(Repo repo)
        {
            string repoURL = "https://api.github.com/repos/" + repo.Owner + "/" + repo.Name + "/stats/code_frequency";

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await _httpClient.GetAsync(repoURL);
            var contents = await response.Content.ReadAsStringAsync();
            var codebases = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int[]>>(contents);

            List<ResponseCodebaseDto> codebaseSet = new List<ResponseCodebaseDto>();

            foreach (var codebase in codebases)
            {
                ResponseCodebaseDto codebaseDto = new ResponseCodebaseDto()
                {
                    date = DateHandler.ConvertToDateString(codebase[0]),
                    numberOfRowsAdded = Convert.ToInt32(codebase[1]),
                    numberOfRowsDeleted = Convert.ToInt32(codebase[2]),
                    numberOfRows = Convert.ToInt32(codebase[1]) + Convert.ToInt32(codebase[2])
                };

                codebaseSet.Add(codebaseDto);
            }

            int thisWeekRows = 0;

            foreach (var codebase in codebaseSet)
            {
                codebase.numberOfRows += thisWeekRows;
                thisWeekRows = codebase.numberOfRows;
            }

            return codebaseSet;
        }

        public override async Task<RequestCommitInfoDto> RequestCommit(Repo repo)
        {
            string url = "https://api.github.com/repos/" + repo.Owner + "/" + repo.Name + "/stats/commit_activity";

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                await Task.Delay(1000);
                response = await _httpClient.GetAsync(url);
            }

            string content = await response.Content.ReadAsStringAsync();
            var commitInfos = JsonSerializer.Deserialize<List<ResponseCommitInfoDto>>(content);

            List<WeekTotalData> weekChartDatas = new List<WeekTotalData>();
            List<DayOfWeekData> detailChartDatas = new List<DayOfWeekData>();

            foreach (var commitInfo in commitInfos)
            {
                weekChartDatas.Add(ConvertToWeekChartData(commitInfo));
                detailChartDatas.Add(ConvertToDetailChartData(commitInfo));
            }

            return new RequestCommitInfoDto
            {
                WeekTotalData = weekChartDatas,
                DayOfWeekData = detailChartDatas
            };
        }

        public override async Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(Repo repo)
        {
            string url = "https://api.github.com/repos/" + repo.Owner + "/" + repo.Name + "/stats/contributors";
            var response = await _httpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<ContributorsCommitActivityDto>>(content);
            // sort by commit 
            result.Sort((r1, r2) => r2.total.CompareTo(r1.total));

            foreach (var item in result)
            {
                item.commitsHtmlUrl = $"https://github.com/{repo.Owner}/{repo.Name}/commits?author={item.author.login}";
                foreach (var week in item.weeks)
                {
                    week.ws = DateHandler.ConvertToDateString(week.w);
                    item.totalAdditions += week.a;
                    item.totalDeletions += week.d;
                }
            }
            return result;
        }

        public override async Task<RepoIssuesDto> RequestIssue(Repo repo)
        {
            RepoIssuesDto result = new RepoIssuesDto();
            List<double> closedTime = new List<double>();
            string url = repo.Url.Replace("github.com/", "api.github.com/repos/");


            result.closeIssues = await GetRepoIssues("closed", url, 100);
            result.openIssues = await GetRepoIssues("open", url, 100);

            foreach (var item in result.closeIssues)
            {
                DateTime closed = Convert.ToDateTime(item.closed_at);
                DateTime created = Convert.ToDateTime(item.created_at);

                item.closed_at = closed.ToString("yyyy-MM-dd HH:mm:ss");
                item.created_at = created.ToString("yyyy-MM-dd HH:mm:ss");

                closedTime.Add((closed - created).TotalSeconds);
            }
            foreach (var item in result.openIssues)
            {
                item.created_at = Convert.ToDateTime(item.created_at).ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (closedTime.Count != 0)
            {
                result.averageDealWithIssueTime = TimeSpan.FromSeconds(closedTime.Average()).ToString(@"dd\.hh\:mm\:\:ss\.\.");
                result.averageDealWithIssueTime = result.averageDealWithIssueTime.Replace("..", "Seconds").Replace("::", "Minute(s) ").Replace(":", "Hour(s) ").Replace(".", "Day(s) ");
            }
            else
            {
                result.averageDealWithIssueTime = "No Data";
            }
            return result;
        }

        private async Task<List<ResponseRepoIssuesDto>> GetRepoIssues(string state, string url, int perPage)
        {
            List<ResponseRepoIssuesDto> result = new List<ResponseRepoIssuesDto>();
            int page = 1;

            var response = await _httpClient.GetAsync(url + $"/issues?state={state}&per_page=100&page={page}&sort=created");
            string content = await response.Content.ReadAsStringAsync();
            var tempList = JsonSerializer.Deserialize<List<ResponseRepoIssuesDto>>(content);
            result.AddRange(tempList);

            return result;
        }

        private WeekTotalData ConvertToWeekChartData(ResponseCommitInfoDto commitInfo)
        {
            return new WeekTotalData
            {
                Total = commitInfo.total,
                Week = DateHandler.ConvertToDateString(commitInfo.week)
            };
        }

        private DayOfWeekData ConvertToDetailChartData(ResponseCommitInfoDto commitInfo)
        {
            List<DayCommit> detailDatas = new List<DayCommit>();

            int dayOfWeekCount = 0;
            foreach (var day in commitInfo.days)
            {
                detailDatas.Add(new DayCommit
                {
                    Day = DateHandler.ConvertToDayOfWeek(dayOfWeekCount++),
                    Commit = day
                });
            }

            return new DayOfWeekData
            {
                Week = DateHandler.ConvertToDateString(commitInfo.week),
                DetailDatas = detailDatas
            };
        }

    }
}
