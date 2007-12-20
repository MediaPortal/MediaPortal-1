<?php
require_once('functions.php');
include('header.php');
global $db_host;
global $db_user;
global $db_pass;
global $db_database;
$image_table = "images";
if (isset($_GET["plugin_id"])) $plugin_id = $_GET["plugin_id"];
if (isset($_GET["plugin_version"])) $plugin_version = $_GET["plugin_version"];
if (isset($_GET["plugin_state"])) $plugin_state = $_GET["plugin_state"];
if (isset($_GET["start"])) $start = $_GET["start"];
if (!$plugin_id || !$plugin_version || !$plugin_state || $start == "") die ("Nice hack job...NOT!");
?>

<div id="menudiv">
<b><a href="<?php echo $sys_url;?>">OpenMAID</a> :: <?php
echo "<a href=\"detail.php?plugin_id=$plugin_id&plugin_version=$plugin_version&plugin_state=$plugin_state\">Back to Plugin Detail</a></b>";
?>

</div>
</td></tr><tr>
<td width="760" id="contentarea">

<?php
@mysql_connect($db_host, $db_user, $db_pass) or die("Error, can't connect to mysql server!");
@mysql_select_db($db_database) or die("Error, can't connect to db!");

$sql = "SELECT * FROM plugins WHERE plugin_ID = '$plugin_id' and plugin_Version = '$plugin_version'";
$res = mysql_query($sql);

$enreg=@mysql_fetch_array($res);
$plugin_author = $enreg["plugin_Author"];
$plugin_name = $enreg["plugin_Name"];

echo "<div align='center'>";
echo "<b>" . $plugin_name . "</b> by <b>$plugin_author</b><br/>";

$sql = "SELECT * FROM $image_table WHERE plugin_ID = '$plugin_id' and plugin_Version = '$plugin_version'";
$res = mysql_query($sql);

$nb = mysql_numrows($res);

$i = 0;
while ($i < $nb){
	if ($i == $start) {
	   $k = $i + 1;

	   if ($i == 0) {
		echo "<b>PREV</b>";
	      }
	   else {
		$j = $i - 1;
		echo "<b><a href='images.php?plugin_id=$plugin_id&plugin_version=$plugin_version&plugin_state=$plugin_state&start=$j'>PREV</a></b>";
		}

	  echo " - ";
	  if ($i == ($nb - 1)) {
	     echo "<b>NEXT</b>";
	     }
	  else {
	     $j = $i + 1;
	     echo "<b><a href='images.php?plugin_id=$plugin_id&plugin_version=$plugin_version&plugin_state=$plugin_state&start=$j'>NEXT</a></b>";
	     }

	  echo "(Showing $k of $nb)&nbsp;&nbsp;&nbsp;";
	  echo "<br />";

	  $image_path = mysql_result($res, $i, "image_path");;
	  $ImageSize = @getimagesize($image_path);
	  if ($ImageSize[0] > 763) $width = "width='763'";
	  else $width = "width='" . $ImageSize[0] . "'";
	  
	  if ($ImageSize[0] > 763) echo "<a href='$image_path' target='_blank'>";
 	  echo "<img src='$image_path' border=0 $width>";
	  if ($ImageSize[0] > 763) echo "</a>";
	  echo "<br />";
	  if ($ImageSize[0] > 763) echo "<b>(Image is larger than what is shown, click image for full size image in a new browser window)</b></div>";
 	  }

	$i++;
	}

mysql_close();

include('footer.php');
?>