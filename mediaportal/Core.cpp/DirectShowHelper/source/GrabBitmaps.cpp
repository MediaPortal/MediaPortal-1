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

#pragma warning(disable: 4244 4996)

#include <streams.h>
#include <atlbase.h>
#pragma include_alias( "dxtrans.h", "qedit.h" )
#define __IDxtCompositor_INTERFACE_DEFINED__
#define __IDxtAlphaSetter_INTERFACE_DEFINED__
#define __IDxtJpeg_INTERFACE_DEFINED__
#define __IDxtKey_INTERFACE_DEFINED__
#include <qedit.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

// Function prototypes
int GrabBitmaps(TCHAR * szFile);
HRESULT GetPin(IBaseFilter * pFilter, PIN_DIRECTION dirrequired,  int iNum, IPin **ppPin);
IPin *  GetInPin ( IBaseFilter *pFilter, int Num );
IPin *  GetOutPin( IBaseFilter *pFilter, int Num );

// Constants
#define NUM_FRAMES_TO_GRAB  1

extern void Log(const char *fmt, ...) ;

//
// Implementation of CSampleGrabberCB object
//
// Note: this object is a SEMI-COM object, and can only be created statically.

class CSampleGrabberCB : public ISampleGrabberCB 
{

public:

    // These will get set by the main thread below. We need to
    // know this in order to write out the bmp
    long Width;
    long Height;

    // Fake out any COM ref counting
    //
    STDMETHODIMP_(ULONG) AddRef() { return 2; }
    STDMETHODIMP_(ULONG) Release() { return 1; }

    // Fake out any COM QI'ing
    //
    STDMETHODIMP QueryInterface(REFIID riid, void ** ppv)
    {
        CheckPointer(ppv,E_POINTER);
        
        if( riid == IID_ISampleGrabberCB || riid == IID_IUnknown ) 
        {
            *ppv = (void *) static_cast<ISampleGrabberCB*> ( this );
            return NOERROR;
        }    

        return E_NOINTERFACE;
    }


    // We don't implement this one
    //
    STDMETHODIMP SampleCB( double SampleTime, IMediaSample * pSample )
    {
        return 0;
    }


    // The sample grabber is calling us back on its deliver thread.
    // This is NOT the main app thread!
    //
    STDMETHODIMP BufferCB( double SampleTime, BYTE * pBuffer, long BufferSize )
    {
      try
      {
		  Log("grab:callback");
        //
        // Convert the buffer into a bitmap
        //
        TCHAR szFilename[MAX_PATH];
        strcpy(szFilename,"temp.bmp");
        DeleteFile(szFilename);
        // Create a file to hold the bitmap
        HANDLE hf = CreateFile(szFilename, GENERIC_WRITE, FILE_SHARE_READ,  NULL, CREATE_ALWAYS, NULL, NULL );

        if( hf == INVALID_HANDLE_VALUE )
        {
            return 0;
        }

        Log("grab:Found a sample at time %ld ms\t[%s]",   long( SampleTime * 1000 ), szFilename );

        // Write out the file header
        //
        BITMAPFILEHEADER bfh;
        memset( &bfh, 0, sizeof( bfh ) );
        bfh.bfType = 'MB';
        bfh.bfSize = sizeof( bfh ) + BufferSize + sizeof( BITMAPINFOHEADER );
        bfh.bfOffBits = sizeof( BITMAPINFOHEADER ) + sizeof( BITMAPFILEHEADER );

        DWORD Written = 0;
        WriteFile( hf, &bfh, sizeof( bfh ), &Written, NULL );

        // Write the bitmap format
        //
        BITMAPINFOHEADER bih;
        memset( &bih, 0, sizeof( bih ) );
        bih.biSize = sizeof( bih );
        bih.biWidth = Width;
        bih.biHeight = Height;
        bih.biPlanes = 1;
        bih.biBitCount = 24;

        Written = 0;
        WriteFile( hf, &bih, sizeof( bih ), &Written, NULL );

        // Write the bitmap bits
        //
        Written = 0;
        WriteFile( hf, pBuffer, BufferSize, &Written, NULL );

        CloseHandle( hf );
      }
      catch(...)
      {
		  Log("grab:bufferCb: exception");
      }
        return 0;
    }
};




