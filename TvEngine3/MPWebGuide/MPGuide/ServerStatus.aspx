<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ServerStatus.aspx.cs" Inherits="ServerStatus" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
  <title>Mediaportal Web guide</title>

  <script src="Script/script.js" type="text/javascript"></script>

  <link href="styles/Styles.css" type="text/css" rel="stylesheet" />
</head>
<body id="body" bottommargin="0" bgcolor="#085988" leftmargin="0" topmargin="0" rightmargin="0">
  <form id="form1" runat="server">
    <div style="height: 100%">
      <img style="z-index: -1; width: 100%; position: absolute; height: 100%" height="100%"
        src="images/bg.jpg" width="100%" >
      <table style="height: 100%;" cellspacing="0" cellpadding="0" width="980" align="center"
        border="0">
        <tbody>
          
            <tr height="1">
              <td>
                <table style="background-image: url(images//top-bar.gif); color: white; height: 62px" cellspacing="0" cellpadding="0" width="100%" border="0">
                  <tbody>
                    <tr>
                      <td align="middle" width="85" rowspan="2">
                      <a href="Default.aspx">
                        <img hspace="15" src="images/mplogo_new.png" border="0"></a>
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
            <td valign="top" style="height: 100%">
              <div style="height: 100%">
                <table height="100%" width="100%" border="0" cellspacing="0" cellpadding="0">
                  <tbody>
                    <tr>
                      <td style="padding-right: 20px" valign="top" align="middle" width="41%">
                        <br>
                        <br>
                        <span style="width: 320px; text-align: center">
                          <asp:Label CssClass="main_date" ID="labelDate" runat="server">woensdag 28 februari 2007</asp:Label><br>
                          <asp:Label CssClass="main_time" ID="labelTime" runat="server" /></span>
                        <br>
                        <br>
                        <br>
                        <table cellpadding="0" align="center">
                          <tbody>
                            <tr>
                              <td class="main_button_td" id="td_guide_button" onmouseover="handleButton('guide_button',true,'main_button')"
                                onclick="showWait();document.location='tvguide.aspx'" onmouseout="handleButton('guide_button',false,'main_button')">
                                <a class="main_button_text_off" id="ctl00_cph_guide_button_hlink" style="width: 175px"
                                  onclick="" href="tvguide.aspx"><span class="main_button_text_off" id="text_guide_button"
                                    style="cursor: pointer; position: relative">Tv Guide</span></a>
                                <img id="over_image_guide_button" style="visibility: hidden" src="images/main-button-over.gif"></td>
                            </tr>
                          </tbody>
                        </table>
                        <table cellpadding="0" align="center">
                          <tbody>
                            <tr>
                              <td class="main_button_td" id="td_search_button" onmouseover="handleButton('search_button',true,'main_button')"
                                onclick="showWait();document.location='search.aspx'" onmouseout="handleButton('search_button',false,'main_button')">
                                <a class="main_button_text_off" id="ctl00_cph_search_button_hlink" style="width: 175px"
                                  onclick="" href="search.aspx"><span class="main_button_text_off" id="text_search_button"
                                    style="cursor: pointer; position: relative">Search</span></a>
                                <img id="over_image_search_button" style="visibility: hidden" src="images/main-button-over.gif"></td>
                            </tr>
                          </tbody>
                        </table>
                        <table cellpadding="0" align="center">
                          <tbody>
                            <tr>
                              <td class="main_button_td" id="td_recordings_button" onmouseover="handleButton('recordings_button',true,'main_button')"
                                onclick="showWait();document.location='recordings.aspx'" onmouseout="handleButton('recordings_button',false,'main_button')">
                                <a class="main_button_text_off" id="ctl00_cph_recordings_button_hlink" style="width: 175px"
                                  onclick="" href="recordings.aspx"><span class="main_button_text_off" id="text_recordings_button"
                                    style="cursor: pointer; position: relative">Recordings</span></a>
                                <img id="over_image_recordings_button" style="visibility: hidden" src="images/main-button-over.gif"></td>
                            </tr>
                          </tbody>
                        </table>
                      </td>
                      <td valign="middle">
                        <div style="height: 100%">
                          <table cellspacing="0" cellpadding="0" width="526" border="0" id="tableStatus"
                            runat="server">
                            <tbody>
                              <tr>
                                <td class="recording_list_top">
                                  <div style="padding-right: 4px; padding-left: 4px; padding-bottom: 4px; padding-top: 4px;
                                    width: 100%;">
                                    <span class="info_box_title_text">Server status</span>
                                  </div>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                          <table cellspacing="0" cellpadding="0" width="526" border="0" id="tableClients"
                            runat="server">
                            <tbody>
                              <tr>
                                <td class="recording_list_top">
                                  <div style="padding-right: 4px; padding-left: 4px; padding-bottom: 4px; padding-top: 4px;
                                    width: 100%;">
                                    <span class="info_box_title_text">Streaming clients</span>
                                  </div>
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
        </tbody>
      </table>
    </div>
  </form>
</body>
</html>
