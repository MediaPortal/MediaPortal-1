using System;
using System.IO;
using DShowNET;

namespace DirectX.Capture
{
	/// <summary>
	/// 
	/// </summary>
	public class FilterPersist
  {
    ICaptureGraphBuilder2		 m_captureGraphBuilder;
    IBaseFilter              m_videoCompressorFilter;
    IBaseFilter              m_audioCompressorFilter;

		public FilterPersist(ICaptureGraphBuilder2		captureGraphBuilder,
                          IBaseFilter              videoCompressorFilter,
                          IBaseFilter              audioCompressorFilter)
		{
      m_captureGraphBuilder=captureGraphBuilder;
      m_videoCompressorFilter=videoCompressorFilter;
      m_audioCompressorFilter=audioCompressorFilter;
		}


    public void LoadSettings(int ID)
    {
      DirectShowUtil.DebugWrite("Load settings card:{0}",ID);
      try
      {
        PropertyPageCollection propertyPages = new PropertyPageCollection( m_captureGraphBuilder, m_videoCompressorFilter, m_audioCompressorFilter);

        foreach ( PropertyPage p in propertyPages )
        {
          if ( p.SupportsPersisting )
          {
            string strFName=String.Format(@"filters\card{0}_{1}.dat",ID, p.Name);
              using (FileStream fs = new FileStream(strFName, FileMode.Open, FileAccess.Read))
              {
                byte[] byData = new byte[fs.Length];
                fs.Read(byData,0,(int)fs.Length);
                p.State=byData;
              }
            }
          }
      }
      catch(Exception ex)
      {
        DirectShowUtil.DebugWrite("ex:{0} {1} {2}", ex.Source, ex.StackTrace, ex.Message);
      }
    }


    public void SaveSettings(int ID)
    {
      try
      {
        DirectShowUtil.DebugWrite("Save settings card:{0}",ID);
        System.IO.Directory.CreateDirectory("filters");
        PropertyPageCollection  propertyPages = new PropertyPageCollection( m_captureGraphBuilder, m_videoCompressorFilter, m_audioCompressorFilter);
        foreach ( PropertyPage p in propertyPages )
        {
          if ( p.SupportsPersisting )
          {
            string strFName=String.Format(@"filters\card{0}_{1}.dat",ID,p.Name);
              using (FileStream fs = new FileStream(strFName, FileMode.Create, FileAccess.Write))
              {
                byte[] byData = p.State;
                fs.Write(byData,0,byData.Length);
              }
          }
        }
      }
      catch(Exception ex)
      {
        DirectShowUtil.DebugWrite("ex:{0} {1} {2}", ex.Source, ex.StackTrace, ex.Message);
      }
    }
	}
}
