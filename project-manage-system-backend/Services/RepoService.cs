using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Factory;
using project_manage_system_backend.Models;
using project_manage_system_backend.Repository;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace project_manage_system_backend.Services
{
    public class RepoService : BaseService
    {
        private readonly HttpClient _httpClient;
        private readonly RepoFactory _repoFactory;
        public RepoService(PMSContext dbContext, HttpClient client = null) : base(dbContext)
        {
            _httpClient = client ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            _repoFactory = new RepoFactory();
        }

        private async Task<ResponseRepoInfoDto> GetRepositoryInformation(string url)
        {
            IRepo repo = _repoFactory.CreateRepoBy(url, _httpClient, null);
            return await repo.GetRepositoryInformation(url);
        }

        public async Task<ResponseDto> AddRepo(AddRepoDto addRepoDto)
        {
            try
            {
                var githubResponse = await GetRepositoryInformation(addRepoDto.url);
                ResponseDto result = new ResponseDto() { success = githubResponse.success, message = githubResponse.message };
                if (githubResponse.success)
                {
                    Repo model = MakeRepoModel(githubResponse, addRepoDto);

                    CreateRepo(model);
                    result.message = "Add Success";
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ResponseDto() { message = ex.Message, success = false };
            }
        }

        private Repo MakeRepoModel(ResponseRepoInfoDto githubResponse, AddRepoDto addRepoDto)
        {
            var project = GetProjectByProjectId(addRepoDto.projectId);
            return new Repo()
            {
                Name = githubResponse.name,
                Owner = githubResponse.owner.login ?? githubResponse.owner.name,
                Url = githubResponse.html_url ?? githubResponse.web_url,
                Project = project,
                RepoId = githubResponse.id.ToString()
            };
        }

        private void CreateRepo(Repo model)
        {
            //get project by id
            var repo = model.Project.Repositories.Where(r => r.Url == model.Url);
            // check duplicate =>  add or throw exception
            if (!repo.Any())
                _dbContext.Add(model);
            else
                throw new Exception("Duplicate repo!");
            //save
            if (_dbContext.SaveChanges() == 0)
                throw new Exception("DB can't save!");
        }

        public List<Repo> GetRepositoryByProjectId(int id)
        {
            var project = _dbContext.Projects.Where(p => p.ID.Equals(id)).Include(p => p.Repositories).First();
            return project.Repositories;
        }

        public Project GetProjectByProjectId(int id)
        {
            var project = _dbContext.Projects.Include(r => r.Repositories).Where(p => p.ID == id).First();
            return project;
        }

        public bool DeleteRepo(int projectId, int repoId)
        {
            try
            {
                var repo = _dbContext.Repositories.Include(p => p.Project).First(r => r.ID == repoId && r.Project.ID == projectId);
                _dbContext.Repositories.Remove(repo);
                return !(_dbContext.SaveChanges() == 0);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
