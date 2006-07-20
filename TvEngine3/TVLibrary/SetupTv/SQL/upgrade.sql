declare @version int 

set @version=0 
if exists (select * from sysobjects where id = object_id(N'Version') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
begin 
select @version= versionNumber from version 
end 
print @version  
if (@version = 0)  
begin  
CREATE TABLE Version (   idVersion int IDENTITY (1, 1) NOT NULL ,   versionNumber int NOT NULL )  
CREATE TABLE Setting (   idSetting int IDENTITY (1, 1) NOT NULL ,   tag varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,   value varchar (200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL)   
ALTER TABLE Card ADD recordingFolder varchar(256) NULL  
ALTER TABLE Card ADD languages varchar(256) NULL  
end   
delete from version  
insert into version(versionNumber) values(1) 

select * from version