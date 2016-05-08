<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LogInfo.ascx.cs" Inherits="Satrabel.OpenContent.LogInfo" %>
<%@ Import Namespace="Newtonsoft.Json" %>

<script type="text/javascript">
    $(document).ready(function () {
        var Logs = <%= JsonConvert.SerializeObject(Satrabel.OpenContent.Components.Loging.LogContext.Curent.Logs) %>;
        if (window.console) {

            for (var i in Logs) {
                console.group(i);
                for (var j = 0; j < Logs[i].length; j++) {
                    console.log(Logs[i][j].Label, Logs[i][j].Message);
                }
                console.groupEnd();
            }
        }
    });
</script>