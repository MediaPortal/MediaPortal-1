<?php
require_once('config.php');
if ($sys_auth !="virtual") require_once('private.forum_integration.php');

//function authenticates user
//Depending on Configuration Settings, this function
//will use phpBB to authenticate the User.
//returns the UserName
function Authenticate()
{
	global $sys_auth;
	
	if ($sys_auth == "virtual")
	{
		if (isset($_COOKIE["user"]))
			return $_COOKIE["user"];
		else
			return "";
	}		
	else //forum authentication
	{
		return getPhpUser();
	}

}

//function returns True when $userName is an administrator 
function IsAdmin($userName)
{
	global $sys_admin_list;
	global $sys_auth;
	
	//loop for every administrator in the system
	foreach ($sys_admin_list as $admin_name) 
	{
	 	//check if the current user is an administrator
		if (strtolower($admin_name) == strtolower($userName) )
		{
		 	//check the admin password for Virtual Authentication
		 	if ($sys_auth == "virtual")
		 	{
				if ($_COOKIE["adminpass"] == "setadminpasshere")
					return true;

				return false;
			}
			return true;
		}
	}
	return false;	
	
}

//returns the URL for the user to logon
function GetLogonURL()
{
	global $sys_auth;
	global $sys_forum_url;

	if ($sys_auth == "virtual")
		return "virtual_auth.php";
	else
		return $sys_forum_url;
	
}

?>
