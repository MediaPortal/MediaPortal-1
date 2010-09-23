USE %TvLibrary%;

ALTER TABLE "Card"
 ADD COLUMN "stopgraph" bit(1) NOT NULL;

UPDATE "Version" SET "versionNumber"=54;
