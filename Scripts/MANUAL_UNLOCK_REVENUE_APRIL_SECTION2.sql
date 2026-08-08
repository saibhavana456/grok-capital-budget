-- =============================================================================
-- Manual unlock example (if Admin Enable did not insert a row)
-- Revenue April for SECTION_ID = 2, FY 2026-27
-- Capital: set PROJECT_ID and SECTION_ID NULL instead.
-- =============================================================================

SET DEFINE OFF;

-- Revenue unlock (section-wise)
INSERT INTO ENTRY_MONTH_UNLOCK
  (PROJECT_ID, SECTION_ID, FINANCIAL_YEAR, ENTRY_MONTH, IS_ENABLED, ENABLED_BY, ENABLED_AT)
SELECT NULL, 2, '2026-27', 'April', 'Y', '100001', SYSTIMESTAMP
FROM DUAL
WHERE NOT EXISTS (
  SELECT 1 FROM ENTRY_MONTH_UNLOCK
  WHERE SECTION_ID = 2 AND FINANCIAL_YEAR = '2026-27' AND ENTRY_MONTH = 'April'
);

COMMIT;

SELECT * FROM ENTRY_MONTH_UNLOCK ORDER BY UNLOCK_ID;
