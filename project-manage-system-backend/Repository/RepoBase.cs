using project_manage_system_backend.Dtos;
using System.Net.Http;
using System.Threading.Tasks;

namespace project_manage_system_backend.Repository
{
    public abstract class RepoBase : IRepo
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _oauthToken;

        protected RepoBase(string oauthToken, HttpClient httpClient)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _oauthToken = oauthToken;
        }

        public abstract Task<ResponseRepoInfoDto> GetRepositoryInformation(string url);
    }
}
