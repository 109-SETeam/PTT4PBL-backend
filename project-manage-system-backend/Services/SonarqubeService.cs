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

        public async Task<CodeSmellDto> GetSonarqubeCodeSmellAsync(int repoId)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string sonarqubeHostUrl = repo.SonarqubeUrl;
            string apiUrl = "api/issues/search?"; 
            string projectKey = repo.ProjectKey;
            const int PAGE_SIZE = 10;
            int pageIndex = 1;
            string query = $"componentKeys=PMS_109&s=FILE_LINE&resolved=false&ps={PAGE_SIZE}&organization=default-organization&facets=severities%2Ctypes&types=CODE_SMELL";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {repo.AccountColonPw}");
            var response = await _httpClient.GetAsync($"{sonarqubeHostUrl}{apiUrl}projectKeys={projectKey}&{query}&p={pageIndex}");
            string content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CodeSmellDto>(content);
            int totalPages = (result.total - 1) / PAGE_SIZE + 1;

            for (int i = 2; i <= totalPages; i++)
            {
                response = await _httpClient.GetAsync($"{sonarqubeHostUrl}{apiUrl}projectKeys={projectKey}&{query}&p={i}");
                content = await response.Content.ReadAsStringAsync();
                var others = JsonSerializer.Deserialize<CodeSmellDto>(content);
                result.issues.AddRange(others.issues);
            }

            return result;
        }

        public async Task<bool> IsHaveSonarqube(int repoId)
        {
            Repo repo = await _dbContext.Repositories.FindAsync(repoId);
            return repo.IsSonarqube;
        }
    }
}
