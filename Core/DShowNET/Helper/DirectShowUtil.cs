/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;
#pragma warning disable 618
namespace DShowNET.Helper
{
	/// <summary>
	/// 
	/// </summary>
	public class DirectShowUtil
  {
    const int magicConstant = -759872593;
		public DirectShowUtil()
		{
		}

    static public IBaseFilter AddFilterToGraph(IGraphBuilder graphBuilder, string strFilterName)
    {
			try
			{
				IBaseFilter NewFilter=null;
        foreach (Filter filter in Filters.LegacyFilters)
				{
					if (String.Compare(filter.Name,strFilterName,true) ==0)
          {
						NewFilter = (IBaseFilter) Marshal.BindToMoniker( filter.MonikerString );

            int hr = graphBuilder.AddFilter(NewFilter, strFilterName);
						if( hr < 0 ) 
						{
							Log.WriteFile(Log.LogType.Error,true,"failed:unable to add filter:{0} to graph", strFilterName);
							NewFilter=null;
						}
						else
						{
							Log.WriteFile(Log.LogType.Log,"added filter:{0} to graph", strFilterName);
						}
						break;
					}
				}
				if (NewFilter==null)
				{
					Log.WriteFile(Log.LogType.Error,true,"failed filter:{0} not found", strFilterName);
				}
				return NewFilter;
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Error,true,"failed filter:{0} not found {0}", strFilterName,ex.Message);
				return null;
			}
    }

    static public IBaseFilter AddAudioRendererToGraph(IGraphBuilder graphBuilder,string strFilterName, bool setAsReferenceClock)
    {
			try
			{
				int hr;
				IPin pinOut=null;
				IBaseFilter NewFilter=null;
				Log.WriteFile(Log.LogType.Log,"add filter:{0} to graph clock:{0}", strFilterName,setAsReferenceClock);
	      
				//check first if audio renderer exists!
				bool bRendererExists=false;
        foreach (Filter filter in Filters.AudioRenderers)
				{
					if (String.Compare(filter.Name,strFilterName,true) ==0)
					{
						bRendererExists=true;
					}
				}
				if (!bRendererExists) 
				{
					Log.WriteFile(Log.LogType.Log,true,"FAILED: audio renderer:{0} doesnt exists", strFilterName);
					return null;
				}

				// first remove all audio renderers
				bool bAllRemoved=false;
				bool bNeedAdd=true;
				IEnumFilters enumFilters;
				hr=graphBuilder.EnumFilters(out enumFilters);
				if (hr>=0 && enumFilters!=null)
				{
					int iFetched;
					enumFilters.Reset();
					while(!bAllRemoved)
					{
            IBaseFilter[] pBasefilter = new IBaseFilter[2];
						hr=enumFilters.Next(1, pBasefilter,out iFetched);
						if (hr<0 || iFetched!=1 || pBasefilter[0]==null) break;

            foreach (Filter filter in Filters.AudioRenderers)
						{
							Guid classId1;
							Guid classId2;
							pBasefilter[0].GetClassID(out classId1);            
	            
							NewFilter = (IBaseFilter) Marshal.BindToMoniker( filter.MonikerString );
							NewFilter.GetClassID(out classId2);
							Marshal.ReleaseComObject( NewFilter );
							NewFilter=null;

							if (classId1.Equals(classId2))
							{ 
								if (filter.Name== strFilterName)
								{
									Log.WriteFile(Log.LogType.Log,"filter already in graph");
									
									if (setAsReferenceClock)
										(graphBuilder as IMediaFilter).SetSyncSource(pBasefilter[0] as IReferenceClock);
                  Marshal.ReleaseComObject(pBasefilter[0]);
									pBasefilter[0] =null;
									bNeedAdd=false;
									break;
								}
								else
								{
									Log.WriteFile(Log.LogType.Log,"remove "+ filter.Name + " from graph");
                  pinOut = FindSourcePinOf(pBasefilter[0]);
                  graphBuilder.RemoveFilter(pBasefilter[0]);
									bAllRemoved=true;
									break;
								}
							}//if (classId1.Equals(classId2))
						}//foreach (Filter filter in filters.AudioRenderers)
            if (pBasefilter[0] != null)
              Marshal.ReleaseComObject(pBasefilter[0]);
					}//while(!bAllRemoved)
					Marshal.ReleaseComObject(enumFilters);
				}//if (hr>=0 && enumFilters!=null)

				if (!bNeedAdd) return null;
				// next add the new one...
        foreach (Filter filter in Filters.AudioRenderers)
				{
					if (String.Compare(filter.Name,strFilterName,true) ==0)
					{
						NewFilter = (IBaseFilter) Marshal.BindToMoniker( filter.MonikerString );
						hr = graphBuilder.AddFilter( NewFilter, strFilterName );
						if( hr < 0 ) 
						{
							Log.WriteFile(Log.LogType.Log,true,"failed:unable to add filter:{0} to graph", strFilterName);
							NewFilter=null;
						}
						else
						{
							Log.WriteFile(Log.LogType.Log,"added filter:{0} to graph", strFilterName);
							if (pinOut!=null)
							{
								hr=graphBuilder.Render(pinOut);
								if (hr==0) Log.WriteFile(Log.LogType.Log," pinout rendererd");
								else Log.WriteFile(Log.LogType.Log,true," failed: pinout render");
							}
							if (setAsReferenceClock)
								(graphBuilder as IMediaFilter).SetSyncSource(NewFilter as IReferenceClock);
							return NewFilter;
						}
					}//if (String.Compare(filter.Name,strFilterName,true) ==0)
				}//foreach (Filter filter in filters.AudioRenderers)
				if (NewFilter==null)
				{
					Log.WriteFile(Log.LogType.Log,true,"failed filter:{0} not found", strFilterName);
				}
			}
			catch(Exception ex)
			{
				Log.WriteFile(Log.LogType.Log,true,"DirectshowUtil. Failed to add filter:{0} to graph :{0} {1} [2}", 
							strFilterName,ex.Message,ex.Source,ex.StackTrace);
			}
      return null;
    }



