using System;
using System.Reflection;

namespace System.Windows
{
	public class ComponentResourceKey : ResourceKey
	{
		#region Constructors

		public ComponentResourceKey()
		{
		}

		public ComponentResourceKey(Type targetType, object id)
		{
			_targetType = targetType;
			_id = id;
		}

		#endregion Constructors

		#region Properties

		public override Assembly Assembly
		{
			get { return _targetType.Assembly; }
		}

		public object ID
		{
			get { return _id; }
			set { _id = value; }
		}

		public Type TargetType
		{
			get { return _targetType; }
			set { _targetType = value; }
		}

		#endregion Properties

		#region Fields

		object						_id;
		Type						_targetType;

		#endregion Fields
	}
}
