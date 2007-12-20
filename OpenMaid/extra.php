<?php
require_once('functions.php');
require_once('process.functions.php');
require_once('authentication.php');
require_once('db.php');
require_once('archive.php');
//session_start();

include('header.php');

?>
<div id="menudiv">
<b><a href="<?php echo $sys_url;?>extra.php">Stats & Tools</a>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;<a href="<?php echo $sys_url; ?>">OpenMAID</a></b>
</div>
</td></tr><tr>
<td id="contentarea">


<?php
$mode = $_GET["mode"];
$u = Authenticate();
$usersplugins = findUsersPlugins($u);
$usersplugins_count = mysql_numrows($usersplugins);

//if user isn't logged in or user has no plugins, use widebar (no rightside menu) 
if (($usersplugins_count > 0) && ($u != "")) echo '<div id="leftbar">';
else echo '<div id="widebar">';

if ($u != "") printPara ("Welcome, $u");

echo "<h1>Stats & Tools</h1>";

if($mode == "")
	$mode = "home";

switch ($mode)
{
	case "home":
		printToolHome($u);
		break;
	case "manageplugins":
		printManagePlugins($u);
		break;
	case "check":
		printCheckForProblems($u);
		break;
}	



function printLine($line)
{
	echo "$line<br />\n";
}



function printPara($para)
{
	echo "<p>$para</p>\n";
}



function printH2($header)
{
	echo "<h2>$header</h2>\n";
}



function printSectionTitle($title)
{
	printH2($title);
	echo "<hr />\n";
}



function printToolHome($user)
{
	global $ftp_repository;
	global $ftp_manual_uploads;

	printSectionTitle("OpenMAID Stats");
	
	//If user is not logged in, display message and link to allow user to login or register with the forum

	//Display total # of all versions of all plugins in db
	//Display total # plugins voted working
	//Display total # of unique plugins in db
	//Display # of all versions of your plugins
	//Display # of your unique plugins
	//Display author/forum id who has most versions of plugins
	//Display author/forum id who has most # of unique plugins
	
	$sql = "SELECT plugin_ID FROM plugins WHERE plugin_Current = true";
	$res = mysql_query($sql); $active_plugins = mysql_numrows($res);
	$sql = "SELECT plugin_ID FROM plugins WHERE plugin_ReviewFlag != true";
	$res = mysql_query($sql); $total_plugins = mysql_numrows($res);
	printLine("OpenMAID has $active_plugins active plugins ($total_plugins including previous versions).");

	$today = date("Y-m-d H:i:s");
	$year = substr($today, 0,4);
	$month = substr($today, 5, 2);
	$day = substr($today, 8, 2);
	$history_date = date("Y-m-d H:i:s", mktime(0, 0, 0, $month, $day - 30, $year));
	$sql = "SELECT plugin_ID FROM plugins WHERE plugin_Date >= '$history_date' AND plugin_ReviewFlag != 'TRUE'";
	$res = mysql_query($sql); $num_uploads_last_month = mysql_numrows($res);
	$sql = "SELECT plugin_ID FROM plugins WHERE plugin_ReviewFlag != 'TRUE' AND plugin_current = true AND plugin_Date >= '$history_date'";
	$res = mysql_query($sql); $num_unique_uploads_last_month = mysql_numrows($res);
	printLine("$num_unique_uploads_last_month plugins have been updated during the past 30 days.");
	printLine("$num_uploads_last_month plugin versions have been uploaded during the past 30 days.");
	
	
	//printLine("Print various OpenMAID stats (and user stats if user is logged in):");
	//printLine("* Total plugins voted working");
	//printLine("* Total plugins downloaded by user");
	//printLine("* Total plugins uploaded by user");
	//printLine("* Author with most uploaded plugins");
}



function printConfirmButton($name, $DisplayName)
{
	echo '<form method="post"><input type="hidden" name="' . $name . '" value="true" /> <input type="submit" value="' . $DisplayName . '" /></form>';
}



function printManagePlugins($user)
{
	//let user manage plugin metadata
	printSectionTitle("Manage My Plugins");
	manage_myplugins("", "", $user);
	//printLine("Stay tuned... this feature will be implemented soon!<br>");
	//printLine("List plugins belonging to logged in user with list of actions:");
	//printLine("* Link to upload page");
	//printLine("* Link to edit each plugin metadata");
	//printLine("* Link to flag a plugin for deletion?");
	//printLine("* Highlight plugin red if it's voted not working");
	//printLine("* Highlight plugin yellow if it's missing some metadata");

//	printPara("Please put info here to manage database");
}



function printCheckForProblems($user)
{
	//check users plugins for missing metadata
	printSectionTitle("Check for Problems");
	printLine("Checking for plugins with potentially important missing metadata.<br>");
	manage_metadata("", "", $user);
	//printLine("Stay tuned... this feature will be implemented soon!<br>");
	//printLine("Check for problems function will check the following (amongst other things):");
	//printLine("* Check for any plugins with potentially important missing metadata");
	//printLine("* Check if any of your plugins are marked as flagged for review");
}


//If user isn't logged in or user has no plugins, don't show side menu
if (($usersplugins_count > 0) && ($u != "")){
	echo '</div><div id="rightbar">';
	echo '<a href="' . $sys_url . '">OpenMAID Home</a><br />';
	echo '<a href="?mode=home">Stats & Tools Home</a><br />';
	//if user has plugins, show Manage Plugins menu item
	echo '<a href="?mode=manageplugins">Manage My Plugins</a><br />';
	//if user has plugins, show Check Plugin Metadata menu item
	echo '<a href="?mode=check">Check Plugin Metadata</a><br />';
	}
	echo '</div>';

	
include('footer.php');
?>
