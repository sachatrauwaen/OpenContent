<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditSettings" CodeBehind="EditSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>

<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/css/alpaca-dnn.css" AddTag="false" />
<dnncl:DnnJsInclude ID="DnnJsInclude6" runat="server" FilePath="~/Resources/Shared/Components/UserFileManager/jquery.dnnUserFileUpload.js" Priority="106" />
<dnncl:DnnJsInclude ID="DnnJsInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/handlebars/handlebars.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude2" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/web/alpaca.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/views/dnn.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/ImageField.js"></script>

<asp:Panel ID="ScopeWrapper" runat="server">
    <div class="dnnFormItem">
        <dnn:Label ID="scriptListLabel" ControlName="scriptList" runat="server" />
        <asp:DropDownList ID="scriptList" runat="server" />
        <asp:HyperLink ID="hlTemplateExchange" runat="server">More...</asp:HyperLink>
    </div>
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

            $("#<%= scriptList.ClientID %>").change(function () {
                if ($("#field1").alpaca("exists")) {
                    $("#field1").alpaca("destroy");
                }
                self.CreateForm();
            });

            self.CreateForm = function () {
                var Template = $("#<%= scriptList.ClientID %>").val();
            if (!Template) return;
            var postData = {};
            //var getData = "tabId=<%=TabId %>&moduleId=<%=ModuleId %>";
            var getData = "Template=" + Template;
            var action = "Settings"; //self.getUpdateAction();

            $.ajax({
                type: "GET",
                url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                data: getData,
                beforeSend: sf.setModuleHeaders
            }).done(function (config) {
                if (config.schema) {
                    var ConnectorClass = Alpaca.getConnectorClass("default");
                    connector = new ConnectorClass("default");
                    connector.servicesFramework = sf;

                    $.alpaca.Fields.DnnFileField = $.alpaca.Fields.FileField.extend({
                        setup: function () {
                            this.base();
                        },
                        afterRenderControl: function (model, callback) {
                            var self = this;
                            this.base(model, function () {
                                self.handlePostRender(function () {
                                    callback();
                                });
                            });
                        },
                        handlePostRender: function (callback) {
                            //var self = this;
                            var el = this.control;
                            self.SetupFileUpload(el);
                            callback();
                        }
                    });
                    Alpaca.registerFieldClass("file", Alpaca.Fields.DnnFileField);

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
        self.FormSubmit = function (data, href) {
            var Template = $("#<%= scriptList.ClientID %>").val();
            //var postData = { 'data': data, 'template': Template };
            var postData = JSON.stringify({ 'data': data, 'template': Template });
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

        self.SetupFileUpload = function (fileupload) {

            //$('#field1 input[type="file"]')
            $(fileupload).fileupload({
                dataType: 'json',
                url: sf.getServiceRoot('Satrabel.Content') + "FileUpload/UploadFile",
                maxFileSize: 25000000,
                formData: { example: 'test' },
                beforeSend: sf.setModuleHeaders,
                add: function (e, data) {
                    //data.context = $(opts.progressContextSelector);
                    //data.context.find($(opts.progressFileNameSelector)).html(data.files[0].name);
                    //data.context.show('fade');
                    data.submit();
                },
                progress: function (e, data) {
                    if (data.context) {
                        var progress = parseInt(data.loaded / data.total * 100, 10);
                        data.context.find(opts.progressBarSelector).css('width', progress + '%').find('span').html(progress + '%');
                    }
                },
                done: function (e, data) {
                    if (data.result) {
                        $.each(data.result, function (index, file) {
                            //$('<p/>').text(file.name).appendTo($(e.target).parent().parent());
                            //$('<img/>').attr('src', file.url).appendTo($(e.target).parent().parent());

                            $(e.target).closest('.alpaca-container').find('.alpaca-field-image input').val(file.url);
                            $(e.target).closest('.alpaca-container').find('.alpaca-image-display img').attr('src', file.url);
                        });
                    }
                }
            }).data('loaded', true);
        }
        self.CreateForm();
    }

        $(document).ready(function () {

            setupStructSettings();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupStructSettings();
            });
        });

    }(jQuery, window.Sys));
</script>
