<?php
require_once('functions.php');
require_once('authentication.php');
require_once('db.php');

//Need to do this stuff before the header or it will screw up.
//Get number of items to display per page and set a cookie
//If no cookie exists and no nb paramater is set, the default is 5 items per page 
if (isset($_GET["nb"]))  $nb = $_GET["nb"];
if (!isset($_GET["nb"]) & isset($_COOKIE["itemsperpage"]))  $nb = $_COOKIE["itemsperpage"];
if (!$nb) $nb = 5;
if ($nb) setcookie("itemsperpage", $nb, time()+315360000);

//Get sort filter setting to filter the listing of plugins
//If no cookie exists and no filter paramater is set, the default filter is to sort by plugin_Name 
if (!isset($_GET["filter"]) & isset($_COOKIE["sortfilter"]))  $sort_filter = $_COOKIE["sortfilter"];
if (isset($_GET["filter"])) $sort_filter = $_GET["filter"];
if (!$sort_filter) $sort_filter = "name";
if ($sort_filter) setcookie("sortfilter", $sort_filter, time()+315360000);

//Get sort order setting to order the listing of plugins in ascending or descending order
//If no cookie exists and no order paramater is set, the default order is ascending order 
if (!isset($_GET["order"]) & isset($_COOKIE["sortorder"]))  $sort_order = $_COOKIE["sortorder"];
if (isset($_GET["order"])) $sort_order = $_GET["order"];
if (!$sort_order && ($sort_filter == "total downloads" || $sort_filter == "current downloads" || $sort_filter == "date") ) $sort_order = "descending";
else if (!$sort_order) $sort_order = "ascending";
if ($sort_order) setcookie("sortorder", $sort_order, time()+315360000);

//After cookie is set, now we can load the header.
include('header.php');
$u = Authenticate();

/*
//<div id="menudiv">
//<b><a href="<?php echo $sys_url;?>extra.php">Stats & Tools</a>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;<a href="<?php echo $sys_url; ?>">OpenMAID</a></b>
//</div>
//</td></tr>
*/
//show search bar.  links, breadcrumblink, enable/disable search box.
SearchBar("<a href=\"" . $sys_url . "extra.php\">Stats & Tools</a>&nbsp;&nbsp;|&nbsp;&nbsp<a href=\"$sys_url\">OpenMAID</a>","",TRUE);

echo "<tr><td id=\"contentarea\"><div id=\"widebar\">";

global $db_host;
global $db_user;
global $db_pass;
global $db_database;
global $plugin_home_directory;

$breadcrumb = "";
$breadcrumb2 = "";

if (isset($_GET["ptype"])) {$ptype = $_GET["ptype"]; $breadcrumb = "&ptype=$ptype";}
if (isset($_GET["start"])) {$start = $_GET["start"]; $breadcrumb .= "&start=$start";}
if (isset($_GET["program"])) {$program = $_GET["program"]; $breadcrumb .= "&program=$program";}
if (isset($_GET["filter"])) {$sort_filter = $_GET["filter"]; $breadcrumb .= "&filter=$sort_filter";}
if (isset($_GET["order"])) {$sort_order = $_GET["order"]; $breadcrumb .= "&order=$sort_order";}
if (isset($_GET["search"])) {$search = $_GET["search"]; $breadcrumb .= "&search=" . urlencode($search);}
if (isset($_GET["author"])) {$author = $_GET["author"]; $breadcrumb .= "&author=" . urlencode($author);}
if (isset($_GET["profile_id"])) {$p_id = $_GET["profile_id"]; $breadcrumb .= "&profile_id=" . urlencode($p_id);}


if (isset($_GET["ptype"])) {$ptype = $_GET["ptype"]; $breadcrumb2 = "&ptype=$ptype";}
if (isset($_GET["program"])) {$program = $_GET["program"]; $breadcrumb2 .= "&program=$program";}
if (isset($_GET["filter"])) {$sort_filter = $_GET["filter"]; $breadcrumb2 .= "&filter=$sort_filter";}
if (isset($_GET["order"])) {$sort_order = $_GET["order"]; $breadcrumb2 .= "&order=$sort_order";}
if (isset($_GET["search"])) {$search = $_GET["search"]; $breadcrumb2 .= "&search=" . urlencode($search);}
if (isset($_GET["author"])) {$author = $_GET["author"]; $breadcrumb2 .= "&author=" . urlencode($author);}
if (isset($_GET["profile_id"])) {$p_id = $_GET["profile_id"]; $breadcrumb2 .= "&profile_id=" . urlencode($p_id);}



