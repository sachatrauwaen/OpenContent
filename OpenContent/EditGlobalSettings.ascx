<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditGlobalSettings" CodeBehind="EditGlobalSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<asp:Panel ID="ScopeWrapper" runat="server" CssClass="dnnForm">
    <div class="dnnFormItem">
        <dnn:Label ID="lRoles" ControlName="ddlRoles" runat="server" />
        <asp:DropDownList ID="ddlRoles" runat="server"></asp:DropDownList>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lMLContent" ControlName="cbMLContent" runat="server" />
        <asp:CheckBox ID="cbMLContent" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lMaxVersions" ControlName="ddlMaxVersions" runat="server" />
        <asp:DropDownList ID="ddlMaxVersions" runat="server"></asp:DropDownList>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lLogging" ControlName="ddlLogging" runat="server" />
        <asp:DropDownList ID="ddlLogging" runat="server">
            <asp:ListItem Value="none" Text="None"></asp:ListItem>
            <asp:ListItem Value="host" Text="Host super"></asp:ListItem>
            <asp:ListItem Value="allways" Text="Always"></asp:ListItem>
        </asp:DropDownList>
    </div>

    <div class="dnnFormItem">
        <dnn:Label ID="lGoogleApiKey" ControlName="tbGoogleApiKey" runat="server" />
        <asp:TextBox ID="tbGoogleApiKey" runat="server"></asp:TextBox>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lEditLayout" ControlName="ddlEditLayout" runat="server" />
        <asp:DropDownList ID="ddlEditLayout" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlEditLayout_SelectedIndexChanged">
            <asp:ListItem Value="1" Text="DNN"></asp:ListItem>
            <asp:ListItem Value="2" Text="Bootstrap"></asp:ListItem>
            <asp:ListItem Value="3" Text="Bootstrap Horizontal"></asp:ListItem>
        </asp:DropDownList>
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lLoadBootstrap" ControlName="cbLoadBootstrap" runat="server" />
        <asp:CheckBox ID="cbLoadBootstrap" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lFastHandlebars" ControlName="cbFastHandlebars" runat="server" />
        <asp:CheckBox ID="cbFastHandlebars" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label ID="lSaveXml" ControlName="cbSaveXml" runat="server" />
        <asp:CheckBox ID="cbSaveXml" runat="server" />
    </div>

    <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
        <li>
            <asp:LinkButton ID="cmdSave" runat="server" class="dnnPrimaryAction" resourcekey="cmdSave" />
        </li>
        <li>
            <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction" resourcekey="cmdCancel" />
        </li>
    </ul>
</asp:Panel>
