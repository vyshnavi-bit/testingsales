using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using Npgsql;
using System.Text.RegularExpressions;
using System.Net;

/// <summary>
/// Summary description for downloadservice
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class downloadservice : System.Web.Services.WebService {
    // employe wise work item details loading
    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();
    NpgsqlCommand postcmd;
    SAPdbmanger postvdm = new SAPdbmanger();
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
   public void empworkitemdetails(string empid, string lastsynced)
    {
        try
        {
            //added by naveeen 
            string token = System.Web.HttpContext.Current.Request.Headers["token"];
            string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
            string uuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
            //end
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            string empcode = empid;
            int i = 0;
            cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM   tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
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

                        DataTable dtresourceallocation = new DataTable();
                        if (lastsynced != "" && lastsynced != "null" && lastsynced != null)
                        {
                            DateTime DTLSY = Convert.ToDateTime(lastsynced);
                            cmd = new SqlCommand("SELECT tbl_TRN_ResourceAllocation.AssignationID, tbl_TRN_ResourceAllocation.EmployeeCode, tbl_TRN_ResourceAllocation.WorkItemCode, tbl_TRN_ResourceAllocation.AllocationDate,  tbl_TRN_ResourceAllocation.PercentageAllocation, tbl_TRN_ResourceAllocation.Remarks, tbl_TRN_ResourceAllocation.LocationCode, tbl_TRN_ResourceAllocation.CompletionTarget, tbl_TRN_ResourceAllocation.CreatedBy,  tbl_TRN_ResourceAllocation.CreatedOn, tbl_TRN_ResourceAllocation.IsPrimary, tbl_TRN_ResourceAllocation.ModifiedBy, tbl_TRN_ResourceAllocation.ModifiedOn, tbl_TRN_ResourceAllocation.AllocatedFrom,   tbl_TRN_ResourceAllocation.AllocatedTo, tbl_TRN_ResourceAllocation.RoleCode, tbl_TRN_ResourceAllocation.Target, tbl_TRN_ResourceAllocation.WorkHour   FROM            tbl_TRN_ResourceAllocation INNER JOIN  tbl_TRN_WorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_TRN_ResourceAllocation.WorkItemCode WHERE (tbl_TRN_ResourceAllocation.EmployeeCode = @EmployeeCode) AND (tbl_TRN_WorkItem.PlanStartDate >= @D1)");
                            cmd.Parameters.Add("@EmployeeCode", empcode);
                            cmd.Parameters.Add("@D1", Convert.ToDateTime(lastsynced));
                            dtresourceallocation = vdm.SelectQuery(cmd).Tables[0];
                        }
                        else
                        {
                            cmd = new SqlCommand("SELECT AssignationID, EmployeeCode, WorkItemCode, AllocationDate, PercentageAllocation, Remarks, LocationCode, CompletionTarget, CreatedBy, CreatedOn, IsPrimary, ModifiedBy, ModifiedOn, AllocatedFrom, AllocatedTo, RoleCode, Target, WorkHour FROM  tbl_TRN_ResourceAllocation WHERE EmployeeCode=@EmployeeCode");
                            cmd.Parameters.Add("@EmployeeCode", empcode);
                            dtresourceallocation = vdm.SelectQuery(cmd).Tables[0];
                        }

                        cmd = new SqlCommand("SELECT tbl_MST_Employee.EmployeeName AS Primaryname, tbl_MST_Employee_1.EmployeeName AS CreatedName, tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode LEFT OUTER JOIN  tbl_MST_Employee ON tbl_TRN_WorkItem.PrimaryOwner = tbl_MST_Employee.EmployeeCode LEFT OUTER JOIN  tbl_MST_Employee AS tbl_MST_Employee_1 ON tbl_TRN_WorkItem.CreatedBy = tbl_MST_Employee_1.EmployeeCode WHERE tbl_TRN_WorkItem.Status <> '28' AND (tbl_TRN_WorkItem.Status <> '30')"); //tbl_MMP_SurveyWorkItem.isActive='true '
                        DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];

                        cmd = new SqlCommand("SELECT tbl_MST_Employee.EmployeeName AS Primaryname, tbl_MST_Employee_1.EmployeeName AS CreatedName, tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode LEFT OUTER JOIN  tbl_MST_Employee ON tbl_TRN_WorkItem.PrimaryOwner = tbl_MST_Employee.EmployeeCode LEFT OUTER JOIN  tbl_MST_Employee AS tbl_MST_Employee_1 ON tbl_TRN_WorkItem.CreatedBy = tbl_MST_Employee_1.EmployeeCode WHERE tbl_TRN_WorkItem.Status = '30'"); //tbl_MMP_SurveyWorkItem.isActive='False'
                        DataTable dtclosedworkitemdetails = vdm.SelectQuery(cmd).Tables[0];

                        // cmd = new SqlCommand("SELECT  SurveyCode, SurveyName, SurveyDesc, SectorCode, QuestionSetCode, Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Type, Frequency, StartDate, EndDate, ProgramCode, InterventionCode FROM tbl_MST_Survey");
                        cmd = new SqlCommand("SELECT tbl_MST_Survey.SurveyCode, tbl_MST_Survey.SurveyName, tbl_MST_Survey.SurveyDesc, tbl_MST_Survey.SectorCode, tbl_MST_Survey.QuestionSetCode, tbl_MST_Survey.Status, tbl_MST_Survey.CreatedBy,  tbl_MST_Survey.CreatedOn, tbl_MST_Survey.ModifiedBy, tbl_MST_Survey.ModifiedOn, tbl_MST_Survey.Type, tbl_MST_Survey.Frequency, tbl_MST_Survey.StartDate, tbl_MST_Survey.EndDate, tbl_MST_Survey.ProgramCode,   tbl_MST_Survey.InterventionCode, tbl_MST_Sector.SectorName, tbl_MST_Program.ProgramName, tbl_MST_Intervention.InterventionName FROM            tbl_MST_Survey LEFT OUTER JOIN  tbl_MST_Sector ON tbl_MST_Sector.SectorCode = tbl_MST_Survey.SectorCode LEFT OUTER JOIN tbl_MST_Program ON tbl_MST_Program.ProgramCode = tbl_MST_Survey.ProgramCode LEFT OUTER JOIN  tbl_MST_Intervention ON tbl_MST_Intervention.InterventionCode = tbl_MST_Survey.InterventionCode");
                        DataTable dtsurveydtls = vdm.SelectQuery(cmd).Tables[0];


                        cmd = new SqlCommand("SELECT tbl_MST_AppConfiguration.ConfigurationCode, tbl_MST_AppConfiguration.ConfigurationValue FROM   tbl_MMP_SurveyWorkItem INNER JOIN   tbl_MST_AppConfiguration ON tbl_MMP_SurveyWorkItem.Frequncy = tbl_MST_AppConfiguration.ConfigurationCode WHERE        (tbl_MST_AppConfiguration.ConfigurationType = 'SurveyFrequency')");
                        DataTable dtappconfig = vdm.SelectQuery(cmd).Tables[0];


                        cmd = new SqlCommand("SELECT  Tbl_MST_Question.QuestionText, Tbl_MST_Question.OptionID, Tbl_MST_Question.ValidationTypeCode, tbl_TRN_QuestionSet.SectionDisplayOrder, tbl_TRN_QuestionSet.DisplayOrder AS questiondisplayorder, (SELECT  SectionName FROM            tbl_MST_QuestionSection  WHERE        (SectionCode = Tbl_MST_Question.SectionCode)) AS sectionname, Tbl_MST_Question.ValidationTypeCode AS Expr1, Tbl_MST_Question.QuestionTypeCode, tbl_TRN_QuestionSet.QuestionType,   tbl_TRN_QuestionSet.HasChildQuestion, tbl_TRN_QuestionSet.QuestionCode, tbl_TRN_QuestionSet.ParentQuestion, tbl_TRN_QuestionSet.ConditionValue, tbl_TRN_QuestionSet.ControlType,   Tbl_MST_Question.QuestionLevel, Tbl_MST_Question.HasOptions, Tbl_MST_Question.OptionID AS Expr2, Tbl_MST_Question.ChildQuestionConditionID, Tbl_MST_Question.HelpText,  Tbl_MST_Question.FrequencyOfQuestions, Tbl_MST_Question.NumberOfTimes, Tbl_MST_Question.FrequencyStartDate, Tbl_MST_Question.SectorCode, Tbl_MST_Question.ProgramCode,  Tbl_MST_Question.InterventionCode, Tbl_MST_Question.CategoryId, Tbl_MST_Question.SuggestiveText, tbl_TRN_QuestionSet.QuestionSetCode FROM tbl_TRN_QuestionSet INNER JOIN Tbl_MST_Question ON tbl_TRN_QuestionSet.QuestionCode = Tbl_MST_Question.QuestionCode ORDER BY tbl_TRN_QuestionSet.SectionDisplayOrder, questiondisplayorder");
                        DataTable dtquestionsdata = vdm.SelectQuery(cmd).Tables[0];

                        cmd = new SqlCommand("SELECT validationtypecode, questioncode,validationtypename, allowedvalues,violationmessage, controldescription FROM tbl_mst_validationtype");
                        DataTable dtvalidations = vdm.SelectQuery(cmd).Tables[0];

                        cmd = new SqlCommand("SELECT questioncode,displaytext,displayvalue,displayorder FROM tbl_trn_questionoptions");
                        DataTable dtquestionoption = vdm.SelectQuery(cmd).Tables[0];


                        DataTable workitemdetails = new DataTable();
                        DataTable locations = new DataTable();
                        DataTable surveys = new DataTable();
                        DataTable questionset = new DataTable();
                        DataTable contrlvalidations = new DataTable();
                        DataTable questionoptions = new DataTable();

                        workitemdetails.Columns.Add("WorkItemCode");
                        workitemdetails.Columns.Add("MileStoneCode");
                        workitemdetails.Columns.Add("WorkItemName");
                        workitemdetails.Columns.Add("WorkItemDesc");
                        workitemdetails.Columns.Add("WorkItemType");
                        workitemdetails.Columns.Add("ParentWorkItemCode");
                        workitemdetails.Columns.Add("PlanStartDate");
                        workitemdetails.Columns.Add("PlanEndDate");
                        workitemdetails.Columns.Add("PlanBudget");
                        workitemdetails.Columns.Add("NonActivityBudget");
                        workitemdetails.Columns.Add("NonActivityBudgetPercentage");
                        workitemdetails.Columns.Add("ActualStartDate");
                        workitemdetails.Columns.Add("ActualEndDate");
                        workitemdetails.Columns.Add("ActualExpenses");
                        workitemdetails.Columns.Add("PrimaryOwner");
                        workitemdetails.Columns.Add("LocationCode");
                        workitemdetails.Columns.Add("Target");
                        workitemdetails.Columns.Add("TargetMeasurementUnit");
                        workitemdetails.Columns.Add("Achievement");
                        workitemdetails.Columns.Add("PercentageCompleted");
                        workitemdetails.Columns.Add("Remarks");
                        workitemdetails.Columns.Add("Status");
                        workitemdetails.Columns.Add("CreatedBy");
                        workitemdetails.Columns.Add("CreatedOn");
                        workitemdetails.Columns.Add("ModifiedBy");
                        workitemdetails.Columns.Add("ModifiedOn");
                        workitemdetails.Columns.Add("DisplayOrder");
                        workitemdetails.Columns.Add("SynchedOn");
                        workitemdetails.Columns.Add("SurveyWorkItemMappingCode");
                        workitemdetails.Columns.Add("SurveyCode");
                        workitemdetails.Columns.Add("isActive");
                        workitemdetails.Columns.Add("Frequncy");
                        workitemdetails.Columns.Add("StartDate");
                        workitemdetails.Columns.Add("EndDDate");
                        workitemdetails.Columns.Add("Primaryname");
                        workitemdetails.Columns.Add("CreatedName");
                        workitemdetails.Columns.Add("Frequncytype");

                        locations.Columns.Add("WorkItemCode");
                        locations.Columns.Add("locationcode");
                        locations.Columns.Add("locationname");
                        locations.Columns.Add("locationtype");

                        surveys.Columns.Add("Workitemcode");
                        surveys.Columns.Add("SurveyCode");
                        surveys.Columns.Add("SurveyName");
                        surveys.Columns.Add("SurveyDesc");
                        surveys.Columns.Add("SectorCode");
                        surveys.Columns.Add("QuestionSetCode");
                        surveys.Columns.Add("Status");
                        surveys.Columns.Add("CreatedBy");
                        surveys.Columns.Add("CreatedOn");
                        surveys.Columns.Add("ModifiedBy");
                        surveys.Columns.Add("ModifiedOn");
                        surveys.Columns.Add("Type");
                        surveys.Columns.Add("Frequency");
                        surveys.Columns.Add("StartDate");
                        surveys.Columns.Add("EndDate");
                        surveys.Columns.Add("ProgramCode");
                        surveys.Columns.Add("InterventionCode");



                        questionset.Columns.Add("Workitemcode");
                        questionset.Columns.Add("QuestionText");
                        questionset.Columns.Add("SectionDisplayOrder");
                        questionset.Columns.Add("questiondisplayorder");
                        questionset.Columns.Add("sectionname");
                        questionset.Columns.Add("ValidationTypeCode");
                        questionset.Columns.Add("QuestionTypeCode");
                        questionset.Columns.Add("QuestionType");
                        questionset.Columns.Add("HasChildQuestion");
                        questionset.Columns.Add("QuestionCode");
                        questionset.Columns.Add("ParentQuestion");
                        questionset.Columns.Add("ConditionValue");
                        questionset.Columns.Add("ControlType");
                        questionset.Columns.Add("QuestionLevel");
                        questionset.Columns.Add("HasOptions");
                        questionset.Columns.Add("OptionID");
                        questionset.Columns.Add("ChildQuestionConditionID");
                        questionset.Columns.Add("HelpText");
                        questionset.Columns.Add("FrequencyOfQuestions");
                        questionset.Columns.Add("NumberOfTimes");
                        questionset.Columns.Add("FrequencyStartDate");
                        questionset.Columns.Add("SectorCode");
                        questionset.Columns.Add("ProgramCode");
                        questionset.Columns.Add("InterventionCode");
                        questionset.Columns.Add("CategoryId");
                        questionset.Columns.Add("SuggestiveText");




                        contrlvalidations.Columns.Add("WorkItemCode");
                        contrlvalidations.Columns.Add("validationtypecode");
                        contrlvalidations.Columns.Add("questioncode");
                        contrlvalidations.Columns.Add("validationtypename");
                        contrlvalidations.Columns.Add("allowedvalues");
                        contrlvalidations.Columns.Add("violationmessage");
                        contrlvalidations.Columns.Add("controldescription");

                        questionoptions.Columns.Add("WorkItemCode");
                        questionoptions.Columns.Add("questioncode");
                        questionoptions.Columns.Add("displaytext");
                        questionoptions.Columns.Add("displayvalue");
                        questionoptions.Columns.Add("displayorder");

                        if (dtresourceallocation.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtresourceallocation.Rows)
                            {
                                string workitemcode = dr["WorkItemCode"].ToString();
                                foreach (DataRow drw in dtworkitemdetails.Select("WorkItemCode='" + workitemcode + "'"))
                                {
                                    string status = drw["Status"].ToString();
                                    if (status != "28")
                                    {
                                        DataRow newrow = workitemdetails.NewRow();
                                        newrow["WorkItemCode"] = drw["WorkItemCode"].ToString();
                                        newrow["MileStoneCode"] = drw["MileStoneCode"].ToString();
                                        newrow["WorkItemName"] = drw["WorkItemName"].ToString();
                                        newrow["WorkItemDesc"] = drw["WorkItemDesc"].ToString();
                                        newrow["WorkItemType"] = drw["WorkItemType"].ToString();
                                        newrow["ParentWorkItemCode"] = drw["ParentWorkItemCode"].ToString();
                                        newrow["PlanStartDate"] = drw["PlanStartDate"].ToString();
                                        newrow["PlanEndDate"] = drw["PlanEndDate"].ToString();
                                        newrow["PlanBudget"] = drw["PlanBudget"].ToString();
                                        newrow["NonActivityBudget"] = drw["NonActivityBudget"].ToString();
                                        newrow["NonActivityBudgetPercentage"] = drw["NonActivityBudgetPercentage"].ToString();
                                        newrow["ActualStartDate"] = drw["ActualStartDate"].ToString();
                                        newrow["ActualEndDate"] = drw["ActualEndDate"].ToString();
                                        newrow["ActualExpenses"] = drw["ActualExpenses"].ToString();
                                        newrow["PrimaryOwner"] = drw["PrimaryOwner"].ToString();
                                        newrow["LocationCode"] = drw["LocationCode"].ToString();
                                        newrow["Target"] = drw["Target"].ToString();
                                        newrow["TargetMeasurementUnit"] = drw["TargetMeasurementUnit"].ToString();
                                        newrow["Achievement"] = drw["Achievement"].ToString();
                                        newrow["PercentageCompleted"] = drw["PercentageCompleted"].ToString();
                                        newrow["Remarks"] = drw["Remarks"].ToString();
                                        newrow["Status"] = drw["Status"].ToString();
                                        newrow["CreatedBy"] = drw["CreatedBy"].ToString();
                                        newrow["CreatedOn"] = drw["CreatedOn"].ToString();
                                        newrow["ModifiedBy"] = drw["ModifiedBy"].ToString();
                                        newrow["ModifiedOn"] = drw["ModifiedOn"].ToString();
                                        newrow["DisplayOrder"] = drw["DisplayOrder"].ToString();
                                        newrow["SynchedOn"] = drw["SynchedOn"].ToString();
                                        newrow["SurveyWorkItemMappingCode"] = drw["SurveyWorkItemMappingCode"].ToString();
                                        newrow["SurveyCode"] = drw["SurveyCode"].ToString();
                                        newrow["isActive"] = drw["isActive"].ToString();
                                        newrow["Frequncy"] = drw["Frequncy"].ToString();
                                        newrow["StartDate"] = drw["StartDate"].ToString();
                                        newrow["EndDDate"] = drw["EndDDate"].ToString();
                                        newrow["CreatedName"] = drw["CreatedName"].ToString();
                                        newrow["Primaryname"] = drw["Primaryname"].ToString();
                                        string frequency = drw["Frequncy"].ToString();
                                        if (frequency != "")
                                        {
                                            foreach (DataRow dras in dtappconfig.Select("ConfigurationCode='" + frequency + "'"))
                                            {
                                                newrow["Frequncytype"] = dras["ConfigurationValue"].ToString();
                                            }
                                        }
                                        workitemdetails.Rows.Add(newrow);


                                        string SurveyCode = drw["SurveyCode"].ToString();
                                        foreach (DataRow drs in dtsurveydtls.Select("SurveyCode='" + SurveyCode + "'"))
                                        {
                                            DataRow newrows = surveys.NewRow();
                                            newrows["Workitemcode"] = workitemcode;
                                            newrows["SurveyCode"] = drs["SurveyCode"].ToString();
                                            newrows["SurveyName"] = drs["SurveyName"].ToString();
                                            newrows["SurveyDesc"] = drs["SurveyDesc"].ToString();
                                            newrows["SectorCode"] = drs["SectorName"].ToString();
                                            newrows["QuestionSetCode"] = drs["QuestionSetCode"].ToString();
                                            newrows["Status"] = drs["Status"].ToString();
                                            newrows["CreatedBy"] = drs["CreatedBy"].ToString();
                                            newrows["CreatedOn"] = drs["CreatedOn"].ToString();
                                            newrows["ModifiedBy"] = drs["ModifiedBy"].ToString();
                                            newrows["ModifiedOn"] = drs["ModifiedOn"].ToString();
                                            newrows["Type"] = drs["Type"].ToString();
                                            newrows["Frequency"] = drs["Frequency"].ToString();
                                            newrows["StartDate"] = drs["StartDate"].ToString();
                                            newrows["EndDate"] = drs["EndDate"].ToString();
                                            newrows["ProgramCode"] = drs["ProgramName"].ToString();
                                            newrows["InterventionCode"] = drs["InterventionName"].ToString();

                                            //newrows["SectorCode"] = drs["SectorName"].ToString();
                                            //newrows["InterventionCode"] = drs["InterventionName"].ToString();
                                            //newrows["ProgramCode"] = drs["ProgramName"].ToString();

                                            surveys.Rows.Add(newrows);

                                            string qcode = drs["QuestionSetCode"].ToString();
                                            foreach (DataRow drq in dtquestionsdata.Select("QuestionSetCode='" + qcode + "'"))
                                            {
                                                DataRow newrowqst = questionset.NewRow();
                                                newrowqst["Workitemcode"] = workitemcode;
                                                newrowqst["QuestionText"] = drq["QuestionText"].ToString();
                                                newrowqst["SectionDisplayOrder"] = drq["SectionDisplayOrder"].ToString();
                                                newrowqst["questiondisplayorder"] = drq["questiondisplayorder"].ToString();
                                                newrowqst["sectionname"] = drq["sectionname"].ToString();
                                                newrowqst["ValidationTypeCode"] = drq["ValidationTypeCode"].ToString();
                                                newrowqst["QuestionTypeCode"] = drq["QuestionTypeCode"].ToString();
                                                newrowqst["QuestionType"] = drq["QuestionType"].ToString();
                                                newrowqst["HasChildQuestion"] = drq["HasChildQuestion"].ToString();
                                                newrowqst["QuestionCode"] = drq["QuestionCode"].ToString();
                                                newrowqst["ParentQuestion"] = drq["ParentQuestion"].ToString();
                                                newrowqst["ConditionValue"] = drq["ConditionValue"].ToString();
                                                newrowqst["ControlType"] = drq["ControlType"].ToString();
                                                newrowqst["QuestionLevel"] = drq["QuestionLevel"].ToString();
                                                newrowqst["HasOptions"] = drq["HasOptions"].ToString();
                                                newrowqst["OptionID"] = drq["OptionID"].ToString();
                                                newrowqst["ChildQuestionConditionID"] = drq["ChildQuestionConditionID"].ToString();
                                                newrowqst["HelpText"] = drq["HelpText"].ToString();
                                                newrowqst["FrequencyOfQuestions"] = drq["FrequencyOfQuestions"].ToString();
                                                newrowqst["NumberOfTimes"] = drq["NumberOfTimes"].ToString();
                                                newrowqst["FrequencyStartDate"] = drq["FrequencyStartDate"].ToString();
                                                newrowqst["SectorCode"] = drq["SectorCode"].ToString();
                                                newrowqst["ProgramCode"] = drq["ProgramCode"].ToString();
                                                newrowqst["InterventionCode"] = drq["InterventionCode"].ToString();
                                                newrowqst["CategoryId"] = drq["CategoryId"].ToString();
                                                newrowqst["SuggestiveText"] = drq["SuggestiveText"].ToString();
                                                questionset.Rows.Add(newrowqst);

                                                string validationtype = drq["validationtypecode"].ToString();
                                                foreach (DataRow drv in dtvalidations.Select("validationtypecode='" + validationtype + "'"))
                                                {
                                                    DataRow newroww = contrlvalidations.NewRow();
                                                    newroww["WorkItemCode"] = workitemcode;
                                                    newroww["validationtypecode"] = drv["validationtypecode"].ToString();
                                                    newroww["questioncode"] = drv["questioncode"].ToString();
                                                    newroww["validationtypename"] = drv["validationtypename"].ToString();
                                                    newroww["allowedvalues"] = drv["allowedvalues"].ToString();
                                                    newroww["violationmessage"] = drv["violationmessage"].ToString();
                                                    newroww["controldescription"] = drv["controldescription"].ToString();
                                                    contrlvalidations.Rows.Add(newroww);
                                                }
                                                string OptionID = drq["OptionID"].ToString();
                                                foreach (DataRow drv in dtquestionoption.Select("questioncode='" + OptionID + "'"))
                                                {
                                                    DataRow newrowop = questionoptions.NewRow();
                                                    newrowop["WorkItemCode"] = workitemcode;
                                                    newrowop["questioncode"] = drv["questioncode"].ToString();
                                                    newrowop["displaytext"] = drv["displaytext"].ToString();
                                                    newrowop["displayvalue"] = drv["displayvalue"].ToString();
                                                    newrowop["displayorder"] = drv["displayorder"].ToString();
                                                    questionoptions.Rows.Add(newrowop);
                                                }
                                                //array_push($wrk,['workitems' => $work_items,'locations' => $locations,'survey'=>$surveys,'questions'=>$questionset,'validations' =>  $validation,'questionoptions'=>$options]);
                                            }
                                        }
                                    }
                                }
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

                                // end work item loop
                            }
                        }

                        List<getall> getallwdtls = new List<getall>();
                        List<getsingleval> getsinglevalddtls = new List<getsingleval>();
                        if (workitemdetails.Rows.Count > 0)
                        {
                            List<Questiontype> qtlist = new List<Questiontype>();
                            List<shroads> shroadslist = new List<shroads>();
                            List<gproads> gproadslist = new List<gproads>();
                            cmd = new SqlCommand("SELECT questiontypecode, questiontype FROM tbl_MST_questiontype");
                            DataTable dtquestiontypes = vdm.SelectQuery(cmd).Tables[0];
                            postcmd = new NpgsqlCommand("SELECT sh_code, ST_AsText(geom) AS geom FROM sh_roads");
                            DataTable dtshroads = postvdm.SelectQuery(postcmd).Tables[0];
                            postcmd = new NpgsqlCommand("SELECT gpr_code, ST_AsText(geom) AS geom FROM gp_roads");
                            DataTable dtgproads = postvdm.SelectQuery(postcmd).Tables[0];
                            foreach (DataRow drl in dtquestiontypes.Rows)
                            {
                                Questiontype qtinfo = new Questiontype();
                                qtinfo.questiontype = drl["questiontype"].ToString();
                                qtinfo.questiontypecode = drl["questiontypecode"].ToString();
                                qtlist.Add(qtinfo);
                            }
                            foreach (DataRow drsh in dtshroads.Rows)
                            {
                                shroads shinfo = new shroads();
                                shinfo.sh_code = drsh["sh_code"].ToString();
                                shinfo.st_astext = drsh["geom"].ToString();
                                shroadslist.Add(shinfo);
                            }
                            foreach (DataRow drgp in dtgproads.Rows)
                            {
                                gproads gpinfo = new gproads();
                                gpinfo.gpr_code = drgp["gpr_code"].ToString();
                                gpinfo.st_astext = drgp["geom"].ToString();
                                gproadslist.Add(gpinfo);
                            }
                            getsingleval getInwadDatas1 = new getsingleval();
                            getInwadDatas1.questiontypes = qtlist;
                            getInwadDatas1.sh_roads_arr = shroadslist;
                            getInwadDatas1.gp_roads_arr = gproadslist;
                            getInwadDatas1.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                            getsinglevalddtls.Add(getInwadDatas1);

                            foreach (DataRow dr in workitemdetails.Rows)
                            {
                                List<workitems> workitemslist = new List<workitems>();
                                List<locations> locationlist = new List<locations>();
                                List<servey> serveylist = new List<servey>();
                                List<questionset> questionlist = new List<questionset>();
                                List<contrlvalidations> validationlist = new List<contrlvalidations>();
                                List<questionoptions> questionoptionlist = new List<questionoptions>();
                                List<status> statuslist = new List<status>();
                                string workitemcode = dr["WorkItemCode"].ToString();
                                workitems winfo = new workitems();
                                winfo.wrkitem = dr["WorkItemCode"].ToString();
                                winfo.workitemcode = dr["WorkItemCode"].ToString();
                                winfo.milestonecode = dr["MileStoneCode"].ToString();
                                winfo.workitemname = dr["WorkItemName"].ToString();
                                winfo.workitemdesc = dr["WorkItemDesc"].ToString();
                                winfo.workitemtype = dr["WorkItemType"].ToString();
                                winfo.parentworkitemcode = dr["ParentWorkItemCode"].ToString();
                                string pstdte = dr["PlanStartDate"].ToString();
                                if (pstdte != "" && pstdte != null && pstdte != "null")
                                {
                                    DateTime dt = Convert.ToDateTime(dr["PlanStartDate"].ToString());
                                    winfo.planstartdate = dt.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.planstartdate = pstdte;
                                }
                                string pstenddte = dr["PlanEndDate"].ToString();
                                if (pstenddte != "" && pstenddte != null && pstenddte != "null")
                                {
                                    DateTime dtPlanEndDate = Convert.ToDateTime(dr["PlanEndDate"].ToString());
                                    winfo.planenddate = dtPlanEndDate.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.planenddate = pstenddte;
                                }
                                string ActualStartDate = dr["ActualStartDate"].ToString();
                                if (ActualStartDate != "" && ActualStartDate != null && ActualStartDate != "null")
                                {
                                    DateTime dtActualStartDateDate = Convert.ToDateTime(dr["ActualStartDate"].ToString());
                                    winfo.actualstartdate = dtActualStartDateDate.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.actualstartdate = ActualStartDate;
                                }
                                string ActualEndDate = dr["ActualEndDate"].ToString();
                                if (ActualEndDate != "" && ActualEndDate != null && ActualEndDate != "null")
                                {
                                    DateTime dtActualEndDateDate = Convert.ToDateTime(dr["ActualEndDate"].ToString());
                                    winfo.actualstartdate = dtActualEndDateDate.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.actualstartdate = ActualEndDate;
                                }


                                winfo.planbudget = dr["PlanBudget"].ToString();
                                winfo.nonactivitybudget = dr["NonActivityBudget"].ToString();
                                winfo.nonactivitybudgetpercentage = dr["NonActivityBudgetPercentage"].ToString();

                                winfo.actualenddate = dr["ActualEndDate"].ToString();
                                winfo.actualexpenses = dr["ActualExpenses"].ToString();
                                string primaryownerid = dr["PrimaryOwner"].ToString();
                                string cretedbyid = dr["CreatedBy"].ToString();
                                string primaryownername = dr["Primaryname"].ToString();
                                string createdbyname = dr["CreatedName"].ToString();

                                string powner = primaryownername + " (" + primaryownerid + ")";
                                string cretedn = createdbyname + " (" + cretedbyid + ")";

                                winfo.primaryowner = powner;
                                winfo.createdby = cretedn;
                                winfo.primaryowner1 = dr["PrimaryOwner"].ToString();
                                winfo.createdby1 = dr["CreatedBy"].ToString();
                                winfo.locationcode = dr["LocationCode"].ToString();
                                winfo.target = dr["Target"].ToString();
                                winfo.targetmeasurementunit = dr["TargetMeasurementUnit"].ToString();
                                winfo.achievement = dr["Achievement"].ToString();
                                winfo.percentagecompleted = dr["PercentageCompleted"].ToString();
                                winfo.remarks = dr["Remarks"].ToString();
                                winfo.status = dr["Status"].ToString();
                                string CreatedOn = dr["CreatedOn"].ToString();
                                if (CreatedOn != "" && CreatedOn != null && CreatedOn != "null")
                                {
                                    DateTime dtCreatedOn = Convert.ToDateTime(dr["CreatedOn"].ToString());
                                    winfo.createdon = dtCreatedOn.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.createdon = dr["CreatedOn"].ToString();
                                }

                                string ModifiedOn = dr["ModifiedOn"].ToString();
                                if (ModifiedOn != "" && ModifiedOn != null && ModifiedOn != "null")
                                {
                                    DateTime dtModifiedOn = Convert.ToDateTime(dr["ModifiedOn"].ToString());
                                    winfo.modifiedon = dtModifiedOn.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.modifiedon = dr["ModifiedOn"].ToString();
                                }


                                string SynchedOn = dr["SynchedOn"].ToString();
                                if (SynchedOn != "" && SynchedOn != null && SynchedOn != "null")
                                {
                                    DateTime dtSynchedOn = Convert.ToDateTime(dr["SynchedOn"].ToString());
                                    winfo.synchedon = dtSynchedOn.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.synchedon = dr["SynchedOn"].ToString();
                                }

                                winfo.modifiedby = dr["ModifiedBy"].ToString();
                                winfo.displayorder = dr["DisplayOrder"].ToString();

                                winfo.surveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                string serveyworkitemmappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                winfo.surveycode = dr["SurveyCode"].ToString();
                                winfo.isactive = dr["isActive"].ToString();
                                winfo.frequncy = dr["Frequncy"].ToString();
                                winfo.frequncytype = dr["Frequncytype"].ToString();

                                string EndDDate = dr["EndDDate"].ToString();
                                if (EndDDate != "" && EndDDate != null && EndDDate != "null")
                                {
                                    DateTime dtEndDDate = Convert.ToDateTime(dr["EndDDate"].ToString());
                                    winfo.endddate = dtEndDDate.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.endddate = dr["EndDDate"].ToString();
                                }

                                string StartDate = dr["StartDate"].ToString();
                                if (StartDate != "" && StartDate != null && StartDate != "null")
                                {
                                    DateTime dtStartDate = Convert.ToDateTime(dr["StartDate"].ToString());
                                    winfo.startdate = dtStartDate.ToString("yyyy-MM-dd hh:mm tt");
                                }
                                else
                                {
                                    winfo.startdate = dr["StartDate"].ToString();
                                }

                                workitemslist.Add(winfo);

                                cmd = new SqlCommand("SELECT COUNT(*) AS count, BlockCode, GramPanchayatCode, VillageCode FROM  tbl_MST_Respondant GROUP BY BlockCode, GramPanchayatCode, VillageCode");
                                DataTable dtrespondentcode = vdm.SelectQuery(cmd).Tables[0];


                                cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
                                cmd.Parameters.Add("@swimc", serveyworkitemmappingcode);
                                DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];

                                double target = 0;
                                double blockcodecount = 0;
                                double gpcount = 0;
                                double villagecount = 0;
                                foreach (DataRow drl in locations.Select("WorkItemCode='" + workitemcode + "'"))
                                {
                                    locations locinfo = new locations();
                                    locinfo.locationcode = drl["locationcode"].ToString();
                                    locinfo.locationname = drl["locationname"].ToString();
                                    locinfo.locationtype = drl["locationtype"].ToString();
                                    locationlist.Add(locinfo);

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

                                foreach (DataRow drs in surveys.Select("Workitemcode='" + workitemcode + "'"))
                                {
                                    servey userinfo = new servey();
                                    userinfo.workitemcode = drs["Workitemcode"].ToString();
                                    userinfo.surveycode = drs["SurveyCode"].ToString();
                                    userinfo.surveyname = drs["SurveyName"].ToString();
                                    userinfo.surveydesc = drs["SurveyDesc"].ToString();
                                    userinfo.sectorcode = drs["SectorCode"].ToString();
                                    userinfo.questionsetcode = drs["QuestionSetCode"].ToString();
                                    userinfo.status = drs["Status"].ToString();
                                    string createdby = drs["CreatedBy"].ToString();
                                    cmd = new SqlCommand("SELECT EmployeeCode, EmployeeName FROM    tbl_MST_Employee where EmployeeCode=@EmployeeCode");
                                    cmd.Parameters.Add("@EmployeeCode", createdby);
                                    DataTable dtempdtls = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtempdtls.Rows.Count > 0)
                                    {
                                        foreach (DataRow dremp in dtempdtls.Rows)
                                        {
                                            string empname = dremp["EmployeeName"].ToString();
                                            string EmployeeCode = dremp["EmployeeCode"].ToString();
                                            userinfo.createdby = empname + " (" + EmployeeCode + ")";
                                        }
                                    }
                                    else
                                    {
                                        userinfo.createdby = drs["CreatedBy"].ToString();
                                    }
                                    userinfo.createdon = drs["CreatedOn"].ToString();
                                    userinfo.modifiedby = drs["ModifiedBy"].ToString();
                                    userinfo.modifiedon = drs["ModifiedOn"].ToString();
                                    userinfo.type = drs["Type"].ToString();
                                    userinfo.frequency = drs["Frequency"].ToString();
                                    userinfo.startdate = drs["StartDate"].ToString();
                                    userinfo.enddate = drs["EndDate"].ToString();
                                    userinfo.programcode = drs["ProgramCode"].ToString();
                                    userinfo.interventioncode = drs["InterventionCode"].ToString();
                                    serveylist.Add(userinfo);
                                }

                                foreach (DataRow drqs in questionset.Select("WorkItemCode='" + workitemcode + "'"))
                                {
                                    questionset qset = new questionset();
                                    qset.questiontext = drqs["QuestionText"].ToString();
                                    qset.sectiondisplayorder = drqs["SectionDisplayOrder"].ToString();
                                    qset.questiondisplayorder = drqs["questiondisplayorder"].ToString();
                                    qset.sectionname = drqs["sectionname"].ToString();
                                    qset.validationtypecode = drqs["ValidationTypeCode"].ToString();
                                    qset.questiontypecode = drqs["QuestionTypeCode"].ToString();
                                    qset.questiontype = drqs["QuestionType"].ToString();
                                    qset.haschildquestion = drqs["HasChildQuestion"].ToString();
                                    qset.questioncode = drqs["QuestionCode"].ToString();
                                    qset.parentquestion = drqs["ParentQuestion"].ToString();
                                    qset.conditionvalue = drqs["ConditionValue"].ToString();
                                    qset.controltype = drqs["ControlType"].ToString();
                                    qset.questionlevel = drqs["QuestionLevel"].ToString();
                                    qset.hasoptions = drqs["HasOptions"].ToString();
                                    qset.optionid = drqs["OptionID"].ToString();
                                    qset.childquestionconditionid = drqs["ChildQuestionConditionID"].ToString();
                                    qset.helptext = drqs["HelpText"].ToString();
                                    qset.frequencyofquestions = drqs["FrequencyOfQuestions"].ToString();
                                    qset.numberoftimes = drqs["NumberOfTimes"].ToString();
                                    qset.frequencystartdate = drqs["FrequencyStartDate"].ToString();
                                    qset.sectorcode = drqs["SectorCode"].ToString();
                                    qset.programcode = drqs["ProgramCode"].ToString();
                                    qset.interventioncode = drqs["InterventionCode"].ToString();
                                    qset.categoryid = drqs["CategoryId"].ToString();
                                    qset.suggestivetext = drqs["SuggestiveText"].ToString();
                                    questionlist.Add(qset);
                                }

                                foreach (DataRow drval in contrlvalidations.Select("WorkItemCode='" + workitemcode + "'"))
                                {
                                    contrlvalidations ctrlvalidations = new contrlvalidations();
                                    ctrlvalidations.validationtypecode = drval["validationtypecode"].ToString();
                                    ctrlvalidations.questioncode = drval["questioncode"].ToString();
                                    ctrlvalidations.validationtypename = drval["validationtypename"].ToString();
                                    ctrlvalidations.allowedvalues = drval["allowedvalues"].ToString();
                                    ctrlvalidations.violationmessage = drval["violationmessage"].ToString();
                                    ctrlvalidations.controldescription = drval["controldescription"].ToString();
                                    validationlist.Add(ctrlvalidations);
                                }

                                foreach (DataRow drop in questionoptions.Select("WorkItemCode='" + workitemcode + "'"))
                                {
                                    questionoptions options = new questionoptions();
                                    options.displayorder = drop["displayorder"].ToString();
                                    options.displaytext = drop["displaytext"].ToString();
                                    options.displayvalue = drop["displayvalue"].ToString();
                                    options.questioncode = drop["questioncode"].ToString();
                                    questionoptionlist.Add(options);
                                }

                                double gtot = blockcodecount + gpcount + villagecount;
                                target = gtot;
                                double submitedcount = 0;
                                double savedcount = 0;
                                double opcount = 0;
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
                                cmd = new SqlCommand("SELECT COUNT(*) AS opencount FROM  tbl_MMP_SurveyBeneficiary WHERE  (SurveyCode = @mpcode)");
                                cmd.Parameters.Add("@mpcode", serveyworkitemmappingcode);
                                DataTable dtselectedrespondents = vdm.SelectQuery(cmd).Tables[0];
                                if (dtselectedrespondents.Rows.Count > 0)
                                {
                                    foreach (DataRow dro in dtselectedrespondents.Rows)
                                    {
                                        opcount = Convert.ToDouble(dro["opencount"].ToString());
                                    }
                                }
                                status syncst = new status();
                                syncst.target = target.ToString();

                                syncst.saved = savedcount.ToString();
                                syncst.submitted = submitedcount.ToString();
                                syncst.synced = submitedcount.ToString();
                                if (lastsynced == "")
                                {
                                    double sumcount = savedcount + submitedcount;
                                    double opencount = opcount - sumcount;
                                    syncst.open = opencount.ToString();
                                }
                                else
                                {
                                    double sumcount = savedcount + submitedcount;
                                    double opencount = opcount - sumcount;
                                    syncst.open = opencount.ToString();
                                }
                                statuslist.Add(syncst);
                                getall getInwadDatas = new getall();
                                getInwadDatas.workitems = workitemslist;
                                getInwadDatas.locations = locationlist;
                                getInwadDatas.survey = serveylist;
                                getInwadDatas.questions = questionlist;
                                getInwadDatas.validations = validationlist;
                                getInwadDatas.questionoptions = questionoptionlist;
                                getInwadDatas.status = statuslist;
                                getallwdtls.Add(getInwadDatas);
                            }


                            List<workitemsupdate> updatelist = new List<workitemsupdate>();
                            List<Closedworkitems> Closedworkitemslist = new List<Closedworkitems>();
                            if (dtresourceallocation.Rows.Count > 0)
                            {
                                DateTime lastsyncheddate = DateTime.Now;
                                if (lastsynced == "")
                                {

                                }
                                else
                                {
                                    lastsyncheddate = Convert.ToDateTime(lastsynced);
                                }

                                foreach (DataRow dr in dtresourceallocation.Rows)
                                {
                                    string workitemcode = dr["WorkItemCode"].ToString();
                                    foreach (DataRow drw in dtworkitemdetails.Select("WorkItemCode='" + workitemcode + "'"))
                                    {
                                        string ModifiedOn = drw["ModifiedOn"].ToString();
                                        string isactive = drw["isActive"].ToString();
                                        string WorkItemCode = drw["WorkItemCode"].ToString();
                                        string ActualStartDate = drw["ActualStartDate"].ToString();
                                        string ActualEndDate = drw["ActualEndDate"].ToString();
                                        string SurveyWorkItemMappingCode = drw["SurveyWorkItemMappingCode"].ToString();
                                        string SurveyCode = drw["SurveyCode"].ToString();
                                        if (ModifiedOn != "")
                                        {
                                            DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                            if (dtModifiedOn > lastsyncheddate)
                                            {
                                                workitemsupdate updateworkitem = new workitemsupdate();
                                                if (ActualStartDate != "" && ActualStartDate != null && ActualStartDate != "null")
                                                {
                                                    DateTime dtActualStartDateDate = Convert.ToDateTime(drw["ActualStartDate"].ToString());
                                                    string actualstartdate = dtActualStartDateDate.ToString("yyyy-MM-dd hh:mm tt");
                                                    string actualenddate = "";
                                                    if (ActualEndDate != "" && ActualEndDate != null && ActualEndDate != "null")
                                                    {
                                                        DateTime dtActualEndDateDate = Convert.ToDateTime(drw["ActualEndDate"].ToString());
                                                        actualenddate = dtActualEndDateDate.ToString("yyyy-MM-dd hh:mm tt");
                                                    }
                                                    updateworkitem.Actualstartdate = actualstartdate;
                                                    updateworkitem.Actualenddate = actualenddate;
                                                    updateworkitem.workitemcode = WorkItemCode;
                                                    updateworkitem.surveymappingcode = SurveyWorkItemMappingCode;
                                                    updateworkitem.surveycode = SurveyCode;
                                                    updatelist.Add(updateworkitem);
                                                }
                                            }
                                        }
                                    }
                                    foreach (DataRow drcw in dtclosedworkitemdetails.Select("WorkItemCode='" + workitemcode + "'"))
                                    {
                                        string isactive = drcw["isActive"].ToString();
                                        string SurveyWorkItemMappingCode = drcw["SurveyWorkItemMappingCode"].ToString();
                                        if (isactive != "True")
                                        {
                                            Closedworkitems closeditems = new Closedworkitems();
                                            closeditems.workitemcode = workitemcode;
                                            closeditems.surveymappingcode = SurveyWorkItemMappingCode;
                                            closeditems.status = isactive;
                                            Closedworkitemslist.Add(closeditems);
                                        }
                                    }
                                }
                            }

                            List<getoveralllist> getoveralllistdtls = new List<getoveralllist>();
                            getoveralllist getoverDatas = new getoveralllist();
                            getoverDatas.getalldtls = getallwdtls;
                            getoverDatas.getsinglevaldtls = getsinglevalddtls;
                            getoverDatas.workitemsupdatedata = updatelist;
                            getoverDatas.closedworkitemsdata = Closedworkitemslist;
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

                            //JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            //Context.Response.Write(jsonSerializer.Serialize(getoveralllistdtls));
                        }
                        else
                        {
                            List<Questiontype> qtlist = new List<Questiontype>();
                            List<shroads> shroadslist = new List<shroads>();
                            List<gproads> gproadslist = new List<gproads>();

                            getsingleval getInwadDatas1 = new getsingleval();
                            getInwadDatas1.questiontypes = qtlist;
                            getInwadDatas1.sh_roads_arr = shroadslist;
                            getInwadDatas1.gp_roads_arr = gproadslist;
                            getInwadDatas1.lastsynced = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt");
                            getsinglevalddtls.Add(getInwadDatas1);

                            List<workitems> workitemslist = new List<workitems>();
                            List<locations> locationlist = new List<locations>();
                            List<servey> serveylist = new List<servey>();
                            List<questionset> questionlist = new List<questionset>();
                            List<contrlvalidations> validationlist = new List<contrlvalidations>();
                            List<questionoptions> questionoptionlist = new List<questionoptions>();
                            List<status> statuslist = new List<status>();
                            getall getInwadDatas = new getall();
                            getInwadDatas.workitems = workitemslist;
                            getInwadDatas.locations = locationlist;
                            getInwadDatas.survey = serveylist;
                            getInwadDatas.questions = questionlist;
                            getInwadDatas.validations = validationlist;
                            getInwadDatas.questionoptions = questionoptionlist;
                            getInwadDatas.status = statuslist;
                            getallwdtls.Add(getInwadDatas);
                            List<workitemsupdate> updatelist = new List<workitemsupdate>();
                            List<Closedworkitems> Closedworkitemslist = new List<Closedworkitems>();
                            List<getoveralllist> getoveralllistdtls = new List<getoveralllist>();
                            DateTime lastsyncheddate = DateTime.Now;
                            if (lastsynced == "")
                            {

                            }
                            else
                            {
                                lastsyncheddate = Convert.ToDateTime(lastsynced);
                            }
                            foreach (DataRow dr in dtresourceallocation.Rows)
                            {
                                string workitemcode = dr["WorkItemCode"].ToString();
                                foreach (DataRow drw in dtworkitemdetails.Select("WorkItemCode='" + workitemcode + "'"))
                                {
                                    string ModifiedOn = drw["ModifiedOn"].ToString();
                                    string isactive = drw["isActive"].ToString();
                                    string WorkItemCode = drw["WorkItemCode"].ToString();
                                    string ActualStartDate = drw["ActualStartDate"].ToString();
                                    string ActualEndDate = drw["ActualEndDate"].ToString();
                                    string SurveyWorkItemMappingCode = drw["SurveyWorkItemMappingCode"].ToString();
                                    string SurveyCode = drw["SurveyCode"].ToString();
                                    if (ModifiedOn != "")
                                    {
                                        DateTime dtModifiedOn = Convert.ToDateTime(ModifiedOn);
                                        if (dtModifiedOn > lastsyncheddate)
                                        {
                                            workitemsupdate updateworkitem = new workitemsupdate();
                                            if (ActualStartDate != "" && ActualStartDate != null && ActualStartDate != "null")
                                            {
                                                DateTime dtActualStartDateDate = Convert.ToDateTime(drw["ActualStartDate"].ToString());
                                                string actualstartdate = dtActualStartDateDate.ToString("yyyy-MM-dd hh:mm tt");
                                                string actualenddate = "";
                                                if (ActualEndDate != "" && ActualEndDate != null && ActualEndDate != "null")
                                                {
                                                    DateTime dtActualEndDateDate = Convert.ToDateTime(drw["ActualEndDate"].ToString());
                                                    actualenddate = dtActualEndDateDate.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                updateworkitem.Actualstartdate = actualstartdate;
                                                updateworkitem.Actualenddate = actualenddate;
                                                updateworkitem.workitemcode = WorkItemCode;
                                                updateworkitem.surveymappingcode = SurveyWorkItemMappingCode;
                                                updateworkitem.surveycode = SurveyCode;
                                                updatelist.Add(updateworkitem);
                                            }
                                        }
                                    }
                                }
                                foreach (DataRow drcw in dtclosedworkitemdetails.Select("WorkItemCode='" + workitemcode + "'"))
                                {
                                    string isactive = drcw["isActive"].ToString();
                                    string SurveyWorkItemMappingCode = drcw["SurveyWorkItemMappingCode"].ToString();
                                    if (isactive != "True")
                                    {
                                        Closedworkitems closeditems = new Closedworkitems();
                                        closeditems.workitemcode = workitemcode;
                                        closeditems.surveymappingcode = SurveyWorkItemMappingCode;
                                        closeditems.status = isactive;
                                        Closedworkitemslist.Add(closeditems);
                                    }
                                }
                            }







                            getoveralllist getoverDatas = new getoveralllist();
                            getoverDatas.getalldtls = getallwdtls;
                            getoverDatas.getsinglevaldtls = getsinglevalddtls;
                            getoverDatas.workitemsupdatedata = updatelist;
                            getoverDatas.closedworkitemsdata = Closedworkitemslist;
                            getoveralllistdtls.Add(getoverDatas);
                            //JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            //Context.Response.Write(jsonSerializer.Serialize(getoveralllistdtls));

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

    public class getsingleval
    {
        public List<Questiontype> questiontypes { get; set; }
        public List<shroads> sh_roads_arr { get; set; }
        public List<gproads> gp_roads_arr { get; set; }
        public string lastsynced { get; set; }
    }

    public class getall  //new
    {
        
        public List<workitems> workitems { get; set; }
        public List<locations> locations { get; set; }
        public List<servey> survey { get; set; }
        public List<questionset> questions { get; set; }
        public List<contrlvalidations> validations { get; set; }
        public List<questionoptions> questionoptions { get; set; }
        public List<status> status { get; set; }
    }

    public class getoveralllist
    {
        
        public List<getsingleval> getsinglevaldtls { get; set; }
        public List<getall> getalldtls { get; set; }
        public List<workitemsupdate> workitemsupdatedata { get; set; }
        public List<Closedworkitems> closedworkitemsdata { get; set; }
    }

    public class servey
    {
        public string workitemcode { get; set; }
        public string surveycode { get; set; }
        public string surveyname { get; set; }
        public string surveydesc { get; set; }
        public string sectorcode { get; set; }
        public string questionsetcode { get; set; } 
        public string status { get; set; } 
        public string createdby { get; set; } 
        public string createdon { get; set; } 
        public string modifiedby { get; set; } 
        public string modifiedon { get; set; } 
        public string type { get; set; } 
        public string frequency { get; set; } 
        public string startdate { get; set; } 
        public string enddate { get; set; } 
        public string programcode { get; set; } 
        public string interventioncode { get; set; } 
    }

    public class workitems
    {
        public string wrkitem { get; set; }
        public string workitemcode { get; set; }
        public string milestonecode { get; set; }
        public string workitemname { get; set; }
        public string workitemdesc { get; set; }
        public string workitemtype { get; set; } 
        public string parentworkitemcode { get; set; } 
        public string planstartdate { get; set; } 
        public string planenddate { get; set; } 
        public string planbudget { get; set; } 
        public string nonactivitybudget { get; set; } 
        public string nonactivitybudgetpercentage { get; set; } 
        public string actualstartdate { get; set; } 
        public string actualenddate { get; set; } 
        public string actualexpenses { get; set; }
        public string primaryowner { get; set; }
        public string primaryowner1 { get; set; } 
        public string locationcode { get; set; } 
        public string target { get; set; } 
        public string targetmeasurementunit { get; set; } 
        public string achievement { get; set; } 
        public string percentagecompleted { get; set; } 
        public string remarks { get; set; } 
        public string status { get; set; }
        public string createdby { get; set; }
        public string createdby1 { get; set; } 
        public string createdon { get; set; } 
        public string modifiedby { get; set; } 
        public string modifiedon { get; set; } 
        public string displayorder { get; set; } 
        public string synchedon { get; set; } 
        public string surveyworkitemmappingcode { get; set; } 
        public string surveycode { get; set; } 
        public string isactive { get; set; } 
        public string frequncy { get; set; } 
        public string startdate { get; set; } 
        public string endddate { get; set; } 
        public string frequncytype { get; set; }

    }
    public class Closedworkitems
    {
        public string workitemcode { get; set; }
        public string surveymappingcode { get; set; }
        public string status { get; set; }
    }
    public class workitemsupdate
    {
        public string workitemcode { get; set; }
        public string surveymappingcode { get; set; }
        public string surveycode { get; set; }
        public string Actualstartdate { get; set; }
        public string Actualenddate { get; set; }
    }

    public class Questiontype
    {
        public string questiontypecode { get; set; }
        public string questiontype { get; set; }
    }

    public class shroads
    {
        public string sh_code { get; set; }
        public string st_astext { get; set; }
        
    }

    public class gproads
    {
        public string gpr_code { get; set; }
        public string st_astext { get; set; }
    }

    public class locations 
    {
        public string locationcode { get; set; }
        public string locationname { get; set; }
        public string locationtype { get; set; } 
    }

    public class questionset
    {
        
        public string questiontext { get; set; }
        public string sectiondisplayorder { get; set; }
        public string questiondisplayorder { get; set; }
        public string sectionname { get; set; }
        public string validationtypecode { get; set; }
        public string questiontypecode { get; set; }
        public string questiontype { get; set; }
        public string haschildquestion { get; set; }
        public string questioncode { get; set; }
        public string parentquestion { get; set; }
        public string conditionvalue { get; set; }
        public string controltype { get; set; }
        public string questionlevel { get; set; }
        public string hasoptions { get; set; }
        public string optionid { get; set; }
        public string childquestionconditionid { get; set; }
        public string helptext { get; set; }
        public string frequencyofquestions { get; set; }
        public string numberoftimes { get; set; }
        public string frequencystartdate { get; set; }
        public string sectorcode { get; set; }
        public string programcode { get; set; }
        public string interventioncode { get; set; }
        public string categoryid { get; set; }
        public string suggestivetext { get; set; }
    }

    public class contrlvalidations
    {
        public string validationtypecode { get; set; }
        public string questioncode { get; set; }
        public string validationtypename { get; set; }
        public string allowedvalues { get; set; }
        public string violationmessage { get; set; }
        public string controldescription { get; set; } 
    }
    public class questionoptions
    {
        public string questioncode { get; set; }
        public string displaytext { get; set; }
        public string displayvalue { get; set; }
        public string displayorder { get; set; }
    }
    public class status
    {
        public string target { get; set; }
        public string open { get; set; }
        public string saved { get; set; }//1
        public string submitted { get; set; }//2
        public string synced { get; set; }//2
    }
}
