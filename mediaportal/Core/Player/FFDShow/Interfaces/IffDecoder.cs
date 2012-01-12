#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
  [Guid("00F99063-70D5-4bcc-9D88-3801F3E3881B"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IffDecoder
  {
    int compat_getParam(uint paramID, out int value);
    int compat_getParam2(uint paramID);
    int compat_putParam(uint paramID, int value);
    int compat_getNumPresets(out uint value);
    int compat_getPresetName(uint i, [In, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint len);
    int compat_getActivePresetName([In, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint len);
    int compat_setActivePreset([In, MarshalAs(UnmanagedType.AnsiBStr)] string name, int create);
    int compat_getAVIdimensions(out uint x, out uint y);
    int compat_getAVIfps(out uint fps1000);
    int compat_saveActivePreset([In, MarshalAs(UnmanagedType.AnsiBStr)] string name);
    int compat_saveActivePresetToFile([In, MarshalAs(UnmanagedType.AnsiBStr)] string flnm);
    int compat_loadActivePresetFromFile([In, MarshalAs(UnmanagedType.AnsiBStr)] string flnm);
    int compat_removePreset([In, MarshalAs(UnmanagedType.AnsiBStr)] string name);
    int compat_notifyParamsChanged();
    int compat_getAVcodecVersion([Out, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint len);
    int compat_getPPmode(out uint ppmode);
    int compat_getRealCrop(out uint left, out uint top, out uint right, out uint bottom);
    int compat_getMinOrder2();
    int compat_getMaxOrder2();
    int compat_saveGlobalSettings();
    int compat_loadGlobalSettings();
    int compat_saveDialogSettings();
    int compat_loadDialogSettings();
    //int compat_getPresets (Tpresets *presets2);
    //int compat_setPresets (const Tpresets *presets2);
    int compat_getPresets(IntPtr presets2);
    int compat_setPresets(IntPtr presets2);
    int compat_savePresets();
    //int compat_getPresetPtr (Tpreset**preset);
    //int compat_setPresetPtr (Tpreset *preset);
    int compat_getPresetPtr(IntPtr preset);
    int compat_setPresetPtr(IntPtr preset);
    int compat_renameActivePreset([In, MarshalAs(UnmanagedType.AnsiBStr)] string newName);
    int compat_setOnChangeMsg(IntPtr wnd, uint msg);
    int compat_setOnFrameMsg(IntPtr wnd, uint msg);
    int compat_isDefaultPreset([In, MarshalAs(UnmanagedType.AnsiBStr)] string presetName);
    int compat_showCfgDlg(IntPtr owner);
    int compat_getXvidVersion([Out, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint len);
    //int compat_getMovieSource (const TvideoCodecDec* *moviePtr);
    int compat_getMovieSource(IntPtr moviePtr);
    int compat_getOutputDimensions(out uint x, out uint y);
    int compat_getCpuUsage2();
    int compat_getOutputFourcc([Out, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint len);
    int compat_getInputBitrate2();
    int compat_getHistogram(uint[] dst);
    int compat_setFilterOrder(uint filterID, uint newOrder);
    //int compat_buildHistogram (const TffPict *pict,int full);
    int compat_buildHistogram(IntPtr pict, int full);
    int compat_cpuSupportsMMX();
    int compat_cpuSupportsMMXEXT();
    int compat_cpuSupportsSSE();
    int compat_cpuSupportsSSE2();
    int compat_cpuSupports3DNOW();
    int compat_cpuSupports3DNOWEXT();
    int compat_getAVIfps1000_2();
    int compat_getParamStr(uint paramID, [Out, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint buflen);
    int compat_putParamStr(uint paramID, [In, MarshalAs(UnmanagedType.AnsiBStr)] string buf);
    int compat_invParam(uint paramID);
    int compat_getInstance(IntPtr hi);
    int compat_saveKeysSettings();
    int compat_loadKeysSettings();
    int compat_seek(int seconds);
    int compat_tell(out int seconds);
    int compat_getDuration(out int seconds);
    int compat_getKeyParamCount2();
    int compat_getKeyParamDescr(uint i, [Out, MarshalAs(UnmanagedType.AnsiBStr)] string[] descr);
    int compat_getKeyParamKey2(uint i);
    int compat_setKeyParamKey(uint i, int key);
    //int compat_getImgFilters (TimgFilters* *imgFiltersPtr);
    int compat_getImgFilters(IntPtr imgFiltersPtr);
    int compat_getQuant(out int[] quantPtr);
    int compat_calcNewSize(uint inDx, uint inDy, out uint outDx, out uint outDy);
    int compat_grabNow();
    int compat_getOverlayControlCapability(int idff); //S_OK - can be set, S_FALSE - not supported
    int compat_getParamName(uint i, [Out, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint len);
    //int compat_getTranslator (Ttranslate* *trans);
    int compat_getTranslator(IntPtr trans);
    int compat_getIffDecoderVersion2();
    int compat_lock(int lockId);
    int compat_unlock(int lockId);
    int compat_getInstance2();
    //int compat_getGraph(out IFilterGraph graphPtr);
    //int compat_getConfig (Tconfig* *configPtr);
    int compat_getGraph(IntPtr graphPtr);
    int compat_getConfig(IntPtr configPtr);
    int compat_initDialog();
    int compat_initPresets();
    int compat_calcMeanQuant(out float quant);
    int compat_initKeys();
    //int compat_savePresetMem (void *buf,uint len); //if len=0, then buf should point to int variable which will be filled with required buffer length
    //int compat_loadPresetMem (const void *buf,uint len);
    //int compat_getGlobalSettings (TglobalSettingsDecVideo* *globalSettingsPtr);
    int compat_createTempPreset([In, MarshalAs(UnmanagedType.AnsiBStr)] string presetName);
    string compat_getParamStr2(uint paramID); //returns const pointer to string, NULL if fail
    int compat_findAutoSubflnm2();
    int compat_getCurrentFrameTime(out uint sec);
    int compat_getFrameTime(uint framenum, out uint sec);
    int compat_getCurTime2();
    //int compat_getPostproc (Tlibmplayer* *postprocPtr);
    int compat_stop();
    int compat_run();
    int compat_getState2();
    int compat_resetFilter(uint filterID);
    int compat_resetFilterEx(uint filterID, uint filterPageId);
    int compat_getFilterTip(uint filterID, [Out, MarshalAs(UnmanagedType.AnsiBStr)] string buf, uint buflen);

    int compat_getFilterTipEx(uint filterID, uint filterPageId, [Out, MarshalAs(UnmanagedType.AnsiBStr)] string buf,
                              uint buflen);

    int compat_filterHasReset(uint filterID);
    int compat_filterHasResetEx(uint filterID, uint filterPageId);
    int compat_shortOSDmessage([In, MarshalAs(UnmanagedType.AnsiBStr)] string msg, uint duration);
    //duration is in frames
    int compat_shortOSDmessageAbsolute([In, MarshalAs(UnmanagedType.AnsiBStr)] string msg, uint duration, uint posX,
                                       uint posY);

    //duration is in frames
    int compat_cleanShortOSDmessages();
    //int compat_setImgFilters (TimgFilters *imgFiltersPtr);
    int compat_registerSelectedMediaTypes();
    int compat_getFrameTimes(out double start, out double stop);
    int compat_getSubtitleTimes(out double start, out double stop);
    int compat_resetSubtitleTimes();
    int compat_setFrameTimes(double start, double stop);
    int compat_cpuSupportsSSE41();
    int compat_cpuSupportsSSE42();
    int compat_cpuSupportsSSE4A();
    int compat_cpuSupportsSSE5();
    int compat_cpuSupportsSSE3();
    int compat_cpuSupportsSSSE3();
    int compat_getIffDecoder2Version();
    int compat_getParamStrW(uint paramID, [Out, MarshalAs(UnmanagedType.BStr)] string buf, uint buflen);
    int compat_putParamStrW(uint paramID, [In, MarshalAs(UnmanagedType.BStr)] string buf);
  } ;
}