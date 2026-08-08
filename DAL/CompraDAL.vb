Imports System.Data
Imports System.Data.SQLite
Imports System.Threading
Imports System.Globalization

Public Class CompraDAL

    Public Shared Function ObtenerTodas() As List(Of Compra)
        Dim list As New List(Of Compra)()

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using pragmaCmd As New SQLiteCommand("PRAGMA busy_timeout = 5000;", conexion)
                pragmaCmd.ExecuteNonQuery()
            End Using

            Using cmd As New SQLiteCommand("SELECT Id, NumeroCompra, ProductoId, SKU, NombreProducto, Cantidad, CostoUnitario, CostoTotal, Proveedor, Estado, Fecha FROM Compras ORDER BY Fecha DESC", conexion)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim c As New Compra With {
                            .Id = If(reader.IsDBNull(reader.GetOrdinal("Id")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Id")))),
                            .NumeroCompra = If(reader.IsDBNull(reader.GetOrdinal("NumeroCompra")), String.Empty, reader.GetString(reader.GetOrdinal("NumeroCompra"))),
                            .ProductoId = If(reader.IsDBNull(reader.GetOrdinal("ProductoId")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("ProductoId")))),
                            .SKU = If(reader.IsDBNull(reader.GetOrdinal("SKU")), String.Empty, reader.GetString(reader.GetOrdinal("SKU"))),
                            .NombreProducto = If(reader.IsDBNull(reader.GetOrdinal("NombreProducto")), String.Empty, reader.GetString(reader.GetOrdinal("NombreProducto"))),
                            .Cantidad = If(reader.IsDBNull(reader.GetOrdinal("Cantidad")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Cantidad")))),
                            .CostoUnitario = If(reader.IsDBNull(reader.GetOrdinal("CostoUnitario")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("CostoUnitario")), CultureInfo.InvariantCulture)),
                            .CostoTotal = If(reader.IsDBNull(reader.GetOrdinal("CostoTotal")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("CostoTotal")), CultureInfo.InvariantCulture)),
                            .Proveedor = If(reader.IsDBNull(reader.GetOrdinal("Proveedor")), String.Empty, reader.GetString(reader.GetOrdinal("Proveedor"))),
                            .Estado = If(reader.IsDBNull(reader.GetOrdinal("Estado")), String.Empty, reader.GetString(reader.GetOrdinal("Estado"))),
                            .Fecha = If(reader.IsDBNull(reader.GetOrdinal("Fecha")), DateTime.MinValue, Convert.ToDateTime(reader.GetValue(reader.GetOrdinal("Fecha")), CultureInfo.InvariantCulture))
                        }
                        list.Add(c)
                    End While
                End Using
            End Using
        End Using

        Return list
    End Function

    ' ... other methods omitted for brevity in this upload
End Class
