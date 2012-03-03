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

#pragma once

#include "StdAfx.h"
#include <streams.h>
#include <vector>
#include <bluray.h>
#include <overlay.h>
#include "OverlayRenderer.h"
#include "BDEventObserver.h"

using namespace std;

enum PLAYBACK_MODE
{
  Navigation = 0,
  TitleBased
};

typedef struct bd_player_settings
{
  int regionCode;
  int parentalControl;
  int audioType;
  char audioLang[4];
  char menuLang[4];
  char subtitleLang[4];
  char countryCode[4];
} bd_player_settings;

// Function pointer definitions for the libbluray API

typedef uint32_t (__cdecl *API_bd_get_titles)(BLURAY *, uint8_t, uint32_t);
typedef BLURAY_TITLE_INFO* (__cdecl *API_bd_get_title_info)(BLURAY *, uint32_t, unsigned);
typedef BLURAY_TITLE_INFO* (__cdecl *API_bd_get_playlist_info)(BLURAY *, uint32_t, unsigned);
typedef void (__cdecl *API_bd_free_title_info)(BLURAY_TITLE_INFO *);
typedef BLURAY* (__cdecl *API_bd_open)(const char*, const char*);
typedef void (__cdecl *API_bd_close)(BLURAY *);
typedef int64_t (__cdecl *API_bd_seek)(BLURAY *, uint64_t);
typedef int64_t (__cdecl *API_bd_seek_time)(BLURAY *, uint64_t);
typedef int (__cdecl *API_bd_read)(BLURAY *, unsigned char *, int);
typedef int (__cdecl *API_bd_read_skip_still)(BLURAY *);
typedef int64_t (__cdecl *API_bd_seek_chapter)(BLURAY *, unsigned);
typedef int64_t (__cdecl *API_bd_chapter_pos)(BLURAY *, unsigned);
typedef uint32_t (__cdecl *API_bd_get_current_chapter)(BLURAY *);
typedef int64_t (__cdecl *API_bd_seek_mark)(BLURAY *, unsigned);
typedef int (__cdecl *API_bd_select_playlist)(BLURAY *, uint32_t);
typedef int (__cdecl *API_bd_select_title)(BLURAY *, uint32_t);
typedef int (__cdecl *API_bd_select_angle)(BLURAY *, unsigned);
typedef void (__cdecl *API_bd_seamless_angle_change)(BLURAY *, unsigned);
typedef uint64_t (__cdecl *API_bd_get_title_size)(BLURAY *);
typedef uint32_t (__cdecl *API_bd_get_current_title)(BLURAY *);
typedef unsigned (__cdecl *API_bd_get_current_angle)(BLURAY *);
typedef uint64_t (__cdecl *API_bd_tell)(BLURAY *);
typedef uint64_t (__cdecl *API_bd_tell_time)(BLURAY *);
typedef const BLURAY_DISC_INFO* (__cdecl *API_bd_get_disc_info)(BLURAY *);
typedef int (__cdecl *API_bd_set_player_setting)(BLURAY *, uint32_t, uint32_t );
typedef int (__cdecl *API_bd_set_player_setting_str)(BLURAY *, uint32_t, const char *);
typedef int (__cdecl *API_bd_start_bdj)(BLURAY *, const char*); 
typedef void (__cdecl *API_bd_stop_bdj)(BLURAY *);
typedef int (__cdecl *API_bd_get_event)(BLURAY *, BD_EVENT *);
typedef int (__cdecl *API_bd_play)(BLURAY *);
typedef int (__cdecl *API_bd_read_ext)(BLURAY *, unsigned char *, int, BD_EVENT *);
typedef int (__cdecl *API_bd_play_title)(BLURAY *, unsigned);
typedef int (__cdecl *API_bd_menu_call)(BLURAY *, int64_t);
typedef void (*bd_overlay_proc_f)(void *, const struct bd_overlay_s * const);
typedef void (__cdecl* API_bd_register_overlay_proc)(BLURAY *, void *, bd_overlay_proc_f);
typedef int (__cdecl* API_bd_user_input)(BLURAY *, int64_t, uint32_t);
typedef int (__cdecl* API_bd_mouse_select)(BLURAY *, int64_t, uint16_t, uint16_t);
typedef struct meta_dl* (__cdecl* API_bd_get_meta)(BLURAY *);
typedef int (__cdecl* API_bd_get_clip_infos)(BLURAY *, int, uint64_t *, uint64_t *, uint64_t *, uint64_t *);

class CLibBlurayWrapper
{
public:
  CLibBlurayWrapper();
  ~CLibBlurayWrapper();

  bool Initialize();
  bool OpenBluray(const char* pRootPath);
  bool CloseBluray();
  UINT32 GetTitles(UINT8 pFlags);
  BLURAY_TITLE_INFO* GetTitleInfo(UINT32 pIndex);
  void FreeTitleInfo(BLURAY_TITLE_INFO* info);
  bool SetAngle(UINT8 pAngle);
  bool SetChapter(UINT32 pChapter);
  void SetTitle(UINT32 pTitle);
  bool GetAngle(UINT8* pAngle);
  bool GetChapter(UINT32* pChapter);
  bool Play();
  int Read(unsigned char* pData, int pSize, bool& pPause, bool pIgnorePauseEvents);
  bool SkipStillTime();
  void SetState(FILTER_STATE newState);
  void SetEventObserver(BDEventObserver* pObserver);
  void RemoveEventObserver(BDEventObserver* pObserver);
  void Seek(UINT64 pPos);
  
