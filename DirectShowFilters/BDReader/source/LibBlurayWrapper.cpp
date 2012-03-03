/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include "LibBlurayWrapper.h"
#include <afx.h>
#include <afxwin.h>
#include <bluray.h>
#include <keys.h>
#include "OSDTexture.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...);

CLibBlurayWrapper::CLibBlurayWrapper() :
  m_hDLL(NULL),
  m_bLibInitialized(false),
  m_pBd(NULL),
  m_playbackMode(TitleBased),
  m_pTitleInfo(NULL),
  m_pDiscInfo(NULL),
  m_currentTitle(-1),
  m_numTitleIdx(-1),
  m_currentTitleIdx(-1),
  m_numTitles(0),
  m_currentClip(0),
  m_pgEnable(0),
  m_pgStream(0),
  m_bForceTitleBasedPlayback(false),
  m_bStopping(false),
  m_bStopReading(false),
  m_bStillModeOn(false),
  m_nStillEndTime(0),
  _bd_get_titles(NULL),
  _bd_get_title_info(NULL),
  _bd_get_playlist_info(NULL),
  _bd_free_title_info(NULL),
  _bd_open(NULL),
  _bd_close(NULL),
  _bd_seek(NULL),
  _bd_seek_time(NULL),
  _bd_read(NULL),
  _bd_read_skip_still(NULL),
  _bd_seek_chapter(NULL),
  _bd_chapter_pos(NULL),
  _bd_get_current_chapter(NULL),
  _bd_seek_mark(NULL),
  _bd_select_playlist(NULL),
  _bd_select_title(NULL),
  _bd_select_angle(NULL),
  _bd_seamless_angle_change(NULL),
  _bd_get_title_size(NULL),
  _bd_get_current_title(NULL),
  _bd_get_current_angle(NULL),
  _bd_tell(NULL),
  _bd_tell_time(NULL),
  _bd_get_disc_info(NULL),
  _bd_set_player_setting(NULL),
  _bd_set_player_setting_str(NULL),
  _bd_start_bdj(NULL), 
  _bd_stop_bdj(NULL),
  _bd_get_event(NULL),
  _bd_play(NULL),
  _bd_read_ext(NULL),
  _bd_play_title(NULL),
  _bd_menu_call(NULL),
  _bd_register_overlay_proc(NULL),
  _bd_user_input(NULL),
  _bd_mouse_select(NULL),
  _bd_get_meta(NULL),
  _bd_get_clip_infos(NULL)
{
  m_pOverlayRenderer = new COverlayRenderer(this);
  ZeroMemory((void*)&m_playerSettings, sizeof(bd_player_settings));
}

CLibBlurayWrapper::~CLibBlurayWrapper()
{
  CAutoLock cRendertLock(&m_csRenderLock);
  CAutoLock cLibLock(&m_csLibLock);

  if (m_pBd)
  {
    _bd_register_overlay_proc(m_pBd, NULL, NULL);
    _bd_close(m_pBd);
  }

  delete m_pOverlayRenderer;

  if (m_pTitleInfo)
  {
    _bd_free_title_info(m_pTitleInfo);
  }

  FreeLibrary(m_hDLL);
}

void CLibBlurayWrapper::StaticOverlayProc(void *this_gen, const BD_OVERLAY * const ov)
{
  CAutoLock cRendertLock(&(((CLibBlurayWrapper *)this_gen)->m_csRenderLock));
  ((CLibBlurayWrapper *)this_gen)->m_pOverlayRenderer->OverlayProc(ov);
}

