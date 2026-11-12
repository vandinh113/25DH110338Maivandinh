
CREATE DATABASE Warehouse_25DH110338;
GO

USE Warehouse_25DH110338;
GO

CREATE TABLE Products
(
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL
);
GO

INSERT INTO Products
(ProductName, Category, Price, Quantity)
VALUES
(N'Ban phim Mai Van Dinh', N'Thiet bi MAi Van Dinh', 250000, 10),
(N'Chuot Mai Van Dinh', N'Thiet bi Mai Van Dinh', 150000, 3),
(N'Tai nghe Mai Van Dinh', N'Phu kien Mai Van Dinh', 350000, 0),
(N'USB Mai Van Dinh', N'Phu kien Mai Van Dinh', 120000, 5),
(N'Loa Mai Van Dinh', N'Thiet bi Mai Van Dinh', 450000, 8);
GO

SELECT * FROM Products;
