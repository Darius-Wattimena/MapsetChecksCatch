using System.Collections.Generic;
using MapsetVerifier.Framework.Objects;

namespace MapsetChecksCatch.Helper
{
    public static class IssueUtil
    {
        public static void AddIfNotNull(this List<Issue> issues, Issue issue)
        {
            if (issue != null)
            {
                issues.Add(issue);
            }
        }
    }
}