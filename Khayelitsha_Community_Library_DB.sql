USE master;
GO

IF DB_ID('KhayelitshaLibraryDB') IS NOT NULL
BEGIN
    ALTER DATABASE KhayelitshaLibraryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE KhayelitshaLibraryDB;
END
GO

CREATE DATABASE KhayelitshaLibraryDB;
GO

USE KhayelitshaLibraryDB;
GO

CREATE TABLE Member
(
MemberID    INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
FullName    VARCHAR(100) NOT NULL,
Address      VARCHAR(200) NOT NULL,
Phone VARCHAR(20) NOT NULL,
JoinDate    DATE NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),

CONSTRAINT CHK_Member_Phone CHECK (LEN(Phone) >=10)
);
GO

CREATE TABLE Staff
(
StaffID     INT IDENTITY(1,1) PRIMARY KEY,
FullName    VARCHAR(100),
Role        VARCHAR(50)
);
GO

CREATE TABLE BookTitle
(
TitleID     INT IDENTITY(1,1) PRIMARY KEY,
Title       VARCHAR(200) NOT NULL,
Author      VARCHAR(100) NOT NULL,
ISBN        VARCHAR(20) NOT NULL UNIQUE,
Genre       VARCHAR(50) NULL
);
GO

CREATE TABLE BookCopy
(
CopyID      INT IDENTITY(1,1) PRIMARY KEY,
TitleID     INT NOT NULL,
Status      VARCHAR(20) NOT NULL DEFAULT 'Available',

CONSTRAINT FK_BookCopy_BookTitle FOREIGN KEY (TitleID)
           REFERENCES BookTitle(TitleID)
           ON DELETE CASCADE,
CONSTRAINT CHK_BookCopy_Status CHECK (Status IN ('Available', 'On Loan', 'Lost', 'Damaged'))
);
GO

CREATE TABLE Loan
(
LoanID      INT IDENTITY(1,1) PRIMARY KEY,
MemberID    INT NOT NULL,
CopyID      INT NOT NULL,
StaffID     INT NOT NULL,
LoanDate    DATE NOT NULL DEFAULT(CAST(GETDATE() AS DATE)),
DueDate     DATE NOT NULL,
ReturnDate  DATE NULL,

CONSTRAINT FK_Loan_Member FOREIGN KEY(MemberID)
    REFERENCES Member(MemberID),
CONSTRAINT FK_Loan_BookCopy FOREIGN KEY (CopyID)
    REFERENCES BookCopy(copyID),
CONSTRAINT FK_Loan_Staff FOREIGN KEY (StaffID)
        REFERENCES Staff(StaffID),
CONSTRAINT CHK_Loan_DueDate CHECK (DueDate >= LoanDate),
CONSTRAINT CHK_loan_ReturnDate CHECK (ReturnDate IS NULL OR ReturnDate >= LoanDate)
);
GO

CREATE INDEX IX_Loan_CopyID_ReturnDate ON Loan(CopyID, ReturnDate);
GO

CREATE UNIQUE INDEX UX_Loan_OneActiveLoanPerCopy
    ON Loan(CopyID)
    WHERE ReturnDate IS NULL;
GO

INSERT INTO Member (FullName, Address, Phone, JoinDate)
VALUES
('Emily Watson',    '14 Maple Drive, Khayelitsha',          '0721234567', '2024-02-10'),
('David Miller',    '88 Victoria Road, Khayelitsha',        '0739876543', '2024-03-15'),
('Sarah Jenkins',   '19 Oak Avenue, Khayelitsha',           '0824567891', '2024-05-01'),
('Michael Smith',   '52 Main Street, Khayelitsha',          '0611239876', '2024-06-20'),
('Jessica Taylor',  '7 Church Street, Khayelitsha',         '0785551234', '2025-01-08');
GO

INSERT INTO Staff (FullName, Role) VALUES
('Christopher Brown', 'Librarian'),
('Emma Wilson',       'Library Assistant'),
('James Anderson',    'Branch Supervisor');
GO

INSERT INTO BookTitle (Title, Author, ISBN, Genre)
VALUES
('To Kill a Mockingbird',                'Harper Lee',           '9780061120084', 'Classic Fiction'),
('1984',                                 'George Orwell',        '9780451524935', 'Dystopian'),
('The Great Gatsby',                     'F. Scott Fitzgerald',  '9780743273565', 'Classic Fiction'),
('Harry Potter and the Sorcerer''s Stone', 'J.K. Rowling',          '9780590353403', 'Fantasy'),
('The Hobbit',                           'J.R.R. Tolkien',       '9780547928227', 'Fantasy');
GO

INSERT INTO BookCopy (TitleID, Status) 
VALUES
(1, 'Available'),
(1, 'Available'),
(2, 'Available'),
(3, 'Available'),
(3, 'Available'),
(4, 'Available'),
(4, 'Available'),
(5, 'Available');
GO

INSERT INTO Loan (MemberID, CopyID, StaffID, LoanDate, DueDate, ReturnDate) 
VALUES
(1, 1, 1, '2025-07-01', '2025-07-15', '2025-07-14'),
(2, 3, 2, '2025-07-05', '2025-07-19', NULL),
(3, 4, 1, '2025-06-20', '2025-07-04', '2025-07-10'),
(4, 6, 3, '2025-07-20', '2025-08-03', NULL),
(5, 8, 2, '2025-06-01', '2025-06-15', NULL);
GO

UPDATE BookCopy SET Status = 'On Loan' WHERE CopyID IN (3, 6, 8);
GO