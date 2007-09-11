USE TvLibrary;
# --- version 31 ---
ALTER TABLE `Program` ADD `url` varchar(200), `bitrate` int(11)
#
UPDATE `TuningDetail` SET `url`='', `bitrate`=0
#
DELETE FROM `Version`
#
INSERT INTO `Version` (`versionNumber`) VALUES(32)
#
