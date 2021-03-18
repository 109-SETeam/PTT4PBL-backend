namespace project_manage_system_backend.Dtos
{
    public class Owner
    {
        public string login { get; set; }
    }

    public class ResponseGithubRepoInfoDto
    {
        public bool IsSucess { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public Owner owner { get; set; }
        public string message { get; set; }
    }
}
