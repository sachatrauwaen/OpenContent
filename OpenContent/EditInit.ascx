<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.EditInit" CodeBehind="EditInit.ascx.cs" %>
<%@ Register Src="~/DesktopModules/OpenContent/TemplateInit.ascx" TagPrefix="uc1" TagName="TemplateInit" %>
<div class="dnnForm" id="tabs-demo">
    <ul class="dnnAdminTabNav">
        <li><a href="#ChangeTemplate">Switch Template</a></li>
        <li><a href="#CloneModule">Clone Module</a></li>
    </ul>
    <div id="ChangeTemplate" class="dnnClear">
        <uc1:TemplateInit runat="server" id="TemplateInitControl" />
    </div>
    <div id="CloneModule" class="dnnClear">
        <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
            
        </ul>
        <asp:CheckBoxList ID="cblPages" runat="server" CssClass="TreeView" RepeatLayout="Table" RepeatColumns="5" RepeatDirection="Vertical">
        </asp:CheckBoxList>
        <ul class="dnnActions dnnClear" style="display: block; padding-left: 35%">
            <li>
                <asp:LinkButton ID="cmdSave" resourcekey="cmdSave" runat="server" CssClass="dnnPrimaryAction" />
            </li>
            <li>
                <asp:HyperLink ID="hlCheckAll" ClientIDMode="Static" resourcekey="cmdCheckAll" runat="server" CssClass="dnnSecondaryAction" NavigateUrl="#" />
            </li>
            <li>
                <asp:HyperLink ID="hlCheckNone" ClientIDMode="Static" resourcekey="cmdCheckNone" runat="server" CssClass="dnnSecondaryAction" NavigateUrl="#" />
            </li>
        </ul>
    </div>
</div>

<script type="text/javascript">
    jQuery(function ($) {
        $('#tabs-demo').dnnTabs();

        $('#hlCheckAll').click(function () {
            $("#CloneModule input:checkbox:not(:disabled)").prop('checked', true);
            return false;
        });
        $('#hlCheckNone').click(function () {
            $("#CloneModule input:checkbox:not(:disabled)").prop('checked', false);
            return false;
        });
    });
</script>
