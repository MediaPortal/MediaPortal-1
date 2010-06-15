USE %TvLibrary%;

ALTER TABLE "Program"
 ADD COLUMN "episodeName" text NOT NULL,
 ADD COLUMN "episodePart" text NOT NULL;

UPDATE "Version" SET "versionNumber"=44;