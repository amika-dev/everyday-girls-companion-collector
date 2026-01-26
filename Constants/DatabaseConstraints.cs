namespace EverydayGirlsCompanionCollector.Constants
{
    /// <summary>
    /// SQL constraint definitions used in migrations and DbContext configuration.
    /// </summary>
    public static class DatabaseConstraints
    {
        /// <summary>
        /// SQL Server CHECK constraint for DisplayName validation (4-16 chars, alphanumeric only).
        /// Note: Uses SQL Server-specific functions (LEN, LIKE with character class patterns).
        /// </summary>
        public const string DisplayNameCheckConstraintSql = 
            "LEN([DisplayName]) >= 4 AND LEN([DisplayName]) <= 16 AND [DisplayName] NOT LIKE '%[^a-zA-Z0-9]%'";
    }
}