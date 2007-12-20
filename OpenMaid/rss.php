<? 
include("feedcreator.class.php");
include("db.php");

$rss = new UniversalFeedCreator();
$rss->useCached();
$rss->title = "MeediOS OpenMAID";
$rss->description = "Plugins";
$rss->link = "http://www.meedios.com/OpenMAID";
$rss->syndicationURL = "http://www.meedios.com/".$PHP_SELF;

function toBool($integer)
{
	if($integer == 0)
		return "false";
	else
		return "true";
}

//$image = new FeedImage();
//$image->title = "OpenMaind Logo";
//$image->url = "http://www.meedios.com/images/logo.gif";
//$image->link = "http://www.meedios.com";
//$image->description = "Feed provided by meedios.com.";
//$rss->image = $image;

ConnectOnce();

$res = mysql_query("SELECT * FROM plugins");
while ($data = mysql_fetch_object($res)) {
    $item = new FeedItem();
    $item->title = $data->plugin_Name;
    $item->link = "[insert plugin url here]";
    $item->description = $data->plugin_ShortDescription;
    $item->date = $data->plugin_Date;//TODO: Format Date
    $item->source = "http://www.meedios.com";
    $item->author = $data->plugin_Author;
    $item->category = $data->plugin_Type;
    $item->guid = $data->plugin_ID;
    
    $item->additionalElements = array("screenshot"=>"http://www.meedios.com/plugins/image.jpg", "version"=>$data->plugin_Version, "state"=>$data->plugin_State, "download"=>$data->plugin_DownloadLink, "key"=>$data->plugin_key,  "current"=>toBool($data->plugin_Current), "copyright"=>$data->plugin_Copyright, "support_url"=>$data->plugin_SupportLink);
    
    $rss->addItem($item);
}

$rss->saveFeed("RSS2.0", "feed.xml");
?> 