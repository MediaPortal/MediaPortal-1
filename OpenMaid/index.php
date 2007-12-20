<?php
require_once('functions.php');
require_once('authentication.php');
include('header.php');

//show search bar.  links, breadcrumblink, enable/disable search box.
SearchBar("<a href=\"" . $sys_url . "extra.php\">Stats & Tools</a>&nbsp;&nbsp;|&nbsp;&nbsp<a href=\"$sys_url\">OpenMAID</a>","",TRUE);

echo "<tr><td id=\"contentarea\"><div id=\"leftbar\">";

$u = Authenticate();

if ($u <> "") {
	echo "<p>Welcome, $u ";
	$usersplugins = findUsersPlugins($u);
	$usersplugins_count = mysql_numrows($usersplugins);
	if ($usersplugins_count > 0) echo "&nbsp;&nbsp;&nbsp;(<a href='extra.php?mode=manageplugins'>Manage My Plugins</a>)";
	if (IsAdmin($u)) echo "&nbsp;&nbsp;&nbsp;(<a href='admin.php'>admin</a>)";
	echo "</p><br />";
	}
else echo '<p><a href="' . GetLogonURL() . '">Login to upload</a></p>';

global $db_host;
global $db_user;
global $db_pass;
global $db_database;


$nb_theme = 0;
$nv_icon = 0;
$nb_general = 0;
$nb_input = 0;
$nb_module = 0;
$nb_import = 0;
$nb_wizard = 0;
$nb_web = 0;
$nb_ext = 0;
$nb_misc = 0;
$nb_hack = 0;

@mysql_connect($db_host, $db_user, $db_pass) or slowDie("Error connecting to sql " . $db_user );

@mysql_select_db($db_database) or slowDie("Error connecting to db");

//Added way to vote without using ajax (robogeek)
if ($_GET["mode"] == "vote") {

	//confirm profil_id
	if ($u == null || $u !== $_GET['profile_id']) die("Are you trying to hack the vote?");

	//confirm plugin_id and plugin_version
	$getPluginId = $_GET['plugin_id'];
	$getPluginVersion = $_GET['plugin_version'];
	$res = mysql_query("SELECT * FROM plugins WHERE plugin_ID='$getPluginId' AND plugin_Version='$getPluginVersion'");
	$numrows = mysql_numrows($res);

	if ($numrows <> 1) die("Are you scamming an old or non-existant plugin?");

	//confirm vote
	$v = $_GET['vote'];
	if ($v == null || $v == "") die("You forgot to vote?");
	if ($v !== "true" && $v !== "false") die("Invalid vote");
	VoteWorking($_GET['profile_id'], $getPluginId, $getPluginVersion, $v, null);
}

$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Icon' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_icon = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Theme' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_theme = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'General' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_general = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Input' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_input = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Module' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_module = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Import' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_import = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Extension' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_ext = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Wizard' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_wizard = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Misc' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_misc = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Web' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_web = $nb;
$sql = "SELECT * FROM plugins WHERE plugin_Type = 'Hack' and plugin_Current = true";
$res = mysql_query($sql); $nb = mysql_numrows($res);
$nb_hack = $nb;
$total = $nb_icon +  $nb_theme + $nb_general + $nb_input + $nb_module + $nb_import + $nb_wizard + $nb_web + $nb_ext + $nb_misc + $nb_hack;
echo "<table width='450px'><tr><th id=\"remositorycontainerhead\">Containers</th><th>Files</th></tr><tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=theme" . " class=\"topictitle\"><strong> Theme</strong></a></h3></td><td>($nb_theme)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=icon" . " class=\"topictitle\"><strong> Icon sets</strong></a></h3></td><td>($nb_icon)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=import" . " class=\"topictitle\"><strong> Import</strong></a></h3></td><td>($nb_import)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=module" . " class=\"topictitle\"><strong> Module</strong></a></h3></td><td>($nb_module)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=general" . " class=\"topictitle\"><strong> General</strong></a></h3></td><td>($nb_general)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=input" . " class=\"topictitle\"><strong> Input</strong></a></h3></td><td>($nb_input)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=extension" . " class=\"topictitle\"><strong> Extension</strong></a></h3></td><td>($nb_ext)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=wizard" . " class=\"topictitle\"><strong> Wizard</strong></a></h3></td><td>($nb_wizard)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=misc" . " class=\"topictitle\"><strong> Misc.</strong></a></h3></td><td>($nb_misc)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=web" . " class=\"topictitle\"><strong> Web</strong></a></h3></td><td>($nb_web)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=hack" . " class=\"topictitle\"><strong> Hack</strong></a></h3></td><td>($nb_hack)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr>";
echo "<tr><td><h3><img src=\"http://www.team-mediaportal.com/components/com_remository/images/folder_icons/folder_yellow.gif\" alt=\"Theme\" height=\"16\" width=\"16\"><a href=" . "thelist.php?program=meedio&ptype=&start=0" . "><strong> All</strong></a></h3></td><td>($total)</td></tr><tr class=\"remositoryfolderinfo\"><td><p>add a valid description for this container</p></td></tr><br /></table>";

