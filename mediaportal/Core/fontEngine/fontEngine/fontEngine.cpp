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
#define MaxNumTextureVertices	600

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
	CUSTOMVERTEX*			vertices;
	int                     iv;
	int                     dwNumTriangles;
} ;

struct TEXTURE_DATA_T
{	
	int						hashCode;
	LPDIRECT3DTEXTURE9		pTexture;
	LPDIRECT3DVERTEXBUFFER9	pVertexBuffer;
	CUSTOMVERTEX*			vertices;
	int                     iv;
	int                     dwNumTriangles;
	D3DSURFACE_DESC			desc;
	bool                    updateVertexBuffer;
};

static FONT_DATA_T*			fontData    = new FONT_DATA_T[MAX_FONTS];
static TEXTURE_DATA_T*		textureData = new TEXTURE_DATA_T[MAX_TEXTURES];
static LPDIRECT3DDEVICE9	m_pDevice=NULL;	
static int                  textureZ[MAX_TEXTURES];
int                         textureCount;

//*******************************************************************************************************************
void FontEngineInitialize()
{
	textureCount=0;
	static bool initialized=false;
	if (!initialized)
	{
		for (int i=0; i < MAX_FONTS;++i)
		{
			fontData[i].pVertexBuffer=NULL;
			fontData[i].pTexture = NULL;
			fontData[i].vertices = NULL;
		}
		for (int i=0; i < MAX_TEXTURES;++i)
		{
			textureData[i].hashCode=-1;
			textureData[i].dwNumTriangles=0;
			textureData[i].iv=0;
			textureData[i].pVertexBuffer=NULL;
			textureData[i].pTexture=NULL;
			textureData[i].vertices = NULL;
			textureData[i].updateVertexBuffer=false;
			textureZ[i]=-1;
		}
		initialized=true;
		textureCount=0;
	}
}
//*******************************************************************************************************************
void FontEngineRemoveTexture(int textureNo)
{
	textureData[textureNo].hashCode=-1;
	textureData[textureNo].dwNumTriangles=0;
	textureData[textureNo].iv=0;
	if (textureData[textureNo].pVertexBuffer!=NULL)
		textureData[textureNo].pVertexBuffer->Release();
	textureData[textureNo].pVertexBuffer=NULL;
	
	if (textureData[textureNo].vertices!=NULL)
		delete[] textureData[textureNo].vertices;
	textureData[textureNo].vertices=NULL;
	textureData[textureNo].pTexture=NULL;
}

//*******************************************************************************************************************
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
		m_pDevice->CreateVertexBuffer(		MaxNumTextureVertices*sizeof(CUSTOMVERTEX),
											0, 
											D3DFVF_CUSTOMVERTEX,
											D3DPOOL_MANAGED, 
											&textureData[selected].pVertexBuffer, 
											NULL) ;  
	}
	if (textureData[selected].vertices==NULL)
	{
		textureData[selected].vertices = new CUSTOMVERTEX[MaxNumTextureVertices];
		for (int iv=0; iv < MaxNumTextureVertices;++iv)
		{
			textureData[selected].vertices[iv].rhw=1.0f;  
			textureData[selected].vertices[iv].z=0.0f;
		}
	}
	textureData[selected].pTexture->GetLevelDesc(0,&textureData[selected].desc);
	return selected;
}

