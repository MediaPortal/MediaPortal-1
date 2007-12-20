<?php

function process_new($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode) {
	
	echo "Scanning for files on server... ";

	$new_plugins = array();
	
	findFiles($new_plugins,$ftp_repository . "*.mpp");
	findFiles($new_plugins,$ftp_manual_uploads . "*.mpp");
	
	$plugin_count = sizeof($new_plugins);
	echo "$plugin_count plugin";
	if ($plugin_count > 1 || $plugin_count == 0) echo "s";
	echo " found<br><br>\n";
	$plugin_count = $plugin_count - $start_at;
	
	if ($plugin_count < 0) slowDie("Error...negative plugin count shouldn't be possible.<br>Please report this error to the site admin.");
	if ($plugin_count > 0) echo "Processing plugin";
	if ($plugin_count > 1) echo "s";
	if ($plugin_count > 0) echo "...<br><br>\n";

	ob_flush();
	flush();

	$plugin_success = 0;
	$plugin_manual = 0;
	$plugin_failure = 0;
	
	if ($plugin_count < 40)
	{
		$process_count = $plugin_count;
	}
	else
	{
		$process_count = 40;
	}

	for ($i=0; $i<$process_count; $i++)
	{	
		$plugin_file = $new_plugins[$i+$start_at];
		if ($i == ($process_count - 1)) $process_multiple = FALSE;
		else $process_multiple = TRUE;
		$process_state = 1;
	 
		echo "<p> <b>$plugin_file</b>  <br>\n";
		if ((file_exists($plugin_file.".fail") || file_exists($plugin_file.".pass")) && !$status) {
			echo "Error!  .pass or .fail file exists for this plugin.<br>";
			echo "You must delete the .mpp manually or delete the .pass/.fail file to reprocess this .mpp.";
			$process_state = 0;
			}

		if ($process_state) {
			$addResult = AdminAddMpp($plugin_file, $ignore_version, $ignore_sanity, FALSE, $force_update, $process_multiple, "");
			}
		else $addResult = 0;
			
		if ($addResult == true && $process_state == true && $status != "pass") {
			$plugin_success++;
			rename($plugin_file,$plugin_file.".pass");
			}
		elseif ($addResult == true && $process_state == true && $status == "pass") $plugin_success++;
		elseif ($status == "pass") $plugin_failure++;
		elseif ($status != "pass") {
			$plugin_failure++;
			rename($plugin_file,$plugin_file.".fail");
			}

		echo "<hr />";
		ob_flush();
		flush();
	}
		
	echo "<br>\n";
	echo "$plugin_success successful<br>\n";
	echo "$plugin_failure failed<br>\n";
	if ($status != "pass") $plugin_count = $plugin_count - ($plugin_success+$plugin_failure);
	else {
		$plugin_count = $plugin_count - ($plugin_success+$plugin_failure);
		$start_at = $start_at + $process_count;
		}
	echo "$plugin_count remaining<br><br>\n";

	if ($status != "pass" || $status != "fail") {
		if ($plugin_count > $process_count) echo "Click <a href='process.php?mode=process_new&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update'>here</a> to continue processing next $process_count plugins<br>\n";
		elseif ( ($plugin_count > 1) && ($plugin_count <= $process_count)) echo "Click <a href='process.php?mode=process_new&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update'>here</a> to continue processing last $plugin_count plugins<br>\n";
		elseif ($plugin_count == 1) echo "Click <a href='process.php?mode=process_new&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update'>here</a> to continue processing the last plugin<br>\n";
		else echo "Done processing new plugins.  Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
		}
	else if ($status == "pass") {
		if ($plugin_count > $process_count) echo "Click <a href='process.php?mode=process_new&cache_mode=manage_passed&file=&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update&status=pass&start_at=$start_at'>here</a> to continue reprocessing next $process_count plugins<br>\n";
		elseif ( ($plugin_count > 1) && ($plugin_count <= $process_count)) echo "Click <a href='process.php?mode=process_new&cache_mode=manage_passed&file=&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update&status=pass&start_at=$start_at'>here</a> to continue processing last $plugin_count plugins<br>\n";
		elseif ($plugin_count == 1) echo "Click <a href='process.php?mode=process_new&cache_mode=manage_passed&file=&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update&status=pass&start_at=$start_at'>here</a> to continue processing the last plugin<br>\n";
		else echo "Done reprocessing plugins.  Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
		}
	else if ($status == "fail") {
		if ($plugin_count > $process_count) echo "Click <a href='process.php?mode=process_new&cache_mode=manage_failed&file=&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update&status=fail&start_at=$start_at'>here</a> to continue reprocessing next $process_count plugins<br>\n";
		elseif ( ($plugin_count > 1) && ($plugin_count <= $process_count)) echo "Click <a href='process.php?mode=process_new&cache_mode=manage_failed&file=&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update&status=pass&start_at=$start_at'>here</a> to continue processing last $plugin_count plugins<br>\n";
		elseif ($plugin_count == 1) echo "Click <a href='process.php?mode=process_new&cache_mode=manage_failed&file=&ignore_version=$ignore_version&ignore_sanity=$ignore_sanity&force_update=$force_update&status=fail&start_at=$start_at'>here</a> to continue processing the last plugin<br>\n";
		else echo "Done reprocessing plugins.  Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
		}
}



