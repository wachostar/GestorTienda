Imports System.Data.SQLite

Public Class TransaccionDAL
    
    Public Shared Function GenerarNumeroTransaccion() As String
        Return "TRX-" & DateTime.Now.ToString("yyyyMMddHHmmss")
    End Function
    
    Public Shared Sub Agregar(transaccion As Transaccion)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("INSERT INTO Transacciones (NumeroTransaccion, ProductoId, SKU, NombreProducto, Cantidad, PrecioUnitario, Total, GananciaTotal, Cambio, MontoPagado) VALUES (@num, @pid, @sku, @nom, @cant, @pu, @tot, @gan, @cam, @mon)", conexion)
            
            cmd.Parameters.AddWithValue("@num", transaccion.NumeroTransaccion)
            cmd.Parameters.AddWithValue("@pid", transaccion.ProductoId)
            cmd.Parameters.AddWithValue("@sku", transaccion.SKU)
            cmd.Parameters.AddWithValue("@nom", transaccion.NombreProducto)
            cmd.Parameters.AddWithValue("@cant", transaccion.Cantidad)
            cmd.Parameters.AddWithValue("@pu", transaccion.PrecioUnitario)
            cmd.Parameters.AddWithValue("@tot", transaccion.Total)
            cmd.Parameters.AddWithValue("@gan", transaccion.GananciaTotal)
            cmd.Parameters.AddWithValue("@cam", transaccion.Cambio)
            cmd.Parameters.AddWithValue("@mon", transaccion.MontoPagado)
            
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
    Public Shared Function ObtenerPorFecha(fechaInicio As DateTime, fechaFin As DateTime) As List(Of Transaccion)
        Dim transacciones As New List(Of Transaccion)
        
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Transacciones WHERE DATE(Fecha) BETWEEN @fi AND @ff ORDER BY Fecha DESC", conexion)
            cmd.Parameters.AddWithValue("@fi", fechaInicio.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@ff", fechaFin.ToString("yyyy-MM-dd"))
            Dim reader = cmd.ExecuteReader()
            
            While reader.Read()
                Dim transaccion As New Transaccion With {
                    .Id = CInt(reader("Id")),
                    .NumeroTransaccion = reader("NumeroTransaccion").ToString(),
                    .ProductoId = CInt(reader("ProductoId")),
                    .SKU = reader("SKU").ToString(),
                    .NombreProducto = reader("NombreProducto").ToString(),
                    .Cantidad = CInt(reader("Cantidad")),
                    .PrecioUnitario = CDec(reader("PrecioUnitario")),
                    .Total = CDec(reader("Total")),
                    .GananciaTotal = CDec(reader("GananciaTotal")),
                    .Cambio = CDec(reader("Cambio")),
                    .MontoPagado = CDec(reader("MontoPagado")),
                    .Fecha = CDate(reader("Fecha"))
                }
                transacciones.Add(transaccion)
            End While
        End Using
        
        Return transacciones
    End Function
    
    Public Shared Function ObtenerGananciaPorFecha(fechaInicio As DateTime, fechaFin As DateTime) As Decimal
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT SUM(GananciaTotal) as TotalGanancia FROM Transacciones WHERE DATE(Fecha) BETWEEN @fi AND @ff", conexion)
            cmd.Parameters.AddWithValue("@fi", fechaInicio.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@ff", fechaFin.ToString("yyyy-MM-dd"))
            
            Dim resultado = cmd.ExecuteScalar()
            If resultado Is DBNull.Value Then
                Return 0
            Else
                Return CDec(resultado)
            End If
        End Using
    End Function
    
End Class