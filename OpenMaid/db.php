<?php
////////////////////////////////////////////////////////////////////////////////
//This should contain all of the Database calls used in the OpenMAID Web
//Application.Please refrain from adding database calls from any of the other
//code files. 
////////////////////////////////////////////////////////////////////////////////

//config.php has all of the database connection info
require_once('config.php');


////////////////////////////////////////////////////////////////////////////////
//Call this function to ensure that there is an open connection to the database
function ConnectOnce()
{
        global $connected;

        global $db_host;
        global $db_user;
        global $db_pass;
        global $db_database;
        
        if ($connected == false)
        {
//              echo "<br>[<b>Host: </b>$db_host<b>User: </b>$db_user<b>Password: </b>$db_pass<b>Database: </b>$db_database]<br>";
                if (mysql_connect($db_host, $db_user, $db_pass))
                {
                        if (mysql_select_db($db_database) )
                        {
                                $connected == true;
                        }
                        else
                        {
                                die("Error connecting to database");
                        }
                }
                else
                {
                        die("Error connecting to mysql");
                } 
        }
}

function Disconnect()
{
        //Place any other closing code here
        mysql_close();
}

function reportDBInfo()
{
        global $db_host;
        global $db_user;
        global $db_database;    
        
        echo "<b>Database Name:</b> $db_database <br />";
        echo "<b>Database User:</b> $db_user <br />";
        echo "<b>Database Host:</b> $db_host <br />";
        echo "<b>Database Password:</b> ********* <br />";
}

//Function Returns True if the Plugin removed, False if failed
function delFromDB($ID, $version, $state, $all)
{       
        ConnectOnce();

        //remove plugin from plugins table (id, version, state)
        if (!$all) {
                $sql = "DELETE FROM plugins WHERE plugin_ID = '$ID' and plugin_Version = '$version' and plugin_State = '$state'";
                mysql_query($sql);
        
                //remove data from images table (id, version)
                $sql = "DELETE FROM images WHERE plugin_ID = '$ID' and plugin_Version = '$version'";
                mysql_query($sql);
        
                //remove data from popularites table (id, version)
                $sql = "DELETE FROM popularites WHERE plugin_ID = '$ID' and plugin_Version = '$version'";
                mysql_query($sql);
        
                //remove data from userdownloads table (id, version)
                $sql = "DELETE FROM userdownloads WHERE plugin_ID = '$ID' and plugin_Version = '$version'";
                mysql_query($sql);
        
                //remove data from votesWorking table (id, version)
                $sql = "DELETE FROM votesWorking WHERE plugin_ID = '$ID' and plugin_Version = '$version'";
                mysql_query($sql);
        }

        //remove all plugin versions from plugins table for specified id(id)
        if ($all) {
                $sql = "DELETE FROM plugins WHERE plugin_ID = '$ID'";
                mysql_query($sql);
        
                //remove data from images table (id, version)
                $sql = "DELETE FROM images WHERE plugin_ID = '$ID'";
                mysql_query($sql);
        
                //remove data from popularites table (id, version)
                $sql = "DELETE FROM popularites WHERE plugin_ID = '$ID'";
                mysql_query($sql);
        
                //remove data from userdownloads table (id, version)
                $sql = "DELETE FROM userdownloads WHERE plugin_ID = '$ID'";
                mysql_query($sql);
        
                //remove data from votesWorking table (id, version)
                $sql = "DELETE FROM votesWorking WHERE plugin_ID = '$ID'";
                mysql_query($sql);
        }
        UpdateCurrentPlugin($ID);
        return TRUE;
}

//Function Returns True if the Plugin exists, False if it does not
function DoesPluginExist($ID, $version, $state)
{       
        $key = GetPluginKey($ID, $version, $state);
        if ($key == -1 )
                return false;
        else
                return true;
}

//Function Returns True if the Plugin ID exists, False if it does not (robogeek)
function DoesPluginExistSimple($ID)
{       
        ConnectOnce();
                
        $sql = "SELECT plugin_key FROM plugins WHERE plugin_id = '$ID' and plugin_ReviewFlag = 'FALSE' and plugin_Current = true";
        $result = QuerySingleValue($sql);
        if ($result == -1 )
                return false;
        else
                return true;
}

