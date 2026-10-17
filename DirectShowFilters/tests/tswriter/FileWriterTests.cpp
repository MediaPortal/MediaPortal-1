// FileWriter, CDiskBuff and MultiFileWriter of TsWriter: the recording file and the timeshift buffer. The buffer is a small
// ".ts.tsbuffer" file plus a ring of data files; TsReader's MultiFileReader reads it back, so the tests of the buffer put the two
// together.
#include <gtest/gtest.h>
#include <winsock2.h>
#include <streams.h>
#include <algorithm>
#include <chrono>
#include <thread>
#include <vector>
#include "FileWriter.h"
#include "CDiskBuff.h"
#include "MultiFileWriter.h"
#include "../../TsReader/source/MultiFileReader.h"
#include "../unit/TempDir.h"
#include "../unit/GuardedBuffer.h"

// the writer logs wide strings too
void LogDebug(const wchar_t* /*fmt*/, ...) {}

namespace
{
  typedef std::vector<uint8_t> Bytes;

  template <class Condition>
  bool WaitFor(Condition condition, int milliseconds = 8000)
  {
    const auto end = std::chrono::steady_clock::now() + std::chrono::milliseconds(milliseconds);
    while (std::chrono::steady_clock::now() < end)
    {
      if (condition())
        return true;
      Sleep(10);
    }
    return condition();
  }
}

// ---------------------------------------------------------------------------------------------------------------------------
// FileWriter
// ---------------------------------------------------------------------------------------------------------------------------

TEST(FileWriter, WritesAFile)
{
  TempDir dir;
  FileWriter w;
  ASSERT_EQ(w.SetFileName(dir.File(L"rec.ts").c_str()), S_OK);
  EXPECT_TRUE(w.IsFileInvalid());
  ASSERT_EQ(w.OpenFile(), S_OK);
  EXPECT_FALSE(w.IsFileInvalid());
  const Bytes data = Pattern(5000, 2);
  EXPECT_EQ(w.Write(const_cast<PBYTE>(data.data()), 3000), S_OK);
  EXPECT_EQ(w.GetFilePointer(), 3000);
  EXPECT_EQ(w.Write(const_cast<PBYTE>(data.data()) + 3000, 2000), S_OK);
  EXPECT_EQ(w.GetFilePointer(), 5000);
  w.CloseFile();
  EXPECT_TRUE(w.IsFileInvalid());
  EXPECT_EQ(dir.Read(L"rec.ts"), data);
}

TEST(FileWriter, WritingWithoutAnOpenFileDoesNothing)
{
  FileWriter w;
  BYTE b[4] = {};
  EXPECT_EQ(w.Write(b, 4), S_FALSE);
  EXPECT_EQ(w.CloseFile(), S_OK);
}

TEST(FileWriter, AnExistingFileIsOverwrittenFromTheStartWithoutBeingShortened)
{
  TempDir dir;
  dir.Write(L"rec.ts", Bytes(100, 0xAA));
  FileWriter w;
  w.SetFileName(dir.File(L"rec.ts").c_str());
  ASSERT_EQ(w.OpenFile(), S_OK);
  Bytes ten(10, 0x11);
  w.Write(ten.data(), 10);
  w.CloseFile();
  const Bytes now = dir.Read(L"rec.ts");
  ASSERT_EQ(now.size(), 100u);
  EXPECT_EQ(now[0], 0x11);
  EXPECT_EQ(now[9], 0x11);
  EXPECT_EQ(now[10], 0xAA);
}

TEST(FileWriter, OthersCanReadButNotWriteWhileItIsOpen)
{
  TempDir dir;
  FileWriter w;
  w.SetFileName(dir.File(L"rec.ts").c_str());
  ASSERT_EQ(w.OpenFile(), S_OK);
  Bytes data(100, 7);
  w.Write(data.data(), 100);
  EXPECT_EQ(dir.Read(L"rec.ts").size(), 100u);
  HANDLE h = CreateFileW(dir.File(L"rec.ts").c_str(), GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, nullptr);
  EXPECT_EQ(h, INVALID_HANDLE_VALUE) << "another writer is refused";
  if (h != INVALID_HANDLE_VALUE)
    CloseHandle(h);
}

