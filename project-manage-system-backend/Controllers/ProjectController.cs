using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System;

namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly UserService _userService;
        private PMSContext _dbContext;

        public ProjectController(PMSContext dbContext)
        {
            _dbContext = dbContext;
            _projectService = new ProjectService(_dbContext);
            _userService = new UserService(_dbContext);
        }

        [HttpPost]
        public IActionResult AddProject(ProjectDto projectDto)
        {
            try
            {
                _projectService.Create(projectDto);
                return Ok(new ResponseDto
                {
                    success = true,
                    message = "�s�W���\"
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseDto
                {
                    success = false,
                    message = "�s�W����" + ex.Message
                });
            }
        }

        [Authorize]
        [HttpDelete("{projectId}/{userId}")]
        public IActionResult DeleteProject(int projectId, string userId)
        {
            if (_userService.CheckUserExist(userId))
            {
                var user = _userService.GetUserModel(userId);
                if (_userService.IsProjectOwner(user, projectId))
                {
                    try
                    {
                        _projectService.DeleteProject(projectId);

                        return Ok(new ResponseDto
                        {
                            success = true,
                            message = "刪除成功",
                        });
                    }
                    catch (Exception e)
                    {
                        return NotFound(new ResponseDto
                        {
                            success = false,
                            message = e.Message,
                        });
                    }
                }
                else
                {
                    return Ok(new ResponseDto
                    {
                        success = false,
                        message = "非專案擁有者，無法刪除此專案",
                    });
                }
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetProject()
        {
            var result = _projectService.GetProjectByUserAccount(User.Identity.Name);
            return Ok(result);
        }
    }
}