//Function Returns array of all plugins in db
function getAllPlugins()
{       
        ConnectOnce();
                
        $sql = "SELECT plugin_ID, plugin_Name, plugin_Version, plugin_State, plugin_Type, plugin_Author, plugin_Date, profil_id, plugin_Current, plugin_ReviewFlag, plugin_SupportLink FROM plugins ORDER BY plugin_Name ASC, plugin_VersionValue DESC";
        $res = mysql_query($sql);
        
        if ($res == -1 )
                return false;
        else
                return $res;
}

//Function Returns array of db records that have profil_id field set to $profil_id
function findUsersPlugins($profil_id)
{       
        ConnectOnce();
                
        $sql = "SELECT plugin_ID, plugin_Name, plugin_Version, plugin_VersionValue, plugin_State, plugin_Author, plugin_Date FROM plugins WHERE profil_id = '$profil_id' ORDER BY plugin_Name ASC, plugin_VersionValue DESC";
        $res = mysql_query($sql);
        
        if ($res == -1 )
                return false;
        else
                return $res;
}

//Function Returns array of db records that have blank plugin_profil_id fields (robogeek)
function findMissingProfileID()
{       
        ConnectOnce();
                
        $sql = "SELECT plugin_ID, plugin_Name, plugin_Version, plugin_State, plugin_Author, plugin_Date FROM plugins WHERE profil_id = '' ORDER BY plugin_Author ASC";
        $res = mysql_query($sql);
        
        if ($res == -1 )
                return false;
        else
                return $res;
}

//Function Returns array of db records that have plugin_ReviewFlag set to TRUE (robogeek)
function findReviewFlag()
{       
        ConnectOnce();
        
        $sql = "SELECT plugin_ID, plugin_Name, plugin_Version, plugin_State, plugin_Date, plugin_ReviewFlag FROM plugins WHERE plugin_ReviewFlag = 'TRUE'";
        $res = mysql_query($sql);

        if ($res == -1 )
                return false;
        else
                return $res;
}

//Function Returns array of db records that have plugin_ReviewFlag set to TRUE (robogeek)
function changeReviewFlag($ID, $version, $state, $newflag)
{       
        ConnectOnce();
        
        $sql = "UPDATE plugins SET plugin_ReviewFlag = '$newflag' WHERE plugin_ID = '$ID' and plugin_Version = '$version' and plugin_State = '$state'";
        $res = mysql_query($sql);
        
        if ($res == -1 )
                return false;
        else {
                //reset current plugin status after successfully approving a flagged plugin
                UpdateCurrentPlugin($ID);
                return $res;
                }
}

//Function Returns array of db records that have key meta-data fields blank (robogeek)
function findMissingMetadata($profil_id)
{       
        ConnectOnce();
        
        if ($profil_id) $sql = "SELECT plugin_ID, plugin_Name, plugin_Version, plugin_State, plugin_Date FROM plugins WHERE plugin_SupportLink = '' AND profil_id = '$profil_id' ORDER BY plugin_Date DESC";
        else $sql = "SELECT plugin_ID, plugin_Name, plugin_Version, plugin_State, plugin_Date FROM plugins WHERE plugin_SupportLink = '' ORDER BY plugin_Date DESC";
        $res = mysql_query($sql);

        if ($res == -1 )
                return false;
        else
                return $res;
}

//Get the key of an existing plugin
function GetPluginKey($ID, $version, $state)
{
        ConnectOnce();
                
        $sql = "SELECT plugin_key FROM plugins WHERE plugin_id = '$ID' and plugin_version = '$version' and plugin_state = '$state'";
        $result = QuerySingleValue($sql);
        
        return $result;
}


//Function Executes a query and returns a single value
//This function should NOT be called directly from outside of this file
//if no values are found, the default value is returned
//if the value is found, it is returned
function QuerySingleValue($sql, $default = -1)
{
        $res = mysql_query($sql);
        $row = mysql_fetch_array($res);
        
        if ($row == "")
        {
                return $default;
        }
        else
        {
                if (is_null($row[0]))
                {
                        return $default;
                }
                else
                {
                        return $row[0];
                }
        }
}

