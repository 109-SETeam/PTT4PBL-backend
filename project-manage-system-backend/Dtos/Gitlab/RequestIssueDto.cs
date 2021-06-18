namespace project_manage_system_backend.Dtos.Gitlab
{
    public class Author
    {
        public string username { get; set; }
        public string web_url { get; set; }
    }
    public class RequestIssueDto
    {
        /// <summary>
        /// number of issue sequence
        /// </summary>
        public int iid { get; set; }

        /// <summary>
        /// url of issue webside
        /// </summary>
        public string web_url { get; set; }

        /// <summary>
        /// title
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// user who proposed the issue
        /// </summary>
        public Author author { get; set; }

        /// <summary>
        /// state of issue
        /// </summary>
        public string state { get; set; }

        /// <summary>
        /// time of creation
        /// </summary>
        public string created_at { get; set; }

        /// <summary>
        /// time of close
        /// </summary>
        public string closed_at { get; set; }
    }
}
