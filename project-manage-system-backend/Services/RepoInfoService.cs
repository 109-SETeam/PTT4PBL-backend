using project_manage_system_backend.Dtos;
using project_manage_system_backend.Factory;
using project_manage_system_backend.Models;
using project_manage_system_backend.RepoInfo;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.Services
{
    public class RepoInfoService : BaseService
    {
        private readonly HttpClient _httpClient;
        private readonly RepoInfoFactory _repoInfoFactory;
        public RepoInfoService(PMSContext dbContext, HttpClient client = null) : base(dbContext)
        {
            _httpClient = client ?? new HttpClient();
            _repoInfoFactory = new RepoInfoFactory();
        }

        public async Task<CommitInfoDto> RequestCommit(int repoId, string oauthToken)
        {
            Repo repo = GetRepoById(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestCommit(repo);
        }

        public async Task<List<CodebaseDto>> RequestCodebase(int repoId, string oauthToken)
        {
            Repo repo = GetRepoById(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestCodebase(repo);
        }

        public async Task<RepoIssuesDto> RequestIssue(int repoId, string oauthToken)
        {
            Repo repo = GetRepoById(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestIssue(repo);
        }

        public async Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(int repoId, string oauthToken)
        {
            Repo repo = GetRepoById(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestContributorsActivity(repo);
        }

        public async Task<SonarqubeInfoDto> GetSonarqubeInfo(int repoId)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string sonarqubeHostUrl = repo.sonarqubeUrl;
            string apiUrl = "api/measures/search?";
            string projectKey = repo.projectKey;
            string query = "&metricKeys=bugs,vulnerabilities,code_smells,duplicated_lines_density,coverage";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {repo.accountColonPw}");
            var response = await _httpClient.GetAsync($"{sonarqubeHostUrl}{apiUrl}projectKeys={projectKey}{query}");
            string content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SonarqubeInfoDto>(content);
            result.projectName = projectKey;
            return result;
        }

        public async Task<bool> IsHaveSonarqube(int repoId)
        {
            Repo repo = await _dbContext.Repositories.FindAsync(repoId);
            return repo.isSonarqube;
        }

        private Repo GetRepoById(int repoId)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            if (null == repo)
            {
                throw new Exception("not found");
            }
            return repo;
        }
    }
}
