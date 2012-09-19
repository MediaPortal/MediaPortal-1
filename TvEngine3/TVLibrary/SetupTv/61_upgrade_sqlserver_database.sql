<<<<<<< HEAD
USE %TvLibrary%;
GO
ALTER TABLE Channel ADD channelNumber INT NOT NULL CONSTRAINT [DF_Channel_ChannelNumber] DEFAULT 10000
GO
UPDATE Channel SET channelNumber = (SELECT TOP 1 t.channelNumber FROM TuningDetail AS t WHERE t.idChannel = channel.idChannel ORDER BY t.idTuning)
GO
UPDATE Version SET versionNumber=61
GO
=======
use %TvLibrary%
GO
 
ALTER TABLE program
  ADD seriesId varchar(200)
GO

ALTER TABLE program
  ADD seriesTermination TINYINT
GO

ALTER TABLE schedule
  ADD seriesId varchar(200)
GO

UPDATE Version SET versionNumber=61
<<<<<<< HEAD
GO#
>>>>>>> 7716a4f... Updated .sql files due to no CRLF being entered in notepad.
=======
GO
>>>>>>> 0e809a4... Fixed small error in MSSQL update which prevented Database update.
