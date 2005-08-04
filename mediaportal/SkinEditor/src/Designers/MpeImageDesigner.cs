using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

using Mpe.Controls;
using Mpe.Controls.Properties;

namespace Mpe.Designers {
	/// <summary>
	///
	/// </summary>
	public class MpeImageDesigner : MpeResourceDesigner {
		#region Variables
		private FileInfo imageFile;
		#endregion

		#region Constructors
		public MpeImageDesigner(MediaPortalEditor mpe, FileInfo image) : base(mpe) {
			imageFile = image;
		}
		#endregion

		#region Properties - Designer
		public override string ResourceName {
			get {
				return imageFile.Name;
			}
		}
		public override bool AllowAdditions {
			get {
				return false;
			}
		}
		public override bool AllowDeletions {
			get {
				return false;
			}
		}
		#endregion

		#region Methods - Designer
		public override void Resume() {
			base.Resume();
			MpeLog.Info("Image designer resumed [" + ResourceName + "]");
		}
		public override void Pause() {
			base.Pause();
			MpeLog.Info("Image designer paused [" + ResourceName + "]");
		}
		public override void Initialize() {
			screen = (MpeScreen)Parser.CreateControl(MpeControlType.Screen);
			screen.Location = new Point(Mask.NodeSize, Mask.NodeSize);

			Controls.Add(screen);

			MpeImageViewer image = new MpeImageViewer();
			image.Id = 1;
			image.Texture = imageFile;
			screen.Controls.Add(image);
			MpeLog.Info("Image designer initialized [" + ResourceName + "]");
		}
		public override void Save() {
			MpeLog.Info("Image designer saved [" + ResourceName + "]");
		}
		public override void Cancel() {
			MpeLog.Info("Image designer cancelled [" + ResourceName + "]");
		}
		public override void Destroy() {
			base.Destroy();
			if (screen != null) {
				screen.Dispose();
			}
			MpeLog.Info("Image designer destroyed [" + ResourceName + "]");
		}

		#endregion
		
		#region Event Handlers
		public override void OnControlStatusChanged(MpeControl sender, bool modified) {
			//
		}
		#endregion
	}
}