int GrabBitmaps(TCHAR * szFile )
{
  try
  {
    USES_CONVERSION;
    CComPtr< ISampleGrabber > pGrabber;
    CComPtr< IBaseFilter >    pSource;
    CComPtr< IGraphBuilder >  pGraph;
    CComPtr< IVideoWindow >   pVideoWindow;
    HRESULT hr;

    if (!szFile)
        return -1;

	Log("grab: bitmaps from %s.", szFile);

    // Create the sample grabber
    //
    pGrabber.CoCreateInstance( CLSID_SampleGrabber );
    if( !pGrabber )
    {
        Log("grab:Could not create CLSID_SampleGrabber" );
        return -1;
    }
    CComQIPtr< IBaseFilter, &IID_IBaseFilter > pGrabberBase( pGrabber );

    // Create the file reader
    //
    pSource.CoCreateInstance( CLSID_StreamBufferSource   );//CLSID_AsyncReader
    if( !pSource )
    {
        Log("grab:Could not create source filter") ;
        return -1;
    }

    // Create the graph
    //
    pGraph.CoCreateInstance( CLSID_FilterGraph );
    if( !pGraph )
    {
        Log("grab:Could not not create the graph" );
        return -1;
    }

//  hr = AddGraphToRot(pGraph, &g_dwGraphRegister);
  // Put them in the graph
    //
    hr = pGraph->AddFilter( pSource, L"Source" );
	if (!SUCCEEDED(hr))
	{
        Log("grab:Could add streambuffersource to grab" );
	}
    hr = pGraph->AddFilter( pGrabberBase, L"Grabber" );

	if (!SUCCEEDED(hr))
	{
        Log("grab:Could add samplegrabber to grab" );
	}
    // Load the source
    //
    CComQIPtr< IFileSourceFilter, &IID_IFileSourceFilter > pLoad( pSource );
    hr = pLoad->Load( T2W( szFile ), NULL );
    if( FAILED( hr ) )
    {
        Log("grab:Could not load the media file") ;
        return -1;
    }

    // Tell the grabber to grab 24-bit video. Must do this
    // before connecting it
    //
    CMediaType GrabType;
    GrabType.SetType( &MEDIATYPE_Video );
    GrabType.SetSubtype( &MEDIASUBTYPE_RGB24);//&MEDIASUBTYPE_RGB24 );
    hr = pGrabber->SetMediaType( &GrabType );

	if (!SUCCEEDED(hr))
	{
        Log("grab:Could set media type on samplegrabber" );
	}
    // Get the output pin and the input pin
    //
    CComPtr< IPin > pSourcePin;
    CComPtr< IPin > pGrabPin;

    pSourcePin = GetOutPin( pSource, 1 );
    pGrabPin   = GetInPin( pGrabberBase, 0 );

    // ... and connect them
    //
    hr = pGraph->Connect( pSourcePin, pGrabPin );
    if( FAILED( hr ) )
    {
        Log("grab:Could not connect source filter to grabber" );
        return -1;
    }

    // This semi-COM object will receive sample callbacks for us
    //
    CSampleGrabberCB CB;

    // Ask for the connection media type so we know its size
    //
    AM_MEDIA_TYPE mt;
    hr = pGrabber->GetConnectedMediaType( &mt );

    VIDEOINFOHEADER * vih = (VIDEOINFOHEADER*) mt.pbFormat;
    CB.Width  = vih->bmiHeader.biWidth;
    CB.Height = vih->bmiHeader.biHeight;
    FreeMediaType( mt );

    // Render the grabber output pin (to a video renderer)
    //
    CComPtr <IPin> pGrabOutPin = GetOutPin( pGrabberBase, 0 );
    hr = pGraph->Render( pGrabOutPin );
    if( FAILED( hr ) )
    {
        Log("grab:Could not render grabber output pin" );
        return -1;
    }

    // Don't buffer the samples as they pass through
    //
    hr = pGrabber->SetBufferSamples( FALSE );

    // Only grab one at a time, stop stream after
    // grabbing one sample
    //
    hr = pGrabber->SetOneShot( TRUE );

    // Set the callback, so we can grab the one sample
    //
    hr = pGrabber->SetCallback( &CB, 1 );

    // Get the seeking interface, so we can seek to a location
    //
    CComQIPtr< IMediaSeeking, &IID_IMediaSeeking > pSeeking( pGraph );

    // Query the graph for the IVideoWindow interface and use it to
    // disable AutoShow.  This will prevent the ActiveMovie window from
    // being displayed while we grab bitmaps from the running movie.
    CComQIPtr< IVideoWindow, &IID_IVideoWindow > pWindow = pGraph;
    if (pWindow)
    {
        hr = pWindow->put_AutoShow(OAFALSE);
    }

    // Find a limited number of frames
    //

    CComQIPtr< IMediaFilter, &IID_IMediaFilter > mediaFilt( pGraph );
    mediaFilt->SetSyncSource(NULL);
   // for( int i = 0 ; i < NUM_FRAMES_TO_GRAB ; i++ )
    {

		
		Log("grab:seek.");
		REFERENCE_TIME Start = (30) * UNITS;

		// set position
		LONGLONG duration;
		pSeeking->GetDuration(&duration);
		if ( duration > (10*60*UNITS) )
			Start = (10*60*UNITS) ;

		hr = pSeeking->SetPositions( &Start, AM_SEEKING_AbsolutePositioning, NULL, AM_SEEKING_NoPositioning );

		Log("grab:run.");
		// activate the threads
		CComQIPtr< IMediaControl, &IID_IMediaControl > pControl( pGraph );
		hr = pControl->Run( );

		Log("grab:wait.");
		// wait for the graph to settle
		CComQIPtr< IMediaEvent, &IID_IMediaEvent > pEvent( pGraph );
		long EvCode = 0;

    // 15 sec timeout
		hr = pEvent->WaitForCompletion( 15*1000, &EvCode );

		Log("grab:done.");
		// callback wrote the sample
		pControl->Stop();
		}

		Log("Sample grabbing complete.");
  }
  catch(...)
  {
	  Log("grab:exception.");
  }
  return 0;
}


