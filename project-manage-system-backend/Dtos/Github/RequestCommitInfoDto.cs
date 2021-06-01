using System.Collections.Generic;

namespace project_manage_system_backend.Dtos
{
    public class WeekTotalData
    {
        // First day of the week
        public string Week { get; set; }
        // Total commits of the week
        public int Total { get; set; }
    }

    public class DayCommit
    {
        // Day of the week
        public string Day { get; set; }
        // Commit count of the day
        public int Commit { get; set; }
    }

    public class DayOfWeekData
    {
        // First day of the week
        public string Week { get; set; }

        public List<DayCommit> DetailDatas { get; set; }
    }

    public class RequestCommitInfoDto
    {
        public List<WeekTotalData> WeekTotalData { get; set; }

        public List<DayOfWeekData> DayOfWeekData { get; set; }
    }
}
