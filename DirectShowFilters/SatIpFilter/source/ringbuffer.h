class RingBuffer
{
public:
	RingBuffer(int sizeBytes);
	~RingBuffer();
	int Read(unsigned char* dataPtr, int numBytes);
	unsigned char ReadOneByte();
	unsigned char MovePointerAndReadOneByte(int numBytes);
	int Write(unsigned char *dataPtr, int numBytes);
	bool Empty(void);
	int GetSize();
	int GetWriteAvail();
	int GetReadAvail();
private:
	unsigned char * _data;
	int _size;
	int _readPtr;
	int _writePtr;
	int _writeBytesAvail;
};