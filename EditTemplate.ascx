<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditTemplate" CodeBehind="EditTemplate.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
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
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/mode/multiplex.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/mode/simple.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/handlebars/handlebars.js" Priority="103" />

<div class="dnnForm dnnRazorHostEditScript dnnClear" id="dnnEditScript">
    <fieldset>
        <div class="dnnFormItem">
            <dnn:Label id="scriptsLabel" runat="Server" controlname="scriptList" />
            <asp:DropDownList ID="scriptList" runat="server" AutoPostBack="true" />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="Label1" ControlName="txtSource" runat="server" CssClass="dnnLabel" Text="" />
            <asp:Label ID="plSource" ControlName="txtSource" runat="server" />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="Label2" runat="server" />
        </div>
        <div>
            <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="30" Columns="140" />
        </div>
    </fieldset>
    <asp:Label ID="lError" runat="server" Visible="false" CssClass="dnnFormMessage dnnFormValidationSummary"></asp:Label>
    <ul class="dnnActions dnnClear">
        <li>
            <asp:LinkButton ID="cmdSave" resourcekey="cmdSave" runat="server" CssClass="dnnPrimaryAction" /></li>
        <li>
            <asp:LinkButton ID="cmdSaveClose" resourcekey="cmdSaveClose" runat="server" CssClass="dnnSecondaryAction" /></li>
        <li>
            <asp:LinkButton ID="cmdCancel" resourcekey="cmdCancel" runat="server" CssClass="dnnSecondaryAction" CausesValidation="False" />
        </li>
        <li>
            <asp:LinkButton ID="cmdCustom" resourcekey="cmdCustom" runat="server" CssClass="dnnSecondaryAction" Visible="false" />
        </li>
        <li>
            <asp:LinkButton ID="cmdBuilder" resourcekey="cmdBuilder" runat="server" CssClass="dnnSecondaryAction" />
        </li>

    </ul>
</div>
<script type="text/javascript">

    jQuery(function ($) {
        var mimeType = dnn.getVar('mimeType') || "text/html";

        CodeMirror.defineMode("htmlhandlebars", function (config) {
            return CodeMirror.multiplexingMode(
              CodeMirror.getMode(config, "text/html"),
              {
                  open: "{{", close: "}}",
                  mode: CodeMirror.getMode(config, "handlebars"),
                  parseDelimiters: true
              });
        });

        var setupModule = function () {

            $('#<%= cmdCustom.ClientID %>').dnnConfirm({
                text: '<%= Localization.GetSafeJSString("OverwriteTemplate.Text") %>',
                yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
            });


            var cm = CodeMirror.fromTextArea($("textarea[id$='txtSource']")[0], {
                lineNumbers: true,
                matchBrackets: true,
                lineWrapping: true,
                mode: mimeType
            });

            var resizeModule = function resizeDnnEditHtml() {
                //$('#dnnEditScript fieldset').height($(window).height() - $('#dnnEditScript ul dnnActions').height() - 18 - 52);
                //$('window.frameElement, body, html').css('overflow', 'hidden');


                var containerHeight = $(window).height() - 18 - 52 - 52 - 30 - 30;

                //$('.editorContainer').height(containerHeight - $('.editorContainer').offset().top - 110);
                //$('.editorContainer').height(containerHeight - 250);
                $('#dnnEditScript .CodeMirror').height(containerHeight);

                cm.refresh();
            };
            var windowTop = parent;
            var popup = windowTop.jQuery("#iPopUp");
            if (popup.length) {

                var $window = $(windowTop),
                                newHeight,
                                newWidth;

                newHeight = $window.height() - 36;
                newWidth = Math.min($window.width() - 40, 1200);

                popup.dialog("option", {
                    close: function () { window.dnnModal.closePopUp(false, ""); },
                    //'position': 'top',
                    height: newHeight,
                    width: newWidth,
                    //position: 'center'
                    resizable: false,
                });
            }

            if (window.frameElement && window.frameElement.id == "iPopUp") {

                resizeModule();

                $(window).resize(function () {
                    var timeout;
                    if (timeout) clearTimeout(timeout);
                    timeout = setTimeout(function () {
                        timeout = null;
                        resizeModule();
                    }, 50);
                });
            }

        };

        setupModule();

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {

            // note that this will fire when _any_ UpdatePanel is triggered,
            // which may or may not cause an issue
            setupModule();

        });
    });

</script>
