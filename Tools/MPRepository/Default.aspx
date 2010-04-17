<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MPRepository.Web._Default" MasterPageFile="~/mp_repository.Master" %>

<asp:Content ID="defaultContent" runat="server" ContentPlaceHolderID="MPRContentHolder1" >
  
  <div>
      <p />
      Logged in as 
      <asp:LoginName ID="LoginName1" runat="server" />
      <br />
      <asp:LoginStatus ID="LoginStatus1" runat="server" />
    
  </div>
    
</asp:Content>
