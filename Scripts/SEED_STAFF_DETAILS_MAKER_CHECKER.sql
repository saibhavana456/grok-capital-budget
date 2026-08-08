-- =============================================================================
-- SEED Oracle STAFF_DETAILS — Maker / Checker sample rows (local testing)
-- EMPLID + EMP_SCALE_* required for login + Admin assign scale checks.
-- Maker scale 1–4 | Checker scale 4+
-- Run AFTER CREATE_ORACLE_STAFF_DETAILS.sql. SET DEFINE OFF.
-- =============================================================================
SET DEFINE OFF;

MERGE INTO STAFF_DETAILS t
USING (
  SELECT '600110' EMPLID, 'SAMPLE Maker' NAME, 'hyderabad' LOCATION, 'RO' DESCR,
         '1' DEPTID, 'DIT' DESCR1, '100000' REGION_CODE, 'hyderabad 1' REGION_NAME,
         '120000' DIVISION_CODE, 'hyderabad bn' DIVISION_NAME,
         'MKR' EMP_DESGN, 'Maker' EMP_DESGN_DESC,
         '3' EMP_SCALE_CODE, 'SCALE 3 OFFICER' EMP_SCALE_DESCR,
         '9999999999' PHONE, 'maker110@example.com' EMAIL FROM DUAL
  UNION ALL
  SELECT '600221', 'SAMPLE Checker', 'hyderabad', 'RO',
         '1', 'DIT', '100000', 'hyderabad 1',
         '120000', 'hyderabad bn',
         'CHK', 'Checker',
         '5', 'SCALE 5 OFFICER',
         '9999999998', 'checker221@example.com' FROM DUAL
  UNION ALL
  SELECT '600111', 'Sai Bhargav', 'hyderabad', 'RO',
         '21', 'DIGIT', '100000', 'hyderabad 1',
         '120000', 'hyderabad bn',
         'MKR', 'Manager',
         '4', 'SCALE 4 OFFICER',
         '9999999997', 'maker111@example.com' FROM DUAL
  UNION ALL
  SELECT '600222', 'Sai', 'hyderabad', 'RO',
         '21', 'DIGIT', '100000', 'hyderabad 1',
         '120000', 'hyderabad bn',
         'CHK', 'CM',
         '6', 'SCALE 6 OFFICER',
         '9999999996', 'checker222@example.com' FROM DUAL
  UNION ALL
  SELECT '600310', 'SAMPLE Maker DIGIT', 'hyderabad', 'RO',
         '21', 'DIGIT', '100000', 'hyderabad 1',
         '120000', 'hyderabad bn',
         'MKR', 'Maker',
         '2', 'SCALE 2 OFFICER',
         '9999999995', 'maker310@example.com' FROM DUAL
  UNION ALL
  SELECT '600321', 'SAMPLE Checker DIGIT', 'hyderabad', 'RO',
         '21', 'DIGIT', '100000', 'hyderabad 1',
         '120000', 'hyderabad bn',
         'CHK', 'Checker',
         '5', 'SCALE 5 OFFICER',
         '9999999994', 'checker321@example.com' FROM DUAL
) s
ON (t.EMPLID = s.EMPLID)
WHEN MATCHED THEN UPDATE SET
  t.NAME = s.NAME,
  t.LOCATION = s.LOCATION,
  t.DESCR = s.DESCR,
  t.DEPTID = s.DEPTID,
  t.DESCR1 = s.DESCR1,
  t.REGION_CODE = s.REGION_CODE,
  t.REGION_NAME = s.REGION_NAME,
  t.DIVISION_CODE = s.DIVISION_CODE,
  t.DIVISION_NAME = s.DIVISION_NAME,
  t.EMP_DESGN = s.EMP_DESGN,
  t.EMP_DESGN_DESC = s.EMP_DESGN_DESC,
  t.EMP_SCALE_CODE = s.EMP_SCALE_CODE,
  t.EMP_SCALE_DESCR = s.EMP_SCALE_DESCR,
  t.PHONE = s.PHONE,
  t.EMAIL = s.EMAIL
WHEN NOT MATCHED THEN INSERT (
  EMPLID, NAME, LOCATION, DESCR, DEPTID, DESCR1,
  REGION_CODE, REGION_NAME, DIVISION_CODE, DIVISION_NAME,
  EMP_DESGN, EMP_DESGN_DESC, EMP_SCALE_CODE, EMP_SCALE_DESCR, PHONE, EMAIL
) VALUES (
  s.EMPLID, s.NAME, s.LOCATION, s.DESCR, s.DEPTID, s.DESCR1,
  s.REGION_CODE, s.REGION_NAME, s.DIVISION_CODE, s.DIVISION_NAME,
  s.EMP_DESGN, s.EMP_DESGN_DESC, s.EMP_SCALE_CODE, s.EMP_SCALE_DESCR, s.PHONE, s.EMAIL
);

COMMIT;

SELECT EMPLID, NAME, EMP_SCALE_CODE, EMP_SCALE_DESCR, EMP_DESGN_DESC
FROM STAFF_DETAILS
WHERE EMPLID IN ('600110','600221','600111','600222','600310','600321')
ORDER BY EMPLID;
