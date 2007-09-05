USE TvLibrary;
# --- version 31 ---
ALTER TABLE Program ADD parentalRating int(11)
#
UPDATE Program SET parentalRating=0
#
DELETE FROM `version`
#
UPDATE `version` SET `versionNumber`=31;
#