function process_single($file, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode) {
	if (!$file) die ("Nice hack job...not!");
	if ($status != "mpp" && $status != "pass" && $status != "ignore" && $status != "fail") die ("Nice hack job...not!");
	if ($status == "mpp") $extension = ".mpp";
	if ($status == "pass") $extension = ".mpp.pass";
	if ($status == "fail") $extension = ".mpp.fail";
	if ($status == "ignore") $extension = ".mpp.ignore";
	$extension_length = strlen($extension);
	$file_length = strlen($file);
	$file_noext = substr($file, 0, $file_length-$extension_length);
	$plugin_file = $file_noext . ".mpp";
	$file_passed = $file_noext . ".mpp.pass";
	$file_failed = $file_noext . ".mpp.fail";
	$process_multiple = FALSE;  
	
	echo "<b>Processing Plugin</b><br><br>";

	$file_count = 0;
	$process_state = 1;
	 
	if (file_exists($plugin_file)) $file_count++;
	if (file_exists($plugin_file.".pass")) $file_count++;
	if (file_exists($plugin_file.".fail")) $file_count++;
	if (file_exists($plugin_file.".ignore")) $file_count++;
		
	if ($file_count > 1) {
		echo "Error!  Multiple files exist for this plugin.<br>";
		echo "You must delete them manually so there is only one.<br>";
		$process_state = 0;
		}

	if ($process_state) $addResult = AdminAddMpp($file, $ignore_version, $ignore_sanity, FALSE, $force_update, $process_multiple, "");
	else $addResult = 0;
			
	if ($addResult == true && $process_state == true) rename($file,$file_passed);
	else echo "<br>There was a failure processing the plugin...no changes were made!<br>";

	echo "<hr />";
	echo ("<br>\n");

	if ($addResult && $process_state) echo("SUCCESS - Done processing plugin.<br><br>\n");
	else echo("FAILURE<br><br>\n");

	echo ("Click <a href='process.php?mode=$cache_mode'>here</a> to return to previous screen.<br>\n");
}



