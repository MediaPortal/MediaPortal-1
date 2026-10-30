# Tests of the native filters

GoogleTest (from vcpkg, feature `tests`), built with CMake. The MSBuild solution does not contain the tests.

## Running

```bash
cd DirectShowFilters
cmake --preset vs2026-x64-unicode -DMP_BUILD_TESTS=ON -DVCPKG_MANIFEST_FEATURES=tests
cmake --build --preset vs2026-x64-unicode-debug --target mp_unit_tests mp_integration_tests
ctest --test-dir build/vs2026-x64-unicode -C Debug --output-on-failure
```

Presets: `<vs2026|vs2022>-<x64|x86>-<unicode|mbcs>`. The tests are built in the character set of the preset, so the
Unicode and the MBCS build of the filters are both exercised. The executables are in `build/<preset>/bin/<Config>/`
next to the filters (`mp_unit_tests.exe`, `mp_integration_tests.exe`); run them from there or with ctest.

Integration tests: `ctest -L integration`. Unit tests only: `ctest -LE "integration|local|tsmuxer|tsreader|tswriter"`.

Besides the three programs above there is one program per filter whose code is tested directly (built from the sources of the
filter, no DLL and no graph): `mp_tsmuxer_tests`, `mp_tsreader_tests` and `mp_tswriter_tests` (labels `tsmuxer`, `tsreader`,
`tswriter`), see "Tests of TsMuxer, TsReader and TsWriter" below.

Last result (VS 2026, all four presets, Debug): 226 unit, 110 integration, 48 to 49 local, 39 TsMuxer, 109 TsReader and 84
TsWriter tests pass. The same programs pass under AddressSanitizer (Release). The only disabled test (`DISABLED_`) is
`MpAudioRenderer.DISABLED_AFormatBlockOfExactlyTheRightSizeIsNeverReadBeyondItsEnd`, which only an ASan build can judge, see
"Found by the tests".

## Unit tests (`unit/`, `mp_unit_tests`)

Small pure code, no DirectShow needed.

| File | Covers |
|---|---|
| `TsHeaderTests` | `CTsHeader`: sync byte, flags, PID, adaption control, payload start limits |
| `PcrTests` | `CPcr`: PCR decode, clock conversion, `+`/`-`, PTS/DTS from a PES header including the bounds checks |
| `AdaptionFieldTests` | `CAdaptionField`: flags, PCR, OPCR, splice countdown, short fields |
| `PacketSyncTests` | `CPacketSync`: resynchronisation for every input chunk size, garbage, lost bytes, `Reset` |
| `PacketSyncAlgorithmTests` | The synchronisation algorithm of `CPacketSync` in detail: identical result for any chunking (random pieces, payloads full of 0x47), start in the middle of a packet, garbage in front, lost bytes and inserted garbage, repaired sync bytes, noise, the limits of the method, `OnRawData2` (TS and 192 byte M2TS detection) and `OnRawDataCheck` |
| `SectionTests` | `CSection`: header decode, length across packets, completeness, copy |
| `Crc32Tests` | `crc32()` (CRC-32/MPEG-2) against the standard check value |
| `BitstreamReaderTests` | H.265 `BitstreamReader`: bits, Exp-Golomb, emulation prevention, reading to the end (guard page) |
| `ResamplerTests` | `AudioStreamResampler` of MPAudioSwitcher (`Audio.cpp` is compiled into the tests as it is): output count, tone frequency and level, aliasing of point sampling against the filter, clipping, 8 bit, no read past the input and no write past the output (guard page), buffers that follow each other without a jump or a loss |
| `RectUtilsTests` | `mpc-hc_subs/src/dsutil/RectUtils.h`: the helpers on top of `RECT`, `POINT` and `SIZE` (size, centre, intersection, union, offset, inflate like the `CRect` operators did, empty and null) |
| `StrUtilsTests` | `mpc-hc_subs/src/dsutil/StrUtils.h`: the functions that replace the methods of `CString` on `std::basic_string` (`Format` with the width of the string, `StringLeft/Right/Mid` clamping, `StringFind` family with -1, `StringReplace/Remove/Insert/Delete` counts, trim, tokenize, compare, narrow/wide conversions, buffers) |
| `SectionDecoderTests` | `CSectionDecoder` (DvbCoreUtils): sections in one packet and over many, two in one packet, CRC, short syntax, transport errors, reset, the longest section, and damaged data in front of a guard page. `SectionBuilder.h` makes sections (with CRC) and their packets, PMT and SDT bodies |
| `PmtParserTests` | `CBasePmtParser` (shared by TsReader and TsWriter), `CPidTable`, `CChannelInfo`: streams of a PMT, stream types, AC-3/E-AC-3 by descriptor, languages, DVB subtitles, teletext pages, HDMV LPCM, damaged tables |
| `DirectShowHelperTests` | DirectShowHelper: `cMulDiv64` (128 bit multiply and divide) against exact integer vectors made with Python (`MulDivVectors.inc`), `GetCurrentTimestamp`, `SystemClockMillis` and `EndTime`, `CMyQueue`, `CAutoInt` |
| `TransformMatrixTests` | fontEngine `TransformMatrix`: translation, scaling, rotations, products, alpha, inverse |
| `BdPacketTests` | BDReader `Packet` and `PacketList`/`PopHead` |
| `TcharConvTests` | `shared/TcharConv.h` (`ToAnsi`, `ToWide`, `ToTchar`) |
| `WStringUtilTests` | `shared/WStringUtil.h` (`ReplaceAll`) |

