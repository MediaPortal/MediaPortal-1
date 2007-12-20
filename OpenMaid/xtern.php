<?php

require_once('functions.php');
require_once('db.php');

//script will timeout after 30 minutes (1800 seconds)  
//keeps script alive if it has to regenerate cache files before sending them to the user
set_time_limit(1800);

if (isset($_GET["gen_cache"])) $gen_cache = TRUE;
else $gen_cache = FALSE;

if (isset($_GET["summary"])) {
xtern_summary($gen_cache);
}

if (isset($_GET["extended"])) {
xtern_extended($gen_cache);
}

if (isset($_GET["sync"])) {
xtern_sync($gen_cache);
}

if (isset($_GET["all"])) {
xtern_all($gen_cache);
}

// The following code is heavily modified to work with new directory structure (robogeek)
// If plugin is set, get data for most recent plugin
if (isset($_GET["plugin"]) && !isset($_GET["version"]) && !isset($_GET["state"])) {

	$plugin_id = formatPluginID($_GET["plugin"]);

	//First see if the requested plugin exists, die if it doesn't
	if (!DoesPluginExistSimple($plugin_id)) die ("Error, that plugin doesn't exist in the database!");

	//Get the plugin data for most recently uploaded plugin
	$plugin = GetPluginObject($plugin_id);

	//Use data to generate the plugin.xml file location
	$pluginxml_file = "plugins/" . $plugin_id . "/" . $plugin->plugin_Version . "/" . $plugin->plugin_State . "/plugin.xml";

	if (!file_exists($pluginxml_file)) $pluginxml_file = "plugins/" . $plugin_id . "/" . $plugin->plugin_Version . "/" . strtolower($plugin->plugin_State) . "/plugin.xml";
	if (!file_exists($pluginxml_file)) die("Sorry, I can't find that plugin...");

	//Send the plugin.xml file
	echo file_get_contents("$pluginxml_file");
}

// All new code to allow fetching plugin.xml for previous versions of a plugin (robogeek)
// Call with xtern.php?plugin=plugin_id&version=plugin_versiong&state=plugin_state
// Example xtern.php?plugin=7018318B-EFEB-45E7-84E7-1F6C49152209&version=0.5.9&state=beta
// If plugin, plugin version, and state is set, get data for specified version/state of the plugin
if (isset($_GET["plugin"]) && isset($_GET["version"]) && isset($_GET["state"])) {

	$plugin_id = formatPluginID($_GET["plugin"]);
	$plugin_version = $_GET["version"];
	$plugin_state = $_GET["state"];

	//First see if the requested plugin exists, die if it doesn't
	if (!DoesPluginExistSimple($plugin_id)) die ("Error, that plugin doesn't exist in the database!");

	//Get the plugin data for specified plugin
	$plugin = GetPreviousPluginObject($plugin_id, $plugin_version, $plugin_state, 1);

	//Use data to generate the plugin.xml file location
	$pluginxml_file = "plugins/" . $plugin_id . "/" . $plugin->plugin_Version . "/" . $plugin->plugin_State . "/plugin.xml";

	if (!file_exists($pluginxml_file)) $pluginxml_file = "plugins/" . $plugin_id . "/" . $plugin->plugin_Version . "/" . strtolower($plugin->plugin_State) . "/plugin.xml";
	if (!file_exists($pluginxml_file)) die("Sorry, I can't find that plugin...");

	//Send the plugin.xml file
	echo file_get_contents("$pluginxml_file");
}
?>