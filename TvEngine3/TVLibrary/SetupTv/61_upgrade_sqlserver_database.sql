USE %TvLibrary%;
GO

ALTER TABLE Channel ADD channelNumber INT NOT NULL CONSTRAINT [DF_Channel_ChannelNumber] DEFAULT 10000
UPDATE Channel c SET c.channelNumber = (SELECT TOP 1 t.channelNumber FROM TuningDetail t WHERE t.idChannel = c.idChannel ORDER BY t.idTuning)

UPDATE Version SET versionNumber=61
GO
