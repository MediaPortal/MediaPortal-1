<?php
require_once('functions.php');
require_once('authentication.php');
require_once('db.php');
require_once('process.functions.php');

include('header2.php');
set_time_limit(1800);
$u = Authenticate();
//set_time_limit(900); //uncomment this on servers that timeout the php scripts too soon.  900 = 900 seconds = 15 minutes
echo "<div id=\"menudiv\">";
echo "<b><a href=\"$sys_url\">OpenMAID</a> :: <a href=\"admin.php\">Admin Panel</a>";
if (isset($_GET["cache_mode"])) echo " :: <a href=\"process.php?mode=" . $_GET["cache_mode"] . "\">" . $_GET["cache_mode"] . "</a>";
echo "</b></div></td></tr><tr>";
echo "<td id=\"contentarea\">";
echo "<div id=\"widebar\">";

//modes: manage_new, manage_all, manage_failed, manage_passed, process_new, process_single, change_status, check_plugin
//parameters: ignore_version, filename
//cache_mode used internally to allow return links to previous page
//
//file extensions: .mpp (new), .pass (processing passed), .fail (processing failed), .ignore (ignored)


if (isset($_GET["mode"])) $mode = $_GET["mode"];
else $mode = "manage_all"; //default mode

if (isset($_GET["cache_mode"])) $cache_mode = $_GET["cache_mode"];
else $cache_mode = "manage_all"; //default cache_mode

if (isset($_GET["status"])) $status = $_GET["status"];
else $status = ""; //default status

if (isset($_GET["file"])) $file = $_GET["file"];
else $file = ""; //default file

if (isset($_GET["confirm"])) $confirm = $_GET["confirm"];
else $confirm = FALSE; //default confirm


if (!isset($u) || $u == "")
	//slowdie( 'You must <a href="' . GetLogonURL() . '">login to access administration panel</a>');

if (!IsAdmin($u))
	//slowdie( "Sorry, you are not authorised to use this module.");


if (isset($_GET["id"])) $id = $_GET["id"];
else $id = ""; //default plugin id value

if (isset($_GET["version"])) $version = $_GET["version"];
else $version = ""; //default plugin version value 

if (isset($_GET["state"])) $state = $_GET["state"];
else $state = ""; //default plugin state value

if (isset($_GET["ignore_version"])) $ignore_version = $_GET["ignore_version"];
else $ignore_version = TRUE; //default ignore_version value 0=enforce version check 1=ignore version check

if (isset($_GET["ignore_sanity"])) $ignore_sanity = $_GET["ignore_sanity"];
else $ignore_sanity = FALSE; //default ignore_sanity value 0=enforce sanity checks 1=ignore sanity checks

if (isset($_GET["force_update"])) $force_update = $_GET["force_update"];
else $force_update = TRUE; //default force_update value 0=don't force update if exists in plugin archive or db 1=update db and plugin archive overwriting old values/files

if (isset($_GET["start_at"])) $start_at = $_GET["start_at"];
else $start_at = 0; //default start_at value

if (isset($_GET["file"])) $file = $_GET["file"];


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Show mpps as list with links to move, rename, process
	//If processing fails for a mpp, rename it with .fail extension
	//If processing succeeds for a mpp, rename it with a .pass extension
	//parse id out of file name and see if it's already in the db, if it is then don't reprocess it

//modes: manage_new, manage_all, manage_failed, manage_passed, process_new, process_single, delete, change_status, check_plugin, 

if ($mode == "process_new") process_new($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode);

if ($mode == "process_single") process_single($file, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode);

if ($mode == "manage_all") manage_all($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode);

if ($mode == "manage_new") manage_new($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode);

if ($mode == "manage_failed") manage_failed($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode);

if ($mode == "manage_passed") manage_passed($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode);

if ($mode == "manage_ignored") manage_ignored($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode);

if ($mode == "manage_flagged") manage_flagged($status, $mode);

if ($mode == "manage_metadata") manage_metadata($status, $mode, "");

if ($mode == "manage_profileID") manage_profileID($status, $mode);

if ($mode == "manage_allplugins") manage_AllPlugins($ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode);

if ($mode == "change_flag") change_flag($file, $id, $version, $state, $status);

if ($mode == "delete") repository_file_delete($file, $confirm, $ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode);

if ($mode == "fulldelete") complete_plugin_delete($file, $id, $version, $state, $confirm, $ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode);

if ($mode == "change_status") change_status($file, $confirm, $ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode);


if ($mode == "check_plugin") {
	echo "Not implemented yet...<br>";
	echo "Click <a href='process.php?mode=$cache_mode'>here</a> to return to previous screen.";
	}

include('footer2.php');
?>
