# Manual de Operación - ServerManager

Este manual está dirigido a los administradores de sistemas, personal de TI y operadores responsables de la supervisión y mantenimiento del servicio **AppComercial**.

---

## 1. Introducción al ServerManager

**AppComercial ServerManager** es la consola de administración en entorno de escritorio (WPF) diseñada para:
- Iniciar, pausar, reiniciar y supervisar el servidor de la API REST.
- Visualizar el consumo de recursos, estado de salud y logs en tiempo real.
- Administrar la conexión a Microsoft SQL Server y las empresas de CONTPAQi® Comercial.
- Administrar parámetros de red, claves API (`X-Api-Key`) y secretos JWT.
- Minimizarse en la bandeja del sistema (System Tray) para operar de forma desatendida.

---

## 2. Puesta en Marcha y Pantalla Principal

### 2.1. Iniciar la Aplicación
1. Inicie sesión en Windows en el servidor donde está instalado CONTPAQi Comercial.
2. Ejecute el acceso directo **AppComercial ServerManager** (o `C:\Program Files (x86)\AppComercial\ServerManager\AppComercial.ServerManager.exe`).
3. La interfaz principal mostrará el estado actual del servicio:
   - 🔴 **Detenido:** El servidor API no está escuchando peticiones.
   - 🟡 **Iniciando:** El proceso hijo se está levantando y verificando puertos.
   - 🟢 **En Ejecución:** La API está lista para recibir peticiones en la URL asignada.

---

## 3. Operación del Servidor API

```
┌────────────────────────────────────────────────────────┐
│                   ESTADO: EN EJECUCIÓN                 │
│         URL: http://*:5271 | Tiempo activo: 04:22:15   │
├────────────────────────────────────────────────────────┤
│  [ Iniciar ]       [ Detener ]       [ Reiniciar ]     │
└────────────────────────────────────────────────────────┘
```

### 3.1. Acciones Básicas
- **Iniciar Servidor:** Lanza el proceso `AppComercial.Api.exe` en segundo plano bajo la sesión de usuario activa.
- **Detener Servidor:** Termina de forma ordenada las conexiones y cierra el proceso API.
- **Reiniciar Servidor:** Útil tras actualizar certificados fiscales o modificar la configuración de red.
- **Abrir Swagger UI:** Abre automáticamente el navegador predeterminado en `http://localhost:5271/` para probar endpoints.

### 3.2. Operación en Segundo Plano (System Tray)
- Al cerrar la ventana principal (`X`), la aplicación se oculta en la **bandeja del sistema** (junto al reloj de Windows).
- Para restaurar la ventana, haga doble clic en el icono de AppComercial.
- Para cerrar definitivamente la aplicación, haga clic derecho en el icono de la bandeja y seleccione **Salir**.

---

## 4. Configuración del Sistema

### 4.1. Conexión a Base de Datos
1. Diríjase a la pestaña **Configuración**.
2. Especifique el **Servidor SQL** (ej. `LOCALHOST\COMPAC2019` o `127.0.0.1,1433`).
3. Ingrese el **Usuario** (`sa`) y **Contraseña**.
4. Haga clic en **Probar Conexión**. La lista desplegable se poblará automáticamente con las empresas registradas en CONTPAQi.
5. Seleccione la empresa activa predeterminada y haga clic en **Guardar Configuración**.

### 4.2. Parámetros de Red y Firewall
- **URL de Escucha Predeterminada:** `http://*:5271` (permite conexiones locales y desde la red local/VPN).
- **Puerto de Comunicación:** Puerto TCP `5271`.
- **Regla en Firewall de Windows:** Debe existir una regla de entrada para el puerto TCP 5271:
  ```powershell
  New-NetFirewallRule -DisplayName "AppComercial API" -Direction Inbound -LocalPort 5271 -Protocol TCP -Action Allow
  ```

### 4.3. Gestión de Claves de Seguridad
- **API Key (`X-Api-Key`):** Clave criptográfica utilizada por clientes externos. Puede copiarse al portapapeles o regenerarse mediante el botón **Generar Nueva Clave**.
- **JWT Secret:** Llave HMAC de 256 bits para firma de tokens de sesión.

---

## 5. Monitoreo y Visor de Logs

En la pestaña **Logs / Registro de Eventos**:
- Se muestran en tiempo real las peticiones HTTP entrantes, códigos de respuesta (200, 400, 500) y tiempos de ejecución.
- Errores nativos devueltos por el SDK de CONTPAQi.
- Botón **Abrir Carpeta de Logs** para acceder directamente a `C:\AppComercialLogs\`.
