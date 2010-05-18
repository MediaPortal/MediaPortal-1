<%@ Page Language="C#" MasterPageFile="~/mp_repository.Master" AutoEventWireup="true" CodeBehind="single_item.aspx.cs" Inherits="MPRepository.Web.single_item" Title="Untitled Page" %>
<asp:Content ID="singleItemContent" ContentPlaceHolderID="MPRContentHolder1" runat="server">

<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
  <Services>
    <asp:ServiceReference path="UIHelper.asmx" />
  </Services>
</asp:ScriptManager>

<asp:UpdatePanel ID="createItemUpdatePanel" runat="server">
  <ContentTemplate>

    <asp:Label ID="statusLabel" runat="server" Visible="false" />

    <asp:DetailsView ID="itemDetailsView" runat="server" AutoGenerateRows="False" DataSourceID="itemDataSource"
      CellPadding="4" ForeColor="#333333" GridLines="None" DataKeyNames="Id" 
    onitemupdating="itemDetailsView_ItemUpdating">
      <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
      <CommandRowStyle BackColor="#C5BBAF" Font-Bold="True" />
      <RowStyle BackColor="#E3EAEB" />
      <FieldHeaderStyle BackColor="#D0D0D0" Font-Bold="True" />
      <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
      <Fields>
        <asp:BoundField HeaderText="Name" DataField="Name" />
        <asp:BoundField HeaderText="Description" DataField="Description" />
        <asp:BoundField HeaderText="Short Description" DataField="DescriptionShort" />
        <asp:BoundField HeaderText="Author" DataField="Author" />
        <asp:BoundField HeaderText="Homepage" DataField="Homepage"/>
        <asp:BoundField HeaderText="License" DataField="License" />              
        <asp:CheckBoxField HeaderText="Must Accept License" DataField="LicenseMustAccept" />
        <asp:TemplateField HeaderText="Categories">
          <ItemTemplate>
            <asp:Label ID="categoriesLabel" runat="server"><%# concatNames<MPRepository.Items.MPCategory>(Eval("Categories")) %></asp:Label>
          </ItemTemplate>
          <EditItemTemplate>
            <asp:ListBox ID="categoriesList" runat="server" SelectionMode="Multiple" />
          </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Tags">
          <ItemTemplate>
            <asp:Label ID="tagsLabel" runat="server"><%# concatNames<MPRepository.Items.MPTag>(Eval("tags")) %></asp:Label>
          </ItemTemplate>
          <EditItemTemplate>
            <asp:TextBox ID="tagsTextBox" runat="server" />
          </EditItemTemplate>
        </asp:TemplateField>
        <asp:CommandField ShowEditButton="true" ShowDeleteButton="true" />
      </Fields>
      <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
      <EditRowStyle BackColor="#7C6F57" />
      <AlternatingRowStyle BackColor="White" />
    </asp:DetailsView>

    <asp:ObjectDataSource ID="itemDataSource" runat="server"  
      TypeName="MPRepository.Web.Support.MPItemDS" 
      DataObjectTypeName="MPRepository.Items.MPItem" SelectMethod="GetById" 
      OldValuesParameterFormatString="original_{0}" >
        <SelectParameters>
          <asp:QueryStringParameter DefaultValue="" Name="id" QueryStringField="itemid" Type="Int64" />
        </SelectParameters>
    </asp:ObjectDataSource>
    
         
    <hr />
    
    <div id="versionsDiv">
    
      <p>Version Information</p>
      
      <asp:DetailsView ID="versionDetailsView" runat="server" 
        AutoGenerateRows="False" DataSourceID="versionDataSource"
        AllowPaging="True" CellPadding="4" ForeColor="#333333" GridLines="None" DataKeyNames="Id"
        onitemdeleting="versionDetailsView_ItemDeleting" 
        oniteminserting="versionDetailsView_ItemInserting" 
        oniteminserted="versionDetailsView_ItemInserted" 
        onitemupdating="versionDetailsView_ItemUpdating" 
        onpageindexchanged="versionDetailsView_PageIndexChanged" 
        onmodechanging="versionDetailsView_ModeChanging" >
        <PagerSettings Mode="NextPreviousFirstLast" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <CommandRowStyle BackColor="#C5BBAF" Font-Bold="True" />
        <RowStyle BackColor="#E3EAEB" />
        <FieldHeaderStyle BackColor="#D0D0D0" Font-Bold="True" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <Fields>
          <asp:BoundField HeaderText="Version" DataField="Version" />
          <asp:BoundField HeaderText="Development Status" DataField="DevelopmentStatus"  />
          <asp:BoundField HeaderText="Minimum MediaPortal version" DataField="MPVersionMin" />
          <asp:BoundField HeaderText="Maximum MediaPortal version" DataField="MPVersionMax" />
          <asp:BoundField HeaderText="Available to users" DataField="AvailableStatus" />
          <asp:BoundField HeaderText="Release Notes" DataField="ReleaseNotes" />
          <asp:BoundField HeaderText="Uploaded By" DataField="Uploader" ReadOnly="True" InsertVisible="false" />
          <asp:BoundField HeaderText="Update date" DataField="UpdateDate" ReadOnly="True" InsertVisible="false" />
          <asp:TemplateField HeaderText="File" Visible="false">
          <InsertItemTemplate>
            <asp:FileUpload ID="fileUpload" runat="server" />
          </InsertItemTemplate>
          </asp:TemplateField>
          <asp:CommandField ShowEditButton="true" ShowInsertButton="true" 
            ShowDeleteButton="true" InsertText="Add" NewText="Add" />          
        </Fields>
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <EditRowStyle BackColor="#7C6F57" />
        <AlternatingRowStyle BackColor="White" />
      </asp:DetailsView>  
    
      <asp:ObjectDataSource ID="versionDataSource" runat="server"  
        TypeName="MPRepository.Web.Support.MPItemVersionDS" 
        DataObjectTypeName="MPRepository.Items.MPItemVersion" SelectMethod="GetByForeignKey" 
        OldValuesParameterFormatString="original_{0}" InsertMethod="Insert" 
        oninserted="versionDataSource_Inserted" >
          <SelectParameters>
            <asp:Parameter DefaultValue="Item" Name="key" Type="String" />
            <asp:QueryStringParameter DefaultValue="0" Name="value" QueryStringField="itemid" Type="Int64" />
            <asp:Parameter DefaultValue="UpdateDate" Name="sortKey" Type="String" />
            <asp:Parameter DefaultValue="1" Name="direction" Type="Int32" />
          </SelectParameters>
      </asp:ObjectDataSource>

      <p>Files</p>
      <asp:GridView ID="filesGridView" runat="server" AutoGenerateColumns="False" DataSourceID="filesDataSource"
        AllowPaging="True" CellPadding="4" ForeColor="#333333" GridLines="None" DataKeyNames="Id" 
        onrowcommand="filesGridView_RowCommand">
        <RowStyle BackColor="#E3EAEB" />
        <Columns>
          <asp:BoundField HeaderText="Filename" DataField="Filename" />
          <asp:ButtonField HeaderText="Download" ButtonType="Button" CommandName="Download" Text="Download" />
        </Columns>
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
        <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <EditRowStyle BackColor="#7C6F57" />
        <AlternatingRowStyle BackColor="White" />
      </asp:GridView>

      <asp:ObjectDataSource ID="filesDataSource" runat="server"  
        TypeName="MPRepository.Web.Support.MPFileDS"
        SelectMethod="GetByForeignKey" 
        DeleteMethod="Delete" OldValuesParameterFormatString="original_{0}" >
          <SelectParameters>
            <asp:Parameter DefaultValue="ItemVersion" Name="key" Type="String" />
            <asp:ControlParameter ControlID="versionDetailsView" DefaultValue="0" Name="value" PropertyName="DataItem.Id" Type="Int64" />
          </SelectParameters>
      </asp:ObjectDataSource>
          
    </div>

  </ContentTemplate>
  <Triggers>
    <asp:PostBackTrigger ControlID="versionDetailsView" />
  </Triggers>
</asp:UpdatePanel>

</asp:Content>
