<%@ Page Language="C#" AutoEventWireup="true" CodeFile="LogonPage.aspx.cs" Inherits="LogonPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Mediaportal Web guide</title>

    <script src="Script/script.js" type="text/javascript"></script>

    <link href="styles/Styles.css" type="text/css" rel="stylesheet" />
</head>
<body id="body" bottommargin="0" leftmargin="0" topmargin="0" rightmargin="0" bgcolor="#085988">
    <form id="form1" runat="server">
        <img style="position: absolute; z-index: -1; width: 100%; height: 100%" width="100%"
            height="100%" src="images/bg.jpg" />
        <br />
        <br />
        <br />
        <br />
        <br />
        <table id="move_table" width="700" border="0" cellpadding="0" cellspacing="0" align="center">
            <tr>
                <td id="top" class="login_list_top">
                    <span class="login_list_top_text">Login</span>
                </td>
            </tr>
            <tr>
                <td class="login_list_middle" align="center">
                    <div style="width: 100%; border-right: white 1px solid; border-top: white 1px solid;
                        border-left: white 1px solid; border-bottom: white 1px solid;">
                        <table width="100%" style="background-color: #11397A; width: 100%; border-right: black 1px solid;
                            border-top: black 1px solid; border-left: black 1px solid; border-bottom: black 1px solid;">
                            <tr>
                                <td valign="top" width="50%">
                                    <br />
                                    <a href="http://www.team-mediaportal.com" target="_blank">
                                        <img hspace="10" vspace="10" src="images/mplogo_new.png" border="0" /></a>
                                </td>
                                <td class="white_bodytext" align="center">
                                    <table class="white_bodytext" cellspacing="0" cellpadding="1" border="0" id="Login1"
                                        style="border-collapse: collapse;">
                                        <tr>
                                            <td>
                                                <table cellpadding="0" border="0">
                                                    <tr>
                                                        <td class="white_bodytext" align="center" colspan="3">
                                                            Please enter your user information:</td>
                                                    </tr>
                                                    <tr>
                                                        <td class="white_bodytext" align="right">
                                                            <label for="textBoxLogin">
                                                                Login:
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="textBoxLogin" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="textBoxLogin"
                                                                ErrorMessage="Login name is required"></asp:RequiredFieldValidator>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="white_bodytext" align="right">
                                                            <label for="textBox1">
                                                                Password:
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="textBox1"  TextMode="Password" runat="server" />
                                                        </td>
                                                        <td>
                                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="textBox1"
                                                                ErrorMessage="Password is required"></asp:RequiredFieldValidator>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="right" colspan="2">
                                                            <asp:Button ID="button1" runat="server" Text="Sign in" OnClick="buttonSignIn_Click" /></td>
                                                            <td></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
            <tr>
                <td id="Td1" class="login_list_bottom">
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
