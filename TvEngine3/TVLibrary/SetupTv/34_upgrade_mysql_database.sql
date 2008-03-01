USE TvLibrary;
#

/*Insert the upgrade statements below */
alter table Channel add `epgHasGaps` bit;
update Channel set `epgHasGaps`=0;


/* Set the new schema version here */
UPDATE `Version` SET `versionNumber`=34;
