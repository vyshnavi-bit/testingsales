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
using System.Net;

/// <summary>
/// Summary description for statusservice
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
 [System.Web.Script.Services.ScriptService]
public class statusservice : System.Web.Services.WebService {

    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();
    NpgsqlCommand postcmd;
    SAPdbmanger postvdm = new SAPdbmanger();

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void updatebeneficiarynew(string empid, string updatedata)
    {
        try
        {
            //added by naveeen 
            string token = System.Web.HttpContext.Current.Request.Headers["token"];
            string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
            string uuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
            //end

            cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM            tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
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

                        DataTable locations = new DataTable();
                        locations.Columns.Add("WorkItemCode");
                        locations.Columns.Add("locationcode");
                        locations.Columns.Add("locationname");
                        locations.Columns.Add("locationtype");
                        List<statusbenf> getbenfstatusdtls = new List<statusbenf>();
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        dynamic objparentResponse1 = jsonSerializer.DeserializeObject(updatedata);
                        foreach (var item in objparentResponse1)
                        {
                            Dictionary<string, object> map = new Dictionary<string, object>(item);
                            string myw = map.ContainsKey("workitemcode").ToString();

                            object svalue;
                            map.TryGetValue("workitemcode", out svalue);
                            string workitemcode = svalue.ToString();

                            object mappingcode;
                            map.TryGetValue("surveyworkitemmappingcode", out mappingcode);
                            string surveyworkitemmappingcode = mappingcode.ToString();

                            object ptype;
                            map.TryGetValue("type", out ptype);
                            string type = ptype.ToString();


                            string SurveyCode = "";
                            cmd = new SqlCommand("SELECT  SurveyWorkItemMappingCode, SurveyCode, WorkItemCode, LocationCode, Status, isActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Frequncy, StartDate, EndDDate FROM  tbl_MMP_SurveyWorkItem WHERE (WorkItemCode = @wicode)");
                            cmd.Parameters.Add("@wicode", workitemcode);
                            DataTable dtsurveycode = vdm.SelectQuery(cmd).Tables[0];
                            if (dtsurveycode.Rows.Count > 0)
                            {
                                foreach (DataRow drsc in dtsurveycode.Rows)
                                {
                                    SurveyCode = drsc["SurveyCode"].ToString();
                                }
                            }

                            // added by naveen 04/05/2020
                            cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.LocationType, tbl_MST_Location.Label, tbl_MST_Location.CreatedBy, tbl_MST_Location.CreatedOn,  tbl_MST_Location.ModifiedBy, tbl_MST_Location.ModifiedOn, tbl_MST_Location.IsActive, tbl_MST_Location.ParentLocationCode, tbl_MST_Location.ParentLocCode, tbl_MMP_WorkItemLocation.RowCode,   tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MMP_WorkItemLocation.LocationCode AS Expr1, tbl_MMP_WorkItemLocation.CreatedBy AS Expr2, tbl_MMP_WorkItemLocation.CreatedOn AS Expr3,  tbl_MMP_WorkItemLocation.ModifiedBy AS Expr4, tbl_MMP_WorkItemLocation.ModifiedOn AS Expr5 FROM            tbl_MST_Location INNER JOIN tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode where tbl_MMP_WorkItemLocation.WorkItemCode=@wcode");
                            cmd.Parameters.Add("@wcode", workitemcode);
                            DataTable dtlocationdetails = vdm.SelectQuery(cmd).Tables[0];
                            if (dtlocationdetails.Rows.Count > 0)
                            {
                                foreach (DataRow loc in dtlocationdetails.Rows)
                                {
                                    DataRow newrowloc = locations.NewRow();
                                    newrowloc["WorkItemCode"] = workitemcode;
                                    newrowloc["locationcode"] = loc["LocationCode"].ToString();
                                    newrowloc["locationname"] = loc["LocationName"].ToString();
                                    newrowloc["locationtype"] = loc["LocationType"].ToString();
                                    locations.Rows.Add(newrowloc);

                                }
                            }

                            cmd = new SqlCommand("SELECT COUNT(*) AS count, BlockCode, GramPanchayatCode, VillageCode FROM  tbl_MST_Respondant GROUP BY BlockCode, GramPanchayatCode, VillageCode");
                            DataTable dtrespondentcode = vdm.SelectQuery(cmd).Tables[0];
                            double target = 0;
                            double blockcodecount = 0;
                            double gpcount = 0;
                            double villagecount = 0;
                            foreach (DataRow drl in locations.Select("WorkItemCode='" + workitemcode + "'"))
                            {
                                string loccode = drl["LocationCode"].ToString();
                                foreach (DataRow drb in dtrespondentcode.Select("BlockCode='" + loccode + "'"))
                                {
                                    string rccode = drb["count"].ToString();
                                    if (rccode != "" || rccode != null)
                                    {
                                        double bcount = Convert.ToDouble(rccode);
                                        blockcodecount += bcount;
                                    }
                                }
                                foreach (DataRow drg in dtrespondentcode.Select("GramPanchayatCode='" + loccode + "'"))
                                {
                                    string rccode = drg["count"].ToString();
                                    double gcount = Convert.ToDouble(rccode);
                                    gpcount += gcount;
                                }
                                foreach (DataRow drv in dtrespondentcode.Select("VillageCode='" + loccode + "'"))
                                {
                                    string rccode = drv["count"].ToString();
                                    double vcount = Convert.ToDouble(rccode);
                                    villagecount += vcount;
                                }
                            }
                            //end

                            if (type == "BS")
                            {
                                if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
                                {
                                    cmd = new SqlCommand("SELECT DISTINCT SurveyWorkItemMappingCode, RespondantCode, Status, SurveyDoneBy, PlannedDate, UploadedFrom FROM  tbl_TRN_SurveyResponse WHERE  (SurveyWorkItemMappingCode=@swmpcode) ORDER BY SurveyWorkItemMappingCode");
                                    cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
                                    DataTable dtbenestatus = vdm.SelectQuery(cmd).Tables[0];

                                    cmd = new SqlCommand("SELECT SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID, UploadedFrom FROM  tbl_TRN_SurveyChildResponse where SurveyWorkItemMappingCode=@swimc AND Status=@status ORDER BY SurveyWorkItemMappingCode");
                                    cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                    cmd.Parameters.Add("@status", "1");
                                    DataTable dtserveychaildresponce = vdm.SelectQuery(cmd).Tables[0];


                                    cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID, UploadedFrom  FROM  tbl_TRN_SurveyResponse  WHERE (Status = '1') AND (SurveyWorkItemMappingCode = @smpcode)");
                                    cmd.Parameters.Add("@smpcode", surveyworkitemmappingcode);
                                    DataTable dtserveyreponcedtls = vdm.SelectQuery(cmd).Tables[0];


                                    List<status> statuslist = new List<status>();
                                    List<empcountstatus> empcountstatuslist = new List<empcountstatus>();
                                    List<parent> parentlist = new List<parent>();
                                    List<child> childlist = new List<child>();
                                    List<bene_status> surveybenelist = new List<bene_status>();
                                    if (dtserveyreponcedtls.Rows.Count > 0)
                                    {
                                        if (type == "BS")
                                        {
                                            cmd = new SqlCommand("SELECT EmployeeCode, EmployeeName FROM  tbl_MST_Employee");
                                            DataTable dtempdtls = vdm.SelectQuery(cmd).Tables[0];
                                            foreach (DataRow drb in dtbenestatus.Rows)
                                            {
                                                bene_status survey = new bene_status();
                                                survey.surveyworkitemmappingcode = drb["SurveyWorkItemMappingCode"].ToString();
                                                survey.respondantcode = drb["RespondantCode"].ToString();
                                                survey.status = drb["Status"].ToString();
                                                survey.surveydoneby = drb["SurveyDoneBy"].ToString();
                                                string SurveyDoneBy = drb["SurveyDoneBy"].ToString();
                                                foreach (DataRow dremp in dtempdtls.Select("EmployeeCode='" + SurveyDoneBy + "'"))
                                                {
                                                    string empname = dremp["EmployeeName"].ToString();
                                                    survey.empname = empname;
                                                }
                                                survey.UploadedFrom = drb["UploadedFrom"].ToString();
                                                surveybenelist.Add(survey);
                                            }
                                            foreach (DataRow dr in dtserveyreponcedtls.Rows)
                                            {
                                                parent objparent = new parent();
                                                objparent.surveyresponseid = dr["SurveyResponseID"].ToString();
                                                objparent.surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                                objparent.respondantcode = dr["RespondantCode"].ToString();
                                                objparent.parentquestioncode = dr["ParentQuestionCode"].ToString();
                                                objparent.questioncode = dr["QuestionCode"].ToString();
                                                objparent.sectioncode = dr["SectionCode"].ToString();
                                                objparent.answer = dr["Answer"].ToString();
                                                objparent.answerremark = dr["AnswerRemark"].ToString();
                                                string SurveyDate = dr["SurveyDate"].ToString();
                                                if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
                                                {
                                                    DateTime dtSD = Convert.ToDateTime(SurveyDate);
                                                    //objparent.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
                                                    objparent.surveydate = dtSD.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objparent.surveydate = dr["SurveyDate"].ToString();
                                                }
                                                string PlannedDate = dr["PlannedDate"].ToString();
                                                if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
                                                {
                                                    DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
                                                    //objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
                                                    objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objparent.planneddate = dr["PlannedDate"].ToString();
                                                }
                                                string CompletionDate = dr["CompletionDate"].ToString();
                                                if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
                                                {
                                                    DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
                                                    objparent.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.completiondate = dr["CompletionDate"].ToString();
                                                }
                                                objparent.surveydoneby = dr["SurveyDoneBy"].ToString();
                                                objparent.status = dr["Status"].ToString();
                                                objparent.isflagged = dr["IsFlagged"].ToString();
                                                objparent.issynced = dr["IsSynced"].ToString();
                                                string FlagDate = dr["FlagDate"].ToString();
                                                if (FlagDate != "" && FlagDate != null && FlagDate != "null")
                                                {
                                                    DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
                                                    objparent.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.flagdate = dr["FlagDate"].ToString();
                                                }

                                                objparent.flagtime = dr["FlagTime"].ToString();
                                                objparent.flagremark = dr["FlagRemark"].ToString();
                                                objparent.createdby = dr["CreatedBy"].ToString();

                                                string CreatedOn = dr["CreatedOn"].ToString();
                                                if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
                                                {
                                                    DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
                                                    objparent.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.createdon = dr["CreatedOn"].ToString();
                                                }

                                                string SynchedOn = dr["SynchedOn"].ToString();
                                                if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
                                                {
                                                    DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
                                                    objparent.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.synchedon = dr["SynchedOn"].ToString();
                                                }

                                                string ModifiedOn = dr["ModifiedOn"].ToString();
                                                if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
                                                {
                                                    DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                                    objparent.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.modifiedon = dr["ModifiedOn"].ToString();
                                                }

                                                objparent.modifiedby = dr["ModifiedBy"].ToString();
                                                objparent.clientid = dr["ClientID"].ToString();
                                                objparent.UploadedFrom = dr["UploadedFrom"].ToString();
                                                parentlist.Add(objparent);
                                            }

                                            foreach (DataRow drc in dtserveychaildresponce.Rows)
                                            {
                                                child objchaild = new child();
                                                objchaild.surveyresponseid = drc["SurveyResponseID"].ToString();
                                                objchaild.responserankid = drc["ResponseRankID"].ToString();
                                                objchaild.surveyworkitemmappingcode = drc["SurveyWorkItemMappingCode"].ToString();
                                                objchaild.respondantcode = drc["RespondantCode"].ToString();
                                                objchaild.parentquestioncode = drc["ParentQuestionCode"].ToString();
                                                objchaild.questioncode = drc["QuestionCode"].ToString();
                                                objchaild.sectioncode = drc["SectionCode"].ToString();
                                                objchaild.answer = drc["Answer"].ToString();
                                                objchaild.answerremark = drc["AnswerRemark"].ToString();
                                                string SurveyDate = drc["SurveyDate"].ToString();
                                                if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
                                                {
                                                    DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
                                                    objchaild.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objchaild.surveydate = drc["SurveyDate"].ToString();
                                                }
                                                string PlannedDate = drc["PlannedDate"].ToString();
                                                if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
                                                {
                                                    DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
                                                    objchaild.planneddate = dtPlannedDate.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objchaild.planneddate = drc["PlannedDate"].ToString();
                                                }
                                                string CompletionDate = drc["CompletionDate"].ToString();
                                                if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
                                                {
                                                    DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
                                                    objchaild.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.completiondate = drc["CompletionDate"].ToString();
                                                }
                                                objchaild.surveydoneby = drc["SurveyDoneBy"].ToString();
                                                objchaild.status = drc["Status"].ToString();
                                                objchaild.isflagged = drc["IsFlagged"].ToString();
                                                objchaild.issynced = drc["IsSynced"].ToString();
                                                string FlagDate = drc["FlagDate"].ToString();
                                                if (FlagDate != "" && FlagDate != null && FlagDate != "null")
                                                {
                                                    DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
                                                    objchaild.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.flagdate = drc["FlagDate"].ToString();
                                                }
                                                objchaild.flagtime = drc["FlagTime"].ToString();
                                                objchaild.flagremark = drc["FlagRemark"].ToString();
                                                objchaild.createdby = drc["CreatedBy"].ToString();
                                                string CreatedOn = drc["CreatedOn"].ToString();
                                                if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
                                                {
                                                    DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
                                                    objchaild.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.createdon = drc["CreatedOn"].ToString();
                                                }
                                                string SynchedOn = drc["SynchedOn"].ToString();
                                                if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
                                                {
                                                    DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
                                                    objchaild.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.synchedon = drc["SynchedOn"].ToString();
                                                }
                                                string ModifiedOn = drc["ModifiedOn"].ToString();
                                                if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
                                                {
                                                    DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                                    objchaild.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.modifiedon = drc["ModifiedOn"].ToString();
                                                }
                                                objchaild.modifiedby = drc["ModifiedBy"].ToString();
                                                objchaild.clientid = drc["ClientID"].ToString();
                                                objchaild.UploadedFrom = drc["UploadedFrom"].ToString();
                                                childlist.Add(objchaild);
                                            }
                                        }

                                        // added by naveen

                                        if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
                                        {
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                        }
                                        else
                                        {
                                            cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
                                            cmd.Parameters.Add("@wicode", workitemcode);
                                            DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
                                            if (dtworkitemdetails.Rows.Count > 0)
                                            {
                                                foreach (DataRow dr in dtworkitemdetails.Rows)
                                                {
                                                    surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                                }
                                            }
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                        }
                                        DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];

                                        double gtot = blockcodecount + gpcount + villagecount;
                                        target = gtot;
                                        double submitedcount = 0;
                                        double savedcount = 0;
                                        if (dtserveyresponce.Rows.Count > 0)
                                        {
                                            foreach (DataRow drsre in dtserveyresponce.Rows)
                                            {
                                                string status = drsre["Status"].ToString();
                                                string count = drsre["count"].ToString();
                                                if (count != "" || count != null)
                                                {
                                                    if (status == "2")
                                                    {
                                                        submitedcount = Convert.ToDouble(count);
                                                    }
                                                    else
                                                    {
                                                        savedcount = Convert.ToDouble(count);
                                                    }
                                                }
                                            }
                                        }

                                        status syncst = new status();
                                        syncst.target = target.ToString();
                                        double sumcount = savedcount + submitedcount;
                                        double opncnt = target - sumcount;
                                        syncst.open = opncnt.ToString();
                                        syncst.saved = savedcount.ToString();
                                        syncst.submitted = submitedcount.ToString();
                                        syncst.synced = submitedcount.ToString();
                                        syncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                        statuslist.Add(syncst);
                                        if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
                                        {
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) AND (SurveyDoneBy = @empid) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                            cmd.Parameters.Add("@empid", empid);
                                        }
                                        else
                                        {
                                            cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
                                            cmd.Parameters.Add("@wicode", workitemcode);
                                            DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
                                            if (dtworkitemdetails.Rows.Count > 0)
                                            {
                                                foreach (DataRow dr in dtworkitemdetails.Rows)
                                                {
                                                    surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                                }
                                            }
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) AND (SurveyDoneBy = @empid) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                            cmd.Parameters.Add("@empid", empid);
                                        }
                                        DataTable dtempserveyresponce = vdm.SelectQuery(cmd).Tables[0];

                                        double empgtot = blockcodecount + gpcount + villagecount;
                                        target = empgtot;
                                        double empsubmitedcount = 0;
                                        double empsavedcount = 0;
                                        if (dtempserveyresponce.Rows.Count > 0)
                                        {
                                            foreach (DataRow dresre in dtempserveyresponce.Rows)
                                            {
                                                string status = dresre["Status"].ToString();
                                                string count = dresre["count"].ToString();
                                                if (count != "" || count != null)
                                                {
                                                    if (status == "2")
                                                    {
                                                        empsubmitedcount = Convert.ToDouble(count);
                                                    }
                                                    else
                                                    {
                                                        empsavedcount = Convert.ToDouble(count);
                                                    }
                                                }
                                            }
                                        }

                                        empcountstatus empsyncst = new empcountstatus();
                                        empsyncst.target = target.ToString();
                                        double empsumcount = empsavedcount + empsubmitedcount;
                                        double empopncnt = target - empsumcount;
                                        empsyncst.open = empopncnt.ToString();
                                        empsyncst.saved = empsavedcount.ToString();
                                        empsyncst.submitted = empsubmitedcount.ToString();
                                        empsyncst.synced = empsubmitedcount.ToString();
                                        empsyncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                        empcountstatuslist.Add(empsyncst);
                                    }
                                    else
                                    {
                                        //naveen new
                                        if (type == "BS")
                                        {
                                            foreach (DataRow drb in dtbenestatus.Rows)
                                            {
                                                bene_status survey = new bene_status();
                                                survey.surveyworkitemmappingcode = drb["SurveyWorkItemMappingCode"].ToString();
                                                survey.respondantcode = drb["RespondantCode"].ToString();
                                                survey.status = drb["Status"].ToString();
                                                survey.surveydoneby = drb["SurveyDoneBy"].ToString();
                                                survey.UploadedFrom = drb["UploadedFrom"].ToString();
                                                surveybenelist.Add(survey);
                                            }
                                            foreach (DataRow dr in dtserveyreponcedtls.Rows)
                                            {
                                                parent objparent = new parent();
                                                objparent.surveyresponseid = dr["SurveyResponseID"].ToString();
                                                objparent.surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                                objparent.respondantcode = dr["RespondantCode"].ToString();
                                                objparent.parentquestioncode = dr["ParentQuestionCode"].ToString();
                                                objparent.questioncode = dr["QuestionCode"].ToString();
                                                objparent.sectioncode = dr["SectionCode"].ToString();
                                                objparent.answer = dr["Answer"].ToString();
                                                objparent.answerremark = dr["AnswerRemark"].ToString();
                                                string SurveyDate = dr["SurveyDate"].ToString();
                                                if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
                                                {
                                                    DateTime dtSD = Convert.ToDateTime(SurveyDate);
                                                    //objparent.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
                                                    objparent.surveydate = dtSD.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objparent.surveydate = dr["SurveyDate"].ToString();
                                                }
                                                string PlannedDate = dr["PlannedDate"].ToString();
                                                if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
                                                {
                                                    DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
                                                    //objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
                                                    objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objparent.planneddate = dr["PlannedDate"].ToString();
                                                }
                                                string CompletionDate = dr["CompletionDate"].ToString();
                                                if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
                                                {
                                                    DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
                                                    objparent.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.completiondate = dr["CompletionDate"].ToString();
                                                }
                                                objparent.surveydoneby = dr["SurveyDoneBy"].ToString();
                                                objparent.status = dr["Status"].ToString();
                                                objparent.isflagged = dr["IsFlagged"].ToString();
                                                objparent.issynced = dr["IsSynced"].ToString();
                                                string FlagDate = dr["FlagDate"].ToString();
                                                if (FlagDate != "" && FlagDate != null && FlagDate != "null")
                                                {
                                                    DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
                                                    objparent.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.flagdate = dr["FlagDate"].ToString();
                                                }

                                                objparent.flagtime = dr["FlagTime"].ToString();
                                                objparent.flagremark = dr["FlagRemark"].ToString();
                                                objparent.createdby = dr["CreatedBy"].ToString();

                                                string CreatedOn = dr["CreatedOn"].ToString();
                                                if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
                                                {
                                                    DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
                                                    objparent.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.createdon = dr["CreatedOn"].ToString();
                                                }

                                                string SynchedOn = dr["SynchedOn"].ToString();
                                                if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
                                                {
                                                    DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
                                                    objparent.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.synchedon = dr["SynchedOn"].ToString();
                                                }

                                                string ModifiedOn = dr["ModifiedOn"].ToString();
                                                if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
                                                {
                                                    DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                                    objparent.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objparent.modifiedon = dr["ModifiedOn"].ToString();
                                                }

                                                objparent.modifiedby = dr["ModifiedBy"].ToString();


                                                objparent.clientid = dr["ClientID"].ToString();
                                                objparent.UploadedFrom = dr["UploadedFrom"].ToString();
                                                parentlist.Add(objparent);
                                            }

                                            foreach (DataRow drc in dtserveychaildresponce.Rows)
                                            {
                                                child objchaild = new child();
                                                objchaild.surveyresponseid = drc["SurveyResponseID"].ToString();
                                                objchaild.responserankid = drc["ResponseRankID"].ToString();
                                                objchaild.surveyworkitemmappingcode = drc["SurveyWorkItemMappingCode"].ToString();
                                                objchaild.respondantcode = drc["RespondantCode"].ToString();
                                                objchaild.parentquestioncode = drc["ParentQuestionCode"].ToString();
                                                objchaild.questioncode = drc["QuestionCode"].ToString();
                                                objchaild.sectioncode = drc["SectionCode"].ToString();
                                                objchaild.answer = drc["Answer"].ToString();
                                                objchaild.answerremark = drc["AnswerRemark"].ToString();
                                                string SurveyDate = drc["SurveyDate"].ToString();
                                                if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
                                                {
                                                    DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
                                                    objchaild.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objchaild.surveydate = drc["SurveyDate"].ToString();
                                                }
                                                string PlannedDate = drc["PlannedDate"].ToString();
                                                if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
                                                {
                                                    DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
                                                    objchaild.planneddate = dtPlannedDate.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    objchaild.planneddate = drc["PlannedDate"].ToString();
                                                }
                                                string CompletionDate = drc["CompletionDate"].ToString();
                                                if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
                                                {
                                                    DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
                                                    objchaild.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.completiondate = drc["CompletionDate"].ToString();
                                                }
                                                objchaild.surveydoneby = drc["SurveyDoneBy"].ToString();
                                                objchaild.status = drc["Status"].ToString();
                                                objchaild.isflagged = drc["IsFlagged"].ToString();
                                                objchaild.issynced = drc["IsSynced"].ToString();
                                                string FlagDate = drc["FlagDate"].ToString();
                                                if (FlagDate != "" && FlagDate != null && FlagDate != "null")
                                                {
                                                    DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
                                                    objchaild.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.flagdate = drc["FlagDate"].ToString();
                                                }
                                                objchaild.flagtime = drc["FlagTime"].ToString();
                                                objchaild.flagremark = drc["FlagRemark"].ToString();
                                                objchaild.createdby = drc["CreatedBy"].ToString();
                                                string CreatedOn = drc["CreatedOn"].ToString();
                                                if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
                                                {
                                                    DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
                                                    objchaild.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.createdon = drc["CreatedOn"].ToString();
                                                }
                                                string SynchedOn = drc["SynchedOn"].ToString();
                                                if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
                                                {
                                                    DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
                                                    objchaild.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.synchedon = drc["SynchedOn"].ToString();
                                                }
                                                string ModifiedOn = drc["ModifiedOn"].ToString();
                                                if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
                                                {
                                                    DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                                    objchaild.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    objchaild.modifiedon = drc["ModifiedOn"].ToString();
                                                }
                                                objchaild.modifiedby = drc["ModifiedBy"].ToString();
                                                objchaild.clientid = drc["ClientID"].ToString();
                                                objchaild.UploadedFrom = drc["UploadedFrom"].ToString();
                                                childlist.Add(objchaild);
                                            }
                                        }

                                        // added by naveen

                                        if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
                                        {
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                        }
                                        else
                                        {
                                            cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
                                            cmd.Parameters.Add("@wicode", workitemcode);
                                            DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
                                            if (dtworkitemdetails.Rows.Count > 0)
                                            {
                                                foreach (DataRow dr in dtworkitemdetails.Rows)
                                                {
                                                    surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                                }
                                            }
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                        }
                                        DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];

                                        double gtot = blockcodecount + gpcount + villagecount;
                                        target = gtot;
                                        double submitedcount = 0;
                                        double savedcount = 0;
                                        if (dtserveyresponce.Rows.Count > 0)
                                        {
                                            foreach (DataRow drsre in dtserveyresponce.Rows)
                                            {
                                                string status = drsre["Status"].ToString();
                                                string count = drsre["count"].ToString();
                                                if (count != "" || count != null)
                                                {
                                                    if (status == "2")
                                                    {
                                                        submitedcount = Convert.ToDouble(count);
                                                    }
                                                    else
                                                    {
                                                        savedcount = Convert.ToDouble(count);
                                                    }
                                                }
                                            }
                                        }

                                        status syncst = new status();
                                        syncst.target = target.ToString();
                                        double sumcount = savedcount + submitedcount;
                                        double opncnt = target - sumcount;
                                        syncst.open = opncnt.ToString();
                                        syncst.saved = savedcount.ToString();
                                        syncst.submitted = submitedcount.ToString();
                                        syncst.synced = submitedcount.ToString();
                                        syncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                        statuslist.Add(syncst);
                                        if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
                                        {
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) AND (SurveyDoneBy = @empid) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                            cmd.Parameters.Add("@empid", empid);
                                        }
                                        else
                                        {
                                            cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
                                            cmd.Parameters.Add("@wicode", workitemcode);
                                            DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
                                            if (dtworkitemdetails.Rows.Count > 0)
                                            {
                                                foreach (DataRow dr in dtworkitemdetails.Rows)
                                                {
                                                    surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                                }
                                            }
                                            cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) AND (SurveyDoneBy = @empid) GROUP BY Status");
                                            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                            cmd.Parameters.Add("@empid", empid);
                                        }
                                        DataTable dtempserveyresponce = vdm.SelectQuery(cmd).Tables[0];

