Imports System.Data.SQLite

Public Class CompraDAL
    
    Public Shared Function GenerarNumeroCompra() As String
        Return "CMP-" & DateTime.Now.ToString("yyyyMMddHHmmss")
    End Function
    
    Public Shared Sub Agregar(compra As Compra)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("INSERT INTO Compras (NumeroCompra, ProductoId, SKU, NombreProducto, Cantidad, CostoUnitario, CostoTotal, Proveedor, Estado) VALUES (@num, @pid, @sku, @nom, @cant, @cu, @tot, @prov, @est)", conexion)
            
            cmd.Parameters.AddWithValue("@num", compra.NumeroCompra)
            cmd.Parameters.AddWithValue("@pid", compra.ProductoId)
            cmd.Parameters.AddWithValue("@sku", compra.SKU)
            cmd.Parameters.AddWithValue("@nom", compra.NombreProducto)
            cmd.Parameters.AddWithValue("@cant", compra.Cantidad)
            cmd.Parameters.AddWithValue("@cu", compra.CostoUnitario)
            cmd.Parameters.AddWithValue("@tot", compra.CostoTotal)
            cmd.Parameters.AddWithValue("@prov", If(compra.Proveedor, ""))
            cmd.Parameters.AddWithValue("@est", compra.Estado)
            
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
    Public Shared Function ObtenerTodas() As List(Of Compra)
        Dim compras As New List(Of Compra)
        
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Compras ORDER BY Fecha DESC", conexion)
            Dim reader = cmd.ExecuteReader()
            
            While reader.Read()
                Dim compra As New Compra With {
                    .Id = CInt(reader("Id")),
                    .NumeroCompra = reader("NumeroCompra").ToString(),
                    .ProductoId = CInt(reader("ProductoId")),
                    .SKU = reader("SKU").ToString(),
                    .NombreProducto = reader("NombreProducto").ToString(),
                    .Cantidad = CInt(reader("Cantidad")),
                    .CostoUnitario = CDec(reader("CostoUnitario")),
                    .CostoTotal = CDec(reader("CostoTotal")),
                    .Proveedor = reader("Proveedor").ToString(),
                    .Estado = reader("Estado").ToString(),
                    .Fecha = CDate(reader("Fecha"))
                }
                compras.Add(compra)
            End While
        End Using
        
        Return compras
    End Function
    
    Public Shared Sub ActualizarEstado(id As Integer, nuevoEstado As String)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("UPDATE Compras SET Estado = @est WHERE Id = @id", conexion)
            cmd.Parameters.AddWithValue("@est", nuevoEstado)
            cmd.Parameters.AddWithValue("@id", id)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
End Class