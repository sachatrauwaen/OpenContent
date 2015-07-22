<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.Edit" Codebehind="Edit.ascx.cs" %>

<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/css/alpaca-dnn.css" AddTag="false" />
<dnncl:DnnJsInclude ID="DnnJsInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/handlebars/handlebars.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude2" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/web/alpaca.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />

<dnncl:DnnJsInclude ID="DnnJsInclude4" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/typeahead.js/dist/typeahead.bundle.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />

<script src="<%=ControlPath %>js/wysihtml/wysihtml-toolbar.js"></script>
<script src="<%=ControlPath %>js/wysihtml/parser_rules/advanced_opencontent.js"></script>  
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/views/dnn.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/ImageField.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/FileField.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/UrlField.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/CKEditorField.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/wysihtmlField.js"></script>

<dnncl:DnnCssInclude ID="DnnCssInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/css/font-awesome/css/font-awesome.min.css" AddTag="false" />

<asp:Panel ID="ScopeWrapper" runat="server">
    <div id="field1" class="alpaca"></div>
   <ul class="dnnActions dnnClear" style="display:block;padding-left:35%">
		<li><asp:HyperLink id="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" /></li>
		<li><asp:HyperLink id="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" /></li>
	</ul>
</asp:Panel>
<asp:HiddenField ID="CKDNNporid" runat="server" ClientIDMode="Static" />
<script type="text/javascript">
    $(document).ready(function () {

        var windowTop = parent;
        var popup = windowTop.jQuery("#iPopUp");
        if (popup.length) {

            var $window = $(windowTop),
                            newHeight,
                            newWidth;

            newHeight = $window.height() - 46;
            newWidth = Math.min($window.width() - 40, 1100);

            popup.dialog("option", {
                close: function () { window.dnnModal.closePopUp(false, ""); },
                //'position': 'top',
                height: newHeight,
                width: newWidth,
                //position: 'center'
            });
            $("#<%=hlCancel.ClientID%>").click(function () {
                dnnModal.closePopUp(false, "");
                return false;
            });
            //$(popup).height(windowTop.innerHeight-111);
            //$(popup).css('height', windowTop.innerHeight + 100).dialog('option', );
            //$(popup).dialog({ position: 'center' });
        }

        var moduleScope = $('#<%=ScopeWrapper.ClientID %>'),
            self = moduleScope,
            sf = $.ServicesFramework(<%=ModuleId %>);

        var postData = {  };
        var getData = "";
        var action = "Edit"; //self.getUpdateAction();

        $.ajax({
            type: "GET",
            url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
            data: getData,
            beforeSend: sf.setModuleHeaders
        }).done(function (config) {
            //alert('ok:' + JSON.stringify(config));
            /*
            config.options.form = {
                "buttons": {
                    "submit": {
                        "title": "View",
                        "click": function (){
                            alert(JSON.stringify(this.getValue()));
                        }
                    }
                }
            };
            */

            var ConnectorClass = Alpaca.getConnectorClass("default");
            connector = new ConnectorClass("default");
            connector.servicesFramework = sf;
            connector.culture = '<%=CurrentCulture%>';

            $("#field1").alpaca({
                "schema": config.schema,
                "options": config.options,
                "data": config.data,
                "view": "dnn-edit",
                "connector": connector,
                "postRender": function (control) {
                    var selfControl = control;
                    $("#<%=cmdSave.ClientID%>").click(function () {
                        selfControl.refreshValidationState(true);
                        if (selfControl.isValid(true)) {
                            var value = selfControl.getValue();
                            //alert(JSON.stringify(value, null, "  "));
                            var href = $(this).attr('href');
                            self.FormSubmit(value, href);
                        }
                        return false;
                    });
                    //$('#field1').dnnPanels();
                    //$('.dnnTooltip').dnnTooltip();
                }
            });
        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });

        self.FormSubmit = function (data, href) {
            //var postData = { form: data };
            var postData = JSON.stringify({ form: data });
            var action = "Update"; //self.getUpdateAction();

            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: postData,
                beforeSend: sf.setModuleHeaders
            }).done(function (data) {
                //alert('ok:' + data);
                //self.loadSettings();
                //window.location.href = href;
                
                var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
                var popup = windowTop.jQuery("#iPopUp");
                if (popup.length > 0)
                {
                    windowTop.__doPostBack('dnn_ctr<%=ModuleId %>_View__UP', '');
                    dnnModal.closePopUp(false, href);
                }
                else
                {
                    window.location.href = href;
                }
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        };

    });
</script>
