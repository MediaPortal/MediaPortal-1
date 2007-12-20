<?php
require_once('authentication.php');

$action = $_POST["action"];
if ($action == "logon")
{
	//save cookies
 	if (isset($_POST["username"]))
		setcookie("user", $_POST["username"], time()+604800);	

 	if (isset($_POST["adminpass"]))
		setcookie("adminpass", $_POST["adminpass"], time()+604800);	

}
elseif($action == "logout")
{
		setcookie("user", "", time()+3600);
		setcookie("adminpass","", time()+3600);	
}

include('header.php');
?>
<div id="menudiv">
<b><a href="<?php echo $sys_url; ?>">OpenMAID</a></b>
</div>
</td></tr><tr>
<td id="contentarea">
<div id="leftbar">

<?php

	echo "<p>Welcome to the virtual Authentication Screen. Virtual authentication is to be used until integration with the forum authentication is available. </p>";
	
	$user = Authenticate();
	
	//user is not logged on
	if ($user == "")
	{
				
		if ($action == "")
		{ 	
			echo "<form method=\"POST\" >";
			echo "<table><tr><td>";
			echo "User Name:</td><td> <input type=\"text\" name=\"username\" /></td></tr><tr><td>";
			echo "Admin Password:</td><td> <input type=\"password\" name=\"adminpass\" /> (Leave blank if you are not an admin)</td></tr><tr ><td colspan=\"2\" align=\"Center\">";
			echo "<input type=\"hidden\" value=\"logon\" name=\"action\" />";
			echo "<input type=\"submit\" value=\"Logon\" /></td></tr></table>";
			echo "</form>";
		}
		elseif ($action == "logon")
		{
			echo "Logged on successful as " .$_POST["username"] . "<br />";
			echo "click <a href=''>here</a> to see status";
		}
	}
	else
	{
		if ($action == "")
		{ 	 
			echo "Currently Logged on User: '$user'<br />";
			if (IsAdmin($user))
				echo "Administrator priviliges granted";
			else
				echo "NO administrator priviliges granted";
			
			echo "<form method=\"POST\">";
			echo "<input type=\"hidden\" value=\"logout\" name=\"action\" />";
			echo "<input type=\"submit\" value=\"Logout\" />";
			echo "</form>";
		}
		elseif ($action == "logout")
		{
			echo "Log out successful. <br />";
			echo "click <a href=''>here</a> to logon";
		}		
	}

?>

</div><div id="rightbar">


</div>

<?php
include('footer.php');
?>
