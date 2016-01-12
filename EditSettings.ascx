<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditSettings" CodeBehind="EditSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<asp:Panel ID="ScopeWrapper" runat="server" CssClass="dnnForm">
    <div id="field1" class="alpaca"></div>
    <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
        <li>
            <asp:HyperLink ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" />
        </li>
        <li>
            <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" />
        </li>
    </ul>
</asp:Panel>

<script type="text/javascript">
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        function setupStructSettings() {
            var windowTop = parent;
            var popup = windowTop.jQuery("#iPopUp");
            if (popup.length > 0) {
                popup.dialog("option", {
                    close: function () { window.dnnModal.closePopUp(false, ""); }
                });
                $("#<%=hlCancel.ClientID%>").click(function () {
                    dnnModal.closePopUp(false, "");
                    return false;
                });
            }

            var moduleScope = $('#<%=ScopeWrapper.ClientID %>'),
                self = moduleScope,
                sf = $.ServicesFramework(<%=ModuleId %>);

            self.CreateForm = function () {
                var postData = {};
                var getData = "";
                var action = "Settings";

                $.ajax({
                    type: "GET",
                    url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                    data: getData,
                    beforeSend: sf.setModuleHeaders
                }).done(function (config) {
                    if (config.schema) {
                        var jsmodules = [];

                        /*
                        oc_loadmodules(config.options, function () {
                            self.FormEdit(config);

                        });
                        */
                        self.FormEdit(config);

                        /*
                        if (config.options) {
                            var types = self.GetFieldTypes(config.options);
                            if ($.inArray("address", types) != -1) {
                                jsmodules.push('addressfield');
                            }
                        }
                        if (jsmodules.length > 0) {
                            require(jsmodules, function () {
                                self.FormEdit(config);
                            });
                        }
                        else {
                            self.FormEdit(config);
                        }
                        */
                    }
                    else {
                        $("#<%=cmdSave.ClientID%>").click(function () {
                        var href = $(this).attr('href');
                        self.FormSubmit("", href);
                        return false;
                    });
                }
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status);
                });
            };

        self.FormEdit = function (config) {
            $.alpaca.setDefaultLocale("<%= AlpacaCulture %>");
            var ConnectorClass = Alpaca.getConnectorClass("default");
            connector = new ConnectorClass("default");
            connector.servicesFramework = sf;
            connector.culture = '<%=CurrentCulture%>';
            connector.defaultCulture = '<%=DefaultCulture%>';
            connector.numberDecimalSeparator = '<%=NumberDecimalSeparator%>';
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
                }
            });
            };

        self.FormSubmit = function (data, href) {
            //var postData = { 'data': data, 'template': Template };
            var postData = JSON.stringify({ 'data': data});
            var action = "UpdateSettings";
            $.ajax({
                type: "POST",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: postData,
                beforeSend: sf.setModuleHeaders
            }).done(function (data) {

                var windowTop = parent; //needs to be assign to a varaible for Opera compatibility issues.
                var popup = windowTop.jQuery("#iPopUp");
                if (popup.length > 0) {
                    windowTop.__doPostBack('dnn_ctr<%=ModuleId %>_View__UP', '');
                        dnnModal.closePopUp(false, href);
                    }
                    else {
                        window.location.href = href;
                    }

                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status + " " + xhr.responseText);
                });
        };
            /*
               
                self.GetFieldTypes = function (options) {
                    var types = [];
                    if (options.fields) {
                        var fields = options.fields;
                        for (var key in fields) {
                            var field = fields[key];
                            if (field.type) {
                                types.push(field.type);
                            }
                            var subtypes = self.GetFieldTypes(field);
                            types = types.concat(subtypes);
                        }
                    }
                    return types;
                }
            */
            self.CreateForm();
        }

        $(document).ready(function () {

            setupStructSettings();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupStructSettings();
            });
            //setTimeout(function () {

            //}, 2000);

        });

    }(jQuery, window.Sys));

    var gminitializecallback;
    function gminitialize() {
        if (gminitializecallback)
            gminitializecallback();
    }
</script>
