using System;
using System.Drawing;
using System.Collections;
using System.IO;

namespace MediaPortal.GUI.Library
{
	public class GUIWaitCursor : GUIControl
	{
		#region Constructors

		private GUIWaitCursor()
		{
		}

		#endregion Constructors

		#region Methods

		GUIImage[] _images;

		public static void Init()
		{
			GUIWaitCursor cursor = GUIWaitCursor.Instance;

			cursor.Initialize();
		}

		public void Initialize()
		{
			ArrayList array = new ArrayList();

			foreach(string filename in System.IO.Directory.GetFiles(GUIGraphicsContext.Skin + @"\media\", "common.waiting.*.png"))
				array.Add(filename);

			int x = 0;
			int y = 0;
			int w = 0;
			int h = 0;

			_images = new GUIImage[array.Count];

			for(int index = 0; index < _images.Length; index++)
			{
				_images[index] = new GUIImage(this.ParentID, 200001 + index, x, y, w, h, (string)array[index], Color.White);
				_images[index].AllocResources();

				if(index != 0)
					continue;

				w = _images[index].Width;
				h = _images[index].Height;

				x = (GUIGraphicsContext.Width - w) / 2;
				y = (GUIGraphicsContext.Height - h) / 2;

				_images[index].SetPosition(x, y);
			}
		}

		float _tickStart = 0;

		public override void Render(float timePassed)
		{
			if(_showCount == 0)
				return;

			if(_images == null)
				return;

			long tick = Environment.TickCount;

			double t = tick - _tickStart;
			double b = 0;
			double c = _images.Length;
			double d = 800;
			double x = c * t / d + b;

			_images[(int)x % _images.Length].Render(timePassed);
		}

		public void Show()
		{
			if(_showCount == 0)
				_tickStart = Environment.TickCount;

			_showCount++;
		}

		public void Hide()
		{
			_showCount--;
		}

		#endregion Methods

		#region Properties
		
		public static GUIWaitCursor Instance
		{
			get { return _instance == null ? _instance = new GUIWaitCursor() : _instance; }
		}

		#endregion Properties

		#region Fields

		static GUIWaitCursor			_instance;
		int								_showCount;

		#endregion Fields
	}
}
