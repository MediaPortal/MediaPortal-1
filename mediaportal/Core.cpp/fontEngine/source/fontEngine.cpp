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

#include "stdafx.h"
#include "fontEngine.h"
#include "transformmatrix.h"

using namespace std;
#include <vector>
#include <shlobj.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#pragma warning(disable:4244)

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved )
{
  switch (ul_reason_for_call)
  {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
      break;
    case DLL_PROCESS_DETACH:
      Cleanup();
    break;
    }
  return TRUE;
}

#define MAX_TEXTURES			    2000
#define MAX_TEXTURE_COORDS		8000
#define MaxNumfontVertices		8000
#define MAX_FONTS				      20
#define MaxNumTextureVertices	3000
#define MAX_TEXT_LINES        200

// A structure for our custom vertex type
struct CUSTOMVERTEX
{
  FLOAT x, y, z;    // The transformed position for the vertex
  DWORD color;      // The vertex color
  FLOAT tu, tv;     // The texture coordinates
};

struct CUSTOMVERTEX2 
{
  FLOAT x, y, z;
  DWORD color;
  FLOAT tu, tv;   // Texture coordinates
  FLOAT tu2, tv2;
};

struct CUSTOMVERTEX3
{
  FLOAT x, y, z;
  DWORD color;
  FLOAT tu, tv;   // Texture coordinates
  FLOAT tu2, tv2;
  FLOAT tu3, tv3;
};

// Our custom FVF, which describes our custom vertex structure
#define D3DFVF_CUSTOMVERTEX (D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX1)
#define D3DFVF_CUSTOMVERTEX2 (D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX2)
#define D3DFVF_CUSTOMVERTEX3 (D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX3)

struct FONT_DATA_T
{
  int                     iFirstChar;
  int                     iEndChar;
  float                   fTextureScale;
  float                   fTextureWidth;
  float                   fTextureHeight;
  float                   fSpacingPerChar;
  LPDIRECT3DTEXTURE9      pTexture;
  LPDIRECT3DVERTEXBUFFER9 pVertexBuffer;
  LPDIRECT3DINDEXBUFFER9  pIndexBuffer;
  float                   textureCoord[MAX_TEXTURE_COORDS][4];
  CUSTOMVERTEX*           vertices;
  int                     iv;
  int                     dwNumTriangles;
  bool                    updateVertexBuffer;
} ;

struct TEXTURE_DATA_T
{	
  int                     hashCode;
  LPDIRECT3DTEXTURE9      pTexture;
  LPDIRECT3DVERTEXBUFFER9 pVertexBuffer;
  LPDIRECT3DINDEXBUFFER9  pIndexBuffer;
  CUSTOMVERTEX*           vertices;
  int                     iv;
  int                     dwNumTriangles;
  D3DSURFACE_DESC         desc;
  bool                    updateVertexBuffer;
  bool                    useAlphaBlend;
  bool                    delayedRemove;
};

struct TEXTURE_PLACE
{
  int numRect;
  D3DRECT rect[200];
};

static FONT_DATA_T*         fontData = new FONT_DATA_T[MAX_FONTS];
static TEXTURE_DATA_T*      textureData = new TEXTURE_DATA_T[MAX_TEXTURES];
static LPDIRECT3DDEVICE9    m_pDevice=NULL;	
static int                  textureZ[MAX_TEXTURES];
static TEXTURE_PLACE*       texturePlace[MAX_TEXTURES];
static D3DTEXTUREFILTERTYPE m_Filter;
int                         textureCount;

static bool inPresentTextures=false; 
static vector<int> texturesToBeRemoved;
bool clipEnabled = false;

TCHAR logFile[MAX_PATH];
static bool pathInitialized=false;

int m_iTexturesInUse=0;
int m_iVertexBuffersUpdated=0;
int m_iFontVertexBuffersUpdated=0;
int m_iScreenWidth=0;
int m_iScreenHeight=0;
D3DPOOL   m_ipoolFormat=D3DPOOL_MANAGED;
DWORD     m_usage = 0;
DWORD     m_lock = 0;
DWORD     m_alphaBlend=-1;

void Log(char* txt)
{
  if(!pathInitialized)
  {
    TCHAR folder[MAX_PATH];
    ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
    sprintf(logFile,"%s\\Team MediaPortal\\MediaPortal\\Log\\fontEngine.log",folder);
    pathInitialized=true;
  }
  
  FILE* fp = fopen(logFile,"a+");
  if (!fp)
  {
    // failed to open log file, folder missing?
    return;
  }

  fseek(fp,0,SEEK_END);

  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);

  fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%x]%s",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
    systemTime.wMilliseconds,
    GetCurrentThreadId(),
    txt);
  fclose(fp);
}
//*******************************************************************************************************************

void Cleanup()
{
  for (int i=0; i < MAX_TEXTURES;++i)
  {
    if (texturePlace[i] != NULL)
    {
      delete[] texturePlace[i];
    }
  }
  delete[] textureData;
  delete[] fontData;
  return;
}

//*******************************************************************************************************************
void FontEngineInitialize(int screenWidth, int screenHeight, int poolFormat)
{
  m_iScreenWidth=screenWidth;
  m_iScreenHeight=screenHeight;
  //Log("FontEngineInitialize()\n");
  textureCount=0;
  static bool initialized=false;
  if (!initialized)
  {
    for (int i=0; i < MAX_FONTS;++i)
    {
      fontData[i].pVertexBuffer=NULL;
      fontData[i].pIndexBuffer=NULL;
      fontData[i].pTexture = NULL;
      fontData[i].vertices = NULL;
      fontData[i].updateVertexBuffer=false;
    }
    for (int i=0; i < MAX_TEXTURES;++i)
    {
      textureData[i].hashCode=-1;
      textureData[i].dwNumTriangles=0;
      textureData[i].iv=0;
      textureData[i].pVertexBuffer=NULL;
      textureData[i].pIndexBuffer=NULL;
      textureData[i].pTexture=NULL;
      textureData[i].vertices = NULL;
      textureData[i].updateVertexBuffer=false;
      textureData[i].useAlphaBlend=true;
      textureData[i].delayedRemove=false;
      textureZ[i]=-1;
      texturePlace[i]=new TEXTURE_PLACE();
      texturePlace[i]->numRect = 0;
    }
    initialized=true;
    textureCount=0;
	}
  if(poolFormat==0)
  {
    m_ipoolFormat = D3DPOOL_DEFAULT;
    //m_usage = D3DUSAGE_DYNAMIC | D3DUSAGE_WRITEONLY;
    //m_lock = D3DLOCK_DISCARD;
    m_usage = D3DUSAGE_WRITEONLY;
    m_lock = 0;
  }
  else
  {
    m_ipoolFormat = D3DPOOL_MANAGED;
    m_usage = D3DUSAGE_WRITEONLY;
    m_lock = 0;
  }
}
//*******************************************************************************************************************
void FontEngineSetDevice(void* device)
{
  if(!device)
  {
    m_pDevice = NULL;
    return;
  }
  
  m_pDevice = (LPDIRECT3DDEVICE9)device;
  m_Filter = D3DTEXF_NONE;

  D3DCAPS9 caps;
  ZeroMemory(&caps, sizeof(caps));
  m_pDevice->GetDeviceCaps(&caps);
  if((caps.StretchRectFilterCaps&D3DPTFILTERCAPS_MINFLINEAR)&&
    (caps.StretchRectFilterCaps&D3DPTFILTERCAPS_MAGFLINEAR))
  {
    m_Filter = D3DTEXF_LINEAR;
  }
  m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
  m_alphaBlend = -1;
}

//*******************************************************************************************************************

void FontEngineSetAlphaBlend(DWORD alphaBlend)
{
  if(alphaBlend!=m_alphaBlend)
  {
    m_pDevice->SetRenderState(D3DRS_ALPHABLENDENABLE ,alphaBlend);
    m_alphaBlend = alphaBlend;
  }
}

//*******************************************************************************************************************

void FontEngineSetClipEnable()
{
  // Set the state of the FontEngine to render with clipping using the SetScissorRect() rectangle.
  // The called FontEngine draw function will disable clipping; the user must set the clip rectangle
  // prior to each FontEngine draw function if subsequent clipping is desired.
  clipEnabled = true;
}

//*******************************************************************************************************************

void FontEngineSetClipDisable()
{
  // Set the state of the FontEngine to render without clipping.
  clipEnabled = false;
}

