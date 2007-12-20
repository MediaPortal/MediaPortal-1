<?php

require_once('pclzip.lib.php');
require_once('image_functions.php');
require_once('db.php');

global $XMLEntities;
$XMLEntities = array(
    '&amp;'  => '&',
    '&lt;'   => '<',
    '&gt;'   => '>',
    '&apos;' => '\'',
    '&quot;' => '"',
);



//Used to exit the system in case of an error
function slowDie($msg)
{
        echo "<b>$msg</b></div><div id=\"rightbar\"></div>";
        include('footer.php');
        die();
}

//takes in a string version and returns
//a numerical value for the version
function CalculateVersionValue($version)
{
        //echo "Version string: $version<br>";
        
        //The rest of the function has been added to, rewritten, and modified extensively (robogeek)
        //Contains regular expressions to do sanity checks and to translate alpha characters and release candidate (RC) data into the version value
        //When version check fails it will post an error message about why it failed
        //To see more variable info when running, set $debug to 1 in the main script
         
        global $debug;                                  //use debug variable to enable extra debug info to be displayed
        $version_segments = 7;                  //000.000.000.000.000.000.000 has 7 segments (last two segments will be used for translating alpha character at end of version string and release candidate #)
        $alpha_version = "000";                 //default version number for alpha segment if no alpha character is found
        $release_candidate = "000";             //default version number rc segment if no rc version is found
        $original_version = $version;   //keep a copy of the original version available


        //Do initial sanity check on the version variable
        //Check if version contains unwanted symbols (should only contain 0-9, a-z, A-Z, and .)
        if (ereg('[^A-Za-z0-9. ]', $version)) { 
                $failed_version_check = 1; 
                echo "Version check failed...Version contains characters other than letters, numbers, and periods ($original_version)<br>";
                echo "Skipping rest of sanity checks and version parsing...<br>";
                }
        else {
                //Create an array for upper/lowercase alpha characters and assign them a value (a = lowest, Z = highest)
                $alpha_array = array( "a" => "001", "b" => "002", "c" => "003", "d" => "004", "e" => "005", "f" => "006", "g" => "007", "h" => "008", "i" => "009", "j" => "010", "k" => "011", "l" => "012", "m" => "013", "n" => "014", "o" => "015", "p" => "016", "q" => "017", "r" => "018", "s" => "019", "t" => "020", "u" => "021", "v" => "022", "w" => "023", "x" => "024", "y" => "025", "z" => "026", "A" => "027", "B" => "028", "C" => "029", "D" => "030", "E" => "031", "F" => "032", "G" => "033", "H" => "034", "I" => "035", "J" => "036", "K" => "037", "L" => "038", "M" => "039", "N" => "040", "O" => "041", "P" => "042", "Q" => "043", "R" => "044", "S" => "045", "T" => "046", "U" => "047", "V" => "048", "W" => "049", "X" => "050", "Y" => "051", "Z" => "052");

                //Check if version contains an RC value
                //split version variable at RC and do sanity checks
                $rc = 0;
                if (preg_match("/RC/", $version)) $rc = explode("RC", $version);
                elseif (preg_match("/rc/", $version)) $rc = explode("rc", $version);
                if ($rc) {
                        $version = trim($rc[0]);
                        $release_candidate = str_pad(trim($rc[1]),3,"0",STR_PAD_LEFT);
                        if ($debug) echo "Release Candidate: $release_candidate <br>";

                        //check rc format (should be a number only, no alpha and no periods)
                        if ($release_candidate == "000" || !(preg_match("/[0-9]{3}/",$release_candidate))) {
                                $failed_version_check = 1;
                                echo "Version check failed...release candidate should only be an integer between 1 and 999 and not contain any other characters.<br><br>";
                                }
                        }

                //Continue sanity checks on version #
                //Check for any alpha at beginning and remove
                $version_length = strlen($version);
                while (!(preg_match("/^[0-9.]/", $version)) && $version_length > 0) {
                        $version = substr($version, 1);
                        $version_length = strlen($version);
                        }

                if ($debug) echo "Version length: $version_length <br>Remove alpha from beginning: " . $version . "<br>";

                if ($version_length < 1){
                        $failed_version_check = 1;
                        echo "Version check failed...version can't be all spaces and/or alpha characters ($original_version)<br><br>";
                        }

                //check for multiple alpha at end or within version string and fail if found.
                if (!$failed_version_check && ((preg_match("/[a-zA-Z]{2}/", $version))) && ($version_length > 1)) {
                        $failed_version_check = 1;
                        echo "Version check failed...too many alpha characters within or at end of version string ($original_version).<br><br>";
                        }

                //check for stuff like .a and 1a and fail if found
                if (!$failed_version_check && ((preg_match("/[0-9]*[.][a-zA-Z]{1}$/", $version)) || ((preg_match("/[a-zA-Z]{1}$/", $version)) && ($version_length<3)))) { //check for stuff like .a and 1a and reject those if found
                        $failed_version_check = 1;
                        echo "Version check failed...alpha can't follow major version number or a period ($original_version).<br><br>";
                        }

                //check for multiple periods and fail if found
                if (!$failed_version_check && (preg_match("/[.]{2}/", $version))) {
                        $failed_version_check = 1;
                        echo "Version check failed...version contains mulitple consecutive periods ($original_version). <br><br>";
                        }

                //check for  version segments of 4 or more digits and fail if found
                if (!$failed_version_check && (preg_match("/[0-9]{4}/", $version))) { 
                        $failed_version_check = 1;
                        echo "Version check failed...version contains segment with more than 3 digits ($original_version). <br><br>";
                        }

                //check for alpha within (not at beginning or end) the version string and fail if found
                if (!$failed_version_check && (preg_match("/[0-9.]*[a-zA-Z][0-9.]/", $version))) { 
                        $failed_version_check = 1;
                        echo "Version check failed...alpha within version is not allowed ($original_version). <br><br>";
                        }

                //Now we can take the alpha character out of version string and convert it to a version number segment, if it exists
                if (!$failed_version_check && (preg_match("/[a-zA-Z]{1}$/", $version)) && $version_length>2) {
                        $alpha = substr($version,-1,1);
                        $alpha_version = $alpha_array["$alpha"]; //convert alpha character to 3 digit version number from the array $alpha_array
                        if ($debug) echo "Alpha: " . $alpha . " (" . $alpha_version . ")<br>";
                        //check if ends with just alpha or .alpha and remove it from the version string
                        if (substr($version,-1,2) == ".")
                                $version = substr($version,0,$version_length-2);
                        else
                                $version = substr($version,0,$version_length-1);
                        }

                $alpha_version = $alpha_version . ".";

                //Take version number and convert it to an array and pad the array so we have the required number of segments ($version_segments-2)
                //We use $version_segments-2 because we need 2 free segments to add the alpha character and release candidate info
                $pre_versions = explode(".",$version);
                $pre_segments = count($pre_versions);
                $pre_versions = array_pad($pre_versions,($version_segments-2),"000");

                //Fail if version contains too many segments
                if ($pre_segments > ($version_segments-2)) {
                        $failed_version_check = 1;
                        echo "Failed version check...version is too long/contains too many segments ($original_version).<br><br>";
                        }

                $verValue = "";
        
                //Generate the initial portion of version value based on user supplied version number and pad each number (if needed) so it is 3 digits
                foreach($pre_versions as $ver) {        
                        $verValue .= str_pad($ver,3,"0",STR_PAD_LEFT). ".";
                        }

                echo "Padded version: " . trim($verValue,".") . "<br>";
                if ($debug) echo "Version segments: $pre_segments <br>Max version segments: " . ($version_segments-2) . " with 2 reservered for alpha and RC. <br>";


                //Generate valid, fully padded version number
                $version = $verValue . $alpha_version . $release_candidate;

                if ($alpha_version != "000") echo "Alpha Version: " . trim($alpha_version,".") . " passed!<br>";
                else echo "No alpha found in version number...passed!<br>";
                if ($release_candidate != "000") echo "Release candidate (RC) version: " . trim($release_candidate,".") . " passed!<br>";
                else echo "No RC found in version number...passed!<br>";
                echo "Full padded version: $version <br>";
                $versions = explode(".",$version);
                $segments = count($versions);

                if ($debug) echo "Number of segments: " . $segments . "<br>";

                $verValue = "";
        
                //Generate version value integer based on fully padded version number from above
                foreach($versions as $ver) {    
                        $verValue .= str_pad($ver,3,"0",STR_PAD_LEFT);
                        }
        }

        if ($failed_version_check)
                return -1;  //if any of the version sanity checks fails, then return -1 so we know it failed
        else
                return $verValue;  //if we passed all sanity checks, then return the version value integer
}

//function to return list of files and directories older than specified # of days
function file_list($path, $file, $days, $only_dir)
{
        $files = array();
        $returned_files = array();
        findFiles($files,$path.$file);
        $count = 0;
        foreach ($files as $found_file) {
                if ($only_dir) {
                        if (is_dir($found_file)) {
                                $diff = floor((time() - filemtime("$found_file"))/60/60/24);
                                if ($diff > $days) {
                                        $returned_files[$count] = $found_file;
                                        $count++;
                                        }
                        }
                }
                else {          
                        $diff = floor((time() - filemtime("$found_file"))/60/60/24);
                        if ($diff > $days) {
                                $returned_files[$count] = $found_file;
                                $count++;
                                }
                        }
                }
        return $returned_files;
}

//recursive function to delete a directory tree
function remove_directory($dir) 
{
        if ($handle = opendir("$dir"))
        {
        while (false !== ($item = readdir($handle))) 
        {
                if ($item != "." && $item != "..")
                {
                        if (is_dir("$dir/$item"))
                        {
                        remove_directory("$dir/$item");
                        }
                        else
                        {
                        unlink("$dir/$item");
                        // echo " removing $dir/$item<br>\n";
                        }
                }
        }
        closedir($handle);
        rmdir($dir);
        //echo "removing $dir<br>\n";
        }
}

//Deletes all files matching a pattern
function delfile($str)
{
   foreach(glob($str) as $fn) {
                
                if (is_dir($fn))
                {
                        remove_directory($fn);
                }
                else
                {
                        //echo "deleting file $fn<br>";
                        unlink($fn);
                }
   }
} 

//files that match the file pattern are appended to the array (Case Sensitive)
function findFiles(&$array, $filePattern)
{
        foreach(glob($filePattern) as $file)
        {
                array_push($array,$file);
        }
}


//Creates an entire directory path, creates each folder in the path as needed
function mkpath($path)
{
        $dirs=array();
        $path=preg_replace('/(\/){2,}|(\\\){1,}/','/',$path); //only forward-slash
        $dirs=explode("/",$path);
        $path="";
        foreach ($dirs as $element)
        {
                $path.=$element."/";
                if(!is_dir($path))
                {
                        if(!mkdir($path)){ echo "something was wrong at : ".$path; return false; }
                }         
        }
        return true;    
}

function GetPopularity($plugin_id)
{
        global $db_host;
        global $db_user;
        global $db_pass;
        global $db_database;
        mysql_connect($db_host, $db_user, $db_pass) or die("Error connecting to mysql");
        mysql_select_db($db_database) or die("Error connecting to database");
        $q = mysql_query("SELECT * FROM popularities WHERE plugin_ID = \"$plugin_id\"");
        $n = mysql_numrows($q);
        if ($n < 1) $n = 0;
        return $n;
}

function VoteWorking($profil_id, $plugin_id, $plugin_version, $vote, $uniqueID)
{
        global $db_host;
        global $db_user;
        global $db_pass;
        global $db_database;
        mysql_connect($db_host, $db_user, $db_pass) or die("Error connecting to mysql");
        mysql_select_db($db_database) or die("Error connecting to database");

        if ($vote == "")
        {

                //header
                echo "<table border=0 cellpadding=0 cellspacing=0 width='100%'><tr><td>&nbsp;Does this plugin work?&nbsp;&nbsp;";
                //your vote
                if ($profil_id == "") echo "</td></tr><tr><td>&nbsp;<a href='http://www.meedios.com/forum/login.php'>Login to vote</a>";
                else
                {
                        $sql = "SELECT * FROM votesWorking WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version' AND profil_id='$profil_id'";
                        $res = mysql_query($sql);
                        $nb = mysql_numrows($res);
                        if ($nb == 0)
                        {
                                echo "<a href=\"$uniqueID&mode=vote&profile_id=$profil_id&plugin_id=$plugin_id&plugin_version=$plugin_version&vote=true\">YES</a> or <a href=\"index.php?mode=vote&profile_id=$profil_id&plugin_id=$plugin_id&plugin_version=$plugin_version&vote=false\">NO</a>";
                        }
                        elseif ($nb == 1)
                        {
                                echo "</td></tr><tr><td>&nbsp;(You voted ";
                                $resultrow = @mysql_fetch_array($res);
                                if ($resultrow["vote"] == "true") echo "<b>Yes</b>";
                                if ($resultrow["vote"] == "false") echo "<b>No</b>";
                                echo ")<br>";
                        }
                }
                echo "</td><tr><td>";

                //community vote
                $sql = "SELECT * FROM votesWorking WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version'";
                $res = mysql_query($sql);
                $nb = mysql_numrows($res);
                $enregNb = 0;
                $yes = 0;
                $no = 0;
                while (($enregNb<$nb)&&($enreg=@mysql_fetch_array($res)))
                {
                        if ($enreg["vote"] == "true") $yes++;
                        else $no++;
                }
                if ($yes == 0 && $no == 0) echo "&nbsp;(Community hasn't voted yet)";
                elseif ($yes > $no) echo "&nbsp;(Community says Yes)";
                elseif ($no > $yes) echo "&nbsp;(Community says <b>No</b>)";
                elseif ($no == $yes) echo "&nbsp;(Community is in a tie...be cautious)";

                echo "</td></td>\n";

                //footer
                echo "</table>\n";

        }
        else
        {
                //mysql_query("CREATE TABLE IF NOT EXISTS `votesWorking` (`plugin_ID` varchar(38) NOT NULL, `plugin_Version` varchar(7) NOT NULL, `profil_id` tinytext NOT NULL, `vote` enum('true','false') NOT NULL)");
                $sql = "SELECT * FROM votesWorking WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version' AND profil_id='$profil_id'";
                $res = mysql_query($sql);
                $nb = mysql_numrows($res);
                if ($nb == 0) mysql_query("INSERT INTO votesWorking (`profil_id`, `plugin_ID`, `plugin_Version`, `vote`) VALUES ('$profil_id', '$plugin_id', '$plugin_version', '$vote')");
                if ($nb == 1) mysql_query("UPDATE votesWorking SET vote='$vote' WHERE profil_id='$profil_id' AND plugin_ID='$plugin_id' AND plugin_Version='$plugin_version'");
                //echo "You just voted ";
                //if ($vote == "true") echo "<b>Yes</b>";
                //if ($vote == "false") echo "<b>No</b>";
        }
}


