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
/// Summary description for beneficiarydownload
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
 [System.Web.Script.Services.ScriptService]
public class beneficiarydownload : System.Web.Services.WebService {
    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();
    NpgsqlCommand postcmd;
    SAPdbmanger postvdm = new SAPdbmanger();
    

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

    public class bene_status
    {
        public string surveyworkitemmappingcode { get; set; }
        public string respondantcode { get; set; }
        public string InterventionCode { get; set; }
        public string UploadedFrom { get; set; }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void benficiarydownloadsave(string workitemcode, string surveyworkitemmappingcode, string type, string empid)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            string mappingcode = surveyworkitemmappingcode;

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

                        if (workitemcode != "" && workitemcode != null && workitemcode != "null")
                        {
                            cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.LocationType, tbl_MST_Location.Label, tbl_MST_Location.CreatedBy, tbl_MST_Location.CreatedOn,  tbl_MST_Location.ModifiedBy, tbl_MST_Location.ModifiedOn, tbl_MST_Location.IsActive, tbl_MST_Location.ParentLocationCode, tbl_MST_Location.ParentLocCode, tbl_MMP_WorkItemLocation.RowCode,   tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MMP_WorkItemLocation.LocationCode AS Expr1, tbl_MMP_WorkItemLocation.CreatedBy AS Expr2, tbl_MMP_WorkItemLocation.CreatedOn AS Expr3,  tbl_MMP_WorkItemLocation.ModifiedBy AS Expr4, tbl_MMP_WorkItemLocation.ModifiedOn AS Expr5 FROM  tbl_MST_Location INNER JOIN tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode where tbl_MMP_WorkItemLocation.WorkItemCode=@wcode");
                            cmd.Parameters.Add("@wcode", workitemcode);
                            DataTable dtlocationdetails = vdm.SelectQuery(cmd).Tables[0];
                            List<roadarray_vill> roadarrayvilllist = new List<roadarray_vill>();
                            List<survey_bene> surveybenelist = new List<survey_bene>();
                            List<buildinggeom> buildinggeomlist = new List<buildinggeom>();
                            List<status> statuslist = new List<status>();
                            List<govtbuildings> govtbuildingslist = new List<govtbuildings>();
                            List<roadarray_vill> roadarrayvillboundries = new List<roadarray_vill>();
                            ArrayList myItems = new ArrayList();
                            if (dtlocationdetails.Rows.Count > 0)
                            {
                                string locationcode = "";
                                string buildingcode = "";
                                string loctype = "";
                                string locname = "";

                                foreach (DataRow dr in dtlocationdetails.Rows)
                                {
                                    string loccode = dr["LocationCode"].ToString();
                                    string locationtype = dr["LocationType"].ToString();
                                    string LocationName = dr["LocationName"].ToString();
                                    string lname = LocationName.ToLower();
                                    if (dtlocationdetails.Rows.Count > 1)
                                    {
                                        if (locationtype == "Block")
                                        {
                                            locationcode += loccode + "','";
                                            loctype += type + "','";
                                            locname += lname + "','";
                                        }
                                        else if (locationtype == "Gram Panchayat")
                                        {
                                            locationcode += loccode + "','";
                                            loctype += type + "','";
                                            locname += lname + "','";
                                        }
                                        else if (locationtype == "Village")
                                        {
                                            locationcode += loccode + "','";
                                            loctype += type + "','";
                                            locname += lname + "','";
                                        }
                                    }
                                    else
                                    {
                                        locationcode += loccode;
                                        loctype += type;
                                        locname += lname;
                                    }
                                }

                                if (type == "BS")
                                {
                                    cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE mb.LocationCode IN ('" + locationcode + "') OR mg.LocationCode IN ('" + locationcode + "') OR mv.LocationCode IN ('" + locationcode + "')");
                                    DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtrespondentdetails.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dtrespondentdetails.Rows)
                                        {
                                            survey_bene survey = new survey_bene();
                                            survey.resondantcode = dr["ResondantCode"].ToString();
                                            survey.respondantname = dr["RespondantName"].ToString();
                                            survey.hohname = dr["HohName"].ToString();
                                            survey.relationwithhoh = dr["RelationWithHoh"].ToString();

                                            string DateOfBirth = dr["DateOfBirth"].ToString();
                                            if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
                                            {
                                                DateTime dtDateOfBirth = Convert.ToDateTime(dr["DateOfBirth"].ToString());
                                                survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                survey.dateofbirth = dr["DateOfBirth"].ToString();
                                            }
                                            survey.gender = dr["Gender"].ToString();
                                            survey.idtype = dr["IdType"].ToString();
                                            survey.idnumber = dr["IdNumber"].ToString();
                                            survey.buildingcode = dr["buildingcode"].ToString();
                                            survey.housecode = dr["HouseCode"].ToString();
                                            survey.blockname = dr["blockname"].ToString();
                                            survey.gpname = dr["gpname"].ToString();
                                            survey.village = dr["village"].ToString();
                                            survey.occupation = dr["Occupation"].ToString();
                                            survey.buildingtype = dr["BuildingType"].ToString();
                                            surveybenelist.Add(survey);
                                        }
                                    }
                                }
                                else
                                {
                                    cmd = new SqlCommand("SELECT tbl_TRN_MileStone.ConceptualizedInterventionCode, tbl_TRN_WorkItem.WorkItemCode FROM   tbl_TRN_WorkItem INNER JOIN   tbl_TRN_MileStone ON tbl_TRN_WorkItem.MileStoneCode = tbl_TRN_MileStone.MileStoneCode  WHERE (tbl_TRN_WorkItem.WorkItemCode = @wicode)");
                                    cmd.Parameters.Add("@wicode", workitemcode);
                                    DataTable dtmilestone = vdm.SelectQuery(cmd).Tables[0];

                                    string InterventionCode = "";
                                    if (dtmilestone.Rows.Count > 0)
                                    {
                                        foreach (DataRow drm in dtmilestone.Rows)
                                        {
                                            InterventionCode = drm["ConceptualizedInterventionCode"].ToString();
                                        }
                                    }

                                    cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE mb.LocationCode IN ('" + locationcode + "') OR mg.LocationCode IN ('" + locationcode + "') OR mv.LocationCode IN ('" + locationcode + "')");
                                    //cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id LEFT OUTER JOIN  tbl_MMP_SurveyBeneficiary AS SB ON tbl_MST_Respondant.ResondantCode = SB.RespondantCode WHERE mb.LocationCode IN ('" + locationcode + "') AND (SB.BeneficiaryListing IN ('" + empid + "') OR SB.BeneficiaryListing IS NULL) OR mg.LocationCode IN ('" + locationcode + "') AND (SB.BeneficiaryListing IN ('" + empid + "') OR SB.BeneficiaryListing IS NULL) OR mv.LocationCode IN ('" + locationcode + "') AND (SB.BeneficiaryListing IN ('" + empid + "') OR SB.BeneficiaryListing IS NULL)");
                                    //cmd.CommandTimeout = 10000;
                                    DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtrespondentdetails.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dtrespondentdetails.Rows)
                                        {
                                            string ResondantCode = dr["ResondantCode"].ToString();
                                            survey_bene survey = new survey_bene();
                                            survey.resondantcode = dr["ResondantCode"].ToString();
                                            survey.respondantname = dr["RespondantName"].ToString();
                                            survey.hohname = dr["HohName"].ToString();
                                            survey.relationwithhoh = dr["RelationWithHoh"].ToString();

                                            string DateOfBirth = dr["DateOfBirth"].ToString();
                                            if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
                                            {
                                                DateTime dtDateOfBirth = Convert.ToDateTime(dr["DateOfBirth"].ToString());
                                                survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                survey.dateofbirth = dr["DateOfBirth"].ToString();
                                            }
                                            survey.gender = dr["Gender"].ToString();
                                            survey.idtype = dr["IdType"].ToString();
                                            survey.idnumber = dr["IdNumber"].ToString();
                                            survey.buildingcode = dr["buildingcode"].ToString();
                                            survey.housecode = dr["HouseCode"].ToString();
                                            survey.blockname = dr["blockname"].ToString();
                                            survey.gpname = dr["gpname"].ToString();
                                            survey.village = dr["village"].ToString();
                                            survey.occupation = dr["Occupation"].ToString();
                                            survey.buildingtype = dr["BuildingType"].ToString();
                                            surveybenelist.Add(survey);
                                        }
                                    }
                                }
                                if (type == "BS")
                                {
                                    postcmd = new NpgsqlCommand("SELECT distinct buildingcode, ST_AsText(geom) as geom FROM buildings_final WHERE lower(block_name) IN ('" + locname + "') OR lower(panchayat_name) IN ('" + locname + "') OR lower(village_name) IN ('" + locname + "')");
                                    DataTable dtbuildinggeom = postvdm.SelectQuery(postcmd).Tables[0];
                                    if (dtbuildinggeom.Rows.Count > 0)
                                    {
                                        int i = 0;
                                        var myList = new List<string>();
                                        SortedList fslist = new SortedList();
                                        foreach (DataRow drb in dtbuildinggeom.Rows)
                                        {
                                            string colunname = drb["buildingcode"].ToString();
                                            string geom = drb["geom"].ToString();

                                            // Adding pairs to fslist 
                                            fslist.Add("" + colunname + "", geom);

                                        }
                                        myItems.Add(fslist);
                                    }
                                }
                                else
                                {
                                    //buildingcode
                                    postcmd = new NpgsqlCommand("SELECT distinct buildingcode, ST_AsText(geom) as geom FROM buildings_final WHERE lower(block_name) IN ('" + locname + "') OR lower(panchayat_name) IN ('" + locname + "') OR lower(village_name) IN ('" + locname + "')");
                                    DataTable dtbuildinggeom = postvdm.SelectQuery(postcmd).Tables[0];
                                    if (dtbuildinggeom.Rows.Count > 0)
                                    {
                                        int i = 0;
                                        var myList = new List<string>();
                                        SortedList fslist = new SortedList();
                                        foreach (DataRow drb in dtbuildinggeom.Rows)
                                        {
                                            string colunname = drb["buildingcode"].ToString();
                                            string geom = drb["geom"].ToString();

                                            // Adding pairs to fslist 
                                            fslist.Add("" + colunname + "", geom);

                                        }
                                        myItems.Add(fslist);
                                    }
                                }

                                postcmd = new NpgsqlCommand("SELECT road_code, ST_AsText(geom) as geom FROM village_roads where lower(layer) IN ('" + locname + "') OR lower(line_gpnam) IN ('" + locname + "')");
                                DataTable dtvillage = postvdm.SelectQuery(postcmd).Tables[0];
                                if (dtvillage.Rows.Count > 0)
                                {
                                    foreach (DataRow drb in dtvillage.Rows)
                                    {
                                        roadarray_vill newvill = new roadarray_vill();
                                        string raodcode = drb["road_code"].ToString();
                                        string geom = drb["geom"].ToString();
                                        newvill.road_code = raodcode;
                                        newvill.st_astext = geom;
                                        roadarrayvilllist.Add(newvill);
                                    }
                                }


                                postcmd = new NpgsqlCommand("SELECT buildingtypename, st_astext(st_centroid(geom)) as geom FROM buildings_final WHERE (lower(block_name) IN ('" + locname + "') OR lower(panchayat_name) IN ('" + locname + "') OR lower(village_name) IN ('" + locname + "')) AND  buildingtypename in ('PUBLIC TOILETS','PHC','CHC','SUB CENTRE','PANCHAYAT BHAWAN','POST OFFICE','POLICE OFFICE','VENTERINERY HOSPITAL','RELIGIOUS BUILDING','CEMETERY & BURIAL GROUND','ELECTRICITY STATION''GOVT. SCHOOL','PVT SCHOOL')");
                                DataTable dtbuildingtypegeom = postvdm.SelectQuery(postcmd).Tables[0];
                                if (dtbuildingtypegeom.Rows.Count > 0)
                                {
                                    int i = 0;
                                    foreach (DataRow drb in dtbuildingtypegeom.Rows)
                                    {
                                        string colunname = drb["buildingtypename"].ToString();
                                        string geom = drb["geom"].ToString();
                                        govtbuildings newgovt = new govtbuildings();
                                        newgovt.type = colunname;
                                        newgovt.geom = geom;
                                        govtbuildingslist.Add(newgovt);
                                    }
                                }

                                postcmd = new NpgsqlCommand("select  village_name, st_astext(geom) AS geom from village_masters");
                                DataTable dtvillageBOUNDRIES = postvdm.SelectQuery(postcmd).Tables[0];
                                if (dtvillageBOUNDRIES.Rows.Count > 0)
                                {
                                    foreach (DataRow drb in dtvillageBOUNDRIES.Rows)
                                    {
                                        roadarray_vill newvill = new roadarray_vill();
                                        string raodcode = drb["village_name"].ToString();
                                        string geom = drb["geom"].ToString();
                                        newvill.villagename = raodcode;
                                        newvill.st_astext = geom;
                                        roadarrayvillboundries.Add(newvill);
                                    }
                                }

                                if (mappingcode != "" && mappingcode != null && mappingcode != "null")
                                {
                                    cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
                                    cmd.Parameters.Add("@swimc", mappingcode);
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
                                            mappingcode = dr["SurveyWorkItemMappingCode"].ToString();
                                        }
                                    }
                                    cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
                                    cmd.Parameters.Add("@swimc", mappingcode);
                                }
                                DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];
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
                                syncst.saved = savedcount.ToString();
                                syncst.submitted = submitedcount.ToString();
                                syncst.synced = submitedcount.ToString();
                                statuslist.Add(syncst);
                            }
                            List<downloadbenf> getdownloadbenfdtls = new List<downloadbenf>();
                            downloadbenf getoverDatas = new downloadbenf();
                            getoverDatas.survey_bene = surveybenelist;
                            getoverDatas.buildinggeom = myItems;
                            getoverDatas.roadarray_vill = roadarrayvilllist;
                            getoverDatas.roadarray_villBOUNDRIES = roadarrayvillboundries;
                            getoverDatas.buildingtypegeom = govtbuildingslist;
                            getoverDatas.status = statuslist;
                            getdownloadbenfdtls.Add(getoverDatas);

                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            string response = jsonSerializer.Serialize(getdownloadbenfdtls);
                            Context.Response.Clear();
                            Context.Response.ContentType = "application/json";
                            Context.Response.AddHeader("content-length", response.Length.ToString());
                            Context.Response.Flush();
                            Context.Response.Write(response);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            string msg = "Please Privede The Work Item";
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            Context.Response.Write(jsonSerializer.Serialize(msg));
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
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void benficiarystatusdownloadsave(string workitemcode, string surveyworkitemmappingcode, string empid)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();
            string mappingcode = surveyworkitemmappingcode;
            string workitem = workitemcode;
            string empcode = empid;

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
                        cmd = new SqlCommand("SELECT  SurveyCode, RespondantCode, InterventionCode, BeneficiaryListing, UploadedFrom FROM  tbl_MMP_SurveyBeneficiary WHERE  (SurveyCode = @swmpcode) AND (InterventionCode=@InterventionCode)");
                        cmd.Parameters.Add("@swmpcode", surveyworkitemmappingcode);
                        cmd.Parameters.Add("@InterventionCode", InterventionCode);
                        DataTable dtbenestatus = vdm.SelectQuery(cmd).Tables[0];
                        List<bene_status> surveybenelist = new List<bene_status>();
                        if (dtbenestatus.Rows.Count > 0)
                        {
                            foreach (DataRow drb in dtbenestatus.Rows)
                            {
                                bene_status survey = new bene_status();
                                survey.surveyworkitemmappingcode = drb["SurveyCode"].ToString();
                                survey.respondantcode = drb["RespondantCode"].ToString();
                                survey.InterventionCode = drb["InterventionCode"].ToString();
                                survey.UploadedFrom = drb["UploadedFrom"].ToString();
                                surveybenelist.Add(survey);
                            }
                        }
                        List<selectbenflift> getdownloadbenfdtls = new List<selectbenflift>();
                        selectbenflift getoverDatas = new selectbenflift();
                        getoverDatas.bene_status = surveybenelist;
                        getdownloadbenfdtls.Add(getoverDatas);
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getdownloadbenfdtls);
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

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void get(string empid, string workitemcode, string uuid, List<string> blocks, List<string> villages, List<string> gps)
    {
        try
        {
            vdm = new SalesDBManager();
            postvdm = new SAPdbmanger();

            //added by naveeen 
            string token = System.Web.HttpContext.Current.Request.Headers["token"];
            string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
            string huuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
            //end

            cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM            tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
            cmd.Parameters.Add("@empcode", employecode);
            cmd.Parameters.Add("@token", token);
            cmd.Parameters.Add("@uuid", huuid);
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
                        cmd.Parameters.Add("@duuid", huuid);
                        cmd.Parameters.Add("@active", true);
                        cmd.Parameters.Add("@extenddate", extendsessionexpdate);
                        vdm.Update(cmd);

                        string locationcode = "";
                        string selectlocationcode = "";
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            string loccode = blocks[i].ToString();
                            selectlocationcode += loccode + "','";
                        }
                        for (int i = 0; i < villages.Count; i++)
                        {
                            string loccode = villages[i].ToString();
                            selectlocationcode += loccode + "','";
                        }
                        for (int i = 0; i < gps.Count; i++)
                        {
                            string loccode = gps[i].ToString();
                            selectlocationcode += loccode + "','";
                        }
                        string mappingcode = workitemcode;
                        string type = "MS";

                        if (workitemcode != "" && workitemcode != null && workitemcode != "null")
                        {
                            cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.LocationType, tbl_MST_Location.Label, tbl_MST_Location.CreatedBy, tbl_MST_Location.CreatedOn,  tbl_MST_Location.ModifiedBy, tbl_MST_Location.ModifiedOn, tbl_MST_Location.IsActive, tbl_MST_Location.ParentLocationCode, tbl_MST_Location.ParentLocCode, tbl_MMP_WorkItemLocation.RowCode,   tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MMP_WorkItemLocation.LocationCode AS Expr1, tbl_MMP_WorkItemLocation.CreatedBy AS Expr2, tbl_MMP_WorkItemLocation.CreatedOn AS Expr3,  tbl_MMP_WorkItemLocation.ModifiedBy AS Expr4, tbl_MMP_WorkItemLocation.ModifiedOn AS Expr5 FROM  tbl_MST_Location INNER JOIN tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode where tbl_MMP_WorkItemLocation.WorkItemCode=@wcode AND (tbl_MST_Location.LocationCode IN ('" + selectlocationcode + "'))");
                            cmd.Parameters.Add("@wcode", workitemcode);
                            DataTable dtlocationdetails = vdm.SelectQuery(cmd).Tables[0];
                            List<roadarray_vill> roadarrayvilllist = new List<roadarray_vill>();
                            List<survey_bene> surveybenelist = new List<survey_bene>();
                            List<buildinggeom> buildinggeomlist = new List<buildinggeom>();
                            List<status> statuslist = new List<status>();
                            List<govtbuildings> govtbuildingslist = new List<govtbuildings>();
                            List<roadarray_vill> roadarrayvillboundries = new List<roadarray_vill>();
                            ArrayList myItems = new ArrayList();
                            if (dtlocationdetails.Rows.Count > 0)
                            {
                                locationcode = "";
                                string buildingcode = "";
                                string loctype = "";
                                string locname = "";

                                foreach (DataRow dr in dtlocationdetails.Rows)
                                {
                                    string loccode = dr["LocationCode"].ToString();
                                    string locationtype = dr["LocationType"].ToString();
                                    string LocationName = dr["LocationName"].ToString();
                                    string lname = LocationName.ToLower();
                                    if (dtlocationdetails.Rows.Count > 1)
                                    {
                                        if (locationtype == "Block")
                                        {
                                            locationcode += loccode + "','";
                                            loctype += type + "','";
                                            locname += lname + "','";
                                        }
                                        else if (locationtype == "Gram Panchayat")
                                        {
                                            locationcode += loccode + "','";
                                            loctype += type + "','";
                                            locname += lname + "','";
                                        }
                                        else if (locationtype == "Village")
                                        {
                                            locationcode += loccode + "','";
                                            loctype += type + "','";
                                            locname += lname + "','";
                                        }
                                    }
                                    else
                                    {
                                        locationcode += loccode;
                                        loctype += type;
                                        locname += lname;
                                    }
                                }

                                if (type == "BS")
                                {
                                    cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE mb.LocationCode IN ('" + locationcode + "') OR mg.LocationCode IN ('" + locationcode + "') OR mv.LocationCode IN ('" + locationcode + "')");
                                    DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtrespondentdetails.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dtrespondentdetails.Rows)
                                        {
                                            survey_bene survey = new survey_bene();
                                            survey.resondantcode = dr["ResondantCode"].ToString();
                                            survey.respondantname = dr["RespondantName"].ToString();
                                            survey.hohname = dr["HohName"].ToString();
                                            survey.relationwithhoh = dr["RelationWithHoh"].ToString();

                                            string DateOfBirth = dr["DateOfBirth"].ToString();
                                            if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
                                            {
                                                DateTime dtDateOfBirth = Convert.ToDateTime(dr["DateOfBirth"].ToString());
                                                survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
                                            }
                                            else
                                            {
                                                survey.dateofbirth = dr["DateOfBirth"].ToString();
                                            }
                                            survey.gender = dr["Gender"].ToString();
                                            survey.idtype = dr["IdType"].ToString();
                                            survey.idnumber = dr["IdNumber"].ToString();
                                            survey.buildingcode = dr["buildingcode"].ToString();
                                            survey.housecode = dr["HouseCode"].ToString();
                                            survey.blockname = dr["blockname"].ToString();
                                            survey.gpname = dr["gpname"].ToString();
                                            survey.village = dr["village"].ToString();
                                            survey.occupation = dr["Occupation"].ToString();
                                            survey.buildingtype = dr["BuildingType"].ToString();
                                            surveybenelist.Add(survey);
                                        }
                                    }
                                }
                                else
                                {
                                    cmd = new SqlCommand("SELECT tbl_TRN_MileStone.ConceptualizedInterventionCode, tbl_TRN_WorkItem.WorkItemCode FROM   tbl_TRN_WorkItem INNER JOIN   tbl_TRN_MileStone ON tbl_TRN_WorkItem.MileStoneCode = tbl_TRN_MileStone.MileStoneCode  WHERE (tbl_TRN_WorkItem.WorkItemCode = @wicode)");
                                    cmd.Parameters.Add("@wicode", workitemcode);
                                    DataTable dtmilestone = vdm.SelectQuery(cmd).Tables[0];
                                    string InterventionCode = "";
                                    string serveyworkitemmappingcode = "";
                                    if (dtmilestone.Rows.Count > 0)
                                    {
                                        foreach (DataRow drm in dtmilestone.Rows)
                                        {
                                            InterventionCode = drm["ConceptualizedInterventionCode"].ToString();

                                        }
                                    }
                                    postcmd = new NpgsqlCommand("select  village_name, st_astext(geom) AS geom from village_masters");
                                    DataTable dtvillageBOUNDRIES = postvdm.SelectQuery(postcmd).Tables[0];

                                    postcmd = new NpgsqlCommand("SELECT distinct buildingcode, hhcode, ST_AsText(geom) as geom FROM buildings_final WHERE lower(block_name) IN ('" + locname + "') OR lower(panchayat_name) IN ('" + locname + "') OR lower(village_name) IN ('" + locname + "')");
                                    DataTable dtbuildinggeom = postvdm.SelectQuery(postcmd).Tables[0];

                                    cmd = new SqlCommand("SELECT  SurveyWorkItemMappingCode, SurveyCode, WorkItemCode, LocationCode, Status, isActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Frequncy, StartDate, EndDDate FROM   tbl_MMP_SurveyWorkItem WHERE  (WorkItemCode = @workitem)");
                                    cmd.Parameters.Add("@workitem", workitemcode);
                                    DataTable dtsurveyworkitemdtls = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtsurveyworkitemdtls.Rows.Count > 0)
                                    {
                                        foreach (DataRow drs in dtsurveyworkitemdtls.Rows)
                                        {
                                            serveyworkitemmappingcode = drs["SurveyWorkItemMappingCode"].ToString();
                                        }
                                    }

                                    cmd = new SqlCommand("SELECT  Id, SurveyCode, RespondantCode, BeneficiaryListing, UploadedFrom FROM tbl_MMP_SurveyBeneficiary WHERE (SurveyCode = @mappingcode) AND (InterventionCode = @InterventionCode)");
                                    cmd.Parameters.Add("@mappingcode", serveyworkitemmappingcode);
                                    cmd.Parameters.Add("@InterventionCode", InterventionCode);
                                    DataTable dtmapping = vdm.SelectQuery(cmd).Tables[0];


                                    cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE mb.LocationCode IN ('" + locationcode + "') OR mg.LocationCode IN ('" + locationcode + "') OR mv.LocationCode IN ('" + locationcode + "')");
                                    DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
                                    if (dtrespondentdetails.Rows.Count > 0)
                                    {
                                        foreach (DataRow dr in dtrespondentdetails.Rows)
                                        {
                                            string mystrval = "";
                                            string ResondantCode = dr["ResondantCode"].ToString();
                                            foreach (DataRow drms in dtmapping.Select("RespondantCode='" + ResondantCode + "'"))
                                            {
                                                mystrval = drms["RespondantCode"].ToString();
                                            }
                                            if (mystrval != "")
                                            {

                                            }
                                            else
                                            {
                                                survey_bene survey = new survey_bene();
                                                survey.resondantcode = dr["ResondantCode"].ToString();
                                                survey.respondantname = dr["RespondantName"].ToString();
                                                survey.hohname = dr["HohName"].ToString();
                                                survey.relationwithhoh = dr["RelationWithHoh"].ToString();

                                                string DateOfBirth = dr["DateOfBirth"].ToString();
                                                if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
                                                {
                                                    DateTime dtDateOfBirth = Convert.ToDateTime(dr["DateOfBirth"].ToString());
                                                    survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
                                                }
                                                else
                                                {
                                                    survey.dateofbirth = dr["DateOfBirth"].ToString();
                                                }
                                                survey.gender = dr["Gender"].ToString();
                                                survey.idtype = dr["IdType"].ToString();
                                                survey.idnumber = dr["IdNumber"].ToString();
                                                survey.buildingcode = dr["buildingcode"].ToString();
                                                survey.housecode = dr["HouseCode"].ToString();
                                                string housecode = dr["HouseCode"].ToString();
                                                survey.blockname = dr["blockname"].ToString();
                                                survey.gpname = dr["gpname"].ToString();
                                                survey.village = dr["village"].ToString();
                                                string villagename = dr["village"].ToString();
                                                foreach (DataRow drb in dtbuildinggeom.Select("hhcode='" + housecode + "'"))
                                                {
                                                    string hcodegeom = drb["geom"].ToString();
                                                    survey.geom = hcodegeom;
                                                }
                                                survey.occupation = dr["Occupation"].ToString();
                                                survey.buildingtype = dr["BuildingType"].ToString();
                                                surveybenelist.Add(survey);
                                            }
                                        }
                                    }
                                }
                            }
                            List<newdownloadbenf> getdownloadbenfdtls = new List<newdownloadbenf>();
                            newdownloadbenf getoverDatas = new newdownloadbenf();
                            getoverDatas.survey_bene = surveybenelist;
                            getdownloadbenfdtls.Add(getoverDatas);
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            string response = jsonSerializer.Serialize(getdownloadbenfdtls);
                            Context.Response.Clear();
                            Context.Response.ContentType = "application/json";
                            Context.Response.AddHeader("content-length", response.Length.ToString());
                            Context.Response.Flush();
                            Context.Response.Write(response);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            string msg = "Please Privede The Work Item";
                            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                            jsonSerializer.MaxJsonLength = Int32.MaxValue;
                            Context.Response.Write(jsonSerializer.Serialize(msg));
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
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }


    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void getbenficiarylist(string empid, string workitemcode, string uuid, List<string> blocks, List<string> villages, List<string> gps)
    //{
    //    try
    //    {
    //        vdm = new SalesDBManager();
    //        postvdm = new SAPdbmanger();

    //        //added by naveeen 
    //        string token = System.Web.HttpContext.Current.Request.Headers["token"];
    //        string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
    //        string huuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
    //        //end

    //        cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM            tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
    //        cmd.Parameters.Add("@empcode", employecode);
    //        cmd.Parameters.Add("@token", token);
    //        cmd.Parameters.Add("@uuid", huuid);
    //        cmd.Parameters.Add("@IsActive", true);
    //        DataTable dttoken = vdm.SelectQuery(cmd).Tables[0];

    //        if (dttoken.Rows.Count > 0)
    //        {
    //            foreach (DataRow drt in dttoken.Rows)
    //            {
    //                string sessionexpdate = drt["SessionExpiryTime"].ToString();
    //                DateTime dtsessionexpdate = Convert.ToDateTime(sessionexpdate);
    //                DateTime nowdatetime = DateTime.Now;
    //                if (nowdatetime > dtsessionexpdate)
    //                {
    //                    Context.Response.Clear();
    //                    Context.Response.StatusCode = 401;
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();

    //                }
    //                else
    //                {
    //                    string GPlocationcode = "";
    //                    string BLOClocationcode = "";
    //                    string type = "MS";
    //                    for (int i = 0; i < gps.Count; i++)
    //                    {
    //                        string loccode = gps[i].ToString();
    //                        GPlocationcode += loccode + "','";
    //                    }

    //                    for (int i = 0; i < blocks.Count; i++)
    //                    {
    //                        string Bloccode = gps[i].ToString();
    //                        BLOClocationcode += Bloccode + "','";
    //                    }
    //                    string locationcode = "";
    //                    string buildingcode = "";
    //                    string loctype = "";
    //                    string locname = "";
    //                    if (workitemcode != "" && workitemcode != null && workitemcode != "null")
    //                    {
    //                        List<roadarray_vill> roadarrayvilllist = new List<roadarray_vill>();
    //                        List<survey_bene> surveybenelist = new List<survey_bene>();
    //                        List<buildinggeom> buildinggeomlist = new List<buildinggeom>();
    //                        List<status> statuslist = new List<status>();
    //                        List<govtbuildings> govtbuildingslist = new List<govtbuildings>();
    //                        List<roadarray_vill> roadarrayvillboundries = new List<roadarray_vill>();
    //                        ArrayList myItems = new ArrayList();

                            

    //                        DataTable Report = new DataTable();
    //                        Report.Columns.Add("VillageCode");
    //                        Report.Columns.Add("VillageName");
    //                        Report.Columns.Add("GPcode");
    //                        cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.ParentLocCode, tbl_MST_Location.LocationType FROM    tbl_MST_Location INNER JOIN   tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @wcode)");
    //                        cmd.Parameters.Add("@wcode", workitemcode);
    //                        DataTable dtblocks = vdm.SelectQuery(cmd).Tables[0];
    //                        foreach (DataRow dr in dtblocks.Rows)
    //                        {
    //                            string locationtype = dr["LocationType"].ToString();
    //                            if (locationtype == "Village")
    //                            {
    //                                DataRow newrow = Report.NewRow();
    //                                newrow["VillageCode"] = dr["LocationCode"].ToString();
    //                                newrow["VillageName"] = dr["LocationName"].ToString();
    //                                newrow["GPcode"] = dr["ParentLocCode"].ToString();
    //                                Report.Rows.Add(newrow);
    //                            }
    //                            if (locationtype == "Gram Panchayat")
    //                            {
    //                                string parentcode = dr["ParentLocCode"].ToString();
    //                                cmd = new SqlCommand("SELECT DISTINCT tbl_MST_Location_1.LocationCode, tbl_MST_Location_1.LocationName, tbl_MST_Location_1.LocationType, tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MST_Location_1.ParentLocCode FROM            tbl_MST_Location INNER JOIN  tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode INNER JOIN  tbl_MST_Location AS tbl_MST_Location_1 ON tbl_MMP_WorkItemLocation.LocationCode = tbl_MST_Location_1.ParentLocCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @workitemcode) AND (tbl_MST_Location_1.ParentLocCode IN ('" + GPlocationcode + "')) AND (tbl_MST_Location_1.LocationType = 'Village')");
    //                                cmd.Parameters.Add("@workitemcode", workitemcode);
    //                                DataTable dtgpblocks = vdm.SelectQuery(cmd).Tables[0];
    //                                if (dtgpblocks.Rows.Count > 0)
    //                                {
    //                                    foreach (DataRow drgpb in dtgpblocks.Rows)
    //                                    {
    //                                        DataRow newrow = Report.NewRow();
    //                                        newrow["VillageCode"] = drgpb["LocationCode"].ToString();
    //                                        newrow["VillageName"] = drgpb["LocationName"].ToString();
    //                                        newrow["GPcode"] = drgpb["ParentLocCode"].ToString();
    //                                        Report.Rows.Add(newrow);
    //                                    }
    //                                }
    //                            }
    //                            if (locationtype == "Block")
    //                            {
    //                                cmd = new SqlCommand("SELECT DISTINCT tbl_MST_Location_1.LocationCode, tbl_MST_Location_1.LocationName, tbl_MST_Location_1.LocationType, tbl_MMP_WorkItemLocation.WorkItemCode FROM            tbl_MST_Location INNER JOIN tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode INNER JOIN tbl_MST_Location AS tbl_MST_Location_1 ON tbl_MMP_WorkItemLocation.LocationCode = tbl_MST_Location_1.ParentLocCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @workitemcode) AND (tbl_MST_Location_1.ParentLocCode IN ('" + BLOClocationcode + "')) AND (tbl_MST_Location_1.LocationCode IN ('" + GPlocationcode + "'))");
    //                                cmd.Parameters.Add("@workitemcode", workitemcode);
    //                                DataTable dtvblocks = vdm.SelectQuery(cmd).Tables[0];
    //                                if (dtvblocks.Rows.Count > 0)
    //                                {
    //                                    foreach (DataRow drvb in dtvblocks.Rows)
    //                                    {
    //                                        string vgloccode = drvb["LocationCode"].ToString();
    //                                        GPlocationcode += vgloccode + "','";
    //                                    }
    //                                    cmd = new SqlCommand("SELECT DISTINCT tbl_MST_Location_1.LocationCode, tbl_MST_Location_1.LocationName, tbl_MST_Location_1.LocationType, tbl_MST_Location_1.ParentLocCode, tbl_MMP_WorkItemLocation.WorkItemCode FROM            tbl_MST_Location INNER JOIN  tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode INNER JOIN  tbl_MST_Location AS tbl_MST_Location_1 ON tbl_MMP_WorkItemLocation.LocationCode = tbl_MST_Location_1.ParentLocCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @workitemcode) AND (tbl_MST_Location_1.ParentLocCode IN ('" + GPlocationcode + "')) AND (tbl_MST_Location_1.LocationType = 'Village')");
    //                                    cmd.Parameters.Add("@workitemcode", workitemcode);
    //                                    DataTable dtgpblocks = vdm.SelectQuery(cmd).Tables[0];
    //                                    if (dtgpblocks.Rows.Count > 0)
    //                                    {
    //                                        foreach (DataRow drgpb in dtgpblocks.Rows)
    //                                        {
    //                                            DataRow newrow = Report.NewRow();
    //                                            newrow["VillageCode"] = drgpb["LocationCode"].ToString();
    //                                            newrow["VillageName"] = drgpb["LocationName"].ToString();
    //                                            newrow["GPcode"] = drgpb["ParentLocCode"].ToString();
    //                                            Report.Rows.Add(newrow);
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        string selectedvillages = "";
    //                        if (Report.Rows.Count > 0)
    //                        {
    //                            for (int i = 0; i < villages.Count; i++)
    //                            {
    //                                string villcode = villages[i].ToString();
    //                                foreach (DataRow drms in Report.Select("VillageCode='" + villcode + "'"))
    //                                {
    //                                    selectedvillages += villcode + "','";
    //                                }
    //                            }
    //                        }


    //                        cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.LocationType, tbl_MST_Location.Label, tbl_MST_Location.CreatedBy, tbl_MST_Location.CreatedOn,  tbl_MST_Location.ModifiedBy, tbl_MST_Location.ModifiedOn, tbl_MST_Location.IsActive, tbl_MST_Location.ParentLocationCode, tbl_MST_Location.ParentLocCode, tbl_MMP_WorkItemLocation.RowCode,   tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MMP_WorkItemLocation.LocationCode AS Expr1, tbl_MMP_WorkItemLocation.CreatedBy AS Expr2, tbl_MMP_WorkItemLocation.CreatedOn AS Expr3,  tbl_MMP_WorkItemLocation.ModifiedBy AS Expr4, tbl_MMP_WorkItemLocation.ModifiedOn AS Expr5 FROM  tbl_MST_Location INNER JOIN tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode where tbl_MMP_WorkItemLocation.WorkItemCode=@wcode AND (tbl_MST_Location.LocationCode IN ('" + selectedvillages + "'))");
    //                        cmd.Parameters.Add("@wcode", workitemcode);
    //                        DataTable dtlocationdetails = vdm.SelectQuery(cmd).Tables[0];
    //                        foreach (DataRow dr in dtlocationdetails.Rows)
    //                        {
    //                            string loccode = dr["LocationCode"].ToString();
    //                            string locationtype = dr["LocationType"].ToString();
    //                            string LocationName = dr["LocationName"].ToString();
    //                            string lname = LocationName.ToLower();
    //                            if (dtlocationdetails.Rows.Count > 1)
    //                            {

    //                                if (locationtype == "Block")
    //                                {
    //                                    locationcode += loccode + "','";
    //                                    loctype += type + "','";
    //                                    locname += lname + "','";
    //                                }
    //                                else if (locationtype == "Gram Panchayat")
    //                                {
    //                                    locationcode += loccode + "','";
    //                                    loctype += type + "','";
    //                                    locname += lname + "','";
    //                                }
    //                                else if (locationtype == "Village")
    //                                {
    //                                    locationcode += loccode + "','";
    //                                    loctype += type + "','";
    //                                    locname += lname + "','";
    //                                }
    //                            }
    //                            else
    //                            {
    //                                locationcode += loccode;
    //                                loctype += type;
    //                                locname += lname;
    //                            }
    //                        }



    //                        if (type == "BS")
    //                        {
    //                            cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE  mv.LocationCode IN ('" + selectedvillages + "')");
    //                            DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtrespondentdetails.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow dr in dtrespondentdetails.Rows)
    //                                {
    //                                    survey_bene survey = new survey_bene();
    //                                    survey.resondantcode = dr["ResondantCode"].ToString();
    //                                    survey.respondantname = dr["RespondantName"].ToString();
    //                                    survey.hohname = dr["HohName"].ToString();
    //                                    survey.relationwithhoh = dr["RelationWithHoh"].ToString();

    //                                    string DateOfBirth = dr["DateOfBirth"].ToString();
    //                                    if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
    //                                    {
    //                                        DateTime dtDateOfBirth = Convert.ToDateTime(dr["DateOfBirth"].ToString());
    //                                        survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                                    }
    //                                    else
    //                                    {
    //                                        survey.dateofbirth = dr["DateOfBirth"].ToString();
    //                                    }
    //                                    survey.gender = dr["Gender"].ToString();
    //                                    survey.idtype = dr["IdType"].ToString();
    //                                    survey.idnumber = dr["IdNumber"].ToString();
    //                                    survey.buildingcode = dr["buildingcode"].ToString();
    //                                    survey.housecode = dr["HouseCode"].ToString();
    //                                    survey.blockname = dr["blockname"].ToString();
    //                                    survey.gpname = dr["gpname"].ToString();
    //                                    survey.village = dr["village"].ToString();
    //                                    survey.occupation = dr["Occupation"].ToString();
    //                                    survey.buildingtype = dr["BuildingType"].ToString();
    //                                    surveybenelist.Add(survey);
    //                                }
    //                            }
    //                        }
    //                        else
    //                        {
    //                            cmd = new SqlCommand("SELECT tbl_TRN_MileStone.ConceptualizedInterventionCode, tbl_TRN_WorkItem.WorkItemCode FROM   tbl_TRN_WorkItem INNER JOIN   tbl_TRN_MileStone ON tbl_TRN_WorkItem.MileStoneCode = tbl_TRN_MileStone.MileStoneCode  WHERE (tbl_TRN_WorkItem.WorkItemCode = @wicode)");
    //                            cmd.Parameters.Add("@wicode", workitemcode);
    //                            DataTable dtmilestone = vdm.SelectQuery(cmd).Tables[0];
    //                            string InterventionCode = "";
    //                            string serveyworkitemmappingcode = "";
    //                            if (dtmilestone.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drm in dtmilestone.Rows)
    //                                {
    //                                    InterventionCode = drm["ConceptualizedInterventionCode"].ToString();

    //                                }
    //                            }
    //                            postcmd = new NpgsqlCommand("select  village_name, st_astext(geom) AS geom from village_masters");
    //                            DataTable dtvillageBOUNDRIES = postvdm.SelectQuery(postcmd).Tables[0];

    //                            postcmd = new NpgsqlCommand("SELECT distinct buildingcode, hhcode, ST_AsText(geom) as geom FROM buildings_final WHERE lower(block_name) IN ('" + locname + "') OR lower(panchayat_name) IN ('" + locname + "') OR lower(village_name) IN ('" + locname + "')");
    //                            DataTable dtbuildinggeom = postvdm.SelectQuery(postcmd).Tables[0];

    //                            cmd = new SqlCommand("SELECT  SurveyWorkItemMappingCode, SurveyCode, WorkItemCode, LocationCode, Status, isActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Frequncy, StartDate, EndDDate FROM   tbl_MMP_SurveyWorkItem WHERE  (WorkItemCode = @workitem)");
    //                            cmd.Parameters.Add("@workitem", workitemcode);
    //                            DataTable dtsurveyworkitemdtls = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtsurveyworkitemdtls.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drs in dtsurveyworkitemdtls.Rows)
    //                                {
    //                                    serveyworkitemmappingcode = drs["SurveyWorkItemMappingCode"].ToString();
    //                                }
    //                            }

    //                            cmd = new SqlCommand("SELECT  Id, SurveyCode, RespondantCode, BeneficiaryListing, UploadedFrom FROM tbl_MMP_SurveyBeneficiary WHERE (SurveyCode = @mappingcode) AND (InterventionCode = @InterventionCode)");
    //                            cmd.Parameters.Add("@mappingcode", serveyworkitemmappingcode);
    //                            cmd.Parameters.Add("@InterventionCode", InterventionCode);
    //                            DataTable dtmapping = vdm.SelectQuery(cmd).Tables[0];


    //                            cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE  mv.LocationCode IN ('" + selectedvillages + "')");
    //                            DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtrespondentdetails.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow dr in dtrespondentdetails.Rows)
    //                                {
    //                                    string mystrval = "";
    //                                    string ResondantCode = dr["ResondantCode"].ToString();
    //                                    foreach (DataRow drms in dtmapping.Select("RespondantCode='" + ResondantCode + "'"))
    //                                    {
    //                                        mystrval = drms["RespondantCode"].ToString();
    //                                    }
    //                                    if (mystrval != "")
    //                                    {

    //                                    }
    //                                    else
    //                                    {
    //                                        survey_bene survey = new survey_bene();
    //                                        survey.resondantcode = dr["ResondantCode"].ToString();
    //                                        survey.respondantname = dr["RespondantName"].ToString();
    //                                        survey.hohname = dr["HohName"].ToString();
    //                                        survey.relationwithhoh = dr["RelationWithHoh"].ToString();

    //                                        string DateOfBirth = dr["DateOfBirth"].ToString();
    //                                        if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
    //                                        {
    //                                            DateTime dtDateOfBirth = Convert.ToDateTime(dr["DateOfBirth"].ToString());
    //                                            survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                                        }
    //                                        else
    //                                        {
    //                                            survey.dateofbirth = dr["DateOfBirth"].ToString();
    //                                        }
    //                                        survey.gender = dr["Gender"].ToString();
    //                                        survey.idtype = dr["IdType"].ToString();
    //                                        survey.idnumber = dr["IdNumber"].ToString();
    //                                        survey.buildingcode = dr["buildingcode"].ToString();
    //                                        survey.housecode = dr["HouseCode"].ToString();
    //                                        string housecode = dr["HouseCode"].ToString();
    //                                        survey.blockname = dr["blockname"].ToString();
    //                                        survey.gpname = dr["gpname"].ToString();
    //                                        survey.village = dr["village"].ToString();
    //                                        string villagename = dr["village"].ToString();
    //                                        foreach (DataRow drb in dtbuildinggeom.Select("hhcode='" + housecode + "'"))
    //                                        {
    //                                            string hcodegeom = drb["geom"].ToString();
    //                                            survey.geom = hcodegeom;
    //                                        }
    //                                        survey.occupation = dr["Occupation"].ToString();
    //                                        survey.buildingtype = dr["BuildingType"].ToString();
    //                                        surveybenelist.Add(survey);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        List<newdownloadbenf> getdownloadbenfdtls = new List<newdownloadbenf>();
    //                        newdownloadbenf getoverDatas = new newdownloadbenf();
    //                        getoverDatas.survey_bene = surveybenelist;
    //                        getdownloadbenfdtls.Add(getoverDatas);
    //                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //                        string response = jsonSerializer.Serialize(getdownloadbenfdtls);
    //                        Context.Response.Clear();
    //                        Context.Response.ContentType = "application/json";
    //                        Context.Response.AddHeader("content-length", response.Length.ToString());
    //                        Context.Response.Flush();
    //                        Context.Response.Write(response);
    //                        HttpContext.Current.ApplicationInstance.CompleteRequest();
    //                    }
    //                    else
    //                    {
    //                        string msg = "Please Privede The Work Item";
    //                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //                        Context.Response.Write(jsonSerializer.Serialize(msg));
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Context.Response.Clear();
    //            Context.Response.StatusCode = 401;
    //            HttpContext.Current.ApplicationInstance.CompleteRequest();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Context.Response.Clear();
    //        Context.Response.StatusCode = 500;
    //        HttpContext.Current.ApplicationInstance.CompleteRequest();
    //    }
    //}
    public class blocks
    {
        public string blockcode { get; set; }
        public string blockname { get; set; }
    }

    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void loadblocks(string empid, string workitemcode, string uuid)
    //{
    //    try
    //    {
    //        //added by naveeen 
    //        string token = System.Web.HttpContext.Current.Request.Headers["token"];
    //        string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
    //        string huuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
    //        //end

    //        cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM            tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
    //        cmd.Parameters.Add("@empcode", employecode);
    //        cmd.Parameters.Add("@token", token);
    //        cmd.Parameters.Add("@uuid", huuid);
    //        cmd.Parameters.Add("@IsActive", true);
    //        DataTable dttoken = vdm.SelectQuery(cmd).Tables[0];
    //        if (dttoken.Rows.Count > 0)
    //        {
    //            foreach (DataRow drt in dttoken.Rows)
    //            {
    //                string sessionexpdate = drt["SessionExpiryTime"].ToString();
    //                DateTime dtsessionexpdate = Convert.ToDateTime(sessionexpdate);
    //                DateTime nowdatetime = DateTime.Now;
    //                if (nowdatetime > dtsessionexpdate)
    //                {
    //                    Context.Response.Clear();
    //                    Context.Response.StatusCode = 401;
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();

    //                }
    //                else
    //                {
    //                    DateTime extendsessionexpdate = nowdatetime.AddHours(1);
    //                    cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET SessionExpiryTime=@extenddate WHERE (EmployeeCode = @emplcode) AND (SessionToken = @stoken) AND (DeviceID=@duuid) AND (IsActive=@active)");
    //                    cmd.Parameters.Add("@emplcode", employecode);
    //                    cmd.Parameters.Add("@stoken", token);
    //                    cmd.Parameters.Add("@duuid", huuid);
    //                    cmd.Parameters.Add("@active", true);
    //                    cmd.Parameters.Add("@extenddate", extendsessionexpdate);
    //                    vdm.Update(cmd);

    //                    cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.ParentLocCode, tbl_MST_Location.LocationType FROM    tbl_MST_Location INNER JOIN   tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @wcode)");
    //                    cmd.Parameters.Add("@wcode", workitemcode);
    //                    DataTable dtblocks = vdm.SelectQuery(cmd).Tables[0];
    //                    List<blocks> blockslist = new List<blocks>();
    //                    DataTable Report = new DataTable();
    //                    Report.Columns.Add("Blockcode");
    //                    Report.Columns.Add("BlockName");
    //                    foreach (DataRow dr in dtblocks.Rows)
    //                    {
    //                        string locationtype = dr["LocationType"].ToString();
    //                        if (locationtype == "Village")
    //                        {
    //                            string parentcode = dr["ParentLocCode"].ToString();
    //                            cmd = new SqlCommand("SELECT  LocationCode, LocationName, LocationType, ParentLocationCode, ParentLocCode FROM  tbl_MST_Location WHERE  (LocationCode = @lcode)");
    //                            cmd.Parameters.Add("@lcode", parentcode);
    //                            DataTable dtgps = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtgps.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drgp in dtgps.Rows)
    //                                {
    //                                    string gpparentcode = drgp["ParentLocCode"].ToString();
    //                                    cmd = new SqlCommand("SELECT  LocationCode, LocationName, LocationType, ParentLocationCode, ParentLocCode FROM  tbl_MST_Location WHERE  (LocationCode = @lgcode)");
    //                                    cmd.Parameters.Add("@lgcode", gpparentcode);
    //                                    DataTable dtgpblocks = vdm.SelectQuery(cmd).Tables[0];
    //                                    if (dtgpblocks.Rows.Count > 0)
    //                                    {
    //                                        foreach (DataRow drgpb in dtgpblocks.Rows)
    //                                        {
    //                                            DataRow newrow = Report.NewRow();
    //                                            newrow["Blockcode"] = drgpb["LocationCode"].ToString();
    //                                            newrow["BlockName"] = drgpb["LocationName"].ToString();
    //                                            Report.Rows.Add(newrow);

    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        if (locationtype == "Gram Panchayat")
    //                        {
    //                            string parentcode = dr["ParentLocCode"].ToString();
    //                            cmd = new SqlCommand("SELECT  LocationCode, LocationName, LocationType, ParentLocationCode, ParentLocCode FROM  tbl_MST_Location WHERE  (LocationCode = @lgcode)");
    //                            cmd.Parameters.Add("@lgcode", parentcode);
    //                            DataTable dtgpblocks = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtgpblocks.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drgpb in dtgpblocks.Rows)
    //                                {
    //                                    DataRow newrow = Report.NewRow();
    //                                    newrow["Blockcode"] = drgpb["LocationCode"].ToString();
    //                                    newrow["BlockName"] = drgpb["LocationName"].ToString();
    //                                    Report.Rows.Add(newrow);
    //                                }
    //                            }
    //                        }
    //                        if (locationtype == "Block")
    //                        {
    //                            DataRow newrow = Report.NewRow();
    //                            newrow["Blockcode"] = dr["LocationCode"].ToString();
    //                            newrow["BlockName"] = dr["LocationName"].ToString();
    //                            Report.Rows.Add(newrow);
    //                        }
    //                    }
    //                    if (Report.Rows.Count > 0)
    //                    {
    //                        DataView view = new DataView(Report);
    //                        DataTable dtpostdetails = view.ToTable(true, "Blockcode", "BlockName");
    //                        if (dtpostdetails.Rows.Count > 0)
    //                        {
    //                            foreach (DataRow drd in dtpostdetails.Rows)
    //                            {
    //                                blocks block = new blocks();
    //                                block.blockcode = drd["Blockcode"].ToString();
    //                                block.blockname = drd["BlockName"].ToString();
    //                                blockslist.Add(block);
    //                            }
    //                        }
    //                    }
    //                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //                    string response = jsonSerializer.Serialize(blockslist);
    //                    Context.Response.Clear();
    //                    Context.Response.ContentType = "application/json";
    //                    Context.Response.AddHeader("content-length", response.Length.ToString());
    //                    Context.Response.Flush();
    //                    Context.Response.Write(response);
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Context.Response.Clear();
    //            Context.Response.StatusCode = 401;
    //            HttpContext.Current.ApplicationInstance.CompleteRequest();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Context.Response.Clear();
    //        Context.Response.StatusCode = 401;
    //        HttpContext.Current.ApplicationInstance.CompleteRequest();
    //    }
    //}

    public class gps
    {
        public string gpcode { get; set; }
        public string gpname { get; set; }
        public string blockcode { get; set; }
    }


    


    //[WebMethod(EnableSession = true)]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void loadgps(string empid, string workitemcode, string uuid, List<string> blocks)
    //{
    //    try
    //    {
    //        //added by naveeen 
    //        string token = System.Web.HttpContext.Current.Request.Headers["token"];
    //        string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
    //        string huuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
    //        //end

    //        cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM            tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
    //        cmd.Parameters.Add("@empcode", employecode);
    //        cmd.Parameters.Add("@token", token);
    //        cmd.Parameters.Add("@uuid", huuid);
    //        cmd.Parameters.Add("@IsActive", true);
    //        DataTable dttoken = vdm.SelectQuery(cmd).Tables[0];

    //        if (dttoken.Rows.Count > 0)
    //        {
    //            foreach (DataRow drt in dttoken.Rows)
    //            {
    //                string sessionexpdate = drt["SessionExpiryTime"].ToString();
    //                DateTime dtsessionexpdate = Convert.ToDateTime(sessionexpdate);
    //                DateTime nowdatetime = DateTime.Now;
    //                if (nowdatetime > dtsessionexpdate)
    //                {
    //                    Context.Response.Clear();
    //                    Context.Response.StatusCode = 401;
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //                }
    //                else
    //                {
    //                    DateTime extendsessionexpdate = nowdatetime.AddHours(1);
    //                    cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET SessionExpiryTime=@extenddate WHERE (EmployeeCode = @emplcode) AND (SessionToken = @stoken) AND (DeviceID=@duuid) AND (IsActive=@active)");
    //                    cmd.Parameters.Add("@emplcode", employecode);
    //                    cmd.Parameters.Add("@stoken", token);
    //                    cmd.Parameters.Add("@duuid", huuid);
    //                    cmd.Parameters.Add("@active", true);
    //                    cmd.Parameters.Add("@extenddate", extendsessionexpdate);
    //                    vdm.Update(cmd);

    //                    cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.ParentLocCode, tbl_MST_Location.LocationType FROM    tbl_MST_Location INNER JOIN   tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @wcode)");
    //                    cmd.Parameters.Add("@wcode", workitemcode);
    //                    DataTable dtblocks = vdm.SelectQuery(cmd).Tables[0];
    //                    List<gps> gplist = new List<gps>();
    //                    string locationcode = "";
    //                    for (int i = 0; i < blocks.Count; i++)
    //                    {
    //                        string loccode = blocks[i].ToString();
    //                        locationcode += loccode + "','";
    //                    }
    //                    DataTable Report = new DataTable();
    //                    Report.Columns.Add("gpcode");
    //                    Report.Columns.Add("gpname");
    //                    Report.Columns.Add("blockcode");
    //                    foreach (DataRow dr in dtblocks.Rows)
    //                    {
    //                        string locationtype = dr["LocationType"].ToString();
    //                        if (locationtype == "Village")
    //                        {
    //                            string parentcode = dr["ParentLocCode"].ToString();
    //                            cmd = new SqlCommand("SELECT  LocationCode, LocationName, LocationType, ParentLocationCode, ParentLocCode FROM  tbl_MST_Location WHERE  (LocationCode = @lcode)");
    //                            cmd.Parameters.Add("@lcode", parentcode);
    //                            DataTable dtgps = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtgps.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drgp in dtgps.Rows)
    //                                {
    //                                    string gpparentcode = drgp["ParentLocCode"].ToString();
    //                                    DataRow newrow = Report.NewRow();
    //                                    newrow["gpcode"] = drgp["LocationCode"].ToString();
    //                                    newrow["gpname"] = drgp["LocationName"].ToString();
    //                                    newrow["blockcode"] = gpparentcode;
    //                                    Report.Rows.Add(newrow);
    //                                }
    //                            }
    //                        }
    //                        if (locationtype == "Gram Panchayat")
    //                        {
    //                            DataRow newrow = Report.NewRow();
    //                            newrow["gpcode"] = dr["LocationCode"].ToString();
    //                            newrow["gpname"] = dr["LocationName"].ToString();
    //                            newrow["blockcode"] = dr["ParentLocCode"].ToString();
    //                            Report.Rows.Add(newrow);
    //                        }
    //                        if (locationtype == "Block")
    //                        {
    //                            string loccode = dr["LocationCode"].ToString();
    //                            cmd = new SqlCommand("SELECT DISTINCT tbl_MST_Location_1.LocationCode, tbl_MST_Location_1.LocationName, tbl_MST_Location_1.LocationType, tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MST_Location_1.ParentLocCode  FROM   tbl_MST_Location INNER JOIN   tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode INNER JOIN  tbl_MST_Location AS tbl_MST_Location_1 ON tbl_MMP_WorkItemLocation.LocationCode = tbl_MST_Location_1.ParentLocCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @workitemcode) AND (tbl_MST_Location_1.ParentLocCode IN ('" + locationcode + "')");
    //                            cmd.Parameters.Add("@workitemcode", workitemcode);
    //                            DataTable dtgps = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtgps.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drgp in dtgps.Rows)
    //                                {
    //                                    string gpparentcode = drgp["ParentLocCode"].ToString();
    //                                    DataRow newrow = Report.NewRow();
    //                                    newrow["gpcode"] = drgp["LocationCode"].ToString();
    //                                    newrow["gpname"] = drgp["LocationName"].ToString();
    //                                    newrow["blockcode"] = gpparentcode;
    //                                    Report.Rows.Add(newrow);
    //                                }
    //                            }
    //                        }
    //                    }
    //                    if (Report.Rows.Count > 0)
    //                    {
    //                        DataView view = new DataView(Report);
    //                        DataTable dtpostdetails = view.ToTable(true, "gpcode", "gpname", "blockcode");
    //                        if (dtpostdetails.Rows.Count > 0)
    //                        {
    //                            foreach (DataRow drd in dtpostdetails.Rows)
    //                            {
    //                                gps block = new gps();
    //                                block.gpcode = drd["gpcode"].ToString();
    //                                block.gpname = drd["gpname"].ToString();
    //                                block.blockcode = drd["blockcode"].ToString();
    //                                gplist.Add(block);
    //                            }
    //                        }
    //                    }
    //                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //                    string response = jsonSerializer.Serialize(gplist);
    //                    Context.Response.Clear();
    //                    Context.Response.ContentType = "application/json";
    //                    Context.Response.AddHeader("content-length", response.Length.ToString());
    //                    Context.Response.Flush();
    //                    Context.Response.Write(response);
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Context.Response.Clear();
    //            Context.Response.StatusCode = 401;
    //            HttpContext.Current.ApplicationInstance.CompleteRequest();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Context.Response.Clear();
    //        Context.Response.StatusCode = 401;
    //        HttpContext.Current.ApplicationInstance.CompleteRequest();
    //    }
    //}

    public class Village
    {
        public string Villagecode { get; set; }
        public string Villagename { get; set; }
        public string GPcode { get; set; }
    }
    public class saveben
    {
        public string msgresponce { get; set; }
    }
    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void loadvillages(string empid, string workitemcode, string uuid, List<string> blocks, List<string> gps)
    //{
    //    try
    //    {
    //        vdm = new SalesDBManager();
    //        postvdm = new SAPdbmanger();
    //        string GPlocationcode = "";
    //        string BLOClocationcode = "";
    //        for (int i = 0; i < gps.Count; i++)
    //        {
    //            string loccode = gps[i].ToString();
    //            GPlocationcode += loccode + "','";
    //        }

    //        for (int i = 0; i < blocks.Count; i++)
    //        {
    //            string Bloccode = gps[i].ToString();
    //            BLOClocationcode += Bloccode + "','";
    //        }
    //        string serveyworkitemmappingcode = "";
    //        //added by naveeen 
    //        string token = System.Web.HttpContext.Current.Request.Headers["token"];
    //        string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
    //        string huuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
    //        //end

    //        cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM  tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
    //        cmd.Parameters.Add("@empcode", employecode);
    //        cmd.Parameters.Add("@token", token);
    //        cmd.Parameters.Add("@uuid", huuid);
    //        cmd.Parameters.Add("@IsActive", true);
    //        DataTable dttoken = vdm.SelectQuery(cmd).Tables[0];

    //        if (dttoken.Rows.Count > 0)
    //        {
    //            foreach (DataRow drt in dttoken.Rows)
    //            {
    //                string sessionexpdate = drt["SessionExpiryTime"].ToString();
    //                DateTime dtsessionexpdate = Convert.ToDateTime(sessionexpdate);
    //                DateTime nowdatetime = DateTime.Now;
    //                if (nowdatetime > dtsessionexpdate)
    //                {
    //                    Context.Response.Clear();
    //                    Context.Response.StatusCode = 401;
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //                }
    //                else
    //                {
    //                    DateTime extendsessionexpdate = nowdatetime.AddHours(1);
    //                    cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET SessionExpiryTime=@extenddate WHERE (EmployeeCode = @emplcode) AND (SessionToken = @stoken) AND (DeviceID=@duuid) AND (IsActive=@active)");
    //                    cmd.Parameters.Add("@emplcode", employecode);
    //                    cmd.Parameters.Add("@stoken", token);
    //                    cmd.Parameters.Add("@duuid", huuid);
    //                    cmd.Parameters.Add("@active", true);
    //                    cmd.Parameters.Add("@extenddate", extendsessionexpdate);
    //                    vdm.Update(cmd);

    //                    cmd = new SqlCommand("SELECT  SurveyWorkItemMappingCode, SurveyCode, WorkItemCode, LocationCode, Status, isActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Frequncy, StartDate, EndDDate FROM   tbl_MMP_SurveyWorkItem WHERE  (WorkItemCode = @workitem)");
    //                    cmd.Parameters.Add("@workitem", workitemcode);
    //                    DataTable dtsurveyworkitemdtls = vdm.SelectQuery(cmd).Tables[0];
    //                    if (dtsurveyworkitemdtls.Rows.Count > 0)
    //                    {
    //                        foreach (DataRow drs in dtsurveyworkitemdtls.Rows)
    //                        {
    //                            serveyworkitemmappingcode = drs["SurveyWorkItemMappingCode"].ToString();
    //                        }
    //                    }
    //                    cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.ParentLocCode, tbl_MST_Location.LocationType FROM    tbl_MST_Location INNER JOIN   tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @wcode)");
    //                    cmd.Parameters.Add("@wcode", workitemcode);
    //                    DataTable dtblocks = vdm.SelectQuery(cmd).Tables[0];

    //                    cmd = new SqlCommand("SELECT  DISTINCT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.LocationType FROM    tbl_MST_Location INNER JOIN   tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @wcode)");
    //                    cmd.Parameters.Add("@wcode", workitemcode);
    //                    DataTable dtworkitemblocks = vdm.SelectQuery(cmd).Tables[0];

    //                    List<blocks> blockslist = new List<blocks>();
    //                    DataTable Report = new DataTable();
    //                    Report.Columns.Add("VillageCode");
    //                    Report.Columns.Add("VillageName");
    //                    Report.Columns.Add("GPcode");
    //                    if (dtworkitemblocks.Rows.Count > 0)
    //                    {
    //                        if (GPlocationcode == "")
    //                        {
    //                            foreach (DataRow drwb in dtworkitemblocks.Rows)
    //                            {
    //                                string locationtype = drwb["LocationType"].ToString();
    //                                if (locationtype == "Gram Panchayat")
    //                                {
    //                                    string locationcode = drwb["LocationCode"].ToString();
    //                                    GPlocationcode += locationcode + "','";
    //                                }
    //                                if (locationtype == "Block")
    //                                {
    //                                    if (BLOClocationcode == "")
    //                                    {
    //                                        string locationcode = drwb["LocationCode"].ToString();
    //                                        BLOClocationcode += locationcode + "','";
    //                                    }
    //                                    else
    //                                    {
    //                                        string locationcode = drwb["LocationCode"].ToString();
    //                                        BLOClocationcode += locationcode + "','";
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }

    //                    foreach (DataRow dr in dtblocks.Rows)
    //                    {
    //                        string locationtype = dr["LocationType"].ToString();
    //                        if (locationtype == "Village")
    //                        {
    //                            DataRow newrow = Report.NewRow();
    //                            newrow["VillageCode"] = dr["LocationCode"].ToString();
    //                            newrow["VillageName"] = dr["LocationName"].ToString();
    //                            newrow["GPcode"] = dr["ParentLocCode"].ToString();
    //                            Report.Rows.Add(newrow);
    //                        }
    //                        if (locationtype == "Gram Panchayat")
    //                        {
    //                            string parentcode = dr["ParentLocCode"].ToString();
    //                            cmd = new SqlCommand("SELECT DISTINCT tbl_MST_Location_1.LocationCode, tbl_MST_Location_1.LocationName, tbl_MST_Location_1.LocationType, tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MST_Location_1.ParentLocCode FROM            tbl_MST_Location INNER JOIN  tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode INNER JOIN  tbl_MST_Location AS tbl_MST_Location_1 ON tbl_MMP_WorkItemLocation.LocationCode = tbl_MST_Location_1.ParentLocCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @workitemcode) AND (tbl_MST_Location_1.ParentLocCode IN ('" + GPlocationcode + "')) AND (tbl_MST_Location_1.LocationType = 'Village')");
    //                            cmd.Parameters.Add("@workitemcode", workitemcode);
    //                            DataTable dtgpblocks = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtgpblocks.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drgpb in dtgpblocks.Rows)
    //                                {
    //                                    DataRow newrow = Report.NewRow();
    //                                    newrow["VillageCode"] = drgpb["LocationCode"].ToString();
    //                                    newrow["VillageName"] = drgpb["LocationName"].ToString();
    //                                    newrow["GPcode"] = drgpb["ParentLocCode"].ToString();
    //                                    Report.Rows.Add(newrow);
    //                                }
    //                            }
    //                        }
    //                        if (locationtype == "Block")
    //                        {
    //                            cmd = new SqlCommand("SELECT DISTINCT tbl_MST_Location_1.LocationCode, tbl_MST_Location_1.LocationName, tbl_MST_Location_1.LocationType, tbl_MMP_WorkItemLocation.WorkItemCode FROM            tbl_MST_Location INNER JOIN tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode INNER JOIN tbl_MST_Location AS tbl_MST_Location_1 ON tbl_MMP_WorkItemLocation.LocationCode = tbl_MST_Location_1.ParentLocCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @workitemcode) AND (tbl_MST_Location_1.ParentLocCode IN ('" + BLOClocationcode + "')) AND (tbl_MST_Location_1.LocationCode IN ('" + GPlocationcode + "'))");
    //                            cmd.Parameters.Add("@workitemcode", workitemcode);
    //                            DataTable dtvblocks = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtvblocks.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drvb in dtvblocks.Rows)
    //                                {
    //                                    string vgloccode = drvb["LocationCode"].ToString();
    //                                    GPlocationcode += vgloccode + "','";
    //                                }
    //                                cmd = new SqlCommand("SELECT DISTINCT tbl_MST_Location_1.LocationCode, tbl_MST_Location_1.LocationName, tbl_MST_Location_1.LocationType, tbl_MST_Location_1.ParentLocCode, tbl_MMP_WorkItemLocation.WorkItemCode FROM            tbl_MST_Location INNER JOIN  tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode INNER JOIN  tbl_MST_Location AS tbl_MST_Location_1 ON tbl_MMP_WorkItemLocation.LocationCode = tbl_MST_Location_1.ParentLocCode WHERE  (tbl_MMP_WorkItemLocation.WorkItemCode = @workitemcode) AND (tbl_MST_Location_1.ParentLocCode IN ('" + GPlocationcode + "')) AND (tbl_MST_Location_1.LocationType = 'Village')");
    //                                cmd.Parameters.Add("@workitemcode", workitemcode);
    //                                DataTable dtgpblocks = vdm.SelectQuery(cmd).Tables[0];
    //                                if (dtgpblocks.Rows.Count > 0)
    //                                {
    //                                    foreach (DataRow drgpb in dtgpblocks.Rows)
    //                                    {
    //                                        DataRow newrow = Report.NewRow();
    //                                        newrow["VillageCode"] = drgpb["LocationCode"].ToString();
    //                                        newrow["VillageName"] = drgpb["LocationName"].ToString();
    //                                        newrow["GPcode"] = drgpb["ParentLocCode"].ToString();
    //                                        Report.Rows.Add(newrow);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                    List<Village> Villagelist = new List<Village>();
    //                    if (Report.Rows.Count > 0)
    //                    {
    //                        DataTable VillReport = new DataTable();
    //                        VillReport.Columns.Add("VillageCode");
    //                        VillReport.Columns.Add("VillageName");
    //                        VillReport.Columns.Add("GPcode");
    //                        DataView view = new DataView(Report);
    //                        DataTable dtpostdetails = view.ToTable(true, "VillageCode", "VillageName", "GPcode");
    //                        if (dtpostdetails.Rows.Count > 0)
    //                        {
    //                            foreach (DataRow drd in dtpostdetails.Rows)
    //                            {
    //                                string locationcode = drd["VillageCode"].ToString();
    //                                cmd = new SqlCommand("SELECT DISTINCT mv.LocationName AS village, mv.LocationCode AS villagecode FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id LEFT OUTER JOIN  tbl_MMP_SurveyBeneficiary ON tbl_MST_Respondant.ResondantCode = tbl_MMP_SurveyBeneficiary.RespondantCode WHERE mb.LocationCode IN ('" + locationcode + "') OR mg.LocationCode IN ('" + locationcode + "') OR mv.LocationCode IN ('" + locationcode + "')");
    //                                DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
    //                                if (dtrespondentdetails.Rows.Count > 0)
    //                                {
    //                                    foreach (DataRow dred in dtrespondentdetails.Rows)
    //                                    {
    //                                        string villagecode = dred["villagecode"].ToString();
    //                                        foreach (DataRow drav in dtpostdetails.Select("VillageCode='" + villagecode + "'"))
    //                                        {
    //                                            DataRow newrow = VillReport.NewRow();
    //                                            newrow["VillageCode"] = drav["VillageCode"].ToString();
    //                                            newrow["VillageName"] = drav["VillageName"].ToString();
    //                                            newrow["GPcode"] = drav["GPcode"].ToString();
    //                                            VillReport.Rows.Add(newrow);
    //                                        }
    //                                    }
    //                                }
    //                            }
    //                        }
    //                        if (VillReport.Rows.Count > 0)
    //                        {
    //                            DataView newview = new DataView(VillReport);
    //                            DataTable dtVillReport = newview.ToTable(true, "VillageCode", "VillageName", "GPcode");
    //                            if (dtVillReport.Rows.Count > 0)
    //                            {
    //                                foreach (DataRow drd in dtVillReport.Rows)
    //                                {
    //                                    Village vil = new Village();
    //                                    vil.Villagecode = drd["VillageCode"].ToString();
    //                                    vil.Villagename = drd["VillageName"].ToString();
    //                                    vil.GPcode = drd["GPcode"].ToString();
    //                                    Villagelist.Add(vil);
    //                                }
    //                            }
    //                        }
    //                    }

    //                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //                    jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //                    string response = jsonSerializer.Serialize(Villagelist);
    //                    Context.Response.Clear();
    //                    Context.Response.ContentType = "application/json";
    //                    Context.Response.AddHeader("content-length", response.Length.ToString());
    //                    Context.Response.Flush();
    //                    Context.Response.Write(response);
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Context.Response.Clear();
    //            Context.Response.StatusCode = 401;
    //            HttpContext.Current.ApplicationInstance.CompleteRequest();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Context.Response.Clear();
    //        Context.Response.StatusCode = 401;
    //        HttpContext.Current.ApplicationInstance.CompleteRequest();
    //    }
    //}

    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void saveBeneficiary(string empid, string uuid, string workitemcode, string surveyworkitemmappingcode, List<string> selectedben, string UploadedFrom)
    //{
    //    try
    //    {
    //        //added by naveeen 
    //        string token = System.Web.HttpContext.Current.Request.Headers["token"];
    //        string employecode = System.Web.HttpContext.Current.Request.Headers["empcode"];
    //        string huuid = System.Web.HttpContext.Current.Request.Headers["uuid"];
    //        //end

    //        cmd = new SqlCommand("SELECT  RowCode, UserID, EmployeeCode, Domain, SessionToken, SessionExpiryTime, LoginTime, LogoutTime, DeviceID, IsActive FROM            tbl_TRN_LogInDetail WHERE (EmployeeCode = @empcode) AND (SessionToken = @token) AND (DeviceID=@uuid) AND (IsActive=@IsActive)");
    //        cmd.Parameters.Add("@empcode", employecode);
    //        cmd.Parameters.Add("@token", token);
    //        cmd.Parameters.Add("@uuid", huuid);
    //        cmd.Parameters.Add("@IsActive", true);
    //        DataTable dttoken = vdm.SelectQuery(cmd).Tables[0];

    //        if (dttoken.Rows.Count > 0)
    //        {
    //            foreach (DataRow drt in dttoken.Rows)
    //            {
    //                string sessionexpdate = drt["SessionExpiryTime"].ToString();
    //                DateTime dtsessionexpdate = Convert.ToDateTime(sessionexpdate);
    //                DateTime nowdatetime = DateTime.Now;
    //                if (nowdatetime > dtsessionexpdate)
    //                {
    //                    Context.Response.Clear();
    //                    Context.Response.StatusCode = 401;
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();

    //                }
    //                else
    //                {
    //                    DateTime extendsessionexpdate = nowdatetime.AddHours(1);
    //                    cmd = new SqlCommand("UPDATE tbl_TRN_LogInDetail SET SessionExpiryTime=@extenddate WHERE (EmployeeCode = @emplcode) AND (SessionToken = @stoken) AND (DeviceID=@duuid) AND (IsActive=@active)");
    //                    cmd.Parameters.Add("@emplcode", employecode);
    //                    cmd.Parameters.Add("@stoken", token);
    //                    cmd.Parameters.Add("@duuid", huuid);
    //                    cmd.Parameters.Add("@active", true);
    //                    cmd.Parameters.Add("@extenddate", extendsessionexpdate);
    //                    vdm.Update(cmd);

    //                    JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //                    List<saveben> savebenlist = new List<saveben>();
    //                    string sempid = empid;
    //                    for (int i = 0; i < selectedben.Count; i++)
    //                    {
    //                        if (selectedben[i] == null)
    //                        {

    //                        }
    //                        else
    //                        {
    //                            string respondentcode = selectedben[i].ToString();
    //                            if (respondentcode == "" || respondentcode == null)
    //                            {

    //                            }
    //                            else
    //                            {
    //                                string InterventionCode = "";
    //                                cmd = new SqlCommand("SELECT tm.ConceptualizedInterventionCode FROM  tbl_TRN_WorkItem AS tw INNER JOIN tbl_TRN_MileStone AS tm ON tw.MileStoneCode = tm.MileStoneCode INNER JOIN  tbl_MMP_SurveyWorkItem AS tsw ON tw.WorkItemCode = tsw.WorkItemCode WHERE (tsw.SurveyWorkItemMappingCode = @swmpc)");
    //                                cmd.Parameters.Add("@swmpc", surveyworkitemmappingcode);
    //                                DataTable dtworkitem = vdm.SelectQuery(cmd).Tables[0];
    //                                if (dtworkitem.Rows.Count > 0)
    //                                {
    //                                    foreach (DataRow dr in dtworkitem.Rows)
    //                                    {
    //                                        InterventionCode = dr["ConceptualizedInterventionCode"].ToString();
    //                                    }
    //                                }

    //                                cmd = new SqlCommand("SELECT BeneficiaryListing, SurveyCode FROM tbl_MMP_SurveyBeneficiary WHERE RespondantCode=@respcode AND InterventionCode=@InterventionCode");
    //                                cmd.Parameters.Add("@InterventionCode", InterventionCode);
    //                                cmd.Parameters.Add("@respcode", respondentcode);
    //                                DataTable dtBeneficiarydetails = vdm.SelectQuery(cmd).Tables[0];
    //                                if (dtBeneficiarydetails.Rows.Count > 0)
    //                                {
    //                                }
    //                                else
    //                                {
    //                                    cmd = new SqlCommand("INSERT INTO tbl_MMP_SurveyBeneficiary (SurveyCode, RespondantCode, IsActive, Type, CreatedBy, CreatedOn,  InterventionCode, UploadedFrom) VALUES (@SurveyCode, @RspondantCode, @IsActive, @Type, @CreatedBy, @CreatedOn, @InterventionCode, @UploadedFrom)");
    //                                    cmd.Parameters.Add("@RspondantCode", respondentcode);
    //                                    cmd.Parameters.Add("@IsActive", "True");
    //                                    cmd.Parameters.Add("@Type", "Individual");
    //                                    cmd.Parameters.Add("@CreatedBy", "51694805");
    //                                    cmd.Parameters.Add("@CreatedOn", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
    //                                    cmd.Parameters.Add("@InterventionCode", InterventionCode);
    //                                    cmd.Parameters.Add("@SurveyCode", surveyworkitemmappingcode);
    //                                    cmd.Parameters.Add("@UploadedFrom", UploadedFrom);
    //                                    vdm.insert(cmd);
    //                                }
    //                            }
    //                        }
    //                    }
    //                    string msg = "Ok";
    //                    saveben vil = new saveben();
    //                    vil.msgresponce = msg;
    //                    savebenlist.Add(vil);

    //                    JavaScriptSerializer jsonSerializerS = new JavaScriptSerializer();
    //                    string response = jsonSerializerS.Serialize(savebenlist);
    //                    Context.Response.Clear();
    //                    Context.Response.ContentType = "application/json";
    //                    Context.Response.AddHeader("content-length", response.Length.ToString());
    //                    Context.Response.Flush();
    //                    Context.Response.Write(response);
    //                    HttpContext.Current.ApplicationInstance.CompleteRequest();
    //                }
    //            }
    //        }
    //        else
    //        {
    //            Context.Response.Clear();
    //            Context.Response.StatusCode = 401;
    //            HttpContext.Current.ApplicationInstance.CompleteRequest();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        string mymsg = ex.Message;
    //        string msg = "";

    //        List<saveben> savebenlist = new List<saveben>();
    //        saveben vil = new saveben();
    //        vil.msgresponce = mymsg;
    //        savebenlist.Add(vil);

    //        Context.Response.Clear();
    //        Context.Response.StatusCode = 401;
    //        HttpContext.Current.ApplicationInstance.CompleteRequest();
    //    }
    //}

    private static List<T> ConvertDataTable<T>(DataTable dt)
    {
        List<T> data = new List<T>();
        foreach (DataRow row in dt.Rows)
        {
            T item = GetItem<T>(row);
            data.Add(item);
        }
        return data;
    }
    private static T GetItem<T>(DataRow dr)
    {
        Type temp = typeof(T);
        T obj = Activator.CreateInstance<T>();
        foreach (DataColumn column in dr.Table.Columns)
        {
            foreach (PropertyInfo pro in temp.GetProperties())
            {
                if (pro.Name == column.ColumnName)
                    pro.SetValue(obj, dr[column.ColumnName], null);
                else
                    continue;
            }
        }
        return obj;
    }

    public class status
    {
        public string saved { get; set; }//1
        public string submitted { get; set; }//2
        public string synced { get; set; }//2
    }

    public class survey_bene
    {
        public string resondantcode { get; set; }
        public string respondantname { get; set; }
        public string hohname { get; set; }
        public string relationwithhoh { get; set; }
        public string dateofbirth { get; set; }
        public string gender { get; set; }
        public string idtype { get; set; }
        public string idnumber { get; set; }
        public string buildingcode { get; set; }
        public string housecode { get; set; }
        public string blockname { get; set; }
        public string gpname { get; set; }
        public string village { get; set; }
        public string occupation { get; set; }
        public string buildingtype { get; set; }
        public string geom { get; set; }
    }
    public class buildinggeom
    {
        public string buildingcode { get; set; }
        //public string st_astext { get; set; }
    }
    public class buildingtypegeom
    {
        public string buildingtypename { get; set; }
        //public string st_astext { get; set; }
    }
    public class selectbenflift
    {
        public List<bene_status> bene_status { get; set; }
    }
    public class downloadbenf  //new
    {
        public List<survey_bene> survey_bene { get; set; }
        public ArrayList buildinggeom { get; set; }
        public List<govtbuildings> buildingtypegeom { get; set; }
        //public List<buildinggeom> buildinggeom { get; set; }
        public List<roadarray_vill> roadarray_vill { get; set; }
        public List<roadarray_vill> roadarray_villBOUNDRIES { get; set; }
        public List<status> status { get; set; }
    }

    public class newdownloadbenf  //new
    {
        public List<survey_bene> survey_bene { get; set; }
    }
    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members
        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1;   // Handle equality as beeing greater
            else
                return result;
        }
        #endregion
    }

    public class roadarray_vill
    {
        public string road_code { get; set; }
        public string st_astext { get; set; }
        public string villagename { get; set; }
    }
    public class govtbuildings
    {
        public string type { get; set; }
        public string geom { get; set; }
    }

    // comment by naveen
    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void loadnewgps(string empid, string workitemcode, string uuid, List<string> blocks)
    //{
    //    try
    //    {
    //        vdm = new SalesDBManager();
    //        postvdm = new SAPdbmanger();
    //        string locationcode = "";
    //        for (int i = 0; i < blocks.Count; i++)
    //        {
    //            string loccode = blocks[i].ToString();
    //            locationcode += loccode + "','";
    //        }
    //        List<gps> gplist = new List<gps>();
    //        cmd = new SqlCommand("SELECT  LocationCode, LocationName, LocationType, ParentLocationCode, ParentLocCode FROM  tbl_MST_Location WHERE (LocationType ='Gram Panchayat') AND (ParentLocCode IN ('" + locationcode + "'))");
    //        DataTable dtgps = vdm.SelectQuery(cmd).Tables[0];
    //        if (dtgps.Rows.Count > 0)
    //        {
    //            foreach (DataRow drd in dtgps.Rows)
    //            {
    //                gps block = new gps();
    //                block.gpcode = drd["LocationCode"].ToString();
    //                block.gpname = drd["LocationName"].ToString();
    //                gplist.Add(block);
    //            }
    //        }
    //        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //        jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //        string response = jsonSerializer.Serialize(gplist);
    //        Context.Response.Clear();
    //        Context.Response.ContentType = "application/json";
    //        Context.Response.AddHeader("content-length", response.Length.ToString());
    //        Context.Response.Flush();
    //        Context.Response.Write(response);
    //        HttpContext.Current.ApplicationInstance.CompleteRequest();
    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //}

    //[WebMethod]
    //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    //public void benficiarydownload(string workitemcode, string surveyworkitemmappingcode, string type)
    //{
    //    try
    //    {
    //        vdm = new SalesDBManager();
    //        postvdm = new SAPdbmanger();
    //        string mappingcode = surveyworkitemmappingcode;
    //        if (workitemcode != "" && workitemcode != null && workitemcode != "null")
    //        {
    //            cmd = new SqlCommand("SELECT tbl_MST_Location.LocationCode, tbl_MST_Location.LocationName, tbl_MST_Location.LocationType, tbl_MST_Location.Label, tbl_MST_Location.CreatedBy, tbl_MST_Location.CreatedOn,  tbl_MST_Location.ModifiedBy, tbl_MST_Location.ModifiedOn, tbl_MST_Location.IsActive, tbl_MST_Location.ParentLocationCode, tbl_MST_Location.ParentLocCode, tbl_MMP_WorkItemLocation.RowCode,   tbl_MMP_WorkItemLocation.WorkItemCode, tbl_MMP_WorkItemLocation.LocationCode AS Expr1, tbl_MMP_WorkItemLocation.CreatedBy AS Expr2, tbl_MMP_WorkItemLocation.CreatedOn AS Expr3,  tbl_MMP_WorkItemLocation.ModifiedBy AS Expr4, tbl_MMP_WorkItemLocation.ModifiedOn AS Expr5 FROM  tbl_MST_Location INNER JOIN tbl_MMP_WorkItemLocation ON tbl_MST_Location.LocationCode = tbl_MMP_WorkItemLocation.LocationCode where tbl_MMP_WorkItemLocation.WorkItemCode=@wcode");
    //            cmd.Parameters.Add("@wcode", workitemcode);
    //            DataTable dtlocationdetails = vdm.SelectQuery(cmd).Tables[0];
    //            List<roadarray_vill> roadarrayvillboundries = new List<roadarray_vill>();
    //            List<roadarray_vill> roadarrayvilllist = new List<roadarray_vill>();
    //            List<survey_bene> surveybenelist = new List<survey_bene>();
    //            List<buildinggeom> buildinggeomlist = new List<buildinggeom>();
    //            List<status> statuslist = new List<status>();
    //            ArrayList myItems = new ArrayList();
    //            ArrayList mybuildingtypeItems = new ArrayList();

    //            if (dtlocationdetails.Rows.Count > 0)
    //            {
    //                string locationcode = "";
    //                string buildingcode = "";
    //                string loctype = "";
    //                string locname = "";

    //                foreach (DataRow dr in dtlocationdetails.Rows)
    //                {
    //                    string loccode = dr["LocationCode"].ToString();
    //                    string locationtype = dr["LocationType"].ToString();
    //                    string LocationName = dr["LocationName"].ToString();
    //                    string lname = LocationName.ToLower();
    //                    if (dtlocationdetails.Rows.Count > 1)
    //                    {
    //                        if (locationtype == "Block")
    //                        {
    //                            locationcode += loccode + "','";
    //                            loctype += type + "','";
    //                            locname += lname + "','";
    //                        }
    //                        else if (locationtype == "Gram Panchayat")
    //                        {
    //                            locationcode += loccode + "','";
    //                            loctype += type + "','";
    //                            locname += lname + "','";
    //                        }
    //                        else if (locationtype == "Village")
    //                        {
    //                            locationcode += loccode + "','";
    //                            loctype += type + "','";
    //                            locname += lname + "','";
    //                        }
    //                    }
    //                    else
    //                    {
    //                        locationcode += loccode;
    //                        loctype += type;
    //                        locname += lname;
    //                    }
    //                }

    //                if (type == "BS")
    //                {
    //                    cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender, tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM  tbl_MST_Respondant INNER JOIN    tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN    tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE mb.LocationCode IN ('" + locationcode + "') OR mg.LocationCode IN ('" + locationcode + "') OR mv.LocationCode IN ('" + locationcode + "')");
    //                    DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];
    //                    if (dtrespondentdetails.Rows.Count > 0)
    //                    {
    //                        foreach (DataRow dr in dtrespondentdetails.Rows)
    //                        {
    //                            survey_bene survey = new survey_bene();
    //                            survey.resondantcode = dr["ResondantCode"].ToString();
    //                            survey.respondantname = dr["RespondantName"].ToString();
    //                            survey.hohname = dr["HohName"].ToString();
    //                            survey.relationwithhoh = dr["RelationWithHoh"].ToString();

    //                            string DateOfBirth = dr["DateOfBirth"].ToString();
    //                            if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
    //                            {
    //                                DateTime dtDateOfBirth = Convert.ToDateTime(dr["DateOfBirth"].ToString());
    //                                survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                            }
    //                            else
    //                            {
    //                                survey.dateofbirth = dr["DateOfBirth"].ToString();
    //                            }
    //                            survey.gender = dr["Gender"].ToString();
    //                            survey.idtype = dr["IdType"].ToString();
    //                            survey.idnumber = dr["IdNumber"].ToString();
    //                            survey.buildingcode = dr["buildingcode"].ToString();
    //                            survey.housecode = dr["HouseCode"].ToString();
    //                            survey.blockname = dr["blockname"].ToString();
    //                            survey.gpname = dr["gpname"].ToString();
    //                            survey.village = dr["village"].ToString();
    //                            survey.occupation = dr["Occupation"].ToString();
    //                            survey.buildingtype = dr["BuildingType"].ToString();
    //                            surveybenelist.Add(survey);
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    cmd = new SqlCommand("SELECT tbl_TRN_MileStone.ConceptualizedInterventionCode, tbl_TRN_WorkItem.WorkItemCode FROM   tbl_TRN_WorkItem INNER JOIN   tbl_TRN_MileStone ON tbl_TRN_WorkItem.MileStoneCode = tbl_TRN_MileStone.MileStoneCode  WHERE (tbl_TRN_WorkItem.WorkItemCode = @wicode)");
    //                    cmd.Parameters.Add("@wicode", workitemcode);
    //                    DataTable dtmilestone = vdm.SelectQuery(cmd).Tables[0];
    //                    if (dtmilestone.Rows.Count > 0)
    //                    {
    //                        foreach (DataRow dr in dtmilestone.Rows)
    //                        {
    //                            string ivcode = dr["ConceptualizedInterventionCode"].ToString();
    //                            cmd = new SqlCommand("SELECT tmr.ResondantCode, tmr.RespondantName, tmr.HohName, tmr.RelationWithHoh, tmr.DateOfBirth, tmr.Gender, tmr.IdType, tmr.IdNumber, SUBSTRING(tmr.HouseCode, 1, 19) AS buildingcode, tmr.HouseCode, tmr.BlockCode, tmr.GramPanchayatCode, mb.LocationName AS blockname, mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_BuildingType.BuildingType, tmr.VillageCode, tmr.Occupation, tmr.IsActive, tmr.CreatedBy, tmr.CreatedOn, tmr.ModifiedBy, tmr.ModifiedOn, tmr.SynchedOn, tmr.ClientID, tmr.IsHOH, tmr.BuildingTypeCode, tmr.FatherName, tmr.MobileNo, tmr.IDTypeImported,   tmr.IDNumberImported, tmr.FatherNameImported, tmr.MobileNoImported, tmr.RelationWithHohImported, tmr.DateOfBirthImported, tmr.GenderImported, tmr.OccupationImported, tmr.IsImported, tmsb.Id, tmsb.SurveyCode,   tmsb.RespondantCode, tmsb.IsActive AS Expr1, tmsb.Type, tmsb.InterventionCode, tmsb.BeneficiaryListing FROM tbl_MST_Respondant AS tmr INNER JOIN tbl_MMP_SurveyBeneficiary AS tmsb ON tmr.ResondantCode = tmsb.RespondantCode  INNER JOIN tbl_MST_Location AS mb ON tmr.BlockCode = mb.LocationCode INNER JOIN  tbl_MST_Location AS mg ON tmr.GramPanchayatCode = mg.LocationCode INNER JOIN tbl_MST_Location AS mv ON tmr.VillageCode = mv.LocationCode INNER JOIN tbl_MST_BuildingType ON tmr.BuildingTypeCode = tbl_MST_BuildingType.id WHERE (tmsb.InterventionCode = @ivcode)");
    //                            cmd.Parameters.Add("@ivcode", ivcode);
    //                            DataTable dtserveybenficery = vdm.SelectQuery(cmd).Tables[0];
    //                            if (dtserveybenficery.Rows.Count > 0)
    //                            {

    //                                foreach (DataRow drsb in dtserveybenficery.Rows)
    //                                {

    //                                    survey_bene survey = new survey_bene();
    //                                    survey.resondantcode = drsb["ResondantCode"].ToString();
    //                                    survey.respondantname = drsb["RespondantName"].ToString();
    //                                    survey.hohname = drsb["HohName"].ToString();
    //                                    survey.relationwithhoh = drsb["RelationWithHoh"].ToString();
    //                                    string DateOfBirth = drsb["DateOfBirth"].ToString();
    //                                    if (DateOfBirth != "" && DateOfBirth != null && DateOfBirth != "null")
    //                                    {
    //                                        DateTime dtDateOfBirth = Convert.ToDateTime(drsb["DateOfBirth"].ToString());
    //                                        survey.dateofbirth = dtDateOfBirth.ToString("yyyy-MM-dd hh:mm tt");
    //                                    }
    //                                    else
    //                                    {
    //                                        survey.dateofbirth = drsb["DateOfBirth"].ToString();
    //                                    }

    //                                    survey.gender = drsb["Gender"].ToString();
    //                                    survey.idtype = drsb["IdType"].ToString();
    //                                    survey.idnumber = drsb["IdNumber"].ToString();
    //                                    survey.buildingcode = drsb["buildingcode"].ToString();
    //                                    string bcode = drsb["buildingcode"].ToString();
    //                                    string msbcode = bcode.ToLower();
    //                                    if (dtserveybenficery.Rows.Count > 1)
    //                                    {
    //                                        buildingcode += msbcode + "','";
    //                                    }
    //                                    else
    //                                    {
    //                                        buildingcode += msbcode;
    //                                    }

    //                                    survey.housecode = drsb["HouseCode"].ToString();
    //                                    survey.blockname = drsb["blockname"].ToString();
    //                                    survey.gpname = drsb["gpname"].ToString();
    //                                    survey.village = drsb["village"].ToString();
    //                                    survey.occupation = drsb["Occupation"].ToString();
    //                                    survey.buildingtype = drsb["BuildingType"].ToString();
    //                                    surveybenelist.Add(survey);
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //                if (type == "BS")
    //                {
    //                    postcmd = new NpgsqlCommand("SELECT distinct buildingcode, ST_AsText(geom) as geom FROM buildings_final WHERE lower(block_name) IN ('" + locname + "') OR lower(panchayat_name) IN ('" + locname + "') OR lower(village_name) IN ('" + locname + "')");
    //                    DataTable dtbuildinggeom = postvdm.SelectQuery(postcmd).Tables[0];
    //                    if (dtbuildinggeom.Rows.Count > 0)
    //                    {
    //                        int i = 0;
    //                        var myList = new List<string>();
    //                        SortedList fslist = new SortedList();
    //                        foreach (DataRow drb in dtbuildinggeom.Rows)
    //                        {
    //                            string colunname = drb["buildingcode"].ToString();
    //                            string geom = drb["geom"].ToString();

    //                            // Adding pairs to fslist 
    //                            fslist.Add("" + colunname + "", geom);

    //                        }
    //                        myItems.Add(fslist);
    //                    }

    //                    postcmd = new NpgsqlCommand("SELECT distinct buildingtypename, ST_AsText(geom) as geom FROM buildings_final WHERE lower(block_name) IN ('" + locname + "') OR lower(panchayat_name) IN ('" + locname + "') OR lower(village_name) IN ('" + locname + "') AND  buildingtypename in ('PUBLIC TOILETS','PHC','CHC','SUB CENTRE','PANCHAYAT BHAWAN','POST OFFICE','POLICE OFFICE','VENTERINERY HOSPITAL','RELIGIOUS BUILDING','CEMETERY & BURIAL GROUND','ELECTRICITY STATION''GOVT. SCHOOL','PVT SCHOOL')");
    //                    DataTable dtbuildingtypegeom = postvdm.SelectQuery(postcmd).Tables[0];
    //                    if (dtbuildingtypegeom.Rows.Count > 0)
    //                    {
    //                        int i = 0;
    //                        var myList = new List<string>();
    //                        SortedList buildingtypelist = new SortedList();
    //                        foreach (DataRow drb in dtbuildinggeom.Rows)
    //                        {
    //                            string colunname = drb["buildingtypename"].ToString();
    //                            string geom = drb["geom"].ToString();

    //                            // Adding pairs to fslist 
    //                            buildingtypelist.Add("" + colunname + "", geom);

    //                        }
    //                        mybuildingtypeItems.Add(buildingtypelist);
    //                    }
    //                }
    //                else
    //                {
    //                    //buildingcode
    //                    postcmd = new NpgsqlCommand("SELECT distinct buildingcode, ST_AsText(geom) as geom FROM buildings_final WHERE lower(buildingcode) IN ('" + buildingcode + "')");
    //                    DataTable dtbuildinggeom = postvdm.SelectQuery(postcmd).Tables[0];
    //                    if (dtbuildinggeom.Rows.Count > 0)
    //                    {
    //                        int i = 0;
    //                        var myList = new List<string>();
    //                        SortedList fslist = new SortedList();
    //                        foreach (DataRow drb in dtbuildinggeom.Rows)
    //                        {
    //                            string colunname = drb["buildingcode"].ToString();
    //                            string geom = drb["geom"].ToString();
    //                            // Adding pairs to fslist 
    //                            fslist.Add("" + colunname + "", geom);
    //                        }
    //                        myItems.Add(fslist);

    //                    }


    //                    postcmd = new NpgsqlCommand("SELECT distinct buildingtypename, ST_AsText(geom) as geom FROM buildings_final WHERE lower(buildingcode) IN ('" + buildingcode + "') AND buildingtypename in ('PUBLIC TOILETS','PHC','CHC','SUB CENTRE','PANCHAYAT BHAWAN','POST OFFICE','POLICE OFFICE','VENTERINERY HOSPITAL','RELIGIOUS BUILDING','CEMETERY & BURIAL GROUND','ELECTRICITY STATION''GOVT. SCHOOL','PVT SCHOOL')");
    //                    DataTable dtbuildingtypegeom = postvdm.SelectQuery(postcmd).Tables[0];
    //                    if (dtbuildingtypegeom.Rows.Count > 0)
    //                    {
    //                        int i = 0;
    //                        var myList = new List<string>();
    //                        SortedList buildingtypelist = new SortedList();
    //                        foreach (DataRow drb in dtbuildinggeom.Rows)
    //                        {
    //                            string colunname = drb["buildingtypename"].ToString();
    //                            string geom = drb["geom"].ToString();

    //                            // Adding pairs to fslist 
    //                            buildingtypelist.Add("" + colunname + "", geom);

    //                        }
    //                        mybuildingtypeItems.Add(buildingtypelist);
    //                    }
    //                }

    //                postcmd = new NpgsqlCommand("SELECT road_code, ST_AsText(geom) as geom FROM village_roads where lower(layer) IN ('" + locname + "') OR lower(line_gpnam) IN ('" + locname + "')");
    //                DataTable dtvillage = postvdm.SelectQuery(postcmd).Tables[0];
    //                if (dtvillage.Rows.Count > 0)
    //                {
    //                    foreach (DataRow drb in dtvillage.Rows)
    //                    {
    //                        roadarray_vill newvill = new roadarray_vill();
    //                        string raodcode = drb["road_code"].ToString();
    //                        string geom = drb["geom"].ToString();
    //                        newvill.road_code = raodcode;
    //                        newvill.st_astext = geom;
    //                        roadarrayvilllist.Add(newvill);
    //                    }
    //                }

    //                postcmd = new NpgsqlCommand("select  village_name, st_astext(geom) AS geom from village_masters");
    //                DataTable dtvillageBOUNDRIES = postvdm.SelectQuery(postcmd).Tables[0];
    //                if (dtvillageBOUNDRIES.Rows.Count > 0)
    //                {
    //                    foreach (DataRow drb in dtvillageBOUNDRIES.Rows)
    //                    {
    //                        roadarray_vill newvill = new roadarray_vill();
    //                        string raodcode = drb["village_name"].ToString();
    //                        string geom = drb["geom"].ToString();
    //                        newvill.villagename = raodcode;
    //                        newvill.st_astext = geom;
    //                        roadarrayvillboundries.Add(newvill);
    //                    }
    //                }
    //                //


    //                if (mappingcode != "" && mappingcode != null && mappingcode != "null")
    //                {
    //                    cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
    //                    cmd.Parameters.Add("@swimc", mappingcode);
    //                }
    //                else
    //                {
    //                    cmd = new SqlCommand("SELECT tbl_TRN_WorkItem.WorkItemCode, tbl_TRN_WorkItem.MileStoneCode, tbl_TRN_WorkItem.WorkItemName, tbl_TRN_WorkItem.WorkItemDesc, tbl_TRN_WorkItem.WorkItemType,  tbl_TRN_WorkItem.ParentWorkItemCode, tbl_TRN_WorkItem.PlanStartDate, tbl_TRN_WorkItem.PlanEndDate, tbl_TRN_WorkItem.PlanBudget, tbl_TRN_WorkItem.NonActivityBudget,  tbl_TRN_WorkItem.NonActivityBudgetPercentage, tbl_TRN_WorkItem.ActualStartDate, tbl_TRN_WorkItem.ActualEndDate, tbl_TRN_WorkItem.ActualExpenses, tbl_TRN_WorkItem.PrimaryOwner,  tbl_TRN_WorkItem.LocationCode, tbl_TRN_WorkItem.Target, tbl_TRN_WorkItem.TargetMeasurementUnit, tbl_TRN_WorkItem.Achievement, tbl_TRN_WorkItem.PercentageCompleted, tbl_TRN_WorkItem.Remarks,  tbl_TRN_WorkItem.Status, tbl_TRN_WorkItem.CreatedBy, tbl_TRN_WorkItem.CreatedOn, tbl_TRN_WorkItem.ModifiedBy, tbl_TRN_WorkItem.ModifiedOn, tbl_TRN_WorkItem.DisplayOrder,  tbl_TRN_WorkItem.SynchedOn, tbl_MMP_SurveyWorkItem.SurveyWorkItemMappingCode, tbl_MMP_SurveyWorkItem.SurveyCode, tbl_MMP_SurveyWorkItem.WorkItemCode AS Expr1,  tbl_MMP_SurveyWorkItem.LocationCode AS Expr2, tbl_MMP_SurveyWorkItem.Status AS Expr3, tbl_MMP_SurveyWorkItem.isActive, tbl_MMP_SurveyWorkItem.CreatedBy AS Expr4,  tbl_MMP_SurveyWorkItem.CreatedOn AS Expr5, tbl_MMP_SurveyWorkItem.ModifiedBy AS Expr6, tbl_MMP_SurveyWorkItem.ModifiedOn AS Expr7, tbl_MMP_SurveyWorkItem.Frequncy, tbl_MMP_SurveyWorkItem.StartDate, tbl_MMP_SurveyWorkItem.EndDDate FROM            tbl_TRN_WorkItem INNER JOIN  tbl_MMP_SurveyWorkItem ON tbl_TRN_WorkItem.WorkItemCode = tbl_MMP_SurveyWorkItem.WorkItemCode WHERE tbl_TRN_WorkItem.WorkItemCode=@wicode");
    //                    cmd.Parameters.Add("@wicode", workitemcode);
    //                    DataTable dtworkitemdetails = vdm.SelectQuery(cmd).Tables[0];
    //                    if (dtworkitemdetails.Rows.Count > 0)
    //                    {
    //                        foreach (DataRow dr in dtworkitemdetails.Rows)
    //                        {
    //                            mappingcode = dr["SurveyWorkItemMappingCode"].ToString();
    //                        }
    //                    }
    //                    cmd = new SqlCommand("SELECT  COUNT(DISTINCT RespondantCode) AS count, Status  FROM   tbl_TRN_SurveyResponse  WHERE (SurveyWorkItemMappingCode = @swimc) GROUP BY Status");
    //                    cmd.Parameters.Add("@swimc", mappingcode);
    //                }
    //                DataTable dtserveyresponce = vdm.SelectQuery(cmd).Tables[0];
    //                double submitedcount = 0;
    //                double savedcount = 0;
    //                if (dtserveyresponce.Rows.Count > 0)
    //                {
    //                    foreach (DataRow drsre in dtserveyresponce.Rows)
    //                    {
    //                        string status = drsre["Status"].ToString();
    //                        string count = drsre["count"].ToString();
    //                        if (count != "" || count != null)
    //                        {
    //                            if (status == "2")
    //                            {
    //                                submitedcount = Convert.ToDouble(count);
    //                            }
    //                            else
    //                            {
    //                                savedcount = Convert.ToDouble(count);
    //                            }
    //                        }
    //                    }
    //                }
    //                status syncst = new status();
    //                syncst.saved = savedcount.ToString();
    //                syncst.submitted = submitedcount.ToString();
    //                syncst.synced = submitedcount.ToString();
    //                statuslist.Add(syncst);
    //            }
    //            List<downloadbenf> getdownloadbenfdtls = new List<downloadbenf>();
    //            downloadbenf getoverDatas = new downloadbenf();
    //            getoverDatas.survey_bene = surveybenelist;
    //            getoverDatas.buildinggeom = myItems;
    //            getoverDatas.roadarray_vill = roadarrayvilllist;
    //            getoverDatas.roadarray_villBOUNDRIES = roadarrayvillboundries;
    //            getoverDatas.status = statuslist;
    //            getdownloadbenfdtls.Add(getoverDatas);
    //            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //            jsonSerializer.MaxJsonLength = Int32.MaxValue;
    //            Context.Response.Write(jsonSerializer.Serialize(getdownloadbenfdtls));
    //        }
    //        else
    //        {
    //            string msg = "Please Privede The Work Item";
    //            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
    //            jsonSerializer.MaxJsonLength = Int32.MaxValue;

    //            Context.Response.Write(jsonSerializer.Serialize(msg));
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //    }

    //}
}
