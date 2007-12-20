<?php
require_once('authentication.php');
require_once('functions.php');



$u = Authenticate();

global $db_host;

global $db_user;

global $db_pass;

global $db_database;

mysql_connect($db_host, $db_user, $db_pass) or die("Error connecting to mysql");

mysql_select_db($db_database) or die("Error connecting to database");



//confirm profil_id

if ($u == null || $u !== $_GET['profile_id']) die("are you scamming profil_id?");



//confirm plugin_id and plugin_version

$getPluginId = $_GET['plugin_id'];

$getPluginVersion = $_GET['plugin_version'];

$res = mysql_query("SELECT * FROM plugins WHERE plugin_ID='$getPluginId' AND plugin_Version='$getPluginVersion'");

$nb = mysql_numrows($res);

mysql_close();

if ($nb <> 1) die("are you scamming an old or non-existant plugin? $nb");



//confirm vote

$v = $_GET['vote'];

if ($v == null || $v == "") die("you forgot to vote?");

if ($v !== "true" && $v !== "false") die("invalid vote");



VoteWorking($_GET['profile_id'], $getPluginId, $getPluginVersion, $v, null);



?>

