using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Shares;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.Services
{
    public class SonarqubeService : BaseService
    {
        private readonly HttpClient _httpClient;

        public SonarqubeService(PMSContext dbContext, HttpClient client = null) : base(dbContext)
        {
            _httpClient = client ?? new HttpClient();
        }

        public async Task<SonarqubeInfoDto> GetSonarqubeInfoAsync(int repoId)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string sonarqubeHostUrl = repo.SonarqubeUrl;
            string apiUrl = "api/measures/search?";
            string projectKey = repo.ProjectKey;
            string query = "&metricKeys=bugs,vulnerabilities,code_smells,duplicated_lines_density,coverage";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {repo.AccountColonPw}");
            var response = await _httpClient.GetAsync($"{sonarqubeHostUrl}{apiUrl}projectKeys={projectKey}{query}");
            string content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SonarqubeInfoDto>(content);
            result.projectName = projectKey;
            return result;

        }

        public async Task<bool> IsHaveSonarqube(int repoId)
        {
            Repo repo = await _dbContext.Repositories.FindAsync(repoId);
            return repo.IsSonarqube;
        }
    }
}
