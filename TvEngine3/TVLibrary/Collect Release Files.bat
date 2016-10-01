xcopy "..\..\Common-MP-TVE3\DirectShowLib\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "..\..\Common-MP-TVE3\Common.Utils\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "SetupControls\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "SetupTv\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "TestApp\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "TvControl\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "TVDatabase\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "TVLibrary\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "TvLibrary.Interfaces\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "TvLibrary.Utils\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "TvService\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "Plugins\PowerScheduler\PowerScheduler.Interfaces\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "Plugins\PowerScheduler\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\ServerBlaster\ServerBlaster (Learn)\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "Plugins\PluginBase\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\" /R /Y
xcopy "Plugins\ComSkipLauncher\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\ConflictsManager\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\PersonalTVGuide\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\ServerBlaster\ServerBlaster\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\TvMovie\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\XmlTvImport\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\WebEPG\WebEPGPlugin\bin\Release\*.*" "_TvServerRelease\MediaPortal TV Server\Plugins\" /R /Y

xcopy "TvPlugin\TvPlugin\Gentle.config" "_TvPluginRelease\MediaPortal\" /R /Y
xcopy "TvPlugin\TvPlugin\bin\Release\TvPlugin.dll" "_TvPluginRelease\MediaPortal\plugins\Windows\" /R /Y
xcopy "TvPlugin\TvPlugin\bin\Release\TvPlugin.pdb" "_TvPluginRelease\MediaPortal\plugins\Windows\" /R /Y
xcopy "_TvServerRelease\MediaPortal TV Server\Gentle*.*" "_TvPluginRelease\MediaPortal\" /R /Y
xcopy "_TvServerRelease\MediaPortal TV Server\MySql.Data.dll" "_TvPluginRelease\MediaPortal\" /R /Y
xcopy "_TvServerRelease\MediaPortal TV Server\Tv*.dll" "_TvPluginRelease\MediaPortal\" /R /Y
xcopy "_TvServerRelease\MediaPortal TV Server\Tv*.pdb" "_TvPluginRelease\MediaPortal\" /R /Y
xcopy "_TvServerRelease\MediaPortal TV Server\Tv*.xml" "_TvPluginRelease\MediaPortal\" /R /Y