function StartElement(&$parser, &$name, &$attrs)
{
   global $depth;
   global $data, $CData, $XMLEntities;
  
   $depth[$parser]++;
 
    // Start with empty CData array.
    $CData = array();

    // Put each attribute into the Data array.
    foreach ($attrs as $Key => $Value) {
        $data["$name:$Key"] = strtr(trim($Value), $XMLEntities);
         //echo "$name:$Key = {$data["$name:$Key"]}<br>\n";
    }

}

function EndElement(&$parser, &$name)
{
   global $depth;
   $depth[$parser]--;

        global $data, $CData, $XMLEntities;
        global $plugin_name;
        global $plugin_description;
        global $plugin_author;
        global $plugin_type;
        global $module_type;
        global $plugin_licence;
        global $plugin_state;
        global $plugin_version;
        global $plugin_id;
        global $plugin_packageid;
        global $plugin_DestinationFolder;
        global $plugin_Copyright;
        global $plugin_Documentation;
        global $plugin_DocumentationText;
        global $plugin_SupportLink;
        global $plugin_DonationLink;
        global $plugin_SourceLink;
        global $plugin_AuthorEmail;
        global $plugin_Site;
        global $plugin_DirectDownloadURL;
        global $plugin_MinRequiredVersion;
        global $plugin_MaxRequiredVersion;
        global $plugin_IsDotNET;
        global $plugin_IsDotNETSimple;
        global $parser_failed;

        if ($parser_failed) $data = utf8_decode($data);

    /*
     * Mush all of the CData lines into a string
     * and put it into the $Data array.
     */
    $data[$name] = strtr( trim( implode('', $CData) ), $XMLEntities);

        switch($name)
        {
                case "NAME":  //-
                        $plugin_name = addslashes($data[$name]);
                        break;
                case "DESCRIPTION": //--
                        $plugin_description = addslashes($data[$name]);
                        break;
                case "AUTHOR":    //-
                        $plugin_author = addslashes($data[$name]);
                        break;
                case "AUTHOR-EMAIL":
                        $plugin_AuthorEmail = addslashes($data[$name]);
                        break;
                case "EXTENSIONTYPE":
                        $plugin_type = addslashes($data[$name]);
                        break;
                case "MODULE-TYPE":
                        $module_type = addslashes($data[$name]);
                        break;
                case "LICENSE-TYPE":
                        $plugin_licence = addslashes($data[$name]);
                        break;
                case "PACKAGEID":  //-       PackageId
                        $plugin_packageid = addslashes($data[$name]);
                        break;
                case "VERSIONTYPE":  //-
                        $plugin_state = addslashes($data[$name]);
                        break;
                case "EXTENSIONID":    //-
                        //Commented out following line...we don't want to strip the curly braces from the plugin-id
                        //$plugin_id = substr(addslashes($data),1,strlen($data)-2);
                        $plugin_id = addslashes($data[$name]);
                        break;
                case "VERSION":       //-
                        $plugin_version = addslashes($data[$name]);
                        break;
                case "COPYRIGHT":
                        $plugin_Copyright = addslashes($data[$name]);
                        break;
                case "DOCUMENT-FILE":
                        $plugin_Documentation = addslashes($data[$name]);
                        break;
                case "DOCUMENT-TEXT":
                        $plugin_DocumentationText = addslashes($data[$name]);
                        break;
                case "SUPPORT-LINK":
                        $plugin_SupportLink = addslashes($data[$name]);
                        break;
                case "DONATION-LINK":
                        $plugin_DonationLink = addslashes($data[$name]);
                        break;
                case "SOURCE-LINK":
                        $plugin_SourceLink = addslashes($data[$name]);
                        break;
                case "DESTINATION-FOLDER":
                        $plugin_DestinationFolder = addslashes($data[$name]);
                        break;
                case "SITE":
                        $plugin_Site = addslashes($data[$name]);
                        break;
                case "DIRECT-DOWNLOAD-URL":
                        $plugin_DirectDownloadURL = addslashes($data[$name]);
                        break;
                case "MIN-REQUIRED-VERSION":
                        $plugin_MinRequiredVersion = addslashes($data[$name]);
                        break;
                case "MAX-REQUIRED-VERSION":
                        $plugin_MaxRequiredVersion = addslashes($data[$name]);
                        break;
                case "IS-DOT-NET":
                        if ((strtolower($data[$name]) == "yes") || (strtolower($data[$name]) == "true"))
                                $plugin_IsDotNET = "1";
                        else $plugin_IsDotNET = "0";
                        break;
                case "IS-DOT-NET-SIMPLE":
                        if ((strtolower($data[$name]) == "yes") || (strtolower($data[$name]) == "true"))
                                $plugin_IsDotNETSimple = "1";
                        else $plugin_IsDotNETSimple = "0";
                        break;
        }
}


// Place lines into an array because elements can contain more than one line of data (ie DOCUMENT-TEXT)
function characterData(&$parser, &$line)
{
    global $CData;
    $CData[] = $line;
}


// Clear plugin images if there exist
// TODO: update for new directory structure and add plugin_version as new argument
function ClearImagesPlugin($plugin_id)
{

global $db_host;
global $db_user;
global $db_pass;
global $db_database;
global $plugin_home_directory;

@mysql_connect($db_host, $db_user, $db_pass) or die("Error");
@mysql_select_db($db_database) or die("Error");

$sql = "SELECT * FROM plugins WHERE plugin_ID LIKE '$plugin_id'";
$res = mysql_query($sql);

$nb = mysql_numrows($res);  // Number of records

$i = 0;
while ($i < $nb)
{
        $Image_Name = mysql_result($res, $i, "nom");

        // delete image
        unlink ("$plugin_home_directory/$plugin_id/$Image_Name.jpg");

        // Delete thumbails
        unlink ("$plugin_home_directory/$plugin_id/$Image_Name.mini.jpg");

        $i++;
}

// Delete records in table
$sql = "DELETE FROM plugins WHERE plugin_ID LIKE '$plugin_id'";
$res = mysql_query($sql);

mysql_close();
}


//Adds a plugin to the System.
//Extracts information from the plugin
//extracts images and creates thumbnails
function AddMpp($file, $u) {
        $ignore_version = FALSE;
        $ignore_sanity = FALSE;
        $delete_upload = TRUE;
        $force_update = FALSE;
        $process_multiple = FALSE;
        return AdminAddMpp($file, $ignore_version, $ignore_sanity, $delete_upload, $force_update, $process_multiple, $u);
}

