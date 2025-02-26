# Demo Multi-Tenant App

### Add migration files

```bash
dotnet ef migrations add InitialCreate -p src/Demo.MultiTenant.App -s src/Demo.MultiTenant.App 
```

### Apply migration

```bash
dotnet ef database update -p src/Demo.MultiTenant.App -s src/Demo.MultiTenant.App
```