//*******************************************************************************************************************
void FontEngineRemoveTexture(int textureNo)
{
  //char log[128];
  //sprintf(log,"FontEngineRemoveTexture(%d)\n", textureNo);
  //Log(log);
  if(!m_pDevice)
  {
    return;
  }

  if(inPresentTextures)
  {
    char log[128];
    sprintf(log,"FontEngineRemoveTexture - called when inPresentTextures = true, using delayed remove for %i\n", textureNo );
    Log(log);
    textureData[textureNo].delayedRemove=true;
    texturesToBeRemoved.push_back(textureNo);
    return;
  }

  // Important to set it to NULL otherwise the textures, etc. will not be freed on release
  m_pDevice->SetStreamSource(0, NULL, 0, 0 );

  if (textureNo < 0 || textureNo>=MAX_TEXTURES) return;
  textureData[textureNo].hashCode=-1;
  textureData[textureNo].dwNumTriangles=0;
  textureData[textureNo].iv=0;
  if (textureData[textureNo].pVertexBuffer!=NULL)
  {
    textureData[textureNo].pVertexBuffer->Release();
  }
  textureData[textureNo].pVertexBuffer=NULL;
  if (textureData[textureNo].pIndexBuffer!=NULL)
  {
    textureData[textureNo].pIndexBuffer->Release();
  }
  textureData[textureNo].pIndexBuffer=NULL;

  if (textureData[textureNo].vertices!=NULL)
  {
    delete[] textureData[textureNo].vertices;
  }
  textureData[textureNo].vertices=NULL;
  if ( textureData[textureNo].pTexture!=NULL)
  {
    textureData[textureNo].pTexture->Release();
  }
  textureData[textureNo].pTexture=NULL;
  textureData[textureNo].updateVertexBuffer=true;
  textureData[textureNo].useAlphaBlend=true;
  textureData[textureNo].delayedRemove=false;
}

//*******************************************************************************************************************
int FontEngineAddTexture(int hashCode, bool useAlphaBlend, void* texture)
{
  int selected=-1;
  for (int i=0; i < MAX_TEXTURES;++i)
  {
    if (textureData[i].hashCode==hashCode)
    {
      selected=i;
      break;
    }
    if (textureData[i].hashCode==-1)
    {
      selected=i;
    }
  }
  if (selected==-1)
  {
    Log("ERROR FontEngine:Ran out of textures!\n");
    return -1;
  }
  textureData[selected].useAlphaBlend=useAlphaBlend;
  textureData[selected].hashCode=hashCode;
  textureData[selected].pTexture=(LPDIRECT3DTEXTURE9)texture;
  textureData[selected].pTexture->AddRef();
  textureData[selected].updateVertexBuffer=true;

  if (textureData[selected].pVertexBuffer==NULL)
  {
    m_pDevice->CreateVertexBuffer(MaxNumTextureVertices*sizeof(CUSTOMVERTEX),
                                  m_usage, 
                                  D3DFVF_CUSTOMVERTEX,
                                  m_ipoolFormat, 
                                  &textureData[selected].pVertexBuffer, 
                                  NULL);  
  }
  if (textureData[selected].vertices==NULL)
  {
    textureData[selected].vertices = new CUSTOMVERTEX[MaxNumTextureVertices];
    for (int i=0; i < MaxNumTextureVertices;++i)
    {
      textureData[selected].vertices[i].z=0;
      //textureData[selected].vertices[i].rhw=1;
    }
  }
  textureData[selected].pTexture->GetLevelDesc(0,&textureData[selected].desc);

  m_pDevice->CreateIndexBuffer(	MaxNumTextureVertices *sizeof(WORD),
                                D3DUSAGE_WRITEONLY, D3DFMT_INDEX16, m_ipoolFormat, 
                                &textureData[selected].pIndexBuffer, NULL ) ;
  WORD* pIndices;
  int triangle=0;
  textureData[selected].pIndexBuffer->Lock(0,0,(VOID**)&pIndices,0);
  for (int i=0; i < MaxNumTextureVertices;i+=6)
  {
    if (i+5 < MaxNumTextureVertices)
    {
      pIndices[i+0]=triangle*4+1;
      pIndices[i+1]=triangle*4+0;
      pIndices[i+2]=triangle*4+3;
      pIndices[i+3]=triangle*4+2;
      pIndices[i+4]=triangle*4+1;
      pIndices[i+5]=triangle*4+3;
    }
    triangle++;
  }
  textureData[selected].pIndexBuffer->Unlock();

  return selected;
}

//*******************************************************************************************************************
int FontEngineAddSurface(int hashCode, bool useAlphaBlend,void* surface)
{
  //char log[128];
  //sprintf(log,"FontEngineAddSurface(%x)\n", hashCode);
  //Log(log);
  int selected=-1;
  for (int i=0; i < MAX_TEXTURES;++i)
  {
    if (textureData[i].hashCode==hashCode)
    {
      selected=i;
      break;
    }
    if (textureData[i].hashCode==-1)
    {
      selected=i;
    }
  }
  if (selected==-1)
  {
    Log("ERROR Fontengine:Ran out of textures!\n");
    return -1;
  }
  LPDIRECT3DSURFACE9 pSurface = (LPDIRECT3DSURFACE9)surface;
  void *pContainer = NULL;
  int hr=pSurface->GetContainer(IID_IDirect3DTexture9,&pContainer);

  textureData[selected].useAlphaBlend=useAlphaBlend;
  textureData[selected].hashCode=hashCode;
  textureData[selected].pTexture=(LPDIRECT3DTEXTURE9)pContainer;

  if (textureData[selected].pVertexBuffer==NULL)
  {
    m_pDevice->CreateVertexBuffer(MaxNumTextureVertices*sizeof(CUSTOMVERTEX),
                                  m_usage, 
                                  D3DFVF_CUSTOMVERTEX,
                                  m_ipoolFormat, 
                                  &textureData[selected].pVertexBuffer, 
                                  NULL) ;  
  }
  if (textureData[selected].vertices==NULL)
  {
    textureData[selected].vertices = new CUSTOMVERTEX[MaxNumTextureVertices];
    for (int i=0; i < MaxNumTextureVertices;++i)
    {
      textureData[selected].vertices[i].z=0;
      //textureData[selected].vertices[i].rhw=1;
    }
  }
  textureData[selected].pTexture->GetLevelDesc(0,&textureData[selected].desc);

  m_pDevice->CreateIndexBuffer( MaxNumTextureVertices *sizeof(WORD),
                                D3DUSAGE_WRITEONLY, D3DFMT_INDEX16, m_ipoolFormat, 
                                &textureData[selected].pIndexBuffer, NULL ) ;
  WORD* pIndices;
  int triangle=0;
  textureData[selected].pIndexBuffer->Lock(0,0,(VOID**)&pIndices,0);
  for (int i=0; i < MaxNumTextureVertices;i+=6)
  {
    if (i+5 < MaxNumTextureVertices)
    {
      pIndices[i+0]=triangle*4+1;
      pIndices[i+1]=triangle*4+0;
      pIndices[i+2]=triangle*4+3;
      pIndices[i+3]=triangle*4+2;
      pIndices[i+4]=triangle*4+1;
      pIndices[i+5]=triangle*4+3;
    }
    triangle++;
  }
  textureData[selected].pIndexBuffer->Unlock();

  return selected;
}

