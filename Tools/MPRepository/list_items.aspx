<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="list_items.aspx.cs" Inherits="MPRepository.Web.list_items" EnableEventValidation="false"  MasterPageFile="~/mp_repository.Master" %>

<asp:Content ID="listItemsContent" runat="server" ContentPlaceHolderID="MPRContentHolder1" >

      <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
        <Services>
          <asp:ServiceReference path="UIHelper.asmx" />
        </Services>
      </asp:ScriptManager>

    <div id="itemsDiv">
    
    <h1>MediaPortal - Files Repository</h1>
    <asp:UpdatePanel ID="itemsUpdatePanel" runat="server">
      <ContentTemplate>
        <table>
          <tr>
            <td valign="top">
              <asp:GridView ID="itemsGridView" runat="server" AllowPaging="True" AutoGenerateColumns="False" 
                  onrowdatabound="itemsGridView_OnRowDataBound" 
                  onselectedindexchanged="itemsGridView_SelectedIndexChanged" 
                CellPadding="4" ForeColor="#333333" GridLines="None">
                <RowStyle BackColor="#E3EAEB" />
                <Columns>
                  <asp:BoundField HeaderText="Name" DataField="Name" />
                  <asp:BoundField HeaderText="Description" DataField="DescriptionShort" />
                  <asp:BoundField HeaderText="Rating" DataField="Rating" />
                  <asp:BoundField HeaderText="Downloads" DataField="Downloads" />
                  <asp:BoundField HeaderText="Author" DataField="Author" />
                  <asp:BoundField HeaderText="Last Updated" DataField="LastUpdated" />
                </Columns>
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <EditRowStyle BackColor="#7C6F57" />
                <AlternatingRowStyle BackColor="White" />
              </asp:GridView>
            </td>
            <td>
              <asp:DetailsView ID="itemDetailsView" runat="server" AutoGenerateRows="False" 
                CellPadding="4" ForeColor="#333333" GridLines="None">
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <CommandRowStyle BackColor="#C5BBAF" Font-Bold="True" />
                <RowStyle BackColor="#E3EAEB" />
                <FieldHeaderStyle BackColor="#D0D0D0" Font-Bold="True" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <Fields>
                  <asp:BoundField HeaderText="Name" DataField="Name" />
                  <asp:BoundField HeaderText="Description" DataField="Description" />
                  <asp:BoundField HeaderText="Author" DataField="Author" />
                  <asp:BoundField HeaderText="Homepage" DataField="Homepage"/>
                  <asp:BoundField HeaderText="License" DataField="License" />              
                </Fields>
                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <EditRowStyle BackColor="#7C6F57" />
                <AlternatingRowStyle BackColor="White" />
              </asp:DetailsView>
              <p />
              <asp:DetailsView ID="versionDetailsView" runat="server" AutoGenerateRows="False" 
                  CellPadding="4" ForeColor="#333333" GridLines="None" >
                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <CommandRowStyle BackColor="#C5BBAF" Font-Bold="True" />
                <RowStyle BackColor="#E3EAEB" />
                <FieldHeaderStyle BackColor="#D0D0D0" Font-Bold="True" />
                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                <Fields>
                  <asp:BoundField HeaderText="Version" DataField="Version" />
                  <asp:BoundField HeaderText="Development Status" DataField="DevelopmentStatus" />
                  <asp:BoundField HeaderText="Minimum MediaPortal version" DataField="MPVersionMin" />
                  <asp:BoundField HeaderText="Maximum MediaPortal version" DataField="MPVersionMax" />
                </Fields>
                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                <EditRowStyle BackColor="#7C6F57" />
                <AlternatingRowStyle BackColor="White" />
              </asp:DetailsView>
              <p />
              <asp:Repeater ID="commentsRepeater" runat="server">
                <HeaderTemplate>
                  <table border="1">
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                      <td><%# DataBinder.Eval(Container.DataItem,"Time") %></td>
                      <td><%# DataBinder.Eval(Container.DataItem,"User") %></td>
                      <td><%# DataBinder.Eval(Container.DataItem,"Text") %></td>                     
                    </tr>
                </ItemTemplate>              
                <FooterTemplate>
                  </table>
                </FooterTemplate>
              </asp:Repeater>
              <div id="editItemDiv">
                <hr />
                <asp:HyperLink ID="singleItemHyperLink" runat="server" Visible="false">More</asp:HyperLink>
              </div>
              <div id="commentAddDiv">
                <hr />
                <asp:TextBox ID="commentAddTextBox" runat="server" Width="450" Rows="3" TextMode="MultiLine" Visible="false" /> 
                <br />              
                <asp:Button ID="commentAddButton" runat="server" Text="Add Comment" Visible="false" onclick="commentAddButton_Click" />
                <p />
                <asp:Label ID="commentAddLabel" runat="server" Visible="false" />
              </div>
            </td>
          </tr>
        </table>
        <asp:Label ID="messagesLabel" runat="server" />
      </ContentTemplate>
    </asp:UpdatePanel>    
    </div>
    
</asp:Content>