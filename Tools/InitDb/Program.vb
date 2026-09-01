Imports System
Imports System.IO
Imports Microsoft.Data.Sqlite

Module Program
    Sub Main()
        Try
            Dim appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            Dim dir = Path.Combine(appData, "GestorTienda")
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)

            Dim dbPath = Path.Combine(dir, "GestorTienda.db")

            Console.WriteLine("Inicializando base de datos en: " & dbPath)

            Dim needCreate As Boolean = Not File.Exists(dbPath)

            If needCreate Then
                ' Create empty file
                Using fs = File.Create(dbPath)
                    fs.Close()
                End Using
            End If

            Dim cs = $"Data Source={dbPath};"
            Using conn As New SqliteConnection(cs)
                conn.Open()

                Using cmd As SqliteCommand = conn.CreateCommand()
                    cmd.CommandText = "PRAGMA foreign_keys = ON;"
                    cmd.ExecuteNonQuery()

                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS Productos (Id INTEGER PRIMARY KEY AUTOINCREMENT, SKU TEXT UNIQUE NOT NULL, Nombre TEXT NOT NULL, Descripcion TEXT, Categoria TEXT, CostoU REAL DEFAULT 0, PrecioVenta REAL DEFAULT 0, GananciaU REAL DEFAULT 0, CantidadStock INTEGER DEFAULT 0, FechaCreacion TEXT)"
                    cmd.ExecuteNonQuery()

                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS Transacciones (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroTransaccion TEXT NOT NULL, Fecha TEXT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, PrecioUnitario REAL, Total REAL, GananciaTotal REAL, Cambio REAL, MontoPagado REAL, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                    cmd.ExecuteNonQuery()

                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS Compras (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroCompra TEXT NOT NULL, Fecha TEXT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, CostoUnitario REAL, CostoTotal REAL, Proveedor TEXT, Estado TEXT, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                    cmd.ExecuteNonQuery()

                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS Gastos (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroGasto TEXT NOT NULL, Tipo TEXT, Descripcion TEXT, Monto REAL, Fecha TEXT)"
                    cmd.ExecuteNonQuery()

                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS MovimientosInventario (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, TipoMovimiento TEXT, Cantidad INTEGER, Observacion TEXT, Fecha TEXT, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                    cmd.ExecuteNonQuery()
                End Using

                conn.Close()
            End Using

            Console.WriteLine("Inicialización completada.")
            Environment.Exit(0)
        Catch ex As Exception
            Console.Error.WriteLine("Error inicializando DB: " & ex.Message)
            Environment.Exit(2)
        End Try
    End Sub
End Module
