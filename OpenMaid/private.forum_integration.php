<?php

define("IN_LOGIN", true);
define('IN_PHPBB', true);
$phpbb_root_path = '/forum/';
require_once($phpbb_root_path . 'extension.inc');
require_once($phpbb_root_path . 'common.php');
$userdata = session_pagestart($user_ip, PAGE_LOGIN);
init_userprefs($userdata);

function getPhpUser()
{
	global $userdata;
	$u = $userdata['username'];
	if ($u == "Anonymous") $u = "";
	return $u;

	global $HTTP_COOKIE_VARS;
	$cookiename = "meediosforum";
	unset($userdata);

	if ( isset($HTTP_COOKIE_VARS[$cookiename . '_sid']) || isset($HTTP_COOKIE_VARS[$cookiename . '_data']) )
	{
		$sessiondata = isset( $HTTP_COOKIE_VARS[$cookiename . '_data'] ) ? unserialize(stripslashes($HTTP_COOKIE_VARS[$cookiename . '_data'])) : array();
		$session_id = isset( $HTTP_COOKIE_VARS[$cookiename . '_sid'] ) ? $HTTP_COOKIE_VARS[$cookiename . '_sid'] : '';
	}
	else
	{
		$sessiondata = array();
		$session_id = ( isset($HTTP_GET_VARS['sid']) ) ? $HTTP_GET_VARS['sid'] : '';
	}

	//
	if (!preg_match('/^[A-Za-z0-9]*$/', $session_id))
	{
		$session_id = '';
	}

	@mysql_connect("localhost", "slug", "foxtr0t") or die("Error 45");
	@mysql_select_db("slugphpbb") or die("Error 6542");

	//
	// Does a session exist?
	//
	if ( !empty($session_id) )
	{
		//
		// session_id exists so go ahead and attempt to grab all
		// data in preparation
		//
		$sql = "SELECT u.*, s.*
			FROM phpbb_sessions s, phpbb_users u
			WHERE s.session_id = '$session_id'
				AND u.user_id = s.session_user_id";
		$res = mysql_query($sql);
		$userdata  = mysql_fetch_array($res);

		if ($userdata['session_logged_in'] <> 1) return FALSE;
		$u = $userdata['username'];
		return $u;
	}
	return FALSE;
}

?>