TEST(FileWriter, ChunkReserveAllocatesAheadAndTrimsOnClose)
{
  TempDir dir;
  FileWriter w;
  w.SetChunkReserve(10000, 50000);
  w.SetFileName(dir.File(L"rec.ts").c_str());
  ASSERT_EQ(w.OpenFile(), S_OK);
  Bytes data(100, 1);
  w.Write(data.data(), 100);
  EXPECT_EQ(dir.Read(L"rec.ts").size(), 10000u) << "the file is made 10000 bytes long ahead of the data";
  EXPECT_EQ(w.GetFilePointer(), 100);
  w.CloseFile();
  EXPECT_EQ(dir.Read(L"rec.ts").size(), 100u) << "and cut to the data when it is closed";
}

TEST(FileWriter, ChunkReserveStopsAtTheMaximumFileSize)
{
  TempDir dir;
  FileWriter w;
  w.SetChunkReserve(4000, 6000);
  w.SetFileName(dir.File(L"rec.ts").c_str());
  ASSERT_EQ(w.OpenFile(), S_OK);
  Bytes data(5000, 1);
  w.Write(data.data(), 5000);
  EXPECT_EQ(dir.Read(L"rec.ts").size(), 6000u) << "the reserve is never above the maximum file size";
}

TEST(FileWriter, FileNameIsKept)
{
  FileWriter w;
  w.SetFileName(L"C:\\x\\rec.ts");
  LPWSTR name = nullptr;
  EXPECT_EQ(w.GetFileName(&name), S_OK);
  ASSERT_NE(name, nullptr);
  EXPECT_STREQ(name, L"C:\\x\\rec.ts");
  EXPECT_EQ(w.SetFileName(nullptr), E_POINTER);
}

TEST(FileWriter, ErrorsAreFailureCodes)
{
  FileWriter w;
  const std::wstring tooLong(300, L'a');
  EXPECT_TRUE(FAILED(w.SetFileName(tooLong.c_str())));
  EXPECT_TRUE(FAILED(w.OpenFile())) << "no file name set";
}

TEST(FileWriter, ADirectoryThatIsNotThereCannotBeOpened)
{
  TempDir dir;
  FileWriter w;
  w.SetFileName(dir.File(L"no\\such\\folder\\rec.ts").c_str());
  EXPECT_TRUE(FAILED(w.OpenFile()));
  EXPECT_TRUE(w.IsFileInvalid());
}

// ---------------------------------------------------------------------------------------------------------------------------
// CDiskBuff
// ---------------------------------------------------------------------------------------------------------------------------

TEST(DiskBuff, CollectsDataUpToItsSize)
{
  CDiskBuff b(100);
  Bytes data = Pattern(60, 5);
  EXPECT_EQ(b.Add(data.data(), 60), 0);
  EXPECT_EQ(b.Length(), 60);
  EXPECT_EQ(b.Add(data.data(), 40), 0);
  EXPECT_EQ(b.Length(), 100);
  EXPECT_EQ(b.Add(data.data(), 1), 1) << "full: nothing taken";
  EXPECT_EQ(b.Length(), 100);
  EXPECT_EQ(0, memcmp(b.Data(), data.data(), 60));
  EXPECT_EQ(0, memcmp(b.Data() + 60, data.data(), 40));
}

TEST(DiskBuff, ADataLengthAboveTheSizeIsRefusedWhole)
{
  CDiskBuff b(100);
  Bytes data(200, 1);
  EXPECT_EQ(b.Add(data.data(), 101), 101);
  EXPECT_EQ(b.Length(), 0);
  EXPECT_EQ(b.Add(nullptr, 10), 10);
}

TEST(DiskBuff, NegativeLengthIsRefused)
{
  CDiskBuff b(100);
  Bytes data(10, 1);
  EXPECT_NE(b.Add(data.data(), -1), 0);
}

// ---------------------------------------------------------------------------------------------------------------------------
// MultiFileWriter (and MultiFileReader on what it writes)
// ---------------------------------------------------------------------------------------------------------------------------

namespace
{
  const wchar_t* kBuffer = L"live.ts.tsbuffer";

  MultiFileWriterParam Params(long minFiles, long maxFiles, __int64 maxSize)
  {
    MultiFileWriterParam p;
    p.minFiles = minFiles;
    p.maxFiles = maxFiles;
    p.maxSize = maxSize;
    p.chunkSize = 0;
    return p;
  }

