<%@ Page Language="C#" AutoEventWireup="true" CodeFile="LogonPage.aspx.cs" Inherits="LogonPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
  <title>Mediaportal Web guide</title>
</head>
<body id="body" bgcolor="#085988">
  <form id="form1" runat="server">
    <table  style="height: 100%" cellspacing="0" cellpadding="0" align="center" border="0" >
    <tr><td>&nbsp;</td></tr>
    <tr><td>&nbsp;</td></tr>
    <tr><td>&nbsp;</td></tr>
    <tr><td>&nbsp;</td></tr>
    <tr><td>&nbsp;</td></tr>
    <tr><td>&nbsp;</td></tr>
    <tr><td>&nbsp;</td></tr>
      <tr>
        <td style="FONT-SIZE: 10pt; COLOR: #ffffff; FONT-FAMILY: 'Trebuchet MS', Verdana, Arial">
          Login:</td>
        <td>
          <asp:TextBox ID="textBoxLogin" runat="server" />
        </td>
      </tr>
      <tr>
        <td  style="FONT-SIZE: 10pt; COLOR: #ffffff; FONT-FAMILY: 'Trebuchet MS', Verdana, Arial">
          Password:</td>
        <td>
          <asp:TextBox ID="textBox1" runat="server" />
        </td>
      </tr>
      <tr>
        <td  style="FONT-SIZE: 10pt; COLOR: #ffffff; FONT-FAMILY: 'Trebuchet MS', Verdana, Arial">
          <asp:Button ID="buttonSignIn" runat="server" Text="Sign in" OnClick="buttonSignIn_Click" />
        </td>
      </tr>
    </table>
  </form>
</body>
</html>
