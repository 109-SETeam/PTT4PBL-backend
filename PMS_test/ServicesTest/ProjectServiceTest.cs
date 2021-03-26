using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Xunit;

namespace PMS_test.ServicesTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class ProjectServiceTest
    {
        private readonly PMSContext _dbContext;
        private readonly ProjectService _projectService;

        public ProjectServiceTest()
        {
            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
                                            .UseSqlite(CreateInMemoryDatabase())
                                            .Options);
            _dbContext.Database.EnsureCreated();
            _projectService = new ProjectService(_dbContext);
            InitialDatabase();
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }

        private void InitialDatabase()
        {
            User testUser1 = new User
            {
                Account = "github_testUser1",
                Name = "testUser1",
                Projects = new List<UserProject>(),
            };
            User testUser2 = new User
            {
                Account = "github_testUser2",
                Name = "testUser2",
            };
            User testUser3 = new User
            {
                Account = "github_testUser3",
                Name = "testUser3",
            };

            Project project1 = new Project
            {
                Name = "testProject1",
                Owner = testUser1,
            };
            Project project2 = new Project
            {
                Name = "testProject2",
                Owner = testUser1,
            };

            List<UserProject> testUser1Project = new List<UserProject>()
            {
                new UserProject
                {
                    User = testUser1,
                    Project = project1,
                },
                new UserProject
                {
                    User = testUser1,
                    Project = project2,
                },
            };

            List<UserProject> testUser2Project = new List<UserProject>()
            {
                new UserProject
                {
                    User = testUser2,
                    Project = project1,
                },
                new UserProject
                {
                    User = testUser2,
                    Project = project2,
                },
            };

            List<UserProject> testUser3Project = new List<UserProject>()
            {
                new UserProject
                {
                    User = testUser3,
                    Project = project1,
                },
            };

            testUser1.Projects = testUser1Project;
            testUser2.Projects = testUser2Project;
            testUser3.Projects = testUser3Project;

            _dbContext.Projects.Add(project1);
            _dbContext.Projects.Add(project2);
            _dbContext.Users.Add(testUser1);
            _dbContext.Users.Add(testUser2);
            _dbContext.Users.Add(testUser3);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void TestGetProjectMember()
        {
            List<UserInfoDto> projectMembers = _projectService.GetProjectMember(1);

            Assert.Equal(3, projectMembers.Count);
            Assert.Equal("testUser1", projectMembers[0].name);
        }

        [Fact]
        public void TestGetProjectMemberByNotExistProjectId()
        {
            Assert.Throws<Exception>(() => _projectService.GetProjectMember(4));
        }

        [Fact]
        public void TestGetProjectById()
        {
            ProjectResultDto result = _projectService.GetProjectByProjectId(1, "github_testUser1");

            Assert.Equal("testProject1", result.Name);
        }

        [Fact]
        public void TestGetProjectByIdButNotProjectMember()
        {
            Assert.Throws<Exception>(() => _projectService.GetProjectByProjectId(2, "github_testUser3"));
        }

        [Fact]
        public void TestGetAllProject()
        {
            List<ProjectResultDto> result = _projectService.GetAllProject();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void TestDeleteProjectMember()
        {
            _projectService.DeleteProjectMember("github_testUser3", 1);

            List<UserInfoDto> result = _projectService.GetProjectMember(1);

            Assert.Equal(2, result.Count);
        }
    }

}
