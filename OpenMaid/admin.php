<?php
require_once('functions.php');
require_once('authentication.php');
require_once('db.php');
require_once('archive.php');
session_start();

include('header2.php');

//show search bar.  links, breadcrumblink, enable/disable search box.
SearchBar("<a href=\"" . $sys_url . "extra.php\">Stats & Tools</a>&nbsp;&nbsp;|&nbsp;&nbsp<a href=\"$sys_url\">OpenMAID</a>","",TRUE);

echo "<tr><td id=\"contentarea\">";

if (isset($_GET["confirm"])) $confirm = $_GET["confirm"];
else $confirm = FALSE; //default mode

if (isset($_GET["func"])) $func = $_GET["func"];
else $func = ""; //default cache_mode


$mode = $_GET["mode"];
if ($mode != "managedb") echo '<div id="leftbar">';
else echo '<div id="widebar">';

$u = Authenticate();

if (!isset($u) || $u == "")
	slowdie( 'You must <a href="' . GetLogonURL() . '">login to access administration panel</a>');

if (!IsAdmin($u))
	//slowdie( "Sorry, you are not authorised to use this module.");


echo "<h1>Administration Panel</h1>";

if($mode == "")
	$mode = "home";

switch ($mode)
{
	case "home":
		printAdminHome($u);
		break;
	case "initdb":
		printInitDB();
		break;
	case "initdir":
		printInitDirectory();
		break;
	case "managedb":
		printManageDB();
		break;
	case "dbinfo":
		printDBInfo();
		break;
	case "phpinfo":
		printPHPInfo();
		break;
	case "check":
		printCheckForProblems($func,$confirm);
		break;
	case "clearcache":
		printClearMAIDCache();
		break;
	case "sourcecode":
		printSourceCodeArchive();
		break;
	case "regendlcounts":
		printRegenDLCounts();
		break;
	case "synccode":
		printSyncCode();
		break;
	case "syncprofilid":
		printSyncProfileID();
		break;
	case "syncdlcount":
		printSyncDLCount();
		break;
}	

function printLine($line)
{
	echo "$line<br />\n";
}
function printPara($para)
{
	echo "<p>$para</p>\n";
}

function printH2($header)
{
	echo "<h2>$header</h2>\n";
}

function printSectionTitle($title)
{
	printH2($title);
	echo "<hr />\n";
}

function printAdminHome($user)
{
	global $ftp_repository;
	global $ftp_manual_uploads;

	printSectionTitle("Admin Panel Home");
	printPara ("Welcome, $user");
	
	//Get # of failed plugins
	//Get # of plugins from db that have no profile id
	//Check DB tables to be sure they all exist
	$failed_plugins = array();
	findFiles($failed_plugins,$ftp_repository."*.mpp.fail");
	$failed_count = count($failed_plugins);
	$no_profile_id = mysql_num_rows(findMissingProfileID());
	$flagged_for_review = mysql_num_rows(findReviewFlag());
	$flagged_for_metadata = mysql_num_rows(findMissingMetadata(""));

	$attention = $failed_count + $no_profile_id + $flagged_for_review + $flagged_for_metadata;

	printLine ("There are <b>$attention</b> items that need attention!");
	if ($failed_count > 0) printLine ("&nbsp;&#8226; Failed Plugins = $failed_count (<a href=\"process.php?mode=manage_failed\">Manage Failed Plugins</a>)");
	if ($flagged_for_review > 0) printLine ("&nbsp;&#8226; Flagged for Review = $flagged_for_review (<a href=\"process.php?mode=manage_flagged\">Manage Flagged Plugins</a>)");
	if ($flagged_for_metadata > 0) printLine ("&nbsp;&#8226; Missing Metadata = $flagged_for_metadata (<a href=\"process.php?mode=manage_metadata\">Manage Missing Metadata</a>)");
	if ($no_profile_id > 0) printLine ("&nbsp;&#8226; No Profile ID = $no_profile_id (<a href=\"process.php?mode=manage_profileID\">Manage Missing Profile ID</a>)");
	
	
	//Find any new FTP Uploaded files
	printLine("");
	printSectionTitle("FTP Uploads/Repository");

	$new_plugins = array();
	findFiles($new_plugins,$ftp_repository."*.mpp");
	findFiles($new_plugins,$ftp_manual_uploads."*.mpp");

	$passed_plugins = array();
	findFiles($passed_plugins,$ftp_repository."*.mpp.pass");

	$ignored_plugins = array();
	findFiles($ignored_plugins,$ftp_repository."*.mpp.ignore");
	
	$plugin_count = count($new_plugins);
	$passed_count = count($passed_plugins);
	$failed_count = count($failed_plugins);
	$ignored_count = count($ignored_plugins);
	
	printLine ("There are $plugin_count FTP plugin(s) that need to be processed : (<a href='process.php?mode=process_new'>Auto</a> or <a href='process.php?mode=manage_new'>Manual</a> Process New Plugins)");
	printLine ("There are $passed_count FTP plugin(s) that processed successfully : (<a href='process.php?mode=manage_passed'>Manage Passed Plugins</a>)");
	printLine ("There are $failed_count FTP plugin(s) that failed to process : (<a href='process.php?mode=manage_failed'>Manage Failed Plugins</a>)");
	printLine ("There are $ignored_count FTP plugin(s) that are ignored : (<a href='process.php?mode=manage_ignored'>Manage Ignored Plugins</a>)");
	printLine ("");
	
	if ($plugin_count > 0) printLine("Click <a href='process.php?mode=manage_new'>here</a> to manually process new plugin(s)");
	printLine("Click <a href='process.php?mode=manage_all'>here</a> to manage all plugin(s)");
	
}

function printConfirmButton($name, $DisplayName)
{
	echo '<form method="post"><input type="hidden" name="' . $name . '" value="true" /> <input type="submit" value="' . $DisplayName . '" /></form>';
}

function printInitDB()
{
	printSectionTitle("Initialize Database");
	
	$initing = $_POST["InitializeDatabase"];
	
	if ($initing != true)
	{
		printPara("Initializing the database will create all of the database tables required for the OpenMaid system to work properly. If the tables already exist in the database, then the tables will not be over written. If a database rebuild is in order, please remove the tables using MySQL Admin (for security reasons) and initialize it through this Administration Panel");
		printPara("Press the button bellow to Initialize the Database");
		
		printConfirmButton("InitializeDatabase","Initialize Database" );
		
	}
	else
	{
		InitializeDatabase();
	}
}

function printDBInfo()
{
	printSectionTitle("Database Information");
	
	reportDBInfo();
	
}

function printPHPInfo()
{
	printSectionTitle("PHP Information");
	
	echo phpinfo();
	
}


function printInitDirectory()
{
	global $ftp_repository;
	global $ftp_manual_uploads;

	printSectionTitle("Initialize Directories");
	
	$initing = $_POST["InitializeDirectory"];
	
	if ($initing != true)
	{
		printPara("Initializing Directories will create the directories on the web server and set their permissions. If the directory existis, just the permissions will be reset.");
		printPara("Press the button bellow to Initialize the Directories");
		
		printConfirmButton("InitializeDirectory", "Initialize Directories");
		
	}
	else
	{
 		printLine("Creating Directories");
		LogCreateDirectory("mpp");
		LogCreateDirectory("data");
		LogCreateDirectory("plugins");
		LogCreateDirectory($ftp_manual_uploads);
		LogCreateDirectory($ftp_repository);
 		printLine("Finished Creating Directories");	
 		printLine("Setting Permissions");
 		printLine("Permissions not yet implemented!");

	}
}

function LogCreateDirectory($name)
{
	echo("Creating Directory $name.....");
	
	$result = @mkdir("$name", 0777);
	
	if ($result)
	{
		echo("Success!");
	}
	else
	{
		if (file_exists("$name"))
		{
			echo("Directory Already Exists!");
		}
		else
		{
			echo("Fail!");
		}
			
	}
	echo "<br/> ";
}

