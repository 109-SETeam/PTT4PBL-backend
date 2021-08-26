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
        private const int PAGE_SIZE = 500;

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
            var result = await RequestCodeSmellData(repoId);
            int totalPages = (result.total - 1) / PAGE_SIZE + 1;

            for (int i = 2; i <= totalPages; i++)
            {
                var others = await RequestCodeSmellData(repoId, i);
                result.issues.AddRange(others.issues);
            }

            return result;
        }

        private async Task<CodeSmellDto> RequestCodeSmellData(int repoId, int pageIndex=1)
        {
            Repo repo = _dbContext.Repositories.Find(repoId);
            string sonarqubeHostUrl = repo.SonarqubeUrl;
            string apiUrl = "api/issues/search?";
            string projectKey = repo.ProjectKey;
            string query = $"componentKeys=PMS_109&s=FILE_LINE&resolved=false&ps={PAGE_SIZE}&organization=default-organization&facets=severities%2Ctypes&types=CODE_SMELL";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {repo.AccountColonPw}");
            var response = await _httpClient.GetAsync($"{sonarqubeHostUrl}{apiUrl}projectKeys={projectKey}&{query}&p={pageIndex}");
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CodeSmellDto>(content);
        }

        public async Task<bool> IsHaveSonarqube(int repoId)
        {
            Repo repo = await _dbContext.Repositories.FindAsync(repoId);
            return repo.IsSonarqube;
        }
    }
}
