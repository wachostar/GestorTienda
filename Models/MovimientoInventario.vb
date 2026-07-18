Public Class MovimientoInventario
    Public Property Id As Integer
    Public Property ProductoId As Integer
    Public Property SKU As String
    Public Property NombreProducto As String
    Public Property TipoMovimiento As String
    Public Property Cantidad As Integer
    Public Property StockAnterior As Integer
    Public Property StockNuevo As Integer
    Public Property Motivo As String
    Public Property Fecha As DateTime
End Class