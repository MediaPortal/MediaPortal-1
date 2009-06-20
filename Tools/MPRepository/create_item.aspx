<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="create_item.aspx.cs" Inherits="MPRepository.Web.create_item" EnableEventValidation="false" MasterPageFile="~/mp_repository.Master" %>

<asp:Content ID="createItemContent" runat="server" ContentPlaceHolderID="MPRContentHolder1" >

  <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
    <Services>
      <asp:ServiceReference path="UIHelper.asmx" />
    </Services>
  </asp:ScriptManager>

  <asp:UpdatePanel ID="createItemUpdatePanel" runat="server">
    <ContentTemplate>

      <asp:Label ID="uploadStatusLabel" runat="server" />
      
      <table>
      <tr><td>Title: <asp:TextBox ID="titleTextBox" runat="server" /></td></tr>
      <tr><td>File: <asp:FileUpload ID="fileUpload" runat="server" /></td></tr>
      <tr>
        <td>
          Type:<br />
          <asp:ListBox ID="typesList" runat="server" AutoPostBack="True" />
        </td>              
        <td>
          Categories:<br />
          <asp:ListBox ID="categoriesList" runat="server" SelectionMode="Multiple" />
        </td>
      </tr>
      <tr><td>Description:<br /><asp:TextBox ID="descriptionTextBox" runat="server" Rows="15" TextMode="MultiLine" /></td></tr>
      <tr><td>Tags:<asp:TextBox ID="tagsTextBox" runat="server" /></td></tr>
      <tr><td>Short Description: <asp:TextBox ID="descriptionShortTextBox" runat="server" Rows="5" TextMode="MultiLine" /></td></tr>
      <tr><td>License: <asp:TextBox ID="licenseTextBox" runat="server" Rows="5" TextMode="MultiLine" /></td></tr>     
      <tr><td>Must Agree to License: <asp:CheckBox ID="licenseMustAccessCheckBox" runat="server" /></td></tr>
      <tr><td>Author: <asp:TextBox ID="authorTextBox" runat="server" /></td></tr>
      <tr><td>Homepage: <asp:TextBox ID="homepageTextbox" runat="server" /></td></tr>
      <tr><td><hr /></td></tr>
      <tr><td>Version: <asp:TextBox ID="versionTextBox" runat="server" /></td></tr>
      <tr><td>Minimum Media Portal Version: <asp:TextBox ID="mpVersionMinTextBox" runat="server" /></td></tr>
      <tr><td>Maximum Media Portal Version: <asp:TextBox ID="mpVersionMaxTextBox" runat="server" /></td></tr>
      <tr><td>Development Status: <asp:DropDownList ID="developmentStatusDropDownList" runat="server" /></td></tr>
      <tr><td></td></tr>
      <tr><td><asp:Button ID="submitButton" runat="server" Text="Submit File" /></td></tr>
      </table>   
  
    </ContentTemplate>
    <Triggers>
      <asp:PostBackTrigger ControlID="submitButton" />
    </Triggers>
  </asp:UpdatePanel>
    
    
</asp:Content>