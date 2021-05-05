using System.Collections.Generic;

namespace project_manage_system_backend.Dtos
{
    // Request 與 Response 共用
    public class Author
    {
        public string login { get; set; }
        public string avatar_url { get; set; }
        public string html_url { get; set; }
        public string email { get; set; }
    }
    /// <summary>
    /// w - Start of the week, given as a Unix timestamp.
    /// a - Number of additions
    /// d - Number of deletions
    /// c - Number of commits
    /// ws - convert w to string date of week
    /// </summary>
    public class Week
    {
        public string ws { get; set; }
        public int w { get; set; }
        public int a { get; set; }
        public int d { get; set; }
        public int c { get; set; }
    }

    public class ContributorsCommitActivityDto
    {
        public Author author { get; set; }
        public List<Week> weeks { get; set; }
        public int total { get; set; }
        public int totalAdditions { get; set; }
        public int totalDeletions { get; set; }
        public string commitsHtmlUrl { get; set; }
    }
}
