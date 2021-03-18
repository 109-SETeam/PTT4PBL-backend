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

        public override Task<CommitInfoDto> RequestCommit(Repo repo)
        {
            throw new NotImplementedException();
        }

        public override Task<List<ContributorsCommitActivityDto>> RequestContributorsActivity(Repo repo)
        {
            throw new NotImplementedException();
        }

        public override Task<RepoIssuesDto> RequestIssue(Repo repo)
        {
            throw new NotImplementedException();
        }
    }
}
