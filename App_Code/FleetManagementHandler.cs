using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Data;
using System.Web.Services;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Collections;

/// <summary>
/// Summary description for FleetManagementHandler
/// </summary>
public class FleetManagementHandler : IHttpHandler, IRequiresSessionState
{
    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();
    MySqlCommand mycmd;
    private SqlDbType sisno;
    public FleetManagementHandler()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public bool IsReusable
    {
        get { return true; }
    }
    private static string GetJson(object obj)
    {
        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        jsonSerializer.MaxJsonLength = 2147483647;
        return jsonSerializer.Serialize(obj);
    }
    class GetJsonData
    {
        public string op { set; get; }
    }
    //  [WebMethod(Description="Delete Template",BufferResponse=false)]
    public void ProcessRequest(HttpContext context)
    {
        try
        {

            string operation = context.Request["op"];
            switch (operation)
            {
                case "get_serveyresponce_dtls":
                    get_serveyresponce_dtls(context);
                    break;
                default:
                    var jsonString = string.Empty;
                    context.Request.InputStream.Position = 0;
                    using (var inputStream = new StreamReader(context.Request.InputStream))
                    {
                        jsonString = HttpUtility.UrlDecode(inputStream.ReadToEnd());
                    }
                    if (jsonString != "")
                    {
                        var js = new JavaScriptSerializer();
                        // var title1 = context.Request.Params[1];
                        GetJsonData obj = js.Deserialize<GetJsonData>(jsonString);
                        switch (obj.op)
                        {
                            //save_possale

                        }
                    }
                    else
                    {
                        var js = new JavaScriptSerializer();
                        var title1 = context.Request.Params[1];
                        GetJsonData obj = js.Deserialize<GetJsonData>(title1);
                        switch (obj.op)
                        {
                            
                        }
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            string response = GetJson(ex.ToString());
            context.Response.Write(response);
        }
    }

    public class companydetails
    {
        public string count { get; set; }
        public string doe { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
        public string status { get; set; }
    }
    private void get_serveyresponce_dtls(HttpContext context)
    {
        try
        {
            vdm = new SalesDBManager();
            string fromdate = context.Request["fromdate"].ToString();
            string todate = context.Request["todate"].ToString();

            DateTime dtfromdate = Convert.ToDateTime(fromdate);
            DateTime dttodate = Convert.ToDateTime(todate);
            cmd = new SqlCommand("SELECT COUNT(*) AS count, Status FROM   tbl_TRN_SurveyResponse WHERE  (SynchedOn BETWEEN @d1 AND @d2) GROUP BY Status");
            cmd.Parameters.Add("@d1", GetLowDate(dtfromdate));
            cmd.Parameters.Add("@d2", GetHighDate(dttodate));
            DataTable dtcount = vdm.SelectQuery(cmd).Tables[0];
            List<companydetails> SectionMaster = new List<companydetails>();
            foreach (DataRow dr in dtcount.Rows)
            {
                companydetails getsectiondetails = new companydetails();
                getsectiondetails.count = dr["count"].ToString();
                getsectiondetails.status = dr["Status"].ToString();
                getsectiondetails.fromdate = fromdate;
                getsectiondetails.todate = todate;
                SectionMaster.Add(getsectiondetails);
            }
            string response = GetJson(SectionMaster);
            context.Response.Write(response);
        }
        catch (Exception ex)
        {
            string Response = GetJson(ex.Message);
            context.Response.Write(Response);
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

   
}







    





                

   