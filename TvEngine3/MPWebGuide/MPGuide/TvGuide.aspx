<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TvGuide.aspx.cs" Inherits="TvGuide" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>MediaPortal Web guide</title>
    <link href="styles/Styles.css" type="text/css" rel="stylesheet" />
</head>
<body bgcolor="#085988" leftmargin="0" topmargin="0" rightmargin="0" style="background-image: url(images/bg.jpg);
    background-repeat: no-repeat">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1"
            DisplayAfter="10">
            <ProgressTemplate>
                <span style="position: absolute; top: 50%; left: 50%; z-index: 2">
                    <object id="FlashWait" height="90" width="90" classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000"
                        codebase="http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=6,0,0,0">
                        <param name="Movie" value="images/wait.swf" />
                        <param name="Src" value="images/wait.swf" />
                        <param name="WMode" value="Transparent" />
                        //Netscape code
                        <embed type="application/x-shockwave-flash" src="images/wait.swf" quality="high"
                            wmode="transparent" id="FlashWaitNs" pluginspage="http://www.macromedia.com/go/getflashplayer"
                            movie="images/wait.swf" name="FlashWait" width="90" height="90">
                 </embed>
                    </object>
                </span>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="conditional">
            <ContentTemplate>
                <div style="height: 100%">
                    <table class="main_table" style="height: 100%" cellspacing="0" cellpadding="0" width="980"
                        align="center" border="0">
                        <tr style="height: 100%">
                            <td valign="top">
                                <div style="height: 100%">
                                    <table style="height: 98%" cellspacing="0" cellpadding="0" width="100%" border="0">
                                        <tbody>
                                            <tr height="1">
                                                <td>
                                                    <table>
                                                        <td width="167px" />
                                                        <td>
                                                            <asp:DropDownList ID="dropDownDate" runat="server" AutoPostBack="True" OnSelectedIndexChanged="dropDownDate_SelectedIndexChanged" />
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="dropDownTime" runat="server" AutoPostBack="True" OnSelectedIndexChanged="dropDownTime_SelectedIndexChanged" />
                                                        </td>
                                                    </table>
                                                </td>
                                                <td colspan="2">
                                                    <!-- show time  !-->
                                                    <table width="880" align="center" border="0">
                                                        <tbody>
                                                            <tr valign="bottom">
                                                                <td width="100%">
                                                                    &nbsp;&nbsp;<span id="spanClock" runat="server" style="font-weight: bold; font-size: large;
                                                                        color: white; font-family: Trebuchet MS">9:06</span></td>
                                                                <td align="right">
                                                                </td>
                                                                <td>
                                                                </td>
                                                                <td align="right">
                                                                </td>
                                                                <td style="padding-top: 5px">
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                    <!-- show time  !-->
                                                </td>
                                            </tr>
                                            <tr>
                                        </tbody>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
                <div id="divGuide" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
