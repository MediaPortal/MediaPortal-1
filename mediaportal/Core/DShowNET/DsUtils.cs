/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DShowNET
{

		[ComVisible(false)]
	public class DsUtils
	{

			const int magicConstant = -759872593;

      static public void FindFilterByClassID(IGraphBuilder m_graphBuilder, Guid classID, out IBaseFilter filterFound)
      {
        filterFound=null;
        
        if (m_graphBuilder==null) return;
				IEnumFilters ienumFilt=null;
				try
				{
					int hr=m_graphBuilder.EnumFilters(out ienumFilt);
					if (hr==0 && ienumFilt!=null)
					{
						uint iFetched;
						IBaseFilter filter;
						ienumFilt.Reset();
						do
						{
							hr=ienumFilt.Next(1,out filter,out iFetched); 
							if (hr==0 && iFetched==1)
							{
								Guid filterGuid;
								filter.GetClassID(out filterGuid);
								Marshal.ReleaseComObject(filter);
								filter=null;
								if (filterGuid == classID)
								{
									filterFound=filter;
									return;
								}
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
        return ; 
      }
		static public void RemoveFilters(IGraphBuilder m_graphBuilder)
    {
			int hr;
      if (m_graphBuilder==null) return;
			for (int counter=0; counter < 100; counter++)
			{
				bool bFound=false;
				IEnumFilters ienumFilt=null;
				try
				{
					hr=m_graphBuilder.EnumFilters(out ienumFilt);
					if (hr==0)
					{
						uint iFetched;
						IBaseFilter filter;
						ienumFilt.Reset();
						do
						{
							hr=ienumFilt.Next(1,out filter,out iFetched); 
							if (hr==0 && iFetched==1)
							{
								m_graphBuilder.RemoveFilter(filter);
								int hres=Marshal.ReleaseComObject(filter);
								filter=null;
								bFound=true;
							}
						} while (iFetched==1 && hr==0);
						if (ienumFilt!=null)
							Marshal.ReleaseComObject(ienumFilt);
						ienumFilt=null;

					}
					if (!bFound) return;
				}
				catch(Exception)
				{
					return;
				}
				finally
				{
					if (ienumFilt!=null)
						hr=Marshal.ReleaseComObject(ienumFilt);
				}
			}
		}
      static public void DumpFilters(IGraphBuilder m_graphBuilder)
      {
        if (m_graphBuilder==null) return;
        Filters filters = new Filters();
        
				IEnumFilters ienumFilt=null;
        try
        {
          int iFilter=0;
          int hr=m_graphBuilder.EnumFilters(out ienumFilt);
          if (hr==0)
          {
            uint iFetched;
            IBaseFilter filter;
            ienumFilt.Reset();
            do
            {
              hr=ienumFilt.Next(1,out filter,out iFetched); 
              if (hr==0 && iFetched==1)
              {
                FilterInfo info=new FilterInfo();
                filter.QueryFilterInfo(info);
                DirectShowUtil.DebugWrite("Filter #{0} :{1}", iFilter,info.achName);
                iFilter++;
              }
							if (filter!=null)
								Marshal.ReleaseComObject(filter);
							filter=null;
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
      }


		/// <summary>
		/// This function resets the crossbar filter(s)
		/// It makes sure that the tuner is routed to the video/audio capture device
		/// and that the video/audio outputs of the crossbars are connected
		/// </summary>
		/// <param name="graphbuilder">IGraphBuilder </param>
			/// <param name="m_captureGraphBuilder">ICaptureGraphBuilder2 </param>
			/// <param name="captureFilter">IBaseFilter containing the capture device filter</param>
		static public void ResetCrossbar(IGraphBuilder graphbuilder, ICaptureGraphBuilder2 m_captureGraphBuilder,  IBaseFilter captureFilter)
    {
      if (graphbuilder==null) return;
      if (m_captureGraphBuilder==null) return;
      if (captureFilter==null) return;
			FixCrossbarRouting(graphbuilder, m_captureGraphBuilder,  captureFilter, true, false, false, false,false,false);
		}

		/// <summary>
		/// FixCrossbarRouting() will search and configure all crossbar filters in the graph
		/// It will
		/// </summary>
		/// <param name="graphbuilder">IGraphBuilder</param>
		/// <param name="m_captureGraphBuilder">ICaptureGraphBuilder2</param>
		/// <param name="captureFilter">IBaseFilter containing the capture device filter</param>
		/// <param name="bTunerIn">configure the crossbars to use the tuner input as source</param>
		/// <param name="useCVBS1">configure the crossbars to use the 1st CVBS input as source</param>
		/// <param name="useCVBS2">configure the crossbars to use the 2nd CVBS input as source</param>
		/// <param name="useSVHS">configure the crossbars to use the SVHS input as source</param>
		/// <param name="logActions">true : log all actions in the logfile
		///                          false: dont log
		/// </param>
		static public void FixCrossbarRouting(IGraphBuilder graphbuilder, ICaptureGraphBuilder2 m_captureGraphBuilder,  IBaseFilter captureFilter, bool useTuner, bool useCVBS1, bool useCVBS2, bool useSVHS, bool useRgb, bool logActions)
    {
      if (graphbuilder==null) return;
      if (m_captureGraphBuilder==null) return;
      if (captureFilter==null) return;
			bool CvbsWanted= (useCVBS1 || useCVBS2);
			int iCVBSVideo=0;
			int iCVBSAudio=0;
			int iSVHSVideo=0;
			int iRgbVideo=0;

			if (logActions) 
				DirectShowUtil.DebugWrite("FixCrossbarRouting: use tuner:{0} use cvbs#1:{1} use cvbs#2:{2} use svhs:{3} use rgb:{4}",useTuner, useCVBS1, useCVBS2, useSVHS,useRgb);
			try
			{
				int icurrentCrossbar=0;
				
				//start search upward from the video capture filter
				IBaseFilter searchfilter=captureFilter ;
				while (true)
				{
					// Find next crossbar
					int  hr=0;
					Guid cat;
					Guid iid;
					object o=null;
					cat = FindDirection.UpstreamOnly;
					iid = typeof(IAMCrossbar).GUID;
					if (logActions) DirectShowUtil.DebugWrite(" Find crossbar:#{0}", 1+icurrentCrossbar);
					hr=m_captureGraphBuilder.FindInterface(new Guid[1]{cat},null,searchfilter, ref iid, out o);
					if (hr ==0 && o != null)
					{
						// we found something, check if it is a crossbar
						IAMCrossbar crossbar = o as IAMCrossbar;

						// next loop, use this filter as start for searching for next crossbar
						searchfilter=o as IBaseFilter;
						if (crossbar!=null)
						{
							// new crossbar found
							icurrentCrossbar++;
							if (logActions) 
									DirectShowUtil.DebugWrite("  crossbar found:{0}",icurrentCrossbar);

							// get the number of input & output pins of the crossbar
							int iOutputPinCount, iInputPinCount;
							crossbar.get_PinCounts(out iOutputPinCount, out iInputPinCount);
							if (logActions) 
								DirectShowUtil.DebugWrite("    crossbar has {0} inputs and {1} outputs",iInputPinCount,iOutputPinCount);
            
							int                   iPinIndexRelated;		// pin related (routed) with this output pin
							int                   iPinIndexRelatedIn; // pin related (routed) with this input pin
							PhysicalConnectorType PhysicalTypeOut;		// type of output pin
							PhysicalConnectorType PhysicalTypeIn;			// type of input pin
							iCVBSVideo=0;
              iCVBSAudio=0;

							//for all output pins of the crossbar
							for (int iOut=0; iOut < iOutputPinCount; ++iOut)
							{
								// get the information about this output pin
								crossbar.get_CrossbarPinInfo(false,iOut,out iPinIndexRelated, out PhysicalTypeOut);
              
								// for all input pins of the crossbar
								for (int iIn=0; iIn < iInputPinCount; iIn++)
								{
									// check if we can make a connection between the input pin -> output pin
									hr=crossbar.CanRoute(iOut, iIn);
									if (hr==0)
									{
										// yes thats possible, now get the information of the input pin
										crossbar.get_CrossbarPinInfo(true,iIn,out iPinIndexRelatedIn, out PhysicalTypeIn);
										if (logActions) 
											DirectShowUtil.DebugWrite("     check:in#{0}->out#{1} / {2} -> {3}",iIn,iOut,PhysicalTypeIn.ToString(), PhysicalTypeOut.ToString());
										
										
										// boolean indicating if current input pin should be connected to the current output pin
										bool bRoute=false; 

										// Check video input options
										// if the input pin is a Tuner Input and we want to use the tuner, then connect this
										if (useTuner && PhysicalTypeIn==PhysicalConnectorType.Video_Tuner )  bRoute=true;

										// if the input pin is a CVBS input and we want to use CVBS then
										if (CvbsWanted    && PhysicalTypeIn==PhysicalConnectorType.Video_Composite)  
										{
											// if this is the first CVBS input then connect
											iCVBSVideo++;
											if (iCVBSVideo==1 && CvbsWanted) bRoute=true;
											
											// if this is the 2nd CVBS input and we want to use the 2nd CVBS input then connect
											if (iCVBSVideo==2 && useCVBS2) bRoute=true;
										}

										// if the input pin is a SVHS input and we want to use SVHS then connect
										if (useSVHS && PhysicalTypeIn==PhysicalConnectorType.Video_SVideo)
										{
											// make sure we only use the 1st SVHS input of the crossbar
											// since the PVR150MCE crossbar has 2 SVHS inputs
											iSVHSVideo++;
											if (iSVHSVideo==1) bRoute=true;
										}

										// if the input pin is a RGB input and we want to use RGB then connect
										if (useRgb && PhysicalTypeIn==PhysicalConnectorType.Video_RGB)
										{
											// make sure we only use the 1st SVHS input of the crossbar
											// since the PVR150MCE crossbar has 2 SVHS inputs
											iRgbVideo++;
											if (iRgbVideo==1) bRoute=true;
										}
                    
										// Check audio input options

										// if this is the audio tuner input and we want to use the tuner, then connect
										if (useTuner)
										{
											if (PhysicalTypeIn==PhysicalConnectorType.Audio_Tuner )  bRoute=true;
										}
										else
										{
											// if this is the audio line input
											if ( /*PhysicalTypeIn==PhysicalConnectorType.Audio_AUX||*/
												   PhysicalTypeIn==PhysicalConnectorType.Audio_Line ||
												   PhysicalTypeIn==PhysicalConnectorType.Audio_AudioDecoder) 
											{
												// if this is the first audio input then connect
                        iCVBSAudio++;
                        if (CvbsWanted && iCVBSAudio==1) bRoute=true;
                        
												// if this is the 2nd audio input and we want to use the 2nd CVBS input then connect
												if (iCVBSAudio==2 && useCVBS2) bRoute=true;

												// if we want to use SVHS then connect
												if (useSVHS) bRoute=true;

												// if we want to use RGB then connect
												if (useRgb) bRoute=true;
											}
										}

										//should current input pin be connected to current output pin?
										if ( bRoute )
										{
											//yes, then connect
											if (logActions) DirectShowUtil.DebugWrite("     connect");
											hr=crossbar.Route(iOut,iIn);
											if (logActions) 
											{
												if (hr!=0) DirectShowUtil.DebugWrite("    connect FAILED");
												else DirectShowUtil.DebugWrite("    connect success");
											}
										}
									}//if (hr==0)
								}//for (int iIn=0; iIn < iInputPinCount; iIn++)
							}//for (int iOut=0; iOut < iOutputPinCount; ++iOut)
						}//if (crossbar!=null)
					}//if (hr ==0 && o != null)
					else
					{
						if (logActions) DirectShowUtil.DebugWrite("  no more crossbars.:0x{0:X}",hr);
						break;
					}
				}//while (true)
				if (logActions) DirectShowUtil.DebugWrite("crossbar routing done");
			}
			catch (Exception ex)
			{
				DirectShowUtil.DebugWrite("crossbar routing exception:{0}",ex.ToString());
			}
		}

			/// <summary>
			/// FixCrossbarRouting() will search and configure all crossbar filters in the graph
			/// It will
			/// </summary>
			/// <param name="graphbuilder">IGraphBuilder</param>
			/// <param name="m_captureGraphBuilder">ICaptureGraphBuilder2</param>
			/// <param name="captureFilter">IBaseFilter containing the capture device filter</param>
			/// <param name="bTunerIn">configure the crossbars to use the tuner input as source</param>
			/// <param name="useCVBS1">configure the crossbars to use the 1st CVBS input as source</param>
			/// <param name="useCVBS2">configure the crossbars to use the 2nd CVBS input as source</param>
			/// <param name="useSVHS">configure the crossbars to use the SVHS input as source</param>
			/// <param name="logActions">true : log all actions in the logfile
			///                          false: dont log
			/// </param>
			static public void FixCrossbarRoutingEx(IGraphBuilder graphbuilder, ICaptureGraphBuilder2 m_captureGraphBuilder,  IBaseFilter captureFilter, bool useTuner, bool useCVBS1, bool useCVBS2, bool useSVHS, bool useRgb,string cardName)
			{
				if (graphbuilder==null) return;
				if (m_captureGraphBuilder==null) return;
				if (captureFilter==null) return;
				bool CvbsWanted= (useCVBS1 || useCVBS2);
				int iCVBSVideo=0;
				int iCVBSAudio=0;
				int iSVHSVideo=0;
				int iRGBVideo=0;
			
				int audioCVBS1=1;
				int audioCVBS2=2;
				int audioSVHS=1;
				int audioRgb=1;
				int videoCVBS1=1;
				int videoCVBS2=2;
				int videoSVHS=1;
				int videoRgb=1;

				string filename=String.Format(@"database\card_{0}.xml", cardName);
				using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(filename))
				{
					audioCVBS1 = 1+xmlreader.GetValueAsInt("mapping", "audio1", 0);
					audioCVBS2 = 1+xmlreader.GetValueAsInt("mapping", "audio2", 1);
					audioSVHS  = 1+xmlreader.GetValueAsInt("mapping", "audio3", 0);
					audioRgb   = 1+xmlreader.GetValueAsInt("mapping", "audio4", 0);

				
					videoCVBS1 = 1+xmlreader.GetValueAsInt("mapping", "video1", 0);
					videoCVBS2 = 1+xmlreader.GetValueAsInt("mapping", "video2", 1);
					videoSVHS  = 1+xmlreader.GetValueAsInt("mapping", "video3", 0);
					videoRgb   = 1+xmlreader.GetValueAsInt("mapping", "video4", 0);
				}

				DirectShowUtil.DebugWrite("FixCrossbarRouting: use tuner:{0} use cvbs#1:{1} use cvbs#2:{2} use svhs:{3} use rgb:{4}",useTuner, useCVBS1, useCVBS2, useSVHS, useRgb);
				try
				{
					int icurrentCrossbar=0;
				
					//start search upward from the video capture filter
					IBaseFilter searchfilter=captureFilter ;
					while (true)
					{
						// Find next crossbar
						int  hr=0;
						Guid cat;
						Guid iid;
						object o=null;
						cat = FindDirection.UpstreamOnly;
						iid = typeof(IAMCrossbar).GUID;
						DirectShowUtil.DebugWrite(" Find crossbar:#{0}", 1+icurrentCrossbar);
						hr=m_captureGraphBuilder.FindInterface(new Guid[1]{cat},null,searchfilter, ref iid, out o);
						if (hr ==0 && o != null)
						{
							// we found something, check if it is a crossbar
							IAMCrossbar crossbar = o as IAMCrossbar;

							// next loop, use this filter as start for searching for next crossbar
							searchfilter=o as IBaseFilter;
							if (crossbar!=null)
							{
								// new crossbar found
								icurrentCrossbar++;
								DirectShowUtil.DebugWrite("  crossbar found:{0}",icurrentCrossbar);

								// get the number of input & output pins of the crossbar
								int iOutputPinCount, iInputPinCount;
								crossbar.get_PinCounts(out iOutputPinCount, out iInputPinCount);
								DirectShowUtil.DebugWrite("    crossbar has {0} inputs and {1} outputs",iInputPinCount,iOutputPinCount);
            
								int                   iPinIndexRelated;		// pin related (routed) with this output pin
								int                   iPinIndexRelatedIn; // pin related (routed) with this input pin
								PhysicalConnectorType PhysicalTypeOut;		// type of output pin
								PhysicalConnectorType PhysicalTypeIn;			// type of input pin
								iCVBSVideo=0;
								iCVBSAudio=0;

								//for all output pins of the crossbar
								for (int iOut=0; iOut < iOutputPinCount; ++iOut)
								{
									// get the information about this output pin
									crossbar.get_CrossbarPinInfo(false,iOut,out iPinIndexRelated, out PhysicalTypeOut);
              
									// for all input pins of the crossbar
									for (int iIn=0; iIn < iInputPinCount; iIn++)
									{
										// check if we can make a connection between the input pin -> output pin
										hr=crossbar.CanRoute(iOut, iIn);
										if (hr==0)
										{
											// yes thats possible, now get the information of the input pin
											crossbar.get_CrossbarPinInfo(true,iIn,out iPinIndexRelatedIn, out PhysicalTypeIn);
											DirectShowUtil.DebugWrite("     check:in#{0}->out#{1} / {2} -> {3}",iIn,iOut,PhysicalTypeIn.ToString(), PhysicalTypeOut.ToString());
										
										
											// boolean indicating if current input pin should be connected to the current output pin
											bool bRoute=false; 

											// Check video input options
											// if the input pin is a Tuner Input and we want to use the tuner, then connect this
											if (useTuner && PhysicalTypeIn==PhysicalConnectorType.Video_Tuner )  bRoute=true;

											// if the input pin is a CVBS input and we want to use CVBS then
											if (CvbsWanted && PhysicalTypeIn==PhysicalConnectorType.Video_Composite)  
											{
												iCVBSVideo++;
												if (useCVBS1 && iCVBSVideo == 1) bRoute=true;
												if (useCVBS1 && iCVBSVideo == videoCVBS1) bRoute=true;
												if (useCVBS2 && iCVBSVideo == videoCVBS2) bRoute=true;
											}

											// if the input pin is a SVHS input and we want to use SVHS then connect
											if (useSVHS && PhysicalTypeIn==PhysicalConnectorType.Video_SVideo)
											{
												iSVHSVideo++;
												if (iSVHSVideo==1) bRoute=true;
												if (iSVHSVideo==videoSVHS) bRoute=true;
											}

											// if the input pin is a RGB input and we want to use RGB then connect
											if (useRgb && PhysicalTypeIn==PhysicalConnectorType.Video_RGB)
											{
												iRGBVideo++;
												if (iRGBVideo==1) bRoute=true;
												if (iRGBVideo==videoRgb) bRoute=true;
											}
                    
											// Check audio input options
											// if this is the audio tuner input and we want to use the tuner, then connect
											if (useTuner)
											{
												if (PhysicalTypeIn==PhysicalConnectorType.Audio_Tuner )  bRoute=true;
											}
											else
											{
												// if this is the audio line input
												if ( /*PhysicalTypeIn==PhysicalConnectorType.Audio_AUX||*/
													PhysicalTypeIn==PhysicalConnectorType.Audio_Line ||
													PhysicalTypeIn==PhysicalConnectorType.Audio_AudioDecoder) 
												{
													// if this is the first audio input then connect
													iCVBSAudio++;
													if (useCVBS1 && iCVBSAudio==1) bRoute=true;
													if (useCVBS2 && iCVBSAudio==1) bRoute=true;
													if (useSVHS  && iCVBSAudio==1) bRoute=true;

													if (useCVBS1 && iCVBSAudio==audioCVBS1) bRoute=true;
													if (useCVBS2 && iCVBSAudio==audioCVBS2) bRoute=true;
													if (useSVHS  && iCVBSAudio==audioSVHS) bRoute=true;
													if (useRgb  && iCVBSAudio==audioRgb) bRoute=true;
												}
											}

											//should current input pin be connected to current output pin?
											if ( bRoute )
											{
												//yes, then connect
												DirectShowUtil.DebugWrite("     connect");
												hr=crossbar.Route(iOut,iIn);
												if (hr!=0) DirectShowUtil.DebugWrite("    connect FAILED");
												else DirectShowUtil.DebugWrite("    connect success");
											}
										}//if (hr==0)
									}//for (int iIn=0; iIn < iInputPinCount; iIn++)
								}//for (int iOut=0; iOut < iOutputPinCount; ++iOut)
							}//if (crossbar!=null)
						}//if (hr ==0 && o != null)
						else
						{
							DirectShowUtil.DebugWrite("  no more crossbars.:0x{0:X}",hr);
							break;
						}
					}//while (true)
					DirectShowUtil.DebugWrite("crossbar routing done");
				}
				catch (Exception ex)
				{
					DirectShowUtil.DebugWrite("crossbar routing exception:{0}",ex.ToString());
				}
			}



		public static bool IsCorrectDirectXVersion()
		{
			return File.Exists( Path.Combine( Environment.SystemDirectory, @"dpnhpast.dll" ) );
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
    public static string GetFriendlyName(System.Runtime.InteropServices.ComTypes.IMoniker mon)
			{
        if (mon==null) return String.Empty;
				object bagObj = null;
				DShowNET.Device.IPropertyBag bag = null;
				try 
				{
					Guid bagId = typeof( DShowNET.Device.IPropertyBag ).GUID;
					mon.BindToStorage( null, null, ref bagId, out bagObj );
					bag = (DShowNET.Device.IPropertyBag) bagObj;
					object val = "";
					int hr = bag.Read( "FriendlyName", ref val, IntPtr.Zero );
					if( hr != 0 )
						Marshal.ThrowExceptionForHR( hr );
					string ret = val as string;
					if( (ret == null) || (ret.Length < 1) )
						throw new NotImplementedException( "Device FriendlyName" );
					return ret;
				}
				catch( Exception )
				{
					return null;
				}
				finally
				{
					bag = null;
					if( bagObj != null )
						Marshal.ReleaseComObject( bagObj ); bagObj = null;
				}
			}
		public static bool ShowCapPinDialog( ICaptureGraphBuilder2 bld, IBaseFilter flt, IntPtr hwnd )
    {
      if (bld==null) return false;
      if (flt==null) return false;
			int hr;
			object comObj = null;
			ISpecifyPropertyPages	spec = null;
			DsCAUUID cauuid = new DsCAUUID();

			try {
				Guid cat  = PinCategory.Capture;
				Guid iid = typeof(IAMStreamConfig).GUID;
        hr = bld.FindInterface( new Guid[1]{cat}, null, flt, ref iid, out comObj );
				if( hr != 0 )
				{
						return false;
				}
				spec = comObj as ISpecifyPropertyPages;
				if( spec == null )
					return false;

				hr = spec.GetPages( out cauuid );
				hr = OleCreatePropertyFrame( hwnd, 30, 30, null, 1,
						ref comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero );
				return true;
			}
			catch( Exception ee )
			{
				Trace.WriteLine( "!Ds.NET: ShowCapPinDialog " + ee.Message );
				return false;
			}
			finally
			{
				if( cauuid.pElems != IntPtr.Zero )
					Marshal.FreeCoTaskMem( cauuid.pElems );
					
				spec = null;
				if( comObj != null )
					Marshal.ReleaseComObject( comObj ); comObj = null;
			}
		}

		public static bool ShowTunerPinDialog( ICaptureGraphBuilder2 bld, IBaseFilter flt, IntPtr hwnd )
    {
      if (bld==null) return false;
      if (flt==null) return false;

			int hr;
			object comObj = null;
			ISpecifyPropertyPages	spec = null;
			DsCAUUID cauuid = new DsCAUUID();

			try {
				Guid cat  = PinCategory.Capture;
				Guid iid = typeof(IAMTVTuner).GUID;
        hr = bld.FindInterface( new Guid[1]{cat}, null, flt, ref iid, out comObj );
				if( hr != 0 )
				{
						return false;
				}
				spec = comObj as ISpecifyPropertyPages;
				if( spec == null )
					return false;

				hr = spec.GetPages( out cauuid );
				hr = OleCreatePropertyFrame( hwnd, 30, 30, null, 1,
						ref comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero );
				return true;
			}
			catch( Exception ee )
			{
				Trace.WriteLine( "!Ds.NET: ShowCapPinDialog " + ee.Message );
				return false;
			}
			finally
			{
				if( cauuid.pElems != IntPtr.Zero )
					Marshal.FreeCoTaskMem( cauuid.pElems );
					
				spec = null;
				if( comObj != null )
					Marshal.ReleaseComObject( comObj ); comObj = null;
			}
		}


		// from 'DShowUtil.cpp'
		static public int GetPin( IBaseFilter filter, PinDirection dirrequired, int num, out IPin ppPin )
    {
      ppPin = null;
      if (filter==null) return -1;
			int hr;
			IEnumPins pinEnum=null;
			hr = filter.EnumPins( out pinEnum );
			if( (hr < 0) || (pinEnum == null) )
				return hr;

			IPin[] pins = new IPin[1];
			int f;
			PinDirection dir;
			do
			{
				IPin[] pinArray = new IPin[1];
				pinEnum.Next(1, pinArray, out f);
				if( (hr != 0) || (f<=0) )
					break;
				dir = (PinDirection) 3;
				hr = pinArray[0].QueryDirection( out dir );
				if( (hr == 0) && (dir == dirrequired) )
				{
					if( num == 0 )
					{
						ppPin = pinArray[0];
						pinArray[0] = null;
						break;
					}
					num--;
				}
				if (pinArray[0]!=null)
					Marshal.ReleaseComObject( pinArray[0] ); 
				pinArray[0] = null;
			}
			while( hr == 0 );

			Marshal.ReleaseComObject( pinEnum ); pinEnum = null;
			return hr;
		}

			public static int GetUnconnectedPin( IBaseFilter filter, PinDirection dirrequired,  out IPin ppPin )
      {
				ppPin = null;
        if (filter==null) return -1;
        int hr;
				IEnumPins pinEnum;
				hr = filter.EnumPins( out pinEnum );
				if( (hr < 0) || (pinEnum == null) )
					return hr;

				IPin[] pins = new IPin[1];
				int f;
				PinDirection dir;
				do
				{
					//IPin[] pinArray = new IPin[1];
					pinEnum.Next(1, pins, out f);
					//pins = new IPin[f];
					
					if( (hr != 0) || (pins[0] == null) )
						break;
					dir = (PinDirection) 3;
					hr = pins[0].QueryDirection( out dir );
					if( (hr == 0) && (dir == dirrequired) )
					{
						IPin tmp;
						
						hr = pins[0].ConnectedTo(out tmp);
						if (tmp!=null) Marshal.ReleaseComObject(tmp);
						tmp = null;
						if (hr != 0) 
						{
							hr = 0;
							ppPin = pins[0];
							break;
						}
					}
					if (pins[0]!=null)
						Marshal.ReleaseComObject( pins[0] ); 
					pins[0] = null;
				}
				while( true );

				Marshal.ReleaseComObject( pinEnum ); pinEnum = null;
				return hr;
			}

		public static int RenderFileToVMR9(IGraphBuilder pGB, string wFileName, 
			IBaseFilter pRenderer)
		{
      if (pGB==null) return  0;
      if (wFileName==null) return 0;
      if (pRenderer==null) return 0;
      return RenderFileToVMR9(pGB, wFileName, pRenderer, true);
		}

		public static int RenderFileToVMR9(IGraphBuilder pGB, string wFileName, 
										   IBaseFilter pRenderer, bool bRenderAudio)
		{

      if (pGB==null) return 0;
      if (wFileName==null) return 0;
      if (pRenderer==null) return 0;
			int hr;
			try 
			{
				hr = pGB.RenderFile(wFileName, null);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			
			//DsROT.AddGraphToRot(pGB, out hr);

			return 0;


		}


		[DllImport("olepro32.dll", CharSet=CharSet.Unicode, ExactSpelling=true) ]
		private static extern int OleCreatePropertyFrame( IntPtr hwndOwner, int x, int y,
			string lpszCaption, int cObjects,
			[In, MarshalAs(UnmanagedType.Interface)] ref object ppUnk,
			int cPages,	IntPtr pPageClsID, int lcid, int dwReserved, IntPtr pvReserved );


	}


// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct DsPOINT		// POINT
{
	public int		X;
	public int		Y;
}
	public struct DsPOINTClass		// POINT
	{
		public int		X;
		public int		Y;
	}

// ---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public class DsRECT		// RECT
{
	public int		Left;
	public int		Top;
	public int		Right;
	public int		Bottom;
}


//---------------------------------------------------------------------------------------

	[StructLayout(LayoutKind.Sequential, Pack=2), ComVisible(false)]
public struct DsBITMAPINFOHEADER
	{
	public int      Size;
	public int      Width;
	public int      Height;
	public short    Planes;
	public short    BitCount;
	public int      Compression;
	public int      ImageSize;
	public int      XPelsPerMeter;
	public int      YPelsPerMeter;
	public int      ClrUsed;
	public int      ClrImportant;
	}




// ---------------------------------------------------------------------------------------

		[ComVisible(false)]
	public class DsROT
	{
		public static bool AddGraphToRot( object graph, out int cookie )
    {
      cookie = 0;
      if (graph==null) return false;
			int hr = 0;
      System.Runtime.InteropServices.ComTypes.IRunningObjectTable rot = null;
			System.Runtime.InteropServices.ComTypes.IMoniker mk = null;
			try {
				hr = GetRunningObjectTable( 0, out rot );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				int id = GetCurrentProcessId();
				IntPtr iuPtr = Marshal.GetIUnknownForObject( graph );
				int iuInt = (int) iuPtr;
				Marshal.Release( iuPtr );
				string item = string.Format( "FilterGraph {0} pid {1}", iuInt.ToString("x8"), id.ToString("x8") );
				hr = CreateItemMoniker( "!", item, out mk );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );
				
				cookie =rot.Register( ROTFLAGS_REGISTRATIONKEEPSALIVE, graph, mk);
				return true;
			}
			catch( Exception )
			{
				return false;
			}
			finally
			{
				if( mk != null )
					Marshal.ReleaseComObject( mk ); mk = null;
				if( rot != null )
					Marshal.ReleaseComObject( rot ); rot = null;
			}
		}

		public static bool RemoveGraphFromRot( ref int cookie )
		{
			System.Runtime.InteropServices.ComTypes.IRunningObjectTable rot = null;
			try {
				int hr = GetRunningObjectTable( 0, out rot );
				if( hr < 0 )
					Marshal.ThrowExceptionForHR( hr );

				rot.Revoke( cookie );
				cookie = 0;
				return true;
			}
			catch( Exception )
			{
				return false;
			}
			finally
			{
				if( rot != null )
					Marshal.ReleaseComObject( rot ); rot = null;
			}
		}

		private const int ROTFLAGS_REGISTRATIONKEEPSALIVE	= 1;

		[DllImport("ole32.dll", ExactSpelling=true) ]
		private static extern int GetRunningObjectTable( int r,
      out System.Runtime.InteropServices.ComTypes.IRunningObjectTable pprot);

		[DllImport("ole32.dll", CharSet=CharSet.Unicode, ExactSpelling=true) ]
		private static extern int CreateItemMoniker( string delim,
      string item, out System.Runtime.InteropServices.ComTypes.IMoniker ppmk);

		[DllImport("kernel32.dll", ExactSpelling=true) ]
		private static extern int GetCurrentProcessId();
	}





// ---------------------------------- ocidl.idl ------------------------------------------------

	[ComVisible(true), ComImport,
	Guid("B196B28B-BAB4-101A-B69C-00AA00341D07"),
	InterfaceType( ComInterfaceType.InterfaceIsIUnknown )]
public interface ISpecifyPropertyPages
{
		[PreserveSig]
	int GetPages( out DsCAUUID pPages );
}

	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
public struct DsCAUUID		// CAUUID
{
	public int		cElems;
	public IntPtr	pElems;
}

// ---------------------------------------------------------------------------------------


	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class DsOptInt64
	{
		public DsOptInt64( long Value )
		{
			this.Value = Value;
		}
		public long		Value;
	}


	[StructLayout(LayoutKind.Sequential), ComVisible(false)]
	public class DsOptIntPtr
	{
		public IntPtr	Pointer;
	}



} // namespace DShowNET
