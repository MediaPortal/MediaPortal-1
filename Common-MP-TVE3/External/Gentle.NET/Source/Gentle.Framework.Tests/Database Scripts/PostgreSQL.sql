
/*	Use these scripts to create and populate the database tables required for
	running the test cases included with Gentle.NET. There is a separate script 
	for every supported database.
	
	The database in which these tables are created must exist. Also remember
	to edit the configuration file to include the required keys for connecting
	to the database. If you're using NUnit from within VS.NET, add a config
	file in the output directory named "Gentle.Framework.dll.config". */

/*	Note: these commands have been tested with PostreSQL 7.3.5 only. If you
	don't have schema support, you'll need to edit them. */

DROP TABLE public.List;
CREATE TABLE public.List (
  ListId SERIAL NOT NULL,
  ListName text NOT NULL,
  SenderAddress varchar(255) NOT NULL,
  PRIMARY KEY(ListId)
);

DROP TABLE public.ListMember;
CREATE TABLE public.ListMember (
  MemberId SERIAL NOT NULL, 
  ListId int NOT NULL, 
  MemberName text NULL, 
  MemberAddress varchar(255) NOT NULL, 
  DatabaseVersion int DEFAULT '1' NOT NULL,
  PRIMARY KEY(MemberId)
);

DROP TABLE public.MemberPicture;
CREATE TABLE public.MemberPicture (
  PictureId SERIAL NOT NULL, 
  PictureData bytea NOT NULL, 
  MemberId int NOT NULL, 
  PRIMARY KEY(PictureId)
);

DROP TABLE public.GuidHolder;
CREATE TABLE public.GuidHolder (
  Guid varchar(36) NOT NULL,
  SomeValue int NOT NULL,
  UNIQUE(Guid),
  PRIMARY KEY(Guid)
);

DROP TABLE public.PropertyHolder;
CREATE TABLE public.PropertyHolder (
  ph_Id SERIAL NOT NULL,
  ph_Name varchar(255) NOT NULL,
  TInt int,
  TLong bigint,
  TDecimal decimal,
  TDouble float,
  TBool boolean,
  TDateTime timestamp,
  TDateTimeNN timestamp NOT NULL,
  TChar char(8),
  TNChar char(8),
  TVarChar varchar(8),
  TNVarChar varchar(8),
  TText text,
  TNText text, 
  PRIMARY KEY(ph_Id)
);

DROP TABLE public.MultiType;
CREATE TABLE public.MultiType (
  Id SERIAL NOT NULL,
  Type varchar(250) NOT NULL,
  Field1 int,
  Field2 decimal,
  Field3 float,
  Field4 text,
  PRIMARY KEY(Id)
);

DROP TABLE public.Users;
CREATE TABLE public.Users (
  UserId SERIAL NOT NULL,
  FirstName text NOT NULL,
  LastName text NOT NULL,
  PrimaryRole int NOT NULL,
  PRIMARY KEY(UserId)
);

DROP TABLE public.Roles;
CREATE TABLE public.Roles (
  RoleId SERIAL NOT NULL,
  RoleName text NOT NULL,
  UNIQUE(RoleName),
  PRIMARY KEY(RoleId)
);

DROP TABLE public.UserRoles;
CREATE TABLE public.UserRoles (
  UserId int NOT NULL,
  RoleId int NOT NULL,
  MemberId int,
  PRIMARY KEY(UserId,RoleId)
);

DROP TABLE public."Order";
CREATE TABLE public."Order" (
	"Identity" SERIAL NOT NULL,
	"Order" varchar(64),
	"Value" varchar(64),
	"Of" varchar(64),
	"Group" varchar(64),
	PRIMARY KEY("Identity")
);

INSERT INTO public.List ( ListName, SenderAddress ) VALUES ( 'Announcements', 'ann-sender@foobar.com' );
INSERT INTO public.List ( ListName, SenderAddress ) VALUES ( 'Discussion', 'first@foobar.com' );
INSERT INTO public.List ( ListName, SenderAddress ) VALUES ( 'Messages', 'info-sender@foobar.org' );

INSERT INTO public.ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion ) 
	VALUES ( 1, 'First User', 'first@foobar.com', 1 );
INSERT INTO public.ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion ) 
	VALUES ( 2, 'First User', 'first@foobar.com', 1 );
INSERT INTO public.ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion ) 
	VALUES ( 1, 'Second User', 'second@bar.com', 1 );
INSERT INTO public.ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion ) 
	VALUES ( 3, 'Third User', 'third@foo.org', 1 );

COMMIT;
