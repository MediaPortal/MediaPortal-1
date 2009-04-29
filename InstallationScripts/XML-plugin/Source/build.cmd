@ECHO OFF
Set MSSDK=D:\Program Files\Microsoft Platform SDK
Set VCDIR=D:\Program Files\Microsoft Visual C++ Toolkit 2003

Set PATH=%VCDIR%\bin;%MSSDK%\bin;%PATH%
Set INCLUDE=%MSSDK%\include;%VCDIR%\include;%INCLUDE%
Set LIB=%MSSDK%\lib;%VCDIR%\lib;%LIB%

cl /O1 /EHsc xml.cpp tinystr.cpp tinyxml.cpp tinyxmlerror.cpp tinyxmlparser.cpp action_store.cpp lex_util.cpp node_set.cpp tokenlist.cpp xml_util.cpp xpath_expression.cpp xpath_processor.cpp xpath_stack.cpp xpath_static.cpp xpath_stream.cpp xpath_syntax.cpp /LD /link kernel32.lib libc.lib /OPT:NOWIN98 /NODEFAULTLIB /OUT:xml.dll

del *.obj
del xml.exp
del xml.lib
@PAUSE