bool CLibBlurayWrapper::Initialize()
{
  TCHAR szDirectory[MAX_PATH] = "";
  TCHAR szPath[MAX_PATH] = "";
  GetModuleFileName(NULL, szPath, sizeof(szPath) - 1);

  strncpy(szDirectory, szPath, strrchr(szPath, '\\') - szPath);
  szDirectory[strlen(szDirectory)] = '\0';

  wsprintf(szDirectory,"%s\\bluray.dll", szDirectory);	
  LogDebug("CLibBlurayWrapper - Load bluray: %s", szDirectory);
  m_hDLL = LoadLibrary(szDirectory);

  if (!m_hDLL)
  {
    LogDebug("Failed to load the DLL from application exe path, trying c:\\");
    m_hDLL = LoadLibrary("c:\\bluray.dll");
  }

  if (!m_hDLL)
  {
    LogDebug("CLibBlurayWrapper - Failed to load c:\\bluray.dll");
    m_bLibInitialized = false;
    return false;
  }

  _bd_get_titles = (API_bd_get_titles)GetProcAddress(m_hDLL, "bd_get_titles");
  _bd_get_title_info = (API_bd_get_title_info)GetProcAddress(m_hDLL, "bd_get_title_info");
  _bd_get_playlist_info = (API_bd_get_playlist_info)GetProcAddress(m_hDLL, "bd_get_playlist_info");
  _bd_free_title_info = (API_bd_free_title_info)GetProcAddress(m_hDLL, "bd_free_title_info");
  _bd_open = (API_bd_open)GetProcAddress(m_hDLL, "bd_open");
  _bd_close = (API_bd_close)GetProcAddress(m_hDLL, "bd_close");
  _bd_seek = (API_bd_seek)GetProcAddress(m_hDLL, "bd_seek");
  _bd_seek_time = (API_bd_seek_time)GetProcAddress(m_hDLL, "bd_seek_time");
  _bd_read = (API_bd_read)GetProcAddress(m_hDLL, "bd_read");
  _bd_read_skip_still = (API_bd_read_skip_still)GetProcAddress(m_hDLL, "bd_read_skip_still");
  _bd_seek_chapter = (API_bd_seek_chapter)GetProcAddress(m_hDLL, "bd_seek_chapter");
  _bd_chapter_pos = (API_bd_chapter_pos)GetProcAddress(m_hDLL, "bd_chapter_pos");
  _bd_get_current_chapter = (API_bd_get_current_chapter)GetProcAddress(m_hDLL, "bd_get_current_chapter");
  _bd_seek_mark = (API_bd_seek_mark)GetProcAddress(m_hDLL, "bd_seek_mark");
  _bd_select_playlist = (API_bd_select_playlist)GetProcAddress(m_hDLL, "bd_select_playlist");
  _bd_select_title = (API_bd_select_title)GetProcAddress(m_hDLL, "bd_select_title");
  _bd_select_angle = (API_bd_select_angle)GetProcAddress(m_hDLL, "bd_select_angle");
  _bd_seamless_angle_change = (API_bd_seamless_angle_change)GetProcAddress(m_hDLL, "bd_seamless_angle_change");
  _bd_get_title_size = (API_bd_get_title_size)GetProcAddress(m_hDLL, "bd_get_title_size");
  _bd_get_current_title = (API_bd_get_current_title)GetProcAddress(m_hDLL, "bd_get_current_title");
  _bd_get_current_angle = (API_bd_get_current_angle)GetProcAddress(m_hDLL, "bd_get_current_angle");
  _bd_tell = (API_bd_tell)GetProcAddress(m_hDLL, "bd_tell");
  _bd_tell_time = (API_bd_tell_time)GetProcAddress(m_hDLL, "bd_tell_time");
  _bd_get_disc_info = (API_bd_get_disc_info)GetProcAddress(m_hDLL, "bd_get_disc_info");
  _bd_set_player_setting = (API_bd_set_player_setting)GetProcAddress(m_hDLL, "bd_set_player_setting");
  _bd_set_player_setting_str = (API_bd_set_player_setting_str)GetProcAddress(m_hDLL, "bd_set_player_setting_str");
  _bd_start_bdj = (API_bd_start_bdj)GetProcAddress(m_hDLL, "bd_start_bdj"); 
  _bd_stop_bdj = (API_bd_stop_bdj)GetProcAddress(m_hDLL, "bd_stop_bdj");
  _bd_get_event = (API_bd_get_event)GetProcAddress(m_hDLL, "bd_get_event");
  _bd_play = (API_bd_play)GetProcAddress(m_hDLL, "bd_play");
  _bd_read_ext = (API_bd_read_ext)GetProcAddress(m_hDLL, "bd_read_ext");
  _bd_play_title = (API_bd_play_title)GetProcAddress(m_hDLL, "bd_play_title");
  _bd_menu_call = (API_bd_menu_call)GetProcAddress(m_hDLL, "bd_menu_call");
  _bd_register_overlay_proc = (API_bd_register_overlay_proc)GetProcAddress(m_hDLL, "bd_register_overlay_proc");
  _bd_user_input = (API_bd_user_input)GetProcAddress(m_hDLL, "bd_user_input");
  _bd_mouse_select = (API_bd_mouse_select)GetProcAddress(m_hDLL, "bd_mouse_select");
  _bd_get_meta = (API_bd_get_meta)GetProcAddress(m_hDLL, "bd_get_meta");

  // This method is not available in the vanilla libbluray 
  _bd_get_clip_infos = (API_bd_get_clip_infos)GetProcAddress(m_hDLL, "bd_get_clip_infos");

  if (!_bd_get_titles ||
      !_bd_get_title_info ||
      !_bd_get_playlist_info ||
      !_bd_free_title_info ||
      !_bd_open ||
      !_bd_close ||
      !_bd_seek ||
      !_bd_seek_time ||
      !_bd_read ||
      !_bd_read_skip_still ||
      !_bd_seek_chapter ||
      !_bd_chapter_pos ||
      !_bd_get_current_chapter ||
      !_bd_seek_mark ||
      !_bd_select_playlist ||
      !_bd_select_title ||
      !_bd_select_angle ||
      !_bd_seamless_angle_change ||
      !_bd_get_title_size ||
      !_bd_get_current_title ||
      !_bd_get_current_angle ||
      !_bd_tell ||
      !_bd_tell_time ||
      !_bd_get_disc_info ||
      !_bd_set_player_setting ||
      !_bd_set_player_setting_str ||
      !_bd_start_bdj ||
      !_bd_stop_bdj ||
      !_bd_get_event ||
      !_bd_play ||
      !_bd_read_ext ||
      !_bd_play_title ||
      !_bd_menu_call ||
      !_bd_register_overlay_proc ||
      !_bd_user_input ||
      !_bd_mouse_select ||
      !_bd_get_meta ||
      !_bd_get_clip_infos)
  {
    LogDebug("CLibBlurayWrapper - failed to load method from lib - a version mismatch?");
    m_bLibInitialized = false;
    return false;
  }

  LogDebug("CLibBlurayWrapper - initialization succeeded");
  m_bLibInitialized = true;
  return true;
}

