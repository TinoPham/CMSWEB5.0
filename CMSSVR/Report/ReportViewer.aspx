<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="CMSSVR.Report.ReportViewer" %>


<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml"  style="height: 100%;">
<head runat="server">
    <title></title>

    <link href="../Content/libs/font/css/font-optimize.css" rel="stylesheet" />
    <style>

        #ReportViewerManager_fixedTable
        {
             width: 100% !important;
             
        }

        /*#ReportViewerManager_ctl05
        {

        }*/

        #ReportViewerManager_ctl09 div > table
        {
            margin: 0 auto !important;
        }

        /*----------------------------------------------------
        <!-- SET ICON CONTROL CUSTOMREPORT -->
        ----------------------------------------------------*/   
 
     .ReportViewerManagerZoomOut{
        
        height:24px;
        width:26px;
       
        cursor:default
        }

        .icon-zoom-in-custom_enable
        {
            background-color: transparent !important;
            border-color:transparent !important;
            cursor:pointer;
        }   

        .icon-zoom-in-custom_enable:hover{
            border-color:#336699 !important;
            background-color: rgb(221, 238, 247) !important;
        }

        .icon-zoom-in-custom_disable
        {
            border-color:#336699 !important;
            background-color: rgb(221, 238, 247) !important;
            cursor:default;
        }   

        .icon-zoom-in-custom{
            border-style:solid !important;
            border-width: 1px !important;
            display:inline-block;
            height:24px;
            width:24px;
            text-align:center;
            vertical-align:middle;
        }

        

        .icon-zoom-in-custom:before { 
            content: '\e9b4'; 
            height:22px;
            width:22px;
            font-size: 14px;
            line-height: 1.5;
        }

        .icon-zoom-out-custom_enable
        {
            background-color: transparent !important;
            border-color:transparent !important;
            cursor:pointer;
        }   

        .icon-zoom-out-custom_enable:hover{
            border-color: #336699 !important;
            background-color: rgb(221, 238, 247) !important;
        }

        .icon-zoom-out-custom_disable
        {
            border-color:#336699 !important;
            background-color: rgb(221, 238, 247) !important;
            cursor:default;
        }   

        .icon-zoom-out-custom{
            border-style:solid;
            border-width: 1px;
            display:inline-block;
            height:24px;
            width:24px;
            text-align:center;
            vertical-align:middle;
        }

        

        .icon-zoom-out-custom:before { 
            content: '\e9b5'; 
            height:22px;
            width:22px;
            font-size: 14px;
            line-height: 1.5;
        }
    </style>



</head>
<body  style="height: 100%; margin: 0;">
    <form id="form1" runat="server"  style="height: 100%; margin: 0;">
        <% if (FailLoadReport == true)
           { %>
        <div class="error-load">
            <h1>Generate report failed, please contact administrator.</h1>
        </div>
        <% }
           else
           { %>

        <div style="height: 100%;" align="center">
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ZoomMode="Percent" ShowZoomControl="False" ShowPrintButton="True" Style="height: 100%; width: 100%" ID="ReportViewerManager" runat="server" SizeToReportContent="true"  BackColor="White"></rsweb:ReportViewer>
        </div>
   

        <% } %>
    </form>

    <script>
        function ClickZoomIn() {

            var form = document.getElementById("VisibleReportContentReportViewerManager_ctl09");
            if (form.offsetWidth > 0 && form.offsetWidth !== undefined) {
                var table = form.firstChild;
                do {
                    if (table === undefined || table === null) {
                        return;
                    }
                    if (table.localName === "table") {
                        break;
                    }
                    table = table.firstChild;
                }
                while (1);

                if (table !== undefined && table !== null) {
                    
                    var zoomPercent;
                    if (table.style.zoom !== "" && table.style.zoom != undefined && table.style.zoom != null) {
                        zoomPercent = parseFloat(table.style.zoom) + 0.25;
                    }
                    if (zoomPercent <= 2.5) {
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomOut").style.opacity = 1;
                        //changeClass(document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomOut").children[0], " icon-zoom-out-custom_disable", " icon-zoom-out-custom_enable");
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomOut").children[0].className = "icon-zoom-out-custom icon-zoom-out-custom_enable"
                        table.style.zoom = zoomPercent;
                        table.style.MozTransform = 'scale(' + zoomPercent + ')';
                        table.style.MozTransformOrigin = 'top';
                    } else {
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomIn").style.opacity = 0.3;
                        //changeClass(document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomIn").children[0], " icon-zoom-in-custom_enable", " icon-zoom-in-custom_disable");
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomIn").children[0].className = "icon-zoom-in-custom icon-zoom-in-custom_disable";
                    }
                    
                }
            }
        }

        function ClickZoomOut() {
            var form = document.getElementById("VisibleReportContentReportViewerManager_ctl09");
            if (form.offsetWidth > 0 && form.offsetWidth !== undefined) {
                var table = form.firstChild;
                do {
                    if (table === undefined || table === null) {
                        return;
                    }
                    if (table.localName === "table") {
                        break;
                    }
                    table = table.firstChild;
                }
                while (1);

                if (table !== undefined && table !== null) {
                    var zoomPercent;

                    if (table.style.zoom !== "" && table.style.zoom != undefined && table.style.zoom != null) {
                        zoomPercent = parseFloat(table.style.zoom) - 0.25;
                    }

                    if (zoomPercent > 0) {
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomIn").style.opacity = 1;
                        //changeClass(document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomIn").children[0], " icon-zoom-in-custom_disable", " icon-zoom-in-custom_enable");
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomIn").children[0].className = "icon-zoom-in-custom icon-zoom-in-custom_enable";
                        table.style.zoom = zoomPercent;
                        table.style.MozTransform = 'scale(' + zoomPercent + ')';
                        table.style.MozTransformOrigin = 'top';
                    } else {
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomOut").style.opacity = 0.3;
                        //changeClass(document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomOut").children[0], " icon-zoom-out-custom_enable", " icon-zoom-out-custom_disable");
                        document.getElementById("ReportViewerManager_ctl05_ctl00_ZoomOut").children[0].className = "icon-zoom-out-custom icon-zoom-out-custom_disable";
                    }

                }
            }
        }

        function changeClass(object, oldClass, newClass) {
            // remove:
            //object.className = object.className.replace( /(?:^|\s)oldClass(?!\S)/g , '' );
            // replace:
            var regExp = new RegExp('(?:^|\\s)' + oldClass + '(?!\\S)', 'g');
            object.className = object.className.replace(regExp, newClass);
            // add
            //object.className += " "+newClass;
        }

    </script>
</body>
</html>



