Imports System.IO

Public Class Logger
    Private Shared logDir As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestorTienda", "logs")
    Private Shared logFile As String = Path.Combine(logDir, "app.log")
n    Public Shared Sub Info(message As String)
        WriteLog("INFO", message)
    End Subnn    Public Shared Sub Error(message As String)
        WriteLog("ERROR", message)
    End Sub
n    Private Shared Sub WriteLog(level As String, message As String)
        Try
            If Not Directory.Exists(logDir) Then Directory.CreateDirectory(logDir)
            Dim line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & " [" & level & "] " & message & Environment.NewLine
            File.AppendAllText(logFile, line)
        Catch
            ' No hacer nada si falla el logger para no romper la app
        End Try
    End SubnEnd Class