`TestPackets.h` builds TS packets, PCR and PTS fields. The shared code calls `LogDebug()`; `TestSupport.cpp` provides an
empty one.

## Integration tests (`integration/`, `mp_integration_tests`)

They load the real filter DLLs from the build folder through `DllGetClassObject`, so nothing is registered in the
registry and no administrator rights are needed.

* `FilterLoadTests`: for each of TsReader, TsWriter, MPAudioRenderer, DVBSub3, MPFileWriter, MPIPTVSource, TsMuxer and
  CCCP: the DLL loads, exports the DirectShow entry points, creates the filter with the right class id, refuses an
  unknown class, joins and leaves a filter graph (quartz.dll), enumerates pins, can be created 20 times and unloaded.
* `TsReaderGraphTests`: TsReader in a graph reading transport streams written by `TsStreamBuilder.h` (PAT, PMT, PCR,
  MPEG-2 video and MPEG-1 audio PES): valid stream, missing file, random bytes, damaged, truncated and empty streams.
  A missing or unreadable file takes TsReader about 2 to 6 seconds (its own open retries), so the suite needs ~25 s.

* `Win7RefreshRateTests`: `Win7RefreshRateHelper.dll` (`W7GetRefreshRate`) against `EnumDisplaySettings`, an unknown monitor,
  repeated and parallel calls.
* `DvbLib.*` (**dvblib.dll**): the PID mapping functions called through the exports on the real MPEG-2 Demultiplexer of Windows
  (`GetPidMapping`, `GetPidMap`, `SetupDemuxerPin`, `SetupDemuxerPids`, `SetupDemuxer`, `DumpMpeg2DemuxerMappings`), the
  B2C2 helpers (`DeleteAllPIDs`, `SetPidToPin`) on a recording stand-in for the B2C2 data control, `SetWmvProfile`.
* `Dshowhelper.*`, `FontEngine.*`, `HelperLibraries.*` (**dshowhelper.dll**, **fontEngine.dll**): they load and unload, and
  export everything that the C# client imports.
* `LiveMediaRtsp.*` (**LiveMedia555**, linked in as a library): `MPRTSPServer` and `MPRTSPClient` talk over the loopback
  interface in one process: no sessions at start, OPTIONS answered, an unknown stream gives 404 and no session, the server
  listens on IPv4 only, a port in use gives no server.

The filters have to be built; `mp_integration_tests` depends on them in CMake. A filter that does not load (missing
dependency) fails its tests with the Windows error text; it is not skipped.

## Local tests (`local/`, `mp_local_tests`)

For the developer machine only: they use the media samples folder and a real Direct3D 9 device, so they are not built by
default and are not meant for CI. They skip (with the reason) when the sample, `mpcSubs.dll` or the hardware is missing.

```bash
cmake --preset vs2026-x64-unicode -DMP_BUILD_TESTS=ON -DMP_BUILD_LOCAL_TESTS=ON -DVCPKG_MANIFEST_FEATURES=tests       [-DMP_SAMPLES_DIR=D:/path/to/MP-MediaInfo.Samples]
cmake --build --preset vs2026-x64-unicode-debug --target mp_local_tests
ctest --test-dir build/vs2026-x64-unicode -C Debug -L local --output-on-failure
```