//Associates an image and thumbnail on the server with
//an existing plugin.
function AssociateImage($plugin_id, $plugin_version, $imagepath, $thumbpath)
{
        ConnectOnce();
        
        $sql = "INSERT INTO images (plugin_ID, plugin_Version, image_path, image_thumb_path) VALUES ('$plugin_id', '$plugin_version', '$imagepath', '$thumbpath') ";
        
        if (mysql_query($sql))
                return true;
        else
                return false;
}

//This one gets version value (plugin_VersionValue)
function GetLatestVersionForState($pluginID, $state)
{
        ConnectOnce();
        if ($state == "ALL") $sql = "SELECT MAX(plugin_VersionValue) FROM plugins WHERE plugin_ID = '$pluginID'";
        else $sql = "SELECT MAX(plugin_VersionValue) FROM plugins WHERE plugin_ID = '$pluginID' and plugin_State = '$state' and plugin_ReviewFlag = 'FALSE'";
        return QuerySingleValue($sql);
}
//This one gets version string (plugin_Version)
function GetLatestVersionForState2($pluginID, $state)
{
        ConnectOnce();
        if ($state == "ALL") $sql = "SELECT plugin_Version FROM plugins WHERE plugin_ID = '$pluginID' ORDER BY plugin_VersionValue DESC LIMIT 1";
        else $sql = "SELECT plugin_Version FROM plugins WHERE plugin_ID = '$pluginID' and plugin_State = '$state' and plugin_ReviewFlag = 'FALSE' ORDER BY plugin_VersionValue DESC LIMIT 1";
        return QuerySingleValue($sql);
}

//for this PluginID, mark all versions and states with the current value of false
//find the greatest version of a stable release. Mark it as the current version
//if there are no stable versions, then find the greatest version of a  beta release and mark it as current
//if there are no beta versins, then find the greatest alpha version  and mark i as current  
function UpdateCurrentPlugin($pluginID)
{
        ConnectOnce();

        //reset all versions and states with the current value of false
        $sql = "UPDATE plugins SET plugin_Current = false WHERE plugin_ID = '$pluginID' and plugin_ReviewFlag = 'FALSE'";
        mysql_query($sql);
        
        //find the latest version of a stable release
        $state = "Stable";
        $version = GetLatestVersionForState($pluginID,$state);
        
        if ($version == -1)
        {
                //find the latest beta release
                $state = "Beta";
                $version = GetLatestVersionForState($pluginID,$state);
                
                if ($version == -1)
                {
                        //find the latest Alpha release
                        $state = "Alpha";
                        $version = GetLatestVersionForState($pluginID,$state);
                        
                        if ($version == -1)
                        {
                                echo("Error finding current version of plugin");
                        }
                }
        }
        
        //update the current version
        $sql = "UPDATE plugins SET plugin_Current = true WHERE plugin_ID = '$pluginID' and plugin_State = '$state' and plugin_VersionValue = $version";
        mysql_query($sql);
        
        
}

//Logs the creation of a table
function LogCreateTable($name,$query)
{
        echo("Creating $name Table.....");

        $queryResult = mysql_query($query);
        
        if ($queryResult)
                echo("Success!");
        else
                echo("Failed!");
                
                
        echo ("<br />");
        return;
}


//GetPluginScreenshots used for fetching plugin screenshots
function GetPluginScreenshots($plugin_id, $plugin_version)
{
        ConnectOnce();

        $sql = "SELECT * FROM images WHERE plugin_ID = '$plugin_id' and plugin_Version = '$plugin_version'";
        $res = mysql_query($sql);
        
        return $res;    
}


//GetPluginHistory used for fetching data for current and previous versions of a plugin
function GetPluginHistory($plugin_id)
{
        ConnectOnce();

        $sql = "SELECT plugin_Name, plugin_Version, plugin_State, plugin_Date, plugin_Licence, plugin_DownloadSize FROM plugins WHERE plugin_ID = '$plugin_id'  and plugin_ReviewFlag = 'FALSE' ORDER BY plugin_VersionValue DESC";
        $res = mysql_query($sql);
        
        return $res;    
}


