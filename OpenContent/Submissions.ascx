<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="Submissions.ascx.cs" Inherits="Satrabel.OpenContent.Submissions" %>

<div class="dnnForm dnnEditBasicSettings" id="dnnEditBasicSettings">
    <p><asp:LinkButton ID="excelDownload" runat="server" Text="Download as Excel" OnClick="ExcelDownload_Click"/></p>
    <fieldset>
        <div class="dnnFormItem">
            <asp:GridView ID="gvData" runat="server" CssClass="dnnGrid" GridLines="None" 
                AutoGenerateColumns="true" Width="98%" 
                EnableViewState="false" BorderStyle="None" >
                <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" />
                <RowStyle CssClass="dnnGridItem" HorizontalAlign="Left" />
                <AlternatingRowStyle CssClass="dnnGridAltItem" />
                <FooterStyle CssClass="dnnGridFooter" />
                <PagerStyle CssClass="dnnGridPager" />
            </asp:GridView>
        </div>
    </fieldset>
</div>
