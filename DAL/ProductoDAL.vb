Imports System.Data.SQLite

Public Class ProductoDAL
    
    Public Shared Function ObtenerTodos() As List(Of Producto)
        Dim productos As New List(Of Producto)
        
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Productos", conexion)
            Dim reader = cmd.ExecuteReader()
            
            While reader.Read()
                Dim producto As New Producto With {
                    .Id = CInt(reader("Id")),
                    .SKU = reader("SKU").ToString(),
                    .Nombre = reader("Nombre").ToString(),
                    .Descripcion = reader("Descripcion").ToString(),
                    .Categoria = reader("Categoria").ToString(),
                    .CostoUnitario = CDec(reader("CostoUnitario")),
                    .PrecioVenta = CDec(reader("PrecioVenta")),
                    .GananciaUnitaria = CDec(reader("GananciaUnitaria")),
                    .CantidadStock = CInt(reader("CantidadStock")),
                    .FechaCreacion = CDate(reader("FechaCreacion"))
                }
                productos.Add(producto)
            End While
        End Using
        
        Return productos
    End Function
    
    Public Shared Function ObtenerPorSKU(sku As String) As Producto
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Productos WHERE SKU = @sku", conexion)
            cmd.Parameters.AddWithValue("@sku", sku)
            Dim reader = cmd.ExecuteReader()
            
            If reader.Read() Then
                Return New Producto With {
                    .Id = CInt(reader("Id")),
                    .SKU = reader("SKU").ToString(),
                    .Nombre = reader("Nombre").ToString(),
                    .Descripcion = reader("Descripcion").ToString(),
                    .Categoria = reader("Categoria").ToString(),
                    .CostoUnitario = CDec(reader("CostoUnitario")),
                    .PrecioVenta = CDec(reader("PrecioVenta")),
                    .GananciaUnitaria = CDec(reader("GananciaUnitaria")),
                    .CantidadStock = CInt(reader("CantidadStock")),
                    .FechaCreacion = CDate(reader("FechaCreacion"))
                }
            End If
        End Using
        
        Return Nothing
    End Function
    
    Public Shared Sub Agregar(producto As Producto)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("INSERT INTO Productos (SKU, Nombre, Descripcion, Categoria, CostoUnitario, PrecioVenta, GananciaUnitaria, CantidadStock) VALUES (@sku, @nombre, @desc, @cat, @cu, @pv, @gu, @cs)", conexion)
            
            cmd.Parameters.AddWithValue("@sku", producto.SKU)
            cmd.Parameters.AddWithValue("@nombre", producto.Nombre)
            cmd.Parameters.AddWithValue("@desc", If(producto.Descripcion, ""))
            cmd.Parameters.AddWithValue("@cat", If(producto.Categoria, ""))
            cmd.Parameters.AddWithValue("@cu", producto.CostoUnitario)
            cmd.Parameters.AddWithValue("@pv", producto.PrecioVenta)
            cmd.Parameters.AddWithValue("@gu", producto.GananciaUnitaria)
            cmd.Parameters.AddWithValue("@cs", producto.CantidadStock)
            
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
    Public Shared Sub Actualizar(producto As Producto)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("UPDATE Productos SET Nombre=@nombre, Descripcion=@desc, Categoria=@cat, CostoUnitario=@cu, PrecioVenta=@pv, GananciaUnitaria=@gu, CantidadStock=@cs WHERE Id=@id", conexion)
            
            cmd.Parameters.AddWithValue("@nombre", producto.Nombre)
            cmd.Parameters.AddWithValue("@desc", If(producto.Descripcion, ""))
            cmd.Parameters.AddWithValue("@cat", If(producto.Categoria, ""))
            cmd.Parameters.AddWithValue("@cu", producto.CostoUnitario)
            cmd.Parameters.AddWithValue("@pv", producto.PrecioVenta)
            cmd.Parameters.AddWithValue("@gu", producto.GananciaUnitaria)
            cmd.Parameters.AddWithValue("@cs", producto.CantidadStock)
            cmd.Parameters.AddWithValue("@id", producto.Id)
            
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
    Public Shared Sub Eliminar(id As Integer)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("DELETE FROM Productos WHERE Id = @id", conexion)
            cmd.Parameters.AddWithValue("@id", id)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
    Public Shared Function ObtenerStockCritico(limite As Integer) As List(Of Producto)
        Dim productos As New List(Of Producto)
        
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Productos WHERE CantidadStock <= @limite", conexion)
            cmd.Parameters.AddWithValue("@limite", limite)
            Dim reader = cmd.ExecuteReader()
            
            While reader.Read()
                Dim producto As New Producto With {
                    .Id = CInt(reader("Id")),
                    .SKU = reader("SKU").ToString(),
                    .Nombre = reader("Nombre").ToString(),
                    .Descripcion = reader("Descripcion").ToString(),
                    .Categoria = reader("Categoria").ToString(),
                    .CostoUnitario = CDec(reader("CostoUnitario")),
                    .PrecioVenta = CDec(reader("PrecioVenta")),
                    .GananciaUnitaria = CDec(reader("GananciaUnitaria")),
                    .CantidadStock = CInt(reader("CantidadStock")),
                    .FechaCreacion = CDate(reader("FechaCreacion"))
                }
                productos.Add(producto)
            End While
        End Using
        
        Return productos
    End Function
    
End Class