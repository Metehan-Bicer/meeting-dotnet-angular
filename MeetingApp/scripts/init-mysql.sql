-- MySQL initialization script for Meeting App
-- This script will be executed when the MySQL container starts for the first time

-- Create database if not exists (already created by environment variables)
USE MeetingAppDB;

-- Grant all privileges to the meeting user
GRANT ALL PRIVILEGES ON MeetingAppDB.* TO 'meetinguser'@'%';
FLUSH PRIVILEGES;

-- Set timezone to UTC
SET time_zone = '+00:00';

-- Log that initialization is complete
SELECT 'MySQL initialization completed for MeetingApp' as Status;