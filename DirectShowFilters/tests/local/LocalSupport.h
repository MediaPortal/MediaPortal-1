// Helpers of the local-only tests (mp_local_tests): they use media samples and a graphics device of the
// developer machine, so they are not part of the normal test run and never run in CI.
#pragma once

#include <gtest/gtest.h>
#include <windows.h>
#include <string>

// Folder with the sample files: the environment variable MP_SAMPLES_DIR, else the folder given to CMake
// (-DMP_SAMPLES_DIR=..., default <repository>/../MP-MediaInfo.Samples). Empty when there is none.
std::wstring SamplesDir();

// Full path of a sample ("HD Audio\\file.ts"), or an empty string when it does not exist.
std::wstring SamplePath(const wchar_t* relative);

// The tests skip (and say why) instead of failing when the machine does not have what they need.
#define REQUIRE_SAMPLE(var, relative)                                                           \
  const std::wstring var = SamplePath(relative);                                                \
  if (var.empty())                                                                              \
    GTEST_SKIP() << "sample not found: " << relative << " (set MP_SAMPLES_DIR to the samples folder)"

std::string ToUtf8(const std::wstring& s);

// The Debug build of the filters (DirectShow base classes) shows a system modal "ASSERT Failed" message box when an ASSERT
// fails, which would stop an unattended test run. LocalSupport.cpp starts a watcher thread in every local test program that
// closes such a box with "Yes" (continue) and reports the assertion text as a failure of the running test.
int AssertsCaught();

// An assertion that is a known defect of the filters (documented in tests/README.md): it is closed like any other but is not
// reported as a failure. 'fragment' is a part of the assertion text. Returns how often it has been seen so far.
void AllowKnownAssert(const char* fragment);
int KnownAssertsSeen();

// Folder for files made by a test; removed with everything in it when the object goes away.
class TempFolder
{
public:
  TempFolder();
  ~TempFolder();
  const std::wstring& Path() const { return m_path; }
  std::wstring File(const wchar_t* name) const { return m_path + L"\\" + name; }
  bool Write(const wchar_t* name, const std::string& bytes) const;
private:
  std::wstring m_path;
};