  bool CurrentPosition(UINT64& pPosition, UINT64& pTotal);
  BLURAY_CLIP_INFO* CurrentClipInfo();
  bool GetClipInfo(int pClip, UINT64* pClipStartTime, UINT64* pStreamStartTime, UINT64* pBytePos, UINT64* pDuration);
  bool ProvideUserInput(INT64 pPts, UINT32 pKey);
  bool OpenMenu(INT64 pPts);
  void ForceTitleBasedPlayback(bool pForce);
  bool ForceTitleBasedPlayback();
  void SetD3DDevice(IDirect3DDevice9* device);
  void SetBDPlayerSettings(bd_player_settings pSettings);
  bd_player_settings& GetBDPlayerSettings();

  static void StaticOverlayProc(void *this_gen, const BD_OVERLAY * const ov);

  void LogAction(int pKey);
  void LogEvent(const BD_EVENT& pEvent, bool pIgnoreNoneEvent);
  void LogDiskInfo(const BLURAY_DISC_INFO* pInfo);
  void LogTitleInfo(int pIndex, bool ignoreShort = false);

  void HandleOSDUpdate(OSDTexture& pTexture);
  void MouseMove(UINT64 pPos, UINT16 pX, UINT16 pY);

private:

  void HandleBDEventQueue(bool pIgnorePauseEvents);
  void HandleBDEvent(BD_EVENT& ev, bool pIgnorePauseEvents);
  void UpdateTitleInfo();

  void StillMode(unsigned int pSeconds);

  HMODULE m_hDLL;
  bool m_bLibInitialized;

  BLURAY* m_pBd;
  const BLURAY_DISC_INFO* m_pDiscInfo;
  BLURAY_TITLE_INFO* m_pTitleInfo;

  int m_numTitleIdx;         // number of relevant playlists
  int m_currentTitleIdx;
  int m_numTitles;           // navigation mode, number of titles in disc index
  int m_currentTitle;        // navigation mode, title from disc index
  
  int m_currentClip;
  int m_pgEnable;
  int m_pgStream;

  COverlayRenderer* m_pOverlayRenderer;
  CCritSec m_csRenderLock;
  CCritSec m_csLibLock;

  PLAYBACK_MODE m_playbackMode;
  bool m_bForceTitleBasedPlayback;

  vector<BDEventObserver*> m_eventObservers;
  typedef vector<BDEventObserver*>::iterator ivecObservers;

  bd_player_settings m_playerSettings;

  FILTER_STATE m_state;
  bool m_bStopping;
  bool m_bStopReading;

  bool m_bStillModeOn;

  DWORD m_nStillEndTime;

  // libbluray API function pointers from DLL
  API_bd_get_titles _bd_get_titles;
  API_bd_get_title_info _bd_get_title_info;
  API_bd_get_playlist_info _bd_get_playlist_info;
  API_bd_free_title_info _bd_free_title_info;
  API_bd_open _bd_open;
  API_bd_close _bd_close;
  API_bd_seek _bd_seek;
  API_bd_seek_time _bd_seek_time;
  API_bd_read _bd_read;
  API_bd_read_skip_still _bd_read_skip_still;
  API_bd_seek_chapter _bd_seek_chapter;
  API_bd_chapter_pos _bd_chapter_pos;
  API_bd_get_current_chapter _bd_get_current_chapter;
  API_bd_seek_mark _bd_seek_mark;
  API_bd_select_playlist _bd_select_playlist;
  API_bd_select_title _bd_select_title;
  API_bd_select_angle _bd_select_angle;
  API_bd_seamless_angle_change _bd_seamless_angle_change;
  API_bd_get_title_size _bd_get_title_size;
  API_bd_get_current_title _bd_get_current_title;
  API_bd_get_current_angle _bd_get_current_angle;
  API_bd_tell _bd_tell;
  API_bd_tell_time _bd_tell_time;
  API_bd_get_disc_info _bd_get_disc_info;
  API_bd_set_player_setting _bd_set_player_setting;
  API_bd_set_player_setting_str _bd_set_player_setting_str;
  API_bd_start_bdj _bd_start_bdj; 
  API_bd_stop_bdj _bd_stop_bdj;
  API_bd_get_event _bd_get_event;
  API_bd_play _bd_play;
  API_bd_read_ext _bd_read_ext;
  API_bd_play_title _bd_play_title;
  API_bd_menu_call _bd_menu_call;
  API_bd_register_overlay_proc _bd_register_overlay_proc;
  API_bd_user_input _bd_user_input;
  API_bd_mouse_select _bd_mouse_select;
  API_bd_get_meta _bd_get_meta;
  API_bd_get_clip_infos _bd_get_clip_infos;
};
