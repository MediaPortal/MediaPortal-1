<?php
require_once('functions.php');
require_once('db.php');
include('header.php');

set_time_limit(7200);

echo "</td></tr><tr><td id=\"contentarea\"><div id=\"widebar\">";

$file[0] = "http://www.meedios.com/OpenMAID/xtern.php?sync";
$site[0] = "MeediOS.com OpenMAID v2";
$prepend[0] = "meediosv2_";
$file[1] = "http://207.44.156.88/~vinny/OpenMaid/xtern.php?sync";
$site[1] = "MeediOS.com OpenMAID v2 Beta Site";
$prepend[1] = "meediostest_";
$file[2] = "http://www.meedio.com/maid/xtern.php?summary";
$site[2] = "Meedio.com MAID";
$prepend[2] = "meedio_";


$total_downloaded = 0;
$skipped = 0;
$site_count = 0;
$sites = count($file);

while ($site_count < $sites) {

	$lines = file($file[$site_count]);

	$lines_total = count($lines);
	$end_line = $lines_total - 2;
	echo "<font face=\"Verdana, Arial, Helvetica, sans-serif\" size=\"2\"><b>Syncing FTP directory with files from $site[$site_count]</b><br><br>";
	echo "<font face=\"Verdana, Arial, Helvetica, sans-serif\" size=\"1\">";
	//start at line 3 go to $end_line

	$line = 3;
	$counter = 1;
	$downloaded = 0;

	while ($line <= $end_line)
		{
		ob_flush(); flush(); ob_flush(); flush();
		$local_dir = $ftp_repository;
	 	//parse line for id=0, version=1, type=2, state=3, name=4, url=5
		$newline = $lines[$line];
		if (substr(trim($newline),0,11) != "<plugin id=") { $line++; continue; }
		$newline = str_replace("<","",$newline);
		$newline = str_replace("/>","",$newline);
		$newline = trim($newline);
		$properties = explode ("\" ",$newline);
		$id = explode ("=\"",$properties[0]);
		$version = explode ("=\"",$properties[1]);
		$version[1] = str_replace(" ","_",$version[1]);
		$type = explode ("=\"",$properties[2]);
		$type[1] = strtolower($type[1]);
		$state = explode ("=\"",$properties[3]);
		$state[1] = strtolower($state[1]);
		$name = explode ("=\"",$properties[4]);
		$name[1] = strtolower($name[1]);
		$name[1] = str_replace(" ","_",$name[1]);
		$url = explode ("=\"",$properties[5]);
		$url[1] = trim($url[1],"\"");
	
		//generate new name from data
		$filename = $prepend[$site_count] . $id[1]."_".$version[1]."_".$type[1]."_".$state[1]."_".$name[1].".mpp";
		$filecheck = "*".$id[1]."_".$version[1]."_".$type[1]."_".$state[1]."_"."*";
		$filecheck = addslashes($filecheck);
		$filename = str_replace("?","~",$filename);
		$filename = str_replace("+","~",$filename);
		$filename = str_replace("/","~",$filename);
		$filename = str_replace("\\","~",$filename);
		$filename = str_replace("*","~",$filename);
		$filename = str_replace("|","~",$filename);
		$filename = str_replace("\"","~",$filename);
		$filename = str_replace("<","[",$filename);
		$filename = str_replace(">","]",$filename);
		$local_file = $local_dir.$filename;
		$local_filecheck = $local_dir.$filecheck;
		$local_file_pass = $local_dir.$filename.".pass";
		$local_file_fail = $local_dir.$filename.".fail";
		$dupe_found = "";
		if ( $dir = opendir($local_dir) ){
		    while ($filelist = readdir($dir)){
				//echo $filecheck . " : " .eregi($filecheck , $filelist) . "<br>";
 		        if (preg_match($filecheck, $filelist)) $dupe_found =  $filelist ;
				}
			}
		closedir($dir);

		if ($dupe_found !="") {
			$line++; $skipped++; 
			//echo "Skipping...$filename...Done!<br>";
			continue;
			}

		echo "<b>Downloading...</b>";
		if (DoesPluginExist($id[1], str_replace("_"," ",$version[1]), $state[1])) {
			$filename .= ".pass";
			$local_file .= ".pass";
			echo "Exists in DB but not in FTP repository. Adding to repository...";
			}
		$contents = file_get_contents($url[1],FALSE);

		echo "Saving...$filename...";
		//store new file locally
		if (!$local_handle = fopen($local_file,"w")) die ("Can't open new local file $local_file");
		fwrite($local_handle, $contents);
		echo "Done!<br>";
		$line++;
		$counter++;
		$downloaded++;
		ob_flush(); flush(); ob_flush(); flush();
		}
	echo "</font><font face=\"Verdana, Arial, Helvetica, sans-serif\" size=\"2\">";
	if ($downloaded == 1) echo "<br>Downloaded " . ($downloaded) . " plugin.<br><br>";
	else echo "<br>Downloaded " . ($downloaded) . " plugins.<br><br>";
	echo "</font><hr />";
	$site_count++;
	$total_downloaded = $total_downloaded+$downloaded;
	}
	
$unprocessed_plugins = array();
findFiles($unprocessed_plugins,$ftp_repository."*.mpp");
$unprocessed_count = count($unprocessed_plugins);

$passed_plugins = array();
findFiles($passed_plugins,$ftp_repository."*.mpp.pass");
$passed_count = count($passed_plugins);

$failed_plugins = array();
findFiles($failed_plugins,$ftp_repository."*.mpp.fail");
$failed_count = count($failed_plugins);

$total_archived = $unprocessed_count+$passed_count+$failed_count;

echo "<font face=\"Verdana, Arial, Helvetica, sans-serif\" size=\"2\">";
echo "<b>SUMMARY<br></b>";
echo "$total_archived plugins in our file repository<br>";
echo "( Unprocessed : $unprocessed_count | Successfully Processed: $passed_count | Failed Processing : $failed_count )<br>";
//echo "$skipped current MAID plugins skipped, we already have these.<br>";
echo "$total_downloaded plugins downloaded during this session.<br><hr />";
if ($unprocessed_count > 0) {
	echo "Click <a href='process.php?mode=process_new'>here</a> to auto process new plugins.<br>";
	echo "Click <a href='process.php?mode=manage_new'>here</a> to manually process new plugins.<br>";
	}
else echo "No new plugins to process. Click <a href='admin.php'>here</a> to return to admin panel.";
echo "</font></div>";

include('footer.php');

?>