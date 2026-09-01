Imports System.Data
Imports System.Data.SQLite
Imports System.Threading
Imports System.Globalization

Public Class ProductoDAL

    Public Shared Function ObtenerTodos() As List(Of Producto)
        Dim list As New List(Of Producto)()

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using cmd As New SQLiteCommand("SELECT Id, SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion FROM Productos ORDER BY Nombre COLLATE NOCASE", conexion)
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

        Dim skuNorm As String = sku.Trim().ToUpperInvariant()

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using cmd As New SQLiteCommand("SELECT Id, SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion FROM Productos WHERE UPPER(SKU) = @sku LIMIT 1", conexion)
                cmd.Parameters.AddWithValue("@sku", skuNorm)
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

        ' Normalizar SKU a mayúsculas
        producto.SKU = producto.SKU?.Trim().ToUpperInvariant()

        ' Verificar duplicado
        If ObtenerPorSKU(producto.SKU) IsNot Nothing Then
            Throw New Exception("El SKU ya existe")
        End If

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Dim sql As String = "INSERT INTO Productos (SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion) VALUES (@sku, @nom, @desc, @cat, @costo, @pv, @gan, @stk, @fecha)"
                Using cmd As New SQLiteCommand(sql, conexion, trans)
                    cmd.Parameters.AddWithValue("@sku", producto.SKU)
                    cmd.Parameters.AddWithValue("@nom", producto.Nombre)
                    cmd.Parameters.AddWithValue("@desc", producto.Descripcion)
                    cmd.Parameters.AddWithValue("@cat", producto.Categoria)
                    cmd.Parameters.AddWithValue("@costo", producto.CostoUnitario)
                    cmd.Parameters.AddWithValue("@pv", producto.PrecioVenta)
                    cmd.Parameters.AddWithValue("@gan", producto.GananciaUnitaria)
                    cmd.Parameters.AddWithValue("@stk", producto.CantidadStock)
                    cmd.Parameters.AddWithValue("@fecha", If(producto.FechaCreacion = DateTime.MinValue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), producto.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss")))

                    cmd.ExecuteNonQuery()
                End Using
                trans.Commit()
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Sub Actualizar(producto As Producto)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Dim sql As String = "UPDATE Productos SET Nombre = @nom, Descripcion = @desc, Categoria = @cat, CostoU = @costo, PrecioVenta = @pv, GananciaU = @gan, CantidadStock = @stk WHERE Id = @id"
                Using cmd As New SQLiteCommand(sql, conexion, trans)
                    cmd.Parameters.AddWithValue("@nom", producto.Nombre)
                    cmd.Parameters.AddWithValue("@desc", producto.Descripcion)
                    cmd.Parameters.AddWithValue("@cat", producto.Categoria)
                    cmd.Parameters.AddWithValue("@costo", producto.CostoUnitario)
                    cmd.Parameters.AddWithValue("@pv", producto.PrecioVenta)
                    cmd.Parameters.AddWithValue("@gan", producto.GananciaUnitaria)
                    cmd.Parameters.AddWithValue("@stk", producto.CantidadStock)
                    cmd.Parameters.AddWithValue("@id", producto.Id)

                    cmd.ExecuteNonQuery()
                End Using
                trans.Commit()
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Sub Eliminar(id As Integer)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using cmd As New SQLiteCommand("DELETE FROM Productos WHERE Id = @id", conexion)
                cmd.Parameters.AddWithValue("@id", id)
                cmd.ExecuteNonQuery()
            End Using
            conexion.Close()
        End Using
    End Sub

    Public Shared Function ObtenerStockCritico(threshold As Integer) As List(Of Producto)
        Dim list As New List(Of Producto)()

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using cmd As New SQLiteCommand("SELECT Id, SKU, Nombre, Descripcion, Categoria, CostoU, PrecioVenta, GananciaU, CantidadStock, FechaCreacion FROM Productos WHERE CantidadStock <= @th ORDER BY CantidadStock ASC", conexion)
                cmd.Parameters.AddWithValue("@th", threshold)
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
