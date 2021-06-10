using project_manage_system_backend.Repository;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace project_manage_system_backend.Factory
{
    public class RepoFactory
    {
        const string GITHUB_COM = "github.com";
        const string GITLAB_COM = "sgit.csie.ntut.edu.tw/gitlab";

        public IRepo CreateRepoBy(string url, HttpClient httpClient, string oauthToken)
        {
            string matchPatten = @"^http(s)?://KEY_WORD/([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$";
            if (Regex.IsMatch(url, matchPatten.Replace("KEY_WORD", GITHUB_COM)))
            {
                return new GithubRepo(oauthToken, httpClient);
            }
            else if (Regex.IsMatch(url, matchPatten.Replace("KEY_WORD", GITLAB_COM)))
            {
                return new GitlabRepo(oauthToken, httpClient);
            }
            else
            {
                throw new Exception("Service not support");
            }
        }
    }
}
