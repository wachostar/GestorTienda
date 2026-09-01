Imports System.Data
Imports System.Data.SQLite
Imports System.Threading
Imports System.Globalization

Public Class CompraDAL

    Public Shared Function GenerarNumeroCompra() As String
        Return "CMP-" & DateTime.Now.ToString("yyyyMMddHHmmss")
    End Function

    Public Shared Sub Agregar(compra As Compra)
        If compra Is Nothing Then Throw New ArgumentNullException(NameOf(compra))

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Try
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

                    ' Si la compra llega ya como "Recibida", actualizar stock dentro de la misma transacción
                    If Not String.IsNullOrWhiteSpace(compra.Estado) AndAlso compra.Estado = "Recibida" Then
                        Using cmdUpd As New SQLiteCommand("UPDATE Productos SET CantidadStock = CantidadStock + @cant WHERE UPPER(SKU) = @sku", conexion, trans)
                            cmdUpd.Parameters.AddWithValue("@cant", compra.Cantidad)
                            cmdUpd.Parameters.AddWithValue("@sku", compra.SKU.Trim().ToUpperInvariant())
                            Dim rows = cmdUpd.ExecuteNonQuery()
                            If rows = 0 Then
                                ' Producto no existe: rollback y error
                                Throw New Exception($"Producto con SKU {compra.SKU} no encontrado para actualizar stock")
                            End If
                        End Using
                    End If

                    trans.Commit()
                Catch ex As Exception
                    trans.Rollback()
                    Throw
                End Try
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Sub ActualizarEstado(id As Integer, estado As String)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Try
                    ' Obtener compra actual
                    Dim compraActual As Compra = Nothing
                    Using cmdGet As New SQLiteCommand("SELECT Id, NumeroCompra, ProductoId, SKU, NombreProducto, Cantidad, CostoUnitario, CostoTotal, Proveedor, Estado, Fecha FROM Compras WHERE Id = @id LIMIT 1", conexion, trans)
                        cmdGet.Parameters.AddWithValue("@id", id)
                        Using reader = cmdGet.ExecuteReader()
                            If reader.Read() Then
                                compraActual = New Compra With {
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
                            Else
                                Throw New Exception($"Compra con Id {id} no encontrada")
                            End If
                        End Using
                    End Using

                    ' Actualizar estado
                    Using cmdUpd As New SQLiteCommand("UPDATE Compras SET Estado = @est WHERE Id = @id", conexion, trans)
                        cmdUpd.Parameters.AddWithValue("@est", estado)
                        cmdUpd.Parameters.AddWithValue("@id", id)
                        cmdUpd.ExecuteNonQuery()
                    End Using

                    ' Si la transición es a "Recibida" desde otro estado distinto, actualizar stock
                    If estado = "Recibida" AndAlso Not String.IsNullOrWhiteSpace(compraActual.Estado) AndAlso compraActual.Estado <> "Recibida" Then
                        Using cmdStock As New SQLiteCommand("UPDATE Productos SET CantidadStock = CantidadStock + @cant WHERE UPPER(SKU) = @sku", conexion, trans)
                            cmdStock.Parameters.AddWithValue("@cant", compraActual.Cantidad)
                            cmdStock.Parameters.AddWithValue("@sku", compraActual.SKU.Trim().ToUpperInvariant())
                            Dim rows = cmdStock.ExecuteNonQuery()
                            If rows = 0 Then
                                Throw New Exception($"Producto con SKU {compraActual.SKU} no encontrado para actualizar stock")
                            End If
                        End Using
                    End If

                    trans.Commit()
                Catch ex As Exception
                    trans.Rollback()
                    Throw
                End Try
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
