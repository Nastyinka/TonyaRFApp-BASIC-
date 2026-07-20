# **Tonya RF Client Manager**

**A beauty clinic management system built with C# and WPF, designed to help small beauty clinics manage clients, appointments, and treatments in one place.**

# Features
Client Management — Add, edit and delete client records including medical history and consent information
Appointment Booking — Book, update and cancel appointments with ease
Week Calendar — Visual week view showing all booked appointments at a glance
Treatments — Manage your treatment menu including prices and durations
Records — View full appointment history per client
Walk-in Booking — Quickly book appointments for new clients before their details are collected

## **Requirements**

**Before installing the app, you need two things:**

1. Windows 10 or Windows 11

The app runs on Windows only.

2. SQL Server Express (Free)

This is the database that stores all your clinic data. It only needs to be installed once.

**Download it here:** https://www.microsoft.com/en-us/sql-server/sql-server-downloads
Choose the Express edition — it is completely free.

**Also download SSMS (SQL Server Management Studio) to manage your database:**
https://aka.ms/ssmsfullsetup

## Installation
**Step 1** — Install SQL Server Express

Run the SQL Server Express installer and follow the steps. When asked for the instance name, leave it as the default: SQLEXPRESS.

**Step 2** — Set up the database
Open SSMS and connect to your server
Download the database backup file: BeautyClinicDB.bak (available in the Releases section of this page)
In SSMS, right click Databases → Restore Database
Select Device → click the ... button → Add → find your .bak file
Click OK to restore

This gives you the database with all the correct tables ready to go.

**Step 3** — Install the app
Go to the Releases section of this page (right hand side on GitHub)
Download TonyaRF_Setup.exe
Run it and follow the installer — it takes about 30 seconds
A shortcut will appear on your Desktop and in your Start Menu
**Step 4** — Open the app

Double click Tonya RF Client Manager on your Desktop and you are ready to go.

## Updating the App

When a new version is released:

Download the new TonyaRF_Setup.exe from the Releases section
Run it — it installs over the existing version automatically
Your database and all client data are completely untouched

❓ Troubleshooting

The app opens but shows a database error
Make sure SQL Server Express is running. Open the Start Menu, search for SQL Server Configuration Manager, and check that the SQLEXPRESS service is started.

I forgot to restore the database
Follow Step 2 of the Installation section above. You can do this at any time without reinstalling the app.

The installer won't run
Right click TonyaRF_Setup.exe → Run as administrator.

## Built With
C# and WPF — application logic and interface
.NET Framework 4.7.2 — included in Windows 10/11, no separate install needed
SQL Server Express — database
Inno Setup — installer

If you run into any issues, open an Issue on this GitHub page and describe what happened. Include any error messages you see — even a screenshot helps.
