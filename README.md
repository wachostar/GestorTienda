# 📦 GESTOR DE TIENDA - ABARROTES

## Sistema de Gestión de Inventario y Punto de Venta para Escritorio

Aplicación de escritorio en VB.NET para gestionar inventario, ventas y gastos de una tienda de abarrotes.

### ✨ Características

#### 🛒 Búsqueda Rápida por SKU
- Pantalla de cobro moderna (Morado/Negro)
- Cálculo automático de cambio
- Actualización automática de stock
- Transacciones numeradas

#### 📦 Gestión de Productos
- SKU único por producto
- C.U (Costo Unitario) - Precio de Compra
- P.V (Precio de Venta)
- G.U (Ganancia Unitaria)
- C.S (Cantidad Stock)
- Categorías y descripción
- CRUD completo (Agregar, Modificar, Eliminar)

#### 📊 Reportes Avanzados
- Ventas por Día (rango de fechas)
- Ventas por Artículo (rango de fechas)
- G.B (Ganancia Bruta)
- Productos más/menos vendidos
- Bajo stock crítico
- Resumen financiero

#### 📥 Compras a Proveedores
- Registro de compras
- Detalles por artículo
- Control de estado (Pendiente, Recibida, Rechazada)

#### 💸 Control de Gastos
- G.S (Gastos de Servicios)
- Tipos: Luz, Agua, Alquiler, Internet, Mantenimiento, Otro
- Pagos de servicios
- Reportes por tipo

#### 📈 Control de Inventario
- Stock en tiempo real
- Historial de movimientos
- Alertas de bajo stock
- Valor total del inventario
- Agregar, Modificar, Eliminar Productos

---

## 🚀 Instalación y Configuración

### Requisitos
- .NET 9.0 (net9.0-windows) o superior
- Visual Studio 2022/2023 (opcional pero recomendado)

### Pasos de Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/wachostar/GestorTienda.git
   cd GestorTienda
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

3. **Compilar el proyecto**
   ```bash
   dotnet build
   ```

4. **Ejecutar la aplicación**
   ```bash
   dotnet run --project GestorTienda.vbproj
   ```

---

## 📁 Estructura del Proyecto

```
GestorTienda/
├── Models/                    # Clases de modelos
│   ├── Producto.vb
│   ├── Transaccion.vb
│   ├── Compra.vb
│   ├── Gasto.vb
│   └── MovimientoInventario.vb
├── DAL/                       # Data Access Layer
│   ├── ProductoDAL.vb
│   ├── TransaccionDAL.vb
│   ├── CompraDAL.vb
│   └── GastoDAL.vb
├── Forms/                     # Formularios principales
│   ├── FormPrincipal.vb
│   ├── FormProductos.vb
│   ├── FormPuntodeVenta.vb
│   ├── FormReportes.vb
│   ├── FormCompras.vb
│   ├── FormGastos.vb
│   └── FormInventario.vb
├── Utilidades/
│   └── ConfiguracionDB.vb
├── GestorTienda.vbproj
└── README.md
```

---

## 💾 Base de Datos

La aplicación utiliza **SQLite** para almacenar los datos. La base de datos se crea automáticamente la primera vez que se ejecuta la aplicación y se guarda como `GestorTienda.db` en la carpeta donde se ejecuta la aplicación (Application.StartupPath).

### Tablas
- **Productos**: Información de productos
- **Transacciones**: Registro de ventas (una fila por producto vendido en una transacción)
- **Compras**: Compras a proveedores
- **Gastos**: Control de gastos
- **MovimientosInventario**: Historial de movimientos

---

## 🎮 Cómo Usar

### 1. Agregar Productos
1. Click en "📦 PRODUCTOS"
2. Ingresa SKU, Nombre, Categoría
3. Define Costo Unitario y Precio de Venta
4. Establece cantidad de stock
5. Click en "➕ Agregar"

### 2. Realizar Ventas
1. Click en "💳 PUNTO DE VENTA"
2. Ingresa SKU del producto
3. Define cantidad
4. Click en "➕ Agregar"
5. Ingresa monto pagado
6. Click en "✅ COBRAR"

### 3. Registrar Compras
1. Click en "📥 COMPRAS"
2. Ingresa SKU, cantidad, costo unitario
3. Selecciona proveedor
4. Define estado
5. Click en "➕ Agregar"

### 4. Ver Reportes
1. Click en "📊 REPORTES"
2. Selecciona tipo de reporte
3. Define rango de fechas
4. Click en "📈 Generar"

---

## 📝 Notas Importantes

- Los SKU son **únicos** (no se pueden repetir)
- El stock se **actualiza automáticamente** al realizar ventas
- La ganancia se calcula automáticamente como: **Precio Venta - Costo Unitario**
- Los productos con stock ≤ 10 se marcan como **stock crítico**
- Todos los números de transacción, compra y gasto son **únicos y numerados**

---

## 🤝 Contribuir

Si encuentras bugs o tienes sugerencias, no dudes en abrir un issue o hacer un pull request.

---

## 📄 Licencia

Este proyecto está disponible bajo la licencia MIT.

---

**Autor**: wachostar  
**Fecha**: 2026
