<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.Edit" CodeBehind="Edit.ascx.cs" %>

<asp:Literal ID="module" runat="server"></asp:Literal>

<%--

<%@ Import Namespace="Newtonsoft.Json" %>

<asp:Panel ID="ScopeWrapper" runat="server">
    <div class="container-fluid <%=AlpacaContext.BuilderV2 ? "lama-editor" : "alpaca-editor" %>">
        <div id="field1" class="alpaca"></div>
    </div>
    <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
        <li>
            <asp:HyperLink ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" />
        </li>
        <li>
            <asp:HyperLink ID="cmdCopy" runat="server" class="dnnSecondaryAction" resourcekey="cmdCopy" />
        </li>
        <li>
            <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" />
        </li>
        <li>
            <asp:HyperLink ID="hlDelete" runat="server" class="dnnSecondaryAction" resourcekey="cmdDelete" NavigateUrl="#" />
        </li>
        <li style="padding-left: 10px;">
            <asp:DropDownList ID="ddlVersions" runat="server" CssClass="oc-ddl-versions form-control" />
        </li>
    </ul>
    <div id="field1validation" style="display: none; color: #b94a48; padding-left: 35%">
        <i class="glyphicon glyphicon-exclamation-sign"></i>
        <span class="serverside" style="display: none;"></span>
        <asp:Label runat="server" Style="display: none;" CssClass="clientside" resourcekey="errInvalid"></asp:Label>
    </div>
</asp:Panel>

<script type="text/javascript">
    $(document).ready(function () {
        var engine = new alpacaEngine.engine(<%=JsonConvert.SerializeObject(AlpacaContext)%>);
        engine.init();
    });
</script>
    --%>
