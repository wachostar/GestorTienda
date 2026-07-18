Imports System.Drawing
Imports System.Data.SQLite

Public Class FormProductos
    Inherits Form
    
    Private dgvProductos As DataGridView
    Private txtSKU As TextBox
    Private txtNombre As TextBox
    Private txtDescripcion As TextBox
    Private txtCategoria As TextBox
    Private txtCostoUnitario As TextBox
    Private txtPrecioVenta As TextBox
    Private txtCantidad As TextBox
    Private btnAgregar As Button
    Private btnActualizar As Button
    Private btnEliminar As Button
    Private btnLimpiar As Button
    Private productoSeleccionado As Producto
    
    Sub New()
        InitializeComponent()
        CargarProductos()
    End Sub
    
    Private Sub InitializeComponent()
        Me.Text = "Gestión de Productos"
        Me.Size = New Size(1200, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 240, 240)
        
        dgvProductos = New DataGridView With {
            .Dock = DockStyle.Top,
            .Height = 250,
            .BackgroundColor = Color.White,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }
        dgvProductos.AllowUserToAddRows = False
        dgvProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        AddHandler dgvProductos.CellClick, AddressOf dgvProductos_CellClick
        Me.Controls.Add(dgvProductos)
        
        Dim pnlEntrada As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(20)
        }
        
        Dim lblSKU As New Label With {.Text = "SKU:", .Location = New Point(10, 10), .AutoSize = True}
        txtSKU = New TextBox With {.Location = New Point(120, 10), .Width = 150}
        
        Dim lblNombre As New Label With {.Text = "Nombre:", .Location = New Point(10, 40), .AutoSize = True}
        txtNombre = New TextBox With {.Location = New Point(120, 40), .Width = 150}
        
        Dim lblDescripcion As New Label With {.Text = "Descripción:", .Location = New Point(10, 70), .AutoSize = True}
        txtDescripcion = New TextBox With {.Location = New Point(120, 70), .Width = 150}
        
        Dim lblCategoria As New Label With {.Text = "Categoría:", .Location = New Point(300, 10), .AutoSize = True}
        txtCategoria = New TextBox With {.Location = New Point(410, 10), .Width = 150}
        
        Dim lblCosto As New Label With {.Text = "C.U (Costo):", .Location = New Point(300, 40), .AutoSize = True}
        txtCostoUnitario = New TextBox With {.Location = New Point(410, 40), .Width = 150}
        
        Dim lblPrecio As New Label With {.Text = "P.V (Venta):", .Location = New Point(300, 70), .AutoSize = True}
        txtPrecioVenta = New TextBox With {.Location = New Point(410, 70), .Width = 150}
        
        Dim lblCantidad As New Label With {.Text = "C.S (Stock):", .Location = New Point(600, 10), .AutoSize = True}
        txtCantidad = New TextBox With {.Location = New Point(710, 10), .Width = 150}
        
        btnAgregar = New Button With {.Text = "➕ Agregar", .Location = New Point(600, 40), .Width = 100, .BackColor = Color.Green, .ForeColor = Color.White}
        btnActualizar = New Button With {.Text = "✏️ Actualizar", .Location = New Point(710, 40), .Width = 100, .BackColor = Color.Blue, .ForeColor = Color.White}
        btnEliminar = New Button With {.Text = "🗑️ Eliminar", .Location = New Point(820, 40), .Width = 100, .BackColor = Color.Red, .ForeColor = Color.White}
        btnLimpiar = New Button With {.Text = "🔄 Limpiar", .Location = New Point(930, 40), .Width = 100, .BackColor = Color.Orange, .ForeColor = Color.White}
        
        pnlEntrada.Controls.Add(lblSKU)
        pnlEntrada.Controls.Add(txtSKU)
        pnlEntrada.Controls.Add(lblNombre)
        pnlEntrada.Controls.Add(txtNombre)
        pnlEntrada.Controls.Add(lblDescripcion)
        pnlEntrada.Controls.Add(txtDescripcion)
        pnlEntrada.Controls.Add(lblCategoria)
        pnlEntrada.Controls.Add(txtCategoria)
        pnlEntrada.Controls.Add(lblCosto)
        pnlEntrada.Controls.Add(txtCostoUnitario)
        pnlEntrada.Controls.Add(lblPrecio)
        pnlEntrada.Controls.Add(txtPrecioVenta)
        pnlEntrada.Controls.Add(lblCantidad)
        pnlEntrada.Controls.Add(txtCantidad)
        pnlEntrada.Controls.Add(btnAgregar)
        pnlEntrada.Controls.Add(btnActualizar)
        pnlEntrada.Controls.Add(btnEliminar)
        pnlEntrada.Controls.Add(btnLimpiar)
        
        Me.Controls.Add(pnlEntrada)
        
        AddHandler btnAgregar.Click, AddressOf AgregarProducto
        AddHandler btnActualizar.Click, AddressOf ActualizarProducto
        AddHandler btnEliminar.Click, AddressOf EliminarProducto
        AddHandler btnLimpiar.Click, AddressOf LimpiarFormulario
    End Sub
    
    Private Sub CargarProductos()
        dgvProductos.DataSource = Nothing
        Dim productos = ProductoDAL.ObtenerTodos()
        
        Dim tabla As New DataTable()
        tabla.Columns.Add("ID")
        tabla.Columns.Add("SKU")
        tabla.Columns.Add("Nombre")
        tabla.Columns.Add("Categoría")
        tabla.Columns.Add("C.U")
        tabla.Columns.Add("P.V")
        tabla.Columns.Add("G.U")
        tabla.Columns.Add("Stock")
        
        For Each producto In productos
            tabla.Rows.Add(producto.Id, producto.SKU, producto.Nombre, producto.Categoria, 
                          producto.CostoUnitario, producto.PrecioVenta, producto.GananciaUnitaria, producto.CantidadStock)
        Next
        
        dgvProductos.DataSource = tabla
    End Sub
    
    Private Sub dgvProductos_CellClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex >= 0 Then
            Dim fila = dgvProductos.Rows(e.RowIndex)
            txtSKU.Text = fila.Cells("SKU").Value.ToString()
            txtNombre.Text = fila.Cells("Nombre").Value.ToString()
            txtCategoria.Text = fila.Cells("Categoría").Value.ToString()
            txtCostoUnitario.Text = fila.Cells("C.U").Value.ToString()
            txtPrecioVenta.Text = fila.Cells("P.V").Value.ToString()
            txtCantidad.Text = fila.Cells("Stock").Value.ToString()
            productoSeleccionado = ProductoDAL.ObtenerPorSKU(txtSKU.Text)
        End If
    End Sub
    
    Private Sub AgregarProducto(sender As Object, e As EventArgs)
        If txtSKU.Text = "" Or txtNombre.Text = "" Then
            MessageBox.Show("SKU y Nombre son obligatorios", "Validación")
            Return
        End If
        
        Try
            Dim producto As New Producto With {
                .SKU = txtSKU.Text,
                .Nombre = txtNombre.Text,
                .Descripcion = txtDescripcion.Text,
                .Categoria = txtCategoria.Text,
                .CostoUnitario = CDec(txtCostoUnitario.Text),
                .PrecioVenta = CDec(txtPrecioVenta.Text),
                .CantidadStock = CInt(txtCantidad.Text)
            }
            producto.CalcularGanancia()
            ProductoDAL.Agregar(producto)
            MessageBox.Show("Producto agregado exitosamente", "Éxito")
            CargarProductos()
            LimpiarFormulario(Nothing, Nothing)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub
    
    Private Sub ActualizarProducto(sender As Object, e As EventArgs)
        If productoSeleccionado Is Nothing Then
            MessageBox.Show("Selecciona un producto", "Validación")
            Return
        End If
        
        Try
            productoSeleccionado.Nombre = txtNombre.Text
            productoSeleccionado.Descripcion = txtDescripcion.Text
            productoSeleccionado.Categoria = txtCategoria.Text
            productoSeleccionado.CostoUnitario = CDec(txtCostoUnitario.Text)
            productoSeleccionado.PrecioVenta = CDec(txtPrecioVenta.Text)
            productoSeleccionado.CantidadStock = CInt(txtCantidad.Text)
            productoSeleccionado.CalcularGanancia()
            
            ProductoDAL.Actualizar(productoSeleccionado)
            MessageBox.Show("Producto actualizado exitosamente", "Éxito")
            CargarProductos()
            LimpiarFormulario(Nothing, Nothing)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
        End Try
    End Sub
    
    Private Sub EliminarProducto(sender As Object, e As EventArgs)
        If productoSeleccionado Is Nothing Then
            MessageBox.Show("Selecciona un producto", "Validación")
            Return
        End If
        
        If MessageBox.Show("¿Estás seguro?", "Confirmar", MessageBoxButtons.YesNo) = DialogResult.Yes Then
            ProductoDAL.Eliminar(productoSeleccionado.Id)
            MessageBox.Show("Producto eliminado", "Éxito")
            CargarProductos()
            LimpiarFormulario(Nothing, Nothing)
        End If
    End Sub
    
    Private Sub LimpiarFormulario(sender As Object, e As EventArgs)
        txtSKU.Clear()
        txtNombre.Clear()
        txtDescripcion.Clear()
        txtCategoria.Clear()
        txtCostoUnitario.Clear()
        txtPrecioVenta.Clear()
        txtCantidad.Clear()
        productoSeleccionado = Nothing
    End Sub
    
End Class