use TvLibrary
GO

---insert the upgrade statements below ---
GO
DELETE FROM Program
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE name = '_dta_index_Program_7_645577338__K3_1_2_4_5_6_7_8') 
ALTER TABLE Program DROP CONSTRAINT _dta_index_Program_7_645577338__K3_1_2_4_5_6_7_8
GO

ALTER TABLE Program ALTER COLUMN title varchar(2000) NOT NULL
GO
ALTER TABLE Program ALTER COLUMN description varchar(8000) NOT NULL
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Program_EPG_Lookup')
CREATE UNIQUE INDEX IX_Program_EPG_Lookup ON Program
	(
	idChannel,
	startTime,
	endTime
	)
GO

ALTER TABLE Recording ALTER COLUMN title varchar(2000) NOT NULL 
GO
ALTER TABLE Recording ALTER COLUMN description varchar(8000) NOT NULL
GO

---set the new schema version here---
GO
UPDATE Version SET versionNumber=36
GO