//Created new GetPluginObject used for downloading previous plugin versions
//Added some parameters and to make it compatible with fetching db info for previous versions for download
//Added plugin_version string, plugin_state string, and get_previous_plugin flag.
function GetPreviousPluginObject($plugin_id, $plugin_version, $plugin_state, $get_previous_plugin)
{
        ConnectOnce();

        if (!$get_previous_plugin) {
                $sql = "SELECT * FROM plugins WHERE plugin_ID = '$plugin_id' and plugin_ReviewFlag = 'FALSE' and plugin_Current = true";
                $res = mysql_query($sql);
                return mysql_fetch_object($res);
                }
                
        //The following sql query will get data for a previous version of a plugin based on the version and state passed to the function
        else {
                $sql = "SELECT * FROM plugins WHERE plugin_ID = '$plugin_id' and plugin_Version = '$plugin_version' and plugin_State = '$plugin_state'";
                $res = mysql_query($sql);
                return mysql_fetch_object($res);
                }
}


//Retrieves an Object containing all of the information of a plugin (based on ID)
//Only the most current stable plugin will be returned
function GetPluginObject($pluginID)
{
        ConnectOnce();
        
                $sql = "SELECT * FROM plugins WHERE plugin_ID = '$pluginID' and plugin_ReviewFlag = 'FALSE' and plugin_Current = true";
                $res = mysql_query($sql);
                return mysql_fetch_object($res);
}


//Retrieves an Object containing partial information of a plugin (based on ID)
//Only the most recently uploaded plugin wil be returned
function GetMostRecentPluginObject($pluginID)
{
        ConnectOnce();
        
                $sql = "SELECT plugin_ID, plugin_Name, plugin_State, plugin_Version, plugin_VersionValue, max(plugin_Date) FROM plugins WHERE plugin_ID = '$pluginID' and plugin_ReviewFlag = 'FALSE' GROUP BY plugin_ID";
                $res = mysql_query($sql);
                return mysql_fetch_object($res);
}

//Checks plugin id to be sure it is unique and not conflicting with another plugin
//Ties GUID to destination folder and plugin type
//If check fails, return name of conflicting plugin. If it passes, return 'passed'
function CheckForUniquePluginID($pluginID, $pluginType, $DestinationFolder)
{
        ConnectOnce();

                $sql = "SELECT plugin_ID, plugin_Type, plugin_DestinationFolder, plugin_Name FROM plugins WHERE plugin_ID = '$pluginID'";
                $res = mysql_query($sql);
                while (($enreg=@mysql_fetch_array($res))) {
                        if ( (addslashes($enreg["plugin_DestinationFolder"]) != $DestinationFolder) && (strtolower($enreg["plugin_Type"]) != strtolower($pluginType)) ) {
                                echo "<br><br>";
                                if (addslashes($enreg["plugin_DestinationFolder"]) != $DestinationFolder) echo "Destination conflict: " . addslashes($enreg["plugin_DestinationFolder"]) . " != $DestinationFolder <br>";
                                if ($enreg["plugin_Type"] != $pluginType) echo "Plugin Type conflict: " . $enreg["plugin_Type"] . " != $pluginType <br>";
                                echo "<br>";
                                return $enreg["plugin_Name"];
                                }
                        }
                return "passed";
}


//Checks plugin destination folder to be sure it is unique so it doesn't clash with another plugin
//If check fails, return name of conflicting plugin. If it passes, return 'passed'
function CheckForDupeDestinationFolder($pluginID, $pluginType, $DestinationFolder)
{
        ConnectOnce();
        
                $sql = "SELECT DISTINCT plugin_DestinationFolder, plugin_Name FROM plugins WHERE plugin_ID != '$pluginID' and plugin_Type = '$pluginType'";
                $res = mysql_query($sql);
                while (($enreg=@mysql_fetch_array($res))) {
                        if (addslashes($enreg["plugin_DestinationFolder"]) == $DestinationFolder) return $enreg["plugin_Name"];
                        }
                return "passed";
}


