#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FFDShow.Interfaces
{
    [Guid("00F99064-70D5-4bcc-9D88-3801F3E3881B"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IffdshowDecVideo
    {
        int getVersion2();
        int getAVIdimensions(out uint x, out uint y);
        int getAVIfps(out uint fps1000);
        int getAVcodecVersion(out string buf, int len);
        int getPPmode(out uint ppmode);
        int getRealCrop(out uint left, out uint top, out uint right, out uint bottom);
        int getXvidVersion(out string buf, int len);
        //int getMovieSource(const TvideoCodecDec* *moviePtr);
        int getOutputDimensions(out uint x, out uint y);
        int getOutputFourcc(out string buf, int len);
        int getInputBitrate2();
        //int getHistogram (uint dst[256]);
        int setFilterOrder(uint filterID, uint newOrder);
        //int buildHistogram_(const TffPict *pict,int full);
        int getAVIfps1000_2();
        int getCurrentFrameTime(out uint sec);
        //int getImgFilters_(void* *imgFiltersPtr);
        //int getQuant(int* *quantPtr);
        int calcNewSize(uint inDx, uint inDy, out uint outDx, out uint outDy);
        int grabNow();
        int getOverlayControlCapability(int idff); //S_OK - can be set, S_FALSE - not supported
        //int lock(int lockId);
        //int unlock(int lockId);
        //int calcMeanQuant(float *quant);
        int findAutoSubflnm2();
        int getFrameTime(uint framenum, out uint sec);
        int shortOSDmessage(string msg, uint duration); //duration is in frames
        int cleanShortOSDmessages();
        int shortOSDmessageAbsolute(string msg, uint duration, uint posX, uint posY); //duration is in frames
        //int setImgFilters_(void *imgFiltersPtr);
        int registerSelectedMediaTypes();
        int getFrameTimes(out double start, out double stop);
        int getSubtitleTimes(out double start, out double stop);
        int resetSubtitleTimes();
        int setFrameTimes(double start, double stop);
        //int getCodecId(const BITMAPINFOHEADER *hdr,const GUID *subtype,FOURCC *AVIfourcc);
        //int getFontManager(TfontManager* *fontManagerPtr);
        int getInIsSync();
        int getVideoWindowPos(out int left, out int top, out uint width, out uint height);
        uint getSubtitleLanguagesCount2();
        int getSubtitleLanguageDesc(uint num, [In, MarshalAs(UnmanagedType.LPWStr)] string descPtr);
        //int fillSubtitleLanguages(IntPtr[] langs);
        int getFrameTimeMS(uint framenum, out uint msec);
        int getCurrentFrameTimeMS(out uint msec);
        int frameStep(int diff);
        int textPinConnected_(uint num);
        int cycleSubLanguages(int diff);
        //int getLevelsMap(uint map[256]);
        IntPtr findAutoSubflnm3();
        int setAverageTimePerFrame(out double avg, int useDef);
        int getLate(out double latePtr);
        int getAverageTimePerFrame(out double avg);
        //string getEmbeddedSubtitleName2_(uint num);
        //int putHistogram_(uint Ihistogram[256]);
        String getCurrentSubFlnm(); // Does not work
        int quantsAvailable();
        int isNeroAVC_();
        //int findOverlayControl(IMixerPinConfig2* *overlayPtr);
        //int getVideoDestRect(RECT *r);
        //FOURCC getMovieFOURCC();
        int getRemainingFrameTime(out uint sec);
        int getInputSAR(out uint a1, out uint a2);
        int getInputDAR(out uint a1, out uint a2);
        //int getQuantMatrices(unsigned char intra8[64],unsigned char inter8[64]);
        //string findAutoSubflnms(IcheckSubtitle *checkSubtitle);
        int addClosedCaption(string line);
        int hideClosedCaptions();
        int getConnectedTextPinCnt();
        //int getConnectedTextPinInfo(int i,const tchar* *name,int *id,int *found);
        //int getConnectedTextPinInfo(int i,const tchar* *trackName, const tchar* *langName,int *id,int *found);
        //int registerOSDprovider(IOSDprovider *provider,const char *name);
        //int unregisterOSDprovider(IOSDprovider *provider);
        //int findOverlayControl2(IhwOverlayControl* *overlayPtr);
        int getOSDtime();
        int getQueuedCount();
        double getLate();
        IntPtr get_current_idct();
        int get_time_on_ffdshow();
        int get_time_on_ffdshow_percent();
        int shouldSkipH264loopFilter();
        int get_downstreamID();
        IntPtr getAviSynthInfo();
        int lockCSReceive();
        int unlockCSReceive();
        //STDMETHOD_(ToutputVideoSettings*,getToutputVideoSettings();
        int getBordersBrightness();
        int getChaptersList(IntPtr[] ppChaptersList);
        int get_CurrentTime(out double time);
        //const Trect*,getDecodedPictdimensions();
        //HANDLE getGlyphThreadHandle();
        IntPtr getRateInfo();
        //int lock_csCodecs_and_imgFilters();
        //int unlock_csCodecs_and_imgFilters();
        //STDMETHOD_(void*, get_csReceive_ptr();
        //STDMETHOD_(void*, get_csCodecs_and_imgFilters_ptr();
        //int reconnectOutput(const TffPict &newpict);
    };
}
