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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.Enum
{
  internal enum IoControlCode
  {
    SetTunerPower = 100,
    GetTunerPower = 101,

    // Obsolete, replaced with SetLnbData/GetLnbData.
    //SetLnb = 102,
    //GetLnb = 103,

    SetDiseqc = 104,
    GetDiseqc = 105,

    LockTuner = 106,
    GetTunerValues = 107,
    GetSignalQualityStrength = 108,

    StartCapture = 109,
    StopCapture = 110,
    GetRingBufferStatus = 111,
    GetCaptureData = 112,

    SetPidFilterInfo = 113,
    GetPidFilterInfo = 114,

    StartRemoteControl = 115,
    StopRemoteControl = 116,
    AddRemoteControlEvent = 117,
    RemoveRemoteControlEvent = 118,
    GetRemoteControlValue = 119,

    ResetDevice = 120,
    CheckInterface = 121,
    SetRegistryParams = 122,
    GetRegistryParams = 123,
    GetDeviceInfo = 124,
    GetDriverInfo = 125,

    SetEepromValue = 126,
    GetEepromValue = 127,

    SetLnbData = 128,
    GetLnbData = 129,

    GetNumberTunerRfInputs = 130,
    SetTunerRfInput = 131,
    GetTunerRfInput = 132,

    HidRemoteControlEnable = 152,
    SetHidRemoteConfig = 153,
    GetHidRemoteConfig = 154,

    CiGetState = 200,
    CiGetApplicationInfo = 201,
    CiInitialiseMmi = 202,
    CiGetMmi = 203,
    CiAnswer = 204,
    CiCloseMmi = 205,
    CiSendPmt = 206,
    CiParserPmt = 207,
    CiEventCreate = 208,
    CiEventClose = 209,

    CiGetPmtReply = 210,

    CiSendRawCommand = 211,
    CiGetRawCommandData = 212,

    EnableVirtualDvbt = 300,
    ResetT2sMapping = 301,
    SetT2sMapping = 302,
    GetT2sMapping = 303,
    EnableMceDvbt = 304,
    GetFrequencyChangeStatus = 305,
    SetT2sMappingS2 = 306,
    GetT2sMappingS2 = 307,

    // 7045
    RegisterBdaInterface = 400,
    SimulatorTsStart = 401,
    SimulatorTsStop = 402,
    SimulatorSetProperty = 403,
    SimulatorGetProperty = 404,

    // 704c
    DownloadTunerFirmware = 410,
    DownloadTunerFirmwareStatus = 411,
    GetTunerFirmwareType = 412
  }
}