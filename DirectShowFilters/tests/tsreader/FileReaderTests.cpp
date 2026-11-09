// FileReader and MultiFileReader of TsReader: reading a recording and reading the timeshift buffer, a set of files that
// TsWriter's MultiFileWriter fills and that is described by a small ".ts.tsbuffer" file.
#include <gtest/gtest.h>
#include <winsock2.h>
#include <windows.h>
#include <vector>
#include "FileReader.h"
#include "MultiFileReader.h"
#include "../unit/TempDir.h"

namespace
{
  typedef std::vector<uint8_t> Bytes;

  // The ".ts.tsbuffer" file: position in the newest file, counters, the list of files, an empty name, the counters again
  Bytes InfoFile(int64_t currentPosition, long filesAdded, long filesRemoved, const std::vector<std::wstring>& names)
  {
    Bytes b;
    auto put = [&b](const void* data, size_t n) { const uint8_t* p = static_cast<const uint8_t*>(data); b.insert(b.end(), p, p + n); };
    put(&currentPosition, sizeof(currentPosition));
    put(&filesAdded, sizeof(filesAdded));
    put(&filesRemoved, sizeof(filesRemoved));
    for (const std::wstring& n : names)
      put(n.c_str(), (n.size() + 1) * sizeof(wchar_t));
    const wchar_t end = 0;
    put(&end, sizeof(end));
    put(&filesAdded, sizeof(filesAdded));
    put(&filesRemoved, sizeof(filesRemoved));
    return b;
  }

  const wchar_t* kInfoName = L"live.ts.tsbuffer";

  // Two files of 1000 and 600 bytes, of which 500 are used in the newest
  struct Buffer
  {
    TempDir dir;
    Bytes first = Pattern(1000, 1), second = Pattern(600, 100);

    Buffer()
    {
      dir.Write(L"live1.ts", first);
      dir.Write(L"live2.ts", second);
      WriteInfo(500, 2, 0, { dir.File(L"live1.ts"), dir.File(L"live2.ts") });
    }

    void WriteInfo(int64_t position, long added, long removed, const std::vector<std::wstring>& names)
    {
      dir.Write(kInfoName, InfoFile(position, added, removed, names));
    }

    // The bytes that the reader should see: the first file, then the used part of the second
    Bytes Expected() const
    {
      Bytes all = first;
      all.insert(all.end(), second.begin(), second.begin() + 500);
      return all;
    }
  };
}

// ---------------------------------------------------------------------------------------------------------------------------
// FileReader
// ---------------------------------------------------------------------------------------------------------------------------

TEST(FileReader, ReadsAFileFromTheStart)
{
  TempDir dir;
  const Bytes data = Pattern(5000, 3);
  ASSERT_TRUE(dir.Write(L"a.ts", data));
  FileReader r;
  ASSERT_EQ(r.SetFileName(dir.File(L"a.ts").c_str()), S_OK);
  EXPECT_TRUE(r.IsFileInvalid());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_FALSE(r.IsFileInvalid());
  EXPECT_EQ(r.GetFileSize(), 5000);
  EXPECT_EQ(r.GetFilePointer(), 0);

  Bytes out(5000);
  ULONG got = 0;
  EXPECT_EQ(r.Read(out.data(), 2000, &got), S_OK);
  EXPECT_EQ(got, 2000u);
  EXPECT_EQ(r.GetFilePointer(), 2000);
  EXPECT_EQ(r.Read(out.data() + 2000, 3000, &got), S_OK);
  EXPECT_EQ(out, data);
}

