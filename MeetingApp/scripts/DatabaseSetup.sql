-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    PhoneNumber TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    ProfileImagePath TEXT,
    CreatedAt TEXT NOT NULL
);

-- Create Meetings table
CREATE TABLE IF NOT EXISTS Meetings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    StartDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    DocumentPath TEXT,
    IsCancelled INTEGER NOT NULL DEFAULT 0,
    CancelledAt TEXT,
    CreatedAt TEXT NOT NULL,
    UserId INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create MeetingLogs table for audit trail
CREATE TABLE IF NOT EXISTS MeetingLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MeetingId INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT,
    StartDate TEXT NOT NULL,
    EndDate TEXT NOT NULL,
    DocumentPath TEXT,
    IsCancelled INTEGER NOT NULL,
    CancelledAt TEXT,
    CreatedAt TEXT NOT NULL,
    UserId INTEGER NOT NULL,
    DeletedAt TEXT NOT NULL,
    DeletedBy TEXT
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);
CREATE INDEX IF NOT EXISTS IX_Meetings_UserId ON Meetings(UserId);
CREATE INDEX IF NOT EXISTS IX_Meetings_IsCancelled ON Meetings(IsCancelled);
CREATE INDEX IF NOT EXISTS IX_Meetings_StartDate ON Meetings(StartDate);