<?php
require_once('functions.php');
require_once('authentication.php');
require_once('db.php');
set_time_limit(3600);

$u = Authenticate();

$plugin_id = "";
// removed curly braces checking dukus

//if (isset($_GET["plugin_id"])) $plugin_id = formatPluginID($_GET["plugin_id"]);
//else { if (isset($_GET["plugin"])) $plugin_id = formatPluginID($_GET["plugin"]); }

//check plugin_id for curly braces.  if none, add them.
$plugin_id = $_GET["plugin_id"];
//Added for downloading previous versions (robogeek)
$plugin_version = "";
$plugin_state = "";
$previous_verison_check = 0;
if (isset($_GET["plugin_version"])) {$plugin_version = $_GET["plugin_version"]; $previous_version_check++;}
if (isset($_GET["plugin_state"])) {$plugin_state = $_GET["plugin_state"]; $previous_version_check++;}
if ($previous_version_check == 1) die ("Error: You can't do that! If you think you should be able to do that, please report this error.");
if ($previous_version_check != 2) {
        $previous_version_check = 0;
        $plugin_version = "";
        $plugin_state = "";
        }

if ($plugin_id =="")
{
        //TODO Try to get a Key!        
}
else
{
        if (!eregi("[A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12}", $plugin_id) || !eregi("[A-Z0-9]{8}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{12}", $plugin_id)) die("nice hack job, not :P");
}

//Changed to use GetPreviousPluginObject to make it compatible with fetching previous version data (robogeek)

$plugin = GetPreviousPluginObject($plugin_id, $plugin_version, $plugin_state, $previous_version_check);

$filename = $plugin_home_directory . "/" . $plugin->plugin_ID . "/" . $plugin->plugin_Version . "/" . $plugin->plugin_State . "/this.mpp";

//For plugins with lowercase state directory. (robogeek)
if (!is_file($filename)) {
        $filename = $plugin_home_directory . "/" . $plugin->plugin_ID . "/" . $plugin->plugin_Version . "/" . strtolower($plugin->plugin_State) . "/this.mpp";
        }
if (!is_file($filename)) die ("Error: Can't find plugin file!");


$dl_filename = $plugin->plugin_Name . "_" . $plugin->plugin_Version . "_" . $plugin->plugin_State ;

//Clean Up the file name for any bad characters
$dl_filename = str_replace(" ", "_", $dl_filename);
$dl_size = filesize($filename);

//Workaround for MSIE download bug where [] are added to downloads with mutliple periods in them
if (strstr($_SERVER['HTTP_USER_AGENT'], "MSIE"))
        //$dl_filename = preg_replace('/\./', '%2e', $dl_filename, substr_count($dl_filename, '.'));
        $dl_filename = str_replace('.', '%2e', $dl_filename);

header("Cache-Control: public, must-revalidate");
header("Content-Type: application/octet-stream");
header("Content-Length: ".$dl_size);
//header("Content-Disposition: attachment; filename=".$dl_filename.".mpp;");
header('Content-Disposition: attachment; filename="' . $dl_filename . '.mpi"');
header("Content-Transfer-Encoding: binary");
session_write_close();
ob_flush();flush();

//getting error calling virtual() (robogeek)
//virtual("$filename");


if (!$fp = @fopen($filename, 'rb')){
        die("Cannot Open File!  Please report this error to the OpenMAID admin!<br>\n");
        } 
else {
        sleep(2);
        if ($dl_size > 69000000) {
                while(!feof($fp)) {
                        $buffer = fread($fp, 32 * 1024);
                        print $buffer;
                        ob_flush(); // flush();
                        } 
                }
        else {
                fpassthru($fp);
                }
        @fclose($fp);
        }


IncrementDownloadCount($plugin->plugin_ID,$plugin->plugin_Version,$u);


?>
