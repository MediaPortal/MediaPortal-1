#include "stdafx.h"
#include "MPUrlSourceSplitter.h"
#include "StaticLogger.h"
#include "FFmpegLogger.h"
#include <curl/curl.h>

extern "C++" CFFmpegLogger *ffmpegLogger;
extern "C++" CStaticLogger *staticLogger;

int main(int argc, const char * argv[])
{
  remove("C:\\ProgramData\\Team MediaPortal\\MediaPortal\\log\\MPUrlSourceSplitter.log");
  remove("D:\\res.mp4");
  LPUNKNOWN lpunk = NULL;
  HRESULT phr = S_OK;
  staticLogger = new CStaticLogger(&phr);
  ffmpegLogger = new CFFmpegLogger(&phr, staticLogger);
  curl_global_init(CURL_GLOBAL_ALL);

  CMPUrlSourceSplitter* mpu = (CMPUrlSourceSplitter*)CMPUrlSourceSplitter::CreateInstanceUrlSourceSplitter(lpunk, &phr);
  mpu->AddRef();
  mpu->Download(L"https://media.dumpert.nl/mobile/b06ebdc1_Trevorvibe_1.mp4.mp4.mp4", L"D:\\res.mp4");
  delete mpu;
  return 0;
}
