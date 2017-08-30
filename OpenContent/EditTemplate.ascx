<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditTemplate" CodeBehind="EditTemplate.ascx.cs" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<%@ Register Assembly="DotnetNuke" Namespace="DotNetNuke.UI.WebControls" TagPrefix="dnn" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<%@ Register TagPrefix="dnnweb" Namespace="DotNetNuke.Web.UI.WebControls" Assembly="DotNetNuke.Web" %>

<%@ Register TagPrefix="My" Namespace="Satrabel.OpenContent" Assembly="OpenContent" %>

<%-- Custom CSS Registration --%>
<dnn:DnnCssInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/lib/codemirror.css" />
<dnn:DnnCssInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/hint/show-hint.css" />
<dnn:DnnCssInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/lint/lint.css" />
<%-- Custom JavaScript Registration --%>
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/lib/codemirror.js" Priority="101" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/clike/clike.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/vb/vb.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/xml/xml.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/javascript/javascript.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/css/css.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/htmlmixed/htmlmixed.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/hint/show-hint.js" Priority="103" />

<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/lint/lint.js" Priority="102" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/jsonlint.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/lint/json-lint.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/csslint.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/lint/css-lint.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/jshint.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/lint/javascript-lint.js" Priority="103" />

<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/mode/multiplex.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/addon/mode/simple.js" Priority="103" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/handlebars/handlebars.js" Priority="104" />
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/oc.codeMirror.js" Priority="105" />

<div class="dnnForm dnnRazorHostEditScript dnnClear" id="dnnEditScript">
    <fieldset class="nomargin">
        <div class="dnnFormItem" style="padding:5px;background-color:gray;width:auto;border-radius: 5px 5px 0 0;">

        <My:GroupDropDownList runat="server" ID="scriptList" AutoPostBack="true" CssClass="nomargin">
        
        </My:GroupDropDownList>

            
            <div style="float:right;padding-right:10px;padding-top:8px;">
            <asp:Label ID="plSource" ControlName="txtSource" runat="server" ForeColor="White" />
                </div>
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
        <li>

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

            var cm = ocSetupCodeMirror(mimeType, $("textarea[id$='txtSource']")[0], model);

            var resizeModule = function resizeDnnEditHtml() {
                //$('#dnnEditScript fieldset').height($(window).height() - $('#dnnEditScript ul dnnActions').height() - 18 - 52);
                //$('window.frameElement, body, html').css('overflow', 'hidden');
                var containerHeight = $(window).height() - 52 - 32 - 0;
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
                jQuery('html').css('overflow', 'hidden');
                popup.css('padding-top', '0');
                //windowTop.jQuery(".ui-dialog-title").hide();
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
