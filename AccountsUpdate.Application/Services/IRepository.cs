using AccountsUpdate.Domain.Account;
using AccountsUpdate.Domain.Common;

namespace AccountsUpdate.Application.Services
{
    public interface IRepository
    {
        public Result<int, Error> ExecuteQuery(string query);
    }
}
