# Makefile for use with MSVC 6.0 for build win32 executable.

DEBUG = 0

CC = cl.exe
CXX = $(CC)
AR = lib.exe
RM = del
LINK = link.exe
NMS = "C:\Program Files\Compuware\DriverStudio\SoftICE\nmsym.exe"

CFLAGS = -nologo -c -ML -W3 -WX -GX
LFLAGS = -nologo -machine:i386 -incremental:no -nodefaultlib -pdb:none
NMSOPT = /TRANSLATE:SOURCE,PACKAGE,ALWAYS /SOURCE:.;

LFLAGS = $(LFLAGS) -subsystem:windows -dll -entry:"_DllMainCRTStartup" -opt:nowin98

!if "$(DEBUG)" == "1"
CFLAGS = $(CFLAGS) -D"_DEBUG" -D"DEBUG" -Z7 -Oid
LFLAGS = $(LFLAGS) -debug -debug:notmapped,FULL -debugtype:cv
!else
CFLAGS = $(CFLAGS) -D"NDEBUG" -D"RELEASE" -O1 -GD -GF -LD -Zl
LFLAGS = $(LFLAGS) -release -debug:none
!endif

PROJECT = nsSCM

LIBRARY_OBJS =
LIBLIST = libc.lib oldnames.lib advapi32.lib kernel32.lib user32.lib

.SUFFIXES: .c .h .cpp .obj $(SUFFIXES)

all: $(PROJECT).dll

clean:
	if exist *.obj $(RM) *.obj
	if exist *.lib $(RM) *.lib
	if exist *.dll $(RM) *.dll
	if exist *.exe $(RM) *.exe
	if exist *.exp $(RM) *.exp
	if exist *.pdb $(RM) *.pdb
	if exist *.nms $(RM) *.nms
	@echo $(PROJECT) cleaned...

$(PROJECT).dll: Makefile $(PROJECT).obj $(LIBRARY_OBJS)
	$(LINK) $(LFLAGS) -out:$@ $(PROJECT).obj $(LIBRARY_OBJS) $(LIBLIST)
!if "$(DEBUG)" == "1"
	$(NMS) $(NMSOPT) $@
	$(NMS) /SYM:$(PROJECT).NMS;
!endif

lib: $(LIBRARY_OBJS)
	$(AR) -out:$(PROJECT).lib $(LIBRARY_OBJS)

src zip:
	@echo ------------------------------------------------------------------
	@echo Backing up to archive $(PROJECT).zip
	@echo ------------------------------------------------------------------
	@cd ..
	-@zip.exe -ru9D $(PROJECT).zip $(PROJECT) -x *.bak *.err *.exe *.exp *.lib *.dll *.map *.nms *.o *.obj *.rar *.scc
	@cd $(PROJECT)

.c.obj:
	$(CXX) $(CFLAGS) -Fo$* -c $<

.cpp.obj:
	$(CXX) $(CFLAGS) -Fo$* -c $<
