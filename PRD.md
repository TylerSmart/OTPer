# OTPer - OTP Extraction and Storage API

## Overview

OTPer is a lightweight .NET 10 API that receives SMS/notification-style messages,
automatically extracts one-time passwords (OTP codes) from them, and stores the
results in a SQLite database. It exposes two endpoints: one for ingesting messages
and one for querying recent codes.

The service is designed to run as a Docker container on a TrueNAS Scale machine.

## Endpoints

### POST /api/otp

Accepts an incoming message and extracts the OTP code from it.

Request body: { message: string, sender: string }

Processing:
1. Persist the raw message, sender, and received timestamp.
2. Parse the message to extract a numeric OTP code (4-8 digits).
3. Store the extracted code (or null if none found) alongside the message.

Response: 201 Created with the saved OtpRecord.

### GET /api/otp

Retrieves the N most recent OTP records with optional filtering.

Query parameters:
- count (int, default 10, max 100): Number of recent records to return.
- sender (string, optional): Filter by sender (case-insensitive contains).
- keyword (string, optional): Filter by keyword in the original message.

Response: 200 OK with an array of OTP records ordered by most recent first.

## Data Model

### OtpRecord
- Id (int): Primary key, auto-increment
- Sender (string): Required
- Message (string): Required, raw message text
- Code (string?): Extracted OTP, nullable
- ReceivedAt (DateTime): UTC timestamp

## Tech Stack
- .NET 10 Web API (controller-based)
- Entity Framework Core with SQLite provider
- Docker (Linux container targeting TrueNAS Scale)

## Project Structure
- OTPer.Core/ - Domain models, OTP parser, DbContext, repository
- OTPer.API/ - ASP.NET Core API controllers, DI wiring, Dockerfile

## Non-Functional Requirements
- SQLite database file configurable via connection string, defaults to data/ volume-mounted path.
- Database is automatically created on startup.
- All timestamps are stored and returned in UTC.
