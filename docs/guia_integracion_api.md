# Guía de Integración con la API REST - AppComercial

La API REST de **AppComercial** proporciona una interfaz para la lectura de catálogos y la generación de documentos comerciales en CONTPAQi® Comercial.

---

## 1. Acceso y Documentación Interactiva

- **Swagger UI:** `http://localhost:5271/` (o la IP/puerto configurado en el servidor).
- **Especificación OpenAPI (JSON):** `http://localhost:5271/swagger/v1/swagger.json`

---

## 2. Métodos de Autenticación

Todas las solicitudes (excepto endpoints públicos o de login si están configurados) requieren autenticación mediante uno de los siguientes métodos:

### Opción A: Cabecera `X-Api-Key` (Recomendada para servicios y backend)
Incluye tu clave API en los encabezados HTTP de cada petición:

```http
GET /api/v1/Productos HTTP/1.1
Host: localhost:5271
X-Api-Key: TU_API_KEY_AQUI
```

> **Nota:** La API Key se autogenera en el primer arranque seguro del servidor y puede consultarse o regenerarse desde la interfaz del **ServerManager** o en `%ProgramData%\AppComercial\Api\appsettings.json`.

---

### Opción B: `Bearer Token` (JWT)
1. Obtén un token enviando tus credenciales al endpoint de autenticación:
   ```http
   POST /api/AuthTokens/login
   Content-Type: application/json

   {
     "usuario": "admin",
     "password": "tu_password"
   }
   ```
2. Envía el token en el header `Authorization`:
   ```http
   GET /api/v1/Clientes HTTP/1.1
   Host: localhost:5271
   Authorization: Bearer eyJhbGciOi...
   ```

---

## 3. Endpoints Principales y Ejemplos de Uso

### 3.1. Productos y Catálogos

#### Consultar Productos con Existencias
```http
GET /api/Productos?take=50&buscar=LAPTOP HTTP/1.1
X-Api-Key: TU_API_KEY
```

**Respuesta (200 OK):**
```json
[
  {
    "cidproducto": 105,
    "ccodigoproducto": "PROD-001",
    "cnombreproducto": "Laptop Dell Inspiron 15",
    "ctipoproducto": 1,
    "cprecio1": 15499.00,
    "ccontrolExistencia": 1,
    "cclaveSat": "43211503"
  }
]
```

---

### 3.2. Clientes y Cuentas por Cobrar

#### Crear o Actualizar Cliente
```http
POST /api/Clientes HTTP/1.1
Content-Type: application/json
X-Api-Key: TU_API_KEY

{
  "codigo": "CTE-0042",
  "razonSocial": "Comercializadora del Norte SA de CV",
  "rfc": "CNO180215AB1",
  "usoCfdi": "G03",
  "regimenFiscal": "601",
  "diasCredito": 30,
  "limiteCredito": 50000.00,
  "email": "facturacion@comercializadoranorte.com"
}
```

---

### 3.3. Facturación y Documentos de Venta

#### Generar Factura de Venta con Partidas y Timbrado
```http
POST /api/Facturas/generar HTTP/1.1
Content-Type: application/json
X-Api-Key: TU_API_KEY

{
  "codigoConcepto": "4",
  "codigoCliente": "CTE-0042",
  "serie": "F",
  "fecha": "2026-09-23T00:00:00",
  "formaPago": "03",
  "metodoPago": "PUE",
  "monedaId": 1,
  "tipoCambio": 1.0,
  "observaciones": "Factura generada vía API",
  "autoTimbrar": true,
  "movimientos": [
    {
      "codigoProducto": "PROD-001",
      "codigoAlmacen": "1",
      "unidades": 2,
      "precio": 15499.00,
      "porcentajeDescuento1": 0.0,
      "observaciones": "Equipo con garantía extendida"
    }
  ]
}
```

**Respuesta Exitosa (200 OK):**
```json
{
  "exitoso": true,
  "documentoId": 18420,
  "folio": 1520,
  "serie": "F",
  "uuid": "8F5E924C-71B3-47E5-B9A2-18A3945CD411",
  "total": 35957.68,
  "mensaje": "Documento creado y timbrado exitosamente en CONTPAQi Comercial."
}
```

---

### 3.4. Movimientos de Almacén e Inventario

- `POST /api/EntradasAlmacen`: Registra entradas al inventario afectando costo y existencia.
- `POST /api/SalidasAlmacen`: Registra salidas o mermas de almacén.
- `POST /api/Traspasos`: Realiza transferencias entre almacenes mediante el SDK.

---

## 4. Códigos de Estado y Manejo de Errores

| Código HTTP | Descripción |
| :---: | :--- |
| `200 OK` | Petición procesada satisfactoriamente. |
| `400 Bad Request` | Error de validación en los datos enviados (ej. RFC inválido, producto inexistente). |
| `401 Unauthorized` | Falta o es inválida la cabecera `X-Api-Key` o el token JWT. |
| `429 Too Many Requests` | Se excedió el límite de peticiones por minuto configurado en Rate Limiting. |
| `500 Internal Server Error` | Excepción no controlada o error devuelto por el SDK nativo de CONTPAQi (ej. `Código de error SDK: -102`). |
