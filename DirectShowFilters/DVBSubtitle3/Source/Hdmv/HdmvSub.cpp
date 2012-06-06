/* 
 * $Id: HdmvSub.cpp 939 2008-12-22 21:31:24Z casimir666 $
 *
 * (C) 2006-2007 see AUTHORS
 *
 * This file is part of mplayerc.
 *
 * Mplayerc is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * Mplayerc is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
#define _AFXDLL
#include "StdAfx.h"
#include "HdmvSub.h"
#include "GolombBuffer.h"
#include <math.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\..\alloctracing.h"

#define INDEX_TRANSPARENT	0xFF

#if (0)		// Set to 1 to activate HDMV subtitles traces
	#define TRACE_HDMVSUB		TRACE
#else
	#define TRACE_HDMVSUB
#endif

// Logging
extern void LogDebug( const char *fmt, ... );
extern void LogDebugPTS( const char *fmt, uint64_t pts );

CHdmvSub::CHdmvSub(void)
{
	m_nColorNumber		= 0;

	m_nCurSegment		= NO_SEGMENT;
	m_pSegBuffer		= NULL;
	m_nTotalSegBuffer	= 0;
	m_nSegBufferPos		= 0;
	m_nSegSize			= 0;
	m_pCurrentObject	= NULL;
	m_pDefaultPalette   = NULL;
	m_nDefaultPaletteNbEntry = 0;

	memset (&m_VideoDescriptor, 0, sizeof(VIDEO_DESCRIPTOR));
}

CHdmvSub::~CHdmvSub()
{
  m_pObjects.RemoveAll();
  delete[] m_pSegBuffer;
  delete[] m_pDefaultPalette;

  delete m_pCurrentObject;

  for( unsigned int i( 0 ) ; i < m_RenderedSubtitles.size() ; i++ )
  {
    m_RenderedSubtitles.erase( m_RenderedSubtitles.begin() );	
  }
}

void CHdmvSub::SetObserver( MSubdecoderObserver* pObserver )
{
  m_pObserver = pObserver;
}

void CHdmvSub::RemoveObserver( MSubdecoderObserver* pObserver )
{
  if( m_pObserver == pObserver )
  {
    m_pObserver = NULL;	
  }
}


CSubtitle* CHdmvSub::GetSubtitle( unsigned int place )
{
  if( m_RenderedSubtitles.size() > place )
  {
    return m_RenderedSubtitles[place];
  }
  else
  {
    return NULL;
  }
}

int CHdmvSub::GetSubtitleCount()
{
  return m_RenderedSubtitles.size();
}


CSubtitle* CHdmvSub::GetLatestSubtitle()
{
  int size = m_RenderedSubtitles.size();

  if( size > 0 )
  {
    return m_RenderedSubtitles[size - 1];
  }
  else
  {
    return NULL;
  }
}

void CHdmvSub::ReleaseOldestSubtitle()
{
  if( m_RenderedSubtitles.size() > 0 )
  {
    delete m_RenderedSubtitles[0];
    m_RenderedSubtitles.erase( m_RenderedSubtitles.begin() );		
  }
}


void CHdmvSub::AllocSegment(int nSize)
{
	if (nSize > m_nTotalSegBuffer)
	{
		delete[] m_pSegBuffer;
		m_pSegBuffer		= new BYTE[nSize];
		m_nTotalSegBuffer	= nSize;
	}
	m_nSegBufferPos	 = 0;
	m_nSegSize       = nSize;
}

POSITION CHdmvSub::GetStartPosition(REFERENCE_TIME rt, double fps)
{
	CompositionObject*	pObject;

  // Cleanup old PG
	while (m_pObjects.GetCount()>0) {
		pObject = m_pObjects.GetHead();
		if (pObject->m_rtStop < rt) {
			//TRACE_HDMVSUB ("CHdmvSub:HDMV remove object %d  %S => %S (rt=%S)\n", pObject->GetRLEDataSize(),
			//			   ReftimeToString (pObject->m_rtStart), ReftimeToString(pObject->m_rtStop), ReftimeToString(rt));
			m_pObjects.RemoveHead();
			delete pObject;
		} else {
			break;
		}
	}

	return m_pObjects.GetHeadPosition();
}

HRESULT CHdmvSub::ParsePES(const unsigned char* data, int len,
                           const unsigned char* header, int headerLen)
{
	HRESULT				hr = S_OK;
	REFERENCE_TIME		rtStart = INVALID_TIME, rtStop = INVALID_TIME;
	BYTE*				pData = (BYTE*)data;
	int					lSampleLen;

	lSampleLen = len;
  
  rtStart = 0; rtStop = 1; // TODO - provide real values

	//pSample->GetTime(&rtStart, &rtStop);
	if (pData) {
		CGolombBuffer		SampleBuffer (pData, lSampleLen);

		while (!SampleBuffer.IsEOF()) {
			if (m_nCurSegment == NO_SEGMENT) {
				HDMV_SEGMENT_TYPE	nSegType	= (HDMV_SEGMENT_TYPE)SampleBuffer.ReadByte();
				USHORT				nUnitSize	= SampleBuffer.ReadShort();
				lSampleLen -=3;

				switch (nSegType) {
					case PALETTE :
					case OBJECT :
					case PRESENTATION_SEG :
					case END_OF_DISPLAY :
						m_nCurSegment = nSegType;
						AllocSegment (nUnitSize);
						break;

					case WINDOW_DEF :
					case INTERACTIVE_SEG :
					case HDMV_SUB1 :
					case HDMV_SUB2 :
						// Ignored stuff...
						SampleBuffer.SkipBytes(nUnitSize);
						break;
					default :
						return VFW_E_SAMPLE_REJECTED;
				}
			}

			if (m_nCurSegment != NO_SEGMENT) {
				if (m_nSegBufferPos < m_nSegSize) {
					int		nSize = min (m_nSegSize-m_nSegBufferPos, lSampleLen);
					SampleBuffer.ReadBuffer (m_pSegBuffer+m_nSegBufferPos, nSize);
					m_nSegBufferPos += nSize;
				}

				if (m_nSegBufferPos >= m_nSegSize) {
					CGolombBuffer	SegmentBuffer (m_pSegBuffer, m_nSegSize);

				switch (m_nCurSegment) {

				case PALETTE :
					//LogDebugPTS("PALETTE", Get_pes_pts( header ));
          TRACE_HDMVSUB ("CHdmvSub:PALETTE            rtStart=%10I64d\n", rtStart);
					ParsePalette(&SegmentBuffer, m_nSegSize);
					break;
				case OBJECT :
					//TRACE_HDMVSUB ("CHdmvSub:OBJECT             %S\n", ReftimeToString(rtStart));
          LogDebugPTS("OBJECT", Get_pes_pts( header ));
          ParseObject(&SegmentBuffer, m_nSegSize);
	  
          m_pCurrentObject->m_rtStart	= Get_pes_pts(header);;
  				m_pCurrentObject->m_rtStop	= _I64_MAX;
          
          m_pObjects.AddTail(m_pCurrentObject);

          CreateSubtitle(); // TODO simultaneous subtitles aren't probably supported - need a sample!

					break;
				case PRESENTATION_SEG :
							//TRACE_HDMVSUB ("CHdmvSub:PRESENTATION_SEG   %S (size=%d)\n", ReftimeToString(rtStart), m_nSegSize);

              LogDebugPTS("ParsePresentationSegment", Get_pes_pts(header));

							if (m_pCurrentObject) {
								//TRACE_HDMVSUB ("CHdmvSub:PRESENTATION_SEG   %d\n", m_pCurrentObject->m_nObjectNumber);
								if(m_pCurrentObject->m_nObjectNumber > 1) {
									m_pCurrentObject->m_nObjectNumber--;
									break;
								}

								m_pCurrentObject->m_rtStop = Get_pes_pts(header);
								//m_pObjects.AddTail (m_pCurrentObject);

								//TRACE_HDMVSUB ("CHdmvSub:HDMV : %S => %S\n", ReftimeToString (m_pCurrentObject->m_rtStart), ReftimeToString(rtStart));
								//m_pCurrentObject = NULL;
							}
					if (ParsePresentationSegment(&SegmentBuffer) > 0) {
						//m_pCurrentObject->m_rtStart	= Get_pes_pts(header);;
						//m_pCurrentObject->m_rtStop	= _I64_MAX;
					}
					break;
				case WINDOW_DEF :
					// TRACE_HDMVSUB ("CHdmvSub:WINDOW_DEF         %S\n", ReftimeToString(rtStart));
			          {
			            LogDebugPTS("WINDOW_DEF", Get_pes_pts( header ));
			          }
					break;
				case END_OF_DISPLAY :
					// TRACE_HDMVSUB ("CHdmvSub:END_OF_DISPLAY     %S\n", ReftimeToString(rtStart));
			          {
                  UINT64 time = Get_pes_pts( header );
			            LogDebugPTS("END_OF_DISPLAY", time);

                  if (m_pObjects.GetCount() == 0 && m_pCurrentObject)
                    m_pObserver->UpdateSubtitleTimeout(m_pCurrentObject->m_rtStop);
                  else
                    m_pObjects.RemoveAll();
		          }
					break;
				default :
		          {
		            LogDebugPTS("UNKNOWN", Get_pes_pts( header ));
		            TRACE_HDMVSUB ("CHdmvSub:UNKNOWN Seg %d     rtStart=0x%10dd\n", m_nCurSegment, rtStart);
		          }
        }

				m_nCurSegment = NO_SEGMENT;
				}
			}
		}
	}
	return hr;
}

void CHdmvSub::CreateSubtitle()
{
  POSITION	pos = m_pObjects.GetHeadPosition();

  while (pos) 
	{
    CompositionObject* object = m_pObjects.GetAt(pos);
              
    if (object && object->IsRLEComplete() && object->GetRLEDataSize() > 0)
    {
		  RECT rect;
		  rect.top = 0;
		  rect.left = 0;
		  rect.bottom = 1080;
		  rect.right = 1920;
            
		  SubPicDesc spd;
		  spd.bpp = 32;
		  spd.vidrect = rect;
		  spd.h = object->m_height;
		  spd.w = object->m_width;

      if (spd.h < 1 || spd.w < 1 || spd.w > 1920 || spd.h > 1080)
      {
        m_pObjects.GetNext(pos);
        continue;
      }

		  // TODO: remove...
		  REFERENCE_TIME rt = 0;
            
		  CSubtitle* sub = new CSubtitle(spd.w, spd.h, m_VideoDescriptor.nVideoWidth, m_VideoDescriptor.nVideoHeight);
      sub->SetPTS(object->m_rtStart);
		  sub->SetFirstScanline(object->m_vertical_position);
      sub->SetHorizontalPosition(object->m_horizontal_position);

      Render(object, spd, rt, rect, *sub);
		  m_RenderedSubtitles.push_back(sub);
		  m_pObserver->NotifySubtitle();
    }
    else
      LogDebug("CreateSubtitle: corrupted object!");

    m_pObjects.GetNext(pos);
	}
}

int CHdmvSub::ParsePresentationSegment(CGolombBuffer* pGBuffer)
{
	COMPOSITION_DESCRIPTOR	CompositionDescriptor;
	BYTE					nObjectNumber;
	//bool					palette_update_flag;
	//BYTE					palette_id_ref;

	ParseVideoDescriptor(pGBuffer, &m_VideoDescriptor);
	ParseCompositionDescriptor(pGBuffer, &CompositionDescriptor);
	pGBuffer->ReadByte(); //palette_update_flag	= !!(pGBuffer->ReadByte() & 0x80);
	pGBuffer->ReadByte(); //palette_id_ref		= pGBuffer->ReadByte();
	nObjectNumber		= pGBuffer->ReadByte();

	//TRACE_HDMVSUB( "CHdmvSub::ParsePresentationSegment Size = %d, nObjectNumber = %d\n", pGBuffer->GetSize(), nObjectNumber);

	if (nObjectNumber > 0) {
		delete m_pCurrentObject;
		m_pCurrentObject = new CompositionObject();
		m_pCurrentObject->m_nObjectNumber = nObjectNumber;
		for(int i=0; i<nObjectNumber; i++) {
			ParseCompositionObject (pGBuffer, m_pCurrentObject);
		}
	}

	return nObjectNumber;
}

void CHdmvSub::ParsePalette(CGolombBuffer* pGBuffer, USHORT nSize)		// #497
{
	int		nNbEntry;
	BYTE	palette_id				= pGBuffer->ReadByte();
	BYTE	palette_version_number	= pGBuffer->ReadByte();
	UNUSED_ALWAYS(palette_id);
	UNUSED_ALWAYS(palette_version_number);

	ASSERT ((nSize-2) % sizeof(HDMV_PALETTE) == 0);
	nNbEntry = (nSize-2) / sizeof(HDMV_PALETTE);
	HDMV_PALETTE*	pPalette = (HDMV_PALETTE*)pGBuffer->GetBufferPos();

	if (m_pDefaultPalette == NULL || m_nDefaultPaletteNbEntry != nNbEntry) {
		delete[] m_pDefaultPalette;
		m_pDefaultPalette		 = new HDMV_PALETTE[nNbEntry];
		m_nDefaultPaletteNbEntry = nNbEntry;
	}
	memcpy (m_pDefaultPalette, pPalette, nNbEntry*sizeof(HDMV_PALETTE));

	if (m_pCurrentObject) {
		m_pCurrentObject->SetPalette (nNbEntry, pPalette, m_VideoDescriptor.nVideoWidth>720);
	}
}

void CHdmvSub::ParseObject(CGolombBuffer* pGBuffer, USHORT nUnitSize)	// #498
{
	SHORT	object_id	= pGBuffer->ReadShort();
	UNUSED_ALWAYS(object_id);
	BYTE	m_sequence_desc;

	ASSERT (m_pCurrentObject != NULL);
	if (m_pCurrentObject && m_pCurrentObject->m_object_id_ref == object_id) {
		m_pCurrentObject->m_version_number	= pGBuffer->ReadByte();
		m_sequence_desc						= pGBuffer->ReadByte();

		if (m_sequence_desc & 0x80) {
			DWORD	object_data_length  = (DWORD)pGBuffer->BitRead(24);

			m_pCurrentObject->m_width			= pGBuffer->ReadShort();
			m_pCurrentObject->m_height 			= pGBuffer->ReadShort();

			m_pCurrentObject->SetRLEData (pGBuffer->GetBufferPos(), nUnitSize-11, object_data_length-4);

			TRACE_HDMVSUB ("CHdmvSub:NewObject	size=%ld, total obj=%d, %dx%d\n", object_data_length, m_pObjects.GetCount(),
						   m_pCurrentObject->m_width, m_pCurrentObject->m_height);
		} else {
			m_pCurrentObject->AppendRLEData (pGBuffer->GetBufferPos(), nUnitSize-4);
		}
	}
}

void CHdmvSub::ParseCompositionObject(CGolombBuffer* pGBuffer, CompositionObject* pCompositionObject)
{
	BYTE	bTemp;
	pCompositionObject->m_object_id_ref	= pGBuffer->ReadShort();
	pCompositionObject->m_window_id_ref	= pGBuffer->ReadByte();
	bTemp = pGBuffer->ReadByte();
	pCompositionObject->m_object_cropped_flag	= !!(bTemp & 0x80);
	pCompositionObject->m_forced_on_flag		= !!(bTemp & 0x40);
	pCompositionObject->m_horizontal_position	= pGBuffer->ReadShort();
	pCompositionObject->m_vertical_position		= pGBuffer->ReadShort();

	if (pCompositionObject->m_object_cropped_flag) {
		pCompositionObject->m_cropping_horizontal_position	= pGBuffer->ReadShort();
		pCompositionObject->m_cropping_vertical_position	= pGBuffer->ReadShort();
		pCompositionObject->m_cropping_width				= pGBuffer->ReadShort();
		pCompositionObject->m_cropping_height				= pGBuffer->ReadShort();
	}
}

void CHdmvSub::ParseVideoDescriptor(CGolombBuffer* pGBuffer, VIDEO_DESCRIPTOR* pVideoDescriptor)
{
	pVideoDescriptor->nVideoWidth   = pGBuffer->ReadShort();
	pVideoDescriptor->nVideoHeight  = pGBuffer->ReadShort();
	pVideoDescriptor->bFrameRate	= pGBuffer->ReadByte();
}

void CHdmvSub::ParseCompositionDescriptor(CGolombBuffer* pGBuffer, COMPOSITION_DESCRIPTOR* pCompositionDescriptor)
{
	pCompositionDescriptor->nNumber	= pGBuffer->ReadShort();
	pCompositionDescriptor->bState	= pGBuffer->ReadByte();
}

void CHdmvSub::Render(CompositionObject* pObject, SubPicDesc& spd, REFERENCE_TIME rt, RECT& bbox, CSubtitle &pSubtitle)
{
	//CompositionObject*	pObject = FindObject (rt);

  if (!pObject) // TODO fix me later
    return;
	//ASSERT (pObject!=NULL && spd.w >= pObject->m_width && spd.h >= pObject->m_height);

	/*if (pObject && pObject->GetRLEDataSize() && pObject->m_width > 0 && pObject->m_height > 0 && 
			spd.w >= (pObject->m_horizontal_position + pObject->m_width) && 
			spd.h >= (pObject->m_vertical_position + pObject->m_height)) {
        */
		if (!pObject->HavePalette()) {
			pObject->SetPalette (m_nDefaultPaletteNbEntry, m_pDefaultPalette, m_VideoDescriptor.nVideoWidth>720);
		}

		TRACE_HDMVSUB ("CHdmvSub:Render	    size=%ld,  ObjRes=%dx%d,  SPDRes=%dx%d\n", pObject->GetRLEDataSize(),
					   pObject->m_width, pObject->m_height, spd.w, spd.h);
		pObject->Render(spd, pSubtitle);

		bbox.left	= pObject->m_horizontal_position;
		bbox.top	= pObject->m_vertical_position;
		bbox.right	= bbox.left + pObject->m_width;
		bbox.bottom	= bbox.top  + pObject->m_height;
	//}
}

