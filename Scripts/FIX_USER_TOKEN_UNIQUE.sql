-- Fix single-session: one USER_TOKEN row per PF (USERID).
-- Run once on existing Oracle schemas that still have a non-unique index.
-- Safe to re-run after cleaning duplicates.

-- Remove duplicate / orphan rows (keep newest REF_NO per USERID)
DELETE FROM USER_TOKEN a
 WHERE a.REF_NO NOT IN (
     SELECT MAX(b.REF_NO) FROM USER_TOKEN b GROUP BY b.USERID
 );

COMMIT;

BEGIN
  EXECUTE IMMEDIATE 'DROP INDEX IX_USER_TOKEN_USERID';
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE != -1418 THEN RAISE; END IF; -- index does not exist
END;
/

BEGIN
  EXECUTE IMMEDIATE 'ALTER TABLE USER_TOKEN ADD CONSTRAINT UK_USER_TOKEN_USERID UNIQUE (USERID)';
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE != -2260 THEN RAISE; END IF; -- constraint already exists
END;
/

COMMIT;
