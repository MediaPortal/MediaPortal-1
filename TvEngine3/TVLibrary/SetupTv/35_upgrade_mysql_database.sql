USE TvLibrary;
#

/*Insert the upgrade statements below */
alter table ChannelMap add `epgOnly` bit;
update ChannelMap set `epgOnly`=0;


/* Set the new schema version here */
UPDATE `Version` SET `versionNumber`=35;
