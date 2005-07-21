using System;
using System.IO;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for TexturePacker.
	/// </summary>
	public class TexturePacker
	{

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineRemoveTexture(int textureNo);

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern int  FontEngineAddTexture(int hasCode,bool useAlphaBlend,void* fontTexture);
		
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern int  FontEngineAddSurface(int hasCode,bool useAlphaBlend,void* fontTexture);
		
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color);

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEnginePresentTextures();

		[Serializable]
		public class PackedTexture
		{
			public PackedTextureNode root;
			public int               textureNo;
			[NonSerialized]
			public Texture           texture;
		};

		[Serializable]
		public class PackedTextureNode
		{
			public PackedTextureNode	 ChildLeft;
			public PackedTextureNode	 ChildRight;
			public Rectangle					 Rect;
			public string              FileName;
			
			public PackedTextureNode Get(string fileName)
			{
				if (fileName==null) return null;
				if (fileName.Length==0) return null;
				if (FileName!=null)
				{
					if (FileName==fileName) 
					{
						return this;
					}
				}
				if (ChildLeft!=null)
				{
					PackedTextureNode node=ChildLeft.Get(fileName);
					if (node!=null) return node;
				}
				if (ChildRight!=null)
				{
					return ChildRight.Get(fileName);
				}
				return null;
			}

			public PackedTextureNode Insert(string fileName,Image img, Image rootImage)
			{
					//Log.Write("rect:({0},{1}) {2}x{3} img:{4}x{5} filename:{6} left:{7} right:{8}",
					//				Rect.Left,Rect.Top,Rect.Width,Rect.Height,img.Width,img.Height,FileName, ChildLeft,ChildRight);
					if (ChildLeft!=null && ChildRight!=null)
					{
						PackedTextureNode node=ChildLeft.Insert(fileName, img, rootImage);
						if (node!=null) return node;
						return ChildRight.Insert(fileName, img, rootImage);
					}
					//(if there's already a lightmap here, return)
					if (FileName!=null && FileName.Length>0) return null;

					//(if we're too small, return)
					if ((img.Width+2) > Rect.Width || (img.Height+2) > Rect.Height)
					{
						return null;
					}
					//(if we're just right, accept)
					if ((img.Width+2) == Rect.Width && (img.Height+2) == Rect.Height)
					{
						using (Graphics g = Graphics.FromImage(rootImage))
						{
							FileName=fileName;
							g.CompositingQuality=System.Drawing.Drawing2D.CompositingQuality.HighQuality;
							g.CompositingMode=System.Drawing.Drawing2D.CompositingMode.SourceCopy;
							g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
							g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.HighQuality;
              // draw oversized image first
              g.DrawImage(img,Rect.Left,Rect.Top,Rect.Width,Rect.Height);              
              // draw original image ontop of oversized image
							g.DrawImage(img,Rect.Left+1,Rect.Top+1,Rect.Width-2,Rect.Height-2);
						}
						return this;
					}

					if (Rect.Width<=2 || Rect.Height <=2) return null;
					//(otherwise, gotta split this node and create some kids)
					ChildLeft = new PackedTextureNode();
					ChildRight= new PackedTextureNode();
	        
					//(decide which way to split)
					int dw = Rect.Width  - (img.Width+2);
					int dh = Rect.Height - (img.Height+2);

					if (dw > dh)
					{
						ChildLeft.Rect  = new Rectangle(Rect.Left, Rect.Top, (img.Width+2), Rect.Height);
						ChildRight.Rect = new Rectangle(Rect.Left+(img.Width+2), Rect.Top, Rect.Width-(img.Width+2), Rect.Height);
					}
					else
					{
						ChildLeft.Rect  = new Rectangle(Rect.Left, Rect.Top, Rect.Width, (img.Height+2));
						ChildRight.Rect = new Rectangle(Rect.Left, Rect.Top+(img.Height+2), Rect.Width, Rect.Height-(img.Height+2));
					}
					//(insert into first child we created)
					PackedTextureNode newNode=ChildLeft.Insert(fileName, img, rootImage);
					if (newNode!=null) return newNode;
					return ChildRight.Insert(fileName, img, rootImage);
			}
		}

		ArrayList packedTextures;
		public TexturePacker()
		{
		}

		bool Add(PackedTextureNode root, Image img, Image rootImage, string fileName)
		{
			PackedTextureNode node=root.Insert(fileName,img,rootImage);
			if (node!=null)
			{
				//Log.Write("added {0} at ({1},{2}) {3}x{4}",fileName,node.Rect.X,node.Rect.Y,node.Rect.Width,node.Rect.Height);
				node.FileName = fileName;
				return true;
			}
			//Log.Write("no room anymore to add:{0}", fileName);
			return false;
		}

		void SavePackedSkin(string skinName)
		{
			string packedXml=String.Format(@"{0}\packedgfx.xml",skinName);
			using(FileStream fileStream = new FileStream(packedXml, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				SoapFormatter formatter = new SoapFormatter();
				formatter.Serialize(fileStream, packedTextures);
				fileStream.Close();
			}
		}

		bool LoadPackedSkin(string skinName)
		{
			string packedXml=String.Format(@"{0}\packedgfx.xml",skinName);
			if(File.Exists(packedXml))
			{
				using(FileStream fileStream = new FileStream(packedXml, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					try
					{
						packedTextures=new ArrayList();
						SoapFormatter formatter = new SoapFormatter();
						packedTextures = (ArrayList)formatter.Deserialize(fileStream);
						fileStream.Close();
						LoadPackedGraphics();
						return true;
					}
					catch
					{
					}
				}
			}
			return false;

		}

		public void PackSkinGraphics(string skinName)
		{
			if (LoadPackedSkin(skinName)) return;

			packedTextures=new ArrayList();
			string[] files1 =System.IO.Directory.GetFiles( String.Format(@"{0}\media",skinName),"*.png" );
			string[] files2 =System.IO.Directory.GetFiles( @"thumbs\tv\logos","*.png" );
			//string[] files3 =System.IO.Directory.GetFiles( @"weather\64x64","*.png" );
			//string[] files4 =System.IO.Directory.GetFiles( @"weather\128x128","*.png" );
			//string[] files5 =System.IO.Directory.GetFiles( String.Format(@"{0}\media\tetris",skinName),"*.png" );
			string [] files = new string[files1.Length+files2.Length/*+files3.Length+files4.Length+files5.Length*/];
			
			int off=0;
			for (int i=0; i < files1.Length;++i)
				files[off++] = files1[i];
			for (int i=0; i < files2.Length;++i)
				files[off++] = files2[i];
/*
			for (int i=0; i < files3.Length;++i)
				files[off++] = files3[i];
			for (int i=0; i < files4.Length;++i)
				files[off++] = files4[i];
			for (int i=0; i < files5.Length;++i)
				files[off++] = files5[i];
*/
			//Determine maximum texture dimensions
			//We limit the max resolution to 2048x2048
			Caps d3dcaps = GUIGraphicsContext.DX9Device.DeviceCaps;
			int iMaxWidth =d3dcaps.MaxTextureWidth;
			int iMaxHeight=d3dcaps.MaxTextureHeight;
			if (iMaxWidth >2048) iMaxWidth =2048;
			if (iMaxHeight>2048) iMaxHeight=2048;
			while (true)
			{
				bool ImagesLeft=false;
				
				PackedTexture bigOne = new PackedTexture();
				bigOne.root =new PackedTextureNode();
				bigOne.texture=null;
				bigOne.textureNo=-1;
				Bitmap rootImage = new Bitmap(iMaxWidth, iMaxHeight);
				bigOne.root.Rect = new Rectangle(0,0,iMaxWidth, iMaxHeight);
				for (int i=0; i < files.Length;++i)
				{
					files[i]=files[i].ToLower();
					if (files[i]!=String.Empty ) 
					{
						if (files[i].IndexOf("preview.")>=0) 
						{
							files[i]=String.Empty;
							continue;
						}
						bool dontAdd;
						if (AddBitmap(bigOne.root,rootImage,files[i], out dontAdd))
						{
							files[i]=String.Empty;
						}
						else
						{
							if (dontAdd) files[i]=String.Empty;
							else ImagesLeft=true;
						}
					}
				}
				string fileName=String.Format(@"{0}\packedgfx{1}.png",GUIGraphicsContext.Skin,packedTextures.Count);
				rootImage.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
				rootImage.Dispose();
				packedTextures.Add(bigOne);
				if (!ImagesLeft) break;
			}
			SavePackedSkin(skinName);
			LoadPackedGraphics();
		}

		void LoadPackedGraphics()
		{
		//	return ;
			int index=0;
			foreach (PackedTexture bigOne in packedTextures)
			{
				Format useFormat=Format.A8R8G8B8;
				//if (IsCompressedTextureFormatOk(Format.Dxt5))
				//	useFormat=Format.Dxt5;
				if (bigOne.texture==null)
				{
					bigOne.textureNo=-1;

					string fileName=String.Format(@"{0}\packedgfx{1}.png",GUIGraphicsContext.Skin,index);
					ImageInformation info2 = new ImageInformation();
					Texture tex = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
						fileName,
						0,0,//width/height
						1,//mipslevels
						0,//Usage.Dynamic,
						useFormat,
						Pool.Managed,
						Filter.None,
						Filter.None,
						(int)0,
						ref info2);
					bigOne.texture=tex;
					Log.Write("TexturePacker: Loaded {0} texture:{1}x{2} miplevels:{3}",fileName, info2.Width,info2.Height, tex.LevelCount);
				}
				index++;
			}
		}

		bool AddBitmap(PackedTextureNode root, Image rootImage, string file, out bool dontAdd)
		{
			bool result=false;
			dontAdd=false;
			using (Image bmp = Image.FromFile(file))
			{
				if (bmp.Width >= GUIGraphicsContext.Width ||
					bmp.Height >= GUIGraphicsContext.Height)
				{
					dontAdd=true;
					return false;
				}
				int pos;
				string skinName=String.Format(@"{0}\media", GUIGraphicsContext.Skin).ToLower();
				pos=file.IndexOf(skinName);
				if (pos>=0)
				{
					file=file.Remove(pos,skinName.Length);
				}
				if (file.StartsWith(@"\"))
				{
					file=file.Remove(0,1);
				}
				result=Add(root,bmp,rootImage,file);
			}
			return result;
		}
		
		public bool Get(string fileName, out float uoffs, out float voffs, out float umax, out float vmax, out int iWidth, out int iHeight, out Texture tex, out int TextureNo)
		{
			uoffs=voffs=umax=vmax=0.0f;
			iWidth=iHeight=0;
			TextureNo=-1;
			tex=null;
			if (packedTextures==null) return false;
			
			if (fileName.StartsWith(@"\"))
			{
				fileName=fileName.Remove(0,1);
			}
			fileName=fileName.ToLower();
			if (fileName==String.Empty) return false;
			int index=0;
			foreach (PackedTexture bigOne in packedTextures)
			{
				PackedTextureNode foundNode=bigOne.root.Get(fileName);
				if (foundNode!=null)
				{
					uoffs  = ((float)foundNode.Rect.Left+1)   / ((float)bigOne.root.Rect.Width);
					voffs  = ((float)foundNode.Rect.Top+1)    / ((float)bigOne.root.Rect.Height);
					umax   = ((float)foundNode.Rect.Width-2)  / ((float)bigOne.root.Rect.Width);
					vmax   = ((float)foundNode.Rect.Height-2) / ((float)bigOne.root.Rect.Height);         
					iWidth = foundNode.Rect.Width-2;
					iHeight= foundNode.Rect.Height-2;
					if (bigOne.texture==null)
					{
						LoadPackedGraphics();
					}

					tex=bigOne.texture;
					if (bigOne.textureNo==-1)
					{	
						unsafe
						{
							IntPtr ptr=DShowNET.DsUtils.GetUnmanagedTexture(bigOne.texture);
							bigOne.textureNo=FontEngineAddTexture(ptr.ToInt32(),true,(void*) ptr.ToPointer());
							Log.Write("TexturePacker: fontengine add texure:{0}",bigOne.textureNo);
						}
					}
					TextureNo=bigOne.textureNo;
					return true;
				}
				index++;
			}
			return false;
		}

		public void Dispose()
		{
			Log.Write("TexturePacker:Dispose()");
			if (packedTextures!=null)
			{
				foreach (PackedTexture bigOne in packedTextures)
				{
					if (bigOne.textureNo>=0)
					{
						Log.Write("TexturePacker: fontengine remove texture:{0}",bigOne.textureNo);
						FontEngineRemoveTexture(bigOne.textureNo);
					}
					if (bigOne.texture!=null)
					{
						if (!bigOne.texture.Disposed)
						{
							bigOne.texture.Dispose();
						}
						bigOne.texture=null;
					}
				}
			}
		}
		bool IsCompressedTextureFormatOk( Direct3D.Format textureFormat) 
		{
			if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, GUIGraphicsContext.DirectXPresentParameters.BackBufferFormat,
				Usage.None, ResourceType.Textures,
				textureFormat))
			{
				Log.Write("TexurePacker:Using compressed textures");
				return true;
			}
			return false;
		}
	}
}
