#define TS_PACKET_SYNC 0x47
#define TS_PACKET_LEN  188

// Name given to the pipe
#define PIPE_NAME "\\\\.\\Pipe\\MyNamedPipe"
// Pipe name format - \\.\pipe\pipename

#define PIPE_BUFFER_SIZE 1024*10 // 10k
#define ACK_MESG_RECV "Ok"