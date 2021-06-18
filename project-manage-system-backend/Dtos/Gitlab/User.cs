using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_manage_system_backend.Dtos.Gitlab
{
    public class User
    {
        public int id { get; set; }
        public string username {get; set;}
        public string avatar_url {get; set;}
        public string web_url {get; set;}
    }
}
