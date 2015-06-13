<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.Edit" Codebehind="Edit.ascx.cs" %>

<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>
<dnncl:DnnCssInclude ID="customJS" runat="server" FilePath="~/DesktopModules/OpenContent/alpaca/css/alpaca-dnn.css" AddTag="false" />
<dnncl:DnnJsInclude ID="DnnJsInclude1" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/handlebars/handlebars.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />
<dnncl:DnnJsInclude ID="DnnJsInclude2" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/alpaca/web/alpaca.js" Priority="107" ForceProvider="DnnPageHeaderProvider" />

<dnncl:DnnJsInclude ID="DnnJsInclude4" runat="server" FilePath="~/DesktopModules/OpenContent/js/alpaca-1.5.8/lib/typeahead.js/dist/typeahead.bundle.min.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />

<dnncl:DnnJsInclude ID="DnnJsInclude5" runat="server" FilePath="~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js" Priority="106" ForceProvider="DnnPageHeaderProvider" />

<script src="<%=ControlPath %>js/wysihtml/wysihtml-toolbar.js"></script>
<script src="<%=ControlPath %>js/wysihtml/parser_rules/advanced_opencontent.js"></script>  
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/views/dnn.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/ImageField.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/UrlField.js"></script>
<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/CKEditorField.js"></script>

<script type="text/javascript" src="<%=ControlPath %>alpaca/js/fields/dnn/MLTextField.js"></script>

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
                window.location.href = href;
            }).fail(function (xhr, result, status) {
                alert("Uh-oh, something broke: " + status);
            });
        };

        self.SetupFileUpload = function (fileupload) {
            //$('#field1 input[type="file"]')
            $(fileupload).fileupload({
                dataType: 'json',
                url: sf.getServiceRoot('OpenContent') + "FileUpload/UploadFile",
                maxFileSize: 25000000,
                formData: {example: 'test'},
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
                            $(e.target).closest('.alpaca-container').find('.alpaca-field-image input').val(file.url);
                            $(e.target).closest('.alpaca-container').find('.alpaca-image-display img').attr('src', file.url);
                        });
                    }
                }
            }).data('loaded', true);
        }
    });
</script>
