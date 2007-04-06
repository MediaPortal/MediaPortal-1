USE TvLibrary;
#
ALTER TABLE `Program` ADD `originalAirDate` datetime NOT NULL DEFAULT  0;
#
ALTER TABLE `Program` ADD `seriesNum` varchar(200) NOT NULL DEFAULT '';
#
ALTER TABLE `Program` ADD `episodeNum` varchar(200) NOT NULL DEFAULT '';
#
ALTER TABLE `Program` ADD `starRating` int NOT NULL DEFAULT 0;
#
ALTER TABLE `Program` ADD `classification` varchar(200) NOT NULL DEFAULT '';
#
UPDATE `version` SET `versionNumber`=25;
#
