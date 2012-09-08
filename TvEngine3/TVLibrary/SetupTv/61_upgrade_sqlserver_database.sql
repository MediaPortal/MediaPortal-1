USE %TvLibrary%
GO

DROP INDEX [IX_TuningDetail_idChannel] ON [dbo].[TuningDetail] WITH ( ONLINE = OFF )
GO

CREATE TABLE LnbType
(
	idLnbType INT IDENTITY(1, 1) NOT NULL,
  name VARCHAR(255) NOT NULL,
  lowBandFrequency INT NOT NULL,
  highBandFrequency INT NOT NULL,
  switchFrequency INT NOT NULL,
  isBandStacked BIT DEFAULT 0,
  isToroidal BIT DEFAULT 0,
  CONSTRAINT PK_LnbType PRIMARY KEY
  (
	  idLnbType ASC
  )
)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('Universal', 9750000, 10600000, 11700000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('C-Band', 5150000, 5650000, 18000000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('10750 MHz', 10750000, 11250000, 18000000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('10750 MHz [22 kHz on]', 10250000, 10750000, 11250000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('11250 MHz (NA Legacy)', 11250000, 11750000, 18000000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('11250 MHz (NA Legacy) [22 kHz on]', 10750000, 11250000, 11750000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('11300 MHz', 11300000, 11800000, 18000000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('11300 MHz [22 kHz on]', 10800000, 11300000, 11800000, 0, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('DishPro Band Stacked FSS', 10750000, 13850000, 18000000, 1, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('DishPro Band Stacked DBS', 11250000, 14350000, 18000000, 1, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('NA Band Stacked FSS', 10750000, 10175000, 18000000, 1, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('NA Band Stacked DBS', 11250000, 10675000, 18000000, 1, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('Sadoun Band Stacked', 10100000, 10750000, 18000000, 1, 0)
GO

INSERT INTO LnbType (name, lowBandFrequency, highBandFrequency, switchFrequency, isBandStacked, isToroidal)
  VALUES ('C-Band Band Stacked', 5150000, 5750000, 18000000, 1, 0)
GO

EXEC SP_RENAME 'TuningDetail.band', 'idLnbType', 'COLUMN'
GO

ALTER TABLE TuningDetail DROP COLUMN switchingFrequency
GO

ALTER TABLE TuningDetail DROP COLUMN switchingFrequency
GO

CREATE NONCLUSTERED INDEX [IX_TuningDetail_idChannel] ON [dbo].[TuningDetail] 
(
	[idChannel] ASC
)
INCLUDE (
  [idTuning],
  [name],
  [provider],
  [channelType],
  [channelNumber],
  [frequency],
  [countryId],
  [isRadio],
  [isTv],
  [networkId],
  [transportId],
  [serviceId],
  [pmtPid],
  [freeToAir],
  [modulation],
  [polarisation],
  [symbolrate],
  [diseqc],
  [bandwidth],
  [majorChannel],
  [minorChannel],
  [videoSource],
  [tuningSource],
  [idLnbType],
  [satIndex],
  [innerFecRate],
  [pilot],
  [rollOff],
  [url],
  [bitrate],
  [audioSource],
  [isVCRSignal]
) WITH (STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

UPDATE TuningDetail SET idLnbType = 14 WHERE idLnbType = 7
GO
UPDATE TuningDetail SET idLnbType = 12 WHERE idLnbType = 5
GO
UPDATE TuningDetail SET idLnbType = 11 WHERE idLnbType = 6
GO
UPDATE TuningDetail SET idLnbType = 5 WHERE idLnbType = 8
GO
UPDATE TuningDetail SET idLnbType = 5 WHERE idLnbType = 9
GO
UPDATE TuningDetail SET idLnbType = 6 WHERE idLnbType = 10
GO
UPDATE TuningDetail SET idLnbType = 9 WHERE idLnbType = 4
GO
UPDATE TuningDetail SET idLnbType = 10 WHERE idLnbType = 3
GO
UPDATE TuningDetail SET idLnbType = 3 WHERE idLnbType = 1
GO
UPDATE TuningDetail SET idLnbType = 1 WHERE idLnbType = 0 AND channelType = 3
GO

UPDATE Version SET versionNumber = 61
GO
