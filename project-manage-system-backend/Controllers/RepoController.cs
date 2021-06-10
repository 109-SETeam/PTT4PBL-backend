using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System;
using System.Threading.Tasks;

namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RepoController : ControllerBase
    {
        private readonly RepoService _repoService;
        public RepoController(PMSContext dbContext)
        {
            _repoService = new RepoService(dbContext);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddRepo(AddRepoDto addRepoDto)
        {
            var result = await _repoService.AddRepo(addRepoDto);
            return Ok(new ResponseDto { success = result.success, message = result.message });
        }

        [Authorize]
        [HttpDelete("{projectId}/{repoId}")]
        public IActionResult DeleteRepo(int repoId, int projectId)
        {
            try
            {
                bool success = _repoService.DeleteRepo(projectId, repoId);
                return Ok(new ResponseDto()
                {
                    success = success,
                    message = success ? "Success" : "Error"
                });
            }
            catch (Exception e)
            {
                return Ok(new ResponseDto()
                {
                    success = false,
                    message = e.Message
                });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetRepoByProjectId(int id)
        {
            var result = _repoService.GetRepositoryByProjectId(id);
            return Ok(result);
        }
    }
}
