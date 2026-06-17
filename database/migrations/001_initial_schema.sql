-- CadastroIgreja initial PostgreSQL schema
-- Creates core entities for churches, users, requests, approvals, letters, and audit logs.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS trigger AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TABLE churches (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name varchar(160) NOT NULL,
    type varchar(32) NOT NULL CHECK (type IN ('Sede', 'Regional', 'Setorial', 'CongregacaoLocal', 'CasaOracao')),
    parent_id uuid NULL REFERENCES churches(id) ON DELETE RESTRICT,
    active boolean NOT NULL DEFAULT true,
    address text NULL,
    city varchar(120) NULL,
    state varchar(2) NULL,
    phone varchar(32) NULL,
    cnpj varchar(32) NULL,
    institutional_info text NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_church_parent_by_type CHECK (
        (type = 'Sede' AND parent_id IS NULL) OR
        (type <> 'Sede' AND parent_id IS NOT NULL)
    )
);

CREATE OR REPLACE FUNCTION validate_church_hierarchy()
RETURNS trigger AS $$
DECLARE
    expected_parent_type varchar(32);
    actual_parent_type varchar(32);
    visited uuid[] := ARRAY[NEW.id];
    cursor_parent uuid := NEW.parent_id;
BEGIN
    expected_parent_type := CASE NEW.type
        WHEN 'Sede' THEN NULL
        WHEN 'Regional' THEN 'Sede'
        WHEN 'Setorial' THEN 'Regional'
        WHEN 'CongregacaoLocal' THEN 'Setorial'
        WHEN 'CasaOracao' THEN 'CongregacaoLocal'
    END;

    IF expected_parent_type IS NULL THEN
        IF NEW.parent_id IS NOT NULL THEN
            RAISE EXCEPTION 'Sede cannot have a parent church';
        END IF;
        RETURN NEW;
    END IF;

    SELECT type INTO actual_parent_type FROM churches WHERE id = NEW.parent_id;

    IF actual_parent_type IS DISTINCT FROM expected_parent_type THEN
        RAISE EXCEPTION 'Church type % must have parent type %, got %', NEW.type, expected_parent_type, actual_parent_type;
    END IF;

    WHILE cursor_parent IS NOT NULL LOOP
        IF cursor_parent = ANY(visited) THEN
            RAISE EXCEPTION 'Church hierarchy cycle detected for church %', NEW.id;
        END IF;

        visited := array_append(visited, cursor_parent);
        SELECT parent_id INTO cursor_parent FROM churches WHERE id = cursor_parent;
    END LOOP;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_churches_validate_hierarchy
BEFORE INSERT OR UPDATE OF type, parent_id ON churches
FOR EACH ROW EXECUTE FUNCTION validate_church_hierarchy();

CREATE TRIGGER trg_churches_updated_at
BEFORE UPDATE ON churches
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

INSERT INTO churches (id, name, type)
VALUES ('11111111-1111-1111-1111-111111111111', 'Sede', 'Sede')
ON CONFLICT (id) DO NOTHING;


CREATE TABLE roles (
    id smallserial PRIMARY KEY,
    name varchar(32) NOT NULL UNIQUE CHECK (name IN ('Membro', 'Diacono', 'Presbitero', 'Pastor', 'Dirigente')),
    created_at timestamptz NOT NULL DEFAULT now()
);

INSERT INTO roles (name) VALUES
    ('Membro'),
    ('Diacono'),
    ('Presbitero'),
    ('Pastor'),
    ('Dirigente')
ON CONFLICT (name) DO NOTHING;

CREATE TABLE users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name varchar(180) NOT NULL,
    email varchar(254) NOT NULL UNIQUE,
    phone varchar(32) NULL,
    password_hash text NOT NULL,
    church_id uuid NOT NULL REFERENCES churches(id) ON DELETE RESTRICT,
    role_id smallint NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    status varchar(24) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'Approved', 'Suspended', 'Rejected')),
    church_joined_at date NULL,
    is_system_admin boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TRIGGER trg_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE refresh_tokens (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash text NOT NULL UNIQUE,
    expires_at timestamptz NOT NULL,
    revoked_at timestamptz NULL,
    replaced_by_token_id uuid NULL REFERENCES refresh_tokens(id) ON DELETE SET NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_refresh_tokens_revoked_before_expiry CHECK (revoked_at IS NULL OR revoked_at <= expires_at)
);

