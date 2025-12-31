CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL
);

CREATE TABLE BadDrivers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CarMake NVARCHAR(50),
    CarModel NVARCHAR(50),
    CarColor NVARCHAR(30),
    CarYear INT,
    Location NVARCHAR(200),
    Latitude FLOAT,
    Longitude FLOAT,
    ViolationType NVARCHAR(100), -- e.g., "No Turn Signal - Left Turn"
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Seed default admin (password: password)
INSERT INTO Users (Username, PasswordHash) 
VALUES ('admin', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8');

-- Seed some sample data
INSERT INTO BadDrivers (CarMake, CarModel, CarColor, CarYear, Location, Latitude, Longitude, ViolationType, CreatedAt)
VALUES 
('BMW', 'X5', 'Black', 2022, 'Main St & 1st Ave', 40.7128, -74.0060, 'Changed lane without signal', DATEADD(day, -1, GETDATE())),
('Tesla', 'Model 3', 'White', 2023, 'Broadway', 40.7580, -73.9855, 'Turned right without signal', DATEADD(day, -2, GETDATE())),
('Audi', 'A4', 'Silver', 2020, '5th Ave', 40.7829, -73.9654, 'Changed lane without signal', DATEADD(hour, -5, GETDATE()));
