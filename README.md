# Instructions

## 1. Add nuget.org as source and restore packages

```
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet restore
```

## 2. Add secrets

```
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=mydb;User=root;Password=1234;Port=3306;"
dotnet user-secrets set "JwtConfig:Issuer" "https://myapi.com"
dotnet user-secrets set "JwtConfig:Audience" "https://myapi.com"
dotnet user-secrets set "JwtConfig:Key" "your-secret-key"
```

## 3. Install dotnet ef and update database

```
dotnet tool install --global dotnet-ef
dotnet ef database update
```

## 4. Run the app
```
dotnet run
```
