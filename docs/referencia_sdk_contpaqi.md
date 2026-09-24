# Referencia Técnica del SDK de CONTPAQi® Comercial

Este documento recopila las firmas nativas, modelos de datos y los códigos de error más frecuentes devueltos por las librerías `MGWServicios.dll` y `MGW_SDK.dll`.

---

## 1. Librerías Nativas del SDK

El sistema interactúa con CONTPAQi Comercial mediante llamadas P/Invoke a:
- **`MGWServicios.dll`**: Librería principal para CONTPAQi Comercial Premium (ubicada habitualmente en `C:\Program Files (x86)\Compac\COMERCIAL`).
- **`MGW_SDK.dll`**: Librería para CONTPAQi Factura Electrónica (ubicada habitualmente en `C:\Program Files (x86)\Compacw\Facturacion`).

> **Regla Crítica:** Todas las invocaciones al SDK deben ejecutarse de forma secuencial y sincronizada (gestionado internamente mediante `SemaphoreSlim(1, 1)` en `ContpaqiSdk.cs`) y bajo arquitectura **x86 (32 bits)**.

---

## 2. Códigos de Retorno y Errores Nativos Frecuentes

En las funciones del SDK de CONTPAQi, un valor de retorno `0` indica ejecución exitosa. Cualquier valor diferente de `0` representa un código de error:

| Código de Error | Descripción / Causa | Acción de Resolución |
| :---: | :--- | :--- |
| `0` | **Éxito (SUCCESS)** | Operación completada satisfactoriamente. |
| `1` | **No existe el registro** | El cliente, producto, documento o almacén no existe con ese código. |
| `2` | **Registro ya existe** | Se intentó dar de alta un registro cuyo código ya está en la base de datos. |
| `3` | **Registro bloqueado** | Otro proceso o usuario de CONTPAQi está modificando el registro simultáneamente. |
| `-1` | **Error general de ejecución** | Fallo en la llamada a la función interna del SDK. |
| `-2` | **SDK no inicializado** | No se llamó previamente a `fInicializaSDK` o `fSetNombrePAQ`. |
| `-3` | **Empresa no abierta** | Se intentó una operación de documentos sin haber abierto la empresa (`fAbreEmpresa`). |
| `-4` | **Error de inicio de sesión** | Usuario o contraseña inválidos en `fInicioSesionSDK`. |
| `-101` | **No se encontró el concepto** | El código de concepto (ej. Factura, Compra) no está configurado en CONTPAQi. |
| `-102` | **Falta información obligatoria** | Algún campo requerido en el documento o partida no fue proporcionado. |
| `-103` | **Existencia insuficiente** | El producto controla inventario y la cantidad solicitada excede las existencias del almacén. |
| `-104` | **Límite de crédito excedido** | El cliente superó el límite de crédito configurado y no permite sobregiro. |
| `-1001` | **Error en DLL del PAC / Timbrado** | Falla de conexión con el servicio de timbrado o servidor SAT. |
| `-1002` | **Certificado CSD inválido o vencido** | El CSD configurado en la empresa caducó o la clave privada es incorrecta. |
| `-1004` | **Error en estructura del XML/CFDI** | Falta algún atributo fiscal obligatorio (ej. Clave SAT de producto o unidad, Uso CFDI, Régimen). |

---

## 3. Estructuras de Datos Nativas (Structs P/Invoke)

### 3.1. `tDocumento` (Cabecera de Documentos)
Estructura nativa utilizada para la creación de documentos en `fAltaDocumento`:

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct tDocumento
{
    public double aFolio;
    public int aNumMoneda;
    public double aTipoCambio;
    public double aImporte;
    public double aDescuentoDoc1;
    public double aDescuentoDoc2;
    public int aSistemaOrigen;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
    public string aCodConcepto;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string aSerie;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
    public string aFecha;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string aCodigoCteProv;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string aCodigoAgente;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string aReferencia;
    public int aAfecta;
    public double aGasto1;
    public double aGasto2;
    public double aGasto3;
}
```

### 3.2. `tMovimiento` (Partidas del Documento)
Estructura nativa utilizada en `fAltaMovimiento`:

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct tMovimiento
{
    public int aConsecutivo;
    public double aUnidades;
    public double aPrecio;
    public double aCosto;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string aCodProdSer;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string aCodAlmacen;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string aReferencia;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
    public string aCodClasificacion;
}
```