TEST(FileReader, ReadingPastTheEndGivesWhatIsThere)
{
  TempDir dir;
  dir.Write(L"a.ts", Pattern(100, 0));
  FileReader r;
  r.SetFileName(dir.File(L"a.ts").c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  Bytes out(500);
  ULONG got = 0;
  EXPECT_EQ(r.Read(out.data(), 500, &got), S_FALSE);
  EXPECT_EQ(got, 100u);
  EXPECT_EQ(r.Read(out.data(), 500, &got), S_FALSE);
  EXPECT_EQ(got, 0u);
}

TEST(FileReader, SeekFromBeginCurrentAndEnd)
{
  TempDir dir;
  const Bytes data = Pattern(1000, 9);
  dir.Write(L"a.ts", data);
  FileReader r;
  r.SetFileName(dir.File(L"a.ts").c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  Bytes b(10);
  ULONG got = 0;

  r.SetFilePointer(100, FILE_BEGIN);
  r.Read(b.data(), 10, &got);
  EXPECT_EQ(b, Bytes(data.begin() + 100, data.begin() + 110));
  r.SetFilePointer(50, FILE_CURRENT);
  r.Read(b.data(), 10, &got);
  EXPECT_EQ(b, Bytes(data.begin() + 160, data.begin() + 170));
  r.SetFilePointer(-10, FILE_END);
  r.Read(b.data(), 10, &got);
  EXPECT_EQ(b, Bytes(data.end() - 10, data.end()));
}

TEST(FileReader, FileSizeBeyond4GigabytesIsReportedInFull)
{
  // a sparse file with a size above 4 GB: the 64 bit paths of GetFileSize() and SetFilePointer()
  TempDir dir;
  HANDLE h = CreateFileW(dir.File(L"big.ts").c_str(), GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
  ASSERT_NE(h, INVALID_HANDLE_VALUE);
  DWORD ignored = 0;
  if (!DeviceIoControl(h, FSCTL_SET_SPARSE, nullptr, 0, nullptr, 0, &ignored, nullptr))
  {
    CloseHandle(h);
    GTEST_SKIP() << "the temporary folder is not on a file system with sparse files";
  }
  LARGE_INTEGER pos;
  pos.QuadPart = 5LL * 1024 * 1024 * 1024;
  SetFilePointerEx(h, pos, nullptr, FILE_BEGIN);
  const char tail[4] = { 1, 2, 3, 4 };
  WriteFile(h, tail, 4, &ignored, nullptr);
  CloseHandle(h);

  FileReader r;
  r.SetFileName(dir.File(L"big.ts").c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.GetFileSize(), pos.QuadPart + 4);
  r.SetFilePointer(pos.QuadPart, FILE_BEGIN);
  EXPECT_EQ(r.GetFilePointer(), pos.QuadPart);
  BYTE out[4] = {};
  ULONG got = 0;
  EXPECT_EQ(r.Read(out, 4, &got), S_OK);
  EXPECT_EQ(out[3], 4);
}

TEST(FileReader, AClosedFileCannotBeRead)
{
  TempDir dir;
  dir.Write(L"a.ts", Pattern(100, 0));
  FileReader r;
  r.SetFileName(dir.File(L"a.ts").c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.CloseFile(), S_OK);
  EXPECT_TRUE(r.IsFileInvalid());
  BYTE out[10];
  ULONG got = 99;
  EXPECT_EQ(r.Read(out, 10, &got), E_FAIL);
  EXPECT_EQ(got, 0u);
  EXPECT_EQ(r.GetFileSize(), -1);
  EXPECT_EQ(r.OpenFile(), S_OK) << "and it can be opened again";
  EXPECT_EQ(r.Read(out, 10, &got), S_OK);
}

TEST(FileReader, OpeningTwiceIsHarmless)
{
  TempDir dir;
  dir.Write(L"a.ts", Pattern(100, 0));
  FileReader r;
  r.SetFileName(dir.File(L"a.ts").c_str());
  EXPECT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.OpenFile(), S_OK);
}

TEST(FileReader, AMissingFileCannotBeOpened)
{
  TempDir dir;
  FileReader r;
  r.SetFileName(dir.File(L"nothing.ts").c_str());
  EXPECT_EQ(r.OpenFile(), E_FAIL);                    // after about a second of retries
  EXPECT_TRUE(r.IsFileInvalid());
}

TEST(FileReader, FileNameIsKept)
{
  FileReader r;
  r.SetFileName(L"C:\\x\\y.ts");
  LPOLESTR name = nullptr;
  EXPECT_EQ(r.GetFileName(&name), S_OK);
  ASSERT_NE(name, nullptr);
  EXPECT_STREQ(name, L"C:\\x\\y.ts");
  r.SetFileName(L"D:\\z.ts");
  r.GetFileName(&name);
  EXPECT_STREQ(name, L"D:\\z.ts");
}

TEST(FileReader, ErrorsAreFailureCodes)
{
  FileReader r;
  const std::wstring tooLong(300, L'a');
  EXPECT_TRUE(FAILED(r.SetFileName(tooLong.c_str())));
  EXPECT_TRUE(FAILED(r.OpenFile())) << "no file name set";
}

TEST(FileReader, TooLongNamesAreNotStored)
{
  FileReader r;
  r.SetFileName(L"C:\\short.ts");
  const std::wstring tooLong(300, L'a');
  r.SetFileName(tooLong.c_str());
  LPOLESTR name = nullptr;
  r.GetFileName(&name);
  EXPECT_STREQ(name, L"C:\\short.ts");
}

// ---------------------------------------------------------------------------------------------------------------------------
// MultiFileReader
// ---------------------------------------------------------------------------------------------------------------------------

TEST(MultiFileReader, SizeIsTheLengthOfAllFilesWithTheNewestCutAtTheWritePosition)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.GetFileSize(), 1500);
  EXPECT_EQ(r.GetFilePointer(), 0);
}

