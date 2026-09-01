Imports System.IO
Imports System.Data.SQLite

Public Class ConfiguracionDB
    Private Shared dbFileName As String = "GestorTienda.db"
    ' Default: local app data (safer for installs). Falls back to Application.StartupPath if not available.
    Private Shared rutaBD As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestorTienda", dbFileName)

    Public Shared Function ObtenerConexion() As String
        EnsureDatabase()
        Return $"Data Source={rutaBD};Version=3;"
    End Function

    Public Shared Sub InicializarBD()
        EnsureDatabase()
    End Sub
n    Private Shared Sub EnsureDatabase()
        Try
            Dim dir = Path.GetDirectoryName(rutaBD)
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
n            ' If DB exists in startup path (old location), migrate it to LocalAppData automatically
            Dim legacyPath = Path.Combine(Application.StartupPath, dbFileName)
            If File.Exists(legacyPath) AndAlso Not File.Exists(rutaBD) Then
                File.Copy(legacyPath, rutaBD)
            End If
n            If Not File.Exists(rutaBD) Then
                CrearTablas()
            End If
        Catch ex As Exception
            Throw New Exception("Error al inicializar la base de datos: " & ex.Message)
        End Try
    End Sub

    Private Shared Sub CrearTablas()
        ' Create an empty file first so SQLite can open it from the target path
        Dim dir = Path.GetDirectoryName(rutaBD)
        If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
        Using fs = File.Create(rutaBD)
            fs.Close()
        End Using
n        Using conexion As New SQLiteConnection($"Data Source={rutaBD};Version=3;")
            conexion.Open()
            Using cmd As New SQLiteCommand(conexion)
                ' Enable foreign keys
                cmd.CommandText = "PRAGMA foreign_keys = ON;"
                cmd.ExecuteNonQuery()

                ' Productos
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Productos (Id INTEGER PRIMARY KEY AUTOINCREMENT, SKU TEXT UNIQUE NOT NULL, Nombre TEXT NOT NULL, Descripcion TEXT, Categoria TEXT, CostoU REAL DEFAULT 0, PrecioVenta REAL DEFAULT 0, GananciaU REAL DEFAULT 0, CantidadStock INTEGER DEFAULT 0, FechaCreacion TEXT)"
                cmd.ExecuteNonQuery()

                ' Transacciones
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Transacciones (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroTransaccion TEXT NOT NULL, Fecha TEXT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, PrecioUnitario REAL, Total REAL, GananciaTotal REAL, Cambio REAL, MontoPagado REAL, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                cmd.ExecuteNonQuery()

                ' Compras
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Compras (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroCompra TEXT NOT NULL, Fecha TEXT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, CostoUnitario REAL, CostoTotal REAL, Proveedor TEXT, Estado TEXT, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                cmd.ExecuteNonQuery()

                ' Gastos
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Gastos (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroGasto TEXT NOT NULL, Tipo TEXT, Descripcion TEXT, Monto REAL, Fecha TEXT)"
                cmd.ExecuteNonQuery()

                ' MovimientosInventario
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS MovimientosInventario (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, TipoMovimiento TEXT, Cantidad INTEGER, Observacion TEXT, Fecha TEXT, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                cmd.ExecuteNonQuery()
            End Using
            conexion.Close()
        End Using
    End SubnEnd Class