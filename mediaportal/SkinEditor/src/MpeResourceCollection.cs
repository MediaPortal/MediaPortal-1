using System;
using System.Collections;

namespace Mpe.Controls {
	/// <summary>
	/// Summary description for MpeResourceCollection.
	/// </summary>
	public class MpeResourceCollection {

		#region Variables
		private ArrayList resources;
		#endregion;

		#region Constuctor
		public MpeResourceCollection() {
			resources = new ArrayList();
		}
		#endregion
		
		#region Properties
		public int Count {
			get {
				return resources.Count;
			}
		}
		public MpeResource this[int index] {
			get {
				return (MpeResource)resources[index];
			}
		}

		public MpeResource this[string id] { 
			get {
				for (int i = 0; i < resources.Count; i++) {
					MpeResource r = (MpeResource)resources[i];
					if (r.Id.Equals(id)) {
						return r;
					}
				}
				return null;
			}
		}

		public ArrayList DataSource {
			get {
				return resources;
			}
		}
		#endregion

		#region Methods
		public void Add(MpeResource resource) {
			if (resource == null) {
				MpeLog.Warn("The control was null and therefore not added to the resource collection.");
				return;
			}
			if (IsUniqueId(resource.Id) == false) {
				int id = GenerateUniqueId();
				MpeLog.Warn("The control id must be unique. The id [" + resource.Id + "] will be changed to [" + id + "]");
				resource.Id = id;
			}
			resources.Add(resource);
		}
		public void Remove(MpeResource resource) {
			if (resource == null) {
				throw new MpCollectionException("A null resource cannot be removed from the collection");
			}
			resources.Remove(resource);
		}

		public void Clear() {
			resources.Clear();
		}

		public bool IsUniqueId(int id) {
			if (id == 1) {
				// Dangerous because we are allowing multiple controls to have this Id
				return true;
			}
			for (int i = 1; i < resources.Count; i++) {
				if (this[i].Id == id) {
					return false;
				}
			}
			return true;
		}
		public int GenerateUniqueId() {
			int id;
			int i = 24;
			do {
				id = i++;
			}	while (IsUniqueId(id) == false);
			return id;
		}
		#endregion
	}

	public class MpCollectionException : Exception {
		public MpCollectionException(string msg) : base(msg) {
			//
		}
	}
}
