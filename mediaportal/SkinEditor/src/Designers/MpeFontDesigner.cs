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
	
	public class FontDesigner : MpeResourceDesigner {
		#region Variables
		private MpeFont font;
		private MpeFontViewer viewer;
		#endregion

		#region Constructors
		public FontDesigner(MediaPortalEditor mpe, MpeFont font) : base(mpe) {
			this.font = font;
		}
		#endregion

		#region Methods - MpeDesigner
		public override void Initialize() {
			try {
				AllowDrop = false;
				PropertyManager.HideResourceList();
				if (font != null) {
					screen = (MpeScreen)Parser.GetControl(MpeControlType.Screen);
					viewer = new MpeFontViewer(font, screen.TextureBack);
					viewer.Location = new Point(Mask.NodeSize,Mask.NodeSize);
					viewer.SelectedIndexChanged += new Mpe.Controls.MpeFontViewer.SelectedIndexChangedHandler(OnViewerIndexChanged);
					viewer.Modified = false;
					Controls.Add(viewer);
				}
				MpeLog.Info("Font designer initialized [" + ResourceName + "]");
			} catch (MpeParserException mpe) {
				MpeLog.Debug(mpe);
				throw new DesignerException(mpe.Message);
			} catch (Exception e) {
				MpeLog.Debug(e);
				throw new DesignerException(e.Message);
			}
		}
		public override void Save() {
			try {
				Parser.SaveFont(font);
				viewer.Modified = false;
				MpeLog.Info("Font designer saved [" + ResourceName + "]");
			} catch (MpeParserException mpe) {
				throw new DesignerException(mpe.Message, mpe);
			}
		}
		public override void Cancel() {
			base.Cancel();
			MpeLog.Info("Font designer cancelled [" + ResourceName + "]");
		}
		public override void Destroy() {
			base.Destroy();
			if (screen != null)
				screen.Destroy();
			if (viewer != null)
				viewer.Destroy();
			MpeLog.Info("Font designer destroyed [" + ResourceName + "]");
		}
		public override void Pause() {
			base.Pause();
			MpeLog.Info("Font designer paused [" + ResourceName + "]");
		}
		public override void Resume() {
			base.Resume();
			PropertyManager.SelectedResource = viewer;
			PropertyManager.HideResourceList();
			MpeLog.Info("Font designer resumed [" + ResourceName + "]");
		}
		#endregion

		#region Designer Implementation Properties
		public override string ResourceName {
			get {
				return font.Name;
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

		private void OnViewerIndexChanged(int oldIndex, int newIndex) {
			PropertyManager.Refresh();
		}
	}
}
