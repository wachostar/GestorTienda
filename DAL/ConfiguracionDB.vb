Imports System.IO
Imports System.Data.SQLite
Imports System.Windows.Forms

Public Class ConfiguracionDB
    Private Shared ReadOnly rutaBD As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GestorTienda", "GestorTienda.db")

    Public Shared Function ObtenerConexion() As String
        Return $"Data Source={rutaBD};Version=3;BusyTimeout=5000;Pooling=True"
    End Function

    Public Shared Sub InicializarBD()
        Try
            Dim dir = Path.GetDirectoryName(rutaBD)
            If Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            If Not File.Exists(rutaBD) Then
                CrearTablas()
            End If
        Catch ex As Exception
            MessageBox.Show($"Error inicializando la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Sub CrearTablas()
        Try
            Using conexion As New SQLiteConnection(ObtenerConexion())
                conexion.Open()

                Using pragma As New SQLiteCommand("PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL; PRAGMA busy_timeout = 5000;", conexion)
                    pragma.ExecuteNonQuery()
                End Using

                Using trans = conexion.BeginTransaction()
                    Using cmd As SQLiteCommand = conexion.CreateCommand()
                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Productos (Id INTEGER PRIMARY KEY AUTOINCREMENT, SKU TEXT UNIQUE NOT NULL, Nombre TEXT NOT NULL, Descripcion TEXT, Categoria TEXT, CostoUnitario REAL NOT NULL, PrecioVenta REAL NOT NULL, GananciaUnitaria REAL, CantidadStock INTEGER DEFAULT 0, FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP);"
                        cmd.ExecuteNonQuery()

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Transacciones (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroTransaccion TEXT UNIQUE NOT NULL, ProductoId INTEGER NOT NULL, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER NOT NULL, PrecioUnitario REAL NOT NULL, Total REAL NOT NULL, GananciaTotal REAL, Cambio REAL, MontoPagado REAL, Fecha DATETIME DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (ProductoId) REFERENCES Productos(Id));"
                        cmd.ExecuteNonQuery()

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Compras (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroCompra TEXT UNIQUE NOT NULL, ProductoId INTEGER NOT NULL, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER NOT NULL, CostoUnitario REAL NOT NULL, CostoTotal REAL NOT NULL, Proveedor TEXT, Estado TEXT DEFAULT 'Pendiente', Fecha DATETIME DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (ProductoId) REFERENCES Productos(Id));"
                        cmd.ExecuteNonQuery()

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Gastos (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroGasto TEXT UNIQUE NOT NULL, Tipo TEXT NOT NULL, Descripcion TEXT, Monto REAL NOT NULL, Fecha DATETIME DEFAULT CURRENT_TIMESTAMP);"
                        cmd.ExecuteNonQuery()

                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS MovimientosInventario (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductoId INTEGER NOT NULL, SKU TEXT, NombreProducto TEXT, TipoMovimiento TEXT NOT NULL, Cantidad INTEGER NOT NULL, StockAnterior INTEGER, StockNuevo INTEGER, Motivo TEXT, Fecha DATETIME DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (ProductoId) REFERENCES Productos(Id));"
                        cmd.ExecuteNonQuery()
                    End Using
                    trans.Commit()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error creando tablas en la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
