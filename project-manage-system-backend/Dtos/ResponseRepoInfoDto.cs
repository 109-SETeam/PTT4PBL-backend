namespace project_manage_system_backend.Dtos
{
    public class Owner
    {
        public string login { get; set; }

        /// <summary>
        /// only for gitlab
        /// </summary>
        public string name { get; set; }
        public string username { get; set; }
        public string avatar_url { get; set; }
        public string web_url { get; set; }
    }

    public class ResponseRepoInfoDto
    {
        public bool success { get; set; }
        public string message { get; set; }
        /// <summary>
        /// only for github
        /// </summary>
        public string name { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public Owner owner { get; set; }
        /// <summary>
        /// only for gitlab
        /// </summary>
        public string web_url { get; set; }
        public int id { get; set; }
    }
}
