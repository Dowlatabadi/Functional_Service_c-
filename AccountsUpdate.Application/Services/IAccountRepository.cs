using AccountsUpdate.Domain.Common;
namespace AccountsUpdate.Application.Services;
public interface IAccountRepository{

        public Result<int, Error> ExecuteQuery(string query);
}
public class DBOptions
{
    public bool usesql { get; set; }
    public string PostgreSqlConnectionString { get; set;  }
}