//Checks plugin destination folder to be sure it is not changed from previous version
//If check fails, return name of previous destination folder. If it passes, return 'passed'
function CheckForMultipleDestinationFolders($pluginID, $DestinationFolder)
{
        ConnectOnce();
        
                $sql = "SELECT plugin_DestinationFolder FROM plugins WHERE plugin_ID = '$pluginID'";
                $res = mysql_query($sql);
                while (($enreg=@mysql_fetch_array($res))) {
                        if (addslashes($enreg["plugin_DestinationFolder"]) != $DestinationFolder) return addslashes($enreg["plugin_DestinationFolder"]);
                        }
                return "passed";
}


//Checks plugin author to be sure user isn't trying to upload someone else's plugin
//If check fails, return name of plugin author profil_id of last uploaded version. If it passes, return 'passed'
function CheckProfileIDUpload($pluginID, $pluginAuthor, $uploaderProfileID)
{
        ConnectOnce();
        
                $sql = "SELECT profil_id, plugin_Author FROM plugins WHERE plugin_ID = '$pluginID' ORDER BY plugin_VersionValue DESC LIMIT 1";
                $res = mysql_query($sql);

                while ($enreg=@mysql_fetch_array($res)) {
                        if ($enreg["profil_id"] == "") $previous_plugin_profile_id = "";
                        else $previous_plugin_profile_id = $enreg["profil_id"];
                        if ($enreg["plugin_Author"] == "") $previous_plugin_author = "";
                        else $previous_plugin_author = $enreg["plugin_Author"];
                        
                        if (($uploaderProfileID != "") && ($previous_plugin_profile_id != "")) {
                                if ( ($uploaderProfileID == $previous_plugin_profile_id) && ($previous_plugin_author == $pluginAuthor) ) return "passed:::OK";
                                if ( ($uploaderProfileID == $previous_plugin_profile_id) && ($previous_plugin_author != $pluginAuthor) ) return "passed:::Uploaded as author, but author name changed";
                                if ( ($uploaderProfileID != $previous_plugin_profile_id) && ($previous_plugin_author == $pluginAuthor) ) return "flagged:::Uploaded as author, but profile ID changed";
                                if ( ($uploaderProfileID != $previous_plugin_profile_id) && ($previous_plugin_author != $pluginAuthor) ) return "flagged:::Uploaded as author, but profile ID and the author tag changed";
                                else return "unknown:::Unknown Error (1)! <br> 1 - $pluginID <br> 2 - $uploaderProfileID <br> 3 - $previous_plugin_profile_id <br> 4 - $pluginAuthor <br> 5 - $previous_plugin_author <br>";
                                }
                        if (($uploaderProfileID != "") && ($previous_plugin_profile_id == "")){
                                if ( ($uploaderProfileID != $previous_plugin_profile_id) && ($previous_plugin_author == $pluginAuthor) ) return "flagged:::Uploaded as author, but prior version has blank profile ID";
                                if ( ($uploaderProfileID != $previous_plugin_profile_id) && ($previous_plugin_author != $pluginAuthor) ) return "flagged:::Uploaded as author, but prior version has blank profile ID and the author tag changed";
                                else return "unknown:::Unknown Error (2)! <br> 1 - $pluginID <br> 2 - $uploaderProfileID <br> 3 - $previous_plugin_profile_id <br> 4 - $pluginAuthor <br> 5 - $previous_plugin_author <br>";
                                }
                        if (($uploaderProfileID == "") && ($previous_plugin_profile_id != "")){
                                if ( ($uploaderProfileID != $previous_plugin_profile_id) && ($previous_plugin_author == $pluginAuthor) ) return "flagged:::Uploaded not as author, otherwise seems OK";
                                if ( ($uploaderProfileID != $previous_plugin_profile_id) && ($previous_plugin_author != $pluginAuthor) ) return "flagged:::Uploaded not as author but the author tag changed";
                                else return "unknown:::Unknown Error (3)! <br> 1 - $pluginID <br> 2 - $uploaderProfileID <br> 3 - $previous_plugin_profile_id <br> 4 - $pluginAuthor <br> 5 - $previous_plugin_author <br>";
                                }
                        if (($uploaderProfileID == "") && ($previous_plugin_profile_id == "")){
                                if ( ($uploaderProfileID == $previous_plugin_profile_id) && ($previous_plugin_author == $pluginAuthor) ) return "flagged:::Uploaded not as author, prior blank profile ID";
                                if ( ($uploaderProfileID == $previous_plugin_profile_id) && ($previous_plugin_author != $pluginAuthor) ) return "flagged:::Uploaded not as author and the author tag changed";
                                else return "unknown:::Unknown Error (4)! <br> 1 - $pluginID <br> 2 - $uploaderProfileID <br> 3 - $previous_plugin_profile_id <br> 4 - $pluginAuthor <br> 5 - $previous_plugin_author <br>";
                                }
                        }
                if ($uploaderProfileID == "") return "flagged:::Uploaded not as author, no prior version, but profile ID is blank";     
                return "passed:::No previous plugins to compare with";
}


