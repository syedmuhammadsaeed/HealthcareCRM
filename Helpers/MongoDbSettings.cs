namespace HealthcareCRM.Helpers
{
    /// <summary>
    /// Configuration settings for MongoDB Atlas connection.
    /// Bound from the "MongoDbSettings" section in appsettings.json.
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>Gets or sets the MongoDB Atlas connection string.</summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>Gets or sets the target database name.</summary>
        public string DatabaseName { get; set; } = string.Empty;
    }
}
