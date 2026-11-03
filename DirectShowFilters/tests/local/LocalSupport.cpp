#include "LocalSupport.h"
#include "FilterHost.h"

#include <shellapi.h>
#include <atomic>
#include <crtdbg.h>
#include <mutex>
#include <vector>
#include <thread>

#ifndef MP_SAMPLES_DIR
#define MP_SAMPLES_DIR L""
#endif

std::wstring SamplesDir()
{
  wchar_t env[MAX_PATH * 2] = {};
  DWORD n = GetEnvironmentVariableW(L"MP_SAMPLES_DIR", env, static_cast<DWORD>(sizeof(env) / sizeof(env[0])));
  std::wstring dir = (n > 0 && n < sizeof(env) / sizeof(env[0])) ? std::wstring(env, n) : std::wstring(MP_SAMPLES_DIR);
  while (!dir.empty() && (dir.back() == L'\\' || dir.back() == L'/'))
    dir.pop_back();
  if (dir.empty())
    return dir;
  DWORD attr = GetFileAttributesW(dir.c_str());
  return (attr != INVALID_FILE_ATTRIBUTES && (attr & FILE_ATTRIBUTE_DIRECTORY)) ? dir : std::wstring();
}

std::wstring SamplePath(const wchar_t* relative)
{
  std::wstring dir = SamplesDir();
  if (dir.empty())
    return dir;
  std::wstring path = dir + L"\\" + relative;
  return GetFileAttributesW(path.c_str()) != INVALID_FILE_ATTRIBUTES ? path : std::wstring();
}

namespace
{
  std::atomic<int> g_assertsCaught{ 0 };
  std::atomic<int> g_knownSeen{ 0 };
  std::mutex g_knownLock;
  std::vector<std::string> g_known;

  BOOL CALLBACK LongestText(HWND child, LPARAM param)
  {
    std::wstring* text = reinterpret_cast<std::wstring*>(param);
    wchar_t buffer[1024] = {};
    GetWindowTextW(child, buffer, 1024);
    if (wcslen(buffer) > text->size())
      *text = buffer;
    return TRUE;
  }

  void WatchForAssertBoxes()
  {
    for (;;)
    {
      Sleep(30);
      HWND box = FindWindowW(L"#32770", L"ASSERT Failed");
      if (!box)
        continue;
      std::wstring text;
      EnumChildWindows(box, LongestText, reinterpret_cast<LPARAM>(&text));
      const std::string utf8 = ToUtf8(text);
      bool known = false;
      {
        std::lock_guard<std::mutex> lock(g_knownLock);
        for (const std::string& k : g_known)
          known = known || utf8.find(k) != std::string::npos;
      }
      if (known)
        g_knownSeen++;
      else
      {
        g_assertsCaught++;
        ADD_FAILURE() << "ASSERT in a filter: " << utf8;
      }
      PostMessageW(box, WM_COMMAND, IDYES, 0);               // "Continue"
      Sleep(200);
    }
  }

  struct AssertWatcher
  {
    AssertWatcher()
    {
      // CRT debug heap reports (corrupted blocks, asserts of the debug CRT) go to stderr instead of a dialog; with
      // MP_TEST_HEAPCHECK=1 the whole heap is checked at every allocation, which finds the block that was overrun.
      for (int type : { _CRT_WARN, _CRT_ERROR, _CRT_ASSERT })
      {
        _CrtSetReportMode(type, _CRTDBG_MODE_FILE);
        _CrtSetReportFile(type, _CRTDBG_FILE_STDERR);
      }
      wchar_t heapCheck[8] = {};
      if (GetEnvironmentVariableW(L"MP_TEST_HEAPCHECK", heapCheck, 8) && heapCheck[0] == L'1')
        _CrtSetDbgFlag(_CrtSetDbgFlag(_CRTDBG_REPORT_FLAG) | _CRTDBG_CHECK_ALWAYS_DF);
      FilterModule::KeepLoaded() = true;
      std::thread(WatchForAssertBoxes).detach();
    }
  } g_assertWatcher;
}

int AssertsCaught()
{
  return g_assertsCaught;
}

void AllowKnownAssert(const char* fragment)
{
  std::lock_guard<std::mutex> lock(g_knownLock);
  g_known.push_back(fragment);
}

int KnownAssertsSeen()
{
  return g_knownSeen;
}

std::string ToUtf8(const std::wstring& s)
{
  if (s.empty())
    return std::string();
  int n = WideCharToMultiByte(CP_UTF8, 0, s.c_str(), static_cast<int>(s.size()), NULL, 0, NULL, NULL);
  std::string out(n, '\0');
  WideCharToMultiByte(CP_UTF8, 0, s.c_str(), static_cast<int>(s.size()), &out[0], n, NULL, NULL);
  return out;
}

TempFolder::TempFolder()
{
  wchar_t dir[MAX_PATH] = {};
  GetTempPathW(MAX_PATH, dir);
  static int counter = 0;
  m_path = std::wstring(dir) + L"mp_local_tests_" + std::to_wstring(GetCurrentProcessId()) + L"_" + std::to_wstring(counter++);
  CreateDirectoryW(m_path.c_str(), NULL);
}

TempFolder::~TempFolder()
{
  // SHFileOperation wants a double-null-terminated list
  std::wstring from = m_path;
  from.push_back(L'\0');
  SHFILEOPSTRUCTW op = {};
  op.wFunc = FO_DELETE;
  op.pFrom = from.c_str();
  op.fFlags = FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT;
  SHFileOperationW(&op);
}

bool TempFolder::Write(const wchar_t* name, const std::string& bytes) const
{
  HANDLE f = CreateFileW(File(name).c_str(), GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
  if (f == INVALID_HANDLE_VALUE)
    return false;
  DWORD written = 0;
  BOOL ok = ::WriteFile(f, bytes.data(), static_cast<DWORD>(bytes.size()), &written, NULL);
  CloseHandle(f);
  return ok && written == bytes.size();
}
