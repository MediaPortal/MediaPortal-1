CC     = gcc
RM     = rm -f
OBJS   = KillProcDLL.o \
         pluginapi.o

LIBS   = -shared -Wl,--kill-at -m32
CFLAGS = -DUNICODE -D_UNICODE -m32 -fno-diagnostics-show-option

.PHONY: clean all clear

all: ../bin/x86-unicode/KillProcDLL.dll

clean:
	$(RM) $(OBJS) ../bin/x86-unicode/KillProcDLL.dll

clear:
	$(RM) $(OBJS)

../bin/x86-unicode/KillProcDLL.dll: $(OBJS)
	$(CC) -Wall -s -O2 -o $@ $(OBJS) $(LIBS)

KillProcDLL.o: KillProcDLL.c exdll.h
	$(CC) -Wall -s -O2 -c $< -o $@ $(CFLAGS)

pluginapi.o: pluginapi.c pluginapi.h
	$(CC) -Wall -s -O2 -c $< -o $@ $(CFLAGS)

