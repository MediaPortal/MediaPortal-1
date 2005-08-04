using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

using Mpe.Controls.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls {

	/// <summary>
	/// This class implements the behaviour for a MediaPortal button object.  It contains
	/// all of the properties and events required for the screen and control editor
	/// to manipulate button objects.
	/// </summary>
	public class MpeToggleButton : MpeButton {

		#region Variables
		private FileInfo alternateTextureFocus;
		private FileInfo alternateTextureNoFocus;
		private bool toggled;
		#endregion

		#region Constructors
		public MpeToggleButton() : base() {
			MpeLog.Debug("MpeToggleButton()");
			Init();
		}
		public MpeToggleButton(MpeToggleButton button) : base(button) {
			MpeLog.Debug("MpeToggleButton(button)");
			Init();
			alternateTextureFocus = button.alternateTextureFocus;
			alternateTextureNoFocus = button.alternateTextureNoFocus;
			toggled = button.toggled;
		}
		private void Init() {
			MpeLog.Debug("MpeToggleButton.Init()");
			Type = MpeControlType.ToggleButton;
			toggled = false;
		}
		#endregion

		#region Properties
		[Category("Textures"),
		Editor(typeof(MpeImageEditor),typeof(System.Drawing.Design.UITypeEditor)),
		RefreshPropertiesAttribute(RefreshProperties.Repaint),
		Description("This property defines the image that will be used to render the button when it has focus.")]
		public FileInfo AlternateTextureFocus {
			get {
				return alternateTextureFocus;
			}
			set {
				if (value != alternateTextureFocus) {
					alternateTextureFocus = value;
					if (Toggled)
						Invalidate(false);
					Modified = true;
					FirePropertyValueChanged("AlternateTextureFocus");
				}
			}
		}
		[Category("Textures"),
		Editor(typeof(MpeImageEditor),typeof(System.Drawing.Design.UITypeEditor)),
		RefreshPropertiesAttribute(RefreshProperties.Repaint),
		Description("This property defines the image that will be used to render the button when it does not have focus.")]
		public FileInfo AlternateTextureNoFocus {
			get {
				return alternateTextureNoFocus;
			}
			set {
				if (value != alternateTextureNoFocus) {
					alternateTextureNoFocus = value;
					if (Toggled)
						Invalidate(false);
					Modified = true;
					FirePropertyValueChanged("AlternateTextureNoFocus");
				}
			}
		}
		[Category("Control"),DefaultValue(false)]
		public bool Toggled { 
			get {
				return toggled;
			}
			set {
				if (toggled != value) {
					toggled = value;
					if (toggled) {
						textureFocusImage = new Bitmap(alternateTextureFocus.FullName);
						textureNoFocusImage = new Bitmap(alternateTextureNoFocus.FullName);
					} else {
						textureFocusImage = new Bitmap(textureFocus.FullName);
						textureNoFocusImage = new Bitmap(textureNoFocus.FullName);
					}
					Invalidate(false);
				}
			}
		}
		#endregion

		#region Methods
		public override MpeControl Copy() {
			return new MpeToggleButton(this);
		}

		public override void Load(System.Xml.XPath.XPathNodeIterator iterator, MpeParser parser) {
			MpeLog.Debug("MpeToggleButton.Load()");
			base.Load(iterator, parser);
			this.parser = parser;
			AlternateTextureFocus = parser.GetImageFile(iterator,"AltTextureFocus",AlternateTextureFocus);
			AlternateTextureNoFocus = parser.GetImageFile(iterator,"AltTextureNoFocus",AlternateTextureNoFocus);
			tags.Remove("AltTextureFocus");
			tags.Remove("AltTextureNoFocus");
			Modified = false;
		}
		public override void Save(System.Xml.XmlDocument doc, System.Xml.XmlNode node, MpeParser parser, MpeControl reference) {
			base.Save (doc, node, parser, reference);
			MpeToggleButton toggle = null;
			if (reference != null && reference is MpeToggleButton) {
				toggle = (MpeToggleButton)reference;
			}
			// TextureFocus
			if (toggle == null || toggle.AlternateTextureFocus == null || toggle.AlternateTextureFocus.Equals(AlternateTextureFocus) == false) {
				if (AlternateTextureFocus == null)
					parser.SetValue(doc, node, "AltTextureFocus", "-");
				else
					parser.SetValue(doc, node, "AltTextureFocus", AlternateTextureFocus.Name);
			}
			// AlternateTextureNoFocus
			if (toggle == null || toggle.AlternateTextureNoFocus == null || toggle.AlternateTextureNoFocus.Equals(AlternateTextureNoFocus) == false) {
				if (AlternateTextureNoFocus == null)
					parser.SetValue(doc, node, "AltTextureNoFocus", "-");
				else
					parser.SetValue(doc, node, "AltTextureNoFocus", AlternateTextureNoFocus.Name);
			}
		}
		#endregion

		#region Event Handlers
		/*
		protected override void OnPaint(PaintEventArgs e) {
			if (Focused && !Toggled) {
				if (textureFocusImage == null) {
					e.Graphics.DrawRectangle(borderPen,0,0,Width-1,Height-1);
				} else {
					e.Graphics.DrawImage(textureFocusImage,0,0,Width,Height);
				}
			} else if (!Focused && !Toggled) {
				if (textureNoFocusImage == null) {
					e.Graphics.DrawRectangle(borderPen,0,0,Width-1,Height-1);
				} else {
					e.Graphics.DrawImage(textureNoFocusImage,0,0,Width,Height);
				}
			} else if (Focused && Toggled) {
				if (alternateTextureFocusImage == null) {
					e.Graphics.DrawRectangle(borderPen,0,0,Width-1,Height-1);
				} else {
					e.Graphics.DrawImage(alternateTextureFocusImage,0,0,Width,Height);
				}
			} else if (!Focused && Toggled) {
				if (alternateTextureNoFocusImage == null) {
					e.Graphics.DrawRectangle(borderPen,0,0,Width-1,Height-1);
				} else {
					e.Graphics.DrawImage(alternateTextureNoFocusImage,0,0,Width,Height);
				}
			}
			if (text != null && text.Length > 0 && !text.Equals("-")) {
				e.Graphics.DrawString(textValue,font.SystemFont,textBrush,(float)Padding.Left, (float)Padding.Top);
			}
		}
		*/
		#endregion
		
	}
}
