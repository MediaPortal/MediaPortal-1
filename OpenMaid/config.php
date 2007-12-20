<?php
        
        //MySQL Database Information for OpenMAID v2
        $db_host                = "localhost";
        $db_user                = "openmaid";
        $db_pass                = "ACER1990";
        $db_database    = "openmaid";

        //MySQL DB Info for previous version database (used to copy profil_id from old db to new db)
        $old_db_host                    = "localhost";
        $old_db_user                    = "old_db_user";
        $old_db_pass                    = "old_db_password";
        $old_db_database                = "old_db_name";
        $old_db_plugins_table   = "plugins";

        //Directory where manually uploaded plugins go to be processed. Include trailing slash.
        $ftp_manual_uploads = "ftp/";

        //Directory where plugin repository is.  This is where fetchplugins.php stores all versions of all plugins incase we need to 
        //rebuild the OpenMAID database and plugins directory structure.  This is set automatically...please don't change this line!!!!
        $ftp_repository = $ftp_manual_uploads . "repository/";

        //Allowable plugin_Type and plugin_ModuleType: 'General','Import','Input','Module','Theme','Web','Wizard','Extension','Sub','Hack','Misc','Icon'
        $types = array(  "Icon",
        "Plugin",
                                         "Theme",
                                         "General",
                                         "Input",
                                         "Module",
                                         "Import",
                                         "Extension",
                                         "Misc",
                                         "Hack", 
                                         "Web", 
                                         "Wizard", 
                                         "Sub");

        //URL should point to the URL where default.php is located      
        $sys_url                = "http://openmaid.team-mediaportal.com/";
        
        //phpBB Forum Root Url
        $sys_forum_url  = "http://your url path to the forum/";
        
        //Specifies how OpenMAID should authenticate values allowed: "virtual" and "forum"
        $sys_auth               = "virtual";
        
        //Specifies all of the OpenMAID administrators bellow
        $sys_admin_list = array(
                                                        "admin_forum_id");
                                                        
        
        //This is the directory where the plugins will be stored after they are processed                                                       
        $plugin_home_directory = "plugins";

        //Sets debug mode. 1=TRUE 0=FALSE
        $debug = 0;
        
?>
