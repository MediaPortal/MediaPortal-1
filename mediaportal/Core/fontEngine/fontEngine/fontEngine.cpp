// fontEngine.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "fontEngine.h"

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

#define MaxNumfontVertices 600
#define MAX_FONTS			20
#define MAX_FONT_CACHE      40

// A structure for our custom vertex type
struct CUSTOMVERTEX
{
    FLOAT x, y, z, rhw; // The transformed position for the vertex
    DWORD color;        // The vertex color
    FLOAT       tu, tv;   // The texture coordinates
};

// Our custom FVF, which describes our custom vertex structure
#define D3DFVF_CUSTOMVERTEX (D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX1)


struct FONT_DATA_T
{
	int						iFirstChar;
	int						iEndChar;
	float					fTextureScale;
	float					fTextureWidth;
	float					fTextureHeight;
	int     				iMaxVertices;
	float   				fSpacingPerChar;
	LPDIRECT3DTEXTURE9		pTexture;
	LPDIRECT3DVERTEXBUFFER9	pVertexBuffer;
	float					textureCoord[MaxNumfontVertices][4];
} ;
struct FONT_CACHE_T
{
	int						fontNumber;
	int						color;
	int						xpos;
	int						ypos;
	LPDIRECT3DVERTEXBUFFER9	pVertexBuffer;
	int                     dwNumTriangles;
	DWORD                   drawn;
	char*                   text;
};

static FONT_DATA_T*			fontData = new FONT_DATA_T[MAX_FONTS];
static LPDIRECT3DDEVICE9	m_pDevice=NULL;	
static FONT_CACHE_T*		fontCache=NULL;

void AddToCache(int fontNumber, int color, int xpos, int ypos, LPDIRECT3DVERTEXBUFFER9 pVertexBuffer, int Triangles,char* text)
{
	DWORD minDrawn=9999999;
	int   selected=0;
	for (int i=0; i < MAX_FONT_CACHE; ++i)
	{
		if (fontCache[i].fontNumber==-1)
		{
			selected=i;
			break;
		}
		if (fontCache[i].drawn < minDrawn)
		{
			selected=i;
			minDrawn=fontCache[i].drawn;
		}
	}

	fontCache[selected].fontNumber=fontNumber;
	fontCache[selected].color=color;
	fontCache[selected].xpos=xpos;
	fontCache[selected].ypos=ypos;
	fontCache[selected].pVertexBuffer=pVertexBuffer; // copy this!
	fontCache[selected].dwNumTriangles=Triangles;
	fontCache[selected].drawn=0;
}

LPDIRECT3DVERTEXBUFFER9 GetCache(int fontNumber,int color, int xpos, int ypos,char* text, int *Triangles)
{
	for (int i=0; i < MAX_FONT_CACHE; ++i)
	{
		if (fontCache[i].fontNumber==fontNumber &&
			fontCache[i].xpos==xpos &&
			fontCache[i].ypos==ypos &&
			fontCache[i].color==color)
		{
			if (strcmp(fontCache[i].text,text)==0)
			{
				fontCache[i].drawn++;
				*Triangles = fontCache[i].dwNumTriangles;
				return fontCache[i].pVertexBuffer;
			}
		}
	}
	return NULL;
}

void AllocFontCache()
{
	if (fontCache!=NULL) return;
	fontCache = new FONT_CACHE_T[MAX_FONT_CACHE];
	for (int i=0; i < MAX_FONT_CACHE; ++i)
	{
		fontCache[i].fontNumber=-1;
		fontCache[i].color=-1;
		fontCache[i].xpos=-1;
		fontCache[i].ypos=-1;
		fontCache[i].pVertexBuffer=NULL;
		fontCache[i].dwNumTriangles=0;
		fontCache[i].drawn=0;
	}
}

void AddFont(void* device, int fontNumber,void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar,int maxVertices)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (fontTexture==NULL) return;
	if (firstChar<0 || firstChar>endChar) return;
	m_pDevice=(LPDIRECT3DDEVICE9)device;
	D3DCAPS9 caps;
	int mem=m_pDevice->GetAvailableTextureMem();
	m_pDevice->GetDeviceCaps(&caps);


	fontData[fontNumber].iFirstChar    = firstChar;
	fontData[fontNumber].iEndChar      = endChar;
	fontData[fontNumber].fTextureScale = textureScale;
	fontData[fontNumber].fTextureWidth = textureWidth;
	fontData[fontNumber].fTextureHeight= textureHeight;
	fontData[fontNumber].iMaxVertices  = maxVertices;
	fontData[fontNumber].pTexture      = (LPDIRECT3DTEXTURE9)fontTexture;
	fontData[fontNumber].fSpacingPerChar = fSpacingPerChar;

	LPDIRECT3DVERTEXBUFFER9 g_pVB        = NULL;
	int hr=m_pDevice->CreateVertexBuffer(	maxVertices*sizeof(CUSTOMVERTEX),
											0, D3DFVF_CUSTOMVERTEX,
											D3DPOOL_MANAGED, 
											&g_pVB, 
											NULL) ;
	fontData[fontNumber].pVertexBuffer=g_pVB;
	int x=123;
}