//Adds a plugin to the system via admin control panel and leaves mpp file intact in ftp directory.
//Extracts information from the plugin
//extracts images and creates thumbnails
//$force_update: 1=update db record for given plugin with new info, 0=behave as before, don't update db record
//$delete_upload: 1=delete mpp file when done (user uploads), 0=don't delete (admin uploads from ftp repository directory)
//$ignore_version: 1=tell it to add regardless of version calculation, 0=don't add if it fails version calculation
function AdminAddMpp($file, $ignore_version, $ignore_sanity, $delete_upload, $force_update, $process_multiple, $u)
{
        global $plugin_home_directory;
        global $plugin_name;
        global $plugin_version;
        global $plugin_description;
        global $plugin_author;
        global $plugin_type;
        global $module_type;
        global $plugin_licence;
        global $plugin_state;
        global $plugin_id;
        global $plugin_packageid;
        global $plugin_date;
        global $plugin_Copyright;
        global $plugin_SupportLink;
        global $plugin_Key;
        global $plugin_Documentation;
        global $plugin_DocumentationText;
        global $plugin_SupportLink;
        global $plugin_DonationLink;
        global $plugin_SourceLink;
        global $plugin_DestinationFolder;
        global $plugin_AuthorEmail;
        global $plugin_Site;
        global $plugin_DirectDownloadURL;
        global $plugin_MinRequiredVersion;
        global $plugin_MaxRequiredVersion;
        global $plugin_IsDotNET;
        global $plugin_IsDotNETSimple;   
        global $parser_failed;
        global $types;

        $parser_failed = FALSE;
        $pluginfile_error = FALSE;
        $plugin_flagged = "FALSE";
        $encoding_unknown = FALSE;
        $plugin_name = "";
        $plugin_version = "";
        $plugin_description = "";
        $plugin_author = "";
        $plugin_type = "";
        $module_type = "";
        $plugin_licence = "";
        $plugin_state = "";
        $plugin_id = "";
        $plugin_packageid = "";
        $plugin_date = "";
        $plugin_Copyright = "";
        $plugin_SupportLink = "";
        $plugin_Key = "";
        $plugin_Documentation = "";
        $plugin_DocumentationText = "";
        $plugin_SupportLink = "";
        $plugin_DonationLink = "";
        $plugin_SourceLink = "";
        $plugin_DestinationFolder = "";
        $plugin_AuthorEmail = "";
        $plugin_Site = "";
        $plugin_DirectDownloadURL = "";
        $plugin_MinRequiredVersion = "";
        $plugin_MaxRequiredVersion = "";
        $plugin_IsDotNET = "";
        $plugin_IsDotNETSimple = "";   
        $temp_directory = Generate_Temp_Directory("data", "plugin_");
        if (!$temp_directory) {
                echo "Error creating temp directory! Sorry, we can't continue... <br><br>\n";
                return FALSE;
                }
        
        echo "Processing file: $file<br>\n";

        echo "Checking uploaded file exists... ";
        if (!is_file($file))
        {
                echo "<b>File doesn't exist!</b><br><br>\n";
                echo "Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
        }
        echo "passed.<br>\n";
        
        $file_size = filesize($file);
        
        echo "Checking zip archive consistency... ";
        $archive = new PclZip($file) or die("pclzip died");
        $list = $archive->listContent();
        
        if ($list == 0)
        {
                echo "<b>Failed! The archive is corrupt or not a Zip.</b><br><br>\n";
                echo "Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
        }
        else
        {
                echo "passed.<br>\n";
        }

        echo "Searching for installer.xmp ... ";
        $duplicatesFound = FALSE;
        for ($i=0; $i<sizeof($list); $i++) {
                $index_archive_plugin = "";
                for (reset($list[$i]); $key = key($list[$i]); next($list[$i])) {
                        if (strlen($list[$i][$key]) > 7) {
                                $slm = substr($list[$i][$key],strlen($list[$i][$key])-7);
                                $slm2 = $list[$i][$key];
                                if (strcasecmp($slm2, 'installer.xmp') == 0) {
                                        echo($slm2."\n");
                                        if ($index_archive_plugin <> "") {
                                                $duplicatesFound = TRUE;
                                        }
                                        $index_archive_plugin = $i;
                                        $pluginfile = $temp_directory. "/" . $list[$i][$key];
                                        $pluginxmlfilename = $list[$i][$key];
                                        break;
                                }
                        }
                }
                if ($index_archive_plugin <> "") {
                        break;
                }
        }

        if (!isset($pluginfile) || $pluginfile == "")
        {
                echo "<b>No .plugin file(s) were found.</b><br>\n";
                echo "You should fix or remove this mpp manually from ftp area: <br>";
                echo "$file <br><br>";
                echo "Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
        }
        if ($duplicatesFound)
                echo "passed (however, there are multiple .plugin's. assuming they're all identical...)<br>\n";
        else
                echo "passed.<br>\n";

        echo ("Clearing Temporary directory<br>\n");
        remove_directory($temp_directory);

        echo "Extracting installer.xmp... ";
        $archive->extract("$temp_directory");

        print_r(glob("$temp_directory\*.*"));

        if (!is_file($pluginfile))
        {
                echo "<b>Failed!</b><br><br>\n";
                echo "Clearing Temporary directory...\n";
                //remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
        }
        echo "passed.<br>\n";

        unset($archive);

        //try to determine the encoding type of the .plugin file or set to UTF-8 if not specified
        $rx = '/<?xml.*encoding=[\'"](.*?)[\'"].*?>/m';
        $xml_content = file_get_contents($pluginfile);
        if (preg_match($rx, $xml_content, $m)) {
          $encoding = strtoupper($m[1]);
        } else {
          $encoding = "UTF-8";
        }
        if (($encoding != "UTF-8") && ($encoding != "US-ASCII") && ($encoding != "ISO-8859-1")) {
                $specified_encoding = $encoding;
                $encoding = "UTF-8";
                $encoding_unknown = TRUE;
                }


        if ($encoding_unknown) echo "Parsing installer.xmp (your encoding '$specified_encoding' is not recognized, trying UTF-8 instead)...";
        else echo "Parsing installer.xmp (using $encoding)...";
        $Data = array();
        $xml_parser = xml_parser_create($encoding);
        xml_set_element_handler($xml_parser, "StartElement", "EndElement");
        xml_set_character_data_handler($xml_parser, "characterData");
        if (!($fp = fopen($pluginfile, "r"))) {
                echo "<b>Error opening installer.xmp file.</b><br><br>\n";
                @unlink($pluginfile);
                if ($delete_upload) @unlink($file);
                echo "Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
        }
        while ($data = @fread($fp, 4096)) {
                if (!@xml_parse($xml_parser, $data, feof($fp))) {
                        $msg = xml_error_string(xml_get_error_code($xml_parser));
                        $line = xml_get_current_line_number($xml_parser);
                        echo "<b>Error in installer.xmp file: '$msg' on line $line</b><br>\n";
                        $parser_failed = TRUE;
                }
        }
        xml_parser_free($xml_parser);
        fclose($fp);

        if ($parser_failed) {
                echo "We don't recognize the encoding type of your installer.xmp XML!<br>";
                echo "Be sure it is encoded with UTF-8, ISO-8859-1, or US-ASCII.<br>";
                echo "If you are sure it is encoded with one of these types, try specifying<br>";
                echo "the type within your .plugin file like this:<br><br>";
                echo "<?xml version=\"1.0\" encoding=\"$encoding\"?><br><br>";
                @unlink($pluginfile);
                if ($delete_upload) @unlink($file);
                echo "Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
        }

        if ($plugin_id == "")
        {
                echo "<b>No valid extension-id node was found.</b><br><br>\n";
                @unlink($pluginfile);
                if ($delete_upload) @unlink($file);
                echo "Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
        }
        echo "passed.<br>\n";

        if ($encoding_unknown) {
                echo "We didn't recognize the encoding type (<b>$specified_encoding</b>) of your installer.xmp XML!<br>";
                echo "You should use UTF-8, ISO-8859-1, or US-ASCII encoding to<br>";
                echo "ensure OpenMAID accurately parses your plugin details.  If<br>";
                echo "you are certain you are using one of these recognized types,<br>";
                echo "you can specify the type within your .plugin file like this:<br><br>";
                echo htmlentities("<?xml version=\"1.0\" encoding=\"$encoding\"?>", ENT_QUOTES);
                echo "<br><br>";
        }

        //Checks plugin_type and plugin_moduletype.  Make sure entries are valid.  If module type exists and is valid,
        //it overrides the plugin_type value.
        $plugin_type = strtolower($plugin_type);
        $module_type = strtolower($module_type);
        echo "Checking for valid plugin type...$plugin_type...";
        $valid_plugin_type = IsPluginTypeValid($plugin_type, $types);
        if ($valid_plugin_type) echo "passed!<br>";
        elseif (!$valid_plugin_type && !$ignore_sanity) {
                echo "<b>failed!</b><br>";
                echo "The plugin type you specified ($plugin_type) is not valid! <br>";
                $pluginfile_error = TRUE;
                }
        elseif (!$valid_plugin_type && $ignore_sanity) {
                echo "<br><b>Error</b>...The plugin type you specified ($plugin_type) is not valid!<br>";
                echo "<b>You chose to ignore sanity checks so we are continuing...</b><br>";
                }
        echo "Checking for valid module type...$module_type...";
        $moduletypes = array("icon", "misc", "hack", "sub");
        $valid_module_type = IsPluginTypeValid($module_type, $moduletypes);
        if ($valid_module_type) echo "passed!<br>";
        elseif (!$valid_module_type && !$ignore_sanity && $module_type != $plugin_type) {
                echo "<b>failed!</b><br>";
                echo "The module type you specified ($module_type) is not valid! <br>";
                if (!$valid_plugin_type) $pluginfile_error = TRUE;
                else echo "Plugin type ($plugin_type) is valid, we will ignore the module type.<br>";
                }
        elseif (!$valid_module_type && $ignore_sanity && $module_type != $plugin_type) {
                echo "<br><b>Error</b>...The module type you specified ($module_type) is not valid!<br>";
                echo "<b>You chose to ignore sanity checks so we are continuing...</b><br>";
                }
        if ($valid_module_type && $module_type != "none" && $module_type != "") {
                echo "Module type is valid.  Setting plugin type to <b>$module_type</b>...";
                $plugin_type = $module_type;
                echo "Done!<br>";
                }
        elseif (!$valid_module_type && $valid_plugin_type && $module_type != $plugin_type) {
                echo "Module type is invalid.  Assuming plugin type is <b>$plugin_type</b>...";
                echo "Done!<br>";
                }
        elseif (!$valid_module_type && $valid_plugin_type && $module_type == $plugin_type) {
                echo "Module and Plugin Types are the same.  Assuming plugin type is <b>$plugin_type</b>...";
                echo "Done!<br>";
                }

        //Checks plugin id to be sure it is unique and not conflicting with an existing plugin
        //Ties plugin id to destination directory and plugin type
        //If check fails, returns name of plugin conflicting plugin. If it passes, return 'passed'
        echo "Checking Plugin ID...$plugin_id...";
        $pluginID_passed = CheckForUniquePluginID($plugin_id, $plugin_type, $plugin_DestinationFolder);
        if ($pluginID_passed != "passed" && !ignore_sanity) {
                echo "<br><b>Error</b>...Your plugin ID (GUID) conflicts with another plugin: <br>";
                echo "<b>$pluginID_passed</b><br>";
                echo "This is an unrecoverable error. You must give your plugin a new GUID and retry the<br>";
                echo "upload again. If you feel this error is incorrect, PM your .plugin file to the<br>";
                echo "OpenMAID admin and ask why it is failing the Plugin ID (GUID) validation check.<br><br>\n";
                @unlink($pluginfile);
                if ($delete_upload) @unlink($file);
                echo "Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
                }
        elseif ($pluginID_passed != "passed" && $ignore_sanity) {
                echo "<br><b>Error</b>...Your plugin ID (GUID) conflicts with another plugin: <br>";
                echo "<b>$pluginID_passed</b><br>";
                echo "<b>You chose to ignore sanity checks so we are continuing...</b><br>";
                }
        else echo "passed!<br>";

        //Checks plugin type to be sure user isn't trying to upload plugin with a new plugin type
        //If check fails, return name of plugin type of last uploaded version. If it passes, return TRUE
        echo "Checking for changed plugin type...";
        $pt_passed = CheckPluginTypeUpload($plugin_id, $plugin_type);
        $plugin_DestinationFolder=$plugin_id;
        if ($pt_passed != "passed" && !$ignore_sanity) {
                echo "<b>failed!</b><br>";
                echo "You cannot change the plugin type from <b>$pt_passed</b> to <b>$plugin_type</b>!<br>";
                echo "To change plugin type, you must make it a new plugin by giving it a new GUID. <br>";
                $pluginfile_error = TRUE;
                }
        elseif ($pt_passed != "passed" && $ignore_sanity) {
                echo "<br><b>Error</b>...You cannot change the plugin type from <b>$plugin_type</b> to <b>$pt_passed</b>!<br>";
                echo "<b>You chose to ignore sanity checks so we are continuing...</b><br>";
                }
        else echo "$plugin_type...passed!<br>";
        //Checks plugin author to be sure user isn't trying to upload someone else's plugin
        //If check fails, return name of plugin author profil_id of last uploaded version. If it passes, return TRUE
        echo "Checking plugin profile ID...";
        $pid_passed = CheckProfileIDUpload($plugin_id, $plugin_author, $u);
        $pid = explode(":::",$pid_passed);
        $pid_passed = $pid[0];
        $pid_reason = $pid[1];
        if ($pid_passed == "unknown") {
                echo "<b>failed!</b><br>";
                echo "<b>Error</b>...$pid_reason";
                echo "<b>Please report this unknown condition data to the OpenMaid admin!</b><br>";
                $pluginfile_error = TRUE;
                }
        elseif (($pid_passed == "flagged") && (!$ignore_sanity && !$ignore_version)) {
                echo "...flagged for admin review.<br>";
                echo "Flag Reason: $pid_reason<br>";
                $plugin_flagged = "TRUE";
                }
        elseif (($pid_passed == "flagged") && ($ignore_sanity || $ignore_version)) {
                echo "...ignoring sanity check, not flagged for admin review.<br>";
                echo "Flag Reason (but not flagged): $pid_reason<br>";
                }
        elseif ($pid_passed == "passed") echo "plugin profile ID is set to $u ...passed.<br>";
        else {
                echo "Unknown condition:<br>1 - $plugin_id <br> 2 - $pid_passed <br> 3 - $ignore_sanity <br> 4 - $u <br> 5 - $plugin_author <br>";
                echo "<b>Please report this unknown condition data to the OpenMaid admin!</b><br>";
                }

        //Check for multiple destination folders with same GUID.  Destination folder shouldn't change from version to version. (robogeek)
        //TRUE = pass, <previous destination folder> = failed check
        echo "Checking for changed destination folder...";
        $df_multiple = CheckForMultipleDestinationFolders($plugin_id, $plugin_DestinationFolder);
        if ($df_multiple != "passed" && !$ignore_sanity) {
                echo "<b>failed!</b><br>";
                echo "<b>Stopping</b>...<br>Your destination folder has changed from a previous version!<br>";
                echo "This is not allowed.  You should change the destination folder back to what it was <br>";
                echo "or give this plugin a new GUID and name and try uploading it again.<br>";
                echo "Uploaded plugin destination folder:<b> $plugin_DestinationFolder </b><br>";
                echo "Previous plugin destination folder:<b> $df_multiple </b><br>\n";
                $pluginfile_error = TRUE;
                }
        elseif ($df_multiple != "passed" && $ignore_sanity) {
                echo "<b>Error</b>...<br>Your destination folder has changed from a previous version!<br>";
                echo "<b>You chose to ignore sanity checks so we are continuing...</b><br>";
                }
        else echo "passed!<br>";

        //Check destination folder for conflicts with other plugins.  Should be a unique destination folder. (robogeek)
        //TRUE = pass, <plugin name> = conflict
        echo "Checking for destination folder conficts...";
        $df_passed = CheckForDupeDestinationFolder($plugin_id, $plugin_type, $plugin_DestinationFolder);
        if ($df_passed != "passed" && !$ignore_sanity) {
                echo "<b>failed!</b><br>";
                echo "<b>Stopping</b>...<br>Your destination folder conflicts with an existing plugin:<br> <b>$df_passed</b><br>";
                echo "This is not allowed.  You should change the destination folder back to what it was <br>";
                echo "or give this plugin a new GUID and name and try uploading it again.";
                $pluginfile_error = TRUE;
                }
        elseif ($df_passed != "passed" && $ignore_sanity) {
                echo "<b>Error</b>...<br>Your destination folder conflicts with an existing plugin:<br> <b>$df_passed</b><br>";
                echo "<b>You chose to ignore sanity checks so we are continuing...</b><br>";
                }
        echo "passed!<br>";

        //Version checking (robogeek)
        echo "Checking version numbering format...<br>";
        //calculate uploaded plugin version value
        $plugin_VersionValue = ltrim(CalculateVersionValue($plugin_version),"0");
        if ($plugin_VersionValue == -1) {
                echo "<b>Stopping</b>...version format check failed.<br>";
                $pluginfile_error = TRUE;
                }
        else echo "Version numbering format passed!<br>";


        //get most recent plugin version value
        $previous_versionvalue = GetLatestVersionForState($plugin_id,"ALL");
        $previous_version = GetLatestVersionForState2($plugin_id,"ALL");
        if ($previous_versionvalue < 0) $previous_versionvalue = 0;
        echo "Uploaded version: $plugin_version ($plugin_VersionValue)<br>";
        echo "Previous version: ";
        if ($previous_version < 0) echo "N/A<br>";
        else echo "$previous_version ($previous_versionvalue)<br>";

        if ($ignore_version == FALSE) {
                echo "Checking uploaded version number (must be a newer version number)...";
                //test to see if uploaded version is newer than most recent plugin in database
                //throw error if version is not newer and exit
                //suggest valid version number based on most recent version of plugin in database (todo)
                if ($plugin_VersionValue <= $previous_versionvalue)  {
                        echo "<b>failed!</b><br>";
                        echo "<b>Version Error!</b>  The uploaded version is not newer than the most recent version in our database.<br>";
                        echo "You can only upload versions that are newer.<br>";
                        $pluginfile_error = TRUE;
                        }
                else echo "passed!<br>";
                }

        if ($ignore_version == TRUE) {
                //test to see if uploaded version is newer than most recent plugin in database
                //throw error if version is not newer and exit
                //suggest valid version number based on most recent version of plugin in database (todo)
                echo "Skipping version number check (should be a newer version number)...<br>";
                if ($plugin_VersionValue <= $previous_versionvalue)  {
                        echo "The version being added is older than the most recent version in the database.<br>";
                        echo "You chose to ignore the version and add anyway.  If the version error is the result of<br>";
                        echo "the author choosing a version number out of sequence, it may mess up the sorting and<br>";
                        echo "history for this plugin on the OpenMaid web site and in the OpenMaid Plugin.<br>";
                        }
                }


        echo "Calculating MD5 Hash:";
        $plugin_hash = md5_file($file);
        echo "$plugin_hash <br>";

        if ($pluginfile_error == TRUE) {
                @unlink($pluginfile);
                if ($delete_upload) @unlink($file);
                echo "<br>Clearing Temporary directory...\n";
                remove_directory($temp_directory);
                echo "Done!<br>";
                return FALSE;
                }

        echo "Submitting to database... ";


        $plugin_date = date("Y-m-d H:i:s", filemtime($file));
        $pt = $plugin_type;
        
        //Determine if we need to add or update
        if (DoesPluginExist($plugin_id, $plugin_version, $plugin_state) == false)
        {
                echo "<b>Adding new plugin...</b> ";
                $sql = "INSERT INTO plugins ( plugin_ID, plugin_packageid, plugin_Name, plugin_ShortDescription, plugin_LongDescription, plugin_Type, plugin_Licence, plugin_State, plugin_Date, plugin_Version, plugin_Author, plugin_AuthorEmail, profil_id, plugin_Copyright, plugin_SupportLink, plugin_DownloadLink, plugin_DonationLink, plugin_SourceLink, plugin_DocumentationFile, plugin_VersionValue, plugin_DocumentText, plugin_DownloadSize, plugin_DestinationFolder, plugin_Site, plugin_DirectDownloadURL, plugin_MinRequiredVersion, plugin_MaxRequiredVersion, plugin_IsDotNET, plugin_IsDotNETSimple, plugin_ReviewFlag, plugin_hash) ";
                $sql .=             "VALUES ('$plugin_id', '$plugin_packageid', '$plugin_name', '$plugin_description', '$plugin_LongDescription', '$pt', '$plugin_licence', '$plugin_state', '$plugin_date', '$plugin_version', '$plugin_author', '$plugin_AuthorEmail', '$u', '$plugin_Copyright', '$plugin_SupportLink', '$plugin_home_directory/$plugin_id/this.mpp', '$plugin_DonationLink', '$plugin_SourceLink', '$plugin_Documentation', '$plugin_VersionValue', '$plugin_DocumentationText', $file_size, '$plugin_DestinationFolder', '$plugin_Site', '$plugin_DirectDownloadURL', '$plugin_MinRequiredVersion', '$plugin_MaxRequiredVersion', '$plugin_IsDotNET', '$plugin_IsDotNETSimple', '$plugin_flagged', '$plugin_hash' )";
                 $res = @mysql_query($sql);
                
                if (!$res)
                {
                        echo ( "Error creating new plugin record: " . mysql_error()) ;
                        @unlink($pluginfile);
                        if ($delete_upload) @unlink($file);
                        echo "<br><br>Clearing Temporary directory...\n";
                        remove_directory($temp_directory);
                        echo "Done!<br>";
                        return false;
                }
        }
                else 
        {
                if ($force_update == FALSE) {
                        echo "<br /><b>Plugin already exists. (Version, State and ID match found in database)</b><br>";
                        echo "If you made changes to the plugin, you should increment the version number and/or change the state (alpha/beta/stable) and re-upload the plugin.<br>";
                        echo "If you are the owner of this plugin and would like to edit just the plugin information, click <a href='edit.php?id=$plugin_id'>here</a><br /><br />";
                        }
                if ($force_update == TRUE) {
                        echo "Plugin already exists...updating plugin record with new information.<br>";
                        echo "<b>Updating plugin database record...</b> ";
                        
                        $sql  = "UPDATE plugins SET plugin_ID='$plugin_id',plugin_packageid='$plugin_packageid', plugin_Name='$plugin_name', plugin_ShortDescription='$plugin_description', plugin_Type='$pt', ";
                        $sql .= "plugin_Licence='$plugin_licence', plugin_State='$plugin_state', plugin_Version='$plugin_version', plugin_Author='$plugin_author', ";
                        $sql .= "plugin_Copyright='$plugin_Copyright', plugin_SupportLink='$plugin_SupportLink', plugin_DocumentationFile='$plugin_Documentation', ";
                        $sql .= "plugin_VersionValue='$plugin_VersionValue', plugin_DocumentText='$plugin_DocumentationText', plugin_DownloadSize=$file_size, ";
                        $sql .= "plugin_DestinationFolder='$plugin_DestinationFolder', plugin_AuthorEmail='$plugin_AuthorEmail', plugin_hash='$plugin_hash', ";
                        $sql .= "plugin_ModuleType='$module_type', plugin_DonationLink='$plugin_DonationLink', plugin_SourceLink='$plugin_SourceLink', plugin_ReviewFlag='$plugin_flagged',";
                        $sql .= "plugin_Site='$plugin_Site', plugin_DirectDownloadURL='$plugin_DirectDownloadURL', plugin_MinRequiredVersion='$plugin_MinRequiredVersion', ";
                        $sql .= "plugin_MaxRequiredVersion='$plugin_MaxRequiredVersion', plugin_IsDotNET='$plugin_IsDotNET', plugin_IsDotNETSimple='$plugin_IsDotNETSimple' ";
                        $sql .= "WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version' AND plugin_State='$plugin_state'";
                        
                        $res = @mysql_query($sql);
                
                        if (!$res) {
                                echo ( "Error updating plugin record: " . mysql_error()) ;
                                return false;
                                }
                        
                        if (is_dir("$plugin_home_directory/$plugin_id/$plugin_version/$plugin_state")) 
                                remove_directory("$plugin_home_directory/$plugin_id/$plugin_version/$plugin_state");
                        }
        }

                
                UpdateCurrentPlugin($plugin_id);
                
                if ($res < 1) $res_text = "failed!";
                else $res_text = "passed!";
                
                echo " to ver " . $plugin_version . "... result = $res_text<br>\n";

                $plugin_key = GetPluginKey($plugin_id, $plugin_version,$plugin_state);

                echo "Plugin Key is $plugin_key <br>";

                $plugin_dest_directory = "$plugin_home_directory/$plugin_id/$plugin_version/$plugin_state" ;

                echo "Moving plugin into archive... ";
                // Test if plugin directory exist and create it else
                if (!is_dir($plugin_dest_directory))
                {
                        
                        if (mkpath("$plugin_dest_directory"))
                        {
                                echo "created new directory... ";
                        }
                        else
                        {
                                echo "Failed to create directory...rolling back...";
                                // mysql statements to remove db entry
                                $sql = "DELETE FROM plugins WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version' and plugin_State='$plugin_state'";
                                @mysql_query($sql); 
                                echo "Done!<br><br>\n";
                                @unlink($pluginfile);
                                if ($delete_upload) @unlink($file);
                                echo "Clearing Temporary directory...\n";
                                remove_directory($temp_directory);
                                echo "Done!<br>";
                                return false;
                        }
                }
                
                // Move plugin.xml file in the plugin directory
                if (!rename("$pluginfile", "$plugin_dest_directory/plugin.xml"))
                {
                        echo "<b>Error moving xml file</b><br><br>\n";
                        // mysql statements to remove db entry
                        $sql = "DELETE FROM plugins WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version' and plugin_State='$plugin_state'";
                        @mysql_query($sql); 
                        @unlink($pluginfile);
                        if ($delete_upload) @unlink($file);
                        echo "Removing plugin directory...\n";
                        remove_directory($plugin_dest_directory);
                        echo "Done!<br><br>";
                        echo "Clearing Temporary directory...\n";
                        remove_directory($temp_directory);
                        echo "Done!<br>";
                        return FALSE;
                }
                
                if ($delete_upload) {
                        // Move mpp file in the plugin directory if we are also deleting upload
                        if (!rename("$file", "$plugin_dest_directory/this.mpp"))
                        {
                                @unlink($file);
                                //remove the plugin.xml if the mpp cannot be moved
                                if (is_file ("$plugin_dest_directory/plugin.xml"))
                                {
                                        @unlink("$plugin_dest_directory/plugin.xml");
                                }
                                echo "<b>Error moving mpp file</b><br>\n";
                                // mysql statements to remove db entry
                                $sql = "DELETE FROM plugins WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version' and plugin_State='$plugin_state'";
                                @mysql_query($sql); 
                                @unlink($pluginfile);
                                if ($delete_upload) @unlink($file);
                                echo "Removing plugin directory...\n";
                                remove_directory($plugin_dest_directory);
                                echo "Done!<br><br>";
                                echo "Clearing Temporary directory...\n";
                                remove_directory($temp_directory);
                                echo "Done!<br>";
                                return FALSE;
                        }
                } else {
                        // Copy mpp file in the plugin directory if we want to keep original copy in place
                        if (!copy("$file", "$plugin_dest_directory/this.mpp"))
                        {
                                //remove the plugin.xml if the mpp cannot be copied
                                if (is_file ("$plugin_dest_directory/plugin.xml"))
                                {
                                        @unlink("$plugin_dest_directory/plugin.xml");
                                }
                                echo "<b>Error copying mpp file</b><br>\n";
                                // mysql statements to remove db entry
                                $sql = "DELETE FROM plugins WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version' and plugin_State='$plugin_state'";
                                @mysql_query($sql); 
                                @unlink($pluginfile);
                                if ($delete_upload) @unlink($file);
                                echo "Removing plugin directory...\n";
                                remove_directory($plugin_dest_directory);
                                echo "Done!<br><br>";
                                echo "Clearing Temporary directory...\n";
                                remove_directory($temp_directory);
                                echo "Done!<br>";
                                return FALSE;
                        }
                }


                echo "passed.<br>\n";


                echo "Processing screenshots... ";

                $screenshots = array();
                findFiles($screenshots, sql_regcase($temp_directory . "/*.jpg"));
                //findFiles($screenshots, sql_regcase($temp_directory . "/screenshots/*.jpg"));
                //findFiles($screenshots, sql_regcase($temp_directory . "/screenshots/*.jpeg"));

                //$screenshots=GetContents("/".$temp_directory . "/screenshots/");

                echo count($screenshots) . " screenshot(s) found.<br>";
                $i = 0;
                foreach($screenshots as $screenshot)
                {
                        $i++;

                        echo "Processing screenshot #$i...";

                        $newScreenshot = "$plugin_dest_directory/$i.jpg";
                        if (rename($screenshot, $newScreenshot))
                        {

                                echo "creating Thumbnail... ";
                                $thumbpath = @CreateThumbnail($newScreenshot);

                                if ($thumbpath != "")
                                {
                                        echo "thumbnail Created....";

                                        if (AssociateImage($plugin_id, $plugin_version, $newScreenshot, $thumbpath))
                                                echo "Inserted Screenshot #$i<br>";
                                        else
                                                echo "could not insert screenshot #$i! <br>" . mysql_error() ;
                                }
                                else
                                        echo "<b>Error creating thumbnail</b><br>\n";
                        }
                        else
                        {
                                echo "<b>Error copying image!</b><br>\n";
                        }
                }
                
                echo "Processing documentation...";
                $doc_file = $temp_directory . "/$plugin_Documentation";

                if (is_file($doc_file))
                {
                        echo "Documentation found, processing...";
                        
                        if (rename($doc_file,"$plugin_dest_directory/$plugin_Documentation" ))
                        {
                                echo "success!<br>";
                        }
                        else
                        {
                                echo "<b>failed to copy documentation!</b><br>";
                        }
                }
                else
                {
                        echo "Documentation not found!<br>";
                }

                @mysql_close();
                
                if ($process_multiple == FALSE) {
                        echo "Regenerating plugin summary cache files...";
                        regenerate_xtern_cache();
                        echo "Done.<br>\n";
                }

                echo "<b>SUCCESS!</b><br>\n";

        //Do some cleanup
        //Delete Temp data directory
        echo ("Clearing Temporary directory");
        remove_directory($temp_directory);

//TODO: send out notifications
//      mail("slug@skyforge.net", "AddMpp", "$u uploaded $plugin_Name version $plugin_Version id $plugin_id for profile $profil_id");

        return true;
}


