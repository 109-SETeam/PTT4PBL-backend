using project_manage_system_backend.Dtos;
using project_manage_system_backend.Factory;
using project_manage_system_backend.Models;
using project_manage_system_backend.RepoInfo;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public async Task<RequestCommitInfoDto> RequestCommit(int repoId, string oauthToken)
        {
            Repo repo = GetRepoBy(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestCommit(repo);
        }

        public async Task<List<ResponseCodebaseDto>> RequestCodebase(int repoId, string oauthToken)
        {
            Repo repo = GetRepoBy(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestCodebase(repo);
        }

        public async Task<RepoIssuesDto> RequestIssue(int repoId, string oauthToken)
        {
            Repo repo = GetRepoBy(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestIssue(repo);
        }

        public async Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(int repoId, string oauthToken)
        {
            Repo repo = GetRepoBy(repoId);
            IRepoInfo repoInfo = _repoInfoFactory.CreateRepoInfo(repo, _httpClient, oauthToken);
            return await repoInfo.RequestContributorsActivity(repo);
        }

        private Repo GetRepoBy(int repoId)
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
