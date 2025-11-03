# Spendn't

Sistema de gestión de gastos e ingresos personales desarrollado con ASP.NET Core y Blazor WebAssembly.

## 📋 Requisitos Previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) o superior
- [Microsoft SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (cualquier edición)
- Un IDE como Visual Studio 2022 o Visual Studio Code

## ⚙️ Configuración Inicial

### 1. Configurar la Base de Datos

Antes de ejecutar el proyecto, debes actualizar la cadena de conexión en los archivos de configuración:

#### Archivos a modificar:
- `Spendnt.API/appsettings.json`
- `Spendnt.API/appsettings.Development.json`

#### Cambiar la cadena de conexión:

**Para SQL Server con autenticación de Windows:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR\\NOMBRE_INSTANCIA;Database=Spendnt;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Para SQL Server con autenticación SQL:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=Spendnt;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True;"
  }
}
```

**Ejemplo con autenticación de Windows:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=STEVEN_OSORIO\\MSSQLSERVER02;Database=Spendnt;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 2. Aplicar Migraciones

Desde la carpeta raíz del proyecto, ejecuta los siguientes comandos en PowerShell o tu terminal preferida:

```powershell
# Navegar al proyecto de la API
cd Spendnt.API

# Aplicar las migraciones a la base de datos
dotnet ef database update
```

Esto creará la base de datos `Spendnt` con todas las tablas necesarias y datos iniciales de prueba.

## 🚀 Ejecutar el Proyecto

### Opción 1: Ejecutar ambos proyectos manualmente

**Terminal 1 - API (Backend):**
```powershell
cd Spendnt.API
dotnet run
```
La API estará disponible en:
- HTTP: `http://localhost:5230`
- HTTPS: `https://localhost:7000`
- Swagger UI: `https://localhost:7000/swagger`

**Terminal 2 - Web (Frontend):**
```powershell
cd Spendnt.WEB
dotnet run
```
La aplicación web estará disponible en:
- HTTP: `http://localhost:5047`
- HTTPS: `https://localhost:8000`

### Opción 2: Ejecutar con Visual Studio

1. Abre la solución `Spendnt.sln`
2. Configura múltiples proyectos de inicio:
   - Click derecho en la solución → Propiedades
   - Selecciona "Proyectos de inicio múltiples"
   - Marca `Spendnt.API` y `Spendnt.WEB` como "Iniciar"
3. Presiona F5 para ejecutar

## 🌐 Puertos de la Aplicación

| Componente | HTTP | HTTPS |
|------------|------|-------|
| API Backend | `http://localhost:5230` | `https://localhost:7000` |
| Web Frontend | `http://localhost:5047` | `https://localhost:8000` |
| Swagger API Docs | - | `https://localhost:7000/swagger` |

## 👤 Usuario de Prueba

El sistema crea automáticamente un usuario de prueba con las siguientes credenciales (consulta `SeedDB.cs` para más detalles):

- **Email**: `testuser@example.com`
- **Contraseña**: `Password123!`

Este usuario ya tiene datos de ejemplo precargados (categorías, ingresos, egresos, metas de ahorro, etc.).

## 📁 Estructura del Proyecto

```
Spendnt-main/
├── Spendnt.API/          # Backend API (ASP.NET Core)
├── Spendnt.Shared/       # Entidades y DTOs compartidos
└── Spendnt.WEB/          # Frontend (Blazor WebAssembly)
```

## 🛠️ Tecnologías Utilizadas

- **Backend**: ASP.NET Core 9.0 Web API
- **Frontend**: Blazor WebAssembly
- **Base de Datos**: SQL Server con Entity Framework Core
- **Autenticación**: JWT (JSON Web Tokens)
- **ORM**: Entity Framework Core 9.0

## 📝 Notas Adicionales

- Asegúrate de que el servidor SQL Server esté en ejecución antes de iniciar la aplicación
- La primera vez que ejecutes el proyecto, se aplicarán las migraciones automáticamente
- El sistema incluye datos de prueba (categorías, saldos, etc.) que se crean automáticamente

