using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Web.Script.Services;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for postgsqlservice
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class postgsqlservice : System.Web.Services.WebService {
    NpgsqlCommand postcmd;
    SAPdbmanger postvdm = new SAPdbmanger();
    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void benficerydetails(string WorkItemCode, string SurveyWorkItemMappingCode, string Type)
    {
        vdm = new SalesDBManager();
        postvdm = new SAPdbmanger();

        string workitemcode = "51621535";
        string mappingcode = "51621535";
        string servaytype = "51621535";
        
        cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode=@mappingcode AND  tbl_TRN_WorkItem.WorkItemCode=@workitemcode");
        cmd.Parameters.Add("@workitemcode", WorkItemCode);
        cmd.Parameters.Add("@mappingcode", SurveyWorkItemMappingCode);
        DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];

        cmd = new SqlCommand("SELECT  RowCode, WorkItemCode, LocationCode, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn FROM   tbl_MMP_WorkItemLocation WHERE WorkItemCode=@workitemcode");
        cmd.Parameters.Add("@workitemcode", workitemcode);
        DataTable dtWorkItemLocationdtls = vdm.SelectQuery(cmd).Tables[0];

        cmd = new SqlCommand("SELECT  SurveyCode, SurveyName, SurveyDesc, SectorCode, QuestionSetCode, Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Type, Frequency, StartDate, EndDate, ProgramCode, InterventionCode FROM tbl_MST_Survey WHERE Type=@type");
        cmd.Parameters.Add("@type", Type);
        DataTable dtsurveydtls = vdm.SelectQuery(cmd).Tables[0];
        
    }

    public class getall  //new
    {
        public List<workitems> workitemdtls { get; set; }
        public List<locations> locationsdtls { get; set; }
        public List<servey> serveyDetails { get; set; }
        public List<questionset> questionsetdtls { get; set; }
        public List<contrlvalidations> contrlvalidations { get; set; }
        public List<questionoptions> questionoptions { get; set; }
    }

    public class servey
    {
        public string workitemcode { get; set; }
        public string SurveyCode { get; set; }
        public string SurveyName { get; set; }
        public string SurveyDesc { get; set; }
        public string SectorCode { get; set; }
        public string QuestionSetCode { get; set; }
    }

    public class workitems
    {
        public string workitemcode { get; set; }
        public string MileStoneCode { get; set; }
        public string WorkItemName { get; set; }
        public string WorkItemDesc { get; set; }
        public string WorkItemType { get; set; }
    }

    public class locations
    {
        public string locationcode { get; set; }
        public string locationname { get; set; }
        public string locationtype { get; set; }
    }
    public class questionset
    {
        public string QuestionText { get; set; }
        public string SectionDisplayOrder { get; set; }
        public string questiondisplayorder { get; set; }
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
}
