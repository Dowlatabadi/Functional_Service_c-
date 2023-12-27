namespace AccountsUpdate.Tests;
using Application.Services;
using AccountsUpdate.Application.Extensions;
using AccountsUpdate.Domain.Account;
using AccountsUpdate.Domain.Common;

public class ChangeGenerationTests
{
    //public enum UserChangeType
    //{
    //    UserCreated = 0,
    //    CustomerCreated = 1,
    //    MobileChanged = 2,
    //    EmailChanged = 3,
    //    ActiveStatusChanged = 4
    //}
//    user create = NationalIdConfirmed = 0
//CustomerCreated = NationalIdConfirmed=1
//در واقع user create
//مشتری اکانت ساخته
//coustomer created
//میشه مشتری شده و کد ملی تایید شده
    //appendProp(11, phoneNumber);
    //appendProp(12, email);
    //appendProp(13, user.HasNationalId()? user.NationalId : null);
    //appendProp(14, firstName);
    //appendProp(15, lastName);
    //appendProp(16, user.HasNationalId()? $"1129{user.NationalId}" : null);
    //appendProp(17, user.IsActive? "1" : "0");
    //appendProp(18,"ChangeByAdminPanel");

    [Fact]
    public void TestChangePhoneNumber()
    {
        var res = new Change(new TrackableMessage("1=8e286045-6f7f-455c-b92f-b4ca6de65a90|2=638286636430410046|3=2|4=1|13=1360561481|16=11291360561481|17=1|11=09037891788|18=ChangeByAdminPanel", "ssssss3424"))
                    .FillDataFields();
        Assert.Equal( "09037891788",(res).Match(x=>x.Account.PhoneNumber,e=>""));
    }


    [Fact]
    public void TestChangeNationalConfirmed()
    {
        var res = new Change(new TrackableMessage("1=8e286045-6f7f-455c-b92f-b4ca6de65a90|2=638286636430410046|3=4|7=1|13=1360561481|16=11291360561481|17=1|11=09037891788|18=ChangeByAdminPanel", "ssssss3424"))
                    .FillDataFields();
        Assert.Equal("1", (res).Match(x => x.Account.NationalIdConfirmed, e => ""));
    }

    [Fact]
    public void TestChangeEmail()
    {
        //
        var res = new Change(new TrackableMessage("1=8e286045-6f7f-455c-b92f-b4ca6de65a90|12=m@n.com|3=3|7=1|13=1360561481|16=11291360561481|17=1|11=09037891788|18=ChangeByAdminPanel", "ssssss3424"))
            .FillDataFields();
        Assert.Equal("m@n.com", (res).Match(x => x.Account.Email, e => ""));
    }

}