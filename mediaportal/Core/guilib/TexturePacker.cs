using System;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for TexturePacker.
	/// </summary>
	public class TexturePacker
	{
		public class PackedTextureNode
		{
			public PackedTextureNode	 ChildLeft;
			public PackedTextureNode	 ChildRight;
			public Rectangle					 Rect;
			public string              FileName;
			
			public PackedTextureNode Get(string fileName)
			{
				if (FileName!=null && FileName.Length>0) 
				{
					if (FileName==FileName) return this;
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
					if (img.Width > Rect.Width || img.Height > Rect.Height)
					{
						return null;
					}
					//(if we're just right, accept)
					if (img.Width == Rect.Width && img.Height == Rect.Height)
					{
						using (Graphics g = Graphics.FromImage(rootImage))
						{
							FileName=fileName;
							g.DrawImage(img,Rect.Left,Rect.Top,Rect.Width,Rect.Height);
						}
						return this;
					}

					if (Rect.Width<=2 || Rect.Height <=2) return null;
					//(otherwise, gotta split this node and create some kids)
					ChildLeft = new PackedTextureNode();
					ChildRight= new PackedTextureNode();
	        
					//(decide which way to split)
					int dw = Rect.Width  - img.Width;
					int dh = Rect.Height - img.Height;
	        
					if (dw > dh)
					{
						ChildLeft.Rect  = new Rectangle(Rect.Left, Rect.Top, img.Width, Rect.Height);
						ChildRight.Rect = new Rectangle(Rect.Left+img.Width, Rect.Top, Rect.Width-img.Width, Rect.Height);
					}
					else
					{
						ChildLeft.Rect  = new Rectangle(Rect.Left, Rect.Top, Rect.Width, img.Height);
						ChildRight.Rect = new Rectangle(Rect.Left, Rect.Top+img.Height, Rect.Width, Rect.Height-img.Height);
					}
					//(insert into first child we created)
					PackedTextureNode newNode=ChildLeft.Insert(fileName, img, rootImage);
					if (newNode!=null) return newNode;
					return ChildRight.Insert(fileName, img, rootImage);
			}
		}

		ArrayList rootNodes;
		ArrayList textures;
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

		
		public void PackSkinGraphics(string skinName)
		{
			rootNodes=new ArrayList();
			textures=new ArrayList();
			string[] files =System.IO.Directory.GetFiles( String.Format(@"skin\{0}\media",skinName),"*.png" );
			
			while (true)
			{
				bool ImagesLeft=false;
				
				PackedTextureNode root =new PackedTextureNode();
				Bitmap rootImage = new Bitmap(2048,2048);
				root.Rect=new Rectangle(0,0,2048,2048);
				for (int i=0; i < files.Length;++i)
				{
					if (files[i]!="") 
					{
						if (AddBitmap(root,rootImage,files[i]))
						{
							files[i]="";
						}
						else
							ImagesLeft=true;
					}
				}
				string fileName=String.Format(@"skin\{0}\bluetwo{0}.png",skinName,rootNodes.Count);
				rootNodes.Add(root);
				rootImage.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
				rootImage.Dispose();
				ImageInformation info2 = new ImageInformation();
				Texture tex = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
																						fileName,
																						0,0,//width/height
																						1,//mipslevels
																						0,//Usage.Dynamic,
																						Format.A8R8G8B8,
																						Pool.Managed,
																						Filter.None,
																						Filter.None,
																						(int)0,
																						ref info2);
				textures.Add(tex);
				if (!ImagesLeft) break;
			}
		}

		bool AddBitmap(PackedTextureNode root, Image rootImage, string file)
		{
			bool result=false;
			using (Image bmp = Image.FromFile(file))
			{
				result=Add(root,bmp,rootImage,file);
			}
			return result;
		}
		
		public bool Get(string fileName, out float uoffs, out float voffs, out float umax, out float vmax)
		{
			uoffs=voffs=umax=vmax=0.0f;
			if (rootNodes==null) return false;
			foreach (PackedTextureNode root in rootNodes)
			{
				PackedTextureNode foundNode=root.Get(fileName);
				if (foundNode!=null)
				{
					uoffs = ((float)foundNode.Rect.Left)   / 2048f;
					voffs = ((float)foundNode.Rect.Top)    / 2048f;
					umax  = ((float)foundNode.Rect.Right)  / 2048f;
					vmax  = ((float)foundNode.Rect.Bottom) / 2048f;
					return true;
				}
			}
			return false;
		}

		public void Dispose()
		{
			rootNodes=null;
			if (textures!=null)
			{
				foreach (Texture tex in textures)
				{
					tex.Dispose();
				}
			}
			textures=null;
		}
	}
}