                                        double empgtot = blockcodecount + gpcount + villagecount;
                                        target = empgtot;
                                        double empsubmitedcount = 0;
                                        double empsavedcount = 0;
                                        if (dtempserveyresponce.Rows.Count > 0)
                                        {
                                            foreach (DataRow dresre in dtempserveyresponce.Rows)
                                            {
                                                string status = dresre["Status"].ToString();
                                                string count = dresre["count"].ToString();
                                                if (count != "" || count != null)
                                                {
                                                    if (status == "2")
                                                    {
                                                        empsubmitedcount = Convert.ToDouble(count);
                                                    }
                                                    else
                                                    {
                                                        empsavedcount = Convert.ToDouble(count);
                                                    }
                                                }
                                            }
                                        }

                                        empcountstatus empsyncst = new empcountstatus();
                                        empsyncst.target = target.ToString();
                                        double empsumcount = empsavedcount + empsubmitedcount;
                                        double empopncnt = target - empsumcount;
                                        empsyncst.open = empopncnt.ToString();
                                        empsyncst.saved = empsavedcount.ToString();
                                        empsyncst.submitted = empsubmitedcount.ToString();
                                        empsyncst.synced = empsubmitedcount.ToString();
                                        empsyncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                        empcountstatuslist.Add(empsyncst);
                                    }
                                    statusbenf getoverDatas = new statusbenf();
                                    getoverDatas.surveyworkitemmappingcode = surveyworkitemmappingcode;
                                    getoverDatas.workitemcode = workitemcode;
                                    getoverDatas.surveycode = SurveyCode;
                                    getoverDatas.type = type;
                                    getoverDatas.bene_status = surveybenelist;
                                    getoverDatas.parent = parentlist;
                                    getoverDatas.child = childlist;
                                    getoverDatas.status = statuslist;
                                    getoverDatas.empstatus = empcountstatuslist;
                                    getbenfstatusdtls.Add(getoverDatas);
                                }
                            }
                            else
                            {
                                object frequency;
                                map.TryGetValue("frequency", out frequency);
                                List<frequency> lstItems = new JavaScriptSerializer().ConvertToType<List<frequency>>(frequency);

                                for (int i = 0; i < lstItems.Count; i++)
                                {
                                    DateTime DTSERVEYDATE = DateTime.Now;
                                    surveyworkitemmappingcode = lstItems[i].surveyworkitemmappingcode;
                                    string surveydate = lstItems[i].surveydate;
                                    if (surveydate != "" && surveydate != null)
                                    {
                                        DTSERVEYDATE = Convert.ToDateTime(surveydate);
                                    }
                                    cmd = new SqlCommand("SELECT DISTINCT SurveyCode, RespondantCode, CreatedBy, UploadedFrom  FROM  tbl_MMP_SurveyBeneficiary WHERE SurveyCode=@swmpcode");
                                    cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
                                    DataTable dtaddedbenf = vdm.SelectQuery(cmd).Tables[0];


                                    cmd = new SqlCommand("SELECT DISTINCT SurveyWorkItemMappingCode, RespondantCode, Status, SurveyDoneBy, PlannedDate, UploadedFrom FROM  tbl_TRN_SurveyResponse WHERE  (SurveyWorkItemMappingCode=@swmpcode) AND (PlannedDate BETWEEN @d1 and @d2) ORDER BY SurveyWorkItemMappingCode");
                                    cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
                                    cmd.Parameters.Add("@d1", GetLowDate(DTSERVEYDATE));
                                    cmd.Parameters.Add("@d2", GetHighDate(DTSERVEYDATE));
                                    DataTable dtbenestatus = vdm.SelectQuery(cmd).Tables[0];





                                    cmd = new SqlCommand("SELECT SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID, UploadedFrom FROM  tbl_TRN_SurveyChildResponse where SurveyWorkItemMappingCode=@swimc AND (Status=@status) AND (PlannedDate BETWEEN @dc1 and @dc2) ORDER BY SurveyWorkItemMappingCode");
                                    cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                    cmd.Parameters.Add("@status", "1");

                                    cmd.Parameters.Add("@dc1", GetLowDate(DTSERVEYDATE));
                                    cmd.Parameters.Add("@dc2", GetHighDate(DTSERVEYDATE));
                                    DataTable dtserveychaildresponce = vdm.SelectQuery(cmd).Tables[0];

                                    cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID, UploadedFrom  FROM  tbl_TRN_SurveyResponse  WHERE (Status = '1') AND (SurveyWorkItemMappingCode = @smpcode) AND (PlannedDate between @dp1 and @dp2)");
                                    cmd.Parameters.Add("@smpcode", surveyworkitemmappingcode);
                                    cmd.Parameters.Add("@dp1", GetLowDate(DTSERVEYDATE));
                                    cmd.Parameters.Add("@dp2", GetHighDate(DTSERVEYDATE));
                                    DataTable dtserveyreponcedtls = vdm.SelectQuery(cmd).Tables[0];

                                    List<status> statuslist = new List<status>();
                                    List<empcountstatus> empstatuslist = new List<empcountstatus>();
                                    List<parent> parentlist = new List<parent>();
                                    List<child> childlist = new List<child>();
                                    List<bene_status> surveybenelist = new List<bene_status>();
                                    if (dtaddedbenf.Rows.Count > 0)
                                    {
                                        cmd = new SqlCommand("SELECT EmployeeCode, EmployeeName FROM  tbl_MST_Employee");
                                        DataTable dtempdtls = vdm.SelectQuery(cmd).Tables[0];

                                        foreach (DataRow drb in dtaddedbenf.Rows)
                                        {
                                            bene_status survey = new bene_status();
                                            string RespondantCode = drb["RespondantCode"].ToString();
                                            survey.surveyworkitemmappingcode = drb["SurveyCode"].ToString();
                                            survey.respondantcode = RespondantCode;
                                            string status = "0";
                                            string SurveyDoneBy = "0";
                                            string empname = "";
                                            string SurveyDoneempid = "";
                                            string addedempid = drb["CreatedBy"].ToString(); 
                                            foreach (DataRow dtbs in dtbenestatus.Select("RespondantCode='" + RespondantCode + "'"))
                                            {
                                                status = dtbs["Status"].ToString();
                                                SurveyDoneBy = dtbs["SurveyDoneBy"].ToString();
                                                foreach (DataRow dremp in dtempdtls.Select("EmployeeCode='" + SurveyDoneBy + "'"))
                                                {
                                                    empname = dremp["EmployeeName"].ToString();
                                                    SurveyDoneempid = SurveyDoneBy;
                                                }
                                            }
                                            if (SurveyDoneBy == "0")
                                            {
                                                SurveyDoneBy = addedempid;
                                                foreach (DataRow dremp in dtempdtls.Select("EmployeeCode='" + SurveyDoneBy + "'"))
                                                {
                                                    empname = dremp["EmployeeName"].ToString();
                                                    SurveyDoneempid = SurveyDoneBy;
                                                }
                                            }
                                            survey.status = status;
                                            survey.surveydate = surveydate;
                                            survey.empname = empname;
                                            survey.UploadedFrom = drb["UploadedFrom"].ToString();
                                            survey.surveydoneby = SurveyDoneempid;
                                            surveybenelist.Add(survey);
                                        }
                                        foreach (DataRow dr in dtserveyreponcedtls.Rows)
                                        {
                                            parent objparent = new parent();
                                            objparent.surveyresponseid = dr["SurveyResponseID"].ToString();
                                            objparent.surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                            objparent.respondantcode = dr["RespondantCode"].ToString();
                                            objparent.parentquestioncode = dr["ParentQuestionCode"].ToString();
                                            objparent.questioncode = dr["QuestionCode"].ToString();
                                            objparent.sectioncode = dr["SectionCode"].ToString();
                                            objparent.answer = dr["Answer"].ToString();
                                            objparent.answerremark = dr["AnswerRemark"].ToString();
                                            string SurveyDate = dr["SurveyDate"].ToString();
                                            if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
                                            {
                                                DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
                                                objparent.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd");
                                            }
                                            else
                                            {
                                                objparent.surveydate = dr["SurveyDate"].ToString();
                                            }
                                            string PlannedDate = dr["PlannedDate"].ToString();
                                            if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
                                            {
                                                DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
                                                objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd");
                                            }
                                            else
                                            {
                                                objparent.planneddate = dr["PlannedDate"].ToString();
                                            }
                                            string CompletionDate = dr["CompletionDate"].ToString();
                                            if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
                                            {
                                                DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
                                                objparent.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objparent.completiondate = dr["CompletionDate"].ToString();
                                            }
                                            objparent.surveydoneby = dr["SurveyDoneBy"].ToString();
                                            objparent.status = dr["Status"].ToString();
                                            objparent.isflagged = dr["IsFlagged"].ToString();
                                            objparent.issynced = dr["IsSynced"].ToString();
                                            string FlagDate = dr["FlagDate"].ToString();
                                            if (FlagDate != "" && FlagDate != null && FlagDate != "null")
                                            {
                                                DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
                                                objparent.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objparent.flagdate = dr["FlagDate"].ToString();
                                            }

                                            objparent.flagtime = dr["FlagTime"].ToString();
                                            objparent.flagremark = dr["FlagRemark"].ToString();
                                            objparent.createdby = dr["CreatedBy"].ToString();

                                            string CreatedOn = dr["CreatedOn"].ToString();
                                            if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
                                            {
                                                DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
                                                objparent.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objparent.createdon = dr["CreatedOn"].ToString();
                                            }

                                            string SynchedOn = dr["SynchedOn"].ToString();
                                            if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
                                            {
                                                DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
                                                objparent.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objparent.synchedon = dr["SynchedOn"].ToString();
                                            }

                                            string ModifiedOn = dr["ModifiedOn"].ToString();
                                            if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
                                            {
                                                DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                                objparent.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objparent.modifiedon = dr["ModifiedOn"].ToString();
                                            }

                                            objparent.modifiedby = dr["ModifiedBy"].ToString();


                                            objparent.clientid = dr["ClientID"].ToString();
                                            objparent.UploadedFrom = dr["UploadedFrom"].ToString();
                                            parentlist.Add(objparent);
                                        }

                                        foreach (DataRow drc in dtserveychaildresponce.Rows)
                                        {
                                            child objchaild = new child();
                                            objchaild.surveyresponseid = drc["SurveyResponseID"].ToString();
                                            objchaild.responserankid = drc["ResponseRankID"].ToString();
                                            objchaild.surveyworkitemmappingcode = drc["SurveyWorkItemMappingCode"].ToString();
                                            objchaild.respondantcode = drc["RespondantCode"].ToString();
                                            objchaild.parentquestioncode = drc["ParentQuestionCode"].ToString();
                                            objchaild.questioncode = drc["QuestionCode"].ToString();
                                            objchaild.sectioncode = drc["SectionCode"].ToString();
                                            objchaild.answer = drc["Answer"].ToString();
                                            objchaild.answerremark = drc["AnswerRemark"].ToString();
                                            string SurveyDate = drc["SurveyDate"].ToString();
                                            if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
                                            {
                                                DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
                                                objchaild.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd");
                                            }
                                            else
                                            {
                                                objchaild.surveydate = drc["SurveyDate"].ToString();
                                            }
                                            string PlannedDate = drc["PlannedDate"].ToString();
                                            if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
                                            {
                                                DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
                                                objchaild.planneddate = dtPlannedDate.ToString("yyyy-MM-dd");
                                            }
                                            else
                                            {
                                                objchaild.planneddate = drc["PlannedDate"].ToString();
                                            }
                                            string CompletionDate = drc["CompletionDate"].ToString();
                                            if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
                                            {
                                                DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
                                                objchaild.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objchaild.completiondate = drc["CompletionDate"].ToString();
                                            }
                                            objchaild.surveydoneby = drc["SurveyDoneBy"].ToString();
                                            objchaild.status = drc["Status"].ToString();
                                            objchaild.isflagged = drc["IsFlagged"].ToString();
                                            objchaild.issynced = drc["IsSynced"].ToString();
                                            string FlagDate = drc["FlagDate"].ToString();
                                            if (FlagDate != "" && FlagDate != null && FlagDate != "null")
                                            {
                                                DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
                                                objchaild.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objchaild.flagdate = drc["FlagDate"].ToString();
                                            }
                                            objchaild.flagtime = drc["FlagTime"].ToString();
                                            objchaild.flagremark = drc["FlagRemark"].ToString();
                                            objchaild.createdby = drc["CreatedBy"].ToString();
                                            string CreatedOn = drc["CreatedOn"].ToString();
                                            if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
                                            {
                                                DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
                                                objchaild.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objchaild.createdon = drc["CreatedOn"].ToString();
                                            }
                                            string SynchedOn = drc["SynchedOn"].ToString();
                                            if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
                                            {
                                                DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
                                                objchaild.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objchaild.synchedon = drc["SynchedOn"].ToString();
                                            }
                                            string ModifiedOn = drc["ModifiedOn"].ToString();
                                            if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
                                            {
                                                DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                                objchaild.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                objchaild.modifiedon = drc["ModifiedOn"].ToString();
                                            }
                                            objchaild.modifiedby = drc["ModifiedBy"].ToString();
                                            objchaild.clientid = drc["ClientID"].ToString();
                                            objchaild.UploadedFrom = drc["UploadedFrom"].ToString();
                                            childlist.Add(objchaild);
                                        }
                                    }
                                    if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
                                    {
                                        cmd = new SqlCommand("SELECT COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) AND (PlannedDate between @p1 and @p2) GROUP BY Status");
                                        cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                        cmd.Parameters.Add("@p1", GetLowDate(DTSERVEYDATE));
                                        cmd.Parameters.Add("@p2", GetHighDate(DTSERVEYDATE));
                                    }
                                    DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];
                                    double submitedcount = 0;
                                    double savedcount = 0;
                                    if (dtserveyresponce.Rows.Count > 0)
                                    {
                                        foreach (DataRow drsre in dtserveyresponce.Rows)
                                        {
                                            string statuss = drsre["Status"].ToString();
                                            string count = drsre["count"].ToString();
                                            if (count != "" || count != null)
                                            {
                                                if (statuss == "2")
                                                {
                                                    submitedcount = Convert.ToDouble(count);
                                                }
                                                else
                                                {
                                                    savedcount = Convert.ToDouble(count);
                                                }
                                            }
                                        }
                                    }
                                    double opcount = 0;
                                    cmd = new SqlCommand("SELECT COUNT(*) AS opencount FROM  tbl_MMP_SurveyBeneficiary WHERE  (SurveyCode = @mpcode)");
                                    cmd.Parameters.Add("@mpcode", surveyworkitemmappingcode);
                                    DataTable dtselectedrespondents = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtselectedrespondents.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dtselectedrespondents.Rows)
                                        {
                                            opcount = Convert.ToDouble(dr["opencount"].ToString());
                                        }
                                    }
                                    //SELECT COUNT(*) AS opencount FROM  tbl_MMP_SurveyBeneficiary WHERE  (SurveyCode = '719')

                                    double gtot = blockcodecount + gpcount + villagecount;
                                    target = gtot;
                                    status syncst = new status();
                                    syncst.target = target.ToString();
                                    double sumcount = savedcount + submitedcount;
                                    double opencount = opcount - sumcount;
                                    syncst.open = opencount.ToString();
                                    syncst.saved = savedcount.ToString();
                                    syncst.submitted = submitedcount.ToString();
                                    syncst.synced = submitedcount.ToString();
                                    syncst.fromdate = DTSERVEYDATE.ToString("yyyy-MM-dd hh:mm tt");
                                    syncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                    statuslist.Add(syncst);




                                    //naveen

                                    if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
                                    {
                                        cmd = new SqlCommand("SELECT COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) AND (PlannedDate between @p1 and @p2) AND (SurveyDoneBy=@empid) GROUP BY Status");
                                        cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
                                        cmd.Parameters.Add("@p1", GetLowDate(DTSERVEYDATE));
                                        cmd.Parameters.Add("@p2", GetHighDate(DTSERVEYDATE));
                                        cmd.Parameters.Add("@empid", empid);
                                    }
                                    DataTable dtempserveyresponce = vdm.SelectQuery(cmd).Tables[0];
                                    double empsubmitedcount = 0;
                                    double empsavedcount = 0;
                                    if (dtempserveyresponce.Rows.Count > 0)
                                    {
                                        foreach (DataRow drempsre in dtempserveyresponce.Rows)
                                        {
                                            string empstatuss = drempsre["Status"].ToString();
                                            string empcount = drempsre["count"].ToString();
                                            if (empcount != "" || empcount != null)
                                            {
                                                if (empstatuss == "2")
                                                {
                                                    empsubmitedcount = Convert.ToDouble(empcount);
                                                }
                                                else
                                                {
                                                    empsavedcount = Convert.ToDouble(empcount);
                                                }
                                            }
                                        }
                                    }
                                    double empopcount = 0;
                                    cmd = new SqlCommand("SELECT COUNT(*) AS opencount FROM  tbl_MMP_SurveyBeneficiary WHERE  (SurveyCode = @mpcode)");
                                    cmd.Parameters.Add("@mpcode", surveyworkitemmappingcode);
                                    DataTable dtempselectedrespondents = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtempselectedrespondents.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dtempselectedrespondents.Rows)
                                        {
                                            empopcount = Convert.ToDouble(dr["opencount"].ToString());
                                        }
                                    }
                                    //SELECT COUNT(*) AS opencount FROM  tbl_MMP_SurveyBeneficiary WHERE  (SurveyCode = '719')

                                    double gtott = blockcodecount + gpcount + villagecount;
                                    target = gtott;
                                    empcountstatus empsyncst = new empcountstatus();
                                    empsyncst.target = target.ToString();
                                    double empsumcount = empsavedcount + empsubmitedcount;
                                    double empopencount = empopcount - empsumcount;
                                    empsyncst.open = empopencount.ToString();
                                    empsyncst.saved = empsavedcount.ToString();
                                    empsyncst.submitted = empsubmitedcount.ToString();
                                    empsyncst.synced = empsubmitedcount.ToString();
                                    empsyncst.fromdate = DTSERVEYDATE.ToString("yyyy-MM-dd hh:mm tt");
                                    empsyncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                                    empstatuslist.Add(empsyncst);

                                    statusbenf getoverDatas = new statusbenf();
                                    getoverDatas.surveyworkitemmappingcode = surveyworkitemmappingcode;
                                    getoverDatas.workitemcode = workitemcode;
                                    getoverDatas.surveycode = SurveyCode;
                                    getoverDatas.type = type;
                                    getoverDatas.bene_status = surveybenelist;
                                    getoverDatas.parent = parentlist;
                                    getoverDatas.child = childlist;
                                    getoverDatas.status = statuslist;
                                    getoverDatas.empstatus = empstatuslist;
                                    getbenfstatusdtls.Add(getoverDatas);
                                }
                            }
                        }
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

    // commented by naveen
    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void benficiarystatus(string workitemcode, string surveyworkitemmappingcode, string type)
    //{
    //    try
    //    {
    //        vdm = new SalesDBManager();
    //        postvdm = new SAPdbmanger();

    //        if (surveyworkitemmappingcode == null || surveyworkitemmappingcode == "" || surveyworkitemmappingcode == "null")
    //        {
    //            if (workitemcode != "" && workitemcode != null && workitemcode != "null")
    //            {
    //                cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
    //                cmd.Parameters.Add("@wicode", workitemcode);
    //                DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
    //                if (dtworkitemdetails.Rows.Count > 0)
    //                {
    //                    foreach (DataRow dr in dtworkitemdetails.Rows)
    //                    {
    //                        surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
    //                    }
    //                }
    //            }
    //        }

    //        if (type == "BS")
    //        {

    //            if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
    //            {
    //                cmd = new SqlCommand("SELECT DISTINCT SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, Status, SurveyDoneBy, PlannedDate FROM  tbl_TRN_SurveyResponse WHERE  (SurveyWorkItemMappingCode=@swmpcode) ORDER BY SurveyWorkItemMappingCode");
    //                cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
    //                DataTable dtbenestatus = vdm.SelectQuery(cmd).Tables[0];

    //                cmd = new SqlCommand("SELECT SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID FROM  tbl_TRN_SurveyChildResponse where SurveyWorkItemMappingCode=@swimc AND Status=@status ORDER BY SurveyWorkItemMappingCode");
    //                cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //                cmd.Parameters.Add("@status", "1");
    //                DataTable dtserveychaildresponce = vdm.SelectQuery(cmd).Tables[0];


    //                cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID  FROM  tbl_TRN_SurveyResponse  WHERE (Status = '1') AND (SurveyWorkItemMappingCode = @smpcode)");
    //                cmd.Parameters.Add("@smpcode", surveyworkitemmappingcode);
    //                DataTable dtserveyreponcedtls = vdm.SelectQuery(cmd).Tables[0];


    //                List<status> statuslist = new List<status>();
    //                List<parent> parentlist = new List<parent>();
    //                List<child> childlist = new List<child>();
    //                List<bene_status> surveybenelist = new List<bene_status>();
    //                if (dtserveyreponcedtls.Rows.Count > 0)
    //                {
    //                    if (type == "BS")
    //                    {
    //                        foreach (DataRow drb in dtbenestatus.Rows)
    //                        {
    //                            bene_status survey = new bene_status();
    //                            survey.surveyworkitemmappingcode = drb["SurveyWorkItemMappingCode"].ToString();
    //                            survey.respondantcode = drb["RespondantCode"].ToString();
    //                            survey.status = drb["Status"].ToString();
    //                            survey.surveydoneby = drb["SurveyDoneBy"].ToString();
    //                            surveybenelist.Add(survey);
    //                        }
    //                        foreach (DataRow dr in dtserveyreponcedtls.Rows)
    //                        {
    //                            parent objparent = new parent();
    //                            objparent.surveyresponseid = dr["SurveyResponseID"].ToString();
    //                            objparent.surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
    //                            objparent.respondantcode = dr["RespondantCode"].ToString();
    //                            objparent.parentquestioncode = dr["ParentQuestionCode"].ToString();
    //                            objparent.questioncode = dr["QuestionCode"].ToString();
    //                            objparent.sectioncode = dr["SectionCode"].ToString();
    //                            objparent.answer = dr["Answer"].ToString();
    //                            objparent.answerremark = dr["AnswerRemark"].ToString();
    //                            string SurveyDate = dr["SurveyDate"].ToString();
    //                            if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
    //                            {
    //                                DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
    //                                objparent.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.surveydate = dr["SurveyDate"].ToString();
    //                            }
    //                            string PlannedDate = dr["PlannedDate"].ToString();
    //                            if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
    //                            {
    //                                DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
    //                                objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.planneddate = dr["PlannedDate"].ToString();
    //                            }
    //                            string CompletionDate = dr["CompletionDate"].ToString();
    //                            if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
    //                            {
    //                                DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
    //                                objparent.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.completiondate = dr["CompletionDate"].ToString();
    //                            }
    //                            objparent.surveydoneby = dr["SurveyDoneBy"].ToString();
    //                            objparent.status = dr["Status"].ToString();
    //                            objparent.isflagged = dr["IsFlagged"].ToString();
    //                            objparent.issynced = dr["IsSynced"].ToString();
    //                            string FlagDate = dr["FlagDate"].ToString();
    //                            if (FlagDate != "" && FlagDate != null && FlagDate != "null")
    //                            {
    //                                DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
    //                                objparent.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.flagdate = dr["FlagDate"].ToString();
    //                            }

    //                            objparent.flagtime = dr["FlagTime"].ToString();
    //                            objparent.flagremark = dr["FlagRemark"].ToString();
    //                            objparent.createdby = dr["CreatedBy"].ToString();

    //                            string CreatedOn = dr["CreatedOn"].ToString();
    //                            if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
    //                            {
    //                                DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
    //                                objparent.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.createdon = dr["CreatedOn"].ToString();
    //                            }

    //                            string SynchedOn = dr["SynchedOn"].ToString();
    //                            if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
    //                            {
    //                                DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
    //                                objparent.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.synchedon = dr["SynchedOn"].ToString();
    //                            }

    //                            string ModifiedOn = dr["ModifiedOn"].ToString();
    //                            if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
    //                            {
    //                                DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
    //                                objparent.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.modifiedon = dr["ModifiedOn"].ToString();
    //                            }

    //                            objparent.modifiedby = dr["ModifiedBy"].ToString();


    //                            objparent.clientid = dr["ClientID"].ToString();
    //                            parentlist.Add(objparent);
    //                        }

    //                        foreach (DataRow drc in dtserveychaildresponce.Rows)
    //                        {
    //                            child objchaild = new child();
    //                            objchaild.surveyresponseid = drc["SurveyResponseID"].ToString();
    //                            objchaild.responserankid = drc["ResponseRankID"].ToString();
    //                            objchaild.surveyworkitemmappingcode = drc["SurveyWorkItemMappingCode"].ToString();
    //                            objchaild.respondantcode = drc["RespondantCode"].ToString();
    //                            objchaild.parentquestioncode = drc["ParentQuestionCode"].ToString();
    //                            objchaild.questioncode = drc["QuestionCode"].ToString();
    //                            objchaild.sectioncode = drc["SectionCode"].ToString();
    //                            objchaild.answer = drc["Answer"].ToString();
    //                            objchaild.answerremark = drc["AnswerRemark"].ToString();
    //                            string SurveyDate = drc["SurveyDate"].ToString();
    //                            if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
    //                            {
    //                                DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
    //                                objchaild.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.surveydate = drc["SurveyDate"].ToString();
    //                            }
    //                            string PlannedDate = drc["PlannedDate"].ToString();
    //                            if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
    //                            {
    //                                DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
    //                                objchaild.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.planneddate = drc["PlannedDate"].ToString();
    //                            }
    //                            string CompletionDate = drc["CompletionDate"].ToString();
    //                            if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
    //                            {
    //                                DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
    //                                objchaild.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.completiondate = drc["CompletionDate"].ToString();
    //                            }
    //                            objchaild.surveydoneby = drc["SurveyDoneBy"].ToString();
    //                            objchaild.status = drc["Status"].ToString();
    //                            objchaild.isflagged = drc["IsFlagged"].ToString();
    //                            objchaild.issynced = drc["IsSynced"].ToString();
    //                            string FlagDate = drc["FlagDate"].ToString();
    //                            if (FlagDate != "" && FlagDate != null && FlagDate != "null")
    //                            {
    //                                DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
    //                                objchaild.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.flagdate = drc["FlagDate"].ToString();
    //                            }
    //                            objchaild.flagtime = drc["FlagTime"].ToString();
    //                            objchaild.flagremark = drc["FlagRemark"].ToString();
    //                            objchaild.createdby = drc["CreatedBy"].ToString();
    //                            string CreatedOn = drc["CreatedOn"].ToString();
    //                            if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
    //                            {
    //                                DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
    //                                objchaild.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.createdon = drc["CreatedOn"].ToString();
    //                            }
    //                            string SynchedOn = drc["SynchedOn"].ToString();
    //                            if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
    //                            {
    //                                DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
    //                                objchaild.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.synchedon = drc["SynchedOn"].ToString();
    //                            }
    //                            string ModifiedOn = drc["ModifiedOn"].ToString();
    //                            if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
    //                            {
    //                                DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
    //                                objchaild.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.modifiedon = drc["ModifiedOn"].ToString();
    //                            }
    //                            objchaild.modifiedby = drc["ModifiedBy"].ToString();
    //                            objchaild.clientid = drc["ClientID"].ToString();
    //                            childlist.Add(objchaild);
    //                        }
    //                    }



    //                    if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
    //                    {
    //                        cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
    //                        cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //                    }
    //                    else
    //                    {
    //                        cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
    //                        cmd.Parameters.Add("@wicode", workitemcode);
    //                        DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
    //                        if (dtworkitemdetails.Rows.Count > 0)
    //                        {
    //                            foreach (DataRow dr in dtworkitemdetails.Rows)
    //                            {
    //                                surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
    //                            }
    //                        }
    //                        cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
    //                        cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //                    }
    //                    DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];
    //                    double submitedcount = 0;
    //                    double savedcount = 0;
    //                    if (dtserveyresponce.Rows.Count > 0)
    //                    {
    //                        foreach (DataRow drsre in dtserveyresponce.Rows)
    //                        {
    //                            string status = drsre["Status"].ToString();
    //                            string count = drsre["count"].ToString();
    //                            if (count != "" || count != null)
    //                            {
    //                                if (status == "2")
    //                                {
    //                                    submitedcount = Convert.ToDouble(count);
    //                                }
    //                                else
    //                                {
    //                                    savedcount = Convert.ToDouble(count);
    //                                }
    //                            }
    //                        }
    //                    }
    //                    status syncst = new status();
    //                    syncst.saved = savedcount.ToString();
    //                    syncst.submitted = submitedcount.ToString();
    //                    syncst.synced = submitedcount.ToString();
    //                    syncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //                    statuslist.Add(syncst);
    //                }
    //                List<statusbenf> getbenfstatusdtls = new List<statusbenf>();
    //                statusbenf getoverDatas = new statusbenf();
    //                getoverDatas.bene_status = surveybenelist;
    //                getoverDatas.parent = parentlist;
    //                getoverDatas.child = childlist;
    //                getoverDatas.status = statuslist;
    //                getbenfstatusdtls.Add(getoverDatas);
    //                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //                jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //                Context.Response.Write(jsonSerializer.Serialize(getbenfstatusdtls));
    //            }
    //        }
    //        else
    //        {
    //            object frequency = new object();
    //            List<frequency> freqItems = new JavaScriptSerializer().ConvertToType<List<frequency>>(frequency);
    //            List<statusbenf> getbenfstatusdtls = new List<statusbenf>();
    //            for (int i = 0; i < freqItems.Count; i++)
    //            {
    //                string surveydate = freqItems[i].surveydate;
    //                surveyworkitemmappingcode = freqItems[i].surveyworkitemmappingcode;


    //            }
    //            string response = GetJson(getbenfstatusdtls);
    //            Context.Response.Write(response);
    //            //JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //           // jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //           // Context.Response.Write(jsonSerializer.Serialize(getbenfstatusdtls));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        string Response = GetJson(ex.Message);
    //        Context.Response.Write(Response);
    //    }
    //}

    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void updatebeneficiary(object updatedata)
    //{
    //    List<updata> lstItems = new JavaScriptSerializer().ConvertToType<List<updata>>(updatedata);
    //    List<statusbenf> getbenfstatusdtls = new List<statusbenf>();
    //    for (int i = 0; i < lstItems.Count; i++)
    //    {
    //        string type = lstItems[i].type;
    //        string surveyworkitemmappingcode = lstItems[i].surveyworkitemmappingcode;
    //        string workitemcode = lstItems[i].workitemcode;
    //        if (type == "BS")
    //        {

    //            if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
    //            {
    //                cmd = new SqlCommand("SELECT DISTINCT SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, Status, SurveyDoneBy, PlannedDate FROM  tbl_TRN_SurveyResponse WHERE  (SurveyWorkItemMappingCode=@swmpcode) ORDER BY SurveyWorkItemMappingCode");
    //                cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
    //                DataTable dtbenestatus = vdm.SelectQuery(cmd).Tables[0];

    //                cmd = new SqlCommand("SELECT SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID FROM  tbl_TRN_SurveyChildResponse where SurveyWorkItemMappingCode=@swimc AND Status=@status ORDER BY SurveyWorkItemMappingCode");
    //                cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //                cmd.Parameters.Add("@status", "1");
    //                DataTable dtserveychaildresponce = vdm.SelectQuery(cmd).Tables[0];


    //                cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID  FROM  tbl_TRN_SurveyResponse  WHERE (Status = '1') AND (SurveyWorkItemMappingCode = @smpcode)");
    //                cmd.Parameters.Add("@smpcode", surveyworkitemmappingcode);
    //                DataTable dtserveyreponcedtls = vdm.SelectQuery(cmd).Tables[0];


    //                List<status> statuslist = new List<status>();
    //                List<parent> parentlist = new List<parent>();
    //                List<child> childlist = new List<child>();
    //                List<bene_status> surveybenelist = new List<bene_status>();
    //                if (dtserveyreponcedtls.Rows.Count > 0)
    //                {
    //                    if (type == "BS")
    //                    {
    //                        foreach (DataRow drb in dtbenestatus.Rows)
    //                        {
    //                            bene_status survey = new bene_status();
    //                            survey.surveyworkitemmappingcode = drb["SurveyWorkItemMappingCode"].ToString();
    //                            survey.respondantcode = drb["RespondantCode"].ToString();
    //                            survey.status = drb["Status"].ToString();
    //                            survey.surveydoneby = drb["SurveyDoneBy"].ToString();
    //                            surveybenelist.Add(survey);
    //                        }
    //                        foreach (DataRow dr in dtserveyreponcedtls.Rows)
    //                        {
    //                            parent objparent = new parent();
    //                            objparent.surveyresponseid = dr["SurveyResponseID"].ToString();
    //                            objparent.surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
    //                            objparent.respondantcode = dr["RespondantCode"].ToString();
    //                            objparent.parentquestioncode = dr["ParentQuestionCode"].ToString();
    //                            objparent.questioncode = dr["QuestionCode"].ToString();
    //                            objparent.sectioncode = dr["SectionCode"].ToString();
    //                            objparent.answer = dr["Answer"].ToString();
    //                            objparent.answerremark = dr["AnswerRemark"].ToString();
    //                            string SurveyDate = dr["SurveyDate"].ToString();
    //                            if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
    //                            {
    //                                DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
    //                                objparent.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.surveydate = dr["SurveyDate"].ToString();
    //                            }
    //                            string PlannedDate = dr["PlannedDate"].ToString();
    //                            if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
    //                            {
    //                                DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
    //                                objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.planneddate = dr["PlannedDate"].ToString();
    //                            }
    //                            string CompletionDate = dr["CompletionDate"].ToString();
    //                            if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
    //                            {
    //                                DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
    //                                objparent.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.completiondate = dr["CompletionDate"].ToString();
    //                            }
    //                            objparent.surveydoneby = dr["SurveyDoneBy"].ToString();
    //                            objparent.status = dr["Status"].ToString();
    //                            objparent.isflagged = dr["IsFlagged"].ToString();
    //                            objparent.issynced = dr["IsSynced"].ToString();
    //                            string FlagDate = dr["FlagDate"].ToString();
    //                            if (FlagDate != "" && FlagDate != null && FlagDate != "null")
    //                            {
    //                                DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
    //                                objparent.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.flagdate = dr["FlagDate"].ToString();
    //                            }

    //                            objparent.flagtime = dr["FlagTime"].ToString();
    //                            objparent.flagremark = dr["FlagRemark"].ToString();
    //                            objparent.createdby = dr["CreatedBy"].ToString();

    //                            string CreatedOn = dr["CreatedOn"].ToString();
    //                            if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
    //                            {
    //                                DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
    //                                objparent.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.createdon = dr["CreatedOn"].ToString();
    //                            }

    //                            string SynchedOn = dr["SynchedOn"].ToString();
    //                            if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
    //                            {
    //                                DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
    //                                objparent.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.synchedon = dr["SynchedOn"].ToString();
    //                            }

    //                            string ModifiedOn = dr["ModifiedOn"].ToString();
    //                            if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
    //                            {
    //                                DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
    //                                objparent.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objparent.modifiedon = dr["ModifiedOn"].ToString();
    //                            }

    //                            objparent.modifiedby = dr["ModifiedBy"].ToString();


    //                            objparent.clientid = dr["ClientID"].ToString();
    //                            parentlist.Add(objparent);
    //                        }

    //                        foreach (DataRow drc in dtserveychaildresponce.Rows)
    //                        {
    //                            child objchaild = new child();
    //                            objchaild.surveyresponseid = drc["SurveyResponseID"].ToString();
    //                            objchaild.responserankid = drc["ResponseRankID"].ToString();
    //                            objchaild.surveyworkitemmappingcode = drc["SurveyWorkItemMappingCode"].ToString();
    //                            objchaild.respondantcode = drc["RespondantCode"].ToString();
    //                            objchaild.parentquestioncode = drc["ParentQuestionCode"].ToString();
    //                            objchaild.questioncode = drc["QuestionCode"].ToString();
    //                            objchaild.sectioncode = drc["SectionCode"].ToString();
    //                            objchaild.answer = drc["Answer"].ToString();
    //                            objchaild.answerremark = drc["AnswerRemark"].ToString();
    //                            string SurveyDate = drc["SurveyDate"].ToString();
    //                            if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
    //                            {
    //                                DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
    //                                objchaild.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.surveydate = drc["SurveyDate"].ToString();
    //                            }
    //                            string PlannedDate = drc["PlannedDate"].ToString();
    //                            if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
    //                            {
    //                                DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
    //                                objchaild.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.planneddate = drc["PlannedDate"].ToString();
    //                            }
    //                            string CompletionDate = drc["CompletionDate"].ToString();
    //                            if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
    //                            {
    //                                DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
    //                                objchaild.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.completiondate = drc["CompletionDate"].ToString();
    //                            }
    //                            objchaild.surveydoneby = drc["SurveyDoneBy"].ToString();
    //                            objchaild.status = drc["Status"].ToString();
    //                            objchaild.isflagged = drc["IsFlagged"].ToString();
    //                            objchaild.issynced = drc["IsSynced"].ToString();
    //                            string FlagDate = drc["FlagDate"].ToString();
    //                            if (FlagDate != "" && FlagDate != null && FlagDate != "null")
    //                            {
    //                                DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
    //                                objchaild.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.flagdate = drc["FlagDate"].ToString();
    //                            }
    //                            objchaild.flagtime = drc["FlagTime"].ToString();
    //                            objchaild.flagremark = drc["FlagRemark"].ToString();
    //                            objchaild.createdby = drc["CreatedBy"].ToString();
    //                            string CreatedOn = drc["CreatedOn"].ToString();
    //                            if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
    //                            {
    //                                DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
    //                                objchaild.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.createdon = drc["CreatedOn"].ToString();
    //                            }
    //                            string SynchedOn = drc["SynchedOn"].ToString();
    //                            if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
    //                            {
    //                                DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
    //                                objchaild.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.synchedon = drc["SynchedOn"].ToString();
    //                            }
    //                            string ModifiedOn = drc["ModifiedOn"].ToString();
    //                            if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
    //                            {
    //                                DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
    //                                objchaild.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                objchaild.modifiedon = drc["ModifiedOn"].ToString();
    //                            }
    //                            objchaild.modifiedby = drc["ModifiedBy"].ToString();
    //                            objchaild.clientid = drc["ClientID"].ToString();
    //                            childlist.Add(objchaild);
    //                        }
    //                    }



    //                    if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
    //                    {
    //                        cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
    //                        cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //                    }
    //                    else
    //                    {
    //                        cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
    //                        cmd.Parameters.Add("@wicode", workitemcode);
    //                        DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
    //                        if (dtworkitemdetails.Rows.Count > 0)
    //                        {
    //                            foreach (DataRow dr in dtworkitemdetails.Rows)
    //                            {
    //                                surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
    //                            }
    //                        }
    //                        cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
    //                        cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //                    }
    //                    DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];
    //                    double submitedcount = 0;
    //                    double savedcount = 0;
    //                    if (dtserveyresponce.Rows.Count > 0)
    //                    {
    //                        foreach (DataRow drsre in dtserveyresponce.Rows)
    //                        {
    //                            string status = drsre["Status"].ToString();
    //                            string count = drsre["count"].ToString();
    //                            if (count != "" || count != null)
    //                            {
    //                                if (status == "2")
    //                                {
    //                                    submitedcount = Convert.ToDouble(count);
    //                                }
    //                                else
    //                                {
    //                                    savedcount = Convert.ToDouble(count);
    //                                }
    //                            }
    //                        }
    //                    }
    //                    status syncst = new status();
    //                    syncst.saved = savedcount.ToString();
    //                    syncst.submitted = submitedcount.ToString();
    //                    syncst.synced = submitedcount.ToString();
    //                    syncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //                    statuslist.Add(syncst);
    //                }

    //                statusbenf getoverDatas = new statusbenf();
    //                getoverDatas.bene_status = surveybenelist;
    //                getoverDatas.parent = parentlist;
    //                getoverDatas.child = childlist;
    //                getoverDatas.status = statuslist;
    //                getbenfstatusdtls.Add(getoverDatas);
    //            }
    //        }
    //        else
    //        {
    //            string status = lstItems[i].frequency.ContainsKey("surveyworkitemmappingcode").ToString();
    //            string surveydate = "";
    //            lstItems[i].frequency.TryGetValue("surveyworkitemmappingcode", out surveyworkitemmappingcode).ToString();
    //            lstItems[i].frequency.TryGetValue("surveydate", out surveydate).ToString();
    //            DateTime DTSERVEYDATE = Convert.ToDateTime(surveydate);
    //            // surveydate = "2019-04-15";
    //            // surveyworkitemmappingcode = "727";
    //            cmd = new SqlCommand("SELECT DISTINCT SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, Status, SurveyDoneBy, PlannedDate FROM  tbl_TRN_SurveyResponse WHERE  (SurveyWorkItemMappingCode=@swmpcode) AND (PlannedDate BETWEEN @d1 and @d2) ORDER BY SurveyWorkItemMappingCode");
    //            cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
    //            cmd.Parameters.Add("@d1", GetLowDate(DTSERVEYDATE));
    //            cmd.Parameters.Add("@d2", GetHighDate(DTSERVEYDATE));
    //            DataTable dtbenestatus = vdm.SelectQuery(cmd).Tables[0];

    //            cmd = new SqlCommand("SELECT SurveyResponseID, ResponseRankID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID FROM  tbl_TRN_SurveyChildResponse where SurveyWorkItemMappingCode=@swimc AND (Status=@status) AND (PlannedDate BETWEEN @dc1 and @dc2) ORDER BY SurveyWorkItemMappingCode");
    //            cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //            cmd.Parameters.Add("@status", "1");

    //            cmd.Parameters.Add("@dc1", GetLowDate(DTSERVEYDATE));
    //            cmd.Parameters.Add("@dc2", GetHighDate(DTSERVEYDATE));

    //            // cmd.Parameters.Add("@pcdate", surveydate);
    //            DataTable dtserveychaildresponce = vdm.SelectQuery(cmd).Tables[0];
    //            cmd = new SqlCommand("SELECT  SurveyResponseID, SurveyWorkItemMappingCode, RespondantCode, ParentQuestionCode, QuestionCode, SectionCode, Answer, AnswerRemark, SurveyDate, PlannedDate, CompletionDate, SurveyDoneBy, Status, IsFlagged, IsSynced, FlagDate, FlagTime, FlagRemark, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, SynchedOn, ClientID  FROM  tbl_TRN_SurveyResponse  WHERE (Status = '1') AND (SurveyWorkItemMappingCode = @smpcode) AND (PlannedDate between @dp1 and @dp2)");
    //            cmd.Parameters.Add("@smpcode", surveyworkitemmappingcode);
    //            // cmd.Parameters.Add("@pPdate", surveydate);
    //            cmd.Parameters.Add("@dp1", GetLowDate(DTSERVEYDATE));
    //            cmd.Parameters.Add("@dp2", GetHighDate(DTSERVEYDATE));
    //            DataTable dtserveyreponcedtls = vdm.SelectQuery(cmd).Tables[0];
    //            List<status> statuslist = new List<status>();
    //            List<parent> parentlist = new List<parent>();
    //            List<child> childlist = new List<child>();
    //            List<bene_status> surveybenelist = new List<bene_status>();
    //            if (dtbenestatus.Rows.Count > 0)
    //            {
    //                foreach (DataRow drb in dtbenestatus.Rows)
    //                {
    //                    bene_status survey = new bene_status();
    //                    survey.surveyworkitemmappingcode = drb["SurveyWorkItemMappingCode"].ToString();
    //                    survey.respondantcode = drb["RespondantCode"].ToString();
    //                    survey.status = drb["Status"].ToString();
    //                    survey.surveydoneby = drb["SurveyDoneBy"].ToString();
    //                    surveybenelist.Add(survey);
    //                }
    //                foreach (DataRow dr in dtserveyreponcedtls.Rows)
    //                {
    //                    parent objparent = new parent();
    //                    objparent.surveyresponseid = dr["SurveyResponseID"].ToString();
    //                    objparent.surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
    //                    objparent.respondantcode = dr["RespondantCode"].ToString();
    //                    objparent.parentquestioncode = dr["ParentQuestionCode"].ToString();
    //                    objparent.questioncode = dr["QuestionCode"].ToString();
    //                    objparent.sectioncode = dr["SectionCode"].ToString();
    //                    objparent.answer = dr["Answer"].ToString();
    //                    objparent.answerremark = dr["AnswerRemark"].ToString();
    //                    string SurveyDate = dr["SurveyDate"].ToString();
    //                    if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
    //                    {
    //                        DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
    //                        objparent.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objparent.surveydate = dr["SurveyDate"].ToString();
    //                    }
    //                    string PlannedDate = dr["PlannedDate"].ToString();
    //                    if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
    //                    {
    //                        DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
    //                        objparent.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objparent.planneddate = dr["PlannedDate"].ToString();
    //                    }
    //                    string CompletionDate = dr["CompletionDate"].ToString();
    //                    if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
    //                    {
    //                        DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
    //                        objparent.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objparent.completiondate = dr["CompletionDate"].ToString();
    //                    }
    //                    objparent.surveydoneby = dr["SurveyDoneBy"].ToString();
    //                    objparent.status = dr["Status"].ToString();
    //                    objparent.isflagged = dr["IsFlagged"].ToString();
    //                    objparent.issynced = dr["IsSynced"].ToString();
    //                    string FlagDate = dr["FlagDate"].ToString();
    //                    if (FlagDate != "" && FlagDate != null && FlagDate != "null")
    //                    {
    //                        DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
    //                        objparent.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objparent.flagdate = dr["FlagDate"].ToString();
    //                    }

    //                    objparent.flagtime = dr["FlagTime"].ToString();
    //                    objparent.flagremark = dr["FlagRemark"].ToString();
    //                    objparent.createdby = dr["CreatedBy"].ToString();

    //                    string CreatedOn = dr["CreatedOn"].ToString();
    //                    if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
    //                    {
    //                        DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
    //                        objparent.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objparent.createdon = dr["CreatedOn"].ToString();
    //                    }

    //                    string SynchedOn = dr["SynchedOn"].ToString();
    //                    if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
    //                    {
    //                        DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
    //                        objparent.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objparent.synchedon = dr["SynchedOn"].ToString();
    //                    }

    //                    string ModifiedOn = dr["ModifiedOn"].ToString();
    //                    if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
    //                    {
    //                        DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
    //                        objparent.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objparent.modifiedon = dr["ModifiedOn"].ToString();
    //                    }

    //                    objparent.modifiedby = dr["ModifiedBy"].ToString();


    //                    objparent.clientid = dr["ClientID"].ToString();
    //                    parentlist.Add(objparent);
    //                }

    //                foreach (DataRow drc in dtserveychaildresponce.Rows)
    //                {
    //                    child objchaild = new child();
    //                    objchaild.surveyresponseid = drc["SurveyResponseID"].ToString();
    //                    objchaild.responserankid = drc["ResponseRankID"].ToString();
    //                    objchaild.surveyworkitemmappingcode = drc["SurveyWorkItemMappingCode"].ToString();
    //                    objchaild.respondantcode = drc["RespondantCode"].ToString();
    //                    objchaild.parentquestioncode = drc["ParentQuestionCode"].ToString();
    //                    objchaild.questioncode = drc["QuestionCode"].ToString();
    //                    objchaild.sectioncode = drc["SectionCode"].ToString();
    //                    objchaild.answer = drc["Answer"].ToString();
    //                    objchaild.answerremark = drc["AnswerRemark"].ToString();
    //                    string SurveyDate = drc["SurveyDate"].ToString();
    //                    if (SurveyDate != "" && SurveyDate != null && SurveyDate != "null")
    //                    {
    //                        DateTime dtDateOfBirth = Convert.ToDateTime(SurveyDate);
    //                        objchaild.surveydate = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objchaild.surveydate = drc["SurveyDate"].ToString();
    //                    }
    //                    string PlannedDate = drc["PlannedDate"].ToString();
    //                    if (PlannedDate != "" && PlannedDate != null && PlannedDate != "null")
    //                    {
    //                        DateTime dtPlannedDate = Convert.ToDateTime(PlannedDate);
    //                        objchaild.planneddate = dtPlannedDate.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objchaild.planneddate = drc["PlannedDate"].ToString();
    //                    }
    //                    string CompletionDate = drc["CompletionDate"].ToString();
    //                    if (CompletionDate != "" && CompletionDate != null && CompletionDate != "null")
    //                    {
    //                        DateTime dtCompletionDate = Convert.ToDateTime(CompletionDate);
    //                        objchaild.completiondate = dtCompletionDate.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objchaild.completiondate = drc["CompletionDate"].ToString();
    //                    }
    //                    objchaild.surveydoneby = drc["SurveyDoneBy"].ToString();
    //                    objchaild.status = drc["Status"].ToString();
    //                    objchaild.isflagged = drc["IsFlagged"].ToString();
    //                    objchaild.issynced = drc["IsSynced"].ToString();
    //                    string FlagDate = drc["FlagDate"].ToString();
    //                    if (FlagDate != "" && FlagDate != null && FlagDate != "null")
    //                    {
    //                        DateTime dtFlagDate = Convert.ToDateTime(FlagDate);
    //                        objchaild.flagdate = dtFlagDate.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objchaild.flagdate = drc["FlagDate"].ToString();
    //                    }
    //                    objchaild.flagtime = drc["FlagTime"].ToString();
    //                    objchaild.flagremark = drc["FlagRemark"].ToString();
    //                    objchaild.createdby = drc["CreatedBy"].ToString();
    //                    string CreatedOn = drc["CreatedOn"].ToString();
    //                    if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
    //                    {
    //                        DateTime dtCreatedOn = Convert.ToDateTime(CreatedOn);
    //                        objchaild.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objchaild.createdon = drc["CreatedOn"].ToString();
    //                    }
    //                    string SynchedOn = drc["SynchedOn"].ToString();
    //                    if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
    //                    {
    //                        DateTime dtSynchedOn = Convert.ToDateTime(SynchedOn);
    //                        objchaild.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objchaild.synchedon = drc["SynchedOn"].ToString();
    //                    }
    //                    string ModifiedOn = drc["ModifiedOn"].ToString();
    //                    if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
    //                    {
    //                        DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
    //                        objchaild.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
    //                    }
    //                    else
    //                    {
    //                        objchaild.modifiedon = drc["ModifiedOn"].ToString();
    //                    }
    //                    objchaild.modifiedby = drc["ModifiedBy"].ToString();
    //                    objchaild.clientid = drc["ClientID"].ToString();
    //                    childlist.Add(objchaild);
    //                }
    //            }
    //            if (surveyworkitemmappingcode != "" && surveyworkitemmappingcode != null && surveyworkitemmappingcode != "null")
    //            {
    //                cmd = new SqlCommand("SELECT COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) AND (PlannedDate between @p1 and @p2) GROUP BY Status");
    //                cmd.Parameters.Add("@swimc", surveyworkitemmappingcode);
    //                cmd.Parameters.Add("@p1", GetLowDate(DTSERVEYDATE));
    //                cmd.Parameters.Add("@p2", GetHighDate(DTSERVEYDATE));
    //            }
    //            DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];
    //            double submitedcount = 0;
    //            double savedcount = 0;
    //            if (dtserveyresponce.Rows.Count > 0)
    //            {
    //                foreach (DataRow drsre in dtserveyresponce.Rows)
    //                {
    //                    string statuss = drsre["Status"].ToString();
    //                    string count = drsre["count"].ToString();
    //                    if (count != "" || count != null)
    //                    {
    //                        if (statuss == "2")
    //                        {
    //                            submitedcount = Convert.ToDouble(count);
    //                        }
    //                        else
    //                        {
    //                            savedcount = Convert.ToDouble(count);
    //                        }
    //                    }
    //                }
    //            }
    //            status syncst = new status();
    //            syncst.saved = savedcount.ToString();
    //            syncst.submitted = submitedcount.ToString();
    //            syncst.synced = submitedcount.ToString();
    //            syncst.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
    //            statuslist.Add(syncst);
    //            statusbenf getoverDatas = new statusbenf();
    //            getoverDatas.bene_status = surveybenelist;
    //            getoverDatas.parent = parentlist;
    //            getoverDatas.child = childlist;
    //            getoverDatas.status = statuslist;
    //            getbenfstatusdtls.Add(getoverDatas);
    //        }

    //    }
    //    string response = GetJson(getbenfstatusdtls);
    //    Context.Response.Write(response);
    //}

    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void responceda(string empid)
    //{
    //    string msg = "not calling";
    //    JavaScriptSerializer jsonSerializerS = new JavaScriptSerializer();
    //    jsonSerializerS.MaxJsonLength = Int32.MaxValue;
    //    string responce = jsonSerializerS.Serialize(msg);
    //    Context.Response.Clear();
    //    Context.Response.ContentType = "application/json";
    //    Context.Response.AddHeader("content-length", responce.Length.ToString());
    //    Context.Response.Flush();
    //    Context.Response.Write(responce);
    //    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //}

    private static string GetJson(object obj)
    {
        JavaScriptSerializer jsonSerializerS= new JavaScriptSerializer();
        jsonSerializerS.MaxJsonLength = Int32.MaxValue; 
        return jsonSerializerS.Serialize(obj);
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

    public class updata
    {
        public string workitemcode { get; set; }//1
        public string surveyworkitemmappingcode { get; set; }//2
        public string type { get; set; }//2
        public Dictionary<string, string> frequency { get; set; }
    }


    public class frequency
    {
        public string surveyworkitemmappingcode { get; set; }//1
        public string surveydate { get; set; }//2
    }

    public class status
    {
        public string target { get; set; }//1
        public string open { get; set; }//1
        public string saved { get; set; }//1
        public string submitted { get; set; }//2
        public string synced { get; set; }//2
        public string lastsynced { get; set; }//2
        public string fromdate { get; set; }
    }

    public class empcountstatus
    {
        public string target { get; set; }//1
        public string open { get; set; }//1
        public string saved { get; set; }//1
        public string submitted { get; set; }//2
        public string synced { get; set; }//2
        public string lastsynced { get; set; }//2
        public string fromdate { get; set; }
    }


    public class parent
    {
        public string surveyresponseid { get; set; }
        public string surveyworkitemmappingcode { get; set; }
        public string respondantcode { get; set; }
        public string parentquestioncode { get; set; }
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
        public string synchedon { get; set; }
        public string clientid { get; set; }
        public string UploadedFrom { get; set; }
    }

    public class child
    {
        public string surveyresponseid { get; set; }
        public string responserankid { get; set; }
        public string surveyworkitemmappingcode { get; set; }
        public string respondantcode { get; set; }
        public string parentquestioncode { get; set; }
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
        public string synchedon { get; set; }
        public string clientid { get; set; }
        public string UploadedFrom { get; set; }
    }

    public class bene_status
    {
        public string surveyworkitemmappingcode { get; set; }
        public string respondantcode { get; set; }
        public string status { get; set; }
        public string surveydoneby { get; set; }
        public string empname { get; set; }
        public string surveydate { get; set; }
        public string UploadedFrom { get; set; }
    }
   
    public class statusbenf  //new
    {
        public string surveyworkitemmappingcode { get; set; }
        public string workitemcode { get; set; }
        public string surveycode { get; set; }
        public string type { get; set; }
        public List<bene_status> bene_status { get; set; }
        public List<parent> parent { get; set; }
        public List<child> child { get; set; }
        public List<status> status { get; set; }
        public List<empcountstatus> empstatus { get; set; }
    }

   
}