function manage_all($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode) {
	$cache_mode = $mode;
	$extension = ".mpp/.mpp.pass/.mpp.fail/.mpp.ignore";
	echo "Scanning for all plugin files on server... ";

	$all_plugins = array();
	findFiles($all_plugins,$ftp_repository . "*.mpp.*");
	
	$plugin_count = sizeof($all_plugins);
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " found<br><br>\n";
	
	//Generate global actions links - delete all, reprocess all, change status (mpp, pass, ignore)
	if ($plugin_count > 0) {
		//echo "<b>Global Actions: [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=mpp2pass'>Mark All Pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=mpp2fail'>Mark All Fail</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=mpp2ignore'>Mark All Ignore</a>]</b><br>";
		echo "<b>This part is not fully implemented yet...</b><br>";
		echo "<hr />";
		}
		
	//Generate list of plugins by parsing filename
	for ($i=0; $i<$plugin_count; $i++)
	{	
		$plugin_file = $all_plugins[$i];
		displayFile($plugin_file, $extension);
		echo "Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=$plugin_file&status=file_only'>File Only</a>] [File and DB Entry]</b><br>";
		//echo "<b>Change Status: [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=mpp2pass'>pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=mpp2fail'>fail</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=mpp2ignore'>ignore</a>]<br>";
		//echo "Process: [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&status=mpp'>process</a>] [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&status=mpp'>process - ignore version error</a>]</b>";
		echo "<hr />";
		}

	//Check if plugin still has db record
	//Check if plugin is still in plugin archive (plugin directory)
	//Display appropriate error based on status of checks
	//Add links for changing status, delete, reprocessing (force-keep stats, force-clear stats)
	
	echo "Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function manage_new($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode) {
	$cache_mode = $mode;
	$extension = ".mpp";
	echo "Scanning for new files on server... ";

	$new_plugins = array();
	findFiles($new_plugins,$ftp_repository . "*.mpp");
	findFiles($new_plugins,$ftp_manual_uploads . "*.mpp");
	
	$plugin_count = sizeof($new_plugins);
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " found<br><br>\n";
	
	//Generate global actions links - delete all, reprocess all, change status (mpp, pass, ignore)
	if ($plugin_count > 0) {
		echo "<b>Global Actions: [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=mpp2pass'>Mark All Pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=mpp2fail'>Mark All Fail</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=mpp2ignore'>Mark All Ignore</a>]</b><br>";
		echo "<b>Global Process: [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&status=mpp'>process</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&status=mpp'>ignore version error</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=mpp'>ignore sanity errors</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=mpp'>ignore sanity errors and force db update</a>]</b><br>";
		echo "<b>Global Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=ALL&status=mpp'>File Only</a>] [File and DB Entry]</b><br>";
		echo "<hr />";
		}
	//Generate list of plugins by parsing filename
	for ($i=0; $i<$plugin_count; $i++)
	{	
		$plugin_file = $new_plugins[$i];
		$parsed_filename = parseFilename($plugin_file,$extension);
		if ($parsed_filename["ManualUpload"]) {
			echo "Manual Upload: " . $plugin_file . "<br>";
			echo "<br>\n";
			}
		else {
			displayFile($plugin_file, $extension);
			}
		echo "<b>Change Status: [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=mpp2pass'>pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=mpp2fail'>fail</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=mpp2ignore'>ignore</a>]<br>";
		echo "Process: [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&status=mpp'>process</a>] [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&status=mpp'>process - ignore version error</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=mpp'>ignore sanity errors</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=mpp'>ignore sanity errors and force db update</a>]<br>";
		echo "Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=$plugin_file&status=file_only'>File Only</a>] [File and DB Entry]</b><br>";
		echo "<hr />";
		}

	//Check if plugin still has db record
	//Check if plugin is still in plugin archive (plugin directory)
	//Display appropriate error based on status of checks
	//Add links for changing status, delete, reprocessing (force-keep stats, force-clear stats)
	
	echo "Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function manage_failed($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode) {
	$cache_mode = $mode;
	$extension = ".mpp.fail";
	echo "Scanning for failed files on server... ";

	$failed_plugins = array();
	findFiles($failed_plugins,$ftp_repository . "*.mpp.fail");
	
	$plugin_count = sizeof($failed_plugins);
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " found<br><br>\n";
	
	//Generate global actions links - delete all, reprocess all, change status (mpp, pass, ignore)
	if ($plugin_count > 0) {
		echo "<b>Global Actions: [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=fail2mpp'>Mark All MPP</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=fail2pass'>Mark All Pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=fail2ignore'>Mark All Ignore</a>]</b><br>";
		echo "<b>Global Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=ALL&status=fail'>File Only</a>]</b><br>";
		echo "<hr />";
		}
	//Generate list of plugins by parsing filename
	for ($i=0; $i<$plugin_count; $i++)
	{	
		$plugin_file = $failed_plugins[$i];
		displayFile($plugin_file, $extension);
		echo "<b>Change Status: [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=fail2mpp'>mpp</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=fail2pass'>pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=fail2ignore'>ignore</a>]<br>";
		echo "Reprocess: [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&status=fail'>reprocess</a>] [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&status=fail'>reprocess - ignore version error</a>]<br>";
		echo "Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=$plugin_file&status=file_only'>File Only</a>] [File and DB Entry]</b><br>";
		echo "<hr />";
		}

	//Check if plugin still has db record
	//Check if plugin is still in plugin archive (plugin directory)
	//Display appropriate error based on status of checks
	//Add links for changing status, delete, reprocessing (force-keep stats, force-clear stats)

	echo "Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function manage_passed($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode) {
	$cache_mode = $mode;
	$extension = ".mpp.pass";
	echo "Scanning for passed files on server... ";

	$passed_plugins = array();
	findFiles($passed_plugins,$ftp_repository . "*.mpp.pass");
	
	$plugin_count = sizeof($passed_plugins);
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " found<br><br>\n";
	
	//Generate global actions links - delete all, reprocess all, change status (mpp, fail, ignore)
	if ($plugin_count > 0) {
		echo "<b>Global Actions: [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=pass2mpp'>Mark All MPP</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=pass2fail'>Mark All Fail</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=pass2ignore'>Mark All Ignore</a>]</b><br>";
		echo "<b>Global Reprocess: [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&status=pass'>reprocess</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&status=pass'>ignore version error</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=pass'>ignore sanity errors</a>] [<a href='process.php?mode=process_new&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=pass'>ignore sanity errors and force db update</a>]</b><br>";
		echo "<b>Global Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=ALL&status=pass'>File Only</a>] [File and DB Entry]</b><br>";
		
		echo "<hr />";
		}
	//Generate list of plugins by parsing filename
	for ($i=0; $i<$plugin_count; $i++)
	{	
		$plugin_file = $passed_plugins[$i];
		displayFile($plugin_file, $extension);
		echo "<b>Change Status: [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=pass2mpp'>mpp</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=pass2fail'>fail</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=pass2ignore'>ignore</a>]<br>";
		echo "Reprocess: [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&status=pass'>reprocess</a>] [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&status=pass'>ignore version error</a>] [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=pass'>ignore sanity errors</a>] [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&ignore_sanity=TRUE&force_update=true&status=pass'>ignore sanity errors and force db update</a>]<br>";
		echo "Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=$plugin_file&status=file_only'>File Only</a>] [File and DB Entry]</b><br>";
		//echo " | Stats: [<a href='process.php?mode=stats&cache_mode=$mode&file=$plugin_file&status=votes'>reset votes</a>] [<a href='process.php?mode=stats&cache_mode=$mode&file=$plugin_file&status=downloads'>reset downloads</a>] [<a href='process.php?mode=stats&cache_mode=$mode&file=$plugin_file&status=popularity'>reset popularity</a>]</b>";
		echo "<hr />";
		}

	//Check if plugin still has db record
	//Check if plugin is still in plugin archive (plugin directory)
	//Display appropriate error based on status of checks
	//Add links for changing status, delete, reprocessing (force-keep stats, force-clear stats)

	echo "Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function manage_ignored($ftp_repository, $ftp_manual_uploads, $ignore_version, $ignore_sanity, $force_update, $process_multiple, $status, $mode, $cache_mode) {
	$cache_mode = $mode;
	$extension = ".mpp.ignore";
	echo "Scanning for ignored files on server... ";

	$ignored_plugins = array();
	findFiles($ignored_plugins,$ftp_repository . "*.mpp.ignore");
	
	$plugin_count = sizeof($ignored_plugins);
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " found<br><br>\n";
	
	//Generate global actions links - delete all, reprocess all, change status (mpp, fail, ignore)
	if ($plugin_count > 0) {
		echo "<b>Global Actions: [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=ignore2mpp'>Mark All MPP</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=ignore2pass'>Mark All Pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=ALL&status=ignore2fail'>Mark All Fail</a>]</b><br>";
		echo "<b>Global Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=ALL&status=ignore'>File Only</a>] [File and DB Entry]</b><br>";
		echo "<hr />";
		}
	//Generate list of plugins by parsing filename
	for ($i=0; $i<$plugin_count; $i++)
	{	
		$plugin_file = $ignored_plugins[$i];
		displayFile($plugin_file, $extension);
		echo "<b>Change Status: [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=ignore2mpp'>mpp</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=ignore2pass'>pass</a>] [<a href='process.php?mode=change_status&cache_mode=$mode&file=$plugin_file&status=ignore2fail'>fail</a>]<br> ";
		echo "Reprocess: [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&status=ignore'>reprocess</a>] [<a href='process.php?mode=process_single&cache_mode=$mode&file=$plugin_file&ignore_version=TRUE&status=ignore'>reprocess - ignore version error</a>]<br>";
		echo "Delete: [<a href='process.php?mode=delete&cache_mode=$mode&file=$plugin_file&status=file_only'>File Only</a>] [File and DB Entry]</b><br>";
		//echo " | Stats: [<a href='process.php?mode=stats&cache_mode=$mode&file=$plugin_file&status=votes'>reset votes</a>] [<a href='process.php?mode=stats&cache_mode=$mode&file=$plugin_file&status=downloads'>reset downloads</a>] [<a href='process.php?mode=stats&cache_mode=$mode&file=$plugin_file&status=popularity'>reset popularity</a>]</b>";
		echo "<hr />";
		}

	//Check if plugin still has db record
	//Check if plugin is still in plugin archive (plugin directory)
	//Display appropriate error based on status of checks
	//Add links for changing status, delete, reprocessing (force-keep stats, force-clear stats)

	echo "Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function manage_flagged($status, $mode) {
	$cache_mode = $mode;
	echo "Scanning for flagged files in DB... ";

	$res = findReviewFlag();
	
	if ($res) $plugin_count = mysql_num_rows($res);
	else $plugin_count = 0;
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " flagged for review<br><br>\n";
	
	//Generate global actions links - delete all, reprocess all, change status (mpp, fail, ignore)
	if ($plugin_count > 0) {
		echo "<b>Global Actions: [<a href='process.php?mode=change_flag&cache_mode=$mode&file=ALL&status=false'>Approve All Plugins</a>]</b><br>";
		echo "<hr />";
		}
	echo "<table border=1 cellspacing=0 width=100%>";
	echo "<caption>Flagged Plugins</caption>";
	echo "<tr><th>Name</th><th>Version</th><th>State</th><th>Date</th><th>Flag</th><th>Actions</th></tr>";

	while ( $row=mysql_fetch_array($res,MYSQL_NUM)) {
		echo "<tr>";
		
		$field_num = 0;
		foreach($row as $field)
		{
			if ($field_num == 0) echo "";
			elseif ($field_num == 1) echo "<td align='center' nowrap><a href='dl.php?plugin_id=" . $row[0] . "&plugin_version=" . $row[2] . "&plugin_state=" . $row[3] . "'>$field</a></td>";
			elseif ($field_num == 4) {
				$theDate = explode(" ",$field);
				echo "<td align='center' nowrap>$theDate[0]</td>";
			}
			else echo "<td align='center' nowrap>$field</td>";
			$field_num++;
		}
		//$row[0] = id  $row[2] = version   $row[3] = state
		echo "<td align='center'><a href='process.php?mode=change_flag&cache_mode=$mode&id=$row[0]&version=$row[2]&state=$row[3]&status=FALSE'>Approve</a></td>";
		
		echo "</tr>";	
	}
	echo "</table>";

	echo "<br>Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function manage_metadata($status, $mode, $user) {
	$cache_mode = $mode;
	$u = Authenticate();
	if (!$user && !IsAdmin($u)) slowdie('You must <a href="' . GetLogonURL() . '">login</a> to manage your plugins!');

	echo "Scanning for plugins... ";

	$res = findMissingMetadata($user);
	
	if ($res) $plugin_count = mysql_num_rows($res);
	else $plugin_count = 0;
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " with missing meta-data<br><br>\n";
	
	echo "<table border=1 cellspacing=0 width=100%>";
	echo "<caption>Flagged Plugins</caption>";
	echo "<tr><th>Name</th><th>Version</th><th>State</th><th>Date</th><th>Actions</th></tr>";

	while ( $row=mysql_fetch_array($res,MYSQL_NUM)) {
		echo "<tr>";
		
		$field_num = 0;
		foreach($row as $field)
		{
			if ($field_num == 0) echo "";
			elseif ($field_num == 1) echo "<td align='center' nowrap><a href='detail.php?plugin_id=" . $row[0] . "&plugin_version=" . $row[2] . "&plugin_state=" . $row[3] . "'>$field</a></td>";
			elseif ($field_num == 4) {
				$theDate = explode(" ",$field);
				echo "<td align='center' nowrap>$theDate[0]</td>";
			}
			else echo "<td align='center' nowrap>$field</td>";
			$field_num++;
		}
		//$row[0] = id  $row[2] = version   $row[3] = state
		echo "<td align='center'><a href='edit.php?plugin_id=$row[0]&plugin_version=$row[2]&plugin_state=$row[3]' target=\"_blank\">Edit</a></td>";
		
		echo "</tr>";	
	}
	echo "</table>";

		if (!$user && IsAdmin($u)) echo "<br>Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
		else echo "<br>Click <a href='extra.php'>here</a> to return to Stats & Tools.<br>\n";
}



