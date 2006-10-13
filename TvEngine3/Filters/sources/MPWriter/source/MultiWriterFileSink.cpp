#include <streams.h>
#include "MultiWriterFileSink.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"
#include "TsHeader.h"
#include "MPEG1or2Demux.hh"

////////// CMultiWriterFileSink //////////

extern void Log(const char *fmt, ...) ;
CMultiWriterFileSink::CMultiWriterFileSink(UsageEnvironment& env, MultiFileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix) 
  : MediaSink(env), fOutFid(fid), fBufferSize(bufferSize) 
{
  fBuffer = new unsigned char[bufferSize];
	m_startPcr=0;
	m_highestPcr=0;
  m_bDetermineNewStartPcr=false;
 
}

CMultiWriterFileSink::~CMultiWriterFileSink() 
{
  if (fOutFid != NULL) 
  {
    fOutFid->CloseFile();
    delete fOutFid;
    fOutFid=NULL;
  }
}

CMultiWriterFileSink* CMultiWriterFileSink::createNew(UsageEnvironment& env, char const* fileName,unsigned bufferSize, Boolean oneFilePerFrame) 
{
  do 
  {
    MultiFileWriter* fid = new MultiFileWriter();
	  WCHAR wstrFileName[2048];
	  MultiByteToWideChar(CP_ACP,0,fileName,-1,wstrFileName,1+strlen(fileName));
    if (FAILED(fid->OpenFile(wstrFileName)))
    {
      delete fid;
      return NULL;
    }
    return new CMultiWriterFileSink(env, fid, bufferSize, NULL);
  } while (0);

  return NULL;
}

Boolean CMultiWriterFileSink::continuePlaying() 
{
  if (fSource == NULL) return False;

  fSource->getNextFrame(fBuffer, fBufferSize,afterGettingFrame, this,onSourceClosure, this);

  return True;
}

void CMultiWriterFileSink::afterGettingFrame(void* clientData, unsigned frameSize,
				 unsigned /*numTruncatedBytes*/,
				 struct timeval presentationTime,
				 unsigned /*durationInMicroseconds*/) {
  CMultiWriterFileSink* sink = (CMultiWriterFileSink*)clientData;
  sink->afterGettingFrame1(frameSize, presentationTime);
} 

void CMultiWriterFileSink::addData(unsigned char* data, unsigned dataSize,struct timeval presentationTime) 
{
  if (fOutFid != NULL && data != NULL) 
  {
    fOutFid->Write(data, dataSize);
  }
}

void CMultiWriterFileSink::afterGettingFrame1(unsigned frameSize,struct timeval presentationTime) 
{
	CAutoLock BufferLock(&m_Lock);
  OnRawData(fBuffer, frameSize);
  //addData(fBuffer, frameSize, presentationTime);
  // Then try getting the next frame:
  continuePlaying();
}


void CMultiWriterFileSink::OnTsPacket(byte* tsPacket)
{
  if (tsPacket==NULL) return;
  CTsHeader header (tsPacket);
  if (header.Pid==0x1e0)
  {
    PatchPcr(tsPacket,header);
    PatchPtsDts(tsPacket,header,m_startPcr);
  }
  else if (header.Pid==0x1c0)
  {
    PatchPtsDts(tsPacket,header,m_startPcr);
  }

  if (fOutFid != NULL) 
  {
    fOutFid->Write(tsPacket, 188);
  }
}

  
void CMultiWriterFileSink::ClearStreams()
{
    m_bDetermineNewStartPcr=true;
}

