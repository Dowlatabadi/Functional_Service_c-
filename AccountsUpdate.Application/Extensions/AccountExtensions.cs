using AccountsUpdate.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AccountsUpdate.Domain.Account;
using AccountsUpdate.Application.Services;
using System.Reflection;
using MassTransit;

namespace AccountsUpdate.Application.Extensions
{
    public enum UserChangeType
    {
        UserCreated = 0,
        CustomerCreated = 1,
        MobileChanged = 2,
        EmailChanged = 3,
        ActiveStatusChanged = 4
    }
    public static class AccountExtensions
    {
      
        public static Dictionary<UserChangeType, (string fieldName, string fieldRX)> ChangeToValue = new Dictionary<UserChangeType, (string name, string valueRX)> {
    {  UserChangeType.MobileChanged,(nameof(Account.PhoneNumber),@"\|11=([0-9]{7,})\|") },
    {  UserChangeType.EmailChanged,(nameof(Account.Email),@"\|12=(*{5,})\|") },
    {  UserChangeType.CustomerCreated,(nameof(Account.NationalIdConfirmed),"") },
};
        //appendProp(11, phoneNumber);
        //appendProp(12, email);
        //appendProp(13, user.HasNationalId()? user.NationalId : null);
        //appendProp(14, firstName);
        //appendProp(15, lastName);
        //appendProp(16, user.HasNationalId()? $"1129{user.NationalId}" : null);
        //appendProp(17, user.IsActive? "1" : "0");
        //appendProp(18,"ChangeByAdminPanel");


      
    }
    public static class AccountEvents{

        public static readonly LogEvent SingleAffected = new("Account.SingleAffected", "single row affected during update.");
    }
    public static class AccountErrors
    {
        public static readonly Error UnAffected = new("Account.UnAffected", "unaffected during update.");
        public static readonly Error MultipleAffected = new("Account.MultipleAffected", "multiple affected during update.");
        public static readonly Error Unhandled = new("Account.Unhandled", "unhandled error during update.");
        public static readonly Error Update = new("Account.Update", "error during update.");
        public static readonly Error Map = new("Account.Mapping", "error during mapping the change.");
        public static readonly Error CantMapChange = new("Account.CantMapChange", "error extracting national code.");
        public static readonly Error ExtractValues = new("Account.ExtractFieldValues", "error extracting field value.");
        public static readonly Error ExtractAccountIdValue = new("Account.ExtractNationalIdValue", "error extracting field value.");
        public static readonly Error ExtractEmailValue = new("Account.ExtractEmailValue", "error extracting field value.");
        public static readonly Error ExtractPhoneNumberValue = new("Account.ExtractPhoneNumberValue", "error extracting field value.");
        public static readonly Error ExtractRequiredValue = new("Account.ExtractFieldValues", "error extracting field value.");
    }

