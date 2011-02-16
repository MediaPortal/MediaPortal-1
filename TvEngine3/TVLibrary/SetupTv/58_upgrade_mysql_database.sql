USE %TvLibrary%;

ALTER TABLE SoftwareEncoder ADD COLUMN reusable bit(1) NOT NULL DEFAULT 1;

INSERT INTO Setting (tag, value) VALUE ('softwareEncoderReuseLimit', '0');

UPDATE Version SET versionNumber = 58;