Public Class Producto
    Public Property Id As Integer
    Public Property SKU As String
    Public Property Nombre As String
    Public Property Descripcion As String
    Public Property Categoria As String
    Public Property CostoUnitario As Decimal
    Public Property PrecioVenta As Decimal
    Public Property GananciaUnitaria As Decimal
    Public Property CantidadStock As Integer
    Public Property FechaCreacion As DateTime
    
    Public Sub CalcularGanancia()
        GananciaUnitaria = PrecioVenta - CostoUnitario
    End Sub
End Class