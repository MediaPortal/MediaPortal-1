/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: UserRole.cs 1232 2008-03-14 05:36:00Z mm $
 */

using Gentle.Common;

namespace Gentle.Framework.Tests
{
	[TableName( "UserRoles" )]
	public class UserRole : Persistent
	{
		[TableColumn( "UserId" ), PrimaryKey, ForeignKey( "Users", "UserId" )]
		protected int userId;
		[TableColumn( "RoleId" ), PrimaryKey, ForeignKey( "Roles", "RoleId" )]
		protected int roleId;
		[TableColumn( "MemberId", NullValue = 0 )]
		protected int memberId;

		public UserRole( int userId, int roleId )
		{
			Check.Verify( userId > 0, "Unable to create relation without valid UserId." );
			Check.Verify( roleId > 0, "Unable to create relation without valid RoleId." );
			this.userId = userId;
			this.roleId = roleId;
		}

		public virtual int UserId
		{
			get { return userId; }
		}

		public virtual int RoleId
		{
			get { return roleId; }
		}

		public virtual int MemberId
		{
			get { return memberId; }
			set { memberId = value; }
		}
	}
}