void CMultiWriterFileSink::PatchPcr(byte* tsPacket,CTsHeader& header)
{
  if (header.PayLoadOnly()) return;
  if (tsPacket[4]<7) return;
  if (tsPacket[5]!=0x10) return;

  // There's a PCR.  Get it
  __int64 pcrBaseHigh = (tsPacket[6]<<24)|(tsPacket[7]<<16)|(tsPacket[8]<<8)|tsPacket[9];
//  double clock = pcrBaseHigh/45000.0;
//  if ((tsPacket[10]&0x80) != 0) clock += 1/90000.0; // add in low-bit (if set)
//  unsigned short pcrExt = ((tsPacket[10]&0x01)<<8) | tsPacket[11];
//  clock += pcrExt/27000000.0;


  __int64 pcrNew=pcrBaseHigh;
  if (m_bDetermineNewStartPcr )
  {
    if (pcrNew!=0) 
    {
      m_bDetermineNewStartPcr=false;
	    //correct pcr rollover
      __int64 duration=m_highestPcr-m_startPcr;
    
      Log("Pcr change detected from:%x to:%x duration:%x", (DWORD)m_highestPcr, (DWORD)pcrNew,(DWORD)duration);
	    __int64 newStartPcr = pcrNew- (duration) ;
	    Log("Pcr new start pcr from:%x to %x ", (DWORD)m_startPcr,(DWORD)newStartPcr);
      m_startPcr=newStartPcr;
      m_highestPcr=newStartPcr;
    }
  }
  
	if (m_startPcr==0)
	{
		m_startPcr = pcrNew;
    m_highestPcr=pcrNew;
		Log("Pcr new start pcr :%x", (DWORD)m_startPcr);
	} 

  if (pcrNew > m_highestPcr)
  {
	  m_highestPcr=pcrNew;
  }

  __int64 pcrHi=pcrNew - m_startPcr;
  tsPacket[6] = ((pcrHi>>24)&0xff);
  tsPacket[7] = ((pcrHi>>16)&0xff);
  tsPacket[8] = ((pcrHi>>8)&0xff);
  tsPacket[9] = ((pcrHi)&0xff);
  tsPacket[10]=0;
  tsPacket[11]=0;
}

void CMultiWriterFileSink::PatchPtsDts(byte* tsPacket,CTsHeader& header,__int64 startPcr)
{
  if (false==header.PayloadUnitStart) return;
  int start=header.PayLoadStart;
  if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) return; 

  byte* pesHeader=&tsPacket[start];
	__int64 pts=0,dts=0;
	if (!GetPtsDts(pesHeader, pts, dts)) 
	{
		return ;
	}
	if (pts>0)
	{
		if (pts < startPcr) 
			pts=0;
		else
			pts-=startPcr;
	//	Log(" pts:%x start:%x", (DWORD)pts,(DWORD)startPcr);
		byte marker=0x21;
		if (dts!=0) marker=0x31;
		pesHeader[13]=(((pts&0x7f)<<1)+1); pts>>=7;
		pesHeader[12]= (pts&0xff);				  pts>>=8;
		pesHeader[11]=(((pts&0x7f)<<1)+1); pts>>=7;
		pesHeader[10]=(pts&0xff);					pts>>=8;
		pesHeader[9]= (((pts&7)<<1)+marker); 
	}
	if (dts >0)
	{
		if (dts < startPcr) 
			dts=0;
		else
			dts-=startPcr;

		pesHeader[18]=(((dts&0x7f)<<1)+1); dts>>=7;
		pesHeader[17]= (dts&0xff);				  dts>>=8;
		pesHeader[16]=(((dts&0x7f)<<1)+1); dts>>=7;
		pesHeader[15]=(dts&0xff);					dts>>=8;
		pesHeader[14]= (((dts&7)<<1)+0x11); 
	}
}


bool CMultiWriterFileSink::GetPtsDts(byte* pesHeader, __int64& pts, __int64& dts)
{
	pts=0;
	dts=0;
	bool ptsAvailable=false;
	bool dtsAvailable=false;
	if ( (pesHeader[7]&0x80)!=0) ptsAvailable=true;
	if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	if (ptsAvailable)
	{	
		pts+= ((pesHeader[13]>>1)&0x7f);					// 7bits	7
		pts+=(pesHeader[12]<<7);								// 8bits	15
		pts+=((pesHeader[11]>>1)<<15);					// 7bits	22
		pts+=((pesHeader[10])<<22);							// 8bits	30
    __int64 k=((pesHeader[9]>>1)&0x7);
    k <<=30;
		pts+=k;			// 3bits
	}
	if (dtsAvailable)
	{
		dts= (pesHeader[18]>>1);								// 7bits	7
		dts+=(pesHeader[17]<<7);								// 8bits	15
		dts+=((pesHeader[16]>>1)<<15);					// 7bits	22
		dts+=((pesHeader[15])<<22);							// 8bits	30
    __int64 k=((pesHeader[14]>>1)&0x7);
    k <<=30;
		dts+=k;			// 3bits
	
	}
	return (ptsAvailable||dtsAvailable);
}