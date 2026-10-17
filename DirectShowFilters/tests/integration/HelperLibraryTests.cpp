// dshowhelper.dll (DirectShowHelper) and fontEngine.dll: the native helper DLLs of the MediaPortal client, called through P/Invoke.
// Here: they load and unload, and export what the C# side imports. What they do needs Direct3D: see the local tests.
#include <gtest/gtest.h>
#include "FilterHost.h"

namespace
{
  const char* const kDshowhelperExports[] = {
    "Vmr9Init", "Vmr9Deinit", "Vmr9SetDeinterlaceMode", "Vmr9SetDeinterlacePrefs", "DvrMsCreate", "DvrMsStart", "DvrMsStop",
    "AddTeeSinkToGraph", "AddWstCodecToGraph", "GrabBitmaps", "AddTeeSinkNameToGraph", "EvrInit", "EvrDeinit", "EVRDrawStats",
    "EVRResetStatCounters", "EVRNotifyRateChange", "EVRNotifyDVDMenuState", "EVRGetVideoFPS", "EVRUpdateDisplayFPS",
    "EVRGetDisplayFPS", "MadInit", "MadDeinit", "MadStopping", "MadVrPaused", "MadVrRepeatFrameSend", "MadVrGrabFrameSend",
    "MadVrGrabCurrentFrameSend", "MadVrGrabScreenshotSend", "MadVrWindowPosition", "MadVr3DRight", "MadVr3DLeft", "MadVr3DEnable",
    "MadVrScreenResizeForce" };

  const char* const kFontEngineExports[] = {
    "FontEngineInitialize", "FontEngineAddFont", "FontEngineRemoveFont", "FontEngineSetCoordinate", "FontEngineDrawText3D",
    "FontEnginePresent3D", "FontEngineAddTexture", "FontEngineRemoveTexture", "FontEngineDrawTexture", "FontEnginePresentTextures",
    "FontEngineAddSurface", "FontEngineSetTexture", "FontEngineDrawSurface", "FontEngineSetDevice", "FontEngineDrawTexture2",
    "FontEngineDrawMaskedTexture", "FontEngineDrawMaskedTexture2", "FontEngineSetClipEnable", "FontEngineSetClipDisable",
    "FontEngineSetRenderState", "FontEngineSetSamplerState", "FontEngineSetTextureStageState", "FontEngineSetMaximumFrameLatency" };
}

TEST(Dshowhelper, LoadsAndExportsEverythingTheClientImports)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(L"dshowhelper.dll")) << "dshowhelper.dll";
  for (const char* name : kDshowhelperExports)
    EXPECT_TRUE(m.HasExport(name)) << name;
}

TEST(Dshowhelper, CanBeLoadedAndUnloadedAgain)
{
  for (int i = 0; i < 10; i++)
  {
    FilterModule m;
    ASSERT_TRUE(m.Load(L"dshowhelper.dll")) << i;
  }
}

TEST(FontEngine, LoadsAndExportsEverythingTheClientImports)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(L"fontEngine.dll")) << "fontEngine.dll";
  for (const char* name : kFontEngineExports)
    EXPECT_TRUE(m.HasExport(name)) << name;
}

TEST(FontEngine, UnloadingWithoutAnyUseCleansUpQuietly)
{
  // DllMain calls Cleanup() on detach; it has to cope with a DLL that was never initialised
  for (int i = 0; i < 10; i++)
  {
    FilterModule m;
    ASSERT_TRUE(m.Load(L"fontEngine.dll")) << i;
  }
}

TEST(HelperLibraries, ExportsAreFoundByName)
{
  FilterModule m;
  ASSERT_TRUE(m.Load(L"fontEngine.dll"));
  EXPECT_FALSE(m.HasExport("FontEngineNoSuchFunction"));
}
