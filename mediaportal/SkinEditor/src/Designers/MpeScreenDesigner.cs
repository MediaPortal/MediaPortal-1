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
	public class MpeScreenDesigner : MpeResourceDesigner {
		#region Variables
		private MpeScreenInfo screenInfo;
		#endregion

		#region Constructors
		public MpeScreenDesigner(MediaPortalEditor mpe, MpeScreenInfo screenInfo) : base(mpe) {
			this.screenInfo = screenInfo;
		}
		#endregion

		#region Properties - MpeDesigner
		public override string ResourceName {
			get {
				return screenInfo.Name;
			}
		}
		public override bool AllowAdditions {
			get {
				return true;
			}
		}
		public override bool AllowDeletions {
			get {
				return true;
			}
		}
		#endregion

		#region Methods - MpeDesigner
		public override void Initialize() {
			try {
				screen = Parser.CreateScreen(screenInfo.File, Mask.NodeSize, Mask.NodeSize, MpeScreenSize.PAL);
				AddControl(screen);
				ClearControlStatus(screen);
				MpeLog.Info("Screen designer initialized [" + ResourceName + "]");
			} catch (MpeParserException e) {
				MpeLog.Debug(e);
				MpeLog.Error(e);
				throw new DesignerException(e.Message);
			}
		}
		public override void Resume() {
			base.Resume();
			MpeLog.Info("Screen designer resumed [" + ResourceName + "]");
		}
		public override void Pause() {
			base.Pause();
			MpeLog.Info("Screen designer paused [" + ResourceName + "]");
		}
		public override void Save() {
			try {
				Parser.SaveScreen(screen, screenInfo.File);
				ClearControlStatus(screen);
				MpeLog.Info("Screen designer saved [" + ResourceName + "]");
			} catch (MpeParserException mpe) {
				MessageBox.Show(this,"Error Saving File: " + mpe.Message,"Save Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw new DesignerException(mpe.Message, mpe);
			}
		}
		public override void Cancel() {
			MpeLog.Info("Screen designer cancelled [" + ResourceName + "]");
		}
		public override void Destroy() {
			base.Destroy();
			MpeLog.Info("Screen designer destroyed [" + ResourceName + "]");
		}
		#endregion

		#region Methods
		private void ClearControlStatus(MpeControl c) {
			if (c != null) {
				c.Modified = false;
				if (c is MpeContainer) {
					for (int i = 0; i < c.Controls.Count; i++) {
						if (c.Controls[i] is MpeControl) {
							ClearControlStatus((MpeControl)c.Controls[i]);
						}
					}
				}
			}
		}
		#endregion

		#region Event Handlers
		public override void OnControlStatusChanged(MpeControl sender, bool modified) {
			if (sender == screen) {
				base.OnControlStatusChanged(sender, modified);
			} else {
				if (modified == true && screen.Modified == false) {
					screen.Modified = true;
				} else if (modified == false && screen.Modified == true) {
					screen.Modified = false;
				}
			}
		}
		#endregion
	}
}