TEST(MultiFileReader, ReadsAcrossTheBoundaryOfTwoFiles)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  Bytes out(1500);
  ULONG got = 0;
  EXPECT_EQ(r.Read(out.data(), 1200, &got), S_OK);
  EXPECT_EQ(got, 1200u);
  EXPECT_EQ(r.GetFilePointer(), 1200);
  const Bytes expected = b.Expected();
  EXPECT_EQ(Bytes(out.begin(), out.begin() + 1200), Bytes(expected.begin(), expected.begin() + 1200));
}

TEST(MultiFileReader, ReadingToTheEndOfTheBufferGivesTheUsedPartOnly)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  Bytes out(3000);
  ULONG got = 0;
  r.Read(out.data(), 3000, &got);
  EXPECT_EQ(got, 1500u) << "the 100 bytes behind the write position of the second file are not part of the buffer";
  EXPECT_EQ(Bytes(out.begin(), out.begin() + 1500), b.Expected());
  ULONG again = 99;
  EXPECT_EQ(r.Read(out.data(), 10, &again), S_FALSE);
  EXPECT_EQ(again, 0u);
}

TEST(MultiFileReader, SeekingIsLimitedToTheBuffer)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  r.SetFilePointer(1200, FILE_BEGIN);
  EXPECT_EQ(r.GetFilePointer(), 1200);
  r.SetFilePointer(-200, FILE_CURRENT);
  EXPECT_EQ(r.GetFilePointer(), 1000);
  r.SetFilePointer(-100, FILE_END);
  EXPECT_EQ(r.GetFilePointer(), 1400);
  r.SetFilePointer(5000, FILE_BEGIN);
  EXPECT_EQ(r.GetFilePointer(), 1500) << "not beyond the end";
  r.SetFilePointer(-5000, FILE_CURRENT);
  EXPECT_EQ(r.GetFilePointer(), 0) << "not before the start";
}

TEST(MultiFileReader, ReadWithSeekFromTheEnd)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  Bytes out(100);
  ULONG got = 0;
  EXPECT_EQ(r.Read(out.data(), 100, &got, 200, FILE_END), S_OK);        // the block ends 200 bytes before the end
  const Bytes expected = b.Expected();
  EXPECT_EQ(out, Bytes(expected.begin() + 1200, expected.begin() + 1300));
}

TEST(MultiFileReader, NamesInTheInfoFileMayPointToAnotherFolder)
{
  Buffer b;
  b.WriteInfo(500, 2, 0, { L"C:\\somewhere\\else\\live1.ts", L"D:\\old\\folder\\live2.ts" });         // only the file names count
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  Bytes out(1500);
  ULONG got = 0;
  r.Read(out.data(), 1500, &got);
  EXPECT_EQ(got, 1500u);
  EXPECT_EQ(out, b.Expected());
}

