'Imports System.Data.OracleClient
Imports System.Drawing.Printing
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms.AxHost
Imports Oracle.ManagedDataAccess.Client

Public Class frmReimpressao
    Dim nContador As Integer = 0
    Dim nContadorTimer As Integer
    Public bFecharForm As Boolean = False
    Public rp As New RawPrinterHelper
    Dim sWhere As String
    Public bBotaoImprimir As Boolean = False
    Public bImpressoOrdemCargas As Boolean
    Dim bImpressoExtrato As Boolean
    Dim sMensagem As String
    Private alturaLinha As Integer
    Private nLoopLinhas As Integer
    Private vRelatImpr As String

    Private WithEvents TimerAtualizaListas As New System.Windows.Forms.Timer()


    Private Sub frmReimpressao_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        DiretorioDll()

        GeraArquivoINI.CriaArquivoConfic()

        FechaAplic()
        ListaDicionarioParametros()

        Conexao.conn = Conexao.fConexao()
        IniciarVerificadorConexao() 'INICIA O VERIFICADOR DE CONEXÃO COM O BANCO DE DADOS

        Try
            If Not TestarConexaoSilencioso() Then
                MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
                Return
            End If

            AtualizaListasCasdastros()
            vGlobal.FgImprBoletiEntrLaredo = False

        Catch ex As Exception
            fGravaLogDebug($"{Now()} Atualização de Cadastros >> {ex.ToString}", "LOG_ERROR")
            fGravaLogDebug($"{Now()} Falha na sincronização de cadatros >> {ex.ToString}", "LOG_ERROR")
            fGravaLogDebug($"{Now()} Serviços não iniciados >> {ex.ToString}", "LOG_ERROR")
            Exit Sub
        End Try


        ParamInicial()

        IniciaProcessos()

        IniciaImpressora()

        IniciaTimerConexao()


        IniciaDictStatusOdemPesagem() 'Inicia o agendador de atualização das ordem de pesagem de produtos


    End Sub

    Private Sub frmReimpressao_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        AjusteFinalTela()
        OcultaCheckBox()
        TextoModeloImpr()
    End Sub

    Private Sub IniciaProcessos()
        Select Case sCliente
            Case "BALCANES"
                lbTexto.Text = "Camión:"
                Me.Width = 550 : Me.Height = 160
                ConfiguracaoBotao(rbOrdemCarga, vGlobal.vImprOrdemCarga, AddressOf IniciaProcOrdemCarga)
                ConfiguracaoBotao(rbExtratoSaida, vGlobal.vImprExtrSaida, AddressOf IniciaProcExtratoSaida)
                ConfiguracaoBotao(rbBolAnl, vGlobal.vImprBolAnl, AddressOf IniciaProcBoletimAnalises)
            Case "BUDEGUER"
                lbTexto.Text = "Camión:"
                Me.Width = 550 : Me.Height = 160
                ConfiguracaoBotao(rbOrdemCarga, vGlobal.vImprOrdemCarga, AddressOf IniciaProcOrdemCarga)
                ConfiguracaoBotao(rbExtratoSaida, vGlobal.vImprExtrSaida, AddressOf IniciaProcExtratoSaida)
                ConfiguracaoBotao(RbTicketRast, vGlobal.vImprTicketRast, AddressOf IniciaProcTicketRastreio)
                ConfiguracaoBotao(RbExtProducto, vGlobal.vImprExtrProduto, AddressOf IniciaProcExtratoProduto)
            Case "LAREDO"
                'ConfiguracaoBotao(rbBolAnl, vGlobal.vImprBolAnl, AddressOf IniciaProcBoletimAnalises)
                ConfiguracaoBotao(RbBoletimEntr, vGlobal.vImprExtrEntr, AddressOf IniciaProcBolEntrLaredo)
                ConfiguracaoBotao(rbExtratoSaida, vGlobal.vImprExtrSaida, AddressOf IniciaProcExtratoSaida)
                ConfiguracaoBotao(RbExtProducto, vGlobal.vImprExtrProduto, AddressOf IniciaProcExtratoProduto)
            Case Else
                Me.Width = 550 : Me.Height = 100
                OcultaCheckBox()
        End Select
    End Sub

    Private Sub btImprimir_Click(sender As Object, e As EventArgs) Handles btImprimir.Click
        If Not TestarConexaoSilencioso() Then
            MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
            Return
        End If

        vGlobal.bBotaoImprimir = True
        vGlobal.nRegistroImpressao = tbBoletim.Text

        If String.IsNullOrWhiteSpace(tbBoletim.Text) Then Exit Sub
        Select Case vGlobal.vModeloImpressao
            Case "01" : ImprimirModelo01()
            Case "02" : ImprimirModelo02()
            Case "03" : ImprimirModelo03()
            Case "00" : ImprimirModeloBalcanes()
            Case "04" : ImprimirModeloBudeguer()
        End Select
        vGlobal.bBotaoImprimir = False
        tbBoletim.Clear()
    End Sub

    Private Sub DiretorioDll()
        ' Configura para procurar DLLs na pasta "libs"
        AddHandler AppDomain.CurrentDomain.AssemblyResolve,
    Function(sender2, args)
        Dim dllName = New Reflection.AssemblyName(args.Name).Name & ".dll"
        Dim dllPath = IO.Path.Combine(Application.StartupPath, "libs", dllName)
        If IO.File.Exists(dllPath) Then
            Return Reflection.Assembly.LoadFile(dllPath)
        End If
        Return Nothing
    End Function

    End Sub

    '*********************************************
    'Novo metodo para realização da impressão de documentos
    '*********************************************
    Private Sub ImprimirDocumento(doc As Printing.PrintDocument, copias As Integer,
                                  Optional relatorio As String = Nothing, Optional condicao As Func(Of Boolean) = Nothing)
        If condicao IsNot Nothing AndAlso Not condicao() Then Exit Sub

        If vImprTela = "N" Then
            If Not String.IsNullOrEmpty(relatorio) Then
                RetratoPaisagemBematech(relatorio, vGlobal.vRetratoPaisagem)
            End If
            doc.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
            For i As Integer = 1 To copias
                doc.Print()
            Next
        Else
            Impressoes.ImprTela(doc)
        End If
    End Sub

    Private Sub LimparBoletim()
        If bBotaoImprimir Then
            bBotaoImprimir = False
            tbBoletim.Clear()
        End If
    End Sub

    Private Sub AtualizaListasCasdastros()
        cadparametros = Cadastros.Parametros.DictParam()
        cadempresas = Cadastros.Empresas.CadEmpresas()
        cadtransportadora = Cadastros.Transportadora.Dados()
        caddestinatario = Cadastros.Destinatario.Dados()
        cadtipomovimento = Cadastros.TipoMovimento.Dados()
        cadproduto = Cadastros.Produtos.Dados()
        cadempresadeposito = Cadastros.EmpresaDeposito.Dados()
        cadfornectransp = Cadastros.CadFornTransp.Dados()
        cadupnivel1 = Cadastros.Upnivel1.Dados()
        caddepositoprod = Cadastros.DepositoProdutos.Dados()
        cadveiculos = Cadastros.CadVeiculos.Dados()
        cadequiptos = Cadastros.Equiptos.Dados()
        cadplacaequiptos = Cadastros.Equiptos.Placa.Dados()
        cadunidind = Cadastros.UnidadeIndustrial.Dados()
        cadfuncionarios = Cadastros.Funcionarios.Dados()
        cadtblbalanca = Cadastros.TblBalanca.Dados()
        ListaCadastrosSincr()
    End Sub

    Private DadosSincr As New Dictionary(Of String, Integer)
    Private Sub GeraLogDadosDictionary()
        Dim consulta As New Dictionary(Of String, Func(Of Integer)) From {
        {"Parâmetros", Function() If(cadparametros IsNot Nothing, cadparametros.Count, 0)},
        {"Empresas", Function() If(cadempresas IsNot Nothing, cadempresas.Count, 0)},
        {"Transportadoras", Function() If(cadtransportadora IsNot Nothing, cadtransportadora.Count, 0)},
        {"Destinatário", Function() If(caddestinatario IsNot Nothing, caddestinatario.Count, 0)},
        {"Tipo de Movimento", Function() If(cadtipomovimento IsNot Nothing, cadtipomovimento.Count, 0)},
        {"Produtos", Function() If(cadproduto IsNot Nothing, cadproduto.Count, 0)},
        {"Empresa Deposito", Function() If(cadempresadeposito IsNot Nothing, cadempresadeposito.Count, 0)},
        {"Fornecedor Transporte", Function() If(cadfornectransp IsNot Nothing, cadfornectransp.Count, 0)},
        {"Upnivel1", Function() If(cadupnivel1 IsNot Nothing, cadupnivel1.Count, 0)},
        {"Deposito Produtos", Function() If(caddepositoprod IsNot Nothing, caddepositoprod.Count, 0)},
        {"Veículos", Function() If(cadveiculos IsNot Nothing, cadveiculos.Count, 0)},
        {"Equipamentos", Function() If(cadequiptos IsNot Nothing, cadequiptos.Count, 0)},
        {"Placa Equipamento", Function() If(cadplacaequiptos IsNot Nothing, cadplacaequiptos.Count, 0)},
        {"Unidade Industrial", Function() If(cadunidind IsNot Nothing, cadunidind.Count, 0)},
        {"Funcionários", Function() If(cadfuncionarios IsNot Nothing, cadfuncionarios.Count, 0)},
        {"Balanças", Function() If(cadtblbalanca IsNot Nothing, cadtblbalanca.Count, 0)}
        }

        For Each item In consulta
            DadosSincr.Add(item.Key, item.Value.Invoke)
        Next

    End Sub

    Private Sub ListaCadastrosSincr()
        GeraLogDadosDictionary()
        ' Exemplo: chave é uma tupla (seu caso), valor é String
        Dim dados As Dictionary(Of String, Integer) = DadosSincr

        ' Caminho do arquivo de log
        Dim caminhoArquivo As String = Application.StartupPath & "\logs\DicionarioCadastros.txt"

        ' Usando StreamWriter para gravar
        Using sw As New StreamWriter(caminhoArquivo, False) ' False = sobrescreve, True = append
            ' Cabeçalho
            sw.WriteLine("=== DICIONARIO DE CADASTROS ===")
            sw.WriteLine("      QUANTIDADE DE DADOS      ")
            sw.WriteLine("Data/Hora: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            sw.WriteLine("--------------------------------------------------")

            ' Percorre o Dictionary e grava chave + valor
            For Each kvp As KeyValuePair(Of String, Integer) In dados
                ' A chave é uma tupla, então você acessa com .Item1 e .Item2
                sw.WriteLine($"{kvp.Key}: [{kvp.Value}]")
            Next

            sw.WriteLine("--------------------------------------------------")
            sw.WriteLine()
        End Using

        Console.WriteLine("Arquivo gravado em: " & caminhoArquivo)
    End Sub

    '*********************************************
    'Final do novo metos implementado
    '*********************************************

    Private Sub TimerAtualizaListas_Tick(sender As Object, e As EventArgs) Handles TimerAtualizaListas.Tick
        AtualuzaListaCadastros()
    End Sub

    Private Sub AtualuzaListaCadastros()
        cadparametros = Cadastros.Parametros.DictParam()
        cadempresas = Cadastros.Empresas.CadEmpresas()
        cadtransportadora = Cadastros.Transportadora.Dados()
        caddestinatario = Cadastros.Destinatario.Dados()
        cadtipomovimento = Cadastros.TipoMovimento.Dados()
        cadproduto = Cadastros.Produtos.Dados()
        cadempresadeposito = Cadastros.EmpresaDeposito.Dados()
        cadfornectransp = Cadastros.CadFornTransp.Dados()
        cadupnivel1 = Cadastros.Upnivel1.Dados()
        caddepositoprod = Cadastros.DepositoProdutos.Dados()
        cadveiculos = Cadastros.CadVeiculos.Dados()
        cadequiptos = Cadastros.Equiptos.Dados()
        cadplacaequiptos = Cadastros.Equiptos.Placa.Dados()
        cadunidind = Cadastros.UnidadeIndustrial.Dados()
        cadfuncionarios = Cadastros.Funcionarios.Dados()
        cadtblbalanca = Cadastros.TblBalanca.Dados()
    End Sub

    Private Sub AtualizaOutrPesagens(noPesagem As Integer)
        Using t As OracleTransaction = conn.BeginTransaction
            Try
                Dim sql As New StringBuilder
                sql.AppendLine("update outr_pesagens")
                sql.AppendLine("set pfg_impr_extrato = :pfg_impr_extrato_s")
                sql.AppendLine("where instancia = :instancia")
                sql.AppendLine("  and no_pesagem = :no_pesagem")
                sql.AppendLine("  and pfg_impr_extrato = :pfg_impr_extrato_n")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Transaction = t
                    cmd.Parameters.Add("pfg_impr_extrato_s", OracleDbType.Varchar2).Value = "S"
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("no_pesagem", OracleDbType.Int32).Value = noPesagem
                    cmd.Parameters.Add("pfg_impr_extrato_n", OracleDbType.Varchar2).Value = "N"
                    cmd.ExecuteNonQuery()
                End Using
                t.Commit()
            Catch ex As Exception
                t.Rollback()
                vGlobal.vMensangem = ex.ToString
                frmMensagem.Show()
                vGlobal.vMensangem = Nothing
            End Try
        End Using
    End Sub

    Public Sub AtualizaTicketRast(noLiberacao As Integer)
        Using t As OracleTransaction = conn.BeginTransaction
            Try
                Dim sql As New StringBuilder
                sql.AppendLine("update apt_cargas")
                sql.AppendLine("set pfg_impr_ticket_rast = 'S'")
                sql.AppendLine("where instancia = :instancia")
                sql.AppendLine("and no_liberacao = :no_liberacao")
                sql.AppendLine("and pfg_impr_ticket_rast = 'N'")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Transaction = t
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = noLiberacao
                    cmd.ExecuteNonQuery()
                End Using
                t.Commit()
            Catch ex As OracleException
                t.Rollback()
                fGravaError("OracleErr>> FrmReimpressao", "AtualizaTicketRast", ex.ToString, "LOG_ERROR")
            Catch ex As Exception
                t.Rollback()
                fGravaError("FrmReimpressao", "AtualizaTicketRast", ex.ToString, "LOG_ERROR")
            End Try
        End Using
    End Sub

    Private Sub FechaAplic()
        Dim s() As Process

        s = Process.GetProcessesByName("AppImpressao")
        If s.Length > 1 Then
            bFecharForm = True
            MsgBox("La aplicación ya está abierta" & vbCrLf &
                   "Ella va a ser cerrada", MsgBoxStyle.Information, "Aviso")
            Close()
        End If

    End Sub

    Private Sub NotifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles NotifyIcon1.DoubleClick
        Show()
        WindowState = FormWindowState.Normal
        NotifyIcon1.Visible = False
    End Sub

    Private Sub frmImprExtr_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        NotifyIcon1.Visible = False
        If WindowState = FormWindowState.Minimized Then
            Hide()
            NotifyIcon1.Visible = True
            NotifyIcon1.ShowBalloonTip("3", "LuxArvum", "Impresión del extracto en segun plan", ToolTipIcon.Info)
        End If
    End Sub

    Private Sub frmImprExtr_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If (MessageBox.Show("Desea salir de la aplicación", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes) Then
            Threads_.FinalizaThreads()
            NotifyIcon1.Visible = False
            bFecharForm = True
        Else
            If bFecharForm = False Then
                e.Cancel = True
                NotifyIcon1.Visible = True
                WindowState = FormWindowState.Minimized
            End If
        End If
    End Sub



    '*** LAREDO - INICIO ***

    Private Function FQuebraLinhasImpressao(texto) As String
        Try
            Dim tamObservacao As Integer = Len(texto)
            Dim tamStr As Integer = Integer.Parse(vNUMERO_CARACTER_LINHA_OBS)
            Dim nDivisao As Double = tamObservacao / tamStr
            Dim nLinhas As Integer = Math.Ceiling(nDivisao)
            Dim newTexto As String = Nothing

            For i = 0 To nLinhas
                nLoopLinhas += 1
                If i < 1 Then
                    If tamObservacao < tamStr Then
                        newTexto = texto.Substring(0, tamObservacao) & vbNewLine
                    Else
                        newTexto = texto.Substring(0, tamStr) & vbNewLine
                    End If
                ElseIf i >= 1 And i < nLinhas Then
                    newTexto += texto.Substring(i * tamStr, If(tamObservacao - (i * tamStr) < tamStr, tamObservacao - (i * tamStr), tamStr)) & vbNewLine
                    alturaLinha += 0
                Else
                    If tamObservacao - (i * tamStr) >= 1 Then
                        newTexto += texto.Substring(i * tamStr, tamObservacao - (i * tamStr)) & vbNewLine
                    End If
                End If
            Next

            'Espaço das linhas
            Dim nDivisaoEspaco As Double = nLoopLinhas / 2
            Dim nDivisaoInteiro As Integer = Math.Ceiling(nDivisaoEspaco)
            nLoopLinhas = nDivisaoInteiro

            fGravaLogDebug("+|-----------------------------------------|", "LOG_DEBUG")
            fGravaLogDebug("+|--QUEBRA DE LINHAS-----------------------|", "LOG_DEBUG")
            fGravaLogDebug("+|-----------------------------------------|", "LOG_DEBUG")
            fGravaLogDebug(newTexto, "LOG_DEBUG")
            fGravaLogDebug("+|-FINAL QUEBRA DE LINHAS------------------|", "LOG_DEBUG")
            fGravaLogDebug("+|-----------------------------------------|", "LOG_DEBUG")

            Return newTexto

        Catch ex As Exception

        End Try
    End Function

    Public Function FVerifPesagProd() As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select * from outr_pesagens" & vbNewLine
            sql += "where instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine
            sql += "  and pfg_impr_extrato = 'N'" & vbNewLine
            sql += "  and fg_status = 'C'"

            rs = fCmd(sql).ExecuteReader
            'vGlobal.gn20 = 0
            If rs.HasRows = True Then
                While rs.Read
                    vGlobal.gn20 = rs("no_pesagem")
                End While

                fCmd(sql).Dispose()
                rs.Close()

                If vGlobal.sCliente = "BUDEGUER" Then
                    If FPesagemBalanza(vGlobal.gn20, vNomePc) = True Then
                        If vGlobal.gn20 > 0 Then
                            AtualizaOutrPesagens(vGlobal.gn20)
                        End If
                        Return True
                    Else
                        If vGlobal.gn20 > 0 Then
                            AtualizaOutrPesagens(vGlobal.gn20)
                        End If
                        Return False
                    End If
                Else
                    If vGlobal.gn20 > 0 Then
                        AtualizaOutrPesagens(vGlobal.gn20)
                    End If
                    Return True
                End If
            Else
                fCmd(sql).Dispose()
                rs.Close()
                If vGlobal.gn20 > 0 Then
                    AtualizaOutrPesagens(vGlobal.gn20)
                End If
                Return False
            End If
        Catch ex As Exception

        End Try
    End Function

    Public Function fVerificaBolAnl() As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select * from apt_cargas c")
            sql.AppendLine("inner join parametros p on p.instancia = c.instancia")
            sql.AppendLine("where c.instancia = :instancia")
            sql.AppendLine("  and c.no_analise is not null")
            sql.AppendLine("  and c.fg_analise = :fganalise")
            sql.AppendLine("  and c.fg_impr_bolanl = :fgimprbolanl")
            sql.AppendLine("  and c.cd_unid_ind = :unidind")
            sql.AppendLine("  and p.secao = :secao")
            sql.AppendLine("  and p.entrada = :entrada")
            sql.AppendLine("  and c.dt_entrada > p.valor")
            sql.AppendLine("  and rownum = 1")

            fGravaLogDebug("***[Inicio fVerificaBolAnl]***", "LOG_DEBUG")
            fGravaLogDebug(sql.ToString, "LOG_DEBUG")
            fGravaLogDebug("***[Final fVerificaBolAnl]***", "LOG_DEBUG")

            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vGlobal.vInstLogin
                cmd.Parameters.Add("fganalise", OracleDbType.Varchar2).Value = "S"
                cmd.Parameters.Add("fgimprbolanl", OracleDbType.Varchar2).Value = "N"
                cmd.Parameters.Add("secao", OracleDbType.Varchar2).Value = "RCMP"
                cmd.Parameters.Add("entrada", OracleDbType.Varchar2).Value = "DT_PROCESSO"
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        result = rs("no_liberacao")
                    End While
                End Using
            End Using

            vGlobal.noLiberacaoBolAnl = result

            If result = Nothing Then
                Return False
            ElseIf result <> Nothing Then
                AtualizaBolAnl_Laredo(vGlobal.noLiberacaoBolAnl)
                Return True
            End If
        Catch ex As Exception

        End Try
    End Function

    Public Function FVerBolEntrLaredo() As Boolean
        Try
            fGravaLog("2. Mirando numero boletin", "LOG_TESTE")
            Dim swhere2 As String = Nothing
            Select Case vGlobal.vTICKETENTRSORT
                Case "N" : swhere2 = " and c.fg_analise = 'S'"
                Case "S" : swhere2 = " and (c.fg_analise in ('S','N') or c.fg_analise is null)"
            End Select

            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.AppendLine("select c.no_liberacao from apt_cargas c")
            sql.AppendLine("inner join parametros p on p.instancia = c.instancia")
            sql.AppendLine("where c.instancia = :instancia")
            sql.AppendLine("  and c.fg_impr_bol_entr = :fg_impr_bol_entr")
            sql.AppendLine("  and p.secao = :secao")
            sql.AppendLine("  and p.entrada = :entrada")
            sql.AppendLine("  and c.dt_entrada > p.valor")
            sql.AppendLine(swhere2)
            sql.AppendLine("  and rownum = 1")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vGlobal.vInstLogin
                cmd.Parameters.Add("fg_impr_bol_entr", OracleDbType.Varchar2).Value = "N" '''N
                cmd.Parameters.Add("secao", OracleDbType.Varchar2).Value = "RCMP"
                cmd.Parameters.Add("entrada", OracleDbType.Varchar2).Value = "DT_PROCESSO"
                Dim textsql As String = GetDebugSql(cmd)
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        noLiberacaoBolEntr = rs("no_liberacao")
                        'AtualizaBolEntr_Laredo(noLiberacaoBolEntr)
                        fGravaLog("5. Finalizando Mirando numero boletin", "LOG_TESTE")
                        Return True
                    End While
                End Using
                fGravaLog("6. Finalizando Mirando numero boletin", "LOG_TESTE")
                Return False
            End Using
        Catch ex As Exception

        End Try
    End Function

    '--+|------------------------------------------------------|
    '--+|-Impressão Boletim de Análise Laredo -----------------|
    '--+|------------------------------------------------------|

    'Private Sub PrintBolAnl_Laredo_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintBolAnl_Laredo.PrintPage
    '    Try
    '        Dim sEmpresa, sCaminhao, sCarreta, sPlaca, sPlacaCarreta, sMotorista, sEntrada, sLiberacao, sViagem, sPesoBruto, sAnalise As String
    '        Dim sCdNivel1, sDeNivel1 As String
    '        Dim alturaLinha As Integer
    '        Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
    '        Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
    '        Dim altura As Integer = 1000
    '        Dim barra As Image

    '        Dim sWhere As String
    '        Dim sFiltro As String
    '        If bBotaoImprimir = True Then
    '            sFiltro = tbBoletim.Text.Replace(";", ",")
    '            sWhere = " And c.no_analise in (" & sFiltro & ")"
    '        ElseIf bBotaoImprimir = False Then
    '            sWhere = " And c.no_liberacao in (" & noLiberacao & ")"
    '        End If

    '        Dim sql As String
    '        Dim rs As OracleDataReader
    '        sql += "select *" & vbNewLine
    '        sql += "from (" & vbNewLine
    '        sql += "  select c.no_liberacao," & vbNewLine
    '        sql += "         c.no_analise," & vbNewLine
    '        sql += "         c.hr_entrada," & vbNewLine
    '        sql += "         r.cd_tp_recurso," & vbNewLine
    '        sql += "         r.cd_equipto," & vbNewLine
    '        sql += "         trunc(u.cd_upnivel1)||' '||u.de_upnivel1 cd_upnivel1," & vbNewLine
    '        sql += "         c.id_viagem," & vbNewLine
    '        sql += "         c.qt_bruto" & vbNewLine
    '        sql += "from apt_cargas c" & vbNewLine
    '        sql += "inner join apt_cargas_rec r on r.instancia = c.instancia and r.no_liberacao = c.no_liberacao" & vbNewLine
    '        sql += "inner join upnivel1 u on u.cd_upnivel1 = c.cd_upnivel1" & vbNewLine
    '        sql += "inner join equiptos e on e.cd_equipto = r.cd_equipto" & vbNewLine
    '        sql += "where c.instancia = r.instancia" & vbNewLine
    '        sql += "  and c.no_liberacao = r.no_liberacao" & vbNewLine
    '        sql += "  and u.cd_upnivel1 = c.cd_upnivel1" & vbNewLine
    '        sql += "  and e.cd_equipto = r.cd_equipto" & vbNewLine
    '        sql += "  and r.cd_tp_recurso in ('CM', 'CT')" & vbNewLine
    '        sql += "  and c.no_analise is not null" & vbNewLine
    '        sql += "  and c.fg_analise = 'S'" & vbNewLine
    '        sql += " " & sWhere & " " & vbNewLine
    '        sql += "   and c.instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine
    '        sql += ")" & vbNewLine
    '        sql += "pivot (" & vbNewLine
    '        sql += "  max(cd_equipto)" & vbNewLine
    '        sql += "  for cd_tp_recurso in ('CM' AS CM, 'CT' AS CT))"

    '        vMensangem += "*** PrintBolAnl_Laredo ***" & vbNewLine
    '        vMensangem += sql & vbNewLine

    '        rs = fCmd(sql).ExecuteReader
    '        While rs.Read
    '            sLiberacao = rs("no_liberacao")
    '            If Not IsDBNull(rs("id_viagem")) Then
    '                sViagem = rs("id_viagem")
    '            Else
    '                sViagem = " "
    '            End If

    '            sCaminhao = rs("CM")
    '            If IsDBNull(rs("CT")) Then
    '                sCarreta = " "
    '            Else
    '                sCarreta = rs("CT")
    '                sPlacaCarreta = fBuscaPlaca(sCarreta, sLiberacao)
    '            End If
    '            sPlaca = fBuscaPlaca(sCaminhao, sLiberacao)
    '            sMotorista = fBuscaMotorista(sCaminhao, sLiberacao, "D")
    '            sEntrada = rs("hr_entrada")
    '            If Not IsDBNull(rs("qt_bruto")) Then
    '                sPesoBruto = rs("qt_bruto")
    '            Else
    '                sPesoBruto = " "
    '            End If
    '            sAnalise = rs("no_analise")
    '            sCdNivel1 = rs("cd_upnivel1")
    '        End While
    '        fCmd(sql).Dispose()
    '        rs.Close()

    '        sEmpresa = fGetParam("GERAIS", "DA_EMPRESA")
    '        sDeNivel1 = fGetParam("NIVEIS", "UPNIVEL1")

    '        alturaLinha = 10
    '        e.Graphics.DrawString("*** " & sEmpresa & " ***", FontesImpressao.Fonte10R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 40
    '        e.Graphics.DrawString("Ticket de Laboratorio", FontesImpressao.Fonte10R, Brushes.Black, 72, alturaLinha)

    '        alturaLinha += 30
    '        e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 20
    '        e.Graphics.DrawString("Camión:  " & sCaminhao & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        e.Graphics.DrawString("Placa:  " & sPlaca & "", FontesImpressao.Fonte9R, Brushes.Black, 122, alturaLinha)
    '        alturaLinha += 20
    '        e.Graphics.DrawString("Chofer:  " & sMotorista & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 20
    '        e.Graphics.DrawString("Fecha/Hora Entrada:  " & sEntrada & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 20
    '        e.Graphics.DrawString("Liberación: " & sLiberacao.PadLeft(8, "0"c), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        e.Graphics.DrawString("Nº Viaje: " & sViagem.PadLeft(8, "0"c), FontesImpressao.Fonte9R, Brushes.Black, 142, alturaLinha)
    '        alturaLinha += 30
    '        If sCarreta <> " " Then
    '            e.Graphics.DrawString("Remolque: " & sCarreta, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '            e.Graphics.DrawString("Placa: " & sCarreta, FontesImpressao.Fonte9R, Brushes.Black, 122, alturaLinha)
    '            alturaLinha += 20
    '        End If
    '        e.Graphics.DrawString("Peso Bruto: " & sPesoBruto, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 20

    '        'Linha 01
    '        Dim Point01 As New Point(2, 45)
    '        Dim Point11 As New Point(270, 45)
    '        Dim blackPen1 = New Pen(Color.Black, 1)
    '        e.Graphics.DrawLine(blackPen1, Point01, Point11)

    '        'Linha 02
    '        Dim Point02 As New Point(2, 70)
    '        Dim Point22 As New Point(270, 70)
    '        Dim blackPen2 = New Pen(Color.Black, 1)
    '        e.Graphics.DrawLine(blackPen2, Point02, Point22)

    '        'Linha 03
    '        Dim Point03 As New Point(2, 180)
    '        Dim Point33 As New Point(270, 180)
    '        Dim blackPen3 = New Pen(Color.Black, 1)
    '        e.Graphics.DrawLine(blackPen3, Point03, Point33)

    '        'Linha Patio
    '        Dim Point04 As New Point(2, 270)
    '        Dim Point44 As New Point(100, 270)
    '        Dim blackPen4 = New Pen(Color.Black, 1)
    '        e.Graphics.DrawLine(blackPen4, Point04, Point44)
    '        e.Graphics.DrawString("Patio", FontesImpressao.Fonte9R, Brushes.Black, 13, 275)

    '        'Linha laboratorio
    '        Dim Point05 As New Point(180, 270)
    '        Dim Point55 As New Point(270, 270)
    '        Dim blackPen5 = New Pen(Color.Black, 1)
    '        e.Graphics.DrawLine(blackPen5, Point05, Point55)
    '        e.Graphics.DrawString("Laboratorio", FontesImpressao.Fonte9R, Brushes.Black, 185, 275)

    '        barra = codbarras.Draw(sAnalise, altura)
    '        e.Graphics.DrawImage(barra, New Rectangle(70, 325, 140, 70))
    '        e.Graphics.DrawString(sAnalise.PadLeft(6, "0"c), FontesImpressao.Fonte9R, Brushes.Black, 70, 400)

    '        e.Graphics.DrawString(".", FontesImpressao.Fonte9R, Brushes.Black, 2, 440)

    '    Catch ex As Exception
    '        vMensangem = ex.ToString & vbNewLine
    '        frmMensagem.Show()
    '        vMensangem = Nothing
    '    End Try
    'End Sub

    Public Sub AtualizaBolAnl_Laredo(noLiberacao)
        Try
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "update apt_cargas" & vbNewLine &
                  "set fg_impr_bolanl = 'S'" & vbNewLine &
                  "where fg_impr_bolanl = 'N'" & vbNewLine &
                  "  and instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and no_liberacao = " & noLiberacao & ""
            fGravaLogDebug("***[Inicio AtualizaBolAnl_Laredo]***", "LOG_DEBUG")
            fGravaLogDebug(sql, "LOG_DEBUG")
            fGravaLogDebug("***[Final AtualizaBolAnl_Laredo]***", "LOG_DEBUG")
            rs = fCmd(sql).ExecuteReader
            fCmd(sql).Dispose()
            rs.Close()
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "AtualizaBolAnl_Laredo", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Public Sub AtualizaBolEntr_Laredo(noLiberacao)
        Using t As OracleTransaction = conn.BeginTransaction
            Try
                fGravaLog("3. Inicio da atualizado de status do boletim de entrada", "LOG_TESTE")
                Dim sql As New StringBuilder
                sql.AppendLine("update apt_cargas")
                sql.AppendLine("set fg_impr_bol_entr = :fg_impr_bol_entr_s")
                sql.AppendLine("where fg_impr_bol_entr = :fg_impr_bol_entr_n")
                sql.AppendLine("and instancia = :instancia")
                sql.AppendLine("and no_liberacao = :no_liberacao")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Transaction = t
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = noLiberacao
                    cmd.Parameters.Add("fg_impr_bol_entr_s", OracleDbType.Varchar2).Value = "S"
                    cmd.Parameters.Add("fg_impr_bol_entr_n", OracleDbType.Varchar2).Value = "N" '''' N
                    cmd.ExecuteNonQuery()
                End Using
                t.Commit()
                fGravaLog("4. Final da atualizado de status do boletim de entrada", "LOG_TESTE")
            Catch ex As Exception
                t.Rollback()
            End Try
        End Using
    End Sub


    '--+|------------------------------------------------------|
    '--+|-Impressão de Extrato de Saida Laredo ----------------|
    '--+|------------------------------------------------------|
    'Private Sub PrintExtratoSaida_Laredo_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Laredo.PrintPage
    '    Try
    '        Dim sEmpresa, sViagem, sNoSequencia, sMotorista, sCdFunc As String

    '        Dim HrInicial, HrFinal As DateTime

    '        Dim nTamMotorista As Integer
    '        Dim nTamNivel1_1, nTamNivel1_2, nTamNivel1_3, nTamNivel1_4 As Integer
    '        Dim nTamForn1_1, nTamForn1_2, nTamForn1_3, nTamForn1_4 As Integer

    '        'String primeira linha
    '        Dim sNoSequencia1, sCaminhao, sPlaca, sEntrada, sSaida, sCdFornec1, sCdNivel1_1, sDeNivel1, sLiberacao1, sPesoBruto1, sPesoTara1, sPesoNeto1 As String

    '        'String segunda linha
    '        Dim sNoSequencia2, sCarreta2, sCdFornec2, sCdNivel1_2, sLiberacao2, sPesoBruto2, sPesoTara2, sPesoNeto2 As String

    '        'String terceira linha
    '        Dim sNoSequencia3, sCarreta3, sCdFornec3, sCdNivel1_3, sLiberacao3, sPesoBruto3, sPesoTara3, sPesoNeto3 As String

    '        'String quarta linha
    '        Dim sNoSequencia4, sCarreta4, sCdFornec4, sCdNivel1_4, sLiberacao4, sPesoBruto4, sPesoTara4, sPesoNeto4 As String

    '        Dim alturaLinha As Integer
    '        Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
    '        Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
    '        Dim altura As Integer = 1000
    '        Dim barra As Image

    '        vMensangem = Nothing
    '        vMensangem += "**PrintExtratoSaida_Laredo**"
    '        Dim sWhere As String
    '        Dim sFiltro As String
    '        If bBotaoImprimir = True Then
    '            sFiltro = tbBoletim.Text.Replace(";", ",")
    '            sWhere = " and c.hr_entrada = to_date('" & FBuscaHrEntrada(sFiltro) & "','dd/mm/yyyy hh24:mi:ss')"
    '            vMensangem += sWhere & vbNewLine
    '        ElseIf bBotaoImprimir = False Then
    '            sWhere = " and c.hr_entrada = to_date('" & hrEntradaCanaLaredo & "','dd/mm/yyyy hh24:mi:ss')"
    '            vMensangem += sWhere & vbNewLine
    '        End If

    '        Dim hrFechamento As String = fGetParam("RCMP", "HR_FECHAMENTO") & ":00"
    '        Dim DtHrIniPeriodo, DtHrFinalPeriodo As String

    '        Dim sql As String
    '        Dim rs As OracleDataReader
    '        sql += "select * from" & vbNewLine
    '        sql += "(select nvl(c.no_liberacao,0) no_liberacao," & vbNewLine
    '        sql += "       nvl(c.id_viagem,0) id_viagem," & vbNewLine
    '        sql += "       nvl(c.no_sequencia,0) no_sequencia," & vbNewLine
    '        sql += "       c.hr_entrada," & vbNewLine
    '        sql += "       c.hr_saida," & vbNewLine
    '        sql += "       nvl(t.cd_fornec,0) cd_fornec," & vbNewLine
    '        sql += "       nvl(t.de_fornec,0) de_fornec," & vbNewLine
    '        sql += "       nvl(c.cd_upnivel1,0) cd_upnivel1," & vbNewLine
    '        sql += "       nvl(u.de_upnivel1,0) de_upnivel1," & vbNewLine
    '        sql += "       nvl(r.cd_equipto,0) cd_equipto," & vbNewLine
    '        sql += "       nvl(r.cd_tp_recurso,0) cd_tp_recurso," & vbNewLine
    '        sql += "       nvl(c.qt_distancia,0) qt_distancia," & vbNewLine
    '        sql += "       nvl(c.qt_bruto,0) qt_bruto," & vbNewLine
    '        sql += "       nvl(c.qt_tara,0) qt_tara," & vbNewLine
    '        sql += "       nvl(c.qt_liquido,0) qt_liquido" & vbNewLine
    '        sql += "from apt_cargas c" & vbNewLine
    '        sql += "inner join apt_cargas_rec r on c.instancia = r.instancia and c.no_liberacao = r.no_liberacao" & vbNewLine
    '        sql += "inner join equiptos e on e.cd_equipto = r.cd_equipto" & vbNewLine
    '        sql += "inner join transporta p on p.cd_transp = e.cd_transp" & vbNewLine
    '        sql += "inner join upnivel1 u on u.cd_upnivel1 = c.cd_upnivel1" & vbNewLine
    '        sql += "inner join fornecs t on t.cd_fornec = u.cd_fornec" & vbNewLine
    '        sql += "where  c.instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine
    '        sql += "" & sWhere & vbNewLine
    '        sql += "  and  c.fg_status in ('C','P')" & vbNewLine
    '        sql += "  and  r.cd_tp_recurso in ('CM','CT'))" & vbNewLine
    '        sql += "pivot (" & vbNewLine
    '        sql += "      max(cd_equipto)" & vbNewLine
    '        sql += "  for cd_tp_recurso in ('CM' as CM,'CT' as CT))"

    '        vMensangem += sql & vbNewLine
    '        fGravaLogDebug("*** [PrintExtratoSaida_Laredo] ***", "LOG_DEBUG")
    '        fGravaLogDebug(sql, "LOG_DEBUG")

    '        rs = fCmd(sql).ExecuteReader
    '        While rs.Read
    '            sNoSequencia = rs("no_sequencia")

    '            If sNoSequencia = 1 Then
    '                sNoSequencia1 = rs("no_sequencia")
    '                sLiberacao1 = rs("no_liberacao")
    '                If Not IsDBNull(rs("id_viagem")) Then
    '                    sViagem = rs("id_viagem")
    '                Else
    '                    sViagem = " "
    '                End If
    '                sCaminhao = rs("CM")
    '                sPlaca = fBuscaPlaca(sCaminhao, sLiberacao1)
    '                sCdFunc = fBuscaMotorista(sCaminhao, sLiberacao1, "D")
    '                nTamMotorista = sCdFunc.Length
    '                If nTamMotorista > 28 Then
    '                    sCdFunc = sCdFunc.Substring(0, 28)
    '                End If
    '                sMotorista = fBuscaMotorista(sCaminhao, sLiberacao1, "C")
    '                sEntrada = rs("hr_entrada")
    '                HrInicial = Convert.ToDateTime(sEntrada)
    '                DtHrIniPeriodo = HrInicial.ToString("dd/MM/yyyy") & " " & hrFechamento
    '                HrFinal = Convert.ToDateTime(DtHrIniPeriodo)
    '                HrFinal = HrFinal.AddHours(24).AddSeconds(-1)
    '                DtHrFinalPeriodo = HrFinal.ToString("dd/MM/yyyy HH:mm:ss")
    '                sSaida = rs("hr_saida")
    '                sPesoBruto1 = rs("qt_bruto")
    '                sPesoTara1 = rs("qt_tara")
    '                sPesoNeto1 = rs("qt_liquido")
    '                sCdNivel1_1 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
    '                nTamNivel1_1 = sCdNivel1_1.Length
    '                If nTamNivel1_1 > 33 Then
    '                    sCdNivel1_1 = sCdNivel1_1.Substring(0, 33)
    '                End If
    '                sCdFornec1 = FBuscaFornec(rs("cd_upnivel1"))
    '                nTamForn1_1 = sCdFornec1.Length
    '                If nTamForn1_1 > 28 Then
    '                    sCdFornec1 = sCdFornec1.Substring(0, 28)
    '                End If
    '            ElseIf sNoSequencia = 2 Then
    '                sNoSequencia2 = rs("no_sequencia")
    '                sLiberacao2 = rs("no_liberacao")
    '                sCarreta2 = rs("CT")
    '                sPesoBruto2 = rs("qt_bruto")
    '                sPesoTara2 = rs("qt_tara")
    '                sPesoNeto2 = rs("qt_liquido")
    '                sCdNivel1_2 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
    '                nTamNivel1_2 = sCdNivel1_2.Length
    '                If nTamNivel1_2 > 33 Then
    '                    sCdNivel1_2 = sCdNivel1_2.Substring(0, 33)
    '                End If
    '                sCdFornec2 = FBuscaFornec(rs("cd_upnivel1"))
    '                nTamForn1_2 = sCdFornec2.Length
    '                If nTamForn1_2 > 28 Then
    '                    sCdFornec2 = sCdFornec2.Substring(0, 28)
    '                End If
    '            ElseIf sNoSequencia = 3 Then
    '                sNoSequencia3 = rs("no_sequencia")
    '                sLiberacao3 = rs("no_liberacao")
    '                sCarreta3 = rs("CT")
    '                sPesoBruto3 = rs("qt_bruto")
    '                sPesoTara3 = rs("qt_tara")
    '                sPesoNeto3 = rs("qt_liquido")
    '                sCdNivel1_3 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
    '                nTamNivel1_3 = sCdNivel1_3.Length
    '                If nTamNivel1_3 > 33 Then
    '                    sCdNivel1_3 = sCdNivel1_3.Substring(0, 33)
    '                End If
    '                sCdFornec3 = FBuscaFornec(rs("cd_upnivel1"))
    '                nTamForn1_3 = sCdFornec3.Length
    '                If nTamNivel1_3 > 28 Then
    '                    sCdFornec3 = sCdFornec3.Substring(0, 28)
    '                End If
    '            ElseIf sNoSequencia = 4 Then
    '                sNoSequencia4 = rs("no_sequencia")
    '                sLiberacao4 = rs("no_liberacao")
    '                sCarreta4 = rs("CT")
    '                sPesoBruto4 = rs("qt_bruto")
    '                sPesoTara4 = rs("qt_tara")
    '                sPesoNeto4 = rs("qt_liquido")
    '                sCdNivel1_4 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
    '                nTamNivel1_4 = sCdNivel1_4.Length
    '                If nTamNivel1_4 > 33 Then
    '                    sCdNivel1_4 = sCdNivel1_4.Substring(0, 33)
    '                End If
    '                sCdFornec4 = FBuscaFornec(rs("cd_upnivel1"))
    '                nTamForn1_4 = sCdFornec4.Length
    '                If nTamForn1_4 > 28 Then
    '                    sCdFornec4 = sCdFornec4.Substring(0, 28)
    '                End If
    '            End If
    '        End While
    '        fCmd(sql).Dispose()
    '        rs.Close()

    '        For i = 1 To 4
    '            If i = 1 And sNoSequencia1 = 1 Then
    '                sEmpresa = fGetParam("GERAIS", "DA_EMPRESA")
    '                sDeNivel1 = fGetParam("NIVEIS", "UPNIVEL1")

    '                Dim sf As New StringFormat()
    '                sf.Alignment = StringAlignment.Center
    '                sf.LineAlignment = StringAlignment.Center

    '                Dim largura1 As Single = 270.0F
    '                Dim altura1 As Single = 30.0F
    '                Dim rect1 As New RectangleF(2.0F, 2.0F, largura1, altura1)
    '                e.Graphics.DrawString("*** " & sEmpresa & " ***", FontesImpressao.Fonte10R, Brushes.Black, rect1, sf)

    '                alturaLinha = 50

    '                'Linha 01
    '                Dim Point01 As New Point(2, alturaLinha)
    '                Dim Point11 As New Point(270, alturaLinha)
    '                Dim blackPen1 = New Pen(Color.Black, 1)
    '                e.Graphics.DrawLine(blackPen1, Point01, Point11)

    '                Dim largura2 As Single = 270.0F
    '                Dim altura2 As Single = 20.0F
    '                Dim rect2 As New RectangleF(2.0F, 50.0F, largura2, altura2)
    '                e.Graphics.DrawString("Extracto de Pesaje de Caña", FontesImpressao.Fonte10R, Brushes.Black, rect2, sf)

    '                'Linha 02
    '                Dim Point02 As New Point(2, alturaLinha + 16)
    '                Dim Point22 As New Point(270, alturaLinha + 16)
    '                Dim blackPen2 = New Pen(Color.Black, 1)
    '                e.Graphics.DrawLine(blackPen2, Point02, Point22)

    '                alturaLinha += 30
    '                e.Graphics.DrawString("Nombre:  " & sCdFunc & vbCrLf & vbCrLf & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 20
    '                e.Graphics.DrawString("Camión:  " & sCaminhao & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                e.Graphics.DrawString("Chofer:  " & sMotorista & "", FontesImpressao.Fonte9R, Brushes.Black, 122, alturaLinha)
    '                alturaLinha += 20
    '                e.Graphics.DrawString("Placa:  " & sPlaca & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 20
    '                e.Graphics.DrawString("Entrada:  " & sEntrada & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 20
    '                e.Graphics.DrawString("Salida:  " & sSaida & "", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 20

    '                'Linha 03
    '                Dim Point03 As New Point(10, alturaLinha + 15)
    '                Dim Point33 As New Point(270, alturaLinha + 15)
    '                Dim blackPen3 = New Pen(Color.Black, 1)
    '                e.Graphics.DrawLine(blackPen3, Point03, Point33)
    '                e.Graphics.DrawString("Nº Viaje: " & sViagem.PadLeft(6, "0"c), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 30
    '                e.Graphics.DrawString("Proveedor: " & sCdFornec1, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 20
    '                e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_1, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 20
    '                e.Graphics.DrawString("Liberación Equipo    Bruto     Tara      Neto", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 20
    '                e.Graphics.DrawString(sLiberacao1.PadLeft(8, "0") & " " & sCaminhao.PadLeft(8, " ") & " " & sPesoBruto1.PadLeft(6, " ") & " " & sPesoTara1.PadLeft(6, " ") & " " & sPesoNeto1.PadLeft(6, " "), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                alturaLinha += 30
    '            ElseIf i = 2 And sNoSequencia2 = 2 Then
    '                If sCarreta2 <> " " Then
    '                    e.Graphics.DrawString("Proveedor:  " & sCdFornec2, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_2, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString("Liberación Equipo    Bruto     Tara      Neto", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString(sLiberacao2.PadLeft(8, "0") & " " & sCarreta2.PadLeft(8, " ") & " " & sPesoBruto2.PadLeft(6, " ") & " " & sPesoTara2.PadLeft(6, " ") & " " & sPesoNeto2.PadLeft(6, " "), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 30
    '                End If
    '            ElseIf i = 3 And sNoSequencia3 = 3 Then
    '                If sCarreta2 <> " " Then
    '                    e.Graphics.DrawString("Proveedor: " & sCdFornec3, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_3, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString("Liberación Equipo    Bruto     Tara      Neto", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString(sLiberacao3.PadLeft(8, "0") & " " & sCarreta3.PadLeft(8, " ") & " " & sPesoBruto3.PadLeft(6, " ") & " " & sPesoTara3.PadLeft(6, " ") & " " & sPesoNeto3.PadLeft(6, " "), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 30
    '                End If
    '            ElseIf i = 4 And sNoSequencia4 = 4 Then
    '                If sCarreta4 <> " " Then
    '                    e.Graphics.DrawString("Proveedor: " & sCdFornec4, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_4, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString("Liberación Equipo    Bruto     Tara      Neto", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 20
    '                    e.Graphics.DrawString(sLiberacao4.PadLeft(8, "0") & " " & sCarreta4.PadLeft(6, " ") & " " & sPesoBruto4.PadLeft(6, " ") & " " & sPesoTara4.PadLeft(6, " ") & " " & sPesoNeto4.PadLeft(6, " "), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '                    alturaLinha += 30
    '                End If
    '            End If
    '        Next

    '        e.Graphics.DrawString("Peso Bruto: " & CDbl(sPesoBruto1) + CDbl(sPesoBruto2) + CDbl(sPesoBruto3) + CDbl(sPesoBruto4), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 20
    '        e.Graphics.DrawString("Peso Tara.: " & CDbl(sPesoTara1) + CDbl(sPesoTara2) + CDbl(sPesoTara3) + CDbl(sPesoTara4), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 20
    '        e.Graphics.DrawString("Peso Neto.: " & CDbl(sPesoNeto1) + CDbl(sPesoNeto2) + CDbl(sPesoNeto3) + CDbl(sPesoNeto4), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        alturaLinha += 30

    '        'Linha 04
    '        Dim Point04 As New Point(2, alturaLinha)
    '        Dim Point44 As New Point(270, alturaLinha)
    '        Dim blackPen4 = New Pen(Color.Black, 1)
    '        e.Graphics.DrawLine(blackPen4, Point04, Point44)
    '        e.Graphics.DrawString("Total Transp: " & FTotalEquiptoExtSaida(sMotorista, DtHrIniPeriodo, DtHrFinalPeriodo), FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
    '        'Linha 05
    '        Dim Point05 As New Point(2, alturaLinha + 15)
    '        Dim Point55 As New Point(270, alturaLinha + 15)
    '        Dim blackPen5 = New Pen(Color.Black, 1)
    '        e.Graphics.DrawLine(blackPen5, Point05, Point55)

    '        e.Graphics.DrawString(".", FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha + 20)

    '    Catch ex As Exception
    '        vGlobal.vMensangem += ex.ToString
    '        frmMensagem.Show()
    '        vMensangem = Nothing
    '    End Try
    'End Sub



    '*** LAREDO - FINAL ***

    Public Sub AtualizaExtratoSaida_Balcanes(idViagem, noliberacao)
        Try
            Dim swhere As String
            If vGlobal.vIMPRLIBSEPARADA = "S" Then
                swhere = " and no_liberacao = " & noliberacao & ""
            ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                swhere = " "
            End If

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "update apt_cargas" & vbNewLine &
                  "set pfg_impr_extr_saida = 'S'" & vbNewLine &
                  "where pfg_impr_extr_saida is null" & vbNewLine &
                  "  and instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and id_viagem = " & idViagem & "" & vbNewLine &
                  "" & swhere & ""
            vGlobal.vMensangem += sql & vbNewLine
            rs = fCmd(sql).ExecuteReader
            fCmd(sql).Dispose()
            rs.Close()
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "AtualizaExtratoSaida_Balcanes", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Public Sub AtualizaBolAnl_Balcanes(idViagem)
        Try
            Dim sql As New StringBuilder
            sql.AppendLine("update apt_cargas")
            sql.AppendLine("set pfg_impr_bolanl = 'S'")
            sql.AppendLine("where pfg_impr_bolanl is null")
            sql.AppendLine("  and instancia = :instancia")
            sql.AppendLine("  and id_viagem = :idviagem")
            Using trans As OracleTransaction = conn.BeginTransaction
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Parameters.Clear()
                    cmd.Transaction = trans
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("idviagem", OracleDbType.Int32).Value = CInt(idViagem)
                    cmd.ExecuteNonQuery()
                    trans.Commit()
                End Using
            End Using
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "AtualizaBolAnl_Balcanes", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Function fListaCartoes(noQueima)
        Try
            Dim resultado As String = Nothing
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select listagg(no_cartao,' / ') within group (order by cd_upnivel1,cd_upnivel2,cd_upnivel3) cartoes from queima_de")
            sql.AppendLine("where no_queima = :queima")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("queima", OracleDbType.Int32).Value = CInt(noQueima)
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        resultado = rs("cartoes")
                    End While
                End Using
            End Using
            Return resultado
        Catch ex As Exception
            fGravaError("frmReimpressao", "fListaCartoes", ex.ToString, "LOG_ERROR")
        End Try
    End Function


    '[BACANES]
    Public Function fVerificaImprBalcanes(sTabela, sColuna, swhere) As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select instancia from " & sTabela)
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and " & sColuna & " is null")
            sql.AppendLine(swhere)
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Dim rs = cmd.ExecuteScalar()
                If rs IsNot Nothing Then
                    Return True
                Else
                    Return False
                End If
            End Using
        Catch ex As OracleException
            fGravaError("OracleErr>> frmReimpressao", "fVerificaImprBalcanes", ex.ToString, "LOG_ERROR")
        Catch ex As Exception
            fGravaError("frmReimpressao", "fVerificaImprBalcanes", ex.ToString, "LOG_ERROR")
        End Try
    End Function

    Public Function fVerificBolAnl() As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("SELECT id_viagem FROM (")
            sql.AppendLine("  SELECT id_viagem")
            sql.AppendLine("  FROM apt_cargas")
            sql.AppendLine("  WHERE instancia = :instancia")
            sql.AppendLine("    AND pfg_impr_bolanl IS NULL")
            sql.AppendLine("    AND qt_bruto IS NOT NULL")
            sql.AppendLine("    AND qt_tara IS NULL")
            sql.AppendLine("    AND fg_status = 'A'")
            sql.AppendLine("    AND last_update >= sysdate - (5/1440)")
            sql.AppendLine("  ORDER BY hr_entrada DESC")
            sql.AppendLine(") WHERE ROWNUM = 1")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        result = If(Not IsDBNull(rs("id_viagem")), rs("id_viagem"), Nothing)
                    End While
                End Using
            End Using
            If result <> Nothing Then
                vGlobal.gn03 = result
                Return True
            Else
                vGlobal.gn03 = 0
                Return False
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString & vbNewLine
            fGravaError("frmReimpressao", "fVerificBolAnl", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function fUltimoDespacho(nCaminhao)
        Try
            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.AppendLine("select max(id_despacho) id_despacho from lt_despachos")
            sql.AppendLine("where cd_equipto = :caminhao")
            sql.AppendLine("  and instancia = :instancia")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("caminhao", OracleDbType.Int32).Value = CInt(nCaminhao)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing Then
                    result = rs.ToString
                End If
            End Using
            If result IsNot Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fUltimoDespacho", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Public Function fIdViagemImprSeparado() As String
        Try
            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select ac.id_viagem")
            sql.AppendLine("  from apt_cargas     ac,")
            sql.AppendLine("       apt_cargas_rec acr,")
            sql.AppendLine("       equiptos       eq,")
            sql.AppendLine("       fornecs        fn,")
            sql.AppendLine("       upnivel1       u1,")
            sql.AppendLine("       transporta     t,")
            sql.AppendLine("       frentransp     f")
            sql.AppendLine(" WHERE f.cd_fren_tran = ac.cd_fren_tran")
            sql.AppendLine("   And t.cd_transp = eq.cd_transp")
            sql.AppendLine("   And u1.cd_upnivel1 = ac.cd_upnivel1")
            sql.AppendLine("   And fn.cd_fornec = u1.cd_fornec")
            sql.AppendLine("   And eq.cd_equipto = acr.cd_equipto")
            sql.AppendLine("   And acr.instancia = ac.instancia")
            sql.AppendLine("   And acr.no_liberacao = ac.no_liberacao")
            sql.AppendLine("   And acr.cd_tp_recurso = :cdtprecurso")
            sql.AppendLine("   and ac.instancia = :instancia")
            sql.AppendLine("   and ac.dt_saida is not null")
            sql.AppendLine("   and ac.pfg_impr_extr_saida is null")
            sql.AppendLine(" order by 1,2")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("cdtprecurso", OracleDbType.Varchar2).Value = "CM"
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing Then
                    result = rs.ToString
                End If
            End Using

            If result IsNot Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fIdViagemImprSeparado", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Public Function fCountLiberViagem(idViagem) As Integer
        Try
            Dim result As Integer = 0
            Dim sql As String = "select count(*) total from apt_cargas where instancia = :instancia and id_viagem = :idViagem"
            vGlobal.vMensangem += sql & vbNewLine
            Using cmd As OracleCommand = New OracleCommand(sql, conn)
                cmd.Parameters.Add(New OracleParameter("instancia", vInstLogin))
                cmd.Parameters.Add(New OracleParameter("idViagem", idViagem))
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso rs IsNot DBNull.Value Then
                    result = Convert.ToInt32(rs)
                End If
            End Using
            Return result
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fCountLiberViagem", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Public Function fLiberacaoImprSeparado(idViagem, noSequencia) As Integer
        Try
            Dim result As Integer = 0
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select no_liberacao from apt_cargas")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and id_viagem = :idViagem")
            sql.AppendLine("  and no_sequencia = :sequencia")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("idViagem", OracleDbType.Int64).Value = Convert.ToInt64(idViagem)
                cmd.Parameters.Add("sequencia", OracleDbType.Int64).Value = Convert.ToInt64(noSequencia)
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso rs IsNot DBNull.Value Then
                    result = Convert.ToInt32(rs)
                End If
            End Using
            Return result
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fLiberacaoImprSeparado", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Public Function fValImprOrdemCarga() As Boolean
        Try
            Dim sDtEmissao, sHoraEmissao, sNoOrdemCarga, sFazenda, sFornecedor, sFrenteColheita, sCaminhao, sPrestador As String
            Dim sNoQueima, sCartoes, sMensagem, sCdEquipto As String

            Dim sql As String
            Dim rs As OracleDataReader

            sql = "select ld.id_despacho," & vbNewLine &
                  "       to_char(ld.dthr_inicio,'dd/mm/yyyy') data," & vbNewLine &
                  "       to_char(ld.dthr_inicio,'hh24:mi:ss') hora," & vbNewLine &
                  "       fn.cd_fornec||'-'||fn.de_fornec fornecedor," & vbNewLine &
                  "       ft.cd_fren_tran||'-'||ft.de_fren_tran frente," & vbNewLine &
                  "       e.cd_equipto||'-'||e.no_placa veiculo,e.cd_equipto," & vbNewLine &
                  "       tt.cd_transp||'-'||tt.de_transp prestador," & vbNewLine &
                  "       trim(u1.cd_upnivel1)||'-'||u1.de_upnivel1 fazenda," & vbNewLine &
                  "       llc.no_queima" & vbNewLine &
                  "from lt_despachos ld, lt_liberacao_carreg llc," & vbNewLine &
                  "     pminstancias pi , equiptos e ," & vbNewLine &
                  "     queima_he qh, queima_de qd, frentransp ft, sistcolh sc," & vbNewLine &
                  "     upnivel1 u1, fornecs fn, transporta tt" & vbNewLine &
                  "where tt.cd_transp = e.cd_transp" & vbNewLine &
                  "  and fn.cd_fornec = u1.cd_fornec" & vbNewLine &
                  "  and u1.cd_upnivel1 = qd.cd_upnivel1" & vbNewLine &
                  "  and sc.cd_sist_colh = llc.cd_sist_colh" & vbNewLine &
                  "  and ft.cd_fren_tran = llc.cd_fren_tran" & vbNewLine &
                  "  and qh.no_queima = llc.no_queima" & vbNewLine &
                  "  and qh.no_queima = qd.no_queima" & vbNewLine &
                  "  and e.cd_equipto = ld.cd_equipto" & vbNewLine &
                  "  and pi.instancia = ld.instancia" & vbNewLine &
                  "  and ld.id_liberacao = llc.id_liberacao" & vbNewLine &
                  "  and pi.instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and ld.fg_status = 'A' and ld.pfg_ordem_carga is null"

            vGlobal.vMensangem += sql & vbNewLine
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                If Not IsDBNull(rs("id_despacho")) Then
                    sNoOrdemCarga = rs("id_despacho")
                    vGlobal.sValor01 = sNoOrdemCarga
                Else
                    sNoOrdemCarga = Nothing
                End If

                If Not IsDBNull(rs("data")) Then
                    sDtEmissao = rs("data")
                Else
                    sDtEmissao = Nothing
                End If
                If Not IsDBNull(rs("hora")) Then
                    sHoraEmissao = rs("hora")
                Else
                    sHoraEmissao = Nothing
                End If
                If Not IsDBNull(rs("fazenda")) Then
                    sFazenda = rs("fazenda")
                Else
                    sFazenda = Nothing
                End If
                If Not IsDBNull(rs("fornecedor")) Then
                    sFornecedor = rs("fornecedor")
                Else
                    sFornecedor = Nothing
                End If
                If Not IsDBNull(rs("frente")) Then
                    sFrenteColheita = rs("frente")
                Else
                    sFrenteColheita = Nothing
                End If
                If Not IsDBNull(rs("veiculo")) Then
                    sCaminhao = rs("veiculo")
                    sCdEquipto = rs("cd_equipto")
                Else
                    sCaminhao = Nothing
                    sCdEquipto = 0
                End If
                If Not IsDBNull(rs("prestador")) Then
                    sPrestador = rs("prestador")
                Else
                    sPrestador = Nothing
                End If
                If Not IsDBNull(rs("no_queima")) Then
                    sNoQueima = rs("no_queima")
                Else
                    sNoQueima = Nothing
                End If
                If Not IsDBNull(fListaCartoes(sNoQueima)) Then
                    sCartoes = fListaCartoes(sNoQueima)
                Else
                    sCartoes = Nothing
                End If

            End While
            fCmd(sql).Dispose()
            rs.Close()
            Dim descrempresa As String = cadempresas(cadparametros(("GERAIS", "CD_EMPRESA")))
            If Not IsDBNull(descrempresa) Then
                vGlobal.sNomeEmpresa = descrempresa
            Else
                vGlobal.sNomeEmpresa = Nothing
            End If
            If (sDtEmissao = Nothing Or sHoraEmissao = Nothing Or sNoOrdemCarga = Nothing Or
                sFazenda = Nothing Or sFornecedor = Nothing Or sFrenteColheita = Nothing Or
                sCaminhao = Nothing Or sPrestador = Nothing Or sNoQueima = Nothing Or sCartoes = Nothing Or
                vGlobal.sNomeEmpresa = Nothing) Then

                Return False
            Else
                If sCliente = "BALCANES" Then
                    If FLibAbastecDespacho(sNoOrdemCarga, sCdEquipto) = True Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Return True
                End If
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fValImprOrdemCarga", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function FLibAbastecDespacho(IdDespacho, CdEquipto) As Boolean
        Try
            Dim FgAbastec As String = Nothing
            Dim FgLiberado As String = Nothing
            Dim sql As New StringBuilder
            sql.AppendLine("select * from lt_despachos l, equiptos e")
            sql.AppendLine("where e.cd_equipto = l.cd_equipto")
            sql.AppendLine("  and l.instancia = '" & vInstLogin & "'")
            sql.AppendLine("  and l.id_despacho = " & IdDespacho)
            sql.AppendLine("  and e.cd_equipto = " & CdEquipto & "")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("id_despacho", OracleDbType.Int64).Value = Convert.ToInt64(IdDespacho)
                cmd.Parameters.Add("cd_equipto", OracleDbType.Int64).Value = Convert.ToInt64(CdEquipto)
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        FgAbastec = rs("fg_abastec")
                        FgLiberado = rs("fg_calculado_abastec")
                    End While
                End Using
            End Using

            If FgAbastec = "N" Then
                Return True
            ElseIf FgAbastec = "S" And FgLiberado = "S" Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception

        End Try
    End Function

    Private Sub lbVersao_DoubleClick(sender As Object, e As EventArgs) Handles lbVersao.DoubleClick
        FParametros.Show()
    End Sub

    Private Function FPesagemBalanza(OrdemCarga, NomePC) As Boolean
        Try
            fGravaLogDebug("Computador a ser impresso>>" & vGlobal.vNomePc, "LOG_DEBUG")
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select * from outr_pesagens")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and cd_balanca_said = :cd_balanca_said")
            sql.AppendLine("  and pfg_impr_extrato = :pfg_impr_extrato")
            sql.AppendLine("  and no_pesagem = :no_pesagem")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("cd_balanca_said", OracleDbType.Int64).Value = Convert.ToInt64(cadtblbalanca(vGlobal.vNomePc))
                cmd.Parameters.Add("pfg_impr_extrato", OracleDbType.Varchar2).Value = "N"
                cmd.Parameters.Add("no_pesagem", OracleDbType.Int64).Value = Convert.ToInt64(OrdemCarga)
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing Then
                    Return True
                Else
                    Return False
                End If
            End Using
        Catch ex As OracleException
            MsgBox("OracleErr>> FPesagemBalanza: " & ex.ToString, MsgBoxStyle.Critical, "Error")
        Catch ex As Exception
            MsgBox("FPesagemBalanza>>" & ex.ToString, MsgBoxStyle.Critical, "Error")
        End Try
    End Function

    '*** BUDEGUER INICIO ***
    Public Function FVerifPesagCana() As Boolean
        Try
            vGlobal.nValor10 = Nothing
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select no_liberacao from apt_cargas")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and pfg_impr_extr_saida = 'N'")
            sql.AppendLine("  and qt_tara is not null")
            sql.AppendLine("  and qt_liquido is not null")
            sql.AppendLine("  and fg_status in ('C','P')")
            sql.AppendLine("  and rownum = 1")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso rs IsNot DBNull.Value Then
                    vGlobal.nValor10 = Convert.ToInt32(rs)
                    Return True
                Else
                    Return False
                End If
            End Using
        Catch ex As Exception
            MsgBox("FVerifPesagCana>>" & ex.ToString, MsgBoxStyle.Critical, "Error")
        End Try
    End Function

    Private Sub PrintTicketRastreio_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintTicketRastreio.PrintPage
        bImpressoOrdemCargas = False
        Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
        Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
        Dim altura As Integer = 1000

        Dim barra As Image

        Dim barras As Image

        Dim Ticket, Caminhao, Transporta, Hora_Guarita, Frente As String

        Dim sql As New StringBuilder
        sql.Clear()
        sql.AppendLine("select c.no_ticket_rastreio ticket,")
        sql.AppendLine("e.cd_equipto||'-'||e.no_placa caminhao,")
        sql.AppendLine("t.cd_transp||'-'||t.de_transp transporta,")
        sql.AppendLine("f.cd_frente||'-'||f.de_frente frente,")
        sql.AppendLine("c.hr_guarita")
        sql.AppendLine("from apt_cargas c, apt_cargas_rec r, transporta t, frentes f, equiptos e")
        sql.AppendLine("Where e.cd_equipto = e.cd_equipto")
        sql.AppendLine("  And f.cd_frente = c.cd_frente")
        sql.AppendLine("  And t.cd_transp = e.cd_transp")
        sql.AppendLine("  And r.instancia = c.instancia")
        sql.AppendLine("  And r.no_liberacao = c.no_liberaca")
        sql.AppendLine("  And c.instancia = :instancia")
        sql.AppendLine("  and c.fg_impr_ticke_rast = 'N'")
        sql.AppendLine("  and r.cd_tp_recurso = 'CM'")
        Using cmd As OracleCommand = fCmd(sql.ToString)
            cmd.Parameters.Clear()
            cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
            Using rs As OracleDataReader = cmd.ExecuteReader
                While rs.Read
                    Ticket = rs("ticket")
                    Caminhao = rs("caminhao")
                    Transporta = rs("transporta")
                    Hora_Guarita = rs("hr_guarita")
                    Frente = rs("frente")
                End While
            End Using
        End Using

        barras = codbarras.Draw(Ticket, altura)

        e.Graphics.DrawString("TICKET RASTREO", FontesImpressao.Fonte10R, Brushes.Black, 10, 30)
        e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte10R, Brushes.Black, 10, 50)

        e.Graphics.DrawString("Camión:    " & Caminhao, FontesImpressao.Fonte10R, Brushes.Black, 10, 100)
        e.Graphics.DrawString("Fletero: " & Transporta, FontesImpressao.Fonte10R, Brushes.Black, 10, 120)
        e.Graphics.DrawString("Frente:    " & Frente, FontesImpressao.Fonte10R, Brushes.Black, 10, 140)
        e.Graphics.DrawString("Recepción:   " & Hora_Guarita, FontesImpressao.Fonte10R, Brushes.Black, 10, 160)
        e.Graphics.DrawString("Ticket:    " & Ticket, FontesImpressao.Fonte10R, Brushes.Black, 10, 180)

        e.Graphics.DrawImage(barras, New Rectangle(472, 420, 140, 70))
        e.Graphics.DrawString(Ticket, FontesImpressao.Fonte10R, Brushes.Black, 472, 500)

    End Sub

    Private Function FVerifiPesagConcluida(NoPesagem) As Boolean
        Try
            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select * from outr_pesagens")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and no_pesagem = :no_pesagem")
            sql.AppendLine("  and fg_status = 'C'")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_pesagem", OracleDbType.Int64).Value = Convert.ToInt64(NoPesagem)
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso rs IsNot DBNull.Value Then
                    Return True
                Else
                    Return False
                End If
            End Using
        Catch ex As Exception
            MsgBox("FVerifiPesagConcluida>>" & ex.ToString, MsgBoxStyle.Critical, "Error")
        End Try
    End Function

    Public Function FVerifTiketRast() As Boolean
        Try
            Dim sql As New StringBuilder
            sql.AppendLine("select no_liberacao from apt_cargas")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and pfg_impr_ticket_rast = 'N'")
            sql.AppendLine("  and fg_status = 'G'")
            sql.AppendLine("  and fg_analise is not null")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Dim rs As Object = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                    vGlobal.gn20 = Convert.ToInt32(rs)
                    Return True
                Else
                    Return False
                End If
            End Using
        Catch ex As Exception
            FMensagem("FrmReimpressao.FVerifTicketRast: " & vbCrLf &
          ex.ToString, "C")
        End Try
    End Function

    '*** BUDEGUER FINAL ***

    Private Sub rbBolAnl_CheckedChanged(sender As Object, e As EventArgs) Handles rbBolAnl.CheckedChanged
        If rbBolAnl.Checked Then
            vRelatImpr = "BA"
        Else
            vRelatImpr = Nothing
        End If

        If vGlobal.vModeloImpressao = "02" Then
            If rbBolAnl.Checked = True Then
                cbAnalise.Visible = True
                cbLiberacao.Visible = True
                lbTexto.Text = "Análisis:"
                cbAnalise.Checked = True
                cbLiberacao.Checked = False
                Me.Width = 599
                Me.Height = 167
            ElseIf rbBolAnl.Checked = False Then
                cbAnalise.Visible = False
                cbLiberacao.Visible = False
                lbTexto.Text = "Análisis:"
                cbAnalise.Checked = False
                cbLiberacao.Checked = False
                Me.Width = 553
                Me.Height = 167
            End If
        ElseIf vGlobal.vModeloImpressao = "00" Then
            lbTexto.Text = "Liberación:"
        End If
    End Sub

    Private Sub rbExtratoSaida_CheckedChanged(sender As Object, e As EventArgs) Handles rbExtratoSaida.CheckedChanged
        If rbExtratoSaida.Checked = True Then
            vRelatImpr = "ES"
        Else
            vRelatImpr = Nothing
        End If

        If vGlobal.vModeloImpressao = "02" Or vGlobal.vModeloImpressao = "04" Then
            If rbExtratoSaida.Checked = True Then
                lbTexto.Text = "Liberación:"
            Else
            End If
        ElseIf vGlobal.vModeloImpressao = "00" Then
            If rbExtratoSaida.Checked = True Then
                lbTexto.Text = "Liberación:"
            End If
        End If
    End Sub

    Private Sub RbExtProducto_CheckedChanged(sender As Object, e As EventArgs) Handles RbExtProducto.CheckedChanged
        If RbExtProducto.Checked Then
            vRelatImpr = "EP"
        Else
            vRelatImpr = Nothing
        End If

        If vGlobal.vModeloImpressao = "02" Or vGlobal.vModeloImpressao = "04" Then
            If RbExtProducto.Checked = True Then
                lbTexto.Text = "Pesaje:"
            End If
        End If
    End Sub

    Private Sub RbTicketRast_CheckedChanged(sender As Object, e As EventArgs) Handles RbTicketRast.CheckedChanged
        If RbTicketRast.Checked Then
            vRelatImpr = "TR"
        Else
            vRelatImpr = Nothing
        End If

        If vGlobal.vModeloImpressao = "04" Then
            If RbTicketRast.Checked = True Then
                lbTexto.Text = "Id Despacho:"
            End If
        End If
    End Sub

    Private Sub RbBoletimEntr_CheckedChanged(sender As Object, e As EventArgs) Handles RbBoletimEntr.CheckedChanged
        If RbBoletimEntr.Checked Then
            vRelatImpr = "BE"
        Else
            vRelatImpr = Nothing
        End If

        If vGlobal.vModeloImpressao = "02" Then
            If RbBoletimEntr.Checked = True Then
                lbTexto.Text = "Liberación:"
                vGlobal.FgImprBoletiEntrLaredo = True
            End If
        End If
    End Sub

    Private Sub cbAnalise_CheckedChanged(sender As Object, e As EventArgs) Handles cbAnalise.CheckedChanged
        If cbAnalise.Checked Then
            vRelatImpr = "AN"
        Else
            vRelatImpr = Nothing
        End If

        tbBoletim.Clear()
        cbLiberacao.Checked = False
        lbTexto.Text = "Análisis:"
    End Sub

    Private Sub cbLiberacao_CheckedChanged(sender As Object, e As EventArgs) Handles cbLiberacao.CheckedChanged
        If cbLiberacao.Checked Then
            vRelatImpr = "LB"
        Else
            vRelatImpr = Nothing
        End If

        tbBoletim.Clear()
        cbAnalise.Checked = False
        lbTexto.Text = "Liberación:"
    End Sub

    Private Sub OcultaCheckBox()
        OcultaCheckBox(Me)
    End Sub

    Private Sub OcultaCheckBox(parent As Control)
        Dim workerDictHabProc As New Dictionary(Of String, String) From {
        {"rbOrdemCarga", dadosarquivoini(("PARAMETROS", "IMPRORDEMCARGA"))},
        {"rbExtratoSaida", dadosarquivoini(("PARAMETROS", "IMPREXTRSAIDA"))},
        {"RbBoletimEntr", dadosarquivoini(("PARAMETROS", "IMPRBOLENTR"))},
        {"rbBolAnl", dadosarquivoini(("PARAMETROS", "IMPRBOLANL"))},
        {"RbTicketRast", dadosarquivoini(("PARAMETROS", "IMPRETICKETRAST"))},
        {"RbExtProducto", dadosarquivoini(("PARAMETROS", "IMPREXTRPRODUTO"))}
        }
        For Each item In workerDictHabProc
            If item.Value = "N" Then
                Dim ctrl As Control = parent.Controls.Find(item.Key, True).FirstOrDefault()
                If ctrl IsNot Nothing Then
                    ctrl.Visible = False
                End If
            End If
        Next
    End Sub

    Private Sub TextoModeloImpr()
        Select Case vGlobal.vModeloImpressao
            Case "01" : lbTexto.Text = "Extrato:"
            Case "02" : lbTexto.Text = "Boletín:"
            Case "03" : lbTexto.Text = "Boletín:"
        End Select
    End Sub

    Private Sub AjusteFinalTela()
        Me.Width = 553
        Me.Height = 167
        cbAnalise.Visible = False
        cbLiberacao.Visible = False

        tbBoletim.Text = Nothing

        lbVersao.Text = "Version: " & vVersion & " / Bal: " & vGlobal.vNomePc
    End Sub

    Private Sub IniciaImpressora()
        ' Executa o carregamento prévio da impressora em uma thread separada para não travar a abertura da tela
        Task.Run(Sub()
                     Try
                         Dim docDummy As New System.Drawing.Printing.PrintDocument()
                         docDummy.PrinterSettings.PrinterName = vNomeImpressora
                         ' Acessar a propriedade IsValid força o Windows Spooler a inicializar o driver
                         Dim val = docDummy.PrinterSettings.IsValid
                     Catch
                         ' Ignora falhas no aquecimento silencioso
                     End Try
                 End Sub)
    End Sub

    Private Sub ConfiguracaoBotao(botao As Control, flag As String, acao As Action)
        If flag = "S" Then
            botao.Visible = True
            acao?.Invoke
        Else
            botao.Visible = False
        End If
    End Sub

    Private Sub ListaDicionarioParametros()
        ' Exemplo: chave é uma tupla (seu caso), valor é String
        Dim dados As Dictionary(Of (String, String), String) = dadosarquivoini

        ' Caminho do arquivo de log
        Dim caminhoArquivo As String = Application.StartupPath & "\logs\DicionarioParam.txt"

        ' Usando StreamWriter para gravar
        Using sw As New StreamWriter(caminhoArquivo, False) ' False = sobrescreve, True = append
            ' Cabeçalho
            sw.WriteLine("=== LOG DE DADOS ===")
            sw.WriteLine("Data/Hora: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            sw.WriteLine("--------------------------------------------------")

            ' Percorre o Dictionary e grava chave + valor
            For Each kvp As KeyValuePair(Of (String, String), String) In dados
                ' A chave é uma tupla, então você acessa com .Item1 e .Item2
                sw.WriteLine($"({kvp.Key.Item1}, {kvp.Key.Item2}) : {kvp.Value}")
            Next

            sw.WriteLine("--------------------------------------------------")
            sw.WriteLine()
        End Using

        Console.WriteLine("Arquivo gravado em: " & caminhoArquivo)
    End Sub

    '*******************************
    'Timer de verificação da conexão
    '*******************************
    Dim TimerConexao As New System.Windows.Forms.Timer

    Private Sub IniciaTimerConexao()
        ' Define o intervalo para 10 segundos (10000 ms) - ideal para não sobrecarregar o banco
        TimerConexao.Interval = 10000

        ' Vincula o evento Tick do Timer ao método que vai rodar a validação
        AddHandler TimerConexao.Tick, AddressOf TimerConexao_Tick

        TimerConexao.Start()

        ' Executa uma vez logo na abertura para não ter que esperar os primeiros 10 segundos
        AtualizarStatusBanco()
    End Sub

    Private Sub FinalizaTimerConexao()
        ' Desvincula o evento para evitar vazamento de memória e para o timer
        RemoveHandler TimerConexao.Tick, AddressOf TimerConexao_Tick
        TimerConexao.Stop()
    End Sub

    ' Este é o evento real que o Timer vai disparar a cada 10 segundos
    Private Sub TimerConexao_Tick(sender As Object, e As EventArgs)
        ' Como o Timer já está configurado para o tempo certo, é só chamar direto!
        AtualizarStatusBanco()
    End Sub

    Public Async Sub AtualizarStatusBanco()
        ' Desativa temporariamente o timer para evitar que ele dispare um novo teste 
        ' se o teste atual demorar mais que o intervalo (evita fila de requisições)
        TimerConexao.Stop()

        ' Modificamos a cor para laranja enquanto o teste roda em segundo plano
        PbStatusConexao.BackColor = Color.Orange
        PbStatusConexao.Refresh()

        ' Aguarda o teste de conexão sem travar a tela
        Dim estaconectado As Boolean = Await fValidaConexaoAsync()

        If estaconectado Then
            PbStatusConexao.BackColor = Color.YellowGreen
        Else
            PbStatusConexao.BackColor = Color.Red
        End If

        PbStatusConexao.Refresh()

        ' Reativa o timer para a próxima contagem após o término deste teste
        TimerConexao.Start()
    End Sub

    Private Sub rbOrdemCarga_CheckedChanged(sender As Object, e As EventArgs) Handles rbOrdemCarga.CheckedChanged
        If rbOrdemCarga.Checked Then
            vRelatImpr = "OC"
            lbTexto.Text = "Orden Carga:"
        Else
            vRelatImpr = Nothing
        End If
    End Sub

    '*******************************

    'Métodos para realização das impressões
    '**************************************
    Private Sub ImprimirModelo01()
        Impressoes.PrintImpurezaMineralMD01.Print()
    End Sub

    Private Sub ImprimirModelo02()
        Select Case True
            Case rbBolAnl.Checked
                Select Case True
                    Case cbLiberacao.Checked : vGlobal.FgImprLiberAnalise = "L"
                    Case cbAnalise.Checked : vGlobal.FgImprLiberAnalise = "A"
                End Select

                If Not IsNumeric(vGlobal.nRegistroImpressao) Then Exit Sub 'Caso não seja um número válido, sai do método
                If Not Impressoes.FValidaStatuLiberacao(vGlobal.nRegistroImpressao) Then Exit Sub 'Caso não exista a liberação , sai do método
                Laredo.ImpBolAnalise.Imprimir()
            Case rbExtratoSaida.Checked
                If Not IsNumeric(vGlobal.nRegistroImpressao) Then Exit Sub 'Caso não seja um número válido, sai do método
                If Not Impressoes.FValidaStatuLiberacao(vGlobal.nRegistroImpressao) Then Exit Sub 'Caso não exista a liberação , sai do método

                Laredo.listaDadosSaida = Laredo.ImprBolSaida.Dados()
                Impressoes.ImprimirSaidaLaredo(Laredo.listaDadosSaida)
            Case RbExtProducto.Checked
                If IsNumeric(vGlobal.nRegistroImpressao) Then
                    'Dim statusordem As String = Nothing
                    'If DictStatusOP.ContainsKey((vInstLogin, vGlobal.nRegistroImpressao)) Then
                    '    statusordem = DictStatusOP((vInstLogin, vGlobal.nRegistroImpressao))
                    'End If
                    Dim statusordem As String = FValidaStatusPesagemProduto(vGlobal.nRegistroImpressao)
                    Select Case statusordem
                        Case "A" : MsgBox($"Pesaje abierta, no será hecho la impresión", MsgBoxStyle.Exclamation, "Información")
                        Case "D" : MsgBox($"Pesaje borrada, no será hecho la impresión", MsgBoxStyle.Exclamation, "Información")
                        Case Nothing : MsgBox($"Pesaje no existe, no será hecho la impresión", MsgBoxStyle.Exclamation, "Información")
                        Case Else : ImprimirDocumento(Impressoes.PrintPesagProdutos_Laredo, vGlobal.vNumCopiasExtrProduto, "EXTRATO_PRODUTO")
                    End Select
                End If
            Case RbBoletimEntr.Checked
                If Not IsNumeric(vGlobal.nRegistroImpressao) Then Exit Sub 'Caso não seja um número válido, sai do método
                If Not Impressoes.FValidaStatuLiberacao(vGlobal.nRegistroImpressao) Then Exit Sub 'Caso não exista a liberação , sai do método

                Laredo.listaDados = Laredo.ImprBolEntrada.Dados()
                Impressoes.Laredo.ImprBolEntrada.ImprimeEntrada()
                'Impressoes.ImprimirBolEntr_Laredo()
        End Select
    End Sub

    Private Sub ImprimirBoletimEntrada_Laredo()
        If vGlobal.vImprExtrEntr <> "S" Then Exit Sub

        Threads.bImprimindoBolEntr = True
        Laredo.listaDados = Laredo.ImprBolEntrada.Dados()

        If Laredo.listaDados.Any Then
            For i As Integer = 1 To vGlobal.vNumCopiasBoletimentr
                With Impressoes.PrintBolEntr_Laredo
                    .DefaultPageSettings.Landscape = False
                    .PrinterSettings.PrinterName = vNomeImpressora
                    .Print()
                End With
            Next
        Else
            FMensagem("No existen datos a seren impresos", "I")
        End If

        Threads.bImprimindoBolEntr = False
    End Sub

    Private Sub ImprimirModelo03()
        ImprimirDocumento(Impressoes.PrintImpurezaMineralMD01, vGlobal.vNumeroCopias, Nothing)
    End Sub

    Private Sub ImprimirModeloBalcanes()
        If sCliente <> "BALCANES" Then Exit Sub

        vGlobal.bBotaoImprimir = True

        If rbOrdemCarga.Checked Then
            ImprimirDocumento(Impressoes.PrintOrdemCarga_Balcanes, vGlobal.vNumeroCopias, Nothing, Nothing)

        ElseIf rbExtratoSaida.Checked Then
            ImprimirDocumento(Impressoes.PrintExtratoSaida_Balcanes, vGlobal.vNumeroCopias)

        ElseIf rbBolAnl.Checked Then
            ImprimirDocumento(Impressoes.PrintBolAnl_Balcanes, vGlobal.vNumeroCopias)
        End If

        LimparBoletim()
    End Sub

    Private Sub ImprimirModeloBudeguer()
        If sCliente <> "BUDEGUER" Then Exit Sub

        bBotaoImprimir = True

        Select Case True
            Case rbOrdemCarga.Checked
                If String.IsNullOrWhiteSpace(tbBoletim.Text) Then Exit Sub
                If Impressoes.Budeguer.FValidaIdDespacho(tbBoletim.Text) Then
                    ImprimirDocumento(Impressoes.PrintOrdemCarga_Budeguer, vGlobal.vNumeroCopias)
                End If
            Case rbExtratoSaida.Checked
                If String.IsNullOrWhiteSpace(tbBoletim.Text) Then Exit Sub
                If Impressoes.Budeguer.FValidaLiberacao(tbBoletim.Text) Then
                    ImprimirDocumento(Impressoes.PrintExtratoSaida_Budeguer, vGlobal.vNumeroCopias)
                End If
            Case RbExtProducto.Checked
                ImprimirExtratoProduto_Budeguer()
            Case RbTicketRast.Checked
                If String.IsNullOrWhiteSpace(tbBoletim.Text) Then Exit Sub
                If Impressoes.Budeguer.FValidaIdDespacho(tbBoletim.Text) Then
                    ImprimirDocumento(Impressoes.PrintTicketRast_Budeguer, vGlobal.vNumeroCopias)
                End If
        End Select

        'If rbOrdemCarga.Checked Then
        '    If String.IsNullOrWhiteSpace(tbBoletim.Text) Then Exit Sub
        '    If Impressoes.Budeguer.FValidaIdDespacho(tbBoletim.Text) Then
        '        ImprimirDocumento(Impressoes.PrintOrdemCarga_Budeguer, vGlobal.vNumeroCopias)
        '    End If
        'ElseIf rbExtratoSaida.Checked Then
        '    If String.IsNullOrWhiteSpace(tbBoletim.Text) Then Exit Sub
        '    If Impressoes.Budeguer.FValidaLiberacao(tbBoletim.Text) Then
        '        ImprimirDocumento(Impressoes.PrintExtratoSaida_Budeguer, vGlobal.vNumeroCopias)
        '    End If
        'ElseIf RbExtProducto.Checked Then
        '    ImprimirExtratoProduto_Budeguer()

        'ElseIf RbTicketRast.Checked Then
        '    If String.IsNullOrWhiteSpace(tbBoletim.Text) Then Exit Sub
        '    If Impressoes.Budeguer.FValidaIdDespacho(tbBoletim.Text) Then
        '        ImprimirDocumento(Impressoes.PrintTicketRast_Budeguer, vGlobal.vNumeroCopias)
        '    End If
        'End If

        LimparBoletim()
    End Sub

    Private Sub ImprimirExtratoProduto_Budeguer()
        If FVerifiPesagConcluida(tbBoletim.Text) Then
            ImprimirDocumento(PrintExtrProduto_Budeguer, vGlobal.vNumeroCopias)
        Else
            MsgBox($"El pesaje: {tbBoletim.Text} no está concluyo", MsgBoxStyle.Exclamation, "Aviso")
        End If
    End Sub

    Private Function FValidaOutrPresagem(nopesagem As Integer) As String
        Try
            fGravaLogDebug($"{Now()} 7.Inicia validacao do status da ordem", "LOG_TESTE")
            Dim fgstatus As String = Nothing
            Dim sql As New StringBuilder
            sql.AppendLine("select instancia, no_pesagem, fg_status from outr_pesagens")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("and no_pesagem = :no_pesagem")
            fGravaLogDebug($"{Now()} 8.Executa o comando SQL", "LOG_TESTE")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_pesagem", OracleDbType.Int32).Value = nopesagem
                Dim txt As String = GetDebugSql(cmd)
                fGravaLogDebug($"{Now()} 8.Comando SQL {vbCrLf}{txt}", "LOG_TESTE")
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        fgstatus = rs("fg_status")
                    End While
                End Using
            End Using
            fGravaLogDebug($"{Now()} 9.Status da pesagem {fgstatus}", "LOG_TESTE")
            Return fgstatus
        Catch ex As OracleException
            MsgBox($"OracleErr>> FrmReimpressao.FValidaOutrPesagem: {ex.ToString}", MsgBoxStyle.Critical, "Error")
        Catch ex As Exception
            MsgBox($"FrmReimpressao.FValidaOutrPesagem: {ex.ToString}", MsgBoxStyle.Critical, "Error")
        End Try
    End Function

    '**************************************

    '[THREADS]
    '[STATUS ORDEM DE PESAGEM DE OUTROS PRODUTOS]
    Private WithEvents tmrAtualizacao As System.Timers.Timer
    Public DictStatusOP As New Dictionary(Of (String, Integer), String)
    Public Sub IniciaDictStatusOdemPesagem()
        ' Dispara a primeira carga imediatamente
        Task.Run(Sub() CarregarDicionarioEmSegundoPlano())

        ' Configura o timer para rodar a cada 5 minutos (300.000 ms)
        tmrAtualizacao = New System.Timers.Timer(300000)
        tmrAtualizacao.AutoReset = True
        tmrAtualizacao.Enabled = True
    End Sub

    Private Sub OnTimerElapsed(sender As Object, e As System.Timers.ElapsedEventArgs) Handles tmrAtualizacao.Elapsed
        ' Executa a atualização fora da thread principal de UI
        Task.Run(Sub() CarregarDicionarioEmSegundoPlano())
    End Sub

    Private Sub CarregarDicionarioEmSegundoPlano()
        DictStatusOP = FDictOrdemPesagem()
    End Sub

    Private Function FDictOrdemPesagem() As Dictionary(Of (String, Integer), String)
        Try
            Dim result As New Dictionary(Of (String, Integer), String)
            Dim sql As New StringBuilder
            sql.AppendLine("select instancia, no_pesagem, fg_status from outr_pesagens")
            sql.AppendLine("where instancia = :instancia")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.FetchSize = cmd.FetchSize * 32
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        Dim vinstancia As String = rs("instancia")
                        Dim vnopesagem As Integer = rs("no_pesagem")
                        Dim vstatus As String = rs("fg_status")
                        result.Add((vinstancia, vnopesagem), vstatus)
                    End While
                End Using
            End Using
            Return result
        Catch ex As OracleException
            FMensagem($"OracleErr>> FrmReimpressao.FDictOrdemPesagem: {ex.ToString}", "C")
        Catch ex As Exception
            FMensagem($"FrmReimpressao.FDictOrdemPesagem: {ex.ToString}", "C")
        End Try
    End Function

    Private Function FValidaStatusPesagemProduto(nopesagem As Long) As String
        Try
            Dim sql As New StringBuilder
            sql.AppendLine("select fg_status from outr_pesagens")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("and no_pesagem = :no_pesagem")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_pesagem", OracleDbType.Int64).Value = nopesagem
                Dim rs = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso rs IsNot DBNull.Value Then
                    Return rs.ToString()
                End If
            End Using
        Catch ex As OracleException
            FMensagem($"OracleErr>> FrmReimpressao.FValidaStatusPesagemProduto: {ex.ToString}", "C")
        Catch ex As Exception
            FMensagem($"FrmReimpressao.FValidaStatusPesagemProduto: {ex.ToString}", "C")
        End Try
        Return Nothing
    End Function

    Private Sub ParamInicial()

        Dim timezone As Integer = Convert.ToInt32(dadosarquivoini(("PARAMETROS", "TIMEZONE")))

        vGlobal.DtHrInicial = DateAdd(DateInterval.Hour, timezone, DateTime.Now)
        vGlobal.NoLiberacaoEntrada = FUltimaLiberacaoEntrada()

        vGlobal.DtHrSaida = DateAdd(DateInterval.Hour, timezone, DateTime.Now)
        vGlobal.NoLiberacaoSaida = FUltimaLiberacaoSaida()
    End Sub
    Public Shared Function FUltimaLiberacaoEntrada() As Integer
        Try
            Dim sql As New StringBuilder
            sql.AppendLine("select max(no_liberacao) no_liberacao")
            sql.AppendLine("from apt_cargas")
            sql.AppendLine("where instancia = :instancia")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Dim rs As Object = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                    Return Convert.ToInt32(rs)
                End If
            End Using
            Return 0
        Catch ex As OracleException
            FMensagem($"OracleErr>> FrmReimpressao.IniciaTela.UltimaLiberacaoEntrada: {ex.ToString}", "C")
        Catch ex As Exception
            FMensagem($"FrmReimpressao.IniciaTela.UltimaLiberacaoEntrada: {ex.ToString}", "C")
        End Try
    End Function

    Public Function FUltimaLiberacaoSaida() As Integer
        Try
            Dim sql As New StringBuilder
            sql.AppendLine("select max(no_liberacao) no_liberacao")
            sql.AppendLine("from apt_cargas")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("and qt_liquido is not null")
            sql.AppendLine("and hr_saida is not null")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Dim rs As Object = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                    Return Convert.ToInt32(rs)
                End If
            End Using
            Return 0
        Catch ex As OracleException
            FMensagem($"OracleErr>> FrmReimpressao.IniciaTela.UltimaLiberacaoSaida: {ex.ToString}", "C")
        Catch ex As Exception
            FMensagem($"FrmReimpressao.IniciaTela.UltimaLiberacaoSaida: {ex.ToString}", "C")
        End Try
    End Function


End Class
