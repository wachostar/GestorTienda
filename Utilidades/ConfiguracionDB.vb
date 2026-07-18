Imports System.IO
Imports System.Data.SQLite

Public Class ConfiguracionDB
    Private Shared rutaBD As String = Path.Combine(Application.StartupPath, "GestorTienda.db")
    
    Public Shared Function ObtenerConexion() As String
        Return $"Data Source={rutaBD};Version=3;"
    End Function
    
    Public Shared Sub InicializarBD()
        If Not File.Exists(rutaBD) Then
            CrearTablas()
        End If
    End Sub
    
    Private Shared Sub CrearTablas()
        Using conexion As New SQLiteConnection(ObtenerConexion())
            conexion.Open()
            
            Dim cmdProductos As New SQLiteCommand("CREATE TABLE Productos (Id INTEGER PRIMARY KEY AUTOINCREMENT,SKU TEXT UNIQUE NOT NULL,Nombre TEXT NOT NULL,Descripcion TEXT,Categoria TEXT,CostoUnitario REAL NOT NULL,PrecioVenta REAL NOT NULL,GananciaUnitaria REAL,CantidadStock INTEGER DEFAULT 0,FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP)", conexion)
            cmdProductos.ExecuteNonQuery()
            
            Dim cmdTransacciones As New SQLiteCommand("CREATE TABLE Transacciones (Id INTEGER PRIMARY KEY AUTOINCREMENT,NumeroTransaccion TEXT UNIQUE NOT NULL,ProductoId INTEGER NOT NULL,SKU TEXT,NombreProducto TEXT,Cantidad INTEGER NOT NULL,PrecioUnitario REAL NOT NULL,Total REAL NOT NULL,GananciaTotal REAL,Cambio REAL,MontoPagado REAL,Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY (ProductoId) REFERENCES Productos(Id))", conexion)
            cmdTransacciones.ExecuteNonQuery()
            
            Dim cmdCompras As New SQLiteCommand("CREATE TABLE Compras (Id INTEGER PRIMARY KEY AUTOINCREMENT,NumeroCompra TEXT UNIQUE NOT NULL,ProductoId INTEGER NOT NULL,SKU TEXT,NombreProducto TEXT,Cantidad INTEGER NOT NULL,CostoUnitario REAL NOT NULL,CostoTotal REAL NOT NULL,Proveedor TEXT,Estado TEXT DEFAULT 'Pendiente',Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY (ProductoId) REFERENCES Productos(Id))", conexion)
            cmdCompras.ExecuteNonQuery()
            
            Dim cmdGastos As New SQLiteCommand("CREATE TABLE Gastos (Id INTEGER PRIMARY KEY AUTOINCREMENT,NumeroGasto TEXT UNIQUE NOT NULL,Tipo TEXT NOT NULL,Descripcion TEXT,Monto REAL NOT NULL,Fecha DATETIME DEFAULT CURRENT_TIMESTAMP)", conexion)
            cmdGastos.ExecuteNonQuery()
            
            Dim cmdMovimientos As New SQLiteCommand("CREATE TABLE MovimientosInventario (Id INTEGER PRIMARY KEY AUTOINCREMENT,ProductoId INTEGER NOT NULL,SKU TEXT,NombreProducto TEXT,TipoMovimiento TEXT NOT NULL,Cantidad INTEGER NOT NULL,StockAnterior INTEGER,StockNuevo INTEGER,Motivo TEXT,Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,FOREIGN KEY (ProductoId) REFERENCES Productos(Id))", conexion)
            cmdMovimientos.ExecuteNonQuery()
            
            conexion.Close()
        End Using
    End Sub
End Class