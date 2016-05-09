<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LogInfo.ascx.cs" Inherits="Satrabel.OpenContent.LogInfo" %>
<%@ Import Namespace="Newtonsoft.Json" %>

<script type="text/javascript">
    //$(document).ready(function () {
        var logs = <%= JsonConvert.SerializeObject(Satrabel.OpenContent.Components.Loging.LogContext.Curent.ModuleLogs(ModuleContext.ModuleId)) %>;
        //$.fn.openContent.printLogs('Module <%= ModuleContext.ModuleId %> - <%= ModuleContext.Configuration.ModuleTitle %>', logs);
    //});
</script>