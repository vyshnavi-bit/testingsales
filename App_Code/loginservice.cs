using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using Npgsql;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.IO;


/// <summary>
/// Summary description for loginservice
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class loginservice : System.Web.Services.WebService {
    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();
    NpgsqlCommand postcmd;
    SAPdbmanger postvdm = new SAPdbmanger();

    private string HttpPostRequest(string url, string postParameters)
    {
        string postData = postParameters;
        HttpWebRequest myHttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
        myHttpWebRequest.Method = "POST";
        myHttpWebRequest.KeepAlive = false;
        byte[] data = Encoding.ASCII.GetBytes(postData);
        myHttpWebRequest.ContentType = "application/json; charset=utf-8";
        myHttpWebRequest.MediaType = "application/json";
        myHttpWebRequest.Accept = "application/json";
        myHttpWebRequest.ContentLength = data.Length;
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
        using (StreamWriter streamWriter = new StreamWriter(myHttpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(postParameters);
            streamWriter.Flush();
            streamWriter.Close();
        }
        HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
        Stream responseStream = myHttpWebResponse.GetResponseStream();
        StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);
        string pageContent = myStreamReader.ReadToEnd();
        myStreamReader.Close();
        responseStream.Close();
        myHttpWebResponse.Close();
        return pageContent;
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void GuestEMPloginservice(string username, string uuid, string version, string manufacturer, string platform, string model)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            string userid = username;
            string imeiid = uuid;
            DataTable dtresourceallocation = new DataTable();
            List<emplogins> surveybenelist = new List<emplogins>();
            cmd = new SqlCommand("SELECT  EmployeeCode, UserName, Password, Gender, GovernmentIdType, GovernmentIdNumber FROM   tbl_MST_GuestEmployee WHERE (EmployeeCode = @empid)");
            cmd.Parameters.Add("@empid", userid);
            DataTable dtgustempdetails = vdm.SelectQuery(cmd).Tables[0];
            if (dtgustempdetails.Rows.Count > 0)
            {
                DateTime currenttime = DateTime.Now;
                DateTime sessiontokenexpirytime = currenttime.AddHours(3);
                string token = Guid.NewGuid().ToString().ToUpper();
                cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE EmployeeCode = @empcode AND IsActive=@active");
                cmd.Parameters.Add("@empcode", userid);
                cmd.Parameters.Add("@active", true);
                DataTable dtimeidtls = vdm.SelectQuery(cmd).Tables[0];
                if (dtimeidtls.Rows.Count > 0)
                {
                    foreach (DataRow dri in dtimeidtls.Rows)
                    {
                        string imei = dri["IMEI"].ToString();
                        if (imei == uuid)
                        {
                            // close previes auth token
                            cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                            cmd.Parameters.Add("@empcode", userid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@status", false);
                            vdm.Update(cmd);
                            //  insert new token
                            cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                            cmd.Parameters.Add("@UserID", userid);
                            cmd.Parameters.Add("@EmployeeCode", userid);
                            cmd.Parameters.Add("@Domain", "HCLTECH");
                            cmd.Parameters.Add("@SessionToken", token);
                            cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                            cmd.Parameters.Add("@LoginTime", currenttime);
                            cmd.Parameters.Add("@DeviceID", uuid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                            cmd.Parameters.Add("@DeviceModel", model);
                            cmd.Parameters.Add("@DeviceVersion", version);
                            cmd.Parameters.Add("@OperatingSystem", platform);
                            // DeviceHostName,  DeviceSku
                            vdm.insert(cmd);
                            foreach (DataRow dr in dtgustempdetails.Rows)
                            {
                                emplogins newemp = new emplogins();
                                newemp.EmpName = dr["UserName"].ToString();
                                newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                                newemp.EmailID = "";
                                newemp.authtoken = token;
                                newemp.Msg = "Login Success";
                                surveybenelist.Add(newemp);
                            }
                            List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                            empoveralllist getoverDatas = new empoveralllist();
                            getoverDatas.getempoveralllist = surveybenelist;
                            getoveralllistdtls.Add(getoverDatas);
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            string response = jsonSerializer.Serialize(getoveralllistdtls);
                            Context.Response.Clear();
                            Context.Response.ContentType = "application/json";
                            Context.Response.AddHeader("content-length", response.Length.ToString());
                            Context.Response.Flush();
                            Context.Response.Write(response);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {

                            string message = "UUID mismatch";
                            emplogins newemp = new emplogins();
                            newemp.Msg = message;
                            surveybenelist.Add(newemp);
                            List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                            empoveralllist getoverDatas = new empoveralllist();
                            getoverDatas.getempoveralllist = surveybenelist;
                            getoveralllistdtls.Add(getoverDatas);
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            string response = jsonSerializer.Serialize(getoveralllistdtls);
                            Context.Response.Clear();
                            Context.Response.ContentType = "application/json";
                            Context.Response.AddHeader("content-length", response.Length.ToString());
                            Context.Response.Flush();
                            Context.Response.Write(response);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
                else
                {
                    cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE  IsActive=@active AND IMEI=@IMEI");
                    cmd.Parameters.Add("@active", true);
                    cmd.Parameters.Add("@IMEI", uuid);
                    DataTable imeidtls = vdm.SelectQuery(cmd).Tables[0];
                    if (imeidtls.Rows.Count > 0)
                    {
                        string message = "this device is already registered with another user";
                        emplogins newemp = new emplogins();
                        newemp.Msg = message;
                        surveybenelist.Add(newemp);
                        List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                        empoveralllist getoverDatas = new empoveralllist();
                        getoverDatas.getempoveralllist = surveybenelist;
                        getoveralllistdtls.Add(getoverDatas);
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                        Context.Response.Clear();
                        Context.Response.ContentType = "application/json";
                        Context.Response.AddHeader("content-length", response.Length.ToString());
                        Context.Response.Flush();
                        Context.Response.Write(response);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                        cmd.Parameters.Add("@empcode", userid);
                        cmd.Parameters.Add("@IsActive", true);
                        cmd.Parameters.Add("@status", false);
                        if (vdm.Update(cmd) == 0)
                        {
                            cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                            cmd.Parameters.Add("@UserID", userid);
                            cmd.Parameters.Add("@EmployeeCode", userid);
                            cmd.Parameters.Add("@Domain", "HCLTECH");
                            cmd.Parameters.Add("@SessionToken", token);
                            cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                            cmd.Parameters.Add("@LoginTime", currenttime);
                            cmd.Parameters.Add("@DeviceID", uuid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                            cmd.Parameters.Add("@DeviceModel", model);
                            cmd.Parameters.Add("@DeviceVersion", version);
                            cmd.Parameters.Add("@OperatingSystem", platform);
                            // DeviceHostName,  DeviceSku
                            vdm.insert(cmd);
                        }
                        else
                        {
                            cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                            cmd.Parameters.Add("@UserID", userid);
                            cmd.Parameters.Add("@EmployeeCode", userid);
                            cmd.Parameters.Add("@Domain", "HCLTECH");
                            cmd.Parameters.Add("@SessionToken", token);
                            cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                            cmd.Parameters.Add("@LoginTime", currenttime);
                            cmd.Parameters.Add("@DeviceID", uuid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                            cmd.Parameters.Add("@DeviceModel", model);
                            cmd.Parameters.Add("@DeviceVersion", version);
                            cmd.Parameters.Add("@OperatingSystem", platform);
                            // DeviceHostName,  DeviceSku
                            vdm.insert(cmd);
                        }

                        cmd = new SqlCommand("INSERT INTO tbl_trn_EmployeeIMEI(EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn) VALUES (@empcode, @imeino, @isactive, @createdby, @createdon, @ModifiedBy, @ModifiedOn)");
                        cmd.Parameters.Add("@empcode", userid);
                        cmd.Parameters.Add("@imeino", uuid);
                        cmd.Parameters.Add("@isactive", true);
                        cmd.Parameters.Add("@createdby", userid);
                        cmd.Parameters.Add("@createdon", DateTime.Now);
                        cmd.Parameters.Add("@ModifiedBy", userid);
                        cmd.Parameters.Add("@ModifiedOn", DateTime.Now);
                        vdm.insert(cmd);

                        foreach (DataRow dr in dtgustempdetails.Rows)
                        {
                            emplogins newemp = new emplogins();
                            newemp.EmpName = dr["UserName"].ToString();
                            newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                            newemp.EmailID = "";
                            newemp.authtoken = token;
                            newemp.Msg = "Login Success";
                            surveybenelist.Add(newemp);
                        }
                        List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                        empoveralllist getoverDatas = new empoveralllist();
                        getoverDatas.getempoveralllist = surveybenelist;
                        getoveralllistdtls.Add(getoverDatas);

                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                        Context.Response.Clear();
                        Context.Response.ContentType = "application/json";
                        Context.Response.AddHeader("content-length", response.Length.ToString());
                        Context.Response.Flush();
                        Context.Response.Write(response);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                }
            }
            else
            {
                string message = "Check Credentials";
                emplogins newemp = new emplogins();
                newemp.Msg = message;
                surveybenelist.Add(newemp);
                List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                empoveralllist getoverDatas = new empoveralllist();
                getoverDatas.getempoveralllist = surveybenelist;
                getoveralllistdtls.Add(getoverDatas);
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                string response = jsonSerializer.Serialize(getoveralllistdtls);
                Context.Response.Clear();
                Context.Response.ContentType = "application/json";
                Context.Response.AddHeader("content-length", response.Length.ToString());
                Context.Response.Flush();
                Context.Response.Write(response);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }
        catch (Exception ex)
        {
            Context.Response.Clear();
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void EMPloginserviceTEST(string username, string password, string uuid, string version, string manufacturer, string platform, string model)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            string[] name = username.Split('@');
            string domainname = name[1].ToString();
            username = name[0].ToString();
            string userid = username;
            string pwd = password;
            string imeiid = uuid;
            DataTable dtresourceallocation = new DataTable();
            List<emplogins> surveybenelist = new List<emplogins>();
            if (domainname == "hcl.com" || domainname == "HCL.COM")
            {
                string LoginAuthenticationURl = "https://staging.myhcl.com/SamudayService_Testing/api/TechnowellLogin/AuthenticateLogin";
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(LoginAuthenticationURl);
                LoginModel lm = new LoginModel();
                lm.email = username;
                lm.password = password;
                string ResponsePayload = JsonConvert.SerializeObject(lm);
                HttpContent content = new StringContent(ResponsePayload, Encoding.UTF8, "application/json");
                var Response = client.PostAsync(client.BaseAddress, content);
                Response.Wait();
                var d = JsonConvert.DeserializeObject<TechnowellLoginResponse>(Response.Result.Content.ReadAsStringAsync().Result);
                if (d.IsValidCredientials == true)
                {
                    string hclEmployecode = d.EmployeeCode;
                    userid = hclEmployecode;
                    cmd = new SqlCommand("SELECT  tbl_MST_Role.RoleName, tbl_MST_Employee.EmployeeCode, tbl_MST_Employee.SAPEmployeeCode, tbl_MST_Employee.EmployeeName, tbl_MST_Employee.EmailID, tbl_MST_Employee.DesignationCode FROM   tbl_MST_Employee LEFT OUTER JOIN tbl_MMP_EmployeeRole ON tbl_MST_Employee.EmployeeCode = tbl_MMP_EmployeeRole.EmployeeCode LEFT OUTER JOIN tbl_MST_Role ON tbl_MMP_EmployeeRole.RoleCode = tbl_MST_Role.RoleCode WHERE (tbl_MST_Employee.EmployeeCode = @empid)");
                    //cmd = new SqlCommand("SELECT tbl_MST_Employee.EmployeeCode, tbl_MST_Employee.SAPEmployeeCode, tbl_MST_Employee.EmployeeName, tbl_MST_Employee.EmailID, DesignationCode FROM  tbl_MST_Employee WHERE (tbl_MST_Employee.EmployeeCode = @empid)");
                    cmd.Parameters.Add("@empid", userid);
                    DataTable dtempdetails = vdm.SelectQuery(cmd).Tables[0];
                    if (dtempdetails.Rows.Count > 0)
                    {
                        DateTime currenttime = DateTime.Now;
                        DateTime sessiontokenexpirytime = currenttime.AddHours(3);
                        string token = Guid.NewGuid().ToString().ToUpper();
                        cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE EmployeeCode = @empcode AND IsActive=@active");
                        cmd.Parameters.Add("@empcode", userid);
                        cmd.Parameters.Add("@active", true);
                        DataTable dtimeidtls = vdm.SelectQuery(cmd).Tables[0];
                        if (dtimeidtls.Rows.Count > 0)
                        {
                            foreach (DataRow dri in dtimeidtls.Rows)
                            {
                                string imei = dri["IMEI"].ToString();
                                if (imei == uuid)
                                {
                                    // close previes auth token
                                    cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                                    cmd.Parameters.Add("@empcode", userid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@status", false);
                                    vdm.Update(cmd);
                                    //  insert new token
                                    cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                    cmd.Parameters.Add("@UserID", userid);
                                    cmd.Parameters.Add("@EmployeeCode", userid);
                                    cmd.Parameters.Add("@Domain", "HCLTECH");
                                    cmd.Parameters.Add("@SessionToken", token);
                                    cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                    cmd.Parameters.Add("@LoginTime", currenttime);
                                    cmd.Parameters.Add("@DeviceID", uuid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                    cmd.Parameters.Add("@DeviceModel", model);
                                    cmd.Parameters.Add("@DeviceVersion", version);
                                    cmd.Parameters.Add("@OperatingSystem", platform);
                                    // DeviceHostName,  DeviceSku
                                    vdm.insert(cmd);
                                    foreach (DataRow dr in dtempdetails.Rows)
                                    {
                                        emplogins newemp = new emplogins();
                                        newemp.EmpName = dr["EmployeeName"].ToString();
                                        newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                                        newemp.EmailID = dr["EmailID"].ToString();
                                        newemp.Role = dr["RoleName"].ToString();
                                        newemp.Designation = dr["DesignationCode"].ToString();
                                        newemp.authtoken = token;
                                        newemp.Msg = "Login Success";
                                        surveybenelist.Add(newemp);
                                    }
                                    List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                    empoveralllist getoverDatas = new empoveralllist();
                                    getoverDatas.getempoveralllist = surveybenelist;
                                    getoveralllistdtls.Add(getoverDatas);
                                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                    string response = jsonSerializer.Serialize(getoveralllistdtls);
                                    Context.Response.Clear();
                                    Context.Response.ContentType = "application/json";
                                    Context.Response.AddHeader("content-length", response.Length.ToString());
                                    Context.Response.Flush();
                                    Context.Response.Write(response);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                }
                                else
                                {

                                    string message = "UUID mismatch";
                                    emplogins newemp = new emplogins();
                                    newemp.Msg = message;
                                    surveybenelist.Add(newemp);
                                    List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                    empoveralllist getoverDatas = new empoveralllist();
                                    getoverDatas.getempoveralllist = surveybenelist;
                                    getoveralllistdtls.Add(getoverDatas);
                                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                    string response = jsonSerializer.Serialize(getoveralllistdtls);
                                    Context.Response.Clear();
                                    Context.Response.ContentType = "application/json";
                                    Context.Response.AddHeader("content-length", response.Length.ToString());
                                    Context.Response.Flush();
                                    Context.Response.Write(response);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                }
                            }
                        }
                        else
                        {
                            cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE  IsActive=@active AND IMEI=@IMEI");
                            cmd.Parameters.Add("@active", true);
                            cmd.Parameters.Add("@IMEI", uuid);
                            DataTable imeidtls = vdm.SelectQuery(cmd).Tables[0];
                            if (imeidtls.Rows.Count > 0)
                            {
                                string message = "this device is already registered with another user";
                                emplogins newemp = new emplogins();
                                newemp.Msg = message;
                                surveybenelist.Add(newemp);
                                List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                empoveralllist getoverDatas = new empoveralllist();
                                getoverDatas.getempoveralllist = surveybenelist;
                                getoveralllistdtls.Add(getoverDatas);
                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                string response = jsonSerializer.Serialize(getoveralllistdtls);
                                Context.Response.Clear();
                                Context.Response.ContentType = "application/json";
                                Context.Response.AddHeader("content-length", response.Length.ToString());
                                Context.Response.Flush();
                                Context.Response.Write(response);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                            }
                            else
                            {
                                cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                                cmd.Parameters.Add("@empcode", userid);
                                cmd.Parameters.Add("@IsActive", true);
                                cmd.Parameters.Add("@status", false);
                                if (vdm.Update(cmd) == 0)
                                {
                                    cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                    cmd.Parameters.Add("@UserID", userid);
                                    cmd.Parameters.Add("@EmployeeCode", userid);
                                    cmd.Parameters.Add("@Domain", "HCLTECH");
                                    cmd.Parameters.Add("@SessionToken", token);
                                    cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                    cmd.Parameters.Add("@LoginTime", currenttime);
                                    cmd.Parameters.Add("@DeviceID", uuid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                    cmd.Parameters.Add("@DeviceModel", model);
                                    cmd.Parameters.Add("@DeviceVersion", version);
                                    cmd.Parameters.Add("@OperatingSystem", platform);
                                    // DeviceHostName,  DeviceSku
                                    vdm.insert(cmd);
                                }
                                else
                                {
                                    cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                    cmd.Parameters.Add("@UserID", userid);
                                    cmd.Parameters.Add("@EmployeeCode", userid);
                                    cmd.Parameters.Add("@Domain", "HCLTECH");
                                    cmd.Parameters.Add("@SessionToken", token);
                                    cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                    cmd.Parameters.Add("@LoginTime", currenttime);
                                    cmd.Parameters.Add("@DeviceID", uuid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                    cmd.Parameters.Add("@DeviceModel", model);
                                    cmd.Parameters.Add("@DeviceVersion", version);
                                    cmd.Parameters.Add("@OperatingSystem", platform);
                                    // DeviceHostName,  DeviceSku
                                    vdm.insert(cmd);
                                }

                                cmd = new SqlCommand("INSERT INTO tbl_trn_EmployeeIMEI(EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn) VALUES (@empcode, @imeino, @isactive, @createdby, @createdon, @ModifiedBy, @ModifiedOn)");
                                cmd.Parameters.Add("@empcode", userid);
                                cmd.Parameters.Add("@imeino", uuid);
                                cmd.Parameters.Add("@isactive", true);
                                cmd.Parameters.Add("@createdby", userid);
                                cmd.Parameters.Add("@createdon", DateTime.Now);
                                cmd.Parameters.Add("@ModifiedBy", userid);
                                cmd.Parameters.Add("@ModifiedOn", DateTime.Now);
                                vdm.insert(cmd);

                                foreach (DataRow dr in dtempdetails.Rows)
                                {
                                    emplogins newemp = new emplogins();
                                    newemp.EmpName = dr["EmployeeName"].ToString();
                                    newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                                    newemp.EmailID = dr["EmailID"].ToString();
                                    newemp.Role = dr["RoleName"].ToString();
                                    newemp.Designation = dr["DesignationCode"].ToString();
                                    newemp.authtoken = token;
                                    newemp.Msg = "Login Success";
                                    surveybenelist.Add(newemp);
                                }
                                List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                empoveralllist getoverDatas = new empoveralllist();
                                getoverDatas.getempoveralllist = surveybenelist;
                                getoveralllistdtls.Add(getoverDatas);

                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                string response = jsonSerializer.Serialize(getoveralllistdtls);
                                Context.Response.Clear();
                                Context.Response.ContentType = "application/json";
                                Context.Response.AddHeader("content-length", response.Length.ToString());
                                Context.Response.Flush();
                                Context.Response.Write(response);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                            }
                        }
                    }
                    else
                    {
                        string message = "Login faild";
                        emplogins newemp = new emplogins();
                        newemp.Msg = message;
                        surveybenelist.Add(newemp);
                        List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                        empoveralllist getoverDatas = new empoveralllist();
                        getoverDatas.getempoveralllist = surveybenelist;
                        getoveralllistdtls.Add(getoverDatas);
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                        Context.Response.Clear();
                        Context.Response.ContentType = "application/json";
                        Context.Response.AddHeader("content-length", response.Length.ToString());
                        Context.Response.Flush();
                        Context.Response.Write(response);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    string message = "Check Credentials";
                    emplogins newemp = new emplogins();
                    newemp.Msg = message;
                    surveybenelist.Add(newemp);
                    List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                    empoveralllist getoverDatas = new empoveralllist();
                    getoverDatas.getempoveralllist = surveybenelist;
                    getoveralllistdtls.Add(getoverDatas);
                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                    string response = jsonSerializer.Serialize(getoveralllistdtls);
                    Context.Response.Clear();
                    Context.Response.ContentType = "application/json";
                    Context.Response.AddHeader("content-length", response.Length.ToString());
                    Context.Response.Flush();
                    Context.Response.Write(response);
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
            else
            {

                string guestLoginAuthenticationURl = "https://staging.myhcl.com/SamudayWebsite_Testing/api/TechnowellLogin/AuthenticateGuestLogin";
                HttpClient guestclient = new HttpClient();
                guestclient.BaseAddress = new Uri(guestLoginAuthenticationURl);
                LoginModel gustlm = new LoginModel();
                gustlm.email = username;
                gustlm.password = password;
                string gustResponsePayload = JsonConvert.SerializeObject(gustlm);
                HttpContent gustcontent = new StringContent(gustResponsePayload, Encoding.UTF8, "application/json");
                var gustResponse = guestclient.PostAsync(guestclient.BaseAddress, gustcontent);
                gustResponse.Wait();
                var data = JsonConvert.DeserializeObject<TechnowellLoginResponse>(gustResponse.Result.Content.ReadAsStringAsync().Result);
                if (data.IsValidCredientials == true)
                {
                    string guesthclEmployecode = data.EmployeeCode;
                    userid = guesthclEmployecode;
                    cmd = new SqlCommand("SELECT  tbl_MST_Role.RoleName, tbl_MST_Employee.EmployeeCode, tbl_MST_Employee.SAPEmployeeCode, tbl_MST_Employee.EmployeeName, tbl_MST_Employee.EmailID, tbl_MST_Employee.DesignationCode FROM   tbl_MST_Employee LEFT OUTER JOIN tbl_MMP_EmployeeRole ON tbl_MST_Employee.EmployeeCode = tbl_MMP_EmployeeRole.EmployeeCode LEFT OUTER JOIN tbl_MST_Role ON tbl_MMP_EmployeeRole.RoleCode = tbl_MST_Role.RoleCode WHERE (tbl_MST_Employee.EmployeeCode = @empid)");
                    cmd.Parameters.Add("@empid", userid);
                    DataTable dtempdetails = vdm.SelectQuery(cmd).Tables[0];
                    if (dtempdetails.Rows.Count > 0)
                    {
                        DateTime currenttime = DateTime.Now;
                        DateTime sessiontokenexpirytime = currenttime.AddHours(3);
                        string token = Guid.NewGuid().ToString().ToUpper();
                        cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE EmployeeCode = @empcode AND IsActive=@active");
                        cmd.Parameters.Add("@empcode", userid);
                        cmd.Parameters.Add("@active", true);
                        DataTable dtimeidtls = vdm.SelectQuery(cmd).Tables[0];
                        if (dtimeidtls.Rows.Count > 0)
                        {
                            foreach (DataRow dri in dtimeidtls.Rows)
                            {
                                string imei = dri["IMEI"].ToString();
                                if (imei == uuid)
                                {
                                    // close previes auth token
                                    cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                                    cmd.Parameters.Add("@empcode", userid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@status", false);
                                    vdm.Update(cmd);
                                    //  insert new token
                                    cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                    cmd.Parameters.Add("@UserID", userid);
                                    cmd.Parameters.Add("@EmployeeCode", userid);
                                    cmd.Parameters.Add("@Domain", "HCLTECH");
                                    cmd.Parameters.Add("@SessionToken", token);
                                    cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                    cmd.Parameters.Add("@LoginTime", currenttime);
                                    cmd.Parameters.Add("@DeviceID", uuid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                    cmd.Parameters.Add("@DeviceModel", model);
                                    cmd.Parameters.Add("@DeviceVersion", version);
                                    cmd.Parameters.Add("@OperatingSystem", platform);
                                    // DeviceHostName,  DeviceSku
                                    vdm.insert(cmd);
                                    foreach (DataRow dr in dtempdetails.Rows)
                                    {
                                        emplogins newemp = new emplogins();
                                        newemp.EmpName = dr["EmployeeName"].ToString();
                                        newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                                        newemp.EmailID = dr["EmailID"].ToString();
                                        newemp.Role = dr["RoleName"].ToString();
                                        newemp.Designation = dr["DesignationCode"].ToString();
                                        newemp.authtoken = token;
                                        newemp.Msg = "Login Success";
                                        surveybenelist.Add(newemp);
                                    }
                                    List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                    empoveralllist getoverDatas = new empoveralllist();
                                    getoverDatas.getempoveralllist = surveybenelist;
                                    getoveralllistdtls.Add(getoverDatas);
                                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                    string response = jsonSerializer.Serialize(getoveralllistdtls);
                                    Context.Response.Clear();
                                    Context.Response.ContentType = "application/json";
                                    Context.Response.AddHeader("content-length", response.Length.ToString());
                                    Context.Response.Flush();
                                    Context.Response.Write(response);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                }
                                else
                                {

                                    string message = "UUID mismatch";
                                    emplogins newemp = new emplogins();
                                    newemp.Msg = message;
                                    surveybenelist.Add(newemp);
                                    List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                    empoveralllist getoverDatas = new empoveralllist();
                                    getoverDatas.getempoveralllist = surveybenelist;
                                    getoveralllistdtls.Add(getoverDatas);
                                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                    string response = jsonSerializer.Serialize(getoveralllistdtls);
                                    Context.Response.Clear();
                                    Context.Response.ContentType = "application/json";
                                    Context.Response.AddHeader("content-length", response.Length.ToString());
                                    Context.Response.Flush();
                                    Context.Response.Write(response);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                }
                            }
                        }
                        else
                        {
                            cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE  IsActive=@active AND IMEI=@IMEI");
                            cmd.Parameters.Add("@active", true);
                            cmd.Parameters.Add("@IMEI", uuid);
                            DataTable imeidtls = vdm.SelectQuery(cmd).Tables[0];
                            if (imeidtls.Rows.Count > 0)
                            {
                                string message = "this device is already registered with another user";
                                emplogins newemp = new emplogins();
                                newemp.Msg = message;
                                surveybenelist.Add(newemp);
                                List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                empoveralllist getoverDatas = new empoveralllist();
                                getoverDatas.getempoveralllist = surveybenelist;
                                getoveralllistdtls.Add(getoverDatas);
                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                string response = jsonSerializer.Serialize(getoveralllistdtls);
                                Context.Response.Clear();
                                Context.Response.ContentType = "application/json";
                                Context.Response.AddHeader("content-length", response.Length.ToString());
                                Context.Response.Flush();
                                Context.Response.Write(response);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                            }
                            else
                            {
                                cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                                cmd.Parameters.Add("@empcode", userid);
                                cmd.Parameters.Add("@IsActive", true);
                                cmd.Parameters.Add("@status", false);
                                if (vdm.Update(cmd) == 0)
                                {
                                    cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                    cmd.Parameters.Add("@UserID", userid);
                                    cmd.Parameters.Add("@EmployeeCode", userid);
                                    cmd.Parameters.Add("@Domain", "HCLTECH");
                                    cmd.Parameters.Add("@SessionToken", token);
                                    cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                    cmd.Parameters.Add("@LoginTime", currenttime);
                                    cmd.Parameters.Add("@DeviceID", uuid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                    cmd.Parameters.Add("@DeviceModel", model);
                                    cmd.Parameters.Add("@DeviceVersion", version);
                                    cmd.Parameters.Add("@OperatingSystem", platform);
                                    // DeviceHostName,  DeviceSku
                                    vdm.insert(cmd);
                                }
                                else
                                {
                                    cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                    cmd.Parameters.Add("@UserID", userid);
                                    cmd.Parameters.Add("@EmployeeCode", userid);
                                    cmd.Parameters.Add("@Domain", "HCLTECH");
                                    cmd.Parameters.Add("@SessionToken", token);
                                    cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                    cmd.Parameters.Add("@LoginTime", currenttime);
                                    cmd.Parameters.Add("@DeviceID", uuid);
                                    cmd.Parameters.Add("@IsActive", true);
                                    cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                    cmd.Parameters.Add("@DeviceModel", model);
                                    cmd.Parameters.Add("@DeviceVersion", version);
                                    cmd.Parameters.Add("@OperatingSystem", platform);
                                    // DeviceHostName,  DeviceSku
                                    vdm.insert(cmd);
                                }

                                cmd = new SqlCommand("INSERT INTO tbl_trn_EmployeeIMEI(EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn) VALUES (@empcode, @imeino, @isactive, @createdby, @createdon, @ModifiedBy, @ModifiedOn)");
                                cmd.Parameters.Add("@empcode", userid);
                                cmd.Parameters.Add("@imeino", uuid);
                                cmd.Parameters.Add("@isactive", true);
                                cmd.Parameters.Add("@createdby", userid);
                                cmd.Parameters.Add("@createdon", DateTime.Now);
                                cmd.Parameters.Add("@ModifiedBy", userid);
                                cmd.Parameters.Add("@ModifiedOn", DateTime.Now);
                                vdm.insert(cmd);

                                foreach (DataRow dr in dtempdetails.Rows)
                                {
                                    emplogins newemp = new emplogins();
                                    newemp.EmpName = dr["EmployeeName"].ToString();
                                    newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                                    newemp.EmailID = dr["EmailID"].ToString();
                                    newemp.Role = dr["RoleName"].ToString();
                                    newemp.Designation = dr["DesignationCode"].ToString();
                                    newemp.authtoken = token;
                                    newemp.Msg = "Login Success";
                                    surveybenelist.Add(newemp);
                                }
                                List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                                empoveralllist getoverDatas = new empoveralllist();
                                getoverDatas.getempoveralllist = surveybenelist;
                                getoveralllistdtls.Add(getoverDatas);

                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                string response = jsonSerializer.Serialize(getoveralllistdtls);
                                Context.Response.Clear();
                                Context.Response.ContentType = "application/json";
                                Context.Response.AddHeader("content-length", response.Length.ToString());
                                Context.Response.Flush();
                                Context.Response.Write(response);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                            }
                        }
                    }
                    else
                    {
                        string message = "Login faild";
                        emplogins newemp = new emplogins();
                        newemp.Msg = message;
                        surveybenelist.Add(newemp);
                        List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                        empoveralllist getoverDatas = new empoveralllist();
                        getoverDatas.getempoveralllist = surveybenelist;
                        getoveralllistdtls.Add(getoverDatas);
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                        Context.Response.Clear();
                        Context.Response.ContentType = "application/json";
                        Context.Response.AddHeader("content-length", response.Length.ToString());
                        Context.Response.Flush();
                        Context.Response.Write(response);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    string message = "Check Credentials";
                    emplogins newemp = new emplogins();
                    newemp.Msg = message;
                    surveybenelist.Add(newemp);
                    List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                    empoveralllist getoverDatas = new empoveralllist();
                    getoverDatas.getempoveralllist = surveybenelist;
                    getoveralllistdtls.Add(getoverDatas);
                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                    string response = jsonSerializer.Serialize(getoveralllistdtls);
                    Context.Response.Clear();
                    Context.Response.ContentType = "application/json";
                    Context.Response.AddHeader("content-length", response.Length.ToString());
                    Context.Response.Flush();
                    Context.Response.Write(response);
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
            
        }
        catch (Exception ex)
        {
            Context.Response.Clear();
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }
    [WebMethod]
    public string Login(string userName, string password)
    {
        string[] words = userName.Split('@');
        string domain = userName.Substring(1, userName.IndexOf('@'));
        string LoginAuthenticationURl = "https://staging.myhcl.com/SamudayService_Testing/api/TechnowellLogin/AuthenticateLogin";
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(LoginAuthenticationURl);
        LoginModel lm = new LoginModel();
        lm.UserName = userName;
        lm.Password = password;
        string ResponsePayload = JsonConvert.SerializeObject(lm);
        HttpContent content = new StringContent(ResponsePayload, Encoding.UTF8, "application/json");
        var Response = client.PostAsync(client.BaseAddress, content);
        Response.Wait();
        var d = JsonConvert.DeserializeObject<TechnowellLoginResponse>(Response.Result.Content.ReadAsStringAsync().Result);
        return d.IsValidCredientials == true ? "Login Success" : "Login Failed";
    }
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void EMPloginservice(string username, string password, string uuid, string version, string manufacturer, string platform, string model)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            string userid = username;
            string pwd = password;
            string imeiid = uuid;
            DataTable dtresourceallocation = new DataTable();
            List<emplogins> surveybenelist = new List<emplogins>();
            
            cmd = new SqlCommand("SELECT EmployeeCode, SAPEmployeeCode, EmployeeName, EmailID, DesignationCode FROM  tbl_MST_Employee WHERE (EmployeeCode = @empid)");
            cmd.Parameters.Add("@empid", userid);
            DataTable dtempdetails = vdm.SelectQuery(cmd).Tables[0];

            cmd = new SqlCommand("SELECT tbl_MST_Role.RoleName, tbl_MMP_EmployeeRole.EmployeeCode FROM   tbl_MMP_EmployeeRole INNER JOIN  tbl_MST_Role ON tbl_MMP_EmployeeRole.RoleCode = tbl_MST_Role.RoleCode WHERE tbl_MMP_EmployeeRole.EmployeeCode=@empcode");
            cmd.Parameters.Add("@empcode", userid);
            DataTable dtroledetails = vdm.SelectQuery(cmd).Tables[0];

            if (dtempdetails.Rows.Count > 0)
            {
                DateTime currenttime = DateTime.Now;
                DateTime sessiontokenexpirytime = currenttime.AddHours(3);
                string token = Guid.NewGuid().ToString().ToUpper();
                

                cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE EmployeeCode = @empcode AND IsActive=@active");
                cmd.Parameters.Add("@empcode", userid);
                cmd.Parameters.Add("@active", true);
                DataTable dtimeidtls = vdm.SelectQuery(cmd).Tables[0];
                if (dtimeidtls.Rows.Count > 0)
                {
                    foreach (DataRow dri in dtimeidtls.Rows)
                    {
                        string imei = dri["IMEI"].ToString();
                        if (imei == uuid)
                        {
                            cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                            cmd.Parameters.Add("@empcode", userid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@status", false);
                            if (vdm.Update(cmd) == 0)
                            {
                                cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                cmd.Parameters.Add("@UserID", userid);
                                cmd.Parameters.Add("@EmployeeCode", userid);
                                cmd.Parameters.Add("@Domain", "HCLTECH");
                                cmd.Parameters.Add("@SessionToken", token);
                                cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                cmd.Parameters.Add("@LoginTime", currenttime);
                                cmd.Parameters.Add("@DeviceID", uuid);
                                cmd.Parameters.Add("@IsActive", true);
                                cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                cmd.Parameters.Add("@DeviceModel", model);
                                cmd.Parameters.Add("@DeviceVersion", version);
                                cmd.Parameters.Add("@OperatingSystem", platform);
                                // DeviceHostName,  DeviceSku
                                vdm.insert(cmd);
                            }
                            else
                            {
                                cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                                cmd.Parameters.Add("@UserID", userid);
                                cmd.Parameters.Add("@EmployeeCode", userid);
                                cmd.Parameters.Add("@Domain", "HCLTECH");
                                cmd.Parameters.Add("@SessionToken", token);
                                cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                                cmd.Parameters.Add("@LoginTime", currenttime);
                                cmd.Parameters.Add("@DeviceID", uuid);
                                cmd.Parameters.Add("@IsActive", true);
                                cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                                cmd.Parameters.Add("@DeviceModel", model);
                                cmd.Parameters.Add("@DeviceVersion", version);
                                cmd.Parameters.Add("@OperatingSystem", platform);
                                // DeviceHostName,  DeviceSku
                                vdm.insert(cmd);
                            }
                            foreach (DataRow dr in dtempdetails.Rows)
                            {
                                emplogins newemp = new emplogins();
                                newemp.EmpName = dr["EmployeeName"].ToString();
                                newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                                newemp.EmailID = dr["EmailID"].ToString();
                                newemp.authtoken = token;
                                string EmployeeCode = dr["EmployeeCode"].ToString();
                                string roleval = "";
                                foreach (DataRow drw in dtroledetails.Select("EmployeeCode='" + EmployeeCode + "'"))
                                {
                                    string role = drw["RoleName"].ToString();
                                    roleval += role + ","; 
                                }
                                newemp.Role = roleval;
                                newemp.Designation = dr["DesignationCode"].ToString();
                                newemp.Msg = "Login Success";
                                surveybenelist.Add(newemp);
                            }
                            List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                            empoveralllist getoverDatas = new empoveralllist();
                            getoverDatas.getempoveralllist = surveybenelist;
                            getoveralllistdtls.Add(getoverDatas);
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            string response = jsonSerializer.Serialize(getoveralllistdtls);
                            Context.Response.Clear();
                            Context.Response.ContentType = "application/json";
                            Context.Response.AddHeader("content-length", response.Length.ToString());
                            Context.Response.Flush();
                            Context.Response.Write(response);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {

                            string message = "UUID mismatch";
                            cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive AND DeviceID=@DeviceID");
                            cmd.Parameters.Add("@empcode", userid);
                            cmd.Parameters.Add("@DeviceID", uuid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@status", false);
                            vdm.Update(cmd);
                            emplogins newemp = new emplogins();
                            newemp.Msg = message;
                            surveybenelist.Add(newemp);
                            List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                            empoveralllist getoverDatas = new empoveralllist();
                            getoverDatas.getempoveralllist = surveybenelist;
                            getoveralllistdtls.Add(getoverDatas);
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            string response = jsonSerializer.Serialize(getoveralllistdtls);
                            Context.Response.Clear();
                            Context.Response.ContentType = "application/json";
                            Context.Response.AddHeader("content-length", response.Length.ToString());
                            Context.Response.Flush();
                            Context.Response.Write(response);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
                else
                {
                    cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE  IsActive=@active AND IMEI=@IMEI");
                    cmd.Parameters.Add("@active", true);
                    cmd.Parameters.Add("@IMEI", uuid);
                    DataTable imeidtls = vdm.SelectQuery(cmd).Tables[0];
                    if (imeidtls.Rows.Count > 0)
                    {
                        string message = "this device is already registered with another user";
                        emplogins newemp = new emplogins();
                        newemp.Msg = message;
                        surveybenelist.Add(newemp);
                        List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                        empoveralllist getoverDatas = new empoveralllist();
                        getoverDatas.getempoveralllist = surveybenelist;
                        getoveralllistdtls.Add(getoverDatas);
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                        Context.Response.Clear();
                        Context.Response.ContentType = "application/json";
                        Context.Response.AddHeader("content-length", response.Length.ToString());
                        Context.Response.Flush();
                        Context.Response.Write(response);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET IsActive=@status WHERE EmployeeCode=@empcode AND IsActive=@IsActive");
                        cmd.Parameters.Add("@empcode", userid);
                        cmd.Parameters.Add("@IsActive", true);
                        cmd.Parameters.Add("@status", false);
                        if (vdm.Update(cmd) == 0)
                        {
                            cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                            cmd.Parameters.Add("@UserID", userid);
                            cmd.Parameters.Add("@EmployeeCode", userid);
                            cmd.Parameters.Add("@Domain", "HCLTECH");
                            cmd.Parameters.Add("@SessionToken", token);
                            cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                            cmd.Parameters.Add("@LoginTime", currenttime);
                            cmd.Parameters.Add("@DeviceID", uuid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                            cmd.Parameters.Add("@DeviceModel", model);
                            cmd.Parameters.Add("@DeviceVersion", version);
                            cmd.Parameters.Add("@OperatingSystem", platform);
                            // DeviceHostName,  DeviceSku
                            vdm.insert(cmd);
                        }
                        else
                        {
                            cmd = new SqlCommand("INSERT INTO tbl_TRN_LogInDetail( UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime,  DeviceID, IsActive, DeviceManufacturer, DeviceModel, OperatingSystem, DeviceVersion) VALUES (@UserID, @EmployeeCode, @Domain, @SessionToken, @SessionExpiryTime, @LoginTime, @DeviceID, @IsActive, @DeviceManufacturer, @DeviceModel, @OperatingSystem, @DeviceVersion)");
                            cmd.Parameters.Add("@UserID", userid);
                            cmd.Parameters.Add("@EmployeeCode", userid);
                            cmd.Parameters.Add("@Domain", "HCLTECH");
                            cmd.Parameters.Add("@SessionToken", token);
                            cmd.Parameters.Add("@SessionExpiryTime", sessiontokenexpirytime);
                            cmd.Parameters.Add("@LoginTime", currenttime);
                            cmd.Parameters.Add("@DeviceID", uuid);
                            cmd.Parameters.Add("@IsActive", true);
                            cmd.Parameters.Add("@DeviceManufacturer", manufacturer);
                            cmd.Parameters.Add("@DeviceModel", model);
                            cmd.Parameters.Add("@DeviceVersion", version);
                            cmd.Parameters.Add("@OperatingSystem", platform);
                            // DeviceHostName,  DeviceSku
                            vdm.insert(cmd);
                        }

                        cmd = new SqlCommand("INSERT INTO tbl_trn_EmployeeIMEI(EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn) VALUES (@empcode, @imeino, @isactive, @createdby, @createdon, @ModifiedBy, @ModifiedOn)");
                        cmd.Parameters.Add("@empcode", userid);
                        cmd.Parameters.Add("@imeino", uuid);
                        cmd.Parameters.Add("@isactive", true);
                        cmd.Parameters.Add("@createdby", userid);
                        cmd.Parameters.Add("@createdon", DateTime.Now);
                        cmd.Parameters.Add("@ModifiedBy", userid);
                        cmd.Parameters.Add("@ModifiedOn", DateTime.Now);
                        vdm.insert(cmd);

                        foreach (DataRow dr in dtempdetails.Rows)
                        {
                            emplogins newemp = new emplogins();
                            newemp.EmpName = dr["EmployeeName"].ToString();
                            newemp.EmployeeCode = dr["EmployeeCode"].ToString();
                            newemp.EmailID = dr["EmailID"].ToString();
                            newemp.authtoken = token;
                            string EmployeeCode = dr["EmployeeCode"].ToString();
                            string roleval = "";
                            foreach (DataRow drw in dtroledetails.Select("EmployeeCode='" + EmployeeCode + "'"))
                            {
                                string role = drw["RoleName"].ToString();
                                roleval += role + ",";
                            }
                            newemp.Role = roleval;
                            newemp.Designation = dr["DesignationCode"].ToString();
                            newemp.Msg = "Login Success";
                            surveybenelist.Add(newemp);
                        }
                        List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                        empoveralllist getoverDatas = new empoveralllist();
                        getoverDatas.getempoveralllist = surveybenelist;
                        getoveralllistdtls.Add(getoverDatas);

                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                        Context.Response.Clear();
                        Context.Response.ContentType = "application/json";
                        Context.Response.AddHeader("content-length", response.Length.ToString());
                        Context.Response.Flush();
                        Context.Response.Write(response);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                }
            }
            else
            {
                cmd = new SqlCommand("SELECT  EmployeeCode, UserName, Password, Gender, GovernmentIdType, GovernmentIdNumber FROM   tbl_MST_GuestEmployee WHERE (EmployeeCode = @empid) AND (Password = @Password)");
                cmd.Parameters.Add("@empid", userid);
                cmd.Parameters.Add("@Password", password);
                DataTable dtgustempdetails = vdm.SelectQuery(cmd).Tables[0];
                if (dtgustempdetails.Rows.Count > 0)
                {

                }
                else
                {
                    string message = "Login faild";
                    emplogins newemp = new emplogins();
                    newemp.Msg = message;
                    surveybenelist.Add(newemp);
                    List<empoveralllist> getoveralllistdtls = new List<empoveralllist>();
                    empoveralllist getoverDatas = new empoveralllist();
                    getoverDatas.getempoveralllist = surveybenelist;
                    getoveralllistdtls.Add(getoverDatas);
                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
                    string response = jsonSerializer.Serialize(getoveralllistdtls);
                    Context.Response.Clear();
                    Context.Response.ContentType = "application/json";
                    Context.Response.AddHeader("content-length", response.Length.ToString());
                    Context.Response.Flush();
                    Context.Response.Write(response);
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
        }
        catch (Exception ex)
        {

        }
    }



    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void EMPattendanceservice(string empcode,  string uuid, string longitude, string latitude)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();


            //added by naveeen 
            string token = System.Web.HttpContext.Current.Request.Headers["token"];
            string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
            string tuuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
            //end


            string userid = empcode;
            string imeiid = uuid;
            DataTable dtresourceallocation = new DataTable();
            List<emploginattendance> empattendancelist = new List<emploginattendance>();

            int i = 0;
            cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM   tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
            cmd.Parameters.Add("@empcode", employecode);
            cmd.Parameters.Add("@token", token);
            cmd.Parameters.Add("@uuid", tuuid);
            cmd.Parameters.Add("@IsActive", true);
            DataTable dttoken = vdm.SelectQuery(cmd).Tables[0];

            if (dttoken.Rows.Count > 0)
            {
                foreach (DataRow drt in dttoken.Rows)
                {
                    string sessionexpdate = drt["SessionExpiryTime"].ToString();
                    DateTime dtsessionexpdate = Convert.ToDateTime(sessionexpdate);
                    DateTime nowdatetime = DateTime.Now;
                    if (nowdatetime > dtsessionexpdate)
                    {
                        Context.Response.Clear();
                        Context.Response.StatusCode = 401;
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        cmd = new SqlCommand("SELECT EmployeeCode, SAPEmployeeCode, EmployeeName, EmailID, DesignationCode, DepartmentName, LocationCode FROM  tbl_MST_Employee WHERE (EmployeeCode = @empid)");
                        cmd.Parameters.Add("@empid", userid);
                        DataTable dtempdetails = vdm.SelectQuery(cmd).Tables[0];
                        if (dtempdetails.Rows.Count > 0)
                        {
                            DateTime currenttime = DateTime.Now;
                            string date = currenttime.ToString("yyyy-MM-dd");
                            string time = currenttime.ToString("HH:mm:ss");
                            cmd = new SqlCommand("SELECT Id, EmployeeCode, IMEI, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM tbl_trn_EmployeeIMEI WHERE EmployeeCode = @empcode AND IsActive=@active");
                            cmd.Parameters.Add("@empcode", userid);
                            cmd.Parameters.Add("@active", true);
                            DataTable dtimeidtls = vdm.SelectQuery(cmd).Tables[0];
                            if (dtimeidtls.Rows.Count > 0)
                            {
                                foreach (DataRow dri in dtimeidtls.Rows)
                                {
                                    string imei = dri["IMEI"].ToString();
                                    if (imei == uuid)
                                    {
                                        //attendance_hist
                                        //attendance
                                        //date and empcpde
                                        string empname = "";
                                        string Department = "";
                                        string Designation = "";
                                        string Role = "";
                                        string EmailID = "";
                                        string locationname = "";
                                        string statusval = "";
                                        foreach (DataRow dr in dtempdetails.Rows)
                                        {
                                            empname = dr["EmployeeName"].ToString();
                                            EmailID = dr["EmailID"].ToString();
                                            string EmployeeCode = dr["EmployeeCode"].ToString();
                                            string roleval = "";
                                            Role = roleval;
                                            Designation = dr["DesignationCode"].ToString();
                                            Department = dr["DepartmentName"].ToString();
                                            string loccode = dr["LocationCode"].ToString();
                                            cmd = new SqlCommand("SELECT   LocationCode, LocationName FROM  tbl_MST_Location WHERE  (LocationCode = @loccode)");
                                            cmd.Parameters.Add("@loccode", loccode);
                                            DataTable dtlocdtls = vdm.SelectQuery(cmd).Tables[0];
                                            if (dtlocdtls.Rows.Count > 0)
                                            {
                                                foreach (DataRow drl in dtlocdtls.Rows)
                                                {
                                                    locationname = drl["LocationName"].ToString();
                                                }
                                            }
                                        }

                                        postcmd = new NpgsqlCommand("SELECT id, employeeid, employeename, imeino, department, designation, locationname,today_date,from_time, to_time, status, latitude, longitude, new_latitude, new_longitude FROM saveattendance WHERE (employeeid='" + userid + "') AND (today_date = '" + date + "')");
                                        DataTable dtattendancedata = postvdm.SelectQuery(postcmd).Tables[0];
                                        if (dtattendancedata.Rows.Count > 0)
                                        {
                                            string id = dtattendancedata.Rows[0]["id"].ToString();
                                            string existlatitude = dtattendancedata.Rows[0]["latitude"].ToString();
                                            string existlongitude = dtattendancedata.Rows[0]["longitude"].ToString();
                                            string new_latitude = dtattendancedata.Rows[0]["new_latitude"].ToString();
                                            if(new_latitude == "" || new_latitude == null)
                                            {
                                                new_latitude = "0";
                                            }
                                            string new_longitude = dtattendancedata.Rows[0]["new_longitude"].ToString();
                                            if (new_longitude == "" || new_longitude == null)
                                            {
                                                new_longitude = "0";
                                            }
                                            string fromtime = dtattendancedata.Rows[0]["from_time"].ToString();
                                            string to_time = dtattendancedata.Rows[0]["to_time"].ToString();
                                            string status = dtattendancedata.Rows[0]["status"].ToString();
                                            int stval = Convert.ToInt32(status);
                                            int countval = stval + 1;
                                            statusval = countval.ToString();
                                            postcmd = new NpgsqlCommand("UPDATE saveattendance set status='"+ statusval + "', to_time = '" + time + "', new_latitude='" + latitude + "', new_longitude='" + longitude + "' WHERE (employeeid='" + userid + "') AND (today_date = '" + date + "')");
                                            postvdm.Update(postcmd);
                                            //string cmdtext = "INSERT INTO saveattendance_hist (SELECT * FROM saveattendance WHERE (employeeid='" + userid + "') AND (today_date = '" + date + "'))";
                                            postcmd = new NpgsqlCommand("INSERT INTO saveattendance_hist(employeeid, employeename, imeino, department, designation, locationname,today_date,from_time, to_time, status, latitude, longitude, new_latitude, new_longitude, id, created_at) values ('" + userid + "','" + empname + "','" + uuid + "','" + Department + "','" + Designation + "','" + locationname + "','" + date + "','" + fromtime + "','" + to_time + "','"+status+"','" + existlatitude + "','" + existlongitude + "','" + new_latitude + "', '" + new_longitude + "', '" + id + "', '" + currenttime + "')");
                                            postvdm.insert(postcmd);
                                        }
                                        else
                                        {
                                            postcmd = new NpgsqlCommand("INSERT INTO saveattendance(employeeid, employeename, imeino, department, designation, locationname,today_date,from_time,status, latitude, longitude, created_at) values ('" + userid + "','" + empname + "','" + uuid + "','" + Department + "','" + Designation + "','" + locationname + "','" + date + "','" + time + "','0','" + latitude + "', '" + longitude + "', '" + currenttime + "')");
                                            postvdm.insert(postcmd);
                                            statusval = "0";
                                        }
                                        emploginattendance newemp = new emploginattendance();
                                        newemp.status_code = "200";
                                        if (statusval == "0")
                                        {
                                            newemp.count = "1";
                                        }
                                        else
                                        {
                                            int tothistcount = Convert.ToInt32(statusval);
                                            tothistcount = tothistcount + 1;
                                            newemp.count = tothistcount.ToString();
                                        }
                                        int statusvalcount = Convert.ToInt32(statusval);
                                        if (statusvalcount % 2 == 0)
                                        {
                                            string message = "You are Logged In  at " + currenttime + ", Have a Nice Day " + empname + "";
                                            newemp.Msg = message;
                                        }
                                        else
                                        {
                                            string message = "You are Logged Out  at " + currenttime + ", Have a Nice Day " + empname + "";
                                            newemp.Msg = message;
                                        }
                                        empattendancelist.Add(newemp);
                                        List<empoverallattendancelist> getoveralllistdtls = new List<empoverallattendancelist>();
                                        empoverallattendancelist getoverDatas = new empoverallattendancelist();
                                        getoverDatas.getempoveralllist = empattendancelist;
                                        getoveralllistdtls.Add(getoverDatas);
                                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                                        Context.Response.Clear();
                                        Context.Response.ContentType = "application/json";
                                        Context.Response.AddHeader("content-length", response.Length.ToString());
                                        Context.Response.Flush();
                                        Context.Response.Write(response);
                                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                                    }
                                    else
                                    {
                                        string message = "UUID mismatch";
                                        emploginattendance newemp = new emploginattendance();
                                        newemp.Msg = message;
                                        newemp.status_code = "205";
                                        empattendancelist.Add(newemp);
                                        List<empoverallattendancelist> getoveralllistdtls = new List<empoverallattendancelist>();
                                        empoverallattendancelist getoverDatas = new empoverallattendancelist();
                                        getoverDatas.getempoveralllist = empattendancelist;
                                        getoveralllistdtls.Add(getoverDatas);
                                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                        string response = jsonSerializer.Serialize(getoveralllistdtls);
                                        Context.Response.Clear();
                                        Context.Response.ContentType = "application/json";
                                        Context.Response.AddHeader("content-length", response.Length.ToString());
                                        Context.Response.Flush();
                                        Context.Response.Write(response);
                                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                                    }
                                }
                            }
                            else
                            {
                                string message = "Please login with registered Mobile";
                                emploginattendance newemp = new emploginattendance();
                                newemp.Msg = message;
                                newemp.status_code = "205";
                                empattendancelist.Add(newemp);
                                List<empoverallattendancelist> getoveralllistdtls = new List<empoverallattendancelist>();
                                empoverallattendancelist getoverDatas = new empoverallattendancelist();
                                getoverDatas.getempoveralllist = empattendancelist;
                                getoveralllistdtls.Add(getoverDatas);
                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                                jsonSerializer.MaxJsonLength = Int32.MaxValue;
                                string response = jsonSerializer.Serialize(getoveralllistdtls);
                                Context.Response.Clear();
                                Context.Response.ContentType = "application/json";
                                Context.Response.AddHeader("content-length", response.Length.ToString());
                                Context.Response.Flush();
                                Context.Response.Write(response);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                            }
                        }
                        else
                        {
                            string message = "Employe not exist in samuday 360";
                            emploginattendance newemp = new emploginattendance();
                            newemp.Msg = message;
                            newemp.status_code = "205";
                            empattendancelist.Add(newemp);
                            List<empoverallattendancelist> getoveralllistdtls = new List<empoverallattendancelist>();
                            empoverallattendancelist getoverDatas = new empoverallattendancelist();
                            getoverDatas.getempoveralllist = empattendancelist;
                            getoveralllistdtls.Add(getoverDatas);
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            string response = jsonSerializer.Serialize(getoveralllistdtls);
                            Context.Response.Clear();
                            Context.Response.ContentType = "application/json";
                            Context.Response.AddHeader("content-length", response.Length.ToString());
                            Context.Response.Flush();
                            Context.Response.Write(response);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
            }
            else
            {
                Context.Response.Clear();
                Context.Response.StatusCode = 401;
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }
        catch (Exception ex)
        {
            Context.Response.Clear();
            Context.Response.StatusCode = 500;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void emplogoutservice()
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            //added by naveeen 
            string token = System.Web.HttpContext.Current.Request.Headers["token"];
            string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
            string uuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
            //end
            List<emplogins> surveybenelist = new List<emplogins>();
            DateTime extendsessionexpdate = DateTime.Now;
            cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET LogoutTime=@extenddate, IsActive=@inactive WHERE (EmployeeCode = @emplcode) AND (SessionToken = @stoken) AND (DeviceID=@duuid) AND (IsActive=@active)");
            cmd.Parameters.Add("@emplcode", employecode);
            cmd.Parameters.Add("@stoken", token);
            cmd.Parameters.Add("@duuid", uuid);
            cmd.Parameters.Add("@active", true);
            cmd.Parameters.Add("@extenddate", extendsessionexpdate);
            cmd.Parameters.Add("@inactive", false);
            vdm.Update(cmd);

            string message = "logout success";
            emplogins newemp = new emplogins();
            newemp.Msg = message;
            surveybenelist.Add(newemp);

            List<empoveralllist> getlogoutdtls = new List<empoveralllist>();
            empoveralllist getoverDatas = new empoveralllist();
            getoverDatas.getempoveralllist = surveybenelist;
            getlogoutdtls.Add(getoverDatas);
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = Int32.MaxValue;
            string response = jsonSerializer.Serialize(getlogoutdtls);
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.AddHeader("content-length", response.Length.ToString());
            Context.Response.Flush();
            Context.Response.Write(response);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {

        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void ClearIMEIdetails(string empid)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            //added by naveeen 
            string token = System.Web.HttpContext.Current.Request.Headers["token"];
            string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
            string uuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
            //end
            List<emplogins> surveybenelist = new List<emplogins>();
            DateTime extendsessionexpdate = DateTime.Now;
            cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM  tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
            cmd.Parameters.Add("@empcode", employecode);
            cmd.Parameters.Add("@token", token);
            cmd.Parameters.Add("@uuid", uuid);
            cmd.Parameters.Add("@IsActive", true);
            DataTable dttoken = vdm.SelectQuery(cmd).Tables[0];
            if (dttoken.Rows.Count > 0)
            {
                foreach (DataRow drt in dttoken.Rows)
                {
                    string sessionexpdate = drt["SessionExpiryTime"].ToString();
                    DateTime dtsessionexpdate = Convert.ToDateTime(sessionexpdate);
                    DateTime nowdatetime = DateTime.Now;
                    if (nowdatetime > dtsessionexpdate)
                    {
                        Context.Response.Clear();
                        Context.Response.StatusCode = 401;
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        cmd = new SqlCommand("DELETE FROM tbl_trn_EmployeeIMEI WHERE (EmployeeCode = @emplcode) AND (IsActive=@active)");
                        cmd.Parameters.Add("@emplcode", empid);
                        cmd.Parameters.Add("@active", true);
                        vdm.Delete(cmd);

                        cmd = new SqlCommand("SELECT EmployeeCode, SAPEmployeeCode, EmployeeName, EmailID, DesignationCode FROM  tbl_MST_Employee WHERE (EmployeeCode = @empid)");
                        cmd.Parameters.Add("@empid", empid);
                        DataTable dtempdetails = vdm.SelectQuery(cmd).Tables[0];
                        string empname = "";
                        if (dtempdetails.Rows.Count > 0)
                        {
                            foreach(DataRow dr in dtempdetails.Rows)
                            {
                                empname = dr["EmployeeName"].ToString();
                            }
                            string message = "" + empname + " device has been deregistered (SAP ID " + empid + ")";
                            emplogins newemp = new emplogins();
                            newemp.Msg = message;
                            surveybenelist.Add(newemp);
                        }
                        else
                        {
                            string message = "No Registered User with this SAP ID " + empid + "";
                            emplogins newemp = new emplogins();
                            newemp.Msg = message;
                            surveybenelist.Add(newemp);
                        }
                    }
                }
            }
            List<empoveralllist> getlogoutdtls = new List<empoveralllist>();
            empoveralllist getoverDatas = new empoveralllist();
            getoverDatas.getempoveralllist = surveybenelist;
            getlogoutdtls.Add(getoverDatas);
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = Int32.MaxValue;
            string response = jsonSerializer.Serialize(getlogoutdtls);
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.AddHeader("content-length", response.Length.ToString());
            Context.Response.Flush();
            Context.Response.Write(response);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {

        }
    }


    private DateTime GetLowDate(DateTime dt)
    {
        double Hour, Min, Sec;
        DateTime DT = DateTime.Now;
        DT = dt;
        Hour = -dt.Hour;
        Min = -dt.Minute;
        Sec = -dt.Second;
        DT = DT.AddHours(Hour);
        DT = DT.AddMinutes(Min);
        DT = DT.AddSeconds(Sec);
        return DT;
    }

    private DateTime GetHighDate(DateTime dt)
    {
        double Hour, Min, Sec;
        DateTime DT = DateTime.Now;
        Hour = 23 - dt.Hour;
        Min = 59 - dt.Minute;
        Sec = 59 - dt.Second;
        DT = dt;
        DT = DT.AddHours(Hour);
        DT = DT.AddMinutes(Min);
        DT = DT.AddSeconds(Sec);
        return DT;
    }
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public string password { get; set; }
        public string email { get; set; }
    }
    public class TechnowellLoginResponse
    {
        public bool IsValidCredientials { get; set; }
        public string Message { get; set; }
        public string EmployeeCode { get; set; }
    }
    public class tokendetails
    {
        public string authtoken { get; set; }
        public string Msg { get; set; }
    }
    public class emplogins
    {
        public string EmpName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailID { get; set; }
        public string authtoken { get; set; }
        public string Msg { get; set; }
        public string Designation { get; set; }
        public string Role { get; set; }
    }
    public class emploginattendance
    {
       
        public string Msg { get; set; }
        public string status_code { get; set; }
        public string count { get; set; }
    }
    public class empoveralllist
    {
        public List<emplogins> getempoveralllist { get; set; }
    }
    public class empoverallattendancelist
    {
        public List<emploginattendance> getempoveralllist { get; set; }
    }

}
