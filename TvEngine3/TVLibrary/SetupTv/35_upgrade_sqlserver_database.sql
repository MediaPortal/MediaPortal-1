use TvLibrary
GO

---insert the upgrabe statements below ---
GO
alter table ChannelMap add epgOnly bit
GO
update ChannelMap set epgOnly=0
GO


---set the new schema version here---
GO
UPDATE Version SET versionNumber=35
GO