if ($sort_filter == "name") $sf = "plugin_Name";
if ($sort_filter == "date") $sf = "plugin_Date";
//if ($sort_filter == "type") $sf = "plugin_Type";
if ($sort_filter == "author") $sf = "plugin_Author";
if ($sort_filter == "profile_id") $sf = "plugin_profil_id";
if ($sort_filter == "total downloads") $sf = "plugin_DownloadCountTotal";
if ($sort_filter == "current downloads") $sf = "plugin_DownloadCount";
//if ($sort_filter == "popularity") $sf = "popularitycount";
//if ($sort_filter == "votes working") $sf = "votecount";
//if ($sort_filter == "votes not working") $sf = "votecount";

if ($sort_order == "ascending") $so = "ASC";
if ($sort_order == "descending") $so = "DESC";

// set variables if missing
if (!isset($ptype)) $ptype = "";
if (!isset($start)) $start = 0;

@mysql_connect($db_host, $db_user, $db_pass) or die("Error");
@mysql_select_db($db_database) or die("Error");

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

$like = " ";
if (isset($ptype) && $ptype <> "") $like = $like . "WHERE plugin_Type='$ptype'";
if (isset($_GET["author"]))
{
	if ($like <> " ") $like = $like . " AND";
	else $like = $like . "WHERE";
	$like = $like . " plugin_Author ='" . $_GET["author"] . "'";
	
	$Author = true;
}
if (isset($_GET["profile_id"]))
{
	if ($like <> " ") $like = $like . " AND";
	else $like = $like . "WHERE";
	$like = $like . " profil_id ='" . $_GET["profile_id"] . "'";
	
	$ProfileID = true;
}
if (isset($_GET["search"]))
{
	if ($like <> " ") $like = $like . " AND";
	else $like = $like . "WHERE";
	$like = $like . " (plugin_Name LIKE '%" . $_GET["search"] . "%' or plugin_ID LIKE '%" . $_GET["search"] . "%' or plugin_ShortDescription LIKE '%" . $_GET["search"] . "%' or plugin_LongDescription LIKE '%" . $_GET["search"] . "%' or plugin_DocumentText LIKE '%" . $_GET["search"] . "%')";
}

if($like == " ")
	$like = " WHERE ";
else
	$like = $like . " and ";

$like = $like . "plugin_Current = '1'";

//$sf = sort filter (column to sort)  $so = sort order (
$orderBy = "$sf $so";

$table = "plugins";

$sql = "SELECT * FROM $table $like";
$totalResults = mysql_num_rows(mysql_query($sql)); //Get the total number of results


$sql = "SELECT * FROM $table $like ORDER BY $orderBy LIMIT $start, $nb";

//echo($sql);

$res = mysql_query($sql);

if (!$res) 
{	
	echo "No Records Found!<br/><br/>";
//	echo mysql_error() . "<br/>";
//	echo($sql) . "<br/>";
}

