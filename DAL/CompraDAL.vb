Imports System.Data
Imports System.Data.SQLite
Imports System.Threading
Imports System.Globalization

Public Class CompraDAL

    Public Shared Function GenerarNumeroCompra() As String
        Return "CMP-" & DateTime.Now.ToString("yyyyMMddHHmmss")
    End Function

    Public Shared Sub Agregar(compra As Compra)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Dim sql As String = "INSERT INTO Compras (NumeroCompra, Fecha, ProductoId, SKU, NombreProducto, Cantidad, CostoUnitario, CostoTotal, Proveedor, Estado) VALUES (@num, @fecha, @pid, @sku, @nom, @cant, @costo, @total, @prov, @est)"
                Using cmd As New SQLiteCommand(sql, conexion, trans)
                    cmd.Parameters.AddWithValue("@num", compra.NumeroCompra)
                    cmd.Parameters.AddWithValue("@fecha", If(compra.Fecha = DateTime.MinValue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), compra.Fecha.ToString("yyyy-MM-dd HH:mm:ss")))
                    cmd.Parameters.AddWithValue("@pid", compra.ProductoId)
                    cmd.Parameters.AddWithValue("@sku", compra.SKU)
                    cmd.Parameters.AddWithValue("@nom", compra.NombreProducto)
                    cmd.Parameters.AddWithValue("@cant", compra.Cantidad)
                    cmd.Parameters.AddWithValue("@costo", compra.CostoUnitario)
                    cmd.Parameters.AddWithValue("@total", compra.CostoTotal)
                    cmd.Parameters.AddWithValue("@prov", compra.Proveedor)
                    cmd.Parameters.AddWithValue("@est", compra.Estado)

                    cmd.ExecuteNonQuery()
                End Using
                trans.Commit()
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Sub ActualizarEstado(id As Integer, estado As String)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using cmd As New SQLiteCommand("UPDATE Compras SET Estado = @est WHERE Id = @id", conexion)
                cmd.Parameters.AddWithValue("@est", estado)
                cmd.Parameters.AddWithValue("@id", id)
                cmd.ExecuteNonQuery()
            End Using
            conexion.Close()
        End Using
    End Sub

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

End Class
