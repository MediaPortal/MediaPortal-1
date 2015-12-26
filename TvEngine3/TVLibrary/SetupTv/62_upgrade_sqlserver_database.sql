USE %TvLibrary%;
GO
ALTER TABLE channelgroup ADD pinCode VARCHAR(5) NULL;
GO
UPDATE Version SET versionNumber=62
GO
