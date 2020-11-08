using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Summary description for tokenauth
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class tokenauth : System.Web.Services.WebService
{

    public securedtokenwebservice soapheader;
    [WebMethod]
    [System.Web.Services.Protocols.SoapHeader("soapheader")]
    public string authenticationuser()
    {
        if (soapheader == null)
            return "please provide username and password";
        if (string.IsNullOrEmpty(soapheader.username) || string.IsNullOrEmpty(soapheader.password))
            return "please provide username and password";
        if (!soapheader.isusercredentialsvalid(soapheader.username, soapheader.password))
            return "username and password";
        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
        string token = Guid.NewGuid().ToString();
        HttpRuntime.Cache.Add(token, soapheader.username, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30), System.Web.Caching.CacheItemPriority.NotRemovable, null);
        return token;
    }

    [WebMethod]
    [System.Web.Services.Protocols.SoapHeader("soapheader")]
    public string HelloWorld()
    {
        if (soapheader == null)
            return "please call  authenticationmethod() first.";
        if (!soapheader.isusercredentialsvalid(soapheader))
            return "Hello" + HttpRuntime.Cache[soapheader.authenticationtoken];
        return "Hello" + HttpRuntime.Cache[soapheader.authenticationtoken];
    }

}
