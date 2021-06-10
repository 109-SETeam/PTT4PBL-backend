using System;
using System.Collections.Generic;

namespace project_manage_system_backend.Dtos.Gitlab
{
    public class Stats
    {
        public int additions { get; set; }
        public int deletions { get; set; }
        public int total { get; set; }
    }

    public class RequestCommitsDto
    {
        public string committer_name { get; set; }
        public string committer_email { get; set; }
        public DateTime committed_date { get; set; }
        public Stats stats { get; set; }
        public List<string> parent_ids { get; set; }
    }
}
