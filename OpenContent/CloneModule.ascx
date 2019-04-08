<%@ Control Language="C#" AutoEventWireup="false" Inherits="Satrabel.OpenContent.CloneModule" CodeBehind="CloneModule.ascx.cs" %>
<div class="dnnForm">
    <asp:CheckBoxList ID="cblPages" ClientIDMode="Static" runat="server" RepeatLayout="Table" RepeatColumns="4" RepeatDirection="Vertical" Width="100%">
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

<script type="text/javascript">
    jQuery(function ($) {
        $('#hlCheckAll').click(function () {
            $("#cblPages input:checkbox:not(:disabled)").prop('checked', true);
            return false;
        });
        $('#hlCheckNone').click(function () {
            $("#cblPages input:checkbox:not(:disabled)").prop('checked', false);
            return false;
        });
    });
</script>
