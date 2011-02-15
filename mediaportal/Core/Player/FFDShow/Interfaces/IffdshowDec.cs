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
using System.Security;

namespace FFDShow.Interfaces
{
  [Guid("10F99065-70D5-4bcc-9D88-3801F3E3881B"), SuppressUnmanagedCodeSecurity,
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IffdshowDec
  {
    int getVersion2();
    int saveKeysSettings();
    int loadKeysSettings();

    //int getGraph(/*IFilterGraph*/IntPtr graphPtr);
    int getGraph([Out, MarshalAs(UnmanagedType.Interface)] out object graphPtr);

    int seek(int seconds);
    int tell(out int seconds);
    int stop();
    int run();
    int getState2();
    int getDuration(out int seconds);
    int getCurTime2();
    int initKeys();
    int getKeyParamCount2();
    int getKeyParamDescr(uint i, out string descr);
    int getKeyParamKey2(uint i);
    int setKeyParamKey(uint i, int key);
    int getNumPresets(out uint value);
    int initPresets();
    int getPresetName(uint i, string buf, int len);
    int getActivePresetName(string buf, int len);
    int setActivePreset(string name, int create);
    int saveActivePreset(string name);
    int saveActivePresetToFile(string flnm);
    int loadActivePresetFromFile(string flnm);
    int removePreset(string name);

    //int getPresets(Tpresets *presets2);
    //int setPresets(Tpresets *presets2);
    int getPresets(object presets2);
    int setPresets(object presets2);

    int savePresets();

    //int getPresetPtr(Tpreset**preset);
    //int setPresetPtr(Tpreset *preset);
    int getPresetPtr(object preset);
    int setPresetPtr(object preset);

    int renameActivePreset(string newName);
    int isDefaultPreset(string presetName);
    int createTempPreset(string presetName);
    int getMinOrder2();
    int getMaxOrder2();
    int resetFilter(uint filterID);
    int resetFilterEx(uint filterID, uint filterPageId);
    int getFilterTip(uint filterID, string buf, int buflen);
    int getFilterTipEx(uint filterID, uint filterPageId, string buf, int buflen);
    int filterHasReset(uint filterID);
    int filterHasResetEx(uint filterID, uint filterPageId);

    //int getPresetsPtr(Tpresets* *presetsPtr);
    //int newSample(IMediaSample* *samplePtr);
    //int deliverSample_unused(IMediaSample *sample);
    //STDMETHOD_(TfilterIDFF*,getFilterIDFF_notimpl();
    int getPresetsPtr(object presetsPtr);
    int newSample(object samplePtr);
    int deliverSample_unused(object sample);

    object getFilterIDFF_notimpl();
    int resetOrder();
    int resetKeys();
    int putStringParams(string parameters, char sep, int loaddef);

    //STDMETHOD_(TfilterIDFF*,getNextFilterIDFF();
    object getNextFilterIDFF();

    int cyclePresets(int step);
    int exportKeysToGML(string flnm);
    int getShortDescription(out string buf, int buflen);
    string getActivePresetName2();

    //int createPresetPages(string presetname,TffdshowPageDec *pages);
    int createPresetPages(string presetname, object pages);

    int getEncoderInfo(out string buf, int buflen);
    string getDecoderName();

    //int getFilterIDFFs(string presetname,TfilterIDFFs* *filters);
    int getFilterIDFFs(string presetname, object filters);

    int initRemote();
    int saveRemoteSettings();
    int loadRemoteSettings();
    int setFilterOrder(uint filterID, uint newOrder);
    uint getPresetAutoloadItemsCount2();

    int getPresetAutoloadItemInfo(uint index, out string name, out string hint, out int allowWildcard, out int isL,
                                  out int isVal, string val, int vallen, out int isList, out int isHelp);

    int setPresetAutoloadItem(uint index, int isL, string val);
    string getPresetAutoloadItemList(uint paramIndex, uint listIndex);
    string[] getSupportedFOURCCs();

    //STDMETHOD_(Tstrptrs*,getCodecsList();
    //int queryFilterInterface(IID &iid,void **ptr);
    //int setOnNewFiltersMsg(IntPtr wnd,uint msg);
    object getCodecsList();
    int queryFilterInterface(out Guid iid, IntPtr[] ptr);
    int setOnNewFiltersMsg(int wnd, uint msg);

    int sendOnNewFiltersMsg();
    int setKeyParamKeyCheck(uint i, int key, out int prev, out string prevDescr);
    int getInputBitrate2();
    int getPresetAutoloadItemHelp(uint index, out string helpPtr);

    //STDMETHOD_(TinputPin*, getInputPin();
    //STDMETHOD_(CTransformOutputPin*, getOutputPin();
    object getInputPin();
    object getOutputPin();

    int extractExternalStreams();

    //int getExternalStreams(void **pAudioStreams, void **pSubtitleStreams);
    //int setExternalStream(int group, long streamNb);
    int getExternalStreams(IntPtr[] pAudioStreams, IntPtr[] pSubtitleStreams);
    int setExternalStream(int group, long streamNb);
    int getCurrentSubtitlesFile([Out, MarshalAs(UnmanagedType.LPWStr)] out string ppSubtitleFile);
    int setSubtitlesFile(string pSubtitleFile);
  } ;
}