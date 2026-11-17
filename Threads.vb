Imports System.Threading
Imports AppImpressao.VerificaImpressao
Imports Oracle.ManagedDataAccess.Client
Module Threads
    Dim Arquivo As String = Application.StartupPath
    Public t As New Timers.Timer
    Public intervalOrdemCarga As Integer = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_ORDEMCARGA"))
    Public intervalBoletimAnalises As Integer = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_BOLANL"))
    Public intervalExtratoSaida As Integer = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_EXTRSAIDA"))
    Public intervalExtratoProduto As Integer = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_EXTRPROD"))
    Public intervalTicketRast As Integer = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_TICKETRAST"))
    Public intervalBoletimEntr As Integer = dadosarquivoini(("PARAMETROS", "TIMER_IMPR_TICKETRAST"))

    Dim vModeloOrdemCarga As String = dadosarquivoini(("PARAMETROS", "MODELO_ORDEMCARGA"))

    Public thrOrdemCarga As Thread
    Public thrBoletimAnalises As Thread
    Public thrExtratoSaida As Thread
    Public thrExtratoProduto As Thread
    Public thrTicketRast As Thread
    Public thrBolEntr As Thread

    Public thrTimerOrdemCarga As Threading.Timer
    Public bStartThrOrdemCarga As Boolean = False

    Public thrTimerBoletimAnalises As Threading.Timer
    Public bStartThrBoletimAnalises As Boolean = False

    Public thrTimerExtratoSaida As Threading.Timer
    Public bStartThrExtratoSaida As Boolean = False

    Public thrTimerExtratoProduto As Threading.Timer
    Public bStartThrExtratoProduto As Boolean = False

    Public thrTimerTicketRast As Threading.Timer
    Public bStartThrTicketRast As Boolean = False

    Public thrTimerBolEntr As Threading.Timer
    Public bStartThrBolentr As Boolean = False

    Public bImprimindoBolEntr As Boolean

    ' Começa True (cancela aberta) para a Thread rodar livremente no início
    Public ControlaThreadImpressao As New System.Threading.ManualResetEvent(True)

    Private VeriImpr As New VerificaImpressao

    '*** [INICIO] Thread Impressão da Ordem de Carga ***
    Public Sub IniciaProcOrdemCarga()
        If Not bStartThrOrdemCarga Then
            bStartThrOrdemCarga = True
            thrOrdemCarga = New Thread(AddressOf ExecThrOrdemCarga)
            thrOrdemCarga.Start()
        Else
            bStartThrOrdemCarga = False
            Thread.Sleep(3000)
            thrTimerOrdemCarga.Dispose()
            thrTimerOrdemCarga = Nothing
            thrOrdemCarga.Abort()
            thrOrdemCarga = Nothing
        End If
    End Sub

    Private Sub ExecThrOrdemCarga()
        thrTimerOrdemCarga = New Threading.Timer(AddressOf thr_TimerOrdemCarga_Tick, Nothing, 0, intervalOrdemCarga)
    End Sub

    Private Sub thr_TimerOrdemCarga_Tick(ByVal stare As Object)
        If bStartThrOrdemCarga Then
            ControlaThreadImpressao.WaitOne()
            Select Case sCliente
                Case "BALCANES" : ImpBalcanesOrdemCarga()
                Case "BUDEGUER" : ImpBudeguerOrdemCarga()
            End Select
        End If
    End Sub

    Private Sub ImpBalcanesOrdemCarga()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        If frmReimpressao.fVerificaImprBalcanes("lt_despachos", "pfg_ordem_carga", " and fg_status = 'A'") = True Then
            If frmReimpressao.fValImprOrdemCarga() = True Then
                If vGlobal.vImprTela = "N" Then
                    frmReimpressao.PrintOrdemCarga_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    frmReimpressao.PrintOrdemCarga_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    frmReimpressao.PrintOrdemCarga_Balcanes.Print()
                ElseIf vGlobal.vImprTela = "S" Then
                    Impressoes.ImprTela(frmReimpressao.PrintOrdemCarga_Balcanes)
                End If
            End If
        End If
    End Sub

    Private Sub ImpBudeguerOrdemCarga()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        ControlaThreadImpressao.Reset()
        Try
            If Impressoes.fValidaImprOrdemCarga() Then
                If vGlobal.vImprTela = "N" Then
                    PrintOrdemCarga_Budeguer.DefaultPageSettings.Landscape = False
                    PrintOrdemCarga_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintOrdemCarga_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintOrdemCarga_Budeguer.Print()
                ElseIf vGlobal.vImprTela = "S" Then
                    Impressoes.ImprTela(PrintOrdemCarga_Budeguer)
                End If
                AtualizaDespachoOCImpresso(vGlobal.nValor01)
            End If
        Finally
            ControlaThreadImpressao.Set()
        End Try
    End Sub

    '*** [FINAL] Thread Impressão da Ordem de Carga ***

    '*** [INICIO] Thread Impressão da Boletim de Analises ***

    Public Sub IniciaProcBoletimAnalises()
        If Not bStartThrBoletimAnalises Then
            bStartThrBoletimAnalises = True
            thrBoletimAnalises = New Thread(AddressOf ExecThrBoletimAnalises)
            thrBoletimAnalises.Start()
        Else
            bStartThrBoletimAnalises = False
            Thread.Sleep(3000)
            thrTimerBoletimAnalises.Dispose()
            thrTimerBoletimAnalises = Nothing
            thrBoletimAnalises.Abort()
            thrBoletimAnalises = Nothing
        End If
    End Sub

    Private Sub ExecThrBoletimAnalises()
        thrTimerBoletimAnalises = New Threading.Timer(AddressOf thr_TimerBoletimAnalises_Tick, Nothing, 0, intervalBoletimAnalises)
    End Sub

    Private Sub thr_TimerBoletimAnalises_Tick(ByVal stare As Object)
        If bStartThrBoletimAnalises Then
            Select Case vGlobal.sCliente
                'Case "LAREDO" : ImpBolAnalise()
                Case "BALCANES" : ImpBalcanesBoletimAnalises()
            End Select
        End If
    End Sub

    Private Sub ImpBalcanesBoletimAnalises()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        If frmReimpressao.fVerificBolAnl() = True Then
            If vGlobal.vImprTela = "N" Then
                frmReimpressao.PrintBolAnl_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                frmReimpressao.PrintBolAnl_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                frmReimpressao.PrintBolAnl_Balcanes.Print()
            ElseIf vGlobal.vImprTela = "S" Then
                Impressoes.ImprTela(frmReimpressao.PrintBolAnl_Balcanes)
            End If
            frmReimpressao.AtualizaBolAnl_Balcanes(vGlobal.gn03)
        End If
    End Sub

    Private Sub ImpBolAnalise()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        If frmReimpressao.fVerificaBolAnl() = True Then
            frmReimpressao.bBotaoImprimir = False
            If vGlobal.vImprTela = "N" Then
                RetratoPaisagemBematech("BOLETIM_LABORATORIO", vGlobal.vRetratoPaisagem)
                Impressoes.PrintBolAnl_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                If bImprimirBolAnl = False Then
                    bImprimirBolAnl = True
                    For x As Integer = 1 To vGlobal.vNumCopiasLaboratCana
                        Impressoes.PrintBolAnl_Laredo.Print()
                    Next x
                    bImprimirBolAnl = False
                End If
            ElseIf vGlobal.vImprTela = "S" Then
                Impressoes.ImprTela(Impressoes.PrintBolAnl_Laredo)
            End If
            frmReimpressao.AtualizaBolAnl_Laredo(vGlobal.noLiberacaoBolAnl)
        End If
    End Sub

    '*** [FINAL] Thread Impressão do Boletim de Analises ***

    '*** [INICIO] Thread Impressão do Extrato de Saida ***

    Public Sub IniciaProcExtratoSaida()
        If Not bStartThrExtratoSaida Then
            bStartThrExtratoSaida = True
            thrExtratoSaida = New Thread(AddressOf ExecThrExtratoSaida)
            thrExtratoSaida.Start()
        Else
            bStartThrExtratoSaida = False
            Thread.Sleep(3000)
            thrTimerExtratoSaida.Dispose()
            thrTimerExtratoSaida = Nothing
            thrExtratoSaida.Abort()
            thrExtratoSaida = Nothing
        End If
    End Sub

    Private Sub ExecThrExtratoSaida()
        'thrTimerExtratoSaida = New Threading.Timer(AddressOf thr_TimerExtratoSaida_Tick, Nothing, 0, intervalExtratoSaida)
        thrTimerExtratoSaida = New Threading.Timer(AddressOf thr_TimerExtratoSaida_Tick, Nothing, 0, Timeout.Infinite)
    End Sub

    Private Sub thr_TimerExtratoSaida_Tick(ByVal stare As Object)
        'ControlaThreadImpressao.WaitOne()
        Try
            Select Case sCliente
                Case "LAREDO" : Impressoes.Laredo.ImprBolSaida.ImpLaredoExtratoSaida()
                Case "BALCANES" : ImpBalcanesExtratoSaida()
                Case "BUDEGUER" : ImpBudeguerExtratoSaida()
            End Select
        Catch ex As Exception
        Finally
            bImprimindoBolEntr = False

            ' Rearma o Timer para disparar após o intervalo desejado (se ainda estiver ativo)
            If bStartThrExtratoSaida AndAlso thrTimerExtratoSaida IsNot Nothing Then
                thrTimerExtratoSaida.Change(intervalExtratoSaida, Timeout.Infinite)
            End If
        End Try
    End Sub

    Private Sub ImpBalcanesExtratoSaida()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        If frmReimpressao.fVerificaImprBalcanes("apt_cargas", "pfg_impr_extr_saida", " and dt_saida is not null and rownum = 1") = True Then
            If vGlobal.vIMPRLIBSEPARADA = "N" Then
                If vGlobal.vImprTela = "N" Then
                    frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    frmReimpressao.PrintExtratoSaida_Balcanes.Print()
                ElseIf vGlobal.vImprTela = "S" Then
                    Impressoes.ImprTela(frmReimpressao.PrintExtratoSaida_Balcanes)
                End If
                frmReimpressao.AtualizaExtratoSaida_Balcanes(vGlobal.sValor02, "")
            ElseIf vGlobal.vIMPRLIBSEPARADA = "S" Then
                vGlobal.sValor03 = frmReimpressao.fIdViagemImprSeparado()
                For i As Integer = 1 To frmReimpressao.fCountLiberViagem(vGlobal.sValor03)
                    vGlobal.sValor04 = frmReimpressao.fLiberacaoImprSeparado(vGlobal.sValor03, i)
                    If vGlobal.vImprTela = "N" Then
                        frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                        frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                        frmReimpressao.PrintExtratoSaida_Balcanes.Print()
                    ElseIf vGlobal.vImprTela = "S" Then
                        Impressoes.ImprTela(frmReimpressao.PrintExtratoSaida_Balcanes)
                    End If
                    frmReimpressao.AtualizaExtratoSaida_Balcanes(vGlobal.sValor02, vGlobal.sValor04)
                Next

            End If
        End If
    End Sub

    Private Sub ImpBudeguerExtratoSaida()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        ' 1. Fecha a cancela (muda para False). Se a Thread passar por aqui de novo, ela vai travar e esperar.
        ControlaThreadImpressao.Reset()
        Try
            If frmReimpressao.FVerifPesagCana() = True Then
                frmReimpressao.bBotaoImprimir = False
                If vGlobal.vImprTela = "N" Then
                    Impressoes.PrintExtratoSaida_Budeguer.DefaultPageSettings.Landscape = False
                    Impressoes.PrintExtratoSaida_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    Impressoes.PrintExtratoSaida_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    Impressoes.PrintExtratoSaida_Budeguer.Print()
                ElseIf vGlobal.vImprTela = "S" Then
                    Impressoes.ImprTela(Impressoes.PrintExtratoSaida_Budeguer)
                End If
                Impressoes.Budeguer.ImprExtratoSaida.AtualizaSaida(vGlobal.nValor10)
            End If
        Finally
            ' 2. Finalizou o processo (com sucesso ou erro)? Abre a cancela!
            ' Isso faz a Thread acordar e continuar a execução imediatamente.
            ControlaThreadImpressao.Set()
        End Try

    End Sub

    '*** [FINAL] Thread Impressão do Extrato de Saida ***

    '*** [INICIO] Thread Impressão do Boletim de Entrada ***

    'Public Sub ImprLaredoBolEntr()
    '    If fValidaConexao() = True Then
    '        If frmReimpressao.FVerBolEntrLaredo() = True Then
    '            frmReimpressao.bBotaoImprimir = False
    '            RetratoPaisagemBematech("BOLETIM_ENTRADA", vGlobal.vRetratoPaisagem)
    '            Impressoes.PrintBolEntr_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
    '            'For x As Integer = 1 To vGlobal.vNumCopiasBoletimentr
    '            '    Impressoes.PrintBolEntr_Laredo.Print()
    '            'Next
    '        ElseIf vGlobal.vImprTela = "S" Then
    '            Impressoes.ImprTela(Impressoes.PrintBolEntr_Laredo)
    '        End If
    '    End If
    'End Sub

    'Public Sub ImprLaredoBolEntr()
    '    If fValidaConexao() = True Then
    '        If frmReimpressao.FVerBolEntrLaredo() = True Then
    '            frmReimpressao.bBotaoImprimir = False
    '            RetratoPaisagemBematech("BOLETIM_ENTRADA", vGlobal.vRetratoPaisagem)
    '            Impressoes.PrintBolEntr_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
    '            Impressoes.PrintBolEntr_Laredo.PrinterSettings.Copies = vGlobal.vNumCopiasBoletimentr
    '        ElseIf vGlobal.vImprTela = "S" Then
    '            Impressoes.ImprTela(Impressoes.PrintBolEntr_Laredo)
    '        End If
    '    End If
    'End Sub

    Private Sub thr_TimerBolEntrLaredo_Tick(ByVal stare As Object)
        If vGlobal.vImprExtrEntr = "N" Then Exit Sub
        Try
            If vGlobal.sCliente = "LAREDO" Then
                fGravaLog("1. *** [Inicio impresión Boletim de Entrada] ***", "LOG_TESTE")
                Dim resultado = VerificaImpressao.FVerBolEntrLaredo()

                If resultado.sucesso Then
                    For Each item In resultado.lista
                        Laredo.listaDados = Laredo.ImprBolEntrada.Dados(item.liberacao)
                        If Not Laredo.listaDados.Any Then Exit Sub
                        Impressoes.ImprimirBolEntr_Laredo()
                        'AtualizaStatusImpressoes.AtualizaBolEntr_Laredo(noLiberacaoBolEntr)
                        For Each item2 In Laredo.listaDados
                            Dim fganalise = item2.fganalise
                            Select Case fganalise
                                Case "S"
                                    If vGlobal.vImprBolAnl = "N" Then Exit For
                                    fGravaLog("12. Impressao do boletim de analise", "LOG_TESTE")
                                    Impressoes.ImprimirTicketLaboratorio_Laredo()
                                    fGravaLog("13. Final da impressao do boletim de analise", "LOG_TESTE")
                            End Select
                        Next
                    Next




                    'Laredo.listaDados = Laredo.ImprBolEntrada.Dados()
                    'If Not Laredo.listaDados.Any Then Exit Sub
                    'Impressoes.ImprimirBolEntr_Laredo()
                    ''AtualizaStatusImpressoes.AtualizaBolEntr_Laredo(noLiberacaoBolEntr)
                    'For Each item In Laredo.listaDados
                    '    Dim fganalise = item.fganalise
                    '    Select Case fganalise
                    '        Case "S"
                    '            If vGlobal.vImprBolAnl = "N" Then Exit For
                    '            fGravaLog("12. Impressao do boletim de analise", "LOG_TESTE")
                    '            Impressoes.ImprimirTicketLaboratorio_Laredo()
                    '            fGravaLog("13. Final da impressao do boletim de analise", "LOG_TESTE")
                    '    End Select
                    'Next
                End If
            End If
            fGravaLog("14. *** [Final impresión Boletín de Entrada] ***", "LOG_TESTE")
        Catch ex As Exception
            'Incluir aqui uma chamada de exceção para log de erro, caso necessário
        Finally
            bImprimindoBolEntr = False

            ' Rearma o Timer para disparar após o intervalo desejado (se ainda estiver ativo)
            If bStartThrBolentr AndAlso thrTimerBolEntr IsNot Nothing Then
                thrTimerBolEntr.Change(intervalBoletimEntr, Timeout.Infinite)
            End If
        End Try
    End Sub

    Private Sub ExecThrBolEntrLaredo()
        'thrTimerBolEntr = New Threading.Timer(AddressOf thr_TimerBolEntrLaredo_Tick, Nothing, 0, intervalBoletimEntr)
        thrTimerBolEntr = New Threading.Timer(AddressOf thr_TimerBolEntrLaredo_Tick, Nothing, 0, Timeout.Infinite)
    End Sub

    Public Sub IniciaProcBolEntrLaredo()
        If Not bStartThrBolentr Then
            bStartThrBolentr = True
            ExecThrBolEntrLaredo()
        Else
            bStartThrBolentr = False
            If thrTimerBolEntr IsNot Nothing Then
                thrTimerBolEntr.Dispose()
                thrTimerBolEntr = Nothing
            End If
        End If
    End Sub

    'Public Sub IniciaProcBolEntrLaredo()
    '    'If Not bStartThrBolentr Then
    '    '    bStartThrBolentr = True
    '    '    thrBolEntr = New Thread(AddressOf ExecThrBolEntrLaredo)
    '    '    thrBolEntr.Start()
    '    'Else
    '    '    bStartThrBolentr = False
    '    '    Thread.Sleep(3000)
    '    '    thrTimerBolEntr.Dispose()
    '    '    thrTimerBolEntr = Nothing
    '    '    thrBolEntr.Abort()
    '    '    thrBolEntr = Nothing
    '    'End If
    '    If Not bStartThrBolentr Then
    '        bStartThrBolentr = True
    '        ExecThrBolEntrLaredo()
    '    Else
    '        bStartThrBolentr = False
    '        If thrTimerBolEntr IsNot Nothing Then
    '            thrTimerBolEntr.Dispose()
    '            thrTimerBolEntr = Nothing
    '        End If
    '    End If
    'End Sub


    '*** [FINAL] Thread Impressão do Boletim de Entrada ***

    '*** [INICIO] Thread Impressão Extrato de Produdo ***

    Public Sub IniciaProcExtratoProduto()
        If Not bStartThrExtratoProduto Then
            bStartThrExtratoProduto = True
            thrExtratoProduto = New Thread(AddressOf ExecThrExtratoProduto)
            thrExtratoProduto.Start()
        Else
            bStartThrExtratoProduto = False
            Thread.Sleep(3000)
            thrTimerExtratoProduto.Dispose()
            thrTimerExtratoProduto = Nothing
            thrExtratoProduto.Abort()
            thrExtratoProduto = Nothing
        End If
    End Sub

    Private Sub ExecThrExtratoProduto()
        'thrTimerExtratoProduto = New Threading.Timer(AddressOf thr_TimerExtratoProduto_Tick, Nothing, 0, intervalExtratoProduto)
        thrTimerExtratoProduto = New Threading.Timer(AddressOf thr_TimerExtratoProduto_Tick, Nothing, 0, Timeout.Infinite)
    End Sub

    Private Sub thr_TimerExtratoProduto_Tick(ByVal stare As Object)
        Try
            If bStartThrExtratoProduto Then
                Select Case sCliente
                    Case "LAREDO" : ImpExtrProduto_Laredo()
                    Case "BUDEGUER" : ImpExtrProduto_Budeguer()
                End Select
            End If
        Catch ex As Exception
        Finally
            bImprimindoBolEntr = False

            ' Rearma o Timer para disparar após o intervalo desejado (se ainda estiver ativo)
            If bStartThrExtratoProduto AndAlso thrTimerExtratoProduto IsNot Nothing Then
                thrTimerExtratoProduto.Change(intervalExtratoProduto, Timeout.Infinite)
            End If
        End Try
    End Sub

    Private Sub ImpExtrProduto_Laredo()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        If frmReimpressao.FVerifPesagProd() = True Then
            frmReimpressao.bBotaoImprimir = False
            If vGlobal.vImprTela = "N" Then
                RetratoPaisagemBematech("EXTRATO_PRODUTO", vGlobal.vRetratoPaisagem)
                Impressoes.PrintPesagProdutos_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                For x As Integer = 1 To vGlobal.vNumCopiasExtrProduto
                    Impressoes.PrintPesagProdutos_Laredo.Print()
                Next
            ElseIf vGlobal.vImprTela = "S" Then
                Impressoes.ImprTela(Impressoes.PrintPesagProdutos_Laredo)
            End If
        End If
    End Sub

    Private Sub ImpExtrProduto_Budeguer()
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        If frmReimpressao.FVerifPesagProd() = True Then
            If FBuscaRemito(vGlobal.gn20) > 0 Then
                fGravaLogDebug("Iniciando a chamada de impressão >>> ", "LOG_DEBUG")
                fGravaLogDebug("Pesagem: " & vGlobal.gn20, "LOG_DEBUG")
                frmReimpressao.bBotaoImprimir = False
                If vGlobal.vImprTela = "N" Then
                    frmReimpressao.PrintExtrProduto_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    frmReimpressao.PrintExtrProduto_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    frmReimpressao.PrintExtrProduto_Budeguer.Print()
                ElseIf vGlobal.vImprTela = "S" Then
                    Impressoes.ImprTela(frmReimpressao.PrintExtrProduto_Budeguer)
                End If
            Else
                If frmReimpressao.bBotaoImprimir = True Then
                    MsgBox("No tiene remito asociado a esta orden de pesaje, impresión no permitida",
                           MsgBoxStyle.Exclamation, "Aviso")
                End If
            End If
        End If

    End Sub

    '*** [FINAL] Thread Impressão Extrato de Produdo - Laredo ***

    '*** [INICIO] Thread Impressão Ticket Rastreio ***
    Public Sub IniciaProcTicketRastreio()
        If Not bStartThrTicketRast Then
            bStartThrTicketRast = True
            thrTicketRast = New Thread(AddressOf ExecThrTicketRastreio)
            thrTicketRast.Start()
        Else
            bStartThrTicketRast = False
            Thread.Sleep(3000)
            thrTimerTicketRast.Dispose()
            thrTimerTicketRast = Nothing
            thrTicketRast.Abort()
            thrTicketRast = Nothing
        End If
    End Sub

    Private Sub ExecThrTicketRastreio()
        thrTimerTicketRast = New Threading.Timer(AddressOf thr_TimerTicketRast_Tick, Nothing, 0, intervalTicketRast)
    End Sub

    Private Sub thr_TimerTicketRast_Tick(ByVal stare As Object)
        If bStartThrTicketRast Then
            ControlaThreadImpressao.WaitOne()
            Select Case sCliente
                Case "BUDEGUER" : ImpTicketRast()
            End Select
        End If
    End Sub

    Private Sub ImpTicketRast()
        ControlaThreadImpressao.Reset()
        Try
            If frmReimpressao.FVerifTiketRast() = True Then
                frmReimpressao.bBotaoImprimir = False
                If vGlobal.vImprTela = "N" Then
                    Impressoes.PrintTicketRast_Budeguer.DefaultPageSettings.Landscape = False
                    Impressoes.PrintTicketRast_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    Impressoes.PrintTicketRast_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    Impressoes.PrintTicketRast_Budeguer.Print()
                    frmReimpressao.AtualizaTicketRast(vGlobal.gn20)
                ElseIf vGlobal.vImprTela = "S" Then
                    Impressoes.ImprTela(Impressoes.PrintTicketRast_Budeguer)
                    frmReimpressao.AtualizaTicketRast(vGlobal.gn20)
                End If
            End If
        Finally
            ControlaThreadImpressao.Set()
        End Try
    End Sub
    '*** [FINAL] Thread Impressão Ticket Rastreio ***
End Module
