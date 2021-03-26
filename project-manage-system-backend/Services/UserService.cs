using Isopoh.Cryptography.Argon2;
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
    public class UserService : BaseService
    {
        public UserService(PMSContext dbContext) : base(dbContext) { }

        public void CreateUser(User model)
        {
            _dbContext.Users.Add(model);
            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("create user fail");
            }
        }

        public void CreateUser(LocalAccountDto dto)
        {
            User user = new User
            {
                Account = dto.Account,
                Authority = "Admin",
                AvatarUrl = "https://i.imgur.com/GzkyKYM.jpg",
                Name = "管理員",
                Password = Argon2.Hash(dto.Password)
            };
            _dbContext.Users.Add(user);
            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("create user fail");
            }
        }

        public bool CheckUserExist(string account)
        {
            var Users = _dbContext.Users.Where(u => u.Account.Equals(account)).ToList();
            return Users.Any();
        }

        public bool IsProjectOwner(User owner, int projectId)
        {
            var project = _dbContext.Projects.Include(p => p.Owner).Where(p => p.ID.Equals(projectId)).First();
            return project.Owner.Equals(owner);
        }

        public UserInfoDto GetUser(string account)
        {
            var user = _dbContext.Users.Find(account);
            if (user == null)
                throw new Exception("User not found!");
            return new UserInfoDto { id = user.Account, name = user.Name, avatarUrl = user.AvatarUrl };
        }

        public List<UserInfoDto> GetAllUser(string inviterId)
        {
            return _dbContext.Users.Where(u => u.Account != inviterId).Select(u => new UserInfoDto
            {
                id = u.Account,
                name = u.Name,
                avatarUrl = u.AvatarUrl
            }).ToList();
        }

        public void EditUserName(string account, string newUserName)
        {
            string regexPattern = "^[A-Za-z0-9]+$";
            Regex regex = new Regex(regexPattern);
            if (newUserName == "" || !regex.IsMatch(newUserName))
            {
                throw new Exception("please enter user name");
            }

            User user = _dbContext.Users.Find(account);
            if (user != null)
            {
                user.Name = newUserName;
                _dbContext.Update(user);
            }
            else
            {
                throw new Exception("user not found");
            }

            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("edit user name fail");
            }
        }

        public User GetUserModel(string account)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(p => p.Project).First(u => u.Account.Equals(account));

            return user;
        }

        public void AddProject(Invitation invitation)
        {
            var user = invitation.Applicant;
            user.Projects.Add(new UserProject { User = user, Project = invitation.InvitedProject });

            if (_dbContext.SaveChanges() == 0)
            {
                throw new Exception("Add project fail!");
            }
        }

        public List<User> GetAllUserNotInclude(string account)
        {
            return _dbContext.Users.Where(u => u.Account != account).ToList();
        }

        public void DeleteUserByAccount(string account)
        {
            var user = _dbContext.Users.Include(u => u.Projects).ThenInclude(up => up.Project).ThenInclude(upp => upp.Owner).FirstOrDefault(u => u.Account == account);
            if (user == null)
                throw new Exception("User not found!");
            if (user.Projects.Any())
            {
                var userProjects = user.Projects.Where(up => up.Project.Owner.Account == user.Account).ToList();
                ProjectService projectService = new ProjectService(_dbContext);
                userProjects.ForEach(up => projectService.DeleteProject(up.ProjectId));
            }
            _dbContext.Users.Remove(user);
            if (_dbContext.SaveChanges() == 0)
                throw new Exception("Delet user fail!");
        }

        public bool IsAdmin(string account)
        {
            return _dbContext.Users.Find(account).Authority.Equals("Admin");
        }

        public bool EditUserInfo(string account, JsonPatchDocument<User> newUserInfo)
        {
            var user = GetUserModel(account);
            newUserInfo.ApplyTo(user);
            return !(_dbContext.SaveChanges() == 0);
        }
    }
}
