using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for securedtokenwebservice
/// </summary>
public class securedtokenwebservice: System.Web.Services.Protocols.SoapHeader
{
    public string username { get; set; }
    public string password { get; set; }
    public string authenticationtoken { get; set; }
	
    public bool isusercredentialsvalid(string username, string password)
    {
        //databaseconection
        if (username == "admin" && password == "admin")
            return true;
        else
            return false;

    }

    public bool isusercredentialsvalid(securedtokenwebservice soapheader)
    {
        if (soapheader == null)
            return false;
        if (!string.IsNullOrEmpty(soapheader.authenticationtoken))
            return (HttpRuntime.Cache[soapheader.authenticationtoken] != null);
            return false;
    }
}