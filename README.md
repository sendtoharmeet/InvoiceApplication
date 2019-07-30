# InvoiceApplication
Xml Parse from a text message and validate

Required frameworks/plateform:
Visual Studio 2017 (>=15.9.2)
.Net Core 2.2 (Runtime & Development)

How to execute:
Open the solution in Visual Studio2017 and if you have the platform ready then build the application. 
It will download the required packages file So please be connected with internet.
You can run application which will open default endpoint /api/invoice/get, just for testing the setup.
Now Api is running you can test other endpoints:
  SetGstPrice
  GetGstPrice
  ProcessMessage:: This is the main method which expect Input message and will return output according to requirement.

Design of application:
I implemented a simple Api application, where I created separate endpoints under one controller 'Invoice'.
I utilized.Net core, Extensions.DependencyInjection to resolve dependency & ExtensionsLogging (With log4net for logs). 
Any exception will be logged in log file. 
Also implemented unit test cases to test business logic at dev side.