    public record AccountEvent(UserChangeType UserChangeType, string valueRX);
    record AccountFieldInfo(string fieldName, Regex fieldRX)
    {
        public static AccountFieldInfo AccountIdInfo = new(nameof(Account.AccountId), new Regex(@"1=([^|]{7,})"));
        public static AccountFieldInfo PhoneNumberInfo = new(nameof(Account.PhoneNumber), new Regex(@"11=([0-9]{7,})"));
        public static AccountFieldInfo NationalIdInfo = new(nameof(Account.NationalId), new Regex(@"13=([0-9]{9,})"));
        public static AccountFieldInfo EmailInfo = new(nameof(Account.Email), new Regex(@"\|12=([^|]{5,})"));
        public static AccountFieldInfo FirstNameInfo = new(nameof(Account.FirstName), new Regex(@"14=([^|]{1,})"));
        public static AccountFieldInfo LastNameInfo = new(nameof(Account.LastName), new Regex(@"15=([^|]{1,})"));
        public static AccountFieldInfo NationalConfirmed = new(nameof(Account.NationalIdConfirmed), new Regex(@"17=([^|]{1,})"));
        public static AccountFieldInfo ChangeTypeValue = new(nameof(Account.UserChangeType), new Regex(@"3=(.)"));
        public static AccountFieldInfo ChangedBy = new(nameof(Account.UserChangeType), new Regex(@"18=([^|]{5,})"));
        public static AccountFieldInfo IsActive = new(nameof(Account.UserChangeType), new Regex(@"18=([^|]{5,})"));
        
    }
    public record Change(TrackableMessage TrackableMessage,  Account? Account = null)
    {
        public Result<Change,Error> FillDataFields()
        {
            try
            {
                var ret= this with
                {
                    Account = new Account
                    {
                         AccountId= AccountFieldInfo.AccountIdInfo.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? string.Empty,
                        Email = AccountFieldInfo.EmailInfo.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? string.Empty,
                        FirstName = AccountFieldInfo.FirstNameInfo.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? string.Empty,
                        LastName = AccountFieldInfo.LastNameInfo.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? string.Empty,
                        NationalId = AccountFieldInfo.NationalIdInfo.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? string.Empty,
                        PhoneNumber = AccountFieldInfo.PhoneNumberInfo.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? string.Empty,
                        NationalIdConfirmed = AccountFieldInfo.NationalConfirmed.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? string.Empty,
                        UserChangeType=Convert.ToByte( AccountFieldInfo.ChangeTypeValue.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? "1"),
                        ChangeBy=AccountFieldInfo.ChangedBy.fieldRX.Match(TrackableMessage.Message).Groups?[1]?.Value ?? "ChangeByUser",
                    }
                };
                return ret;
            }
            catch (Exception ex)
            {
                //log maybe
                return AccountErrors.ExtractValues;
            }
        }
        public MobileChange ToMobileChange() => new MobileChange(TrackableMessage,Account);
        public EmailChange ToEmailChange() => new EmailChange(TrackableMessage,Account);
        public ConfirmtionChange ToConfirmtionChange() => new ConfirmtionChange(TrackableMessage,Account);
        public ActivatedChange ToActivatedChange() => new ActivatedChange(TrackableMessage,Account);
        public CustomerCreated ToCreationChange() => new CustomerCreated(TrackableMessage,Account);
        public Result<Change, Error> MapToChange() => Account.UserChangeType switch
        {
            (int)UserChangeType.UserCreated => ToCreationChange(),
            (int)UserChangeType.CustomerCreated => ToConfirmtionChange(),
            (int)UserChangeType.EmailChanged => ToEmailChange(),
            (int)UserChangeType.MobileChanged => ToMobileChange(),
            (int)UserChangeType.ActiveStatusChanged => ToActivatedChange(),
            //(int)UserChangeType.ActiveStatusChanged => this.,
            _ => AccountErrors.CantMapChange,
        };
        public Result<string,Error> GenerateQuery()
          => this switch
          {
              //errors
              Change Change when string.IsNullOrEmpty( Change.Account.AccountId)  => AccountErrors.ExtractAccountIdValue,
              EmailChange EmailChange when string.IsNullOrEmpty(EmailChange.Account.Email) && string.Equals(EmailChange.Account.ChangeBy, "ChangeByUser") => AccountErrors.ExtractEmailValue,
              MobileChange MobileChange when string.IsNullOrEmpty(MobileChange.Account.PhoneNumber) && string.Equals(MobileChange.Account.ChangeBy, "ChangeByUser") => AccountErrors.ExtractPhoneNumberValue,
         
              EmailChange EmailChange when string.IsNullOrEmpty(EmailChange.Account.Email) => $"update {PGNameMappings.tableName} set {PGNameMappings.Email }='{PGNameMappings.Email }' where  {PGNameMappings.AccountId}='{EmailChange.Account?.AccountId}'",
              MobileChange MobileChange => $"update {PGNameMappings.tableName} set {PGNameMappings.PhoneNumber}='{MobileChange.Account.PhoneNumber}' where  {PGNameMappings.AccountId}='{MobileChange.Account?.AccountId}'",
           //   ConfirmtionChange ConfirmtionChange => $"update {PGNameMappings.tableName} set {PGNameMappings.NationalIdConfirmed}='{ConfirmtionChange.Account.NationalIdConfirmed}' where  {PGNameMappings.AccountId}='{PGNameMappings.AccountId}'",
           //   ActivatedChange ActivatedChange => $"update {PGNameMappings.tableName} set {PGNameMappings.IsActive}=1 where  {PGNameMappings.AccountId}='{ActivatedChange.Account?.AccountId}'",
              CustomerCreated CustomerCreated => $@"INSERT INTO {PGNameMappings.tableName} ({PGNameMappings.AccountId}, {PGNameMappings.PhoneNumber}, {PGNameMappings.Email}, {PGNameMappings.FirstName}, {PGNameMappings.LastName},{PGNameMappings.created_at},{PGNameMappings.modified_at})
VALUES('{CustomerCreated.Account?.AccountId}', '{CustomerCreated.Account?.PhoneNumber}', '{CustomerCreated.Account?.Email}', N'{CustomerCreated.Account?.FirstName}', N'{CustomerCreated.Account?.LastName}','{DateTimeOffset.Now:yyyy-MM-dd HH:mm:sszzz}','{DateTimeOffset.Now:yyyy-MM-dd HH:mm:sszzz}')",
              _ => ""
          };

    }
    public static class SQLNameMappings
    {
        public static string tableName { get; } = "Accounts";
        public static string FirstName { get; } = "FirstName";
        public static string LastName { get; } = "LastName";
        public static string NationalIdConfirmed { get; } = "NationalIdConfirmed";
       // public static string PhoneNumber { get; } = "PhoneNumber";
        public static string Email { get; } = "Email";
        public static string AccountId { get; } = "AccountId";
        public static string IsActive { get; } = "IsActive";

    }
    public static class PGNameMappings
    {
        public static string tableName { get; } = "customer_customer";
        public static string FirstName { get; } = "first_name";
        public static string LastName { get; } = "last_name";
        public static string NationalIdConfirmed { get; } = "NationalIdConfirmed";
        public static string PhoneNumber { get; } = "phone_number";
        public static string Email { get; } = "email";
        public static string AccountId { get; } = "customer_pk";
        public static string IsActive { get; } = "IsActive";
        public static string created_at { get; } = "created_at";
        public static string modified_at { get; } = "modified_at";
    }
    public sealed record MobileChange(TrackableMessage TrackableMessage, Account? Account) : Change(TrackableMessage,Account);
    public sealed record EmailChange(TrackableMessage TrackableMessage, Account? Account) : Change(TrackableMessage,Account);
    public sealed record ConfirmtionChange(TrackableMessage TrackableMessage, Account? Account) : Change(TrackableMessage, Account);
    public sealed record ActivatedChange(TrackableMessage TrackableMessage, Account? Account) : Change(TrackableMessage, Account);
    public sealed record CustomerCreated(TrackableMessage TrackableMessage, Account? Account) : Change(TrackableMessage,Account);

}
