using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using Npgsql;
using System.Web.Script.Services;
using System.Data;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Collections;
using System.Linq;
using System.IO;
using System.Net;

/// <summary>z
/// Summary description for uploadservice
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
 [System.Web.Script.Services.ScriptService]
public class uploadservice : System.Web.Services.WebService {

    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();
    NpgsqlCommand postcmd;
    SAPdbmanger postvdm = new SAPdbmanger();
    

    public class emp_status
    {
        public string empid { get; set; }
        public string mappingcode { get; set; }
        public string respondentcode { get; set; }
        public string UploadedFrom { get; set; }
    }
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void sdnew(string empid, string lastsynced, string parentresponse, string childresponse, string surveydata, string selectedben)
    {
        try
        {
            //added by naveeen 
            string token = System.Web.HttpContext.Current.Request.Headers["token"];
            string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
            string uuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
            //end

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
                        DateTime extendsessionexpdate = nowdatetime.AddHours(1);
                        cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET SessionExpiryTime=@extenddate WHERE (EmployeeCode = @emplcode) AND (SessionToken = @stoken) AND (DeviceID=@duuid) AND (IsActive=@active)");
                        cmd.Parameters.Add("@emplcode", employecode);
                        cmd.Parameters.Add("@stoken", token);
                        cmd.Parameters.Add("@duuid", uuid);
                        cmd.Parameters.Add("@active", true);
                        cmd.Parameters.Add("@extenddate", extendsessionexpdate);
                        vdm.Update(cmd);

                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        dynamic objparentResponse1 = jsonSerializer.DeserializeObject(parentresponse);
                        dynamic objchildResponse = jsonSerializer.DeserializeObject(childresponse);
                        dynamic objsurveydata = jsonSerializer.DeserializeObject(surveydata);
                        dynamic objselectedben = jsonSerializer.DeserializeObject(selectedben);
                        //save ben
                        foreach (var slben in objselectedben)
                        {
                            string val = slben.Key;
                            Dictionary<string, object> smap = new Dictionary<string, object>(slben.Value);
                            object swmapcode;
                            smap.TryGetValue("surveyworkitemmappingcode", out swmapcode);
                            string surveyworkitemmappingcode = swmapcode.ToString();

                            object swrescode;
                            smap.TryGetValue("resondantcode", out swrescode);
                            string Respondentcode = swrescode.ToString();

                            if (Respondentcode == "" || Respondentcode == null)
                            {

                            }
                            else
                            {
                                string InterventionCode = "";
                                cmd = new SqlCommand("SELECT tm.ConceptualizedInterventionCode FROM  tbl_TRN_WorkItem AS tw INNER JOIN tbl_TRN_MileStone AS tm ON tw.MileStoneCode = tm.MileStoneCode INNER JOIN  tbl_MMP_SurveyWorkItem AS tsw ON tw.WorkItemCode = tsw.WorkItemCode WHERE (tsw.SurveyWorkItemMappingCode = @swmpc)");
                                cmd.Parameters.Add("@swmpc", surveyworkitemmappingcode);
                                DataTable dtworkitem = vdm.SelectQuery(cmd).Tables[0];
                                if (dtworkitem.Rows.Count > 0)
                                {
                                    foreach (DataRow dr in dtworkitem.Rows)
                                    {
                                        InterventionCode = dr["ConceptualizedInterventionCode"].ToString();
                                    }
                                }

                                cmd = new SqlCommand("SELECT BeneficiaryListing, SurveyCode FROM tbl_MMP_SurveyBeneficiary WHERE RespondantCode=@respcode AND InterventionCode=@InterventionCode");
                                cmd.Parameters.Add("@InterventionCode", InterventionCode);
                                cmd.Parameters.Add("@respcode", Respondentcode);
                                DataTable dtBeneficiarydetails = vdm.SelectQuery(cmd).Tables[0];
                                if (dtBeneficiarydetails.Rows.Count > 0)
                                {
                                }
                                else
                                {
                                    cmd = new SqlCommand("INSERT INTO tbl_MMP_SurveyBeneficiary (SurveyCode, RespondantCode, IsActive, Type, CreatedBy, CreatedOn,  InterventionCode, UploadedFrom) VALUES (@SurveyCode, @RspondantCode, @IsActive, @Type, @CreatedBy, @CreatedOn, @InterventionCode, @UploadedFrom)");
                                    cmd.Parameters.Add("@RspondantCode", Respondentcode);
                                    cmd.Parameters.Add("@IsActive", "True");
                                    cmd.Parameters.Add("@Type", "Individual");
                                    cmd.Parameters.Add("@CreatedBy", empid);
                                    cmd.Parameters.Add("@CreatedOn", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
                                    cmd.Parameters.Add("@InterventionCode", InterventionCode);
                                    cmd.Parameters.Add("@SurveyCode", surveyworkitemmappingcode);
                                    cmd.Parameters.Add("@UploadedFrom", "Android");
                                    vdm.insert(cmd);
                                }
                            }

                        }
                           
                        // end ben
                        int i = 0;
                        List<emp_status> empbenelist = new List<emp_status>();
                        foreach (var item in objparentResponse1)
                        {
                            string val = item.Key;
                            Dictionary<string, object> map = new Dictionary<string, object>(item.Value);
                            Guid g = Guid.NewGuid();
                            string surveyresponseid = g.ToString();

                            object pempid;
                            map.TryGetValue("empid", out pempid);
                            string parentempid = pempid.ToString();

                            object svalue;
                            map.TryGetValue("surveyworkitemmappingcode", out svalue);
                            string surveyworkitemmappingcode = svalue.ToString();

                            object rspcode;
                            map.TryGetValue("respondentcode", out rspcode);
                            string respondentcode = rspcode.ToString();

                            object pqtion;
                            map.TryGetValue("parentquestion", out pqtion);
                            string parentquestion = pqtion.ToString();


                            object qcode;
                            map.TryGetValue("questioncode", out qcode);
                            string questioncode = qcode.ToString();

                            object seccode;
                            map.TryGetValue("sectioncode", out seccode);
                            string sectioncode = seccode.ToString();

                            object ans;
                            map.TryGetValue("answer", out ans);
                            string answer = ans.ToString();


                            object sdate;
                            map.TryGetValue("surveydate", out sdate);
                            string surveydate = sdate.ToString();

                            object stats;
                            map.TryGetValue("status", out stats);
                            string status = stats.ToString();

                            object cron;
                            map.TryGetValue("createdon", out cron);
                            string createdon = cron.ToString();

                            object UploadFrom;
                            map.TryGetValue("UploadedFrom", out UploadFrom);
                            string UploadedFrom = UploadFrom.ToString();

                            object syncon;
                            map.TryGetValue("syncedon", out syncon);
                            string syncedon = "";
                            if (syncon == null || syncon == String.Empty)
                            {
                                syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                            }
                            else
                            {
                                syncedon = syncon.ToString();
                            }
                            DateTime servydt = DateTime.Now;
                            if (surveydate != "")
                            {
                                servydt = Convert.ToDateTime(surveydate);
                            }
                            cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn, PlannedDate FROM  tbl_TRN_SurveyResponse WHERE (SurveyWorkItemMappingCode = @dswimpc) AND (RespondantCode = @drspcode) AND (ParentQuestionCode=@dpqc) AND (QuestionCode = @dqc)");
                            cmd.Parameters.Add("@dswimpc", surveyworkitemmappingcode);
                            cmd.Parameters.Add("@drspcode", respondentcode);
                            cmd.Parameters.Add("@dqc", questioncode);
                            cmd.Parameters.Add("@dpqc", parentquestion);
                            DataTable dtsrwithoudate = vdm.SelectQuery(cmd).Tables[0];
                            if (dtsrwithoudate.Rows.Count > 0)
                            {
                                foreach (DataRow drd in dtsrwithoudate.Rows)
                                {
                                    string mpdate = drd["PlannedDate"].ToString();
                                    DateTime dtsd = Convert.ToDateTime(mpdate);
                                    servydt = dtsd;
                                }
                            }
                            cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn, PlannedDate, UploadedFrom FROM  tbl_TRN_SurveyResponse WHERE (SurveyWorkItemMappingCode = @swimpc) AND (RespondantCode = @rspcode) AND (ParentQuestionCode=@pqc) AND (QuestionCode = @qc) AND (PlannedDate BETWEEN @plndt AND @plndt2)");
                            cmd.Parameters.Add("@swimpc", surveyworkitemmappingcode);
                            cmd.Parameters.Add("@rspcode", respondentcode);
                            cmd.Parameters.Add("@qc", questioncode);
                            cmd.Parameters.Add("@pqc", parentquestion);
                            cmd.Parameters.Add("@plndt", GetLowDate(servydt));
                            cmd.Parameters.Add("@plndt2", GetHighDate(servydt));
                            DataTable dtsr = vdm.SelectQuery(cmd).Tables[0];
                            if (dtsr.Rows.Count > 0)
                            {
                                foreach (DataRow dr in dtsr.Rows)
                                {
                                    string chkstatus = dr["Status"].ToString();
                                    if (chkstatus == "2")
                                    {

                                    }
                                    else
                                    {
                                        cmd = new SqlCommand("UPDATE tbl_TRN_SurveyResponse SET  Answer=@Answer, SurveyDate=@SurveyDate, PlannedDate=@pdate, SurveyDoneBy=@empid, SynchedOn=@doe, Status=@Status, UploadedFrom=@UploadedFrom WHERE SurveyWorkItemMappingCode=@swmc AND RespondantCode=@rc AND QuestionCode=@qcc AND PlannedDate=@pdate");
                                        cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
                                        cmd.Parameters.Add("@rc", respondentcode);
                                        cmd.Parameters.Add("@pqc", parentquestion);
                                        cmd.Parameters.Add("@qcc", questioncode);
                                        cmd.Parameters.Add("@sc", sectioncode);
                                        cmd.Parameters.Add("@Answer", answer);
                                        cmd.Parameters.Add("@SurveyDate", createdon);
                                        cmd.Parameters.Add("@pdate", surveydate);
                                        cmd.Parameters.Add("@Status", status);
                                        cmd.Parameters.Add("@empid", parentempid);
                                        if (syncedon == null)
                                        {
                                            syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                        }
                                        cmd.Parameters.Add("@doe", syncedon);
                                        cmd.Parameters.Add("@UploadedFrom", UploadedFrom);
                                        if (vdm.Update(cmd) == 0)
                                        {
                                            cmd = new SqlCommand("UPDATE tbl_TRN_SurveyResponse SET  Answer=@Answer, SurveyDate=@SurveyDate, PlannedDate=@pdate, SurveyDoneBy=@empid, SynchedOn=@doe, Status=@Status, UploadedFrom=@UploadedFrom WHERE SurveyWorkItemMappingCode=@swmc AND RespondantCode=@rc AND QuestionCode=@qcc");
                                            cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
                                            cmd.Parameters.Add("@rc", respondentcode);
                                            cmd.Parameters.Add("@pqc", parentquestion);
                                            cmd.Parameters.Add("@qcc", questioncode);
                                            cmd.Parameters.Add("@sc", sectioncode);
                                            cmd.Parameters.Add("@Answer", answer);
                                            cmd.Parameters.Add("@SurveyDate", createdon);
                                            cmd.Parameters.Add("@pdate", surveydate);
                                            cmd.Parameters.Add("@Status", status);
                                            cmd.Parameters.Add("@empid", parentempid);
                                            if (syncedon == null)
                                            {
                                                syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            cmd.Parameters.Add("@doe", syncedon);
                                            cmd.Parameters.Add("@UploadedFrom", UploadedFrom);
                                            vdm.Update(cmd);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                cmd = new SqlCommand("INSERT INTO tbl_TRN_SurveyResponse(SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn, CreatedBy, PlannedDate, SurveyDoneBy, UploadedFrom) values (@SRID, @swmc, @rc, @pqc, @qcc, @sc, @Answer, @SurveyDate, @Status, @CreatedOn, @syncon, @empid, @PlannedDate, @sdoneby, @UploadedFrom)");
                                cmd.Parameters.Add("@SRID", surveyresponseid);
                                cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
                                cmd.Parameters.Add("@rc", respondentcode);
                                cmd.Parameters.Add("@pqc", parentquestion);
                                cmd.Parameters.Add("@qcc", questioncode);
                                cmd.Parameters.Add("@sc", sectioncode);
                                cmd.Parameters.Add("@Answer", answer);
                                cmd.Parameters.Add("@PlannedDate", surveydate);
                                cmd.Parameters.Add("@SurveyDate", createdon);
                                cmd.Parameters.Add("@Status", status);
                                cmd.Parameters.Add("@CreatedOn", createdon);
                                if (syncedon == null)
                                {
                                    syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                cmd.Parameters.Add("@syncon", syncedon);
                                cmd.Parameters.Add("@empid", parentempid);
                                cmd.Parameters.Add("@sdoneby", parentempid);
                                cmd.Parameters.Add("@UploadedFrom", UploadedFrom);
                                vdm.insert(cmd);
                            }
                        }

                        foreach (var itemchild in objchildResponse)
                        {
                            string val = itemchild.Key;
                            Dictionary<string, object> map = new Dictionary<string, object>(itemchild.Value);

                            Guid g = Guid.NewGuid();
                            string surveyresponseid = g.ToString();
                            object cempid;
                            map.TryGetValue("empid", out cempid);
                            string childempid = cempid.ToString();

                            object rrkid;
                            map.TryGetValue("responserankid", out rrkid);
                            string responserankid = rrkid.ToString();

                            object svalue;
                            map.TryGetValue("surveyworkitemmappingcode", out svalue);
                            string surveyworkitemmappingcode = svalue.ToString();

                            object rspcode;
                            map.TryGetValue("respondentcode", out rspcode);
                            string respondentcode = rspcode.ToString();

                            object pqtion;
                            map.TryGetValue("parentquestion", out pqtion);
                            string parentquestion = pqtion.ToString();


                            object qcode;
                            map.TryGetValue("questioncode", out qcode);
                            string questioncode = qcode.ToString();

                            object seccode;
                            map.TryGetValue("sectioncode", out seccode);
                            string sectioncode = seccode.ToString();

                            object ans;
                            map.TryGetValue("answer", out ans);
                            string answer = ans.ToString();

                            object sdate;
                            map.TryGetValue("surveydate", out sdate);
                            string surveydate = sdate.ToString();

                            object stats;
                            map.TryGetValue("status", out stats);
                            string status = stats.ToString();

                            object cron;
                            map.TryGetValue("createdon", out cron);
                            string createdon = cron.ToString();

                            object UploadFrom;
                            map.TryGetValue("UploadedFrom", out UploadFrom);
                            string UploadedFrom = UploadFrom.ToString();

                            

                            object syncon;
                            map.TryGetValue("syncedon", out syncon);
                            string syncedon = "";
                            if (syncon == null || syncon == String.Empty)
                            {
                                syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                            }
                            else
                            {
                                syncedon = syncon.ToString();
                            }
                            DateTime servydt = DateTime.Now;
                            if (surveydate != "")
                            {
                                servydt = Convert.ToDateTime(surveydate);
                            }
                            cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn FROM  tbl_TRN_SurveyChildResponse WHERE (SurveyWorkItemMappingCode = @swimpc) AND (RespondantCode = @rspcode) AND (ParentQuestionCode=@pqc) AND (QuestionCode = @qc) AND (PlannedDate=@d1)");
                            cmd.Parameters.Add("@swimpc", surveyworkitemmappingcode);
                            cmd.Parameters.Add("@rspcode", respondentcode);
                            cmd.Parameters.Add("@qc", questioncode);
                            cmd.Parameters.Add("@pqc", parentquestion);
                            cmd.Parameters.Add("@d1", surveydate);
                            DataTable dtsrc = vdm.SelectQuery(cmd).Tables[0];
                            if (dtsrc.Rows.Count > 0)
                            {
                                cmd = new SqlCommand("UPDATE tbl_TRN_SurveyChildResponse SET  Answer=@Answer, SurveyDate=@SurveyDate,  SurveyDoneBy=@empid, SynchedOn=@doe, Status=@Status, UploadedFrom=@UploadedFrom WHERE SurveyWorkItemMappingCode=@swmc AND RespondantCode=@rc AND QuestionCode=@qcc AND PlannedDate=@pdate");
                                cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
                                cmd.Parameters.Add("@rc", respondentcode);
                                cmd.Parameters.Add("@pqc", parentquestion);
                                cmd.Parameters.Add("@qcc", questioncode);
                                cmd.Parameters.Add("@sc", sectioncode);
                                cmd.Parameters.Add("@Answer", answer);
                                cmd.Parameters.Add("@SurveyDate", createdon);
                                cmd.Parameters.Add("@pdate", surveydate);
                                cmd.Parameters.Add("@Status", status);
                                cmd.Parameters.Add("@empid", childempid);
                                if (syncedon == null)
                                {
                                    syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                cmd.Parameters.Add("@doe", syncedon);
                                cmd.Parameters.Add("@UploadedFrom", UploadedFrom);
                                vdm.Update(cmd);
                            }
                            else
                            {
                                cmd = new SqlCommand("INSERT INTO tbl_TRN_SurveyChildResponse(SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, PlannedDate, SurveyDoneBy, Status, SynchedOn, CreatedOn, CreatedBy) values (@srid, @rrid, @swmc, @rc, @pqc, @qcc, @sc, @Answer, @SurveyDate, @pdate, @empid, @Status,  @SynchedOn, @CreatedOn, @empidd)");
                                cmd.Parameters.Add("@srid", surveyresponseid);
                                cmd.Parameters.Add("@rrid", responserankid);
                                cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
                                cmd.Parameters.Add("@rc", respondentcode);
                                cmd.Parameters.Add("@pqc", parentquestion);
                                cmd.Parameters.Add("@qcc", questioncode);
                                cmd.Parameters.Add("@sc", sectioncode);
                                cmd.Parameters.Add("@Answer", answer);
                                cmd.Parameters.Add("@SurveyDate", createdon);
                                cmd.Parameters.Add("@pdate", surveydate);
                                cmd.Parameters.Add("@Status", status);
                                cmd.Parameters.Add("@CreatedOn", createdon);
                                cmd.Parameters.Add("@empid", childempid);
                                cmd.Parameters.Add("@empidd", childempid);
                                if (syncedon == null)
                                {
                                    syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                cmd.Parameters.Add("@SynchedOn", syncedon);
                                cmd.Parameters.Add("@UploadedFrom", UploadedFrom);
                                vdm.insert(cmd);
                            }
                        }

                        foreach (var sitem in objsurveydata)
                        {
                            Dictionary<string, object> smap = new Dictionary<string, object>(sitem.Value);
                            object svalue;
                            smap.TryGetValue("surveyworkitemmappingcode", out svalue);
                            string surveyworkitemmappingcode = svalue.ToString();
                            object rspcode;
                            smap.TryGetValue("workitemcode", out rspcode);
                            string workitemcode = rspcode.ToString();

                            cmd = new SqlCommand("SELECT BeneficiaryListing, SurveyCode, RespondantCode, UploadedFrom FROM tbl_MMP_SurveyBeneficiary WHERE  SurveyCode=@surveycode");
                            cmd.Parameters.Add("@surveycode", surveyworkitemmappingcode);
                            DataTable dtBeneficiarydetails = vdm.SelectQuery(cmd).Tables[0];

                            cmd = new SqlCommand("SELECT DISTINCT SurveyWorkItemMappingCode, RespondantCode, Status, SurveyDoneBy, PlannedDate, UploadedFrom FROM  tbl_TRN_SurveyResponse WHERE  (SurveyWorkItemMappingCode=@swmpcode) ORDER BY SurveyWorkItemMappingCode");
                            cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
                            DataTable dtbenestatus = vdm.SelectQuery(cmd).Tables[0];


                            if (dtBeneficiarydetails.Rows.Count > 0)
                            {
                                foreach (DataRow dr in dtBeneficiarydetails.Rows)
                                {
                                    string respondentcode = dr["RespondantCode"].ToString();
                                    emp_status empdetails = new emp_status();
                                    empdetails.respondentcode = respondentcode;

                                    empdetails.mappingcode = surveyworkitemmappingcode;
                                    empdetails.UploadedFrom = dr["UploadedFrom"].ToString();

                                    string SurveyDoneBy = "0";
                                    string empname = "";
                                    string SurveyDoneempid = "";
                                    foreach (DataRow dtbs in dtbenestatus.Select("RespondantCode='" + respondentcode + "'"))
                                    {
                                        SurveyDoneBy = dtbs["SurveyDoneBy"].ToString();
                                        SurveyDoneempid = SurveyDoneBy;
                                    }
                                    empdetails.empid = SurveyDoneempid;
                                    empbenelist.Add(empdetails);
                                }
                            }
                            else
                            {
                                if (dtbenestatus.Rows.Count > 0)
                                {

                                }
                            }
                        }

                        List<empstatusbenf> getbenfstatusdtls = new List<empstatusbenf>();
                        empstatusbenf getoverDatas = new empstatusbenf();
                        getoverDatas.employebenelist = empbenelist;
                        getbenfstatusdtls.Add(getoverDatas);

                        jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getbenfstatusdtls);
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

    

    public class empstatusbenf  //new
    {
        public List<emp_status> employebenelist { get; set; }
    }
    private static string GetJson(object obj)
    {
        JavaScriptSerializer jsonSerializerS = new JavaScriptSerializer();
        jsonSerializerS.MaxJsonLength = Int32.MaxValue;
        return jsonSerializerS.Serialize(obj);
    }


    [System.Serializable]
    public struct KeyValuePair<TKey, TValue>
    {

    }

    public class updata
    {
        public Dictionary<string, string> fre { get; set; }
        
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

    public class surveydata
    {
        public string surveyresponseid { get; set; }
        public string surveyworkitemmappingcode { get; set; }
        public string respondentcode { get; set; }
        public string parentquestion { get; set; }
        public string questioncode { get; set; }
        public string sectioncode { get; set; }
        public string answer { get; set; }
        public string answerremark { get; set; }
        public string surveydate { get; set; }
        public string planneddate { get; set; }
        public string completiondate { get; set; }
        public string surveydoneby { get; set; }
        public string status { get; set; }
        public string isflagged { get; set; }
        public string issynced { get; set; }
        public string flagdate { get; set; }
        public string flagtime { get; set; }
    }

    public class parent
    {
        public string surveyresponseid { get; set; }
        public string surveyworkitemmappingcode { get; set; }
        public string respondentcode { get; set; }
        public string parentquestion { get; set; }
        public string questioncode { get; set; }
        public string sectioncode { get; set; }
        public string answer { get; set; }
        public string answerremark { get; set; }
        public string surveydate { get; set; }
        public string planneddate { get; set; }
        public string completiondate { get; set; }
        public string surveydoneby { get; set; }
        public string status { get; set; }
        public string isflagged { get; set; }
        public string issynced { get; set; }
        public string flagdate { get; set; }
        public string flagtime { get; set; }
        public string flagremark { get; set; }
        public string createdby { get; set; }
        public string createdon { get; set; }
        public string modifiedby { get; set; }
        public string modifiedon { get; set; }
        public string syncedon { get; set; }
        public string clientid { get; set; }
    }

    public class child
    {
        public string surveyresponseid { get; set; }
        public string responserankid { get; set; }
        public string surveyworkitemmappingcode { get; set; }
        public string respondentcode { get; set; }
        public string parentquestion { get; set; }
        public string questioncode { get; set; }
        public string sectioncode { get; set; }
        public string answer { get; set; }
        public string answerremark { get; set; }
        public string surveydate { get; set; }
        public string planneddate { get; set; }
        public string completiondate { get; set; }
        public string surveydoneby { get; set; }
        public string status { get; set; }
        public string isflagged { get; set; }
        public string issynced { get; set; }
        public string flagdate { get; set; }
        public string flagtime { get; set; }
        public string flagremark { get; set; }
        public string createdby { get; set; }
        public string createdon { get; set; }
        public string modifiedby { get; set; }
        public string modifiedon { get; set; }
        public string syncedon { get; set; }
        public string clientid { get; set; }
    }

    public class bene_status
    {
        public string surveyworkitemmappingcode { get; set; }
        public string respondantcode { get; set; }
        public string status { get; set; }
        public string surveydoneby { get; set; }
    }

    public class beneficiarydtls  //new
    {
        public List<parent> parent { get; set; }
        public List<child> child { get; set; }
    }


    public static string myArray1 { get; set; }

    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //[WebMethod]
    //public void savedtls(string empid, string lastsynced, List<parent> parentresponse, List<child> childresponse, List<surveydata> surveydata)
    //{
    //    try
    //    {
    //        for (int i = 0; i < parentresponse.Count; i++)
    //        {
    //            Guid g = Guid.NewGuid();
    //            string surveyresponseid = g.ToString();
    //            string surveyworkitemmappingcode = parentresponse[i].surveyworkitemmappingcode;
    //            string respondentcode = parentresponse[i].respondentcode;
    //            string parentquestion = parentresponse[i].parentquestion;
    //            string questioncode = parentresponse[i].questioncode;
    //            string sectioncode = parentresponse[i].sectioncode;
    //            string answer = parentresponse[i].answer;
    //            string surveydate = parentresponse[i].surveydate;
    //            DateTime servydt = DateTime.Now;
    //            if (surveydate != "")
    //            {
    //                servydt = Convert.ToDateTime(surveydate);
    //            }
    //            string status = parentresponse[i].status;
    //            string createdon = parentresponse[i].createdon;
    //            string syncedon = parentresponse[i].syncedon;

    //            cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn, PlannedDate FROM  tbl_TRN_SurveyResponse WHERE (SurveyWorkItemMappingCode = @swimpc) AND (RespondantCode = @rspcode) AND (ParentQuestionCode=@pqc) AND (QuestionCode = @qc) AND (PlannedDate=@plndt)");
    //            cmd.Parameters.Add("@swimpc", surveyworkitemmappingcode);
    //            cmd.Parameters.Add("@rspcode", respondentcode);
    //            cmd.Parameters.Add("@qc", questioncode);
    //            cmd.Parameters.Add("@pqc", parentquestion);
    //            cmd.Parameters.Add("@plndt", surveydate);
    //            DataTable dtsr = vdm.SelectQuery(cmd).Tables[0];
    //            if (dtsr.Rows.Count > 0)
    //            {
    //                foreach (DataRow dr in dtsr.Rows)
    //                {
    //                    cmd = new SqlCommand("UPDATE tbl_TRN_SurveyResponse SET  Answer=@Answer, SurveyDate=@SurveyDate, PlannedDate=@pdate, SurveyDoneBy=@empid, SynchedOn=@doe, Status=@Status WHERE SurveyWorkItemMappingCode=@swmc AND RespondantCode=@rc AND QuestionCode=@qcc AND PlannedDate=@pdate");
    //                    cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //                    cmd.Parameters.Add("@rc", respondentcode);
    //                    cmd.Parameters.Add("@pqc", parentquestion);
    //                    cmd.Parameters.Add("@qcc", questioncode);
    //                    cmd.Parameters.Add("@sc", sectioncode);
    //                    cmd.Parameters.Add("@Answer", answer);
    //                    cmd.Parameters.Add("@SurveyDate", createdon);
    //                    cmd.Parameters.Add("@pdate", surveydate);
    //                    cmd.Parameters.Add("@Status", status);
    //                    cmd.Parameters.Add("@empid", empid);
    //                    if (syncedon == null)
    //                    {
    //                        syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    cmd.Parameters.Add("@doe", syncedon);
    //                    vdm.Update(cmd);
    //                }
    //            }
    //            else
    //            {
    //                cmd = new SqlCommand("INSERT INTO tbl_TRN_SurveyResponse(SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn, CreatedBy, PlannedDate, SurveyDoneBy) values (@SRID, @swmc, @rc, @pqc, @qcc, @sc, @Answer, @SurveyDate, @Status, @CreatedOn, @syncon, @empid, @PlannedDate, @sdoneby)");
    //                cmd.Parameters.Add("@SRID", surveyresponseid);
    //                cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //                cmd.Parameters.Add("@rc", respondentcode);
    //                cmd.Parameters.Add("@pqc", parentquestion);
    //                cmd.Parameters.Add("@qcc", questioncode);
    //                cmd.Parameters.Add("@sc", sectioncode);
    //                cmd.Parameters.Add("@Answer", answer);
    //                cmd.Parameters.Add("@PlannedDate", surveydate);
    //                cmd.Parameters.Add("@SurveyDate", createdon);
    //                cmd.Parameters.Add("@Status", status);
    //                cmd.Parameters.Add("@CreatedOn", createdon);
    //                if (syncedon == null)
    //                {
    //                    syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //                }
    //                cmd.Parameters.Add("@syncon", syncedon);
    //                cmd.Parameters.Add("@empid", empid);
    //                cmd.Parameters.Add("@sdoneby", empid);
    //                vdm.insert(cmd);
    //            }
    //        }
    //        for (int i = 0; i < childresponse.Count; i++)
    //        {
    //            Guid g = Guid.NewGuid();
    //            string surveyresponseid = g.ToString();
    //            string responserankid = childresponse[i].responserankid;
    //            string surveyworkitemmappingcode = childresponse[i].surveyworkitemmappingcode;
    //            string respondentcode = childresponse[i].respondentcode;
    //            string parentquestion = childresponse[i].parentquestion;
    //            string questioncode = childresponse[i].questioncode;
    //            string sectioncode = childresponse[i].sectioncode;
    //            string answer = childresponse[i].answer;
    //            string surveydate = childresponse[i].surveydate;
    //            string status = childresponse[i].status;
    //            string createdon = childresponse[i].createdon;
    //            string syncedon = childresponse[i].syncedon;
    //            DateTime servydt = DateTime.Now;
    //            if (surveydate != "")
    //            {
    //                servydt = Convert.ToDateTime(surveydate);
    //            }
    //            cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn FROM  tbl_TRN_SurveyChildResponse WHERE (SurveyWorkItemMappingCode = @swimpc) AND (RespondantCode = @rspcode) AND (ParentQuestionCode=@pqc) AND (QuestionCode = @qc) AND (PlannedDate=@d1)");
    //            cmd.Parameters.Add("@swimpc", surveyworkitemmappingcode);
    //            cmd.Parameters.Add("@rspcode", respondentcode);
    //            cmd.Parameters.Add("@qc", questioncode);
    //            cmd.Parameters.Add("@pqc", parentquestion);
    //            cmd.Parameters.Add("@d1", surveydate);
    //            DataTable dtsrc = vdm.SelectQuery(cmd).Tables[0];
    //            if (dtsrc.Rows.Count > 0)
    //            {
    //                cmd = new SqlCommand("UPDATE tbl_TRN_SurveyChildResponse SET  Answer=@Answer, SurveyDate=@SurveyDate, PlannedDate=@pdate, SurveyDoneBy=@empid, SynchedOn=@doe, Status=@Status WHERE SurveyWorkItemMappingCode=@swmc AND RespondantCode=@rc AND QuestionCode=@qcc AND PlannedDate=@pdate");
    //                cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //                cmd.Parameters.Add("@rc", respondentcode);
    //                cmd.Parameters.Add("@pqc", parentquestion);
    //                cmd.Parameters.Add("@qcc", questioncode);
    //                cmd.Parameters.Add("@sc", sectioncode);
    //                cmd.Parameters.Add("@Answer", answer);
    //                cmd.Parameters.Add("@SurveyDate", createdon);
    //                cmd.Parameters.Add("@pdate", surveydate);
    //                cmd.Parameters.Add("@Status", status);
    //                cmd.Parameters.Add("@empid", empid);
    //                if (syncedon == null)
    //                {
    //                    syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //                }
    //                cmd.Parameters.Add("@doe", syncedon);
    //                vdm.Update(cmd);
    //            }
    //            else
    //            {
    //                cmd = new SqlCommand("INSERT INTO tbl_TRN_SurveyChildResponse(SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, PlannedDate, SurveyDoneBy, Status, SynchedOn, CreatedOn, CreatedBy) values (@srid, @rrid, @swmc, @rc, @pqc, @qcc, @sc, @Answer, @SurveyDate, @pdate, @empid, @Status,  @SynchedOn, @CreatedOn, @empidd)");
    //                cmd.Parameters.Add("@srid", surveyresponseid);
    //                cmd.Parameters.Add("@rrid", responserankid);
    //                cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //                cmd.Parameters.Add("@rc", respondentcode);
    //                cmd.Parameters.Add("@pqc", parentquestion);
    //                cmd.Parameters.Add("@qcc", questioncode);
    //                cmd.Parameters.Add("@sc", sectioncode);
    //                cmd.Parameters.Add("@Answer", answer);
    //                cmd.Parameters.Add("@SurveyDate", createdon);
    //                cmd.Parameters.Add("@pdate", surveydate);
    //                cmd.Parameters.Add("@Status", status);
    //                cmd.Parameters.Add("@CreatedOn", createdon);
    //                cmd.Parameters.Add("@empid", empid);
    //                cmd.Parameters.Add("@empidd", empid);
    //                if (syncedon == null)
    //                {
    //                    syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //                }
    //                cmd.Parameters.Add("@SynchedOn", syncedon);
    //                vdm.insert(cmd);
    //            }
    //        }
    //        //string msg = "Data Sucessfully Inserted";
    //        //JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //        //jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //        //Context.Response.Write(jsonSerializer.Serialize(msg));
    //    }
    //    catch (Exception ex)
    //    {
    //        string msg = "Please Privede Input Data";
    //        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //        jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //        Context.Response.Write(jsonSerializer.Serialize(msg));
    //    }
    //}

    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void sd(string empid, string lastsynced, string parentresponse, string childresponse, string surveydata)
    //{
    //    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //    dynamic objparentResponse1 = jsonSerializer.DeserializeObject(parentresponse);
    //    dynamic objchildResponse = jsonSerializer.DeserializeObject(childresponse);
    //    int i = 0;
    //    foreach (var item in objparentResponse1)
    //    {
    //        string val = item.Key;
    //        Dictionary<string, object> map = new Dictionary<string, object>( item.Value );
    //        Guid g = Guid.NewGuid();
    //        string surveyresponseid = g.ToString();
    //        object svalue;
    //        map.TryGetValue("surveyworkitemmappingcode", out svalue);
    //        string surveyworkitemmappingcode = svalue.ToString();

    //        object rspcode;
    //        map.TryGetValue("respondentcode", out rspcode);
    //        string respondentcode = rspcode.ToString();

    //        object pqtion;
    //        map.TryGetValue("parentquestion", out pqtion);
    //        string parentquestion = pqtion.ToString();


    //        object qcode;
    //        map.TryGetValue("questioncode", out qcode);
    //        string questioncode = qcode.ToString();

    //        object seccode;
    //        map.TryGetValue("sectioncode", out seccode);
    //        string sectioncode = seccode.ToString();

    //        object ans;
    //        map.TryGetValue("answer", out ans);
    //        string answer = ans.ToString();


    //        object sdate;
    //        map.TryGetValue("surveydate", out sdate);
    //        string surveydate = sdate.ToString();

    //        object stats;
    //        map.TryGetValue("status", out stats);
    //        string status = stats.ToString();

    //        object cron;
    //        map.TryGetValue("createdon", out cron);
    //        string createdon = cron.ToString();

    //        object syncon;
    //        map.TryGetValue("syncedon", out syncon);
    //        string syncedon = "";
    //        if (syncon == null || syncon == String.Empty)
    //        {
    //            syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //        }
    //        else
    //        {
    //            syncedon = syncon.ToString();
    //        }
    //        DateTime servydt = DateTime.Now;
    //        if (surveydate != "")
    //        {
    //            servydt = Convert.ToDateTime(surveydate);
    //        }




    //        cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn, PlannedDate FROM  tbl_TRN_SurveyResponse WHERE (SurveyWorkItemMappingCode = @swimpc) AND (RespondantCode = @rspcode) AND (ParentQuestionCode=@pqc) AND (QuestionCode = @qc) AND (PlannedDate BETWEEN @plndt AND @plndt2)");
    //        cmd.Parameters.Add("@swimpc", surveyworkitemmappingcode);
    //        cmd.Parameters.Add("@rspcode", respondentcode);
    //        cmd.Parameters.Add("@qc", questioncode);
    //        cmd.Parameters.Add("@pqc", parentquestion);
    //        cmd.Parameters.Add("@plndt", GetLowDate(servydt));
    //        cmd.Parameters.Add("@plndt2", GetHighDate(servydt));
    //        DataTable dtsr = vdm.SelectQuery(cmd).Tables[0];
    //        if (dtsr.Rows.Count > 0)
    //        {
    //            foreach (DataRow dr in dtsr.Rows)
    //            {
    //                cmd = new SqlCommand("UPDATE tbl_TRN_SurveyResponse SET  Answer=@Answer, SurveyDate=@SurveyDate, PlannedDate=@pdate, SurveyDoneBy=@empid, SynchedOn=@doe, Status=@Status WHERE SurveyWorkItemMappingCode=@swmc AND RespondantCode=@rc AND QuestionCode=@qcc AND PlannedDate=@pdate");
    //                cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //                cmd.Parameters.Add("@rc", respondentcode);
    //                cmd.Parameters.Add("@pqc", parentquestion);
    //                cmd.Parameters.Add("@qcc", questioncode);
    //                cmd.Parameters.Add("@sc", sectioncode);
    //                cmd.Parameters.Add("@Answer", answer);
    //                cmd.Parameters.Add("@SurveyDate", createdon);
    //                cmd.Parameters.Add("@pdate", surveydate);
    //                cmd.Parameters.Add("@Status", status);
    //                cmd.Parameters.Add("@empid", empid);
    //                if (syncedon == null)
    //                {
    //                    syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //                }
    //                cmd.Parameters.Add("@doe", syncedon);
    //                vdm.Update(cmd);
    //            }
    //        }
    //        else
    //        {
    //            cmd = new SqlCommand("INSERT INTO tbl_TRN_SurveyResponse(SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn, CreatedBy, PlannedDate, SurveyDoneBy) values (@SRID, @swmc, @rc, @pqc, @qcc, @sc, @Answer, @SurveyDate, @Status, @CreatedOn, @syncon, @empid, @PlannedDate, @sdoneby)");
    //            cmd.Parameters.Add("@SRID", surveyresponseid);
    //            cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //            cmd.Parameters.Add("@rc", respondentcode);
    //            cmd.Parameters.Add("@pqc", parentquestion);
    //            cmd.Parameters.Add("@qcc", questioncode);
    //            cmd.Parameters.Add("@sc", sectioncode);
    //            cmd.Parameters.Add("@Answer", answer);
    //            cmd.Parameters.Add("@PlannedDate", surveydate);
    //            cmd.Parameters.Add("@SurveyDate", createdon);
    //            cmd.Parameters.Add("@Status", status);
    //            cmd.Parameters.Add("@CreatedOn", createdon);
    //            if (syncedon == null)
    //            {
    //                syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //            }
    //            cmd.Parameters.Add("@syncon", syncedon);
    //            cmd.Parameters.Add("@empid", empid);
    //            cmd.Parameters.Add("@sdoneby", empid);
    //            vdm.insert(cmd);
    //        }
    //    }

    //    foreach (var itemchild in objchildResponse)
    //    {
    //        string val = itemchild.Key;
    //        Dictionary<string, object> map = new Dictionary<string, object>(itemchild.Value);

    //        Guid g = Guid.NewGuid();
    //        string surveyresponseid = g.ToString();
    //        object rrkid;
    //        map.TryGetValue("responserankid", out rrkid);
    //        string responserankid = rrkid.ToString();

    //        object svalue;
    //        map.TryGetValue("surveyworkitemmappingcode", out svalue);
    //        string surveyworkitemmappingcode = svalue.ToString();

    //        object rspcode;
    //        map.TryGetValue("respondentcode", out rspcode);
    //        string respondentcode = rspcode.ToString();

    //        object pqtion;
    //        map.TryGetValue("parentquestion", out pqtion);
    //        string parentquestion = pqtion.ToString();


    //        object qcode;
    //        map.TryGetValue("questioncode", out qcode);
    //        string questioncode = qcode.ToString();

    //        object seccode;
    //        map.TryGetValue("sectioncode", out seccode);
    //        string sectioncode = seccode.ToString();

    //        object ans;
    //        map.TryGetValue("answer", out ans);
    //        string answer = ans.ToString();

    //        object sdate;
    //        map.TryGetValue("surveydate", out sdate);
    //        string surveydate = sdate.ToString();

    //        object stats;
    //        map.TryGetValue("status", out stats);
    //        string status = stats.ToString();

    //        object cron;
    //        map.TryGetValue("createdon", out cron);
    //        string createdon = cron.ToString();

    //        object syncon;
    //        map.TryGetValue("syncedon", out syncon);
    //        string syncedon = "";
    //        if (syncon == null || syncon == String.Empty)
    //        {
    //            syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //        }
    //        else
    //        {
    //            syncedon = syncon.ToString();
    //        }
    //        DateTime servydt = DateTime.Now;
    //        if (surveydate != "")
    //        {
    //            servydt = Convert.ToDateTime(surveydate);
    //        }
    //        cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, Status, CreatedOn, SynchedOn FROM  tbl_TRN_SurveyChildResponse WHERE (SurveyWorkItemMappingCode = @swimpc) AND (RespondantCode = @rspcode) AND (ParentQuestionCode=@pqc) AND (QuestionCode = @qc) AND (PlannedDate=@d1)");
    //        cmd.Parameters.Add("@swimpc", surveyworkitemmappingcode);
    //        cmd.Parameters.Add("@rspcode", respondentcode);
    //        cmd.Parameters.Add("@qc", questioncode);
    //        cmd.Parameters.Add("@pqc", parentquestion);
    //        cmd.Parameters.Add("@d1", surveydate);
    //        DataTable dtsrc = vdm.SelectQuery(cmd).Tables[0];
    //        if (dtsrc.Rows.Count > 0)
    //        {
    //            cmd = new SqlCommand("UPDATE tbl_TRN_SurveyChildResponse SET  Answer=@Answer, SurveyDate=@SurveyDate,  SurveyDoneBy=@empid, SynchedOn=@doe, Status=@Status WHERE SurveyWorkItemMappingCode=@swmc AND RespondantCode=@rc AND QuestionCode=@qcc AND PlannedDate=@pdate");
    //            cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //            cmd.Parameters.Add("@rc", respondentcode);
    //            cmd.Parameters.Add("@pqc", parentquestion);
    //            cmd.Parameters.Add("@qcc", questioncode);
    //            cmd.Parameters.Add("@sc", sectioncode);
    //            cmd.Parameters.Add("@Answer", answer);
    //            cmd.Parameters.Add("@SurveyDate", createdon);
    //            cmd.Parameters.Add("@pdate", surveydate);
    //            cmd.Parameters.Add("@Status", status);
    //            cmd.Parameters.Add("@empid", empid);
    //            if (syncedon == null)
    //            {
    //                syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //            }
    //            cmd.Parameters.Add("@doe", syncedon);
    //            vdm.Update(cmd);
    //        }
    //        else
    //        {
    //            cmd = new SqlCommand("INSERT INTO tbl_TRN_SurveyChildResponse(SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, SurveyDate, PlannedDate, SurveyDoneBy, Status, SynchedOn, CreatedOn, CreatedBy) values (@srid, @rrid, @swmc, @rc, @pqc, @qcc, @sc, @Answer, @SurveyDate, @pdate, @empid, @Status,  @SynchedOn, @CreatedOn, @empidd)");
    //            cmd.Parameters.Add("@srid", surveyresponseid);
    //            cmd.Parameters.Add("@rrid", responserankid);
    //            cmd.Parameters.Add("@swmc", surveyworkitemmappingcode);
    //            cmd.Parameters.Add("@rc", respondentcode);
    //            cmd.Parameters.Add("@pqc", parentquestion);
    //            cmd.Parameters.Add("@qcc", questioncode);
    //            cmd.Parameters.Add("@sc", sectioncode);
    //            cmd.Parameters.Add("@Answer", answer);
    //            cmd.Parameters.Add("@SurveyDate", createdon);
    //            cmd.Parameters.Add("@pdate", surveydate);
    //            cmd.Parameters.Add("@Status", status);
    //            cmd.Parameters.Add("@CreatedOn", createdon);
    //            cmd.Parameters.Add("@empid", empid);
    //            cmd.Parameters.Add("@empidd", empid);
    //            if (syncedon == null)
    //            {
    //                syncedon = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //            }
    //            cmd.Parameters.Add("@SynchedOn", syncedon);
    //            vdm.insert(cmd);
    //        }
    //    }
    //    string msg = "SAVE";
    //    JavaScriptSerializer jsonSerializerS = new JavaScriptSerializer();
    //    string response = jsonSerializerS.Serialize(msg);
    //    Context.Response.Clear();
    //    Context.Response.ContentType = "application/json";
    //    Context.Response.AddHeader("content-length", response.Length.ToString());
    //    Context.Response.Flush();
    //    Context.Response.Write(response);
    //    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //}
}
