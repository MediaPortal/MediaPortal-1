#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.Drawing;

namespace MediaPortal.Visualization
{
  public interface IVisualization
  {
    Un4seen.Bass.AddOn.Vis.BASS_VIS_PARAM VizParam { get; }
    bool Initialized { get; }
    bool PreRenderRequired { get; }
    bool IsEngineInstalled();
    bool IsWinampVis();
    bool IsWmpVis();
    bool Initialize();
    bool InitializePreview(); // Used for visualization previews
    bool Config();
    int PreRenderVisualization();
    int RenderVisualization();
    bool Start();
    bool Pause();
    bool Stop();
    bool WindowSizeChanged(Size newSize);
    bool WindowChanged(VisualizationWindow vizWindow);
    bool SetOutputContext(VisualizationBase.OutputContextType outputType);
    bool Close();
  }
}