//*******************************************************************************************************************
void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color)
{
	TEXTURE_DATA_T* texture=&textureData[textureNo];
	if (texture->iv==0)
	{
		textureZ[textureCount]=textureNo;
		textureCount++;
	}
	int iv=texture->iv;
	if (iv+6 >=MaxNumTextureVertices)
	{
		OutputDebugString("Ran out of texture vertices\n");
		return;
	}
	

	float xpos=x-0.5f;
	float xpos2=x+nw-0.5f;
	float ypos=y-0.5f;
	float ypos2=y+nh-0.5f;
	
	D3DVIEWPORT9 viewport;
	m_pDevice->GetViewport(&viewport);
	if (viewport.X>0 || viewport.Y>0)
	{
		float w=xpos2-xpos;
		float h=ypos2-xpos;
		if (xpos <	viewport.X)
		{
			float off=viewport.X - xpos;
			uoff += (off / ((float)texture->desc.Width));
			xpos=viewport.X;
		}
		if (xpos2 >	viewport.X+viewport.Width)
		{
			float off= (viewport.X+viewport.Width) - xpos2;
			umax += (off / ((float)texture->desc.Width));
			xpos2=viewport.X+viewport.Width;
		}

		if (ypos <	viewport.Y)
		{
			float off=viewport.Y - ypos;
			voff += (off / ((float)texture->desc.Height));
			ypos=viewport.Y;
		}
		if (ypos2 >	viewport.Y+viewport.Height)
		{
			float off= (viewport.Y+viewport.Height) - ypos2;
			vmax += (off / ((float)texture->desc.Height));
			ypos2=viewport.Y+viewport.Height;
		}
	}
	float tx1=uoff;
	float tx2=uoff+umax;
	float ty1=voff;
	float ty2=voff+vmax;

	if (texture->vertices[iv].tu != tx1 || texture->vertices[iv].tv !=ty2 || texture->vertices[iv].color!=color)
		texture->updateVertexBuffer=true;
	texture->vertices[iv].x=xpos ;  texture->vertices[iv].y=ypos2 ; texture->vertices[iv].color=color;texture->vertices[iv].tu=tx1; texture->vertices[iv].tv=ty2;iv++;

	if (texture->vertices[iv].x != xpos || texture->vertices[iv].y !=ypos || texture->vertices[iv].tv!=ty1)
		texture->updateVertexBuffer=true;
	texture->vertices[iv].x=xpos ;  texture->vertices[iv].y=ypos  ; texture->vertices[iv].color=color;texture->vertices[iv].tu=tx1; texture->vertices[iv].tv=ty1;iv++;

	if (texture->vertices[iv].x != xpos2 || texture->vertices[iv].y!=ypos2 || texture->vertices[iv].tu!=tx2)
		texture->updateVertexBuffer=true;
	texture->vertices[iv].x=xpos2;  texture->vertices[iv].y=ypos2 ; texture->vertices[iv].color=color;texture->vertices[iv].tu=tx2; texture->vertices[iv].tv=ty2;iv++;
	texture->vertices[iv].x=xpos2;  texture->vertices[iv].y=ypos  ; texture->vertices[iv].color=color;texture->vertices[iv].tu=tx2; texture->vertices[iv].tv=ty1;iv++;
	texture->vertices[iv].x=xpos2;  texture->vertices[iv].y=ypos2 ; texture->vertices[iv].color=color;texture->vertices[iv].tu=tx2; texture->vertices[iv].tv=ty2;iv++;
	texture->vertices[iv].x=xpos ;  texture->vertices[iv].y=ypos  ; texture->vertices[iv].color=color;texture->vertices[iv].tu=tx1; texture->vertices[iv].tv=ty1;iv++;

	texture->iv=texture->iv+6;
	texture->dwNumTriangles=texture->dwNumTriangles+2;
	
}

//*******************************************************************************************************************
void FontEnginePresentTextures()
{
	
	for (int i=0; i < textureCount; ++i)
	{
		int index=textureZ[i];
		TEXTURE_DATA_T* texture = &(textureData[index]);
		if (texture->dwNumTriangles!=0)
		{
			if (texture->updateVertexBuffer)
			{
				CUSTOMVERTEX* pVertices;
				
				texture->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, D3DLOCK_DISCARD ) ;
				memcpy(pVertices,texture->vertices, (texture->iv)*sizeof(CUSTOMVERTEX));
				texture->pVertexBuffer->Unlock();
			}

			m_pDevice->SetTexture(0, texture->pTexture);
			m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
			m_pDevice->SetStreamSource(0, texture->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
			m_pDevice->DrawPrimitive(D3DPT_TRIANGLELIST, 0, texture->dwNumTriangles);
			m_pDevice->SetTexture(0, NULL);
		}
		texture->dwNumTriangles = 0;
		texture->iv = 0;
		texture->updateVertexBuffer=false;
		textureZ[i]=0;
		
	}
	textureCount=0;
}


//*******************************************************************************************************************
void FontEngineAddFont(void* device, int fontNumber,void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar,int maxVertices)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (fontTexture==NULL) return;
	if (firstChar<0 || firstChar>endChar) return;
	m_pDevice=(LPDIRECT3DDEVICE9)device;


	fontData[fontNumber].vertices      = new CUSTOMVERTEX[MaxNumfontVertices];
	for (int iv=0; iv < MaxNumfontVertices;++iv)
	{
		fontData[fontNumber].vertices[iv].rhw=1.0f;  
		fontData[fontNumber].vertices[iv].z=0.0f; 
	}

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

//*******************************************************************************************************************
void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (index < 0     || index > MAX_TEXTURE_COORDS) return;
	if (subindex < 0  || subindex > 3) return;
	fontData[fontNumber].textureCoord[index][subindex]=fValue;
}


