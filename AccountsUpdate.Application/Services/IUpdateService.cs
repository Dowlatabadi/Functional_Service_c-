using AccountsUpdate.Domain.Common;
namespace AccountsUpdate.Application.Services
{
    public interface IUpdateService
    {
        public Result<int,Error> UpdateAccount(TrackableMessage message);
    }
    public sealed record TrackableMessage(string Message,string CorrelationId);
}