//Checks plugin type to be sure user isn't trying to change plugin type
//If check fails, return plugin type of last uploaded version. If it passes, return 'passed'
function CheckPluginTypeUpload($pluginID, $pluginType)
{
        ConnectOnce();
        
                $sql = "SELECT plugin_Type FROM plugins WHERE plugin_ID = '$pluginID' ORDER BY plugin_VersionValue DESC LIMIT 1";
                $res = mysql_query($sql);
                while ($enreg=@mysql_fetch_array($res)) {
                        if ( strtolower($pluginType) != strtolower($enreg["plugin_Type"]) ) return strtolower($enreg["plugin_Type"]);
                }
                return "passed";
}


//Increments the number of plugin downloads
//if user has already downloaded this plugin, it is ignored
function IncrementDownloadCount($pluginID, $pluginVersion, $user)
{
        ConnectOnce();
        
        if ($user == "") $user = "Guest";
        
        $sql = "SELECT download_key FROM userdownloads WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion' and download_profil_id ='$user'";
        
        echo mysql_error();
        
        $result = QuerySingleValue($sql);

        //if record already exists, do nothing
        if ($result == -1 || $user == "Guest")
        {
                $sql = "INSERT INTO userdownloads (plugin_ID, plugin_Version, download_Date, download_profil_id) VALUES ('$pluginID','$pluginVersion',NOW(),'$user')";
                mysql_query($sql);
                echo mysql_error();
                
                //get aggregate count for this version/state of the plugin
                $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion'";       
                $thisversion_downloads = QuerySingleValue($sql, 0);
                //get aggregate count for all versions of this plugin
                $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID'";     
                $total_downloads = QuerySingleValue($sql, 0);
                //put aggregate counts into plugins table for easy sorting in thelist.php
                $sql = "UPDATE plugins SET plugin_DownloadCount = '$thisversion_downloads', plugin_DownloadCountTotal = '$total_downloads' WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion'";
                mysql_query($sql);
                $sql = "UPDATE plugins SET plugin_DownloadCountTotal = '$total_downloads' WHERE plugin_ID = '$pluginID'";
                mysql_query($sql);
                echo mysql_error();
        }
        
}

//Retrieves the total downloads for a plugin id for the day. 
function GetDownloadsToday($pluginID)
{
        ConnectOnce();
        $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID' and download_Date = NOW()";   
        return QuerySingleValue($sql, 0);
}

//Retrieves the total downloads for a plugin id for the past 30 days.
function GetDownloadsMonth($pluginID)
{
        ConnectOnce();
        $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID' and download_Date > DATE_SUB(CURDATE(),INTERVAL 30 DAY)";     
        return QuerySingleValue($sql, 0);       
}

//Retrieves total number of downloads for all versions of a plugin.
function GetDownloadsTotal($pluginID)
{
        ConnectOnce();
        $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID'";     
        return QuerySingleValue($sql, 0);       
}

