USE %TvLibrary%;

CREATE TABLE "SoftwareEncoder" (
  "idEncoder" int(11) NOT NULL auto_increment,
  "priority" int(11) NOT NULL,
  "name" varchar(200) NOT NULL,
  "type" int(11) NOT NULL,
  PRIMARY KEY  ("idEncoder")
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

/*Data for the table "SoftwareEncoder" */

insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (1,1,'InterVideo Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (2,2,'Ulead MPEG Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (3,3,'MainConcept MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (4,4,'MainConcept Demo MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (5,5,'CyberLink MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (6,6,'CyberLink MPEG Video Encoder(Twinhan)',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (7,7,'MainConcept (Hauppauge) MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (8,8,'nanocosmos MPEG Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (9,9,'Pinnacle MPEG 2 Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (10,10,'MainConcept (HCW) MPEG-2 Video Encoder',0);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (11,11,'ATI MPEG Video Encoder',0);

insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (12,1,'InterVideo Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (13,2,'Ulead MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (14,3,'MainConcept MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (15,4,'MainConcept Demo MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (16,5,'CyberLink Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (17,6,'CyberLink Audio Encoder(Twinhan)',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (18,7,'Pinnacle MPEG Layer-2 Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (19,8,'MainConcept (Hauppauge) MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (20,9,'NVIDIA Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (21,10,'MainConcept (HCW) Layer II Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (22,11,'CyberLink MPEG Audio Encoder',1);
insert  into "SoftwareEncoder"("idEncoder","priority","name","type") values (23,12,'ATI MPEG Audio Encoder',1);

UPDATE Version SET versionNumber=53;
