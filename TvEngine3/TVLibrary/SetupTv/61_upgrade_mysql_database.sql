USE %TvLibrary%;

CREATE TABLE "LnbType"
(
	"idLnbType" INT(11) NOT NULL AUTO_INCREMENT,
  "name" VARCHAR(255) NOT NULL,
  "lowBandFrequency" INT(11) NOT NULL,
  "highBandFrequency" INT(11) NOT NULL,
  "switchFrequency" INT(11) NOT NULL,
  "isBandStacked" BIT(1) DEFAULT 0,
  "isToroidal" BIT(1) DEFAULT 0,
  PRIMARY KEY ("idLnbType")
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('Universal', 9750000, 10600000, 11700000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('C-Band', 5150000, 5650000, 18000000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('10750 MHz', 10750000, 11250000, 18000000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('10750 MHz [22 kHz on]', 10250000, 10750000, 11250000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('11250 MHz (NA Legacy)', 11250000, 11750000, 18000000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('11250 MHz (NA Legacy) [22 kHz on]', 10750000, 11250000, 11750000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('11300 MHz', 11300000, 11800000, 18000000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('11300 MHz [22 kHz on]', 10800000, 11300000, 11800000, 0, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('DishPro Band Stacked FSS', 10750000, 13850000, 18000000, 1, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('DishPro Band Stacked DBS', 11250000, 14350000, 18000000, 1, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('NA Band Stacked FSS', 10750000, 10175000, 18000000, 1, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('NA Band Stacked DBS', 11250000, 10675000, 18000000, 1, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('Sadoun Band Stacked', 10100000, 10750000, 18000000, 1, 0);
INSERT INTO "LnbType" ("name", "lowBandFrequency", "highBandFrequency", "switchFrequency", "isBandStacked", "isToroidal") VALUES ('C-Band Band Stacked', 5150000, 5750000, 18000000, 1, 0);

ALTER TABLE "TuningDetail" CHANGE COLUMN "band" "idLnbType" INT(11) NOT NULL;
ALTER TABLE "TuningDetail" DROP COLUMN "switchingFrequency";

UPDATE "TuningDetail" SET "idLnbType" = 14 WHERE "idLnbType" = 7;
UPDATE "TuningDetail" SET "idLnbType" = 12 WHERE "idLnbType" = 5;
UPDATE "TuningDetail" SET "idLnbType" = 11 WHERE "idLnbType" = 6;
UPDATE "TuningDetail" SET "idLnbType" = 5 WHERE "idLnbType" = 8;
UPDATE "TuningDetail" SET "idLnbType" = 5 WHERE "idLnbType" = 9;
UPDATE "TuningDetail" SET "idLnbType" = 6 WHERE "idLnbType" = 10;
UPDATE "TuningDetail" SET "idLnbType" = 9 WHERE "idLnbType" = 4;
UPDATE "TuningDetail" SET "idLnbType" = 10 WHERE "idLnbType" = 3;
UPDATE "TuningDetail" SET "idLnbType" = 3 WHERE "idLnbType" = 1;
UPDATE "TuningDetail" SET "idLnbType" = 1 WHERE "idLnbType" = 0 AND "channelType" = 3;

UPDATE "Version" SET "versionNumber" = 61;