//Retrieves total number of downloads for a specific version of a plugin.
function GetDownloadsTotalThisVersion($pluginID,$pluginVersion)
{
        ConnectOnce();
        //$sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion'";     
        $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion'";       
        return QuerySingleValue($sql, 0);       
}

//Retrieves the total downloads for a specific version of a plugin for the day.
function GetDownloadsTodayThisVersion($pluginID,$pluginVersion)
{
        ConnectOnce();
        $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion' and download_Date = NOW()";     
        return QuerySingleValue($sql, 0);
}

//Retrieves the total downloads for a specific version of a plugin for the past 30 days.
function GetDownloadsMonthThisVersion($pluginID,$pluginVersion)
{
        ConnectOnce();
        $sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion' and download_Date > DATE_SUB(CURDATE(),INTERVAL 30 DAY)";       
        return QuerySingleValue($sql, 0);       
}

//returns the thumbnail of a particular Plugin's icon
function GetPluginIcon($pluginID,$pluginVersion)
{
        ConnectOnce();
        $sql = "SELECT image_thumb_path FROM images WHERE plugin_ID = '$pluginID' and plugin_Version = '$pluginVersion' ORDER BY image_key ASC";
        return QuerySingleValue($sql, "images/missing.gif");
}


//function to add column to db table if it doesn't exist
function add_column($table_name, $column_name, $column_config) {

    $show=mysql_query("SHOW COLUMNS FROM $table_name");
    $columns=array();
    while( $fetch = mysql_fetch_array($show ) ) {
        $columns[$fetch['Field']] = $fetch;
            }
        foreach ($columns as $column => $val ) {
                //echo("column ($column) == column_name ($column_name)<br />");
                if ($column == $column_name) return true;
                }
                
        //didn't find it try to create it.
        $sql = "ALTER TABLE $table_name ADD COLUMN `$column_name` $column_config";
        $res = mysql_query($sql);
                
        // we cannot directly tell that whether this succeeded so check again!
    $show=mysql_query("SHOW COLUMNS FROM $table_name");
    $columns=array();
    while( $fetch = mysql_fetch_array($show ) ) {
        $columns[$fetch['Field']] = $fetch;
            }
        foreach ($columns as $column => $val ) {
                //echo("column ($column) == column_name ($column_name)<br />");
                if ($column == $column_name) return true;
                }
        return false;
}


