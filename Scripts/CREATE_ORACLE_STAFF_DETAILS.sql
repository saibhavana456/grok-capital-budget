-- =============================================================================
-- CREATE / ALIGN Oracle STAFF_DETAILS (local testing mirror)
-- Matches Organisations dbo.StaffDetails columns (EMPLID) — Personal/SCV script.sql
-- Run as app Oracle user (e.g. IT_BUDGET_MONITORING_PORTAL). SET DEFINE OFF.
-- Does NOT touch Organisations DB (read-only there).
-- =============================================================================
SET DEFINE OFF;

-- Create table if missing (same core columns as Organisations StaffDetails)
BEGIN
  EXECUTE IMMEDIATE q'[
    CREATE TABLE STAFF_DETAILS (
      EMPLID            VARCHAR2(50) NOT NULL,
      NAME              VARCHAR2(250),
      LOCATION          VARCHAR2(250),
      DESCR             VARCHAR2(250),
      DEPTID            VARCHAR2(250),
      DESCR1            VARCHAR2(250),
      REGION_CODE       VARCHAR2(250),
      REGION_NAME       VARCHAR2(250),
      DIVISION_CODE     VARCHAR2(250),
      DIVISION_NAME     VARCHAR2(250),
      EMP_DESGN         VARCHAR2(250),
      EMP_DESGN_DESC    VARCHAR2(250),
      EMP_SCALE_CODE    VARCHAR2(250),
      EMP_SCALE_DESCR   VARCHAR2(250),
      PHONE             VARCHAR2(250),
      EMAIL             VARCHAR2(250),
      CONSTRAINT PK_STAFF_DETAILS PRIMARY KEY (EMPLID)
    )
  ]';
EXCEPTION
  WHEN OTHERS THEN
    IF SQLCODE = -955 THEN NULL; -- name already used
    ELSE RAISE;
    END IF;
END;
/

-- Add columns if table already exists without them (safe re-run)
DECLARE
  PROCEDURE add_col(p_name VARCHAR2, p_ddl VARCHAR2) IS
  BEGIN
    EXECUTE IMMEDIATE p_ddl;
  EXCEPTION
    WHEN OTHERS THEN
      IF SQLCODE = -1430 THEN NULL; -- column already exists
      ELSE RAISE;
      END IF;
  END;
BEGIN
  add_col('NAME',            'ALTER TABLE STAFF_DETAILS ADD NAME VARCHAR2(250)');
  add_col('LOCATION',        'ALTER TABLE STAFF_DETAILS ADD LOCATION VARCHAR2(250)');
  add_col('DESCR',           'ALTER TABLE STAFF_DETAILS ADD DESCR VARCHAR2(250)');
  add_col('DEPTID',          'ALTER TABLE STAFF_DETAILS ADD DEPTID VARCHAR2(250)');
  add_col('DESCR1',          'ALTER TABLE STAFF_DETAILS ADD DESCR1 VARCHAR2(250)');
  add_col('REGION_CODE',     'ALTER TABLE STAFF_DETAILS ADD REGION_CODE VARCHAR2(250)');
  add_col('REGION_NAME',     'ALTER TABLE STAFF_DETAILS ADD REGION_NAME VARCHAR2(250)');
  add_col('DIVISION_CODE',   'ALTER TABLE STAFF_DETAILS ADD DIVISION_CODE VARCHAR2(250)');
  add_col('DIVISION_NAME',   'ALTER TABLE STAFF_DETAILS ADD DIVISION_NAME VARCHAR2(250)');
  add_col('EMP_DESGN',       'ALTER TABLE STAFF_DETAILS ADD EMP_DESGN VARCHAR2(250)');
  add_col('EMP_DESGN_DESC',  'ALTER TABLE STAFF_DETAILS ADD EMP_DESGN_DESC VARCHAR2(250)');
  add_col('EMP_SCALE_CODE',  'ALTER TABLE STAFF_DETAILS ADD EMP_SCALE_CODE VARCHAR2(250)');
  add_col('EMP_SCALE_DESCR', 'ALTER TABLE STAFF_DETAILS ADD EMP_SCALE_DESCR VARCHAR2(250)');
  add_col('PHONE',           'ALTER TABLE STAFF_DETAILS ADD PHONE VARCHAR2(250)');
  add_col('EMAIL',           'ALTER TABLE STAFF_DETAILS ADD EMAIL VARCHAR2(250)');
END;
/

COMMIT;

-- Verify
SELECT COLUMN_NAME FROM USER_TAB_COLUMNS
WHERE TABLE_NAME = 'STAFF_DETAILS'
ORDER BY COLUMN_ID;
