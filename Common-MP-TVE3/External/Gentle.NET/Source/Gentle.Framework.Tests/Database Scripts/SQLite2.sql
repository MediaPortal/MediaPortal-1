


/* THIS SCRIPT ONLY WORKS WITH SQLite 2.x */



DROP TABLE [List];
CREATE TABLE [List]
(
        [ListID] INTEGER AUTOINCREMENT PRIMARY KEY NOT NULL,
        [ListName] VARCHAR (50) NOT NULL,
        [SenderAddress] VARCHAR (255) NOT NULL
);
INSERT INTO List VALUES(1,'Announcements','ann-sender@foobar.com');
INSERT INTO List VALUES(2,'Discussion','first@foobar.com');
INSERT INTO List VALUES(3,'Messages','info-sender@foobar.org');

DROP TABLE [ListMember];
CREATE TABLE [ListMember]
(
        [MemberID] INTEGER AUTOINCREMENT PRIMARY KEY NOT NULL,
        [ListID] INTEGER NOT NULL DEFAULT 0,
        [MemberName] VARCHAR (50),
        [MemberAddress] VARCHAR (255) NOT NULL,
        [DatabaseVersion] INTEGER NOT NULL DEFAULT 1
);
INSERT INTO ListMember VALUES(1,1,'First User','first@foobar.com',1);
INSERT INTO ListMember VALUES(2,2,'First User','first@foobar.com',1);
INSERT INTO ListMember VALUES(3,1,'Second User','second@bar.com',1);
INSERT INTO ListMember VALUES(4,3,'Third User','third@foo.org',1);

DROP TABLE [MemberPicture];
CREATE TABLE [MemberPicture]
(
        [PictureID] INTEGER AUTOINCREMENT PRIMARY KEY NOT NULL,
        [MemberID] INTEGER NOT NULL,
        [PictureData] BLOB NOT NULL
);

DROP TABLE [GuidHolder];
CREATE TABLE [GuidHolder]
(
        [Guid] TEXT PRIMARY KEY NOT NULL,
        [SomeValue] INTEGER NOT NULL
);

DROP TABLE [PropertyHolder];
CREATE TABLE [PropertyHolder]
(
        [ph_Id] INTEGER AUTOINCREMENT PRIMARY KEY NOT NULL,
        [ph_Name] VARCHAR (50) NOT NULL ,
        [TInt] INTEGER DEFAULT -1,
        [TLong] LONG DEFAULT -1,
        [TDecimal] NUMERIC (0,0) DEFAULT -1,
        [TDouble] FLOAT DEFAULT -1,
        [TBool] BOOLEAN,
        [TDateTime] DATETIME,
        [TDateTimeNN] DATETIME NOT NULL,
        [TChar] VARCHAR (8),
        [TNChar] VARCHAR (8),
        [TVarChar] VARCHAR (8),
        [TNVarChar] VARCHAR (8),
        [TText] VARCHAR (50),
        [TNText] VARCHAR (50)
);

DROP TABLE [Users];
CREATE TABLE [Users]
(
        [UserID] INTEGER AUTOINCREMENT PRIMARY KEY NOT NULL,
        [FirstName] TEXT (255) NOT NULL,
        [LastName] TEXT (255) NOT NULL,
        [PrimaryRole] INTEGER NOT NULL
);

DROP TABLE [Roles];
CREATE TABLE [Roles]
(
        [RoleID] INTEGER AUTOINCREMENT PRIMARY KEY NOT NULL,
        [RoleName] TEXT (255) NOT NULL
);

DROP TABLE [UserRoles];
CREATE TABLE [UserRoles]
(
        [UserID] INTEGER NOT NULL,
        [RoleID] INTEGER NOT NULL,
        [MemberID] INTEGER NOT NULL,
        PRIMARY KEY (UserID,RoleID)
);

DROP TABLE [MultiType];
CREATE TABLE [MultiType]
(
        [ID] INTEGER AUTOINCREMENT PRIMARY KEY NOT NULL,
        [Type] TEXT (250) NOT NULL,
        [Field1] INTEGER NOT NULL,
        [Field2] TEXT NOT NULL,
        [Field3] REAL NOT NULL,
        [Field4] TEXT NOT NULL
);

COMMIT;
