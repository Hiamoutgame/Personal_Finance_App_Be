-- ============================================================
-- FinJar — PostgreSQL Schema (Phase 1 MVP)
-- Convention: uuid PK, timestamptz, numeric(18,2), snake_case
-- ============================================================

-- ==================== MIGRATION 1 ==========================
-- Identity & Access

CREATE TABLE roles (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    code        VARCHAR(30)  NOT NULL UNIQUE,
    name        VARCHAR(50)  NOT NULL,
    description TEXT         NULL,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE accounts (
    id                       UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    role_id                  UUID         NOT NULL REFERENCES roles(id),
    username                 VARCHAR(50)  NOT NULL UNIQUE,
    email                    VARCHAR(255) NOT NULL UNIQUE,
    password_hash            TEXT         NOT NULL,
    full_name                VARCHAR(150) NOT NULL,
    phone                    VARCHAR(20)  NULL,
    avatar_url               TEXT         NULL,
    status                   VARCHAR(20)  NOT NULL DEFAULT 'Active'
                                          CHECK (status IN ('Active','Banned')),
    status_reason            TEXT         NULL,
    preferred_currency       CHAR(3)      NOT NULL DEFAULT 'VND',
    is_onboarding_completed  BOOLEAN      NOT NULL DEFAULT FALSE,
    last_login_at            TIMESTAMPTZ  NULL,
    created_at               TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at               TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_accounts_role_status    ON accounts(role_id, status);
CREATE INDEX ix_accounts_last_login_at  ON accounts(last_login_at);

CREATE TABLE audit_logs (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    actor_account_id UUID        NOT NULL REFERENCES accounts(id),
    action_type      VARCHAR(50) NOT NULL,
    entity_type      VARCHAR(50) NOT NULL,
    entity_id        UUID        NULL,
    description      TEXT        NOT NULL,
    metadata_json    JSONB       NULL,
    ip_address       INET        NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_audit_logs_actor ON audit_logs(actor_account_id, created_at DESC);

-- ==================== MIGRATION 2 ==========================
-- Onboarding & Financial Setup

CREATE TABLE onboarding_profiles (
    id                        UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                   UUID        NOT NULL UNIQUE REFERENCES accounts(id),
    monthly_income            NUMERIC(18,2) NULL,
    occupation_type           VARCHAR(50) NULL,
    financial_goal_types      TEXT[]      NULL,
    budget_method_preference  VARCHAR(30) NOT NULL DEFAULT 'Undecided',
    age_range                 VARCHAR(30) NULL,
    spending_challenges       TEXT[]      NULL,
    recommended_method        VARCHAR(30) NULL,
    completed_at              TIMESTAMPTZ NOT NULL,
    created_at                TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at                TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE jar_setups (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID        NOT NULL UNIQUE REFERENCES accounts(id),
    method_type VARCHAR(30) NOT NULL
                            CHECK (method_type IN ('SixJars','Rule503020','Custom','Undecided')),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE financial_accounts (
    id                      UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                 UUID          NOT NULL REFERENCES accounts(id),
    name                    VARCHAR(100)  NOT NULL,
    account_type            VARCHAR(20)   NOT NULL
                                          CHECK (account_type IN ('Cash','Bank','EWallet','Other')),
    connection_mode         VARCHAR(20)   NOT NULL
                                          CHECK (connection_mode IN ('Manual','LinkedApi')),
    provider_code           VARCHAR(50)   NULL,
    provider_name           VARCHAR(100)  NULL,
    external_account_id     VARCHAR(150)  NULL,
    external_account_ref    VARCHAR(150)  NULL,
    masked_account_number   VARCHAR(50)   NULL,
    account_holder_name     VARCHAR(150)  NULL,
    currency                CHAR(3)       NOT NULL DEFAULT 'VND',
    current_balance         NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (current_balance >= 0),
    available_balance       NUMERIC(18,2) NULL,
    balance_as_of           TIMESTAMPTZ   NULL,
    sync_status             VARCHAR(20)   NOT NULL DEFAULT 'NeverSynced',
    last_synced_at          TIMESTAMPTZ   NULL,
    last_sync_error         TEXT          NULL,
    access_token_ref        TEXT          NULL,
    token_expires_at        TIMESTAMPTZ   NULL,
    consent_expires_at      TIMESTAMPTZ   NULL,
    last_sync_cursor        TEXT          NULL,
    webhook_subscription_id VARCHAR(150)  NULL,
    is_default              BOOLEAN       NOT NULL DEFAULT FALSE,
    is_active               BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at              TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_financial_accounts_user_id         ON financial_accounts(user_id);
CREATE INDEX ix_financial_accounts_user_default     ON financial_accounts(user_id, is_default);
CREATE INDEX ix_financial_accounts_sync_status      ON financial_accounts(sync_status);

CREATE TABLE categories (
    id            UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name          VARCHAR(100) NOT NULL,
    icon          VARCHAR(50)  NULL,
    color         VARCHAR(20)  NULL,
    is_default    BOOLEAN      NOT NULL DEFAULT FALSE,
    owner_user_id UUID         NULL REFERENCES accounts(id),
    display_order INT          NOT NULL DEFAULT 0,
    is_active     BOOLEAN      NOT NULL DEFAULT TRUE,
    deleted_at    TIMESTAMPTZ  NULL,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE jars (
    id           UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id      UUID          NOT NULL REFERENCES accounts(id),
    jar_setup_id UUID          NULL REFERENCES jar_setups(id),
    name         VARCHAR(100)  NOT NULL,
    percentage   NUMERIC(5,2)  NULL,
    balance      NUMERIC(18,2) NOT NULL DEFAULT 0 CHECK (balance >= 0),
    currency     CHAR(3)       NOT NULL DEFAULT 'VND',
    color        VARCHAR(20)   NULL,
    icon         VARCHAR(50)   NULL,
    is_default   BOOLEAN       NOT NULL DEFAULT FALSE,
    status       VARCHAR(20)   NOT NULL DEFAULT 'Active'
                               CHECK (status IN ('Active','Paused','Archived')),
    created_at   TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_jars_user_id ON jars(user_id);

-- ==================== MIGRATION 3 ==========================
-- Transactions & Jar Operations

CREATE TABLE jar_allocations (
    id                          UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                     UUID          NOT NULL REFERENCES accounts(id),
    source_financial_account_id UUID          NULL REFERENCES financial_accounts(id),
    total_amount                NUMERIC(18,2) NOT NULL,
    note                        TEXT          NULL,
    created_at                  TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE TABLE jar_allocation_items (
    id                       UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    allocation_id            UUID          NOT NULL REFERENCES jar_allocations(id),
    jar_id                   UUID          NOT NULL REFERENCES jars(id),
    amount                   NUMERIC(18,2) NOT NULL,
    balance_after_allocation NUMERIC(18,2) NOT NULL
);

CREATE TABLE jar_transfers (
    id          UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID          NOT NULL REFERENCES accounts(id),
    from_jar_id UUID          NOT NULL REFERENCES jars(id),
    to_jar_id   UUID          NOT NULL REFERENCES jars(id),
    amount      NUMERIC(18,2) NOT NULL CHECK (amount > 0),
    note        TEXT          NULL,
    created_at  TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_jar_transfers_diff CHECK (from_jar_id <> to_jar_id)
);

CREATE TABLE import_jobs (
    id                    UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id               UUID         NOT NULL REFERENCES accounts(id),
    financial_account_id  UUID         NOT NULL REFERENCES financial_accounts(id),
    file_name             VARCHAR(255) NOT NULL,
    original_content_type VARCHAR(100) NULL,
    stored_file_path      TEXT         NOT NULL,
    bank_code             VARCHAR(50)  NULL,
    status                VARCHAR(30)  NOT NULL DEFAULT 'Pending',
    progress              INT          NOT NULL DEFAULT 0,
    estimated_rows        INT          NULL,
    parsed_count          INT          NOT NULL DEFAULT 0,
    failed_count          INT          NOT NULL DEFAULT 0,
    error_message         TEXT         NULL,
    uploaded_at           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at            TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE transactions (
    id                     UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                UUID          NOT NULL REFERENCES accounts(id),
    financial_account_id   UUID          NOT NULL REFERENCES financial_accounts(id),
    jar_id                 UUID          NULL REFERENCES jars(id),
    category_id            UUID          NULL REFERENCES categories(id),
    import_job_id          UUID          NULL REFERENCES import_jobs(id),
    type                   VARCHAR(20)   NOT NULL CHECK (type IN ('Income','Expense')),
    amount                 NUMERIC(18,2) NOT NULL CHECK (amount > 0),
    note                   TEXT          NULL,
    raw_description        TEXT          NULL,
    transaction_date       TIMESTAMPTZ   NOT NULL,
    posted_at              TIMESTAMPTZ   NULL,
    source_type            VARCHAR(20)   NOT NULL DEFAULT 'Manual',
    external_transaction_id VARCHAR(150) NULL,
    raw_payload_json       JSONB         NULL,
    is_deleted             BOOLEAN       NOT NULL DEFAULT FALSE,
    deleted_at             TIMESTAMPTZ   NULL,
    created_at             TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at             TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_transactions_user_date       ON transactions(user_id, transaction_date DESC);
CREATE INDEX ix_transactions_account_date    ON transactions(financial_account_id, transaction_date DESC);
CREATE INDEX ix_transactions_user_jar_date   ON transactions(user_id, jar_id, transaction_date DESC);
CREATE INDEX ix_transactions_user_cat_date   ON transactions(user_id, category_id, transaction_date DESC);

-- ==================== MIGRATION 4 ==========================
-- Limits, Goals, Reminders, Notifications

CREATE TABLE spending_limits (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id             UUID          NOT NULL REFERENCES accounts(id),
    jar_id              UUID          NULL REFERENCES jars(id),
    category_id         UUID          NULL REFERENCES categories(id),
    limit_amount        NUMERIC(18,2) NOT NULL,
    period              VARCHAR(20)   NOT NULL CHECK (period IN ('Daily','Monthly')),
    alert_at_percentage NUMERIC(5,2)  NOT NULL,
    is_active           BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE TABLE goals (
    id            UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id       UUID          NOT NULL REFERENCES accounts(id),
    title         VARCHAR(150)  NOT NULL,
    target_amount NUMERIC(18,2) NOT NULL,
    saved_amount  NUMERIC(18,2) NOT NULL DEFAULT 0,
    due_date      DATE          NOT NULL,
    status        VARCHAR(20)   NOT NULL DEFAULT 'Active'
                                CHECK (status IN ('Active','Completed','Cancelled')),
    linked_jar_id UUID          NULL REFERENCES jars(id),
    note          TEXT          NULL,
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE TABLE goal_contributions (
    id                          UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    goal_id                     UUID          NOT NULL REFERENCES goals(id),
    user_id                     UUID          NOT NULL REFERENCES accounts(id),
    source_jar_id               UUID          NULL REFERENCES jars(id),
    source_financial_account_id UUID          NULL REFERENCES financial_accounts(id),
    amount                      NUMERIC(18,2) NOT NULL CHECK (amount > 0),
    note                        TEXT          NULL,
    created_at                  TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE TABLE reminders (
    id                UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id           UUID        NOT NULL REFERENCES accounts(id),
    title             VARCHAR(150) NOT NULL,
    amount            NUMERIC(18,2) NULL,
    frequency         VARCHAR(20) NOT NULL
                                  CHECK (frequency IN ('Daily','Weekly','Monthly','Quarterly','Yearly')),
    day_of_month      SMALLINT    NULL,
    start_date        DATE        NOT NULL,
    next_due_date     DATE        NOT NULL,
    category_id       UUID        NULL REFERENCES categories(id),
    note              TEXT        NULL,
    status            VARCHAR(20) NOT NULL DEFAULT 'Active',
    notify_days_before SMALLINT   NOT NULL DEFAULT 1,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE broadcasts (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    created_by_admin_id UUID        NOT NULL REFERENCES accounts(id),
    title               VARCHAR(200) NOT NULL,
    body                TEXT        NOT NULL,
    target_audience     VARCHAR(50) NOT NULL DEFAULT 'All',
    status              VARCHAR(20) NOT NULL DEFAULT 'Queued',
    scheduled_at        TIMESTAMPTZ NULL,
    sent_at             TIMESTAMPTZ NULL,
    target_count        INT         NOT NULL DEFAULT 0,
    delivered_count     INT         NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE notifications (
    id            UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id       UUID        NOT NULL REFERENCES accounts(id),
    type          VARCHAR(30) NOT NULL,
    title         VARCHAR(200) NOT NULL,
    body          TEXT        NOT NULL,
    is_read       BOOLEAN     NOT NULL DEFAULT FALSE,
    read_at       TIMESTAMPTZ NULL,
    broadcast_id  UUID        NULL REFERENCES broadcasts(id),
    metadata_json JSONB       NULL,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_notifications_user_unread ON notifications(user_id, is_read) WHERE is_read = FALSE;

-- ==================== MIGRATION 5 ==========================
-- Import Drafts

CREATE TABLE import_transaction_drafts (
    id                   UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    import_job_id        UUID          NOT NULL REFERENCES import_jobs(id),
    row_index            INT           NOT NULL,
    transaction_date     TIMESTAMPTZ   NULL,
    amount               NUMERIC(18,2) NULL,
    type                 VARCHAR(20)   NULL,
    raw_description      TEXT          NULL,
    suggested_note       TEXT          NULL,
    suggested_category_id UUID         NULL REFERENCES categories(id),
    suggested_jar_id     UUID          NULL REFERENCES jars(id),
    is_valid             BOOLEAN       NOT NULL DEFAULT TRUE,
    validation_error     TEXT          NULL,
    normalized_payload_json JSONB      NULL,
    created_at           TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    UNIQUE (import_job_id, row_index)
);

-- ==================== MIGRATION 6 ==========================
-- AI Settings

CREATE TABLE ai_settings (
    id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    model_name           VARCHAR(100) NOT NULL,
    system_prompt        TEXT         NOT NULL,
    temperature          NUMERIC(3,2) NOT NULL DEFAULT 0.7,
    max_tokens           INT          NOT NULL DEFAULT 1000,
    api_key_encrypted    TEXT         NULL,
    is_enabled           BOOLEAN      NOT NULL DEFAULT TRUE,
    updated_by_admin_id  UUID         NULL REFERENCES accounts(id),
    updated_at           TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- ==================== SEED DATA ==========================

INSERT INTO roles(id, code, name) VALUES
('11111111-1111-1111-1111-111111111111', 'User',       'User'),
('22222222-2222-2222-2222-222222222222', 'Admin',      'Admin'),
('33333333-3333-3333-3333-333333333333', 'SuperAdmin', 'SuperAdmin');
