<%@ Page Language="C#" AutoEventWireup="true" CodeFile="showProgram.aspx.cs" Inherits="showProgram" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Program info</title>
</head>
<body bgcolor="#085988" leftmargin="0" topmargin="0" rightmargin="0" style="background-image: url(images/bg.jpg); background-repeat: no-repeat" >
    <form id="form1" runat="server">
    <div>
            <table>
                <tr>
                    <td>
                        Channel:</td>
                    <td>
                        <asp:Label ID="textBoxChannel" runat="server" /></td>
                </tr>
                <tr>
                    <td>
                        from:</td>
                    <td>
                        <asp:Label ID="textBoxStart" runat="server" />-<asp:Label ID="textEnd" runat="server" /></td>
                </tr>
                <tr>
                    <td>
                        Title:</td>
                    <td>
                        <asp:Label ID="textBoxTitle" runat="server" /></td>
                </tr>
                <tr>
                    <td>
                        Genre:</td>
                    <td>
                        <asp:Label ID="textBoxGenre" runat="server" /></td>
                </tr>
                <tr>
                    <td>
                        Description:</td>
                    <td>
                        <asp:Label ID="textBoxDescription" runat="server" /></td>
                </tr>
            </table>
    
    </div>
    </form>
</body>
</html>
