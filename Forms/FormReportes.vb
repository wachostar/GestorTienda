Imports System.Drawing

Public Class FormReportes
    Inherits Form
    
    Private dgvReporte As DataGridView
    Private dtpFechaInicio As DateTimePicker
    Private dtpFechaFin As DateTimePicker
    Private cmbTipoReporte As ComboBox
    Private btnGenerar As Button
    Private lblResumen As Label
    
    Sub New()
        InitializeComponent()
    End Sub
    
    Private Sub InitializeComponent()
        Me.Text = "📊 Reportes Avanzados"
        Me.Size = New Size(1200, 650)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(240, 240, 240)
        
        Dim pnlFiltros As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 80,
            .BackColor = Color.White,
            .Padding = New Padding(20)
        }
        
        Dim lblTipo As New Label With {.Text = "Tipo Reporte:", .AutoSize = True, .Location = New Point(10, 15)}
        cmbTipoReporte = New ComboBox With {.Location = New Point(120, 10), .Width = 200}
        cmbTipoReporte.Items.AddRange(New String() {"Ventas por Día", "Ventas por Artículo", "Gastos", "Stock Crítico", "Resumen Financiero"})
        cmbTipoReporte.SelectedIndex = 0
        
        Dim lblInicio As New Label With {.Text = "Desde:", .AutoSize = True, .Location = New Point(10, 50)}
        dtpFechaInicio = New DateTimePicker With {.Location = New Point(120, 50), .Width = 150, .Value = DateTime.Now.AddDays(-30)}
        
        Dim lblFin As New Label With {.Text = "Hasta:", .AutoSize = True, .Location = New Point(280, 50)}
        dtpFechaFin = New DateTimePicker With {.Location = New Point(350, 50), .Width = 150, .Value = DateTime.Now}
        
        btnGenerar = New Button With {.Text = "📈 Generar", .Location = New Point(520, 35), .Width = 120, .BackColor = Color.Blue, .ForeColor = Color.White}
        
        pnlFiltros.Controls.Add(lblTipo)
        pnlFiltros.Controls.Add(cmbTipoReporte)
        pnlFiltros.Controls.Add(lblInicio)
        pnlFiltros.Controls.Add(dtpFechaInicio)
        pnlFiltros.Controls.Add(lblFin)
        pnlFiltros.Controls.Add(dtpFechaFin)
        pnlFiltros.Controls.Add(btnGenerar)
        
        Me.Controls.Add(pnlFiltros)
        
        dgvReporte = New DataGridView With {
            .Dock = DockStyle.Top,
            .Height = 350,
            .BackgroundColor = Color.White,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        }
        dgvReporte.AllowUserToAddRows = False
        Me.Controls.Add(dgvReporte)
        
        lblResumen = New Label With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.FromArgb(240, 240, 240),
            .Font = New Font("Arial", 12, FontStyle.Bold),
            .Padding = New Padding(20)
        }
        Me.Controls.Add(lblResumen)
        
        AddHandler btnGenerar.Click, AddressOf GenerarReporte
    End Sub
    
    Private Sub GenerarReporte(sender As Object, e As EventArgs)
        Select Case cmbTipoReporte.SelectedItem.ToString()
            Case "Ventas por Día"
                ReportesVentasPorDia()
            Case "Ventas por Artículo"
                ReportesVentasPorArticulo()
            Case "Gastos"
                ReportesGastos()
            Case "Stock Crítico"
                ReportesStockCritico()
            Case "Resumen Financiero"
                ResumenFinanciero()
        End Select
    End Sub
    
    Private Sub ReportesVentasPorDia()
        Dim transacciones = TransaccionDAL.ObtenerPorFecha(dtpFechaInicio.Value, dtpFechaFin.Value)
        Dim tabla As New DataTable()
        tabla.Columns.Add("Fecha")
        tabla.Columns.Add("Cantidad Transacciones")
        tabla.Columns.Add("Monto Total")
        tabla.Columns.Add("Ganancia Bruta")
        
        Dim grupos = transacciones.GroupBy(Function(t) t.Fecha.Date)
        For Each grupo In grupos
            Dim monto = grupo.Sum(Function(t) t.Total)
            Dim ganancia = grupo.Sum(Function(t) t.GananciaTotal)
            tabla.Rows.Add(grupo.Key.ToString("yyyy-MM-dd"), grupo.Count(), monto, ganancia)
        Next
        
        dgvReporte.DataSource = tabla
        lblResumen.Text = "Total Ventas: $" & transacciones.Sum(Function(t) t.Total).ToString("F2") & " | Ganancia: $" & transacciones.Sum(Function(t) t.GananciaTotal).ToString("F2")
    End Sub
    
    Private Sub ReportesVentasPorArticulo()
        Dim transacciones = TransaccionDAL.ObtenerPorFecha(dtpFechaInicio.Value, dtpFechaFin.Value)
        Dim tabla As New DataTable()
        tabla.Columns.Add("SKU")
        tabla.Columns.Add("Producto")
        tabla.Columns.Add("Cantidad Vendida")
        tabla.Columns.Add("Monto Total")
        tabla.Columns.Add("Ganancia")
        
        Dim grupos = transacciones.GroupBy(Function(t) t.SKU)
        For Each grupo In grupos
            tabla.Rows.Add(grupo.Key, grupo.First().NombreProducto, grupo.Sum(Function(t) t.Cantidad), 
                          grupo.Sum(Function(t) t.Total), grupo.Sum(Function(t) t.GananciaTotal))
        Next
        
        dgvReporte.DataSource = tabla
    End Sub
    
    Private Sub ReportesGastos()
        Dim gastos = GastoDAL.ObtenerPorFecha(dtpFechaInicio.Value, dtpFechaFin.Value)
        Dim tabla As New DataTable()
        tabla.Columns.Add("Tipo")
        tabla.Columns.Add("Descripción")
        tabla.Columns.Add("Monto")
        tabla.Columns.Add("Fecha")
        
        For Each gasto In gastos
            tabla.Rows.Add(gasto.Tipo, gasto.Descripcion, gasto.Monto, gasto.Fecha.ToString("yyyy-MM-dd"))
        Next
        
        dgvReporte.DataSource = tabla
        lblResumen.Text = "Total Gastos: $" & gastos.Sum(Function(g) g.Monto).ToString("F2")
    End Sub
    
    Private Sub ReportesStockCritico()
        Dim productos = ProductoDAL.ObtenerStockCritico(10)
        Dim tabla As New DataTable()
        tabla.Columns.Add("SKU")
        tabla.Columns.Add("Producto")
        tabla.Columns.Add("Stock Actual")
        tabla.Columns.Add("Valor Total")
        
        For Each producto In productos
            tabla.Rows.Add(producto.SKU, producto.Nombre, producto.CantidadStock, producto.CostoUnitario * producto.CantidadStock)
        Next
        
        dgvReporte.DataSource = tabla
        lblResumen.Text = "Productos con Stock Crítico (≤10): " & productos.Count()
    End Sub
    
    Private Sub ResumenFinanciero()
        Dim transacciones = TransaccionDAL.ObtenerPorFecha(dtpFechaInicio.Value, dtpFechaFin.Value)
        Dim gastos = GastoDAL.ObtenerPorFecha(dtpFechaInicio.Value, dtpFechaFin.Value)
        
        Dim totalVentas = transacciones.Sum(Function(t) t.Total)
        Dim gananciaVentas = transacciones.Sum(Function(t) t.GananciaTotal)
        Dim totalGastos = gastos.Sum(Function(g) g.Monto)
        Dim gananciaNetaTotal = gananciaVentas - totalGastos
        
        lblResumen.Text = "RESUMEN FINANCIERO" & vbCrLf &
                         "Total Ventas: $" & totalVentas.ToString("F2") & vbCrLf &
                         "Ganancia Bruta: $" & gananciaVentas.ToString("F2") & vbCrLf &
                         "Total Gastos: $" & totalGastos.ToString("F2") & vbCrLf &
                         "GANANCIA NETA: $" & gananciaNetaTotal.ToString("F2")
    End Sub
    
End Class