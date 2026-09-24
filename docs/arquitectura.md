# Arquitectura del Sistema - AppComercial

## 1. Visión General

**AppComercial** es una solución empresarial de alto rendimiento diseñada para interactuar e integrarse de forma bidireccional con **CONTPAQi® Comercial**. 

El sistema implementa una **Arquitectura Híbrida** para solventar las limitaciones del SDK COM nativo de CONTPAQi (32 bits, llamadas secuenciales y costo en consultas masivas):

1. **Lectura de Alto Rendimiento (Entity Framework Core / SQL Directo):**
   - Consultas de catálogos (clientes, proveedores, productos, existencias, precios, clasificaciones, etc.).
   - Reportes, listados de facturas y documentos históricos.
   - Acceso concurrente y sin bloqueo del motor SDK.
2. **Escritura Transaccional Segura (CONTPAQi SDK P/Invoke x86):**
   - Creación y modificación de documentos comerciales (Facturas, Compras, Pedidos, Cotizaciones, Devoluciones, Traspasos, Entradas/Salidas de almacén).
   - Timbrado fiscal CFDI (SAT) y generación de cadenas digitales.
   - Afectación automática de saldos, existencias e inventarios bajo las reglas de negocio nativas de CONTPAQi.

---

## 2. Diagrama de Capas (Clean Architecture + CQRS)

```
┌────────────────────────────────────────────────────────┐
│             Clientes Externos / Web / ERP              │
└───────────────────────────┬────────────────────────────┘
                            │ HTTP / REST / JSON
┌───────────────────────────▼────────────────────────────┐
│                  AppComercial.Api                      │
│   - Controladores REST (Versioning, Swagger, RateLimit)│
│   - Autenticación Dual: JWT Bearer & X-Api-Key         │
│   - Middleware Global de Errores y Logging             │
└───────────────────────────┬────────────────────────────┘
                            │ MediatR
┌───────────────────────────▼────────────────────────────┐
│               AppComercial.Application                 │
│   - Comandos (Commands) y Consultas (Queries) (CQRS)   │
│   - DTOs, Mappings (AutoMapper) y Validadores (Fluent) │
│   - Interfaces de Repositorios y Servicios SDK         │
└─────────────┬────────────────────────────┬─────────────┘
              │                            │
┌─────────────▼──────────────┐ ┌───────────▼─────────────┐
│    AppComercial.Domain     │ │AppComercial.Infrastructure│
│  - Entidades de Catálogos  │ │  - Entity Framework Core │
│  - Modelos de Dominio      │ │  - SDK CONTPAQi Wrapper  │
│  - Interfaces base         │ │  - Multi-empresa Context │
└────────────────────────────┘ └───────────┬─────────────┘
                                           │
                        ┌──────────────────┴──────────────────┐
                        │                                     │
             ┌──────────▼──────────┐               ┌──────────▼──────────┐
             │ SQL Server Database │               │  CONTPAQi SDK x86   │
             │ (Bases CONTPAQi)    │               │  (MGW_DLL / COM)    │
             └─────────────────────┘               └─────────────────────┘
```

---

## 3. Módulos y Proyectos de la Solución

### `AppComercial.Api`
- API Web desarrollada sobre ASP.NET Core (`net10.0`, arquitectura x86).
- Expone endpoints RESTful para la integración con sistemas frontend, e-commerce, terminales punto de venta y aplicaciones móviles.
- Soporta versionado de API (`/api/v1/...`).
- Integra documentación OpenAPI interactiva (Swagger UI) con autenticación dual.

### `AppComercial.Application`
- Contiene la lógica de negocio desacoplada mediante el patrón **CQRS** con **MediatR**.
- Validaciones automáticas de datos de entrada vía **FluentValidation**.
- Transformación de modelos de datos y DTOs con **AutoMapper**.

### `AppComercial.Domain`
- Entidades que representan los esquemas de bases de datos de CONTPAQi (`admDocumentos`, `admClientes`, `admProductos`, `admMovimientos`, etc.).
- Constantes, enumeraciones y definiciones de contratos de datos.

### `AppComercial.Infrastructure`
- **Acceso a Datos:** `ContpaqiDbContext` y `CompacWAdminDbContext` para la gestión multi-empresa y lectura directa de base de datos.
- **Integración SDK:** Implementación de `IContpaqiSdkService` mediante llamadas P/Invoke a las bibliotecas nativas de CONTPAQi (`MGW_DLL.dll`).
- Gestión de conexión dinámica basada en el alias/GUID de la empresa seleccionada.

### `AppComercial.ServerManager`
- Aplicación de escritorio moderna desarrollada en **WPF** (Windows Presentation Foundation).
- Diseñada para ejecutarse en el servidor o estación de trabajo donde reside CONTPAQi Comercial.
- **Funcionalidades:**
  - Iniciar, detener y supervisar el servidor de la API en tiempo real.
  - Visualizar logs de solicitudes HTTP, errores y operaciones del SDK.
  - Configurar parámetros de red (puerto, URL de escucha `http://*:5271`), API Keys y secretos JWT.
  - Gestionar empresas registradas en CONTPAQi y probar conexiones.

### `AppComercial.UnitTests`
- Batería de pruebas unitarias para validadores, mapeos y manejadores de comandos de la capa de aplicación.

---

## 4. Consideraciones Técnicas y de Ejecución (Session 0 Isolation)

1. **Arquitectura de 32 bits (x86):**
   Las librerías del SDK de CONTPAQi Comercial están compiladas exclusivamente en 32 bits. Por este motivo, todos los proyectos ejecutables (`Api` y `ServerManager`) están configurados para compilarse con `<PlatformTarget>x86</PlatformTarget>`.
2. **Aislamiento de Sesión (Session 0):**
   Los componentes COM del SDK de CONTPAQi requieren interactuar con la sesión interactiva del usuario de Windows. Por ende, la API es administrada por `ServerManager` dentro de la sesión de escritorio activa en lugar de un Servicio de Windows aislado.

---

## 5. Esquema de Seguridad

- **X-Api-Key:** Autenticación recomendada para integraciones backend-to-backend (servidores locales, sincronizadores y microservicios). Se genera automáticamente en el primer arranque seguro.
- **JWT (JSON Web Tokens):** Diseñado para usuarios de aplicaciones web o móviles con expiración y roles.
- **Rate Limiting:** Control de concurrencia y prevención de saturación de peticiones por IP usando `AspNetCoreRateLimit`.