function manage_myplugins($status, $mode, $user) {
	$cache_mode = $mode;
	$u = Authenticate();
	if (!$user && !IsAdmin($u)) slowdie('You must <a href="' . GetLogonURL() . '">login</a> to manage your plugins!');

	echo "Scanning for plugins... ";

	$res = findUsersPlugins($user);
	
	if ($res) $plugin_count = mysql_num_rows($res);
	else $plugin_count = 0;
	echo "You have $plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " in the OpenMAID database.<br><br>\n";
	
	echo "<table border=1 cellspacing=0 width=100%>";
	echo "<caption>Your Plugins</caption>";
	echo "<tr><th>Name</th><th>Version</th><th>State</th><th>Date</th><th>Actions</th></tr>";

	while ( $row=mysql_fetch_array($res,MYSQL_NUM)) {
		echo "<tr>";
		
		$field_num = 0;
		foreach($row as $field)
		{
			if ($field_num == 0 || $field_num == 3 || $field_num == 5) echo "";
			elseif ($field_num == 1) echo "<td align='center' nowrap><a href='detail.php?plugin_id=" . $row[0] . "&plugin_version=" . $row[2] . "&plugin_state=" . $row[4] . "'>$field</a></td>";
			elseif ($field_num == 6) {
				$theDate = explode(" ",$field);
				echo "<td align='center' nowrap>$theDate[0]</td>";
			}
			else echo "<td align='center' nowrap>$field</td>";
			$field_num++;
		}
		//$row[0] = id  $row[2] = version   $row[4] = state
		echo "<td align='center'><a href='edit.php?plugin_id=$row[0]&plugin_version=$row[2]&plugin_state=$row[4]' target=\"_blank\">Edit</a></td>";
		
		echo "</tr>";	
	}
	echo "</table>";

		if (!$user && IsAdmin($u)) echo "<br>Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
		else echo "<br>Click <a href='extra.php'>here</a> to return to Stats & Tools.<br>\n";
}



