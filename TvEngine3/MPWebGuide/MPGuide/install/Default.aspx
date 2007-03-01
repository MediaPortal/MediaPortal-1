<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="install_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            Please specify the host name of the master tv server
            <table>
                <tr>
                    <td>
                        TvServer:</td>
                    <td colspan="2">
                        <asp:TextBox ID="idTvserver" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td>
                    </td>
                    <td>
                        <asp:Button ID="idTest" runat="server" Text="Test" OnClick="idTest_Click" /></td>
                    <td>
                        <asp:Button ID="idSave" runat="server" Text="Save" visible="false" OnClick="Button1_Click" />
                    </td>
                </tr>
            </table>
            <table id="tableResult" runat="server" visible="false">
                <tr>
                    <td>
                        <asp:Label runat="server" ID="textBoxResult" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label runat="server" ID="textBoxDatabaseResult" />
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