HRESULT CHdmvSub::GetTextureSize (POSITION pos, SIZE& MaxTextureSize, SIZE& VideoSize, POINT& VideoTopLeft)
{
	CompositionObject*	pObject = m_pObjects.GetAt (pos);
	if (pObject) {
		MaxTextureSize.cx	= m_VideoDescriptor.nVideoWidth;
		MaxTextureSize.cy	= m_VideoDescriptor.nVideoHeight;

		VideoSize.cx	= m_VideoDescriptor.nVideoWidth;
		VideoSize.cy	= m_VideoDescriptor.nVideoHeight;

		// The subs will be directly rendered into the proper position!
		VideoTopLeft.x	= 0; //pObject->m_horizontal_position;
		VideoTopLeft.y	= 0; //pObject->m_vertical_position;

		return S_OK;
	}

	ASSERT (FALSE);
	return E_INVALIDARG;
}


void CHdmvSub::Reset()
{
	CompositionObject*	pObject;
	while (m_pObjects.GetCount() > 0) {
		pObject = m_pObjects.RemoveHead();
		delete pObject;
	}

  for( unsigned int i( 0 ) ; i < m_RenderedSubtitles.size() ; i++ )
  {
    m_RenderedSubtitles.erase( m_RenderedSubtitles.begin() );	
  }
}

