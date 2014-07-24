#region Copyright (C) 2005-2012 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Runtime.InteropServices;

namespace MediaPortal
{
  public enum FontEngineBlendMode
  {
    BLEND_NONE = 0,
    BLEND_DIFFUSE = 1,
    BLEND_OVERLAY = 2
  }
}

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A proxy class which handles all native fontEngine.dll calls
  /// </summary>
  public class DXNative
  {
    // Synchronize access to methods known to cause AccessViolationException 
    // on native side when called simultaneously from multiple threads
    private static readonly object _lock = new object();

    public static void FontEngineRemoveTextureSync(int textureNo)
    {
      lock (_lock)
      {
        FontEngineRemoveTexture(textureNo);
      }
    }

    public static unsafe int FontEngineAddTextureSync(int hasCode, bool useAlphaBlend, void* fontTexture)
    {
      lock (_lock)
      {
        return FontEngineAddTexture(hasCode, useAlphaBlend, fontTexture);
      }
    }

    public static void FontEngineDrawTextureSync(int textureNo, float x, float y, float nw, float nh,
                                             float uoff, float voff, float umax, float vmax, uint color,
                                             float[,] matrix)
    {
      lock (_lock)
      {
        FontEngineDrawTexture(textureNo, x, y, nw, nh, uoff, voff, umax, vmax, color, matrix);
      }
    }

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineInitialize(int iScreenWidth, int iScreenHeight, int poolFormat);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetRenderState(Int32 state, System.UInt32 dwValue);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetSamplerState(System.UInt32 dwStage, Int32 state, System.UInt32 dwValue);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetTextureStageState(System.UInt32 dwStage, Int32 state, System.UInt32 dwValue);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetTexture(void* texture);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe int FontEngineAddTexture(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineRemoveTexture(int textureNo);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawTexture(int textureNo, float x, float y, float nw, float nh,
                                                            float uoff, float voff, float umax, float vmax, uint color,
                                                            float[,] matrix);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineDrawMaskedTexture(int textureNo1, float x, float y, float nw, float nh,
                                                                 float uoff, float voff, float umax, float vmax,
                                                                 uint color,
                                                                 float[,] matrix, int textureNo2, float uoff2,
                                                                 float voff2,
                                                                 float umax2, float vmax2);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineDrawMaskedTexture2(int textureNo1, float x, float y, float nw, float nh,
                                                                  float uoff, float voff, float umax, float vmax,
                                                                  uint color,
                                                                  float[,] matrix, int textureNo2, float uoff2,
                                                                  float voff2,
                                                                  float umax2, float vmax2, int textureNo3, float uoff3,
                                                                  float voff3, float umax3, float vmax3,
                                                                  FontEngineBlendMode blendMode);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineDrawTexture2(int textureNo1, float x, float y, float nw, float nh,
                                                            float uoff, float voff, float umax, float vmax,
                                                            uint color,
                                                            float[,] matrix, int textureNo2, float uoff2,
                                                            float voff2,
                                                            float umax2, float vmax2,
                                                            FontEngineBlendMode blendMode);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEnginePresentTextures();

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetDevice(void* device);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetClipEnable();

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetClipDisable();

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineAddFont(int fontNumber, void* fontTexture, int firstChar, int endChar,
                                                        float textureScale, float textureWidth, float textureHeight,
                                                        float fSpacingPerChar, int maxVertices);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineRemoveFont(int fontNumber);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue1,
                                                              float fValue2, float fValue3, float fValue4);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEngineDrawText3D(int fontNumber, void* text, int xposStart, int yposStart,
                                                           uint intColor, int maxWidth, float[,] matrix);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe void FontEnginePresent3D(int fontNumber);

    [DllImport("fontEngine.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern unsafe int FontEngineSetMaximumFrameLatency(uint maxLatency);
  }
}
