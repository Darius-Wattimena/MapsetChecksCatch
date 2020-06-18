using System.Collections.Generic;
using MapsetVerifierFramework.objects;

namespace MapsetChecksCatch.helper
{
    public static class Extension
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
