<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.View" CodeBehind="View.ascx.cs" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:Literal ID="lOutput" runat="server"></asp:Literal>
    </ContentTemplate>
</asp:UpdatePanel>


<asp:Panel ID="pHelp" runat="server" Visible="false">
    <h3>Get started</h3>
    <ol>
        <li>
            <asp:Label ID="scriptListLabel" runat="server" Text="Get a template > " />
            <asp:HyperLink ID="hlTempleteExchange" runat="server" Visible="false">Template Exchange</asp:HyperLink>
        </li>
        <li>
            <asp:Label ID="Label1" runat="server" Text="Chose a template > " />
            <asp:HyperLink ID="hlEditSettings" runat="server" Visible="false">Template Settings</asp:HyperLink>
        </li>
        <li>
            <asp:Label ID="Label2" runat="server" Text="Enter the content > " />
            <asp:HyperLink ID="hlEditContent" runat="server" Visible="false">Edit Content</asp:HyperLink>
        </li>
    </ol>
</asp:Panel>
<asp:Panel ID="pDemo" runat="server" Visible="false">
    <p>
        <asp:Label ID="Label3" runat="server" Text="This is demo data. Enter your content to replace it : " />
        <asp:HyperLink ID="hlEditContent2" runat="server" Visible="false">Edit Content</asp:HyperLink>
    </p>
</asp:Panel>


<script>
    var EditModal<%= ModuleId %> = function () {

        dnnModal.show('/DesktopModules/OpenContent/HtmlPage.html', false, 550, 950, false, '');

        var iframe = document.getElementById("iPopUp");
        var sf = $.ServicesFramework(<%=ModuleId %>);
        var refresh = function() {__doPostBack('<%= UpdatePanel1.ClientID %>', '')};
        iframe.dnn = {moduleid : <%= ModuleId %>, sf : sf, refresh : refresh};

    };
</script>
