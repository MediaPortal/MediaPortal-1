/*	Use these scripts to create and populate the database tables required for
	running the test cases included with Gentle.NET. There is a separate script
	for every supported database.

	The database in which these tables are created must exist. Also remember
	to edit the configuration file to include the required keys for connecting
	to the database. If you're using NUnit from within VS.NET, add a config
	file in the output directory named 'Gentle.Framework.dll.config'. */

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='GuidHolder')
	DROP TABLE GuidHolder;

CREATE TABLE GuidHolder (
  Guid uniqueidentifierstr NOT NULL,
  SomeValue int NOT NULL,
  PRIMARY KEY (Guid)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='MemberPicture')
	DROP TABLE MemberPicture

CREATE TABLE MemberPicture (
  PictureId int NOT NULL DEFAULT autoincrement,
  MemberId int NOT NULL,
  PictureData long binary NOT NULL,
  PRIMARY KEY (PictureId)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='List')
	DROP TABLE List;

CREATE TABLE List (
  ListId int NOT NULL DEFAULT autoincrement,
  ListName text NOT NULL,
  SenderAddress varchar(255) NOT NULL,
  PRIMARY KEY (ListId)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='ListMember')
	DROP TABLE ListMember;

CREATE TABLE ListMember (
  MemberId int NOT NULL DEFAULT autoincrement,
  ListId int NOT NULL,
  MemberName text,
  MemberAddress varchar(255) NOT NULL,
  DatabaseVersion int DEFAULT 1 NOT NULL,
  PRIMARY KEY (MemberId)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='PropertyHolder')
	DROP TABLE PropertyHolder;

CREATE TABLE PropertyHolder (
  ph_Id int NOT NULL DEFAULT autoincrement,
  ph_Name text NOT NULL,
  TInt int,
  TLong bigint,
  TDecimal decimal,
  TDouble float,
  TBool bit,
  TDateTime datetime,
  TDateTimeNN datetime NOT NULL,
  TChar char(8),
  TNChar char(8),
  TVarChar varchar(8),
  TNVarChar varchar(8),
  TText text,
  TNText text,
  PRIMARY KEY (ph_Id)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='Users')
	DROP TABLE Users;
	
CREATE TABLE Users (
  UserId int NOT NULL DEFAULT autoincrement,
  FirstName text NOT NULL,
  LastName text NOT NULL,
  PrimaryRole int NOT NULL,
  PRIMARY KEY (UserId)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='Roles')
	DROP TABLE Roles;
	
CREATE TABLE Roles (
  RoleId int NOT NULL DEFAULT autoincrement,
  RoleName text NOT NULL,
  PRIMARY KEY (RoleId)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='UserRoles')
	DROP TABLE UserRoles;
	
CREATE TABLE UserRoles (
  UserId int NOT NULL,
  RoleId int NOT NULL,
  MemberId int,
  PRIMARY KEY (UserId,RoleId)
);

IF EXISTS(SELECT table_name FROM SYSTABLE WHERE table_name='MultiType')
	DROP TABLE MultiType;
	
CREATE TABLE MultiType (
  Id int NOT NULL DEFAULT autoincrement,
  Type varchar(250) NOT NULL,
  Field1 int,
  Field2 decimal,
  Field3 float,
  Field4 text,
  PRIMARY KEY (Id)
);

INSERT INTO List ( ListName, SenderAddress ) VALUES ( 'Announcements', 'ann-sender@foobar.com' );
INSERT INTO List ( ListName, SenderAddress ) VALUES ( 'Discussion', 'first@foobar.com' );
INSERT INTO List ( ListName, SenderAddress ) VALUES ( 'Messages', 'info-sender@foobar.org' );

INSERT INTO ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion )
	VALUES ( 1, 'First User', 'first@foobar.com', 1 );
INSERT INTO ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion )
	VALUES ( 2, 'First User', 'first@foobar.com', 1 );
INSERT INTO ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion )
	VALUES ( 1, 'Second User', 'second@bar.com', 1 );
INSERT INTO ListMember ( ListId, MemberName, MemberAddress, DatabaseVersion )
	VALUES ( 3, 'Third User', 'third@foo.org', 1 );
COMMIT;
