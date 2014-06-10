#ifndef   CONFIG_H
#define   CONFIG_H

#define TS_PACKET_SYNC 0x47
#define TS_PACKET_LEN  188

// Name given to the pipe
#define PIPE_NAME "\\\\.\\Pipe\\MyNamedPipe"
// Pipe name format - \\.\pipe\pipename

#define PIPE_BUFFER_SIZE 1024*10 // 10k
#define ACK_MESG_RECV "Ok"

const int NUMBER_OF_STREAMING_SLOTS = 8;

#define SATIP_PROT_ADDPID 0u
#define SATIP_PROT_DELPID 1u
#define SATIP_PROT_SYNCPID 2u
#define SATIP_PROT_CLIENTIP 3u
#define SATIP_PROT_CLIENTPORT 4u
#define SATIP_PROT_STARTSTREAM 5u
#define SATIP_PROT_STOPSTREAM 6u
#define SATIP_PROT_NEWSLOT 7u

#endif