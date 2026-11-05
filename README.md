ASP.NET Core Web API for a healthcare appointment booking system. 
Implements JWT authentication with role-based access control for three user types: 
Patient, Doctor, and Admin. Patients can book and cancel appointments, doctors can 
manage their schedule and update appointment statuses, and admins have full system 
access. Built with in-memory storage, Swagger UI for testing, and SHA256 password hashing.



Step 1. Registration Patient 

Найди POST /api/auth/register → Try it out → insert this example:

json
{
  "username": "patient1",
  "password": "pass123",
  "email": "p@test.com",
  "firstName": "Anna",
  "lastName": "Smith",
  "role": "Patient"
}

Push Execute

Step 2. Logging
POST /api/auth/login:

json
{
  "username": "patient1",
  "password": "pass123"
}
