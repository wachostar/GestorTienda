Imports System.Data.SQLite

Public Class GastoDAL
    
    Public Shared Function GenerarNumeroGasto() As String
        Return "GST-" & DateTime.Now.ToString("yyyyMMddHHmmss")
    End Function
    
    Public Shared Sub Agregar(gasto As Gasto)
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("INSERT INTO Gastos (NumeroGasto, Tipo, Descripcion, Monto) VALUES (@num, @tipo, @desc, @monto)", conexion)
            
            cmd.Parameters.AddWithValue("@num", gasto.NumeroGasto)
            cmd.Parameters.AddWithValue("@tipo", gasto.Tipo)
            cmd.Parameters.AddWithValue("@desc", If(gasto.Descripcion, ""))
            cmd.Parameters.AddWithValue("@monto", gasto.Monto)
            
            cmd.ExecuteNonQuery()
        End Using
    End Sub
    
    Public Shared Function ObtenerTodos() As List(Of Gasto)
        Dim gastos As New List(Of Gasto)
        
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Gastos ORDER BY Fecha DESC", conexion)
            Dim reader = cmd.ExecuteReader()
            
            While reader.Read()
                Dim gasto As New Gasto With {
                    .Id = CInt(reader("Id")),
                    .NumeroGasto = reader("NumeroGasto").ToString(),
                    .Tipo = reader("Tipo").ToString(),
                    .Descripcion = reader("Descripcion").ToString(),
                    .Monto = CDec(reader("Monto")),
                    .Fecha = CDate(reader("Fecha"))
                }
                gastos.Add(gasto)
            End While
        End Using
        
        Return gastos
    End Function
    
    Public Shared Function ObtenerPorFecha(fechaInicio As DateTime, fechaFin As DateTime) As List(Of Gasto)
        Dim gastos As New List(Of Gasto)
        
        Using conexion As New SQLiteConnection(ConfiguracionDB.ObtenerConexion())
            conexion.Open()
            Dim cmd As New SQLiteCommand("SELECT * FROM Gastos WHERE DATE(Fecha) BETWEEN @fi AND @ff ORDER BY Fecha DESC", conexion)
            cmd.Parameters.AddWithValue("@fi", fechaInicio.ToString("yyyy-MM-dd"))
            cmd.Parameters.AddWithValue("@ff", fechaFin.ToString("yyyy-MM-dd"))
            Dim reader = cmd.ExecuteReader()
            
            While reader.Read()
                Dim gasto As New Gasto With {
                    .Id = CInt(reader("Id")),
                    .NumeroGasto = reader("NumeroGasto").ToString(),
                    .Tipo = reader("Tipo").ToString(),
                    .Descripcion = reader("Descripcion").ToString(),
                    .Monto = CDec(reader("Monto")),
                    .Fecha = CDate(reader("Fecha"))
                }
                gastos.Add(gasto)
            End While
        End Using
        
        Return gastos
    End Function
    
End Class