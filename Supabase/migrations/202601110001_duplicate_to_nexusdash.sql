
BEGIN;

-- 0) Ensure target schema exists
CREATE SCHEMA IF NOT EXISTS "SominnercoreDash";

-- 1) Duplicate tables + data
DO $$
DECLARE r RECORD;
BEGIN
  FOR r IN
    SELECT tablename
    FROM pg_tables
    WHERE schemaname = 'SominnercoreWebsite'
  LOOP
    -- Create table if not exists (structure + constraints + indexes + defaults)
    EXECUTE format(
      'CREATE TABLE IF NOT EXISTS "SominnercoreDash".%I (LIKE "SominnercoreWebsite".%I INCLUDING ALL);',
      r.tablename, r.tablename
    );

    -- Copy data only if target table is empty (prevents duplicate inserts on rerun)
    EXECUTE format(
      'INSERT INTO "SominnercoreDash".%I OVERRIDING SYSTEM VALUE
       SELECT * FROM "SominnercoreWebsite".%I
       WHERE NOT EXISTS (SELECT 1 FROM "SominnercoreDash".%I LIMIT 1);',
      r.tablename, r.tablename, r.tablename
    );
  END LOOP;
END $$;

-- 2) Sync identity sequences (so future inserts don't collide)
DO $$
DECLARE r RECORD;
DECLARE seq text;
BEGIN
  FOR r IN
    SELECT table_name, column_name
    FROM information_schema.columns
    WHERE table_schema = 'SominnercoreDash'
      AND is_identity = 'YES'
  LOOP
    -- Find the identity sequence name for this table/column
    EXECUTE format(
      'SELECT pg_get_serial_sequence(''"SominnercoreDash".%I'', ''%I'');',
      r.table_name, r.column_name
    ) INTO seq;

    IF seq IS NOT NULL THEN
      EXECUTE format(
        'SELECT setval(%L, COALESCE((SELECT MAX(%I) FROM "SominnercoreDash".%I), 0));',
        seq, r.column_name, r.table_name
      );
    END IF;
  END LOOP;
END $$;

-- 3) Enable RLS on all tables in SominnercoreDash
DO $$
DECLARE r RECORD;
BEGIN
  FOR r IN
    SELECT tablename
    FROM pg_tables
    WHERE schemaname = 'SominnercoreDash'
  LOOP
    EXECUTE format('ALTER TABLE "SominnercoreDash".%I ENABLE ROW LEVEL SECURITY;', r.tablename);
  END LOOP;
END $$;

-- 4) Copy policies (drop+create) from SominnercoreWebsite -> SominnercoreDash
DO $$
DECLARE p RECORD;
DECLARE role_list text;
DECLARE using_clause text;
DECLARE check_clause text;
BEGIN
  FOR p IN
    SELECT
      tablename,
      policyname,
      cmd,
      roles,
      qual,
      with_check
    FROM pg_policies
    WHERE schemaname = 'SominnercoreWebsite'
    ORDER BY tablename, policyname
  LOOP
    -- Drop existing policy on target (safe)
    EXECUTE format('DROP POLICY IF EXISTS %I ON "SominnercoreDash".%I;', p.policyname, p.tablename);

    -- Build optional TO clause
    IF p.roles IS NULL OR array_length(p.roles, 1) IS NULL THEN
      role_list := '';
    ELSE
      role_list := ' TO ' || array_to_string(p.roles, ', ');
    END IF;

    -- Build optional USING / WITH CHECK
    using_clause := CASE
      WHEN p.qual IS NULL THEN ''
      ELSE ' USING (' || p.qual || ')'
    END;

    check_clause := CASE
      WHEN p.with_check IS NULL THEN ''
      ELSE ' WITH CHECK (' || p.with_check || ')'
    END;

    -- Create policy on target table
    EXECUTE
      'CREATE POLICY ' || quote_ident(p.policyname) ||
      ' ON "SominnercoreDash".' || quote_ident(p.tablename) ||
      ' FOR ' || p.cmd ||
      role_list ||
      using_clause ||
      check_clause ||
      ';';
  END LOOP;
END $$;

COMMIT;

-- You still need to expose "SominnercoreDash" in Supabase Settings -> Data API -> Exposed schemas.


-- For verification:

---Tables exist

select table_name
from information_schema.tables
where table_schema = 'SominnercoreDash';

---Policies exist

select tablename, policyname
from pg_policies
where schemaname = 'SominnercoreDash';


---RLS enabled

select relname, relrowsecurity
from pg_class
join pg_namespace on pg_namespace.oid = pg_class.relnamespace
where nspname = 'SominnercoreDash';




