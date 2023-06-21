<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.AlpacaFormBuilder" CodeBehind="AlpacaFormBuilder.ascx.cs" %>
<%@ Import Namespace="Newtonsoft.Json" %>

<asp:Panel ID="ScopeWrapper" runat="server" CssClass="dnnForm form-builder">
    <div class="">
        <div class="container-fluid">
            <div id="field1" class="alpaca" style="display: none;"></div>
        </div>

        <div class="fb-container" style="display: none;">
            <div class="fb-left">
                <div class="fb-wrap">
                    <h2>Fields</h2>
                    <div id="form"></div>
                    <div class="loading">
                        <img src="/DesktopModules/OpenContent/images/loading.gif" alt="Loading" />
                    </div>

                </div>
            </div>
            <div class="fb-right">
                <div class="fb-wrap">
                    <h2>Form preview</h2>
                    <div id="form2"></div>
                </div>
            </div>
            <div style="clear: both;"></div>
        </div>
        <ul class="dnnActions dnnClear" style="display: none;">
            <li>
                <asp:HyperLink ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" />
            </li>
            <li>
                <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" />
            </li>
            <li>
                <asp:HyperLink ID="hlRefresh" runat="server" class="dnnSecondaryAction" resourcekey="cmdRefresh" />
            </li>
            <li>
                <asp:DropDownList ID="ddlForms" runat="server" AutoPostBack="true"></asp:DropDownList>
            </li>
        </ul>
    </div>
</asp:Panel>

<script type="text/javascript">
    $(document).ready(function () {

        var BuilderV2 = <%= AlpacaContext.BuilderV2 ? "true" : "false"%>;

        var windowTop = parent;
        var popup = windowTop.jQuery("#iPopUp");
        if (popup.length) {

            var $window = $(windowTop),
                newHeight,
                newWidth;

            
            newHeight = $window.height() - 110;
            newWidth = Math.min($window.width() - 110, 1200);

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

            if (!BuilderV2) {
                $(".form-builder .fb-container").show();
                $('body').css('overflow', 'hidden');
                $(".form-builder .fb-left .fb-wrap").height('100%').css('overflow', 'hidden');
                var formHeight = newHeight - 100 - 20 - 62;
                $(".form-builder .fb-left .fb-wrap #form").height(formHeight + 'px').css('overflow-y', 'auto').css('overflow-x', 'hidden');
                $(".form-builder .fb-left .fb-wrap #form > .alpaca-field-object").css('margin', '0');
                $(".form-builder .fb-right .fb-wrap #form2").height(formHeight + 'px').css('overflow-x', 'hidden').css('overflow-y', 'auto').css('overflow-x', 'hidden');
            }
        }

        var moduleScope = $('#<%=ScopeWrapper.ClientID %>'),
            self = moduleScope,
            sf = $.ServicesFramework(<%=ModuleId %>);

        var getData = { key: $("#<%=ddlForms.ClientID %>").val() };
        if (getData.key == "form") {
            ContactForm = true;
        }

        if (getData.key == "") {
            Indexable = true;
        }

        // global variables for formbuilmder.js
        BootstrapForm = <%= AlpacaContext.Bootstrap ? "true" : "false"%>;
        BootstrapHorizontal = <%= AlpacaContext.Horizontal ? "true" : "false"%>;


        if (BootstrapForm) {
            formbuilderConfig.view = "dnnbootstrap-edit-horizontal";
        }
        var app = null;
        $.ajax({
            type: "GET",
            url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/LoadBuilder",
            data: getData,
            beforeSend: sf.setModuleHeaders
        }).done(function (res) {
            if (!res.data) res.data = {};
            if (BuilderV2) {
                var connector = Lama.getConnectorClass("default");
                app = Lama.mountBuilder("#field1", {
                    "data": { schema: res.schema, options: res.options },
                    "connector": connector
                });
                $("#field1").show();
                $(".form-builder div.loading").hide();
                $(".form-builder .dnnActions").show();
            } else {
                //preview
                showForm(res.data);
                //formbuilder
                formbuilderConfig.data = res.data;
                $("#form").alpaca(
                    formbuilderConfig
                );
                
            }

        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
        $("#<%=cmdSave.ClientID%>").click(function () {
            var href = $(this).attr('href');
            if (BuilderV2) {
                //app.validate(function () {

                var value = app.getValue();
                //alert(JSON.stringify(value, null, "  "));
                var data = {};
                var schema = value.schema;
                var options = value.options;
                var view = {};
                var index = {};
                var postData = JSON.stringify({ 'data': data, 'schema': schema, 'options': options, 'view': view, 'index': index, 'key': $("#<%=ddlForms.ClientID %>").val() });
                var action = "UpdateBuilder";
                $.ajax({
                    type: "POST",
                    url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: postData,
                    beforeSend: sf.setModuleHeaders
                }).done(function (data) {
                    window.location.href = href;
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status + " " + xhr.responseText);
                });
                //});
            } else {
                var form = $("#form").alpaca("get");
                var data = form.getValue();
                var schema = getSchema(data);
                var options = getOptions(data);
                var view = getView(data);
                var index = getIndex(data);
                var postData = JSON.stringify({ 'data': data, 'schema': schema, 'options': options, 'view': view, 'index': index, 'key': $("#<%=ddlForms.ClientID %>").val() });
                var action = "UpdateBuilder";
                $.ajax({
                    type: "POST",
                    url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/" + action,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: postData,
                    beforeSend: sf.setModuleHeaders
                }).done(function (data) {
                    window.location.href = href;
                }).fail(function (xhr, result, status) {
                    alert("Uh-oh, something broke: " + status + " " + xhr.responseText);
                });
            }
            return false;
        });
        $("#<%=hlRefresh.ClientID%>").click(function () {
            var href = $(this).attr('href');
            var form = $("#form").alpaca("get");
            form.refreshValidationState(true);
            if (!form.isValid(true)) {
                form.focus();
                return;
            }
            var value = form.getValue();
            showForm(value);
            return false;
        });
    });
</script>

