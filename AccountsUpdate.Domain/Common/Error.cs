namespace AccountsUpdate.Domain.Common;

public sealed record Error(string Code,string Description):LogEvent(Code,Description)
{
    public static readonly Error None = new(string.Empty,string.Empty);
    public static readonly Error NullValue = new("Error.NullValue","Null value was provided.");
   
}