//*******************************************************************************************************************
void FontEngineDrawText3D(int fontNumber, char* text, int xposStart, int yposStart, DWORD intColor, int maxWidth)
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

	D3DVIEWPORT9 viewport;
	m_pDevice->GetViewport(&viewport);
	int off=(fontData[fontNumber].fSpacingPerChar+1);
	if (viewport.X>=off)
	{
		viewport.X -= (fontData[fontNumber].fSpacingPerChar+1);
		viewport.Width+=((fontData[fontNumber].fSpacingPerChar+1)*2);
	}
	if (viewport.Y>0)
	{
		viewport.Y--;
		viewport.Height+=2;
	}

	float totalWidth=0;
	if (maxWidth <=0) maxWidth=2000;
	
	for (int i=0; i < (int)strlen(text);++i)
	{
        char c=text[i];
		if (c == '\n')
		{
			totalWidth=0;
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

		if (xpos<0 || ypos<0)
		{
			c=' ';
		}

		totalWidth += (w - fSpacing);
		if (totalWidth>= maxWidth) break;

		if (c != ' ' && xpos >= viewport.X && ypos >= viewport.Y)
		{
			float xpos2=xpos+w;
			float ypos2=ypos+h;
			if (xpos2 <= viewport.X+viewport.Width && ypos2 <=viewport.Y+viewport.Height)
			{
				font->vertices[font->iv].x=xpos ;  font->vertices[font->iv].y=ypos2 ; font->vertices[font->iv].color=intColor;font->vertices[font->iv].tu=tx1; font->vertices[font->iv].tv=ty2;font->iv++;
				font->vertices[font->iv].x=xpos ;  font->vertices[font->iv].y=ypos  ; font->vertices[font->iv].color=intColor;font->vertices[font->iv].tu=tx1; font->vertices[font->iv].tv=ty1;font->iv++;
				font->vertices[font->iv].x=xpos2;  font->vertices[font->iv].y=ypos2 ; font->vertices[font->iv].color=intColor;font->vertices[font->iv].tu=tx2; font->vertices[font->iv].tv=ty2;font->iv++;
				font->vertices[font->iv].x=xpos2;  font->vertices[font->iv].y=ypos  ; font->vertices[font->iv].color=intColor;font->vertices[font->iv].tu=tx2; font->vertices[font->iv].tv=ty1;font->iv++;
				font->vertices[font->iv].x=xpos2;  font->vertices[font->iv].y=ypos2 ; font->vertices[font->iv].color=intColor;font->vertices[font->iv].tu=tx2; font->vertices[font->iv].tv=ty2;font->iv++;
				font->vertices[font->iv].x=xpos ;  font->vertices[font->iv].y=ypos  ; font->vertices[font->iv].color=intColor;font->vertices[font->iv].tu=tx1; font->vertices[font->iv].tv=ty1;font->iv++;

				font->dwNumTriangles += 2;
				if (font->iv > (MaxNumfontVertices-12))
				{
					FontEnginePresentTextures();
					FontEnginePresent3D(fontNumber);
					font->dwNumTriangles = 0;
					font->iv = 0;
				}
			}
		}

		xpos += w - fSpacing;
	}
}

//*******************************************************************************************************************
void FontEnginePresent3D(int fontNumber)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (fontData[fontNumber].dwNumTriangles==0) return;

	FONT_DATA_T* font = &(fontData[fontNumber]);
	
	CUSTOMVERTEX* pVertices;
	font->pVertexBuffer->Lock( 0, 0, (void**)&pVertices, D3DLOCK_DISCARD ) ;
	memcpy(pVertices,font->vertices, (font->iv)*sizeof(CUSTOMVERTEX));
	font->pVertexBuffer->Unlock();

	m_pDevice->SetTexture(0, font->pTexture);
	m_pDevice->SetFVF( D3DFVF_CUSTOMVERTEX );
	m_pDevice->SetStreamSource(0, font->pVertexBuffer, 0, sizeof(CUSTOMVERTEX) );
	m_pDevice->DrawPrimitive(D3DPT_TRIANGLELIST, 0, font->dwNumTriangles);
	m_pDevice->SetTexture(0, NULL);
	font->dwNumTriangles = 0;
	font->iv = 0;
}


//*******************************************************************************************************************
void FontEngineRemoveFont(int fontNumber)
{
	if (fontNumber< 0 || fontNumber>=MAX_FONTS) return;
	if (fontData[fontNumber].pVertexBuffer!=NULL) 
	{
		fontData[fontNumber].pVertexBuffer->Release();
	}
	fontData[fontNumber].pVertexBuffer=NULL;

	if (fontData[fontNumber].vertices!=NULL)
		delete[] fontData[fontNumber].vertices;
	fontData[fontNumber].vertices=NULL;

	fontData[fontNumber].pTexture=NULL;
}