<?PHP
require_once('functions.php');
//require_once('authentication.php');

//$u = Authenticate();

global $db_host;
global $db_user;
global $db_pass;
global $db_database;

mysql_connect($db_host, $db_user, $db_pass) or myDie("Error connecting to mysql $db_user $db_host");
mysql_select_db($db_database) or myDie("Error connecting to database");


$date = date ("l, F jS, Y");
$time = date ("h:i A");

mysql_query("CREATE TABLE IF NOT EXISTS popularities (`plugin_ID` varchar(38) NOT NULL, `Date` date NOT NULL, `profil_id` tinytext NOT NULL, `machine` text NOT NULL)");

$date = date("Y-m-d");
$profil = $_POST["user"];
$machine = $_POST["uniqueId"];
$plugins = explode(",", $_POST["plugins"]);
$msg = "";
mysql_query("DELETE FROM popularities WHERE `profil_id`=\"$profil\" AND `machine`=\"$machine\";") or myDie("failed to delete");
foreach ($plugins as $plugin)
{
	mysql_query("INSERT INTO popularities (`plugin_ID`, `Date`, `profil_id`, `machine`) VALUES (\"$plugin\", \"$date\", \"$profil\", \"$machine\")") or myDie("failed to insert");
}


echo("SUCCESS");

function myDie($msg)
{
	mail("slug@skyforge.net", "ERROR in MeediOS\MAID\PopularityContest.php", $msg);
	die($msg);
}

?>