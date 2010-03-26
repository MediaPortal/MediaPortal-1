USE %TvLibrary%;

DELETE FROM "ChannelLinkageMap";
ALTER TABLE "ChannelLinkageMap"
 ADD COLUMN "displayName" VARCHAR(200) NOT NULL;

UPDATE Version SET versionNumber=52;
