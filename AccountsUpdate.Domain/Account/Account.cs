using AccountsUpdate.Domain.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AccountsUpdate.Domain.Account;

public class Account
{

    public string AccountId { get; set; }
    public string NationalId { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NationalIdConfirmed { get; set;  }
    public string ChangeBy { get; set; }
    public byte UserChangeType { get; set; }
    public byte IsActive { get; set;  }
}