<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditGlobalSettings" CodeBehind="EditGlobalSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<asp:Panel ID="ScopeWrapper" runat="server">
    <div class="dnnFormItem">
        <dnn:Label ID="lEditWitoutPostback" ControlName="cbEditWitoutPostback" runat="server" />
        <asp:CheckBox ID="cbEditWitoutPostback" runat="server" />
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
