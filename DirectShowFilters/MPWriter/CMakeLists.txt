# MPFileWriter: translated from MPWriter.vcxproj.

add_library(MPFileWriter SHARED
  source/ChannelScan.cpp
  source/FileSinkRecorder.cpp
  source/FileWriter.cpp
  source/Hamming.cpp
  source/MemoryBuffer.cpp
  source/MemoryStreamSource.cpp
  source/MPFileWriter.cpp
  source/MultiFileWriter.cpp
  source/MultiWriterFileSink.cpp
  source/PacketSync.cpp
  source/ProgramToTransportStream.cpp
  source/ProgramToTransportStreamRecorder.cpp
  source/SubChannel.cpp
  source/TeletextGrabber.cpp
  source/TSThread.cpp
  source/WaitEvent.cpp
  source/MPFileWriter.def
)

mp_target(MPFileWriter
  SUFFIX ".ax"
  OUTPUT_NAME "MPFileWriter"
  FOLDER "Filters"
  INCLUDES
    ../LiveMedia555/MediaPortal
  DEFINITIONS
    _WIN32
    $<$<CONFIG:Debug>:INC_OLE2>
    $<$<CONFIG:Debug>:STRICT>
    $<$<NOT:$<CONFIG:Debug>>:NDEBUGT>
  LIBS
    winmm
    ws2_32
    LiveMedia555
    BaseClasses
)
