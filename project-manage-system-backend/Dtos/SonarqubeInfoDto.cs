using System.Collections.Generic;

namespace project_manage_system_backend.Dtos
{

    public class SonarqubeInfoDto
    {
        public List<Measure> measures { get; set; }
        public string projectName { get; set; }
    }

    public class Measure
    {
        public string metric { get; set; }
        public string value { get; set; }
        public string component { get; set; }
        public bool bestValue { get; set; }
    }
}
