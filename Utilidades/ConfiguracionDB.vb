Imports System.IO
Imports System.Data.SQLite
Imports System.Windows.Forms

Public Class ConfiguracionDB
    Private Shared rutaBD As String = Path.Combine(Application.StartupPath, "GestorTienda.db")

    Public Shared Function ObtenerConexion() As String
        Return $"Data Source={rutaBD};Version=3;"
    End Function

    Public Shared Sub InicializarBD()
        Try
            ' Asegura que la carpeta exista (Application.StartupPath normalmente existe)
            Dim dir As String = Path.GetDirectoryName(rutaBD)
            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            Using conexion As New SQLiteConnection(ObtenerConexion())
                conexion.Open()

                ' Habilitar claves foráneas por si se requieren en el futuro
                Using pragma = New SQLiteCommand("PRAGMA foreign_keys = ON;", conexion)
                    pragma.ExecuteNonQuery()
                End Using

                Using trans = conexion.BeginTransaction()
                    Dim sqlProductos As String = "CREATE TABLE IF NOT EXISTS Productos (Id INTEGER PRIMARY KEY AUTOINCREMENT, SKU TEXT UNIQUE NOT NULL, Nombre TEXT NOT NULL, Descripcion TEXT, Categoria TEXT, CostoU REAL, PrecioVenta REAL, GananciaU REAL, CantidadStock INTEGER)"
                    Using cmd As New SQLiteCommand(sqlProductos, conexion, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    Dim sqlTransacciones As String = "CREATE TABLE IF NOT EXISTS Transacciones (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroTransaccion TEXT UNIQUE NOT NULL, Fecha TEXT NOT NULL, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, PrecioUnitario REAL, Total REAL, Ganancia REAL)"
                    Using cmd As New SQLiteCommand(sqlTransacciones, conexion, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    Dim sqlCompras As String = "CREATE TABLE IF NOT EXISTS Compras (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroCompra TEXT UNIQUE NOT NULL, Fecha TEXT NOT NULL, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, Cantidad INTEGER, CostoUnitario REAL, Total REAL, Proveedor TEXT, Estado TEXT)"
                    Using cmd As New SQLiteCommand(sqlCompras, conexion, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    Dim sqlGastos As String = "CREATE TABLE IF NOT EXISTS Gastos (Id INTEGER PRIMARY KEY AUTOINCREMENT, NumeroGasto TEXT UNIQUE NOT NULL, Fecha TEXT NOT NULL, Tipo TEXT NOT NULL, Descripcion TEXT, Monto REAL NOT NULL)"
                    Using cmd As New SQLiteCommand(sqlGastos, conexion, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    Dim sqlMovimientos As String = "CREATE TABLE IF NOT EXISTS MovimientosInventario (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductoId INTEGER, SKU TEXT, NombreProducto TEXT, TipoMovimiento TEXT, Cantidad INTEGER, Fecha TEXT, Observaciones TEXT)"
                    Using cmd As New SQLiteCommand(sqlMovimientos, conexion, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                End Using

                conexion.Close()
            End Using

        Catch ex As Exception
            MessageBox.Show("Error al inicializar la base de datos: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Throw
        End Try
    End Sub
End Class
