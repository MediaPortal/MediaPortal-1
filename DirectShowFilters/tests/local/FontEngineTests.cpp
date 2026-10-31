// fontEngine.dll: the texture drawing of the skin engine of the MediaPortal client, on a real Direct3D 9 device. The DLL is a
// singleton with static state, so every test initialises it again. Needs a graphics device: local only.
#include <gtest/gtest.h>
#include <d3d9.h>
#include <string>
#include <vector>
#include "FilterHost.h"
#include "LocalSupport.h"

namespace
{
  typedef void(__cdecl* InitializeFn)(int, int, int);
  typedef void(__cdecl* SetDeviceFn)(void*);
  typedef int(__cdecl* AddTextureFn)(INT64, bool, void*);
  typedef void(__cdecl* RemoveTextureFn)(int);
  typedef void(__cdecl* DrawTextureFn)(int, float, float, float, float, float, float, float, float, unsigned int, float[3][4]);
  typedef void(__cdecl* PresentTexturesFn)();
  typedef void(__cdecl* ClipFn)();

  const int kWidth = 640, kHeight = 360;

  class FontEngine : public testing::Test
  {
  protected:
    ComScope com;
    FilterModule module;
    IDirect3D9* d3d = nullptr;
    IDirect3DDevice9* device = nullptr;
    IDirect3DSurface9* target = nullptr;
    IDirect3DSurface9* system = nullptr;
    HWND window = nullptr;

    InitializeFn initialize = nullptr;
    SetDeviceFn setDevice = nullptr;
    AddTextureFn addTexture = nullptr;
    RemoveTextureFn removeTexture = nullptr;
    DrawTextureFn drawTexture = nullptr;
    PresentTexturesFn present = nullptr;

