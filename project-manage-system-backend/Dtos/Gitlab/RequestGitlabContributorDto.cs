using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_manage_system_backend.Dtos
{
    public class Stats
    {
        public int additions { get; set; }
        public int deletions { get; set;}
        public int total { get; set;}
    }
    public class RequestGitlabContributorDto
    {
        public string name { get; set; }
        public string email { get; set; }
        public int commits { get; set; }
        public Stats stats{get; set;}
    }
}
