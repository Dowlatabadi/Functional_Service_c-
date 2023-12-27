using AccountsUpdate.Application.Extensions;
using AccountsUpdate.Domain.Account;
using AccountsUpdate.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccountsUpdate.Application.Services
{
    public class UpdateService : IUpdateService
    {
        //private readonly ConfigurationManager _conf;
        private readonly IAccountRepository _repo;
        private readonly ILogger _logger;

        public UpdateService(IAccountRepository repo, ILogger logger)
        {
            _repo = repo;
            _logger = logger;
        }
        public Result<int, Error> UpdateAccount(TrackableMessage trackableMessage)
        {
            return new Change(trackableMessage).FillDataFields().Match(
               change => change.MapToChange().
                Match(change => change.GenerateQuery().Match(
                    query => _repo.ExecuteQuery(query),
                    error => error
                    ),
                    error => error
                )
                , error => error);
        }
    }

}
