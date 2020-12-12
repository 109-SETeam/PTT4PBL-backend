using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
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
        public async Task<IActionResult> AddRepo(RequestAddRepoDto addRepoDto)
        {
            var response = await _repoService.CheckRepoExist(addRepoDto.url);
            if (response.IsSucess)
            {
                var project = _repoService.GetProjectByProjectId(addRepoDto.projectId);

                Repo model = new Repo()
                {
                    Name = response.name,
                    Owner = response.owner.login,
                    Url = response.html_url,
                    Project = project
                };
                try
                {
                    _repoService.CreateRepo(model);
                    return Ok(new ResponseDto
                    {
                        success = true,
                        message = "Add Success"
                    });
                }
                catch (Exception e)
                {
                    return Ok(new ResponseDto
                    {
                        success = false,
                        message = "Add Fail:" + e.Message
                    });

                }
            }
            else
            {
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "Add Fail:" + response.message
                });

            }
        }


        [HttpDelete]
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
        public IActionResult GetRepositoryByProjectId(int id)
        {
            var result = _repoService.GetRepositoryByProjectId(id);
            return Ok(result);
        }
    }
}