bool CLibBlurayWrapper::OpenBluray(const char* pRootPath)
{
  LogDebug("CLibBlurayWrapper - OpenBluray: %s", pRootPath);

  if (!m_bLibInitialized)
  {
    LogDebug("CLibBlurayWrapper - OpenBluray - DLL not initialized!");
    return false;
  }

  CAutoLock cLibLock(&m_csLibLock);

  if (m_pBd)
  {
    LogDebug("CLibBlurayWrapper - closing previously opened bluray");
    _bd_close(m_pBd);
  }

  m_pBd = _bd_open(pRootPath, NULL); // no decryption keys are provided
  
  if (!m_pBd)
  {
    LogDebug("CLibBlurayWrapper - failed to open!"); 
    _bd_close(m_pBd);
    m_pBd = NULL;
    return false;
  }

  m_pDiscInfo = _bd_get_disc_info(m_pBd);

  LogDiskInfo(m_pDiscInfo);

  if (!m_pDiscInfo->bluray_detected) 
  {
    LogDebug("CLibBlurayWrapper - No disk detected!");
    _bd_close(m_pBd);
    m_pBd = NULL;
    return false;
  }

  if ((m_pDiscInfo->aacs_detected && !m_pDiscInfo->aacs_handled) ||
     ( m_pDiscInfo->bdplus_detected && !m_pDiscInfo->bdplus_handled))
  {
    LogDebug("CLibBlurayWrapper - Disk is encrypted, no decryption is currently supported");
    _bd_close(m_pBd);
    m_pBd = NULL;
    return false;
  }

  if (!m_pDiscInfo->first_play_supported)
  {
    LogDebug("CLibBlurayWrapper - First play is not supported - cannot play in navigation mode!");
    m_playbackMode = TitleBased;
  }
  else
  {
    LogDebug("CLibBlurayWrapper - Using HDMV playback mode");
    m_playbackMode = Navigation;
  }

  _bd_set_player_setting(m_pBd, BLURAY_PLAYER_SETTING_REGION_CODE, m_playerSettings.regionCode);
  _bd_set_player_setting(m_pBd, BLURAY_PLAYER_SETTING_PARENTAL, m_playerSettings.parentalControl);
  _bd_set_player_setting_str(m_pBd, BLURAY_PLAYER_SETTING_AUDIO_LANG, m_playerSettings.audioLang);
  _bd_set_player_setting_str(m_pBd, BLURAY_PLAYER_SETTING_PG_LANG, m_playerSettings.subtitleLang);
  _bd_set_player_setting_str(m_pBd, BLURAY_PLAYER_SETTING_MENU_LANG, m_playerSettings.menuLang);
  _bd_set_player_setting_str(m_pBd, BLURAY_PLAYER_SETTING_COUNTRY_CODE, m_playerSettings.countryCode);

  // Init event queue
  _bd_get_event(m_pBd, NULL);

  m_numTitles = GetTitles(TITLES_ALL);

  _bd_register_overlay_proc(m_pBd, this, StaticOverlayProc);

  return true;
}

