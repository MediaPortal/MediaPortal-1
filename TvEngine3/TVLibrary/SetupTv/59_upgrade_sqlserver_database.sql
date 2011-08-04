use %TvLibrary%
GO

alter table Card drop constraint DF_Card_stopgraph
GO


ALTER TABLE Card
  ADD CONSTRAINT DF_Card_stopgraph
  DEFAULT (1) FOR stopgraph
GO

UPDATE Version SET versionNumber=59
GO
