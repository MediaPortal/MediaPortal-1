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

#define MAX_TEXTURES			200
#define MAX_TEXTURE_COORDS		300
#define MaxNumfontVertices		2400
#define MAX_FONTS				20

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
	float   				fSpacingPerChar;
	LPDIRECT3DTEXTURE9		pTexture;
	LPDIRECT3DVERTEXBUFFER9	pVertexBuffer;
	float					textureCoord[MAX_TEXTURE_COORDS][4];
	int                     iv;
	int                     dwNumTriangles;
} ;

struct TEXTURE_DATA_T
{	
	int						hashCode;
	LPDIRECT3DTEXTURE9		pTexture;
	LPDIRECT3DVERTEXBUFFER9	pVertexBuffer;
	int                     iv;
	int                     dwNumTriangles;
};

static FONT_DATA_T*			fontData    = new FONT_DATA_T[MAX_FONTS];
static TEXTURE_DATA_T*		textureData = new TEXTURE_DATA_T[MAX_TEXTURES];
static LPDIRECT3DDEVICE9	m_pDevice=NULL;	
static int                  textureZ[MAX_TEXTURES];
int                         textureCount;
void FontEngineInitialize()
{
	textureCount=0;
	static bool initialized=false;
	if (!initialized)
	{
		for (int i=0; i < MAX_FONTS;++i)
		{
			fontData[i].pVertexBuffer=NULL;
			fontData[i].pTexture=NULL;
		}
		for (int i=0; i < MAX_TEXTURES;++i)
		{
			textureData[i].hashCode=-1;
			textureData[i].dwNumTriangles=0;
			textureData[i].iv=0;
			textureData[i].pVertexBuffer=NULL;
			textureData[i].pTexture=NULL;
		}
		initialized=true;
	}
}

void FontEngineRemoveTexture(int textureNo)
{
	textureData[textureNo].hashCode=-1;
	textureData[textureNo].dwNumTriangles=0;
	textureData[textureNo].iv=0;
	if (textureData[textureNo].pVertexBuffer!=NULL)
		textureData[textureNo].pVertexBuffer->Release();
	textureData[textureNo].pVertexBuffer=NULL;
	textureData[textureNo].pTexture=NULL;
}

int FontEngineAddTexture(int hashCode, void* texture)
{
	int selected=0;
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
	textureData[selected].hashCode=hashCode;
	textureData[selected].pTexture=(LPDIRECT3DTEXTURE9)texture;
	
	if (textureData[selected].pVertexBuffer==NULL)
	{
		m_pDevice->CreateVertexBuffer(	MaxNumfontVertices*sizeof(CUSTOMVERTEX),
											0, D3DFVF_CUSTOMVERTEX,
											D3DPOOL_MANAGED, 
											&textureData[selected].pVertexBuffer, 
											NULL) ;
	}
	return selected;
}

void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color)
{
	textureZ[textureCount]=textureNo;
	textureCount++;
	TEXTURE_DATA_T* texture=&textureData[textureNo];
	CUSTOMVERTEX*	pVertices=NULL;
	texture->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, D3DLOCK_DISCARD ) ;
	int iv=texture->iv;
    pVertices[iv].x= x- 0.5f; pVertices[iv].y=y+nh- 0.5f; pVertices[iv].z= 0.0f; pVertices[iv].rhw=1.0f ;
    pVertices[iv].color = (int)color;
    pVertices[iv].tu = uoff;
    pVertices[iv].tv = voff+vmax;
	iv++;

    pVertices[iv].x= x- 0.5f; pVertices[iv].y= y- 0.5f; pVertices[iv].z= 0.0f; pVertices[iv].rhw= 1.0f;
    pVertices[iv].color = (int)color;
    pVertices[iv].tu = uoff;
    pVertices[iv].tv = voff;
	iv++;

    pVertices[iv].x=  x+nw- 0.5f; pVertices[iv].y=y+nh- 0.5f;pVertices[iv].z= 0.0f; pVertices[iv].rhw= 1.0f;
    pVertices[iv].color = (int)color;
    pVertices[iv].tu = uoff+umax;
    pVertices[iv].tv = voff+vmax;
	iv++;

    pVertices[iv].x= x+nw- 0.5f; pVertices[iv].y= y- 0.5f; pVertices[iv].z=0.0f; pVertices[iv].rhw=1.0f ;
    pVertices[iv].color = (int)color;
    pVertices[iv].tu = uoff+umax;
    pVertices[iv].tv = voff;
	iv++;
	texture->iv=iv;
	texture->dwNumTriangles+=2;
	texture->pVertexBuffer->Unlock();
}

