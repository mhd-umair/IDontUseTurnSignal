using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TurnSignalViolationTracker.Core.Interfaces;
using TurnSignalViolationTracker.Infrastructure.Configuration;

namespace TurnSignalViolationTracker.Infrastructure.Data;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public SqlConnectionFactory(IOptions<DatabaseSettings> settings)
    {
        _settings = settings.Value;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_settings.ConnectionString);
    }
}
