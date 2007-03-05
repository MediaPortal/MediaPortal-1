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
    <img style="z-index: -1; width: 100%; position: absolute; height: 100%" height="100%"
      src="images/bg.jpg" width="100%" />
    <table class="main_table" style="height: 100%" cellspacing="0" cellpadding="0" width="980"
      align="center" border="0">
      <tbody>
        <tr height="1">
          <td>
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
                  <td style="margin-right: 2px; padding-top: 0px" align="left" width="170">
                    <table style="width: 160px; text-align: left" cellspacing="4" cellpadding="0">
                    </table>
                    <span class="header_search_span" style="display: none">
                      <input class="header_search_input" id="header_search_box" onkeydown="if(event.keyCode==13){document.location='search.aspx?keyword=' + header_search_box.value}"
                        onblur="if (value == '') {value = 'Search...'};" onfocus="if (value == 'Search...') {value =''};"
                        value="Search..."><span class="header_search_button" onclick="document.location='search.aspx?keyword=' + header_search_box.value">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span></span></td>
                </tr>
                <tr>
                  <td valign="bottom" align="middle" colspan="3">
                    <table cellspacing="0" cellpadding="0">
                      <tbody>
                        <tr>
                          <td class="header_button_td" id="td_header_guide_button" onmouseover="handleButton('header_guide_button',true,'header_button')"
                            onclick="showWait();document.location='guide.aspx'" onmouseout="handleButton('header_guide_button',false,'header_button')">
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
                          <td width="2">
                            <img height="21" hspace="10" src="images/divider.gif" width="2"></td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                  <td style="padding-right: 8px" valign="bottom" align="right" colspan="2">
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
                    <td class="nav_bar" id="ctl00_cph_nav_bar" align="middle">
                      <table width="100%">
                        <tbody>
                          <tr>
                            <td class="nav_bar_text" align="middle">
                              Recording type: <span class="nav_bar_text" loc_string="recordings_recordings_button">
                                <input id="ctl00_cph_recorded_radio" onclick="" type="radio" checked value="recorded_radio"
                                  name="ctl00$cph$recording_type"><label for="ctl00_cph_recorded_radio">Recorded</label></span>&nbsp;<a
                                    id="ctl00_cph_rss_link" href="http://82.148.207.59/rss.aspx?uid=NllEWXJBTVJqQzdqaEhjckVueG5LMGI0VVBldXVpaGZiTjRNT2JoRVF4TGhDSXp3d2pGSVp5QkNXTHRiSHM0SA2"
                                    target="_blank"> </a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span class="nav_bar_text"
                                      loc_string="recordings_scheduled_button"><input id="ctl00_cph_scheduled_radio" onclick="showWait();setTimeout('__doPostBack(\'ctl00$cph$scheduled_radio\',\'\')', 0)"
                                        type="radio" value="scheduled_radio" name="ctl00$cph$recording_type"><label for="ctl00_cph_scheduled_radio">Scheduled</label></span></td>
                            <td>
                              &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
                            <td class="nav_bar_text" align="middle">
                              Group/Sort: <span class="nav_bar_text" loc_string="recordings_group_by_title">
                                <input id="ctl00_cph_sort_by_title" onclick="" type="radio" checked value="sort_by_title"
                                  name="ctl00$cph$sort_by"><label for="ctl00_cph_sort_by_title">Title</label></span>&nbsp;&nbsp;&nbsp;<span
                                    class="nav_bar_text" loc_string="recordings_group_by_date"><input id="ctl00_cph_sort_by_date"
                                      onclick="showWait();setTimeout('__doPostBack(\'ctl00$cph$sort_by_date\',\'\')', 0)"
                                      type="radio" value="sort_by_date" name="ctl00$cph$sort_by"><label for="ctl00_cph_sort_by_date">Date/Time</label></span>
                            </td>
                            <td>
                              &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
                            <td align="right">
                              <table cellpadding="0" align="center">
                                <tbody>
                                  <tr>
                                    <td class="small_button_td" id="td_add_recording" onmouseover="handleButton('add_recording',true,'small_button')"
                                      onclick=";//__doPostBack('ctl00$cph$add_recording$blink', '');" onmouseout="handleButton('add_recording',false,'small_button')">
                                      <a class="small_button_text_off" id="ctl00_cph_add_recording_hlink" style="width: 130px"
                                        onclick="javascript:loadInfo('new_recording')" href="javascript:;"><span class="small_button_text_off"
                                          id="text_add_recording" style="cursor: pointer; position: relative">Add</span></a>
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
                      <div id="scrollContent" style="margin-left: 48px; overflow: auto; height: 100%" onscroll="scrollWindow()">
                        <table cellspacing="0" cellpadding="0" width="880" border="0" runat="server" id="tableList">
                          <tbody>
                            <tr >
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
                              <iframe id="info_frame" src="images/blank.htm" frameborder="0" width="100%" scrolling="no" height="0" allowtransparency></iframe>
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
                                        <a class="icon_button_text_off" id="ctl00_Info_box1_info_close_image_hlink" onclick="javascript:window.parent.closeInfo();" href="javascript:;">
                                        <span class="icon_button_text_off" id="text_info_close_image" style="cursor: pointer; position: relative">&nbsp;&nbsp;&nbsp;</span></a>
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
  </form>
</body>
</html>
