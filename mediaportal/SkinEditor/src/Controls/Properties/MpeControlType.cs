using System;
using System.Collections;
using System.Reflection;

namespace Mpe.Controls.Properties
{
	public class MpeControlType {
		private string type;
		private string displayName;
		private static SortedList types;
		private MpeControlType(string type) {
			this.type = type;
			this.displayName = null;
		}
		public string DisplayName {
			get {
				if (IsKnown) {
					if (displayName == null) {
						FieldInfo[] info = GetType().GetFields(BindingFlags.Static|BindingFlags.Public);
						for (int i = 0; i < info.Length; i++) {
							MpeControlType t = (MpeControlType)info[i].GetValue(null);
							if (t.type == type) {
								displayName = info[i].Name;
							}
						}
					}
					return displayName;
				}
				return type;
			}
		}
		public bool IsCustom {
			get {
				return !IsKnown;
			}
		}
		public bool IsKnown {
			get {
				if (types == null || types[type] == null)
					return false;
				return true;
			}
		}
		public static MpeControlType Create(string type) {
			return new MpeControlType(type);
		}
		public static bool operator ==(MpeControlType t1, MpeControlType t2) {
			return t1.type.Equals(t2.type);
		}
		public static bool operator !=(MpeControlType t1, MpeControlType t2) {
			return !(t1.type.Equals(t2.type));
		}
		public override bool Equals(object obj) {
			if (obj != null && obj is MpeControlType) {
				MpeControlType t = (MpeControlType)obj;
				return type.Equals(t.type);
			}
			return base.Equals(obj);
		}
		public override int GetHashCode() {
			return type.GetHashCode();
		}
		public override string ToString() {
			return type;
		}
		// Empty or Default Type
		public static MpeControlType Empty = new MpeControlType("");
		// Known Types
		public static MpeControlType Image = new MpeControlType("image");
		public static MpeControlType Label = new MpeControlType("label");
		public static MpeControlType FadeLabel = new MpeControlType("fadelabel");
		public static MpeControlType FacadeView = new MpeControlType("facadeview");
		public static MpeControlType TextArea = new MpeControlType("textbox");
		public static MpeControlType Button = new MpeControlType("button");
		public static MpeControlType ToggleButton = new MpeControlType("togglebutton");
		public static MpeControlType SpinButton = new MpeControlType("spincontrol");
		public static MpeControlType SelectButton = new MpeControlType("selectbutton");
		public static MpeControlType CheckBox = new MpeControlType("checkmark");
		public static MpeControlType Group = new MpeControlType("group");
		public static MpeControlType Screen = new MpeControlType("window");
		
		public static MpeControlType[] KnownControlTypes {
			get {
				if (types == null) {
					types = new SortedList();
					FieldInfo[] info = (typeof(MpeControlType)).GetFields(BindingFlags.Static|BindingFlags.Public);
					for (int i = 0; i < info.Length; i++) {
						if (info[i].Name != "Empty") {
							MpeControlType t = (MpeControlType)info[i].GetValue(null);
							types.Add(t.ToString(), t);
						}
					}
				}
				MpeControlType[] result = new MpeControlType[types.Count];
				for (int i = 0; i < types.Count; i++) {
					result[i] = (MpeControlType)types.GetByIndex(i);
				}
				return result;
			}
		}

	}

	/*
	public enum MpeControlType { 
		Image = 10000,
		Label, 
		FadeLabel,
		//ProgressBar, 
		//ScrollBar,
		Button,
		SelectButton,
		ToggleButton,
		//MultiButton,
		SpinButton,
		//RadioButton,
		CheckBox,
		TextArea,
		Group,
		//ListPanel,
		//ThumbnailPanel,
		//FilmstripPanel,
		//Dialog,
		Screen,
		ImageViewer,
		FontViewer,
		Custom
	}*/
}
