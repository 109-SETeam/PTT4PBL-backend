using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System.Linq;
using System.Threading.Tasks;

namespace project_manage_system_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RepoInfoController : ControllerBase
    {
        private readonly RepoInfoService _repoInfoService;
        private readonly SonarqubeService _sonarqubeService;
        public RepoInfoController(PMSContext dbContext)
        {
            _repoInfoService = new RepoInfoService(dbContext);
            _sonarqubeService = new SonarqubeService(dbContext);
        }

        [Authorize]
        [HttpGet("commit/{repoId}")]
        public async Task<IActionResult> GetCommit(int repoId)
        {
            string oauth_token = User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value;
            return Ok(await _repoInfoService.RequestCommit(repoId, oauth_token));
        }

        [Authorize]
        [HttpGet("codebase/{repoId}")]
        public async Task<IActionResult> GetCodebase(int repoId)
        {
            string oauthToken = User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value;
            return Ok(await _repoInfoService.RequestCodebase(repoId, oauthToken));
        }

        [Authorize]
        [HttpGet("contribute/{repoId}")]
        public async Task<IActionResult> GetContributorsActtivity(int repoId)
        {
            string oauthToken = User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value;
            return Ok(await _repoInfoService.RequestContributorsActivity(repoId, oauthToken));
        }

        [Authorize]
        [HttpGet("issue/{repoId}")]
        public async Task<IActionResult> GetIssue(int repoId)
        {
            string oauthToken = User.Claims.FirstOrDefault(c => c.Type.Equals("oauth")).Value;
            return Ok(await _repoInfoService.RequestIssue(repoId, oauthToken));
        }

        [Authorize]
        [HttpGet("sonarqube/{repoId}")]
        public async Task<IActionResult> GetSonarqube(int repoid)
        {
            return Ok(await _sonarqubeService.GetSonarqubeInfoAsync(repoid));
        }

        [Authorize]
        [HttpGet("ishavesonarqube/{repoId}")]
        public async Task<IActionResult> IsHaveSonarqube(int repoid)
        {
            return Ok(await _sonarqubeService.IsHaveSonarqube(repoid));
        }

        [HttpGet("sonarqube/codesmell/{repoId}")]
        public async Task<IActionResult> GetSonarqubeCodeSmell(int repoid)
        {
            return Ok(await _sonarqubeService.GetSonarqubeCodeSmellAsync(repoid));
        }
    }
}