HRESULT GetPin( IBaseFilter * pFilter, PIN_DIRECTION dirrequired, int iNum, IPin **ppPin)
{
    CComPtr< IEnumPins > pEnum;
    *ppPin = NULL;

    HRESULT hr = pFilter->EnumPins(&pEnum);
    if(FAILED(hr)) 
        return hr;

    ULONG ulFound;
    IPin *pPin;
    hr = E_FAIL;

    while(S_OK == pEnum->Next(1, &pPin, &ulFound))
    {
        PIN_DIRECTION pindir = (PIN_DIRECTION)3;

        pPin->QueryDirection(&pindir);
        if(pindir == dirrequired)
        {
            if(iNum == 0)
            {
                *ppPin = pPin;  // Return the pin's interface
                hr = S_OK;      // Found requested pin, so clear error
                break;
            }
            iNum--;
        } 

        pPin->Release();
    } 

    return hr;
}


IPin * GetInPin( IBaseFilter * pFilter, int nPin )
{
    CComPtr<IPin> pComPin=0;
    GetPin(pFilter, PINDIR_INPUT, nPin, &pComPin);
    return pComPin;
}


IPin * GetOutPin( IBaseFilter * pFilter, int nPin )
{
    CComPtr<IPin> pComPin=0;
    GetPin(pFilter, PINDIR_OUTPUT, nPin, &pComPin);
    return pComPin;
}




