using System;
using System.IO;

namespace Mpe.Controls.Properties
{
	public class MpeScreenInfo {
	
		private FileInfo file;
		private MpeScreenType type;
		public MpeScreenInfo() {
			type = MpeScreenType.Window;
		}
		public MpeScreenInfo(FileInfo file, MpeScreenType type) {
			this.file = file;
			this.type = type;
		}
		public FileInfo File {
			get {
				return file;
			}
			set {
				file = value;
			}
		}
		public string Name {
			get {
				if (file != null)
					return file.Name;
				return "";
			}
		}
		public MpeScreenType Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
	}
}
