using AccountsUpdate.Domain.Account;
using AccountsUpdate.Domain.Common;

namespace AccountsUpdate.Application.Services
{
    public class Repository : IRepository
    {
        public Result<int, Error> ExecuteQuery(string query)
        {
            return  23;
        }
    }
}
