# Guía de Compilación, Despliegue e Instalación - AppComercial

Este documento describe los pasos necesarios para compilar la solución, publicar los artefactos en 32 bits (x86) y empaquetar el instalador ejecutable para Windows mediante **Inno Setup**.

---

## 1. Requisitos Previos

- **Sistema Operativo:** Windows 10 / Windows 11 / Windows Server 2016 o superior.
- **SDK de .NET:** .NET 10 SDK (o versión configurada en los proyectos).
- **Entorno CONTPAQi:** CONTPAQi® Comercial Premium / Start / Pro instalado y activado en el servidor.
- **Motor de Base de Datos:** Microsoft SQL Server (instancia donde residen las bases de datos de empresas CONTPAQi).
- **Inno Setup 6:** Para generar el archivo de instalación `.exe` ([Inno Setup Downloads](https://jrsoftware.org/isdl.php)).

---

## 2. Publicación de Proyectos (x86)

Debido a la dependencia obligatoria de 32 bits del SDK de CONTPAQi, la publicación **debe realizarse para el Runtime Identifier `win-x86`**.

### 2.1. Publicar `AppComercial.Api`
```powershell
dotnet publish AppComercial.Api/AppComercial.Api.csproj `
    -c Release `
    -r win-x86 `
    --self-contained false `
    -o ./publish/Api
```

### 2.2. Publicar `AppComercial.ServerManager` (WPF)
```powershell
dotnet publish AppComercial.ServerManager/AppComercial.ServerManager.csproj `
    -c Release `
    -r win-x86 `
    --self-contained false `
    -o ./publish/ServerManager
```

---

## 3. Generación del Instalador con Inno Setup

El repositorio incluye el script de empaquetado [`Setup.iss`](../Setup.iss).

1. Abre **Inno Setup Compiler**.
2. Carga el archivo [`Setup.iss`](../Setup.iss).
3. Asegúrate de haber realizado previamente la publicación en `./publish/Api` y `./publish/ServerManager`.
4. Haz clic en **Build -> Compile** (o presiona `Ctrl + F9`).
5. El instalador resultante se generará en la carpeta `Output/AppComercial_Setup.exe`.

---

## 4. Estructura de Archivos en Producción

Al instalar en el servidor de producción:

- **Binarios de la Aplicación:**
  `C:\Program Files (x86)\AppComercial\`
  - `ServerManager\AppComercial.ServerManager.exe`
  - `Api\AppComercial.Api.exe`

- **Configuraciones Persistentes:**
  `C:\ProgramData\AppComercial\`
  - `Api\appsettings.json` (Contiene cadenas de conexión a SQL Server, URLs de escucha, JWT Secret y API Key).
  - `ServerManager\appsettings.json`

- **Archivos de Registro (Logs):**
  `C:\AppComercialLogs\`
  - `ApiServer.log`
  - `ServerManager.log`

---

## 5. Puesta en Marcha

1. Ejecutar como Administrador `AppComercial.ServerManager.exe`.
2. En la pestaña de **Configuración**, verificar la cadena de conexión a SQL Server y seleccionar la empresa activa de CONTPAQi.
3. Hacer clic en **Iniciar Servidor API**.
4. Abrir un navegador web y verificar que Swagger responde en `http://localhost:5271/`.
