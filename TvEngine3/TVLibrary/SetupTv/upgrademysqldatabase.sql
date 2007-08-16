USE TvLibrary;
# --- version 30 ---
ALTER TABLE Channel ADD displayName varchar(200)
#
UPDATE Channel SET DisplayName=(SELECT name FROM Channel p WHERE Channel.idChannel=p.idChannel)
#
UPDATE `version` SET `versionNumber`=30;
#
