using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PMS_test.ControllersTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class UserControllerTests : BaseControllerTests
    {
        public UserControllerTests() : base()
        {
            InitialDatabase();
        }

        internal void InitialDatabase()
        {
            _dbContext.Users.Add(new User
            {
                Account = "testAccount",
                Authority = "User",
                AvatarUrl = "none",
                Name = "name"
            });
            _dbContext.Users.Add(new User
            {
                Account = "testAccount2",
                Authority = "User2",
                AvatarUrl = "none2",
                Name = "name2"
            });
            var user = _dbContext.Users.Find("testAccount");
            var project = new UserProject
            {
                Project = new Project
                {
                    Name = "testproject",
                    Owner = user,
                    Repositories = null
                },
                User = user
            };
            user.Projects.Add(project);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task EditUserNameTest()
        {
            UserInfoDto dto = new UserInfoDto
            {
                name = "testEditUser",
            };

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var requestTask = await _client.PostAsync("/user/edit", content);

            Assert.True(requestTask.IsSuccessStatusCode);

            requestTask = await _client.GetAsync("/user");

            string resultContent = await requestTask.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<UserInfoDto>(resultContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            string expectedName = "testEditUser";

            Assert.Equal(expectedName, actual.name);
        }

    }
}

