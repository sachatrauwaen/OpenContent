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
<dnn:DnnJsInclude runat="server" FilePath="~/DesktopModules/OpenContent/js/CodeMirror/mode/handlebars/handlebars.js" Priority="103" />

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
        
        <li>
            Ctrl-Space : variables | 
        </li>
        <li>
            Shift-Space : helpers | 
        </li>
        <li>
            <a href="https://opencontent.readme.io/docs/tokens" target="_blank">Help</a>
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

        var handlebarsHelpers = [     
            {'text':'{{#each var}}{{/each}}', 'displayText': 'each', 'sort':'each'},
            {'text':'{{#if var}}{{/if}}', 'displayText': 'if', 'sort': 'if'},
            {'text':'{{#ifand var1 var2 var3}}{{/ifand}}', 'displayText': 'ifand', 'sort': 'ifand'},
            {'text':'{{#ifor var1 var2 var3}}{{/ifor}}', 'displayText': 'ifor', 'sort': 'ifor'},
            {'text':'{{multiply var 2}}', 'displayText': 'multiply', 'sort': 'multiply'},
            {'text':'{{divide var 2}}', 'displayText': 'divide', 'sort': 'divide'},
            {'text':'{{add var 2}}', 'displayText': 'add', 'sort': 'add'},
            {'text':'{{substract var 2}}', 'displayText': 'substract', 'sort': 'substract'},
            {'text':'{{registerscript "javascript.js"}}', 'displayText': 'registerscript', 'sort': 'registerscript'},
            {'text':'{{registerstylesheet "stylesheet.css"}}', 'displayText': 'registerstylesheet', 'sort': 'registerstylesheet'},
            {'text':'{{formatNumber var "0.00"}}', 'displayText': 'formatNumber', 'sort': 'formatNumber'},
            {'text':'{{formatDateTime var "dd/MMM/yy" "nl-NL" }}', 'displayText': 'formatDateTime', 'sort': 'formatDateTime'},
            {'text':'{{convertHtmlToText var }}', 'displayText': 'convertHtmlToText', 'sort': 'convertHtmlToText'},
            {'text':'{{convertToJson var }}', 'displayText': 'convertToJson', 'sort': 'convertToJson'},
            {'text':'{{truncateWords var 50 "..." }}', 'displayText': 'formatDateTime', 'sort': 'formatDateTime'},
            {'text':'{{#equal var "value"}}{{/equal}}', 'displayText': 'equal', 'sort': 'equal'},
            {'text':'{{#unless var}}{{/unless}}', 'displayText': 'unless', 'sort': 'unless'},                    
            {'text':'{{#with var}}{{/with}}', 'displayText': 'with', 'sort': 'with'}

        ];

        var handlebarsHints = [
                    {'text':'{{#each Items}}{{/each}}', 'displayText': '#each', 'sort':'#each'},
                    {'text':'{{#unless var}}{{/unless}}', 'displayText': '#unless', 'sort': '#unless'},                    
                    {'text':'{{#if var}}{{/if}}', 'displayText': '#if', 'sort': '#if'}];


        var schema = <%= Schema.ToString() %>;

        var addProperties = function(sch, parent, prefix){
            if (sch && sch.properties){
                for (k in sch.properties) {            
                    p = sch.properties[k];
                    var vartext = (prefix ? prefix+'.':'')+k;
                    if (p.type == 'object'){
                        //handlebarsHints.push({'text':'{{#with '+k+'}} {{/with}}', 'displayText': k+ (parent ? ' ('+ parent+')' : ''), 'sort': (parent ? parent+'.' : '')+k});
                    } else if (p.type == 'array'){
                        handlebarsHints.push({'text':'{{#each '+vartext+'}} {{/each}}', 'displayText': vartext + (parent ? ' ('+ parent+')' : ''), 'sort': (parent ? parent+'.' : '')+k});
                    } else {
                        handlebarsHints.push({'text':'{{'+vartext+'}}', 'displayText': vartext+ (parent ? ' ('+ parent+')' : ''), 'sort': (parent ? parent+'.' : '')+k});
                    }

                    if(k == 'Image'){
                        handlebarsHints.push({'text':'<img src="{{'+vartext+'}}" alt="" />', 'displayText': vartext+ (parent ? ' <img> ('+ parent+')' : ''), 'sort': (parent ? parent+'.' : '')+k});
                    }

                    if (p.type == 'object'){
                        addProperties(p, '', k);
                    } else if (p.type == 'array' && p.items){
                        addProperties(p.items, k, '');
                    }
                }
            }
        };
        if (schema){
            addProperties(schema, '', '');
        };
    
        CodeMirror.registerHelper("hint", "htmlhandlebars", function (editor) {
            var list = handlebarsHints;
            var cursor = editor.getCursor();
            var currentLine = editor.getLine(cursor.line);
            var start = cursor.ch;
            var end = start;
            while (end < currentLine.length && /[\w$]+/.test(currentLine.charAt(end))) ++end;
            while (start && /[\w$]+/.test(currentLine.charAt(start - 1))) --start;
            var curWord = start != end && currentLine.slice(start, end);            
            var regex = new RegExp('^' + curWord, 'i');
            var result = {
                list: (!curWord ? list : list.filter(function (item) {
                    return item.displayText.match(regex);
                })).sort(function compare(a,b) {
                  if (a.sort < b.sort)
                    return -1;
                  if (a.sort > b.sort)
                    return 1;
                  return 0;
                }),
                from: CodeMirror.Pos(cursor.line, start),
                to: CodeMirror.Pos(cursor.line, end)
            };
            return result;
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
                mode: mimeType,
                extraKeys: {"Ctrl-Space": "autocomplete","Shift-Space": function(editor){ 
                    var options = {
                        hint: function() {

                            var list = handlebarsHelpers;
                            var cursor = editor.getCursor();
                            var currentLine = editor.getLine(cursor.line);
                            var start = cursor.ch;
                            var end = start;
                            while (end < currentLine.length && /[\w$]+/.test(currentLine.charAt(end))) ++end;
                            while (start && /[\w$]+/.test(currentLine.charAt(start - 1))) --start;
                            var curWord = start != end && currentLine.slice(start, end);            
                            var regex = new RegExp('^' + curWord, 'i');
                            var result = {
                                list: (!curWord ? list : list.filter(function (item) {
                                    return item.displayText.match(regex);
                                })).sort(function compare(a,b) {
                                  if (a.sort < b.sort)
                                    return -1;
                                  if (a.sort > b.sort)
                                    return 1;
                                  return 0;
                                }),
                                from: CodeMirror.Pos(cursor.line, start),
                                to: CodeMirror.Pos(cursor.line, end)
                            };
                            return result;
/*
                          return {
                            from: editor.getDoc().getCursor(),
                              to: editor.getDoc().getCursor(),
                            list: handlebarsHelpers
                          }
*/
                        }
                      };
                      editor.showHint(options);
                } },
                hintOptions: {hint: CodeMirror.hint.htmlhandlebars}
            });
/*
            cm.on("inputRead", function(editor, change) {
              if (change.text[0] == "{"){
                var options = {
                    hint: function() {
                      return {
                        from: editor.getDoc().getCursor(),
                          to: editor.getDoc().getCursor(),
                        list: handlebarsHints
                      }
                    }
                  };
                  editor.showHint(options);
                }
            });
*/
            var resizeModule = function resizeDnnEditHtml() {
                //$('#dnnEditScript fieldset').height($(window).height() - $('#dnnEditScript ul dnnActions').height() - 18 - 52);
                //$('window.frameElement, body, html').css('overflow', 'hidden');


                var containerHeight = $(window).height() - 52 - 52 - 40 ;

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
