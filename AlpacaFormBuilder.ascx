<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.AlpacaFormBuilder" CodeBehind="AlpacaFormBuilder.ascx.cs" %>
<%@ Import Namespace="Newtonsoft.Json" %>

<asp:Panel ID="ScopeWrapper" runat="server" CssClass="form-builder">
    <div class="">
        <div class="fb-container">
            <div class="fb-left">
                <h2>Fields</h2>
                <div id="form"></div>
            </div>
            <div class="fb-right">
                <h2>Form preview</h2>
                <div id="form2"></div>
            </div>
            <div style="clear:both;"></div>
        </div>
        <div class="row" style="display:none;">
            <div class="col-sm-6">
                schema<br />
                <textarea class="form-control" rows="10" id="schema"></textarea>
            </div>
            <div class="col-sm-6">
                options<br />
                <textarea class="form-control" rows="10" id="options"></textarea>
            </div>
            <div style="clear:both;"></div>
        </div>
        <div class="row" style="display:none;">
            <div class="col-sm-12">
                Builder<br />
                <textarea class="form-control" rows="10" id="builder"></textarea>
            </div>
        </div>
        
    </div>
    <ul class="dnnActions dnnClear" _style="display: block; padding-left: 35%">
        <li>
            <asp:HyperLink ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" />
        </li>
        <li>
            <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" />
        </li>
         <li>
            <asp:HyperLink ID="hlRefresh" runat="server" class="dnnSecondaryAction" resourcekey="cmdRefresh" />
        </li>

    </ul>
</asp:Panel>

<script type="text/javascript">
    $(document).ready(function () {

        var windowTop = parent;
        var popup = windowTop.jQuery("#iPopUp");
        if (popup.length) {

            var $window = $(windowTop),
                            newHeight,
                            newWidth;

            newHeight = $window.height() - 36;
            newWidth = Math.min($window.width() - 40, 1600);

            popup.dialog("option", {
                close: function () { window.dnnModal.closePopUp(false, ""); },
                //'position': 'top',
                height: newHeight,
                width: newWidth,
                //position: 'center'
                resizable: false,
            });
        }

        var moduleScope = $('#<%=ScopeWrapper.ClientID %>'),
                self = moduleScope,
                sf = $.ServicesFramework(<%=ModuleId %>);

        var getData = { key: "" };

        $.ajax({
            type: "GET",
            url: sf.getServiceRoot('OpenContent') + "OpenContentAPI/LoadBuilder",
            data: getData,
            beforeSend: sf.setModuleHeaders
        }).done(function (res) {
            $('#builder').val(JSON.stringify(res.data, null, "  "));
            if (!res.data) res.data = {};
            showForm(res.data);
            formbuilderConfig.data = res.data;
            $("#form").alpaca(
                formbuilderConfig
           );
        }).fail(function (xhr, result, status) {
            alert("Uh-oh, something broke: " + status);
        });
        $("#<%=cmdSave.ClientID%>").click(function () {
            var href = $(this).attr('href');
            //var data = $('#builder').val();
            var form = $("#form").alpaca("get");
            var data = form.getValue();
            var schema = getSchema(data);
            var options = getOptions(data);
            var postData = JSON.stringify({ 'data': data, 'schema': schema, 'options': options, 'key': "" });
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
            $('#builder').val(JSON.stringify(value, null, "  "));
            showForm(value);
            return false;
        });
    });
</script>

