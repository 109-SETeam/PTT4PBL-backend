using System.Collections.Generic;

namespace project_manage_system_backend.Dtos
{
    public class WeekTotalData
    {
        public string Week { get; set; }

        public int Total { get; set; }
    }

    public class DayCommit
    {
        public string Day { get; set; }

        public int Commit { get; set; }
    }

    public class DayOfWeekData
    {
        public string Week { get; set; }

        public List<DayCommit> DetailDatas { get; set; }
    }

    public class CommitInfoDto
    {
        public List<WeekTotalData> WeekTotalData { get; set; }

        public List<DayOfWeekData> DayOfWeekData { get; set; }
    }
}
