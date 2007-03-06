<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TvGuide.aspx.cs" Inherits="TvGuide" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/ xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>MediaPortal Web guide</title>
  <link href="styles/Styles.css" type="text/css" rel="stylesheet" />
  <script src="Script/script.js" type="text/javascript"></script>
</head>
<body bgcolor="#085988" leftmargin="0" topmargin="0" rightmargin="0">
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
    <img style="z-index: -1; width: 100%; position: absolute; height: 100%" height="100%" src="images/bg.jpg" width="100%" />
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="conditional">
      <ContentTemplate>
        <div style="height: 100%">
          <table style="height: 100%" cellspacing="0" cellpadding="0" width="80%" align="left" border="0">
            <tr height="1">
              <td >
                <table style="background-image: url(images//top-bar.gif); color: white; height: 62px"
                  cellspacing="0" cellpadding="0" width="100%" border="0">
                  <tbody>
                    <tr>
                      <td align="middle" width="85" rowspan="2">
                        <img hspace="15" src="images/mplogo_new.png" border="0"></td>
                      <td valign="bottom" align="middle" width="15">
                      </td>
                      <td valign="bottom" align="middle" width="130">
                        <a class="header_message" id="ctl00_Header1_notice"></a>
                      </td>
                      <td valign="bottom" width="250">
                      </td>
                    </tr>
                    <tr>
                      <td valign="bottom" align="middle" colspan="3">
                        <table cellspacing="0" cellpadding="0">
                          <tbody>
                            <tr>
                              <td class="header_button_td" id="td_header_guide_button" onmouseover="handleButton('header_guide_button',true,'header_button')"
                                onclick="showWait();document.location='tvguide.aspx'" onmouseout="handleButton('header_guide_button',false,'header_button')">
                                <span class="header_button_text_off" id="text_header_guide_button" href="tvguide.aspx">
                                  TvGuide </span>
                                <img id="over_image_header_guide_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                              <td class="header_button_td" id="td_header_search_button" onmouseover="handleButton('header_search_button',true,'header_button')"
                                onclick="showWait();document.location='search.aspx'" onmouseout="handleButton('header_search_button',false,'header_button')">
                                <span class="header_button_text_off" id="text_header_search_button" href="search.aspx">
                                  Search </span>
                                <img id="over_image_header_search_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                              <td class="header_button_td" id="td_header_recordings_button" onmouseover="handleButton('header_recordings_button',true,'header_button')"
                                onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_recordings_button',false,'header_button')">
                                <span class="header_button_text_off" id="text_header_recordings_button" href="recordings.aspx">
                                  Recordings </span>
                                <img id="over_image_header_recordings_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                            </tr>
                          </tbody>
                        </table>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </td>
            </tr>
            <tr style="height: 100%">
              <td valign="top">
                <div style="height: 100%">
                  <table style="height: 100%" cellspacing="0" cellpadding="0" width="100%" border="0">
                    <tbody>
                      <tr >
                        <td class="nav_bar" align="middle">
                          <table  width="100%">
                            <tr>
                              <td class="nav_bar_text" align="middle">
                                <asp:DropDownList ID="dropDownDate" runat="server" AutoPostBack="True" OnSelectedIndexChanged="dropDownDate_SelectedIndexChanged" />
                                <asp:DropDownList ID="dropDownTime" runat="server" AutoPostBack="True" OnSelectedIndexChanged="dropDownTime_SelectedIndexChanged" />
                                
                                <span id="spanClock" runat="server" style="padding-left:50px;font-weight: bold; font-size: large;
                                    color: white; font-family: Trebuchet MS">9:06</span>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </td>
            </tr>
          </table>
          <div id="divGuide" style="position: absolute; top: 50px; left: 50px; height: 590px;
            width: 1024px; overflow: auto" runat="server" />
      </ContentTemplate>
    </asp:UpdatePanel>
  </form>
</body>
</html>