function printManageDB()
{
global $db_host;
global $db_user;
global $db_pass;

	printSectionTitle("Manage Plugin Database");
//********************************************//
//*** CONFIGURE YOUR SERVER HERE *** START ***//
$DBH=$db_host;
$DBU=$db_user;
$DBP=$db_pass;
//*** CONFIGURE YOUR SERVER HERE *** STOP ***//
//*******************************************//

$VER=array(
 "NAME"=>"WizMySQLAdmin",
 "WEB"=>"Wiz's Shelf",
 "URL"=>"http://www.wizshelf.org/",
 "MAJOR"=>"0.11",
 "MINOR"=>"4",
 "BUILD"=>"99"
);
$WIZ=$_SERVER[SCRIPT_NAME] . "?mode=managedb";
if(!$_SESSION[RPP]) $_SESSION[RPP]=20;

$dbl=@mysql_connect($DBH,$DBU,$DBP) or die("Access denied. Check configuration.");

switch($_REQUEST[hop]) {
//SET DATABASE
case "1":
 $_SESSION[DBN]=$_GET[dbn];
 $_SESSION[msg]="Database <b>$_SESSION[DBN]</b> selected";
 break;

//UNSET DATABASE AND TABLE
case "2":
 unset($_SESSION[DBN]);
 unset($_SESSION[TBN]);
 unset($_SESSION[select]);
 unset($_SESSION[order]);
 $_SESSION[msg]="Databases on server";
 break;

//CREATE NEW DATABASE
case "3":
 if(mysql_query("CREATE DATABASE `$_POST[dbnew]`",$dbl)) {
  $_SESSION[DBN]=$_POST[dbnew];
  $_SESSION[msg]="Database <b>$_SESSION[DBN]</b> created!";
 } else {
  unset($_SESSION[DBN]);
  $_SESSION[msg]="Error creating database <b>$_SESSION[TBN].$_POST[dbnew]</b>";
 }
 break;

//DROP DATABASE
case "4":
 if(mysql_query("DROP DATABASE `$_GET[dbn2drop]`",$dbl)) {
  $_SESSION[msg]="Database <b>$_GET[dbn2drop]</b> removed!";
  unset($_SESSION[DBN]);
 } else {
  $_REQUEST[op]=1;
  $_SESSION[msg]="Error removing database <b>$_GET[dbn2drop]</b>";
 }
 break;

//SET TABLE
case "5":
 $_SESSION[TBN]=$_REQUEST[tbn];
 $_SESSION[PG]=0;
 $_SESSION[WHERE]="";
 $_SESSION[msg]="Table <b>$_SESSION[DBN].$_SESSION[TBN]</b> selected";
 break;

//UNSET TABLE
case "6":
 unset($_SESSION[TBN]);
 $_SESSION[msg]="Tables of <b>$_SESSION[DBN]</b>";
 break;

//SET RPP VALUE
case "7":
 $_SESSION[RPP]=$_POST[rpp];
 $_SESSION[PG]=0;
 break;

//SET PG VALUE
case "8":
 $_SESSION[PG]=($_REQUEST[pg]) ? $_REQUEST[pg] : 0;
 break;

//SET WHERE FOR QUERY
case "9":
 if($_REQUEST[tbn]) $_SESSION[TBN]=$_REQUEST[tbn];
 $_SESSION[PG]=0;
 $where=stripslashes(trim($_POST[where]));
 $_POST[qr]="SELECT * FROM `$_SESSION[TBN]` WHERE ".(($where)?$where:"1");
 $_SESSION[msg]="Table <b>$_SESSION[DBN].$_SESSION[TBN]</b> selected";
 break;
case "9b":
 if(!$_SESSION[PG]) $_SESSION[PG]=0;
 break;

//10:ESPORTA CSV
case "10":
 $campi=mysql_list_fields($_SESSION[DBN],$_SESSION[TBN],$dbl);
 $cols=mysql_num_fields($campi);
 for($i=0;$i<$cols;$i++) $dump.="\"".mysql_field_name($campi,$i)."\",";
 $dump=substr($dump,0,-1)."\n";
 $rs=mysql_query("SELECT * FROM `$_SESSION[TBN]`",$dbl);
 while($rc=mysql_fetch_row($rs)) {
  for($i=0;$i<$cols;$i++) $dump.="\"".addslashes($rc[$i])."\",";
  $dump=substr($dump,0,-1)."\n";
 }
 header("Content-type: text/plain");
 header("Content-Disposition: filename=$_SESSION[DBN].$_SESSION[TBN].sql");
 die($dump);
 break;

//11:ALTER TABLE
case "11":
 $_SESSION[msg]="Alter table <b>$_SESSION[DBN].$_SESSION[TBN]</b>";
 break;
}

if($_SESSION[DBN]) {
 mysql_select_db($_SESSION[DBN],$dbl);
 $rs=mysql_list_tables($_SESSION[DBN],$dbl);
 for($i=0;$i<mysql_num_rows($rs);$i++) {
  $tbn=mysql_tablename($rs,$i);
  $TABLES.="<option value='$tbn' ".(($tbn==$_SESSION[TBN])?"selected":"").">$tbn</option>";
 }
} elseif ($_REQUEST[op]!="999") unset($_REQUEST[op]);
?>
<table cellspacing="0" cellpadding="0" width="100%" border="0">
 <tr valign="middle" height="20" bgcolor="#CCCCCC">
  <td width="75%">&nbsp; <b><?php echo("$VER[NAME] $VER[MAJOR].$VER[MINOR]"); ?></b></td>
  <td width="25%" align="right"><a href="<?php echo($VER[URL]); ?>" target='_blank'><b>P</b>owered by <b><?php echo($VER[WEB]); ?></b></a> &nbsp;</td>
 </tr>
</table>

<table cellspacing="0" cellpadding="0" width="100%" border="0">
 <tr valign="middle" height="20" bgcolor="#DDDDDD">
  <td width="75%">&nbsp; <b>Menu:</b> |
   <a href="<?php echo($WIZ); ?>&hop=2"><b>S</b>how databases</a>
<?php
if($_REQUEST[op]=="1") {
 //SHOW TABLES
 echo(" | <a href='javascript:if(confirm(\"Sure to drop database $_SESSION[DBN]?\")) window.open(\"$WIZ&hop=4&op=1&dbn2drop=$_SESSION[DBN]\",\"_self\")'><b>D</b>rop current database ($_SESSION[DBN])</a>");
} else {
 //TABLE FUNCTIONS
 echo("| <a href='$WIZ&op=1&hop=6'><b>B</b>ack to tables</a> | | ");
 if($_SESSION[TBN]) echo("| <a href='$WIZ&op=2'><b>P</b>roperties</a>
  | <a href='$WIZ&op=4&hop=9'><b>B</b>rowse</a>
  | <a href='$WIZ&op=8'><b>I</b>nsert</a>
  | <a href='javascript:if(confirm(\"Sure to EMPTY table $_SESSION[TBN]?\")) window.location=\"$WIZ&op=6\"'><b>E</b>mpty</a>
  | <a href='javascript:if(confirm(\"Sure to DROP table $_SESSION[TBN]?\")) window.location=\"$WIZ&op=7&table2drop=$_SESSION[TBN]\"'><b>D</b>rop</a>
  | <a href='$WIZ&op=14'><b>O</b>ptimize</a>
  | <a href='$WIZ&op=10'><b>I</b>mport</a>
  | <a href='$WIZ&hop=10' target='_blank'><b>E</b>xport</a>
 ");
}
?>
  | | | <a href="<?php echo($WIZ); ?>&op=999"><b>C</b>redits</a>
  | <a href="admin.php?mode=home">OpenMAID Admin Home</a> |</td>
  <td width="25%" align="right"><b>C</b>urrent database: <font color="#3366AA"><b>
   <?php
    if($_SESSION[DBN]) {
     echo($_SESSION[DBN]);
     if($_SESSION[TBN]) echo(".$_SESSION[TBN]");
    } else {
     echo("<i>none</i>");
    }
   ?> 
  </b></font> &nbsp;</td>
 </tr>
</table>

<table cellspacing="0" cellpadding="0" width="100%" border="0">
 <tr valign="top">
  <?php if($_REQUEST[op]<100) { ?>
   <td width="120" align="center" bgcolor="#EEEEEE" style="padding: 10pt 10pt 10pt 10pt">
    <?php if($_SESSION[DBN]) { ?>
     <form name='qry' action="<?php echo($WIZ); ?>&op=4" method="post">
      <b>.: QUERY :.</b><br>
      <textarea name="qr" style="width:110pt;height:75pt;"><?php echo(($_POST[qr])?stripslashes($_POST[qr]):$_SESSION[select]); ?></textarea><br>
      <input type="submit" style="width:110pt;" value="Execute"><br>
      <input onClick=javascript:this.form.qr.focus();this.form.qr.select(); type="button" value="Select" style="width:75pt;"><input onClick="javascript:this.form.qr.focus();this.form.qr.value='';" type="button" value="Clear" style="width:75pt;">
     </form>
     <form name="qry" action="<?php echo($WIZ); ?>&op=4" method="post" enctype="multipart/form-data">
      <b>.: QUERY FROM FILE:.</b><br>
      <input type="file" name="qrf" style="width:110pt;"><br>
      <input type="submit" style="width:110pt;" value="Execute">
     </form>
     <?php echo(($TABLES)?"<form name='sel' action='$WIZ&op=4&hop=9' method='post'>
      <b>.: SELECT :.</b><br>
      SELECT * FROM<br>
      <select name='tbn'>$TABLES</select><br>
      WHERE<br>
      <textarea name='where' style='width:110pt;height:75pt;'>$_SESSION[WHERE]</textarea><br>
      <input type='submit' style='width:110pt;' value='Execute'><br>
     </form>":""); ?>
     <form action="<?php echo($WIZ); ?>&p=12" method="post">
      <b>.: CREATE TABLE :.</b><br>
      Table name:<br>
      <input type="text" name="tablenew" style="width:110pt;"><br>
      Number of fields: <input type="text" name="tablefields" style="width:50pt;"><br>
      <input type="submit" style="width:110pt;" value="Execute">
     </form>
    <?php } ?>
    <form action="<?php echo($WIZ); ?>&op=1&hop=3" method="post">
     <b>.: CREATE DATABASE :.</b><br>
     Database name:<br>
     <input type="text" name="dbnew" style="width:110pt;"><br>
     <input type="submit" style="width:110pt;" value="Execute">
    </form>
   </td>
  <?php } ?>
  <td bgcolor="#FFFFFF" style="padding: 15pt 15pt 15pt 15pt">

<?php
switch($_REQUEST[op]) {
//1: SHOW TABLES
case "1":
 echo("<table cellspacing='0' cellpadding='2' border='0' style='border:1pt solid #666666;border-collapse:collapse;'>
  <tr style='background-color:#DDDDDD;'>
   <th width='100'>TABLE</th>
   <th width='60'>Records</th>
  </tr>\n");
 $rs=mysql_list_tables($_SESSION[DBN],$dbl);
 for($i=0;$i<mysql_num_rows($rs);$i++) {
  $bgcolor=($bgcolor=="#EEEEEE")?"#FFFFFF":"#EEEEEE";
  $tbn=mysql_tablename($rs,$i);
  $nr=mysql_fetch_object(mysql_query("SELECT count(*) AS nr FROM `$tbn`",$dbl));
  echo("<tr align='center' style='background-color:$bgcolor' onMouseOver='javascript:this.style.backgroundColor=\"#CCFF00\"' onMouseOut='javascript:this.style.backgroundColor=\"$bgcolor\"'>
   <td align='left'><a href='$WIZ&op=2&hop=5&tbn=$tbn'><b>$tbn</b></a></td>
   <td>".$nr->nr."</td>");
 }
 echo("</tr></table>");
 break;

//2:TABLE PROPERTIES
case "2":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 $BORDER="style='border:1pt dotted #666666;border-collapse:collapse;'";
 echo("<b>TABLE STRUCTURE</b><br><table cellspacing='0' cellpadding='2' border='0' style='border:1pt solid #666666;border-collapse:collapse;'>
  <tr style='background-color:#DDDDDD;'>
   <th width='90' $BORDER>FIELD</th>
   <th width='90' $BORDER>TYPE</th>
   <th width='90' $BORDER>NULL</th>
   <th width='90' $BORDER>DEFAULT</th>
   <th width='90' $BORDER>EXTRA</th>
   <th $BORDER colspan='3'>ACTIONS</th>
  </tr>\n");
 $campi=mysql_list_fields("$_SESSION[DBN]","$_SESSION[TBN]",$dbl);
 $cols=mysql_num_fields($campi);
 for($i=0;$i<$cols;$i++) {
  $bgcolor=($bgcolor=="#EEEEEE")?"#FFFFFF":"#EEEEEE";
  $col_info=mysql_fetch_array(mysql_query("SHOW COLUMNS FROM `$_SESSION[TBN]` LIKE '".mysql_field_name($campi,$i)."'"));
  echo("<tr style='background-color:$bgcolor' onMouseOver='javascript:this.style.backgroundColor=\"#CCFF00\"' onMouseOut='javascript:this.style.backgroundColor=\"$bgcolor\"'>
   <td $BORDER><b>".(($col_info[3]=="PRI")?"<u>":"")."$col_info[0]</u></b></td>
   <td width='90' style='word-wrap:break-word' $BORDER>$col_info[1]</td>
   <td style='word-wrap:break-word' $BORDER>$col_info[2]</td>
   <td style='word-wrap:break-word' $BORDER>$col_info[4]</td>
   <td style='word-wrap:break-word' $BORDER>$col_info[5]</td>
   <td $BORDER><a href='$WIZ&hop=11&op=16&field=$col_info[0]&act=1'>change</a></td>
   <td $BORDER><a href='javascript:if(confirm(\"Sure to drop field &#39;$col_info[0]&#39;?\")) window.open(\"$WIZ&hop=11&op=15&field=$col_info[0]\",\"_self\")'>drop</a></td>
   <td $BORDER><a href='$WIZ&hop=11&op=16&field=$col_info[0]&act=2'>add field</a></td>
  </tr>");
 }
 echo("</table>
  <p>&nbsp;</p>
  <b>TABLE INDEXES</b><br><table cellspacing='0' cellpadding='2' border='0' style='border:1pt solid #666666;border-collapse:collapse;'>
  <tr style='background-color:#DDDDDD;'>
   <th width='90' $BORDER>KEY NAME</th>
   <th width='90' $BORDER>FIELD</th>
   <th width='90' $BORDER>TYPE</th>
   <th width='90' $BORDER>CARDINALITY</th>
   <th $BORDER>ACTIONS</th>
  </tr>\n");
 $rs_idx=mysql_query("SHOW INDEX FROM `$_SESSION[TBN]`",$dbl);
 while($rc_idx=mysql_fetch_object($rs_idx)) {
  if($rc_idx->Index_type=="BTREE") {
   if($rc_idx->Non_unique==1) $idx_type="INDEX";
   elseif($rc_idx->Key_name=="PRIMARY") $idx_type="PRIMARY";
   else $idx_type="UNIQUE";
  } elseif($rc_idx->Index_type=="FULLTEXT") {
   $idx_type="FULLTEXT";
  }
  $idx[$rc_idx->Key_name][type]=$idx_type;
  $idx[$rc_idx->Key_name][column][]=$rc_idx->Column_name;
  $idx[$rc_idx->Key_name][cardinality]=(isset($rc_idx->Cardinality))?$rc_idx->Cardinality:"None";
 }
 if(is_array($idx)) foreach($idx as $idx_name=>$idx_prop) {
  $bgcolor=($bgcolor=="#EEEEEE")?"#FFFFFF":"#EEEEEE";
  echo("<tr style='background-color:$bgcolor' onMouseOver='javascript:this.style.backgroundColor=\"#CCFF00\"' onMouseOut='javascript:this.style.backgroundColor=\"$bgcolor\"'>
   <td $BORDER><b>$idx_name</b></td>
   <td $BORDER>");
  foreach($idx_prop[column] as $col) echo("$col<br>");
  echo("</td>
   <td $BORDER>$idx_prop[type]</td>
   <td $BORDER>$idx_prop[cardinality]</td>
   <td $BORDER align='center'><a href='javascript:if(confirm(\"Sure to drop index &#39;$idx_name&#39;?\")) window.open(\"$WIZ&hop=11&op=18&index=$idx_name\",\"_self\")'>drop</a></td>
  </tr>");
 }
 echo("</table>\n<p>Add index on <select onChange='javascript:if(this.value>0 && confirm(\"Create an index on \"+this.value+\" columns?\")) window.location=\"$WIZ&hop=11&op=19&cols=\"+this.value'>");
 for($i=0;$i<$cols;$i++) echo("<option value='$i'>".(($i>0)?$i:"")."</option>\n");
 echo("</select> colums</p>
  <p>&nbsp;</p>
  <p><b>TABLE OPERATIONS</b><br><table cellspacing='0' cellpadding='2' border='0'>
   <tr><td><form method='post' action='$WIZ&hop=11&op=21'>
    Rename table to: <input type='text' name='new_table_name' value='$_SESSION[TBN]'> <input type='submit' value='Rename'>
   </form></td></tr>
  </table></p>");
 break;

//4:EXECUTE QUERY
case "4":
 $_POST[qr]=trim($_POST[qr]);
 if($_GET[order]) $_SESSION[ORDER]=$_GET[order];
 $orderByOrderPos = strpos(strtolower($_SESSION[select]), "order");
 $orderByByPos = strpos(strtolower($_SESSION[select]), "by");
 if($orderByOrderPos!==false && $orderByByPos!==false && $orderByOrderPos<$orderByByPos) $_SESSION[ORDER]="";
 $_SESSION["desc"] = $_GET["desc"];
 if(strtolower(substr($_POST[qr],0,6))=="select" || (!$_POST[qr] && $_SESSION[select])) {
  if($_POST[qr]) $_SESSION[select]=stripslashes($_POST[qr]);
  if(strrpos($_SESSION[select],";") == (strlen($_SESSION[select])-1)) $_SESSION[select]=substr($_SESSION[select],0,-1);
  if($rs=@mysql_query($_SESSION[select],$dbl)) {
   $rc_columns=mysql_list_fields($_SESSION[DBN],$_SESSION[TBN],$dbl);
   for($i=0;$i<mysql_num_fields($rc_columns);$i++) $columns[]=mysql_field_name($rc_columns, $i);
   for($i=0;($i*$_SESSION[RPP])<mysql_num_rows($rs);$i++) $pages.="<option value='$i' ".(($_SESSION[PG]==$i)?"selected":"").">".($i+1)."</option>\n";
   $disablenext=(mysql_num_rows($rs)<=($_SESSION[RPP]*($_SESSION[PG]+1)))?"disabled":"";
   $rs=mysql_query("$_SESSION[select] " . (($_SESSION[ORDER] && in_array($_SESSION[ORDER], $columns)) ? "ORDER BY $_SESSION[ORDER]" . (($_SESSION["desc"]=="1") ? " DESC" : "") : "") . " LIMIT ".($_SESSION[PG]*$_SESSION[RPP]).",$_SESSION[RPP]",$dbl);
   echo("<p><b>Query</b>: ".$_SESSION[select]."</p>
    <table cellspacing='0' cellpadding='2' border='0' style='border:1pt solid #666666;border-collapse:collapse;'>
     <tr style='background-color:#DDDDDD;'><td colspan='2'>&nbsp;</td>");
   $i=0;
   while($fn[$i]=@mysql_field_name($rs,$i)) {
    echo("<th style='border:1pt dotted #999999;border-collapse:collapse;'><a href='$WIZ&op=4&order=$fn[$i]&desc=" . (($_SESSION["desc"]=="0") ? "1" : "0") . "'>$fn[$i]</a></th>");
    $i++;
   }
   echo("</tr>");
   while($rc=mysql_fetch_row($rs)) {
    $bgcolor=($bgcolor=="#EEEEEE")?"#FFFFFF":"#EEEEEE";
    echo("<tr style='background-color:$bgcolor;' onMouseOver='javascript:this.style.backgroundColor=\"#CCFF00\"' onMouseOut='javascript:this.style.backgroundColor=\"$bgcolor\"'>");
    $where=$fields="";
    for($j=0;$j<$i;$j++) {
     $fields.="<td style='border:1pt dotted #999999;'>".nl2br($rc[$j])."</td>\n";
     if($rc[$j]) $where.="`".$fn[$j]."`='".addslashes($rc[$j])."' AND ";
    }
    echo("
     <td><form method='post' action='$WIZ&op=8'>
      <input type='hidden' name='edit' value='".base64_encode($where)."'>
      <input type='submit' value='Edit'>
     </form></td>
     <td><form method='post' action='$WIZ&op=5'>
      <input type='hidden' name='del' value='".base64_encode($where)."'>
      <input type='button' value='Delete' onClick='javascript:if(confirm(\"Delete record?\")) submit();'>
     </form></td>
     $fields
    </tr>");
   }
   echo("</table>
    <p>
     <input type='button' value='&lt;&lt; Previous page' style='width:150pt;' ".((($_SESSION[PG]-1)<0)?"disabled":"")." onClick='javascript:window.location=\"$WIZ&op=4&hop=8&pg=".($_SESSION[PG]-1)."\"'>
     &nbsp;
     <input type='button' value='Next page &gt;&gt;' style='width:150pt;' $disablenext onClick='javascript:window.location=\"$WIZ&op=4&hop=8&pg=".($_SESSION[PG]+1)."\"'>
    </p>
    <form method='post' action='$WIZ&op=4&hop=7'>
     <input type='submit' value='Show'> <input type='text' name='rpp' value='$_SESSION[RPP]' size='4'> records for each page
    </form>
    <form method='post' action='$WIZ&op=4&hop=8'>
     <input type='submit' value='Go'> to page <select name='pg' onChange='javascript:submit()'>$pages</select>
    </form>
   ");
  } else {
   echo("<p><b>SELECT FAILED!</b> - check your query<br>Error: ".mysql_error()."</p>\n");
  }
 } else {
  echo("<p><b>Q</b>uery results:</p>\n");
  if(file_exists($_FILES[qrf][tmp_name])) {
   $queries=file($_FILES[qrf][tmp_name]);
   for($i=0;$i<count($queries);$i++) {
    $queries[$i]=trim($queries[$i]);
    if($queries[$i][0]!="#") $new_queries[]=$queries[$i];
   }
   $qr=split(";\n",implode("\n",$new_queries));
  } else {
   $qr=split(";\n",stripslashes($_POST[qr]));
  }
  foreach($qr as $qry) {
   if(trim($qry)) echo("<p>".((mysql_query(trim($qry),$dbl))?"<b>OK!</b> - $qry<br>":"<b>FAILED!</b> - $qry")."</p>\n");
  }
 }
 if(!$_SESSION[TBN]) echo("<p><a href='$WIZ&op=1'><b>&gt;&gt; Show tables</b></a></p>");
 else echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//5:DELETE RECORD
case "5":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 echo("<p>".((mysql_query("DELETE FROM `$_SESSION[TBN]` WHERE ".base64_decode($_POST[del])." 1 LIMIT 1",$dbl))?"Record deleted":"Unable to delete record")."</p>");
 echo("<p><a href='$WIZ&op=4&hop=9b'><b>&gt;&gt; Browse table</b></a></p>");
 break;

//6:EMPTY TABLE
case "6":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 mysql_query("TRUNCATE TABLE `$_SESSION[TBN]`");
 echo("<b>Table $_SESSION[TBN] is now empty.</b></p>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//7:DROP TABLE
case "7":
 if(!$_GET[table2drop]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 mysql_query("DROP TABLE `$_GET[table2drop]`");
 echo("<b>Table $_GET[table2drop] dropped.</b></p>");
 echo("<p><a href='$WIZ&op=1'><b>&gt;&gt; List tables</b></a></p>");
 unset($_SESSION[TBN]);
 break;

//8:INSERT/EDIT RECORD
case "8":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 echo("<form method='post' action='$WIZ&op=9'><input type='hidden' name='edit' value='$_POST[edit]'>");
 if($_POST[edit]) $rc=mysql_fetch_row(mysql_query("SELECT * FROM $_SESSION[TBN] WHERE ".base64_decode($_POST[edit])." 1 LIMIT 1",$dbl));
 $campi=mysql_list_fields($_SESSION[DBN],$_SESSION[TBN],$dbl);
 $cols=mysql_num_fields($campi);
 echo("<p><table>");
 for($i=0;$i<$cols;$i++) echo("<tr><td align='right'><b>".mysql_field_name($campi,$i)."</b>: </td><td>".((mysql_field_type($campi,$i)=="blob")?"<textarea cols='40' rows='4' name='".mysql_field_name($campi,$i)."'>$rc[$i]</textarea>":"<input type='text' name='".mysql_field_name($campi,$i)."' value='$rc[$i]' size='50'>")."</td></tr>");
 echo("</table></p>");
 echo("<input type='submit' value='Save'> <input type='reset' value='Reset'> <input type='button' value='Back to table content' onClick='javascript:window.location=\"$WIZ&op=4&hop=9b\"'></form>");
 break;

//9:SAVE RECORD
case "9":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 $campi=mysql_list_fields($_SESSION[DBN],$_SESSION[TBN],$dbl);
 $cols=mysql_num_fields($campi);
 for($i=0;$i<$cols;$i++) {
  $field="$"."_POST['".mysql_field_name($campi,$i)."']";
  eval("\$field=$field;");
  //$fields.="`".mysql_field_name($campi,$i)."`='" . str_replace("\$","\\$",$field) . "', ";
  $fields.= mysql_field_name($campi,$i)." = '" . addslashes($field) . "', ";
	//echo $field . " = " . addslashes($field) . "<br>";
 }
 //eval("\$fields=\"$fields\";");
 $fields=substr($fields,0,-2);
 $qry=($_POST[edit])?"UPDATE $_SESSION[TBN] SET $fields WHERE ".base64_decode($_POST[edit])." 1 LIMIT 1":"INSERT INTO $_SESSION[TBN] SET $fields";
 //echo((mysql_query($qry,$dbl))?"Query executed":"Error executing query <br>($qry, $dbl)");
 echo((mysql_query($qry,$dbl))?"Query executed <br> ($qry)":"Error executing query <br>($qry)");
 echo("<p><a href='$WIZ&op=4&hop=9b'><b>&gt;&gt; Browse table</b></a></p>");
 break;

//10:SET CSV IMPORT
case "10":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 echo("<p><b>S</b>elect <b>CSV</b> file to import into <b>$_SESSION[DBN].$_SESSION[TBN]</b>:</p>
  <form action='$WIZ&op=11' method='post' enctype='multipart/form-data'>
   <p>CSV file: <input name='csv' type='file'></p>
   <p><input type='submit' value='Import CSV'></p>
  </form>");
 break;

//11:IMPORT CSV
case "11":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 if(!mysql_query("LOAD DATA LOCAL INFILE '".$_FILES['csv']['tmp_name']."' REPLACE INTO TABLE `$_SESSION[TBN]` FIELDS TERMINATED BY ',' ENCLOSED BY '\"' ESCAPED BY '\\\' LINES TERMINATED BY '\n'",$dbl)) $no="NOT";
 echo("<p><b>CSV $no imported into $_SESSION[DBN].$_SESSION[TBN]</b></p>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//12:CREATE TABLE
case "12":
 echo("<p>Define <b>$_POST[tablefields]</b> fields for new table <b>$_POST[tablenew]</b>:</p>");
 echo("<form method='post' action='$WIZ&op=13&tablefields=$_POST[tablefields]&tablenew=$_POST[tablenew]'>");
 CreateTableStructure($_POST[tablefields],NULL);
 echo("<p><input type='submit' value='Create table'></p></form>");
 break;

//13:CREATE TABLE EXECUTE
case "13":
 $queryindex=array();
 echo("<p>Creating table <b>$_GET[tablenew]</b>:</p>");
 $query="CREATE TABLE `$_GET[tablenew]` (";
 for($i=0;$i<$_GET[tablefields];$i++) {
  $query.="`".$_POST[field][$i]."` ";
  $query.=$_POST[type][$i]." ";
  if($_POST[len][$i]) $query.="(".stripslashes($_POST[len][$i]).") ";
  $query.=$_POST[attr][$i]." ";
  $query.=$_POST[null][$i]." ";
  if($_POST[def][$i]) $query.=" DEFAULT '".$_POST[def][$i]."' ";
  $query.=$_POST[extra][$i]." ";
  if($_POST[index][$i]) {
   if($_POST[index][$i]=="INDEX") $queryindex[]="ALTER TABLE $_GET[tablenew] ADD INDEX (".$_POST[field][$i]."); ";
   else $query.=$_POST[index][$i];
  }
  $query.=", ";
 }
 $query=substr($query,0,-2).");";
 echo("<p>");
 if(@mysql_query($query,$dbl)) {
  foreach($queryindex as $qi) @mysql_query($qi,$dbl);
  echo("Table <b>$_GET[tablenew]</b> created");
  $_SESSION[TBN]=$_GET[tablenew];
  $_SESSION[PG]=0;
  $_SESSION[WHERE]="";
  $_SESSION[msg]="Table <b>$_SESSION[DBN].$_GET[tablenew]</b> created";
 } else {
  echo("Error creating table <b>$_GET[tablenew]</b>");
 }
 echo("</p>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//14:OPTIMIZE TABLE
case "14":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 mysql_query("OPTIMIZE TABLE $_SESSION[TBN]");
 echo("<b>Table $_SESSION[TBN] optimized.</b></p>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//15:DROP FIELD FROM TABLE
case "15":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 echo("<b>".((mysql_query("ALTER TABLE `$_SESSION[TBN]` DROP COLUMN `$_GET[field]`"))?"Field &quot;$_GET[field]&quot; from table &quot;$_SESSION[TBN]&quot; dropped.":"Unable to drop Field &quot;$_GET[field]&quot; from table &quot;$_SESSION[TBN]&quot;")."</b></p>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//16:ALTER TABLE
case "16":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 echo((($_GET[act]==1)?"Edit field <b>$_GET[field]</b>":"Add field") . " into table <b>$_SESSION[DBN].$_SESSION[TBN]</b>");
 echo("<form method='post' action='$WIZ&hop=11&op=17&act=$_GET[act]&field=$_GET[field]'>");
 if($_GET[act]==1) $col_info=mysql_fetch_object(mysql_query("SHOW COLUMNS FROM `$_SESSION[TBN]` LIKE '$_GET[field]'"));
 CreateTableStructure(1,$col_info);
 if($_GET[act]==2) echo("<p>Add field <select name='where'><option value='AFTER'>AFTER COLUMN $_GET[field]</option><option value='FIRST'>AT BEGINNING OF TABLE</option><option value='LAST'>AT END OF TABLE</option></select></p>\n");
 echo("<p><input type='submit' value='".(($_GET[act]==1)?"Edit field":"Add field")."'></p></form>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//17:ALTER TABLE EXECUTE
case "17":
 echo("<p>Alter table <b>$_SESSION[TBN]</b>:</p>");
 $query="ALTER TABLE `$_SESSION[TBN]` ";
 $query.=(($_GET[act]=="1")?"CHANGE `$_GET[field]` `".$_POST[field][0]:"ADD `".$_POST[field][0])."` ";
 $query.=$_POST[type][0]." ";
 if($_POST[len][0]) $query.="(".stripslashes($_POST[len][0]).") ";
 $query.=$_POST[attr][0]." ";
 $query.=$_POST[null][0]." ";
 if($_POST[def][0]) $query.=" DEFAULT '".$_POST[def][0]."' ";
 $query.=$_POST[extra][0]." ";
 if($_POST[index][0]) {
  if($_POST[index][0]=="INDEX") $queryindex="ALTER TABLE `$_SESSION[TBN]` ADD INDEX (".$_POST[field][0]."); ";
  else $query.=$_POST[index][0];
 }
 if($_POST[where]) {
  switch($_POST[where]) {
   case "FIRST":
    $query.=" FIRST";
    break;
   case "AFTER":
    $query.=" AFTER `$_GET[field]`";
    break;
  }
 }
 echo("<p>");
 if(@mysql_query($query,$dbl)) {
  @mysql_query($queryindex,$dbl);
  echo("Table <b>$_SESSION[TBN]</b> altered");
  $_SESSION[msg]="Table <b>$_SESSION[TBN]</b> altered";
 } else {
  echo("Unable to alter table <b>$_SESSION[TBN]</b>");
 }
 echo("</p>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//18:DROP INDEX FROM TABLE
case "18":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 echo("<b>".((mysql_query("ALTER TABLE `$_SESSION[TBN]` DROP INDEX $_GET[index]"))?"Index &quot;$_GET[index]&quot; from table &quot;$_SESSION[TBN]&quot; dropped.":"Unable to drop Index &quot;$_GET[index]&quot; from table &quot;$_SESSION[TBN]&quot;")."</b></p>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//19:CREATE INDEX ON MULTIPLE COLUMNS
case "19":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 $campi=mysql_list_fields("$_SESSION[DBN]","$_SESSION[TBN]",$dbl);
 $cols=mysql_num_fields($campi);
 for($i=0;$i<$cols;$i++) $field_list.="<option value='".mysql_field_name($campi,$i)."'>".mysql_field_name($campi,$i)."</option>\n";
 echo("<p><b>Create index on $_GET[cols] columns</b></p>");
 echo("<form method='post' action='$WIZ&hop=11&op=20'><table>
  <tr><td align='right'>Index name: </td><td><input type='text' name='idx_name' size='20'></td></tr>
  <tr><td align='right'>Index type: </td><td><select name='idx_type'><option value='PRIMARY KEY'>PRIMARY</option><option value='INDEX'>INDEX</option><option value='UNIQUE'>UNIQUE</option><option value='FULLTEXT'>FULLTEXT</option></select> (* a table can have only one PRIMARY KEY, that's always called PRIMARY)</td></tr>");
 for($i=0;$i<$_GET[cols];$i++) echo("<tr><td align='right'>Column number $i: </td><td><select name='idx_col[]'><option value=''>-- none</option>$field_list</select></td></tr>");
 echo("<tr><td>&nbsp;</td><td><input type='submit' value='Create index'></td></tr></table></form>");
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//20:CREATE INDEX ON MULTIPLE COLUMNS EXECUTE
case "20":
 echo("<p>Creating index for table <b>$_SESSION[TBN]</b>:</p>");
 $query="ALTER TABLE `$_SESSION[TBN]` ADD $_POST[idx_type] `$_POST[idx_name]` (";
 foreach($_POST[idx_col] as $col) {
  if($col) $query.="`$col`, ";
 }
 $query=substr($query,0,-2).")";
 if(@mysql_query($query,$dbl)) {
  echo("<p>Index <b>$_POST[idx_name]</b> for table <b>$_SESSION[TBN]</b> created</p>");
  $_SESSION[msg]="Table <b>$_SESSION[TBN]</b> altered";
 } else {
  echo("<p>Unable to create index <b>$_POST[idx_name]</b> for table <b>$_SESSION[TBN]</b></p>");
 }
 echo("<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//21:RENAME TABLE
case "21":
 if(!$_SESSION[TBN]) die("<script language='javascript'>window.location='$WIZ&op=1'</script>");
 echo("<p><b>");
 if(mysql_query("ALTER TABLE `$_SESSION[TBN]` RENAME `$_POST[new_table_name]`")) {
  echo("Table &quot;$_SESSION[TBN]&quot; renamed as &quot;$_POST[new_table_name]&quot;.");
  $_SESSION[TBN]=$_POST[new_table_name];
 } else {
  echo("Unable to rename table &quot;$_SESSION[TBN]&quot;");
 }
 echo("</b></p>\n<p><a href='$WIZ&op=2'><b>&gt;&gt; Table properties</b></a></p>");
 break;

//999:CREDITS
case "999":
 echo("
  <p><b>Maintainer</b>:<br>
   &nbsp; <font color='#3366AA'><b>Marco Avidano</b></font>
  </p>
  <p><b>Some information about this program</b>:<br>
   &nbsp; Project name: <font color='#3366AA'><b>$VER[NAME]</b></font><br>
   &nbsp; Major version: <font color='#3366AA'><b>$VER[MAJOR]</b></font><br>
   &nbsp; Minor version: <font color='#3366AA'><b>$VER[MINOR]</b></font><br>
   &nbsp; Build: <font color='#3366AA'><b>$VER[BUILD]</b></font><br>
   &nbsp; Shortly: <font color='#3366AA'><b>$VER[MAJOR].$VER[MINOR]</b></font><br>
   &nbsp; Web site: <font color='#3366AA'><b>$VER[WEB]</b></font><br>
   &nbsp; URL: <a href='$VER[URL]' target='_blank'><b>$VER[URL]</b></a>
  </p>
  <p><b>Support my work</b>:<br>
   WizMySQLAdmin is totally free. I don't make any profit by its development and maintenance.<br>
   If you think it's great and useful, you can make me a little donation through Paypal.<br>
   If you want go to:<br>
    <a href='https://www.paypal.com/xclick/business=paypal%40wizshelf.org&item_name=WizMySQLAdmin&no_note=1&tax=0&currency_code=EUR' target='_blank'>https://www.paypal.com/xclick/business=paypal%40wizshelf.org&item_name=Wiz%27s+Shelf+-+WizMySQLAdmin&no_note=1&tax=0&currency_code=EUR</a><br>
   and fill the Paypal form to make me a donation.<br>
   Thank you in advance!
  <p><b>License</b>:<br>
   Copyright &copy; 2004-2007 Wiz's Shelf<br>
   This program is free software; you can redistribute it and/or modify<br>
   it under the terms of the GNU General Public License as published by<br>
   the Free Software Foundation; either version 2 of the License, or<br>
   (at your option) any later version.<br>
   This program is distributed in the hope that it will be useful,<br>
   but WITHOUT ANY WARRANTY; without even the implied warranty of<br>
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the<br>
   GNU General Public License for more details.<br>
   You should have received a copy of the GNU General Public License<br>
   along with this program; if not, write to the Free Software<br>
   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA<br>
   See <a href='http://www.gnu.org/licenses/gpl.html' target='_blank'>http://www.gnu.org/licenses/gpl.html</a> for the complete text of the license.
  </p>
  <p><b>Something else</b>:<br>
   &nbsp; &quot;I don't know half of you half as well as I should like;<br>
   &nbsp; and I like less than half of you half as well as you deserve.&quot;<br>
   &nbsp; - Bilbo Baggins
  </p>
  <p>&nbsp;</p>
 ");
 $_SESSION[msg]="Take it easy!";
 break;

//DEFAULT: SHOW DATABASES
default:
 echo("<table cellspacing='0' cellpadding='2' border='0' style='border:1pt solid #666666;border-collapse:collapse;'>
  <tr style='background-color:#DDDDDD;'>
   <th width='100'>DATABASE LIST</th>
  </tr>");
 $rs=mysql_list_dbs($dbl);
 while($rc=mysql_fetch_object($rs)) {
  $bgcolor=($bgcolor=="#EEEEEE")?"#FFFFFF":"#EEEEEE";
  echo("<tr style='background-color:$bgcolor' onMouseOver='javascript:this.style.backgroundColor=\"#CCFF00\"' onMouseOut='javascript:this.style.backgroundColor=\"$bgcolor\"'><td><a href='$WIZ&op=1&hop=1&dbn=$rc->Database'><b>$rc->Database</b></a></td></tr>\n");
 }
 echo("</table>");
 break;
}
?>
  </td>
 </tr>
</table>

<table cellspacing="0" cellpadding="0" width="100%" border="0">
 <tr valign="middle" height="20" bgcolor="#DDDDDD">
  <td width="100%">&nbsp; <b>Log:</b> <?php echo($_SESSION[msg]); ?></td>
 </tr>
</table>

<table cellspacing="0" cellpadding="0" width="100%" border="0" valign="middle"><tr height="20">
 <th bgcolor="#CCCCCC">Copyright &copy; 2004-2007 <a href="<?php echo($VER[URL]); ?>" target='_blank'><?php echo($VER[WEB]); ?></a></td>
</tr></table>
</body>
</html>
<?php
mysql_close($dbl);
//	printPara("Please put info here to manage database");
}

function CreateTableStructure($FieldNum,$FieldStructure) {
 echo("<table cellspacing='0' cellpadding='2' border='0' style='border:1pt solid #666666;border-collapse:collapse;'>
   <tr style='background-color:#DDDDDD;'><th>FIELD</th><th>TYPE</th><th>LENGTH/VALUES*</th><th>ATTRIBUTES</th><th>NULL</th><th>DEFAULT</th><th>EXTRA</th><th>INDEX</th></tr>\n");
 for($i=0;$i<$FieldNum;$i++) {
  $bgcolor=($bgcolor=="#EEEEEE")?"#FFFFFF":"#EEEEEE";
  $field_type=strtoupper(substr($FieldStructure->Type,0,strpos($FieldStructure->Type,"(")));
  $field_values=substr($FieldStructure->Type,strpos($FieldStructure->Type,"(")+1,strpos($FieldStructure->Type,")")-strpos($FieldStructure->Type,"(")-1);
  $field_attr=split(" ",$FieldStructure->Type,2);
  echo("<tr align='center' style='background-color:$bgcolor' onMouseOver='javascript:this.style.backgroundColor=\"#CCFF00\"' onMouseOut='javascript:this.style.backgroundColor=\"$bgcolor\"'>
   <td><input type='text' name='field[$i]' style='width:70pt;' value='$FieldStructure->Field'></td>
   <td>
    <select name='type[$i]'>");
     $types=array("","TINYINT","SMALLINT","MEDIUMINT","INT","BIGINT","DOUBLE","DECIMAL","FLOAT","DATE","TIME","TIMESTAMP","DATETIME","YEAR","VARCHAR","TINYTEXT","TEXT","MEDIUMTEXT","LONGTEXT","TINYBLOB","BLOB","MEDIUMBLOB","LONGBLOB","ENUM","SET");
     foreach($types as $type) echo("<option value='$type' ".(($type==$field_type)?"selected":"").">$type</option>\n");
     echo("</select>
   </td>
   <td><input type='text' name='len[$i]' style='width:100pt;' value=\"$field_values\"></td>
   <td>
    <select name='attr[$i]'>");
     $attrs=array("","BINARY","UNSIGNED","UNSIGNED ZEROFILL");
     foreach($attrs as $attr) echo("<option value='$attr' ".(($attr==strtoupper($field_attr[1]))?"selected":"").">$attr</option>\n");
     echo("</select>
   </td>
   <td>
    <select name='null[$i]'>");
     $nulls=array("NOT NULL","NULL");
     foreach($nulls as $null) echo("<option value='$null' ".(($FieldStructure->Null=="YES" && $null=="NULL")?"selected":"").">$null</option>\n");
     echo("</select>
   </td>
   <td><input type='text' name='def[$i]' style='width:50pt;'  value=\"$FieldStructure->Default\"></td>
   <td>
    <select name='extra[$i]'>");
     $extras=array("","AUTO_INCREMENT");
     foreach($extras as $extra) echo("<option value='$extra' ".(($FieldStructure->Extra=="auto_increment" && $extra=="AUTO_INCREMENT")?"selected":"").">$extra</option>\n");
     echo("</select>
   </td>
   <td>
    <select name='index[$i]'>");
     $indexs=array("","PRIMARY KEY","INDEX","UNIQUE KEY");
     foreach($indexs as $index) echo("<option value='$index' ".(($FieldStructure->Key=="PRI" && $index=="PRIMARY KEY")?"selected":"") . (($FieldStructure->Key=="MUL" && $index=="INDEX")?"selected":"") . (($FieldStructure->Key=="UNI" && $index=="UNIQUE KEY")?"selected":"").">$index</option>\n");
     echo("</select>
   </td>
  </tr>");
 }
 echo("</table>
  <p>*
    For &quot;enum&quot; and &quot;set&quot; fields enter the values using this format: 'first','second','third'.<br>
    Escape backslash or single quote with a backslash (example: 'first','se\'cond','thi\\\\rd').
  </p>");
 return;
}


function printCheckForProblems($func, $confirm)
{
	$mode = "check";
	printSectionTitle("Check for Problems");
	//printLine("Check for problems function will check the following (amongst other things):");
	//printLine("* Makes sure that the temporary 'Data' directory is empty. If it is not, have the option to clear it.''");
	//printLine("* Makes sure that the temporary 'mpp' directory is empty. If it is not, have the option to clear it.''");
	//printLine("* Makes sure that every plugin record in the database has files that actually exist in the directory structure");
	//printLine("* Makes sure that every plugin directory has a corresponding record in the database");

	//if confirm FALSE show confirmation link
	if ($func && $confirm == FALSE) {
		echo "You are about to permanently delete directories.<br><br>";
		echo "OK to proceed? [<a href='admin.php?mode=$mode&func=$func&confirm=TRUE'>OK</a>] [<a href='admin.php?mode=$mode'>CANCEL</a>]<br>";
		}

		else {
	
		if ($func == "deldata" && $confirm) {
			$data_files = array();
			$data_files = file_list("data/", "*", 1, TRUE);
			$data_files_count = count($data_files);
			if ($data_files_count > 0 )
			{
				printLine("");
				printLine("The following directories are being deleted: <br>");
				foreach ($data_files as $file) {
					$diff = floor((time() - filemtime("$file"))/60/60/24);
					if ($diff > 0) echo "$file is $diff days old.<br>";
					remove_directory($file);
					}
				printLine("<br>Click <a href='admin.php?mode=check'>here</a> to go back.");
			}
		}
	
		if ($func == "delmpp" && $confirm) {
			$data_files = array();
			$data_files = file_list("mpp/", "*", 1, TRUE);
			$data_files_count = count($data_files);
			if ($data_files_count > 0 )
			{
				printLine("");
				printLine("The following directories are being deleted: <br>");
				foreach ($data_files as $file) {
					$diff = floor((time() - filemtime("$file"))/60/60/24);
					if ($diff > 0) echo "$file is $diff days old.<br>";
					remove_directory($file);
					}
				printLine("<br>Click <a href='admin.php?mode=check'>here</a> to go back.");
			}
		}
	}

	if (!$func) {
		$data_files = array();
		$data_files = file_list("data/", "*", 1, TRUE);
		$data_files_count = count($data_files);
		if ($data_files_count > 0 )
		{
			printLine("<hr>");
			printLine("There are $data_files_count temp directories in the data directory! This could cause problems when uploading new plugins. <br>");
			foreach ($data_files as $file) {
				$diff = floor((time() - filemtime("$file"))/60/60/24);
				if ($diff > 0) echo "$file is $diff days old.<br>";
				}
			printLine("<br>Click <a href='admin.php?mode=check&func=deldata'>here</a> to clear directories more than 1 day old.");
			echo "<br>";
		}
		else printLine("Data directory appears to be OK...");
	
		$mpp_files = array();
		$mpp_files = file_list("mpp/", "*", 1, TRUE);
		$mpp_files_count = count($mpp_files);
		if ($mpp_files_count > 0 )
		{
			printLine("<hr>");
			printLine("There are $mpp_files_count temp directories in the mpp directory! This could cause problems when uploading new plugins. <br>");
			foreach ($mpp_files as $file) {
				$diff = floor((time() - filemtime("$file"))/60/60/24);
				if ($diff > 0) echo "$file is $diff days old.<br>";
				}
			printLine("<br>Click <a href='admin.php?mode=check&func=delmpp'>here</a> to clear directories more than 1 day old.");
			echo "<br>";
		}
		else printLine("MPP directory appears to be OK...");

		//check records in db and check if plugin exists in plugin_home_directory
		//if plugin file doesn't exist, remove record from db or flag it.
	
		//check plugins in plugin_home_directory and check if it has corresponding db entry
		//if plugin file exists, but no db record...remove the plugin file.

		printLine("Done...");
	}
}


function printSourceCodeArchive()
{
	$source_file = "openmaid_source.gzip";
	printSectionTitle("Generate Source Code Archive");
	echo "Deleting existing archive...";
	if (file_exists($source_file)) @unlink ($source_file);
	echo "Done.<br />\n";
	echo "Generating new gzip archive ($source_file)...";
	
	// Create new bzip file in the directory below the current one 
	$test = new gzip_file($source_file); 
	// All files added will be relative to the directory in which the script is 
	//    executing since no basedir is set. 
	// Do not recurse through subdirectories 
	// Set compression level to max 9
	$test->set_options(array('recurse' => 0, 'level' => 9)); 
	// Add files to archive 
	$test->add_files(array("*.gif", "*.png", "*.jpg", "*.css", "PopularityContest.php", "admin.php", "archive.php", "authentication.php.sample", "db.php", "edit.php", "detail.php", "dl.php", "feedcreator.class.php", "fetchplugins.php", "footer.php", "functions.php", "header.php", "image_functions.php", "images.php", "index.php", "pclzip.lib.php", "process.php", "rss.php", "thelist.php", "upload.php", "virtual_auth.php", "vote.php", "xtern.php", "extra.php", "process.functions.php", "config.php.sample", "private.forum_integration.php.sample")); 
	//$test->add_files(array("*.gif", "*.png", "*.jpg", "*.css", "*.php")); 
	// Add all images directory to archive 
	$test->add_files(array("images/*.jp*g", "images/*.gif", "images/*.png")); 
	// Create archive 
	$test->create_archive(); 

	if (count($test->errors) > 0) {
		echo "Errors occurred.<br />"; // Check for errors
		slowDie("Failed to create source code gzip file: $source_file");
		}
	else {
		echo "SUCCESS!<br />\n";
		echo "Click <a href='$source_file'>here</a> to download $source_file<br>";
		}
}

function printSyncCode()
{
	printSectionTitle("Sync Source Code With MAID Test Site");
	$source_file = "openmaid_source.gzip";
	$source_url = "http://207.44.156.88/~vinny/OpenMaid/openmaid_source.gzip";

	//download archive from test site
	echo "<b>Downloading archive from test site...";
	//if (file_exists($source_file)) delfile($source_file);
	$contents = file_get_contents($source_url,FALSE);
	echo "Done. <br>";
	echo "Saving...$source_file...";
	//store archive file locally
	if (!$local_handle = fopen($source_file,"w")) die ("Can't open new local file $source_file");
	fwrite($local_handle, $contents);
	echo "Done!<br>";
	//
	//extract source code
	echo "Extracting archive...<br><br>";
	// Open local gzip source code file
	$test = new gzip_file("$source_file"); 
	// Overwrite existing files 
	$test->set_options(array('overwrite' => 1)); 
	// Extract contents of archive to disk 
	// Write out the name and size of each file extracted 
	foreach ($test->files as $file) 
	    echo "File " . $file['name'] . " is " . $file['stat'][7] . " bytes\n <br>"; 
	$test->extract_files(); 
	echo "Done.<br />\n";
	echo "<br>";
	print_r ($test->errors);
	echo "<br>";
	echo "Click <a href='admin.php'>here</a> to return to admin panel.<br>";
}

function printClearMAIDCache()
{
	$all_cache = "all.cache";
	$summary_cache = "summary.cache";
	$extended_cache = "extended.cache";
	
	delfile($all_cache);
	echo "$all_cache is deleted...<br>";
	delfile($extended_cache);
	echo "$extended_cache is deleted...<br>";
	delfile($summary_cache);
	echo "$summary_cache is deleted...<br><br>";
	
	echo "Done.<br />\n";
}

function printSyncProfileID() {
	global 	$db_host;
	global 	$db_user;
	global 	$db_pass;
	global 	$db_database;
	global 	$old_db_host;
	global 	$old_db_user;
	global 	$old_db_pass;
	global 	$old_db_database;
	global 	$old_db_plugins_table;

	//mysql_close();
	//Connect to old OpenMAID DB
	if (mysql_connect($old_db_host, $old_db_user, $old_db_pass))
	{
		if (!mysql_select_db($old_db_database) ) slowDie("Error connecting to old OpenMAID database...check config.php settings.");
	}
	else
	{
		slowDie("Error connecting to mysql...check config.php settings.");
	} 
	
	//get array with plugin ID's and their respective profil_id data
	$sql = "SELECT plugin_ID,profil_id FROM $old_db_plugins_table";
	$res = mysql_query($sql);
	$old_db_data = array();
	if (!$res) echo "No Records Found!<br/>";
	else {
		while ( $olddb=@mysql_fetch_array($res)) {
			$plugin_ID = $olddb["plugin_ID"];
			if ($olddb["profil_id"]) $old_db_data["$plugin_ID"] = $olddb["profil_id"];
			}
		}
	//close connection to db
	mysql_close();


	//Connect to new OpenMAID DB
	if (mysql_connect($db_host, $db_user, $db_pass))
	{
		if (!mysql_select_db($db_database) ) slowDie("Error connecting to new OpenMAID database...check config.php settings.");
	}
	else
	{
		slowDie("Error connecting to mysql...check config.php settings.");
	} 

	//loop through new OpenMAID DB and set profil_id to profile_id of corresponding plugin_ID of array from old DB
	foreach ($old_db_data as $key => $value) {
		$sql = "SELECT * FROM plugins WHERE plugin_ID = '$key'";
		$res = mysql_query($sql);
		echo "$key::$value<br>";
		if (!$res) {echo "No Records Found for plugin ID: $key!<br/>"; continue;}
		else {
			while ( $newdb=@mysql_fetch_array($res)) {
				$plugin_ID = $newdb["plugin_ID"];
				$plugin_State = $newdb["plugin_State"];
				$plugin_VersionValue = $newdb["plugin_VersionValue"];
				$update = "UPDATE plugins SET profil_id = '$value' WHERE plugin_ID = '$plugin_ID' and plugin_State = '$plugin_State' and plugin_VersionValue = $plugin_VersionValue";
				mysql_query($update);
				echo "<b>$update</b><br><br>\n";
				}
			}
		}
	mysql_close();
}


function printRegenDLCounts() {
	global 	$db_host;
	global 	$db_user;
	global 	$db_pass;
	global 	$db_database;

	if (mysql_connect($db_host, $db_user, $db_pass))
	{
		if (!mysql_select_db($db_database) ) slowDie("Error connecting to new OpenMAID database...check config.php settings.");
	}
	else
	{
		slowDie("Error connecting to mysql...check config.php settings.");
	} 

	//loop through new OpenMAID DB and set profil_id to profile_id of corresponding plugin_ID of array from old DB
		$sql = "SELECT * FROM plugins";
		$res = mysql_query($sql);
		if (!$res) {echo "No Records Found for plugin ID: $key!<br/>"; continue;}
		else {
			while ( $newdb=@mysql_fetch_array($res)) {
				$plugin_ID = $newdb["plugin_ID"];
				$plugin_Version = $newdb["plugin_Version"];
				//get aggregate count for this version/state of the plugin
				$sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$plugin_ID' and plugin_Version = '$plugin_Version'";	
				$thisversion_downloads = QuerySingleValue($sql, 0);
				//get aggregate count for all versions of this plugin
				$sql = "SELECT COUNT(download_profil_id) FROM userdownloads WHERE plugin_ID = '$plugin_ID'";	
				$total_downloads = QuerySingleValue($sql, 0);
				//put aggregate counts into plugins table for easy sorting in thelist.php
				$sql = "UPDATE plugins SET plugin_DownloadCount = '$thisversion_downloads', plugin_DownloadCountTotal = '$total_downloads' WHERE plugin_ID = '$plugin_ID' and plugin_Version = '$plugin_Version'";
				mysql_query($sql);
				echo "<b>$sql</b><br>\n";
				$sql = "UPDATE plugins SET plugin_DownloadCountTotal = '$total_downloads' WHERE plugin_ID = '$plugin_ID'";
				mysql_query($sql);
				echo mysql_error();
				echo "<b>$sql</b><br><br>\n";
				}
			}
	mysql_close();
}

function printSyncDLCount() {
	global 	$db_host;
	global 	$db_user;
	global 	$db_pass;
	global 	$db_database;
	global 	$old_db_host;
	global 	$old_db_user;
	global 	$old_db_pass;
	global 	$old_db_database;
	global 	$old_db_plugins_table;

	//mysql_close();
	//Connect to old OpenMAID DB
	if (mysql_connect($old_db_host, $old_db_user, $old_db_pass))
	{
		if (!mysql_select_db($old_db_database) ) slowDie("Error connecting to old OpenMAID database...check config.php settings.");
	}
	else
	{
		slowDie("Error connecting to mysql...check config.php settings.");
	} 
	
	//get array with plugin ID's and their respective profil_id data
	$sql = "SELECT plugin_ID,plugin_DownloadCount FROM $old_db_plugins_table";
	$res = mysql_query($sql);
	$old_db_data = array();
	if (!$res) echo "No Records Found!<br/>";
	else {
		while ( $olddb=@mysql_fetch_array($res)) {
			$plugin_ID = $olddb["plugin_ID"];
			if ($olddb["plugin_DownloadCount"]) $old_db_data["$plugin_ID"] = $olddb["plugin_DownloadCount"];
			}
		}
	//close connection to db
	mysql_close();


	//Connect to new OpenMAID DB
	if (mysql_connect($db_host, $db_user, $db_pass))
	{
		if (!mysql_select_db($db_database) ) slowDie("Error connecting to new OpenMAID database...check config.php settings.");
	}
	else
	{
		slowDie("Error connecting to mysql...check config.php settings.");
	} 

	//loop through new OpenMAID DB and set profil_id to profile_id of corresponding plugin_ID of array from old DB
	foreach ($old_db_data as $key => $value) {
		$sql = "SELECT * FROM plugins WHERE plugin_ID = '$key'";
		$res = mysql_query($sql);
		$num_ids = mysql_numrows($res);
		$newvalue = $value / $num_ids;
		echo "ID = $key <br>Old DL Count = $value<br>Number of Versions in DB = $num_ids<br>Adding $newvalue to each version of this ID<br>";
		if (!$res) {echo "No Records Found for plugin ID: $key!<br/>"; continue;}
		else {
			while ( $newdb=@mysql_fetch_array($res)) {
				for ($i = 1; $i <= $newvalue; $i++) {
					$plugin_ID = $newdb["plugin_ID"];
					$plugin_Version = $newdb["plugin_Version"];
					$update = "INSERT INTO userdownloads (plugin_ID, plugin_Version, download_Date, download_profil_id) VALUES ('$plugin_ID','$plugin_Version',NOW(),'Guest')";
					echo "$update<br><hr>";
					//mysql_query($update);
					}
				}
			}
		}
	mysql_close();
}



if ($mode != "managedb"){
	echo '</div><div id="rightbar">';
	echo '<a href="admin.php?mode=home">Admin Home</a><br />';
	echo '<a href="fetchplugins.php">Sync w/ Meedio MAID</a><br />';
	echo '<a href="process.php?mode=manage_all">Manage Repository</a><br />';
	echo '<a href="process.php?mode=manage_allplugins">Manage Plugins</a><br />';
	echo '<a href="admin.php?mode=managedb">Manage Database</a><br />';
	echo '<a href="admin.php?mode=dbinfo">Database Information</a><br />';
	echo '<a href="admin.php?mode=phpinfo">PHP Information</a><br />';
	echo '<a href="admin.php?mode=initdb">Initialize Database</a><br />';
	echo '<a href="admin.php?mode=initdir">Initialize Directories</a><br />';
	echo '<a href="admin.php?mode=check">Check for Problems</a><br />';
	echo '<a href="admin.php?mode=clearcache">Clear MAID Cache Files</a><br />';
	echo '<a href="admin.php?mode=regendlcounts">Regen DL Counts</a><br />';
	echo '<a href="admin.php?mode=syncprofilid">Sync Profile ID w/old DB</a><br />';
	echo '<a href="admin.php?mode=syncdlcount">Sync DL Count w/old DB</a><br />';
	echo '<a href="http://207.44.156.88/~vinny/OpenMaid/admin.php?mode=sourcecode" target="_blank">Archive Source Code</a><br />';
	echo '<a href="admin.php?mode=synccode">Sync Source Code</a><br />';
	echo '<a href="http://207.44.156.88/~vinny/OpenMaid/openmaid_source.gzip">Download Source Code</a><br />';
	echo '</div>';
	}
	
include('footer2.php');
?>
