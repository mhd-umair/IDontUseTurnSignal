USE TurnSignalViolationTracker;
GO

-- Seed Car Brands with logo URLs (using car-logos-api or similar public sources)
-- These are placeholder URLs - in production, you'd host these or use a reliable CDN

IF NOT EXISTS (SELECT * FROM CarBrands WHERE Name = 'Toyota')
BEGIN
    INSERT INTO CarBrands (Name, LogoUrl, IsActive) VALUES
    ('Toyota', 'https://www.carlogos.org/car-logos/toyota-logo-2019-3700x1200.png', 1),
    ('Honda', 'https://www.carlogos.org/car-logos/honda-logo-2000-full-1920x1080.png', 1),
    ('Ford', 'https://www.carlogos.org/car-logos/ford-logo-2017-1500x1101.png', 1),
    ('Chevrolet', 'https://www.carlogos.org/car-logos/chevrolet-logo-2013-1920x1080.png', 1),
    ('BMW', 'https://www.carlogos.org/car-logos/bmw-logo-2020-grey-1920x1080.png', 1),
    ('Mercedes-Benz', 'https://www.carlogos.org/car-logos/mercedes-benz-logo-2011-1920x1080.png', 1),
    ('Audi', 'https://www.carlogos.org/car-logos/audi-logo-2016-1920x1080.png', 1),
    ('Volkswagen', 'https://www.carlogos.org/car-logos/volkswagen-logo-2019-1500x1500.png', 1),
    ('Nissan', 'https://www.carlogos.org/car-logos/nissan-logo-2020-1920x1080.png', 1),
    ('Hyundai', 'https://www.carlogos.org/car-logos/hyundai-logo-2017-1920x1080.png', 1),
    ('Kia', 'https://www.carlogos.org/car-logos/kia-logo-2021-3000x1200.png', 1),
    ('Mazda', 'https://www.carlogos.org/car-logos/mazda-logo-2018-1920x1080.png', 1),
    ('Subaru', 'https://www.carlogos.org/car-logos/subaru-logo-2019-1920x1080.png', 1),
    ('Lexus', 'https://www.carlogos.org/car-logos/lexus-logo-1988-1920x1080.png', 1),
    ('Acura', 'https://www.carlogos.org/car-logos/acura-logo-1990-1920x1080.png', 1),
    ('Infiniti', 'https://www.carlogos.org/car-logos/infiniti-logo-1989-2560x1440.png', 1),
    ('Tesla', 'https://www.carlogos.org/car-logos/tesla-logo-2007-1920x1080.png', 1),
    ('Jeep', 'https://www.carlogos.org/car-logos/jeep-logo-1993-1920x1080.png', 1),
    ('Ram', 'https://www.carlogos.org/car-logos/ram-logo-2012-1920x1080.png', 1),
    ('GMC', 'https://www.carlogos.org/car-logos/gmc-logo-2200x900.png', 1),
    ('Dodge', 'https://www.carlogos.org/car-logos/dodge-logo-2011-1920x1080.png', 1),
    ('Cadillac', 'https://www.carlogos.org/car-logos/cadillac-logo-2021-1920x1080.png', 1),
    ('Buick', 'https://www.carlogos.org/car-logos/buick-logo-2022-1920x1080.png', 1),
    ('Chrysler', 'https://www.carlogos.org/car-logos/chrysler-logo-2010-1920x1080.png', 1),
    ('Lincoln', 'https://www.carlogos.org/car-logos/lincoln-logo-2019-1920x1080.png', 1),
    ('Volvo', 'https://www.carlogos.org/car-logos/volvo-logo-2014-1920x1080.png', 1),
    ('Porsche', 'https://www.carlogos.org/car-logos/porsche-logo-2014-1920x1080.png', 1),
    ('Land Rover', 'https://www.carlogos.org/car-logos/land-rover-logo-2020-1920x1080.png', 1),
    ('Jaguar', 'https://www.carlogos.org/car-logos/jaguar-logo-2012-1920x1080.png', 1),
    ('Mini', 'https://www.carlogos.org/car-logos/mini-logo-2018-1920x1080.png', 1),
    ('Mitsubishi', 'https://www.carlogos.org/car-logos/mitsubishi-logo-2020-1920x1080.png', 1),
    ('Fiat', 'https://www.carlogos.org/car-logos/fiat-logo-2020-1920x1080.png', 1),
    ('Alfa Romeo', 'https://www.carlogos.org/car-logos/alfa-romeo-logo-2015-1920x1080.png', 1),
    ('Maserati', 'https://www.carlogos.org/car-logos/maserati-logo-2020-1920x1080.png', 1),
    ('Genesis', 'https://www.carlogos.org/car-logos/genesis-logo-2020-1920x1080.png', 1),
    ('Rivian', 'https://www.carlogos.org/car-logos/rivian-logo-1920x1080.png', 1),
    ('Lucid', 'https://www.carlogos.org/car-logos/lucid-motors-logo-1920x1080.png', 1),
    ('Polestar', 'https://www.carlogos.org/car-logos/polestar-logo-1920x1080.png', 1);