const double Rec601_Kr = 0.299;
const double Rec601_Kb = 0.114;
const double Rec601_Kg = 0.587;
COLORREF CHdmvSub::YCrCbToRGB_Rec601(BYTE Y, BYTE Cr, BYTE Cb)
{

  double rp = Y + 2*(Cr-128)*(1.0-Rec601_Kr);
  double gp = Y - 2*(Cb-128)*(1.0-Rec601_Kb)*Rec601_Kb/Rec601_Kg - 2*(Cr-128)*(1.0-Rec601_Kr)*Rec601_Kr/Rec601_Kg;
  double bp = Y + 2*(Cb-128)*(1.0-Rec601_Kb);

//  R = fabs(rp); G = fabs(gp); B = fabs(bp);

  return RGB (fabs(rp), fabs(gp), fabs(bp));
}


const double Rec709_Kr = 0.2125;
const double Rec709_Kb = 0.0721;
const double Rec709_Kg = 0.7154;
COLORREF CHdmvSub::YCrCbToRGB_Rec709(BYTE Y, BYTE Cr, BYTE Cb)
{

  double rp = Y + 2*(Cr-128)*(1.0-Rec709_Kr);
  double gp = Y - 2*(Cb-128)*(1.0-Rec709_Kb)*Rec709_Kb/Rec709_Kg - 2*(Cr-128)*(1.0-Rec709_Kr)*Rec709_Kr/Rec709_Kg;
  double bp = Y + 2*(Cb-128)*(1.0-Rec709_Kb);

//  R = fabs(rp); G = fabs(gp); B = fabs(bp);

  return RGB (fabs(rp), fabs(gp), fabs(bp));
}

