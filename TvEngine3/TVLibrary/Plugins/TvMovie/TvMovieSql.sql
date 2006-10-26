use TvLibrary
GO

IF OBJECT_ID (N'TvMovieMapping',N'U') IS NULL
CREATE TABLE TvMovieMapping(
	idMapping int IDENTITY(1,1) NOT NULL,
	idChannel int NOT NULL,
	stationName varchar(200) NOT NULL,
	timeSharingStart varchar(200) NOT NULL,
	timeSharingEnd varchar(200) NOT NULL
 CONSTRAINT PK_TvMovieMapping PRIMARY KEY  
(
	idMapping ASC
)
) 
GO
