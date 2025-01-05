## Add secrets

```
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=mydb;User=root;Password=1234;Port=3306;"
dotnet user-secrets set "JwtConfig:Issuer" "https://myapi.com"
dotnet user-secrets set "JwtConfig:Audience" "https://myapi.com"
dotnet user-secrets set "JwtConfig:Key" "your-secret-key"
```
