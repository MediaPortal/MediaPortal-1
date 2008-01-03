USE TvLibrary;
#

/*Insert the upgrade statements below */
alter table channel add `epgHasGaps` bit;
update channel set `epgHasGaps`=0;


/* Set the new schema version here */
UPDATE `Version` SET `versionNumber`=34;
