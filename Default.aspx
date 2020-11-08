<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
 <meta content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" name="viewport" />
   <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js" type="text/javascript"></script>
    <title></title>
    <script type="text/javascript">
        $(function () {
            get_updatebeneficiary();
        });
        function loadempworkitemdetails() {
            var myarray = { "empid": "51636763", "lastsynced": ""};
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                headers: { 'token': '22E91F79-C396-4905-B680-583B7EA24D22', 'empcode': '51636763', 'uuid': '5eaf634de3c80200' },
                dataType: "JSON",
                type: "POST",
                url: "downloadservice.asmx/empworkitemdetails",
                success: function (data) {
                    var myval = data;
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                        document.getElementById('spndata').innerHTML = data.responseText;
                    }
                }
            });
        }

        function benficiarydownloadsave() {
            var myarray = { "workitemcode": "448", "surveyworkitemmappingcode": "719", "type": "MS", "empid": "51636763"};
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                headers: { 'token': 'B1091F1F-1F9E-446D-B50E-B65E696E4797', 'empcode': '51623721', 'uuid': 'e6ed257c0fb4e2e' },
                dataType: "JSON",
                type: "POST",
                url: "beneficiarydownload.asmx/benficiarydownloadsave",
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                        document.getElementById('spndata').innerHTML = data.responseText;
                    }
                }
            });
        }

        function benficiarystatusdownloadsave() {
            var myarray = { "workitemcode": "440", "surveyworkitemmappingcode": "719", "type": "MS", "empid": "516869337" };
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                dataType: "JSON",
                type: "POST",
                url: "beneficiarydownload.asmx/benficiarydownloadsave",
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                    }
                }
            });
        }

        function loadvillages() {
            var blockarray = [];
            blockarray.push("BL0001");
            var gparray = [];
            gparray.push("GP0001");
            gparray.push("GP0002");
            var myarray = { "empid": "516869337", "workitemcode": "440", "uuid": "234232gfggfdf", "blocks": blockarray, "gps": gparray };
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                dataType: "JSON",
                type: "POST",
                url: "beneficiarydownload.asmx/loadvillages",
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                    }
                }
            });
        }
        function loadgps() {
            var blockarray = [];
            blockarray.push("BL0001");
            var myarray = { "empid": "516869337", "workitemcode": "440", "uuid": "234232gfggfdf", "blocks": blockarray };
            var jsonval = JSON.stringify(myarray);
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                dataType: "JSON",
                type: "POST",
                url: "beneficiarydownload.asmx/loadgps",
                async: false,
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                    }
                }
            });
        }

        function loadblocks() {
            var myarray = { "empid": "51636763", "workitemcode": "448", "uuid": "234232gfggfdf"};
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                dataType: "JSON",
                type: "POST",
                url: "beneficiarydownload.asmx/loadblocks",
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                    }
                }
            });
        }


        function saveBeneficiary() {
            var myarray = { "empid": "51636763", "uuid": "234232gfggfdf", "workitemcode": "440", "selectedben": ["10003428", "10003429", null, "10003431"], "surveyworkitemmappingcode": "719" };
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                dataType: "JSON",
                type: "POST",
                url: "beneficiarydownload.asmx/saveBeneficiary",
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                    }
                }
            });
        }

        function get_beneficiarylist() {
            var blockarray = [];
            var villagearray = [];
            var gparray = [];
            blockarray.push("BL0001");
            gparray.push("GP0001");
            villagearray.push("VL000003");
            var myarray = { "empid": "516869337", "workitemcode": "440", "uuid": "234232gfggfdf", "blocks": blockarray, "villages": villagearray, "gps": gparray };
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                dataType: "JSON",
                type: "POST",
                url: "beneficiarydownload.asmx/getbenficiarylist",
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                    }
                }
            });
        }


        function get_updatebeneficiary() {
            updatebeneficiary = [];
            var dataarr = [{ "workitemcode": "448", "surveyworkitemmappingcode": "719", "type": "MS", "frequency": [{ "surveyworkitemmappingcode": "719", "surveydate": "2019-01-24"}]}];
            dataarr = JSON.stringify(dataarr);
            var myarray = { "empid": "51636763", "updatedata": dataarr };
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myarray),
                headers: { 'token': 'E2DFD5BF-0E6E-4E60-93B4-6D2033F6A4F4', 'empcode': '51636763', 'uuid': '5eaf634de3c80200' },
                dataType: "JSON",
                type: "POST",
                url: "statusservice.asmx/updatebeneficiarynew",
                success: function (data) {
                    var myval = data;
                    alert("success");
                },
                error: function (data) {
                    if (data == undefined) {
                        alert("Error : 465");
                    }
                    else {
                        alert(data.responseText);
                    }
                }
            });
        }

        function get_newformet() {
            var myaraydata = { "empid": "51636763", "lastsynced": "", "parentresponse": "{\"0\":{\"empid\":\"51636763\", \"surveyworkitemmappingcode\":\"721\",\"respondentcode\":\"10038912\",\"parentquestion\":\"Q101\",\"questioncode\":\"Q101\",\"sectioncode\":\"1\",\"answer\":\"Naveennew\",\"surveydate\":\"2020-05-19\",\"status\":\"1\",\"createdon\":\"2020-04-19 06:13 AM\",\"syncedon\":null},\"1\":{\"empid\":\"51623721\", \"surveyworkitemmappingcode\":\"1735\",\"respondentcode\":\"10010225\",\"parentquestion\":\"Q102\",\"questioncode\":\"Q102\",\"sectioncode\":\"1\",\"answer\":\"\",\"surveydate\":\"2020-04-19\",\"status\":\"2\",\"createdon\":\"2020-04-19 06:13 AM\",\"syncedon\":null},\"2\":{\"empid\":\"51623721\", \"surveyworkitemmappingcode\":\"1735\",\"respondentcode\":\"10010225\",\"parentquestion\":\"Q103\",\"questioncode\":\"Q103\",\"sectioncode\":\"1\",\"answer\":\"\",\"surveydate\":\"2020-04-19\",\"status\":\"1\",\"createdon\":\"2020-04-19 06:13 AM\",\"syncedon\":null}}", "childresponse": "{}", "surveydata": "{\"0\":{\"surveyworkitemmappingcode\":\"719\",\"workitemcode\":\"448\"}}" }
            var ip = JSON.stringify(myaraydata);
            $.ajax({
                contentType: "application/json;",
                data: JSON.stringify(myaraydata),
                dataType: "json",
                type: "POST",
                url: "uploadservice.asmx/sdnew",
                success: function (data) {
                    alert("success");
                },
                error: function (err) {
                    alert(err);
                }
            });
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    
    <div>
     <a href="#" onclick="emploginserviceclick();">EMPloginservice</a><br />
     <a href="#" onclick="loadempworkitemdetails();">empworkitemdetails</a><br />
     <a href="#" onclick="loadempworkitemdetails();">benficiarydownloadsave</a><br />
     <a href="#" onclick="loadempworkitemdetails();">benficiarystatusdownloadsave</a><br />
     <a href="#" onclick="loadempworkitemdetails();">getbenficiarylist</a><br />
     <a href="#" onclick="loadempworkitemdetails();">loadblocks</a><br />
     <a href="#" onclick="loadempworkitemdetails();">loadgps</a><br />
     <a href="#" onclick="loadempworkitemdetails();">loadvillages</a><br />
     <a href="#" onclick="loadempworkitemdetails();">saveBeneficiary</a><br />
     <a href="#" onclick="loadempworkitemdetails();">updatebeneficiarynew</a><br />
     <a href="#" onclick="loadempworkitemdetails();">sdnew</a><br />
     <span id="spndata"></span>
    </div>
    </form>
</body>
</html>
