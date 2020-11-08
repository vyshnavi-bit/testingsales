<%@ Page Language="C#" AutoEventWireup="true" CodeFile="statusrpt.aspx.cs" Inherits="statusrpt" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>360 </title>
    <meta charset="UTF-8" />
    <link rel="shortcut icon" href="https://#/themes/default/assets/images/icon.png" />
    <meta content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no"
        name="viewport" />
    <link href="css/styles.css" rel="stylesheet" type="text/css" />
    <script src="css/jQuery-2.1.4.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            var date = new Date();
            var day = date.getDate();
            var month = date.getMonth() + 1;
            var year = date.getFullYear();
            if (month < 10) month = "0" + month;
            if (day < 10) day = "0" + day;
            today = year + "-" + month + "-" + day;
            $('#txt_frmdate').val(today);
            $('#txt_todate').val(today);6
        });
        function btngenarateclick() {
            get_serveyresponce_dtls();
        }
        function get_serveyresponce_dtls() {
            var fromdate = document.getElementById('txt_frmdate').value;
            var todate = document.getElementById('txt_todate').value;
            var data = { 'op': 'get_serveyresponce_dtls', 'fromdate': fromdate, 'todate': todate };
            var s = function (msg) {
                if (msg) {
                    if (msg.length > 0) {
                        filldtls(msg);
                    }
                    else {
                    }
                }
                else {
                }
            };
            var e = function (x, h, e) {
            }; $(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);
            callHandler(data, s, e);
        }
        function filldtls(msg) {
            var results = '<div class="box-body"><table class="table table-bordered table-hover dataTable no-footer" role="grid" aria-="" describedby="example2_info" id="tbl_Stores_value">';
            results += '<thead><tr role="row"><th>Sno</th><th scope="col">From Date</th><th scope="col">To Date</th><th scope="col">Status</th><th scope="col">Count</th><th scope="col" style="font-weight: bold;">Actions</th></tr></thead></tbody>';
            var tcount = 0;
            for (var i = 0; i < msg.length; i++) {
                var k = i + 1;
                results += '<tr>';
                results += '<td scope="row" class="1" >' + k + '</td>';
                results += '<td scope="row" class="2" style="text-align: center; font-weight: bold;" >' + msg[i].fromdate + '</td>';
                results += '<td scope="row" class="3" style="text-align: center; font-weight: bold;" >' + msg[i].todate + '</td>';
                if (msg[i].status == "1") {
                    results += '<td scope="row" class="4" style="text-align: center; font-weight: bold;" >Saved</td>';
                    document.getElementById('spntotsaveresp').innerHTML = msg[i].count;
                }
                else {
                    results += '<td scope="row" class="4" style="text-align: center; font-weight: bold;" >Submitted</td>';
                    document.getElementById('spntotsubresp').innerHTML = msg[i].count;
                }

                results += '<td scope="row" class="4" style="text-align: center; font-weight: bold;" >' + msg[i].count + '</td>';
                results += '<td style="text-align: center;"><a href="#" title="View" class="tip btn btn-primary btn-xs" data-toggle="ajax"><i class="fa fa-file-text-o"></i></a></td>';
                results += '<td style="display:none" class="16"></td></tr>';
                tcount += parseFloat(msg[i].count);
            }
            document.getElementById('spntotresp').innerHTML = tcount;
            results += '</table></div>';
            $("#divresponsedtls").html(results);
        }

       
        function getedit(thisid) {
        }

        function callHandler(d, s, e) {
            $.ajax({
                url: 'FleetManagementHandler.axd',
                data: d,
                type: 'GET',
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                async: true,
                cache: true,
                success: s,
                Error: e
            });
        }
        function CallHandlerUsingJson(d, s, e) {
            d = JSON.stringify(d);
            d = encodeURIComponent(d);
            $.ajax({
                type: "GET",
                url: "FleetManagementHandler.axd?json=",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: d,
                async: true,
                cache: true,
                success: s,
                error: e
            });
        }


        function getAnnotations() {

            window.AmCharts.cachedAnnotations = chart.export.getAnnotations({}, function (items) {
                alert("Cached " + items.length + " annotations!\nReenter the annotation mode to repply the cached annotations!");
                console.log("Cached items: ", items);
            });
        }

        function applyAnnotations() {
            if (!window.AmCharts.cachedAnnotations || !window.AmCharts.cachedAnnotations.length) {
                alert("No cached annotations available!");
                return;
            }
            chart.export.setAnnotations({
                data: window.AmCharts.cachedAnnotations
            });
        }
    </script>
    <script type="text/javascript">
        
    </script>
