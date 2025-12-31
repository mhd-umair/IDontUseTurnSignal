-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TurnSignalViolationTracker')
BEGIN
    CREATE DATABASE TurnSignalViolationTracker;
END
GO

USE TurnSignalViolationTracker;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        Salt NVARCHAR(256) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2 NULL
    );
    
    CREATE INDEX IX_Users_Username ON Users(Username);
END
GO

-- Create CarBrands table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CarBrands')
BEGIN
    CREATE TABLE CarBrands (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        LogoUrl NVARCHAR(500) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
    
    CREATE INDEX IX_CarBrands_Name ON CarBrands(Name);
END
GO

-- Create Violations table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Violations')
BEGIN
    CREATE TABLE Violations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CarMake NVARCHAR(100) NOT NULL,
        CarModel NVARCHAR(100) NOT NULL,
        CarColor NVARCHAR(50) NOT NULL,
        CarYear INT NULL,
        ViolationType INT NOT NULL,
        Latitude FLOAT NULL,
        Longitude FLOAT NULL,
        LocationDescription NVARCHAR(500) NOT NULL DEFAULT '',
        City NVARCHAR(100) NOT NULL DEFAULT '',
        State NVARCHAR(100) NOT NULL DEFAULT '',
        Country NVARCHAR(100) NOT NULL DEFAULT '',
        OccurredAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedByUserId INT NOT NULL,
        OriginalVoiceTranscript NVARCHAR(MAX) NULL,
        Notes NVARCHAR(MAX) NULL,
        
        CONSTRAINT FK_Violations_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
    );
    
    CREATE INDEX IX_Violations_CarMake ON Violations(CarMake);
    CREATE INDEX IX_Violations_CarColor ON Violations(CarColor);
    CREATE INDEX IX_Violations_ViolationType ON Violations(ViolationType);
    CREATE INDEX IX_Violations_OccurredAt ON Violations(OccurredAt);
    CREATE INDEX IX_Violations_CreatedByUserId ON Violations(CreatedByUserId);
END
GO
