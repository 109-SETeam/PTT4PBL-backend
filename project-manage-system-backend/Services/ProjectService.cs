using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace project_manage_system_backend.Services
{
    public class ProjectService : BaseService
    {
        public ProjectService(PMSContext dbContext) : base(dbContext) { }

        public void CreateProject(ProjectDto projectDto, string userId)
        {
            string regexPattern = "^[A-Za-z0-9]+";
            Regex regex = new Regex(regexPattern);
            if (projectDto.ProjectName == "" || !regex.IsMatch(projectDto.ProjectName))
            {
                throw new Exception("please enter project name");
            }
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).FirstOrDefault(u => u.Account.Equals(userId));

            if (user != null)
            {
                var userProject = (from up in user.Projects
                                   where up.Project.Name == projectDto.ProjectName
                                   select up.Project.Name).ToList();
                if (userProject.Count == 0)
                {
                    var newProject = new Models.UserProject
                    {
                        Project = new Models.Project { Name = projectDto.ProjectName, Owner = user },
                    };

                    user.Projects.Add(newProject);
                }
                else
                {
                    throw new Exception("duplicate project name");
                }
            }
            else
            {
                throw new Exception("user fail, can not find this user");
            }


            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("create project fail");
            }
        }

        public void EditProjectName(ProjectDto projectDto)
        {
            string regexPattern = "^[A-Za-z0-9_-]+$";
            Regex regex = new Regex(regexPattern);
            if (projectDto.ProjectName == "" || !regex.IsMatch(projectDto.ProjectName))
            {
                throw new Exception("please enter project name");
            }
            else if (_dbContext.Projects.Where(p => p.Name == projectDto.ProjectName).ToList().Count != 0)
            {
                throw new Exception("duplicate project name");
            }

            var project = _dbContext.Projects.Find(projectDto.ProjectId);

            project.Name = projectDto.ProjectName;

            _dbContext.Update(project);

            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("edit project name fail");
            }
        }

        public List<ProjectResultDto> GetProjectByUserAccount(string account)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).ThenInclude(p => p.Owner).FirstOrDefault(u => u.Account.Equals(account));
            var query = (from up in user.Projects
                         select new ProjectResultDto { Id = up.Project.ID, Name = up.Project.Name, OwnerId = up.Project.Owner.Account, OwnerName = up.Project.Owner.Name }).ToList();
            return query;
        }

        public ProjectResultDto GetProjectByProjectId(int projectId, string account)
        {
            List<ProjectResultDto> userProject = GetProjectByUserAccount(account);

            foreach (ProjectResultDto projectResultDto in userProject)
            {
                if (projectResultDto.Id == projectId)
                {
                    return projectResultDto;
                }
            }
            throw new Exception("error project");
        }

        public List<UserInfoDto> GetProjectMember(int projectId)
        {
            var projectById = _dbContext.Projects.Include(p => p.Users).ThenInclude(u => u.User).FirstOrDefault(p => p.ID == projectId);

            if (projectById != null)
            {
                var projectMember = projectById.Users;

                List<UserInfoDto> result = new List<UserInfoDto>();

                foreach (UserProject userProject in projectMember)
                {
                    result.Add(new UserInfoDto { id = userProject.User.Account, name = userProject.User.Name });
                }

                return result;
            }
            else
            {
                throw new Exception("project is not found");
            }

        }

        public List<ProjectResultDto> GetAllProject()
        {
            return _dbContext.Projects.Select(p => new ProjectResultDto { Id = p.ID, Name = p.Name, OwnerId = p.Owner.Account, OwnerName = p.Owner.Name, Members = p.Users.Count }).ToList();
        }

        public void DeleteProject(int projectId)
        {
            var invitations = _dbContext.Invitations.Where(i => i.InvitedProject.ID.Equals(projectId));
            _dbContext.Invitations.RemoveRange(invitations);
            var repos = _dbContext.Repositories.Where(r => r.Project.ID.Equals(projectId));
            _dbContext.Repositories.RemoveRange(repos);
            var project = _dbContext.Projects.Find(projectId);
            _dbContext.Projects.Remove(project);

            if (_dbContext.SaveChanges() == 0)
                throw new Exception("Delete project fail!");
        }

        public void DeleteProjectMember(string userId, int projectId)
        {
            var user = _dbContext.Users.Find(userId);
            var project = user.Projects.Where(p => p.ProjectId.Equals(projectId)).FirstOrDefault();
            user.Projects.Remove(project);

            if (_dbContext.SaveChanges() == 0)
                throw new Exception("Delete project member fail!");
        }

        public bool EditProjectNameByAdmin(int projectId, JsonPatchDocument<Project> newProject)
        {
            var project = _dbContext.Projects.Find(projectId);
            newProject.ApplyTo(project);
            return !(_dbContext.SaveChanges() == 0);
        }
    }
}
