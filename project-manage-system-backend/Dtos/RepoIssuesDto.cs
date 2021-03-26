using System.Collections.Generic;

namespace project_manage_system_backend.Dtos
{
    public class RepoIssuesDto
    {
        public string averageDealWithIssueTime { get; set; }

        public List<ResponseRepoIssuesDto> openIssues { get; set; }

        public List<ResponseRepoIssuesDto> closeIssues { get; set; }
    }
}
