using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace project_manage_system_backend.RepoInfo
{
    public interface IRepoInfo
    {
        public Task<RequestCommitInfoDto> RequestCommit(Repo repo);
        public Task<List<ResponseCodebaseDto>> RequestCodebase(Repo repo);
        public Task<RepoIssuesDto> RequestIssue(Repo repo);
        public Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(Repo repo);
    }
}
