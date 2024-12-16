<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.View" CodeBehind="View.ascx.cs" %>

<%-- 
<%@ Register Src="~/DesktopModules/OpenContent/TemplateInit.ascx" TagPrefix="uc1" TagName="TemplateInit" %>
<uc1:TemplateInit runat="server" ID="TemplateInitControl" />
--%>

<asp:Panel ID="pInit" runat="server" Visible="false">

<asp:Panel ID="pVue2" runat="server"  >
</asp:Panel>

<script>
    /*globals jQuery, window, Sys */
    (function ($, Sys) {
        $(document).ready(function () {
            var sf = $.ServicesFramework(<%= ModuleContext.ModuleId %>);

            var app = OpenContent.init.mount('#<%= pVue2.ClientID %>', {
                sf: sf,
                settingsUrl:"<%= ModuleContext.EditUrl("EditSettings") %>",
                editUrl: "<%= ModuleContext.EditUrl("Edit") %>",
                IsPageAdmin:  <%=Satrabel.OpenContent.Components.Dnn.DnnPermissionsUtils.IsPageAdmin() ? "true" : "false"%>,
                resources: {
                    CreateHelp: '<%= Resource("CreateHelp") %>',
                    lTemplate: '<%= Resource("lTemplate") %>',
                    lUseContent: '<%= Resource("lUseContent") %>',
                    lDetailPage: '<%= Resource("lDetailPage") %>',
                    Advanced: '<%=Resource("Advanced")%>',
                    More: '<%=Resource("More")%>',
                    lAddNewContent: '<%=Resource("lAddNewContent")%>',
                    lUseExistingContent: '<%=Resource("lUseExistingContent")%>',
                    lDataSource: '<%=Resource("lDataSource")%>',
                    lTemplateName: '<%=Resource("lTemplateName")%>',
                    liCreateNewTemplate: '<%=Resource("liCreateNewTemplate")%>',
                    liFromSite: '<%=Resource("liFromSite")%>',
                    liFromWeb: '<%=Resource("liFromWeb")%>',
                    Create: '<%=Resource("Create")%>',
                    Save: '<%=Resource("Save")%>',
                    liUseExistingTemplate: '<%=Resource("liUseExistingTemplate")%>',
                    liOtherModule: '<%=Resource("liOtherModule")%>',
                    lTab: '<%=Resource("lTab")%>',
                    lModule: '<%=Resource("lModule")%>',
                    CreateNewTemplate: '<%=Resource("CreateNewTemplate")%>',
                }

            });
        });
    }(jQuery, window.Sys));
</script>

</asp:Panel>
