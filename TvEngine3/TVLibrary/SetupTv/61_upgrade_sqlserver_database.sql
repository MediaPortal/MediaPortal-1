USE %TvLibrary%;
GO
ALTER TABLE Channel ADD channelNumber INT NOT NULL CONSTRAINT [DF_Channel_ChannelNumber] DEFAULT 10000
GO
UPDATE Channel SET channelNumber = (SELECT TOP 1 t.channelNumber FROM TuningDetail AS t WHERE t.idChannel = channel.idChannel ORDER BY t.idTuning)
GO
UPDATE Version SET versionNumber=61
GO