  // Feeds 'data' to the writer in pieces of 1880 bytes (ten packets) with buffers of that size, so that every piece reaches the
  // disk soon. The last buffer is only passed on when the next one is started: a last piece of the size of a buffer pushes it.
  void FeedAndFlush(MultiFileWriter& w, const Bytes& data)
  {
    for (size_t pos = 0; pos < data.size(); pos += 1880)
    {
      const int n = static_cast<int>((std::min<size_t>)(1880, data.size() - pos));
      w.AddToBuffer(const_cast<byte*>(&data[pos]), n, 1880);
    }
    const Bytes extra(1880, 0xEE);
    w.AddToBuffer(const_cast<byte*>(extra.data()), 1880, 1880);
    w.DiscardBuffer();
  }
}

TEST(MultiFileWriter, WritesTheDataAndTheBufferFile)
{
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  const Bytes data = Pattern(20000, 4);
  FeedAndFlush(w, data);
  __int64 position = 0;
  long fileId = 0;
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return position == 20000; }));
  EXPECT_EQ(fileId, 1);
  EXPECT_TRUE(dir.Exists(kBuffer));
  EXPECT_EQ(dir.Read(std::wstring(kBuffer) + L"1.ts"), data);
  w.Close();
  EXPECT_FALSE(dir.Exists(kBuffer));
}

TEST(MultiFileWriter, TheReaderSeesWhatWasWritten)
{
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  const Bytes data = Pattern(20000, 4);
  FeedAndFlush(w, data);
  __int64 position = 0;
  long fileId = 0;
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return position == 20000; }));

  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(dir.File(kBuffer).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.GetFileSize(), 20000);
  Bytes out(20000);
  ULONG got = 0;
  r.Read(out.data(), 20000, &got);
  ASSERT_EQ(got, 20000u);
  EXPECT_EQ(out, data);
  r.CloseFile();
  w.Close();
}

TEST(MultiFileWriter, TheReaderFollowsTheRingOfFiles)
{
  // files of 10000 bytes, three of them: the first 15000 bytes fill the first file and half of the second. The reader attaches
  // then. 20000 more bytes fill the second and the third file and go on into the first one again (the oldest file is reused):
  // the buffer then holds the bytes 10000 to 34999.
  TempDir dir;
  MultiFileWriterParam params = Params(3, 3, 10000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  const Bytes data = Pattern(35000, 9);
  FeedAndFlush(w, Bytes(data.begin(), data.begin() + 15000));
  __int64 position = 0;
  long fileId = 0;
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return fileId == 2 && position == 5000; }));

  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(dir.File(kBuffer).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.GetFileSize(), 15000);

  FeedAndFlush(w, Bytes(data.begin() + 15000, data.end()));
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return fileId == 1 && position == 5000; }));
  EXPECT_EQ(r.GetFileSize(), 25000);
  Bytes out(25000);
  ULONG got = 0;
  r.SetFilePointer(0, FILE_BEGIN);
  r.Read(out.data(), 25000, &got);
  ASSERT_EQ(got, 25000u);
  EXPECT_EQ(out, Bytes(data.begin() + 10000, data.end()));
  r.CloseFile();
  w.Close();
}

TEST(MultiFileWriter, AReaderThatOpensAWrappedBufferSeesAllFiles)
{
  TempDir dir;
  MultiFileWriterParam params = Params(3, 3, 10000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  const Bytes data = Pattern(35000, 9);
  FeedAndFlush(w, data);
  __int64 position = 0;
  long fileId = 0;
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return fileId == 1 && position == 5000; }));

  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(dir.File(kBuffer).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  EXPECT_EQ(r.GetFileSize(), 25000);
  EXPECT_EQ(r.GetFileSize(), 25000) << "and again";
  r.CloseFile();
  w.Close();
}

TEST(MultiFileWriter, ARunningReaderKeepsTheOldestFileFromBeingReused)
{
  // while the reader has the oldest file open it cannot be deleted and reused: the writer takes a new file instead (up to the
  // maximum number of files)
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 10000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  const Bytes first = Pattern(15000, 3);
  FeedAndFlush(w, first);
  __int64 position = 0;
  long fileId = 0;
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return fileId == 2 && position == 5000; }));

  MultiFileReader r(FALSE, FALSE, nullptr, FALSE, FALSE);
  r.SetFileName(dir.File(kBuffer).c_str());
  ASSERT_EQ(r.OpenFile(), S_OK);
  Bytes out(100);
  ULONG got = 0;
  r.SetFilePointer(0, FILE_BEGIN);
  ASSERT_EQ(r.Read(out.data(), 100, &got), S_OK);            // the reader now holds the first file open

  const Bytes more = Pattern(10000, 77);
  FeedAndFlush(w, more);
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return fileId == 3 && position == 5000; }));
  r.SetFilePointer(0, FILE_BEGIN);
  ASSERT_EQ(r.Read(out.data(), 100, &got), S_OK);
  EXPECT_EQ(out, Bytes(first.begin(), first.begin() + 100)) << "the first file is still there";
  r.CloseFile();
  w.Close();
}

