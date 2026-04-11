# AppComercial

Sistema de gestión comercial e integración con CONTPAQi Comercial.

## Descripción

Este proyecto es una solución integral para la gestión de puntos de venta e integración con sistemas CONTPAQi. Incluye una API robusta, una aplicación de escritorio para la gestión de servidores y diversos módulos para facturación y reportes.

## Estructura del Proyecto

- **AppComercial.Api**: API REST de ASP.NET Core que sirve como puerta de enlace a la lógica de negocio y datos.
- **AppComercial.Application**: Capa de aplicación que contiene la lógica de negocio, comandos y consultas (CQRS).
- **AppComercial.Domain**: Entidades de dominio y lógica central.
- **AppComercial.Infrastructure**: Implementaciones de acceso a datos, servicios externos e integración con CONTPAQi.
- **AppComercial.ServerManager**: Aplicación de escritorio (WPF) para la administración del servidor y servicios.
- **AppComercial.UnitTests**: Pruebas unitarias para asegurar la calidad del código.

## Requisitos

- .NET 8 SDK
- SQL Server (con bases de datos de CONTPAQi Comercial)
- CONTPAQi Comercial instalado y configurado (para integraciones)

## Configuración

1. Clonar el repositorio.
2. Configurar las cadenas de conexión en `AppComercial.Api/appsettings.json`.
3. Ejecutar las migraciones de base de datos si es necesario.
4. Compilar y ejecutar la solución usando Visual Studio 2022 o el CLI de .NET.

## Tecnologías Utilizadas

- ASP.NET Core / Web API
- Entity Framework Core
- MediatR (CQRS)
- WPF (Windows Presentation Foundation)
- SQL Server

---

Desarrollado para la integración eficiente de sistemas comerciales.
