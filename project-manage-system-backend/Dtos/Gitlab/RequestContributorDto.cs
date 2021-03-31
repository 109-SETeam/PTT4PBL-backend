using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_manage_system_backend.Dtos
{
    public class RequestContributorDto
    {
        public string name { get; set; }
        public string email { get; set; }
        public int commits { get; set; }
    }
}
