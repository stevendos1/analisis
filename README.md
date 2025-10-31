# Spendn't

## Descripción general
Spendn't es una plataforma de gestión financiera personal compuesta por una API ASP.NET Core y un cliente web Blazor. El sistema permite a las personas registrar ingresos y egresos, planificar metas de ahorro, administrar recordatorios de gastos y visualizar un calendario de eventos financieros consolidado. La solución utiliza Entity Framework Core como ORM y expone documentación interactiva mediante Swagger para facilitar la exploración de los endpoints.

## Arquitectura y componentes
- **Spendnt.API:** API REST protegida con autenticación JWT. Organiza la lógica de negocio en controladores, servicios auxiliares, DTOs y entidades persistidas en SQL Server. Incluye inicialización de datos (`SeedDB`) y almacenamiento de archivos.
- **Spendnt.Shared:** Biblioteca de clases compartida con las entidades del dominio y los DTOs que permiten desacoplar la capa de transporte de la capa de datos.
- **Spendnt.WEB:** Aplicación cliente Blazor WebAssembly que consume la API para ofrecer experiencias como dashboards, formularios de registro de movimientos y seguimiento de metas.

## Objetivo principal
Brindar a las personas una herramienta centralizada para tomar decisiones informadas sobre sus finanzas personales mediante el registro estructurado de sus operaciones y la visualización de indicadores clave.

## Objetivos específicos
1. **Planificación y análisis:** Levantar los requisitos funcionales y no funcionales del caso de uso financiero, identificando entidades, flujos y reglas de negocio.
2. **Diseño de arquitectura y datos:** Definir una arquitectura en capas con contratos desacoplados (interfaces, DTOs) y modelar las relaciones entre entidades utilizando Entity Framework Core.
3. **Implementación de funcionalidades:** Desarrollar la API y el cliente web que materializan los casos de uso principales (autenticación, manejo de ingresos/egresos, metas de ahorro, recordatorios y calendario de eventos).
4. **Pruebas y aseguramiento de calidad:** Validar el funcionamiento de los endpoints y la interfaz mediante pruebas manuales y la documentación Swagger para garantizar la integridad de los datos y la experiencia del usuario.
5. **Despliegue y operación:** Preparar la solución para su publicación configurando la cadena de conexión, seeding inicial y políticas de seguridad (JWT, CORS) que permitan su operación en distintos entornos.

## Características destacadas
- Generación automática de un calendario financiero que integra transacciones, recordatorios y metas del usuario.
- Gestión completa del ciclo de vida de los ingresos, egresos, saldos, metas de ahorro y recordatorios.
- Autenticación y autorización basada en ASP.NET Core Identity con tokens JWT.
- Documentación Swagger UI disponible en entornos de desarrollo para explorar y probar la API.
- Separación clara entre entidades de dominio, DTOs y lógica de presentación para favorecer el mantenimiento y la escalabilidad.

## Tecnologías principales
- ASP.NET Core 7
- Entity Framework Core
- SQL Server
- Blazor WebAssembly
- ASP.NET Core Identity & JWT
- Swagger / OpenAPI
