<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditData" CodeBehind="EditData.ascx.cs" %>
<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.css" />
<%-- Custom JavaScript Registration --%>
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.js" Priority="101" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/clike/clike.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/vb/vb.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/xml/xml.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/javascript/javascript.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/css/css.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/mode/htmlmixed/htmlmixed.js" Priority="103" />

<div class="dnnForm dnnRazorHostEditScript dnnClear" id="dnnEditScript">
    <fieldset>
        <div>
            <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="30" Columns="140" />

        </div>
    </fieldset>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdSave" resourcekey="cmdSave" runat="server" CssClass="dnnPrimaryAction" />
        </li>
        <li>
            <asp:LinkButton ID="cmdCancel" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" />
        </li>
    </ul>
</div>
<script type="text/javascript">

    jQuery(function ($) {
        var mimeType = dnn.getVar('mimeType') || "text/html";

        var setupModule = function () {
            CodeMirror.fromTextArea($("textarea[id$='txtSource']")[0], {
                lineNumbers: true,
                matchBrackets: true,
                lineWrapping: true,
                mode: 'application/json'
            });


        };

        setupModule();

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {

            // note that this will fire when _any_ UpdatePanel is triggered,
            // which may or may not cause an issue
            setupModule();

        });
    });

</script>
