Imports System.Data
Imports System.Data.SQLite
Imports System.Threading
Imports System.Globalization

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

    ' Inserta varias transacciones (un solo ticket) y actualiza stocks de forma atómica
    Public Shared Sub RegistrarVentaCompleta(items As List(Of Transaccion), montoPagado As Decimal)
        If items Is Nothing OrElse items.Count = 0 Then Throw New ArgumentException("No hay items para registrar")

        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Using trans = conexion.BeginTransaction()
                Try
                    ' Usar un mismo número de transacción para todo el ticket
                    Dim numeroTicket As String = GenerarNumeroTransaccion()
                    Dim fechaAhora As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

                    For Each item In items
                        ' Verificar stock actual
                        Using cmdCheck As New SQLiteCommand("SELECT CantidadStock FROM Productos WHERE UPPER(SKU) = @sku", conexion, trans)
                            cmdCheck.Parameters.AddWithValue("@sku", item.SKU.Trim().ToUpperInvariant())
                            Dim current = cmdCheck.ExecuteScalar()
                            If current Is Nothing OrElse current Is DBNull.Value Then
                                Throw New Exception($"Producto con SKU {item.SKU} no encontrado en stock")
                            End If
                            Dim stockActual As Integer = Convert.ToInt32(current)
                            If stockActual < item.Cantidad Then
                                Throw New Exception($"Stock insuficiente para SKU {item.SKU}. Disponible: {stockActual}, requerido: {item.Cantidad}")
                            End If
                        End Using

                        ' Insertar transacción (detalle)
                        Dim sqlInsert As String = "INSERT INTO Transacciones (NumeroTransaccion, Fecha, ProductoId, SKU, NombreProducto, Cantidad, PrecioUnitario, Total, GananciaTotal, Cambio, MontoPagado) VALUES (@num, @fecha, @pid, @sku, @nom, @cant, @pu, @tot, @gan, @cam, @mon)"
                        Using cmdIns As New SQLiteCommand(sqlInsert, conexion, trans)
                            cmdIns.Parameters.AddWithValue("@num", numeroTicket)
                            cmdIns.Parameters.AddWithValue("@fecha", If(item.Fecha = DateTime.MinValue, fechaAhora, item.Fecha.ToString("yyyy-MM-dd HH:mm:ss")))
                            cmdIns.Parameters.AddWithValue("@pid", item.ProductoId)
                            cmdIns.Parameters.AddWithValue("@sku", item.SKU)
                            cmdIns.Parameters.AddWithValue("@nom", item.NombreProducto)
                            cmdIns.Parameters.AddWithValue("@cant", item.Cantidad)
                            cmdIns.Parameters.AddWithValue("@pu", item.PrecioUnitario)
                            cmdIns.Parameters.AddWithValue("@tot", item.Total)
                            cmdIns.Parameters.AddWithValue("@gan", item.GananciaTotal)
                            cmdIns.Parameters.AddWithValue("@cam", montoPagado - items.Sum(Function(i) i.Total))
                            cmdIns.Parameters.AddWithValue("@mon", montoPagado)

                            cmdIns.ExecuteNonQuery()
                        End Using

                        ' Actualizar stock
                        Using cmdUpd As New SQLiteCommand("UPDATE Productos SET CantidadStock = CantidadStock - @cant WHERE UPPER(SKU) = @sku", conexion, trans)
                            cmdUpd.Parameters.AddWithValue("@cant", item.Cantidad)
                            cmdUpd.Parameters.AddWithValue("@sku", item.SKU.Trim().ToUpperInvariant())
                            cmdUpd.ExecuteNonQuery()
                        End Using
                    Next

                    trans.Commit()
                Catch ex As Exception
                    trans.Rollback()
                    Throw
                End Try
            End Using
            conexion.Close()
        End Using
    End Sub

End Class
