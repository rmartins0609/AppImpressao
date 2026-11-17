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
        Dim sGravaLog As String = dadosarquivoini(("LOGS", "GRAVALOG"))
        Dim FileName As String = dadosarquivoini(("LOGS", sFileName))
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

    Private ReadOnly logLock As New Object()
    Public Function fGravaLogDebug(ByVal sLog As String, ByVal sFileName As String)
        Try
            ' 1. Valida se o log está ativo
            Dim sGravaLog As String = dadosarquivoini(("LOGS", "LOGDEBUG"))
            If sGravaLog <> "S" Then Exit Function

            ' 2. Prepara os caminhos
            Dim fileNameConfig As String = dadosarquivoini(("LOGS", sFileName))
            Dim caminhoDir As String = Arquivo_log
            Dim sArquivo As String = System.IO.Path.Combine(caminhoDir, fileNameConfig)

            ' 3. Garante a existência do diretório (Directory.CreateDirectory já faz a checagem internamente)
            If Not System.IO.Directory.Exists(caminhoDir) Then
                System.IO.Directory.CreateDirectory(caminhoDir)
            End If

            ' 4. Bloqueia a execução paralela na mesma aplicação
            SyncLock logLock
                ' Verifica o tamanho do arquivo se ele existir
                Dim infoArquivo As New System.IO.FileInfo(sArquivo)
                If infoArquivo.Exists AndAlso (infoArquivo.Length / 1024) > 100 Then
                    ' Limpa o arquivo sem precisar deletar/recriar na mão
                    System.IO.File.WriteAllText(sArquivo, String.Empty)
                End If

                ' 5. Escreve no log permitindo que outros processos também leiam/escrevam (FileShare.ReadWrite)
                Using fs As New System.IO.FileStream(sArquivo, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite)
                    Using writer As New System.IO.StreamWriter(fs)
                        writer.WriteLine($"{Now} - {sLog}")
                    End Using
                End Using
            End SyncLock

        Catch ex As Exception
            ' Evita exibir pop-up para o usuário final em falhas secundárias de gravação de log
            System.Diagnostics.Debug.WriteLine($"Erro ao gravar log: {ex.Message}")
        End Try
    End Function

    Public Sub fGravaError(ByVal sModulo As String, sProcesso As String, ByVal sMensagem As String, ByVal sArquivo As String)
        fGravaLog("ERROR *** [" & sModulo & "] ***", sArquivo)
        fGravaLog("ERROR - [" & sProcesso & "]: " & sMensagem & vbCrLf & "", sArquivo)
    End Sub

End Module