//*******************************************************************************************************************
void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color, float m[3][4])
{
  if (textureNo < 0 || textureNo>=MAX_TEXTURES) 
	  return;

  // Avoid drawing textures outside the viewport.
  D3DVIEWPORT9 viewport;
  m_pDevice->GetViewport(&viewport);

  if ((x+nw <= viewport.X) || 
    (y+nh <=viewport.Y) || 
    (x >= viewport.X+viewport.Width) || 
    (y >= viewport.Y+viewport.Height)) 
  {
    return;
  }

  // If clipping is enabled, avoid drawing textures completely outside the clip rectangle.
  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    if ((x+nw <= clipRect.left) || 
      (y+nh <=clipRect.top) || 
      (x >= clipRect.right) || 
      (y >= clipRect.bottom)) 
    {
      return;
    }
  }

  TEXTURE_DATA_T* texture;
  TransformMatrix matrix(m);
  //1-2-1
  bool needRedraw=false;
  bool textureAlreadyDrawn=false;
  for (int i=0; i < textureCount; ++i)
  {
    if (textureZ[i] == textureNo)
    {
      textureAlreadyDrawn=true;
    }
    if (textureAlreadyDrawn && textureZ[i] != textureNo)
    {
      //check if textures intersect
      int count=textureZ[i];
      D3DRECT rectThis;
      rectThis.x1=x; rectThis.y1=y;rectThis.x2=x+nw;rectThis.y2=y+nh;
      for (int r=0; r < texturePlace[count]->numRect;r++)
      {
        D3DRECT rect2=texturePlace[count]->rect[r];
        if (((rect2.x1 < (rectThis.x2)) && (rectThis.x1 < (rect2.x2))) && (rect2.y1 < (rectThis.y2)))
        {
          if (rectThis.y1 < (rect2.y2))
          {
            needRedraw=true;
          }
        }
      }
    }
  }

  if (needRedraw)
  {
    FontEnginePresentTextures();
  }

  texture=&textureData[textureNo];
  if (texture->iv==0)
  {
    textureZ[textureCount]=textureNo;
    textureCount++;
  }
  texturePlace[textureNo]->rect[texture->dwNumTriangles/2].x1=x;
  texturePlace[textureNo]->rect[texture->dwNumTriangles/2].y1=y;
  texturePlace[textureNo]->rect[texture->dwNumTriangles/2].x2=x+nw;
  texturePlace[textureNo]->rect[texture->dwNumTriangles/2].y2=y+nh;
  texturePlace[textureNo]->numRect = texturePlace[textureNo]->numRect+1;
  int iv=texture->iv;
  if (iv+6 >=MaxNumTextureVertices)
  {
    Log("ERROR Fontengine:Ran out of texture vertices\n");
    return;
  }

  float xpos=x;
  float xpos2=x+nw;
  float ypos=y;
  float ypos2=y+nh;

  float tx1=uoff;
  float tx2=uoff+umax;
  float ty1=voff;
  float ty2=voff+vmax;

  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    // This clipping is done algorthimically for the texture being drawn.
    // This texture is maintained in the list of textures managed by the FontEngine.
    // Since the FontEngine does not store clip rectangles with the textures we cannot use the hardware to perform
    // the clipping.
    if (clipRect.top > 0 || clipRect.left > 0)
    {
      float w = xpos2 - xpos;
      float h = ypos2 - ypos;
      
      // Clipping on left side.
      if (xpos <	clipRect.left)
      {
        float off = clipRect.left - xpos;
        xpos = (float)clipRect.left;
        tx1 += ((off / w) * umax);

        if (tx1 >= 1.0f)
        {
          tx1 = 1.0f;
        }
      }

      // Clipping on right side.
      if (xpos2 >	clipRect.right)
      {
        float off = (clipRect.right) - xpos2;
        xpos2 = clipRect.right;
        tx2 += ((off / w) * umax); 

        if (tx2 >= 1.0f)
        {
          tx2 = 1.0f;
        }
      }

      // Clipping top.
      if (ypos <	clipRect.top)
      {
        float off = clipRect.top - ypos;
        ypos = (float)clipRect.top;
        ty1 += ((off / h) * vmax);
      }

      // Clipping bottom.
      if (ypos2 >	clipRect.bottom)
      {
        float off = clipRect.bottom - ypos2;
        ypos2 = (float)clipRect.bottom;
        ty2 += ((off / h) * vmax);

        if (ty2 >= 1.0f)
        {
          ty2=1.0f;
        }
      }
    }
  }

  xpos-=0.5f;
  ypos-=0.5f;
  xpos2-=0.5f;
  ypos2-=0.5f;

  //upper left
  float x1=matrix.ScaleFinalXCoord(xpos,ypos);
  float y1=matrix.ScaleFinalYCoord(xpos,ypos);
  float z1 = matrix.ScaleFinalZCoord(xpos,ypos);

  //bottom left
  float x2=matrix.ScaleFinalXCoord(xpos,ypos2);
  float y2=matrix.ScaleFinalYCoord(xpos,ypos2);
  float z2=matrix.ScaleFinalZCoord(xpos,ypos2);

  //bottom right
  float x3=matrix.ScaleFinalXCoord(xpos2,ypos2);
  float y3=matrix.ScaleFinalYCoord(xpos2,ypos2);
  float z3=matrix.ScaleFinalZCoord(xpos2,ypos2);

  //upper right
  float x4=matrix.ScaleFinalXCoord(xpos2,ypos);
  float y4=matrix.ScaleFinalYCoord(xpos2,ypos);
  float z4=matrix.ScaleFinalZCoord(xpos2,ypos);

  //upper left
  if (texture->vertices[iv].tu != tx1 || texture->vertices[iv].tv !=ty1 || texture->vertices[iv].color!=color ||
      texture->vertices[iv].x != x1 || texture->vertices[iv].y !=y1 || texture->vertices[iv].z!=z1)
  {
    texture->updateVertexBuffer=true;
  }
  texture->vertices[iv].x=x1 ;  
  texture->vertices[iv].y=y1 ; 
  texture->vertices[iv].z=z1;
  texture->vertices[iv].color=color;
  texture->vertices[iv].tu=tx1; 
  texture->vertices[iv].tv=ty1; 
  iv++;

  //bottom left
  if (texture->vertices[iv].tu != tx1 || texture->vertices[iv].tv !=ty2 || texture->vertices[iv].color!=color ||
      texture->vertices[iv].x != x2 || texture->vertices[iv].y !=y2 || texture->vertices[iv].z!=z2)
  {
    texture->updateVertexBuffer=true;
  }
  texture->vertices[iv].x=x2;  
  texture->vertices[iv].y=y2;
  texture->vertices[iv].z=z2; 
  texture->vertices[iv].color=color;
  texture->vertices[iv].tu=tx1; 
  texture->vertices[iv].tv=ty2;
  iv++;

  //bottom right
  if (texture->vertices[iv].tu != tx2 || texture->vertices[iv].tv !=ty2 || texture->vertices[iv].color!=color ||
      texture->vertices[iv].x != x3 || texture->vertices[iv].y !=y3 || texture->vertices[iv].z!=z3)
  {
    texture->updateVertexBuffer=true;
  }
  texture->vertices[iv].x=x3;  
  texture->vertices[iv].y=y3; 
  texture->vertices[iv].z=z3;
  texture->vertices[iv].color=color;
  texture->vertices[iv].tu=tx2; 
  texture->vertices[iv].tv=ty2;iv++;

  //upper right
  if (texture->vertices[iv].tu != tx2 || texture->vertices[iv].tv !=ty1 || texture->vertices[iv].color!=color ||
      texture->vertices[iv].x != x4 || texture->vertices[iv].y !=y4 || texture->vertices[iv].z!=z4)
  {
    texture->updateVertexBuffer=true;
  }
  texture->vertices[iv].x=x4;  
  texture->vertices[iv].y=y4;
  texture->vertices[iv].z=z4; 
  texture->vertices[iv].color=color;
  texture->vertices[iv].tu=tx2; 
  texture->vertices[iv].tv=ty1;
  iv++;

  texture->iv=texture->iv+4;
  texture->dwNumTriangles=texture->dwNumTriangles+2;
}


