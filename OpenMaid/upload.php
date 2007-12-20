<?php
require_once('functions.php');
require_once('authentication.php');

include('header.php');
?>

<div id="menudiv">
<b><a href="<?php echo $sys_url; ?>">OpenMAID</a></b>
</div>
</td></tr><tr>
<td id="contentarea">
<div id="widebar">

<?php
$u = Authenticate();

if (!isset($u) || $u == "") 
        die("You must <a href=\"/forum/login.php\">login to upload</a>");


if (!isset($_FILES["userfile"]) && !isset($_FILES["pluginfile"]))
{
        // display plugin archive upload form
        echo "<table border=0 align=\"center\"><tr><td align=\"center\"><b>UPLOAD EXTENSION</b><br /><br />Use this form to upload any type of add-in. The add-in must be a compressed zip archive, but can be any extension (e.g., .zip, .mpp). If your upload file size is larger than " . ini_get('post_max_size') . " it will most likely fail, in which case <a href='mailto:slug@skyforge.net'>contact us</a> and provide a http download link for manual insertion.<br /><br />";
        echo "<form method=post enctype='multipart/form-data' action='upload.php' method=post>";
        echo "Add-in file: <input type='file' name='userfile' size=50 /><br />";
        echo "<input type=\"checkbox\" value=\"IsAuthor\" checked=\"checked\"name=\"IsAuthor\" /> I am the author of this plugin<br />";
        echo "<input type=submit class=formbutton value='Submit'>";
        echo "</form></td></tr></table><br /><hr />";

        // display .plugin xml file upload form
        echo "<br /><table border=0 align=\"center\"><tr><td align=\"center\"><b>TEST .XMP FILE</b><br /><br />Use this form to check your .xmp file for errors before uploading your plugin.<br /><br />";
        echo "<form method=post enctype='multipart/form-data' action='upload.php' method=post>";
        echo ".plugin file: <input type='file' name='pluginfile' size=50 /><br />";
        echo "<input type=\"checkbox\" value=\"IsAuthor\" checked=\"checked\"name=\"IsAuthor\" /> I am the author of this plugin<br />";
        echo "<input type=submit class=formbutton value='Submit'>";
        echo "</form></td></tr></table>";
}
elseif (isset($_FILES["userfile"]))
{
        $f = $_FILES["userfile"];
        $bytes = $f["size"];
        $name = $f["name"];

        $isAuthor = $_POST["IsAuthor"];
        
        if ($isAuthor)
                $author = $u;
        else
                $author = "";
        
        
        if ($f["error"] <> 0)
        {
                echo "<b>Critical Failure:</b> the php upload system said ERROR. Please tell <a href='mailto:slug@skyforge.net'>binary64</a> about this error.";
                if (file_exists($f["tmp_name"])) unlink($f["tmp_name"]);
        }
        
        $temp_directory = Generate_Temp_Directory("mpp", "mppfile_");
        
        if (move_uploaded_file($f["tmp_name"], "$temp_directory/$name"))
        {
                echo "<h1>OpenMAID : Upload</h1><p>";
                echo "Detected file upload: $name ($bytes bytes).<br>\n";
                if (AddMpp($maiddir . "$temp_directory/$name", $author))
                {
                        echo "<p>Finished. Please <a href='./'>continue to the main MAID page</a>";
                }
                else
                {
                        echo "<p>Upload encountered errors!<br>";
                        echo "Please read the output above and correct the errors indicated and try uploading again.";
                }
                remove_directory($temp_directory);
                echo"</p>";
        }
        else
        {
                echo "<b>Critical Failure:</b> PHP shouted profanity at the security of your upload. Please tell <a href='mailto:slug@skyforge.net'>binary64</a> if you believe this to be an error.";
                if (file_exists($f["tmp_name"])) unlink($f["tmp_name"]);
        
        }
}
elseif (isset($_FILES["pluginfile"]))
{
        $f = $_FILES["pluginfile"];
        $bytes = $f["size"];
        $name = $f["name"];
        
        $isAuthor = $_POST["IsAuthor"];
        
        if ($isAuthor)
                $author = $u;
        else
                $author = "";
        
        if ($f["error"] <> 0)
        {
                echo "<b>Critical Failure:</b> the php upload system said ERROR. Please tell <a href='mailto:slug@skyforge.net'>binary64</a> about this error.";
                if (file_exists($f["tmp_name"])) unlink($f["tmp_name"]);
        }

        $temp_directory = Generate_Temp_Directory("mpp", "pluginfile_");
        
        if (move_uploaded_file($f["tmp_name"], "$temp_directory/$name"))
        {
                echo "<h1>OpenMAID : Upload</h1><p>";
                
                echo "Detected file upload: $name ($bytes bytes).<br>\n";       
                if (TestPluginFile($maiddir . "$temp_directory/$name", $author))
                {
                        echo "<p><b>SUCCESS!</b><br><br>Your .plugin file is valid. You can now <a href='upload.php'>upload</a> your plugin.";
                }
                else
                {
                        echo "<p><b>FAILURE!</b><br><br>The .plugin file has errors!<br>";
                        echo "Please read the output above and correct the errors indicated and try testing the file again.";
                }
                remove_directory($temp_directory);
                echo"</p>";
        }
        else
        {
                echo "<b>Critical Failure:</b> PHP shouted profanity at the security of your upload. Please tell <a href='mailto:slug@skyforge.net'>binary64</a> if you believe this to be an error.";
                if (file_exists($f["tmp_name"])) unlink($f["tmp_name"]);
        
        }
}

include('footer.php');
?>