function manage_profileID($status, $mode) {
	$cache_mode = $mode;
	echo "Scanning for missing profile IDs in DB... ";

	$res = findMissingProfileID();
	
	if ($res) $plugin_count = mysql_num_rows($res);
	else $plugin_count = 0;
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " with missing profile ID<br><br>\n";
	
	echo "<table border=1 cellspacing=0 width=100%>";
	echo "<caption>Flagged Plugins</caption>";
	echo "<tr><th>Name</th><th>Version</th><th>State</th><th>Author</th><th>Date</th><th>Actions</th></tr>";

	while ( $row=mysql_fetch_array($res,MYSQL_NUM)) {
		echo "<tr>";
		
		$field_num = 0;
		foreach($row as $field)
		{
			if ($field_num == 0) echo "";
			elseif ($field_num == 1) echo "<td align='center' nowrap><a href='detail.php?plugin_id=" . $row[0] . "&plugin_version=" . $row[2] . "&plugin_state=" . $row[3] . "'>$field</a></td>";
			elseif ($field_num == 5) {
				$theDate = explode(" ",$field);
				echo "<td align='center' nowrap>$theDate[0]</td>";
			}
			else echo "<td align='center' nowrap>$field</td>";
			$field_num++;
		}
		//$row[0] = id  $row[2] = version   $row[3] = state
		echo "<td align='center' NOWRAP><a href='edit.php?plugin_id=$row[0]&plugin_version=$row[2]&plugin_state=$row[3]' target=\"_blank\">Edit</a></td>";
		
		echo "</tr>";	
	}
	echo "</table>";

		echo "<br>Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function manage_AllPlugins($ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode) {
	$cache_mode = $mode;
	echo "Scanning plugins in DB... ";

	$res = getAllPlugins();
	
	if ($res) $plugin_count = mysql_num_rows($res);
	else $plugin_count = 0;
	echo "$plugin_count plugin";
	if ($plugin_count > 1) echo "s";
	echo " found!<br><br>\n";
	echo "<b>Actions:</b><br>&nbsp;&nbsp;&nbsp;<b>Edit</b> = Edit basic plugin metadata<br>";
	echo "&nbsp;&nbsp;&nbsp;<b>Del</b> = Delete only selected plugin (removes associated db records and file from repository)<br>";
	echo "&nbsp;&nbsp;&nbsp;<b>Del All</b> = Delete all plugins w/same plugin ID (removes associated db records and files from repository)<br>";
	echo "&nbsp;&nbsp;&nbsp;<b>Approve</b> = Approve plugin if it is flagged for admin review<br><br>";
	echo "<b>Colors:</b> <br>In number column, blue means plugin is marked as current (will be the default plugin showing in thelist.php for that plugin ID).  Red row means plugin is flagged for admin review. ";
	echo "Yellow row means plugin is missing some basic metadata such as profile ID or Support Link.  Green row means plugin is good...not missing profile ID or basic metadata.<br><br>";
	
	echo "<table border=1 cellspacing=0 width=100%>";
	echo "<caption><b>All Plugins</b></caption>";
	echo "<tr><th>#</th><th>Name</th><th>Version</th><th>State</th><th>Author</th><th>Profile ID</th><th>Date</th><th>Actions</th></tr>";

	$counter = 1;
	while ( $row=@mysql_fetch_array($res)) {
		$plugin_ID = $row["plugin_ID"];
		$plugin_Name = $row["plugin_Name"];
		$plugin_Version = $row["plugin_Version"];
		$plugin_State = $row["plugin_State"];
		$plugin_Type = $row["plugin_Type"];
		$plugin_Author = $row["plugin_Author"];
		$plugin_Date = $row["plugin_Date"];
		$profil_id = $row["profil_id"];
		$plugin_Current = $row["plugin_Current"];
		$plugin_ReviewFlag = $row["plugin_ReviewFlag"];
		$plugin_SupportLink = $row["plugin_SupportLink"];
		
		$row_color = "bgcolor='99FF99'";
		if ($profil_id == "" || $plugin_SupportLink == "") $row_color = "bgcolor='FFFF99'";
		if ($plugin_ReviewFlag == "TRUE") $row_color = "bgcolor='FF6666'";

		echo "<tr  $row_color>";
	
		$count_color = "bgcolor='FFFFFF'";
		if ($plugin_Current == 1) $count_color = "bgcolor='3399FF'";
		echo "<td align='center' nowrap $count_color><b>$counter</b></td>";
		echo "<td align='center'><a href='detail.php?plugin_id=$plugin_ID&plugin_version=$plugin_Version&plugin_state=$plugin_State'>$plugin_Name</a></td>";
		echo "<td align='center' nowrap>$plugin_Version</td>";
		echo "<td align='center' nowrap>$plugin_State</td>";
		echo "<td align='center'>$plugin_Author</td>";
		echo "<td align='center' nowrap>$profil_id </td>";
		$theDate = explode(" ",$plugin_Date);
		echo "<td align='center' nowrap>$theDate[0]</td>";
		$file = "_" . $plugin_ID . "_" . $plugin_Version . "_" . strtolower($plugin_Type) . "_" . strtolower($plugin_State) . "_";
		
		//$file, $id, $confirm, $ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode
		echo "<td align='center' NOWRAP><a href='edit.php?plugin_id=$plugin_ID&plugin_version=$plugin_Version&plugin_state=$plugin_State' target=\"_blank\">Edit</a> - ";
		echo "<a href='process.php?mode=fulldelete&cache_mode=$mode&file=$file&id=$plugin_ID&version=$plugin_Version&state=$plugin_State'>Del</a> - ";
		echo "<a href='process.php?mode=fulldelete&cache_mode=$mode&file=ALL&id=$plugin_ID'>Del All</a><br>";
		if ($plugin_ReviewFlag == "TRUE") echo "<a href='process.php?mode=change_flag&cache_mode=$mode&id=$plugin_ID&version=$plugin_Version&state=$plugin_State&status=FALSE'>Approve</td>";
		echo "</tr>";
		$counter++;
	}
	echo "</table>";

		echo "<br>Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
}