//*******************************************************************************************************************
// blendMode 0 = ignore/bypass blending operation (completely disregard the use of textureNo2)
// blendMode 1 = diffuse blending (choose D3DTOP_MODULATE)
// blendMode 2 = linear blending (choose D3DTOP_BLENDTEXTUREALPHA)
void FontEngineDrawTexture2(int textureNo1,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax,
                            int color, float m[3][4],
                            int textureNo2, float uoff2, float voff2, float umax2, float vmax2,
                            int blendMode)
{
  if (textureNo1 < 0 || textureNo1>=MAX_TEXTURES) return;
  if (textureNo2 < 0 || textureNo2>=MAX_TEXTURES) return;

  // Avoid drawing textures outside the viewport.
  D3DVIEWPORT9 viewport;
  m_pDevice->GetViewport(&viewport);

  if ((x+nw <= viewport.X) || 
    (y+nh <=viewport.Y) || 
    (x >= viewport.X+viewport.Width) || 
    (y >= viewport.Y+viewport.Height)) 
  {
    return;
  }

  // If clipping is enabled, avoid drawing textures completely outside the clip rectangle.
  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    if ((x+nw <= clipRect.left) || 
      (y+nh <=clipRect.top) || 
      (x >= clipRect.right) || 
      (y >= clipRect.bottom)) 
    {
      return;
    }
  }

  TransformMatrix matrix(m);
  FontEnginePresentTextures();

  TEXTURE_DATA_T* texture1;
  TEXTURE_DATA_T* texture2;
  texture1=&textureData[textureNo1];
  texture2=&textureData[textureNo2];

  float xpos=x;
  float xpos2=x+nw;
  float ypos=y;
  float ypos2=y+nh;

  float tx1=uoff;
  float tx2=umax;
  float ty1=voff;
  float ty2=vmax;

  float tx1_2=uoff2;
  float tx2_2=umax2;
  float ty1_2=voff2;
  float ty2_2=vmax2; 

  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    // This clipping is done algorthimically for the texture being drawn.
    // This texture is maintained in the list of textures managed by the FontEngine.
    // Since the FontEngine does not store clip rectangles with the textures we cannot use the hardware to perform
    // the clipping.
    if (clipRect.top > 0 || clipRect.left > 0)
    {
      float w = xpos2 - xpos;
      float h = ypos2 - ypos;
      
      // Clipping on left side.
      if (xpos <	clipRect.left)
      {
        float off = clipRect.left - xpos;
        xpos = (float)clipRect.left;
        tx1 += ((off / w) * umax);
        tx1_2 += ((off / w) * umax);

        if (tx1 >= 1.0f)
        {
          tx1 = 1.0f;
        }
        if (tx1_2 >= 1.0f)
        {
          tx1_2 = 1.0f;
        }
      }

      // Clipping on right side.
      if (xpos2 >	clipRect.right)
      {
        float off = (clipRect.right) - xpos2;
        xpos2 = clipRect.right;
        tx2 += ((off / w) * umax); 
        tx2_2 += ((off / w) * umax); 

        if (tx2 >= 1.0f)
        {
          tx2 = 1.0f;
        }
        if (tx2_2 >= 1.0f)
        {
          tx2_2 = 1.0f;
        }
      }

      // Clipping top.
      if (ypos <	clipRect.top)
      {
        float off = clipRect.top - ypos;
        ypos = (float)clipRect.top;
        ty1 += ((off / h) * vmax);
        ty1_2 += ((off / h) * vmax);
      }

      // Clipping bottom.
      if (ypos2 >	clipRect.bottom)
      {
        float off = clipRect.bottom - ypos2;
        ypos2 = (float)clipRect.bottom;
        ty2 += ((off / h) * vmax);
        ty2_2 += ((off / h) * vmax);

        if (ty2 >= 1.0f)
        {
          ty2=1.0f;
        }
        if (ty2_2 >= 1.0f)
        {
          ty2_2=1.0f;
        }
      }
    }
  }

  xpos-=0.5f;
  ypos-=0.5f;
  xpos2-=0.5f;
  ypos2-=0.5f;

  //upper left
  float x1=matrix.ScaleFinalXCoord(xpos,ypos);
  float y1=matrix.ScaleFinalYCoord(xpos,ypos);
  float z1 = matrix.ScaleFinalZCoord(xpos,ypos);

  //bottom left
  float x2=matrix.ScaleFinalXCoord(xpos,ypos+nh);
  float y2=matrix.ScaleFinalYCoord(xpos,ypos+nh);
  float z2 = matrix.ScaleFinalZCoord(xpos,ypos+nh);

  //bottom right
  float x3=matrix.ScaleFinalXCoord(xpos+nw,ypos+nh);
  float y3=matrix.ScaleFinalYCoord(xpos+nw,ypos+nh);
  float z3 = matrix.ScaleFinalZCoord(xpos+nw,ypos+nh);

  //upper right
  float x4=matrix.ScaleFinalXCoord(xpos+nw,ypos);
  float y4=matrix.ScaleFinalYCoord(xpos+nw,ypos);
  float z4 = matrix.ScaleFinalZCoord(xpos+nw,ypos);

  CUSTOMVERTEX2 verts[4];
  //CUSTOMVERTEX verts[4];
  verts[0].x = x1; 
  verts[0].y = y1; 
  verts[0].z = z1;
  //verts[0].rhw = 1.0f;
  verts[0].tu = tx1;//u1;   
  verts[0].tv = ty1;//v1; 
  verts[0].tu2 =tx1_2 ;//u1*m_diffuseScaleU; 
  verts[0].tv2 =ty1_2 ;//v1*m_diffuseScaleV;
  verts[0].color = color;

  verts[1].x = x2; 
  verts[1].y = y2; 
  verts[1].z = z2;
  //verts[1].rhw = 1.0f;
  verts[1].tu = tx1;//u2;   
  verts[1].tv = ty2;//v1; 
  verts[1].tu2 = tx1_2;//u2*m_diffuseScaleU; 
  verts[1].tv2 = ty2_2;//v1*m_diffuseScaleV;
  verts[1].color = color;

  verts[2].x = x3; 
  verts[2].y = y3; 
  verts[2].z = z3;
  //verts[2].rhw = 1.0f;
  verts[2].tu = tx2;//u2;   
  verts[2].tv = ty2;//v2; 
  verts[2].tu2 = tx2_2;//u2*m_diffuseScaleU; 
  verts[2].tv2 = ty2_2;//v2*m_diffuseScaleV;
  verts[2].color = color;

  verts[3].x = x4; 
  verts[3].y = y4;
  verts[3].z = z4;
  //verts[3].rhw = 1.0f;
  verts[3].tu = tx2;//u1;   
  verts[3].tv = ty1;//v2; 
  verts[3].tu2 = tx2_2;//u1*m_diffuseScaleU; 
  verts[3].tv2 = ty1_2;//v2*m_diffuseScaleV;
  verts[3].color = color;

  FontEngineSetAlphaBlend(TRUE);
  m_pDevice->SetRenderState( D3DRS_SRCBLEND, D3DBLEND_SRCALPHA );
  m_pDevice->SetRenderState( D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA );

  // If clipping is enabled then set the render pipeline to apply the SetScissorRect() setting.
  // The caller must have already called SetScissorRect() to set the clipping rectangle.
  if (clipEnabled)
  {
    m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, TRUE);
  }
  else
  {
    m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE);
  }

  if (blendMode > 0)
  {
    m_pDevice->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG2, D3DTA_DIFFUSE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG2, D3DTA_DIFFUSE);

    if (blendMode == 1)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_COLOROP, D3DTOP_MODULATE);
    }
    else if (blendMode == 2)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_COLOROP, D3DTOP_BLENDTEXTUREALPHA);
    }

    m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG2, D3DTA_CURRENT);

    if (blendMode == 1)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    }
    else if (blendMode == 2)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAOP, D3DTOP_BLENDTEXTUREALPHA);
    }

    m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG2, D3DTA_CURRENT);

    // Disable the remainder of the texture stages.
    m_pDevice->SetTextureStageState(2, D3DTSS_COLOROP, D3DTOP_DISABLE);
    m_pDevice->SetTextureStageState(2, D3DTSS_ALPHAOP, D3DTOP_DISABLE);
  }

  m_pDevice->SetTexture(0, texture1->pTexture);
  if (blendMode > 0)
  {
    m_pDevice->SetTexture(1, texture2->pTexture);
  }

  m_pDevice->SetFVF(D3DFVF_CUSTOMVERTEX2);
  m_pDevice->DrawPrimitiveUP(D3DPT_TRIANGLEFAN, 2, verts, sizeof(CUSTOMVERTEX2));

  m_pDevice->SetTexture(0, NULL);
  if (blendMode > 0)
  {
    m_pDevice->SetTexture(1, NULL);
  }

  // Important - the scissor test (for clipping) must be disabled before return.
  // Clipping may not be defined other FontEngine calls that draw textures.
  m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE);
}

