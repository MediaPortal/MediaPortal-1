USE %TvLibrary%;

ALTER TABLE "Server" 
  ADD COLUMN "rtspPort" INT NOT NULL DEFAULT 554; 
  
UPDATE "Version" SET "versionNumber"=48;
