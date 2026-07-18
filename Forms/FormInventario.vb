Imports System.Drawing

Public Class FormInventario
    Inherits Form
    
    Private dgvInventario As DataGridView
    Private lblValorTotal As Label
    
    Sub New()
        InitializeComponent()
        CargarInventario()
    End Sub
    
    Private Sub InitializeComponent()
        Me.Text = "📈 Control de Inventario"
        Me.Size = New Size(1200, 650)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 240, 240)
        
        Dim pnlSuperior As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 80,
            .BackColor = Color.FromArgb(50, 50, 50),
            .Padding = New Padding(20)
        }
        
        Dim lblTitulo As New Label With {
            .Text = "📦 STOCK EN TIEMPO REAL",
            .ForeColor = Color.White,
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .AutoSize = False,
            .Width = 400
        }
        pnlSuperior.Controls.Add(lblTitulo)
        Me.Controls.Add(pnlSuperior)
        
        dgvInventario = New DataGridView With {
            .Dock = DockStyle.Top,
            .Height = 350,
            .BackgroundColor = Color.White,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }
        dgvInventario.AllowUserToAddRows = False
        Me.Controls.Add(dgvInventario)
        
        Dim pnlResumen As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(20)
        }
        
        Dim lblValorTxt As New Label With {
            .Text = "Valor Total del Inventario:",
            .Font = New Font("Arial", 14, FontStyle.Bold),
            .Location = New Point(20, 20),
            .AutoSize = True
        }
        
        lblValorTotal = New Label With {
            .Text = "$0.00",
            .Font = New Font("Arial", 16, FontStyle.Bold),
            .ForeColor = Color.Green,
            .Location = New Point(250, 20),
            .AutoSize = True
        }
        
        pnlResumen.Controls.Add(lblValorTxt)
        pnlResumen.Controls.Add(lblValorTotal)
        Me.Controls.Add(pnlResumen)
    End Sub
    
    Private Sub CargarInventario()
        dgvInventario.DataSource = Nothing
        Dim productos = ProductoDAL.ObtenerTodos()
        
        Dim tabla As New DataTable()
        tabla.Columns.Add("SKU")
        tabla.Columns.Add("Producto")
        tabla.Columns.Add("Categoría")
        tabla.Columns.Add("Stock")
        tabla.Columns.Add("Costo Unitario")
        tabla.Columns.Add("Valor Total")
        tabla.Columns.Add("Estado")
        
        Dim valorTotalInventario As Decimal = 0
        
        For Each producto In productos
            Dim valorProducto = producto.CostoUnitario * producto.CantidadStock
            valorTotalInventario += valorProducto
            
            Dim estado = If(producto.CantidadStock <= 10, "⚠️ CRÍTICO", "✅ NORMAL")
            
            tabla.Rows.Add(producto.SKU, producto.Nombre, producto.Categoria, producto.CantidadStock,
                          producto.CostoUnitario, valorProducto, estado)
        Next
        
        dgvInventario.DataSource = tabla
        lblValorTotal.Text = "$" & valorTotalInventario.ToString("F2")
    End Sub
    
End Class