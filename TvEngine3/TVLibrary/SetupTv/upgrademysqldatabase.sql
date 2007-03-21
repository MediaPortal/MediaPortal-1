USE TvLibrary;
#
ALTER TABLE `recording` ADD COLUMN `stopTime` INTEGER NOT NULL DEFAULT 0;
#
UPDATE `version` SET `versionNumber`=24;
#
