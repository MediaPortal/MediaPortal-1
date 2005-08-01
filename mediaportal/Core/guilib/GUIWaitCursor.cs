using System;

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

			cursor.FinalizeConstruction();
			cursor.PreAllocResources();
			cursor.AllocResources();
		}

		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction();

			string format = GUIGraphicsContext.Skin + @"\media\common.waiting.{0}.png";

			int x = (GUIGraphicsContext.Width / 2) - 24;
			int y = (GUIGraphicsContext.Height / 2) - 24;

			_images = new GUIImage[8];

			for(int index = 0; index < _images.Length; index++)
				_images[index] = new GUIImage(this.ParentID, 200001 + index, x, y, 48, 48, string.Format(format, index + 1), 0xFFFFFFFF);
		}

		public override void AllocResources()
		{
			base.AllocResources();

			if(_images == null)
				return;

			for(int index = 0; index < _images.Length; index++)
				_images[index].AllocResources();
		}

		public override void PreAllocResources()
		{
			base.PreAllocResources();

			if(_images == null)
				return;

			for(int index = 0; index < _images.Length; index++)
				_images[index].PreAllocResources();
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
