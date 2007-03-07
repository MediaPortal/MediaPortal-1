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
    <img style="z-index: -1; width: 100%; position: absolute; height: 100%" height="100%"
      src="images/bg.jpg" width="100%" />
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="conditional">
      <ContentTemplate>
        <asp:Button ID="showProgram" runat="server" Text="clickme" OnClick="showProgram_Click" CssClass="visibility:hidden"/>
        <asp:HiddenField ID="idProgram" runat="server" />
      <script language="javascript">
      function onProgramClicked(id)
      {
        document.getElementById('idProgram').value=id;
        
       setTimeout('__doPostBack(\'showProgram\',\'\')', 0);
      }
      </script>
        <div style="height: 100%">
          <table style="height: 100%" cellspacing="0" cellpadding="0" width="80%" align="left"
            border="0">
            <tr height="1">
              <td>
                <table style="background-image: url(images//top-bar.gif); color: white; height: 62px"
                  cellspacing="0" cellpadding="0" width="100%" border="0">
                  <tbody>
                    <tr>
                      <td align="middle" width="85" rowspan="2">
                        <a href="Default.aspx">
                          <img hspace="15" src="images/mplogo_new.png" border="0"></a></td>
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
                      <tr>
                        <td class="nav_bar" align="middle">
                          <table width="100%">
                            <tr>
                              <td class="nav_bar_text" align="middle">
                                <asp:DropDownList ID="dropDownDate" runat="server" AutoPostBack="True" OnSelectedIndexChanged="dropDownDate_SelectedIndexChanged" />
                                <asp:DropDownList ID="dropDownTime" runat="server" AutoPostBack="True" OnSelectedIndexChanged="dropDownTime_SelectedIndexChanged" />
                                <span id="spanClock" runat="server" style="padding-left: 50px; font-weight: bold;
                                  font-size: large; color: white; font-family: Trebuchet MS">9:06</span>
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
          <div id="divInfoBox" style="z-index: 2; position: absolute; top: 490px; left: 120px;
            height: 200px; width: 900px; border-right: white 1px solid; border-top: white 1px solid;
            border-left: white 1px solid; border-bottom: white 1px solid;" runat="server" visible="false">
            <table style="background-image: url(/MPWebGuide/images/bg.jpg); height: 100%; width: 100%;"
              cellspacing="0" cellpadding="0">
              <tr>
                <td style="width: 20%;" align="center">
                  <img id="imgLogo" runat="server" src="logos/Nederland 1.png" /></td>
                <td style="width: 70%; padding-left: 15px; padding-right: 15px;">
                  <table border="0" cellpadding="0" cellspacing="0" width="100%">
                    <tr>
                      <td colspan="2">
                        <asp:Label CssClass="header_message" ID="labelTitle" runat="server">titel</asp:Label>
                      </td>
                    </tr>
                    <tr>
                      <td colspan="2">
                        <asp:Label CssClass="recording_list_text" ID="labelDescription" runat="server">description</asp:Label>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <asp:Label CssClass="recording_list_text" ID="labelStartEnd" runat="server">10:00-11:00</asp:Label>
                      </td>
                      <td align="right">
                        <asp:Label CssClass="recording_list_text" ID="labelChannel" runat="server">Net5</asp:Label>
                      </td>
                    </tr>
                    <tr>
                      <td colspan="2">
                        <asp:Label CssClass="recording_list_text" ID="labelGenre" runat="server">genre</asp:Label>
                      </td>
                    </tr>
                  </table>
                </td>
                <td align="right" style="width: 10%;">
                  <table cellpadding="0" cellspacing="0">
                    <tr>
                      <td onclick="document.getElementById('divInfoBox').style.display='none';" style="cursor:pointer;">
                        <img id="buttonClose" src="images/icon_button_close.png" /></td>
                    </tr>
                    <tr>
                      <td class="header_button_td" id="td_header_recorddont_button" onmouseover="handleButton('header_recorddont_button',true,'header_button')"
                        onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_recorddont_button',false,'header_button')">
                        <span class="header_button_text_off" id="text_header_recorddont_button" href="recordings.aspx">
                          Dont record </span>
                        <img id="over_image_header_recorddont_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                    </tr>
                    <tr>
                      <td class="header_button_td" id="td_header_record_button" onmouseover="handleButton('header_record_button',true,'header_button')"
                        onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_record_button',false,'header_button')">
                        <span class="header_button_text_off" id="text_header_record_button" href="recordings.aspx">
                          Once </span>
                        <img id="over_image_header_record_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                    </tr>
                    <tr>
                      <td class="header_button_td" id="td_header_recorddaily_button" onmouseover="handleButton('header_recorddaily_button',true,'header_button')"
                        onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_recorddaily_button',false,'header_button')">
                        <span class="header_button_text_off" id="text_header_recorddaily_button" href="recordings.aspx">
                          Daily </span>
                        <img id="over_image_header_recorddaily_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                    </tr>
                    <tr>
                      <td class="header_button_td" id="td_header_recordweekly_button" onmouseover="handleButton('header_recordweekly_button',true,'header_button')"
                        onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_recordweekly_button',false,'header_button')">
                        <span class="header_button_text_off" id="text_header_recordweekly_button" href="recordings.aspx">
                          weekly </span>
                        <img id="over_image_header_recordweekly_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                    </tr>
                    <tr>
                      <td class="header_button_td" id="td_header_recordworking_button" onmouseover="handleButton('header_recordworking_button',true,'header_button')"
                        onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_recordworking_button',false,'header_button')">
                        <span class="header_button_text_off" id="text_header_recordworking_button" href="recordings.aspx">
                          mon-fri </span>
                        <img id="over_image_header_recordworking_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                    </tr>
                    <tr>
                      <td class="header_button_td" id="td_header_recordalways_button" onmouseover="handleButton('header_recordalways_button',true,'header_button')"
                        onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_recordalways_button',false,'header_button')">
                        <span class="header_button_text_off" id="text_header_recordalways_button" href="recordings.aspx">
                          this channel</span>
                        <img id="over_image_header_recordalways_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                    </tr>
                    <tr>
                      <td class="header_button_td" id="td_header_recordalways2_button" onmouseover="handleButton('header_recordalways2_button',true,'header_button')"
                        onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('header_recordalways2_button',false,'header_button')">
                        <span class="header_button_text_off" id="text_header_recordalways2_button" href="recordings.aspx">
                          every channel </span>
                        <img id="over_image_header_recordalways2_button" style="visibility: hidden" src="images/menu-over.gif"></td>
                    </tr>
                    <tr>
                    <td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </div>
      </ContentTemplate>
    </asp:UpdatePanel>
  </form>
</body>
</html>
