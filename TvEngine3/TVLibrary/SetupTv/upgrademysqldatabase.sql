USE TvLibrary;
#
ALTER TABLE `channelgroup` ADD COLUMN `sortOrder` INTEGER NOT NULL DEFAULT 0 AFTER `groupName`;
#
CREATE INDEX IDX_SortOrder ON ChannelGroup (sortOrder);
#
UPDATE `version` SET `versionNumber`=23;
#
