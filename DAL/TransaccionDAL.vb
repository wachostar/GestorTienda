Imports System.Data.SQLite

Public Class TransaccionDAL
    
    Public Shared Function GenerarNumeroTransaccion() As String
        Return "TRX-" & DateTime.Now.ToString("yyyyMMddHHmmss")
    End Function
    
    Public Shared Sub Agregar(transaccion As Transaccion)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Dim sql As String = "INSERT INTO Transacciones (NumeroTransaccion, Fecha, ProductoId, SKU, NombreProducto, Cantidad, PrecioUnitario, Total, GananciaTotal, Cambio, MontoPagado) VALUES (@num, @fecha, @pid, @sku, @nom, @cant, @pu, @tot, @gan, @cam, @mon)"
                Using cmd As New SQLiteCommand(sql, conexion, trans)
                    cmd.Parameters.AddWithValue("@num", transaccion.NumeroTransaccion)
                    cmd.Parameters.AddWithValue("@fecha", If(transaccion.Fecha = DateTime.MinValue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), transaccion.Fecha.ToString("yyyy-MM-dd HH:mm:ss")))
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
                trans.Commit()
            End Using
            conexion.Close()
        End Using
    End Sub
    
    Public Shared Function ObtenerPorFecha(fechaInicio As DateTime, fechaFin As DateTime) As List(Of Transaccion)
        Dim transacciones As New List(Of Transaccion)
        
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT Id, NumeroTransaccion, Fecha, ProductoId, SKU, NombreProducto, Cantidad, PrecioUnitario, Total, GananciaTotal, Cambio, MontoPagado FROM Transacciones WHERE DATE(Fecha) BETWEEN @fi AND @ff ORDER BY Fecha DESC", conexion)
            cmd.Parameters.AddWithValue("@fi", fechaInicio.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@ff", fechaFin.ToString("yyyy-MM-dd"))
            Dim reader = cmd.ExecuteReader()
            
            While reader.Read()
                Dim transaccion As New Transaccion With {
                    .Id = CInt(reader("Id")),
                    .NumeroTransaccion = reader("NumeroTransaccion").ToString(),
                    .ProductoId = If(IsDBNull(reader("ProductoId")), 0, CInt(reader("ProductoId"))),
                    .SKU = reader("SKU").ToString(),
                    .NombreProducto = reader("NombreProducto").ToString(),
                    .Cantidad = If(IsDBNull(reader("Cantidad")), 0, CInt(reader("Cantidad"))),
                    .PrecioUnitario = If(IsDBNull(reader("PrecioUnitario")), 0D, CDec(reader("PrecioUnitario"))),
                    .Total = If(IsDBNull(reader("Total")), 0D, CDec(reader("Total"))),
                    .GananciaTotal = If(IsDBNull(reader("GananciaTotal")), 0D, CDec(reader("GananciaTotal"))),
                    .Cambio = If(IsDBNull(reader("Cambio")), 0D, CDec(reader("Cambio"))),
                    .MontoPagado = If(IsDBNull(reader("MontoPagado")), 0D, CDec(reader("MontoPagado"))),
                    .Fecha = If(IsDBNull(reader("Fecha")), DateTime.MinValue, CDate(reader("Fecha")))
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
            If resultado Is DBNull.Value OrElse resultado Is Nothing Then
                Return 0D
            Else
                Return CDec(resultado)
            End If
        End Using
    End Function
    
End Class
