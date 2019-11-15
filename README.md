# Precipitation Data Parsing - WPF Application

A quick WPF application for reading, parsing, saving and presenting precipitation data from `.pre` files.

## Requirements: 
  - .NET Framework 4.8
  - Visual Studio 2019 (will almost certainly work on VS2017 - and perhaps earlier versions - but I'm unable to check for certain)
  
## Operation:
  - Open solution in Visual Studio
  - Right click **PrecipitationDataApp_WPF** project
  - Choose **Set As Startup Project** 
  - Run via **Start/Debug**

## Assessor Notes 
  - File parsing and data handling happens primarily in `FileHandler.cs`, with a handful of extra methods held in `Functions.cs`
  - To get a feel for the steps the project takes when parsing and saving the data, it would probably be easiest 
to run and step through the `TestFileHandler_SaveData` method of the **Tests** project.
