Imports System.IO
Imports System.Data.SQLite

Public Class ConfiguracionDB
    Private Shared ReadOnly dbFileName As String = "GestorTienda.db"
    Private Shared ReadOnly rutaBD As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestorTienda", dbFileName)

    Public Shared Function ObtenerConexion() As String
        EnsureDatabase()
        Return $"Data Source={rutaBD};Version=3;"
    End Function

    Public Shared Sub InicializarBD()
        Try
            EnsureDatabase()
        Catch ex As Exception
            ' Registrar error y continuar para que la UI no se bloquee en el arranque
            Logger.Error("Error en InicializarBD: " & ex.ToString())
        End Try
    End Sub

    Private Shared Sub EnsureDatabase()
        Try
            Dim dir = Path.GetDirectoryName(rutaBD)
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)

            ' Ruta legacy: directorio de la aplicación (base directory)
            Dim legacyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName)

            ' Si existe legacy DB y no existe la nueva, hacer backup y copiarla
            If File.Exists(legacyPath) AndAlso Not File.Exists(rutaBD) Then
                Try
                    Dim backupDir = Path.Combine(dir, "migrations")
                    If Not Directory.Exists(backupDir) Then Directory.CreateDirectory(backupDir)
                    Dim bakName = Path.Combine(backupDir, $"GestorTienda_legacy_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.db")
                    File.Copy(legacyPath, bakName)
                    Logger.Info($"Legacy DB backed up to: {bakName}")

                    File.Copy(legacyPath, rutaBD)
                    Logger.Info($"Legacy DB copied from {legacyPath} to {rutaBD}")
                Catch ex As Exception
                    Logger.Error("Error al migrar legacy DB: " & ex.ToString())
                    ' Intentar continuar creando una BD nueva
                End Try
            End If

            If Not File.Exists(rutaBD) Then
                CrearTablas()
            End If
        Catch ex As Exception
            Logger.Error("Error al inicializar la base de datos: " & ex.ToString())
            Throw
        End Try
    End Sub

    Private Shared Sub CrearTablas()
        Dim dir = Path.GetDirectoryName(rutaBD)
        If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)

        ' Crear archivo vacío para que SQLite lo abra correctamente
        Using fs = File.Create(rutaBD)
            fs.Close()
        End Using

        Using conexion As New SQLiteConnection($"Data Source={rutaBD};Version=3;")
            conexion.Open()
            Using cmd As New SQLiteCommand(conexion)
                ' Habilitar foreign keys
                cmd.CommandText = "PRAGMA foreign_keys = ON;"
                cmd.ExecuteNonQuery()

                ' Tabla Productos
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Productos (Id INTEGER PRIMARY KEY AUTOINCREMENT, SKU TEXT UNIQUE NOT NULL, Nombre TEXT NOT NULL, Descripcion TEXT, Categoria TEXT, CostoU REAL DEFAULT 0, PrecioVenta REAL DEFAULT 0, GananciaU REAL DEFAULT 0, CantidadStock INTEGER DEFAULT 0, FechaCreacion TEXT)"
                cmd.ExecuteNonQuery()

                ' Tabla Transacciones
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Transacciones (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroTransaccion TEXT NOT NULL, Fecha TEXT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, PrecioUnitario REAL, Total REAL, GananciaTotal REAL, Cambio REAL, MontoPagado REAL, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                cmd.ExecuteNonQuery()

                ' Tabla Compras
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Compras (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroCompra TEXT NOT NULL, Fecha TEXT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, CostoUnitario REAL, CostoTotal REAL, Proveedor TEXT, Estado TEXT, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                cmd.ExecuteNonQuery()

                ' Tabla Gastos
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Gastos (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroGasto TEXT NOT NULL, Tipo TEXT, Descripcion TEXT, Monto REAL, Fecha TEXT)"
                cmd.ExecuteNonQuery()

                ' Tabla MovimientosInventario
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS MovimientosInventario (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, TipoMovimiento TEXT, Cantidad INTEGER, Observacion TEXT, Fecha TEXT, FOREIGN KEY(ProductoId) REFERENCES Productos(Id))"
                cmd.ExecuteNonQuery()
            End Using
            conexion.Close()
        End Using
    End Sub
End Class
