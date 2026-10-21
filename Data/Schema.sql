-- ==============================================================
-- TechZone Store - SQL Database Schema & Seed Script
-- ==============================================================

CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName NVARCHAR(100) NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Products (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name NVARCHAR(120) NOT NULL,
    Category NVARCHAR(60) NOT NULL,
    Brand NVARCHAR(60) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    Description NVARCHAR(1000) NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ==============================================================
-- Sample SQL Queries Used by the Application (Admin CRUD & User Read)
-- ==============================================================
-- 1. Read All Products (Available to Admin & User):
--    SELECT Id, Name, Category, Brand, Price, StockQuantity, Description, ImageUrl, CreatedAt FROM Products ORDER BY Id DESC;
--
-- 2. Add Product (Admin Only):
--    INSERT INTO Products (Name, Category, Brand, Price, StockQuantity, Description, ImageUrl, CreatedAt)
--    VALUES (@Name, @Category, @Brand, @Price, @StockQuantity, @Description, @ImageUrl, @CreatedAt);
--
-- 3. Update Product (Admin Only):
--    UPDATE Products SET Name=@Name, Category=@Category, Brand=@Brand, Price=@Price, StockQuantity=@StockQuantity,
--    Description=@Description, ImageUrl=@ImageUrl WHERE Id=@Id;
--
-- 4. Delete Product (Admin Only):
--    DELETE FROM Products WHERE Id=@Id;
