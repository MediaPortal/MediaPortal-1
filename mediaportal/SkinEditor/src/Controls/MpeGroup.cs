using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml;
using System.Xml.XPath;

using Mpe.Controls.Properties;


namespace Mpe.Controls {

	public class MpeGroup : MpeContainer {

		#region Variables
		private MpeAnimationType animation;
		#endregion

		#region Constructors
		public MpeGroup() : base() {
			MpeLog.Debug("MpeGroup()");
			Type = MpeControlType.Group;
			AllowDrop = true;
			animation = MpeAnimationType.None;
		}
		public MpeGroup(MpeGroup group) : base(group) {
			MpeLog.Debug("MpeGroup(group)");
			Type = MpeControlType.Group;
			AllowDrop = true;
			animation = group.animation;
		}
		#endregion

		#region Properties
		[Category("Control")]
		public MpeAnimationType Animation {
			get {
				return animation;
			}
			set {
				if (animation != value) {
					animation = value;
					Modified = true;
					FirePropertyValueChanged("Animation");
				}
			}
		}
		#endregion

		#region Methods
		public override MpeControl Copy() {
			return new MpeGroup(this);
		}
		public override void Load(System.Xml.XPath.XPathNodeIterator iterator, MpeParser parser) {
			MpeLog.Debug("MpeGroup.Load()");
			base.Load(iterator, parser);
			this.parser = parser;
			Animation = parser.GetAnimation(iterator, "animation", Animation);
			tags.Remove("animation");

			// Mpe Specific Tags
			bool firstLoad = false;
			if (parser.GetString(iterator, "mpe/layout", null) == null) {
				MpeLog.Debug("This is a group that has never been opened with MPE!");
				firstLoad = true;
				Left = 0;
				Top = 0;
			}
			LayoutStyle = parser.GetLayout(iterator, "mpe/layout", LayoutStyle);
			Spring = parser.GetBoolean(iterator,"mpe/spring",Spring);
			Spacing = parser.GetInt(iterator,"mpe/spacing",Spacing);
			Padding = parser.GetPadding(iterator,"mpe/padding",Padding);
			// Child Controls
			XPathNodeIterator i = iterator.Current.Select("control");
			bool firstControl = true;
			int x = int.MaxValue;
			int y = int.MaxValue;
			int r = 0;
			int b = 0;
			while (i.MoveNext()) {
				XPathNodeIterator typeIterator = i.Current.SelectChildren("type","");
				if (typeIterator.MoveNext()) {
					MpeControlType ctype = MpeControlType.Create(typeIterator.Current.Value);
					if (firstControl && ctype == MpeControlType.Image) {
						firstControl = false;
						backImage.Load(i, parser);
					} else {
						MpeControl c = parser.CreateControl(ctype);
						Controls.Add(c);
						c.Load(i, parser);
						c.BringToFront();		
						if (firstLoad) {
							if (c.Left < x) x = c.Left;
							if (c.Top < y) y = c.Top;
							if ((c.Left + c.Width) > r) r = c.Left + c.Width;
							if ((c.Top + c.Height) > b) b = c.Top + c.Height;
						}
					}
				}
			} 
			if (firstLoad) {
				MpeLog.Info("x=" + x + " y=" + y);
				Left = x - 4;
				Top = y - 4;
				for (int a = 0; a < Controls.Count; a++) {
					if (Controls[a] is MpeControl) {
						Controls[a].Left -= x - 4;
						Controls[a].Top -= y - 4 ;
					}
				}
				Width = r - x + 8;
				Height = b - y + 8;
			}

			if (Spring) {
				Width = parser.GetInt(iterator,"width",Width);
				Height = parser.GetInt(iterator,"height",Height);
			}
			Modified = false;
		}
		public override void Save(System.Xml.XmlDocument doc, System.Xml.XmlNode node, MpeParser parser, MpeControl reference) {
			if (doc != null && node != null) {
				
				base.Save(doc, node, parser, reference);
				parser.SetValue(doc, node, "animation", Animation.ToString());

				XmlElement mpenode = doc.CreateElement("mpe");
				node.AppendChild(mpenode);

				parser.SetValue(doc, mpenode, "layout", LayoutStyle.ToString());
				parser.SetValue(doc, mpenode, "spring", Spring ? "yes" : "no");
				parser.SetInt(doc, mpenode, "spacing", Spacing);
				parser.SetPadding(doc, mpenode, "padding", Padding);

				if (backImage != null && backImage.Texture != null) {
					XmlElement image = doc.CreateElement("control");
					backImage.Location = AbsoluteLocation;
					backImage.Save(doc, image, parser, parser.GetControl(MpeControlType.Image));
					backImage.Location = Point.Empty;
					node.AppendChild(image);
				}

				if (reference != null) {

					for (int i = Controls.Count - 1; i >= 0; i--) {
						if (Controls[i] is MpeControl) {
							MpeControl control = (MpeControl)Controls[i];
							try {
								XmlElement element = doc.CreateElement("control");
								MpeControl referenceControl = parser.GetControl(control.Type);
								control.Save(doc, element, parser, referenceControl);
								node.AppendChild(element);
							} catch (Exception e) {
								MpeLog.Debug(e);
								MpeLog.Error(e);
								throw new MpeParserException(e.Message);
							}
						}
					}
				}
			}
		}
		#endregion

		#region Event Handlers
		protected override void OnPaint(PaintEventArgs e) {
			if (ShowBorder) {
				e.Graphics.DrawRectangle(borderPen,0,0,Width-1,Height-1);
			}
			if (Masked) {
				if (Padding.None == false) {
					e.Graphics.DrawRectangle(borderPen,Padding.Left,Padding.Top,Width-Padding.Left-Padding.Right-1,Height-Padding.Top-Padding.Bottom-1);
				}
			}
			base.OnPaint(e);
		}
		#endregion

	}

}
