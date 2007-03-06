<%@ Page Language="C#" AutoEventWireup="true" CodeFile="recordings.aspx.cs" Inherits="recordings" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>Mediaportal Web guide</title>

  <script src="Script/script.js" type="text/javascript"></script>

  <link href="styles/Styles.css" type="text/css" rel="stylesheet" />
</head>
<body>
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
        <img style="z-index: -1; width: 100%; position: absolute; height: 200%" height="100%" src="images/bg.jpg" width="100%" />
        <table cellspacing="0" cellpadding="0"  border="0">
          <tbody>
            <tr height="1">
              <td>
                <table style="background-image: url(images//top-bar.gif); color: white; height: 62px" cellspacing="0" cellpadding="0" width="100%" border="0">
                  <tbody>
                    <tr>
                      <td  width="85" rowspan="2">
                      <a href="Default.aspx">
                        <img hspace="15" src="images/mplogo_new.png" border="0"></a>
                      </td>
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
                            <tbody>
                              <tr>
                                <td class="nav_bar_text" align="middle">
                                  <td>
                                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
                                  <td class="nav_bar_text" align="middle">
                                    Group/Sort: <span class="nav_bar_text">
                                      <asp:RadioButton runat="server" ID="radioTitle" AutoPostBack="True" OnCheckedChanged="radioTitle_CheckedChanged" />
                                      <label>
                                        Title</label>
                                    </span>&nbsp;&nbsp;&nbsp; <span class="nav_bar_text">
                                      <asp:RadioButton runat="server" type="radio" ID="radioDate" AutoPostBack="True" OnCheckedChanged="radioDate_CheckedChanged" />
                                      <label>
                                        Date/Time</label>
                                    </span>
                                  </td>
                                  <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
                                <td align="right">
                                  <table cellpadding="0" align="center">
                                    <tbody>
                                      <tr>
                                        <td class="small_button_td" onmouseover="handleButton('add_recording',true,'small_button')" onmouseout="handleButton('add_recording',false,'small_button')">
                                          <a class="small_button_text_off" style="width: 130px">
                                          <span class="small_button_text_off" id="text_add_recording" style="cursor: pointer; position: relative">Add</span></a>
                                          <img id="over_image_add_recording" style="visibility: hidden" src="images/small-button-over.gif"></td>
                                      </tr>
                                    </tbody>
                                  </table>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td id="scrollHolder" valign="top">
                          <div style="margin-left: 48px; overflow: auto; height: 510px;" >
                            <table cellspacing="0" cellpadding="0" width="880" border="0" runat="server" id="tableList">
                              <tbody>
                                <tr>
                                  <td class="info_box_top">
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </div>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </td>
            </tr>
            <tr id="info_row" style="visibility: hidden; padding-top: 10px">
              <td style="padding-left: 48px">
                <table cellspacing="0" cellpadding="0" width="880">
                  <tbody>
                    <tr>
                      <td class="info_box_top">
                      </td>
                    </tr>
                    <tr>
                      <td class="info_box_middle" style="padding-right: 7px">
                        <div style="width: 100%">
                          <table cellspacing="0" cellpadding="0" width="100%" border="0">
                            <tbody>
                              <tr>
                                <td>
                                  <iframe id="info_frame" src="images/blank.htm" frameborder="0" width="100%" scrolling="no"
                                    height="0" allowtransparency></iframe>
                                </td>
                                <td valign="top" width="20">
                                  <div id="info_close_button" style="display: none">
                                    <table cellspacing="0" cellpadding="0" align="center">
                                      <tbody>
                                        <tr>
                                          <td class="icon_button_td" id="td_info_close_image" onmouseover="handleButton('info_close_image',true,'icon_button')"
                                            style="background-image: url(images//icon_button_close.png); width: 25px; height: 26px"
                                            onclick="handleButton('info_close_image',false,'icon_button');" onmouseout="handleButton('info_close_image',false,'icon_button')"
                                            align="middle">
                                            <a class="icon_button_text_off" id="ctl00_Info_box1_info_close_image_hlink" onclick="javascript:window.parent.closeInfo();"
                                              href="javascript:;"><span class="icon_button_text_off" id="text_info_close_image"
                                                style="cursor: pointer; position: relative">&nbsp;&nbsp;&nbsp;</span></a>
                                            <img id="over_image_info_close_image" style="visibility: hidden" src="images/icon_button_close_over.png"></td>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </div>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                        </div>
                      </td>
                    </tr>
                    <tr>
                      <td class="info_box_bottom">
                      </td>
                    </tr>
                  </tbody>
                </table>
              </td>
            </tr>
          </tbody>
        </table>
      </ContentTemplate>
    </asp:UpdatePanel>
  </form>
</body>
</html>