</head>
<body class="skin-green fixed sidebar-mini">
    <div class="wrapper rtl rtl-inv">
        <header class="main-header">
        <nav class="navbar navbar-static-top" role="navigation">
            <div class="navbar-custom-menu">
                <ul class="nav navbar-nav">
                     <li class="dropdown user user-menu" style="padding-right:5px;">
                        <a href="#" class="dropdown-toggle" data-toggle="dropdown">
                           <img src="images/male.png" class="user-image" alt="Avatar"/> 
                            <span id="lblmyname" class="hidden-xs">naveen</span>
                        </a>
                        <ul class="dropdown-menu" style="padding-right:3px;">
                            <li class="user-header">
                                <img src="images/male.png" class="img-circle" alt="Avatar">
                                <p>
                                     <span id="lblRole" class="hidden-xs">SuperAdmin</span> 
                                </p>
                            </li>
                            <li class="user-footer">
                                <div class="pull-left">
                                    <a href="Switchaccounts.aspx" class="btn btn-default btn-flat">Switch To Accouunt</a>
                                </div>
                                <div class="pull-right">
                                    <a href="login.aspx" class="btn btn-default btn-flat sign_out">Sign Out</a>
                                </div>
                            </li>
                        </ul>
                    </li>
                </ul>
            </div>
        </nav>
    </header>
        <div class="content-wrapper">
        <section class="content-header">
    <script type="text/javascript">
        $('body').addClass('skin-green sidebar-collapse sidebar-mini pos');
    </script>
    <div class="box-body">
        <div class="col-sm-12">
        <div class="row">
        <div class="row">
           <div align="center">
                            <table>
                                <tbody><tr>
                                    <td>
                                        <span id="lblfrom_date" style="font-weight:bold;">From Date</span>&nbsp;

                                        <input type="date" id="txt_frmdate" class="form-control" />
                                    </td>
                                    <td style="width:6px;"></td>
                                    <td>
                                        <span id="lblto_date" style="font-weight:bold;">To Date</span>&nbsp;
                                        <input type="date" id="txt_todate" class="form-control" />
                                        
                                    </td>
                                    <td style="width:6px;"></td>
                                    <td style="padding-top:20px;">
                                        <span id="Label1"></span>&nbsp;
                                        <input type="submit" name="Button2" value="GENERATE" id="Button2" class="btn btn-primary" onclick="btngenarateclick();" /><br>
                                    </td>
                                </tr>
                            </tbody></table>
                        </div><br />
                        </div>
                         <div class="clearfix">
            </div>
            <div class="row">
                <div class="col-md-4 col-sm-6 col-xs-12">
                    <div class="info-box bg-aqua">
                        <span class="info-box-icon"><i class="fa fa-shopping-cart" style="line-height: 2 !important;"></i></span>
                        <div class="info-box-content">
                            <span class="info-box-text">Total Survey Responses</span> <span class="info-box-number" id="spntotresp">0</span>
                            <div class="progress">
                                <div style="width: 100%" class="progress-bar">
                                </div>
                            </div>
                            <span class="progress-description" id="spnsalevalueprg">0</span>
                        </div>
                    </div>
                </div>
                <div class="col-md-4 col-sm-6 col-xs-12">
                    <div class="info-box bg-yellow">
                        <span class="info-box-icon"><i class="fa fa-plus"  style="line-height: 2 !important;"></i></span>
                        <div class="info-box-content">
                            <span class="info-box-text">Total Saved Responses</span> <span class="info-box-number" id="spntotsaveresp">0.00</span>
                            <div class="progress">
                                <div style="width: 0%" class="progress-bar">
                                </div>
                            </div>
                            <span class="progress-description" id="spnpurchaseprg">0 </span>
                        </div>
                    </div>
                </div>
                <div class="col-md-4 col-sm-6 col-xs-12">
                    <div class="info-box bg-red">
                        <span class="info-box-icon"><i class="fa fa-circle-o"  style="line-height: 2 !important;"></i></span>
                        <div class="info-box-content">
                            <span class="info-box-text">Total Submited Responses</span> <span class="info-box-number" id="spntotsubresp">0.00</span> 
                            <div class="progress">
                                <div style="width: 0%" class="progress-bar">
                                </div>
                            </div>
                            <span class="progress-description" id="spnexpencesprg">0</span>
                        </div>
                    </div>
                </div>
                
            </div>
            <div class="clearfix">
            </div>
            
            <div class="row">
                <div class="col-xs-12">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="box box-success">
                                <div class="box-header">
                                    <h3 class="box-title">
                                         SurveyResponse Details</h3>
                                </div>
                                <div class="box-body">
                                    <div id="divresponsedtls" style="width: 100%; height: 300px;">
                                    </div>
                                    
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="box box-danger">
                                <div class="box-header">
                                    <h3 class="box-title">
                                         Mapping Code Wise Details</h3>
                                </div>
                                <div class="box-body">
                                    <div id="divexpences" style="width: 100%; height: 300px;">
                                    </div>
                                    
                                </div>
                            </div>
                        </div>
                        
                        
                    </div>
                </div>
            </div>
        </div>
    </div>
    </div>
        </section>
            <div class="clearfix">
            </div>
        </div>
        <footer class="main-footer">
    <div class="pull-right hidden-xs">
       
    </div>
    Copyright © 2020 Technowell. All rights reserved.
</footer>
    </div>
    
    <div id="ajaxCall">
        <i class="fa fa-spinner fa-pulse"></i>
    </div>
    <script src="css/libraries.min.js" type="text/javascript"></script>
    <script src="css/scripts.min.js" type="text/javascript"></script>
    <script src="css/spos_ad.min.js" type="text/javascript"></script>
</body>
</html>
