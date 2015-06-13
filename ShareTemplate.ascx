<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.ShareTemplate" CodeBehind="ShareTemplate.ascx.cs" %>
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
        <div class="dnnFormItem">
            <dnn:Label ID="lblAction" ControlName="scriptList" runat="server" />
            <asp:DropDownList ID="rblAction" runat="server" AutoPostBack="true" OnSelectedIndexChanged="rblAction_SelectedIndexChanged">
                <asp:ListItem Text="--select--" ></asp:ListItem>
                <asp:ListItem Text="Import from file" Value="importfile"></asp:ListItem>
                <asp:ListItem Text="Export" Value="exportfile"></asp:ListItem>
                <asp:ListItem Text="Import from web" Value="importweb"></asp:ListItem>
                <asp:ListItem Text="Copy template" Value="copy"></asp:ListItem>
            </asp:DropDownList>
        </div>
    </fieldset>
</div>

<asp:PlaceHolder ID="phImport" runat="server" Visible="false">
    <div class="dnnForm dnnImport dnnClear" id="dnnImport">
        <fieldset>
            <div class="dnnFormItem">
                <dnn:Label ID="lblFile" ControlName="fuFile" runat="server" />
                <asp:FileUpload ID="fuFile" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="lblImportName" ControlName="tbImportName" runat="server" />
                <asp:TextBox runat="server" ID="tbImportName" /> 
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear" style="display:block;padding-left:35%">
            <li>
                <asp:LinkButton ID="cmdImport" resourcekey="cmdImport" runat="server" CssClass="dnnPrimaryAction" OnClick="cmdImport_Click" />
            </li>
        </ul>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="phExport" runat="server" Visible="false">
    <div class="dnnForm dnnExport dnnClear" id="dnnExport">
        <fieldset>

            <div class="dnnFormItem">
                <dnn:Label ID="lblTemplates" ControlName="ddlTemplates" runat="server" />
                <asp:DropDownList ID="ddlTemplates" runat="server" />
            </div>
             <div class="dnnFormItem">
                <dnn:Label ID="lblExportName" ControlName="tbName" runat="server" />
                <asp:TextBox runat="server" ID="tbExportName" /> 
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear" style="display:block;padding-left:35%">
            <li>
                <asp:LinkButton ID="cmdExport" resourcekey="cmdExport" runat="server" CssClass="dnnPrimaryAction" OnClick="cmdExport_Click" />
            </li>
          
        </ul>
    </div>

</asp:PlaceHolder>

<asp:PlaceHolder ID="phImportWeb" runat="server" Visible="false">
    <div class="dnnForm dnnImport dnnClear" id="dnnImportWeb">
        <fieldset>
            <div class="dnnFormItem">
                <dnn:Label ID="lblWebTemplates" ControlName="ddlWebTemplates" runat="server" />
                <asp:DropDownList ID="ddlWebTemplates" runat="server"></asp:DropDownList>
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="lblMoreinfo" ControlName="fuFile" runat="server" />
                <asp:HyperLink ID="hlMoreInfo" runat="server" NavigateUrl="http://www.openextensions.net/templates/open-content" Target="_blank">Template exchange on OpenExtensions.net</asp:HyperLink>
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear" style="display:block;padding-left:35%">
            <li>
                <asp:LinkButton ID="cmdImportWeb" resourcekey="cmdImport" runat="server" CssClass="dnnPrimaryAction" OnClick="cmdImportWeb_Click" />
            </li>
        </ul>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="phCopy" runat="server" Visible="false">
    <div class="dnnForm dnnImport dnnClear" id="dnnCopy">
        <fieldset>
            <div class="dnnFormItem">
                <dnn:Label ID="lCopyTemplates" ControlName="ddlTemplates" runat="server" />
                <asp:DropDownList ID="ddlCopyTemplate" runat="server" />
            </div>
            <div class="dnnFormItem">
                <dnn:Label ID="lCopyName" ControlName="tbCopyName" runat="server" />
                <asp:TextBox runat="server" ID="tbCopyName" /> 
            </div>
        </fieldset>
        <ul class="dnnActions dnnClear" style="display:block;padding-left:35%">
            <li>
                <asp:LinkButton ID="lbCopy" resourcekey="cmdCopy" runat="server" CssClass="dnnPrimaryAction" OnClick="lbCopy_Click" />
            </li>
        </ul>
    </div>
</asp:PlaceHolder>

<script type="text/javascript">
    jQuery(function ($) {

        var setupModule = function () {

        };

        setupModule();

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {

            // note that this will fire when _any_ UpdatePanel is triggered,
            // which may or may not cause an issue
            setupModule();

        });
    });
</script>
