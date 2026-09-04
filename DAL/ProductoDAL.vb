Imports System.Data
Imports System.Data.SQLite
Imports System.Threading
Imports System.Globalization

Public Class ProductoDAL

    Public Shared Function ObtenerTodos() As List(Of Producto)
        Dim list As New List(Of Producto)()

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using pragmaCmd As New SQLiteCommand("PRAGMA busy_timeout = 5000;", conexion)
                pragmaCmd.ExecuteNonQuery()
            End Using

            Using cmd As New SQLiteCommand("SELECT Id, SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion FROM Productos ORDER BY Nombre", conexion)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim p As New Producto With {
                            .Id = If(reader.IsDBNull(reader.GetOrdinal("Id")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Id")))),
                            .SKU = If(reader.IsDBNull(reader.GetOrdinal("SKU")), String.Empty, reader.GetString(reader.GetOrdinal("SKU"))),
                            .Nombre = If(reader.IsDBNull(reader.GetOrdinal("Nombre")), String.Empty, reader.GetString(reader.GetOrdinal("Nombre"))),
                            .Descripcion = If(reader.IsDBNull(reader.GetOrdinal("Descripcion")), String.Empty, reader.GetString(reader.GetOrdinal("Descripcion"))),
                            .Categoria = If(reader.IsDBNull(reader.GetOrdinal("Categoria")), String.Empty, reader.GetString(reader.GetOrdinal("Categoria"))),
                            .CostoUnitario = If(reader.IsDBNull(reader.GetOrdinal("CostoU")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("CostoU")), CultureInfo.InvariantCulture)),
                            .PrecioVenta = If(reader.IsDBNull(reader.GetOrdinal("PrecioVenta")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("PrecioVenta")), CultureInfo.InvariantCulture)),
                            .GananciaUnitaria = If(reader.IsDBNull(reader.GetOrdinal("GananciaU")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("GananciaU")), CultureInfo.InvariantCulture)),
                            .CantidadStock = If(reader.IsDBNull(reader.GetOrdinal("CantidadStock")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("CantidadStock")))),
                            .FechaCreacion = If(reader.IsDBNull(reader.GetOrdinal("FechaCreacion")), DateTime.MinValue, Convert.ToDateTime(reader.GetValue(reader.GetOrdinal("FechaCreacion")), CultureInfo.InvariantCulture))
                        }
                        list.Add(p)
                    End While
                End Using
            End Using
        End Using

        Return list
    End Function

    Public Shared Function ObtenerPorSKU(sku As String) As Producto
        If String.IsNullOrWhiteSpace(sku) Then Return Nothing

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using cmd As New SQLiteCommand("SELECT Id, SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion FROM Productos WHERE UPPER(SKU) = @sku LIMIT 1", conexion)
                cmd.Parameters.AddWithValue("@sku", sku.Trim().ToUpperInvariant())
                Using reader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Dim p As New Producto With {
                            .Id = If(reader.IsDBNull(reader.GetOrdinal("Id")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Id")))),
                            .SKU = If(reader.IsDBNull(reader.GetOrdinal("SKU")), String.Empty, reader.GetString(reader.GetOrdinal("SKU"))),
                            .Nombre = If(reader.IsDBNull(reader.GetOrdinal("Nombre")), String.Empty, reader.GetString(reader.GetOrdinal("Nombre"))),
                            .Descripcion = If(reader.IsDBNull(reader.GetOrdinal("Descripcion")), String.Empty, reader.GetString(reader.GetOrdinal("Descripcion"))),
                            .Categoria = If(reader.IsDBNull(reader.GetOrdinal("Categoria")), String.Empty, reader.GetString(reader.GetOrdinal("Categoria"))),
                            .CostoUnitario = If(reader.IsDBNull(reader.GetOrdinal("CostoU")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("CostoU")), CultureInfo.InvariantCulture)),
                            .PrecioVenta = If(reader.IsDBNull(reader.GetOrdinal("PrecioVenta")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("PrecioVenta")), CultureInfo.InvariantCulture)),
                            .GananciaUnitaria = If(reader.IsDBNull(reader.GetOrdinal("GananciaU")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("GananciaU")), CultureInfo.InvariantCulture)),
                            .CantidadStock = If(reader.IsDBNull(reader.GetOrdinal("CantidadStock")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("CantidadStock")))),
                            .FechaCreacion = If(reader.IsDBNull(reader.GetOrdinal("FechaCreacion")), DateTime.MinValue, Convert.ToDateTime(reader.GetValue(reader.GetOrdinal("FechaCreacion")), CultureInfo.InvariantCulture))
                        }
                        Return p
                    End If
                End Using
            End Using
        End Using

        Return Nothing
    End Function

    Public Shared Sub Agregar(producto As Producto)
        If producto Is Nothing Then Throw New ArgumentNullException(NameOf(producto))
        If String.IsNullOrWhiteSpace(producto.SKU) Then Throw New ArgumentException("SKU es obligatorio")

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Try
                    ' Verificar duplicado por SKU
                    Using cmdCheck As New SQLiteCommand("SELECT COUNT(1) FROM Productos WHERE UPPER(SKU) = @sku", conexion, trans)
                        cmdCheck.Parameters.AddWithValue("@sku", producto.SKU.Trim().ToUpperInvariant())
                        Dim count = Convert.ToInt32(cmdCheck.ExecuteScalar())
                        If count > 0 Then
                            Throw New Exception("El SKU ya existe")
                        End If
                    End Using

                    Dim sql As String = "INSERT INTO Productos (SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion) VALUES (@sku, @nombre, @desc, @cat, @costo, @precio, @gan, @stock, @fecha)"
                    Using cmd As New SQLiteCommand(sql, conexion, trans)
                        cmd.Parameters.AddWithValue("@sku", producto.SKU.Trim().ToUpperInvariant())
                        cmd.Parameters.AddWithValue("@nombre", producto.Nombre)
                        cmd.Parameters.AddWithValue("@desc", If(producto.Descripcion, String.Empty))
                        cmd.Parameters.AddWithValue("@cat", If(producto.Categoria, String.Empty))
                        cmd.Parameters.AddWithValue("@costo", producto.CostoUnitario)
                        cmd.Parameters.AddWithValue("@precio", producto.PrecioVenta)
                        cmd.Parameters.AddWithValue("@gan", producto.GananciaUnitaria)
                        cmd.Parameters.AddWithValue("@stock", producto.CantidadStock)
                        cmd.Parameters.AddWithValue("@fecha", If(producto.FechaCreacion = DateTime.MinValue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), producto.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss")))

                        cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                    Logger.Info($"Producto agregado: {producto.SKU}")
                Catch ex As Exception
                    Try
                        trans.Rollback()
                    Catch
                    End Try
                    Logger.Error("Error en ProductoDAL.Agregar: " & ex.ToString())
                    Throw
                End Try
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Sub Actualizar(producto As Producto)
        If producto Is Nothing Then Throw New ArgumentNullException(NameOf(producto))

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Try
                    Dim sql As String = "UPDATE Productos SET SKU = @sku, Nombre = @nombre, Descripcion = @desc, Categoria = @cat, CostoU = @costo, PrecioVenta = @precio, GananciaU = @gan, CantidadStock = @stock WHERE Id = @id"
                    Using cmd As New SQLiteCommand(sql, conexion, trans)
                        cmd.Parameters.AddWithValue("@sku", producto.SKU.Trim().ToUpperInvariant())
                        cmd.Parameters.AddWithValue("@nombre", producto.Nombre)
                        cmd.Parameters.AddWithValue("@desc", If(producto.Descripcion, String.Empty))
                        cmd.Parameters.AddWithValue("@cat", If(producto.Categoria, String.Empty))
                        cmd.Parameters.AddWithValue("@costo", producto.CostoUnitario)
                        cmd.Parameters.AddWithValue("@precio", producto.PrecioVenta)
                        cmd.Parameters.AddWithValue("@gan", producto.GananciaUnitaria)
                        cmd.Parameters.AddWithValue("@stock", producto.CantidadStock)
                        cmd.Parameters.AddWithValue("@id", producto.Id)

                        Dim rows = cmd.ExecuteNonQuery()
                        If rows = 0 Then
                            Throw New Exception($"Producto con Id {producto.Id} no encontrado para actualizar")
                        End If
                    End Using

                    trans.Commit()
                    Logger.Info($"Producto actualizado: {producto.SKU} (Id={producto.Id})")
                Catch ex As Exception
                    Try
                        trans.Rollback()
                    Catch
                    End Try
                    Logger.Error("Error en ProductoDAL.Actualizar: " & ex.ToString())
                    Throw
                End Try
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Sub Eliminar(id As Integer)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Try
                    Using cmd As New SQLiteCommand("DELETE FROM Productos WHERE Id = @id", conexion, trans)
                        cmd.Parameters.AddWithValue("@id", id)
                        Dim rows = cmd.ExecuteNonQuery()
                        If rows = 0 Then
                            Throw New Exception($"Producto con Id {id} no encontrado para eliminar")
                        End If
                    End Using

                    trans.Commit()
                    Logger.Info($"Producto eliminado. Id={id}")
                Catch ex As Exception
                    Try
                        trans.Rollback()
                    Catch
                    End Try
                    Logger.Error("Error en ProductoDAL.Eliminar: " & ex.ToString())
                    Throw
                End Try
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Function ObtenerStockCritico(limite As Integer) As List(Of Producto)
        Dim list As New List(Of Producto)()

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using cmd As New SQLiteCommand("SELECT Id, SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion FROM Productos WHERE CantidadStock <= @limite ORDER BY CantidadStock ASC", conexion)
                cmd.Parameters.AddWithValue("@limite", limite)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim p As New Producto With {
                            .Id = If(reader.IsDBNull(reader.GetOrdinal("Id")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Id")))),
                            .SKU = If(reader.IsDBNull(reader.GetOrdinal("SKU")), String.Empty, reader.GetString(reader.GetOrdinal("SKU"))),
                            .Nombre = If(reader.IsDBNull(reader.GetOrdinal("Nombre")), String.Empty, reader.GetString(reader.GetOrdinal("Nombre"))),
                            .Descripcion = If(reader.IsDBNull(reader.GetOrdinal("Descripcion")), String.Empty, reader.GetString(reader.GetOrdinal("Descripcion"))),
                            .Categoria = If(reader.IsDBNull(reader.GetOrdinal("Categoria")), String.Empty, reader.GetString(reader.GetOrdinal("Categoria"))),
                            .CostoUnitario = If(reader.IsDBNull(reader.GetOrdinal("CostoU")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("CostoU")), CultureInfo.InvariantCulture)),
                            .PrecioVenta = If(reader.IsDBNull(reader.GetOrdinal("PrecioVenta")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("PrecioVenta")), CultureInfo.InvariantCulture)),
                            .GananciaUnitaria = If(reader.IsDBNull(reader.GetOrdinal("GananciaU")), 0D, Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("GananciaU")), CultureInfo.InvariantCulture)),
                            .CantidadStock = If(reader.IsDBNull(reader.GetOrdinal("CantidadStock")), 0, Convert.ToInt32(reader.GetValue(reader.GetOrdinal("CantidadStock")))),
                            .FechaCreacion = If(reader.IsDBNull(reader.GetOrdinal("FechaCreacion")), DateTime.MinValue, Convert.ToDateTime(reader.GetValue(reader.GetOrdinal("FechaCreacion")), CultureInfo.InvariantCulture))
                        }
                        list.Add(p)
                    End While
                End Using
            End Using
        End Using

        Return list
    End Function

End Class
