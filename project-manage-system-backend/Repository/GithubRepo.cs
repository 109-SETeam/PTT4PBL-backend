using project_manage_system_backend.Dtos;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.Repository
{
    public class GithubRepo : RepoBase
    {
        public GithubRepo(string oauthToken, System.Net.Http.HttpClient httpClient) : base(oauthToken, httpClient)
        {
        }

        public override async Task<ResponseRepoInfoDto> GetRepositoryInformation(string url)
        {
            url = url.Replace(".git", "");
            url = url.Replace("github.com", "api.github.com/repos");

            var result = await _httpClient.GetAsync(url);
            string content = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<ResponseRepoInfoDto>(content);
            response.success = string.IsNullOrEmpty(response.message);
            return response;
        }
    }
}
