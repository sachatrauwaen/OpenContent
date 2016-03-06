<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditQuery" CodeBehind="EditQuery.ascx.cs" %>
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
            <asp:LinkButton ID="bIndex" runat="server" class="dnnSecondaryAction" Text="Reindex module" OnClick="bIndex_Click" />
        </li>
        <li>
            <asp:LinkButton ID="bGenerate" runat="server" class="dnnSecondaryAction" Text="Generate 10k" OnClick="bGenerate_Click" Visible="False" />
        </li>

    </ul>
</asp:Panel>

<script type="text/javascript">
    $(document).ready(function () {
        var config = <%=JsonConvert.SerializeObject(AlpacaContext)%>;

        config.editAction = "EditSettings";
        config.updateAction = "UpdateSettings";
        config.data = { "key": "query"};
        var engine = new alpacaEngine.engine(config);
        engine.init();
    });
</script>
