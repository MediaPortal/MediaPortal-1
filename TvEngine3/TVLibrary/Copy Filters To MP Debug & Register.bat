xcopy "..\..\DirectShowFilters\bin\Release\*.*" "_TvPluginDebug\MediaPortal\" /R /Y

chdir "..\..\DirectShowFilters\bin\Release\"
regsvr32 cdxareader.ax /s
regsvr32 CLDump.ax /s
regsvr32 DVBSub2.ax /s
regsvr32 mmaacd.ax /s
regsvr32 MpaDecFilter.ax /s
regsvr32 MPAudioSwitcher.ax /s
regsvr32 Mpeg2DecFilter.ax /s
regsvr32 MPFileWriter.ax /s
regsvr32 MPSA.ax /s
regsvr32 PDMpgMux.ax /s
regsvr32 shoutcastsource.ax /s
regsvr32 TsReader.ax /s
regsvr32 TsWriter.ax /s
