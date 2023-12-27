namespace AccountsUpdate.Domain.Common;

public record LogEvent(string Code, string Description, string CorrelationId="") {

    public static readonly LogEvent Incomming = new("Account.Incomming", "The request has been received in pipeline.");
    public static readonly LogEvent HandingReuest = new("Account.StartingHandler", "The Account update request is being handled.");
    public static readonly LogEvent NotFound = new("Account.NotFound", "The AcountId could not be found.");
    public static readonly LogEvent OneUpdate = new("Account.OneUpdated", "The Acount has been updated successfully.");
    public static readonly LogEvent MultipleUpdated = new("Account.MultipleUpdated", "The AcountId updated multiple associated accounts.");


}


public static class EventExtensions {
public static LogEvent AddCorelationId(this LogEvent logEvent,string CorrelationId)=>new(logEvent.Code,logEvent.Description,CorrelationId);

}
