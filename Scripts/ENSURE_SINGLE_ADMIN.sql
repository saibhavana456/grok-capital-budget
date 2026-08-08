-- =============================================================================
-- ENSURE single Admin login path (SCV style)
-- Admin console uses ApiKey:ADMIN_USER_ID + ApiKey:PWD (AdminApp / Ubi#8790) — not APP_USER.
-- Deactivate any APP_USER rows with ROLE_CODE = ADMIN so only config admin can open Admin.
-- Maker/Checker APP_USER rows stay active.
-- =============================================================================
SET DEFINE OFF;

UPDATE APP_USER
SET IS_ACTIVE = 'N',
    UPDATED_AT = SYSTIMESTAMP,
    UPDATED_BY = 'ENSURE_SINGLE_ADMIN'
WHERE UPPER(ROLE_CODE) = 'ADMIN'
  AND IS_ACTIVE = 'Y';

COMMIT;

-- Expect 0 active ADMIN rows
SELECT USER_ID, PF_NO, USER_NAME, ROLE_CODE, IS_ACTIVE
FROM APP_USER
WHERE UPPER(ROLE_CODE) = 'ADMIN';

-- Active Maker/Checker should remain
SELECT PF_NO, USER_NAME, ROLE_CODE, IS_ACTIVE
FROM APP_USER
WHERE IS_ACTIVE = 'Y'
ORDER BY ROLE_CODE, PF_NO;
