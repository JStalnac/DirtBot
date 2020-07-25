# DirtBot
 The unfinished source code for DirtBot
 
 ## Building
 - Clone the repository
 - Initializing Entity Framework Core
     - If you have done this step already you can skip it
     - Install the EF Core CLI Tool with `dotnet tool install --global dotnet-ef`
     - Run `dotnet ef migrations add InitialCreate` in `src/DirtBot`
     - Run `dotnet ef database update`
     - Add a password with SQLCipher with you favorite tool.
        - Remember to update the password in `src/DirtBot/Database/DatabaseContext.cs`
        - TODO: Better way
 - Run `dotnet run` in `src/DirtBot`
 The bot is in very early development now, not very many
 features are implemented and optimized and bugs will most likely
 appear. Be patient.

 ## Helping
 You can help by pull requesting if you want to.
 I have moved back to Discord.Net and trying to
 catch up with progress in `dsharpplus`.
 The goal is to just make a Discord bot now.
 