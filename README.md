To add a migration:

dotnet ef migrations add InitialCreate --project Data --startup-project API

To update the database:

dotnet ef database update --project Data --startup-project API

to drop all database 
dotnet ef database drop --project Data --startup-project API