function TestPluginFile($pluginfile, $u)
{

        global $plugin_name;
        global $plugin_version;
        global $plugin_description;
        global $plugin_author;
        global $plugin_type;
        global $module_type;
        global $plugin_licence;
        global $plugin_state;
        global $plugin_id;
        global $plugin_packageid;
        global $plugin_date;
        global $plugin_Copyright;
        global $plugin_SupportLink;
        global $plugin_Key;
        global $plugin_Documentation;
        global $plugin_DocumentationText;
        global $plugin_SupportLink;
        global $plugin_DonationLink;
        global $plugin_SourceLink;
        global $plugin_DestinationFolder;
        global $plugin_AuthorEmail;
        global $plugin_Site;
        global $plugin_DirectDownloadURL;
        global $plugin_MinRequiredVersion;
        global $plugin_MaxRequiredVersion;
        global $plugin_IsDotNET;
        global $plugin_IsDotNETSimple;   
        global $parser_failed;
        global $types;
        
        $ignore_sanity = FALSE;
        $parser_failed = FALSE;
        $pluginfile_error = FALSE;
        $plugin_flagged = "FALSE";
        $encoding_unknown = FALSE;
        $plugin_name = "";
        $plugin_version = "";
        $plugin_description = "";
        $plugin_author = "";
        $plugin_type = "";
        $module_type = "";
        $plugin_licence = "";
        $plugin_state = "";
        $plugin_id = "";
        $plugin_packageid = "";
        $plugin_date = "";
        $plugin_Copyright = "";
        $plugin_SupportLink = "";
        $plugin_Key = "";
        $plugin_Documentation = "";
        $plugin_DocumentationText = "";
        $plugin_SupportLink = "";
        $plugin_DonationLink = "";
        $plugin_SourceLink = "";
        $plugin_DestinationFolder = "";
        $plugin_AuthorEmail = "";
        $plugin_Site = "";
        $plugin_DirectDownloadURL = "";
        $plugin_MinRequiredVersion = "";
        $plugin_MaxRequiredVersion = "";
        $plugin_IsDotNET = "";
        $plugin_IsDotNETSimple = "";   
        
        echo "Checking .plugin file: $file<br>\n";

        echo "Checking uploaded file exists... ";
        if (!is_file($pluginfile))
        {
                echo "<b>File doesn't exist!</b><br><br>\n";
                return FALSE;
        }
        echo "passed.<br><br>\n";

        //try to determine the encoding type of the .plugin file or set to UTF-8 if not specified
        $rx = '/<?xml.*encoding=[\'"](.*?)[\'"].*?>/m';
        $xml_content = file_get_contents($pluginfile);
        if (preg_match($rx, $xml_content, $m)) {
          $encoding = strtoupper($m[1]);
        } else {
          $encoding = "UTF-8";
        }
        if (($encoding != "UTF-8") && ($encoding != "US-ASCII") && ($encoding != "ISO-8859-1")) {
                $specified_encoding = $encoding;
                $encoding = "UTF-8";
                $encoding_unknown = TRUE;
                }


        if ($encoding_unknown) echo "Parsing .plugin (your encoding '$specified_encoding' is not recognized, trying UTF-8 instead)...";
        else echo "Parsing .plugin (using $encoding)...";
        $Data = array();
        $xml_parser = xml_parser_create($encoding);
        xml_set_element_handler($xml_parser, "StartElement", "EndElement");
        xml_set_character_data_handler($xml_parser, "characterData");
        if (!($fp = fopen($pluginfile, "r"))) {
                echo "<b>Error opening .plugin file.</b><br><br>\n";
                @unlink($pluginfile);
                return FALSE;
        }
        while ($data = @fread($fp, 4096)) {
                if (!@xml_parse($xml_parser, $data, feof($fp))) {
                        $msg = xml_error_string(xml_get_error_code($xml_parser));
                        $line = xml_get_current_line_number($xml_parser);
                        echo "<b>Error in .plugin file: '$msg' line $line</b><br>\n";
                        $parser_failed = TRUE;
                }
        }
        xml_parser_free($xml_parser);
        fclose($fp);
        
        if ($parser_failed) {
                echo "We don't recognize the encoding type of your .plugin XML!<br>";
                echo "Be sure it is encoded with UTF-8, ISO-8859-1, or US-ASCII.<br>";
                echo "If you are sure it is encoded with one of these types, try specifying<br>";
                echo "the type within your .plugin file like this:<br><br>";
                echo "<?xml version=\"1.0\" encoding=\"$encoding\"?><br><br>";
                @unlink($pluginfile);
                return FALSE;
        }


        if ($plugin_id == "")
        {
                echo "<b>No valid plugin-id node was found.</b><br><br>\n";
                @unlink($pluginfile);
                return FALSE;
        }
        echo "passed.<br><br>\n";

        if ($encoding_unknown) {
                echo "We didn't recognize the encoding type (<b>$specified_encoding</b>) of your .plugin XML!<br>";
                echo "You should use UTF-8, ISO-8859-1, or US-ASCII encoding to<br>";
                echo "ensure OpenMAID accurately parses your plugin details.  If<br>";
                echo "you are certain you are using one of these recognized types,<br>";
                echo "you can specify the type within your .plugin file like this:<br><br>";
                echo htmlentities("<?xml version=\"1.0\" encoding=\"$encoding\"?>", ENT_QUOTES);
                echo "<br><br>";
        }

        //Checks plugin_type and plugin_moduletype.  Make sure entries are valid.  If module type exists and is valid,
        //it overrides the plugin_type value.
        $plugin_type = strtolower($plugin_type);
        $module_type = strtolower($module_type);
        echo "Checking for valid plugin type...$plugin_type...";
        $valid_plugin_type = IsPluginTypeValid($plugin_type, $types);
        if ($valid_plugin_type) echo "passed!<br><br>";
        else echo "The plugin type you specified ($plugin_type) is not valid! <br><br>";
        echo "Checking for valid module type...$module_type...";
        $moduletypes = array("icon", "misc", "hack", "web", "wizard", "sub");
        $valid_module_type = IsPluginTypeValid($module_type, $moduletypes);
        if ($valid_module_type || ($module_type && $plugin_type && $module_type == $plugin_type)) echo "passed!<br><br>";
        else {
                echo "The module type you specified ($module_type) is not valid! <br><br>";
                if (!$valid_plugin_type) {
                        echo "<b>Error</b>...The module AND plugin types you specified ($plugin_type/$module_type) are not valid!<br><br>";
                        $pluginfile_error = TRUE;
                        }
                else echo "Plugin type ($plugin_type) is valid, we will ignore the module type.<br><br>";               
                }
        if ($valid_module_type && $module_type != "none" && $module_type != "") {
                echo "Module type is valid.  Setting plugin type to <b>$module_type</b>...";
                $plugin_type = $module_type;
                echo "Done!<br><br>";
                }
        elseif (!$valid_module_type && $valid_plugin_type && $module_type != $plugin_type) {
                echo "Module type ($module_type) is invalid.  Assuming plugin type is <b>$plugin_type</b>...";
                echo "Done!<br><br>";
                }
        elseif (!$valid_module_type && $valid_plugin_type && $module_type == $plugin_type) {
                echo "Module and Plugin Types are the same.  Assuming plugin type is <b>$plugin_type</b>...";
                echo "Done!<br>";
                }

        //Checks plugin id to be sure it is unique and not conflicting with an existing plugin
        //Ties plugin id to destination directory and plugin type
        //If check fails, returns name of plugin conflicting plugin. If it passes, return 'passed'
        echo "Checking Plugin ID...$plugin_id...";
        $pluginID_passed = CheckForUniquePluginID($plugin_id, $plugin_type, $plugin_DestinationFolder);
        if ($pluginID_passed != "passed") {
                echo "<br><b>Error</b>...Your plugin ID (GUID) conflicts with another plugin: <br>";
                echo "<b>$pluginID_passed</b><br>";
                echo "This is an unrecoverable error. You must give your plugin a new GUID and retry<br>";
                echo "the upload again to continue testing this .plugin file.  If you feel this error<br>";
                echo "is incorrect, PM your .plugin file to the OpenMAID admin and ask why it is failing<br>";
                echo "the Plugin ID (GUID) validation check.<br><br>\n";
                echo ("Removing .plugin upload...");
                @unlink($pluginfile);
                echo "Done!<br><br>\n";
                return FALSE;
                }
        else echo "passed!<br><br>";

        //Checks plugin type to be sure user isn't trying to upload plugin with a new plugin type
        //If check fails, return name of plugin type of last uploaded version. If it passes, return 'passed'
        echo "Checking Plugin Type...";
        $pt_passed = CheckPluginTypeUpload($plugin_id, $plugin_type);
        if ($pt_passed != "passed") {
                echo "<br><b>Error</b>...You cannot change the plugin type from <b>$plugin_type</b> to <b>$pt_passed</b>!<br>";
                echo "To change plugin type, you must make it a new plugin by giving it a new GUID. <br><br>\n";
                $pluginfile_error = TRUE;
                }
        else echo "$plugin_type...passed!<br><br>";
        
        //Checks plugin author to be sure user isn't trying to upload someone else's plugin
        //If check fails, return name of plugin author profil_id of last uploaded version. If it passes, return 'passed'
        echo "Checking plugin profile ID...";
        $pid_passed = CheckProfileIDUpload($plugin_id, $plugin_author, $u);
        $pid = explode(":::",$pid_passed);
        $pid_passed = $pid[0];
        $pid_reason = $pid[1];
        if ($pid_passed == "unknown") {
                echo "<b>failed!</b><br>";
                echo "<b>Error</b>...$pid_reason";
                echo "<b>Please report this unknown condition data to the OpenMaid admin!</b><br><br>";
                $pluginfile_error = TRUE;
                }
        elseif ($pid_passed == "flagged") {
                echo "...flagged for admin review.<br>";
                echo "Flag Reason: $pid_reason<br>";
                echo "Flagged means your plugin will not be displayed in OpenMAID until it has been reviewed by an admin.<br><br>";
                $plugin_flagged = "TRUE";
                }
        elseif ($pid_passed == "passed") echo "plugin profile ID is set to $u ...passed.<br><br>";
        else {
                echo "Unknown condition:<br>1 - $plugin_id <br> 2 - $pid_passed <br> 3 - $ignore_sanity <br> 4 - $u <br> 5 - $plugin_author <br>";
                echo "<b>Please report this unknown condition data to the OpenMaid admin!</b><br><br>";
                }

        //Check for multiple destination folders with same GUID.  Destination folder shouldn't change from version to version. (robogeek)
        //Pass returns 'passed', <previous destination folder> = failed check
        echo "Checking for changed destination folder...";
        $df_multiple = CheckForMultipleDestinationFolders($plugin_id, $plugin_DestinationFolder);
        if ($df_multiple != "passed") {
                echo "<b>Error</b>...<br>Your destination folder has changed from a previous version!<br>";
                echo "This is not allowed.  You should change the destination folder back to what it was <br>";
                echo "or give this plugin a new GUID and name and try uploading it again.<br>";
                echo "Uploaded plugin destination folder:<b> $plugin_DestinationFolder </b><br>";
                echo "Previous plugin destination folder:<b> $df_multiple </b><br><br>\n";
                $pluginfile_error = TRUE;
                }
        else echo "passed!<br><br>";
        
        //Check destination folder for conflicts with other plugins.  Should be a unique destination folder. (robogeek)
        //Pass returns 'passed', <plugin name> = conflict
        echo "Checking for destination folder conflicts...";
        $df_passed = CheckForDupeDestinationFolder($plugin_id, $plugin_type, $plugin_DestinationFolder);
        if ($df_passed != "passed") {
                echo "<b>Error</b>...<br>Your destination folder conflicts with an existing plugin:<br> <b>$df_passed</b><br>";
                echo "This is not allowed.  You should change the destination folder back to what it was <br>";
                echo "or give this plugin a new GUID and name and try uploading it again.<br><br>\n";
                $pluginfile_error = TRUE;
                }
        else echo "passed!<br><br>";

        //Version checking (robogeek)
        echo "Checking version numbering format...<br>";
        //calculate uploaded plugin version value
        $plugin_VersionValue = ltrim(CalculateVersionValue($plugin_version),"0");
        if ($plugin_VersionValue == -1) {
                echo "<b>Error</b>...version format check failed.<br><br>";
                $pluginfile_error = TRUE;
                }
        else echo "Version numbering format passed!<br><br>";
                
        //get most recent plugin version value
        $previous_versionvalue = GetLatestVersionForState($plugin_id,"ALL");
        $previous_version = GetLatestVersionForState2($plugin_id,"ALL");
        if ($previous_versionvalue < 0) $previous_versionvalue = 0;
        echo "Uploaded version: $plugin_version ($plugin_VersionValue)<br>";
        echo "Previous version: ";
        if ($previous_version < 0) echo "N/A<br><br>";
        else echo "$previous_version ($previous_versionvalue)<br><br>";

                //test to see if uploaded version is newer than most recent plugin in database
                //throw error if version is not newer and exit
                //suggest valid version number based on most recent version of plugin in database (todo)
        echo "Checking uploaded version number (must be a newer version number)...";
        if ($plugin_VersionValue <= $previous_versionvalue)  {
                echo "<b>Error</b><br><b>Version Error!</b>  The uploaded version is not newer than the most recent version<br>";
                echo "in our database.  You can only upload versions that are newer. <br><br>";
                $pluginfile_error = TRUE;
                }
        else echo "passed!<br><br>";
        
        // Delete plugin file
        echo ("Removing .plugin upload...");
        @unlink($pluginfile);
        echo "Done!<br><br>\n";
        
        if ($pluginfile_error) return false;
        else return true;
}