CHdmvSub::CompositionObject*	CHdmvSub::FindObject(REFERENCE_TIME rt)
{
	POSITION	pos = m_pObjects.GetHeadPosition();

	while (pos) {
		CompositionObject*	pObject = m_pObjects.GetAt (pos);

		if (rt >= pObject->m_rtStart && rt < pObject->m_rtStop) {
			return pObject;
		}

		m_pObjects.GetNext(pos);
	}

	return NULL;
}


// ===== CHdmvSub::CompositionObject

CHdmvSub::CompositionObject::CompositionObject()
{
	m_rtStart		= 0;
	m_rtStop		= 0;
	m_pRLEData		= NULL;
	m_nRLEDataSize	= 0;
	m_nRLEPos		= 0;
	m_nColorNumber	= 0;
	memset (m_Colors, 0, sizeof(m_Colors));
}

CHdmvSub::CompositionObject::~CompositionObject()
{
	delete[] m_pRLEData;
}

void CHdmvSub::CompositionObject::SetPalette (int nNbEntry, HDMV_PALETTE* pPalette, bool bIsHD)
{
	m_nColorNumber	= nNbEntry;

	for (int i=0; i<m_nColorNumber; i++)
	{
		if (bIsHD)
			m_Colors[pPalette[i].entry_id] = pPalette[i].T<<24| 
					YCrCbToRGB_Rec709 (pPalette[i].Y, pPalette[i].Cr, pPalette[i].Cb);
		else
			m_Colors[pPalette[i].entry_id] = pPalette[i].T<<24| 
					YCrCbToRGB_Rec601 (pPalette[i].Y, pPalette[i].Cr, pPalette[i].Cb);
//		TRACE_HDMVSUB ("%03d : %08x\n", pPalette[i].entry_id, m_Colors[pPalette[i].entry_id]);
	}
}