if ($res) {
	if ($Author) {
		echo "<h1>Plugins by " . $_GET["author"] . " </h1><br>";
		$c=@mysql_num_rows($res);
		echo $_GET["author"] . " has $c plugins.<br><hr/>"; }
	if ($ProfileID) {
		echo "<h1>Plugins by " . $_GET["profile_id"] . " </h1><br>";
		$c=@mysql_num_rows($res);
		echo $_GET["profile_id"] . " has $c plugins.<br><hr/>"; }
		
	$enregNb = 0;
	$profil_id_for_vote = Authenticate();


	//Show nav links and filters (added by robogeek)
	//show_navfilter_options($program, $ptype, $nb, $start, $totalResults, $back, $forw, $sort_filter, $sort_order);
	show_navfilter_options($breadcrumb2, $nb, $start, $totalResults, $back, $forw, $sort_filter, $sort_order);
	
	echo "<hr />\n";

	while ( $enreg=@mysql_fetch_array($res)) {

		$plugin_key = $enreg["plugin_key"];
		$plugin_id = $enreg["plugin_ID"];
		$plugin_name = $enreg["plugin_Name"];
		$plugin_author = $enreg["plugin_Author"];
		$plugin_ShortDesc = $enreg["plugin_ShortDescription"];
		$plugin_version = $enreg["plugin_Version"];
		$plugin_versionvalue = $enreg["plugin_VersionValue"];
		$plugin_state = $enreg["plugin_State"];
		//$plugin_DownloadCount = $enreg["plugin_DownloadCount"];  //deprecated, now uses userdownloads table for download count (robogeek)
		$plugin_Type =$enreg["plugin_Type"];
		$plugin_License =$enreg["plugin_Licence"];
		$plugin_Copyright = $enreg["plugin_Copyright"];
		$plugin_Date = $enreg["plugin_Date"];
		$profil_id = $enreg["profil_id"];
	
		$newest_plugin = GetMostRecentPluginObject($plugin_id);
		$newest_id = $newest_plugin->plugin_ID;
		$newest_versionvalue = $newest_plugin->plugin_VersionValue;
		$newest_version = $newest_plugin->plugin_Version;
		$newest_state = $newest_plugin->plugin_State;
		
		echo "<table border=0 cellpadding=0 cellspacing=0 width=100% ";
		echo "><tr><td align='center' valign='top' width=140>\n";
		echo "<a href='detail.php?plugin_id=$plugin_id$breadcrumb'>\n";
	
		$plugin_directory = "$plugin_home_directory/$plugin_id/$plugin_version/$plugin_state";
		
		echo "<img src='" . GetPluginIcon($plugin_id,$plugin_version) . "' border=1 >"; //width=100 height=100
	
	
		echo "</a>";
		
		//if user owns this plugin, show an edit link
		if (($u !== null) && ($u == $profil_id)) echo "<br><b>(<a href=edit.php?plugin_id=$plugin_id&plugin_version=$plugin_version&plugin_state=$plugin_state>Edit Plugin Details</a>)</b>";
		echo "</td><td>";
	
		echo " <a href='detail.php?plugin_id=$plugin_id$breadcrumb'><font size=+2>$plugin_name</font></a> v$plugin_version ($plugin_state)";
		echo " by <a href='".$_SERVER["PHP_SELF"]."?author=" . urlencode($plugin_author) . "'><i>$plugin_author</i></a><br />\n";
		echo "<b>Plugin type:</b> $plugin_Type<br />\n";
		echo "<b>License:</b> $plugin_License ";
		if (trim($plugin_Copyright) != "") echo "- $plugin_Copyright";
		echo "<br />\n";
		echo "<b>Plugin date:</b> $plugin_Date <br />";
		echo "<br />$plugin_ShortDesc<br /><br />\n";
		echo "<table border=0 cellpadding=0 cellspacing=0 width=40%><tr><td width=50%>";
		echo "<i>Downloads: " . GetDownloadsTotal($plugin_id) . "</i></td><td>";
		echo "<i>Popularity: " .GetPopularity($plugin_id) . "</i></td></tr>";
		echo "</table>";

		echo "<br />[ <a href=\"dl.php?plugin_id=$plugin_id\">Download Now</a> ] - [ <a href='detail.php?plugin_id=$plugin_id$breadcrumb'>Full Details</a> ]<br />";
		if ($plugin_versionvalue < $newest_versionvalue) echo "[ <a href=\"dl.php?plugin_id=$newest_id&plugin_version=$newest_version&plugin_state=$newest_state\">Download Latest $newest_state</a> ] - [ <a href='detail.php?plugin_id=$newest_id&plugin_version=$newest_version'>Full Details for Latest $newest_state</a> ]<br />";
		echo "<br />\n";
		VoteWorking($u, $plugin_id, $plugin_version, null, "thelist.php?$breadcrumb");
		
		echo "</td></tr><tr><td>&nbsp;</td></tr></table>\n";
		echo "<hr />";
		$enregNb++;
	}

	//Show nav links and filters (added by robogeek)
	//show_navfilter_options($program, $ptype, $nb, $start, $totalResults, $back, $forw, $sort_filter, $sort_order);
	show_navfilter_options($breadcrumb2, $nb, $start, $totalResults, $back, $forw, $sort_filter, $sort_order);
}
?>

</div>

<?php
include('footer.php');
?>