void SetCoordinate(int fontNumber, int index, int subindex, float fValue)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (index < 0     || index > MaxNumfontVertices) return;
	if (subindex < 0  || subindex > 3) return;
	fontData[fontNumber].textureCoord[index][subindex]=fValue;
}

void Present3D()
{
	m_pDevice->Present(NULL,NULL,NULL,NULL);
}

void DrawText3D(int fontNumber, char* text, int xposStart, int yposStart, DWORD intColor)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (m_pDevice==NULL) return;
	if (fontData[fontNumber].pVertexBuffer==NULL) return;

	FONT_DATA_T* font = &(fontData[fontNumber]);
	float xpos = (float)xposStart;
	float ypos = (float)yposStart;
	xpos -= fontData[fontNumber].fSpacingPerChar;
	xpos-=0.5f;
	float fStartX = xpos;
	ypos -=0.5f;

	float yoff    = (font->textureCoord[0][3]-font->textureCoord[0][1])*font->fTextureHeight;
	float fScaleX = font->fTextureWidth  / font->fTextureScale;
	float fScaleY = font->fTextureHeight / font->fTextureScale;
	float fSpacing= 2 * font->fSpacingPerChar;

	CUSTOMVERTEX* pVertices=NULL;
	font->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, D3DLOCK_DISCARD ) ;

	int dwNumTriangles=0;
	int iv=0;
	
	for (int i=0; i < (int)strlen(text);++i)
	{
        char c=text[i];
		if (c == '\n')
		{
			xpos = fStartX;
			ypos += yoff;
		}

		if (c < font->iFirstChar || c >= font->iEndChar )
			continue;

        int index=c-font->iFirstChar;
		float tx1 = font->textureCoord[index][0];
		float ty1 = font->textureCoord[index][1];
		float tx2 = font->textureCoord[index][2];
		float ty2 = font->textureCoord[index][3];

		float w = (tx2-tx1) * fScaleX;
		float h = (ty2-ty1) * fScaleY;

		if (xpos<0 || xpos+2 > 768 ||
			ypos<0 || ypos+h > 576+100)
		{
			c=' ';
		}

		if (c != ' ')
		{
			float xpos2=xpos+w;
			float ypos2=ypos+h;
			pVertices[iv].rhw=1.0f;  pVertices[iv].z=0.0f;  pVertices[iv].x=xpos ;  pVertices[iv].y=ypos2 ; pVertices[iv].color=intColor;pVertices[iv].tu=tx1; pVertices[iv].tv=ty2;iv++;
			pVertices[iv].rhw=1.0f;  pVertices[iv].z=0.0f;  pVertices[iv].x=xpos ;  pVertices[iv].y=ypos  ; pVertices[iv].color=intColor;pVertices[iv].tu=tx1; pVertices[iv].tv=ty1;iv++;
			pVertices[iv].rhw=1.0f;  pVertices[iv].z=0.0f;  pVertices[iv].x=xpos2;  pVertices[iv].y=ypos2 ; pVertices[iv].color=intColor;pVertices[iv].tu=tx2; pVertices[iv].tv=ty2;iv++;
			pVertices[iv].rhw=1.0f;  pVertices[iv].z=0.0f;  pVertices[iv].x=xpos2;  pVertices[iv].y=ypos  ; pVertices[iv].color=intColor;pVertices[iv].tu=tx2; pVertices[iv].tv=ty1;iv++;
			pVertices[iv].rhw=1.0f;  pVertices[iv].z=0.0f;  pVertices[iv].x=xpos2;  pVertices[iv].y=ypos2 ; pVertices[iv].color=intColor;pVertices[iv].tu=tx2; pVertices[iv].tv=ty2;iv++;
			pVertices[iv].rhw=1.0f;  pVertices[iv].z=0.0f;  pVertices[iv].x=xpos ;  pVertices[iv].y=ypos  ; pVertices[iv].color=intColor;pVertices[iv].tu=tx1; pVertices[iv].tv=ty1;iv++;

			dwNumTriangles += 2;
			if (iv > (MaxNumfontVertices-12))
			{
				font->pVertexBuffer->Unlock();
				m_pDevice->SetTexture(0, font->pTexture);
				m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
				m_pDevice->SetStreamSource(0, font->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
				m_pDevice->DrawPrimitive(D3DPT_TRIANGLELIST, 0, dwNumTriangles);
				m_pDevice->SetTexture(0, NULL);
				dwNumTriangles = 0;
				iv = 0;
				font->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, D3DLOCK_DISCARD ) ;
			}
		}

		xpos += w - fSpacing;
	}

	if (iv > 0)
	{
		font->pVertexBuffer->Unlock();
		m_pDevice->SetTexture(0, font->pTexture);
		m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
		m_pDevice->SetStreamSource(0, font->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
		m_pDevice->DrawPrimitive(D3DPT_TRIANGLELIST, 0, dwNumTriangles);
		m_pDevice->SetTexture(0, NULL);
		dwNumTriangles = 0;
		iv = 0;
	}
	else
	{
		font->pVertexBuffer->Unlock();
	}
}