    void SetUp() override
    {
      ASSERT_TRUE(SUCCEEDED(com.Result()));
      ASSERT_TRUE(module.Load(L"fontEngine.dll")) << ToUtf8(module.Error());
      initialize = reinterpret_cast<InitializeFn>(module.Export("FontEngineInitialize"));
      setDevice = reinterpret_cast<SetDeviceFn>(module.Export("FontEngineSetDevice"));
      addTexture = reinterpret_cast<AddTextureFn>(module.Export("FontEngineAddTexture"));
      removeTexture = reinterpret_cast<RemoveTextureFn>(module.Export("FontEngineRemoveTexture"));
      drawTexture = reinterpret_cast<DrawTextureFn>(module.Export("FontEngineDrawTexture"));
      present = reinterpret_cast<PresentTexturesFn>(module.Export("FontEnginePresentTextures"));
      ASSERT_TRUE(initialize && setDevice && addTexture && removeTexture && drawTexture && present);

      d3d = Direct3DCreate9(D3D_SDK_VERSION);
      if (!d3d)
        GTEST_SKIP() << "Direct3D 9 is not available";
      window = CreateWindowExW(0, L"STATIC", L"mp_local_tests", WS_POPUP, 0, 0, kWidth, kHeight, nullptr, nullptr, GetModuleHandleW(nullptr), nullptr);
      D3DPRESENT_PARAMETERS pp = {};
      pp.Windowed = TRUE;
      pp.SwapEffect = D3DSWAPEFFECT_DISCARD;
      pp.BackBufferFormat = D3DFMT_UNKNOWN;
      pp.hDeviceWindow = window;
      pp.BackBufferWidth = kWidth;
      pp.BackBufferHeight = kHeight;
      const DWORD flags = D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE;
      HRESULT hr = d3d->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, window, flags | D3DCREATE_HARDWARE_VERTEXPROCESSING, &pp, &device);
      if (FAILED(hr))
        hr = d3d->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, window, flags | D3DCREATE_SOFTWARE_VERTEXPROCESSING, &pp, &device);
      if (FAILED(hr))
        GTEST_SKIP() << "cannot create a Direct3D 9 device";
      ASSERT_EQ(device->CreateRenderTarget(kWidth, kHeight, D3DFMT_X8R8G8B8, D3DMULTISAMPLE_NONE, 0, FALSE, &target, nullptr), D3D_OK);
      ASSERT_EQ(device->CreateOffscreenPlainSurface(kWidth, kHeight, D3DFMT_X8R8G8B8, D3DPOOL_SYSTEMMEM, &system, nullptr), D3D_OK);
      initialize(kWidth, kHeight, 1);
      setDevice(device);
    }

    void TearDown() override
    {
      if (setDevice)
        setDevice(nullptr);
      if (target) target->Release();
      if (system) system->Release();
      if (device) device->Release();
      if (d3d) d3d->Release();
      if (window) DestroyWindow(window);
    }

    // A texture of one colour
    IDirect3DTexture9* SolidTexture(int size, D3DCOLOR color)
    {
      IDirect3DTexture9* texture = nullptr;
      if (FAILED(device->CreateTexture(size, size, 1, 0, D3DFMT_A8R8G8B8, D3DPOOL_MANAGED, &texture, nullptr)))
        return nullptr;
      D3DLOCKED_RECT rect;
      texture->LockRect(0, &rect, nullptr, 0);
      for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
          reinterpret_cast<D3DCOLOR*>(static_cast<BYTE*>(rect.pBits) + y * rect.Pitch)[x] = color;
      texture->UnlockRect(0);
      return texture;
    }

    // The state that the skin engine sets up before it draws: pixel coordinates, no lighting, no culling
    void PrepareScene()
    {
      device->SetRenderTarget(0, target);
      device->Clear(0, nullptr, D3DCLEAR_TARGET, D3DCOLOR_XRGB(0, 0, 0), 1.0f, 0);
      D3DMATRIX projection = {};
      projection._11 = 2.0f / kWidth;
      projection._22 = -2.0f / kHeight;
      projection._33 = 1.0f;
      projection._41 = -1.0f;
      projection._42 = 1.0f;
      projection._44 = 1.0f;
      D3DMATRIX identity = {};
      identity._11 = identity._22 = identity._33 = identity._44 = 1.0f;
      device->SetTransform(D3DTS_WORLD, &identity);
      device->SetTransform(D3DTS_VIEW, &identity);
      device->SetTransform(D3DTS_PROJECTION, &projection);
      device->SetRenderState(D3DRS_LIGHTING, FALSE);
      device->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);
      device->SetRenderState(D3DRS_ZENABLE, FALSE);
    }

    // Number of pixels of the target with the colour 'color' inside the rectangle
    int Count(D3DCOLOR color, RECT area)
    {
      if (FAILED(device->GetRenderTargetData(target, system)))
        return -1;
      D3DLOCKED_RECT rect;
      if (FAILED(system->LockRect(&rect, nullptr, D3DLOCK_READONLY)))
        return -1;
      int count = 0;
      for (int y = area.top; y < area.bottom; y++)
        for (int x = area.left; x < area.right; x++)
          if ((reinterpret_cast<D3DCOLOR*>(static_cast<BYTE*>(rect.pBits) + y * rect.Pitch)[x] & 0xFFFFFF) == (color & 0xFFFFFF))
            count++;
      system->UnlockRect();
      return count;
    }

    static void Identity(float m[3][4])
    {
      memset(m, 0, 12 * sizeof(float));
      m[0][0] = m[1][1] = m[2][2] = 1.0f;
    }
  };
}

TEST_F(FontEngine, TexturesAreKeptBySlot)
{
  IDirect3DTexture9* a = SolidTexture(16, 0xFFFF0000);
  IDirect3DTexture9* b = SolidTexture(16, 0xFF00FF00);
  ASSERT_TRUE(a && b);
  const int slotA = addTexture(1001, true, a);
  const int slotB = addTexture(1002, true, b);
  EXPECT_GE(slotA, 0);
  EXPECT_GE(slotB, 0);
  EXPECT_NE(slotA, slotB);
  EXPECT_EQ(addTexture(1001, true, a), slotA) << "the same hash code is the same slot";
  removeTexture(slotA);
  removeTexture(slotB);
  removeTexture(-1);                                     // outside the table: nothing happens
  removeTexture(100000);
  a->Release();
  b->Release();
}

