'Imports System.Data.OracleClient
Imports Oracle.ManagedDataAccess.Client
Imports System.Text
Imports System.IO
Module MDI
    Public Sub AtualizaVariaveis()

        vGlobal.vInstLogin = dadosarquivoini(("PARAMETROS", "INSTANCIA"))
        'vGlobal.vNomePc = ArquivosTXT.lerINI(vGlobal.Arquivo, "PARAMETROS", "NOME_PC")

        Dim nomepc As String
        If Environment.GetEnvironmentVariable("CLIENTNAME") = Nothing Then
            nomepc = Environment.MachineName
        Else
            nomepc = Environment.GetEnvironmentVariable("CLIENTNAME")
        End If
        vGlobal.vNomePc = nomepc

        'Banco de Dados
        vGlobal.sServiceName = dadosarquivoini(("BANCODADOS", "SERVICE_NAME"))
        vGlobal.sUser = dadosarquivoini(("BANCODADOS", "USER"))
        vGlobal.sPassword = Senha.fDescrSenha(dadosarquivoini(("BANCODADOS", "PASSWORD")))
        vGlobal.sSensitiveLogon = dadosarquivoini(("BANCODADOS", "SENSITIVE_LOGON"))
        vGlobal.sUpperCase = dadosarquivoini(("BANCODADOS", "UPPERCASE"))

        vGlobal.vModeloImpressao = dadosarquivoini(("PARAMETROS", "MODELOREL"))
        vGlobal.gs01 = dadosarquivoini(("PARAMETROS", "DESCRMARCDAGUA1"))
        vGlobal.gs02 = dadosarquivoini(("PARAMETROS", "DESCRMARCDAGUA2"))
        vGlobal.vTimerImpressao = dadosarquivoini(("PARAMETROS", "TIMER_IMPRESSAO"))
        vGlobal.vNomeImpressora = dadosarquivoini(("PARAMETROS", "NOMEIMPRESSORA"))
        vGlobal.vNumeroCopias = dadosarquivoini(("PARAMETROS", "NUMEROCOPIAS"))
        vGlobal.vNumCopiasExtrProduto = dadosarquivoini(("PARAMETROS", "NUMCOPIAS_EXTR_PRODUTO"))
        vGlobal.vNumCopiasTicketPatio = dadosarquivoini(("PARAMETROS", "NUMCOPIAS_TICKET_PATIO"))
        vGlobal.vNumCopiasExtrSaidaCana = dadosarquivoini(("PARAMETROS", "NUMCOPIAS_EXTR_SAIDA_CANA"))
        vGlobal.vNumCopiasLaboratCana = dadosarquivoini(("PARAMETROS", "NUMCOPIAS_LABORAT_CANA"))
        vGlobal.vNumCopiasBoletimentr = dadosarquivoini(("PARAMETROS", "NUMCOPIAS_BOLETIM_ENTR"))
        vGlobal.vRetratoPaisagem = dadosarquivoini(("PARAMETROS", "RETRATOPAISAGEM"))
        vGlobal.sCliente = dadosarquivoini(("PARAMETROS", "CLIENTE"))
        vGlobal.vIMPRLIBSEPARADA = dadosarquivoini(("PARAMETROS", "IMPRLIBSEPARADA"))
        vGlobal.vNUMERO_CARACTER_LINHA_OBS = dadosarquivoini(("PARAMETROS", "NUMERO_CARACTER_LINHA_OBS"))
        vGlobal.vTICKETENTRSORT = dadosarquivoini(("PARAMETROS", "TICKETENTRSORT"))
        vGlobal.vImprTela = dadosarquivoini(("PARAMETROS", "IMPR_TELA"))
        vGlobal.vTipoFonte = dadosarquivoini(("PARAMETROS", "FONTE_IMPRESSAO"))
        vGlobal.ModeloImprTermica = dadosarquivoini(("PARAMETROS", "MODELO_IMPR_TERMICA"))


        'Habilita Impressão
        vGlobal.vImprExtrSaida = dadosarquivoini(("PARAMETROS", "IMPREXTRSAIDA"))
        vGlobal.vImprExtrEntr = dadosarquivoini(("PARAMETROS", "IMPRBOLENTR"))
        vGlobal.vImprOrdemCarga = dadosarquivoini(("PARAMETROS", "IMPRORDEMCARGA"))
        vGlobal.vImprBolAnl = dadosarquivoini(("PARAMETROS", "IMPRBOLANL"))
        vGlobal.vImprExtrProduto = dadosarquivoini(("PARAMETROS", "IMPREXTRPRODUTO"))
        vGlobal.vImprTicketRast = dadosarquivoini(("PARAMETROS", "IMPRETICKETRAST"))

        'Timer Threads
        vGlobal.VTimerOrdemCarga = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_ORDEMCARGA"))
        vGlobal.VTimerBoletimAnalises = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_BOLANL"))
        vGlobal.VTimerExtratoSaida = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_EXTRSAIDA"))
        vGlobal.VTimerExtrProduto = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_EXTRPROD"))

        'BUDEGUER - Descrições da Empresa a ser impressa
        vGlobal.sDESCR_EMPRESA = dadosarquivoini(("EMPRESA", "DESCR_EMPRESA"))
        vGlobal.sDESCR_L1 = dadosarquivoini(("EMPRESA", "DESCR_L1"))
        vGlobal.sDESCR_L2 = dadosarquivoini(("EMPRESA", "DESCR_L2"))
        vGlobal.sDESCR_L3 = dadosarquivoini(("EMPRESA", "DESCR_L3"))
        vGlobal.sDESCR_L4 = dadosarquivoini(("EMPRESA", "DESCR_L4"))

        'BALCANES - Realiza a impressão da quantidade de litros na ordem de carga
        vGlobal.sValor05 = dadosarquivoini(("PARAMETROS", "FG_IMPR_LITRO"))

        vGlobal.EMPRESA_PRD = dadosarquivoini(("EMPRESA", "EMPRESA_PRD"))

        'Tipo de corte do papel, para impressora termica
        vGlobal.TipoCortePapel = dadosarquivoini(("PARAMETROS", "TIPO_CORTE_PAPEL"))

        'Unidade Industrial
        vGlobal.sUnidInd = dadosarquivoini(("PARAMETROS", "CD_UNID_IND"))

    End Sub

    Public Sub Mensagem(ByVal mensagem)
        vMensangem = mensagem
        frmMensagem.Show()
    End Sub

    Public Function FBuscaRemito(OdemCarga) As String
        Try
            Dim result As String
            Dim sql As String
            Dim rs As OracleDataReader
            sql += "select nvl(no_remito,0) remito from SYN_INT_PIMS_093m" & vbNewLine
            sql += "where instancia = '" & vInstLogin & "'" & vbNewLine
            sql += "  and cd_ordem_car = " & OdemCarga & ""
            fGravaLog("*** Busca Remito ***", "LOG_DEBUG")
            fGravaLog(sql, "LOG_DEBUG")
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("remito")
            End While
            fGravaLog(result, "LOG_DEBUG")
            fCmd(sql).Dispose()
            rs.Close()
            Return result
        Catch ex As Exception
            MsgBox("FBuscaRemito>>" & ex.ToString, MsgBoxStyle.Critical, "Error")
        End Try
    End Function

    Public Function GetDebugSql(cmd As OracleCommand) As String
        Dim sb As New Text.StringBuilder(cmd.CommandText)
        'For Each param As OracleParameter In cmd.Parameters
        '    Dim value = If(param.Value Is Nothing, "NULL", $"{param.Value.ToString().Replace("'", "''")}")
        '    sb.Replace(param.ParameterName, value)
        'Next
        For Each p As OracleParameter In cmd.Parameters
            Dim valor As String

            If p.Value Is Nothing OrElse p.Value Is DBNull.Value Then
                valor = "NULL"
            ElseIf TypeOf p.Value Is String OrElse TypeOf p.Value Is Date Then
                valor = "'" & p.Value.ToString().Replace("'", "''") & "'"
            Else
                valor = p.Value.ToString()
            End If

            sb = sb.Replace(":" & p.ParameterName, valor)
        Next
        Return sb.ToString()
    End Function

    Public Function FMensagem(mensagem As String, tipo As String) As Boolean
        Select Case tipo
            Case "I"
                MsgBox(mensagem, MsgBoxStyle.Information, "Aviso")
            Case "E"
                MsgBox(mensagem, MsgBoxStyle.Exclamation, "Aviso")
            Case "C"
                MsgBox(mensagem, MsgBoxStyle.Critical, "Aviso")
            Case "Q"
                If (MsgBox(mensagem, MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Aviso") = MsgBoxResult.Yes) Then
                    Return True
                Else
                    Return False
                End If
        End Select
    End Function

    Public Class GeraArquivoINI
        Public Shared Function Dados(carminhoarquivo As String) As Dictionary(Of (String, String), String)
            Dim resultado As New Dictionary(Of (String, String), String)
            Dim secaoatual As String = Nothing

            For Each linha In File.ReadAllLines(carminhoarquivo)
                Dim l = linha.Trim()

                ' Ignora linhas vazias e comentários

                If String.IsNullOrEmpty(l) OrElse l.StartsWith("#") Then Continue For
                If l.StartsWith("[") AndAlso l.EndsWith("]") Then
                    secaoatual = l.Substring(1, l.Length - 2)
                ElseIf secaoatual IsNot Nothing AndAlso l.Contains("=") Then
                    Dim partes = l.Split({"="c}, 2)
                    Dim chave = partes(0).Trim()
                    Dim valor = partes(1).Trim()
                    If Not resultado.ContainsKey((secaoatual, chave)) Then
                        resultado.Add((secaoatual, chave), valor)
                    End If
                End If
            Next
            Return resultado
        End Function

        Private Shared dicconfiginicial As New Dictionary(Of (String, String), String) From {
        {("BANCODADOS", "SERVICE_NAME"), "localhost:1521/orapims"},
        {("BANCODADOS", "SENSITIVE_LOGON"), "S"},
        {("BANCODADOS", "UPPERCASE"), "S"},
        {("BANCODADOS", "USER"), "PIMSCS"},
        {("BANCODADOS", "PASSWORD"), "072064069078067090"},
        {("EMPRESA", "EMPRESA_PRD"), "1"},
        {("EMPRESA", "DESCR_EMPRESA"), "*** DESCRICAO DA EMPRESA ***"},
        {("EMPRESA", "DESCR_L1"), "*** DESCRICAO LABEL 1 ***"},
        {("EMPRESA", "DESCR_L2"), "*** DESCRICAO LABEL 2 ***"},
        {("EMPRESA", "DESCR_L3"), "*** DESCRICAO LABLE 3 ***"},
        {("EMPRESA", "DESCR_L4"), "*** DESCRICAO LABEL 4 ***"},
        {("PARAMETROS", "MODELO_IMPR_TERMICA"), "BEMATHECH"},
        {("PARAMETROS", "INSTANCIA"), "001"},
        {("PARAMETROS", "NOMEIMPRESSORA"), "NOME DA IMPRESSORA"},
        {("PARAMETROS", "FONTE_IMPRESSAO"), "Arial"},
        {("PARAMETROS", "NUMEROCOPIAS"), "1"},
        {("PARAMETROS", "NUMCOPIAS_EXTR_PRODUTO"), "1"},
        {("PARAMETROS", "NUMCOPIAS_TICKET_PATIO"), "1"},
        {("PARAMETROS", "NUMCOPIAS_EXTR_SAIDA_CANA"), "1"},
        {("PARAMETROS", "NUMCOPIAS_LABORAT_CANA"), "1"},
        {("PARAMETROS", "NUMCOPIAS_BOLETIM_ENTR"), "1"},
        {("PARAMETROS", "TIMER_IMPRESSAO"), "60"},
        {("PARAMETROS", "TIMER_IMPR_ORDEMCARGA"), "1000"},
        {("PARAMETROS", "TIMER_IMPR_BOLANL"), "2000"},
        {("PARAMETROS", "TIMER_IMPR_EXTRSAIDA"), "3000"},
        {("PARAMETROS", "TIMER_IMPR_BOL_ENTR"), "4000"},
        {("PARAMETROS", "TIMER_IMPR_EXTRPROD"), "5000"},
        {("PARAMETROS", "TIMER_IMPR_TICKETRAST"), "6000"},
        {("PARAMETROS", "RETRATOPAISAGEM"), "R"},
        {("PARAMETROS", "TIPO_CORTE_PAPEL"), "TOTAL"},
        {("PARAMETROS", "MODELOREL"), "01"},
        {("PARAMETROS", "CLIENTE"), "NOME_CLIENTE"},
        {("PARAMETROS", "IMPRORDEMCARGA"), "N"},
        {("PARAMETROS", "IMPREXTRSAIDA"), "N"},
        {("PARAMETROS", "IMPRBOLENTR"), "N"},
        {("PARAMETROS", "TICKETENTRSORT"), "N"},
        {("PARAMETROS", "IMPRBOLANL"), "N"},
        {("PARAMETROS", "IMPRETICKETRAST"), "N"},
        {("PARAMETROS", "IMPREXTRPRODUTO"), "N"},
        {("PARAMETROS", "DESCRMARCDAGUA1"), "CLIENTE"},
        {("PARAMETROS", "DESCRMARCDAGUA2"), "CLIENTE"},
        {("PARAMETROS", "IMPRLIBSEPARADA"), "N"},
        {("PARAMETROS", "MODELO_ORDEMCARGA"), "02"},
        {("PARAMETROS", "NUMERO_CARACTER_LINHA_OBS"), "40"},
        {("PARAMETROS", "IMPR_TELA"), "N"},
        {("PARAMETROS", "NOME_PC"), "NULO"},
        {("PARAMETROS", "FG_IMPR_LITRO"), "N"},
        {("PARAMETROS", "CD_UNID_IND"), "0"},
        {("LOGS", "GRAVALOG"), "N"},
        {("LOGS", "LOGDEBUG"), "N"},
        {("LOGS", "LOG_OC_ERROR"), "Log_OrdenCarga_Error.log"},
        {("LOGS", "LOG_EXS_ERROR"), "Log_ExtrSalida_Error.log"},
        {("LOGS", "LOG_BOLA_ERROR"), "Log_BolAnalisis_Error.log"},
        {("LOGS", "LOG_ERROR"), "Log_Error.log"},
        {("LOGS", "LOG_TIMER_IMPR"), "Log_Timer_Impresion.log"},
        {("LOGS", "LOG_DEBUG"), "Log_Debug.log"},
        {("LOGS", "LOG_TESTE"), "Log_Testes.log"},
        {("PARAMETROS", "TIMEZONE"), "0"}
        }

        Public Shared Sub CriaArquivoConfic()
            Dim arquivo As String = Application.StartupPath & "\configuracao.ini"
            If Not IO.File.Exists(arquivo) Then
                Using fs As IO.FileStream = IO.File.Create(arquivo)
                End Using
            End If

            If dadosarquivoini Is Nothing Then
                dadosarquivoini = New Dictionary(Of (String, String), String)()
            End If

            For Each kvp In dicconfiginicial
                Dim secao As String = kvp.Key.Item1
                Dim chave As String = kvp.Key.Item2
                Dim valor As String = kvp.Value

                If Not dadosarquivoini.ContainsKey((secao, chave)) Then
                    ArquivosTXT.fSetParamINI(secao, chave, valor, arquivo)
                    dadosarquivoini.Add((secao, chave), valor)
                End If
            Next
        End Sub
    End Class



End Module
