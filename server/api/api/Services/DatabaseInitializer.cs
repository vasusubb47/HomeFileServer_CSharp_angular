using LinqToDB;
using LinqToDB.Data;
using api.Models;
using api.Services.DatabaseService;

namespace api.Services;

public static class DatabaseInitializer
{
    public static bool IsDatabaseInitialized(IDbService dbService)
    {
        if (dbService is not DataConnection db) return false;

        // This query returns 1 if the 'users' table exists, otherwise 0
        var tableExists = db.Execute<int>(@"
        SELECT COUNT(*) 
        FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'users';");

        return tableExists > 0;
    }
    
    public static void Initialize(IDbService dbService)
    {
        // Cast the interface to DataConnection to access Execute and CreateTable
        if (dbService is not DataConnection db)
        {
            throw new InvalidCastException("IDbService must be a LinqToDB DataConnection");
        }

        Console.WriteLine("🚀 Initializing PostgreSQL Schema...");

        // 0. Ensure UUID extension exists (Required for gen_random_uuid)
        db.Execute("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");

        // 1. Create the Custom Postgres Enum Type
        db.Execute(@"
            DO $$ BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'bucket_perm_type') THEN
                    CREATE TYPE bucket_perm_type AS ENUM ('admin', 'readonly', 'read_and_write');
                END IF;
            END $$;");

        // 2. Create Tables in dependency order
        CreateTableSafely<User>(db);
        CreateTableSafely<Bucket>(db);
        CreateTableSafely<BucketUsers>(db);
        CreateTableSafely<UserFile>(db);
        CreateTableSafely<FileMetadata>(db);

        // 3. Apply SQL Logic (Defaults, Triggers, Constraints)
        ApplyPostgresLogic(db);
        
        Console.WriteLine("✅ Database Schema is ready.");
    }

    private static void CreateTableSafely<T>(DataConnection db)
    {
        try 
        { 
            db.CreateTable<T>(); 
            Console.WriteLine($"   ✔ Table {typeof(T).Name} checked/created.");
        } 
        catch { /* Table already exists */ }
    }

    private static void ApplyPostgresLogic(DataConnection db)
    {
        // --- DEFAULTS for UUIDs ---
        db.Execute("ALTER TABLE users ALTER COLUMN user_id SET DEFAULT gen_random_uuid();");
        db.Execute("ALTER TABLE buckets ALTER COLUMN bucket_id SET DEFAULT gen_random_uuid();");
        
        // --- DEFAULTS for TIMESTAMPS ---
        var tablesWithTime = new[] { "users", "buckets", "bucket_users", "user_files", "file_metadata" };
        foreach (var table in tablesWithTime)
        {
            db.Execute($"ALTER TABLE {table} ALTER COLUMN created_at SET DEFAULT CURRENT_TIMESTAMP;");
        }

        // --- UPDATED_AT TRIGGER FUNCTION ---
        db.Execute(@"
            CREATE OR REPLACE FUNCTION update_timestamp() RETURNS TRIGGER AS $$
            BEGIN
                NEW.updated_at = now();
                RETURN NEW;
            END;
            $$ language 'plpgsql';");

        // --- ATTACH TRIGGERS ---
        foreach (var table in tablesWithTime)
        {
            db.Execute($@"
                DROP TRIGGER IF EXISTS trg_update_{table} ON {table};
                CREATE TRIGGER trg_update_{table}
                BEFORE UPDATE ON {table}
                FOR EACH ROW EXECUTE PROCEDURE update_timestamp();");
        }
        
        // --- FOREIGN KEY CONSTRAINTS (Try-Catch blocks for FKs in hobby projects prevent crashes on re-runs) ---
        string[] constraints =
        [
            "ALTER TABLE users ADD CONSTRAINT un_user_email UNIQUE (email)",
            "ALTER TABLE buckets ADD CONSTRAINT fk_buckets_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE",
            "ALTER TABLE bucket_users ADD CONSTRAINT fk_bu_bucket FOREIGN KEY (bucket_id) REFERENCES buckets(bucket_id) ON DELETE CASCADE",
            "ALTER TABLE bucket_users ADD CONSTRAINT fk_bu_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE",
            "ALTER TABLE user_files ADD CONSTRAINT fk_files_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE",
            "ALTER TABLE user_files ADD CONSTRAINT fk_files_bucket FOREIGN KEY (bucket_id) REFERENCES buckets(bucket_id) ON DELETE CASCADE",
            "ALTER TABLE file_metadata ADD CONSTRAINT fk_meta_file FOREIGN KEY (file_id) REFERENCES user_files(file_id) ON DELETE CASCADE"
        ];

        foreach (var sql in constraints)
        {
            try { db.Execute(sql); } catch { /* Constraint already exists */ }
        }
    }
}
