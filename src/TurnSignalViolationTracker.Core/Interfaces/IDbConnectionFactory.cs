using System.Data;

namespace TurnSignalViolationTracker.Core.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
