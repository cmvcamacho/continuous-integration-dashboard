# Continuous Integration Dashboard #

Dashboard to display build information from TeamCity (and other CI's). Each authenticated user can create his own dashboard view, aggregating Builds in project panels.

TODO: add screenshoots

## Installation

The installation scripts allow you to install this in your local machine or on remote machines.
* Local machine
	1. Compile solution with VS2013 and run it (it will use LocalDB and IISExpress)
	1. Install it locally
		* Configure installation properties file ".\deploy\config\local.properties"  
		* Open a Command Prompt with Administrator rights and run the command ".\deploy\Install.bat local"
		* The 'local' keyword maps to the file ".\deploy\config\local.properties"
		* This will use local IIS and SQL Server

* Remote machine
	* Configure installation properties file ".\deploy\config\remote.properties"
	* Open a Command Prompt with Administrator rights and run the command ".\deploy\Install.bat remote"
	* The 'local' keyword maps to the file ".\deploy\config\remote.properties"

* Configuration:
	The properties file have meaningful names so it should be easy to configure it. 
	
## Usage

The dashboard contains the following features:
* Integrated authentication with company Active Directory
* Allow customized views per user
* Aggregate view of builds in logical projects
* Automatically refresh status (configured by default to update every 5 minutes)
* Only allows selecting TeamCity build configurations that aren't archived

## Roadmap

* Add support to other CI's like Jenkins, TFS, CruiseControl and so on...
* Add support to sort projects and builds
* Improve deployment scripts

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## Technology and Dependencies

This project is built using the following stack:
* AngularJS
* BootStrap
* Toastr
* Microsoft MVC
* Microsoft SignalR
* Hangfire (to refresh build in the background every 5 minutes)
* EntityFramework
* Autofac
* AutoMapper
* Serilog
* Newtonsoft.Json
* NUnit
* AutoFixture
* FakeItEasy
* FluentAssertions
* [TeamCitySharp-forked](https://github.com/y-gagar1n/TeamCitySharp)


## Credits

Copyright (c) 2015 Carlos Camacho

## License

See the [LICENSE](LICENSE.md) file for license rights and limitations (MIT).

