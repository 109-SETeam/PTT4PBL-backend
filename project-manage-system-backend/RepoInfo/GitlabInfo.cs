using project_manage_system_backend.Dtos;
using project_manage_system_backend.Dtos.Gitlab;
using project_manage_system_backend.Models;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.RepoInfo
{
    public class GitlabInfo : RepoInfoBase
    {
        private const string token = "access_token=nKswk3SkyZVyMR_q9KJ4";

        public GitlabInfo(string oauthToken, HttpClient httpClient = null) : base(oauthToken, httpClient)
        {
        }

        public override async Task<List<ResponseCodebaseDto>> RequestCodebase(Repo repo)
        {
            var commits = await GetRequestCommits(repo.RepoId);
            List<ResponseCodebaseDto> codebaseDtos = BuildResponseCodebaseDto(commits[^1].committed_date);
            foreach (var commit in commits)
            {
                var codebaseDto = codebaseDtos.Find(codebaseDto => codebaseDto.date.Equals(GetDateOfWeek(commit.committed_date).ToShortDateString()));
                codebaseDto.numberOfRowsAdded += commit.stats.additions;
                codebaseDto.numberOfRowsDeleted -= commit.stats.deletions;
            }

            int numberOfRows = 0;
            foreach (var codebaseDto in codebaseDtos)
            {
                string[] splitedString = codebaseDto.date.Split("/");
                codebaseDto.date = $"{splitedString[1].PadLeft(2, '0')}/{splitedString[2].PadLeft(2, '0')}";
                numberOfRows += codebaseDto.numberOfRowsAdded + codebaseDto.numberOfRowsDeleted;
                codebaseDto.numberOfRows = numberOfRows;
            }
            return codebaseDtos;
        }

        public override async Task<RequestCommitInfoDto> RequestCommit(Repo repo)
        {
            RequestCommitInfoDto requestCommitInfo = new RequestCommitInfoDto();
            List<RequestCommitsDto> requestCommits = await GetRequestCommits(repo.RepoId);
            requestCommitInfo.WeekTotalData = GetWeekTotalDatas(requestCommits);
            requestCommitInfo.DayOfWeekData = GetDayOfWeekDatas(requestCommits);

            return requestCommitInfo;
        }

        private List<WeekTotalData> GetWeekTotalDatas(List<RequestCommitsDto> requestCommits)
        {
            List<WeekTotalData> weekTotalDatas = new List<WeekTotalData>();
            List<Week> weeks = BuildWeeks(requestCommits[^1].committed_date);

            foreach (var requestCommit in requestCommits)
            {
                String commitWeek = GetDateOfWeek(requestCommit.committed_date).ToShortDateString();
                Week week = weeks.Find(week => week.ws.Equals(commitWeek));
                week.c += 1;
            }

            foreach (Week week in weeks)
            {
                weekTotalDatas.Add(new WeekTotalData { Week = week.ws, Total = week.c });
            }

            return weekTotalDatas;
        }

        private List<DayOfWeekData> GetDayOfWeekDatas(List<RequestCommitsDto> requestCommits)
        {
            List<DayOfWeekData> dayOfWeekData = new List<DayOfWeekData>();
            List<Week> weeks = BuildWeeks(requestCommits[^1].committed_date);

            foreach (Week week in weeks)
            {
                List<DayCommit> dayCommits = new List<DayCommit>();
                dayOfWeekData.Add(new DayOfWeekData { Week = week.ws, DetailDatas = dayCommits });
                DateTime weekDate = DateTime.Parse(week.ws);
                for (int i = 0; i < 7; i++)
                {
                    int commitsCount = requestCommits.FindAll(requestCommit => { return requestCommit.committed_date.Date == weekDate.Date; }).Count;
                    dayCommits.Add(new DayCommit { Day = DateHandler.ConvertToDayOfWeek(i), Commit = commitsCount });
                    weekDate = weekDate.AddDays(1);
                }
            }

            return dayOfWeekData;
        }

        public override async Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(Repo repo)
        {
            string contributorUrl = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repo.RepoId}/repository/contributors?{token}";
            var contributorResponse = await _httpClient.GetAsync(contributorUrl);
            string contributorContent = await contributorResponse.Content.ReadAsStringAsync();
            var contributorResult = JsonSerializer.Deserialize<List<RequestContributorDto>>(contributorContent);
            var commitsResult = await GetRequestCommits(repo.RepoId);

            List<ContributorsCommitActivityDto> contributors = new List<ContributorsCommitActivityDto>();
            foreach (var item in contributorResult)
            {
                contributors.Add(new ContributorsCommitActivityDto
                {
                    author = new Dtos.Author { login = item.name, email = item.email },
                    // ^1 = commitsResult.Count - 1
                    weeks = BuildWeeks(commitsResult[^1].committed_date)
                });
            }
            MapCommitsToWeeks(commitsResult, contributors);
            contributors.Sort((r1, r2) => r2.total.CompareTo(r1.total));
            //List<Dtos.Gitlab.User> gitlabUsers = await GetUsers(repo.RepoId);
            //foreach (var user in gitlabUsers)
            //{
            //    var selectedUser = contributors.Find(contributor => user.username.Equals(contributor.author.login));
            //    if (selectedUser != null)
            //    {
            //        selectedUser.author.html_url = user.web_url;
            //        selectedUser.author.avatar_url = user.avatar_url;
            //    }
            //}
            return contributors;
        }

        private async Task<List<Dtos.Gitlab.User>> GetUsers(string repoId)
        {
            string url = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repoId}/users?{token}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<Dtos.Gitlab.User>>(content);
            return users;
        }

        private async Task<List<RequestCommitsDto>> GetRequestCommits(string repoId)
        {
            string commitsUrl = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repoId}/repository/commits?{token}&with_stats=true&per_page=100";
            var commitsResponse = await _httpClient.GetAsync(commitsUrl);
            string commitsContent = await commitsResponse.Content.ReadAsStringAsync();
            var commitsResult = JsonSerializer.Deserialize<List<RequestCommitsDto>>(commitsContent).FindAll(commit => !(commit.parent_ids.Count > 1));
            var xTotalPages = Enumerable.ToList<string>(commitsResponse.Headers.GetValues("X-Total-Pages"));
            int xTotalPage = int.Parse(xTotalPages[0]);

            for (int i = 2; i <= xTotalPage; i++)
            {
                var response = await _httpClient.GetAsync($"{commitsUrl}&page={i}");
                var content = await response.Content.ReadAsStringAsync();
                commitsResult.AddRange(JsonSerializer.Deserialize<List<RequestCommitsDto>>(content).
                    FindAll(commit => !(commit.parent_ids.Count > 1)));
            }
            return commitsResult;
        }

        private List<Week> BuildWeeks(DateTime commitDate)
        {
            List<Week> weeks = new List<Week>();
            var vs = BuildFirstDaysOfWeeks(commitDate);

            foreach (var v in vs)
            {
                weeks.Add(new Week { ws = v });
            }

            return weeks;
        }

        private List<ResponseCodebaseDto> BuildResponseCodebaseDto(DateTime commitDate)
        {
            List<ResponseCodebaseDto> codebaseDtos = new List<ResponseCodebaseDto>();
            var weeks = BuildFirstDaysOfWeeks(commitDate);
            foreach (var week in weeks)
            {
                codebaseDtos.Add(new ResponseCodebaseDto
                {
                    date = week,
                    numberOfRows = 0,
                    numberOfRowsAdded = 0,
                    numberOfRowsDeleted = 0
                });
            }
            return codebaseDtos;
        }

        private List<string> BuildFirstDaysOfWeeks(DateTime commitDate)
        {
            List<string> weeks = new List<string>();
            var dateOfCommitWeek = GetDateOfWeek(commitDate);
            var dateOfCurrentWeek = GetDateOfWeek(DateTime.Today);
            while (dateOfCommitWeek <= dateOfCurrentWeek)
            {
                weeks.Add(dateOfCommitWeek.ToShortDateString());
                dateOfCommitWeek = dateOfCommitWeek.AddDays(7);
            }

            return weeks;
        }

        private DateTime GetDateOfWeek(DateTime dateTime)
        {
            return dateTime.AddDays(-((int)dateTime.DayOfWeek)).Date;
        }

        private void MapCommitsToWeeks(List<RequestCommitsDto> commitsResult, List<ContributorsCommitActivityDto> contributors)
        {
            foreach (var commit in commitsResult)
            {
                string commitWeek = GetDateOfWeek(commit.committed_date).ToShortDateString();
                foreach (var contributor in contributors)
                {
                    if (contributor.author.login.Equals(commit.committer_name) && contributor.author.email.Equals(commit.committer_email))
                    {
                        Week week = contributor.weeks.Find(week => week.ws.Equals(commitWeek));
                        week.a += commit.stats.additions;
                        week.d += commit.stats.deletions;
                        week.c += 1;
                        contributor.totalAdditions += commit.stats.additions;
                        contributor.totalDeletions += commit.stats.deletions;
                        contributor.total += 1;
                        break;
                    }
                }
            }
        }

        public override async Task<RepoIssuesDto> RequestIssue(Repo repo)
        {
            string issueUrl = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repo.RepoId}/issues?{token}";
            var issueResponse = await _httpClient.GetAsync(issueUrl);
            string issueContent = await issueResponse.Content.ReadAsStringAsync();
            var issueResult = JsonSerializer.Deserialize<List<RequestIssueDto>>(issueContent);
            List<ResponseRepoIssuesDto> openIssues = new List<ResponseRepoIssuesDto>();
            List<ResponseRepoIssuesDto> closeIssues = new List<ResponseRepoIssuesDto>();
            List<double> closedTime = new List<double>();
            RepoIssuesDto repoIssues = new RepoIssuesDto
            {
                openIssues = openIssues,
                closeIssues = closeIssues
            };

            foreach (var issue in issueResult)
            {
                if (issue.state.Equals("opened"))
                {
                    openIssues.Add(CreateResponseRepoIssuesDto(issue));
                }
                else //closed
                {
                    closeIssues.Add(CreateResponseRepoIssuesDto(issue));
                }
            }



            foreach (var item in repoIssues.closeIssues)
            {
                DateTime closed = Convert.ToDateTime(item.closed_at);
                DateTime created = Convert.ToDateTime(item.created_at);

                item.closed_at = closed.ToString("yyyy-MM-dd HH:mm:ss");
                item.created_at = created.ToString("yyyy-MM-dd HH:mm:ss");

                closedTime.Add((closed - created).TotalSeconds);
            }
            foreach (var item in repoIssues.openIssues)
            {
                item.created_at = Convert.ToDateTime(item.created_at).ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (closedTime.Count != 0)
            {
                repoIssues.averageDealWithIssueTime = TimeSpan.FromSeconds(closedTime.Average()).ToString(@"dd\.hh\:mm\:\:ss\.\.");
                repoIssues.averageDealWithIssueTime = repoIssues.averageDealWithIssueTime.Replace("..", "Seconds").Replace("::", "Minute(s) ").Replace(":", "Hour(s) ").Replace(".", "Day(s) ");
            }
            else
            {
                repoIssues.averageDealWithIssueTime = "No Data";
            }
            return repoIssues;
        }

        private ResponseRepoIssuesDto CreateResponseRepoIssuesDto(RequestIssueDto issue)
        {
            return new ResponseRepoIssuesDto
            {
                number = issue.iid,
                title = issue.title,
                state = issue.state,
                html_url = issue.web_url,
                created_at = issue.created_at,
                closed_at = issue.closed_at,
                user = new user
                {
                    html_url = issue.author.web_url,
                    login = issue.author.username
                }
            };
        }
    }
}
