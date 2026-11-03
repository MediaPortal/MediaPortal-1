// A folder for the files of one test, removed with everything in it afterwards.
#pragma once

#include <windows.h>
#include <cstdint>
#include <string>
#include <vector>

class TempDir
{
public:
  TempDir()
  {
    wchar_t base[MAX_PATH] = {};
    GetTempPathW(MAX_PATH, base);
    static int counter = 0;
    m_path = std::wstring(base) + L"mp_test_" + std::to_wstring(GetCurrentProcessId()) + L"_" + std::to_wstring(GetTickCount()) + L"_" + std::to_wstring(++counter);
    CreateDirectoryW(m_path.c_str(), nullptr);
  }

  ~TempDir() { Remove(m_path); }

  TempDir(const TempDir&) = delete;
  TempDir& operator=(const TempDir&) = delete;

  const std::wstring& Path() const { return m_path; }
  std::wstring File(const std::wstring& name) const { return m_path + L"\\" + name; }

  bool Write(const std::wstring& name, const std::vector<uint8_t>& bytes) const
  {
    HANDLE h = CreateFileW(File(name).c_str(), GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (h == INVALID_HANDLE_VALUE)
      return false;
    DWORD written = 0;
    const BOOL ok = bytes.empty() ? TRUE : WriteFile(h, bytes.data(), static_cast<DWORD>(bytes.size()), &written, nullptr);
    CloseHandle(h);
    return ok && written == bytes.size();
  }

  std::vector<uint8_t> Read(const std::wstring& name) const
  {
    std::vector<uint8_t> bytes;
    HANDLE h = CreateFileW(File(name).c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (h == INVALID_HANDLE_VALUE)
      return bytes;
    LARGE_INTEGER size = {};
    GetFileSizeEx(h, &size);
    bytes.resize(static_cast<size_t>(size.QuadPart));
    DWORD read = 0;
    if (!bytes.empty())
      ReadFile(h, bytes.data(), static_cast<DWORD>(bytes.size()), &read, nullptr);
    bytes.resize(read);
    CloseHandle(h);
    return bytes;
  }

  bool Exists(const std::wstring& name) const { return GetFileAttributesW(File(name).c_str()) != INVALID_FILE_ATTRIBUTES; }

private:
  static void Remove(const std::wstring& folder)
  {
    WIN32_FIND_DATAW data;
    HANDLE find = FindFirstFileW((folder + L"\\*").c_str(), &data);
    if (find != INVALID_HANDLE_VALUE)
    {
      do
      {
        const std::wstring name = data.cFileName;
        if (name == L"." || name == L"..")
          continue;
        const std::wstring full = folder + L"\\" + name;
        if (data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
          Remove(full);
        else
        {
          SetFileAttributesW(full.c_str(), FILE_ATTRIBUTE_NORMAL);
          DeleteFileW(full.c_str());
        }
      } while (FindNextFileW(find, &data));
      FindClose(find);
    }
    RemoveDirectoryW(folder.c_str());
  }

  std::wstring m_path;
};

// Bytes 0, 1, 2, ... of a pattern that depends on 'seed'
inline std::vector<uint8_t> Pattern(size_t n, uint8_t seed)
{
  std::vector<uint8_t> v(n);
  for (size_t i = 0; i < n; i++)
    v[i] = static_cast<uint8_t>(seed + i * 7 + (i >> 8));
  return v;
}
