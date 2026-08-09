Imports System.IO
Imports System.Text
Imports Models

Namespace Utilidades
    Public NotInheritable Class CsvUtils
        Private Sub New()
        End Sub

        Public Shared Sub ExportProductsToCsv(products As IEnumerable(Of Product), filePath As String, Optional includeBom As Boolean = True)
            Dim encoding = If(includeBom, New UTF8Encoding(True), New UTF8Encoding(False))
            Using sw = New StreamWriter(filePath, False, encoding)
                sw.WriteLine("Id,Codigo,Nombre,Descripcion,Precio,Stock")
                For Each p In products
                    Dim fields = New List(Of String) From {
                        p.Id.ToString(),
                        EscapeCsv(p.Codigo),
                        EscapeCsv(p.Nombre),
                        EscapeCsv(p.Descripcion),
                        p.Precio.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        p.Stock.ToString()
                    }
                    sw.WriteLine(String.Join(",", fields))
                Next
            End Using
        End Sub

        Private Shared Function EscapeCsv(value As String) As String
            If String.IsNullOrEmpty(value) Then Return ""
            Dim needsQuotes = value.Contains(",") OrElse value.Contains(""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf)
            Dim v = value.Replace("""", """""")
            Return If(needsQuotes, $"""{v}""", v)
        End Function

        Public Shared Function ImportProductsFromCsv(filePath As String) As List(Of Product)
            Dim list As New List(Of Product)
            Using sr = New StreamReader(filePath, Encoding.UTF8)
                Dim header = sr.ReadLine()
                If header Is Nothing Then Return list
                Dim colsHeader = header.Split(","c).Select(Function(s) s.Trim().ToLower()).ToArray()
                While Not sr.EndOfStream
                    Dim line = sr.ReadLine()
                    If String.IsNullOrWhiteSpace(line) Then Continue While
                    Dim fields = ParseCsvLine(line)
                    Dim p As New Product()
                    If colsHeader.Length >= 6 Then
                        Dim idx = Function(name As String) As Integer
                                      For i = 0 To colsHeader.Length - 1
                                          If colsHeader(i) = name Then Return i
                                      Next
                                      Return -1
                                  End Function
                        Dim iId = idx("id")
                        Dim iCodigo = idx("codigo")
                        Dim iNombre = idx("nombre")
                        Dim iDescripcion = idx("descripcion")
                        Dim iPrecio = idx("precio")
                        Dim iStock = idx("stock")
                        If iId >= 0 AndAlso iId < fields.Count Then Integer.TryParse(fields(iId), p.Id)
                        If iCodigo >= 0 AndAlso iCodigo < fields.Count Then p.Codigo = fields(iCodigo)
                        If iNombre >= 0 AndAlso iNombre < fields.Count Then p.Nombre = fields(iNombre)
                        If iDescripcion >= 0 AndAlso iDescripcion < fields.Count Then p.Descripcion = fields(iDescripcion)
                        If iPrecio >= 0 AndAlso iPrecio < fields.Count Then Decimal.TryParse(fields(iPrecio), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, p.Precio)
                        If iStock >= 0 AndAlso iStock < fields.Count Then Integer.TryParse(fields(iStock), p.Stock)
                    Else
                        If fields.Count >= 6 Then
                            Integer.TryParse(fields(0), p.Id)
                            p.Codigo = fields(1)
                            p.Nombre = fields(2)
                            p.Descripcion = fields(3)
                            Decimal.TryParse(fields(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, p.Precio)
                            Integer.TryParse(fields(5), p.Stock)
                        Else
                            Continue While
                        End If
                    End If
                    list.Add(p)
                End While
            End Using
            Return list
        End Function

        Private Shared Function ParseCsvLine(line As String) As List(Of String)
            Dim result As New List(Of String)
            Dim cur As New System.Text.StringBuilder()
            Dim inQuotes = False
            Dim i = 0
            While i < line.Length
                Dim c = line(i)
                If c = """"c Then
                    If inQuotes AndAlso i + 1 < line.Length AndAlso line(i + 1) = """"c Then
                        cur.Append(""""c)
                        i += 2
                        Continue While
                    Else
                        inQuotes = Not inQuotes
                        i += 1
                        Continue While
                    End If
                End If
                If c = ","c AndAlso Not inQuotes Then
                    result.Add(cur.ToString())
                    cur.Clear()
                Else
                    cur.Append(c)
                End If
                i += 1
            End While
            result.Add(cur.ToString())
            Return result
        End Function
    End Class
End Namespace