bool CLibBlurayWrapper::CloseBluray()
{
  LogDebug("CLibBlurayWrapper - CloseBluray");

  CAutoLock cLibLock(&m_csLibLock);

  if (!m_pBd)
  {
    LogDebug("CLibBlurayWrapper - No disk has been opened!");
    return false;
  }

  _bd_close(m_pBd);
  m_pBd = NULL;
  
  return true;
}

void CLibBlurayWrapper::SetBDPlayerSettings(bd_player_settings pSettings)
{
  LogDebug("CLibBlurayWrapper - Settings: Audio(%s), Menu(%s), Sub(%s), Ctry(%s), Reg(%i), Prtl(%i)", pSettings.audioLang,
		pSettings.menuLang, pSettings.subtitleLang, pSettings.countryCode, pSettings.regionCode, pSettings.parentalControl);
  memcpy(&m_playerSettings, &pSettings, sizeof(bd_player_settings));
}

bd_player_settings& CLibBlurayWrapper::GetBDPlayerSettings()
{
  return m_playerSettings;
}

UINT32 CLibBlurayWrapper::GetTitles(UINT8 pFlags)
{
  CAutoLock cLibLock(&m_csLibLock);
  return _bd_get_titles(m_pBd, pFlags, 0); // TODO - provide angle!
}

bool CLibBlurayWrapper::SetAngle(UINT8 pAngle)
{
  CAutoLock cLibLock(&m_csLibLock);
  if (m_pBd && m_pTitleInfo && pAngle < m_pTitleInfo->angle_count)
  {
    if (_bd_select_angle(m_pBd, pAngle) == 1)
      return true;
  }
  return false;
}

bool CLibBlurayWrapper::SetChapter(UINT32 pChapter)
{
  CAutoLock cLibLock(&m_csLibLock);
  if (m_pBd && m_pTitleInfo && pChapter < m_pTitleInfo->chapter_count)
  {
    INT64 pos = _bd_seek_chapter(m_pBd, pChapter);
    if (pos >= 0)
      return true;
  }
  return false;
}

void CLibBlurayWrapper::SetTitle(UINT32 pTitle)
{
  if (pTitle >= 0)
    m_currentTitle = pTitle;
}

bool CLibBlurayWrapper::GetAngle(UINT8* pAngle)
{
  CAutoLock cLibLock(&m_csLibLock);
  if (m_pBd)
  {
    *pAngle = _bd_get_current_angle(m_pBd);
    return true;
  }
  return false;
}

bool CLibBlurayWrapper::GetChapter(UINT32* pChapter)
{
  CAutoLock cObjectLock(&m_csLibLock);
  if (m_pBd)
  {
    *pChapter = _bd_get_current_chapter(m_pBd);
    return true;
  }
  return false;
}

BLURAY_TITLE_INFO* CLibBlurayWrapper::GetTitleInfo(UINT32 pIndex)
{
  CAutoLock cLibLock(&m_csLibLock);

  BLURAY_TITLE_INFO* info = NULL;
  
  if (pIndex == BLURAY_TITLE_CURRENT)
  {
    UINT32 index = _bd_get_current_title(m_pBd);
    info = _bd_get_title_info(m_pBd, index, 0); // TODO - provide angle!
  }
  else
  {
    info = _bd_get_title_info(m_pBd, pIndex, 0); // TODO - provide angle!
  }
  
  return info;
}

void CLibBlurayWrapper::FreeTitleInfo(BLURAY_TITLE_INFO* info)
{
   _bd_free_title_info(info);
}

void CLibBlurayWrapper::MouseMove(UINT64 pPos, UINT16 pX, UINT16 pY)
{
  CAutoLock cLibLock(&m_csLibLock);
  if (m_pBd)
  {		
    _bd_mouse_select(m_pBd, pPos, pX, pY); // TODO check return value
  }    
}

