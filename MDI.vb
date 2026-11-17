Imports System.Data.OracleClient
Module MDI
    Public Sub AtualizaVariaveis()

        vGlobal.Arquivo = "" & Application.StartupPath & "\configuracao.ini"
        vGlobal.vInstLogin = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "INSTANCIA")

        'Banco de Dados
        vGlobal.sServiceName = ArquivosTXT.lerINI(vGlobal.Arquivo, "BANCODADOS", "SERVICE_NAME")
        vGlobal.sUser = ArquivosTXT.lerINI(vGlobal.Arquivo, "BANCODADOS", "USER")
        vGlobal.sPassword = Senha.fDescrSenha(ArquivosTXT.lerINI(vGlobal.Arquivo, "BANCODADOS", "PASSWORD"))
        vGlobal.sSensitiveLogon = ArquivosTXT.lerINI(vGlobal.Arquivo, "BANCODADOS", "SENSITIVE_LOGON")
        vGlobal.sUpperCase = ArquivosTXT.lerINI(vGlobal.Arquivo, "BANCODADOS", "UPPERCASE")

        vGlobal.vModeloImpressao = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "MODELOREL")
        vGlobal.gs01 = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "DESCRMARCDAGUA1")
        vGlobal.gs02 = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "DESCRMARCDAGUA2")
        vGlobal.vTimerImpressao = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "TIMER_IMPRESSAO")
        vGlobal.vNomeImpressora = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "NOMEIMPRESSORA")
        vGlobal.vNumeroCopias = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "NUMEROCOPIAS")
        vGlobal.sCliente = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "CLIENTE")
        vGlobal.vIMPRLIBSEPARADA = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "IMPRLIBSEPARADA")


        'Habilita Impressão
        vGlobal.vImprExtrSaida = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "IMPREXTRSAIDA")
        vGlobal.vImprOrdemCarga = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "IMPRORDEMCARGA")
        vGlobal.vImprBolAnl = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "IMPRBOLANL")
        vGlobal.vImprExtrProduto = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "IMPREXTRPRODUTO")

        'Timer Threads
        vGlobal.VTimerOrdemCarga = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "TIMER_IMPR_ORDEMCARGA")
        vGlobal.VTimerBoletimAnalises = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "TIMER_IMPR_BOLANL")
        vGlobal.VTimerExtratoSaida = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "TIMER_IMPR_EXTRSAIDA")
        vGlobal.VTimerExtrProduto = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "TIMER_IMPR_EXTRPROD")





    End Sub

    <Obsolete> Public Function fValidaUsuario(ByVal usuario) As Boolean
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select * from pmusuarios" & vbCrLf &
                  "where cd_usuario = '" & usuario & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("cd_usuario")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            If resultado = Nothing Then
                Return False
            Else
                Return True
            End If

        Catch ex As Exception
            MsgBox("fValidaUsuario: " & ex.ToString, MsgBoxStyle.Critical, "Error")
        End Try
    End Function

    <Obsolete> Public Function fValidaSenhaUser(ByVal usuario) As String
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select fg_pwd from pmusuarios" & vbCrLf &
                  "where cd_usuario = '" & usuario & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("fg_pwd")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            resultado = fDescrSenha(resultado)
            Return resultado

        Catch ex As Exception
            MsgBox("fValidaUsuario: " & ex.ToString, MsgBoxStyle.Critical, "Error")
        End Try
    End Function

    <Obsolete> Public Function fValidaPerfil(ByVal usuario) As Boolean
        Dim resultado As String = Nothing
        Dim sql As String
        Dim rs As OracleDataReader
        sql = "SELECT CD_ACAO" & vbCrLf &
                "From PMUSUARIOS U, PMUSUARIOS_INST UI," & vbCrLf &
                     "PMAUTORIDADES PA" & vbCrLf &
                "Where PA.CD_GRUPO = UI.CD_GRUPO" & vbCrLf &
                  "And U.CD_USUARIO = UI.CD_USUARIO" & vbCrLf &
                  "And UI.INSTANCIA = '" & vInstLogin & "'" & vbCrLf &
                  "And U.CD_USUARIO = '" & usuario & "'" & vbCrLf &
                  "And PA.CD_ACAO = '0'" & vbCrLf &
                  "And PA.CD_APLICAT = 'ZBC_CASS'"
        rs = fCmd(sql).ExecuteReader
        While rs.Read
            resultado = rs("cd_acao")
        End While
        fCmd(sql).Dispose()
        rs.Close()

        If resultado = Nothing Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Sub Mensagem(ByVal mensagem)
        vMensangem = mensagem
        FrmMensagem.Show()
    End Sub

    Public Function fBuscaCadastro(ByVal tabela, ByVal coluna, ByVal where, ByVal valor)
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select " & coluna & " coluna from " & tabela & vbCrLf &
                  "where " & where & " = '" & valor & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("coluna")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            If resultado = Nothing Then
                Return "###"
            ElseIf resultado <> Nothing Then
                Return resultado
            End If

        Catch ex As Exception
msgbox(ex.ToString,MsgBoxStyle.Critical,"Error")
        End Try
    End Function

    Public Function fGetParam(ByVal secao, ByVal entrada) As String
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select valor from parametros" & vbCrLf &
                  "where instancia = '" & vInstLogin & "'" & vbCrLf &
                  "  and secao = '" & secao & "'" & vbCrLf &
                  "  and entrada = '" & entrada & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("valor")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If resultado <> Nothing Then
                Return resultado
            Else
                Return 0
            End If
        Catch ex As Exception
            vMensangem += "Erro [fGetParam] - " & ex.ToString & vbCrLf
        End Try
    End Function

    Public Function fValidaRemito(ByVal NoRemito, ByVal fazenda, ByVal Liberacao) As Boolean
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select * from apt_cargas" & vbCrLf &
                  "where pno_remito = '" & NoRemito & "'" & vbCrLf &
                  "  and instancia = '" & vInstLogin & "'" & vbCrLf &
                  "  and cd_upnivel1 = '" & fazenda & "'" & vbCrLf &
                  "  and no_liberacao != '" & Liberacao & "'"
            vMensangem += sql & vbCrLf
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("pno_remito")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If resultado <> Nothing Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            vMensangem += "" & ex.ToString & vbCrLf
            frmMensagem.Show()
        End Try
    End Function

    Public Function fDescricao(tabela, descricao, coluna, valor) As String
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select " & descricao & " descricao from " & tabela & vbNewLine &
                  "where " & coluna & "= '" & valor & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("descricao")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If resultado = Nothing Then
                Return " "
            ElseIf resultado <> Nothing Then
                Return resultado
            End If
        Catch ex As Exception
            vMensangem += "" & ex.ToString & vbCrLf
            frmMensagem.Show()
        End Try
    End Function

    Public Function FCgcParaCuit(valor) As String
        Dim sValor As String = Nothing
        sValor = valor.Replace(".", "").Replace("/", "")
        sValor = sValor.Insert(2, "-").Insert(sValor.Length - 1, "-")
        Return sValor
    End Function

End Module