TEST(MultiFileWriter, CloseRemovesTheFilesWhenNobodyHoldsThem)
{
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 5000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  FeedAndFlush(w, Pattern(8000, 1));
  __int64 position = 0;
  long fileId = 0;
  ASSERT_TRUE(WaitFor([&]() { w.GetPosition(&position, &fileId); return fileId == 2 && position == 3000; }));
  EXPECT_TRUE(dir.Exists(kBuffer));
  EXPECT_TRUE(dir.Exists(std::wstring(kBuffer) + L"1.ts"));
  EXPECT_TRUE(dir.Exists(std::wstring(kBuffer) + L"2.ts"));
  w.Close();
  EXPECT_FALSE(dir.Exists(kBuffer));
  EXPECT_FALSE(dir.Exists(std::wstring(kBuffer) + L"1.ts"));
  EXPECT_FALSE(dir.Exists(std::wstring(kBuffer) + L"2.ts"));
}

TEST(MultiFileWriter, DataFilesAreNamedAfterTheBufferFile)
{
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  const Bytes data = Pattern(3760, 1);
  w.AddToBuffer(const_cast<byte*>(data.data()), 1880, 1880);
  w.AddToBuffer(const_cast<byte*>(data.data()) + 1880, 1880, 1880);
  ASSERT_TRUE(WaitFor([&]() { return dir.Exists(std::wstring(kBuffer) + L"1.ts"); }));
  ASSERT_TRUE(WaitFor([&]() { return dir.Read(std::wstring(kBuffer) + L"1.ts").size() >= 1880; }));
  const Bytes first = dir.Read(std::wstring(kBuffer) + L"1.ts");
  EXPECT_EQ(Bytes(first.begin(), first.begin() + 1880), Bytes(data.begin(), data.begin() + 1880));
  w.Close();
}

TEST(MultiFileWriter, ABufferFileCannotBeOpenedTwice)
{
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  EXPECT_EQ(w.Open(dir.File(L"other.ts.tsbuffer").c_str()), E_FAIL);
  w.Close();
}

TEST(MultiFileWriter, NullNameIsRefused)
{
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  EXPECT_EQ(w.Open(nullptr), E_POINTER);
}

TEST(MultiFileWriter, PositionBeforeTheFirstWriteIsUnknown)
{
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  __int64 position = 5;
  long fileId = 5;
  w.GetPosition(&position, &fileId);
  EXPECT_EQ(position, -1);
  EXPECT_EQ(fileId, 0);
}

TEST(MultiFileWriter, DataBeforeOpenIsDroppedWhenItIsPushed)
{
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  const Bytes data = Pattern(1000, 1);
  EXPECT_EQ(w.AddToBuffer(const_cast<byte*>(data.data()), 1000, 4096), S_OK);
  EXPECT_EQ(w.DiscardBuffer(), S_OK);
}

TEST(MultiFileWriter, AFullBufferRollsOverIntoTheNextOne)
{
  TempDir dir;
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  ASSERT_EQ(w.Open(dir.File(kBuffer).c_str()), S_OK);
  const Bytes data = Pattern(1000, 1);
  EXPECT_EQ(w.AddToBuffer(const_cast<byte*>(data.data()), 1000, 1500), S_OK);
  EXPECT_EQ(w.AddToBuffer(const_cast<byte*>(data.data()), 1000, 1500), S_FALSE) << "S_FALSE: a new buffer was started";
  w.Close();
}

TEST(MultiFileWriter, TooLongNameIsAFailure)
{
  MultiFileWriterParam params = Params(2, 4, 1000000);
  MultiFileWriter w(&params);
  const std::wstring tooLong(300, L'a');
  EXPECT_TRUE(FAILED(w.Open(tooLong.c_str())));
}
