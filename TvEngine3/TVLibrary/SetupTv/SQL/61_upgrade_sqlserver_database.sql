USE %TvLibrary%;
GO
ALTER TABLE Channel ADD channelNumber INT NOT NULL CONSTRAINT [DF_Channel_ChannelNumber] DEFAULT 10000
GO
UPDATE Channel SET channelNumber = (SELECT TOP 1 t.channelNumber ORDER BY t.idTuning) FROM TuningDetail AS t INNER JOIN Channel AS c ON t.idChannel = c.idChannel
GO
UPDATE Version SET versionNumber=61
GO
