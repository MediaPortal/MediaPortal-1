using System;
using System.Collections;

namespace Mpe.Controls
{
	/// <summary>
	/// Summary description for MpeStringTable.
	/// </summary>
	public class MpeStringTable {

		#region Variables
		private SortedList strings;
		private string language;
		#endregion

		#region Constructors
		public MpeStringTable(string language) {
			this.strings = new SortedList();
			this.language = language;
		}
		#endregion

		#region Methods
		public void Add(int id, string value) {
			strings.Add(id, value);
		}
		public void Clear() {
			strings.Clear();
		}
		#endregion

		#region Properties
		public string this[int id] {
			get {
				return (string)strings[id];	
			}
		}
		public int[] Keys {
			get {
				ICollection c = strings.Keys;
				int[] ids = new int[c.Count];
				int i = 0;
				IEnumerator e = c.GetEnumerator();
				while (e.MoveNext()) {
					ids[i++] = (int)e.Current;
				}
				return ids;
			}
		}
		public string Language {
			get {
				return language;
			}
		}
		#endregion

	}
}