//*******************************************************************************************************************
void FontEngineDrawMaskedTexture(int textureNo1, float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax,
                                 int color, float m[3][4],
                                 int textureNo2, float uoff2, float voff2, float umax2, float vmax2)
{
  // textureNo1 - main image
  // textureNo2 - diffuse image
  // Draws textureNo1 masked by textureNo2.

  if (textureNo1 < 0 || textureNo1>=MAX_TEXTURES) return;
  if (textureNo2 < 0 || textureNo2>=MAX_TEXTURES) return;

  // Avoid drawing textures outside the viewport.
  D3DVIEWPORT9 viewport;
  m_pDevice->GetViewport(&viewport);

  if ((x+nw <= viewport.X) || 
    (y+nh <=viewport.Y) || 
    (x >= viewport.X+viewport.Width) || 
    (y >= viewport.Y+viewport.Height)) 
  {
    return;
  }

  // If clipping is enabled, avoid drawing textures completely outside the clip rectangle.
  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    if ((x+nw <= clipRect.left) || 
      (y+nh <=clipRect.top) || 
      (x >= clipRect.right) || 
      (y >= clipRect.bottom)) 
    {
      return;
    }
  }

  TransformMatrix matrix(m);
  FontEnginePresentTextures();

  TEXTURE_DATA_T* texture1;
  TEXTURE_DATA_T* texture2;
  texture1=&textureData[textureNo1];
  texture2=&textureData[textureNo2];

  float xpos=x;
  float xpos2=x+nw;
  float ypos=y;
  float ypos2=y+nh;

  float tx1=uoff;
  float tx2=umax;
  float ty1=voff;
  float ty2=vmax;

  float tx1_2=uoff2;
  float tx2_2=umax2;
  float ty1_2=voff2;
  float ty2_2=vmax2;

  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    // This clipping is done algorthimically for the texture being drawn.
    // This texture is maintained in the list of textures managed by the FontEngine.
    // Since the FontEngine does not store clip rectangles with the textures we cannot use the hardware to perform
    // the clipping.
    if (clipRect.top > 0 || clipRect.left > 0)
    {
      float w = xpos2 - xpos;
      float h = ypos2 - ypos;
      
      // Clipping on left side.
      if (xpos <	clipRect.left)
      {
        float off = clipRect.left - xpos;
        xpos = (float)clipRect.left;
        tx1 += ((off / w) * umax);
        tx1_2 += ((off / w) * umax);

        if (tx1 >= 1.0f)
        {
          tx1 = 1.0f;
        }
        if (tx1_2 >= 1.0f)
        {
          tx1_2 = 1.0f;
        }
      }

      // Clipping on right side.
      if (xpos2 >	clipRect.right)
      {
        float off = (clipRect.right) - xpos2;
        xpos2 = clipRect.right;
        tx2 += ((off / w) * umax); 
        tx2_2 += ((off / w) * umax); 

        if (tx2 >= 1.0f)
        {
          tx2 = 1.0f;
        }
        if (tx2_2 >= 1.0f)
        {
          tx2_2 = 1.0f;
        }
      }

      // Clipping top.
      if (ypos <	clipRect.top)
      {
        float off = clipRect.top - ypos;
        ypos = (float)clipRect.top;
        ty1 += ((off / h) * vmax);
        ty1_2 += ((off / h) * vmax);
      }

      // Clipping bottom.
      if (ypos2 >	clipRect.bottom)
      {
        float off = clipRect.bottom - ypos2;
        ypos2 = (float)clipRect.bottom;
        ty2 += ((off / h) * vmax);
        ty2_2 += ((off / h) * vmax);

        if (ty2 >= 1.0f)
        {
          ty2=1.0f;
        }
        if (ty2_2 >= 1.0f)
        {
          ty2_2=1.0f;
        }
      }
    }
  }

  xpos-=0.5f;
  ypos-=0.5f;
  xpos2-=0.5f;
  ypos2-=0.5f;

  //upper left
  float x1=matrix.ScaleFinalXCoord(xpos,ypos);
  float y1=matrix.ScaleFinalYCoord(xpos,ypos);
  float z1 = matrix.ScaleFinalZCoord(xpos,ypos);

  //bottom left
  float x2=matrix.ScaleFinalXCoord(xpos,ypos+nh);
  float y2=matrix.ScaleFinalYCoord(xpos,ypos+nh);
  float z2 = matrix.ScaleFinalZCoord(xpos,ypos+nh);

  //bottom right
  float x3=matrix.ScaleFinalXCoord(xpos+nw,ypos+nh);
  float y3=matrix.ScaleFinalYCoord(xpos+nw,ypos+nh);
  float z3 = matrix.ScaleFinalZCoord(xpos+nw,ypos+nh);

  //upper right
  float x4=matrix.ScaleFinalXCoord(xpos+nw,ypos);
  float y4=matrix.ScaleFinalYCoord(xpos+nw,ypos);
  float z4 = matrix.ScaleFinalZCoord(xpos+nw,ypos);

  CUSTOMVERTEX2 verts[4];
  verts[0].x = x1; 
  verts[0].y = y1; 
  verts[0].z = z1;
  verts[0].tu = tx1;
  verts[0].tv = ty1;
  verts[0].tu2 = tx1_2;
  verts[0].tv2 = ty1_2;
  verts[0].color = color;

  verts[1].x = x2; 
  verts[1].y = y2; 
  verts[1].z = z2;
  verts[1].tu = tx1;
  verts[1].tv = ty2;
  verts[1].tu2 = tx1_2;
  verts[1].tv2 = ty2_2;
  verts[1].color = color;

  verts[2].x = x3; 
  verts[2].y = y3; 
  verts[2].z = z3;
  verts[2].tu = tx2;
  verts[2].tv = ty2;
  verts[2].tu2 = tx2_2;
  verts[2].tv2 = ty2_2;
  verts[2].color = color;

  verts[3].x = x4; 
  verts[3].y = y4;
  verts[3].z = z4;
  verts[3].tu = tx2;
  verts[3].tv = ty1;
  verts[3].tu2 = tx2_2;
  verts[3].tv2 = ty1_2;
  verts[3].color = color;

  FontEngineSetAlphaBlend(TRUE);

  m_pDevice->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_SRCALPHA);
  m_pDevice->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA);

  // If clipping is enabled then set the render pipeline to apply the SetScissorRect() setting.
  // The caller must have already called SetScissorRect() to set the clipping rectangle.
  if (clipEnabled)
  {
    m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, TRUE);
  }
  else
  {
    m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE);
  }

  // This stage simply selects color and alpha from texture1.
  // Choose the alpha and color from the current texture (texture1).
  m_pDevice->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_MODULATE);
  m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_TEXTURE);
  m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_SELECTARG1);
  m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);

  // This stage blends the alpha of texture1 and texture2.
  // Select the color from the current texture (the output of stage 1) but select the alpha from the new texture (texture2)
  // and modulate (multiply) the alpha values of the new texture (texture2) and the current texture (output of stage 1).
  // Alpha values of zero in texture2 (the mask) result in alpha values of zero (transparent pixels) in the output.
  m_pDevice->SetTextureStageState(1, D3DTSS_COLOROP, D3DTOP_SELECTARG1);
  m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG1, D3DTA_CURRENT);
  m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
  m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);

  // This stage blends the masked resultant texture with the background (the diffuse texture).
  m_pDevice->SetTextureStageState(2, D3DTSS_COLOROP, D3DTOP_MODULATE);
  m_pDevice->SetTextureStageState(2, D3DTSS_COLORARG1, D3DTA_DIFFUSE);
  m_pDevice->SetTextureStageState(2, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
  m_pDevice->SetTextureStageState(2, D3DTSS_ALPHAARG1, D3DTA_DIFFUSE);

  // Disable the remainder of the texture stages.
  m_pDevice->SetTextureStageState(3, D3DTSS_COLOROP, D3DTOP_DISABLE);
  m_pDevice->SetTextureStageState(3, D3DTSS_ALPHAOP, D3DTOP_DISABLE);

  m_pDevice->SetTexture(0, texture1->pTexture);
  m_pDevice->SetTexture(1, texture2->pTexture);

  m_pDevice->SetFVF(D3DFVF_CUSTOMVERTEX2);
  m_pDevice->DrawPrimitiveUP(D3DPT_TRIANGLEFAN, 2, verts, sizeof(CUSTOMVERTEX2));

  m_pDevice->SetTexture(0, NULL);
  m_pDevice->SetTexture(1, NULL);

  // Important - the scissor test (for clipping) must be disabled before return.
  // Clipping may not be defined other FontEngine calls that draw textures.
  m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE);
}

