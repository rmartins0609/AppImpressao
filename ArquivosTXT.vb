Imports System.Text
Module ArquivosTXT
    Public Declare Auto Function GetPrivateProfileString Lib "Kernel32" (ByVal lpAppName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As StringBuilder, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    Private Declare Auto Function WritePrivateProfileString Lib "Kernel32" (ByVal lpAppName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Integer
    Public Arquivo_log As String = "" & Application.StartupPath & "\logs"

    Public Function lerINI(ByVal strIniFile As String, ByVal strKey As String, ByVal strItem As String) As String
        Dim strValue As StringBuilder = New StringBuilder(255)
        Dim intSize As Integer
        intSize = GetPrivateProfileString(strKey, strItem, "", strValue, 255, strIniFile)
        Return strValue.ToString
    End Function

    Public Function fSetParamINI(ByVal sSecao, ByVal sChave, ByVal sTexto, ByVal sArquivo)
        fSetParamINI = WritePrivateProfileString(sSecao, sChave, sTexto, sArquivo)
    End Function

    Public Function fGravaLog(ByVal sLog As String, ByVal sFileName As String)
        Dim sGravaLog As String = lerINI(vGlobal.Arquivo, "LOGS", "GRAVALOG")
        Dim FileName As String = lerINI(vGlobal.Arquivo, "LOGS", sFileName)
        Try
            If sGravaLog = "S" Then
                Dim dir As IO.Directory
                Dim caminho As String = Arquivo_log

                If dir.Exists(caminho) = False Then
                    dir.CreateDirectory(caminho)
                End If

                Dim sMsg As String = ""
                Dim sArquivo As String = "" & Arquivo_log & "\" & FileName & ""
                Dim tamanho As New System.IO.FileInfo(sArquivo)

                If System.IO.File.Exists(sArquivo) = False Then
                    System.IO.File.Create(sArquivo).Dispose()
                End If
                If System.IO.File.Exists(sArquivo) = True Then
                    Dim calculo As Integer = tamanho.Length() / 1024
                    If calculo > 100 Then
                        System.IO.File.Delete(sArquivo)
                        System.IO.File.Create(sArquivo).Dispose()
                    End If

                    Dim ArquivoWriter As New System.IO.StreamWriter(sArquivo, True)
                    sMsg = sMsg & Now() & " - " & sLog
                    ArquivoWriter.WriteLine(sMsg)
                    ArquivoWriter.Close()
                End If
            End If
        Catch ex As Exception
            '  MsgBox("Erro:" & ex.Message, MsgBoxStyle.Information, "Error")
        End Try
    End Function

    Public Sub fGravaError(ByVal sModulo As String, sProcesso As String, ByVal sMensagem As String, ByVal sArquivo As String)
        fGravaLog("ERROR *** [" & sModulo & "] ***", sArquivo)
        fGravaLog("ERROR - [" & sProcesso & "]: " & sMensagem & vbCrLf & "", sArquivo)
    End Sub

End Module
