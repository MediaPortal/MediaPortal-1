use %TvLibrary%
GO

CREATE TABLE SoftwareEncoder(
	idEncoder int IDENTITY(1,1) NOT NULL,
	priority int NOT NULL,
	name varchar(200) NOT NULL,
	type int NOT NULL,
 CONSTRAINT PK_SoftwareEncoder PRIMARY KEY
(
	idEncoder ASC
)
)

GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (1,'InterVideo Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (2,'Ulead MPEG Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (3,'MainConcept MPEG Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (4,'MainConcept Demo MPEG Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (5,'CyberLink MPEG Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (6,'CyberLink MPEG Video Encoder(Twinhan)',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (7,'MainConcept (Hauppauge) MPEG Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (8,'nanocosmos MPEG Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (9,'Pinnacle MPEG 2 Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (10,'MainConcept (HCW) MPEG-2 Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (11,'ATI MPEG Video Encoder',0)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (1,'InterVideo Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (2,'Ulead MPEG Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (3,'MainConcept MPEG Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (4,'MainConcept Demo MPEG Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (5,'CyberLink Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (6,'CyberLink Audio Encoder(Twinhan)',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (7,'Pinnacle MPEG Layer-2 Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (8,'MainConcept (Hauppauge) MPEG Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (9,'NVIDIA Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (10,'MainConcept (HCW) Layer II Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (11,'CyberLink MPEG Audio Encoder',1)
GO

INSERT INTO SoftwareEncoder (priority,name,type)
     VALUES (12,'ATI MPEG Audio Encoder',1)
GO

UPDATE Version SET versionNumber=53
GO
