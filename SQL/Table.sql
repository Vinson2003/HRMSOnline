CREATE TABLE Mst_Department (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE Mst_LeaveType (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TypeName VARCHAR(50) NOT NULL,
    RequiresAttachment BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE Mst_Employee (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeNumber VARCHAR(20) NOT NULL UNIQUE,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    DepartmentId INT NOT NULL,
    ManagerId INT NULL,
    AnnualLeaveBalance INT NOT NULL DEFAULT 12,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_Employee_Department FOREIGN KEY (DepartmentId) 
        REFERENCES Mst_Department(Id),
    CONSTRAINT FK_Employee_Manager FOREIGN KEY (ManagerId) 
        REFERENCES Mst_Employee(Id)
);

CREATE TABLE Trx_LeaveRequest (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    LeaveTypeId INT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalDays INT NOT NULL,
    Reason VARCHAR(500) NOT NULL,
    AttachmentPath VARCHAR(255) NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    RequestDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_LeaveRequest_Employee FOREIGN KEY (EmployeeId) 
        REFERENCES Mst_Employee(Id),
    CONSTRAINT FK_LeaveRequest_LeaveType FOREIGN KEY (LeaveTypeId) 
        REFERENCES Mst_LeaveType(Id)
);

CREATE TABLE Trx_LeaveApproval (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    LeaveRequestId INT NOT NULL,
    ApproverId INT NOT NULL,
    ActionStatus VARCHAR(20) NOT NULL,
    ActionDate DATETIME NOT NULL DEFAULT GETDATE(),
    Remarks VARCHAR(255) NULL,
    
    CONSTRAINT FK_LeaveApproval_LeaveRequest FOREIGN KEY (LeaveRequestId) 
        REFERENCES Trx_LeaveRequest(Id),
    CONSTRAINT FK_LeaveApproval_Approver FOREIGN KEY (ApproverId) 
        REFERENCES Mst_Employee(Id)
);