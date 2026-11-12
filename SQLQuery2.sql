
USE Warehouse_25DH110338;
GO

-- Xóa 5 sản phẩm bị thêm trùng
DELETE FROM Products
WHERE ProductID BETWEEN 6 AND 10;
GO

-- Kiểm tra lại danh sách
SELECT * FROM Products
ORDER BY ProductID;
USE Warehouse_25DH110338;
GO

EXEC sp_help 'Products';