function change_flag($file, $id, $version, $state, $status) {
	//check if file is ALL, if ALL loop through all flagged plugins and change status to $status
	if ($file == "ALL") {
		$res = findReviewFlag();
		while ( $row=mysql_fetch_array($res,MYSQL_NUM)) {
			//$row[0] = id  $row[2] = version   $row[3] = state
			echo "Changing $row[1] review flag to $status...";
			changeReviewFlag($row[0], $row[2], $row[3], $status);
			UpdateCurrentPlugin($row[0]);
			echo "Done.<br>";	
			}
		echo "<br>Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
		echo "Click <a href='process.php?manage_flagged'>here</a> to continue managing flagged.<br>\n";
	}
	
	//if not ALL, change status of specified plugin to $status
	else {
		if ($id && $version && $state && $status) {
			echo "Changing plugin review flag to $status...";
			changeReviewFlag($id, $version, $state, $status);
			UpdateCurrentPlugin($id);
			echo "Done.<br>";
			echo "<br>Click <a href='admin.php'>here</a> to return to admin panel.<br>\n";
			echo "Click <a href='process.php?manage_flagged'>here</a> to continue managing flagged.<br>\n";
			}
		else die("Nice hack job...NOT!");
	}
}



function repository_file_delete($file, $confirm, $ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode) {
	if ($file == "") die ("Nice hack job...not!");
	if (!file_exists($file) && $file != "ALL") die ("Nice hack job...not!");

	//if confirm FALSE show confirmation link
	if ($confirm == FALSE) {
		echo "You are about to DELETE '<b>$file</b>' from the ftp repository on the server hard drive.<br><br>";
		echo "OK to proceed? [<a href='process.php?mode=$mode&cache_mode=$cache_mode&file=$file&confirm=TRUE&status=$status'>OK</a>] [<a href='process.php?mode=$cache_mode'>CANCEL</a>]<br>";
		}
	//if file = ALL then get list of all files and send them one at a time to delfile function
	if ($confirm == TRUE) {
		if ($file == "ALL") {
			$passed_plugins = array();
			if ($status == "mpp") findFiles($passed_plugins,$ftp_repository . "*." . $status);
			else findFiles($passed_plugins,$ftp_repository . "*.mpp." . $status);
	
			$plugin_count = sizeof($passed_plugins);
			echo "$plugin_count plugin";
			if ($plugin_count > 1) echo "s";
			echo " found<br><br>\n";
			echo "<hr />";
			//Send each file to the delfile function
			for ($i=0; $i<$plugin_count; $i++) {
				echo "Deleting...$passed_plugins[$i]";	
				delfile($passed_plugins[$i]);
				echo "...done.";
				}
			}
		//if file is an actual file, check it's existence and send it to the delfile function or throw an error
		else 	{
			if (file_exists($file)) delfile($file);
			else echo ("File ($file) not found!<br><br>");
			echo "File ($file) has been deleted.<br><br>";
			echo "Click <a href='process.php?mode=$cache_mode'>here</a> to return to previous screen.";
			}
		}
	}



