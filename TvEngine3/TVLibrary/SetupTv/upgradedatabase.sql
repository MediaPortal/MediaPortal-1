USE TvLibrary
GO
ALTER TABLE Program ADD originalAirDate datetime NOT NULL DEFAULT  0
GO
ALTER TABLE Program ADD seriesNum varchar(200) NOT NULL DEFAULT ''
GO
ALTER TABLE Program ADD episodeNum varchar(200) NOT NULL DEFAULT ''
GO
ALTER TABLE Program ADD starRating int NOT NULL DEFAULT 0
GO
ALTER TABLE Program ADD classification varchar(200) NOT NULL DEFAULT ''
GO
delete from version
GO
insert into version(versionNumber) values(25)
GO
