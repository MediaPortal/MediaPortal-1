
--	Use these scripts to create and populate the database tables required for
--	running the test cases included with Gentle.NET. There is a separate script
--	for every supported database.

--	The database in which these tables are created must exist. Also remember
--	to edit the configuration file to include the required keys for connecting
--	to the database. If you're using NUnit from within VS.NET, add a config
--	file in the output directory named 'Gentle.Framework.dll.config'. 

DROP TABLE List;
CREATE TABLE List (
  ListId int(4) UNIQUE NOT NULL auto_increment,
  ListName text NOT NULL,
  SenderAddress varchar(255) NOT NULL,
  PRIMARY KEY (ListId)
) TYPE=InnoDB;

DROP TABLE ListMember;
CREATE TABLE ListMember (
  MemberId int(4) UNIQUE NOT NULL auto_increment,
  ListId int(4) NOT NULL,
  MemberName text NULL,
  MemberAddress varchar(255) NOT NULL,
  DatabaseVersion int(4) default 1 NOT NULL,
  PRIMARY KEY (MemberId),
  INDEX (ListId),
  FOREIGN KEY (ListId) REFERENCES List (ListId)
) TYPE=InnoDB;

DROP TABLE MemberPicture;
CREATE TABLE MemberPicture (
  PictureId int(4) NOT NULL auto_increment,
  MemberId int(4) NOT NULL,
  PictureData mediumblob NOT NULL,
  PRIMARY KEY (PictureId),
  INDEX (MemberId),
  FOREIGN KEY MemberId (MemberId) REFERENCES ListMember (MemberId),
  UNIQUE KEY PictureId (PictureId)
) TYPE=InnoDB;

DROP TABLE GuidHolder;
CREATE TABLE GuidHolder (
  Guid varchar(36) NOT NULL,
  SomeValue int(4) NOT NULL,
  PRIMARY KEY (Guid),
  UNIQUE KEY Guid (Guid)
) TYPE=InnoDB;

DROP TABLE PropertyHolder;
CREATE TABLE PropertyHolder (
  ph_Id int(4) NOT NULL auto_increment,
  ph_Name text NOT NULL,
  TInt int,
  TLong bigint,
  TDecimal decimal,
  TDouble float,
  TBool bit,
  TDateTime datetime,
  TDateTimeNN datetime NOT NULL,
  TChar char(8),
  TNChar nchar(8),
  TVarChar varchar(8),
  TNVarChar varchar(8),
  TText text,
  TNText text,  
  PRIMARY KEY (ph_Id),
  UNIQUE KEY ph_Id (ph_Id)
) TYPE=InnoDB;

DROP TABLE Users;
CREATE TABLE Users (
  UserId int(4) UNIQUE PRIMARY KEY NOT NULL auto_increment,
  FirstName text NOT NULL,
  LastName text NOT NULL,
  PrimaryRole int(4) NOT NULL
) TYPE=InnoDB;

DROP TABLE Roles;
CREATE TABLE Roles (
  RoleId int(4) UNIQUE PRIMARY KEY NOT NULL auto_increment,
  RoleName text NOT NULL
) TYPE=InnoDB;

DROP TABLE UserRoles;
CREATE TABLE UserRoles (
  UserId int(4) NOT NULL,
  RoleId int(4) NOT NULL,
  MemberId int(4),
  PrimaryRoleId int(4),
  PRIMARY KEY (UserId,RoleId),
  INDEX (UserId),
  INDEX (RoleId),
  INDEX (MemberId),
  FOREIGN KEY UserId (UserId) REFERENCES Users (UserId),
  FOREIGN KEY RoleId (RoleId) REFERENCES Roles (RoleId),
  FOREIGN KEY PrimaryRoleId (PrimaryRoleId) REFERENCES Roles (RoleId),
  FOREIGN KEY MemberId (MemberId) REFERENCES ListMember (MemberId)
) TYPE=InnoDB;

DROP TABLE MultiType;
CREATE TABLE MultiType (
	Id int(4) UNIQUE PRIMARY KEY NOT NULL auto_increment,
	Type varchar(250) NOT NULL,
	Field1 int(4),
	Field2 decimal,
	Field3 float,
	Field4 text
) TYPE=InnoDB;

DROP TABLE `Order`;
CREATE TABLE `Order` (
	`Identity` int(4) UNIQUE PRIMARY KEY NOT NULL auto_increment,
	`Order` varchar(64),
	`Value` varchar(64),
	`Of` varchar(64),
	`Group` varchar(64)
) TYPE=InnoDB;

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