function complete_plugin_delete($file, $id, $version, $state, $confirm, $ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode) {
	if ($file == "") die ("Nice hack job...not!");
	if (!$id && !file_exists($file) && $file != "ALL") die ("Nice hack job...not!");

	//if ID set, then we must be dealing with plugin db management.
	if ($id != "") {
	//START of plugin db management
	//if confirm FALSE show confirmation link
		if ($confirm == FALSE) {
			if ($id && $file == "ALL") echo "You are about to DELETE all plugins with GUID=$id from the ftp repository on the server hard drive ";
			if ($id && $file != "ALL") echo "You are about to DELETE plugin with GUID=$id from the ftp repository on the server hard drive ";
			else echo "You are about to DELETE '<b>$file</b>' from the ftp repository on the server hard drive ";
			echo "and remove any db records for this plugin if they exist!<br><br>";
			echo "OK to proceed? [<a href='process.php?mode=$mode&cache_mode=$cache_mode&file=$file&id=$id&version=$version&state=$state&confirm=TRUE&status=$status'>OK</a>] [<a href='process.php?mode=$cache_mode'>CANCEL</a>]<br>";
			}
		//if file = ALL then get list of all files and send them one at a time to the delfile and delFromDB functions
		if ($confirm == TRUE) {
			if ($file == "ALL") {
				$passed_plugins = array();
				findFiles($passed_plugins,$ftp_repository . "*_" . $id. "_*");
		
				$plugin_count = sizeof($passed_plugins);
				echo "$plugin_count plugin";
				if ($plugin_count > 1) echo "s";
				echo " found<br><br>\n";
				echo "<hr />";
				//Send each file to the delfile and delFromDB functions
				if ($plugin_count < 1) {
					echo "Couldn't find files in repository...continuing to delete from db...";
					delFromDB($id, $version, $state, TRUE);
					echo "Done.<br>";
					}
				else {
					for ($i=0; $i<$plugin_count; $i++) removePluginFile($passed_plugins[$i], $id, $version, $state, FALSE);
					delFromDB($id, "", "", TRUE);
					}
				echo "<br>Click <a href='process.php?mode=$cache_mode'>here</a> to return to previous screen.";
				}
			//else file will be id_version_type_state...check it's existence, and send it to the delfile and delFromDB functions or throw an error
			else 	{
				$passed_plugins = array();
				findFiles($passed_plugins,$ftp_repository . "*". $file . "*");
				
				$plugin_count = sizeof($passed_plugins);
				if ($plugin_count < 1) {
					echo "Couldn't find file in repository...continuing to delete from db...";
					delFromDB($id, $version, $state, FALSE);
					echo "Done!<br>";
					}
				else {
					for ($i=0; $i<$plugin_count; $i++) removePluginDB($passed_plugins[$i], $id, $version, $state, FALSE);
					}
				echo "<br>Click <a href='process.php?mode=$cache_mode'>here</a> to return to previous screen.";
				}
			}
		}
	//END of plugin db management
	
	//if ID is not set, we must be dealing with file repository management
	if ($id == "") {
	//START of file repository management
	//if confirm FALSE show confirmation link
		if ($confirm == FALSE) {
			echo "You are about to DELETE '<b>$file</b>' from the ftp repository on the server hard drive ";
			echo "and remove the db entry for this plugin if it exists!<br><br>";
			echo "OK to proceed? [<a href='process.php?mode=$mode&cache_mode=$cache_mode&file=$file&confirm=TRUE&status=$status'>OK</a>] [<a href='process.php?mode=$cache_mode'>CANCEL</a>]<br>";
			}
		//if file = ALL then get list of all plugins with the specified $id and send them to delfile and delFromDB functions
		if ($confirm == TRUE) {
			if ($file == "ALL") {
				$passed_plugins = array();
				if ($status == "mpp") findFiles($passed_plugins,$ftp_repository . "*." . $status);
				else findFiles($passed_plugins,$ftp_repository . "*.mpp." . $status);
		
				$plugin_count = sizeof($passed_plugins);
				echo "$plugin_count plugin";
				if ($plugin_count > 1) echo "s";
				echo " found<br><br>\n";
				echo "<hr />";
				//Send each file to the delfile and delFromDB functions
				for ($i=0; $i<$plugin_count; $i++) removePluginFile($passed_plugins[$i]);
				}
			//if file is an actual file, check it's existence and send it to the delfile and delFromDB functions or throw an error
			else 	{
				if (file_exists($file)) removePluginFile($file);
				else echo ("File ($file) not found!<br><br>");
				echo "Click <a href='process.php?mode=$cache_mode'>here</a> to return to previous screen.";
				}
			}
		}
	//END of file repository management
	}