TEST(MultiFileReader, NewFilesAndGrowthAreSeenAfterARefresh)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.GetFileSize(), 1500);

  // the newest file grows
  b.WriteInfo(600, 2, 0, { b.dir.File(L"live1.ts"), b.dir.File(L"live2.ts") });
  EXPECT_EQ(r.GetFileSize(), 1600);

  // a third file is added
  const Bytes third = Pattern(300, 50);
  b.dir.Write(L"live3.ts", third);
  b.WriteInfo(200, 3, 0, { b.dir.File(L"live1.ts"), b.dir.File(L"live2.ts"), b.dir.File(L"live3.ts") });
  EXPECT_EQ(r.GetFileSize(), 1000 + 600 + 200);
  r.SetFilePointer(1590, FILE_BEGIN);
  Bytes out(30);
  ULONG got = 0;
  r.Read(out.data(), 30, &got);
  EXPECT_EQ(got, 30u);
  EXPECT_EQ(Bytes(out.begin(), out.begin() + 10), Bytes(b.second.end() - 10, b.second.end()));
  EXPECT_EQ(Bytes(out.begin() + 10, out.end()), Bytes(third.begin(), third.begin() + 20));
}

TEST(MultiFileReader, RemovedFilesMoveTheStart)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  r.SetFilePointer(1100, FILE_BEGIN);
  EXPECT_EQ(r.GetFileSize(), 1500);

  // the oldest file is gone: the buffer only has the second file now
  b.WriteInfo(500, 2, 1, { b.dir.File(L"live2.ts") });
  EXPECT_EQ(r.GetFileSize(), 500);
  Bytes out(100);
  ULONG got = 0;
  r.SetFilePointer(0, FILE_BEGIN);
  EXPECT_EQ(r.Read(out.data(), 100, &got), S_OK);
  EXPECT_EQ(out, Bytes(b.second.begin(), b.second.begin() + 100));
}

TEST(MultiFileReader, ADataFileThatIsMissingFailsTheRead)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  DeleteFileW(b.dir.File(L"live1.ts").c_str());
  Bytes out(100);
  ULONG got = 99;
  EXPECT_EQ(r.Read(out.data(), 100, &got), E_FAIL);
  EXPECT_EQ(got, 0u);
}

TEST(MultiFileReader, AClosedReaderCannotRead)
{
  Buffer b;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  r.CloseFile();
  EXPECT_TRUE(r.IsFileInvalid());
  Bytes out(10);
  ULONG got = 99;
  EXPECT_EQ(r.Read(out.data(), 10, &got), E_FAIL);
  EXPECT_EQ(got, 0u);
}

TEST(MultiFileReader, AnInfoFileThatDoesNotAddUpGivesAnEmptyBuffer)
{
  // the counters say two files, the list has one. OpenFile() does not report it (it only waits while the buffer is "not there
  // yet"), but nothing of the buffer is taken over: no size, and reads fail
  Buffer b;
  b.WriteInfo(500, 2, 0, { b.dir.File(L"live1.ts") });
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  r.OpenFile();
  EXPECT_EQ(r.GetFileSize(), 0);
  Bytes out(10);
  ULONG got = 99;
  EXPECT_EQ(r.Read(out.data(), 10, &got), E_FAIL);
  EXPECT_EQ(got, 0u);
}

TEST(MultiFileReader, AnInfoFileThatIsTooShortGivesAnEmptyBuffer)
{
  Buffer b;
  b.dir.Write(kInfoName, Bytes(10, 0));
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  r.OpenFile();
  EXPECT_EQ(r.GetFileSize(), 0);
  Bytes out(10);
  ULONG got = 99;
  EXPECT_EQ(r.Read(out.data(), 10, &got), E_FAIL);
}

TEST(MultiFileReader, AnInfoFileWithOddLengthIsNotTrusted)
{
  Buffer b;
  Bytes info = InfoFile(500, 2, 0, { b.dir.File(L"live1.ts"), b.dir.File(L"live2.ts") });
  info.push_back(0);
  b.dir.Write(kInfoName, info);
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(b.dir.File(kInfoName).c_str());
  r.OpenFile();
  EXPECT_EQ(r.GetFileSize(), 0);
}

TEST(MultiFileReader, AMissingInfoFileMakesTheOpenFail)
{
  TempDir dir;
  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(dir.File(kInfoName).c_str());
  EXPECT_NE(r.OpenFile(), S_OK);
  EXPECT_TRUE(r.IsFileInvalid());
}
