#include "TsMPEG2TransportFileServerMediaSubsession.h"
#include "SimpleRTPSink.hh"
#include "TsStreamFileSource.hh"
#include "MPEG2TransportStreamFramer.hh"


TsMPEG2TransportFileServerMediaSubsession* TsMPEG2TransportFileServerMediaSubsession::createNew(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource) 
{
  return new TsMPEG2TransportFileServerMediaSubsession(env, fileName, reuseFirstSource);
}

TsMPEG2TransportFileServerMediaSubsession::TsMPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,char const* fileName, Boolean reuseFirstSource)
  : FileServerMediaSubsession(env, fileName, reuseFirstSource) 
{
}

TsMPEG2TransportFileServerMediaSubsession::~TsMPEG2TransportFileServerMediaSubsession() 
{
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* TsMPEG2TransportFileServerMediaSubsession::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) 
{
  estBitrate = 5000; // kbps, estimate

  // Create the video source:
  unsigned const inputDataChunkSize= TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
  TsStreamFileSource* fileSource= TsStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
  if (fileSource == NULL) return NULL;
  fFileSize = fileSource->fileSize();

  // Create a framer for the Transport Stream:
  return MPEG2TransportStreamFramer::createNew(envir(), fileSource);
}

RTPSink* TsMPEG2TransportFileServerMediaSubsession::createNewRTPSink(Groupsock* rtpGroupsock,unsigned char /*rtpPayloadTypeIfDynamic*/,FramedSource* /*inputSource*/) 
{
  return SimpleRTPSink::createNew(envir(), rtpGroupsock,
				  33, 90000, "video", "mp2t",
				  1, True, False /*no 'M' bit*/);
}