bool CLibBlurayWrapper::Play()
{
  CAutoLock cLibLock(&m_csLibLock);

  bool ret = false;

  // This can be used to force the titlebased playback mode - currently for testing only
  if (m_bForceTitleBasedPlayback)
  {
    LogDebug("CLibBlurayWrapper - m_bForceTitleBasedPlayback %d", m_bForceTitleBasedPlayback);
    m_playbackMode = TitleBased;
  }

  LogDebug("CLibBlurayWrapper - Play");
  if (m_pBd)
  {
    if (m_playbackMode == Navigation)
    {
      ret = _bd_play(m_pBd) ? true : false;
      LogDebug("CLibBlurayWrapper - _bd_play %d", ret);

      if (ret)
      {
        m_currentTitleIdx = _bd_get_current_title(m_pBd);
        UpdateTitleInfo();
      }
    }
    else
    {
      ret = _bd_select_title(m_pBd, m_currentTitle) == 1 ? true : false;
      LogDebug("CLibBlurayWrapper - _bd_select_title %d", ret);

      if (ret)
        UpdateTitleInfo();
    }
  }

  if (!ret)
  {
    LogDebug("CLibBlurayWrapper - Play failed!");
  }

  return ret;
}

int CLibBlurayWrapper::Read(unsigned char* pData, int pSize, bool& pPause, bool pIgnorePauseEvents)
{
  CAutoLock cLibLock(&m_csLibLock);

  int readBytes = 0;
  m_bStopReading = false;

  if (m_playbackMode == Navigation)
  {
    BD_EVENT ev = {0};
    ev.event = BD_EVENT_ERROR;

    while (readBytes == 0 && ev.event != BD_EVENT_NONE && !m_bStopping && !m_bStopReading)
    {
      // TODO add error handling
      readBytes = _bd_read_ext(m_pBd, pData, pSize, &ev); 
      HandleBDEvent(ev, pIgnorePauseEvents);
    }
    pPause = m_bStopReading;
  }
  else
  {
    // No still frames in title mode
    pPause = false;
    
    readBytes = _bd_read(m_pBd, pData, pSize);
    HandleBDEventQueue(pIgnorePauseEvents);
  }

  return readBytes;
}

bool CLibBlurayWrapper::SkipStillTime()
{
  CAutoLock cLibLock(&m_csLibLock);
  
  bool ret = false;
  if (m_pBd)
    ret = _bd_read_skip_still(m_pBd) ? true : false;

  return ret;
}

void CLibBlurayWrapper::Seek(UINT64 pPos)
{
  CAutoLock cLibLock(&m_csLibLock);
  
  if (m_pBd)
    (void)_bd_seek_time(m_pBd, pPos);
}

void CLibBlurayWrapper::SetState(FILTER_STATE newState)
{
  m_state = newState;

  if ( m_state == State_Paused && newState == State_Stopped)
    m_bStopping = true;
  else
    m_bStopping = false;
}

void CLibBlurayWrapper::SetEventObserver(BDEventObserver* pObserver)
{
  bool found = false;

  ivecObservers it = m_eventObservers.begin();
  while (it != m_eventObservers.end())
  {
    if (pObserver == *it)
    {
      found = true;
      break;
    }
    ++it;
  }

  if (!found)
    m_eventObservers.push_back(pObserver);
}

void CLibBlurayWrapper::RemoveEventObserver(BDEventObserver* pObserver)
{
  ivecObservers it = m_eventObservers.begin();
  while (it != m_eventObservers.end())
  {
    if (pObserver == *it)
    {
      m_eventObservers.erase(it);
      break;
    }
    ++it;  
  }
}

void CLibBlurayWrapper::HandleBDEventQueue(bool pIgnorePauseEvents)
{
  BD_EVENT ev;
  while (_bd_get_event(m_pBd, &ev)) 
  {
    HandleBDEvent(ev, pIgnorePauseEvents);
    if (ev.event == BD_EVENT_NONE || ev.event == BD_EVENT_ERROR)
      break;
  }
}

void CLibBlurayWrapper::HandleBDEvent(BD_EVENT& ev, bool pIgnorePauseEvents)
{
  LogEvent(ev, true);
  switch (ev.event)
  {
    case BD_EVENT_SEEK:
      m_bStillModeOn = false;
      m_nStillEndTime = 0;
      break;

    case BD_EVENT_STILL_TIME:
      if (!pIgnorePauseEvents)
        StillMode(ev.param);
      else
      {
        m_nStillEndTime = 0;
        _bd_read_skip_still(m_pBd);
      }

      m_bStopReading = true;
      break;

    case BD_EVENT_STILL:
      if (!pIgnorePauseEvents)
        m_bStillModeOn = ev.param ? true : false;
      break;

    case BD_EVENT_ANGLE:
      // TODO
      break;

    case BD_EVENT_END_OF_TITLE:
      break;

    case BD_EVENT_TITLE:
      break;

    case BD_EVENT_CHAPTER:
      break;

    case BD_EVENT_PLAYLIST:
      m_currentTitleIdx = _bd_get_current_title(m_pBd);
      m_currentClip = 0;
      UpdateTitleInfo();
      break;

    case BD_EVENT_PLAYITEM:
      if (m_pTitleInfo && ev.param < m_pTitleInfo->clip_count)
        m_currentClip = ev.param;
      else
        m_currentClip = 0;

      break;
  }

  UINT64 pos = _bd_tell_time(m_pBd);

  ivecObservers it = m_eventObservers.begin();
  while (it != m_eventObservers.end())
  {
    (*it)->HandleBDEvent(ev, pos);
    ++it;
  }
}

