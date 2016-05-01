<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.Builder" CodeBehind="Builder.ascx.cs" %>
<%@ Import Namespace="Newtonsoft.Json" %>

<asp:Panel ID="ScopeWrapper" runat="server">
    <div id="field1" class="alpaca"></div>
    <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
        <li>
            <asp:HyperLink ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" />
        </li>
        <li>
            <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" />
        </li>
        <li>
            <asp:HyperLink ID="hlDelete" runat="server" class="dnnSecondaryAction" resourcekey="cmdDelete" />
        </li>
        <li style="padding-left: 10px;">
            <asp:DropDownList ID="ddlVersions" runat="server" CssClass="oc-ddl-versions" />
        </li>
    </ul>
</asp:Panel>
<div class="container-fluid">
    <div class="row">
        <div class="col-sm-6">
            <div id="form"></div>
        </div>
        <div class="col-sm-6">
            <div id="form2"></div>
        </div>
    </div>
    <div class="row">
        <div class="col-sm-6">
            schema<br />
            <textarea class="form-control" rows="10" id="schema"></textarea>
        </div>
        <div class="col-sm-6">
            options<br />
            <textarea class="form-control" rows="10" id="options"></textarea>
        </div>
    </div>
    <div class="row">
        <div class="col-sm-12">
            Builder<br />
            <textarea class="form-control" rows="10" id="builder"></textarea>
        </div>
    </div>
    <button id="load">Load</button>

</div>
<script type="text/javascript">
    $(document).ready(function () {
        var alpacadata = {};

        //var cook = $.cookie('alpacadata');
        var cook = $('#builder').val();
        if (typeof (cook) != "undefined" && cook) {
            alpacadata = JSON.parse(cook);
            formbuilderConfig.data = alpacadata;
            showForm(alpacadata);
        }

        $("#form").alpaca(
            formbuilderConfig
            );

        $("#load").click(function () {
            var alpacadata = Load();
            //$("#form").alpaca("get").setValue({ "formfields": alpacadata });

            var exists = $("#form").alpaca("exists");
            if (exists) {
                $("#form").alpaca("destroy");
            }
            formbuilderConfig.data = { "formfields": alpacadata };
            $("#form").alpaca(formbuilderConfig);

        });


    });
</script>