function scan_directory_recursively($directory, $filter=FALSE)
 {
     // if the path has a slash at the end we remove it here
     if(substr($directory,-1) == '/')
     {
         $directory = substr($directory,0,-1);
     }

     // if the path is not valid or is not a directory ...
     if(!file_exists($directory) || !is_dir($directory))
     {
         // ... we return false and exit the function
         return FALSE;

     // ... else if the path is readable
     }elseif(is_readable($directory))
     {
         // we open the directory
         $directory_list = opendir($directory);

         // and scan through the items inside
         while (FALSE !== ($file = readdir($directory_list)))
         {
             // if the filepointer is not the current directory
             // or the parent directory
             if($file != '.' && $file != '..')
             {
                 // we build the new path to scan
                 $path = $directory.'/'.$file;

                 // if the path is readable
                 if(is_readable($path))
                 {
                     // we split the new path by directories
                     $subdirectories = explode('/',$path);

                     // if the new path is a directory
                     if(is_dir($path))
                     {
                         // add the directory details to the file list
                         $directory_tree[] = array(
                             'path'    => $path,
                             'name'    => end($subdirectories),
                             'kind'    => 'directory',

                             // we scan the new path by calling this function
                             'content' => scan_directory_recursively($path, $filter));

                     // if the new path is a file
                     }elseif(is_file($path))
                     {
                         // get the file extension by taking everything after the last dot
                         $extension = end(explode('.',end($subdirectories)));

                         // if there is no filter set or the filter is set and matches
                         if($filter === FALSE || $filter == $extension)
                         {
                             // add the file details to the file list
                             $directory_tree[] = array(
                                 'path'      => $path,
                                 'name'      => end($subdirectories),
                                 'extension' => $extension,
                                 'size'      => filesize($path),
                                 'kind'      => 'file');
                         }
                     }
                 }
             }
         }
         // close the directory
         closedir($directory_list);

         // return file list
         if (!isset($directory_tree)) return NULL;
         return $directory_tree;

     // if the path is not readable ...
     }else{
         // ... we return false
         return FALSE;
     }
 }

function formatFileSize($sizeBytes)
{
        if ($sizeBytes > 1024)
        {
                $sizeKBytes = $sizeBytes/1024;

                if ($sizeKBytes > 1024)
                {
                        $sizeMBytes = $sizeKBytes /1024;

                        return round($sizeMBytes,0) . "MB";
                }
                else
                {
                        return round($sizeKBytes,0) . "KB";
                }
        }
        else
        {
                return $sizeBytes . " Bytes";
        }
}

function formatPluginID($plugin_id)
{
        // removed curly braces checking dukus
        //if ((substr($plugin_id,0,1) != "{") && (substr($plugin_id,-1,1) != "}")) $plugin_id = "{".$plugin_id."}";

        return $plugin_id;
}


function replaceLinks($text)
{
        $text = eregi_replace('((((f|ht){1}(tp://|tps://))|(svn://))[-a-zA-Z0-9@:%_\+.;~#?&//=]+)', '<a href="\\1">CLICK HERE</a>', $text);
        $text = eregi_replace('(^www.[-a-zA-Z0-9@:%_\+.~#?&//=]+)', '<a href="http://\\1">\\1</a>', $text);
        $text = eregi_replace('([[:space:]()[{}])(www.[-a-zA-Z0-9@:%_\+.~#?&//=]+)', '<a href="http://\\2">\\2</a>', $text);
        $text = eregi_replace('([_\.0-9a-z-]+@([0-9a-z][0-9a-z-]+\.)+[a-z]{2,3})', '<a href="mailto:\\1">\\1</a>', $text);
        return $text;
}

