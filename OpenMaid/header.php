<?php

include_once('config.php');


 global $sys_url;

?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">

<head>
<title>MediaPortal - the Open Source Mediacenter! - Installer</title>
<meta name="description" content="MediaPortal turns your PC in a very advanced Multi-Media Center / HTPC. It allows you to listen to your favorite music &amp; radio, watch your video's and DVD's, view, schedule and record live TV and much more." />
<meta name="keywords" content="CVS, SVN, Snapshot, CVS-Snapshots, MyShare, MediaPortal,Media Portal,Media,Portal,download,open source,open,source" />
<meta name="Generator" content="Joomla! - Copyright (C) 2005 Open Source Matters. All rights reserved." />
<meta name="robots" content="index, follow" />
<base href="http://openmaid.team-mediaportal.com/" />
	<link rel="shortcut icon" href="http://www.team-mediaportal.com/images/favicon.ico" />
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
<link href="http://svn.team-mediaportal.com/style.css" rel="stylesheet" type="text/css" />
<link href="http://www.team-mediaportal.com/templates/jw_onemorething/css/template_css.css" rel="stylesheet" type="text/css" />
	<link href="style.css" rel="stylesheet" type="text/css" />
<link rel="shortcut icon" href="http://www.team-mediaportal.com/images/favicon.ico" />
<!--[if gte IE 6]>
<style type="text/css">
html {overflow-x:hidden;}
</style>
<![endif]-->
</head>

<body id="page_bg" class="red">
<a name="up" id="up"></a>

<div class="center" align="center">
	<div id="wrapper">
		<div id="top">
			<div>
				<div>
					<span id="logo_header" style="filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='http://www.team-mediaportal.com/templates/jw_onemorething/images/omt_logo_header.png',sizingMethod='image');"></span>

          <span id="search2">
<form action="http://www.team-mediaportal.com/search/" method="get">
	<input name="searchword" id="mod_search_searchword" maxlength="20" alt="search" class="inputbox" type="text" size="20" value="search..."  onblur="if(this.value=='') this.value='search...';" onfocus="if(this.value=='search...') this.value='';" />
	<input type="hidden" name="option" value="com_search" />
	<input type="hidden" name="Itemid" value="5" />	
</form></span>
				</div>
			</div>
		</div>
		<div id="middle">

			<div id="middle_2">
				<div id="middle_3">
					<div id="middle_4">
						<div id="navigation">
							<div id="centernav">
								<span id="topnav">
									<ul id="mainlevel"><li class="red"><a href="http://www.team-mediaportal.com">Home</a></li>
<li class="red"><a href="http://www.team-mediaportal.com/about_mediaportal.html">About</a></li>
<li class="red"><a href="http://www.team-mediaportal.com/features.html">Features</a></li>
<li class="red"><a href="http://www.team-mediaportal.com/support_mediaportal.html">Support</a></li>
<li class="red"><a href="http://www.team-mediaportal.com/contribute_to_mediaportal.html">Contribute</a></li>
<li class="red"><a href="http://forum.team-mediaportal.com/">Forum</a></li>
</ul>								</span>
								<div class="clr"></div>
							</div>
						</div>
												<div>

							<table border="0" cellspacing="0" cellpadding="0" width="100%" class="contentarea">
								<tr valign="top">

<td id="leftnav">
										<div id="lefttop">
											<!-- google_ad_section_start -->										
													<div class="moduletable">
							<h3>
					Main Menu				</h3>

				
<table width="100%" border="0" cellpadding="0" cellspacing="0">
<tr align="left"><td><a href="/" class="mainlevel" id="active_menu">Home</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/news/global/" class="mainlevel" >News</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/articles/" class="mainlevel" >Articles</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/download.html" class="mainlevel" >Download</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/files/" class="mainlevel" >Plugins + Extensions</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/recommended_tv_cards/" class="mainlevel" >Recommended HW</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/showcase.html" class="mainlevel" >Showcase Videos</a></td></tr>
<tr align="left"><td><a href="http://wiki.team-mediaportal.com/" class="mainlevel" >Documentation</a></td></tr>

<tr align="left"><td><a href="http://www.team-mediaportal.com/blogs/" class="mainlevel" >Blogs</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/screenshots/" class="mainlevel" >Gallery</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/bookmarks/" class="mainlevel" >Bookmarks</a></td></tr>
<tr align="left"><td><a href="http://www.team-mediaportal.com/component/option,com_contact/Itemid,109/" class="mainlevel" >Contact</a></td></tr>
</table>		</div>
				<div class="moduletable">
							<h3>
					Supporting				</h3>

				<p align="center">
If you like <a title="MediaPortal">MediaPortal</a> please support it by making a small donation: 
</p>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <a href="http://sourceforge.net/donate/index.php?group_id=107397" target="_blank" title="Donate to MediaPortal!"><img src="http://www.team-mediaportal.com/images/stories/x-click-but7.gif" border="0" alt="Donate to MediaPortal!" title="Donate to MediaPortal!" hspace="5" vspace="5" width="72" height="29" align="absmiddle" /></a> 
		</div>
				<div class="moduletable">
							<h3>
					Login Form				</h3>

							<table width="100%" border="0" cellpadding="0" cellspacing="0">
          			<tr align="left"><td>
					PLace for login-form

					</td></tr>
			</table>		</div>
				

				<div class="moduletable">
							<h3>
					RSS Newsfeeds				</h3>
				<div class="small">
<p><a href='http://www.team-mediaportal.com/index2.php?option=ds-syndicate&version=1&feed_id=1'><img src='http://www.team-mediaportal.com/modules/mod_rd_rssfeed/images/feedicon_12.gif' border='0' alt='RSS- Symbol'>&nbsp;MediaPortal Newsfeed</a><br /><span class=''></span></p><hr size='1' /><p><a href='http://www.team-mediaportal.com/files/rss/no_html,1/'><img src='http://www.team-mediaportal.com/modules/mod_rd_rssfeed/images/feedicon_12.gif' border='0' alt='RSS- Symbol'>&nbsp;Recent Plugins & Skins</a><br /><span class=''></span></p><hr size='1' /><p><a href='http://www.team-mediaportal.com/cvs-snapshots/changelog.rss'><img src='http://www.team-mediaportal.com/modules/mod_rd_rssfeed/images/feedicon_12.gif' border='0' alt='RSS- Symbol'>&nbsp;SVN-Changelog</a><br /><span class=''></span></p><hr size='1' /></div>
</div>		</div>

				<div class="moduletable">
							<h3>
					Awards				</h3>
				<a id="fm_file" href="http://www.team-mediaportal.com/images/stories/files/reviews/opcz_6_06_test.pdf" target="_blank" title="Read the whole PDF article"><img src="http://www.team-mediaportal.com/images/stories/news/opcz_testsieger4.jpg" border="0" alt="opcz_testsieger" title="Read the whole PDF article" hspace="10" vspace="10" width="60" height="76" align="left" /></a>
<br />
<br />
<br />
<strong><br />
<br />
<br />
</strong>
		</div>

												</div>
									</td>



									<td>
<div id="pathway">
	<span class="pathway"><a href="http://wwww.team-mediaportal.com" class="pathway">MediaPortal</a> <img src="http://www.team-mediaportal.com/templates/jw_onemorething/images/arrow.png" border="0" alt="arrow" />   MediaPortal Installer </span>										
</div>
										<div id="mainbody">

<table border="0" cellspacing="0" cellpadding="0" align="center">
	<tr>
         <td id="menu">