void CLibBlurayWrapper::UpdateTitleInfo()
{
  CAutoLock cLibLock(&m_csLibLock);

  LogDebug("UpdateTitleInfo:");

  if (m_pTitleInfo)
    _bd_free_title_info(m_pTitleInfo);

  if (m_playbackMode == TitleBased)
  {
    ASSERT(m_currentTitle < m_numTitles);
    m_pTitleInfo = _bd_get_title_info(m_pBd, m_currentTitle, 0); // TODO - provide angle!
    LogTitleInfo(m_currentTitle);
  }
  else
  {
    m_currentTitleIdx = _bd_get_current_title(m_pBd);
    ASSERT(m_currentTitleIdx < m_numTitles);

    m_pTitleInfo = _bd_get_title_info(m_pBd, m_currentTitleIdx, 0); // TODO - provide angle!
    LogTitleInfo(m_currentTitleIdx);
  }

  if (!m_pTitleInfo) 
    LogDebug("UpdateTitleInfo (%d) failed", m_currentTitleIdx);
}

bool CLibBlurayWrapper::CurrentPosition(UINT64& pPosition, UINT64& pTotal)
{
  CAutoLock cLibLock(&m_csLibLock);
  
  if (!m_pBd)
    return false;

  pPosition = _bd_tell_time(m_pBd);

  // TODO - remove when libbluray is fixed to provide correct events on 
  // HDVM suspend / resume. Currently we need to poll the title index...
  UINT32 index = _bd_get_current_title(m_pBd);

  if (!m_pTitleInfo || m_currentTitleIdx != index)
  {
    UpdateTitleInfo();
    m_currentTitleIdx = index;
  }

  if (m_pTitleInfo)
  {
    pTotal = m_pTitleInfo->duration;
    return true;
  }
  else
  {
    pTotal = 0;
    pPosition = 0;
    return false;
  }
}

BLURAY_CLIP_INFO* CLibBlurayWrapper::CurrentClipInfo()
{
  CAutoLock cLibLock(&m_csLibLock);
  
  if (m_pTitleInfo && m_pTitleInfo->clips)
    return &m_pTitleInfo->clips[m_currentClip];
  else
    return NULL;
}

bool CLibBlurayWrapper::GetClipInfo(int pClip, UINT64* pClipStartTime, UINT64* pStreamStartTime, UINT64* pBytePos, UINT64* pDuration)
{
  CAutoLock cLibLock(&m_csLibLock);
  return _bd_get_clip_infos(m_pBd, pClip, pClipStartTime, pStreamStartTime, pBytePos, pDuration) == 1 ? true : false;
}

bool CLibBlurayWrapper::ProvideUserInput(INT64 pPts, UINT32 pKey)
{
  CAutoLock cLibLock(&m_csLibLock);
  
  if (m_pBd)
  {
    // libbluray doesn't open the main menu with BD_VK_ROOT_MENU key 
    if (pKey == BD_VK_ROOT_MENU)
      return OpenMenu(pPts);
    else
      return _bd_user_input(m_pBd, pPts, pKey) >= 0 ? true : false;
  }
  return false;
}

bool CLibBlurayWrapper::OpenMenu(INT64 pPts)
{
  CAutoLock cLibLock(&m_csLibLock); 
  
  if (m_pBd)
    return _bd_menu_call(m_pBd, pPts) == 1 ? true : false;
  else
    return false;
}

void CLibBlurayWrapper::StillMode(unsigned pSeconds)
{
  if (m_nStillEndTime > 0) 
  {
    if (GetTickCount() / 1000 >= m_nStillEndTime) 
    {
      LogDebug("Still image ends");
      m_nStillEndTime = 0;
      _bd_read_skip_still(m_pBd);
      return;
    }
  }
  else if (pSeconds > 0) 
  {
    if (pSeconds > 300) 
      pSeconds = 300;

    LogDebug("Still image, pause for %d seconds", pSeconds);
    m_nStillEndTime = GetTickCount() / 1000 + pSeconds;
  }
}

