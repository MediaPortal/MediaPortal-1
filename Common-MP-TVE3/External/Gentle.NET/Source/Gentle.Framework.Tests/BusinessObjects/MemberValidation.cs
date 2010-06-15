using System;
using System.Collections;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// The MemberValidation class is used to verify custom validators.
	/// </summary>
	[TableName( "ListMember" )]
	public class MemberValidation : Member
	{
		private DateTime dt1 = new DateTime( 2004, 12, 31 );
		private DateTime dt2 = new DateTime( 2005, 1, 1 );

		// construct new list instance (new record)
		public MemberValidation( int id, int listId, string name, string address ) :
			base( id, listId, name, address )
		{
		}

		#region CustomValidator Declaration
		[AttributeUsage( AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
		public class CustomValidatorAttribute : ValidatorBaseAttribute
		{
			public override bool Validate( string propertyName, object propertyValue, object propertyOwner )
			{
				MemberValidation owner = (MemberValidation) propertyOwner;
				if( (int) propertyValue < owner.Id )
				{
					MessageText = "ListId is less than id!";
					return false;
				}
				return true;
			}
		}
		#endregion

		#region Properties
		[TableColumn( "ListId", NotNull = true ), ForeignKey( "List", "ListId" )]
		[CustomValidator]
		public override int ListId
		{
			get { return listId; }
			set { listId = value; }
		}

		[RangeValidator( "2004-01-01", "2004-12-31" )]
		public DateTime DateTime1
		{
			get { return dt1; }
			set { dt1 = value; }
		}
		[RangeValidator( "2005", "2005-01-20 14:08" )]
		public DateTime DateTime2
		{
			get { return dt2; }
			set { dt2 = value; }
		}
		#endregion

		public new static MemberValidation Retrieve( int id )
		{
			Key key = new Key( typeof(MemberValidation), true, "Id", id );
			return Broker.RetrieveInstance( typeof(MemberValidation), key ) as MemberValidation;
		}

		public new static IList ListAll
		{
			get { return Broker.RetrieveList( typeof(MemberValidation) ); }
		}
	}
}