The samples folder is `MP_SAMPLES_DIR` (CMake cache variable, or the environment variable of the same name at run
time); by default `<repository>/../MP-MediaInfo.Samples` when it exists. `MP_MPCSUBS_DLL` can point to another
`mpcSubs.dll` than the one in the build folder.

* `DvbSubtitleTests` (**DVBSub3**): the subtitle PID of `HD Audio\12-10_19-18-52_BBC HD_Wild China.ts` (a BBC HD
  recording, PID 0x69, English) is found through PAT/PMT (`TsSubtitleExtractor`), its TS packets go through a real graph
  (`TestSourceFilter` stands in for TsReader) into the filter, and the bitmaps come back through the callback that the
  client uses. Checked: 13 decoded subtitles with sane size, position, time stamps and pixels; the result does not depend
  on how the stream is cut into samples; no output without a PID or with another PID or while the filter is stopped;
  reset callback on seek; flush, channel change and restart; damaged and random data. Needs only `DVBSub3.ax`.
* `MpcSubsTests` (**mpcSubs.dll**, the MPC-HC engine): driven through its C API like `MpcEngine.cs`. SubRip and
  SubStation Alpha files written next to a movie name are found and listed, stream switching, enable and delay,
  reloading, freeing, broken files, and the text is drawn on a Direct3D 9 render target that is read back (lit pixels
  while a cue is displayed, none outside). Two tests take embedded subtitles from `HD Audio\zx.eva.renewal.01.divx511.mkv`
  (SubRip) and the Kametsu `.mkv` in `HD` (SubStation Alpha) and need a DirectShow splitter that exposes them.
  mpcSubs.dll is built with the other filters (no MFC, no ATL). The 16 `MpcSubs.*` tests are skipped only when the DLL is
  not found; they ran against the real DLL on x64 and x86.
* `MpAudioRendererTests` (**MPAudioRenderer**): the renderer in a graph behind `TestSourceFilter` that delivers generated PCM
  (`AudioSupport.h`); it opens the default Windows audio endpoint (WASAPI) and plays silence only, so it needs an audio
  device (skips without one). Connection of the usual PCM formats, refusal of non-audio, real-time playback to the end of
  stream, float and 5.1, stop while samples wait, flush, volume and balance, `IMediaSeeking` passed on to the upstream
  filter, settings (`IMPAudioRendererConfig`) and refused out-of-range values. `LocalSupport.cpp` starts a watcher that
  closes the Debug build's modal ASSERT box and turns it into a failure of the running test.
* `BdReaderTests` (**BDReader**): path checks of `Load()` (no BD, wrong case, missing disc), the sample disc
  `BD\BDMV\index.bdmv` (loads, title count, angle, output pins) and playback: BDReader in a graph with a sink on every output
  pin, run for a few seconds, samples with time stamps arrive. BDReader loads libbluray as `bluray.dll` from its own folder; the
  build copies `bluray.dll` (x86 or x64) and the BD-J jars next to the test from the NuGet package `MediaPortal.libbluray`
  1.5.1. Both builds ran against the HDMV sample disc; a disc with BD-J menus is not tested yet.
* `LiveMediaPesToTs` (**LiveMedia555**): `MPMPEG2TransportStreamFromPESSource` converts `Video\a52_dvb_remuxed.m2p` to
  transport packets: 188 byte multiples, sync bytes, PAT present, at least 3 PIDs.
* `FontEngineTests` (**fontEngine.dll**): on a real Direct3D 9 device: slots of the texture table, a texture drawn as a quad
  where it is placed (pixels read back), the diffuse colour modulating the texture, textures outside the viewport, numbers
  outside the table, the limit of 2000 textures.
* `LocalEnvironment.*` check the test bench itself (Direct3D read back, the video renderer by name, temp folder) and run
  everywhere.

## Tests of TsMuxer, TsReader and TsWriter

Three more programs, each built from the sources of its filter together with DvbCoreUtils; they test the code of the filter, not
the DLL (that is `mp_integration_tests`). Common helpers are in `unit/`: `BitWriter.h` (bit fields, Exp-Golomb, emulation
prevention), `SectionBuilder.h`, `GuardedBuffer.h` (guard page), `TempDir.h`, `AbortGuard.h` (turns an `assert()`/`abort()` of
the code under test into a return value), `CrtAsserts.h` (counts CRT assertions instead of showing the dialog).

