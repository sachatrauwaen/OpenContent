<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditTemplate" CodeBehind="EditTemplate.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>
<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/lib/codemirror.css" />
<dnn:DnnCssInclude runat="server" FilePath="~/Resources/Shared/components/CodeEditor/addon/hint/show-hint.css" />
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
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/hint/show-hint.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/handlebars/handlebars.js" Priority="104" />

<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/oc.codeMirror.js" Priority="105" />

<div class="dnnForm dnnRazorHostEditScript dnnClear" id="dnnEditScript">
    <fieldset class="nomargin">
        <div class="dnnFormItem">
            <dnn:Label id="scriptsLabel" runat="Server" controlname="scriptList" />
            <asp:DropDownList ID="scriptList" runat="server" AutoPostBack="true" CssClass="nomargin" />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="Label1" ControlName="txtSource" runat="server" CssClass="dnnLabel" Text="" />
            <asp:Label ID="plSource" ControlName="txtSource" runat="server" />
        </div>
        <div>
            <asp:TextBox ID="txtSource" runat="server" TextMode="MultiLine" Rows="30" Columns="140" />
        </div>
    </fieldset>

    <ul class="dnnActions dnnClear" style="padding-top: 8px;">
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
        <asp:PlaceHolder ID="phHandlebars" runat="server">
            <li>Ctrl-Space : variables | 
            </li>
            <li>Shift-Space : helpers | 
            </li>
            <li>Shift-Ctrl-Space : snippets | 
            </li>
            <li>
                <a href="https://opencontent.readme.io/docs/tokens" target="_blank">Handlebars Help</a>
            </li>
        </asp:PlaceHolder>

    </ul>
    <div style="position: absolute; bottom: 50px; right: 10px;">
        <asp:Label ID="lError" runat="server" Visible="false" CssClass="dnnFormMessage dnnFormValidationSummary"></asp:Label>
    </div>
</div>
<script type="text/javascript">

    jQuery(function ($) {
        var mimeType = dnn.getVar('mimeType') || "text/html";

        var model = <%= Model.ToString() %>;
        

        ocInitCodeMirror(mimeType, model);

        var setupModule = function () {

            $('#<%= cmdCustom.ClientID %>').dnnConfirm({
                text: '<%= Localization.GetSafeJSString("OverwriteTemplate.Text") %>',
                yesText: '<%= Localization.GetSafeJSString("Yes.Text", Localization.SharedResourceFile) %>',
                noText: '<%= Localization.GetSafeJSString("No.Text", Localization.SharedResourceFile) %>',
                title: '<%= Localization.GetSafeJSString("Confirm.Text", Localization.SharedResourceFile) %>'
            });

            var cm = ocSetupCodeMirror(mimeType, $("textarea[id$='txtSource']")[0]);

            var resizeModule = function resizeDnnEditHtml() {
                //$('#dnnEditScript fieldset').height($(window).height() - $('#dnnEditScript ul dnnActions').height() - 18 - 52);
                //$('window.frameElement, body, html').css('overflow', 'hidden');


                var containerHeight = $(window).height() - 52 - 52 - 0;

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
                    minWidth: newWidth,
                    minHeight: newHeight,
                    //position: 'center'
                    resizable: false,
                });
                jQuery('html').css('overflow','hidden');
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
