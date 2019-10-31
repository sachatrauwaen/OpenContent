<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="Submissions.ascx.cs" Inherits="Satrabel.OpenContent.Submissions" %>

<div class="dnnForm dnnEditBasicSettings" id="dnnEditBasicSettings">
    <p><asp:LinkButton ID="excelDownload" runat="server" Text="Download as Excel" OnClick="ExcelDownload_Click"/></p>
    <fieldset>
        <div class="dnnFormItem">
            <asp:GridView ID="gvData" runat="server" CssClass="dnnGrid" GridLines="None"  Width="98%" 
                EnableViewState="true" BorderStyle="None" >
                <Columns>
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>

                            <asp:Button ID="btnDelete" runat="server" OnClick="btnDelete_Click" CommandArgument='<%# Eval("Id") %>' Text="Delete" />
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
                <HeaderStyle CssClass="dnnGridHeader" VerticalAlign="Top" />
                <RowStyle CssClass="dnnGridItem" HorizontalAlign="Left" />
                <AlternatingRowStyle CssClass="dnnGridAltItem" />
                <FooterStyle CssClass="dnnGridFooter" />
                <PagerStyle CssClass="dnnGridPager" />
                
            </asp:GridView>
        </div>
    </fieldset>
</div>
