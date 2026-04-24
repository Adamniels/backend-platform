namespace Platform.Infrastructure.Persistence;

internal static class StatsSeedJson
{
    public const string Value =
        """
        {"tiles":[{"label":"Sessions Completed","value":24,"unit":"","color":"var(--accent)","sub":"+3 this week"},{"label":"Articles Read","value":187,"unit":"","color":"var(--accent)","sub":"+12 this week"},{"label":"Saved Items","value":43,"unit":"","color":"rgba(232,237,248,0.45)","sub":"5 added recently"},{"label":"Day Streak","value":12,"unit":"d","color":"#34d399","sub":"Personal best: 18d"},{"label":"Avg Session Length","value":38,"unit":"min","color":"var(--accent)","sub":"Target: 45 min"},{"label":"Topics Explored","value":9,"unit":"","color":"#a78bfa","sub":"3 in progress"},{"label":"Knowledge Score","value":94,"unit":"%","color":"#34d399","sub":"Top 8% of users"},{"label":"Hours Learned","value":41,"unit":"h","color":"#fbbf24","sub":"This month"}],"progress":[{"label":"Weekly Learning Goal","value":68,"color":"var(--accent)"},{"label":"AI Ethics Mastery","value":82,"color":"var(--accent)"},{"label":"Reading Streak","value":45,"color":"#34d399"},{"label":"Profile Completion","value":91,"color":"#a78bfa"}],"activity":[{"day":"Mon","sessions":2},{"day":"Tue","sessions":1},{"day":"Wed","sessions":3},{"day":"Thu","sessions":0},{"day":"Fri","sessions":2},{"day":"Sat","sessions":1},{"day":"Sun","sessions":2}]}
        """;
}
