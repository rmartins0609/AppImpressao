Module Senha
    Dim sSensitive_Logon As String = lerINI(vGlobal.Arquivo, "BANCODADOS", "SENSITIVE_LOGON")

    Public Function fDescrSenha(ByVal sPass)
        Dim Ascii As String = ""
        Dim sSenha As String = ""
        Dim sNLetras As Integer = Len(sPass) / 3
        Dim sNCount As Integer = 0
        Dim sNPosi As Integer = 0
        While sNCount < sNLetras
            If sNCount = 0 Then
                sNPosi = 1
            Else
                sNPosi = (sNCount * 3) + 1
            End If
            Ascii = Chr((Mid(sPass, sNPosi, 3) + (8 + (sNCount * (2 - sNCount)))))
            sSenha = (sSenha & Ascii).ToUpper()
            sNCount = sNCount + 1
        End While
        If sSensitive_Logon = "L" Then
            fDescrSenha = LCase(sSenha)
        ElseIf sSensitive_Logon = "U" Then
            fDescrSenha = UCase(sSenha)
        Else
            fDescrSenha = sSenha
        End If
    End Function

    Public Function fCrypto(ByVal sSenha) As String
        Try
            Dim nCount As Integer = 0
            Dim nPosicao As Integer = 0
            Dim nQtdeLetras As Integer = 0
            Dim sLetra As String = ""
            Dim sCodAscii As String = ""
            Dim Ascii As Integer = 0
            Dim sChave As String = ""
            nQtdeLetras = Len(sSenha)
            While nCount < nQtdeLetras
                nPosicao = nPosicao + 1
                sLetra = Mid(sSenha, nPosicao, 1).ToUpper()

                Ascii = Asc(sLetra)
                sCodAscii = (Ascii - (8 + (nCount * (2 - nCount)))).ToString.PadLeft(3, "0")

                sChave = sChave & sCodAscii
                nCount = nCount + 1
            End While
            Return sChave
        Catch ex As Exception
            Return "Erro: Valor informado inválido"
        End Try
    End Function
End Module
