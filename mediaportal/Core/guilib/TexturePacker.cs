using System;
using System.Drawing;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for TexturePacker.
	/// </summary>
	public class TexturePacker
	{
		class PackedTextureNode
		{
			public PackedTextureNode	 ChildLeft;
			public PackedTextureNode	 ChildRight;
			public Rectangle					 Rect;
			public string              FileName;
			
			public PackedTextureNode Insert(string fileName,Image img, Image rootImage)
			{
					Log.Write("rect:({0},{1}) {2}x{3} img:{4}x{5} filename:{6} left:{7} right:{8}",
									Rect.Left,Rect.Top,Rect.Width,Rect.Height,img.Width,img.Height,FileName, ChildLeft,ChildRight);
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

		PackedTextureNode root ;
		Bitmap rootImage;
		public TexturePacker()
		{
			root = new PackedTextureNode();
			rootImage = new Bitmap(1024,1024);
			root.Rect=new Rectangle(0,0,1024,1024);
		}
		public bool Add(Image img, string fileName)
		{
			PackedTextureNode node=root.Insert(fileName,img,rootImage);
			if (node!=null)
			{
				node.FileName = fileName;
				return true;
			}
			return false;
		}
		public void test()
		{
			AddBitmap(512,512,Brushes.Red);
			AddBitmap(256,256,Brushes.Yellow);
			AddBitmap(300,32,Brushes.Blue);
			rootImage.Save("test.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
		}
		public void AddBitmap(int w, int h,Brush brush)
		{
			Log.Write("---------------------");
			Bitmap bmp = new Bitmap(w,h);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.FillRectangle(brush,0,0,w,h);
			}
			Add(bmp,"bla");
			bmp.Dispose();
		}
	}
}