//*******************************************************************************************************************
// blendMode 0 = ignore/bypass blending operation (completely disregard the use of textureNo2)
// blendMode 1 = diffuse blending (choose D3DTOP_MODULATE)
// blendMode 2 = linear blending (choose D3DTOP_BLENDTEXTUREALPHA)
void FontEngineDrawMaskedTexture2(int textureNo1,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax,
                                  int color, float m[3][4],
                                  int textureNo2, float uoff2, float voff2, float umax2, float vmax2,
                                  int textureNo3, float uoff3, float voff3, float umax3, float vmax3,
                                  int blendMode)
{
  // textureNo1 - main image
  // textureNo2 - diffuse image
  // textureNo3 - mask image
  // Draws textureNo1 blended with textureNo2 all masked by textureNo3.

  if (textureNo1 < 0 || textureNo1>=MAX_TEXTURES) return;
  if (textureNo2 < 0 || textureNo2>=MAX_TEXTURES) return;
  if (textureNo3 < 0 || textureNo3>=MAX_TEXTURES) return;

  // Avoid drawing textures outside the viewport.
  D3DVIEWPORT9 viewport;
  m_pDevice->GetViewport(&viewport);

  if ((x+nw <= viewport.X) || 
    (y+nh <=viewport.Y) || 
    (x >= viewport.X+viewport.Width) || 
    (y >= viewport.Y+viewport.Height)) 
  {
    return;
  }

  // If clipping is enabled, avoid drawing textures completely outside the clip rectangle.
  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    if ((x+nw <= clipRect.left) || 
      (y+nh <=clipRect.top) || 
      (x >= clipRect.right) || 
      (y >= clipRect.bottom)) 
    {
      return;
    }
  }

  TransformMatrix matrix(m);
  FontEnginePresentTextures();

  TEXTURE_DATA_T* texture1;
  TEXTURE_DATA_T* texture2;
  TEXTURE_DATA_T* texture3;
  texture1=&textureData[textureNo1];
  texture2=&textureData[textureNo2];
  texture3=&textureData[textureNo3];

  float xpos=x;
  float xpos2=x+nw;
  float ypos=y;
  float ypos2=y+nh;

  float tx1=uoff;
  float tx2=umax;
  float ty1=voff;
  float ty2=vmax;

  float tx1_2=uoff2;
  float tx2_2=umax2;
  float ty1_2=voff2;
  float ty2_2=vmax2; 

  float tx1_3=uoff3;
  float tx2_3=umax3;
  float ty1_3=voff3;
  float ty2_3=vmax3; 

  if (clipEnabled)
  {
    RECT clipRect;
    m_pDevice->GetScissorRect(&clipRect);

    // This clipping is done algorthimically for the texture being drawn.
    // This texture is maintained in the list of textures managed by the FontEngine.
    // Since the FontEngine does not store clip rectangles with the textures we cannot use the hardware to perform
    // the clipping.
    if (clipRect.top > 0 || clipRect.left > 0)
    {
      float w = xpos2 - xpos;
      float h = ypos2 - ypos;
      
      // Clipping on left side.
      if (xpos <	clipRect.left)
      {
        float off = clipRect.left - xpos;
        xpos = (float)clipRect.left;
        tx1 += ((off / w) * umax);
        tx1_2 += ((off / w) * umax);
        tx1_3 += ((off / w) * umax);

        if (tx1 >= 1.0f)
        {
          tx1 = 1.0f;
        }
        if (tx1_2 >= 1.0f)
        {
          tx1_2 = 1.0f;
        }
        if (tx1_3 >= 1.0f)
        {
          tx1_3 = 1.0f;
        }
      }

      // Clipping on right side.
      if (xpos2 >	clipRect.right)
      {
        float off = (clipRect.right) - xpos2;
        xpos2 = clipRect.right;
        tx2 += ((off / w) * umax); 
        tx2_2 += ((off / w) * umax); 
        tx2_3 += ((off / w) * umax); 

        if (tx2 >= 1.0f)
        {
          tx2 = 1.0f;
        }
        if (tx2_2 >= 1.0f)
        {
          tx2_2 = 1.0f;
        }
        if (tx2_3 >= 1.0f)
        {
          tx2_3 = 1.0f;
        }
      }

      // Clipping top.
      if (ypos <	clipRect.top)
      {
        float off = clipRect.top - ypos;
        ypos = (float)clipRect.top;
        ty1 += ((off / h) * vmax);
        ty1_2 += ((off / h) * vmax);
        ty1_3 += ((off / h) * vmax);
      }

      // Clipping bottom.
      if (ypos2 >	clipRect.bottom)
      {
        float off = clipRect.bottom - ypos2;
        ypos2 = (float)clipRect.bottom;
        ty2 += ((off / h) * vmax);
        ty2_2 += ((off / h) * vmax);
        ty2_3 += ((off / h) * vmax);

        if (ty2 >= 1.0f)
        {
          ty2=1.0f;
        }
        if (ty2_2 >= 1.0f)
        {
          ty2_2=1.0f;
        }
        if (ty2_3 >= 1.0f)
        {
          ty2_3=1.0f;
        }
      }
    }
  }

  xpos-=0.5f;
  ypos-=0.5f;
  xpos2-=0.5f;
  ypos2-=0.5f;

  //upper left
  float x1=matrix.ScaleFinalXCoord(xpos,ypos);
  float y1=matrix.ScaleFinalYCoord(xpos,ypos);
  float z1 = matrix.ScaleFinalZCoord(xpos,ypos);

  //bottom left
  float x2=matrix.ScaleFinalXCoord(xpos,ypos+nh);
  float y2=matrix.ScaleFinalYCoord(xpos,ypos+nh);
  float z2 = matrix.ScaleFinalZCoord(xpos,ypos+nh);

  //bottom right
  float x3=matrix.ScaleFinalXCoord(xpos+nw,ypos+nh);
  float y3=matrix.ScaleFinalYCoord(xpos+nw,ypos+nh);
  float z3 = matrix.ScaleFinalZCoord(xpos+nw,ypos+nh);

  //upper right
  float x4=matrix.ScaleFinalXCoord(xpos+nw,ypos);
  float y4=matrix.ScaleFinalYCoord(xpos+nw,ypos);
  float z4 = matrix.ScaleFinalZCoord(xpos+nw,ypos);

  CUSTOMVERTEX3 verts[4];
  verts[0].x = x1; 
  verts[0].y = y1; 
  verts[0].z = z1;
  verts[0].tu = tx1;
  verts[0].tv = ty1;
  verts[0].tu2 = tx1_2;
  verts[0].tv2 = ty1_2;
  verts[0].tu3 = tx1_3;
  verts[0].tv3 = ty1_3;
  verts[0].color = color;

  verts[1].x = x2; 
  verts[1].y = y2; 
  verts[1].z = z2;
  verts[1].tu = tx1;
  verts[1].tv = ty2;
  verts[1].tu2 = tx1_2;
  verts[1].tv2 = ty2_2;
  verts[1].tu3 = tx1_3;
  verts[1].tv3 = ty2_3;
  verts[1].color = color;

  verts[2].x = x3; 
  verts[2].y = y3; 
  verts[2].z = z3;
  verts[2].tu = tx2;
  verts[2].tv = ty2;
  verts[2].tu2 = tx2_2;
  verts[2].tv2 = ty2_2;
  verts[2].tu3 = tx2_3;
  verts[2].tv3 = ty2_3;
  verts[2].color = color;

  verts[3].x = x4; 
  verts[3].y = y4;
  verts[3].z = z4;
  verts[3].tu = tx2;
  verts[3].tv = ty1;
  verts[3].tu2 = tx2_2;
  verts[3].tv2 = ty1_2;
  verts[3].tu3 = tx2_3;
  verts[3].tv3 = ty1_3;
  verts[3].color = color;

  FontEngineSetAlphaBlend(TRUE);

  m_pDevice->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_SRCALPHA);
  m_pDevice->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA);

  // If clipping is enabled then set the render pipeline to apply the SetScissorRect() setting.
  // The caller must have already called SetScissorRect() to set the clipping rectangle.
  if (clipEnabled)
  {
    m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, TRUE);
  }
  else
  {
    m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE);
  }

  if (blendMode > 0)
  {
    // This stage blends the color and alpha of the current texture (texture1) with the background (diffuse image).
    m_pDevice->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG2, D3DTA_DIFFUSE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG2, D3DTA_DIFFUSE);

    // This stage blends the alpha of result of the last stage with texture2 (our own alpha mask).
    if (blendMode == 1)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_COLOROP, D3DTOP_MODULATE);
    }
    else if (blendMode == 2)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_COLOROP, D3DTOP_BLENDTEXTUREALPHA);
    }

    m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG2, D3DTA_CURRENT);

    if (blendMode == 1)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    }
    else if (blendMode == 2)
    {
      m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAOP, D3DTOP_BLENDTEXTUREALPHA);
    }

    m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG2, D3DTA_CURRENT);

    // This stage selects the color of the mask (arg1 = texture3) and blends it with the result of the
    // last stage (our alpha adjusted texture).
    m_pDevice->SetTextureStageState(2, D3DTSS_COLOROP, D3DTOP_SELECTARG1);
    m_pDevice->SetTextureStageState(2, D3DTSS_COLORARG1, D3DTA_CURRENT);
    m_pDevice->SetTextureStageState(2, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(2, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);

    // This stage blends the masked resultant texture with the background (the diffuse texture).
    m_pDevice->SetTextureStageState(3, D3DTSS_COLOROP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(3, D3DTSS_COLORARG1, D3DTA_DIFFUSE);
    m_pDevice->SetTextureStageState(3, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(3, D3DTSS_ALPHAARG1, D3DTA_DIFFUSE);

    // Disable the remainder of the texture stages.
    m_pDevice->SetTextureStageState(4, D3DTSS_COLOROP, D3DTOP_DISABLE);
    m_pDevice->SetTextureStageState(4, D3DTSS_ALPHAOP, D3DTOP_DISABLE);
  }

  m_pDevice->SetTexture(0, texture1->pTexture);
  m_pDevice->SetTexture(1, texture2->pTexture);
  m_pDevice->SetTexture(2, texture3->pTexture); 

  m_pDevice->SetFVF(D3DFVF_CUSTOMVERTEX3);
  m_pDevice->DrawPrimitiveUP(D3DPT_TRIANGLEFAN, 2, verts, sizeof(CUSTOMVERTEX3));

  m_pDevice->SetTexture(0, NULL);
  m_pDevice->SetTexture(1, NULL);
  m_pDevice->SetTexture(2, NULL);

  // Important - the scissor test (for clipping) must be disabled before return.
  // Clipping may not be defined other FontEngine calls that draw textures.
  m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE);
}

//*******************************************************************************************************************
void FontEnginePresentTextures()
{
  if(inPresentTextures)
  {
    char log[128];
    sprintf(log,"ERROR Fontengine:FontEnginePresentTextures() re-entrance\n");
    Log(log);
  }
  inPresentTextures=true;
  try
  {
    m_pDevice->SetFVF(D3DFVF_CUSTOMVERTEX);

    // Set the texture blending operations for default rendering.
    m_pDevice->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG2, D3DTA_DIFFUSE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG2, D3DTA_DIFFUSE);

    m_pDevice->SetTextureStageState(1, D3DTSS_COLOROP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG2, D3DTA_CURRENT);
    m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
    m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
    m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG2, D3DTA_CURRENT);

    m_pDevice->SetTextureStageState(2, D3DTSS_COLOROP, D3DTOP_DISABLE);
    m_pDevice->SetTextureStageState(2, D3DTSS_ALPHAOP, D3DTOP_DISABLE);

    for (int i=0; i < textureCount; ++i)
    {
      int index=textureZ[i];
      if (index < 0 || index >= MAX_TEXTURES) continue;

      TEXTURE_DATA_T* texture = &(textureData[index]);
      if( !texture->delayedRemove )
      {
        try
        {
          if (texture->dwNumTriangles!=0)
          {
            m_iTexturesInUse++;
            if (texture->updateVertexBuffer)
            {
              m_iVertexBuffersUpdated++;
              CUSTOMVERTEX* pVertices;
              texture->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, m_lock) ;
              memcpy(pVertices,texture->vertices, (texture->iv)*sizeof(CUSTOMVERTEX));
              texture->pVertexBuffer->Unlock();
            }

            if(texture->useAlphaBlend)
            {
              FontEngineSetAlphaBlend(TRUE);
            }
            else
            {
              FontEngineSetAlphaBlend(FALSE);
            }

            m_pDevice->SetTexture(0, texture->pTexture);
            m_pDevice->SetStreamSource(0, texture->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
            m_pDevice->SetIndices( texture->pIndexBuffer );
            m_pDevice->DrawIndexedPrimitive(D3DPT_TRIANGLELIST, 
                                            0,                      //baseVertexIndex,
                                            0,                      //minVertexIndex,
                                            texture->iv,            //NumVertices
                                            0,                      //StartIndex,
                                            texture->dwNumTriangles //MaxPrimitives
                                            );
          }
        }
        catch(...)
        {
          char log[128];
          sprintf(log,"ERROR Fontengine:FontEnginePresentTextures() exception drawing texture:%d\n", index);
          Log(log);
        }
        texture->dwNumTriangles = 0;
        texture->iv = 0;
        texture->updateVertexBuffer=false;
        textureZ[i]=0;
        texturePlace[index]->numRect = 0;
      }
    }
    textureCount=0;

    //#ifdef _DEBUG
    //	if (m_iTexturesInUse>0)
    //	{
    //		PrintStatistics();
    //	}
    //#endif
    m_iTexturesInUse=0;
    m_iVertexBuffersUpdated=0;
    m_iFontVertexBuffersUpdated=0;
  }
  catch(...)
  {
    char log[128];
    sprintf(log,"ERROR Fontengine:FontEnginePresentTextures()\n");
    Log(log);  
  }

  inPresentTextures=false;

  if(texturesToBeRemoved.size() > 0)
  {
    for( int i(texturesToBeRemoved.size()-1); i >= 0; i--)
    {
      int index(texturesToBeRemoved[i]);
      FontEngineRemoveTexture(index);
      texturesToBeRemoved.pop_back();
      char log[128];
      sprintf(log,"FontEnginePresentTextures -- delayed texture removal %i\n", index);
      Log(log);
    }
  }
}