**`mp_tsmuxer_tests`** (`tsmuxer/TsMuxerTests.cpp`): Hamming 8/4 table against the code for every byte, the packet
synchroniser, `CMemoryBuffer` (order, limits, blocking read, stop, callback), the teletext grabber (25 packets per callback,
bit order), the analog channel scan (station name from packet 30 of magazine 8).

**`mp_tsreader_tests`**: `GolombBufferTests` (bits, Exp-Golomb, behaviour at the end of the data), `FrameHeaderParserTests` (MPEG
audio layers 1 to 3 and MPEG-2.5, ADTS, AC-3, E-AC-3, DTS, MPEG-2 sequence header, PES header, start codes, H.264 SPS/PPS built
with `BitWriter` including emulation prevention, aspect ratios and timing, `CMpegPesParser` as the demultiplexer uses it; random,
damaged and truncated data of every kind in front of a guard page; random HEVC SPS), `FileReaderTests` (`FileReader` on real files
including a sparse file above 4 GB, `MultiFileReader` on hand made timeshift buffers: files across the boundary, seeking, new and
removed files, damaged info files), `PsiParserTests` (TsReader's `CPmtParser` and `CPatParser`).

**`mp_tswriter_tests`**: `TextUtilTests` (DVB text to UTF-8: ISO 6937 with accents, UCS-2, UTF-8, other tables, buffer limits,
Huffman, random data against exact-size buffers), `ParserTests` (`CPcrDecoder` including PTS/DTS rewriting, `CSdtParser`,
`CPmtParser`, `CPesDecoder`/`CPesPacket`, damaged SDT and PMT tables), `FileWriterTests` (`FileWriter` with chunk reserve,
`CDiskBuff`, `MultiFileWriter` writing a timeshift buffer that TsReader's `MultiFileReader` reads back: ring of files, reuse of the
oldest file, a reader that holds a file, cleanup on close).

## AddressSanitizer

```bash
cmake --preset vs2026-x64-unicode -DMP_BUILD_TESTS=ON -DVCPKG_MANIFEST_FEATURES=tests -DMP_TEST_ASAN=ON
cmake --build --preset vs2026-x64-unicode-release --target mp_unit_tests
# the ASan runtime (clang_rt.asan_*.dll) from VC\Tools\MSVC\<version>\bin\Hostx64\x64 has to be on PATH
```

Only the test executables (and the code compiled into them, e.g. `BitstreamReader.cpp`) are instrumented; ASan does not
work together with `/RTC1`, so use Release. `BitstreamReaderTests` also uses a guard page, which finds the same kind of
read past the end of a buffer without ASan.

## Found by the tests

* `CPacketSync` (`shared/PacketSync.cpp`), found while testing the synchronisation. Not changed, the tests record it:
  * Once in sync a packet is accepted when *one* of its sync byte and the next one is 0x47, and the other is overwritten
    with 0x47 in the caller's buffer. Bytes lost or garbage inserted inside the stream therefore give one block made of the
    end of one packet and the start of the next (two for exactly 188 bytes of garbage), and the recovery costs the packets
    around the damage.
  * A lone 0x47 exactly one packet length before the stream start makes the data in front of it a packet of its own.
  * When no packet is found within 8 packet lengths of *one call*, the state is reset and the call returns: the valid packets
    later in the same buffer are lost (a file reader with big buffers loses the rest of a buffer after a 1.5 KB gap).
  * `OnRawData2` keeps nothing while it is not in sync and the buffer has less than 5 packets (it returns true, the caller has
    to read more), so it must be fed with at least 5 packets to begin with.

* `DVBSubtitle3/Source/PesDecoder/PacketSync.cpp` was an older private copy of `CPacketSync`. With data left over from
  the previous sample it copied `188 - leftover` bytes from the new sample without checking the sample length, so a sample
  shorter than that was read past its end and the packet corrupted (0 of 13 subtitles decoded with 7 byte samples).
  Fixed by using the shared `CPacketSync` (`DvbCoreUtils/PacketSync.cpp`, `shared/PacketSync.h`) in DVBSubtitle3 and
  deleting the copy (audit item READ-02 for this filter); `DvbSub3Graph.SamplesShorterThanOnePacketAreDecodedLikeAnyOther`
  and the sample size test (down to 1 byte) cover it.

* `shared/HEVC/BitstreamReader.cpp`: `getBit()` and `skipBits()` looked at `m_ptr[m_posBase]` for the emulation
  prevention byte after the last byte had been consumed, one byte behind the buffer (heap buffer overflow, read of 1
  byte). Fixed with `m_posBase < m_size`; the guard page tests fail without the fix.

* `MPAudioRenderer/source/dllmain.cpp` `CopyWaveFormatEx` copied `sizeof(WAVEFORMATEXTENSIBLE) + cbSize` (62 bytes) from a
  format block that is 40 bytes (heap over-read, found with ASan). Fixed: it copies `sizeof(WAVEFORMATEX) + cbSize` into a
  zeroed block of at least the extensible size.

* Also in MPAudioRenderer, found by the tests and fixed (the tests are enabled):
  * Reconnecting a renderer with another format: the WASAPI render thread kept the buffer of `IAudioRenderClient::GetBuffer`
    while it waited for a sample with `m_csResources` unlocked; the format change released the audio client in that time and
    the thread wrote into the freed buffer (ASan: heap-use-after-free WRITE, freed in `AUDIOSES.DLL`). `CWASAPIRenderFilter`
    now counts the audio clients (`m_nClientGeneration`), takes a new buffer when the client was replaced, and
    `GetWASAPIBuffer` refuses to work without clients. `ReconnectingWithAnotherFormatDoesNotCorruptTheHeap` fails about one
    run in six without the fix (a race), 25 of 25 pass with it.
  * `CTimeStretchFilter::clear()` asserted `m_Stream || (m_bBitstreaming && !m_Stream)` in Debug builds. The assert is right
    (stream operations need a stream or bitstreaming); the callers were wrong: the processing thread called `clear()` when it
    stopped and on a flush command even if no format was ever set (refused connection, filter destroyed unconnected).
    They call `clearIfConfigured()` now. The local tests no longer tolerate any ASSERT.
  * `AudioRendererSettings::GetString` returned E_POINTER for every call. It returns the end point id of
    `SETTING_AUDIO_DEVICE` (empty for the default device) as a `CoTaskMemAlloc` string, E_NOTIMPL for other settings, and
    sets the output to null on failure.

* MPAudioSwitcher down sampling (`EnableDownSamplingTo441`), found by the resampler tests and fixed:
  * `AudioStreamResampler::Downsample` never decreased `samplesin`: it read about `filter_width` samples behind the input and
    started the next call with samples it had already used. It now consumes the input like VirtualDub's original.
  * `CAudioSwitcherFilter::Transform` asked for exactly `lenout` samples; when they were produced before the input was used
    up, the rest of the input was thrown away (a jump of one to three input samples at every buffer boundary, also with
    point sampling). Now the output has room for `lenout + 2` samples (`CreateNewOutputMediaType`) and the length of the
    output sample is what the resampler delivered. `Resampler.ConsecutiveBuffersJoinWithoutAJump` (join steps are not bigger
    than the steps inside a buffer) and `NoInputIsLostOverManyBuffers` cover it. **`AudioSwitcher.cpp` itself is compiled in
    `MPAudioSwitcher.ax` but not run in a graph**; the resampler is tested the way `Transform` calls it.
* `AudioRendererSettings` (MPAudioRenderer): `SetInt(LIB_RESAMPLE_QUALITY)` had no `break` and ran on into the `USE_FILTERS`
  check (a valid quality was refused with E_INVALIDARG); `m_wWASAPIPreferredDeviceId` was a null pointer until the registry
  load allocated it, and `SetAudioDevice()` wrote into it. It is allocated (empty) by the constructor now.
  `ResamplingQualityIsAcceptedAndKept` and the audio device tests cover it.

* Found while writing the tests for the libraries and for TsMuxer, TsReader and TsWriter; every one is fixed and its test
  enabled:
  * `CSectionDecoder` (DvbCoreUtils): `CSection::CalcSectionLength()` read `tsPacket[start + 1]` for a one byte payload at the
    very end of a packet (access violation behind the packet). It returns when the second length byte is in the next packet.
  * `CBasePmtParser` and TsReader's `CPmtParser`: the closing braces of the ISO 639 branch were misplaced, so the language of
    an AC-3 stream (stream type 6, the usual DVB case) was lost when no audio stream came before it; a stream that was not
    found removed the last PID of the list (`pop_back()`) instead of its own. The teletext page of magazine 8 (UK subtitles
    888) was reported as 88: magazine 0 in the descriptor is 8.
  * `CChannelInfo::Copy()` (`shared/ChannelInfo.cpp`) did not copy the EIT flags, `RunningStatus` and `PatVersion`.
  * `CMyQueue::Resize()` (DirectShowHelper) did not allocate, so growing the queue overflowed the array.
    `EndTime::InfiniteValue` was `490000000000` cut to 32 bits (373728256): a timeout of four days was "infinite". It is the
    largest unsigned number now.
  * `TransformMatrix` (fontEngine): `SetXRotation`, `SetYRotation` and `SetZRotation` ended with `angle = 1.0f;` instead of
    `alpha = 1.0f;`.
  * dvblib: `SetupDemuxerPin(NULL, ...)` crashed (`pMap->MapPID` with a null `pMap`); like the other setup functions it does
    nothing and returns S_OK without a pin.
  * TsMuxer: `CPacketSync` copied "188 - waiting" bytes from the new data without a look at its length (the same defect that
    was fixed in DVBSubtitle3; one byte per call gave 563 "packets" for 3 real ones); `CMemoryBuffer::PutBuffer` accepted an
    offset beyond the length (`new byte[negative]`) and gave the callback the wrong length (`length - offset` now);
    `CChannelScan` left `m_pCallback`, `m_bChannelFound` and `m_bScanningPossible` undefined; `CTeletextGrabber` and
    `CChannelScan` copied samples of any size into 20000 byte buffers (a sample that does not fit starts the buffer again, one
    larger than the buffer is dropped).
  * TsReader: `CFrameHeaderParser::Read(eac3hdr)` divided by zero for fscod 3 with fscod2 3 (reserved) when a media type is
    asked for; `Read(avchdr)` trusted the NAL length from the data for `malloc` (64 MB for 20 bytes of data), it has to lie
    inside the data now; the HEVC `BitstreamReader` `assert(num <= 32)` was reached with counts from the stream (abort in Debug,
    undefined shift in Release), `getBits`/`showBits` throw `std::range_error` now, which the NAL decoder catches like a lack
    of data; `FileReader` returned `ERROR_FILENAME_EXCED_RANGE`/`ERROR_INVALID_NAME` as an HRESULT, which `SUCCEEDED()` takes
    for success (`HRESULT_FROM_WIN32` now); `CPatParser::GetChannel(index)` tested `index > Count()` (an empty parser crashed on
    `GetChannel(0)`); `MultiFileReader` lost its oldest file at the second refresh when it opened a buffer that had already
    wrapped around (the files removed before it came are counted now).
  * TsWriter: `CTextUtil` read `buf[2]` for the three byte selector 0x10 with less than three bytes of input (it compared the
    size of the output); `CBuffer::Write` looked for a sequence header up to three bytes behind the payload; `CPesDecoder` did
    not initialise `m_bStartFound`; `FileWriter`, `FileWriterThreaded` and `MultiFileWriter` returned Win32 error codes as
    HRESULTs like `FileReader`; `CDiskBuff::Add` took a negative length.
  * Win7RefreshRateHelper: `DEVMODE` and `DISPLAY_DEVICE` were not initialised; in an optimised build monitor 99 got the rate of
    the previous call (240 instead of -1). A monitor that is not there gives -1.
  * Left disabled on purpose: `MpAudioRenderer.DISABLED_AFormatBlockOfExactlyTheRightSizeIsNeverReadBeyondItsEnd`. The defect
    (`CopyWaveFormatEx`) is fixed; the test passes a format block of exactly the right size, which only ASan or a guard page can
    tell from a padded one.

## Next steps (not done)

* MPAudioSwitcher as a filter (source, switcher, sink): `MPAudioSwitcher.ax` builds, but only the resampler is tested.
* LiveMedia555 RTSP sessions (SETUP, PLAY, PAUSE, `RemoveSessionById`) need a media session on the server.
* Demultiplexers: `TsReader`/`BDReader` `CDeMultiplexer` (PES assembly, `unique_ptr<Packet>` ownership). Needs a
  test harness around `CTsReaderFilter` or an extraction of the PES assembly; the stream builder is ready.
* Graph tests with decoders: connect the output pins of TsReader to a renderer/decoder and run the graph.
* TsWriter: write a graph that records a stream and compare the output.
* A CI step: `native_filters.yml` could run the unit tests and the integration tests after the build.
