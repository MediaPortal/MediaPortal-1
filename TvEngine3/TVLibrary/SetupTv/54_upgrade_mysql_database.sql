USE %TvLibrary%;

ALTER TABLE "Card"
 ADD COLUMN "stopgraph" bit(1) NOT NULL DEFAULT 1;

UPDATE "Version" SET "versionNumber"=54;
