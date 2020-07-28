 # DirtBot
 The unfinished source code for DirtBot

 ## Features
 Coming...
 
 ## Building
 - Clone the repository
 - Initializing Entity Framework Core
     - If you have done this step already you can skip it
     - Install the EF Core CLI Tool with `dotnet tool install --global dotnet-ef`
     - Run `dotnet ef migrations add InitialCreate` in `src/DirtBot`
     - Run `dotnet ef database update`
        - Make sure you have your MySql database running else this will complain
 - Run `dotnet run` in `src/DirtBot`
 
 The bot is in very early development now, not very many
 features are implemented and optimized and bugs will most likely
 appear. Be patient.

 ## Contributing
 You can help by pull requesting if you want to.
 I have moved back to Discord.Net and trying to
 catch up with progress in `dsharpplus`.
 The goal is to just make a Discord bot now.
 
 ### Active projects / TODO
   - Redis caching
   - Translation service
     - Not static
     - A directory with files which will be copied to build?
   - Automod & moderation commands
   - Random commands
   - ... 