function change_status($file, $confirm, $ftp_repository, $ftp_manual_uploads, $status, $mode, $cache_mode) {
	if ($file == "" || $status == "") die ("Nice hack job...not!");

	//parse status info (from what, to what)
	$change = explode("2",$status);
	if ($change[0] != "mpp" && $change[0] != "pass" && $change[0] != "fail" && $change[0] != "ignore") die ("Nice hack job...not!");
	if ($change[1] != "mpp" && $change[1] != "pass" && $change[1] != "fail" && $change[1] != "ignore") die ("Nice hack job...not!");
	if ($change[0] == $change[1]) die ("Nice hack job...not!");
	
	//if confirm FALSE show confirmation link
	if ($confirm == FALSE) {
		if ($file == "ALL") echo "You are about to change ALL '<b>$change[0]</b>' plugins to '<b>$change[1]</b>' status.<br>";
		else echo "You are about to change a '<b>$change[0]</b>' status plugin to a '<b>$change[1]</b>' status:<br><br>$file<br><br>";
		echo "OK to proceed? [<a href='process.php?mode=$mode&cache_mode=$cache_mode&file=$file&status=$status&confirm=TRUE'>OK</a>] [<a href='process.php?mode=$cache_mode'>CANCEL</a>]<br>";
		}
	//if file = ALL then get list of all files and send them one at a time to change status function
	if ($confirm == TRUE) {
		if ($file == "ALL") {
			$passed_plugins = array();
			if ($change[0] == "mpp") findFiles($passed_plugins,$ftp_repository . "*." . $change[0]);
			else findFiles($passed_plugins,$ftp_repository . "*.mpp." . $change[0]);
	
			$plugin_count = sizeof($passed_plugins);
			echo "$plugin_count plugin";
			if ($plugin_count > 1) echo "s";
			echo " found<br><br>\n";
			echo "<hr />";
			//Generate list of plugins by parsing filename
			for ($i=0; $i<$plugin_count; $i++) {	
				changePluginStatus($passed_plugins[$i], $change[0], $change[1]);
				}
			}
		//if file is an actual file, check it's existence and send it to the change status function or throw an error
		else 	{	if (file_exists($file)) changePluginStatus($file, $change[0], $change[1]);
					else echo ("File ($file) not found!<br><br>");
				}
		echo "Click <a href='process.php?mode=$cache_mode'>here</a> to return to previous screen.";
		}
	}



//convert filename to array with id, version, plugin type, state, and name
function parseFilename($file, $extension) {

	$extensions = explode("/", $extension);
	$extension_count = sizeof($extensions);
	
	for ($i=0; $i<$extension_count; $i++){
		if (substr($file,-(strlen($extensions[$i])),strlen($extensions[$i])) == $extensions[$i]) {
			$file = substr($file,0,(strlen($file)-strlen($extensions[$i])));
			$temp1 = explode("_", $file, 3);
			$parsed_file["ID"] = $temp1[1];
			$temp2 = explode("_", $temp1[2], 4);
			$parsed_file["Version"] = $temp2[0];
			$parsed_file["Type"] = $temp2[1];
			$parsed_file["State"] = $temp2[2];
			$parsed_file["Name"] = str_replace("_", " ", $temp2[3]);
			}
		}
		
		if (!$parsed_file["Version"] || !$parsed_file["Type"] || !$parsed_file["State"] || !$parsed_file["Name"] || !$parsed_file["ID"])
			$parsed_file["ManualUpload"] = $file;
	
	return $parsed_file;
	}


function removePluginFile($file) {
	//Remove plugin entry from db (or mark as deleted) and remove file from ftp repository
	
	//ADD DELETE: DELETE FROM PLUGINS DIRECTORY TOO!!!!

				echo "Deleting file and DB entries...$file";
				$parsed_filename = parseFilename($file,".mpp"); //extension not important
				delfile($file);
				delFromDB($parsed_filename["ID"], $parsed_filename["Version"], $parsed_filename["State"], FALSE);
				echo "...done.<br>";
	}


function removePluginDB($file, $id, $version, $state, $all) {
	//Remove plugin entry from db (or mark as deleted) and remove file from ftp repository

	//ADD DELETE: DELETE FROM PLUGINS DIRECTORY TOO!!!!

				echo "Deleting file and DB entries...$file";
				$parsed_filename = parseFilename($file,".mpp"); //extension not important
				delfile($file);
				delFromDB($id, $version, $state, $all);
				echo "...done.<br>";
	}


//Change plugin status (.pass, .fail, .ignore, .mpp)
function changePluginStatus($file, $ext_from, $ext_to) {
	//generate extension strings
	if ($ext_from == "pass") $ext_from = ".mpp.pass";
	elseif ($ext_from == "fail") $ext_from = ".mpp.fail";
	elseif ($ext_from == "ignore") $ext_from = ".mpp.ignore";
	elseif ($ext_from == "mpp") $ext_from = ".mpp";

	if ($ext_to == "pass") $ext_to = ".mpp.pass";
	elseif ($ext_to == "fail") $ext_to = ".mpp.fail";
	elseif ($ext_to == "ignore") $ext_to = ".mpp.ignore";
	elseif ($ext_to == "mpp") $ext_to = ".mpp";
	
	//strip ext_from off of file and make new file name with new extension
	$ext_from_length = strlen($ext_from);
	$file_length = strlen($file);
	$newfile = substr($file, 0, $file_length-$ext_from_length) . $ext_to;

	//rename the file
	rename($file,$newfile);
	
	//output the info
	echo "Renamed: $file<br><br>";
	echo "To: $newfile<br>";
	echo "<hr />";
	}

function displayFile($plugin_file, $extension) {
	$parsed_filename = parseFilename($plugin_file,$extension);
	echo "<p> <b>$plugin_file</b>  <br>\n";
	echo "<b>GUID:</b> " . $parsed_filename["ID"] . "<br>";
	echo $parsed_filename["Name"] . " v" . $parsed_filename["Version"] . " - " . $parsed_filename["State"] . " (" . $parsed_filename["Type"] . ")<br>";
	echo "<br>\n";
}

?>
