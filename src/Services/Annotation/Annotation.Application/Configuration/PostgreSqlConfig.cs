using System.Text.Json.Serialization;

namespace PreciPoint.Ims.Services.Annotation.Application.Configuration;

public class PostgreSqlConfig
{
    /// <summary>
    /// The host that provides access to the database.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// The port in use to connect.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The database the service connects to.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// User-name credentials decide if a service is allowed to connect or not.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// The password must match with the corresponding user-name to establish a connection.
    /// </summary>
    [JsonIgnore]
    public string Password { get; set; }

    /// <summary>
    /// Transforms all given parameters to a connection string that can be passed over to the database connection factory.
    /// </summary>
    /// <returns>The resulting string could be used directly as connection parameter.</returns>
    public string ParametersToConnectionString()
    {
        return $"Server={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
    }
}