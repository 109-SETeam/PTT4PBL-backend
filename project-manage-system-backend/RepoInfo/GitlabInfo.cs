using project_manage_system_backend.Dtos;
using project_manage_system_backend.Dtos.Gitlab;
using project_manage_system_backend.Models;
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
        public GitlabInfo(string oauthToken, HttpClient httpClient = null) : base(oauthToken, httpClient)
        {
        }

        public override Task<List<ResponseCodebaseDto>> RequestCodebase(Repo repo)
        {
            throw new NotImplementedException();
        }

        public override Task<RequestCommitInfoDto> RequestCommit(Repo repo)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(Repo repo)
        {
            const string token = "access_token=nKswk3SkyZVyMR_q9KJ4";
            string contributorUrl = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repo.RepoId}/repository/contributors?{token}";
            string commitsUrl = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repo.RepoId}/repository/commits?{token}&with_stats=true&per_page=100";
            
            var contributorResponse = await _httpClient.GetAsync(contributorUrl);
            var commitsResponse = await _httpClient.GetAsync(commitsUrl);
            
            string contributorContent = await contributorResponse.Content.ReadAsStringAsync();
            string commitsContent = await commitsResponse.Content.ReadAsStringAsync();

            var contributorResult = JsonSerializer.Deserialize<List<RequestContributorDto>>(contributorContent);
            var commitsResult = JsonSerializer.Deserialize<List<RequestCommitsDto>>(commitsContent);

            var xTotalPages = Enumerable.ToList<string>(commitsResponse.Headers.GetValues("X-Total-Pages"));
            int xTotalPage = int.Parse(xTotalPages[0]);

            for (int i = 2; i <= xTotalPage; i++)
            {
                var response = await _httpClient.GetAsync($"{commitsUrl}&page={i}");
                var content = await response.Content.ReadAsStringAsync();
                commitsResult.AddRange(JsonSerializer.Deserialize<List<RequestCommitsDto>>(content));
            }

            List<ContributorsCommitActivityDto> contributors = new List<ContributorsCommitActivityDto>();
            foreach (var item in contributorResult)
            {
                contributors.Add(new ContributorsCommitActivityDto
                {
                    author = new Author { login = item.name, email = item.email },
                    // ^1 = commitsResult.Count - 1
                    weeks = BuildWeeks(commitsResult[^1].committed_date)
                });
            }
            MapCommitsToWeeks(commitsResult, contributors);
            return contributors;
        }

        private List<Week> BuildWeeks(DateTime commitDate)
        {
            List<Week> weeks = new List<Week>();
            var dateOfCommitWeek = commitDate.AddDays(-((int)commitDate.DayOfWeek));
            var dateOfCurrentWeek = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek));

            while (dateOfCommitWeek <= dateOfCurrentWeek)
            {
                weeks.Add(new Week { ws = dateOfCommitWeek.ToShortDateString() });
                dateOfCommitWeek = dateOfCommitWeek.AddDays(7);
            }
            return weeks;
        }

        private void MapCommitsToWeeks(List<RequestCommitsDto> commitsResult, List<ContributorsCommitActivityDto> contributors)
        {
            foreach (var commit in commitsResult)
            {
                String commitWeek = commit.committed_date.AddDays(-((int)commit.committed_date.DayOfWeek)).ToShortDateString();
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

        public override Task<RepoIssuesDto> RequestIssue(Repo repo)
        {
            throw new NotImplementedException();
        }
    }
}