//*******************************************************************************************************************
void FontEngineAddFont( int fontNumber,void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar,int maxVertices)
{
  if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
  if (fontTexture==NULL) return;
  if (firstChar<0 || firstChar>endChar) return;

  fontData[fontNumber].vertices = new CUSTOMVERTEX[MaxNumfontVertices];
  for (int i=0; i < MaxNumfontVertices;++i)
  {
    fontData[fontNumber].vertices[i].z=0;
    //fontData[fontNumber].vertices[i].rhw=1;
  }

  fontData[fontNumber].iFirstChar    = firstChar;
  fontData[fontNumber].iEndChar      = endChar;
  fontData[fontNumber].fTextureScale = textureScale;
  fontData[fontNumber].fTextureWidth = textureWidth;
  fontData[fontNumber].fTextureHeight= textureHeight;
  fontData[fontNumber].pTexture      = (LPDIRECT3DTEXTURE9)fontTexture;
  fontData[fontNumber].fSpacingPerChar = fSpacingPerChar;
  fontData[fontNumber].iv=0;
  fontData[fontNumber].dwNumTriangles=0;

  LPDIRECT3DVERTEXBUFFER9 g_pVB = NULL;
  int hr=m_pDevice->CreateVertexBuffer(	MaxNumfontVertices*sizeof(CUSTOMVERTEX),
                                        D3DUSAGE_WRITEONLY, D3DFVF_CUSTOMVERTEX,
                                        m_ipoolFormat, 
                                        &g_pVB, 
                                        NULL) ;
  fontData[fontNumber].pVertexBuffer=g_pVB;
  m_pDevice->CreateIndexBuffer(	MaxNumfontVertices *sizeof(WORD),
                                D3DUSAGE_WRITEONLY, D3DFMT_INDEX16, m_ipoolFormat, 
                                &fontData[fontNumber].pIndexBuffer, NULL ) ;
  WORD* pIndices;
  int triangle=0;
  fontData[fontNumber].pIndexBuffer->Lock(0,0,(VOID**)&pIndices,0);
  for (int i=0; i < MaxNumfontVertices;i+=6)
  {
    if (i+5 < MaxNumfontVertices)
    {
      pIndices[i+0]=triangle*4+1;
      pIndices[i+1]=triangle*4+0;
      pIndices[i+2]=triangle*4+3;
      pIndices[i+3]=triangle*4+2;
      pIndices[i+4]=triangle*4+1;
      pIndices[i+5]=triangle*4+3;
    }
    triangle++;
  }
  fontData[fontNumber].pIndexBuffer->Unlock();
}

//*******************************************************************************************************************
void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue1, float fValue2, float fValue3, float fValue4)
{
  if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
  if (index < 0     || index > MAX_TEXTURE_COORDS) return;
  if (subindex < 0  || subindex > 3) return;
  fontData[fontNumber].textureCoord[index][0]=fValue1;
  fontData[fontNumber].textureCoord[index][1]=fValue2;
  fontData[fontNumber].textureCoord[index][2]=fValue3;
  fontData[fontNumber].textureCoord[index][3]=fValue4;
}

// Updates a vertex in the memory buffer if needed
void UpdateVertex(TransformMatrix& matrix, FONT_DATA_T* pFont, CUSTOMVERTEX* pVertex, float x, float y, float tu, float tv, DWORD color)
{
  float x1 = matrix.ScaleFinalXCoord(x,y);
  float y1 = matrix.ScaleFinalYCoord(x,y);
  float z = matrix.ScaleFinalZCoord(x,y);
  if(pVertex->x != x1 || pVertex->y != y1 || pVertex->z != z || pVertex->tu != tu || pVertex->tv != tv || pVertex->color != color)
  {
	  pVertex->x = x1;
	  pVertex->y = y1;
	  pVertex->z = z;
	  pVertex->tu = tu;
	  pVertex->tv = tv;
	  pVertex->color = color;
	  pFont->updateVertexBuffer = true;		// We need to update gfx card vertex buffer
  }
  }

  //*******************************************************************************************************************
  void FontEngineDrawText3D(int fontNumber, void* textVoid, int xposStart, int yposStart, DWORD intColor, int maxWidth, float m[3][4])
  {
  if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
  if (m_pDevice==NULL) return;
  if (fontData[fontNumber].pVertexBuffer==NULL) return;
  if (textVoid==NULL) return;

  TransformMatrix matrix(m);

  WCHAR* text = (WCHAR*)textVoid;

  FONT_DATA_T* font = &(fontData[fontNumber]);
  float xpos = (float)xposStart;
  float ypos = (float)yposStart;
  xpos -= fontData[fontNumber].fSpacingPerChar;
  xpos-=0.5f;
  float fStartX = xpos;
  ypos -=0.5f;
  float fStartY = ypos;

  float yoff    = (font->textureCoord[0][3]-font->textureCoord[0][1])*font->fTextureHeight;
  float fScaleX = font->fTextureWidth  / font->fTextureScale;
  float fScaleY = font->fTextureHeight / font->fTextureScale;
  float fSpacing= 2 * font->fSpacingPerChar;

  unsigned int off=(int)(fontData[fontNumber].fSpacingPerChar+1);

  if (maxWidth <=0) 
  {
    maxWidth=2000;
  }

  float totalWidth = 0;
  float lineWidths[MAX_TEXT_LINES];
  int lineNr=0;
  for (int i=0; i < (int)wcslen(text);++i)
  {
    WCHAR c=text[i];
    if (c == '\n')
    {
      // don't overflow the array
      if (lineNr >= MAX_TEXT_LINES-1)
        continue;

      lineWidths[lineNr]=totalWidth;
      totalWidth=0;
      xpos = fStartX;
      ypos += yoff;
      lineNr++;
      continue;
    }
    else if (c < font->iFirstChar || c >= font->iEndChar)
      continue;
    else if (totalWidth >= maxWidth)		// Reached max width?
      continue;							// Skip until row break or end of text

    int index=c-font->iFirstChar;
	  float tx1 = font->textureCoord[index][0];
	  float tx2 = font->textureCoord[index][2];

	  float w = (tx2-tx1) * fScaleX;
	  totalWidth += (w - fSpacing);
	  xpos += (w - fSpacing);
	  lineWidths[lineNr]=totalWidth;
  }

  totalWidth=0;
  xpos = fStartX;
  ypos = fStartY;
  lineNr=0;

  for (int i=0; i < (int)wcslen(text);++i)
  {
    WCHAR c=text[i];
    if (c == '\n')
    {
      totalWidth=0;
      xpos = fStartX;
      ypos += yoff;
      lineNr++;
      continue;
    }
    else if (c < font->iFirstChar || c >= font->iEndChar)
      continue;
    else if (totalWidth >= maxWidth)		// Reached max width?
      continue;							// Skip until row break or end of text

    int index=c-font->iFirstChar;
    float tx1 = font->textureCoord[index][0];
    float ty1 = font->textureCoord[index][1];
    float tx2 = font->textureCoord[index][2];
    float ty2 = font->textureCoord[index][3];

    float w = (tx2-tx1) * fScaleX;
    float h = (ty2-ty1) * fScaleY;

    // Will hold clipped coordinates
    float xpos1 = xpos;
    float ypos1 = ypos;
    float xpos2 = xpos + w;
    float ypos2 = ypos + h;

    // Check if inside viewport.
    // Avoid drawing text that is not inside the viewport.
    D3DVIEWPORT9 viewport;
    m_pDevice->GetViewport(&viewport);

    if(xpos1 < (viewport.X + viewport.Width) && xpos2 >= viewport.X &&
      ypos1 < (viewport.Y + viewport.Height) && ypos2 >= viewport.Y)
    {
      if (clipEnabled)
      {
        // Get the clip rectangle.
        RECT clipRect;
        m_pDevice->GetScissorRect(&clipRect);
        float minX = clipRect.left;
        float minY = clipRect.top;
        float maxX = clipRect.right;
        float maxY = clipRect.bottom;

        // A clip rectangle is defined.  Deteremine if the character is inside the clip rectangle.
        // If the character is inside the clip rectangle then clip it as necessary at the clip rectangle boundary.
        // If the character is not inside the clip rectangle then move on to the next character (continue).
        if (xpos1 < maxX && xpos2 >= minX &&
          ypos1 < maxY && ypos2 >= minY)
        {
          // Clipping is performed manually here, not in the render pipeline (we don't set SCISSORTESTENABLE).
          if(xpos1 < minX)
          {
            tx1 += (minX - xpos1) / fScaleX;
            xpos1 += minX - xpos1;
          }
          if(xpos2 > maxX)
          {
            tx2 -= (xpos2 - maxX) / fScaleX;
            xpos2 -= xpos2 - maxX;
          }
          if(ypos1 < minY)
          {
            ty1 += (minY - ypos1) / fScaleY;
            ypos1 += minY - ypos1;
          }
          if(ypos2 > maxY)
          {
            ty2 -= (ypos2 - maxY) / fScaleY;
            ypos2 -= ypos2 - maxY;
          }
        }
        else
        {
          continue;
        }
      }

      int alpha1=intColor;
      int alpha2=intColor;
      if ((lineNr >= MAX_TEXT_LINES || lineWidths[lineNr] >= maxWidth) && totalWidth+50 >= maxWidth && maxWidth > 0 && maxWidth < 2000)
      {
        int maxAlpha=intColor>>24;
        float diff=(float)(maxWidth-totalWidth);
        diff/=50.0f;
        if (diff>1) diff = 1;
        alpha1=(int)(maxAlpha * diff);

        diff=(float)(maxWidth-totalWidth);
        diff+=(w - fSpacing);
        diff/=50.0f;
        if (diff>1) diff = 1;
        alpha2=(int)(maxAlpha * diff);

        if (alpha1<0) alpha1=0;
        if (alpha1>0xff) alpha1=maxAlpha;
        if (alpha2<0) alpha2=0;
        if (alpha2>0xff) alpha2=maxAlpha;

        alpha1 <<=24;
        alpha2 <<=24;
        alpha1|= (intColor & 0xffffff);
        alpha2|= (intColor & 0xffffff);
      }
      int vertices=font->iv;
      UpdateVertex(matrix,font, &font->vertices[font->iv++], xpos1, ypos1, tx1, ty1, alpha2);
      UpdateVertex(matrix,font, &font->vertices[font->iv++], xpos1, ypos2, tx1, ty2, alpha2);
      UpdateVertex(matrix,font, &font->vertices[font->iv++], xpos2, ypos2, tx2, ty2, alpha1);
      UpdateVertex(matrix,font, &font->vertices[font->iv++], xpos2, ypos1, tx2, ty1, alpha1);
      //UpdateVertex(font, &font->vertices[font->iv++], xpos1, ypos2, tx1, ty2, alpha2);
      //UpdateVertex(font, &font->vertices[font->iv++], xpos2, ypos1, tx2, ty1, alpha1);

      font->dwNumTriangles += 2;
      if (font->iv > (MaxNumfontVertices-12))
      {
        FontEnginePresentTextures();
        FontEnginePresent3D(fontNumber);
        font->dwNumTriangles = 0;
        font->iv = 0;
      }
    }
    totalWidth += (w - fSpacing);
    xpos += (w - fSpacing);
  }
}

