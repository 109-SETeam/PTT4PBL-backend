using System.Collections.Generic;

namespace project_manage_system_backend.Dtos
{
    public class CodeSmellDataDto
    {
        public int total { get; set; }
        public List<Issues> issues { get; set; }
    }

    public class Issues
    {
        public string key { get; set; }
        public string severity { get; set; }
        public string component { get; set; }
        public int line { get; set; }
        public string message { get; set; }
    }
}
