USE %TvLibrary%;

ALTER TABLE "Card" ADD COLUMN "idleMode" INT(1) NOT NULL DEFAULT 1;
ALTER TABLE "Card" ADD COLUMN "multiChannelDecryptMode" INT(1) NOT NULL DEFAULT 0;
ALTER TABLE "Card" ADD COLUMN "alwaysSendDiseqcCommands" BIT(1) NOT NULL DEFAULT 0;
ALTER TABLE "Card" ADD COLUMN "diseqcCommandRepeatCount" INT(1) NOT NULL DEFAULT 0;
ALTER TABLE "Card" ADD COLUMN "pidFilterMode" INT(1) NOT NULL DEFAULT 2;
ALTER TABLE "Card" ADD COLUMN "useCustomTuning" BIT(1) NOT NULL DEFAULT 0;
ALTER TABLE "Card" ADD COLUMN "useConditionalAccess" BIT(1) NOT NULL DEFAULT 0;

UPDATE "Card" SET "idleMode" = "stopgraph";
UPDATE "Card" SET "useConditionalAccess" = "CAM";

ALTER TABLE "Card" DROP COLUMN "recordingFormat";
ALTER TABLE "Card" DROP COLUMN "stopgraph";
ALTER TABLE "Card" DROP COLUMN "CAM";

UPDATE "Version" SET "versionNumber" = 62;