    static public IPin FindSourcePinOf(IBaseFilter filter)
    {
      int hr=0;
      IEnumPins pinEnum;
      hr=filter.EnumPins(out pinEnum);
      if( (hr == 0) && (pinEnum != null) )
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int f;
        do
        {
          // Get the next pin
          hr = pinEnum.Next( 1, pins, out f );
          if( (hr == 0) && (pins[0] != null) )
          {
            PinDirection pinDir;
            pins[0].QueryDirection(out pinDir);
            if (pinDir==PinDirection.Input)
            {
              IPin pSourcePin=null;
              hr=pins[0].ConnectedTo(out pSourcePin);
              if (hr>=0)
							{
								Marshal.ReleaseComObject(pinEnum);
                return pSourcePin;
              }
            }
            Marshal.ReleaseComObject( pins[0] );
          }
        }
				while( hr == 0 );
				Marshal.ReleaseComObject(pinEnum);
      }
      return null;
    }

		static public bool RenderOutputPins(IGraphBuilder graphBuilder,IBaseFilter filter)
		{
			return RenderOutputPins(graphBuilder,filter,100);
		}
    static public bool RenderOutputPins(IGraphBuilder graphBuilder,IBaseFilter filter, int maxPinsToRender)
    {
			int  pinsRendered=0;
      bool bAllConnected=true;
      IEnumPins pinEnum;
      int hr=filter.EnumPins(out pinEnum);
      if( (hr == 0) && (pinEnum != null) )
      {
        Log.WriteFile(Log.LogType.Log,"got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo=0;
        do
        {
          // Get the next pin
          //Log.WriteFile(Log.LogType.Log,"  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next( 1, pins, out iFetched );
          if( hr == 0 )
          {
            if (iFetched==1 && pins[0]!=null) 
            {
              PinInfo pinInfo = new PinInfo();
              hr=pins[0].QueryPinInfo(out pinInfo);
							if (hr==0)
							{
								Log.WriteFile(Log.LogType.Log,"  got pin#{0}:{1}",iPinNo-1,pinInfo.name);
								//Marshal.ReleaseComObject(pinInfo.filter);
							}
							else
							{
								Log.WriteFile(Log.LogType.Log,"  got pin:?");
							}
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir==PinDirection.Output)
              {
                IPin pConnectPin=null;
                hr=pins[0].ConnectedTo(out pConnectPin);  
								if (hr!=0 || pConnectPin==null)
								{
									hr=graphBuilder.Render(pins[0]);
									if (hr==0) 
									{
										Log.WriteFile(Log.LogType.Log,"  render ok");
									}
									else 
									{
										Log.WriteFile(Log.LogType.Log,true,"  render failed:{0:x}",hr);
										bAllConnected=false;
									}
									pinsRendered++;
								}
								if (pConnectPin!=null)
									Marshal.ReleaseComObject(pConnectPin);
								pConnectPin=null;
                //else Log.WriteFile(Log.LogType.Log,"pin is already connected");
              }
              Marshal.ReleaseComObject( pins[0] );
            }
            else 
            {
              iFetched=0;
              Log.WriteFile(Log.LogType.Log,"no pins?");
              break;
            }
          }
          else iFetched=0;
				}while( iFetched==1 && pinsRendered < maxPinsToRender);
				Marshal.ReleaseComObject(pinEnum);
      }
      return bAllConnected;
    }

    static public void DisconnectOutputPins(IGraphBuilder graphBuilder,IBaseFilter filter)
    {
      IEnumPins pinEnum;
      int hr=filter.EnumPins(out pinEnum);
      if( (hr == 0) && (pinEnum != null) )
      {
        //Log.WriteFile(Log.LogType.Log,"got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo=0;
        do
        {
          // Get the next pin
          //Log.WriteFile(Log.LogType.Log,"  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next( 1, pins, out iFetched );
          if( hr == 0 )
          {
            if (iFetched==1 && pins[0]!=null) 
            {
              //Log.WriteFile(Log.LogType.Log,"  find pin info");
              PinInfo pinInfo = new PinInfo();
              hr=pins[0].QueryPinInfo(out pinInfo);
							if (hr>=0)
							{
								//Marshal.ReleaseComObject(pinInfo.filter);
								Log.WriteFile(Log.LogType.Log,"  got pin#{0}:{1}",iPinNo-1,pinInfo.name);
							}
							else
								Log.WriteFile(Log.LogType.Log,"  got pin:?");
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir==PinDirection.Output)
              {
                //Log.WriteFile(Log.LogType.Log,"  is output");
                IPin pConnectPin=null;
                hr=pins[0].ConnectedTo(out pConnectPin);  
                if (hr==0 && pConnectPin!=null)
                {
                  //Log.WriteFile(Log.LogType.Log,"  pin is connected ");
                  hr=pins[0].Disconnect();
                  if (hr==0) Log.WriteFile(Log.LogType.Log,"  disconnected ok");
                  else 
                  {
                    Log.WriteFile(Log.LogType.Log,true,"  disconnected failed");
                  }
									Marshal.ReleaseComObject(pConnectPin);
									pConnectPin=null;
                }
                //else Log.WriteFile(Log.LogType.Log,"pin is already connected");
              }
              Marshal.ReleaseComObject( pins[0] );
            }
            else 
            {
              iFetched=0;
              Log.WriteFile(Log.LogType.Log,"no pins?");
              break;
            }
          }
          else iFetched=0;
				}while( iFetched==1 );
				Marshal.ReleaseComObject(pinEnum);
      }
    }

    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    static public void SetARMode(IGraphBuilder graphBuilder, AspectRatioMode ARRatioMode)
    {
      int hr;
      IBaseFilter overlay;
      graphBuilder.FindFilterByName("Overlay Mixer2",out overlay);
        
      if (overlay!=null)
      {
        IPin iPin;
        overlay.FindPin("Input0", out iPin);
        if (iPin!=null)
        {
          IMixerPinConfig pMC = iPin as IMixerPinConfig ;
          if (pMC!=null)
          {
            AspectRatioMode mode;
            hr=pMC.SetAspectRatioMode(ARRatioMode);
						hr=pMC.GetAspectRatioMode(out mode);
						//Marshal.ReleaseComObject(pMC);
          }
					Marshal.ReleaseComObject(iPin);
				}
				Marshal.ReleaseComObject(overlay);
      }
        

      IEnumFilters enumFilters;
      hr=graphBuilder.EnumFilters(out enumFilters);
      if (hr>=0 && enumFilters!=null)
      {
        int iFetched;
        enumFilters.Reset();
        IBaseFilter[] pBasefilter = new IBaseFilter[2];
        do
        {
          pBasefilter=null;
          hr=enumFilters.Next(1, pBasefilter,out iFetched);
          if (hr==0 && iFetched==1 &&  pBasefilter[0]!=null)
          {

            IVMRAspectRatioControl pARC = pBasefilter[0] as IVMRAspectRatioControl;
            if (pARC!=null)
            {
              pARC.SetAspectRatioMode(VMRAspectRatioMode.None);
            }
            IVMRAspectRatioControl9 pARC9 = pBasefilter[0] as IVMRAspectRatioControl9;
            if (pARC9!=null)
            {
              pARC9.SetAspectRatioMode(VMRAspectRatioMode.None);
            }

            IEnumPins pinEnum;
            hr = pBasefilter[0].EnumPins(out pinEnum);
            if( (hr == 0) && (pinEnum != null) )
            {
              pinEnum.Reset();
              IPin[] pins = new IPin[1];
              int f;
              do
              {
                // Get the next pin
                hr = pinEnum.Next( 1, pins, out f );
                if(f==1&& hr == 0 && pins[0] != null )
                {
                  IMixerPinConfig pMC = pins[0] as IMixerPinConfig ;
                  if (null!=pMC)
                  {
                    pMC.SetAspectRatioMode(ARRatioMode);
									}
                  Marshal.ReleaseComObject( pins[0] );
                }
              } while( f ==1);
							Marshal.ReleaseComObject(pinEnum);
            }
            Marshal.ReleaseComObject(pBasefilter[0]);
          }
        } while (iFetched == 1 && pBasefilter[0] != null);
				Marshal.ReleaseComObject(enumFilters);
      }
    }

		static bool IsInterlaced(uint x) 
		{
      return ((x) & ((uint)AMInterlace.IsInterlaced)) != 0;
		}
		static bool IsSingleField(uint x) 
		{
      return ((x) & ((uint)AMInterlace.OneFieldPerSample)) != 0;
		}
		static bool  IsField1First(uint x)
		{
      return ((x) & ((uint)AMInterlace.Field1First)) != 0;
		}

    static VMR9SampleFormat ConvertInterlaceFlags(uint dwInterlaceFlags)
		{
			if (IsInterlaced(dwInterlaceFlags)) 
			{
				if (IsSingleField(dwInterlaceFlags)) 
				{
					if (IsField1First(dwInterlaceFlags)) 
					{
						return VMR9SampleFormat.FieldSingleEven;
					}
					else 
					{
						return VMR9SampleFormat.FieldSingleOdd;
					}
				}
				else 
				{
					if (IsField1First(dwInterlaceFlags)) 
					{
						return VMR9SampleFormat.FieldInterleavedEvenFirst;
					}
					else 
					{
						return VMR9SampleFormat.FieldInterleavedOddFirst;
					}
				}
			}
			else 
			{
				return VMR9SampleFormat.ProgressiveFrame;  // Not interlaced.
			}
		}
    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    static public void EnableDeInterlace(IGraphBuilder graphBuilder)
    {
      //not used anymore
    }

    static public IPin FindVideoPort(ref ICaptureGraphBuilder2 captureGraphBuilder ,ref IBaseFilter videoDeviceFilter,ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.VideoPort);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter,PinDirection.Output, cat,new DsGuid( mediaType),false,0,out pPin);
      if (hr>=0 && pPin!=null)
        Log.WriteFile(Log.LogType.Log,"Found videoport pin");
      return pPin;
    }

    static public IPin FindPreviewPin(ref ICaptureGraphBuilder2 captureGraphBuilder ,ref IBaseFilter videoDeviceFilter,ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.Preview);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter, PinDirection.Output,  cat, new DsGuid(mediaType), false, 0, out pPin);
      if (hr>=0 && pPin!=null)
        Log.WriteFile(Log.LogType.Log,"Found preview pin");
      return pPin;
    }

    static public IPin FindCapturePin(ref ICaptureGraphBuilder2 captureGraphBuilder ,ref IBaseFilter videoDeviceFilter,ref Guid mediaType)
    {
      IPin pPin=null;
      DsGuid cat = new DsGuid(PinCategory.Capture);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter,PinDirection.Output, cat,new DsGuid (mediaType),false,0,out pPin);
      if (hr>=0 && pPin!=null)
        Log.WriteFile(Log.LogType.Log,"Found capture pin");
      return pPin;
    }

		static public IBaseFilter GetFilterByName(IGraphBuilder graphBuilder,string name)
		{
			int hr=0;
			IEnumFilters ienumFilt=null;
			IBaseFilter[] foundfilter = new IBaseFilter[2];
			int iFetched=0;
			try
			{
				hr=graphBuilder.EnumFilters(out ienumFilt);
				if (hr==0 && ienumFilt!=null)
				{
					ienumFilt.Reset();
					do
					{
						hr=ienumFilt.Next(1, foundfilter,out iFetched);
						if (hr==0 && iFetched==1)
						{
							FilterInfo filter_infos=new FilterInfo();
							foundfilter[0].QueryFilterInfo(out filter_infos);

							Log.Write("GetFilterByName: {0}, {1}", name, filter_infos.achName);
            
							if (filter_infos.achName.LastIndexOf(name)!=-1)
							{
								Marshal.ReleaseComObject(ienumFilt);ienumFilt=null;
                return foundfilter[0];
							}
              Marshal.ReleaseComObject(foundfilter[0]);
						}
					} while (iFetched==1 && hr==0);
					if (ienumFilt!=null)
						Marshal.ReleaseComObject(ienumFilt);
					ienumFilt=null;
				}
			}
			catch(Exception)
			{
			}
			finally
			{
				if (ienumFilt!=null)
					Marshal.ReleaseComObject(ienumFilt);
			}
			return null;
		}

    static public void RemoveFilters(IGraphBuilder m_graphBuilder)
    {
      int hr;
      if (m_graphBuilder == null) return;
      for (int counter = 0; counter < 100; counter++)
      {
        bool bFound = false;
        IEnumFilters ienumFilt = null;
        try
        {
          hr = m_graphBuilder.EnumFilters(out ienumFilt);
          if (hr == 0)
          {
            int iFetched;
            IBaseFilter[] filter = new IBaseFilter[2]; ;
            ienumFilt.Reset();
            do
            {
              hr = ienumFilt.Next(1,  filter, out iFetched);
              if (hr == 0 && iFetched == 1)
              {
                m_graphBuilder.RemoveFilter(filter[0]);
                int hres = Marshal.ReleaseComObject(filter[0]);
                filter[0] = null;
                bFound = true;
              }
            } while (iFetched == 1 && hr == 0);
            if (ienumFilt != null)
              Marshal.ReleaseComObject(ienumFilt);
            ienumFilt = null;

          }
          if (!bFound) return;
        }
        catch (Exception)
        {
          return;
        }
        finally
        {
          if (ienumFilt != null)
            hr = Marshal.ReleaseComObject(ienumFilt);
        }
      }
    }

    public static IntPtr GetUnmanagedSurface(Microsoft.DirectX.Direct3D.Surface surface)
    {
      return surface.GetObjectByValue(magicConstant);
    }
    public static IntPtr GetUnmanagedDevice(Microsoft.DirectX.Direct3D.Device device)
    {
      return device.GetObjectByValue(magicConstant);
    }
    public static IntPtr GetUnmanagedTexture(Microsoft.DirectX.Direct3D.Texture texture)
    {
      return texture.GetObjectByValue(magicConstant);
    }
    static public void FindFilterByClassID(IGraphBuilder m_graphBuilder, Guid classID, out IBaseFilter filterFound)
    {
      filterFound = null;

      if (m_graphBuilder == null) return;
      IEnumFilters ienumFilt = null;
      try
      {
        int hr = m_graphBuilder.EnumFilters(out ienumFilt);
        if (hr == 0 && ienumFilt != null)
        {
          int iFetched;
          IBaseFilter[] filter = new IBaseFilter[2];
          ienumFilt.Reset();
          do
          {
            hr = ienumFilt.Next(1,  filter, out iFetched);
            if (hr == 0 && iFetched == 1)
            {
              Guid filterGuid;
              filter[0].GetClassID(out filterGuid);
              if (filterGuid == classID)
              {
                filterFound = filter[0];
                return;
              }
              Marshal.ReleaseComObject(filter[0]);
              filter[0] = null;
            }
          } while (iFetched == 1 && hr == 0);
          if (ienumFilt != null)
            Marshal.ReleaseComObject(ienumFilt);
          ienumFilt = null;
        }
      }
      catch (Exception)
      {
      }
      finally
      {
        if (ienumFilt != null)
          Marshal.ReleaseComObject(ienumFilt);
      }
      return;
    }
    public static string GetFriendlyName(IMoniker mon)
    {
      if (mon == null) return String.Empty;
      object bagObj = null;
      IPropertyBag bag = null;
      try
      {
        IErrorLog errorLog = null;
        Guid bagId = typeof(IPropertyBag).GUID;
        mon.BindToStorage(null, null, ref bagId, out bagObj);
        bag = (IPropertyBag)bagObj;
        object val = "";
        int hr = bag.Read("FriendlyName", out val, errorLog);
        if (hr != 0)
          Marshal.ThrowExceptionForHR(hr);
        string ret = val as string;
        if ((ret == null) || (ret.Length < 1))
          throw new NotImplementedException("Device FriendlyName");
        return ret;
      }
      catch (Exception)
      {
        return null;
      }
      finally
      {
        bag = null;
        if (bagObj != null)
          Marshal.ReleaseComObject(bagObj); bagObj = null;
      }
    }
    static public IPin FindPin(IBaseFilter filter, PinDirection dir, string strPinName)
    {
      int hr = 0;

      IEnumPins pinEnum;
      hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int f;
        do
        {
          // Get the next pin
          hr = pinEnum.Next(1, pins, out f);
          if ((hr == 0) && (pins[0] != null))
          {
            PinDirection pinDir;
            pins[0].QueryDirection(out pinDir);
            if (pinDir == dir)
            {
              PinInfo info;
              pins[0].QueryPinInfo(out info);
              //Marshal.ReleaseComObject(info.filter);
              if (String.Compare(info.name, strPinName) == 0)
              {
                Marshal.ReleaseComObject(pinEnum);
                return pins[0];
              }
            }
            Marshal.ReleaseComObject(pins[0]);
          }
        }
        while (hr == 0);
        Marshal.ReleaseComObject(pinEnum);
      }
      return null;
    }
    static public void RemoveDownStreamFilters(IGraphBuilder graphBuilder, IBaseFilter fromFilter, bool remove)
    {
      IEnumPins enumPins;
      fromFilter.EnumPins(out enumPins);
      if (enumPins == null) return;
      IPin[] pins = new IPin[2];
      int fetched;
      while (enumPins.Next(1, pins, out fetched)==0)
      {
        if (fetched != 1) break;
        PinDirection dir;
        pins[0].QueryDirection(out dir);
        if (dir != PinDirection.Output)
        {
          Marshal.ReleaseComObject(pins[0]);
          continue;
        }
        IPin pinConnected;
        pins[0].ConnectedTo(out pinConnected);
        if (pinConnected==null)
        {
          Marshal.ReleaseComObject(pins[0]);
          continue;
        }
        PinInfo info;
        pinConnected.QueryPinInfo(out info);
        if (info.filter != null)
        {
          RemoveDownStreamFilters(graphBuilder, info.filter, true);
        }
        Marshal.ReleaseComObject(pins[0]);
      }
      if (remove)
        graphBuilder.RemoveFilter(fromFilter);
      Marshal.ReleaseComObject(enumPins);
    }
    static public void RemoveDownStreamFilters(IGraphBuilder graphBuilder, IPin pin)
    {
      IPin pinConnected;
      pin.ConnectedTo(out pinConnected);
      if (pinConnected == null)
      {
        return;
      }
      PinInfo info;
      pinConnected.QueryPinInfo(out info);
      if (info.filter != null)
      {
        RemoveDownStreamFilters(graphBuilder, info.filter, true);
      }
    }
	}
}