void CLibBlurayWrapper::ForceTitleBasedPlayback(bool pForce)
{
  m_bForceTitleBasedPlayback = pForce;
}

bool CLibBlurayWrapper::ForceTitleBasedPlayback()
{
  return m_bForceTitleBasedPlayback;
}

void CLibBlurayWrapper::SetD3DDevice(IDirect3DDevice9* device)
{
  m_pOverlayRenderer->SetD3DDevice(device);
}

void CLibBlurayWrapper::LogAction(int pKey)
{
  switch (pKey)
  {
  case BD_VK_NONE:
    LogDebug("    BD_VK_NONE");
    break;
  case BD_VK_0:
  case BD_VK_1:
  case BD_VK_2:
  case BD_VK_3:
  case BD_VK_4:
  case BD_VK_5:
  case BD_VK_6:
  case BD_VK_7:
  case BD_VK_8:
  case BD_VK_9:
    LogDebug("    BD_VK_%c", pKey - 2); 
    break;
  case BD_VK_ROOT_MENU:
    LogDebug("    BD_VK_ROOT_MENU");
    break;
  case BD_VK_POPUP:
    LogDebug("    BD_VK_POPUP");
    break;
  case BD_VK_UP:
    LogDebug("    BD_VK_UP");
    break;
  case BD_VK_DOWN:
    LogDebug("    BD_VK_DOWN");
    break;
  case BD_VK_LEFT:
    LogDebug("    BD_VK_LEFT");
    break;
  case BD_VK_RIGHT:
    LogDebug("    BD_VK_RIGHT");
    break;
  case BD_VK_ENTER:
    LogDebug("    BD_VK_ENTER");
    break;
  case BD_VK_MOUSE_ACTIVATE:
    LogDebug("    BD_VK_MOUSE_ACTIVATE");
    break;
  default:
    LogDebug("    UNKNOWN key!");
  }
}

void CLibBlurayWrapper::LogEvent(const BD_EVENT& pEvent, bool pIgnoreNoneEvent)
{
  switch (pEvent.event)
  {
  case BD_EVENT_NONE:
    if(!pIgnoreNoneEvent)
      LogDebug("    BD_EVENT_NONE - %d", pEvent.param);
    break;
  case BD_EVENT_ERROR:
    LogDebug("    BD_EVENT_ERROR - %d", pEvent.param);
    break;
  case BD_EVENT_READ_ERROR:
    LogDebug("    BD_EVENT_READ_ERROR - %d", pEvent.param);
    break;
  case BD_EVENT_ENCRYPTED:
    LogDebug("    BD_EVENT_ENCRYPTED - %d", pEvent.param);
    break;
  case BD_EVENT_ANGLE:
    LogDebug("    BD_EVENT_ANGLE - %d", pEvent.param);
    break;
  case BD_EVENT_TITLE:
    LogDebug("    BD_EVENT_TITLE - %d", pEvent.param);
    break;
  case BD_EVENT_PLAYLIST:
    LogDebug("    BD_EVENT_PLAYLIST - %d", pEvent.param);
    break;
  case BD_EVENT_PLAYITEM:
    LogDebug("    BD_EVENT_PLAYITEM - %d", pEvent.param);
    break;
  case BD_EVENT_CHAPTER:
    LogDebug("    BD_EVENT_CHAPTER - %d", pEvent.param);
    break;
  case BD_EVENT_END_OF_TITLE:
    LogDebug("    BD_EVENT_NONE - %d", pEvent.param);
    break;
  case BD_EVENT_AUDIO_STREAM:
    LogDebug("    BD_EVENT_AUDIO_STREAM - %d", pEvent.param);
    break;
  case BD_EVENT_IG_STREAM:
    LogDebug("    BD_EVENT_IG_STREAM - %d", pEvent.param);
    break;
  case BD_EVENT_PG_TEXTST_STREAM:
    LogDebug("    BD_EVENT_PG_TEXTST_STREAM - %d", pEvent.param);
    break;
  case BD_EVENT_PIP_PG_TEXTST_STREAM:
    LogDebug("    BD_EVENT_PIP_PG_TEXTST_STREAM - %d", pEvent.param);
    break;
  case BD_EVENT_SECONDARY_AUDIO_STREAM:
    LogDebug("    BD_EVENT_SECONDARY_AUDIO_STREAM - %d", pEvent.param);
    break;
  case BD_EVENT_SECONDARY_VIDEO_STREAM:
    LogDebug("    BD_EVENT_SECONDARY_VIDEO_STREAM - %d", pEvent.param);
    break;
  case BD_EVENT_PG_TEXTST:
    LogDebug("    BD_EVENT_PG_TEXTST - %d", pEvent.param);
    break;
  case BD_EVENT_PIP_PG_TEXTST:
    LogDebug("    BD_EVENT_PIP_PG_TEXTST - %d", pEvent.param);
    break;
  case BD_EVENT_SECONDARY_AUDIO:
    LogDebug("    BD_EVENT_SECONDARY_AUDIO - %d", pEvent.param);
    break;
  case BD_EVENT_SECONDARY_VIDEO:
    LogDebug("    BD_EVENT_SECONDARY_VIDEO - %d", pEvent.param);
    break;
  case BD_EVENT_SECONDARY_VIDEO_SIZE:
    LogDebug("    BD_EVENT_SECONDARY_VIDEO_SIZE - %d", pEvent.param);
    break;
  case BD_EVENT_SEEK:
    LogDebug("    BD_EVENT_SEEK - %d", pEvent.param);
    break;
  case BD_EVENT_STILL:
    if (m_bStillModeOn && pEvent.param == 0)
      LogDebug("    BD_EVENT_STILL - off");
    if (!m_bStillModeOn && pEvent.param == 1)
      LogDebug("    BD_EVENT_STILL - on");
    break;
  case BD_EVENT_STILL_TIME:
    //LogDebug("    BD_EVENT_STILL_TIME - %d", pEvent.param);
    break;
  case BD_EVENT_SOUND_EFFECT:
    //LogDebug("    BD_EVENT_SOUND_EFFECT - %d", pEvent.param);
    break;
  case BD_EVENT_POPUP:
    LogDebug("    BD_EVENT_POPUP - %d", pEvent.param);
    break;
  case BD_EVENT_MENU:
    LogDebug("    BD_EVENT_MENU - %d", pEvent.param);
    break;
  default:
    LogDebug("    ERROR - no event!");
  }
}

