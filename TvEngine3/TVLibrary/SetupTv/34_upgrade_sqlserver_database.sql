use TvLibrary
GO

---insert the upgrabe statements below ---
GO
alter table Channel add epgHasGaps bit
GO
update Channel set epgHasGaps=0
GO


---set the new schema version here---
GO
UPDATE Version SET versionNumber=34
GO