TEST_F(FontEngine, ATextureIsDrawnAsAQuadWhereItIsPlaced)
{
  IDirect3DTexture9* red = SolidTexture(64, 0xFFFF0000);
  ASSERT_TRUE(red);
  const int slot = addTexture(2001, false, red);
  ASSERT_GE(slot, 0);
  PrepareScene();
  float m[3][4];
  Identity(m);
  ASSERT_EQ(device->BeginScene(), D3D_OK);
  drawTexture(slot, 100, 80, 200, 100, 0, 0, 1, 1, 0xFFFFFFFF, m);
  present();
  device->EndScene();

  const RECT inside = { 100, 80, 300, 180 };
  const RECT whole = { 0, 0, kWidth, kHeight };
  const int red_inside = Count(0xFF0000, inside);
  const int red_total = Count(0xFF0000, whole);
  EXPECT_GT(red_inside, 200 * 100 * 9 / 10) << "the quad covers its rectangle";
  EXPECT_LT(red_total, 202 * 102) << "and nothing much else";
  removeTexture(slot);
  red->Release();
}

TEST_F(FontEngine, TheColourOfTheCallModulatesTheTexture)
{
  IDirect3DTexture9* white = SolidTexture(64, 0xFFFFFFFF);
  ASSERT_TRUE(white);
  const int slot = addTexture(2002, false, white);
  PrepareScene();
  float m[3][4];
  Identity(m);
  device->BeginScene();
  drawTexture(slot, 50, 50, 100, 100, 0, 0, 1, 1, 0xFF0000FF, m);        // white texture, blue diffuse colour
  present();
  device->EndScene();
  const RECT inside = { 50, 50, 150, 150 };
  EXPECT_GT(Count(0x0000FF, inside), 100 * 100 * 9 / 10);
  removeTexture(slot);
  white->Release();
}

TEST_F(FontEngine, TexturesOutsideTheViewportAreNotDrawn)
{
  IDirect3DTexture9* red = SolidTexture(64, 0xFFFF0000);
  const int slot = addTexture(2003, false, red);
  PrepareScene();
  float m[3][4];
  Identity(m);
  device->BeginScene();
  drawTexture(slot, kWidth + 10, 50, 100, 100, 0, 0, 1, 1, 0xFFFFFFFF, m);
  drawTexture(slot, -200, 50, 100, 100, 0, 0, 1, 1, 0xFFFFFFFF, m);
  drawTexture(slot, 50, kHeight + 5, 100, 100, 0, 0, 1, 1, 0xFFFFFFFF, m);
  present();
  device->EndScene();
  const RECT whole = { 0, 0, kWidth, kHeight };
  EXPECT_EQ(Count(0xFF0000, whole), 0);
  removeTexture(slot);
  red->Release();
}

TEST_F(FontEngine, ATextureNumberOutsideTheTableIsIgnored)
{
  PrepareScene();
  float m[3][4];
  Identity(m);
  device->BeginScene();
  drawTexture(-1, 10, 10, 50, 50, 0, 0, 1, 1, 0xFFFFFFFF, m);
  drawTexture(5000, 10, 10, 50, 50, 0, 0, 1, 1, 0xFFFFFFFF, m);
  present();
  device->EndScene();
  SUCCEED();
}

TEST_F(FontEngine, PresentWithoutAnythingToDrawIsHarmless)
{
  present();
  present();
  SUCCEED();
}

TEST_F(FontEngine, TheTableOfTexturesIsFinite)
{
  // 2000 slots: the 2001st texture gets -1 and nothing breaks
  IDirect3DTexture9* tiny = SolidTexture(4, 0xFFFFFFFF);
  ASSERT_TRUE(tiny);
  std::vector<int> slots;
  int last = 0;
  for (int i = 0; i < 2005; i++)
  {
    last = addTexture(5000 + i, true, tiny);
    if (last >= 0)
      slots.push_back(last);
  }
  EXPECT_EQ(slots.size(), 2000u);
  EXPECT_EQ(last, -1);
  for (int slot : slots)
    removeTexture(slot);
  tiny->Release();
}