void CLibBlurayWrapper::LogDiskInfo(const BLURAY_DISC_INFO* pInfo)
{
  if (m_pDiscInfo)
  {
    LogDebug("Disk Info:");
    LogDebug("--------------------------");
    LogDebug("aacs_detected:          %d", pInfo->aacs_detected);
    LogDebug("aacs_handled:           %d", pInfo->aacs_handled);
    LogDebug("bdplus_detected:        %d", pInfo->bdplus_detected);
    LogDebug("bdplus_handled:         %d", pInfo->bdplus_handled);
    LogDebug("bluray_detected:        %d", pInfo->bluray_detected);
    LogDebug("first_play_supported:   %d", pInfo->first_play_supported);
    LogDebug("libaacs_detected:       %d", pInfo->libaacs_detected);
    LogDebug("libbdplus_detected:     %d", pInfo->libbdplus_detected);
    LogDebug("num_bdj_titles:         %d", pInfo->num_bdj_titles);
    LogDebug("num_hdmv_titles:        %d", pInfo->num_hdmv_titles);
    LogDebug("num_unsupported_titles: %d", pInfo->num_unsupported_titles);
    LogDebug("top_menu_supported:     %d", pInfo->top_menu_supported);
    LogDebug("--------------------------");
  }
  else
    LogDebug("m_pDiscInfo == NULL");
}

void CLibBlurayWrapper::LogTitleInfo(int pIndex, bool ignoreShort)
{
  BLURAY_TITLE_INFO* ti = _bd_get_title_info(m_pBd, pIndex, 0); // TODO - provide min_title_length!
  if (ti)
  {
    if (ignoreShort && (ti->duration / 90000 < 0))
    {
      _bd_free_title_info(ti);
      return;
    }

    LogDebug(
      "index: %d duration: %I64d:%I64d:%I64d chapters: %d angles: %u clips %u",
      pIndex,
      (ti->duration / 90000) / (3600),
      ((ti->duration / 90000) % 3600) / 60,
      ((ti->duration / 90000) % 60),
      ti->chapter_count, ti->angle_count, ti->clip_count);
  }

  _bd_free_title_info(ti);
}

void CLibBlurayWrapper::HandleOSDUpdate(OSDTexture& texture)
{
  ivecObservers it = m_eventObservers.begin();
  while (it != m_eventObservers.end())
  {
    (*it)->HandleOSDUpdate(texture);
    ++it;
  }
}
