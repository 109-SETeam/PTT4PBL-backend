using project_manage_system_backend.Dtos;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.Repository
{
    public class GitlabRepo : RepoBase
    {
        private const string ACCESS_TOKEN = "access_token=nKswk3SkyZVyMR_q9KJ4";
        public GitlabRepo(string oauthToken, HttpClient httpClient) : base(oauthToken, httpClient)
        {
        }

        public override async Task<ResponseRepoInfoDto> GetRepositoryInformation(string url)
        {
            const string API_URL = "https://sgit.csie.ntut.edu.tw/gitlab/api/v4/projects/";
            const string TEMP_KEY_WORD = "KEY_WORD";
            url = url.Replace(".git", "");
            url = url.Replace("https://sgit.csie.ntut.edu.tw/gitlab/", TEMP_KEY_WORD);
            url = url.Replace("/", "%2F");
            url = url.Replace(TEMP_KEY_WORD, API_URL);
            url = $"{url}?{ACCESS_TOKEN}";
            var result = await _httpClient.GetAsync(url);
            string content = await result.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<ResponseRepoInfoDto>(content);
            response.success = string.IsNullOrEmpty(response.message);
            return response;
        }
    }
}
