namespace project_manage_system_backend.Models
{
    public class Repo
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
        /// <summary>
        /// 實際 repository (第三方) 的擁有者名稱
        /// </summary>
        public string Owner { get; set; }

        public Project Project { get; set; }
        /// <summary>
        /// 有無sonarqube
        /// </summary>
        public bool IsSonarqube { get; set; }
        /// <summary>
        /// foramt: "帳號:密碼"
        /// encryption: base64
        /// </summary>
        public string AccountColonPw { get; set; }

        /// <summary>
        /// Sonarqube url
        /// </summary>
        public string SonarqubeUrl { get; set; }

        /// <summary>
        /// sonarqube projectkey
        /// </summary>
        public string ProjectKey { get; set; }

        /// <summary>
        /// gitlab repository id
        /// </summary>
        public string RepoId { get; set; }
    }
}
