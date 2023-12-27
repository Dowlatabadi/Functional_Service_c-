//using System.Data;
//using System.Data.Common;
//using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AccountsUpdate.Domain;
using AccountsUpdate.Domain.Account;
using AccountsUpdate.Domain.Common;
using AccountsUpdate.Application.Services;
using MassTransit.Configuration;
using Microsoft.Extensions.Options;

namespace AccountsUpdate.Infrastructure;
public class AccountRepository : IAccountRepository
{
    public AccountsDbContext DbContext { get; set; }
    public AccountRepository(AccountsDbContext dbContext)
    {
        DbContext = dbContext;
    }
    public Result<int, Error> ExecuteQuery(string query)
    {
        if (string.IsNullOrEmpty(query)) return 0;
        return DbContext.Database.ExecuteSqlRaw(query);
    }
}

public class AccountsDbContext : DbContext
{
    private readonly DBOptions _dbOptions;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!_dbOptions.usesql)
        {
            optionsBuilder.UseNpgsql();
        }
    }
    public DbSet<Account> Accounts { get; set; }
  public AccountsDbContext(DbContextOptions<AccountsDbContext> options,IOptions<DBOptions> dbOptions) : base(options)
    {
        _dbOptions = dbOptions.Value;
    }
 
}
