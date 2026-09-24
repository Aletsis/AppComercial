# Guía de Soporte y Resolución de Problemas (Troubleshooting)

Este documento está diseñado para el equipo de soporte técnico y administradores de infraestructura. Contiene procedimientos de diagnóstico, causas raíz y soluciones a los incidentes más frecuentes.

---

## 1. Matriz Rápida de Diagnóstico

```
                               ¿Cuál es el síntoma?
                                        │
        ┌───────────────────────────────┼───────────────────────────────┐
        ▼                               ▼                               ▼
 [No inicia el Servidor]      [Error al Facturar / Timbrar]   [Error 401 / 403 API]
        │                               │                               │
  • Puerto en uso (5271)         • Error SDK (-102, -1004)        • API Key inválida
  • SQL Server inaccesible       • CSD caducado / contraseña      • Falta header X-Api-Key
  • Dependencias C++ faltantes   • Sin folios / serie bloqueada   • Token JWT expirado
```

---

## 2. Errores de Inicialización y Arranque

### 2.1. El Servidor no inicia: "Address already in use" o puerto bloqueado
- **Causa:** El puerto `5271` está siendo ocupado por otro proceso o una instancia previa colgada de `AppComercial.Api.exe`.
- **Diagnóstico:**
  ```cmd
  netstat -ano | findstr :5271
  ```
- **Solución:**
  1. En el Administrador de Tareas (`taskmgr`), finalice cualquier proceso `AppComercial.Api.exe`.
  2. Si el puerto está reservado por otra aplicación, modifique la URL de escucha en `C:\ProgramData\AppComercial\Api\appsettings.json` (ej. `http://*:5272`).

---

### 2.2. Error: "No se puede cargar el archivo DLL 'MGWServicios.dll' o una de sus dependencias"
- **Causa:**
  1. La aplicación fue compilada en 64 bits en lugar de 32 bits (`x86`).
  2. CONTPAQi Comercial no está instalado en la máquina.
  3. Faltan los paquetes redistribuibles de Visual C++ (x86).
- **Solución:**
  - Instalar **Visual C++ Redistributable 2015-2022 (x86)**.
  - Asegurarse de que el directorio de instalación de CONTPAQi (`C:\Program Files (x86)\Compac\COMERCIAL` o `C:\Program Files (x86)\Compacw\Facturacion`) exista y contenga las DLLs requeridas.

---

## 3. Errores en la Conexión a Base de Datos (SQL Server)

### 3.1. Error: "A network-related or instance-specific error occurred while establishing a connection to SQL Server"
- **Puntos de verificación:**
  1. Verificar que el servicio `SQL Server (INSTANCIA)` esté en ejecución (`services.msc`).
  2. Abrir **SQL Server Configuration Manager** -> **SQL Server Network Configuration** -> **Protocols for INSTANCIA** y comprobar que **TCP/IP** y **Named Pipes** estén en estado **Enabled**.
  3. Comprobar que el puerto TCP `1433` esté permitido en el Firewall de Windows.
  4. Validar que la autenticación de SQL Server esté configurada en **Modo Mixto** (SQL Server and Windows Authentication).

---

## 4. Errores del SDK CONTPAQi Comercial

### 4.1. Error al abrir empresa o inicializar sesión
- **Síntoma en Logs:** `Error al inicializar sesión en SDK CONTPAQi` o `fAbreEmpresa retornado != 0`.
- **Causa:**
  - Otra sesión de CONTPAQi tiene bloqueada la empresa en modo exclusivo.
  - El usuario/contraseña de SDK es incorrecto.
  - Se alcanzó el límite de terminales concurrentes licenciadas en CONTPAQi.
- **Solución:**
  1. Verificar en CONTPAQi Comercial que no haya procesos de reindexación o cierre de ejercicio activos.
  2. Cerrar sesiones inactivas de usuarios en el servidor de licencias de CONTPAQi.
  3. Reiniciar el servicio desde **AppComercial ServerManager**.

---

### 4.2. Error de Timbrado CFDI (SAT)
- **Síntoma:** El documento se genera pero falla al timbrar con mensaje de error del PAC o SAT.
- **Causa:**
  - Certificado de Sello Digital (CSD) vencido o revocado.
  - Contraseña del CSD incorrecta configurada en CONTPAQi.
  - RFC del emisor o receptor no coincide con la lista LCO del SAT.
  - Código Postal fiscal no coincide con la dirección registrada en la Constancia de Situación Fiscal.
- **Solución:**
  1. Ingresar a CONTPAQi Comercial -> Configuración -> Conceptos -> Factura.
  2. Verificar la vigencia del CSD y probar timbrado manual de una factura de prueba en CONTPAQi.
  3. Validar los datos fiscales del cliente en `POST /api/Clientes`.

---

## 5. Ubicación y Análisis de Registros (Logs)

Todos los eventos, advertencias y excepciones se registran en:

| Archivo de Log | Contenido |
| :--- | :--- |
| `C:\AppComercialLogs\ApiServer.log` | Registro de peticiones HTTP, middlewares, errores de base de datos y respuestas del SDK. |
| `C:\AppComercialLogs\ServerManager.log` | Registro de arranque de procesos, monitoreo de estado y cambios de configuración. |

---

## 6. Procedimiento de Contingencia y Reinicio Limpio

Si el sistema no responde o el motor SDK entra en un estado inconsistente:
1. Abra **ServerManager** y haga clic en **Detener Servidor**.
2. Si el botón no responde, abra `cmd` como Administrador y ejecute:
   ```cmd
   taskkill /F /IM AppComercial.Api.exe
   ```
3. Espere 10 segundos para que el sistema operativo libere los descriptores COM.
4. En **ServerManager**, haga clic en **Iniciar Servidor**.
5. Verifique en `http://localhost:5271/` que el servicio responda correctamente.
