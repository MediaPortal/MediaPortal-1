using System;
using System.Drawing;
using System.Collections;

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

		ArrayList rootImages;
		ArrayList rootNodes;
		public TexturePacker()
		{
		}
		public bool Add(PackedTextureNode root, Image img, Image rootImage, string fileName)
		{
			PackedTextureNode node=root.Insert(fileName,img,rootImage);
			if (node!=null)
			{
				Log.Write("added {0} at ({1},{2}) {3}x{4}",fileName,node.Rect.X,node.Rect.Y,node.Rect.Width,node.Rect.Height);
				node.FileName = fileName;
				return true;
			}
			Log.Write("no room anymore to add:{0}", fileName);
			return false;
		}

		public void test()
		{
			rootImages=new ArrayList();
			rootNodes=new ArrayList();
			
			string[] files =System.IO.Directory.GetFiles(@"skin\bluetwo\media","*.png");
			
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
				rootImages.Add(rootImage);
				rootNodes.Add(root);
				rootImage.Save(String.Format("bluetwo{0}.png",rootImages.Count), System.Drawing.Imaging.ImageFormat.Png);
				if (!ImagesLeft) break;
			}
		}

		public bool AddBitmap(PackedTextureNode root, Image rootImage, string file)
		{
			//Log.Write("---------------------");
			Image bmp = Image.FromFile(file);
			bool result=Add(root,bmp,rootImage,file);
			bmp.Dispose();
			return result;
		}
	}
}
