using System;
using System.Text;
using System.ComponentModel;

using Mpe.Controls.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls {

	#region MpeItem
	[TypeConverter(typeof(MpeItemConverter))]
	public class MpeItem {
		private string text;
		private string textValue;
		private MpeItemType type;

		public MpeItem() {
			text = "";
			textValue = "";
			type = MpeItemType.Text;
		}
		public MpeItem(MpeItem item) {
			text = item.text;
			textValue = item.textValue;
			type = item.type;
		}
		[Category("Item"),
		RefreshPropertiesAttribute(RefreshProperties.Repaint),
		Editor(typeof(MpeStringEditor),typeof(System.Drawing.Design.UITypeEditor))]
		public string Value {
			get {
				return text;
			}
			set {
				text = value;
				switch (Type) {
					case MpeItemType.Text:
						try {
							textValue = MediaPortalEditor.Global.Parser.GetString("English",int.Parse(text));
							if (textValue == null || textValue.Length <= 0)
								textValue = value;
						} catch (Exception ee) {
							textValue = value;
							MpeLog.Warn(ee);
						}
						break;
					case MpeItemType.Integer:
						textValue = text;
						break;
					case MpeItemType.Float:
						textValue = text;
						break;
				}
			}
		}
		[Category("Item")]
		public string Display {
			get {
				switch (Type) {
					case MpeItemType.Text:
						if (text.Equals(textValue))
							return text;
						return "[" + text + "] " + textValue;
					default:
						return text;
				}
			}
		}
		
		[Browsable(false)]
		public MpeItemType Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
	}
	#endregion

	#region MpeItemType
	public enum MpeItemType {
		Integer,
		Float,
		Text
	}
	#endregion

	#region MpeItemConverter
	internal class MpeItemConverter : StringConverter {
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destType ) {
			if (destType == typeof(string) && value is MpeItem) {
				string s = ((MpeItem)value).Display;
				if (s.Length <= 0)
					return "0";
				return s;
			}
			return base.ConvertTo(context,culture,value,destType);
		}
	}
	#endregion

	#region MpeItemDescriptor
	internal class MpeItemDescriptor : PropertyDescriptor {
		
		private MpeItem item;
		private int index;
		private int count;

		public MpeItemDescriptor(MpeItem item, int index, int count) : base(item.Value, null) {
			this.item = item;
			this.index = index;
			this.count = count;
		}
		public override AttributeCollection Attributes {
			get { 
				return new AttributeCollection(null);
			}
		}

		public override bool CanResetValue(object component) {
			return true;
		}

		public override Type ComponentType {
			get { 
				return typeof(MpeItem);
			}
		}

		public override string DisplayName {
			get { 
				string s = (count - 1).ToString();
				return "[" + index.ToString("D" + s.Length.ToString()) + "]";
			}
		}

		public override string Description {
			get {
				return "";
			}
		}

		public override object GetValue(object component) {
			return item;
		}

		public override bool IsReadOnly {
			get {
				return true;
			}
		}

		public override string Name {
			get { 
				string s = (count - 1).ToString();
				return "[" + index.ToString("D" + s.Length.ToString()) + "]";
			}
		}

		public override Type PropertyType {
			get { 
				return item.GetType(); 
			}
		}

		public override void ResetValue(object component) {
			//
		}

		public override bool ShouldSerializeValue(object component) {
			return true;
		}

		public override void SetValue(object component, object value) {
			if (value is MpeItem)
				item = (MpeItem)value;
		}
	}
	#endregion

}
