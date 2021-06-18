namespace project_manage_system_backend.Dtos
{
    public class user
    {
        /// <summary>
        /// uesr name who proposed the issue 
        /// </summary>
        public string login { get; set; }

        /// <summary>
        /// user url
        /// </summary>
        public string html_url { get; set; }
    }

    // Response 與 Request 共用
    public class ResponseRepoIssuesDto
    {
        /// <summary>
        /// issue number
        /// </summary>
        public int number { get; set; }

        /// <summary>
        /// issue url
        /// </summary>
        public string html_url { get; set; }

        /// <summary>
        /// issue title
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// uesr who proposed the issue 
        /// </summary>
        public user user { get; set; }

        /// <summary>
        /// issue state
        /// </summary>
        public string state { get; set; }

        /// <summary>
        /// issue created datetime
        /// </summary>
        public string created_at { get; set; }

        /// <summary>
        /// issue closed datetime
        /// </summary>
        public string closed_at { get; set; }
    }
}