function xtern_summary($gen_cache){
        global $sys_url;
        $summary_cache_tempfile = "summary.cache.temp";
        $summary_cache_file = "summary.cache";
        $send_gen_cache = FALSE;

        //when gen_cache = FALSE, check for cache file.  If file exists, echo it's contents and exit.
        //If file doesn't exist, set gen_cache = TRUE and continue to generate the cache file and echo
        //it's contents.
        if (!$gen_cache) {
                if (file_exists($summary_cache_file)) echo file_get_contents($summary_cache_file);
                else $send_gen_cache = TRUE;
        }

        //if we need to generate a cache file (gen_cache=TRUE), start creating a new temp cache file
        //once we have the temp cache file, delete the old cache file and rename the temp cache file
        if ($gen_cache || $send_gen_cache) {
                if (file_exists($summary_cache_tempfile)) unlink($summary_cache_tempfile) or die("Error!  Temp cache file is in use, try again later.");
                $filehandle = fopen($summary_cache_tempfile, 'w') or die("Error opening temp cache file: $summary_cache_tempfile");


                fwrite($filehandle, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n");
                fwrite($filehandle, "<plugins>\n");
                fwrite($filehandle, "  <appversion>1.41</appversion>\n");

                global $db_host;
                global $db_user;
                global $db_pass;
                global $db_database;

                @mysql_connect($db_host, $db_user, $db_pass) or die("Error");
                @mysql_select_db($db_database) or die("Error");

                //$sql = "SELECT * FROM plugins ORDER BY plugin_Name";
                $sql = "SELECT plugin_ID, max(plugin_VersionValue) as max_VersionValue, max(plugin_Date) as max_Date FROM plugins WHERE plugin_Type='Module' OR plugin_Type='Theme' OR plugin_Type='Import' OR plugin_Type='Input' OR plugin_Type='General' AND plugin_ReviewFlag='FALSE' GROUP BY plugin_ID";
                $preres = mysql_query($sql);

                while (($prereg=@mysql_fetch_array($preres))) {
                //$sql = "SELECT * FROM plugins WHERE plugin_ID='" . $prereg["plugin_ID"] . "' and plugin_VersionValue='" . $prereg["max_VersionValue"] . "' and plugin_Date='" . $prereg["max_Date"] . "' ORDER BY plugin_Date DESC";
                $sql = "SELECT * FROM plugins WHERE plugin_ID='" . $prereg["plugin_ID"] . "' and plugin_VersionValue='" . $prereg["max_VersionValue"] . "' ORDER BY plugin_Date DESC";
                $res = mysql_query($sql);

                        while (($enreg=@mysql_fetch_array($res))) {
                                $plugin_id = utf8_encode(htmlspecialchars($enreg["plugin_ID"]));
                                $plugin_name = utf8_encode(htmlspecialchars($enreg["plugin_Name"]));
                                $plugin_version = utf8_encode(htmlspecialchars($enreg["plugin_Version"]));
                                $plugin_state = utf8_encode(htmlspecialchars($enreg["plugin_State"]));
                                $plugin_type = utf8_encode(htmlspecialchars($enreg["plugin_Type"]));

                                //Added the following $plugin_ values (robogeek)
                                $plugin_key = utf8_encode(htmlspecialchars($enreg["plugin_key"]));
                                $plugin_versionvalue = utf8_encode(htmlspecialchars($enreg["plugin_VersionValue"]));
                                $plugin_license = utf8_encode(htmlspecialchars($enreg["plugin_Licence"]));
                                $plugin_date = utf8_encode(htmlspecialchars($enreg["plugin_Date"]));
                                $plugin_downloadsize = utf8_encode(htmlspecialchars($enreg["plugin_DownloadSize"]));
                                $plugin_hash = utf8_encode(htmlspecialchars($enreg["plugin_hash"]));
                                $plugin_LongDescription = utf8_encode(htmlspecialchars($enreg["plugin_LongDescription"]));
                                $plugin_AuthorEmail = utf8_encode(htmlspecialchars($enreg["plugin_AuthorEmail"]));
                                $plugin_Copyright = utf8_encode(htmlspecialchars($enreg["plugin_Copyright"]));
                                $plugin_SupportLink = utf8_encode(htmlspecialchars($enreg["plugin_SupportLink"]));
                                $plugin_DonationLink = utf8_encode(htmlspecialchars($enreg["plugin_DonationLink"]));
                                $plugin_SourceLink = utf8_encode(htmlspecialchars($enreg["plugin_SourceLink"]));
                                $plugin_DocumentationFile = utf8_encode(htmlspecialchars($enreg["plugin_DocumentationFile"]));
                                //$plugin_DocumentText = utf8_encode(htmlspecialchars($enreg["plugin_DocumentText"]));
                                $plugin_Site = utf8_encode(htmlspecialchars($enreg["plugin_Site"]));
                                $plugin_DirectDownloadURL = utf8_encode(htmlspecialchars($enreg["plugin_DirectDownloadURL"]));
                                $plugin_MinRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MinRequiredVersion"]));
                                $plugin_MaxRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MaxRequiredVersion"]));
                                $plugin_IsDotNET = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNET"]));
                                $plugin_IsDotNETSimple = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNETSimple"]));
                                $plugin_DestinationFolder = utf8_encode(htmlspecialchars($enreg["plugin_DestinationFolder"]));


                                //changed this to get download count from userdownloads table (robogeek)
                                $sql = "SELECT * FROM userdownloads WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version'";
                                $resD = mysql_query($sql);
                                $resD_rows = mysql_numrows($resD);
                                $plugin_downloads = utf8_encode(htmlspecialchars($resD_rows));

                                //added plugin popularity (robogeek)
                                $plugin_popularity = GetPopularity($plugin_id);

                                //not implemented yet?
                                $plugin_trusted = utf8_encode(htmlspecialchars("unknown"));

                                $sql = "SELECT * FROM votesWorking WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version'";
                                $resV = mysql_query($sql);
                                $nbV = mysql_numrows($resV);
                                $plugin_works = "unknown";
                                $enregNbV = 0;
                                $yes = 0;
                                $no = 0;
                                while (($enregNbV<$nbV)&&($enregV=@mysql_fetch_array($resV)))
                                {
                                        if ($enregV["vote"] == "true") $yes++;
                                        else $no++;
                                }
                                if ($yes == 0 && $no == 0) $plugin_works = "unknown";
                                elseif ($yes > $no) $plugin_works = "true";
                                elseif ($no > $yes) $plugin_works = "false";
                                elseif ($no == $yes) $plugin_works = "undecided";

                                $plugin_desc = utf8_encode(htmlspecialchars($enreg["plugin_ShortDescription"]));
                                $plugin_author = utf8_encode(htmlspecialchars($enreg["plugin_Author"]));
                                if (isset($enreg["profil_id"]) && $enreg["profil_id"] != "") $plugin_author = $plugin_author . " (" . htmlspecialchars($enreg["profil_id"]) . ")";

                                fwrite($filehandle, "   <plugin id=\"$plugin_id\" version=\"$plugin_version\" type=\"$plugin_type\" state=\"$plugin_state\" name=\"$plugin_name\" ");
                                fwrite($filehandle, "url=\"" . $sys_url . "dl.php?plugin_id=$plugin_id\" versionvalue=\"$plugin_versionvalue\" size=\"$plugin_downloadsize\" date=\"$plugin_date\" ");
                                fwrite($filehandle, "license=\"$plugin_license\" downloads=\"$plugin_downloads\" popularity=\"$plugin_popularity\" trusted=\"$plugin_trusted\" works=\"$plugin_works\" ");
                                fwrite($filehandle, "desc=\"$plugin_desc\" longdesc=\"$plugin_LongDescription\" author=\"$plugin_author\" authoremail=\"$plugin_AuthorEmail\" copyright=\"$plugin_Copyright\" ");
                                fwrite($filehandle, "supportlink=\"$plugin_SupportLink\" donationlink=\"$plugin_DonationLink\" sourcelink=\"$plugin_SourceLink\" documentationfile=\"$plugin_DocumentationFile\" ");
                                fwrite($filehandle, "website=\"$plugin_Site\" directdownloadurl=\"$plugin_DirectDownloadURL\" minreqversion=\"$plugin_MinRequiredVersion\" ");
                                fwrite($filehandle, "maxreqversion=\"$plugin_MaxRequiredVersion\" isdotnet=\"$plugin_IsDotNET\" isdotnetsimple=\"$plugin_IsDotNETSimple\" destinationfolder=\"$plugin_DestinationFolder\" hash=\"$plugin_hash\" />\n");
                        }
                }
                fwrite($filehandle, "</plugins>\n");

                //once we have the temp cache file, delete the old cache file and rename the temp cache file
                if (!file_exists($summary_cache_tempfile)) die("Error!  Temp cache file wasn't created.");
                if (file_exists($summary_cache_file)) unlink($summary_cache_file);
                rename($summary_cache_tempfile, $summary_cache_file);
                fclose($filehandle);
                if ($send_gen_cache) echo file_get_contents($summary_cache_file);
                }
}


function xtern_extended($gen_cache){
        global $sys_url;
        $extended_cache_tempfile = "extended.cache.temp";
        $extended_cache_file = "extended.cache";
        $send_gen_cache = FALSE;

        //when gen_cache = FALSE, check for cache file.  If file exists, echo it's contents and exit.
        //If file doesn't exist, set gen_cache = TRUE and continue to generate the cache file and echo
        //it's contents.
        if (!$gen_cache) {
                if (file_exists($extended_cache_file)) echo file_get_contents($extended_cache_file);
                else $send_gen_cache = TRUE;
        }
        
        //if we need to generate a cache file (gen_cache=TRUE), start creating a new temp cache file
        //once we have the temp cache file, delete the old cache file and rename the temp cache file
        if ($gen_cache || $send_gen_cache) {
                if (file_exists($extended_cache_tempfile)) unlink($extended_cache_tempfile) or die("Error!  Temp cache file is in use, try again later.");
                $filehandle = fopen($extended_cache_tempfile, 'w') or die("Error opening temp cache file: $extended_cache_tempfile");
        
        
                fwrite($filehandle, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n");
                fwrite($filehandle, "<plugins>\n");
                fwrite($filehandle, "  <appversion>1.41</appversion>\n");
        
                global $db_host;
                global $db_user;
                global $db_pass;
                global $db_database;
                $state = array('Stable', 'Beta', 'Alpha');
        
                @mysql_connect($db_host, $db_user, $db_pass) or die("Error");
                @mysql_select_db($db_database) or die("Error");
        
                foreach ($state as $stateValue){
                        $sql  = "SELECT *, max(plugin_Date) FROM plugins ";
                        $sql .= "WHERE (plugin_Type='Module' OR plugin_Type='Theme' OR plugin_Type='Import' OR plugin_Type='Input' OR plugin_Type='General') AND plugin_State='$stateValue' AND plugin_ReviewFlag='FALSE' ";
                        $sql .= "GROUP BY plugin_ID ORDER BY plugin_Name ASC";
                        $res = mysql_query($sql);
        
                        while (($enreg=@mysql_fetch_array($res))) {
                                $plugin_id = utf8_encode(htmlspecialchars($enreg["plugin_ID"]));
                                $plugin_name = utf8_encode(htmlspecialchars($enreg["plugin_Name"]));
                                $plugin_version = utf8_encode(htmlspecialchars($enreg["plugin_Version"]));
                                $plugin_state = utf8_encode(htmlspecialchars($enreg["plugin_State"]));
                                $plugin_type = utf8_encode(htmlspecialchars($enreg["plugin_Type"]));
                                
                                //Added the following $plugin_ values (robogeek)
                                $plugin_key = utf8_encode(htmlspecialchars($enreg["plugin_key"]));
                                $plugin_versionvalue = utf8_encode(htmlspecialchars($enreg["plugin_VersionValue"]));
                                $plugin_license = utf8_encode(htmlspecialchars($enreg["plugin_Licence"]));
                                $plugin_date = utf8_encode(htmlspecialchars($enreg["plugin_Date"]));
                                $plugin_downloadsize = utf8_encode(htmlspecialchars($enreg["plugin_DownloadSize"]));
                                $plugin_hash = utf8_encode(htmlspecialchars($enreg["plugin_hash"]));
                                $plugin_LongDescription = utf8_encode(htmlspecialchars($enreg["plugin_LongDescription"]));
                                $plugin_AuthorEmail = utf8_encode(htmlspecialchars($enreg["plugin_AuthorEmail"]));
                                $plugin_Copyright = utf8_encode(htmlspecialchars($enreg["plugin_Copyright"]));
                                $plugin_SupportLink = utf8_encode(htmlspecialchars($enreg["plugin_SupportLink"]));
                                $plugin_DonationLink = utf8_encode(htmlspecialchars($enreg["plugin_DonationLink"]));
                                $plugin_SourceLink = utf8_encode(htmlspecialchars($enreg["plugin_SourceLink"]));
                                $plugin_DocumentationFile = utf8_encode(htmlspecialchars($enreg["plugin_DocumentationFile"]));
                                $plugin_DocumentText = utf8_encode(htmlspecialchars($enreg["plugin_DocumentText"]));
                                $plugin_Site = utf8_encode(htmlspecialchars($enreg["plugin_Site"]));
                                $plugin_DirectDownloadURL = utf8_encode(htmlspecialchars($enreg["plugin_DirectDownloadURL"]));
                                $plugin_MinRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MinRequiredVersion"]));
                                $plugin_MaxRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MaxRequiredVersion"]));
                                $plugin_IsDotNET = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNET"]));
                                $plugin_IsDotNETSimple = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNETSimple"]));
                                $plugin_DestinationFolder = utf8_encode(htmlspecialchars($enreg["plugin_DestinationFolder"]));
                                
        
                                //changed this to get download count from userdownloads table (robogeek)
                                $sql = "SELECT * FROM userdownloads WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version'";
                                $resD = mysql_query($sql);
                                $resD_rows = mysql_numrows($resD);
                                $plugin_downloads = utf8_encode(htmlspecialchars($resD_rows));
                        
                                //added plugin popularity (robogeek)
                                $plugin_popularity = GetPopularity($plugin_id);
                
                                //not implemented yet?
                                $plugin_trusted = utf8_encode(htmlspecialchars("unknown"));
                                $plugin_works = "unknown";
        
                                $sql = "SELECT * FROM votesWorking WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version'";
                                $resV = mysql_query($sql);
                                $nbV = mysql_numrows($resV);
                                $enregNbV = 0;
                                $yes = 0;
                                $no = 0;
                                while (($enregNbV<$nbV)&&($enregV=@mysql_fetch_array($resV)))
                                {
                                        if ($enregV["vote"] == "true") $yes++;
                                        else $no++;
                                }
                                if ($yes == 0 && $no == 0) $plugin_works = "unknown";
                                elseif ($yes > $no) $plugin_works = "true";
                                elseif ($no > $yes) $plugin_works = "false";
                                elseif ($no == $yes) $plugin_works = "undecided";
                
                                $plugin_desc = utf8_encode(htmlspecialchars($enreg["plugin_ShortDescription"]));
                                $plugin_author = utf8_encode(htmlspecialchars($enreg["plugin_Author"]));
                                if (isset($enreg["profil_id"]) && $enreg["profil_id"] != "") $plugin_author = $plugin_author . " (" . htmlspecialchars($enreg["profil_id"]) . ")";
                
                                fwrite($filehandle, "   <plugin id=\"$plugin_id\" version=\"$plugin_version\" type=\"$plugin_type\" state=\"$plugin_state\" name=\"$plugin_name\" ");
                                fwrite($filehandle, "url=\"" . $sys_url . "dl.php?plugin_id=$plugin_id&amp;plugin_version=$plugin_version&amp;plugin_state=$plugin_state\" versionvalue=\"$plugin_versionvalue\" size=\"$plugin_downloadsize\" date=\"$plugin_date\" ");
                                fwrite($filehandle, "license=\"$plugin_license\" downloads=\"$plugin_downloads\" popularity=\"$plugin_popularity\" trusted=\"$plugin_trusted\" works=\"$plugin_works\" ");
                                fwrite($filehandle, "desc=\"$plugin_desc\" longdesc=\"$plugin_LongDescription\" author=\"$plugin_author\" authoremail=\"$plugin_AuthorEmail\" copyright=\"$plugin_Copyright\" ");
                                fwrite($filehandle, "supportlink=\"$plugin_SupportLink\" donationlink=\"$plugin_DonationLink\" sourcelink=\"$plugin_SourceLink\" documentationfile=\"$plugin_DocumentationFile\" ");
                                fwrite($filehandle, "documenttext=\"$plugin_DocumentText\" website=\"$plugin_Site\" directdownloadurl=\"$plugin_DirectDownloadURL\" minreqversion=\"$plugin_MinRequiredVersion\" ");
                                fwrite($filehandle, "maxreqversion=\"$plugin_MaxRequiredVersion\" isdotnet=\"$plugin_IsDotNET\" isdotnetsimple=\"$plugin_IsDotNETSimple\" destinationfolder=\"$plugin_DestinationFolder\" hash=\"$plugin_hash\" />\n");
                        }
                }
                fwrite($filehandle, "</plugins>\n");
        
                //once we have the temp cache file, delete the old cache file and rename the temp cache file
                if (!file_exists($extended_cache_tempfile)) die("Error!  Temp cache file wasn't created.");
                if (file_exists($extended_cache_file)) unlink($extended_cache_file);
                rename($extended_cache_tempfile, $extended_cache_file);
                fclose($filehandle);
                if ($send_gen_cache) echo file_get_contents($extended_cache_file);
                }
}


function xtern_all($gen_cache){
        global $sys_url;
        $all_cache_tempfile = "all.cache.temp";
        $all_cache_file = "all.cache";
        $send_gen_cache = FALSE;

        //when gen_cache = FALSE, check for cache file.  If file exists, echo it's contents and exit.
        //If file doesn't exist, set gen_cache = TRUE and continue to generate the cache file and echo
        //it's contents.
        if (!$gen_cache) {
                if (file_exists($all_cache_file)) echo file_get_contents($all_cache_file);
                else $send_gen_cache = TRUE;
        }
        
        //if we need to generate a cache file (gen_cache=TRUE), start creating a new temp cache file
        //once we have the temp cache file, delete the old cache file and rename the temp cache file
        if ($gen_cache || $send_gen_cache) {
                if (file_exists($all_cache_tempfile)) unlink($all_cache_tempfile) or die("Error!  Temp cache file is in use, try again later.");
                $filehandle = fopen($all_cache_tempfile, 'w') or die("Error opening temp cache file: $all_cache_tempfile");
        
        
                fwrite($filehandle, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n");
                fwrite($filehandle, "<plugins>\n");
                fwrite($filehandle, "  <appversion>1.41</appversion>\n");
        
                global $db_host;
                global $db_user;
                global $db_pass;
                global $db_database;
        
                @mysql_connect($db_host, $db_user, $db_pass) or die("Error");
                @mysql_select_db($db_database) or die("Error");

                //$sql = "SELECT * FROM plugins ORDER BY plugin_Name";
                $sql = "SELECT * FROM plugins WHERE plugin_Type='Module' OR plugin_Type='Theme' OR plugin_Type='Import' OR plugin_Type='Input' OR plugin_Type='General' AND plugin_ReviewFlag='FALSE' ORDER BY plugin_Name ASC";
                $res = mysql_query($sql);
        
                while (($enreg=@mysql_fetch_array($res))) {
                        $plugin_id = utf8_encode(htmlspecialchars($enreg["plugin_ID"]));
                        $plugin_packageid = utf8_encode(htmlspecialchars($enreg["plugin_packageid"]));
                        $plugin_name = utf8_encode(htmlspecialchars($enreg["plugin_Name"]));
                        $plugin_version = utf8_encode(htmlspecialchars($enreg["plugin_Version"]));
                        $plugin_state = utf8_encode(htmlspecialchars($enreg["plugin_State"]));
                        $plugin_type = utf8_encode(htmlspecialchars($enreg["plugin_Type"]));

                        //Added the following $plugin_ values (robogeek)
                        $plugin_key = utf8_encode(htmlspecialchars($enreg["plugin_key"]));
                        $plugin_versionvalue = utf8_encode(htmlspecialchars($enreg["plugin_VersionValue"]));
                        $plugin_license = utf8_encode(htmlspecialchars($enreg["plugin_Licence"]));
                        $plugin_date = utf8_encode(htmlspecialchars($enreg["plugin_Date"]));
                        $plugin_downloadsize = utf8_encode(htmlspecialchars($enreg["plugin_DownloadSize"]));
                        $plugin_hash = utf8_encode(htmlspecialchars($enreg["plugin_hash"]));
                        $plugin_LongDescription = utf8_encode(htmlspecialchars($enreg["plugin_LongDescription"]));
                        $plugin_AuthorEmail = utf8_encode(htmlspecialchars($enreg["plugin_AuthorEmail"]));
                        $plugin_Copyright = utf8_encode(htmlspecialchars($enreg["plugin_Copyright"]));
                        $plugin_SupportLink = utf8_encode(htmlspecialchars($enreg["plugin_SupportLink"]));
                        $plugin_DonationLink = utf8_encode(htmlspecialchars($enreg["plugin_DonationLink"]));
                        $plugin_SourceLink = utf8_encode(htmlspecialchars($enreg["plugin_SourceLink"]));
                        $plugin_DocumentationFile = utf8_encode(htmlspecialchars($enreg["plugin_DocumentationFile"]));
                        $plugin_DocumentText = utf8_encode(htmlspecialchars($enreg["plugin_DocumentText"]));
                        $plugin_Site = utf8_encode(htmlspecialchars($enreg["plugin_Site"]));
                        $plugin_DirectDownloadURL = utf8_encode(htmlspecialchars($enreg["plugin_DirectDownloadURL"]));
                        $plugin_MinRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MinRequiredVersion"]));
                        $plugin_MaxRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MaxRequiredVersion"]));
                        $plugin_IsDotNET = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNET"]));
                        $plugin_IsDotNETSimple = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNETSimple"]));
                        $plugin_DestinationFolder = utf8_encode(htmlspecialchars($enreg["plugin_DestinationFolder"]));
                        
        
                        //changed this to get download count from userdownloads table (robogeek)
                        $sql = "SELECT * FROM userdownloads WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version'";
                        $resD = mysql_query($sql);
                        $resD_rows = mysql_numrows($resD);
                        $plugin_downloads = utf8_encode(htmlspecialchars($resD_rows));
                        
                        //added plugin popularity (robogeek)
                        $plugin_popularity = GetPopularity($plugin_id);
        
                        //not implemented yet?
                        $plugin_trusted = utf8_encode(htmlspecialchars("unknown"));
        
                        $sql = "SELECT * FROM votesWorking WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version'";
                        $resV = mysql_query($sql);
                        $nbV = mysql_numrows($resV);
                        $plugin_works = "unknown";
                        $enregNbV = 0;
                        $yes = 0;
                        $no = 0;
                        while (($enregNbV<$nbV)&&($enregV=@mysql_fetch_array($resV)))
                        {
                                if ($enregV["vote"] == "true") $yes++;
                                else $no++;
                        }
                        if ($yes == 0 && $no == 0) $plugin_works = "unknown";
                        elseif ($yes > $no) $plugin_works = "true";
                        elseif ($no > $yes) $plugin_works = "false";
                        elseif ($no == $yes) $plugin_works = "undecided";
        
                        $plugin_desc = utf8_encode(htmlspecialchars($enreg["plugin_ShortDescription"]));
                        $plugin_author = utf8_encode(htmlspecialchars($enreg["plugin_Author"]));
                        if (isset($enreg["profil_id"]) && $enreg["profil_id"] != "") $plugin_author = $plugin_author . " (" . htmlspecialchars($enreg["profil_id"]) . ")";

                        fwrite($filehandle, "   <plugin id=\"$plugin_id\" packageid=\"$plugin_packageid\" version=\"$plugin_version\" type=\"$plugin_type\" state=\"$plugin_state\" name=\"$plugin_name\" ");
                        fwrite($filehandle, "url=\"" . $sys_url . "dl.php?plugin_id=$plugin_id&amp;plugin_version=$plugin_version&amp;plugin_state=$plugin_state\" versionvalue=\"$plugin_versionvalue\" size=\"$plugin_downloadsize\" date=\"$plugin_date\" ");
                        fwrite($filehandle, "license=\"$plugin_license\" downloads=\"$plugin_downloads\" popularity=\"$plugin_popularity\" trusted=\"$plugin_trusted\" works=\"$plugin_works\" ");
                        fwrite($filehandle, "desc=\"$plugin_desc\" longdesc=\"$plugin_LongDescription\" author=\"$plugin_author\" authoremail=\"$plugin_AuthorEmail\" copyright=\"$plugin_Copyright\" ");
                        fwrite($filehandle, "supportlink=\"$plugin_SupportLink\" donationlink=\"$plugin_DonationLink\" sourcelink=\"$plugin_SourceLink\" documentationfile=\"$plugin_DocumentationFile\" ");
                        fwrite($filehandle, "documenttext=\"$plugin_DocumentText\" website=\"$plugin_Site\" directdownloadurl=\"$plugin_DirectDownloadURL\" minreqversion=\"$plugin_MinRequiredVersion\" ");
                        fwrite($filehandle, "maxreqversion=\"$plugin_MaxRequiredVersion\" isdotnet=\"$plugin_IsDotNET\" isdotnetsimple=\"$plugin_IsDotNETSimple\" destinationfolder=\"$plugin_DestinationFolder\" hash=\"$plugin_hash\" />\n");
                        }
                        
                fwrite($filehandle, "</plugins>\n");
        
                //once we have the temp cache file, delete the old cache file and rename the temp cache file
                if (!file_exists($all_cache_tempfile)) die("Error!  Temp cache file wasn't created.");
                if (file_exists($all_cache_file)) unlink($all_cache_file);
                rename($all_cache_tempfile, $all_cache_file);
                fclose($filehandle);
                if ($send_gen_cache) echo file_get_contents($all_cache_file);
                }
}

function xtern_sync($gen_cache){
        global $sys_url;
        $sync_cache_tempfile = "sync.cache.temp";
        $sync_cache_file = "sync.cache";
        $send_gen_cache = FALSE;

        //when gen_cache = FALSE, check for cache file.  If file exists, echo it's contents and exit.
        //If file doesn't exist, set gen_cache = TRUE and continue to generate the cache file and echo
        //it's contents.
        if (!$gen_cache) {
                if (file_exists($sync_cache_file)) echo file_get_contents($sync_cache_file);
                else $send_gen_cache = TRUE;
        }
        
        //if we need to generate a cache file (gen_cache=TRUE), start creating a new temp cache file
        //once we have the temp cache file, delete the old cache file and rename the temp cache file
        if ($gen_cache || $send_gen_cache) {
                if (file_exists($sync_cache_tempfile)) unlink($sync_cache_tempfile) or die("Error!  Temp cache file is in use, try again later.");
                $filehandle = fopen($sync_cache_tempfile, 'w') or die("Error opening temp cache file: $all_cache_tempfile");
        
        
                fwrite($filehandle, "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n");
                fwrite($filehandle, "<plugins>\n");
                fwrite($filehandle, "  <appversion>1.41</appversion>\n");
        
                global $db_host;
                global $db_user;
                global $db_pass;
                global $db_database;
        
                @mysql_connect($db_host, $db_user, $db_pass) or die("Error");
                @mysql_select_db($db_database) or die("Error");
        
                //$sql = "SELECT * FROM plugins ORDER BY plugin_Name";
                $sql = "SELECT * FROM plugins WHERE plugin_ReviewFlag='FALSE' ORDER BY plugin_Name ASC";
                $res = mysql_query($sql);
        
                while (($enreg=@mysql_fetch_array($res))) {
                        $plugin_id = utf8_encode(htmlspecialchars($enreg["plugin_ID"]));
                        $plugin_packageid = utf8_encode(htmlspecialchars($enreg["plugin_packageid"]));
                        $plugin_name = utf8_encode(htmlspecialchars($enreg["plugin_Name"]));
                        $plugin_version = utf8_encode(htmlspecialchars($enreg["plugin_Version"]));
                        $plugin_state = utf8_encode(htmlspecialchars($enreg["plugin_State"]));
                        $plugin_type = utf8_encode(htmlspecialchars($enreg["plugin_Type"]));
                        
                        //Added the following $plugin_ values (robogeek)
                        $plugin_key = utf8_encode(htmlspecialchars($enreg["plugin_key"]));
                        $plugin_versionvalue = utf8_encode(htmlspecialchars($enreg["plugin_VersionValue"]));
                        $plugin_license = utf8_encode(htmlspecialchars($enreg["plugin_Licence"]));
                        $plugin_date = utf8_encode(htmlspecialchars($enreg["plugin_Date"]));
                        $plugin_downloadsize = utf8_encode(htmlspecialchars($enreg["plugin_DownloadSize"]));
                        $plugin_hash = utf8_encode(htmlspecialchars($enreg["plugin_hash"]));
                        $plugin_LongDescription = utf8_encode(htmlspecialchars($enreg["plugin_LongDescription"]));
                        $plugin_AuthorEmail = utf8_encode(htmlspecialchars($enreg["plugin_AuthorEmail"]));
                        $plugin_Copyright = utf8_encode(htmlspecialchars($enreg["plugin_Copyright"]));
                        $plugin_SupportLink = utf8_encode(htmlspecialchars($enreg["plugin_SupportLink"]));
                        $plugin_DonationLink = utf8_encode(htmlspecialchars($enreg["plugin_DonationLink"]));
                        $plugin_SourceLink = utf8_encode(htmlspecialchars($enreg["plugin_SourceLink"]));
                        $plugin_DocumentationFile = utf8_encode(htmlspecialchars($enreg["plugin_DocumentationFile"]));
                        $plugin_DocumentText = utf8_encode(htmlspecialchars($enreg["plugin_DocumentText"]));
                        $plugin_Site = utf8_encode(htmlspecialchars($enreg["plugin_Site"]));
                        $plugin_DirectDownloadURL = utf8_encode(htmlspecialchars($enreg["plugin_DirectDownloadURL"]));
                        $plugin_MinRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MinRequiredVersion"]));
                        $plugin_MaxRequiredVersion = utf8_encode(htmlspecialchars($enreg["plugin_MaxRequiredVersion"]));
                        $plugin_IsDotNET = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNET"]));
                        $plugin_IsDotNETSimple = utf8_encode(htmlspecialchars($enreg["plugin_IsDotNETSimple"]));
                        $plugin_DestinationFolder = utf8_encode(htmlspecialchars($enreg["plugin_DestinationFolder"]));
                        
        
                        //changed this to get download count from userdownloads table (robogeek)
                        $sql = "SELECT * FROM userdownloads WHERE plugin_ID='$plugin_id' and plugin_Version='$plugin_version'";
                        $resD = mysql_query($sql);
                        $resD_rows = mysql_numrows($resD);
                        $plugin_downloads = utf8_encode(htmlspecialchars($resD_rows));
                        
                        //added plugin popularity (robogeek)
                        $plugin_popularity = GetPopularity($plugin_id);
        
                        //not implemented yet?
                        $plugin_trusted = utf8_encode(htmlspecialchars("unknown"));
        
                        $sql = "SELECT * FROM votesWorking WHERE plugin_ID='$plugin_id' AND plugin_Version='$plugin_version'";
                        $resV = mysql_query($sql);
                        $nbV = mysql_numrows($resV);
                        $plugin_works = "unknown";
                        $enregNbV = 0;
                        $yes = 0;
                        $no = 0;
                        while (($enregNbV<$nbV)&&($enregV=@mysql_fetch_array($resV)))
                        {
                                if ($enregV["vote"] == "true") $yes++;
                                else $no++;
                        }
                        if ($yes == 0 && $no == 0) $plugin_works = "unknown";
                        elseif ($yes > $no) $plugin_works = "true";
                        elseif ($no > $yes) $plugin_works = "false";
                        elseif ($no == $yes) $plugin_works = "undecided";
        
                        $plugin_desc = utf8_encode(htmlspecialchars($enreg["plugin_ShortDescription"]));
                        $plugin_author = utf8_encode(htmlspecialchars($enreg["plugin_Author"]));
                        if (isset($enreg["profil_id"]) && $enreg["profil_id"] != "") $plugin_author = $plugin_author . " (" . htmlspecialchars($enreg["profil_id"]) . ")";
        
                        fwrite($filehandle, "   <plugin id=\"$plugin_id\" packageid=\"$plugin_packageid\" version=\"$plugin_version\" type=\"$plugin_type\" state=\"$plugin_state\" name=\"$plugin_name\" ");
                        fwrite($filehandle, "url=\"" . $sys_url . "dl.php?plugin_id=$plugin_id&amp;plugin_version=$plugin_version&amp;plugin_state=$plugin_state\" versionvalue=\"$plugin_versionvalue\" size=\"$plugin_downloadsize\" date=\"$plugin_date\" ");
                        fwrite($filehandle, "license=\"$plugin_license\" downloads=\"$plugin_downloads\" popularity=\"$plugin_popularity\" trusted=\"$plugin_trusted\" works=\"$plugin_works\" ");
                        fwrite($filehandle, "desc=\"$plugin_desc\" longdesc=\"$plugin_LongDescription\" author=\"$plugin_author\" authoremail=\"$plugin_AuthorEmail\" copyright=\"$plugin_Copyright\" ");
                        fwrite($filehandle, "supportlink=\"$plugin_SupportLink\" donationlink=\"$plugin_DonationLink\" sourcelink=\"$plugin_SourceLink\" documentationfile=\"$plugin_DocumentationFile\" ");
                        fwrite($filehandle, "documenttext=\"$plugin_DocumentText\" website=\"$plugin_Site\" directdownloadurl=\"$plugin_DirectDownloadURL\" minreqversion=\"$plugin_MinRequiredVersion\" ");
                        fwrite($filehandle, "maxreqversion=\"$plugin_MaxRequiredVersion\" isdotnet=\"$plugin_IsDotNET\" isdotnetsimple=\"$plugin_IsDotNETSimple\" destinationfolder=\"$plugin_DestinationFolder\" hash=\"$plugin_hash\" />\n");
                        }
                        
                fwrite($filehandle, "</plugins>\n");
        
                //once we have the temp cache file, delete the old cache file and rename the temp cache file
                if (!file_exists($sync_cache_tempfile)) die("Error!  Temp cache file wasn't created.");
                if (file_exists($sync_cache_file)) unlink($sync_cache_file);
                rename($sync_cache_tempfile, $sync_cache_file);
                fclose($filehandle);
                if ($send_gen_cache) echo file_get_contents($sync_cache_file);
                }
}

function regenerate_xtern_cache() {
        $summary_cache_tempfile = "all.cache.temp";
        $summary_cache_file = "all.cache";
        $extended_cache_tempfile = "all.cache.temp";
        $extended_cache_file = "all.cache";
        $all_cache_tempfile = "all.cache.temp";
        $all_cache_file = "all.cache";
        $sync_cache_tempfile = "sync.cache.temp";
        $sync_cache_file = "sync.cache";
        $gen_cache = TRUE;      
        set_time_limit(90);

        //if a temp file exists, another user must have kicked of the regeneration.
        //we'll wait for up to 15 seconds before moving on
        $timer = 0;
        while (file_exists($summary_cache_tempfile)) {
                sleep(5);
                $timer++;
                echo ".";
                if ($timer == 3) continue;
                }
        if (!file_exists($summary_cache_tempfile)) xtern_summary($gen_cache);
        
        //if a temp file exists, another user must have kicked of the regeneration.
        //we'll wait for up to 15 seconds before moving on
        $timer = 0;
        while (file_exists($extended_cache_tempfile)) {
                sleep(5);
                $timer++;
                echo ".";
                if ($timer == 3) continue;
                }
        if (!file_exists($extended_cache_tempfile)) xtern_extended($gen_cache);

        //if a temp file exists, another user must have kicked of the regeneration.
        //we'll wait for up to 15 seconds before moving on
        $timer = 0;
        while (file_exists($all_cache_tempfile)) {
                sleep(5);
                $timer++;
                echo ".";
                if ($timer == 3) continue;
                }
        if (!file_exists($all_cache_tempfile)) xtern_all($gen_cache);

        //if a temp file exists, another user must have kicked of the regeneration.
        //we'll wait for up to 15 seconds before moving on
        $timer = 0;
        while (file_exists($sync_cache_tempfile)) {
                sleep(5);
                $timer++;
                echo ".";
                if ($timer == 3) continue;
                }
        if (!file_exists($sync_cache_tempfile)) xtern_sync($gen_cache);

}


//Generate a temporary directory name for plugin processing
//creates temp directory with a unique name at the specified
//path with the specified prefix.
// Returns directory name on success, false otherwise
function Generate_Temp_Directory($path, $prefix)
{
        // Use PHP's tmpfile function to create a temporary
        // directory name. Delete the file and keep the name.
                // To add more uniqueness to the name, we append the
                // unix timestamp to the $prefix.
                $timestamp = time();
                $prefix = $prefix . $timestamp;
        $tempname = tempnam($path,$prefix);
        if (!$tempname)
                return false;

        if (!unlink($tempname))
                return false;

        // Create the temporary directory and returns its name.
        if (mkdir($tempname))
                return $tempname;

        return false;
}


//This function shows the nav links and filter options in thelist.php
//function show_navfilter_options($program, $ptype, $nb, $start, $totalResults, $back, $forw, $sort_filter, $sort_order)
function show_navfilter_options($breadcrumb2, $nb, $start, $totalResults, $back, $forw, $sort_filter, $sort_order)
{
        echo "<table width='100%'><tr><td NOWRAP>";
        if ($start>0) {
                $newstart = $start-$nb;
                if ($newstart < 0) $newstart = 0;
                $back = $_SERVER["PHP_SELF"]."?bc$breadcrumb2&start=" . $newstart . "&nb=$nb";
                if (isset($_GET["author"])) $back .= "&author=".$_GET["author"];
                echo "<a href=\"$back\" onclick=\"ajaxManager('load_page','$back','layer1'); return false;\">PREV</a> ";
        }
        else {
                echo "PREV ";
        }

        // If records available
        // Added NEXT/PREV page and number of items per page links to top of listings (robogeek)
        if (($start + $nb) < $totalResults )  {
                $forw = $_SERVER["PHP_SELF"]."?bc$breadcrumb2&start=" . ($start+$nb) . "&nb=$nb";
                if (isset($_GET["author"])) $forw .= "&author=".$_GET["author"];
                echo "<a href=\"$forw\" onclick=\"ajaxManager('load_page','$forw','layer1'); return false;\">NEXT</a>\n";
        }
        else {
                echo "NEXT";
        }
        $itemsperpage_5 = $_SERVER["PHP_SELF"]."?bc$breadcrumb2&start=" . $start . "&nb=5";
        $itemsperpage_10 = $_SERVER["PHP_SELF"]."?bc$breadcrumb2&start=" . $start . "&nb=10";
        $itemsperpage_25 = $_SERVER["PHP_SELF"]."?bc$breadcrumb2&start=" . $start . "&nb=25";
        $itemsperpage_50 = $_SERVER["PHP_SELF"]."?bc$breadcrumb2&start=" . $start . "&nb=50";
        $itemsperpage_100 = $_SERVER["PHP_SELF"]."?bc$breadcrumb2&start=" . $start . "&nb=100";
        echo "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Items Per Page: \n";
        if ($nb == "5") echo "[";
        echo "<a href=\"$itemsperpage_5\">5</a>";
        if ($nb == "5") echo "] "; else echo " ";
        if ($nb == "10") echo "[";
        echo "<a href=\"$itemsperpage_10\">10</a>";
        if ($nb == "10") echo "] "; else echo " ";
        if ($nb == "25") echo "[";
        echo "<a href=\"$itemsperpage_25\">25</a>";
        if ($nb == "25") echo "] "; else echo " ";
        if ($nb == "50") echo "[";
        echo "<a href=\"$itemsperpage_50\">50</a>";
        if ($nb == "50") echo "] "; else echo " ";
        if ($nb == "100") echo "[";
        echo "<a href=\"$itemsperpage_100\">100</a>";
        if ($nb == "100") echo "] "; else echo " ";
        echo "</td></tr><tr><td align=\"right\" NOWRAP>\n";
        echo "<form action=\"thelist.php\" method=\"get\">Sort by: <select name=\"filter\">\n";
        echo "<option ";
        if ($sort_filter == "name") echo "selected";
        echo ">name</option>\n";
        echo "<option ";
        if ($sort_filter == "total downloads") echo "selected";
        echo ">total downloads</option>\n";
        echo "<option ";
        if ($sort_filter == "current downloads") echo "selected";
        echo ">current downloads</option>\n";
        echo "<option ";
        if ($sort_filter == "date") echo "selected";
        echo ">date</option>\n";
/*
//      echo "<option ";
";";";";";";";";//      if ($sort_filter == "type") echo "selected";
";";";";";";";";//      echo ">type</option>\n";
//      echo "<option ";
";";";";";";";";//      if ($sort_filter == "popularity") echo "selected";
";";";";";";";";//      echo ">popularity</option>\n";
//      echo "<option ";
";";";";";";";";//      if ($sort_filter == "voted working") echo "selected";
";";";";";";";";//      echo ">voted working</option>\n";
//      echo "<option ";
";";";";";";";";//      if ($sort_filter == "voted not working") echo "selected";
";";";";";";";";//      echo ">voted not working</option>\n";
*/
        echo "<option ";
        if ($sort_filter == "author") echo "selected";
        echo ">author</option>\n";
        echo "</select> &nbsp;&nbsp;&nbsp;\n";
        echo "Sort order: \n";
        echo "<select name=\"order\">\n";
        echo "<option ";
        if ($sort_order == "ascending") echo "selected";
        echo ">ascending</option>\n";
        echo "<option ";
        if ($sort_order == "descending") echo "selected";
        echo ">descending</option>\n";
        echo "</select>\n";
        echo "<input type=\"submit\" value=\"Submit\">\n";
        while (list($name, $value) = each($_GET)) {
                if ($name == "filter" || $name == "order") continue;
                echo "<input type=\"hidden\" value=\"$value\" name=\"$name\">\n"; 
                }
        echo "</form>\n";
        echo "</td></tr></table>";
}

function SearchBar($links, $breadcrumblink, $search) {
        if ($search) {
                //show search box
                echo "<form action=\"thelist.php\" method=\"get\"><input name=\"search\"><input type=\"submit\" name=\"Search\" value=\"Search\"></form>";
                echo "</td><td>&nbsp;&nbsp;|&nbsp;&nbsp;</td><td id=\"menu\">";
                }
                
        echo "<div id=\"menudiv\">";
        echo "<b>$links";

        if ($breadcrumblink) echo "&nbsp;&nbsp;|&nbsp;&nbsp;" . $breadcrumblink;
        echo "</b>";
        echo "</div>";
        echo "</td></tr></table><table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"center\" id=\"wrapper\">\n";
}

function IsPluginTypeValid($plugin_type, $types) {
        foreach($types as $type) {
                if (strtolower($plugin_type) == strtolower($type)) return TRUE;
                }
        if (strtolower($plugin_type) == "none" || $plugin_type == "") return TRUE;
        else return FALSE;
}

function GetContents($dir,$files=array()) {
  if(!($res=opendir($dir))) exit("$dir doesn't exist!");
  while(($file=readdir($res))==TRUE)
    if($file!="." && $file!="..")
      if(is_dir("$dir/$file")) $files=GetContents("$dir/$file",$files);
        else array_push($files,"$dir/$file");

  closedir($res);
  return $files;
}


?>
