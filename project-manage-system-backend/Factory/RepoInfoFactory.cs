using project_manage_system_backend.Models;
using project_manage_system_backend.RepoInfo;
using System;
using System.Net.Http;

namespace project_manage_system_backend.Factory
{
    public class RepoInfoFactory
    {
        public IRepoInfo CreateRepoInfo(Repo repo, HttpClient httpClient, string oauthToken)
        {
            if (repo.Url.Contains("github"))
            {
                return new GithubInfo(oauthToken, httpClient);
            }
            else if (repo.Url.Contains("gitlab"))
            {
                return new GitlabInfo(oauthToken, httpClient);
            }
            else
            {
                throw new Exception("not support");
            }
        }
    }
}