CREATE TABLE password_reset_tokens (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash text NOT NULL UNIQUE,
    expires_at timestamptz NOT NULL,
    consumed_at timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE role_change_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    requested_role_id smallint NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    status varchar(24) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'Approved', 'Rejected')),
    justification text NULL,
    decided_by uuid NULL REFERENCES users(id) ON DELETE SET NULL,
    decided_at timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE preacher_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status varchar(32) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'Approved', 'Rejected')),
    current_step varchar(32) NOT NULL DEFAULT 'Setorial' CHECK (current_step IN ('CasaOracao', 'CongregacaoLocal', 'Setorial', 'Completed')),
    origin_church_id uuid NOT NULL REFERENCES churches(id) ON DELETE RESTRICT,
    destination_church_id uuid NULL REFERENCES churches(id) ON DELETE RESTRICT,
    notes text NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    decided_at timestamptz NULL,
    letter_id uuid NULL
);

CREATE TABLE approvals (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    request_type varchar(32) NOT NULL CHECK (request_type IN ('Member', 'RoleChange', 'Preacher')),
    request_id uuid NOT NULL,
    approver_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    church_id uuid NOT NULL REFERENCES churches(id) ON DELETE RESTRICT,
    decision varchar(16) NOT NULL CHECK (decision IN ('Approved', 'Rejected')),
    notes text NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE (request_type, request_id, approver_id)
);

CREATE TABLE storage_files (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_user_id uuid NULL REFERENCES users(id) ON DELETE SET NULL,
    entity_name varchar(80) NOT NULL,
    entity_id text NOT NULL,
    file_kind varchar(32) NOT NULL CHECK (file_kind IN ('ProfilePhoto', 'Document', 'PreachingLetterPdf')),
    storage_path text NOT NULL UNIQUE,
    content_type varchar(120) NOT NULL,
    size_bytes bigint NOT NULL CHECK (size_bytes > 0),
    checksum_sha256 char(64) NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE preaching_letters (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    preacher_request_id uuid NOT NULL UNIQUE REFERENCES preacher_requests(id) ON DELETE RESTRICT,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    number varchar(40) NOT NULL UNIQUE,
    issue_date date NOT NULL DEFAULT current_date,
    expiration_date date NOT NULL,
    church_id uuid NOT NULL REFERENCES churches(id) ON DELETE RESTRICT,
    destination_church_id uuid NULL REFERENCES churches(id) ON DELETE RESTRICT,
    status varchar(16) NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Suspended', 'Expired', 'Cancelled')),
    approved_at timestamptz NOT NULL DEFAULT now(),
    pdf_file_id uuid NULL REFERENCES storage_files(id) ON DELETE SET NULL,
    pdf_path text NOT NULL,
    qr_code_payload text NOT NULL,
    issued_by uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_preaching_letters_expiration_after_issue CHECK (expiration_date > issue_date)
);

CREATE TABLE leader_signatures (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    leader_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    storage_path text NOT NULL,
    mime_type varchar(120) NOT NULL,
    active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TRIGGER trg_leader_signatures_updated_at
BEFORE UPDATE ON leader_signatures
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TABLE audit_logs (
    id bigserial PRIMARY KEY,
    user_id uuid NULL REFERENCES users(id) ON DELETE SET NULL,
    action varchar(80) NOT NULL,
    entity_name varchar(80) NOT NULL,
    entity_id text NOT NULL,
    ip_address inet NULL,
    metadata jsonb NOT NULL DEFAULT '{}'::jsonb,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_churches_parent_id ON churches(parent_id);
CREATE INDEX ix_churches_type ON churches(type);
CREATE INDEX ix_users_church_id ON users(church_id);
CREATE INDEX ix_users_role_id ON users(role_id);
CREATE INDEX ix_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX ix_refresh_tokens_expires_at ON refresh_tokens(expires_at);
CREATE INDEX ix_password_reset_tokens_user_id ON password_reset_tokens(user_id);
CREATE INDEX ix_role_change_requests_user_id ON role_change_requests(user_id);
CREATE INDEX ix_preacher_requests_user_id ON preacher_requests(user_id);
CREATE INDEX ix_preacher_requests_origin_church_id ON preacher_requests(origin_church_id);
CREATE INDEX ix_approvals_request ON approvals(request_type, request_id);
CREATE INDEX ix_storage_files_entity ON storage_files(entity_name, entity_id);
CREATE INDEX ix_leader_signatures_leader_id ON leader_signatures(leader_id);
CREATE INDEX ix_audit_logs_entity ON audit_logs(entity_name, entity_id);
CREATE INDEX ix_audit_logs_created_at ON audit_logs(created_at);
