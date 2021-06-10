using project_manage_system_backend.Dtos;
using System.Threading.Tasks;

namespace project_manage_system_backend.Repository
{
    public interface IRepo
    {
        public Task<ResponseRepoInfoDto> GetRepositoryInformation(string url);
    }
}