//*******************************************************************************************************************
void FontEnginePresent3D(int fontNumber)
{
  if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
  if (fontData[fontNumber].dwNumTriangles==0) return;

  FONT_DATA_T* font = &(fontData[fontNumber]);
  try
  {
    if (font->dwNumTriangles !=0)
	  {
      if (font->updateVertexBuffer)
      {
        m_iFontVertexBuffersUpdated++;
        CUSTOMVERTEX* pVertices;
        font->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, m_lock) ;
        memcpy(pVertices,font->vertices, (font->iv)*sizeof(CUSTOMVERTEX));
        font->pVertexBuffer->Unlock();
      }

      // Set the texture blending operations for default rendering.
      m_pDevice->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_MODULATE);
      m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_TEXTURE);
      m_pDevice->SetTextureStageState(0, D3DTSS_COLORARG2, D3DTA_DIFFUSE);
      m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
      m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
      m_pDevice->SetTextureStageState(0, D3DTSS_ALPHAARG2, D3DTA_DIFFUSE);

      m_pDevice->SetTextureStageState(1, D3DTSS_COLOROP, D3DTOP_MODULATE);
      m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG1, D3DTA_TEXTURE);
      m_pDevice->SetTextureStageState(1, D3DTSS_COLORARG2, D3DTA_CURRENT);
      m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAOP, D3DTOP_MODULATE);
      m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG1, D3DTA_TEXTURE);
      m_pDevice->SetTextureStageState(1, D3DTSS_ALPHAARG2, D3DTA_CURRENT);

      m_pDevice->SetTextureStageState(2, D3DTSS_COLOROP, D3DTOP_DISABLE);
      m_pDevice->SetTextureStageState(2, D3DTSS_ALPHAOP, D3DTOP_DISABLE);

      FontEngineSetAlphaBlend(1);
      m_pDevice->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE);

      m_pDevice->SetTexture(0, font->pTexture);
      m_pDevice->SetStreamSource(0, font->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
      m_pDevice->SetIndices( font->pIndexBuffer );
      m_pDevice->SetFVF(D3DFVF_CUSTOMVERTEX);
      m_pDevice->DrawIndexedPrimitive(D3DPT_TRIANGLELIST, 
                                      0,                    //baseVertexIndex,
                                      0,                    //minVertexIndex,
                                      font->iv,             //NumVertices
                                      0,                    //StartIndex,
                                      font->dwNumTriangles //MaxPrimitives
                                      );
      font->dwNumTriangles = 0;
      font->iv = 0;
      font->updateVertexBuffer=false;
    }
  }
  catch(...)
  {	
    char log[128];
    sprintf(log,"ERROR Fontengine:FontEnginePresent3D(%i) exception \n", fontNumber);
    Log(log);
    font->dwNumTriangles = 0;
    font->iv = 0;
    font->updateVertexBuffer=false;
  }
}
 
//*******************************************************************************************************************
void FontEngineRemoveFont(int fontNumber)
{
  // Important to set it to NULL  otherwise the textures, etc. will not be freed on release
  m_pDevice->SetStreamSource(0, NULL, 0, 0 );
  if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
  if (fontData[fontNumber].pVertexBuffer!=NULL) 
  {
    fontData[fontNumber].pVertexBuffer->Release();
  }
  fontData[fontNumber].pVertexBuffer=NULL;
  if (fontData[fontNumber].pIndexBuffer!=NULL) 
  {
    fontData[fontNumber].pIndexBuffer->Release();
  }
  fontData[fontNumber].pIndexBuffer=NULL;

  if (fontData[fontNumber].vertices!=NULL)
  {
    delete[] fontData[fontNumber].vertices;
  }
  fontData[fontNumber].vertices=NULL;
  fontData[fontNumber].pTexture=NULL;
}

void PrintStatistics()
{
  char log[128];
  sprintf(log,"fontengine: Textures InUse:%d VertexBuffer Updates:%d %d\n",m_iTexturesInUse, m_iVertexBuffersUpdated,m_iFontVertexBuffersUpdated);
  OutputDebugString(log);
}

//*******************************************************************************************************************
void FontEngineSetTexture(void* surface)
{
  try
  {
    //LPDIRECT3DSURFACE9 pSurface = (LPDIRECT3DSURFACE9)surface;
    //void *pContainer = NULL;
    //int hr=pSurface->GetContainer(IID_IDirect3DTexture9,&pContainer);

    //LPDIRECT3DTEXTURE9 pTexture = (LPDIRECT3DTEXTURE9)pContainer;
    LPDIRECT3DTEXTURE9 pTexture = (LPDIRECT3DTEXTURE9)surface;
    m_pDevice->SetTexture(0, pTexture);
    //pTexture->Release();
  }
  catch(...)
  {
    Log("error in FontEngineSetTexture");
  }
}

//*******************************************************************************************************************
void FontEngineDrawSurface( int fx, int fy, int nw, int nh, int dstX, int dstY,
                            int dstWidth, int dstHeight, void* surface )
{
  try
  {
    IDirect3DSurface9* pBackBuffer;

    FontEngineSetAlphaBlend(FALSE);
    m_pDevice->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &pBackBuffer);

    LPDIRECT3DSURFACE9 pSurface = (LPDIRECT3DSURFACE9)surface;
    if(pBackBuffer)
    {
      RECT srcRect,dstRect;
      srcRect.left=(int)fx;
      srcRect.top =(int)fy;
      srcRect.right=srcRect.left+(int)nw;
      srcRect.bottom=srcRect.top+(int)nh;

      dstRect.left=(int)dstX;
      dstRect.top =(int)dstY;
      dstRect.right=dstRect.left+(int)dstWidth;
      dstRect.bottom=dstRect.top+(int)dstHeight;
      // IMPORTANT: rSrcVid has to be aligned on mod2 for yuy2->rgb conversion with StretchRect!!!
      srcRect.left &= ~1; srcRect.right &= ~1;
      srcRect.top &= ~1; srcRect.bottom &= ~1;
      m_pDevice->StretchRect(pSurface, &srcRect, pBackBuffer, &dstRect, m_Filter);

      pBackBuffer->Release();
    }
  }
  catch(...)
  {
  }
}
