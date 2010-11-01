use %TvLibrary%
GO

CREATE TABLE "PendingDeletion"(
	idPendingDeletion int IDENTITY(1,1) NOT NULL,	
	fileName varchar(200) NOT NULL	
 CONSTRAINT PK_PendingDeletion PRIMARY KEY
(
	idPendingDeletion ASC
)
)

GO

UPDATE Version SET versionNumber=55
GO