void FontEnginePresentTextures()
{
	
	for (int i=0; i < textureCount; ++i)
	{
		int index=textureZ[i];
		TEXTURE_DATA_T* texture = &(textureData[index]);
		if (texture->dwNumTriangles!=0)
		{
			m_pDevice->SetTexture(0, texture->pTexture);
			m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
			m_pDevice->SetStreamSource(0, texture->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
			m_pDevice->DrawPrimitive(D3DPT_TRIANGLESTRIP, 0, texture->dwNumTriangles);
			m_pDevice->SetTexture(0, NULL);
			texture->dwNumTriangles = 0;
			texture->iv = 0;
		}
	}
	textureCount=0;
}


void FontEngineAddFont(void* device, int fontNumber,void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar,int maxVertices)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (fontTexture==NULL) return;
	if (firstChar<0 || firstChar>endChar) return;
	m_pDevice=(LPDIRECT3DDEVICE9)device;


	fontData[fontNumber].iFirstChar    = firstChar;
	fontData[fontNumber].iEndChar      = endChar;
	fontData[fontNumber].fTextureScale = textureScale;
	fontData[fontNumber].fTextureWidth = textureWidth;
	fontData[fontNumber].fTextureHeight= textureHeight;
	fontData[fontNumber].pTexture      = (LPDIRECT3DTEXTURE9)fontTexture;
	fontData[fontNumber].fSpacingPerChar = fSpacingPerChar;
	fontData[fontNumber].iv			   =0;
	fontData[fontNumber].dwNumTriangles=0;

	LPDIRECT3DVERTEXBUFFER9 g_pVB        = NULL;
	int hr=m_pDevice->CreateVertexBuffer(	MaxNumfontVertices*sizeof(CUSTOMVERTEX),
											0, D3DFVF_CUSTOMVERTEX,
											D3DPOOL_MANAGED, 
											&g_pVB, 
											NULL) ;
	fontData[fontNumber].pVertexBuffer=g_pVB;
	int x=123;
}

void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (index < 0     || index > MAX_TEXTURE_COORDS) return;
	if (subindex < 0  || subindex > 3) return;
	fontData[fontNumber].textureCoord[index][subindex]=fValue;
}


void FontEngineDrawText3D(int fontNumber, char* text, int xposStart, int yposStart, DWORD intColor)
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
			pVertices[font->iv].rhw=1.0f;  pVertices[font->iv].z=0.0f;  pVertices[font->iv].x=xpos ;  pVertices[font->iv].y=ypos2 ; pVertices[font->iv].color=intColor;pVertices[font->iv].tu=tx1; pVertices[font->iv].tv=ty2;font->iv++;
			pVertices[font->iv].rhw=1.0f;  pVertices[font->iv].z=0.0f;  pVertices[font->iv].x=xpos ;  pVertices[font->iv].y=ypos  ; pVertices[font->iv].color=intColor;pVertices[font->iv].tu=tx1; pVertices[font->iv].tv=ty1;font->iv++;
			pVertices[font->iv].rhw=1.0f;  pVertices[font->iv].z=0.0f;  pVertices[font->iv].x=xpos2;  pVertices[font->iv].y=ypos2 ; pVertices[font->iv].color=intColor;pVertices[font->iv].tu=tx2; pVertices[font->iv].tv=ty2;font->iv++;
			pVertices[font->iv].rhw=1.0f;  pVertices[font->iv].z=0.0f;  pVertices[font->iv].x=xpos2;  pVertices[font->iv].y=ypos  ; pVertices[font->iv].color=intColor;pVertices[font->iv].tu=tx2; pVertices[font->iv].tv=ty1;font->iv++;
			pVertices[font->iv].rhw=1.0f;  pVertices[font->iv].z=0.0f;  pVertices[font->iv].x=xpos2;  pVertices[font->iv].y=ypos2 ; pVertices[font->iv].color=intColor;pVertices[font->iv].tu=tx2; pVertices[font->iv].tv=ty2;font->iv++;
			pVertices[font->iv].rhw=1.0f;  pVertices[font->iv].z=0.0f;  pVertices[font->iv].x=xpos ;  pVertices[font->iv].y=ypos  ; pVertices[font->iv].color=intColor;pVertices[font->iv].tu=tx1; pVertices[font->iv].tv=ty1;font->iv++;

			font->dwNumTriangles += 2;
			if (font->iv > (MaxNumfontVertices-12))
			{
				font->pVertexBuffer->Unlock();
				FontEnginePresentTextures();
				m_pDevice->SetTexture(0, font->pTexture);
				m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
				m_pDevice->SetStreamSource(0, font->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
				m_pDevice->DrawPrimitive(D3DPT_TRIANGLELIST, 0, font->dwNumTriangles);
				m_pDevice->SetTexture(0, NULL);
				font->dwNumTriangles = 0;
				font->iv = 0;
				font->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, D3DLOCK_DISCARD ) ;
			}
		}

		xpos += w - fSpacing;
	}
	font->pVertexBuffer->Unlock();
}

void FontEnginePresent3D(int fontNumber)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (fontData[fontNumber].dwNumTriangles==0) return;

	FONT_DATA_T* font = &(fontData[fontNumber]);
	m_pDevice->SetTexture(0, font->pTexture);
	m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
	m_pDevice->SetStreamSource(0, font->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
	m_pDevice->DrawPrimitive(D3DPT_TRIANGLELIST, 0, font->dwNumTriangles);
	m_pDevice->SetTexture(0, NULL);
	font->dwNumTriangles = 0;
	font->iv = 0;
}

void FontEngineRemoveFont(int fontNumber)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (fontData[fontNumber].pVertexBuffer!=NULL) 
	{
		fontData[fontNumber].pVertexBuffer->Release();
		fontData[fontNumber].pVertexBuffer=NULL;
		fontData[fontNumber].pTexture=NULL;
	}
}