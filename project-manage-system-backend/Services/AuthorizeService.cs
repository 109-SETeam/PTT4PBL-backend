using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Configuration;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Models;
using project_manage_system_backend.Shares;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace project_manage_system_backend.Services
{
    public class AuthorizeService : BaseService
    {
        private readonly JwtHelper _jwtHelper;
        private readonly UserService _userService;
        private readonly HttpClient _httpClient;
        public AuthorizeService(PMSContext dbContext,
                                IConfiguration configuration,
                                JwtHelper jwt,
                                HttpClient client = null) : base(dbContext, configuration)
        {
            _configuration = configuration;
            _jwtHelper = jwt;
            _httpClient = client ?? new HttpClient();

            _userService = new UserService(dbContext);
        }

        public async Task<AuthorizeDto> AuthenticateGithub(GithubLoginDto dto)
        {
            string accessToken = await RequestGithubAccessToken(dto.Code);

            if (!string.IsNullOrEmpty(accessToken))
            {
                User user = await RequestGithubUserInfo(accessToken);

                if (!_userService.CheckUserExist(user.Account))
                {
                    _userService.CreateUser(user);
                }

                return new AuthorizeDto
                {
                    Token = _jwtHelper.GenerateToken(user.Account, accessToken, user.Authority),
                    Authority = user.Authority,
                    UserAccount = user.Account
                };
            }
            else
            {
                throw new Exception("error code");
            }
        }

        public AuthorizeDto AuthenticateLocal(LocalAccountDto dto)
        {
            User loginUser = _dbContext.Users.Find(dto.Account);

            if (loginUser != null && Argon2.Verify(loginUser.Password, dto.Password))
            {
                string accessToken = _configuration.GetValue<string>("githubConfig:admin_token");
                return new AuthorizeDto
                {
                    Token = _jwtHelper.GenerateToken(loginUser.Account, accessToken, loginUser.Authority),
                    Authority = loginUser.Authority,
                    UserAccount = loginUser.Account
                };
            }
            return null;
        }

        public async Task<string> RequestGithubAccessToken(string code)
        {
            const string url = "https://github.com/login/oauth/access_token";
            string clientId = _configuration.GetValue<string>("githubConfig:client_id");
            string clientSecret = _configuration.GetValue<string>("githubConfig:client_secret");

            RequestOauthDto requestData = new RequestOauthDto
            {
                Client_id = clientId,
                Client_secret = clientSecret,
                Code = code
            };

            var stringContent = new StringContent(JsonSerializer.Serialize(requestData).ToLower(), Encoding.UTF8, "application/json");
            var responseTask = await _httpClient.PostAsync(url, stringContent);

            string resultContent = await responseTask.Content.ReadAsStringAsync();

            string result = null;
            resultContent.Split('&').ToList().ForEach(x =>
            {
                if (x.Contains("access_token"))
                {
                    result = x.Split('=').ToList()[1];
                }
            });
            return result;
        }

        public async Task<User> RequestGithubUserInfo(string accessToken)
        {
            const string url = "https://api.github.com/user";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");
            var responseTask = await _httpClient.GetAsync(url);

            string resultContent = await responseTask.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<ResponseUserInfoDto>(resultContent);//反序列化
            User result = new User
            {
                Account = "github_" + userInfo.login,
                Name = userInfo.login,
                AvatarUrl = userInfo.avatar_url,
                Authority = "User"
            };
            return result;
        }
    }
}
