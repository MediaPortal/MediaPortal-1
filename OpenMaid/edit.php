<?php
require_once('functions.php');
include('header2.php');
require_once('db.php');
require_once('authentication.php');

global $sys_url;


function printTextBox($caption, $filedName,$currentValue)
{
	echo "<tr><td>$caption:</td><td><input type=\"text\" size=\"50\" name=\"$filedName\" value=\"$currentValue\" title=\"$caption\"></td></tr>";
}


function printTextArea($caption, $fieldName, $currentValue)
{
	echo "<tr><td>$caption:</td><td><textarea name=\"$fieldName\" cols=\"70\" rows=\"5\" title=\"$caption\">$currentValue</textarea></td></tr>";
}


function printHidden($fieldName, $currentValue)
{
	echo "<input name=\"$fieldName\" type=\"hidden\" value=\"$currentValue\">";
}


function printDropDown($caption, $fieldName, $currentValue, $possibleValues){
	$currentValue=strtolower($currentValue);
	$num_possibleValues = sizeof($possibleValues);
	echo "<tr><td>$caption:</td><td><select name=\"$fieldName\">";
	for ($i=0; $i < $num_possibleValues; $i++) {
		echo "<option value=\"$possibleValues[$i]\"";
		if ($currentValue == strtolower($possibleValues[$i])) echo " selected>";
		else echo ">";
		echo "$possibleValues[$i]</option>";
	}
	echo "</select></td></tr>";
}
?>

<div id="menudiv">
<b><a href="<?php echo $sys_url; ?>">OpenMAID</a> > Directory</b>
</div>
</td></tr><tr>
<td id="contentarea">
<div id="widebar">

<?php
if (isset($_GET["plugin_id"])) $plugin_id = $_GET["plugin_id"];
if (isset($_GET["plugin_version"])) $plugin_version = $_GET["plugin_version"];
if (isset($_GET["plugin_state"])) $plugin_state = $_GET["plugin_state"];
if (!$plugin_id || !$plugin_version || !$plugin_state) slowdie("Invalid input...go back and try again or report this error to the OpenMAID admin.");

//Update database record for plugin
if (isset($_POST["plugin_ID"]) && isset($_POST["plugin_Version"]) && isset($_POST["plugin_State"])) {
	$currentUser = Authenticate();
	if (!isset($currentUser) || $currentUser == "")
		//slowdie( 'You must <a href="' . GetLogonURL() . '">login to edit this addon</a>');

	if (!(IsAdmin($currentUser) | $currentUser == $plugin->plugin_Author))
		//slowdie( "Sorry, you are not authorised to edit this addon.");
	
	ConnectOnce();
	//echo $currentUser . "<br>";
	$profil_id = $_POST["profil_id"]; //echo $profil_id . "<br>";
	$plugin_ID = $_POST["plugin_ID"]; //echo $plugin_ID . "<br>";
	$plugin_Version = $_POST["plugin_Version"]; //echo $plugin_Version . "<br>";
	$plugin_State = $_POST["plugin_State"]; //echo $plugin_State . "<br>";
	$plugin_Name = $_POST["plugin_Name"]; //echo $plugin_Name . "<br>";
	$plugin_Licence = $_POST["plugin_Licence"]; //echo $plugin_Licence . "<br>";
	$plugin_Copyright = $_POST["plugin_Copyright"]; //echo $plugin_Copyright . "<br>";
	$plugin_SupportLink = $_POST["plugin_SupportLink"]; //echo $plugin_SupportLink . "<br>";
	$plugin_DonationLink = $_POST["plugin_DonationLink"]; //echo $plugin_DonationLink . "<br>";
	$plugin_SourceLink = $_POST["plugin_SourceLink"]; //echo $plugin_SourceLink . "<br>";
	$plugin_ShortDescription = $_POST["plugin_ShortDescription"]; //echo $plugin_ShortDescription . "<br>";
	$plugin_DocumentText = $_POST["plugin_DocumentText"]; //echo $plugin_DocumentText . "<br>";
	$sql = "UPDATE plugins SET plugin_Name = '$plugin_Name', plugin_Licence = '$plugin_Licence', plugin_Copyright = '$plugin_Copyright', profil_id = '$profil_id' WHERE plugin_ID = '$plugin_ID' AND plugin_Version = '$plugin_Version' AND plugin_State = '$plugin_State'";
	mysql_query($sql);
	$sql = "UPDATE plugins SET plugin_SupportLink = '$plugin_SupportLink', plugin_DonationLink = '$plugin_DonationLink', plugin_SourceLink = '$plugin_SourceLink' WHERE plugin_ID = '$plugin_ID' AND plugin_Version = '$plugin_Version' AND plugin_State = '$plugin_State'";
	mysql_query($sql);
	$sql = "UPDATE plugins SET plugin_ShortDescription = '$plugin_ShortDescription', plugin_DocumentText = '$plugin_DocumentText' WHERE plugin_ID = '$plugin_ID' AND plugin_Version = '$plugin_Version' AND plugin_State = '$plugin_State'";
	mysql_query($sql);
	echo "Database record has been updated...<br><br>";
	
	//if database is updated, regenerate xtern cache files
	//regenerate_xtern_cache();
	//takes a while to regen cache.  implement a scheduler/flagging system to regen cache via xtern.php.
}

//gets the plugin object
$plugin = GetPreviousPluginObject($plugin_id, $plugin_version, $plugin_state, TRUE);

$currentUser = Authenticate();

if (!isset($currentUser) || $currentUser == "")
	//slowdie( 'You must <a href="' . GetLogonURL() . '">login to edit this addon</a>');

if (!(IsAdmin($currentUser) | $currentUser == $plugin->plugin_Author))
	//slowdie( "Sorry, you are not authorised to edit this addon.");
	
echo '<form method="post">';


echo '<table>';

printTextBox("Name","plugin_Name", $plugin->plugin_Name);
//commented out plugin_State...if we change it after plugin is uploaded, we will need to change the directory the plugin
//is in or it won't be found after the state is changed.
//printDropDown("State","plugin_State", $plugin->plugin_State, array("stable", "beta", "alpha"));
//commented out plugin_Type...shouldn't need to change this once it's set!  
//printDropDown("Type","plugin_Type", $plugin->plugin_Type, array("general","module","import","theme","input"));
printDropDown("License","plugin_Licence", $plugin->plugin_Licence, array("commercial","donationware","freeware","shareware"));
printTextBox("Copyright","plugin_Copyright", $plugin->plugin_Copyright);
if (IsAdmin($currentUser)) printTextBox("Profile ID","profil_id", $plugin->profil_id);
else printHidden("profil_id", $plugin->profil_id);
printTextBox("Support Link","plugin_SupportLink", $plugin->plugin_SupportLink);
printTextBox("Donation Link","plugin_DonationLink", $plugin->plugin_DonationLink);
printTextBox("Source Code Link","plugin_SourceLink", $plugin->plugin_SourceLink);
printTextArea("Description","plugin_ShortDescription",$plugin->plugin_ShortDescription);
printTextArea("Document Text","plugin_DocumentText",$plugin->plugin_DocumentText);
printHidden("plugin_ID",$plugin->plugin_ID);
printHidden("plugin_Version",$plugin->plugin_Version);
printHidden("plugin_State",$plugin->plugin_State);


//plugin_Date
echo '</table>';
echo '<br><input type="submit" value="Update" />';
echo '</form>';

echo "</div>";

include('footer2.php');
?>

