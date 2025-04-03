OBS Booking Console Application


Overview

This project is a service-oriented .NET console application designed to simulate and automate employee check-in and check-out bookings.
It integrates with various OBS APIs to synchronize personnel data, work schedules, and booking records.


Features

🔹 Automated Employee Bookings
Employees are automatically booked based on their work shifts.

🔹 OBS API Integration
The application fetches personnel and work schedule data from OBS services.

🔹 Logging & Monitoring
All events and booking processes are logged using Serilog for monitoring and debugging.

🔹 Configurable Architecture
Settings are stored in a separate appsettings.OBS.Configuration.json file for better maintainability.



Architecture

The application follows a service-oriented architecture with distinct responsibilities:

EmployeeBookingService → Manages the booking processes.
ObserverService (formerly Worker) → Monitors and processes scheduled bookings.
EmployeeDetailsProvider → Fetches and updates relevant employee details.


Technologies

 - .NET 5
 - Microsoft Extensions (Logging, Configuration, Dependency Injection)
 - Serilog (Logging)
 - OBS APIs (Personnel, Calendar, Booking)


Setup

1️⃣ Configure the Application

Edit appsettings.OBS.Configuration.json with your API URLs and authentication details.

2️⃣ Install Dependencies (if not already installed)

 dotnet restore

3️⃣ Run the Application

 dotnet run


License

This project is licensed under the MIT License. See the LICENSE file for more details.

