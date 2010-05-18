/*	Use these scripts to create and populate the database tables required for
	running the test cases included with Gentle.NET. There is a separate script 
	for every supported database.
	
	The database in which these tables are created must exist. Also remember
	to edit the configuration file to include the required keys for connecting
	to the database. If you're using NUnit from within VS.NET, add a config
	file in the output directory named "Gentle.Framework.dll.config". */

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MemberPicture]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[MemberPicture]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[ListMember]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[ListMember]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[List]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[List]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[PropertyHolder]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[PropertyHolder]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GuidHolder]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[GuidHolder]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UserRoles]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[UserRoles]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Users]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[Users]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Roles]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[Roles]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MultiType]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[MultiType]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[Order]') 
	and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	drop table [dbo].[Order]
GO

CREATE TABLE [dbo].[List] (
	[ListId] [int] IDENTITY (1, 1) NOT NULL,
	[ListName] [text] NOT NULL,
	[SenderAddress] [nvarchar] (255) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[List] ADD 
	CONSTRAINT [PK_List] PRIMARY KEY  CLUSTERED 
	(
		[ListId]
	)  ON [PRIMARY] 
GO

CREATE TABLE [dbo].[ListMember] (
	[MemberId] [int] IDENTITY (1, 1) NOT NULL,
	[ListId] [int] NOT NULL,
	[MemberName] [text] NULL,
	[MemberAddress] [nvarchar] (255) NOT NULL,
	[DatabaseVersion] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[ListMember] ADD 
	CONSTRAINT [DF_ListMember_DatabaseVersion] DEFAULT (1) FOR [DatabaseVersion],
	CONSTRAINT [PK_ListMember] PRIMARY KEY  CLUSTERED 
	(
		[MemberId]
	)  ON [PRIMARY] 
GO
ALTER TABLE [dbo].[ListMember] ADD
	CONSTRAINT [FK_ListMember_List] FOREIGN KEY
	(
		[ListId]
	) REFERENCES [dbo].[List] (
		[ListId]
	)
GO

CREATE TABLE [dbo].[MemberPicture] (
	[PictureId] [int] IDENTITY (1, 1) NOT NULL,
	[MemberId] [int] NOT NULL,
	[PictureData] [image] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MemberPicture] ADD 
	CONSTRAINT [PK_MemberPicture] PRIMARY KEY  CLUSTERED 
	(
		[PictureId]
	)  ON [PRIMARY] 
GO
ALTER TABLE [dbo].[MemberPicture] ADD 
	CONSTRAINT [FK_MemberPicture_ListMember] FOREIGN KEY
	(
		[MemberId]
	) REFERENCES [dbo].[ListMember] (
		[MemberId]
	)
GO

CREATE TABLE [dbo].[GuidHolder] (
	[Guid] [uniqueidentifier] NOT NULL,
	[SomeValue] [int] NOT NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[GuidHolder] ADD 
	CONSTRAINT [PK_GuidHolder] PRIMARY KEY  CLUSTERED 
	(
		[Guid]
	)  ON [PRIMARY] 
GO

CREATE TABLE [dbo].[PropertyHolder] (
	[ph_Id] [int] IDENTITY (1, 1) NOT NULL,
	[ph_Name] [text] NOT NULL,
	[TInt] [int],
	[TLong] [bigint],
	[TDecimal] [decimal] (30,0),
	[TDouble] [float],
	[TBool] [bit],
	[TDateTime] [datetime],
	[TDateTimeNN] [datetime] NOT NULL,
	[TChar] [char] (8),
	[TNChar] [nchar] (8),
	[TVarChar] [varchar] (8),
	[TNVarChar] [nvarchar] (8),
	[TText] [text],
	[TNText] [ntext]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[PropertyHolder] ADD 
	CONSTRAINT [PK_PropertyHolder] PRIMARY KEY  CLUSTERED 
	(
		[ph_Id]
	)  ON [PRIMARY] 
GO

CREATE TABLE [dbo].[Users] (
	[UserId] [int] IDENTITY (1, 1) NOT NULL,
	[FirstName] [nvarchar] (255) NOT NULL,
	[LastName] [nvarchar] (255) NOT NULL,
	[PrimaryRole] [int] NOT NULL
) ON [PRIMARY] 
GO
ALTER TABLE [dbo].[Users] ADD 
	CONSTRAINT [PK_Users] PRIMARY KEY  CLUSTERED 
	(
		[UserId]
	)  ON [PRIMARY] 
GO

CREATE TABLE [dbo].[Roles] (
	[RoleId] [int] IDENTITY (1, 1) NOT NULL,
	[RoleName] [nvarchar] (255) NOT NULL
) ON [PRIMARY] 
GO
ALTER TABLE [dbo].[Roles] ADD 
	CONSTRAINT [PK_Roles] PRIMARY KEY  CLUSTERED 
	(
		[RoleId]
	)  ON [PRIMARY] 
GO

CREATE TABLE [dbo].[UserRoles] (
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[MemberId] [int],
	[PrimaryRoleId] [int]
) ON [PRIMARY] 
GO
ALTER TABLE [dbo].[UserRoles] ADD 
	CONSTRAINT [PK_UserRoles] PRIMARY KEY  CLUSTERED 
	(
		[UserId],
		[RoleId]
	)  ON [PRIMARY] 
GO
ALTER TABLE [dbo].[UserRoles] ADD
	CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY
	(
		[RoleId]
	) REFERENCES [dbo].[Roles] (
		[RoleId]
	),
	CONSTRAINT [FK_UserRoles_PrimaryRoles] FOREIGN KEY
	(
		[PrimaryRoleId]
	) REFERENCES [dbo].[Roles] (
		[RoleId]
	),
	CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY
	(
		[UserId]
	) REFERENCES [dbo].[Users] (
		[UserId]
	),
	CONSTRAINT [FK_UserRoles_ListMember] FOREIGN KEY
	(
		[MemberId]
	) REFERENCES [dbo].[ListMember] (
		[MemberId]
	)
GO

CREATE TABLE [dbo].[MultiType] (
	[Id] [int] IDENTITY (1, 1) NOT NULL,
	[Type] [nvarchar] (250) NOT NULL,
	[Field1] [int],
	[Field2] [decimal],
	[Field3] [float],
	[Field4] [ntext]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[MultiType] ADD 
	CONSTRAINT [PK_MultiType] PRIMARY KEY  CLUSTERED 
	(
		[Id]
	)  ON [PRIMARY] 
GO

CREATE TABLE [dbo].[Order] (
	[Identity] [int] IDENTITY (1, 1) NOT NULL,
	[Order] [nvarchar] (64),
	[Value] [nvarchar] (64),
	[Of] [nvarchar] (64),
	[Group] [nvarchar] (64),
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Order] ADD 
	CONSTRAINT [PK_Order] PRIMARY KEY  CLUSTERED 
	(
		[Identity]
	)  ON [PRIMARY] 
GO

INSERT INTO [dbo].[List] ( ListName, SenderAddress ) values ( 'Announcements', 'ann-sender@foobar.com' );
INSERT INTO [dbo].[List] ( ListName, SenderAddress ) values ( 'Discussion', 'first@foobar.com' );
INSERT INTO [dbo].[List] ( ListName, SenderAddress ) values ( 'Messages', 'info-sender@foobar.org' );

INSERT INTO [dbo].[ListMember] ( ListId, MemberName, MemberAddress, DatabaseVersion ) values 
	( 1, 'First User', 'first@foobar.com', 1 );
INSERT INTO [dbo].[ListMember] ( ListId, MemberName, MemberAddress, DatabaseVersion ) values 
	( 2, 'First User', 'first@foobar.com', 1 );
INSERT INTO [dbo].[ListMember] ( ListId, MemberName, MemberAddress, DatabaseVersion ) values 
	( 1, 'Second User', 'second@bar.com', 1 );
INSERT INTO [dbo].[ListMember] ( ListId, MemberName, MemberAddress, DatabaseVersion ) values 
	( 3, 'Third User', 'third@foo.org', 1 );
GO


