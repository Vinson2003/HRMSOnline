-- 1. Roles (RoleName is UNIQUE, so check before inserting)
IF NOT EXISTS (SELECT 1 FROM Mst_Role WHERE RoleName = 'Admin')
INSERT INTO Mst_Role (RoleName, Created, CreatedBy) VALUES ('Admin', GETUTCDATE(), 'System');

IF NOT EXISTS (SELECT 1 FROM Mst_Role WHERE RoleName = 'HR')
INSERT INTO Mst_Role (RoleName, Created, CreatedBy) VALUES ('HR', GETUTCDATE(), 'System');

IF NOT EXISTS (SELECT 1 FROM Mst_Role WHERE RoleName = 'Manager')
INSERT INTO Mst_Role (RoleName, Created, CreatedBy) VALUES ('Manager', GETUTCDATE(), 'System');

IF NOT EXISTS (SELECT 1 FROM Mst_Role WHERE RoleName = 'Employee')
INSERT INTO Mst_Role (RoleName, Created, CreatedBy) VALUES ('Employee', GETUTCDATE(), 'System');

-- 2. Department for the first employee
IF NOT EXISTS (SELECT 1 FROM Mst_Department WHERE DepartmentName = 'Administration' AND IsDeleted = 0)
INSERT INTO Mst_Department (DepartmentName, Created, CreatedBy)
VALUES ('Administration', GETUTCDATE(), 'System');

-- 3. Employee that owns the admin account
IF NOT EXISTS (SELECT 1 FROM Mst_Employee WHERE EmployeeNumber = 'EMP-2026-0001')
INSERT INTO Mst_Employee (EmployeeNumber, FullName, Email, DepartmentId, ManagerId, AnnualLeaveBalance, Created, CreatedBy)
VALUES (
        'EMP-2026-0001',
        'Administrator',
        'admin@example.com',
        (SELECT TOP 1 Id FROM Mst_Department WHERE DepartmentName = 'Administration' AND IsDeleted = 0),
        NULL,
        12,
        GETUTCDATE(),
        'System');

-- 4. Admin account (password is stored as a hash, never as plain text)
IF NOT EXISTS (SELECT 1 FROM Mst_User WHERE Username = 'admin')
INSERT INTO Mst_User (Username, PasswordHash, EmployeeId, RoleId, IsActive, Created, CreatedBy, IsDeleted)
VALUES (
        'admin',
        'AQAAAAIAAYagAAAAEIc8FR/7ZKBPx8435BgRpJH2sT959fzJm3K7cMMrdyNsY+skjqskvUYhW5b2rh+2vg==',
        (SELECT Id FROM Mst_Employee WHERE EmployeeNumber = 'EMP-2026-0001'),
        (SELECT Id FROM Mst_Role WHERE RoleName = 'Admin'),
        1,
        GETUTCDATE(),
        'System',
        0);