void CHdmvSub::CompositionObject::SetRLEData(BYTE* pBuffer, int nSize, int nTotalSize)
{
	delete[] m_pRLEData;
	m_pRLEData		= new BYTE[nTotalSize];
	m_nRLEDataSize	= nTotalSize;
	m_nRLEPos		= nSize;

	memcpy (m_pRLEData, pBuffer, nSize);
}

void CHdmvSub::CompositionObject::AppendRLEData(BYTE* pBuffer, int nSize)
{
	ASSERT (m_nRLEPos+nSize <= m_nRLEDataSize);
	if (m_nRLEPos+nSize <= m_nRLEDataSize)
	{
		memcpy (m_pRLEData+m_nRLEPos, pBuffer, nSize);
		m_nRLEPos += nSize;
	}
}

void CHdmvSub::CompositionObject::Render(SubPicDesc& spd, CSubtitle &pSubtitle)
{
	if (!m_pRLEData)
    return;

	CGolombBuffer	GBuffer (m_pRLEData, m_nRLEDataSize);
	BYTE			bTemp;
	BYTE			bSwitch;
	bool			bEndOfLine = false;

	BYTE			nPaletteIndex;
	SHORT			nCount;
	SHORT			nX	= 0;
	SHORT			nY	= 0;

	while ((nY < m_height) && !GBuffer.IsEOF())
	{
		bTemp = GBuffer.ReadByte();
		if (bTemp != 0)
		{
			nPaletteIndex = bTemp;
			nCount		  = 1;
		}
		else
		{
			bSwitch = GBuffer.ReadByte();
			if (!(bSwitch & 0x80))
			{
				if (!(bSwitch & 0x40))
				{
					nCount = bSwitch & 0x3F;
					if (nCount > 0)
						nPaletteIndex = 0;
				}
				else
				{
					nCount			= (bSwitch&0x3F) <<8 | (SHORT)GBuffer.ReadByte();
					nPaletteIndex	= 0;
				}
			}
			else
			{
				if (!(bSwitch & 0x40))
				{
					nCount			= bSwitch & 0x3F;
					nPaletteIndex	= GBuffer.ReadByte();
				}
				else
				{
					nCount			= (bSwitch&0x3F) <<8 | (SHORT)GBuffer.ReadByte();
					nPaletteIndex	= GBuffer.ReadByte();
				}
			}
		}

		if (nCount > 0)
		{
      long colors(m_Colors[nPaletteIndex]);
	    DWORD alpha = (colors >> 24) & 0xff;
	    colors = (255 - alpha) << 24 | colors & 0xffffff;

      pSubtitle.DrawRect(nX, nY, nCount, GetRValue(colors), GetGValue(colors), GetBValue(colors), alpha);

      nX += nCount;
		}
		else
		{
			nY++;
			nX = 0;
		}
  }
}

