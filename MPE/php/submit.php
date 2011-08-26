<?php
$url = $_GET['url'];
$name = $_GET['name'];
$version = $_GET['version'];
$id = $_GET['id'];
$myFile = "extensions.txt";
$found = 0;
?>
<HTML>
<BODY>
<?php
if (isset($url))
{
		$lines = file($myFile);

		// Loop through our array, show HTML source as HTML source; and line numbers too.
		foreach ($lines as $line_num => $line) {
			$items = explode(";", $line);	
			/* echo "Line #<b>{$line_num}</b> : " . htmlspecialchars($items[0]) . "<br />\n"; */
			if($items[0] == $url)
				$found = 1;
		}

		if($found == 0){
			$fh = fopen($myFile, 'a') or die("can't open file");
			fwrite($fh, $url.";".$id.";".$name.";".$version.";PENDING\n");
			fclose($fh);
			echo "<HR>";
			echo "THX for submiting the extension ";
			echo "<HR>";
			echo "<table border='1'>";
			echo "<tr><td>Extension Id</td><td>".$id."</td></tr>";
			echo "<tr><td>Name</td><td>".$name."</td></tr>";
			echo "<tr><td>Version</td><td>".$version."</td></tr>";
			echo "<tr><td>Update url</td><td>".$url."</td></tr>";
			echo "</table>";
		}
		else
		{
			echo "Url already submitted ! !";
		}
}
else
{
    echo "No url was specified !!!!!";
}

?>
</BODY>
</HTML>
