<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.View" CodeBehind="View.ascx.cs" %>

<asp:Panel ID="pHelp" runat="server" Visible="false" CssClass="dnnForm">
    <fieldset>
        <div class="dnnFormItem">
            <asp:Label ID="lUseContent" runat="server" ControlName="rblDataSource" ResourceKey="lUseContent" CssClass="dnnLabel" />
            <asp:RadioButtonList runat="server" ID="rblDataSource" AutoPostBack="true" OnSelectedIndexChanged="rblDataSource_SelectedIndexChanged"
                RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                <asp:ListItem Text="This module" Selected="True" />
                <asp:ListItem Text="Other module" />
            </asp:RadioButtonList>
        </div>
        <asp:PlaceHolder ID="phDataSource" runat="server" Visible="false">
            <div class="dnnFormItem">
                <asp:Label runat="server" ControlName="ddlDataSource" ResourceKey="lDataSource" CssClass="dnnLabel" />
                <asp:DropDownList runat="server" ID="ddlDataSource" AutoPostBack="true" OnSelectedIndexChanged="ddlDataSource_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
        </asp:PlaceHolder>
        <div class="dnnFormItem">
            <asp:Label ID="lUseTemplate" runat="server" ControlName="rblUseTemplate" ResourceKey="lUseTemplate" CssClass="dnnLabel" />
            <asp:RadioButtonList runat="server" ID="rblUseTemplate" AutoPostBack="true" OnSelectedIndexChanged="rblUseTemplate_SelectedIndexChanged"
                RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                <asp:ListItem Text="Use a existing template" Selected="True" />
                <asp:ListItem Text="Create a new template" />
            </asp:RadioButtonList>
        </div>
        <asp:PlaceHolder ID="phFrom" runat="server" Visible="false">
            <div class="dnnFormItem">
                <asp:Label ID="Label4" runat="server" ControlName="rblFrom" CssClass="dnnLabel" ResourceKey="lFrom" />
                <asp:RadioButtonList runat="server" ID="rblFrom" AutoPostBack="true" OnSelectedIndexChanged="rblFrom_SelectedIndexChanged"
                    RepeatDirection="Horizontal" CssClass="dnnFormRadioButtons">
                    <asp:ListItem Text="Site" Selected="True" />
                    <asp:ListItem Text="Web (openextensions.net)" />
                </asp:RadioButtonList>
            </div>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="phTemplate" runat="server">
            <div class="dnnFormItem">
                <asp:Label runat="server" ControlName="ddlTemplate" ResourceKey="lTemplate" CssClass="dnnLabel" />
                <asp:DropDownList runat="server" ID="ddlTemplate" AutoPostBack="true" OnSelectedIndexChanged="ddlTemplate_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="phTemplateName" runat="server" Visible="false">
            <div class="dnnFormItem">
                <asp:Label runat="server" ControlName="tbTemplateName" ResourceKey="lTemplateName" CssClass="dnnLabel" />
                <asp:TextBox ID="tbTemplateName" runat="server"></asp:TextBox>
            </div>
        </asp:PlaceHolder>
    </fieldset>
    <ul class="dnnActions dnnClear" style="padding-left: 32%; margin-left: 38px;">
        <li>
            <asp:LinkButton ID="bSave" runat="server" CssClass="dnnPrimaryAction" ResourceKey="Save" OnClick="bSave_Click" />
        </li>
        <li>
            <asp:HyperLink ID="hlEditSettings" runat="server" Enabled="false" CssClass="dnnSecondaryAction">Template Settings</asp:HyperLink>
        </li>
        <li>
            <asp:HyperLink ID="hlEditContent" runat="server" Enabled="false" CssClass="dnnSecondaryAction">Edit Content</asp:HyperLink>
        </li>
    </ul>
</asp:Panel>
<asp:Panel ID="pDemo" runat="server" Visible="false">
    <p>
        <asp:Label ID="Label3" runat="server" Text="This is demo data. Enter your content to replace it : " />
        <asp:HyperLink ID="hlEditContent2" runat="server" Visible="false">Edit Content</asp:HyperLink>
    </p>
</asp:Panel>


<asp:PlaceHolder ID="phEdit" runat="server" Visible="false">
    <asp:Panel ID="ScopeWrapper" runat="server">
        <div class="alpaca oc-form"></div>
        <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
            <li>
                <asp:HyperLink ID="cmdSave" runat="server" class="dnnPrimaryAction oc-btn-save" resourcekey="cmdSave" /></li>
            <li>
                <asp:HyperLink ID="hlCancel" runat="server" class="dnnSecondaryAction oc-btn-cancel" resourcekey="cmdCancel" /></li>
            <li>
                <asp:HyperLink ID="hlDelete" runat="server" class="dnnSecondaryAction oc-btn-delete" resourcekey="cmdDelete" /></li>
            <li style="padding-left: 10px;">
                <asp:DropDownList ID="ddlVersions" runat="server" CssClass="oc-ddl-versions" />
            </li>
        </ul>
    </asp:Panel>
    <script type="text/javascript">
        $(document).ready(function () {
            //var itemId = "<%=Page.Request.QueryString["id"]%>";
            var moduleScope = $('#<%=ScopeWrapper.ClientID %>'),
            self = moduleScope;
            self.oc = new openContent($, { 
                moduleId : <%=ModuleId %>, 
                culture : '<%=CurrentCulture%>', 
                numberDecimalSeparator : '<%=NumberDecimalSeparator%>'
            });
            self.oc.init(moduleScope);
            document['openContent<%=ModuleId %>'] = function(itemId){
                self.oc.open(itemId);
            };
        });
    </script>
</asp:PlaceHolder>
