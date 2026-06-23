-- PaymentsService - SQL Server schema and sample data
-- Run against the 'Payments' database.

IF OBJECT_ID('dbo.Movements', 'U') IS NOT NULL DROP TABLE dbo.Movements;
IF OBJECT_ID('dbo.Wallets', 'U') IS NOT NULL DROP TABLE dbo.Wallets;

CREATE TABLE dbo.Wallets (
    Id          INT IDENTITY PRIMARY KEY,
    DocumentId  VARCHAR(20)    NOT NULL,
    Name        NVARCHAR(120)  NOT NULL,
    Balance     DECIMAL(19,4)  NOT NULL,
    CreatedAt   DATETIME2      NOT NULL,
    UpdatedAt   DATETIME2      NOT NULL
);

CREATE TABLE dbo.Movements (
    Id        INT IDENTITY PRIMARY KEY,
    WalletId  INT            NOT NULL REFERENCES dbo.Wallets(Id),
    Amount    DECIMAL(19,4)  NOT NULL,
    Type      VARCHAR(10)    NOT NULL,  -- 'Debit' | 'Credit'
    CreatedAt DATETIME2      NOT NULL
);
-- NOTE: no additional indexes on Movements (intentional for the exercise).

INSERT INTO dbo.Wallets (DocumentId, Name, Balance, CreatedAt, UpdatedAt) VALUES
('0102030405', N'Ana Torres', 100.0000, SYSUTCDATETIME(), SYSUTCDATETIME()),
('0607080910', N'Luis Pena',   50.0000, SYSUTCDATETIME(), SYSUTCDATETIME()),
('1112131415', N'Marta Ruiz', 250.0000, SYSUTCDATETIME(), SYSUTCDATETIME());

-- Wallet 1: net movements = 100 (matches balance).
-- Wallet 2: net movements = 80 - 30 = 50 (matches balance).
-- Wallet 3: balance says 250 but net movements = 200 (inconsistency to be detected).
INSERT INTO dbo.Movements (WalletId, Amount, Type, CreatedAt) VALUES
(1, 100.0000, 'Credit', DATEADD(MINUTE, -60, SYSUTCDATETIME())),
(2,  80.0000, 'Credit', DATEADD(MINUTE, -50, SYSUTCDATETIME())),
(2,  30.0000, 'Debit',  DATEADD(MINUTE, -40, SYSUTCDATETIME())),
(3, 200.0000, 'Credit', DATEADD(MINUTE, -30, SYSUTCDATETIME()));
