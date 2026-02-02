

--- Create the app-level users table

CREATE TABLE "SominnercoreDash".users (
  id uuid PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,

  email text,
  full_name text,
  avatar_url text,

  role text DEFAULT 'user',
  is_active boolean DEFAULT true,

  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);


-- Enable RLS on users

ALTER TABLE "SominnercoreDash".users ENABLE ROW LEVEL SECURITY;


-- Enable RLS on users

CREATE POLICY "Users can read own profile"
ON "SominnercoreDash".users
FOR SELECT
USING (auth.uid() = id);


CREATE POLICY "Users can update own profile"
ON "SominnercoreDash".users
FOR UPDATE
USING (auth.uid() = id)
WITH CHECK (auth.uid() = id);


CREATE POLICY "Service role full access"
ON "SominnercoreDash".users
FOR ALL
TO service_role
USING (true)
WITH CHECK (true);

-- Auto-create user row on signup

CREATE OR REPLACE FUNCTION "SominnercoreDash".handle_new_user()
RETURNS trigger
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
  INSERT INTO "SominnercoreDash".users (id, email)
  VALUES (NEW.id, NEW.email);
  RETURN NEW;
END;
$$;

CREATE TRIGGER on_auth_user_created
AFTER INSERT ON auth.users
FOR EACH ROW
EXECUTE FUNCTION "SominnercoreDash".handle_new_user();