//Creates all the tables needed for OpenMaid to work properly
function InitializeDatabase()
{
        ConnectOnce();
        
        echo "Initialization of Database Begin!<br />" ;
        
        LogCreateTable("Plugins","CREATE TABLE IF NOT EXISTS `plugins` (
                                          `plugin_key` int(11) NOT NULL auto_increment,
                                          `plugin_ID` varchar(38) collate latin1_general_ci NOT NULL default '',
                                          `plugin_Name` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_ShortDescription` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_LongDescription` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_Type` enum('None','General','Import','Input','Module','Theme','Web','Wizard','Extension','Sub','Hack','Misc','Icon') collate latin1_general_ci NOT NULL default 'None',
                                          `plugin_ModuleType` enum('None','Sub','Hack','Misc','Icon') collate latin1_general_ci NOT NULL default 'None',
                                          `plugin_Licence` enum('Commercial','Donationware','Freeware','Shareware') collate latin1_general_ci NOT NULL default 'Commercial',
                                          `plugin_State` enum('Alpha','Beta','Stable') collate latin1_general_ci NOT NULL default 'Alpha',
                                          `plugin_Date` datetime NOT NULL default '0000-00-00 00:00:00',
                                          `plugin_Version` varchar(15) collate latin1_general_ci NOT NULL,
                                          `plugin_Author` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_AuthorEmail` varchar(255) collate latin1_general_ci NOT NULL,
                                          `profil_id` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_Copyright` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_SupportLink` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_DownloadLink` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_DownloadSize` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_DonationLink` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_SourceLink` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_DocumentationFile` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_VersionValue` bigint(21) NOT NULL,
                                          `plugin_Current` tinyint(1) NOT NULL,
                                          `plugin_DocumentText` text collate latin1_general_ci NOT NULL,
                                          `plugin_DestinationFolder` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_Site` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_DirectDownloadURL` varchar(255) collate latin1_general_ci NOT NULL,
                                          `plugin_MinRequiredVersion` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_MaxRequiredVersion` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_IsDotNET` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_IsDotNETSimple` tinytext collate latin1_general_ci NOT NULL,
                                          `plugin_DownloadCount` int collate latin1_general_ci NOT NULL,
                                          `plugin_DownloadCountTotal` int collate latin1_general_ci NOT NULL,
                                          `plugin_VoteCount` int collate latin1_general_ci NOT NULL,
                                          `plugin_VoteCountTotal` int collate latin1_general_ci NOT NULL,
                                          `plugin_PopularityCount` int collate latin1_general_ci NOT NULL,
                                          `plugin_PopularityCountTotal` int collate latin1_general_ci NOT NULL,
                                          `plugin_ReviewFlag` enum('TRUE','FALSE') collate latin1_general_ci NOT NULL default 'FALSE',
                                          `plugin_hash` varchar(32) collate latin1_general_ci NOT NULL,
                                          PRIMARY KEY  (`plugin_key`),
                                          UNIQUE KEY `plugin_ID` (`plugin_ID`,`plugin_State`,`plugin_Version`)
                                        ) ENGINE=MyISAM AUTO_INCREMENT=4 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;");
                                        
        LogCreateTable("Images","CREATE TABLE IF NOT EXISTS `images` (
                                          `image_key` int(11) NOT NULL auto_increment,
                                          `plugin_ID` varchar(38) NOT NULL,
                                          `plugin_Version` varchar(15) NOT NULL,
                                          `image_path` varchar(255) collate latin1_general_ci NOT NULL,
                                          `image_thumb_path` varchar(255) collate latin1_general_ci NOT NULL,
                                          PRIMARY KEY  (`image_key`)
                                        ) ENGINE=MyISAM AUTO_INCREMENT=19 DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;");
                                        
        LogCreateTable("Dependencies","CREATE TABLE IF NOT EXISTS `dependency` (
                                          `plugin_ID` varchar(38) collate latin1_general_ci NOT NULL,
                                          `plugin_Version` varchar(15) collate latin1_general_ci NOT NULL,
                                          `requires_ID` varchar(38) collate latin1_general_ci NOT NULL,
                                          `requires_Version` varchar(15) collate latin1_general_ci NOT NULL
                                        ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;");
                                        
        LogCreateTable("User Downloads","CREATE TABLE IF NOT EXISTS `userdownloads` (
                                          `download_key` int(11) NOT NULL auto_increment,
                                          `plugin_ID` varchar(38) collate latin1_general_ci NOT NULL,
                                          `plugin_Version` varchar(15) collate latin1_general_ci NOT NULL,
                                          `download_Date` date NOT NULL,
                                          `download_profil_id` tinytext collate latin1_general_ci NOT NULL,
                                          PRIMARY KEY  (`download_key`)
                                        ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;");
                                        
        LogCreateTable("Votes Working","CREATE TABLE IF NOT EXISTS `votesWorking` (
                                          `plugin_ID` varchar(38) collate latin1_general_ci NOT NULL,
                                          `plugin_Version` varchar(15) collate latin1_general_ci NOT NULL,
                                          `profil_id` tinytext collate latin1_general_ci NOT NULL,
                                          `vote` enum('true','false') collate latin1_general_ci NOT NULL
                                        ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;");
        
        LogCreateTable("Popularity Contest","CREATE TABLE IF NOT EXISTS `popularities` (
                                        `plugin_ID` varchar(38) NOT NULL, 
                                        `plugin_Version` varchar(15) NOT NULL, 
                                        `Date` date NOT NULL, 
                                        `profil_id` tinytext NOT NULL, 
                                        `machine` text NOT NULL
                                        ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_ci;");
                                        
        //LogCreateTable("","");
        //LogCreateTable("","");
        
        Disconnect();
        
        echo "Database Initialization Finished!<br />" ;
        
}


function InsertPlugin()
{
echo "<b>InsertPlugin Function NOT yet implemented!!!</br><br>";
        return;
}
?>
