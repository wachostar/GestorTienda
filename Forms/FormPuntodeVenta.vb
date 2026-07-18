Imports System.Drawing

Public Class FormPuntodeVenta
    Inherits Form
    
    Private txtSKU As TextBox
    Private txtCantidad As TextBox
    Private dgvCarrito As DataGridView
    Private lblTotal As Label
    Private lblCambio As Label
    Private txtMontoPagado As TextBox
    Private btnAgregar As Button
    Private btnRemover As Button
    Private btnCobrar As Button
    Private btnLimpiar As Button
    Private carrito As New List(Of Transaccion)
    Private totalCarrito As Decimal = 0
    
    Sub New()
        InitializeComponent()
    End Sub
    
    Private Sub InitializeComponent()
        Me.Text = "💳 Punto de Venta"
        Me.Size = New Size(1000, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 240, 240)
        
        Dim pnlBusqueda As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 100,
            .BackColor = Color.FromArgb(50, 50, 50),
            .Padding = New Padding(20)
        }
        
        Dim lblSKU As New Label With {.Text = "SKU:", .ForeColor = Color.White, .AutoSize = True, .Location = New Point(10, 15)}
        txtSKU = New TextBox With {.Location = New Point(100, 15), .Width = 200}
        
        Dim lblCant As New Label With {.Text = "Cantidad:", .ForeColor = Color.White, .AutoSize = True, .Location = New Point(10, 50)}
        txtCantidad = New TextBox With {.Location = New Point(100, 50), .Width = 200, .Text = "1"}
        
        btnAgregar = New Button With {.Text = "➕ Agregar", .Location = New Point(320, 15), .Width = 120, .BackColor = Color.Green, .ForeColor = Color.White}
        
        pnlBusqueda.Controls.Add(lblSKU)
        pnlBusqueda.Controls.Add(txtSKU)
        pnlBusqueda.Controls.Add(lblCant)
        pnlBusqueda.Controls.Add(txtCantidad)
        pnlBusqueda.Controls.Add(btnAgregar)
        
        Me.Controls.Add(pnlBusqueda)
        
        dgvCarrito = New DataGridView With {
            .Dock = DockStyle.Top,
            .Height = 300,
            .BackgroundColor = Color.White,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }
        dgvCarrito.AllowUserToAddRows = False
        Me.Controls.Add(dgvCarrito)
        
        Dim pnlResumen As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(20)
        }
        
        Dim lblTotalText As New Label With {.Text = "Total:", .Font = New Font("Arial", 14, FontStyle.Bold), .Location = New Point(20, 20)}
        lblTotal = New Label With {.Text = "$0.00", .Font = New Font("Arial", 14, FontStyle.Bold), .ForeColor = Color.Green, .Location = New Point(100, 20)}
        
        Dim lblMonto As New Label With {.Text = "Monto Pagado:", .Location = New Point(20, 60)}
        txtMontoPagado = New TextBox With {.Location = New Point(150, 60), .Width = 150}
        
        Dim lblCambioText As New Label With {.Text = "Cambio:", .Font = New Font("Arial", 12), .Location = New Point(20, 100)}
        lblCambio = New Label With {.Text = "$0.00", .Font = New Font("Arial", 12, FontStyle.Bold), .ForeColor = Color.Blue, .Location = New Point(100, 100)}
        
        btnRemover = New Button With {.Text = "🗑️ Remover", .Location = New Point(330, 20), .Width = 100, .BackColor = Color.Red, .ForeColor = Color.White}
        btnCobrar = New Button With {.Text = "✅ COBRAR", .Location = New Point(450, 20), .Width = 100, .BackColor = Color.Green, .ForeColor = Color.White, .Font = New Font("Arial", 12, FontStyle.Bold)}
        btnLimpiar = New Button With {.Text = "🔄 Limpiar", .Location = New Point(570, 20), .Width = 100, .BackColor = Color.Orange, .ForeColor = Color.White}
        
        pnlResumen.Controls.Add(lblTotalText)
        pnlResumen.Controls.Add(lblTotal)
        pnlResumen.Controls.Add(lblMonto)
        pnlResumen.Controls.Add(txtMontoPagado)
        pnlResumen.Controls.Add(lblCambioText)
        pnlResumen.Controls.Add(lblCambio)
        pnlResumen.Controls.Add(btnRemover)
        pnlResumen.Controls.Add(btnCobrar)
        pnlResumen.Controls.Add(btnLimpiar)
        
        Me.Controls.Add(pnlResumen)
        
        AddHandler btnAgregar.Click, AddressOf AgregarAlCarrito
        AddHandler btnRemover.Click, AddressOf RemoverDelCarrito
        AddHandler btnCobrar.Click, AddressOf RegistrarVenta
        AddHandler btnLimpiar.Click, AddressOf LimpiarCarrito
        AddHandler txtMontoPagado.TextChanged, AddressOf CalcularCambio
    End Sub
    
    Private Sub AgregarAlCarrito(sender As Object, e As EventArgs)
        Dim sku = txtSKU.Text.Trim()
        Dim cantidad = 0
        
        If sku = "" Or Not Integer.TryParse(txtCantidad.Text, cantidad) Then
            MessageBox.Show("Ingresa SKU y cantidad válidos", "Validación")
            Return
        End If
        
        Dim producto = ProductoDAL.ObtenerPorSKU(sku)
        If producto Is Nothing Then
            MessageBox.Show("Producto no encontrado", "Error")
            Return
        End If
        
        If producto.CantidadStock < cantidad Then
            MessageBox.Show("Stock insuficiente", "Error")
            Return
        End If
        
        Dim transaccion As New Transaccion With {
            .NumeroTransaccion = TransaccionDAL.GenerarNumeroTransaccion(),
            .ProductoId = producto.Id,
            .SKU = producto.SKU,
            .NombreProducto = producto.Nombre,
            .Cantidad = cantidad,
            .PrecioUnitario = producto.PrecioVenta,
            .Total = producto.PrecioVenta * cantidad,
            .GananciaTotal = (producto.PrecioVenta - producto.CostoUnitario) * cantidad
        }
        
        carrito.Add(transaccion)
        ActualizarCarrito()
        txtSKU.Clear()
        txtCantidad.Text = "1"
    End Sub
    
    Private Sub ActualizarCarrito()
        dgvCarrito.DataSource = Nothing
        
        Dim tabla As New DataTable()
        tabla.Columns.Add("SKU")
        tabla.Columns.Add("Producto")
        tabla.Columns.Add("Cantidad")
        tabla.Columns.Add("Precio")
        tabla.Columns.Add("Total")
        tabla.Columns.Add("Ganancia")
        
        totalCarrito = 0
        For Each item In carrito
            tabla.Rows.Add(item.SKU, item.NombreProducto, item.Cantidad, item.PrecioUnitario, item.Total, item.GananciaTotal)
            totalCarrito += item.Total
        Next
        
        dgvCarrito.DataSource = tabla
        lblTotal.Text = "$" & totalCarrito.ToString("F2")
    End Sub
    
    Private Sub CalcularCambio(sender As Object, e As EventArgs)
        Dim monto As Decimal = 0
        If Decimal.TryParse(txtMontoPagado.Text, monto) Then
            Dim cambio = monto - totalCarrito
            lblCambio.Text = "$" & cambio.ToString("F2")
        End If
    End Sub
    
    Private Sub RemoverDelCarrito(sender As Object, e As EventArgs)
        If dgvCarrito.SelectedRows.Count > 0 Then
            carrito.RemoveAt(dgvCarrito.SelectedRows(0).Index)
            ActualizarCarrito()
        End If
    End Sub
    
    Private Sub RegistrarVenta(sender As Object, e As EventArgs)
        If carrito.Count = 0 Then
            MessageBox.Show("Carrito vacío", "Validación")
            Return
        End If
        
        Dim monto As Decimal = 0
        If Not Decimal.TryParse(txtMontoPagado.Text, monto) Or monto < totalCarrito Then
            MessageBox.Show("Monto insuficiente", "Error")
            Return
        End If
        
        Try
            For Each item In carrito
                item.MontoPagado = monto
                item.Cambio = monto - totalCarrito
                item.Fecha = DateTime.Now
                
                TransaccionDAL.Agregar(item)
                
                Dim producto = ProductoDAL.ObtenerPorSKU(item.SKU)
                producto.CantidadStock -= item.Cantidad
                ProductoDAL.Actualizar(producto)
            Next
            
            MessageBox.Show("Venta registrada exitosamente", "Éxito")
            LimpiarCarrito(Nothing, Nothing)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub
    
    Private Sub LimpiarCarrito(sender As Object, e As EventArgs)
        carrito.Clear()
        ActualizarCarrito()
        txtMontoPagado.Clear()
        lblCambio.Text = "$0.00"
    End Sub
    
End Class