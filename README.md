# DirtBot
The unfinished source code for DirtBot

## Features
Coming...
 
## Building
### Initializing Entity Framework Core
   - If you have done this step already you can skip it
   - Install the EF Core CLI Tool with `dotnet tool install --global dotnet-ef`
   - Run `dotnet ef migrations add InitialCreate`
   - Run `dotnet ef database update`
   - NOTE 1: Make sure you have your MySql database running else this will complain
   - NOTE 2: Update credentials in `Database/DatabaseContext.cs`
### Installing Redis
#### If you are on Windows:
   - Follow [these instruction](https://docs.microsoft.com/en-us/windows/wsl/install-win10) on how to enable Windows Subsystem for Linux

#### Installation
   - Run `sudo apt update`
   - Run `sudo apt install redis-server`
   - Run `sudo service redis-server restart`

### Building the bot
   - Run `dotnet run` in the project root

The bot is in very early development now, not very many
features are implemented and optimized and bugs will most likely
appear. Be patient.

## Contributing
You can help by pull requesting if you want to. (See TODO list below)
The goal is to just make a Discord bot now.

### Active projects / TODO
   - Automod & moderation commands
   - Random commands
   - ... 
