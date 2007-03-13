#include <streams.h>
#include "MultiWriterFileSink.h"
#include "GroupsockHelper.hh"
#include "OutputFile.hh"
#include "TsHeader.h"
#include "MPEG1or2Demux.hh"

////////// CMultiWriterFileSink //////////

extern void LogDebug(const char *fmt, ...) ;
CMultiWriterFileSink::CMultiWriterFileSink(UsageEnvironment& env, MultiFileWriter* fid, unsigned bufferSize,char const* perFrameFileNamePrefix) 
  : MediaSink(env), fOutFid(fid), fBufferSize(bufferSize) 
{
  fBuffer = new unsigned char[bufferSize];
	m_startPcr=0;
	m_highestPcr=0;
  m_bDetermineNewStartPcr=false;
  m_bStartPcrFound=false;
  LogDebug("CMultiWriterFileSink::ctor");
 
}

CMultiWriterFileSink::~CMultiWriterFileSink() 
{
  LogDebug("CMultiWriterFileSink::dtor");
  if (fOutFid != NULL) 
  {
    fOutFid->CloseFile();
    delete fOutFid;
    fOutFid=NULL;
  }
}

CMultiWriterFileSink* CMultiWriterFileSink::createNew(UsageEnvironment& env, char const* fileName,int minFiles, int maxFiles, ULONG maxFileSize,unsigned bufferSize, Boolean oneFilePerFrame) 
{
  do 
  {
    LogDebug("CMultiWriterFileSink::create file:%s",fileName);
    MultiFileWriter* fid = new MultiFileWriter();
    fid->setMinTSFiles(minFiles);
    fid->setMaxTSFiles(maxFiles);
    fid->setChunkReserve(maxFileSize);
    fid->setMaxTSFileSize(maxFileSize);
	  WCHAR wstrFileName[2048];
	  MultiByteToWideChar(CP_ACP,0,fileName,-1,wstrFileName,1+strlen(fileName));
    if (FAILED(fid->OpenFile(wstrFileName)))
    {
      LogDebug("CMultiWriterFileSink::create file:%s failed",fileName);
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
  /*
  if (tsPacket==NULL) return;
  CTsHeader header (tsPacket);
  if (header.Pid==0x1e0)
  {
    PatchPcr(tsPacket,header);
    PatchPtsDts(tsPacket,header,m_startPcr);
    
  }
  else if (header.Pid==0x1c0)
  {
    PatchPcr(tsPacket,header);
    PatchPtsDts(tsPacket,header,m_startPcr);
  }

  if (fOutFid != NULL) 
  {
		if (m_bDetermineNewStartPcr==false && m_bStartPcrFound) 
		{
      fOutFid->Write(tsPacket, 188);
    }
  }*/
      fOutFid->Write(tsPacket, 188);

}

  
void CMultiWriterFileSink::ClearStreams()
{
    m_bDetermineNewStartPcr=true;
}

void CMultiWriterFileSink::PatchPcr(byte* tsPacket,CTsHeader& header)
{
  if (header.PayLoadOnly()) return;
  if (tsPacket[4]<7) return; //adaptation field length
  if (tsPacket[5]!=0x10) return;


/*
	char buf[1255];
	strcpy(buf,"");
	for (int i=0; i < 30;++i)
	{
		char tmp[200];
		sprintf(tmp,"%02.2x ", tsPacket[i]);
		strcat(buf,tmp);
	}
	LogDebug(buf);*/

  // There's a PCR.  Get it
	UINT64 pcrBaseHigh=0LL;
	UINT64 k=tsPacket[6]; k<<=25LL;pcrBaseHigh+=k;
	k=tsPacket[7]; k<<=17LL;pcrBaseHigh+=k;
	k=tsPacket[8]; k<<=9LL;pcrBaseHigh+=k;
	k=tsPacket[9]; k<<=1LL;pcrBaseHigh+=k;
	k=((tsPacket[10]>>7)&0x1); pcrBaseHigh +=k;


//  UINT64 pcrBaseHigh = (tsPacket[6]<<24)|(tsPacket[7]<<16)|(tsPacket[8]<<8)|tsPacket[9];
//	pcrBaseHigh<<=1LL;
//pcrBaseHigh += ((tsPacket[10]>>7)&0x1);
//  double clock = pcrBaseHigh/45000.0;
//  if ((tsPacket[10]&0x80) != 0) clock += 1/90000.0; // add in low-bit (if set)
//  unsigned short pcrExt = ((tsPacket[10]&0x01)<<8) | tsPacket[11];
//  clock += pcrExt/27000000.0;


  UINT64 pcrNew=pcrBaseHigh;
  if (m_bDetermineNewStartPcr )
  {
   if (pcrNew!=0LL) 
    {
      m_bDetermineNewStartPcr=false;
	    //correct pcr rollover
      UINT64 duration=m_highestPcr-m_startPcr;
    
      LogDebug("Pcr change detected from:%I64d to:%I64d  duration:%I64d ", m_highestPcr, pcrNew,duration);
	    UINT64 newStartPcr = pcrNew- (duration) ;
	    LogDebug("Pcr new start pcr from:%I64d  to %I64d  ", m_startPcr,newStartPcr);
      m_startPcr=newStartPcr;
      m_highestPcr=newStartPcr;
    }
  }
  
	if (m_bStartPcrFound==false)
	{
		m_bStartPcrFound=true;
		m_startPcr = pcrNew;
    m_highestPcr=pcrNew;
		LogDebug("Pcr new start pcr :%I64d", m_startPcr);
	} 

  if (pcrNew > m_highestPcr)
  {
	  m_highestPcr=pcrNew;
  }

	
  UINT64 pcrHi=pcrNew - m_startPcr;
  tsPacket[6] = ((pcrHi>>25)&0xff);
  tsPacket[7] = ((pcrHi>>17)&0xff);
  tsPacket[8] = ((pcrHi>>9)&0xff);
  tsPacket[9] = ((pcrHi>>1)&0xff);
  tsPacket[10]=	 (pcrHi&0x1);
  tsPacket[11]=0;
//	LogDebug("pcr: org:%x new:%x start:%x", (DWORD)pcrBaseHigh,(DWORD)pcrHi,(DWORD)m_startPcr);
}

void CMultiWriterFileSink::PatchPtsDts(byte* tsPacket,CTsHeader& header,UINT64 startPcr)
{
  if (false==header.PayloadUnitStart) return;
  int start=header.PayLoadStart;
  if (tsPacket[start] !=0 || tsPacket[start+1] !=0  || tsPacket[start+2] !=1) return; 

  byte* pesHeader=&tsPacket[start];
	UINT64 pts=0LL;
	UINT64 dts=0LL;
	if (!GetPtsDts(pesHeader, pts, dts)) 
	{
		return ;
	}
	if (pts>0LL)
	{
		/*
		char buf[1255];
		strcpy(buf,"");
		for (int i=0; i < 30;++i)
		{
			char tmp[200];
			sprintf(tmp,"%02.2x ", tsPacket[i]);
			strcat(buf,tmp);
		}
		LogDebug(buf);
		*/
		UINT64 ptsorg=pts;
		if (pts > startPcr) 
			pts = (UINT64)( ((UINT64)pts) - ((UINT64)startPcr) );
		else pts=0LL;
	//	LogDebug("pts: org:%I64d new:%I64d start:%I64d pid:%x", ptsorg,pts,startPcr,header.Pid);
		
		byte marker=0x21;
		if (dts!=0) marker=0x31;
		pesHeader[13]=(((pts&0x7f)<<1)+1); pts>>=7;
		pesHeader[12]= (pts&0xff);				  pts>>=8;
		pesHeader[11]=(((pts&0x7f)<<1)+1); pts>>=7;
		pesHeader[10]=(pts&0xff);					pts>>=8;
		pesHeader[9]= (((pts&7)<<1)+marker); 
	}
	if (dts >0LL)
	{
		if (dts > startPcr) 
			dts = (UINT64)( ((UINT64)dts) - ((UINT64)startPcr) );
		else dts=0LL;
		pesHeader[18]=(((dts&0x7f)<<1)+1); dts>>=7;
		pesHeader[17]= (dts&0xff);				  dts>>=8;
		pesHeader[16]=(((dts&0x7f)<<1)+1); dts>>=7;
		pesHeader[15]=(dts&0xff);					dts>>=8;
		pesHeader[14]= (((dts&7)<<1)+0x11); 
	}
}


bool CMultiWriterFileSink::GetPtsDts(byte* pesHeader, UINT64& pts, UINT64& dts)
{
	pts=0LL;
	dts=0LL;
	bool ptsAvailable=false;
	bool dtsAvailable=false;
	if ( (pesHeader[7]&0x80)!=0) ptsAvailable=true;
	if ( (pesHeader[7]&0x40)!=0) dtsAvailable=true;
	if (ptsAvailable)
	{	
		pts+= ((pesHeader[13]>>1)&0x7f);				// 7bits	7
		pts+=(pesHeader[12]<<7);								// 8bits	15
		pts+=((pesHeader[11]>>1)<<15);					// 7bits	22
		pts+=((pesHeader[10])<<22);							// 8bits	30
    UINT64 k=((pesHeader[9]>>1)&0x7);
    k <<=30LL;
		pts+=k;			// 3bits
		pts &= 0x1FFFFFFFFLL;
	}
	if (dtsAvailable)
	{
		dts= (pesHeader[18]>>1);								// 7bits	7
		dts+=(pesHeader[17]<<7);								// 8bits	15
		dts+=((pesHeader[16]>>1)<<15);					// 7bits	22
		dts+=((pesHeader[15])<<22);							// 8bits	30
    UINT64 k=((pesHeader[14]>>1)&0x7);
    k <<=30LL;
		dts+=k;			// 3bits
		dts &= 0x1FFFFFFFFLL;
	
	}
	
	return (ptsAvailable||dtsAvailable);
}