//removed search box here...it's in the link bar at the top of the screen now.
//echo "<form action=\"thelist.php\" method=\"get\"><input name=\"search\"><input type=\"submit\" name=\"Search\" value=\"Search\"></form><p>";

if ($debug) echo "<a href='xtern.php?summary'>(debug) test xtern</a><br>";
if ($u <> "") echo "<a href='upload.php'>Add plugin</a><br />";
else echo "<span title=\"Login to the forum before uploading\">add/update plugin</span><br />";

	//reoved admin link from here...it's by the welcome message if the currently logged on user is an admin
	//if ($debug || IsAdmin($u)) echo "<br><a href='admin.php'>admin panel</a> (this link displays for admins only)<br>";
?>

</div><div id="rightbar">

<?php
echo "<br><p><b>Last 10 Updated:</b><br><br>";
//Original line was: $res = mysql_query("SELECT * FROM plugins ORDER BY plugin_Date DESC LIMIT 0,10");
//Was showing duplicate plugins in the list if the plugin was updated more than once recently
//The query now shows the last 10 updated plugins without duplicating entries (robogeek)
$res = mysql_query("SELECT plugin_ID, plugin_Name, plugin_Author, max(plugin_Date) as maxdate FROM plugins WHERE plugin_ReviewFlag = 'FALSE' GROUP BY plugin_ID ORDER BY maxdate desc LIMIT 10");
while (($enreg=@mysql_fetch_array($res)))
{
	echo "<a href=\"detail.php?plugin_id=" . $enreg["plugin_ID"] . "\"><b>" . $enreg["plugin_Name"] . "</b></a> by <a href=\"thelist.php?author=" . urlencode($enreg["plugin_Author"]) . "\"><i>" . $enreg["plugin_Author"] . "</i></a><br>";
}

echo "<hr />";
echo "<br><p><b>10 Most Downloaded:</b><br><br>";
//Original line was: $res = mysql_query("SELECT * FROM plugins ORDER BY plugin_DownloadCount DESC LIMIT 0,10");
//Now we use the userdownloads table and we need to account for multiple versions of a plugin
$res = mysql_query("SELECT plugins.plugin_ID, plugins.plugin_Name, plugins.plugin_Author, plugins.plugin_Version, userdownloads.plugin_Version, userdownloads.plugin_ID, count(*) AS dl_num FROM userdownloads, plugins WHERE plugins.plugin_ID = userdownloads.plugin_ID AND plugins.plugin_Version = userdownloads.plugin_Version GROUP BY userdownloads.plugin_ID ORDER BY dl_num DESC LIMIT 10");
while (($enreg=@mysql_fetch_array($res)))
{
	echo "<a href=\"detail.php?plugin_id=" . $enreg["plugin_ID"] . "\"><b>" . $enreg["plugin_Name"] . "</b></a> by <a href=\"thelist.php?author=" . urlencode($enreg["plugin_Author"]) . "\"><i>" . $enreg["plugin_Author"] . "</i></a><br>";
}

echo "<hr />";
echo "<br><p><b>Your Last 10 Downloads:</b><br><br>";
$where = "WHERE download_profil_id='$u'";
if ($u == "binary64") $where = "";
$res = mysql_query("SELECT * FROM userdownloads $where ORDER BY download_Date DESC LIMIT 0,10");
while (($enreg=@mysql_fetch_array($res)))
{
	$plugin_id = $enreg["plugin_ID"];
	$plugin_version = $enreg["plugin_Version"];
	$plugin_date = $enreg["plugin_Date"];
	$profil_id = $enreg["profil_id"];
	//is vote still latest version?
	$res2 = mysql_query("SELECT * FROM plugins WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version'");
	$enreg2 = @mysql_fetch_array($res2);
	if ($enreg2["plugin_Version"] == $plugin_version)
	{
		echo "<a href=\"detail.php?plugin_id=" . $enreg2["plugin_ID"] . "\"><b>" . $enreg2["plugin_Name"] . "</b></a> by <a href=\"thelist.php?author=" . urlencode($enreg2["plugin_Author"]) . "\"><i>" . $enreg2["plugin_Author"] . "</i></a> ";
		VoteWorking($u, $plugin_id, $plugin_version, "", "index.php?");
	}
}
?>

</div>

<?php
include('footer.php');
?>