END
GO

-- Create default admin user (password: admin123)
-- Salt and hash are pre-computed for 'admin123'
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    DECLARE @Salt NVARCHAR(256) = 'defaultSaltForAdmin123';
    DECLARE @PasswordHash NVARCHAR(256) = 'qYHGZtH5SZ8zZPXRrV+p6L8wXEXlbCvJcqFNP6tYz0M=';
    
    INSERT INTO Users (Username, PasswordHash, Salt, FullName, Email, IsActive, CreatedAt)
    VALUES ('admin', @PasswordHash, @Salt, 'System Administrator', 'admin@turnsignaltracker.com', 1, GETUTCDATE());
END
GO

-- Insert some sample violations for testing
IF NOT EXISTS (SELECT TOP 1 * FROM Violations)
BEGIN
    DECLARE @AdminUserId INT = (SELECT Id FROM Users WHERE Username = 'admin');
    
    INSERT INTO Violations (CarMake, CarModel, CarColor, CarYear, ViolationType, Latitude, Longitude, LocationDescription, City, State, Country, OccurredAt, CreatedByUserId, Notes)
    VALUES 
    ('Toyota', 'Camry', 'Black', 2022, 3, 40.7128, -74.0060, 'Broadway and 42nd Street', 'New York', 'NY', 'USA', DATEADD(hour, -2, GETUTCDATE()), @AdminUserId, 'Changed lanes without signaling'),
    ('BMW', '3 Series', 'White', 2021, 1, 40.7580, -73.9855, '5th Avenue and 53rd Street', 'New York', 'NY', 'USA', DATEADD(hour, -5, GETUTCDATE()), @AdminUserId, 'Left turn without signal'),
    ('Honda', 'Civic', 'Red', 2020, 4, 40.7484, -73.9857, 'Madison Avenue', 'New York', 'NY', 'USA', DATEADD(day, -1, GETUTCDATE()), @AdminUserId, 'Lane change right without signal'),
    ('Ford', 'F-150', 'Blue', 2023, 2, 40.7589, -73.9851, 'Times Square', 'New York', 'NY', 'USA', DATEADD(day, -1, GETUTCDATE()), @AdminUserId, 'Right turn without signal'),
    ('Tesla', 'Model 3', 'Gray', 2023, 3, 40.7614, -73.9776, 'Park Avenue', 'New York', 'NY', 'USA', DATEADD(day, -2, GETUTCDATE()), @AdminUserId, 'Lane change left'),
    ('Mercedes-Benz', 'C-Class', 'Silver', 2022, 5, 40.7527, -73.9772, 'Lexington Avenue', 'New York', 'NY', 'USA', DATEADD(day, -3, GETUTCDATE()), @AdminUserId, 'U-turn without signal'),
    ('Chevrolet', 'Malibu', 'Black', 2021, 1, 40.7484, -73.9857, 'Herald Square', 'New York', 'NY', 'USA', DATEADD(day, -4, GETUTCDATE()), @AdminUserId, 'Left turn'),
    ('Nissan', 'Altima', 'White', 2020, 3, 40.7580, -73.9855, 'Central Park South', 'New York', 'NY', 'USA', DATEADD(day, -5, GETUTCDATE()), @AdminUserId, 'Lane change'),
    ('Hyundai', 'Sonata', 'Blue', 2022, 4, 40.7614, -73.9776, 'Columbus Circle', 'New York', 'NY', 'USA', DATEADD(day, -6, GETUTCDATE()), @AdminUserId, 'Changed lanes right'),
    ('Toyota', 'RAV4', 'Green', 2023, 2, 40.7527, -73.9772, 'Upper East Side', 'New York', 'NY', 'USA', DATEADD(day, -7, GETUTCDATE()), @AdminUserId, 'Right turn violation');
END
GO
