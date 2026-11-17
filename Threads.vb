Imports System.Threading
Imports System.Data.OracleClient
Module Threads
    Dim Arquivo As String = Application.StartupPath
    Public t As New Timers.Timer
    Public intervalOrdemCarga As Integer = ArquivosTXT.lerINI("" & Arquivo & "\configuracao.ini", "PARAMETROS", "TIMER_IMPR_ORDEMCARGA")
    Public intervalBoletimAnalises As Integer = ArquivosTXT.lerINI("" & Arquivo & "\configuracao.ini", "PARAMETROS", "TIMER_IMPR_BOLANL")
    Public intervalExtratoSaida As Integer = ArquivosTXT.lerINI("" & Arquivo & "\configuracao.ini", "PARAMETROS", "TIMER_IMPR_EXTRSAIDA")
    Public intervalExtratoProduto As Integer = ArquivosTXT.lerINI("" & Arquivo & "\configuracao.ini", "PARAMETROS", "TIMER_IMPR_EXTRPROD")

    Dim vModeloOrdemCarga As String = ArquivosTXT.lerINI("" & Arquivo & "\configuracao.ini", "PARAMETROS", "MODELO_ORDEMCARGA")

    Public thrOrdemCarga As Thread
    Public thrBoletimAnalises As Thread
    Public thrExtratoSaida As Thread
    Public thrExtratoProduto As Thread

    Public thrTimerOrdemCarga As Threading.Timer
    Public bStartThrOrdemCarga As Boolean = False

    Public thrTimerBoletimAnalises As Threading.Timer
    Public bStartThrBoletimAnalises As Boolean = False

    Public thrTimerExtratoSaida As Threading.Timer
    Public bStartThrExtratoSaida As Boolean = False

    Public thrTimerExtratoProduto As Threading.Timer
    Public bStartThrExtratoProduto As Boolean = False

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
            If vModeloOrdemCarga = "01" Then
                ImpBalcanesOrdemCarga()
            ElseIf vModeloOrdemCarga = "02" Then
                ImpBudeguerOrdemCarga()
            End If
        End If
    End Sub

    Private Sub ImpBalcanesOrdemCarga()
        If fValidaConexao() = True Then
            fGravaLog("Inicio impresión Orden de Carga", "LOG_TIMER_IMPR")
            If frmReimpressao.fVerificaImprBalcanes("lt_despachos", "pfg_ordem_carga", " and fg_status = 'A'") = True Then
                If frmReimpressao.fValImprOrdemCarga() = True Then
                    frmReimpressao.PrintOrdemCarga_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    frmReimpressao.PrintOrdemCarga_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    frmReimpressao.PrintOrdemCarga_Balcanes.Print()
                    frmReimpressao.AtualizaDespachoOCImpresso(vGlobal.sValor01)
                End If
            End If
            fGravaLog("Final impresión Orden de Carga", "LOG_TIMER_IMPR")
        End If
    End Sub

    Private Sub ImpBudeguerOrdemCarga()
        If fValidaConexao() = True Then
            fGravaLog("Inicio impresión Orden de Carga", "LOG_TIMER_IMPR")
            If frmReimpressao.fVerificaImprBalcanes("lt_despachos", "pfg_ordem_carga", " and fg_status = 'A'") = True Then
                If frmReimpressao.fValImprOrdemCarga() = True Then
                    frmReimpressao.PrintOrdemCarga_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    frmReimpressao.PrintOrdemCarga_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    frmReimpressao.PrintOrdemCarga_Budeguer.Print()
                    frmReimpressao.AtualizaDespachoOCImpresso(vGlobal.sValor01)
                End If
            End If
            fGravaLog("Final impresión Orden de Carga", "LOG_TIMER_IMPR")
        End If
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
            If vGlobal.sCliente = "LAREDO" Then
                ImpBolAnalise()
            ElseIf vGlobal.sCliente = "BALCANES" Then
                ImpBalcanesBoletimAnalises()
            End If
        End If
    End Sub

    Private Sub ImpBalcanesBoletimAnalises()
        If fValidaConexao() = True Then
            fGravaLog("Inicio impresión Taló de Laboratorio", "LOG_TIMER_IMPR")
            If frmReimpressao.fVerificBolAnl() = True Then
                frmReimpressao.PrintBolAnl_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                frmReimpressao.PrintBolAnl_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                frmReimpressao.PrintBolAnl_Balcanes.Print()
                frmReimpressao.AtualizaBolAnl_Balcanes(vGlobal.gn03)
            End If
            fGravaLog("Final impresión Taló de Laboratorio", "LOG_TIMER_IMPR")
        End If
    End Sub

    Private Sub ImpBolAnalise()
        If fValidaConexao() = True Then
            fGravaLog("Inicio impresión Taló de Laboratorio", "LOG_TIMER_IMPR")
            If frmReimpressao.fVerificaBolAnl() = True Then
                frmReimpressao.bBotaoImprimir = False
                frmReimpressao.PrintBolAnl_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                frmReimpressao.PrintBolAnl_Laredo.PrinterSettings.Copies = vGlobal.vNumeroCopias
                frmReimpressao.PrintBolAnl_Laredo.Print()
                frmReimpressao.AtualizaBolAnl_Laredo(vGlobal.gn03)
            End If
            fGravaLog("Final impresión Taló de Laboratorio", "LOG_TIMER_IMPR")
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
        thrTimerExtratoSaida = New Threading.Timer(AddressOf thr_TimerExtratoSaida_Tick, Nothing, 0, intervalExtratoSaida)
    End Sub

    Private Sub thr_TimerExtratoSaida_Tick(ByVal stare As Object)
        If bStartThrExtratoSaida Then
            ImpBalcanesExtratoSaida()
        End If
    End Sub

    Private Sub ImpBalcanesExtratoSaida()
        If fValidaConexao() = True Then
            fGravaLog("Inicio impresión Extrato de Salida", "LOG_TIMER_IMPR")
            If frmReimpressao.fVerificaImprBalcanes("apt_cargas", "pfg_impr_extr_saida", " and dt_saida is not null and rownum = 1") = True Then
                If vGlobal.vIMPRLIBSEPARADA = "N" Then
                    frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    frmReimpressao.PrintExtratoSaida_Balcanes.Print()
                    frmReimpressao.AtualizaExtratoSaida_Balcanes(vGlobal.sValor02, "")
                ElseIf vGlobal.vIMPRLIBSEPARADA = "S" Then
                    vGlobal.sValor03 = frmReimpressao.fIdViagemImprSeparado()
                    For i As Integer = 1 To frmReimpressao.fCountLiberViagem(vGlobal.sValor03)
                        vGlobal.sValor04 = frmReimpressao.fLiberacaoImprSeparado(vGlobal.sValor03, i)
                        frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                        frmReimpressao.PrintExtratoSaida_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                        frmReimpressao.PrintExtratoSaida_Balcanes.Print()
                        frmReimpressao.AtualizaExtratoSaida_Balcanes(vGlobal.sValor02, vGlobal.sValor04)
                    Next

                End If
            End If
            fGravaLog("Final impresión Extrato de Salida", "LOG_TIMER_IMPR")
        End If
    End Sub
    '*** [FINAL] Thread Impressão do Extrato de Saida ***

    '*** [INICIO] Thread Impressão Extrato de Produdo - Laredo ***
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
        thrTimerExtratoProduto = New Threading.Timer(AddressOf thr_TimerExtratoProduto_Tick, Nothing, 0, intervalExtratoProduto)
    End Sub

    Private Sub thr_TimerExtratoProduto_Tick(ByVal stare As Object)
        If bStartThrExtratoProduto Then
            ImpExtrProduto_Laredo()
        End If
    End Sub

    Private Sub ImpExtrProduto_Laredo()
        If fValidaConexao() = True Then
            fGravaLog("Inicio impresión extracto de productos", "LOG_TIMER_IMPR")
            If frmReimpressao.FVerifPesagProd() = True Then
                frmReimpressao.bBotaoImprimir = False
                frmReimpressao.PrintPesagProdutos_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                frmReimpressao.PrintPesagProdutos_Laredo.PrinterSettings.Copies = vGlobal.vNumeroCopias
                frmReimpressao.PrintPesagProdutos_Laredo.Print()
            End If
            fGravaLog("Final impresión extracto de productos", "LOG_TIMER_IMPR")
        End If
    End Sub

    '*** [FINAL] Thread Impressão Extrato de Produdo - Laredo ***
End Module
