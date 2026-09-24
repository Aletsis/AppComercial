# AppComercial

> **Sistema de Integración Empresarial y API REST para CONTPAQi® Comercial**

**AppComercial** es una solución de alto rendimiento diseñada para interconectar aplicaciones web, móviles, puntos de venta (POS) y plataformas de comercio electrónico con **CONTPAQi® Comercial**. 

Combina lectura de alta velocidad mediante **Entity Framework Core / SQL Server** con escritura y timbrado fiscal transaccional a través del **SDK nativo (P/Invoke x86)** de CONTPAQi.

---

## 🏛️ Arquitectura y Estructura del Repositorio

La solución está construida siguiendo los principios de **Clean Architecture** y **CQRS (Command Query Responsibility Segregation)**:

```
AppComercial/
├── AppComercial.Api/             # API REST ASP.NET Core (Swagger, Versioning, Auth Dual)
├── AppComercial.Application/     # CQRS (MediatR), Validaciones (FluentValidation), DTOs
├── AppComercial.Domain/          # Entidades de BD CONTPAQi, constantes y modelos base
├── AppComercial.Infrastructure/  # EF Core DbContexts, Wrapper SDK P/Invoke y repositorios
├── AppComercial.ServerManager/   # Monitor de escritorio WPF para administración del servicio
├── AppComercial.UnitTests/       # Pruebas unitarias
├── docs/                         # Documentación técnica extendida
└── Setup.iss                     # Script de instalación para Windows (Inno Setup)
```

Para una explicación detallada de los patrones y el diseño híbrido, consulta el documento [Arquitectura del Sistema](docs/arquitectura.md).

---

## 🚀 Requisitos del Sistema

- **Sistema Operativo:** Windows 10, Windows 11 o Windows Server 2016+ (64-bit).
- **Entorno de Ejecución:** .NET 10 SDK / Runtime.
- **CONTPAQi Comercial:** Versión Comercial Premium, Start o Pro instalada y licenciada localmente.
- **Base de Datos:** Microsoft SQL Server con las bases de datos de empresa de CONTPAQi activas.
- **Compilador Inno Setup 6** (Opcional, sólo para empaquetar el instalador `.exe`).

---

## ⚙️ Configuración Rápida

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/Aletsis/AppComercial.git
   ```

2. **Configuración de Conexión:**
   Ajusta las credenciales de SQL Server y los parámetros de escucha en `AppComercial.Api/appsettings.json` o configúralos gráficamente desde la aplicación **ServerManager**.

3. **Compilación (Arquitectura x86 obligatoria):**
   ```bash
   dotnet build -r win-x86
   ```

4. **Ejecución y Supervisión:**
   Inicia la aplicación de administración de escritorio `AppComercial.ServerManager` para arrancar la API con aislamiento de sesión y supervisar los logs en tiempo real.

---

## 🔐 Seguridad y Autenticación

La API soporta dos esquemas de autenticación:
- **`X-Api-Key` (Recomendado para servidores y servicios):** Llave segura autogenerada en el primer arranque (`ApiSettings:ApiKey`).
- **`Bearer Token` (JWT):** Generación de tokens para usuarios de aplicaciones web o frontend mediante `POST /api/AuthTokens/login`.

Para ejemplos de consumo y catálogo de endpoints, consulta la [Guía de Integración con la API](docs/guia_integracion_api.md).

---

## 📦 Despliegue y Empaquetado

Para publicar los binarios en modo producción y generar el instalador de Windows, sigue las instrucciones de la [Guía de Despliegue e Instalación](docs/despliegue_e_instalacion.md).

---

## 📚 Documentación y Manuales

### 🛠️ Manuales Técnicos y de Desarrollo
- 📄 [Arquitectura y Diseño del Sistema](docs/arquitectura.md) — Patrón híbrido EF Core + SDK x86, CQRS y Clean Architecture.
- 🔌 [Guía de Integración y Consumo de la API](docs/guia_integracion_api.md) — Ejemplos de peticiones JSON para Facturas, Clientes, Inventarios y Catálogos.
- 🧩 [Referencia Técnica del SDK CONTPAQi](docs/referencia_sdk_contpaqi.md) — Firmas P/Invoke, estructuras nativas C++ y códigos de retorno.
- 🚀 [Guía de Compilación, Despliegue e Instalación](docs/despliegue_e_instalacion.md) — Instrucciones de publicación en x86 y compilación de instalador con Inno Setup.

### 🖥️ Manuales de Operación
- 📘 [Manual de Operación de ServerManager](docs/manual_operacion_servermanager.md) — Guía para administradores de sistemas y operadores (arranque, monitoreo, firewall y configuración de empresa).

### 🩺 Documentos de Soporte y Diagnóstico
- 🩺 [Guía de Soporte y Solución de Problemas (Troubleshooting)](docs/guia_soporte_troubleshooting.md) — Matriz de diagnóstico, resolución de incidentes del SDK/SQL Server, fallos de timbrado CFDI y contingencias.
