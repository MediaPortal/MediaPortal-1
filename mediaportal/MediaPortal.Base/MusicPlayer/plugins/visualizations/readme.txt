This folder contains Visualisation Plugins used within Mediaportal.

Attention:

- Sonique Visualisations can be placed anywhere in this folder or in sub folders. We would recommend using the Sonique folder for that

- BASS BOX Plugins MUST be placed into the <mediaportal install dir>\BBPlugin subfolder, otherwise the Textures will not be found.

- Winamp Visualisations MUST be placed in the <mediaportal install dir>\Plugins subfolder.
  This is the folder, which contains all the MediaPortal plugins as well.
  
  No other sub folders should be created, except when a plugin needs it.
  
  Let's use Milkdrop as an example:
  
  Program Files (x86)
     !
	 !- Team Mediaportal
	      !
		  !- MediaPortal
		          !
				  !- Plugins
						!
						!- ExternalPlayers  <--- MP External Players
						!- process          <--- MP Process Plugins
						!- Windows          <--- MP GUI Plugins
						!- Milkdrop2    	<--- This is the folder, which you copied over from your Winamp Installation's Plugins folder
						!- vis_milk2.dll	<--- This is the milkdrop2 executable, which you copied over from your Winamp installation's Plugins folder 