uint64_t CHdmvSub::Get_pes_pts( const unsigned char* buf ) 
{
  UINT64 k=0LL;
  UINT64 pts=0LL;
  UINT64 dts=0LL;
  bool PTS_available=false;
  bool DTS_available=false;

  if ( (buf[7]&0x80)!=0) PTS_available=true;
  if ( (buf[7]&0x40)!=0) DTS_available=true;

  if (PTS_available)
  {
    pts+= ((buf[13]>>1)&0x7f);				// 7bits	7
    pts+=(buf[12]<<7);								// 8bits	15
    pts+=((buf[11]>>1)<<15);					// 7bits	22
    pts+=((buf[10])<<22);							// 8bits	30
    k=((buf[9]>>1)&0x7);
    k <<=30LL;
    pts+=k;			// 3bits
    pts &= 0x1FFFFFFFFLL;
  }
  if (DTS_available) 
  {
    dts= (buf[18]>>1);								// 7bits	7
    dts+=(buf[17]<<7);								// 8bits	15
    dts+=((buf[16]>>1)<<15);					// 7bits	22
    dts+=((buf[15])<<22);							// 8bits	30
    k=((buf[14]>>1)&0x7);
    k <<=30LL;
    dts+=k;			// 3bits
    dts &= 0x1FFFFFFFFLL;
  }
  return( pts );
}
