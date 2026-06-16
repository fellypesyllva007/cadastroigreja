-- CadastroIgreja initial PostgreSQL schema
-- Creates core entities for churches, users, requests, approvals, letters, and audit logs.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE churches (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name varchar(160) NOT NULL,
    type varchar(32) NOT NULL CHECK (type IN ('Sede', 'Regional', 'Setorial', 'CongregacaoLocal', 'CasaOracao')),
    parent_id uuid NULL REFERENCES churches(id) ON DELETE RESTRICT,
    active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_church_parent_by_type CHECK (
        (type = 'Sede' AND parent_id IS NULL) OR
        (type <> 'Sede' AND parent_id IS NOT NULL)
    )
);

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
    status varchar(24) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'Active', 'Suspended', 'Rejected')),
    is_system_admin boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE role_change_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    requested_role_id smallint NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    status varchar(24) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Canceled')),
    justification text NULL,
    decided_by uuid NULL REFERENCES users(id) ON DELETE SET NULL,
    decided_at timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE preacher_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    status varchar(32) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'HouseApproved', 'LocalApproved', 'SetorialApproved', 'Rejected', 'Canceled')),
    origin_church_id uuid NOT NULL REFERENCES churches(id) ON DELETE RESTRICT,
    requested_at timestamptz NOT NULL DEFAULT now(),
    finalized_at timestamptz NULL,
    valid_until date NULL,
    rejection_reason text NULL
);

CREATE TABLE approvals (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    request_type varchar(32) NOT NULL CHECK (request_type IN ('Member', 'RoleChange', 'Preacher')),
    request_id uuid NOT NULL,
    approver_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    church_id uuid NOT NULL REFERENCES churches(id) ON DELETE RESTRICT,
    decision varchar(16) NOT NULL CHECK (decision IN ('Approved', 'Rejected')),
    notes text NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE preaching_letters (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    preacher_request_id uuid NOT NULL UNIQUE REFERENCES preacher_requests(id) ON DELETE RESTRICT,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    number varchar(40) NOT NULL UNIQUE,
    issue_date date NOT NULL DEFAULT current_date,
    expiration_date date NOT NULL,
    status varchar(16) NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Suspended', 'Expired', 'Canceled')),
    pdf_path text NOT NULL,
    qr_code_payload text NOT NULL,
    issued_by uuid NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    created_at timestamptz NOT NULL DEFAULT now()
);

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
CREATE INDEX ix_role_change_requests_user_id ON role_change_requests(user_id);
CREATE INDEX ix_preacher_requests_user_id ON preacher_requests(user_id);
CREATE INDEX ix_preacher_requests_origin_church_id ON preacher_requests(origin_church_id);
CREATE INDEX ix_approvals_request ON approvals(request_type, request_id);
CREATE INDEX ix_audit_logs_entity ON audit_logs(entity_name, entity_id);
CREATE INDEX ix_audit_logs_created_at ON audit_logs(created_at);
