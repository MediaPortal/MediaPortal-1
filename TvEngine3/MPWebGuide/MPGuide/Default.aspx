<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Mediaportal Web guide</title>

    <script src="Script/script.js" type="text/javascript"></script>

    <link href="styles/Styles.css" type="text/css" rel="stylesheet" />
</head>
<body id="body" bottommargin="0" bgcolor="#085988" leftmargin="0" topmargin="0" rightmargin="0">
    <form id="form1" runat="server">
        <div style="height: 100%">
            <img style="z-index: -1; width: 100%; position: absolute; height: 100%" height="100%"
                src="images/bg.jpg" width="100%">
            <table class="main_table" style="height: 100%" cellspacing="0" cellpadding="0" width="980" align="center" border="0">
                <tbody>
                    <tr height="1">
                        <td>
                            <table style="background-image: url(images/top-bar.gif); color: white; height: 62px"
                                cellspacing="0" cellpadding="0" width="100%" border="0">
                                <tbody>
                                    <tr>
                                        <td align="middle" width="85" rowspan="2">
                                            <td valign="bottom" align="middle" width="130">
                                                <a class="header_message" id="ctl00_Header1_notice"></a>
                                            </td>
                                            <td style="margin-right: 2px; padding-top: 0px" align="left" width="170">
                                                <table style="width: 160px; text-align: left" cellspacing="4" cellpadding="0">
                                                    <tbody>
                                                        <tr>
                                                            <td valign="top" width="130">
                                                                <div id="sub_menu" style="border-right: #aaaaaa 1px solid; border-top: #aaaaaa 1px solid;
                                                                    display: none; z-index: 100; filter: progid:DXImageTransform.Microsoft.Alpha(opacity=94);
                                                                    border-left: #aaaaaa 1px solid; width: 130px; border-bottom: #aaaaaa 1px solid;
                                                                    position: absolute; background-color: #effcfa; opacity: .94">
                                                                    <table style="border-right: white 1px solid; border-top: white 1px solid; border-left: white 1px solid;
                                                                        border-bottom: white 1px solid" width="100%">
                                                                        <tbody>
                                                                            <tr>
                                                                                <td style="padding-left: 10px">
                                                                                    <a class="black_bodytext" id="ctl00_Header1_settings_link" href="Settings.aspx">Setup</a>
                                                                                </td>
                                                                            </tr>
                                                                            <tr>
                                                                                <td style="padding-left: 10px">
                                                                                    <a class="black_bodytext" id="ctl00_Header1_HyperLink1" href="about.aspx">Info</a>
                                                                                </td>
                                                                            </tr>
                                                                            <!--<tr>
                            <td style="padding-left:10px">
                                <a id="ctl00_Header1_HyperLink3" class="black_bodytext" href="help.aspx">Help</a>
                            </td></tr>-->
                                                                            <tr>
                                                                                <td style="border-top: #aaaaaa 1px solid; padding-left: 10px; padding-top: 0px">
                                                                                    <a class="black_bodytext" id="ctl00_Header1_LoginStatus1" href="logout.aspx">Log out</a>
                                                                                </td>
                                                                            </tr>
                                                                        </tbody>
                                                                    </table>
                                                                </div>
                                                            </td>
                                                            <td title="...">

                                                                <script language="javascript">
<!--

            function toggleMenu(){
		           if(sub_menu.style.display=='block')
		           {sub_menu.style.display='none'}
		           else{sub_menu.style.display='block'}
				 
			}

//-->
                                                                </script>

                                                                <table cellspacing="0" cellpadding="0" align="center">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td class="icon_button_td" id="td_channel_forward" onmouseover="handleButton('channel_forward',true,'icon_button')"
                                                                                style="background-image: url(images/icon_button_blank.png); width: 25px; height: 26px"
                                                                                onclick="handleButton('channel_forward',false,'icon_button');" onmouseout="handleButton('channel_forward',false,'icon_button')"
                                                                                align="middle">
                                                                                <a class="icon_button_text_off" id="ctl00_Header1_channel_forward_hlink" onclick="javascript:toggleMenu()"
                                                                                    href="javascript:;"><span class="icon_button_text_off" id="text_channel_forward"
                                                                                        style="cursor: pointer; position: relative">...</span></a>
                                                                                <img id="over_image_channel_forward" style="visibility: hidden" src="images/icon_button_blank_over.png"></td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </tbody>
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
                                            <td valign="top">
                                                <div  style=" HEIGHT: 100%">
                                                    <table cellspacing="0" cellpadding="0" width="526" border="0" id="tableRecordings" runat="server">
                                                        <tbody>
                                                            <tr >
                                                                <td class="recording_list_top">
                                                                    <div style="padding-right: 4px; padding-left: 4px; padding-bottom: 4px;  padding-top: 4px;width: 100%;">
                                                                        <span class="info_box_title_text">Recent Recordings</span>
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                    <table cellspacing="0" cellpadding="0" width="526" border="0" id="tableSchedules" runat="server">
                                                        <tbody>
                                                            <tr>
                                                                <td class="recording_list_top">
                                                                    <div style="padding-right: 4px; padding-left: 4px; padding-bottom: 4px;  padding-top: 4px;width: 100%;">
                                                                        <span class="info_box_title_text">Scheduled recordings</span>
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
