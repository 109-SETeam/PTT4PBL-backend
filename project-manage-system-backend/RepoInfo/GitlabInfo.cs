using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using System;
using System.Collections.Generic;
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
            string commitsUrl = $"https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/{repo.RepoId}/repository/commits?{token}&with_stats=true&per_page=100s";
            
            var contributorResponse = await _httpClient.GetAsync(contributorUrl);
            var commitsResponse = await _httpClient.GetAsync(commitsUrl);
            
            string contributorContent = await contributorResponse.Content.ReadAsStringAsync();
            string commitsContent = await commitsResponse.Content.ReadAsStringAsync();
            
            var contributorResult = JsonSerializer.Deserialize<List<RequestContributorDto>>(contributorContent);
            var commitsResult = JsonSerializer.Deserialize<List<RequestContributorDto>>(commitsContent);
            
            List<ContributorsCommitActivityDto> contributors = new List<ContributorsCommitActivityDto>();
            foreach (var item in contributorResult)
            {
                contributors.Add(new ContributorsCommitActivityDto
                {
                    total = item.commits,
                    author = new Author { login = item.name }
                });
            }
            return contributors;
        }

        public override Task<RepoIssuesDto> RequestIssue(Repo repo)
        {
            throw new NotImplementedException();
        }
    }
}
