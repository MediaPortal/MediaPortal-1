xcopy "DirectShowLib\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "SetupControls\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "SetupTv\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "TestApp\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "TvControl\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "TVDatabase\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "TVLibrary\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "TvLibrary.Interfaces\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "TvService\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "Plugins\PowerScheduler\PowerScheduler.Interfaces\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "Plugins\ServerBlaster\ServerBlaster (Learn)\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "Plugins\PluginBase\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\" /R /Y
xcopy "Plugins\ComSkipLauncher\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\ConflictsManager\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\PersonalTVGuide\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\ServerBlaster\ServerBlaster\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\TvMovie\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\Plugins\" /R /Y
xcopy "Plugins\XmlTvImport\bin\Debug\*.*" "_TvServerDebug\MediaPortal TV Server\Plugins\" /R /Y

xcopy "TvPlugin\TvPlugin\Gentle.config" "_TvPluginDebug\MediaPortal\" /R /Y
xcopy "TvPlugin\TvPlugin\bin\Debug\TvPlugin.dll" "_TvPluginDebug\MediaPortal\plugins\Windows\" /R /Y
xcopy "TvPlugin\TvPlugin\bin\Debug\TvPlugin.pdb" "_TvPluginDebug\MediaPortal\plugins\Windows\" /R /Y
xcopy "_TvServerDebug\MediaPortal TV Server\Gentle*.*" "_TvPluginDebug\MediaPortal\" /R /Y
xcopy "_TvServerDebug\MediaPortal TV Server\Tv*.dll" "_TvPluginDebug\MediaPortal\" /R /Y
xcopy "_TvServerDebug\MediaPortal TV Server\Tv*.pdb" "_TvPluginDebug\MediaPortal\" /R /Y
xcopy "_TvServerDebug\MediaPortal TV Server\Tv*.xml" "_TvPluginDebug\MediaPortal\" /R /Y
