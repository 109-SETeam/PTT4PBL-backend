using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace project_manage_system_backend.RepoInfo
{
    public abstract class RepoInfoBase : IRepoInfo
    {
        protected readonly HttpClient _httpClient;

        protected RepoInfoBase(string oauthToken, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

        }

        public abstract Task<List<ResponseCodebaseDto>> RequestCodebase(Repo repo);

        public abstract Task<RequestCommitInfoDto> RequestCommit(Repo repo);

        public abstract Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(Repo repo);

        public abstract Task<RepoIssuesDto> RequestIssue(Repo repo);
    }
}
