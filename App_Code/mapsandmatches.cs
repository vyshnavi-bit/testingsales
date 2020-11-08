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
/// Summary description for mapsandmatches
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class mapsandmatches : System.Web.Services.WebService
{
    SqlCommand cmd;
    SalesDBManager vdm = new SalesDBManager();
    NpgsqlCommand postcmd;
    SAPdbmanger postvdm = new SAPdbmanger();

    public class blocks
    {
        public string blockcode { get; set; }
        public string blockname { get; set; }
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void loadblocks()
    {
        try
        {
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

                        postcmd = new NpgsqlCommand("SELECT block_code,block_name FROM block_master");
                        DataTable dtblocks = postvdm.SelectQuery(postcmd).Tables[0];
                        List<blocks> blockslist = new List<blocks>();
                        foreach (DataRow dr in dtblocks.Rows)
                        {
                            blocks block = new blocks();
                            block.blockcode = dr["block_code"].ToString();
                            block.blockname = dr["block_name"].ToString();
                            blockslist.Add(block);
                        }
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(blockslist);
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
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }

    public class gps
    {
        public string gpcode { get; set; }
        public string gpname { get; set; }
        public string blockcode { get; set; }
    }


    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void loadgps(string block)
    {
        try
        {
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
                        postcmd = new NpgsqlCommand("SELECT panchayat_code, panchayat_name, block_code FROM panchayat_master WHERE block_code='" + block + "'");
                        DataTable dtblocks = postvdm.SelectQuery(postcmd).Tables[0];
                        List<gps> gplist = new List<gps>();
                        foreach (DataRow dr in dtblocks.Rows)
                        {
                            gps gpli = new gps();
                            gpli.gpcode = dr["panchayat_code"].ToString();
                            gpli.gpname = dr["panchayat_name"].ToString();
                            gpli.blockcode = dr["block_code"].ToString();
                            gplist.Add(gpli);
                        }
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(gplist);
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
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }


    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void loadvillages(string block, string gp)
    {
        try
        {
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
                        postcmd = new NpgsqlCommand("SELECT village_code, village_name, block_code, panchayat_code FROM village_master WHERE block_code='" + block + "' AND panchayat_code='" + gp + "'");
                        DataTable dtblocks = postvdm.SelectQuery(postcmd).Tables[0];
                        List<Village> villlist = new List<Village>();
                        foreach (DataRow dr in dtblocks.Rows)
                        {
                            Village vli = new Village();
                            vli.Villagecode = dr["village_code"].ToString();
                            vli.Villagename = dr["village_name"].ToString();
                            vli.GPcode = dr["panchayat_code"].ToString();
                            villlist.Add(vli);
                        }
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(villlist);
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
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }

    public class shroads
    {
        public string geom { get; set; }
        public string sh_code { get; set; }
    }
    public class gproads
    {
        public string gpr_code { get; set; }
        public string geom { get; set; }
        
    }
    public class villageroads
    {
        public string road_code { get; set; }
        public string geom { get; set; }
    }
    public class blockboundaries
    {
        public string block_name { get; set; }
        public string geom { get; set; }
    }
    public class gpboundaries
    {
        public string gp_name { get; set; }
        public string geom { get; set; }
    }
    public class villageboundaries
    {
        public string village_name { get; set; }
        public string geom { get; set; }
    }
    public class buildinggeom
    {
        public string buildingcode { get; set; }
        public string geom { get; set; }
    }
    
    public class mapsandmatcheslist
    {
        public List<shroads> getshroadslist { get; set; }
        public List<gproads> getgproadslist { get; set; }
        public List<villageroads> getvillageroadslist { get; set; }
        public List<blockboundaries> getblockboundarieslist { get; set; }
        public List<gpboundaries> getgpboundarieslist { get; set; }
        public List<villageboundaries> getvillageboundarieslist { get; set; }
        public List<survey_bene> getsurveybenelist { get; set; }
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

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void mapsandmatchesservice(string block, string gp, string village)
    {
        try
        {
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

                        

                        postcmd = new NpgsqlCommand("SELECT sh_code, ST_AsText(geom) AS geom FROM sh_roads");
                        DataTable dtshroads = postvdm.SelectQuery(postcmd).Tables[0];
                        postcmd = new NpgsqlCommand("SELECT gpr_code, ST_AsText(geom) AS geom FROM gp_roads");
                        DataTable dtgproads = postvdm.SelectQuery(postcmd).Tables[0];
                        postcmd = new NpgsqlCommand("SELECT road_code, ST_AsText(geom) as geom from village_roads WHERE lower(line_gpnam)=lower('" + gp + "')");
                        DataTable dtvillageroads = postvdm.SelectQuery(postcmd).Tables[0];
                        
                        postcmd = new NpgsqlCommand("SELECT block_name, ST_AsText(geom) AS geom FROM block_master");
                        DataTable dtblockboundaries = postvdm.SelectQuery(postcmd).Tables[0];
                        postcmd = new NpgsqlCommand("SELECT textstring as panchayat_name, ST_AsText(geom) AS geom FROM gp_boundaries");
                        DataTable dtgpboundaries = postvdm.SelectQuery(postcmd).Tables[0];
                        postcmd = new NpgsqlCommand("SELECT textstring as village_name, ST_AsText(geom) AS geom FROM village_boundries");
                        DataTable dtvillageboundaries = postvdm.SelectQuery(postcmd).Tables[0];

                        postcmd = new NpgsqlCommand("SELECT distinct buildingcode, hhcode, ST_AsText(geom) as geom FROM buildings_final where lower(village_name) = lower('" + village + "') AND lower(block_name) = lower('" + block + "') AND lower(panchayat_name) = lower('" + gp + "')");
                        DataTable dtbuildinggeom = postvdm.SelectQuery(postcmd).Tables[0];


                        cmd = new SqlCommand("SELECT tbl_MST_Respondant.ResondantCode, tbl_MST_Respondant.RespondantName, tbl_MST_Respondant.HohName, tbl_MST_Respondant.RelationWithHoh, tbl_MST_Respondant.DateOfBirth, tbl_MST_Respondant.Gender,  tbl_MST_Respondant.IdType, tbl_MST_Respondant.IdNumber, SUBSTRING(tbl_MST_Respondant.HouseCode, 1, 19) AS buildingcode, tbl_MST_Respondant.HouseCode, mb.LocationName AS blockname,  mg.LocationName AS gpname, mv.LocationName AS village, tbl_MST_Respondant.Occupation, tbl_MST_BuildingType.BuildingType FROM   tbl_MST_Respondant INNER JOIN  tbl_MST_Location AS mb ON tbl_MST_Respondant.BlockCode = mb.LocationCode INNER JOIN  tbl_MST_Location AS mg ON tbl_MST_Respondant.GramPanchayatCode = mg.LocationCode INNER JOIN  tbl_MST_Location AS mv ON tbl_MST_Respondant.VillageCode = mv.LocationCode INNER JOIN  tbl_MST_BuildingType ON tbl_MST_Respondant.BuildingTypeCode = tbl_MST_BuildingType.id WHERE (mv.LocationName = @locname) AND (tbl_MST_Respondant.IsActive = 'True')");
                        cmd.Parameters.Add("@locname", village);
                        DataTable dtrespondentdetails = vdm.SelectQuery(cmd).Tables[0];


                        List<shroads> shroadslist = new List<shroads>();
                        List<gproads> gproadslist = new List<gproads>();
                        List<villageroads> villageroadslist = new List<villageroads>();
                        List<buildinggeom> buildinggeomlist = new List<buildinggeom>();

                        List<blockboundaries> blockboundarieslist = new List<blockboundaries>();
                        List<gpboundaries> gpboundarieslist = new List<gpboundaries>();
                        List<villageboundaries> villageboundarieslist = new List<villageboundaries>();
                        List<survey_bene> surveybenelist = new List<survey_bene>();

                        if (dtshroads.Rows.Count > 0)
                        {
                            foreach(DataRow drs in dtshroads.Rows)
                            {
                                shroads shinfo = new shroads();
                                shinfo.sh_code = drs["sh_code"].ToString();
                                shinfo.geom = drs["geom"].ToString();
                                shroadslist.Add(shinfo);
                            }
                        }
                        if (dtgproads.Rows.Count > 0)
                        {
                            foreach (DataRow drg in dtgproads.Rows)
                            {
                                gproads gpinfo = new gproads();
                                gpinfo.gpr_code = drg["gpr_code"].ToString();
                                gpinfo.geom = drg["geom"].ToString();
                                gproadslist.Add(gpinfo);
                            }
                        }
                        if (dtvillageroads.Rows.Count > 0)
                        {
                            foreach (DataRow drv in dtvillageroads.Rows)
                            {
                                villageroads vilinfo = new villageroads();
                                vilinfo.road_code = drv["road_code"].ToString();
                                vilinfo.geom = drv["geom"].ToString();
                                villageroadslist.Add(vilinfo);
                            }
                        }
                        
                        if (dtblockboundaries.Rows.Count > 0)
                        {
                            foreach (DataRow drbbs in dtblockboundaries.Rows)
                            {
                                blockboundaries bbinfo = new blockboundaries();
                                bbinfo.block_name = drbbs["block_name"].ToString();
                                bbinfo.geom = drbbs["geom"].ToString();
                                blockboundarieslist.Add(bbinfo);
                            }
                        }
                        if (dtgpboundaries.Rows.Count > 0)
                        {
                            foreach (DataRow drgpbs in dtgpboundaries.Rows)
                            {
                                gpboundaries gpbinfo = new gpboundaries();
                                gpbinfo.gp_name = drgpbs["panchayat_name"].ToString();
                                gpbinfo.geom = drgpbs["geom"].ToString();
                                gpboundarieslist.Add(gpbinfo);
                            }
                        }
                        if (dtvillageboundaries.Rows.Count > 0)
                        {
                            foreach (DataRow drgpbs in dtvillageboundaries.Rows)
                            {
                                villageboundaries vilbinfo = new villageboundaries();
                                vilbinfo.village_name = drgpbs["village_name"].ToString();
                                vilbinfo.geom = drgpbs["geom"].ToString();
                                villageboundarieslist.Add(vilbinfo);
                            }
                        }
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
                                string housecode = dr["HouseCode"].ToString();
                                foreach (DataRow drb in dtbuildinggeom.Select("hhcode='" + housecode + "'"))
                                {
                                    string hcodegeom = drb["geom"].ToString();
                                    survey.geom = hcodegeom;
                                }
                                surveybenelist.Add(survey);
                            }
                        }

                        List<mapsandmatcheslist> getmapsandmatchesdtls = new List<mapsandmatcheslist>();
                        mapsandmatcheslist getoverDatas = new mapsandmatcheslist();
                        getoverDatas.getshroadslist = shroadslist;
                        getoverDatas.getgproadslist = gproadslist;
                        getoverDatas.getvillageroadslist = villageroadslist;
                        getoverDatas.getblockboundarieslist = blockboundarieslist;
                        getoverDatas.getgpboundarieslist = gpboundarieslist;
                        getoverDatas.getvillageboundarieslist = villageboundarieslist;
                        getoverDatas.getsurveybenelist = surveybenelist;
                        getmapsandmatchesdtls.Add(getoverDatas);
                        JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                        jsonSerializer.MaxJsonLength = Int32.MaxValue;
                        string response = jsonSerializer.Serialize(getmapsandmatchesdtls);
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
            Context.Response.StatusCode = 401;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }

    public class Village
    {
        public string Villagecode { get; set; }
        public string Villagename { get; set; }
        public string GPcode { get; set; }
    }
}
