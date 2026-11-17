Imports System.Data.OracleClient
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar
Imports System.IO
Imports System.Drawing.Printing
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Drawing

Public Class frmReimpressao
    Dim nContador As Integer = 0
    Dim nContadorTimer As Integer
    Public bFecharForm As Boolean = False
    Public rp As New RawPrinterHelper
    Dim sWhere As String
    Public bBotaoImprimir As Boolean = False
    Dim bImpressoOrdemCargas As Boolean
    Dim bImpressoExtrato As Boolean
    Dim sMensagem As String
    Public noLiberacao As String

    Private Sub PrintImpurezaMineralMD01_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintImpurezaMineralMD01.PrintPage
        Try
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim TipoFonte As String = "arial"
            Dim Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
            Dim Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
            Dim Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
            Dim Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)

            Dim Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
            Dim Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
            Dim Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
            Dim Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)

            Dim cdCaminhao, cdMotorista, hrEntrada, cdCarretas, cdFrenTran, noLiberacao, noAnalise As String

            cdCarretas = " "

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select c.no_liberacao,c.no_analise," & vbNewLine &
                  "       r.cd_tp_recurso," & vbNewLine &
                  "       r.cd_operador," & vbNewLine &
                  "       r.cd_equipto," & vbNewLine &
                  "       c.hr_entrada," & vbNewLine &
                  "       f.cd_fren_tran || f.de_fren_tran frente" & vbNewLine &
                  "  from apt_cargas c inner join apt_cargas_rec r" & vbNewLine &
                  "    on r.no_liberacao = c.no_liberacao" & vbNewLine &
                  "   and r.instancia = c.instancia" & vbNewLine &
                  "    inner join  frentransp f" & vbNewLine &
                  "    on f.cd_fren_tran = c.cd_fren_tran" & vbNewLine &
                  "   and r.cd_tp_recurso in ('CM', 'CT')" & vbNewLine &
                  "   and c.fg_impressao_impureza is null" & vbNewLine &
                  "   and c.fg_analise = 'S'" & vbNewLine &
                  "   and rownum = 1"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                noLiberacao = rs("no_liberacao")
                noAnalise = rs("no_analise")
                hrEntrada = rs("hr_entrada")
                cdFrenTran = rs("frente")
                If rs("cd_tp_recurso") = "CM" Then
                    cdCaminhao = rs("cd_equipto")
                    cdMotorista = rs("cd_operador")
                ElseIf rs("cd_tp_recurso") = "CT" Then
                    cdCarretas = rs("cd_equipto")
                End If
            End While
            fCmd(sql).Dispose()
            rs.Close()

            AtualizaTablacargas(noLiberacao)

            vGlobal.sNomeEmpresa = fDescricao("empresas", "de_empresa", "cd_empresa", fGetParam("GERAIS", "CD_EMPRESA"))

            e.Graphics.DrawString("**            " & vGlobal.sNomeEmpresa & "            **", Fonte11R, Brushes.Black, 10, 10)
            e.Graphics.DrawString("      Boletín de Análisis de Trash", Fonte11R, Brushes.Black, 10, 50)
            e.Graphics.DrawString("Camión: " & cdCaminhao & "   Chofer: " & cdMotorista & "", Fonte10R, Brushes.Black, 10, 90)
            e.Graphics.DrawString("Remolques: " & cdCarretas & "", Fonte10R, Brushes.Black, 10, 110)
            e.Graphics.DrawString("Entrada: " & hrEntrada & "", Fonte10R, Brushes.Black, 10, 130)
            e.Graphics.DrawString("Frente de Cosecha: " & cdFrenTran & "", Fonte10R, Brushes.Black, 10, 150)

            barra = codbarras.Draw(noAnalise, altura)
            e.Graphics.DrawImage(barra, New Rectangle(35, 200, 140, 70))
            e.Graphics.DrawString(noAnalise, Fonte9R, Brushes.Black, 85, 280)

            cdCaminhao = Nothing
            hrEntrada = Nothing
            cdCarretas = Nothing
            cdFrenTran = Nothing
            noLiberacao = Nothing
            noAnalise = Nothing
        Catch ex As Exception
            vGlobal.vMensangem = ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Sub btImprimir_Click(sender As Object, e As EventArgs) Handles btImprimir.Click
        'tbBoletim.Text = "61751"
        If tbBoletim.Text <> "" Or tbBoletim.Text <> Nothing Then
            If vGlobal.vModeloImpressao = "01" Then
                PrintImpurezaMineralMD01.Print()
            ElseIf vGlobal.vModeloImpressao = "02" Then
                bBotaoImprimir = True
                If rbBolAnl.Checked = True Then
                    PrintBolAnl_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintBolAnl_Laredo.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintBolAnl_Laredo.Print()
                    'PrintPreviewDialog1.Document = PrintBolAnl_Laredo
                    'PrintPreviewDialog1.Show()
                ElseIf rbExtratoSaida.Checked = True Then
                    PrintExtratoSaida_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintExtratoSaida_Laredo.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintExtratoSaida_Laredo.Print()
                    'PrintPreviewDialog1.Document = PrintExtratoSaida_Laredo
                    'PrintPreviewDialog1.Show()
                ElseIf RbExtProducto.Checked = True Then
                    PrintPesagProdutos_Laredo.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintPesagProdutos_Laredo.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintPesagProdutos_Laredo.Print()
                    'PrintPreviewDialog1.Document = PrintPesagProdutos_Laredo
                    'PrintPreviewDialog1.Show()
                End If

            ElseIf vGlobal.vModeloImpressao = "03" Then
                PrintPreviewDialog1.Document = PrintImpurezaMineralMD01
                PrintPreviewDialog1.ShowDialog()
            ElseIf vGlobal.vModeloImpressao = "00" And sCliente = "BALCANES" Then
                bBotaoImprimir = True
                If rbOrdemCarga.Checked = True Then
                    PrintOrdemCarga_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintOrdemCarga_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintOrdemCarga_Balcanes.Print()
                ElseIf rbExtratoSaida.Checked Then
                    PrintExtratoSaida_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintExtratoSaida_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintExtratoSaida_Balcanes.Print()
                ElseIf rbBolAnl.Checked Then
                    PrintBolAnl_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintBolAnl_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintBolAnl_Balcanes.Print()
                End If
                If bBotaoImprimir = True Then
                    bBotaoImprimir = False
                    tbBoletim.Clear()
                End If
            ElseIf vGlobal.vModeloImpressao = "04" And sCliente = "BUDEGUER" Then
                bBotaoImprimir = True
                'PrintOrdemCarga_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                'PrintOrdemCarga_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                'PrintOrdemCarga_Budeguer.Print()
                PrintPreviewDialog1.Document = PrintOrdemCarga_Budeguer
                PrintPreviewDialog1.ShowDialog()
                If bBotaoImprimir = True Then
                    bBotaoImprimir = False
                    tbBoletim.Clear()
                End If
            End If
        End If
    End Sub

    Private Sub AtualizaTablacargas(noLiberacao)
        Dim sql As String
        Dim rs As OracleDataReader
        sql = "update apt_cargas set fg_impressao_impureza = 'S' where no_liberacao = " & noLiberacao & ""
        rs = fCmd(sql).ExecuteReader
        fCmd(sql).Dispose()
        rs.Close()
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick
        'If nContadorTimer = vTimerImpressao Then
        '    If vGlobal.vModeloImpressao = "01" Then
        '        If fVerificaImpressao() = True Then
        '            PrintPesagProdutos.Print()
        '        End If
        '    ElseIf vGlobal.vModeloImpressao = "02" Then
        '        If fVerificaImpressaoImpureza() = True Then
        '            PrintImpurezaMineralMD01.Print()
        '        End If
        '        nContadorTimer = 0
        '    ElseIf vGlobal.vModeloImpressao = "03" Then
        '        'Impressão de etiqeta zebra na Laredo
        '        'Imprimir_Zebra()
        '        If fVerificaBolAnl() = True Then
        '            PrintBolAnl_Laredo.Print()
        '        End If
        '        nContadorTimer = 0
        '    ElseIf vGlobal.vModeloImpressao = "00" And vGlobal.sCliente = "BALCANES" Then
        '        'Impressões Balcanes
        '        fGravaLog("Inicio de la Impresión", "LOG_TIMER_IMPR")
        '        Impr_Balcanes()
        '        nContadorTimer = 0
        '    End If
        'ElseIf nContadorTimer < vTimerImpressao Then
        '    nContadorTimer += 1
        'End If
    End Sub

    Private Sub frmReimpressao_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        rbExtratoSaida.Visible = False
        rbOrdemCarga.Visible = False
        rbBolAnl.Visible = False
        RbExtProducto.Visible = False

        FechaAplic()
        AtualizaVariaveis()
        Conexao.conn = Conexao.fConexao()

        'Timer.Start()

        If vGlobal.vModeloImpressao = "01" Then
            lbTexto.Text = "Extrato:"
        ElseIf vGlobal.vModeloImpressao = "02" Then
            lbTexto.Text = "Boletín:"
        ElseIf vGlobal.vModeloImpressao = "03" Then
            lbTexto.Text = "Boletín:"
        End If

        If sCliente = "BALCANES" Or sCliente = "BUDEGUER" Then
            lbTexto.Text = "Camión:"
            Me.Width = 550
            Me.Height = 150
            If vGlobal.vImprOrdemCarga = "S" Then
                rbOrdemCarga.Visible = True
                IniciaProcOrdemCarga()
            Else
                rbOrdemCarga.Visible = False
            End If
            If vGlobal.vImprExtrSaida = "S" Then
                rbExtratoSaida.Visible = True
                IniciaProcExtratoSaida()
            Else
                rbExtratoSaida.Visible = False
            End If
            If vGlobal.vImprBolAnl = "S" Then
                rbBolAnl.Visible = True
                IniciaProcBoletimAnalises()
            Else
                rbBolAnl.Visible = False
            End If
        ElseIf sCliente = "LAREDO" Then
            If vGlobal.vImprBolAnl = "S" Then
                rbBolAnl.Visible = True
                IniciaProcBoletimAnalises()
            Else
                rbBolAnl.Visible = False
            End If

            If vGlobal.vImprExtrSaida = "S" Then
                rbExtratoSaida.Visible = True
            End If

            If vGlobal.vImprOrdemCarga = "S" Then
                rbOrdemCarga.Visible = True
            End If

            If vGlobal.vImprExtrSaida = "S" Then
                rbExtratoSaida.Visible = True
            End If

            If vGlobal.vImprExtrProduto = "S" Then
                RbExtProducto.Visible = True
                rbOrdemCarga.Visible = False
                RbExtProducto.Location = New Point(99, 58)
            End If

        Else
            Me.Width = 550
            Me.Height = 100
            rbExtratoSaida.Visible = False
            rbOrdemCarga.Visible = False
            rbBolAnl.Visible = False
            RbExtProducto.Visible = False
        End If

        tbBoletim.Text = Nothing

        lbVersao.Text = "Version: " & vVersion

    End Sub

    Private Sub AtualizaOutrPesagens(noPesagem)
        Try
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "update outr_pesagens" & vbNewLine &
                  "set pfg_impr_extrato = 'S'" & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and no_pesagem = " & noPesagem & ""
            rs = fCmd(sql).ExecuteReader
            fCmd(sql).Dispose()
            rs.Close()
        Catch ex As Exception
            vGlobal.vMensangem = ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Function fVerificaImpressao() As Boolean
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select * from outr_pesagens o" & vbNewLine &
                  "where o.zfg_impr_extrato != 'S'" & vbNewLine &
                  "  and o.instancia = '001'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("instancia")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If resultado <> Nothing Then
                Return True
            ElseIf resultado = Nothing Then
                Return False
            End If
        Catch ex As Exception
            Mensagem(ex.ToString)
        End Try
    End Function

    Private Function fVerificaImpressaoImpureza() As Boolean
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select fg_impressao_impureza from apt_cargas where fg_impressao_impureza is null"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("fg_impressao_impureza")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If resultado <> Nothing Then
                Return True
            ElseIf resultado = Nothing Then
                Return False
            End If
        Catch ex As Exception
            Mensagem(ex.ToString)
        End Try
    End Function

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
        If WindowState = FormWindowState.Minimized Then
            Hide()
            NotifyIcon1.Visible = True
            NotifyIcon1.ShowBalloonTip("3", "LuxArvum", "Impresión del extracto en segun plan", ToolTipIcon.Info)
        End If
    End Sub

    Private Sub frmImprExtr_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If (MessageBox.Show("Desea salir de la aplicación", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes) Then
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
    Private Sub PrintPesagProdutos_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintPesagProdutos_Laredo.PrintPage
        Try
            Dim noPesagem, noOrdemCarga, cdTransacao, cdTipoPesagem, cdTipoPesagemCod, cdProdutoCant, cdOrigem, cdDestino, cdTransportadora, cdProvedor, cdCaminhao As String
            Dim cdCondutor, cdReboque1, cdReboque2, cdReboque3, qtPesoLiquido, qtPesoEmbalagem, qtPesoLiquidoSemEmbalagem As String
            Dim dtHoraEntrada, dtHoraSaida, cdUsuario, deObservacao, pesagem, fgPesagEs, strDescricao, noDocumento As String
            Dim cdNivel1_O, cdNivel2_O, cdNivel3_O, cdNivel1_D, cdNivel2_D, cdNivel3_D, DescNivel1, DescNivel2, DescNivel3 As String
            Dim cdRecursoCG, cdRecursoTR, cdOperCG, cdOperTR, cdVariedade As String
            Dim qtPeso1, qtPeso2, qtPesoTara, qtPesoBruto, alturaLinha As Integer

            Dim sWhere As String

            DescNivel1 = fGetParam("NIVEIS", "UPNIVEL1")
            DescNivel2 = fGetParam("NIVEIS", "UPNIVEL2")
            DescNivel3 = fGetParam("NIVEIS", "UPNIVEL3")

            If bBotaoImprimir = True Then
                sWhere = "  and o.no_pesagem = " & tbBoletim.Text & ""
            Else
                sWhere = "  and o.no_pesagem = " & vGlobal.gn03 & vbNewLine &
                         "  and o.pfg_impr_extrato = 'N'"
            End If

            Dim sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select o.instancia,lpad(o.no_pesagem,10,0) no_pesagem, o.no_pesagem pesagem," & vbNewLine &
                  "       o.hr_entrada,o.hr_saida,o.fg_pesag_es,nvl(op.cd_varied,0) cd_varied," & vbNewLine &
                  "       tm.cd_tp_mov || '-' || tm.de_tp_mov tipo_transacao,o.fg_tp_pesag fg_tp_pesag_cod," & vbNewLine &
                  "       case" & vbNewLine &
                  "         when o.fg_tp_pesag = 'P' then" & vbNewLine &
                  "          'P - Prod.Acabados'" & vbNewLine &
                  "         when o.fg_tp_pesag = 'A' then" & vbNewLine &
                  "          'A - Açúcar'" & vbNewLine &
                  "         when o.fg_tp_pesag = 'T' then" & vbNewLine &
                  "          'T - Insumos-Resíduos'" & vbNewLine &
                  "         when o.fg_tp_pesag = 'M' then" & vbNewLine &
                  "          'M - Mudas de Cana'" & vbNewLine &
                  "         when o.fg_tp_pesag = 'O' then" & vbNewLine &
                  "          'O - Outros'" & vbNewLine &
                  "       end fg_tp_pesagem," & vbNewLine &
                  "       o.no_documento," & vbNewLine &
                  "       nvl(op.cd_origem,0) cd_origem," & vbNewLine &
                  "       nvl(op.cd_destino,0) cd_destino," & vbNewLine &
                  "       nvl(o.cd_fornec,0) cd_fornec," & vbNewLine &
                  "       nvl(o.cd_transport,0) cd_transport," & vbNewLine &
                  "       o.caminhao,o.motorista," & vbNewLine &
                  "       nvl(o.reboque,0) reboque_1," & vbNewLine &
                  "       nvl(o.reboque_2,0) reboque_2," & vbNewLine &
                  "       nvl(o.reboque_3,0) reboque_3," & vbNewLine &
                  "       o.cd_produto || '-' || p.de_produto produto," & vbNewLine &
                  "       op.qt_produto,o.qt_peso1,o.qt_peso2,o.qt_pesoliq," & vbNewLine &
                  "       o.qt_peso_emb,nvl(o.no_ordem_car,' ') no_ordem_car,nvl(o.de_observacao,' ') observacao,o.cd_usuario" & vbNewLine &
                  "  from outr_pesagens o" & vbNewLine &
                  " inner join outr_pesagens_prd op" & vbNewLine &
                  "    on o.instancia = op.instancia" & vbNewLine &
                  "   and o.no_pesagem = op.no_pesagem" & vbNewLine &
                  " inner join cad_produto p" & vbNewLine &
                  "    on p.cd_produto = op.cd_produto" & vbNewLine &
                  " inner join tipo_movimento tm" & vbNewLine &
                  "    on tm.cd_tp_mov = o.cd_tp_mov" & vbNewLine &
                  "   and o.instancia = '" & vInstLogin & "'" & vbNewLine &
                  "   and o.fg_status = 'C'" & vbNewLine &
                  "   " & sWhere & vbNewLine &
                  "   and rownum = 1"

            vGlobal.vMensangem += sql & vbNewLine

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                noPesagem = rs("no_pesagem")
                noOrdemCarga = rs("no_ordem_car")
                cdTransacao = rs("tipo_transacao")
                cdTipoPesagem = rs("fg_tp_pesagem")
                cdTipoPesagemCod = rs("fg_tp_pesag_cod")
                cdProdutoCant = rs("produto")
                If Not IsDBNull(rs("no_documento")) Then
                    noDocumento = rs("no_documento")
                Else
                    noDocumento = " "
                End If
                cdOrigem = rs("cd_origem") & " - " & FBuscaDescricaoProdutos("deposito", "cd_deposito", "de_deposito", rs("cd_origem"))
                cdDestino = rs("cd_destino") & " - " & FBuscaDescricaoProdutos("destinatario", "cd_destinat", "de_destinat", rs("cd_destino"))
                cdTransportadora = rs("cd_transport") & " - " & FBuscaDescricaoProdutos("transportadora", "cd_transport", "de_transport", rs("cd_transport"))
                cdProvedor = rs("cd_fornec") & " - " & FBuscaDescricaoProdutos("cad_fornec", "cd_fornec", "de_fornec", rs("cd_fornec"))
                cdCaminhao = rs("caminhao")
                cdCondutor = rs("motorista")
                cdReboque1 = rs("reboque_1")
                cdReboque2 = rs("reboque_2")
                cdReboque3 = rs("reboque_3")
                qtPeso1 = rs("qt_peso1")
                qtPeso2 = rs("qt_peso2")
                qtPesoLiquido = rs("qt_pesoliq")
                qtPesoEmbalagem = rs("qt_peso_emb")
                qtPesoLiquidoSemEmbalagem = (qtPesoLiquido - qtPesoEmbalagem)
                dtHoraEntrada = rs("hr_entrada")
                dtHoraSaida = rs("hr_saida")
                cdUsuario = rs("cd_usuario")
                deObservacao = rs("observacao")
                pesagem = rs("pesagem")
                fgPesagEs = rs("fg_pesag_es")

                If cdTipoPesagemCod = "M" Then
                    cdNivel1_O = FBuscaNiveisPrd(noPesagem, "1", "O")
                    cdNivel2_O = FBuscaNiveisPrd(noPesagem, "2", "O")
                    cdNivel3_O = FBuscaNiveisPrd(noPesagem, "3", "O")

                    cdNivel1_D = FBuscaNiveisPrd(noPesagem, "1", "D")
                    cdNivel2_D = FBuscaNiveisPrd(noPesagem, "2", "D")
                    cdNivel3_D = FBuscaNiveisPrd(noPesagem, "3", "D")

                    cdRecursoCG = FBuscaRecursoPrd(noPesagem, "CG", "EQ")
                    cdRecursoTR = FBuscaRecursoPrd(noPesagem, "TR", "EQ")
                    cdOperCG = FBuscaRecursoPrd(noPesagem, "CG", "OP")
                    cdOperTR = FBuscaRecursoPrd(noPesagem, "TR", "OP")
                    cdVariedade = rs("cd_varied")
                End If
            End While

            fCmd(sql).Dispose()
            rs.Close()

            vGlobal.sNomeEmpresa = fDescricao("empresas", "de_empresa", "cd_empresa", fGetParam("GERAIS", "CD_EMPRESA"))

            alturaLinha = 10

            Dim largura1 As Single = 270.0F
            Dim altura1 As Single = 30.0F
            Dim rect1 As New RectangleF(10, alturaLinha, largura1, altura1)
            e.Graphics.DrawString("*** " & vGlobal.sNomeEmpresa & " ***", Fonte10R, Brushes.Black, rect1, sf)

            alturaLinha += 30

            Dim largura2 As Single = 270.0F
            Dim altura2 As Single = 30.0F
            Dim rect2 As New RectangleF(10, alturaLinha, largura2, altura2)
            e.Graphics.DrawString("Extracto de Pesaje de Productos", Fonte10R, Brushes.Black, rect2, sf)

            alturaLinha += 40
            e.Graphics.DrawString("Numero Pesaje:   " & noPesagem & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Numero Orden Carga:  " & noOrdemCarga & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Tipo Movimentación:  " & cdTransacao & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            If noDocumento <> " " Then
                e.Graphics.DrawString("Documento:  " & noDocumento & "", Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
            End If
            e.Graphics.DrawString("Tipo de Pesaje:  " & cdTipoPesagem & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 30
            e.Graphics.DrawString("Producto Cant", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString(cdProdutoCant, Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            If cdTipoPesagemCod <> "M" Then
                e.Graphics.DrawString("Origen:  " & cdOrigem, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString("Destino:  " & cdDestino, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 30
            ElseIf cdTipoPesagemCod = "M" Then
                e.Graphics.DrawString("Variedad: " & cdVariedade, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString("O.:" & DescNivel1 & cdNivel1_O, Fonte9R, Brushes.Black, 10, alturaLinha)
                e.Graphics.DrawString("O.:" & DescNivel2 & cdNivel2_O, Fonte9R, Brushes.Black, 200, alturaLinha)
                e.Graphics.DrawString("O.:" & DescNivel3 & cdNivel3_O, Fonte9R, Brushes.Black, 300, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString("D.:" & DescNivel1 & cdNivel1_D, Fonte9R, Brushes.Black, 10, alturaLinha)
                e.Graphics.DrawString("D.:" & DescNivel2 & cdNivel2_D, Fonte9R, Brushes.Black, 200, alturaLinha)
                e.Graphics.DrawString("D.:" & DescNivel3 & cdNivel3_D, Fonte9R, Brushes.Black, 300, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString("Carg.:" & cdRecursoCG & "   Oper.:" & cdOperCG, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString("Tract.:" & cdRecursoTR & "   Oper.:" & cdOperTR, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
            End If
            e.Graphics.DrawString("Transportadora:  " & cdTransportadora & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Proveedor:  " & cdProvedor & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Camion:  " & cdCaminhao & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Conductor:  " & cdCondutor & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20

            If cdReboque1 <> "0" Then
                alturaLinha += 20
                e.Graphics.DrawString("1.Reboque:  " & cdReboque1 & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            End If
            If cdReboque2 <> "0" Then
                alturaLinha += 20
                e.Graphics.DrawString("1.Reboque:  " & cdReboque2 & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            End If
            If cdReboque3 <> "0" Then
                alturaLinha += 20
                e.Graphics.DrawString("1.Reboque:  " & cdReboque3 & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            End If

            'Na tela de pesagem
            'Peso Tara = Peso 1
            'Peso Bruto = Peso 2

            If fgPesagEs = "E" Then
                qtPesoTara = qtPeso2
                qtPesoBruto = qtPeso1
                strDescricao = "Entrada de Producto"
            ElseIf fgPesagEs = "S" Then
                qtPesoTara = qtPeso1
                qtPesoBruto = qtPeso2
                strDescricao = "Salida de Producto"
            End If

            alturaLinha += 40

            Dim largura3 As Single = 270.0F
            Dim altura3 As Single = 30.0F
            Dim rect3 As New RectangleF(10, alturaLinha, largura3, altura3)
            e.Graphics.DrawString("*** " & strDescricao & " ***", Fonte10R, Brushes.Black, rect3, sf)

            alturaLinha += 20
            e.Graphics.DrawString("Peso Tara:  " & qtPesoTara & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Peso Bruto:  " & qtPesoBruto & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Peso Neto:  " & qtPesoLiquido & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Peso Emb:  " & qtPesoEmbalagem & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Peso Neto Sem Emb:  " & qtPesoLiquidoSemEmbalagem & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Fec/Hr Entrada:  " & dtHoraEntrada & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Fec/Hr Salida:  " & dtHoraSaida & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Usuario:  " & cdUsuario & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Observacion:  " & deObservacao & "", Fonte9R, Brushes.Black, 10, alturaLinha)

            If pesagem <> Nothing Then
                AtualizaOutrPesagens(pesagem)
            End If

        Catch ex As Exception
            vGlobal.vMensangem += New System.Diagnostics.StackTrace(ex, True).GetFrame(0).GetFileLineNumber() & vbNewLine
            vGlobal.vMensangem += ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Function FBuscaNiveisPrd(Pesagem, Nivel, Tipo) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            Dim swhere As String = Nothing

            If Nivel = "1" And Tipo = "O" Then
                swhere = " nvl(cd_upnivel1_o,'0') "
            ElseIf Nivel = "1" And Tipo = "D" Then
                swhere = " nvl(cd_upnivel1_d,'0') "
            ElseIf Nivel = "2" And Tipo = "O" Then
                swhere = " nvl(cd_upnivel2_o,'0') "
            ElseIf Nivel = "2" And Tipo = "D" Then
                swhere = " nvl(cd_upnivel2_d,'0') "
            ElseIf Nivel = "3" And Tipo = "O" Then
                swhere = " nvl(cd_upnivel3_o,'0') "
            ElseIf Nivel = "3" And Tipo = "D" Then
                swhere = " nvl(cd_upnivel3_d,'0') "
            End If


            sql = "select" & swhere & "resultado from outr_pesagens_prd" & vbNewLine &
                  "where instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  "  and no_pesagem = " & Pesagem & ""

            vGlobal.vMensangem += "*** FBuscaNiveisPrd ***"
            vGlobal.vMensangem += sql & vbNewLine

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("resultado")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            If result <> Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function FBuscaRecursoPrd(Pesagem, TpRecurso, Dado) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader

            Dim swhere As String
            If Dado = "EQ" Then
                swhere = " nvl(cd_equipto,0) "
            ElseIf Dado = "OP" Then
                swhere = " nvl(cd_operador,0) "
            End If

            sql = "select" & swhere & "resultado from outr_pesagens_rec" & vbNewLine &
                  "where instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  "  and no_pesagem = " & Pesagem & vbNewLine &
                  "  and cd_tp_recurso = '" & TpRecurso & "'"

            vGlobal.vMensangem += "*** FBuscaRecursoPrd ***"
            vGlobal.vMensangem += sql & vbNewLine

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("resultado")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If result <> Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Public Function FVerifPesagProd() As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select * from outr_pesagens" & vbNewLine &
                  "where instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  "  and pfg_impr_extrato = 'N'" & vbNewLine &
                  "  and fg_status = 'C'"
            rs = fCmd(sql).ExecuteReader

            If rs.HasRows = True Then
                vGlobal.gn03 = rs("no_pesagem")
                fCmd(sql).Dispose()
                rs.Close()
                Return True
            Else
                fCmd(sql).Dispose()
                rs.Close()
                Return False
            End If

        Catch ex As Exception

        End Try
    End Function

    Private Function FBuscaDescricaoProdutos(Tabela, ColunaValor, ColunaDescricao, Valor) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select " & ColunaDescricao & " descricao from " & Tabela & vbNewLine &
                  "where " & ColunaValor & " = " & Valor & ""
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                If Not IsDBNull(rs("descricao")) Then
                    result = rs("descricao")
                End If
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If result <> Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception

        End Try
    End Function

    Private Sub PrintZebra_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintZebra.PrintPage

        Try
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim TipoFonte As String = "arial"
            Dim Fonte6R As New Font(TipoFonte, 6, FontStyle.Regular)
            Dim Fonte8R As New Font(TipoFonte, 8, FontStyle.Regular)
            Dim Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
            Dim Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
            Dim Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
            Dim Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)

            Dim Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
            Dim Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
            Dim Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
            Dim Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)

            Dim noLiberacao, noAnalise As String

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select c.no_liberacao,c.no_analise,r.cd_tp_recurso,r.cd_operador," & vbNewLine &
                  "r.cd_equipto, c.hr_entrada,f.cd_fren_tran || f.de_fren_tran frente" & vbNewLine &
                  "from apt_cargas c inner join apt_cargas_rec r" & vbNewLine &
                  "  on  r.no_liberacao = c.no_liberacao" & vbNewLine &
                  "inner join frentransp f" & vbNewLine &
                  "  on  f.cd_fren_tran = c.cd_fren_tran" & vbNewLine &
                  "  And r.cd_tp_recurso in ('CM','CT')" & vbNewLine &
                  "  And c.fg_analise = 'S'" & vbNewLine &
                  "  and c.fg_impressao_impureza is null" & vbNewLine &
                  "  and rownum = 1"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                noLiberacao = rs("no_liberacao")
                noAnalise = rs("no_analise")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            AtualizaTablacargas(noLiberacao)

            e.Graphics.DrawString("Materia Extraña", Fonte8R, Brushes.Black, 5, 3)
            barra = codbarras.Draw(noAnalise, altura)
            e.Graphics.DrawImage(barra, New Rectangle(3, 6, 31, 18))
            e.Graphics.DrawString(noLiberacao, Fonte6R, Brushes.Black, 3, 22)

            noLiberacao = Nothing

        Catch ex As Exception
            vGlobal.vMensangem = ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try

    End Sub

    Private Sub Imprimir_Zebra()
        Dim sFileName As String = "ImprZebra1.prn"
        Dim sFolder As String = ""
        Dim sSourceFile As String = ""
        Dim sString As String

        sFolder = Application.StartupPath
        sSourceFile = sFolder & "\" & sFileName

        If System.IO.File.Exists(sSourceFile) = False Then
            System.IO.File.Delete(sSourceFile)
            System.IO.File.Create(sSourceFile).Dispose()
        ElseIf System.IO.File.Exists(sSourceFile) = True Then
            System.IO.File.Create(sSourceFile).Dispose()
        End If

        'RawPrinterHelper.SendFileToPrinter(PrintDialog1.PrinterSettings.PrinterName, sSourceFile)
        RawPrinterHelper.SendFileToPrinter(vGlobal.vNomeImpressora, sSourceFile)

        Dim noLiberacao, noAnalise As String

        Dim sql As String
        Dim rs As OracleDataReader
        sql = "select c.no_liberacao,c.no_analise, r.cd_tp_recurso,r.cd_operador," & vbNewLine &
                  "r.cd_equipto, c.hr_entrada,f.cd_fren_tran || f.de_fren_tran frente" & vbNewLine &
                  "from apt_cargas c inner join apt_cargas_rec r" & vbNewLine &
                  "  on  r.no_liberacao = c.no_liberacao" & vbNewLine &
                  "inner join frentransp f" & vbNewLine &
                  "  on  f.cd_fren_tran = c.cd_fren_tran" & vbNewLine &
                  "  And r.cd_tp_recurso in ('CM','CT')" & vbNewLine &
                  "  And c.fg_analise = 'S'" & vbNewLine &
                  "" & sWhere & vbNewLine &
                  "  and c.fg_impressao_impureza is null" & vbNewLine &
                  "  and rownum = 1"
        rs = fCmd(sql).ExecuteReader
        While rs.Read
            noLiberacao = rs("no_liberacao")
            noAnalise = rs("no_analise")
        End While
        fCmd(sql).Dispose()
        rs.Close()

        AtualizaTablacargas(noLiberacao)

        sString += "^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR2,2~SD7^JUS^LRN^CI0^XZ" & vbCrLf
        sString += "^XA" & vbCrLf
        sString += "^MMT" & vbCrLf
        sString += "^PW240" & vbCrLf
        sString += "^LL0160" & vbCrLf
        sString += "^LS0" & vbCrLf
        sString += "^FT55,31^A0N,20,19^FH\^FDMateria Extra\A4a^FS" & vbCrLf
        sString += "^BY1,3,112^FT53,147^BCN,,Y,N" & vbCrLf
        sString += "^FD>;" & noAnalise & ">777747774^FS" & vbCrLf
        sString += "^PQ1,0,1,Y^XZ"

        If System.IO.File.Exists(sSourceFile) = True Then
            Dim ArquivoWriter As New System.IO.StreamWriter(sSourceFile, True)
            ArquivoWriter.WriteLine(sString)
            ArquivoWriter.Close()
        End If

        Dim sFileName2 As String = sFolder & "\" & "ImprZebra2.prn"
        Dim fs As New FileStream(sFileName2, FileMode.Create)
        Dim utf8 As New UTF8Encoding()

        Dim bw As New BinaryWriter(fs, utf8)

        bw.Write(sString)
        bw.Close()
        fs.Close()

        'RawPrinterHelper.SendFileToPrinter(PrintDialog1.PrinterSettings.PrinterName, sFileName2)
        RawPrinterHelper.SendFileToPrinter(vGlobal.vNomeImpressora, sFileName2)

    End Sub

    Public Function fVerificaBolAnl() As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select * from apt_cargas" & vbNewLine &
                  "where instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  "  and no_analise is not null" & vbNewLine &
                  "  and fg_analise = 'S'" & vbNewLine &
                  "  and fg_impr_bolanl = 'N'" & vbNewLine &
                  "  and rownum = 1"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("no_liberacao")
            End While

            noLiberacao = result

            If result = Nothing Then
                Return False
            ElseIf result <> Nothing Then
                AtualizaBolAnl_Laredo(noLiberacao)
                Return True
            End If
            fCmd(sql).Dispose()
            rs.Close()
        Catch ex As Exception

        End Try
    End Function

    Private Sub PrintBolAnl_Laredo_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintBolAnl_Laredo.PrintPage
        Try
            Dim sEmpresa, sCaminhao, sCarreta, sPlaca, sMotorista, sEntrada, sLiberacao, sViagem, sPesoBruto, sAnalise As String
            Dim sCdNivel1, sDeNivel1 As String
            Dim alturaLinha As Integer
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000
            Dim barra As Image

            Dim sWhere As String
            Dim sFiltro As String
            If bBotaoImprimir = True Then
                sFiltro = tbBoletim.Text.Replace(";", ",")
                sWhere = " and c.no_analise in (" & sFiltro & ")"
            ElseIf bBotaoImprimir = False Then
                sWhere = " and c.no_liberacao in (" & noLiberacao & ")"
            End If

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "SELECT *" & vbNewLine &
                  "FROM (" & vbNewLine &
                  "  SELECT c.no_liberacao," & vbNewLine &
                  "         c.no_analise," & vbNewLine &
                  "         c.hr_entrada," & vbNewLine &
                  "         r.cd_tp_recurso," & vbNewLine &
                  "         r.cd_equipto," & vbNewLine &
                  "         TRUNC(u.cd_upnivel1)||' '||u.de_upnivel1 cd_upnivel1," & vbNewLine &
                  "         c.id_viagem," & vbNewLine &
                  "         c.qt_bruto" & vbNewLine &
                  "  FROM apt_cargas c, apt_cargas_rec r, upnivel1 u, equiptos e" & vbNewLine &
                  "  WHERE c.instancia = r.instancia" & vbNewLine &
                  "    AND c.no_liberacao = r.no_liberacao" & vbNewLine &
                  "    AND u.cd_upnivel1 = c.cd_upnivel1" & vbNewLine &
                  "    AND e.cd_equipto = r.cd_equipto" & vbNewLine &
                  "    AND r.cd_tp_recurso IN ('CM', 'CT')" & vbNewLine &
                  "    AND c.no_analise IS NOT NULL" & vbNewLine &
                  "    AND c.fg_analise = 'S'" & vbNewLine &
                  " " & sWhere & " " & vbNewLine &
                  "   and c.instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  ")" & vbNewLine &
                  "PIVOT (" & vbNewLine &
                  "  MAX(cd_equipto)" & vbNewLine &
                  "  FOR cd_tp_recurso IN ('CM' AS CM, 'CT' AS CT))"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                sLiberacao = rs("no_liberacao")
                sViagem = rs("id_viagem")
                    sCaminhao = rs("CM")
                    If IsDBNull(rs("CT")) Then
                        sCarreta = " "
                    Else
                        sCarreta = rs("CT")
                    End If
                    sPlaca = fBuscaPlaca(sCaminhao, sLiberacao)
                sMotorista = fBuscaMotorista(sCaminhao, sLiberacao, "D")
                sEntrada = rs("hr_entrada")
                    sPesoBruto = rs("qt_bruto")
                    sAnalise = rs("no_analise")
                    sCdNivel1 = rs("cd_upnivel1")
            End While

            'sEmpresa = fDescricao("empresas", "de_empresa", "cd_empresa", fGetParam("GERAIS", "CD_EMPRESA"))
            sEmpresa = fGetParam("GERAIS", "DA_EMPRESA")
            sDeNivel1 = fGetParam("NIVEIS", "UPNIVEL1")

            alturaLinha = 10
            e.Graphics.DrawString("*** " & sEmpresa & " ***", Fonte10R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 40
            e.Graphics.DrawString("Ticket de Laboratorio", Fonte10R, Brushes.Black, 80, alturaLinha)

            alturaLinha += 30
            e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1, Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Camión:  " & sCaminhao & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            e.Graphics.DrawString("Patente:  " & sPlaca & "", Fonte9R, Brushes.Black, 130, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Chofer:  " & sMotorista & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Fecha/Hora Entrada:  " & sEntrada & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Liberación: " & sLiberacao.PadLeft(8, "0"c), Fonte9R, Brushes.Black, 10, alturaLinha)
            e.Graphics.DrawString("Nº Viaje: " & sViagem.PadLeft(6, "0"c), Fonte9R, Brushes.Black, 150, alturaLinha)
            alturaLinha += 30
            If sCarreta <> " " Then
                e.Graphics.DrawString("Remolque: " & sCarreta, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
            End If
            e.Graphics.DrawString("Peso Bruto: " & sPesoBruto, Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20

            'Linha 01
            Dim Point01 As New Point(10, 45)
            Dim Point11 As New Point(270, 45)
            Dim blackPen1 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen1, Point01, Point11)

            'Linha 02
            Dim Point02 As New Point(10, 70)
            Dim Point22 As New Point(270, 70)
            Dim blackPen2 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen2, Point02, Point22)

            'Linha 03
            Dim Point03 As New Point(10, 180)
            Dim Point33 As New Point(270, 180)
            Dim blackPen3 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen3, Point03, Point33)

            'Linha Patio
            Dim Point04 As New Point(10, 270)
            Dim Point44 As New Point(100, 270)
            Dim blackPen4 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen4, Point04, Point44)
            e.Graphics.DrawString("Patio", Fonte9R, Brushes.Black, 15, 275)

            'Linha laboratorio
            Dim Point05 As New Point(180, 270)
            Dim Point55 As New Point(270, 270)
            Dim blackPen5 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen5, Point05, Point55)
            e.Graphics.DrawString("Laboratorio", Fonte9R, Brushes.Black, 185, 275)

            barra = codbarras.Draw(sAnalise, altura)
            e.Graphics.DrawImage(barra, New Rectangle(70, 325, 140, 70))
            e.Graphics.DrawString(sAnalise.PadLeft(6, "0"c), Fonte9R, Brushes.Black, 70, 400)


        Catch ex As Exception
            vGlobal.vMensangem = ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Public Sub AtualizaBolAnl_Laredo(noLiberacao)
        Try
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "update apt_cargas" & vbNewLine &
                  "set fg_impr_bolanl = 'S'" & vbNewLine &
                  "where fg_impr_bolanl = 'N'" & vbNewLine &
                  "  and instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and no_liberacao = " & noLiberacao & ""
            vGlobal.vMensangem += sql & vbNewLine
            rs = fCmd(sql).ExecuteReader
            fCmd(sql).Dispose()
            rs.Close()
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "AtualizaBolAnl_Laredo", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Function fBuscaPlaca(equipamento, liberacao) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select e.cd_equipto,r.no_compl,e.no_placa from apt_cargas_rec r, equiptos e" & vbNewLine &
                  "where e.cd_equipto = r.cd_equipto" & vbNewLine &
                  "  and e.cd_equipto = " & equipamento & vbNewLine &
                  "  and r.no_liberacao = " & liberacao & vbNewLine &
                  "  and r.instancia = '" & vGlobal.vInstLogin & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                If IsDBNull(rs("no_compl")) Then
                    result = rs("no_placa")
                Else
                    result = rs("no_compl")
                End If
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If result = Nothing Then
                Return " "
            ElseIf result <> Nothing Then
                Return result
            End If
        Catch ex As Exception

        End Try
    End Function

    Private Function fBuscaMotorista(equipamento, liberacao, TipoDado) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select f.cd_func,f.de_func from apt_cargas_rec r, funcionars f" & vbNewLine &
                  "where f.cd_func = r.cd_operador" & vbNewLine &
                  "  and r.cd_tp_recurso = 'CM'" & vbNewLine &
                  "  and r.cd_equipto = " & equipamento & vbNewLine &
                  "  and r.no_liberacao = " & liberacao & "" & vbNewLine &
                  "  and r.instancia = '" & vGlobal.vInstLogin & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                If TipoDado = "D" Then
                    result = rs("cd_func") & " " & rs("de_func")
                ElseIf TipoDado = "C" Then
                    result = rs("cd_func")
                End If
            End While
            fCmd(sql).Dispose()
            rs.Close()

            If result = Nothing Then
                Return " "
            ElseIf result <> Nothing Then
                Return result
            End If
        Catch ex As Exception

        End Try
    End Function

    Private Function FBuscaFornec(CdNivel1) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader

            sql = "select f.cd_fornec||'-'||f.de_fornec fornec from upnivel1 u, fornecs f" & vbNewLine &
                  "where f.cd_fornec = u.cd_fornec" & vbNewLine &
                  "  and u.cd_upnivel1 = '" & CdNivel1 & "'"

            rs = fCmd(sql).ExecuteReader()
            While rs.Read
                result = rs("fornec")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            If result = Nothing Then
                Return 0
            Else
                Return result
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "FBuscaFornec", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Sub PrintExtratoSaida_Laredo_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Laredo.PrintPage
        Try
            Dim sEmpresa, sViagem, sNoSequencia, sMotorista, sCdFunc As String

            Dim HrInicial, HrFinal As DateTime

            Dim nTamMotorista As Integer
            Dim nTamNivel1_1, nTamNivel1_2, nTamNivel1_3, nTamNivel1_4 As Integer
            Dim nTamForn1_1, nTamForn1_2, nTamForn1_3, nTamForn1_4 As Integer

            'String primeira linha
            Dim sNoSequencia1, sCaminhao, sPlaca, sEntrada, sSaida, sCdFornec1, sCdNivel1_1, sDeNivel1, sLiberacao1, sPesoBruto1, sPesoTara1, sPesoNeto1 As String

            'String segunda linha
            Dim sNoSequencia2, sCarreta2, sCdFornec2, sCdNivel1_2, sLiberacao2, sPesoBruto2, sPesoTara2, sPesoNeto2 As String

            'String terceira linha
            Dim sNoSequencia3, sCarreta3, sCdFornec3, sCdNivel1_3, sLiberacao3, sPesoBruto3, sPesoTara3, sPesoNeto3 As String

            'String quarta linha
            Dim sNoSequencia4, sCarreta4, sCdFornec4, sCdNivel1_4, sLiberacao4, sPesoBruto4, sPesoTara4, sPesoNeto4 As String

            Dim alturaLinha As Integer
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000
            Dim barra As Image

            Dim sWhere As String
            Dim sFiltro As String
            If bBotaoImprimir = True Then
                sFiltro = tbBoletim.Text.Replace(";", ",")
                sWhere = " and c.id_viagem in (" & FBuscaIdViagem(sFiltro.Replace(";", ",")) & ")"
            ElseIf bBotaoImprimir = False Then
                sWhere = " and c.id_viagem in (" & noLiberacao & ")"
            End If

            Dim hrFechamento As String = fGetParam("RCMP", "HR_FECHAMENTO") & ":00"
            Dim DtHrIniPeriodo, DtHrFinalPeriodo As String

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select * from" & vbNewLine &
                    "(select c.no_liberacao," & vbNewLine &
                    "       c.id_viagem," & vbNewLine &
                    "       c.no_sequencia," & vbNewLine &
                    "       c.hr_entrada," & vbNewLine &
                    "       c.hr_saida," & vbNewLine &
                    "       t.cd_fornec," & vbNewLine &
                    "       t.de_fornec," & vbNewLine &
                    "       c.cd_upnivel1," & vbNewLine &
                    "       u.de_upnivel1," & vbNewLine &
                    "       r.cd_equipto," & vbNewLine &
                    "       r.cd_tp_recurso," & vbNewLine &
                    "       c.qt_distancia," & vbNewLine &
                    "       c.qt_bruto," & vbNewLine &
                    "       c.qt_tara," & vbNewLine &
                    "       c.qt_liquido" & vbNewLine &
                    "  from apt_cargas     c," & vbNewLine &
                    "       apt_cargas_rec r," & vbNewLine &
                    "       equiptos       e," & vbNewLine &
                    "       fornecs        t," & vbNewLine &
                    "       transporta     p," & vbNewLine &
                    "       upnivel1       u" & vbNewLine &
                    "where  u.cd_upnivel1 = c.cd_upnivel1" & vbNewLine &
                    "  and  p.cd_transp = e.cd_transp" & vbNewLine &
                    "  and  t.cd_fornec = c.cd_fornec" & vbNewLine &
                    "  and  e.cd_equipto = r.cd_equipto" & vbNewLine &
                    "  and  r.instancia = c.instancia" & vbNewLine &
                    "  and  r.no_liberacao = c.no_liberacao" & vbNewLine &
                    "  and  c.instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                    "" & sWhere & vbNewLine &
                    "  and  c.fg_status in ('C','P')" & vbNewLine &
                    "  and  r.cd_tp_recurso in ('CM','CT'))" & vbNewLine &
                    "pivot (" & vbNewLine &
                    "      max(cd_equipto)" & vbNewLine &
                    "  for cd_tp_recurso in ('CM' as CM,'CT' as CT))"

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                sNoSequencia = rs("no_sequencia")

                If sNoSequencia = 1 Then
                    sNoSequencia1 = rs("no_sequencia")
                    sLiberacao1 = rs("no_liberacao")
                    sViagem = rs("id_viagem")
                    sCaminhao = rs("CM")
                    sPlaca = fBuscaPlaca(sCaminhao, sLiberacao1)
                    sCdFunc = fBuscaMotorista(sCaminhao, sLiberacao1, "D")
                    nTamMotorista = sCdFunc.Length
                    If nTamMotorista > 28 Then
                        sCdFunc = sCdFunc.Substring(0, 28)
                    End If
                    sMotorista = fBuscaMotorista(sCaminhao, sLiberacao1, "C")
                    sEntrada = rs("hr_entrada")
                    HrInicial = Convert.ToDateTime(sEntrada)
                    DtHrIniPeriodo = HrInicial.ToString("dd/MM/yyyy") & " " & hrFechamento
                    HrFinal = Convert.ToDateTime(DtHrIniPeriodo)
                    HrFinal = HrFinal.AddHours(24).AddSeconds(-1)
                    DtHrFinalPeriodo = HrFinal.ToString
                    sSaida = rs("hr_saida")
                    sPesoBruto1 = rs("qt_bruto")
                    sPesoTara1 = rs("qt_tara")
                    sPesoNeto1 = rs("qt_liquido")
                    sCdNivel1_1 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
                    nTamNivel1_1 = sCdNivel1_1.Length
                    If nTamNivel1_1 > 33 Then
                        sCdNivel1_1 = sCdNivel1_1.Substring(0, 33)
                    End If
                    sCdFornec1 = FBuscaFornec(rs("cd_upnivel1"))
                    nTamForn1_1 = sCdFornec1.Length
                    If nTamForn1_1 > 28 Then
                        sCdFornec1 = sCdFornec1.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 2 Then
                    sNoSequencia2 = rs("no_sequencia")
                    sLiberacao2 = rs("no_liberacao")
                    sCarreta2 = rs("CT")
                    sPesoBruto2 = rs("qt_bruto")
                    sPesoTara2 = rs("qt_tara")
                    sPesoNeto2 = rs("qt_liquido")
                    sCdNivel1_2 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
                    nTamNivel1_2 = sCdNivel1_2.Length
                    If nTamNivel1_2 > 33 Then
                        sCdNivel1_2 = sCdNivel1_2.Substring(0, 33)
                    End If
                    sCdFornec2 = FBuscaFornec(rs("cd_upnivel1"))
                    nTamForn1_2 = sCdFornec2.Length
                    If nTamForn1_2 > 28 Then
                        sCdFornec2 = sCdFornec2.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 3 Then
                    sNoSequencia3 = rs("no_sequencia")
                    sLiberacao3 = rs("no_liberacao")
                    sCarreta3 = rs("CT")
                    sPesoBruto3 = rs("qt_bruto")
                    sPesoTara3 = rs("qt_tara")
                    sPesoNeto3 = rs("qt_liquido")
                    sCdNivel1_3 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
                    nTamNivel1_3 = sCdNivel1_3.Length
                    If nTamNivel1_3 > 33 Then
                        sCdNivel1_3 = sCdNivel1_3.Substring(0, 33)
                    End If
                    sCdFornec3 = FBuscaFornec(rs("cd_upnivel1"))
                    nTamForn1_3 = sCdFornec3.Length
                    If nTamNivel1_3 > 28 Then
                        sCdFornec3 = sCdFornec3.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 4 Then
                    sNoSequencia4 = rs("no_sequencia")
                    sLiberacao4 = rs("no_liberacao")
                    sCarreta4 = rs("CT")
                    sPesoBruto4 = rs("qt_bruto")
                    sPesoTara4 = rs("qt_tara")
                    sPesoNeto4 = rs("qt_liquido")
                    sCdNivel1_4 = rs("cd_upnivel1") & " " & FBucaNivel1(rs("cd_upnivel1"))
                    nTamNivel1_4 = sCdNivel1_4.Length
                    If nTamNivel1_4 > 33 Then
                        sCdNivel1_4 = sCdNivel1_4.Substring(0, 33)
                    End If
                    sCdFornec4 = FBuscaFornec(rs("cd_upnivel1"))
                    nTamForn1_4 = sCdFornec4.Length
                    If nTamForn1_4 > 28 Then
                        sCdFornec4 = sCdFornec4.Substring(0, 28)
                    End If
                End If
            End While

            For i = 1 To 4
                If i = 1 And sNoSequencia1 = 1 Then
                    sEmpresa = fGetParam("GERAIS", "DA_EMPRESA")
                    sDeNivel1 = fGetParam("NIVEIS", "UPNIVEL1")

                    Dim sf As New StringFormat()
                    sf.Alignment = StringAlignment.Center
                    sf.LineAlignment = StringAlignment.Center

                    Dim largura1 As Single = 270.0F
                    Dim altura1 As Single = 30.0F
                    Dim rect1 As New RectangleF(10.0F, 10.0F, largura1, altura1)
                    e.Graphics.DrawString("*** " & sEmpresa & " ***", Fonte10R, Brushes.Black, rect1, sf)

                    alturaLinha = 50

                    'Linha 01
                    Dim Point01 As New Point(10, alturaLinha)
                    Dim Point11 As New Point(270, alturaLinha)
                    Dim blackPen1 = New Pen(Color.Black, 1)
                    e.Graphics.DrawLine(blackPen1, Point01, Point11)

                    Dim largura2 As Single = 270.0F
                    Dim altura2 As Single = 20.0F
                    Dim rect2 As New RectangleF(10.0F, 50.0F, largura2, altura2)
                    e.Graphics.DrawString("Extracto de Pesaje de Caña", Fonte10R, Brushes.Black, rect2, sf)

                    'Linha 02
                    Dim Point02 As New Point(10, alturaLinha + 16)
                    Dim Point22 As New Point(270, alturaLinha + 16)
                    Dim blackPen2 = New Pen(Color.Black, 1)
                    e.Graphics.DrawLine(blackPen2, Point02, Point22)

                    alturaLinha += 30
                    e.Graphics.DrawString("Nombre:  " & sCdFunc & vbCrLf & vbCrLf & "", Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 20
                    e.Graphics.DrawString("Camión:  " & sCaminhao & "", Fonte9R, Brushes.Black, 10, alturaLinha)
                    e.Graphics.DrawString("Chofer:  " & sMotorista & "", Fonte9R, Brushes.Black, 130, alturaLinha)
                    alturaLinha += 20
                    e.Graphics.DrawString("Entrada:  " & sEntrada & "", Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 20
                    e.Graphics.DrawString("Salida:  " & sSaida & "", Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 20

                    'Linha 03
                    Dim Point03 As New Point(10, alturaLinha + 15)
                    Dim Point33 As New Point(270, alturaLinha + 15)
                    Dim blackPen3 = New Pen(Color.Black, 1)
                    e.Graphics.DrawLine(blackPen3, Point03, Point33)
                    e.Graphics.DrawString("Nº Viaje: " & sViagem.PadLeft(6, "0"c), Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 30
                    e.Graphics.DrawString("Proveedor: " & sCdFornec1, Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 20
                    e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_1, Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 20
                    e.Graphics.DrawString("Liberación   Equipo    Bruto    Tara       Neto", Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 20
                    e.Graphics.DrawString(sLiberacao1.PadLeft(8, "0") & "    " & sCaminhao & "    " & sPesoBruto1 & "    " & sPesoTara1 & "    " & sPesoNeto1, Fonte9R, Brushes.Black, 10, alturaLinha)
                    alturaLinha += 30
                ElseIf i = 2 And sNoSequencia2 = 2 Then
                    If sCarreta2 <> " " Then
                        e.Graphics.DrawString("Proveedor:  " & sCdFornec2, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_2, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString("Liberación   Equipo    Bruto    Tara       Neto", Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString(sLiberacao2.PadLeft(8, "0") & "    " & sCarreta2 & "    " & sPesoBruto2 & "    " & sPesoTara2 & "    " & sPesoNeto2, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 30
                    End If
                ElseIf i = 3 And sNoSequencia3 = 3 Then
                    If sCarreta2 <> " " Then
                        e.Graphics.DrawString("Proveedor: " & sCdFornec3, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_3, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString("Liberación   Equipo    Bruto    Tara       Neto", Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString(sLiberacao3.PadLeft(8, "0") & "    " & sCarreta3 & "    " & sPesoBruto3 & "    " & sPesoTara3 & "    " & sPesoNeto3, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 30
                    End If
                ElseIf i = 4 And sNoSequencia4 = 4 Then
                    If sCarreta4 <> " " Then
                        e.Graphics.DrawString("Proveedor: " & sCdFornec4, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString(sDeNivel1 & " " & sCdNivel1_4, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString("Liberación   Equipo    Bruto    Tara       Neto", Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 20
                        e.Graphics.DrawString(sLiberacao4.PadLeft(8, "0") & "    " & sCarreta4 & "    " & sPesoBruto4 & "    " & sPesoTara4 & "    " & sPesoNeto4, Fonte9R, Brushes.Black, 10, alturaLinha)
                        alturaLinha += 30
                    End If
                End If
            Next

            e.Graphics.DrawString("Peso Bruto: " & CDbl(sPesoBruto1) + CDbl(sPesoBruto2) + CDbl(sPesoBruto3) + CDbl(sPesoBruto4), Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Peso Tara.: " & CDbl(sPesoTara1) + CDbl(sPesoTara2) + CDbl(sPesoTara3) + CDbl(sPesoTara4), Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Peso Neto.: " & CDbl(sPesoNeto1) + CDbl(sPesoNeto2) + CDbl(sPesoNeto3) + CDbl(sPesoNeto4), Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 30

            'e.Graphics.DrawString("Peso Neto.: ", Fonte9R, Brushes.Black, 10, alturaLinha)
            'alturaLinha += 20
            'e.Graphics.DrawString("Peso Neto.: ", Fonte9R, Brushes.Black, 10, alturaLinha)
            'alturaLinha += 30

            'Linha 04
            Dim Point04 As New Point(10, alturaLinha)
            Dim Point44 As New Point(270, alturaLinha)
            Dim blackPen4 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen4, Point04, Point44)
            e.Graphics.DrawString("Total Transp: " & FTotalEquiptoExtSaida(sMotorista, DtHrIniPeriodo, DtHrFinalPeriodo), Fonte9R, Brushes.Black, 10, alturaLinha)
            'Linha 05
            Dim Point05 As New Point(10, alturaLinha + 15)
            Dim Point55 As New Point(270, alturaLinha + 15)
            Dim blackPen5 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen5, Point05, Point55)

        Catch ex As Exception
            vGlobal.vMensangem = ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Function FBuscaIdViagem(liberacao) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select id_viagem from apt_cargas" & vbNewLine &
                  "where instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  "  and no_liberacao in (" & liberacao & ")" & vbNewLine &
                  "  and rownum = 1"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("id_viagem")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If result = Nothing Then
                Return 0
            Else
                Return result
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "FBuscaIdViagem", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function FBucaNivel1(Nivel1) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select de_upnivel1 from upnivel1 where cd_upnivel1 = '" & Nivel1 & "'"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("de_upnivel1")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            If result = Nothing Then
                Return 0
            Else
                Return result
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "FBucaNivel1", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function FTotalEquiptoExtSaida(operador, hrInicial, hrFinal) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select qt_liquido - (desc_impur + desc_sacar) qt_liquido" & vbNewLine &
                  "  from (select r.qt_liquido," & vbNewLine &
                  "               nvl(c.pe_desc_impur, 0) / 100 pe_impur," & vbNewLine &
                  "               nvl(c.pe_desc_sacar, 0) / 100 pe_sacar," & vbNewLine &
                  "               r.qt_liquido * (nvl(c.pe_desc_impur, 0) / 100) desc_impur," & vbNewLine &
                  "               r.qt_liquido * (nvl(c.pe_desc_sacar, 0) / 100) desc_sacar" & vbNewLine &
                  "          from apt_cargas c, apt_cargas_rec r" & vbNewLine &
                  "         where c.instancia = r.instancia" & vbNewLine &
                  "           And c.no_liberacao = r.no_liberacao" & vbNewLine &
                  "           And r.cd_operador = " & operador & vbNewLine &
                  "           And r.instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  "           and c.hr_entrada between" & vbNewLine &
                  "               to_date('" & hrInicial & "', 'dd/mm/yyyy hh24:mi:ss') and" & vbNewLine &
                  "               to_date('" & hrFinal & "', 'dd/mm/yyyy hh24:mi:ss')" & vbNewLine &
                  "           and r.cd_tp_recurso = 'CM')"

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("qt_liquido")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            If result = Nothing Then
                Return 0
            Else
                Return result
            End If
        Catch ex As Exception

        End Try
    End Function

    '*** LAREDO - FINAL ***
    Private Function fDescrEmpresa(instancia)
        Try
            Dim resultado As String = Nothing
            Dim sql1, sql2 As String
            Dim rs1, rs2 As OracleDataReader

            sql1 = "select * from parametros" & vbNewLine &
                   "where instancia = '" & instancia & "'" & vbNewLine &
                   "  and secao = 'GERAIS'" & vbNewLine &
                   "  and entrada = 'CD_EMPRESA'"
            vGlobal.vMensangem += sql1 & vbNewLine
            rs1 = fCmd(sql1).ExecuteReader()
            While rs1.Read
                resultado = rs1("valor")
            End While
            fCmd(sql1).Dispose()
            rs1.Close()

            sql2 = "select de_empresa from empresas" & vbNewLine &
                   "where cd_empresa = '" & resultado & "'"
            vGlobal.vMensangem += sql2 & vbNewLine
            rs2 = fCmd(sql2).ExecuteReader
            While rs2.Read
                resultado = rs2("de_empresa")
            End While
            fCmd(sql2).Dispose()
            rs2.Close()

            Return resultado

        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fDescrEmpresa", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Public Sub AtualizaDespachoOCImpresso(IdDespacho)
        Try
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "update lt_despachos" & vbNewLine &
                  "set pfg_ordem_carga = 'S'" & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and id_despacho = " & IdDespacho & "" & vbNewLine &
                  "  and pfg_ordem_carga is null"
            vGlobal.vMensangem += sql & vbNewLine
            rs = fCmd(sql).ExecuteReader
            fCmd(sql).Dispose()
            rs.Close()
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "AtualizaDespachoOCImpresso", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

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
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "update apt_cargas" & vbNewLine &
                  "set pfg_impr_bolanl = 'S'" & vbNewLine &
                  "where pfg_impr_bolanl is null" & vbNewLine &
                  "  and instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and id_viagem = " & idViagem & ""
            vGlobal.vMensangem += sql & vbNewLine
            rs = fCmd(sql).ExecuteReader
            fCmd(sql).Dispose()
            rs.Close()
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "AtualizaBolAnl_Balcanes", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Function fListaCartoes(noQueima)
        Try
            Dim resultado As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select listagg(no_cartao,' / ') within group (order by cd_upnivel1,cd_upnivel2,cd_upnivel3) cartoes from queima_de" & vbNewLine &
                 "where no_queima = " & noQueima & ""
            vGlobal.vMensangem += sql & vbNewLine
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                resultado = rs("cartoes")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            Return resultado
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fListaCartoes", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function fPesoBrutoBolAnl(noLiberacao) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select sum(nvl(qt_bruto,0)) qt_bruto from apt_cargas" & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and no_liberacao = " & noLiberacao & ""
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("qt_bruto")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If result <> Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception
            MsgBox("fPesoBrutoBolAnl: " & ex.ToString)
        End Try
    End Function

    Private Sub PrintBolAnl_Balcanes_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintBolAnl_Balcanes.PrintPage
        Try
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim Fonte_9Cr As New Font("consolas", 9, FontStyle.Regular)
            Dim Fonte_9Ab As New Font("arial", 9, FontStyle.Bold)
            Dim Fonte_10Cr As New Font("arial", 10, FontStyle.Regular)
            Dim Fonte_10Ab As New Font("arial", 10, FontStyle.Bold)
            Dim Fonte_11Cr As New Font("arial", 11, FontStyle.Regular)
            Dim Fonte_11Ab As New Font("arial", 11, FontStyle.Bold)
            Dim Fonte_12Cr As New Font("arial", 12, FontStyle.Regular)
            Dim Fonte_12Ab As New Font("arial", 12, FontStyle.Bold)

            Dim noSeq, noLiberacao, noAnalise, cdCaminhao, cdMotorista, hrEntrada, cdTransporta, qtBruto As String
            Dim cdFazenda, cdFornecedor As String

            Dim bSeq1, bSeq2, bSeq3 As Boolean

            Dim barras1 As Image
            Dim barras2 As Image
            Dim barras3 As Image

            Dim barras4 As Image
            Dim barras5 As Image
            Dim barras6 As Image

            Dim IdViagem As String = vGlobal.gn03

            Dim swhere As String = Nothing
            If bBotaoImprimir = True Then
                Dim sIdViagem As String = fUltimaViagem(tbBoletim.Text, "A,P")
                swhere = " And c.pfg_impr_bolanl = 'S'" & vbNewLine &
                         " and c.id_viagem = " & fIdViagem(sIdViagem)
            ElseIf bBotaoImprimir = False Then
                swhere = "  and c.id_viagem =" & IdViagem & vbNewLine &
                         "  and c.pfg_impr_bolanl is null"
            End If

            Dim sql0 As String
            Dim rs0 As OracleDataReader

            Dim sql1 As String
            Dim rs1 As OracleDataReader
            sql1 = "select c.no_sequencia,c.no_liberacao,c.no_analise,e.cd_equipto||' '||e.no_placa equipo,r.cd_operador,t.cd_transp||'-'||t.de_transp transp," & vbNewLine &
                       "       c.hr_guarita,u.cd_upnivel1||'-'||u.de_upnivel1 upnivel1,f.cd_fornec||'-'||f.de_fornec fornec" & vbNewLine &
                       "from apt_cargas c, apt_cargas_rec r, equiptos e," & vbNewLine &
                       "     transporta t, upnivel1 u, fornecs f" & vbNewLine &
                       "where f.cd_fornec = u.cd_fornec" & vbNewLine &
                       "  and u.cd_upnivel1 = c.cd_upnivel1" & vbNewLine &
                       "  and t.cd_transp = e.cd_transp" & vbNewLine &
                       "  and e.cd_equipto = r.cd_equipto" & vbNewLine &
                       "  and r.instancia = c.instancia" & vbNewLine &
                       "  and r.no_liberacao = c.no_liberacao" & vbNewLine &
                       "  and c.instancia = '" & vInstLogin & "'" & vbNewLine &
                       "  and r.cd_tp_recurso = 'CM'" & vbNewLine &
                       "  and c.fg_analise = 'S'" & vbNewLine &
                       "" & swhere & ""
            rs1 = fCmd(sql1).ExecuteReader
            While rs1.Read
                noSeq = rs1("no_sequencia")
                vMensangem += "noSeq: " & rs1("no_sequencia") & vbNewLine
                noLiberacao = rs1("no_liberacao")
                vMensangem += "noLiberacao: " & rs1("no_liberacao") & vbNewLine
                cdCaminhao = rs1("equipo")
                vMensangem += "cdCaminhao: " & rs1("equipo") & vbNewLine
                If IsDBNull(rs1("cd_operador")) Then
                    cdMotorista = "0"
                Else
                    cdMotorista = rs1("cd_operador")
                End If

                hrEntrada = rs1("hr_guarita")
                cdTransporta = rs1("transp").ToString.Replace(",", " ")
                cdFazenda = rs1("upnivel1").ToString.Replace(",", " ")
                cdFornecedor = rs1("fornec").ToString.Replace(",", " ")
                noAnalise = rs1("no_analise")
                qtBruto = fPesoBrutoBolAnl(noLiberacao)

                barras1 = codbarras.Draw(noAnalise, altura)
                barras2 = codbarras.Draw(noAnalise, altura)
                barras3 = codbarras.Draw(noAnalise, altura)

                barras4 = codbarras.Draw(noLiberacao, altura)
                barras5 = codbarras.Draw(noLiberacao, altura)
                barras6 = codbarras.Draw(noLiberacao, altura)

                If noSeq = 1 Then
                    'Impressão para analise de PCTS
                    e.Graphics.DrawString("TALON PARA LABORATORIO", Fonte_12Ab, Brushes.Black, 36, 30)
                    e.Graphics.DrawString("Vehículo: " & cdCaminhao, Fonte_11Cr, Brushes.Black, 28, 60)
                    e.Graphics.DrawString("Conductor: " & cdMotorista, Fonte_11Cr, Brushes.Black, 28, 80)
                    e.Graphics.DrawString("Fletero: " & cdTransporta, Fonte_11Cr, Brushes.Black, 28, 100)
                    e.Graphics.DrawString("Entrada: " & hrEntrada, Fonte_11Cr, Brushes.Black, 28, 120)
                    e.Graphics.DrawString("Finca: " & cdFazenda, Fonte_11Cr, Brushes.Black, 28, 140)
                    e.Graphics.DrawString("Cañero: " & cdFornecedor, Fonte_11Cr, Brushes.Black, 28, 160)
                    e.Graphics.DrawString("Peso Bruto: " & qtBruto, Fonte_11Ab, Brushes.Black, 28, 180)
                    e.Graphics.DrawImage(barras1, New Rectangle(100, 220, 140, 70))
                    e.Graphics.DrawString("Nº Análisis: " & noAnalise & "", New Font("arial", 9, FontStyle.Bold), Brushes.Black, 100, 300)

                    'Impressão para analise de Impurezas
                    e.Graphics.DrawString("TALON PARA LABORATORIO TRASH", Fonte_12Ab, Brushes.Black, 408, 30)
                    e.Graphics.DrawString("Vehículo: " & cdCaminhao, Fonte_11Cr, Brushes.Black, 400, 60)
                    e.Graphics.DrawString("Conductor: " & cdMotorista, Fonte_11Cr, Brushes.Black, 400, 80)
                    e.Graphics.DrawString("Fletero: " & cdTransporta, Fonte_11Cr, Brushes.Black, 400, 100)
                    e.Graphics.DrawString("Entrada: " & hrEntrada, Fonte_11Cr, Brushes.Black, 400, 120)
                    e.Graphics.DrawString("Finca: " & cdFazenda, Fonte_11Cr, Brushes.Black, 400, 140)
                    e.Graphics.DrawString("Cañero: " & cdFornecedor, Fonte_11Cr, Brushes.Black, 400, 160)
                    e.Graphics.DrawString("Peso Bruto: " & qtBruto, Fonte_11Ab, Brushes.Black, 400, 180)
                    e.Graphics.DrawImage(barras4, New Rectangle(472, 220, 140, 70))
                    e.Graphics.DrawString("Nº Liberación: " & noLiberacao & "", New Font("arial", 9, FontStyle.Bold), Brushes.Black, 472, 300)

                    bSeq1 = True
                    bSeq2 = False
                    bSeq3 = False
                ElseIf noSeq = 2 Then
                    e.Graphics.DrawImage(barras2, New Rectangle(100, 320, 140, 70))
                    e.Graphics.DrawString("Nº Análisis: " & noAnalise & "", New Font("arial", 9, FontStyle.Bold), Brushes.Black, 100, 400)

                    e.Graphics.DrawImage(barras5, New Rectangle(472, 320, 140, 70))
                    e.Graphics.DrawString("Nº Liberación: " & noLiberacao & "", New Font("arial", 9, FontStyle.Bold), Brushes.Black, 472, 400)
                    bSeq1 = False
                    bSeq2 = True
                    bSeq3 = False
                ElseIf noSeq = 3 Then
                    e.Graphics.DrawImage(barras3, New Rectangle(100, 420, 140, 70))
                    e.Graphics.DrawString("Nº Análisis: " & noAnalise & "", New Font("arial", 9, FontStyle.Bold), Brushes.Black, 100, 500)

                    e.Graphics.DrawImage(barras6, New Rectangle(472, 420, 140, 70))
                    e.Graphics.DrawString("Nº Liberación: " & noLiberacao & "", New Font("arial", 9, FontStyle.Bold), Brushes.Black, 472, 500)
                    bSeq1 = False
                    bSeq2 = False
                    bSeq3 = True
                End If
            End While
            fCmd(sql1).Dispose()
            rs1.Close()

            'PCTS
            If bSeq1 = True Then
                Dim wPen02 As New Pen(Color.Black, 2)
                e.Graphics.DrawRectangle(wPen02, 15, 10, 350, 320)
            ElseIf bSeq2 = True Then
                Dim wPen02 As New Pen(Color.Black, 2)
                e.Graphics.DrawRectangle(wPen02, 15, 10, 350, 420)
            ElseIf bSeq3 = True Then
                Dim wPen02 As New Pen(Color.Black, 2)
                e.Graphics.DrawRectangle(wPen02, 15, 10, 350, 520)
            End If

            'IMPUREZA
            If bSeq1 = True Then
                Dim wPen02 As New Pen(Color.Black, 2)
                e.Graphics.DrawRectangle(wPen02, 390, 10, 350, 320)
            ElseIf bSeq2 = True Then
                Dim wPen02 As New Pen(Color.Black, 2)
                e.Graphics.DrawRectangle(wPen02, 390, 10, 350, 420)
            ElseIf bSeq3 = True Then
                Dim wPen02 As New Pen(Color.Black, 2)
                e.Graphics.DrawRectangle(wPen02, 390, 10, 350, 520)
            End If

            vGlobal.gn03 = IdViagem
        Catch ex As Exception
            'If vMensangem <> Nothing Then
            '    frmMensagem.Show()
            'End If
            fGravaError("frmReimpressao", "PrintBolAnl_Balcanes_PrintPage", ex.ToString, "LOG_BOLA_ERROR")
        End Try
    End Sub

    Private Sub PrintOrdemCarga_Balcanes_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintOrdemCarga_Balcanes.PrintPage
        Try
            bImpressoOrdemCargas = False
            vGlobal.sValor01 = Nothing
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim TipoFonte As String = "Courier New"
            Dim Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
            Dim Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
            Dim Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
            Dim Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)
            Dim Fonte13R As New Font(TipoFonte, 13, FontStyle.Regular)
            Dim Fonte14R As New Font(TipoFonte, 14, FontStyle.Regular)

            Dim Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
            Dim Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
            Dim Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
            Dim Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)
            Dim Fonte13B As New Font(TipoFonte, 13, FontStyle.Bold)
            Dim Fonte14B As New Font(TipoFonte, 14, FontStyle.Bold)

            Dim sDtEmissao, sHoraEmissao, sNoOrdemCarga, sFazenda, sFornecedor, sFrenteColheita, sCaminhao, sPrestador As String
            Dim sNoQueima, sCartoes As String
            Dim sWhere As String

            If bBotaoImprimir = True Then
                sWhere = "  and ld.pfg_ordem_carga = 'S' and ld.id_despacho = " & fUltimoDespacho(tbBoletim.Text) & ""
            ElseIf bBotaoImprimir = False Then
                sWhere = "  and ld.fg_status = 'A' and ld.pfg_ordem_carga is null"
            End If

            Dim sql As String
            Dim rs As OracleDataReader
            'Alteração dia 12/06/2023
            '"  and ld.instancia = llc.instancia" & vbNewLine &
            'Retirada do relacionamento da instancia da liberação com o despacho

            sql = "select ld.id_despacho," & vbNewLine &
                  "       to_char(ld.dthr_ret_planej,'dd/mm/yyyy') data," & vbNewLine &
                  "       to_char(ld.dthr_ret_planej,'hh24:mi:ss') hora," & vbNewLine &
                  "       fn.cd_fornec||'-'||fn.de_fornec fornecedor," & vbNewLine &
                  "       ft.cd_fren_tran||'-'||ft.de_fren_tran frente," & vbNewLine &
                  "       e.cd_equipto||'-'||e.no_placa veiculo," & vbNewLine &
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
                  "" & sWhere & ""
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                sNoOrdemCarga = rs("id_despacho")
                vGlobal.sValor01 = sNoOrdemCarga
                sDtEmissao = rs("data")
                sHoraEmissao = rs("hora")
                sFazenda = rs("fazenda")
                sFornecedor = rs("fornecedor")
                sFrenteColheita = rs("frente")
                sCaminhao = rs("veiculo")
                sPrestador = rs("prestador")
                sNoQueima = rs("no_queima")
                sCartoes = fListaCartoes(sNoQueima)
            End While
            fCmd(sql).Dispose()
            rs.Close()

            vGlobal.sNomeEmpresa = fDescrEmpresa(vInstLogin)

            'MARCA D'AGUA
            Dim Descr1 As String = vGlobal.gs01
            Dim Descr2 As String = vGlobal.gs02
            Dim font As New Font("Arial", 40, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
            Dim img As Image = Image.FromFile(Application.StartupPath & "\logotipo.jpg")
            'VIA 01
            e.Graphics.DrawString(Descr1, font, brush, 80, 155)
            e.Graphics.DrawString(Descr2, font, brush, 95, 205)
            e.Graphics.DrawImage(img, 30, 30)
            'VIA 02
            e.Graphics.DrawString(Descr1, font, brush, 80, 605)
            e.Graphics.DrawString(Descr2, font, brush, 95, 655)
            e.Graphics.DrawImage(img, 30, 480)

            'Primeira Via
            'e.Graphics.DrawString("Cia Azucarea Los Balcanes S.A", Fonte11R, Brushes.Black, 30, 30)
            e.Graphics.DrawString("ORDEN DE CARGA", Fonte13B, Brushes.Black, 195, 60)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte12B, Brushes.Black, 150, 100)
            e.Graphics.DrawString("Fecha:    " & sDtEmissao & "     Hora: " & sHoraEmissao & "     Número: " & sNoOrdemCarga, Fonte10R, Brushes.Black, 30, 140)
            e.Graphics.DrawString("Cañero:   " & sFornecedor, Fonte10R, Brushes.Black, 30, 160)
            e.Graphics.DrawString("Frente:   " & sFrenteColheita, Fonte10R, Brushes.Black, 30, 180)
            e.Graphics.DrawString("Vehículo: " & sCaminhao, Fonte10R, Brushes.Black, 30, 200)
            e.Graphics.DrawString("Fletero:  " & sPrestador, Fonte10R, Brushes.Black, 30, 220)
            e.Graphics.DrawString("Finca:    " & sFazenda, Fonte10R, Brushes.Black, 30, 240)
            e.Graphics.DrawString("Orden de Cosecha: " & sNoQueima, Fonte10R, Brushes.Black, 30, 260)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, Fonte10B, Brushes.Black, 30, 280)

            e.Graphics.DrawString("Descargar en: ", Fonte10R, Brushes.Black, 30, 310)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte12B, Brushes.Black, 165, 310)

            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 300, 140, 70))

            Dim wPen01 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen01, 25, 25, 650, 360)

            'Linha pontilhada
            Dim dashValues As Single() = {2, 2, 2, 2}
            Dim blackPen = New Pen(Color.Black, 1)
            Dim Point01 As New Point(10, 430)
            Dim Point02 As New Point(810, 430)
            blackPen.DashPattern = dashValues
            e.Graphics.DrawLine(blackPen, Point01, Point02)

            'Segunda Via
            'e.Graphics.DrawString("Cia Azucarea Los Balcanes S.A", Fonte11R, Brushes.Black, 30, 480)
            e.Graphics.DrawString("ORDEN DE CARGA", Fonte13B, Brushes.Black, 195, 510)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte12B, Brushes.Black, 150, 550)
            e.Graphics.DrawString("Fecha:    " & sDtEmissao & "     Hora: " & sHoraEmissao & "     Número: " & sNoOrdemCarga, Fonte10R, Brushes.Black, 30, 590)
            e.Graphics.DrawString("Cañero:   " & sFornecedor, Fonte10R, Brushes.Black, 30, 610)
            e.Graphics.DrawString("Frente:   " & sFrenteColheita, Fonte10R, Brushes.Black, 30, 630)
            e.Graphics.DrawString("Vehículo: " & sCaminhao, Fonte10R, Brushes.Black, 30, 650)
            e.Graphics.DrawString("Fletero:  " & sPrestador, Fonte10R, Brushes.Black, 30, 670)
            e.Graphics.DrawString("Finca:    " & sFazenda, Fonte10R, Brushes.Black, 30, 690)
            e.Graphics.DrawString("Orden de Cosecha: " & sNoQueima, Fonte10R, Brushes.Black, 30, 710)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, Fonte10B, Brushes.Black, 30, 730)

            e.Graphics.DrawString("Descargar en: ", Fonte10R, Brushes.Black, 30, 760)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte12B, Brushes.Black, 165, 760)

            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 750, 140, 70))

            Dim wPen02 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen02, 25, 475, 650, 360)

            bImpressoOrdemCargas = True

        Catch ex As Exception
            'vGlobal.vMensangem = ex.ToString
            'frmMensagem.Show()
            'vGlobal.vMensangem = Nothing
            fGravaError("frmReimpressao", "PrintOrdemCarga_Balcanes_PrintPage", ex.ToString, "LOG_OC_ERROR")
        End Try
    End Sub

    Private Sub PrintExtratoSaida_Balcanes_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Balcanes.PrintPage
        Try
            bImpressoOrdemCargas = False
            vGlobal.sValor02 = Nothing
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim TipoFonte As String = "Courier New"
            Dim Fonte8R As New Font(TipoFonte, 8, FontStyle.Regular)
            Dim Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
            Dim Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
            Dim Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
            Dim Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)
            Dim Fonte13R As New Font(TipoFonte, 13, FontStyle.Regular)
            Dim Fonte14R As New Font(TipoFonte, 14, FontStyle.Regular)

            Dim Fonte8B As New Font(TipoFonte, 8, FontStyle.Bold)
            Dim Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
            Dim Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
            Dim Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
            Dim Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)
            Dim Fonte13B As New Font(TipoFonte, 13, FontStyle.Bold)
            Dim Fonte14B As New Font(TipoFonte, 14, FontStyle.Bold)

            Dim sInstancia, sIdViagem, sNoLiberacao1, sNoLiberacao2, sNoLiberacao3, sFazenda As String
            Dim sNoLiberacao1_, sNoLiberacao2_, sNoLiberacao3_ As String
            Dim sNoSequencia, sNoSequencia1, sNoSequencia2, sNoSequencia3 As String
            Dim sQtBruto1, sQtBruto2, sQtBruto3 As String
            Dim sQtTara1, sQtTara2, sQtTara3 As String
            Dim sQtLiquido1, sQtLiquido2, sQtLiquido3 As String
            Dim sFornecedor, sEquipamento, sDtEntrada, sHrEntrada, sOrdemCarga, sTipoCana As String
            Dim sNoCartao1, sOrdemCorte1, sNivel11, sNivel21, sNivel31 As String
            Dim sNoCartao2, sOrdemCorte2, sNivel12, sNivel22, sNivel32 As String
            Dim sNoCartao3, sOrdemCorte3, sNivel13, sNivel23, sNivel33 As String
            Dim sNumeroRemito1, sNumeroRemito2, sNumeroRemito3 As String
            Dim sDtSaida, sHrSaida, sDtMovimento As String
            Dim sWhere, sLiberacoes, sTransportCM, sFrenTransp, sColhedora, sTrator As String

            Dim nTotalBruto, nTotalTara, nTotalLiqu As Double
            Dim sTotalBruto, sTotalTara, sTotalLiqu As String

            If bBotaoImprimir = True Then
                sLiberacoes = fUltimaViagem(tbBoletim.Text, "P, C")
                If vGlobal.vIMPRLIBSEPARADA = "S" Then
                    sWhere = "  And ac.pfg_impr_extr_saida = 'S' and ac.id_viagem = " & fIdViagem(sLiberacoes) & " and ac.no_liberacao = " & vGlobal.sValor04 & ""
                ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                    sWhere = "  and ac.pfg_impr_extr_saida = 'S' and ac.id_viagem = " & fIdViagem(sLiberacoes) & ""
                End If

            ElseIf bBotaoImprimir = False Then
                If vGlobal.vIMPRLIBSEPARADA = "S" Then
                    sWhere = "  and ac.pfg_impr_extr_saida is null and ac.no_liberacao = " & vGlobal.sValor04 & ""
                ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                    sWhere = "  and ac.pfg_impr_extr_saida is null"
                End If
            End If

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select ac.instancia,ac.id_viagem,ac.no_sequencia," & vbNewLine &
                  "       ac.no_liberacao,nvl(ac.ordem_carga,0) ordem_carga,nvl(ac.numero_remito,0) numero_remito," & vbNewLine &
                  "       fn.cd_fornec || '-' || fn.de_fornec fornecedor," & vbNewLine &
                  "       eq.cd_equipto || '-' || eq.no_placa equipamento," & vbNewLine &
                  "       t.cd_transp||'-'||t.de_transp transporta,decode(ac.fg_cana_crua,'S','Cruda','Quemada') tipo_cana," & vbNewLine &
                  "       trim(u1.cd_upnivel1) || '-' || u1.de_upnivel1 fazenda," & vbNewLine &
                  "       f.cd_fren_tran||'-'||f.de_fren_tran frente," & vbNewLine &
                  "       ac.dt_entrada,ac.hr_entrada," & vbNewLine &
                  "       ac.dt_saida,ac.hr_saida," & vbNewLine &
                  "       ac.dt_movimento,nvl(ac.qt_bruto,0) qt_bruto," & vbNewLine &
                  "       nvl(ac.qt_tara,0) qt_tara,nvl(ac.qt_liquido,0) qt_liquido," & vbNewLine &
                  "       ac.fg_cana_crua,ac.no_cartao,ac.no_queima," & vbNewLine &
                 "        trim(ac.cd_upnivel1) cd_upnivel1,trim(ac.cd_upnivel2) cd_upnivel2,trim(ac.cd_upnivel3) cd_upnivel3" & vbNewLine &
                  "  from apt_cargas     ac," & vbNewLine &
                  "       apt_cargas_rec acr," & vbNewLine &
                  "       equiptos       eq," & vbNewLine &
                  "       fornecs        fn," & vbNewLine &
                  "       upnivel1       u1," & vbNewLine &
                  "       transporta     t," & vbNewLine &
                  "       frentransp     f" & vbNewLine &
                  " WHERE f.cd_fren_tran = ac.cd_fren_tran" & vbNewLine &
                  "   And t.cd_transp = eq.cd_transp" & vbNewLine &
                  "   And u1.cd_upnivel1 = ac.cd_upnivel1" & vbNewLine &
                  "   And fn.cd_fornec = u1.cd_fornec" & vbNewLine &
                  "   And eq.cd_equipto = acr.cd_equipto" & vbNewLine &
                  "   And acr.instancia = ac.instancia" & vbNewLine &
                  "   And acr.no_liberacao = ac.no_liberacao" & vbNewLine &
                  "   And acr.cd_tp_recurso = 'CM'" & vbNewLine &
                  "   and ac.instancia = '" & vInstLogin & "'" & vbNewLine &
                  "   and ac.dt_saida is not null" & vbNewLine &
                  "" & sWhere & "" & vbNewLine &
                  " order by 2, 3"

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                sInstancia = rs("instancia")
                sIdViagem = rs("id_viagem")
                sNoSequencia = rs("no_sequencia")

                If sColhedora = Nothing Or sColhedora = 0 Then
                    sColhedora = fEquiptoPesagem(rs("no_liberacao"), "CD")
                End If

                If sTrator = Nothing Or sTrator = 0 Then
                    sTrator = fEquiptoPesagem(rs("no_liberacao"), "TT")
                End If

                If sNoSequencia = 1 Then
                    sNoSequencia1 = sNoSequencia
                    sNoLiberacao1 = rs("no_liberacao")
                    sNoLiberacao1 = sNoLiberacao1.PadRight(14, " ")
                    sNoLiberacao1_ = rs("no_liberacao")
                    nTotalBruto += rs("qt_bruto")
                    nTotalTara += rs("qt_tara")
                    nTotalLiqu += rs("qt_liquido")
                    sQtBruto1 = rs("qt_bruto")
                    sQtBruto1 = sQtBruto1.PadRight(14, " ")
                    sQtTara1 = rs("qt_tara")
                    sQtTara1 = sQtTara1.PadRight(14, " ")
                    sQtLiquido1 = rs("qt_liquido")
                    sQtLiquido1 = sQtLiquido1.PadRight(14, " ")
                    sFrenTransp = rs("frente")
                    sTipoCana = rs("tipo_cana")
                    sNoCartao1 = rs("no_cartao")
                    sOrdemCorte1 = rs("no_queima")
                    sNivel11 = rs("cd_upnivel1")
                    sNivel21 = rs("cd_upnivel2")
                    sNivel31 = rs("cd_upnivel3")
                ElseIf sNoSequencia = 2 Then
                    sNoSequencia2 = sNoSequencia
                    sNoLiberacao2 = rs("no_liberacao")
                    sNoLiberacao2 = sNoLiberacao2.PadRight(14, " ")
                    sNoLiberacao2_ = rs("no_liberacao")
                    nTotalBruto += rs("qt_bruto")
                    nTotalTara += rs("qt_tara")
                    nTotalLiqu += rs("qt_liquido")
                    sQtBruto2 = rs("qt_bruto")
                    sQtBruto2 = sQtBruto2.PadRight(14, " ")
                    sQtTara2 = rs("qt_tara")
                    sQtTara2 = sQtTara2.PadRight(14, " ")
                    sQtLiquido2 = rs("qt_liquido")
                    sQtLiquido2 = sQtLiquido2.PadRight(14, " ")
                    sNoCartao2 = rs("no_cartao")
                    sOrdemCorte2 = rs("no_queima")
                    sNivel12 = rs("cd_upnivel1")
                    sNivel22 = rs("cd_upnivel2")
                    sNivel32 = rs("cd_upnivel3")
                ElseIf sNoSequencia = 3 Then
                    sNoSequencia3 = sNoSequencia
                    sNoLiberacao3 = rs("no_liberacao")
                    sNoLiberacao3 = sNoLiberacao3.PadRight(14, " ")
                    sNoLiberacao3_ = rs("no_liberacao")
                    nTotalBruto += rs("qt_bruto")
                    nTotalTara += rs("qt_tara")
                    nTotalLiqu += rs("qt_liquido")
                    sQtBruto3 = rs("qt_bruto")
                    sQtBruto3 = sQtBruto3.PadRight(14, " ")
                    sQtTara3 = rs("qt_tara")
                    sQtTara3 = sQtTara3.PadRight(14, " ")
                    sQtLiquido3 = rs("qt_liquido")
                    sQtLiquido3 = sQtLiquido3.PadRight(14, " ")
                    sNoCartao3 = rs("no_cartao")
                    sOrdemCorte3 = rs("no_queima")
                    sNivel13 = rs("cd_upnivel1")
                    sNivel23 = rs("cd_upnivel2")
                    sNivel33 = rs("cd_upnivel3")
                End If
                sTotalTara = nTotalTara
                sTotalLiqu = nTotalLiqu
                sTotalBruto = nTotalBruto
                sTotalTara = sTotalTara.PadRight(14, " ")
                sTotalLiqu = sTotalLiqu.PadRight(14, " ")
                sTotalBruto = sTotalBruto.PadRight(14, " ")
                sFazenda = rs("fazenda")
                sFornecedor = rs("fornecedor")
                sEquipamento = rs("equipamento")
                sTransportCM = rs("transporta")
                sDtEntrada = rs("dt_entrada")
                sHrEntrada = rs("hr_entrada")
                sDtSaida = rs("dt_saida")
                sHrSaida = rs("hr_saida")
                sDtMovimento = rs("dt_movimento")
                sOrdemCarga = rs("ordem_carga")
                If sNoSequencia = 1 Then
                    sNumeroRemito1 = rs("numero_remito")
                    sNumeroRemito1 = sNumeroRemito1.PadRight(14, " ")
                ElseIf sNoSequencia = 2 Then
                    sNumeroRemito2 = rs("numero_remito")
                    sNumeroRemito2 = sNumeroRemito2.PadRight(14, " ")
                ElseIf sNoSequencia = 3 Then
                    sNumeroRemito3 = rs("numero_remito")
                    sNumeroRemito3 = sNumeroRemito3.PadRight(14, " ")
                End If

            End While
            fCmd(sql).Dispose()
            rs.Close()

            vGlobal.sNomeEmpresa = fDescrEmpresa(vInstLogin)

            vGlobal.sValor02 = sIdViagem

            vGlobal.gs03 = vGlobal.sValor02.PadLeft(10, "0")


            'MARCA D'AGUA
            Dim Descr1 As String = vGlobal.gs01
            Dim Descr2 As String = vGlobal.gs02
            Dim font As New Font("Arial", 40, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
            Dim img As Image = Image.FromFile(Application.StartupPath & "\logotipo.jpg")
            'VIA 01
            e.Graphics.DrawString(Descr1, font, brush, 50, 105)
            e.Graphics.DrawString(Descr2, font, brush, 65, 155)
            e.Graphics.DrawImage(img, 30, 30)
            'VIA 02
            e.Graphics.DrawString(Descr1, font, brush, 50, 360)
            e.Graphics.DrawString(Descr2, font, brush, 65, 410)
            e.Graphics.DrawImage(img, 30, 285)
            'VIA 03
            e.Graphics.DrawString(Descr1, font, brush, 50, 615)
            e.Graphics.DrawString(Descr2, font, brush, 65, 665)
            e.Graphics.DrawImage(img, 30, 540)
            'VIA 04
            e.Graphics.DrawString(Descr1, font, brush, 50, 875)
            e.Graphics.DrawString(Descr2, font, brush, 65, 925)
            e.Graphics.DrawImage(img, 30, 800)

            'Linha pontilhada
            Dim dashValues As Single() = {2, 2, 2, 2}
            Dim blackPen = New Pen(Color.Black, 1)
            blackPen.DashPattern = dashValues
            'VIA01
            Dim PointV101 As New Point(10, 270)
            Dim PointV102 As New Point(810, 270)
            e.Graphics.DrawLine(blackPen, PointV101, PointV102)
            'VIA02
            Dim PointV201 As New Point(10, 525)
            Dim PointV202 As New Point(810, 525)
            e.Graphics.DrawLine(blackPen, PointV201, PointV202)
            'VIA03
            Dim PointV301 As New Point(10, 780)
            Dim PointV302 As New Point(810, 780)
            e.Graphics.DrawLine(blackPen, PointV301, PointV302)

            'Primeira Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 30)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 50)

            e.Graphics.DrawString("Nº: " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 80)

            e.Graphics.DrawString("Origen:    " & sFazenda, Fonte8R, Brushes.Black, 30, 100)
            e.Graphics.DrawString("Orden de Carga: " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 100)
            e.Graphics.DrawString("Cañero:    " & sFornecedor, Fonte8R, Brushes.Black, 30, 110)
            e.Graphics.DrawString("Entrada:   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 120)
            e.Graphics.DrawString("Salida:    " & sHrSaida, Fonte8R, Brushes.Black, 250, 120)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 140)
            e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 140)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 150)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 150)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 160)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 160)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 170)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 170)
            End If
            Dim sDescrTotal As String
            sDescrTotal = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 180)
            e.Graphics.DrawString("Camión: " & sEquipamento, Fonte8R, Brushes.Black, 29, 190)
            e.Graphics.DrawString("Transportista: " & sTransportCM, Fonte8R, Brushes.Black, 30, 200)
            e.Graphics.DrawString("Frente: " & sFrenTransp, Fonte8R, Brushes.Black, 30, 210)
            e.Graphics.DrawString("Cosechadora: " & sColhedora, Fonte8R, Brushes.Black, 180, 210)
            e.Graphics.DrawString("Cuartero: " & sTrator, Fonte8R, Brushes.Black, 320, 210)
            e.Graphics.DrawString("Tipo Caña:" & sTipoCana, Fonte8R, Brushes.Black, 440, 210)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, Fonte8R, Brushes.Black, 29, 220)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, Fonte8R, Brushes.Black, 29, 230)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, Fonte8R, Brushes.Black, 29, 240)
            End If


            Dim wPen01 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen01, 25, 25, 570, 235)

            'Segunda Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 285)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 305)
            e.Graphics.DrawString("Nº " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 335)

            e.Graphics.DrawString("Origen    " & sFazenda, Fonte8R, Brushes.Black, 30, 355)
            e.Graphics.DrawString("Orden de Carga " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 355)
            e.Graphics.DrawString("Cañero    " & sFornecedor, Fonte8R, Brushes.Black, 30, 365)
            e.Graphics.DrawString("Entrada   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 375)
            e.Graphics.DrawString("Salida    " & sHrSaida, Fonte8R, Brushes.Black, 250, 375)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 395)
            e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 395)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 405)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 405)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 415)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 415)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 425)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 425)
            End If
            Dim sDescrTotal2 As String
            sDescrTotal2 = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal2 & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 435)
            e.Graphics.DrawString("Camión " & sEquipamento, Fonte8R, Brushes.Black, 29, 445)
            e.Graphics.DrawString("Transportista " & sTransportCM, Fonte8R, Brushes.Black, 30, 455)
            e.Graphics.DrawString("Frente " & sFrenTransp, Fonte8R, Brushes.Black, 30, 465)
            e.Graphics.DrawString("Cosechadora " & sColhedora, Fonte8R, Brushes.Black, 180, 465)
            e.Graphics.DrawString("Cuartero " & sTrator, Fonte8R, Brushes.Black, 320, 465)
            e.Graphics.DrawString("Tipo Caña" & sTipoCana, Fonte8R, Brushes.Black, 440, 465)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, Fonte8R, Brushes.Black, 29, 475)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, Fonte8R, Brushes.Black, 29, 485)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, Fonte8R, Brushes.Black, 29, 495)
            End If

            Dim wPen02 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen02, 25, 280, 570, 235)

            'Terceira Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 540)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 560)
            e.Graphics.DrawString("Nº " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 590)

            e.Graphics.DrawString("Origen    " & sFazenda, Fonte8R, Brushes.Black, 30, 610)
            e.Graphics.DrawString("Orden de Carga " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 610)
            e.Graphics.DrawString("Cañero    " & sFornecedor, Fonte8R, Brushes.Black, 30, 620)
            e.Graphics.DrawString("Entrada   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 630)
            e.Graphics.DrawString("Salida    " & sHrSaida, Fonte8R, Brushes.Black, 250, 630)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 650)
            e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 650)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 660)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 660)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 670)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 670)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 680)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 680)
            End If
            Dim sDescrTotal3 As String
            sDescrTotal3 = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal3 & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 690)
            e.Graphics.DrawString("Camión " & sEquipamento, Fonte8R, Brushes.Black, 29, 700)
            e.Graphics.DrawString("Transportista " & sTransportCM, Fonte8R, Brushes.Black, 30, 710)
            e.Graphics.DrawString("Frente " & sFrenTransp, Fonte8R, Brushes.Black, 30, 720)
            e.Graphics.DrawString("Cosechadora " & sColhedora, Fonte8R, Brushes.Black, 180, 720)
            e.Graphics.DrawString("Cuartero " & sTrator, Fonte8R, Brushes.Black, 320, 720)
            e.Graphics.DrawString("Tipo Caña" & sTipoCana, Fonte8R, Brushes.Black, 440, 720)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, Fonte8R, Brushes.Black, 29, 730)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, Fonte8R, Brushes.Black, 29, 740)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, Fonte8R, Brushes.Black, 29, 750)
            End If

            Dim wPen03 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen03, 25, 535, 570, 235)

            'Quarta Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 800)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 820)
            e.Graphics.DrawString("Nº " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 850)

            e.Graphics.DrawString("Origen    " & sFazenda, Fonte8R, Brushes.Black, 30, 870)
            e.Graphics.DrawString("Orden de Carga " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 870)
            e.Graphics.DrawString("Cañero    " & sFornecedor, Fonte8R, Brushes.Black, 30, 880)
            e.Graphics.DrawString("Entrada   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 890)
            e.Graphics.DrawString("Salida    " & sHrSaida, Fonte8R, Brushes.Black, 250, 890)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 910)
            e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 910)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 920)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 920)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 930)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 930)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 940)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 940)
            End If
            Dim sDescrTotal4 As String
            sDescrTotal4 = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal4 & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 950)
            e.Graphics.DrawString("Camión " & sEquipamento, Fonte8R, Brushes.Black, 29, 960)
            e.Graphics.DrawString("Transportista " & sTransportCM, Fonte8R, Brushes.Black, 30, 970)
            e.Graphics.DrawString("Frente " & sFrenTransp, Fonte8R, Brushes.Black, 30, 980)
            e.Graphics.DrawString("Cosechadora " & sColhedora, Fonte8R, Brushes.Black, 180, 980)
            e.Graphics.DrawString("Cuartero " & sTrator, Fonte8R, Brushes.Black, 320, 980)
            e.Graphics.DrawString("Tipo Caña" & sTipoCana, Fonte8R, Brushes.Black, 440, 980)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, Fonte8R, Brushes.Black, 29, 990)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, Fonte8R, Brushes.Black, 29, 1000)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, Fonte8R, Brushes.Black, 29, 1010)
            End If

            Dim wPen04 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen03, 25, 795, 570, 235)

            bImpressoOrdemCargas = True

        Catch ex As Exception
            'vGlobal.vMensangem = ex.ToString
            'frmMensagem.Show()
            'vGlobal.vMensangem = Nothing
            fGravaError("frmReimpressao", "PrintExtratoSaida_Balcanes_PrintPage", ex.ToString, "LOG_EXS_ERROR")
        End Try
    End Sub

    'Private Sub PrintExtratoSaida_Balcanes_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Balcanes.PrintPage
    '    Try
    '        bImpressoOrdemCargas = False
    '        vGlobal.sValor02 = Nothing
    '        Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
    '        Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
    '        Dim altura As Integer = 1000

    '        Dim barra As Image

    '        Dim TipoFonte As String = "Courier New"
    '        Dim Fonte8R As New Font(TipoFonte, 8, FontStyle.Regular)
    '        Dim Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
    '        Dim Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
    '        Dim Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
    '        Dim Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)
    '        Dim Fonte13R As New Font(TipoFonte, 13, FontStyle.Regular)
    '        Dim Fonte14R As New Font(TipoFonte, 14, FontStyle.Regular)

    '        Dim Fonte8B As New Font(TipoFonte, 8, FontStyle.Bold)
    '        Dim Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
    '        Dim Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
    '        Dim Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
    '        Dim Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)
    '        Dim Fonte13B As New Font(TipoFonte, 13, FontStyle.Bold)
    '        Dim Fonte14B As New Font(TipoFonte, 14, FontStyle.Bold)

    '        Dim sInstancia, sIdViagem, sNoLiberacao1, sNoLiberacao2, sNoLiberacao3, sFazenda As String
    '        Dim sNoSequencia, sNoSequencia1, sNoSequencia2, sNoSequencia3 As String
    '        Dim sQtBruto1, sQtBruto2, sQtBruto3 As String
    '        Dim sQtTara1, sQtTara2, sQtTara3 As String
    '        Dim sQtLiquido1, sQtLiquido2, sQtLiquido3 As String
    '        Dim sFornecedor, sEquipamento, sDtEntrada, sHrEntrada, sOrdemCarga, sTipoCana, sNoCartao, sOrdemCorte, sNivel1, sNivel2, sNivel3 As String
    '        Dim sNumeroRemito1, sNumeroRemito2, sNumeroRemito3 As String
    '        Dim sDtSaida, sHrSaida, sDtMovimento As String
    '        Dim sWhere, sLiberacoes, sTransportCM, sFrenTransp, sColhedora, sTrator As String

    '        Dim nTotalBruto, nTotalTara, nTotalLiqu As Double
    '        Dim sTotalBruto, sTotalTara, sTotalLiqu As String

    '        If bBotaoImprimir = True Then
    '            sLiberacoes = fUltimaViagem(tbBoletim.Text, "P,C")
    '            sWhere = "  And ac.pfg_impr_extr_saida = 'S' and ac.id_viagem = " & fIdViagem(sLiberacoes) & ""
    '        ElseIf bBotaoImprimir = False Then
    '            sWhere = "  and ac.pfg_impr_extr_saida is null"
    '        End If

    '        Dim sql As String
    '        Dim rs As OracleDataReader
    '        sql = "select ac.instancia,ac.id_viagem,ac.no_sequencia," & vbNewLine &
    '              "       ac.no_liberacao,nvl(ac.ordem_carga,0) ordem_carga,nvl(ac.numero_remito,0) numero_remito," & vbNewLine &
    '              "       fn.cd_fornec || '-' || fn.de_fornec fornecedor," & vbNewLine &
    '              "       eq.cd_equipto || '-' || eq.no_placa equipamento," & vbNewLine &
    '              "       t.cd_transp||'-'||t.de_transp transporta,decode(ac.fg_cana_crua,'S','Cruda','Quemada') tipo_cana," & vbNewLine &
    '              "       trim(u1.cd_upnivel1) || '-' || u1.de_upnivel1 fazenda," & vbNewLine &
    '              "       f.cd_fren_tran||'-'||f.de_fren_tran frente," & vbNewLine &
    '              "       ac.dt_entrada,ac.hr_entrada," & vbNewLine &
    '              "       ac.dt_saida,ac.hr_saida," & vbNewLine &
    '              "       ac.dt_movimento,nvl(ac.qt_bruto,0) qt_bruto," & vbNewLine &
    '              "       nvl(ac.qt_tara,0) qt_tara,nvl(ac.qt_liquido,0) qt_liquido," & vbNewLine &
    '              "       ac.fg_cana_crua,ac.no_cartao,ac.no_queima," & vbNewLine &
    '             "        trim(ac.cd_upnivel1) cd_upnivel1,trim(ac.cd_upnivel2) cd_upnivel2,trim(ac.cd_upnivel3) cd_upnivel3" & vbNewLine &
    '              "  from apt_cargas     ac," & vbNewLine &
    '              "       apt_cargas_rec acr," & vbNewLine &
    '              "       equiptos       eq," & vbNewLine &
    '              "       fornecs        fn," & vbNewLine &
    '              "       upnivel1       u1," & vbNewLine &
    '              "       transporta     t," & vbNewLine &
    '              "       frentransp     f" & vbNewLine &
    '              " WHERE f.cd_fren_tran = ac.cd_fren_tran" & vbNewLine &
    '              "   And t.cd_transp = eq.cd_transp" & vbNewLine &
    '              "   And u1.cd_upnivel1 = ac.cd_upnivel1" & vbNewLine &
    '              "   And fn.cd_fornec = u1.cd_fornec" & vbNewLine &
    '              "   And eq.cd_equipto = acr.cd_equipto" & vbNewLine &
    '              "   And acr.instancia = ac.instancia" & vbNewLine &
    '              "   And acr.no_liberacao = ac.no_liberacao" & vbNewLine &
    '              "   And acr.cd_tp_recurso = 'CM'" & vbNewLine &
    '              "   and ac.instancia = '" & vInstLogin & "'" & vbNewLine &
    '              "   and ac.dt_saida is not null" & vbNewLine &
    '              "" & sWhere & "" & vbNewLine &
    '              " order by 2, 3"


    '        rs = fCmd(sql).ExecuteReader
    '        While rs.Read
    '            sInstancia = rs("instancia")
    '            sIdViagem = rs("id_viagem")
    '            sNoSequencia = rs("no_sequencia")

    '            If sColhedora = Nothing Or sColhedora = 0 Then
    '                sColhedora = fEquiptoPesagem(rs("no_liberacao"), "CD")
    '            End If

    '            If sTrator = Nothing Or sTrator = 0 Then
    '                sTrator = fEquiptoPesagem(rs("no_liberacao"), "TT")
    '            End If

    '            If sNoSequencia = 1 Then
    '                sNoSequencia1 = sNoSequencia
    '                sNoLiberacao1 = rs("no_liberacao")
    '                sNoLiberacao1 = sNoLiberacao1.PadRight(14, " ")
    '                nTotalBruto += rs("qt_bruto")
    '                nTotalTara += rs("qt_tara")
    '                nTotalLiqu += rs("qt_liquido")
    '                sQtBruto1 = rs("qt_bruto")
    '                sQtBruto1 = sQtBruto1.PadRight(14, " ")
    '                sQtTara1 = rs("qt_tara")
    '                sQtTara1 = sQtTara1.PadRight(14, " ")
    '                sQtLiquido1 = rs("qt_liquido")
    '                sQtLiquido1 = sQtLiquido1.PadRight(14, " ")
    '                sFrenTransp = rs("frente")
    '                sTipoCana = rs("tipo_cana")
    '                sNoCartao = rs("no_cartao")
    '                sOrdemCorte = rs("no_queima")
    '                sNivel1 = rs("cd_upnivel1")
    '                sNivel2 = rs("cd_upnivel2")
    '                sNivel3 = rs("cd_upnivel3")
    '            ElseIf sNoSequencia = 2 Then
    '                sNoSequencia2 = sNoSequencia
    '                sNoLiberacao2 = rs("no_liberacao")
    '                sNoLiberacao2 = sNoLiberacao2.PadRight(14, " ")
    '                nTotalBruto += rs("qt_bruto")
    '                nTotalTara += rs("qt_tara")
    '                nTotalLiqu += rs("qt_liquido")
    '                sQtBruto2 = rs("qt_bruto")
    '                sQtBruto2 = sQtBruto2.PadRight(14, " ")
    '                sQtTara2 = rs("qt_tara")
    '                sQtTara2 = sQtTara2.PadRight(14, " ")
    '                sQtLiquido2 = rs("qt_liquido")
    '                sQtLiquido2 = sQtLiquido2.PadRight(14, " ")
    '            ElseIf sNoSequencia = 3 Then
    '                sNoSequencia3 = sNoSequencia
    '                sNoLiberacao3 = rs("no_liberacao")
    '                sNoLiberacao3 = sNoLiberacao3.PadRight(14, " ")
    '                nTotalBruto += rs("qt_bruto")
    '                nTotalTara += rs("qt_tara")
    '                nTotalLiqu += rs("qt_liquido")
    '                sQtBruto3 = rs("qt_bruto")
    '                sQtBruto3 = sQtBruto3.PadRight(14, " ")
    '                sQtTara3 = rs("qt_tara")
    '                sQtTara3 = sQtTara3.PadRight(14, " ")
    '                sQtLiquido3 = rs("qt_liquido")
    '                sQtLiquido3 = sQtLiquido3.PadRight(14, " ")
    '            End If
    '            sTotalTara = nTotalTara
    '            sTotalLiqu = nTotalLiqu
    '            sTotalBruto = nTotalBruto
    '            sTotalTara = sTotalTara.PadRight(14, " ")
    '            sTotalLiqu = sTotalLiqu.PadRight(14, " ")
    '            sTotalBruto = sTotalBruto.PadRight(14, " ")
    '            sFazenda = rs("fazenda")
    '            sFornecedor = rs("fornecedor")
    '            sEquipamento = rs("equipamento")
    '            sTransportCM = rs("transporta")
    '            sDtEntrada = rs("dt_entrada")
    '            sHrEntrada = rs("hr_entrada")
    '            sDtSaida = rs("dt_saida")
    '            sHrSaida = rs("hr_saida")
    '            sDtMovimento = rs("dt_movimento")
    '            sOrdemCarga = rs("ordem_carga")
    '            If sNoSequencia = 1 Then
    '                sNumeroRemito1 = rs("numero_remito")
    '                sNumeroRemito1 = sNumeroRemito1.PadRight(14, " ")
    '            ElseIf sNoSequencia = 2 Then
    '                sNumeroRemito2 = rs("numero_remito")
    '                sNumeroRemito2 = sNumeroRemito2.PadRight(14, " ")
    '            ElseIf sNoSequencia = 3 Then
    '                sNumeroRemito3 = rs("numero_remito")
    '                sNumeroRemito3 = sNumeroRemito3.PadRight(14, " ")
    '            End If

    '        End While
    '        fCmd(sql).Dispose()
    '        rs.Close()

    '        vGlobal.sNomeEmpresa = fDescrEmpresa(vInstLogin)

    '        vGlobal.sValor02 = sIdViagem

    '        vGlobal.gs03 = vGlobal.sValor02.PadLeft(10, "0")


    '        'MARCA D'AGUA
    '        Dim Descr1 As String = vGlobal.gs01
    '        Dim Descr2 As String = vGlobal.gs02
    '        Dim font As New Font("Arial", 40, FontStyle.Bold)
    '        Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
    '        Dim img As Image = Image.FromFile(Application.StartupPath & "\logotipo.jpg")
    '        'VIA 01
    '        e.Graphics.DrawString(Descr1, font, brush, 50, 105)
    '        e.Graphics.DrawString(Descr2, font, brush, 65, 155)
    '        e.Graphics.DrawImage(img, 30, 30)
    '        'VIA 02
    '        e.Graphics.DrawString(Descr1, font, brush, 50, 345)
    '        e.Graphics.DrawString(Descr2, font, brush, 65, 395)
    '        e.Graphics.DrawImage(img, 30, 270)
    '        'VIA 03
    '        e.Graphics.DrawString(Descr1, font, brush, 50, 585)
    '        e.Graphics.DrawString(Descr2, font, brush, 65, 635)
    '        e.Graphics.DrawImage(img, 30, 510)
    '        'VIA 04
    '        e.Graphics.DrawString(Descr1, font, brush, 50, 825)
    '        e.Graphics.DrawString(Descr2, font, brush, 65, 875)
    '        e.Graphics.DrawImage(img, 30, 750)

    '        'Linha pontilhada
    '        Dim dashValues As Single() = {2, 2, 2, 2}
    '        Dim blackPen = New Pen(Color.Black, 1)
    '        blackPen.DashPattern = dashValues
    '        'VIA01
    '        Dim PointV101 As New Point(10, 250)
    '        Dim PointV102 As New Point(810, 250)
    '        e.Graphics.DrawLine(blackPen, PointV101, PointV102)
    '        'VIA02
    '        Dim PointV201 As New Point(10, 490)
    '        Dim PointV202 As New Point(810, 490)
    '        e.Graphics.DrawLine(blackPen, PointV201, PointV202)
    '        'VIA03
    '        Dim PointV301 As New Point(10, 730)
    '        Dim PointV302 As New Point(810, 730)
    '        e.Graphics.DrawLine(blackPen, PointV301, PointV302)

    '        'Primeira Via
    '        e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 30)
    '        e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 50)

    '        e.Graphics.DrawString("Nº: " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 30)
    '        e.Graphics.DrawString("Orden Corte: " & sOrdemCorte, Fonte8R, Brushes.Black, 400, 40)
    '        e.Graphics.DrawString("Finca: " & sNivel1, Fonte8R, Brushes.Black, 400, 50)
    '        e.Graphics.DrawString("Seccion: " & sNivel2, Fonte8R, Brushes.Black, 400, 60)
    '        e.Graphics.DrawString("Tablón: " & sNivel3, Fonte8R, Brushes.Black, 400, 70)
    '        e.Graphics.DrawString("Tarjeta: " & sNoCartao.PadLeft(5, "0"), Fonte8R, Brushes.Black, 400, 80)

    '        e.Graphics.DrawString("Origen:    " & sFazenda, Fonte8R, Brushes.Black, 30, 100)
    '        e.Graphics.DrawString("Orden de Carga: " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 100)
    '        e.Graphics.DrawString("Cañero:    " & sFornecedor, Fonte8R, Brushes.Black, 30, 110)
    '        e.Graphics.DrawString("Entrada:   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 120)
    '        e.Graphics.DrawString("Salida:    " & sHrSaida, Fonte8R, Brushes.Black, 250, 120)
    '        e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 140)
    '        e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 140)
    '        If sNoSequencia1 = 1 Then
    '            e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 150)
    '            e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 150)
    '        End If
    '        If sNoSequencia2 = 2 Then
    '            e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 160)
    '            e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 160)
    '        End If
    '        If sNoSequencia3 = 3 Then
    '            e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 170)
    '            e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 170)
    '        End If
    '        Dim sDescrTotal As String
    '        sDescrTotal = "Total".PadRight(39, " ")
    '        e.Graphics.DrawString(sDescrTotal & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 180)
    '        e.Graphics.DrawString("Camión: " & sEquipamento, Fonte8R, Brushes.Black, 29, 190)
    '        e.Graphics.DrawString("Transportista: " & sTransportCM, Fonte8R, Brushes.Black, 30, 200)
    '        e.Graphics.DrawString("Frente: " & sFrenTransp, Fonte8R, Brushes.Black, 30, 210)
    '        e.Graphics.DrawString("Cosechadora: " & sColhedora, Fonte8R, Brushes.Black, 180, 210)
    '        e.Graphics.DrawString("Cuartero: " & sTrator, Fonte8R, Brushes.Black, 320, 210)
    '        e.Graphics.DrawString("Tipo Caña:" & sTipoCana, Fonte8R, Brushes.Black, 440, 210)

    '        Dim wPen01 As New Pen(Color.Black, 2)
    '        e.Graphics.DrawRectangle(wPen01, 25, 25, 550, 212)

    '        'Segunda Via
    '        e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 270)
    '        e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 290)
    '        e.Graphics.DrawString("Nº: " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 270)
    '        e.Graphics.DrawString("Orden Corte: " & sOrdemCorte, Fonte8R, Brushes.Black, 400, 280)
    '        e.Graphics.DrawString("Finca: " & sNivel1, Fonte8R, Brushes.Black, 400, 290)
    '        e.Graphics.DrawString("Seccion: " & sNivel2, Fonte8R, Brushes.Black, 400, 300)
    '        e.Graphics.DrawString("Tablón: " & sNivel3, Fonte8R, Brushes.Black, 400, 310)
    '        e.Graphics.DrawString("Tarjeta: " & sNoCartao.PadLeft(5, "0"), Fonte8R, Brushes.Black, 400, 320)

    '        e.Graphics.DrawString("Origen:    " & sFazenda, Fonte8R, Brushes.Black, 30, 340)
    '        e.Graphics.DrawString("Orden de Carga: " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 340)
    '        e.Graphics.DrawString("Cañero:    " & sFornecedor, Fonte8R, Brushes.Black, 30, 350)
    '        e.Graphics.DrawString("Entrada:   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 360)
    '        e.Graphics.DrawString("Salida:    " & sHrSaida, Fonte8R, Brushes.Black, 250, 360)
    '        e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 380)
    '        e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 380)
    '        If sNoSequencia1 = 1 Then
    '            e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 390)
    '            e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 390)
    '        End If
    '        If sNoSequencia2 = 2 Then
    '            e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 400)
    '            e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 400)
    '        End If
    '        If sNoSequencia3 = 3 Then
    '            e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 410)
    '            e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 410)
    '        End If
    '        Dim sDescrTotal2 As String
    '        sDescrTotal2 = "Total".PadRight(39, " ")
    '        e.Graphics.DrawString(sDescrTotal2 & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 420)
    '        e.Graphics.DrawString("Camión: " & sEquipamento, Fonte8R, Brushes.Black, 29, 430)
    '        e.Graphics.DrawString("Transportista: " & sTransportCM, Fonte8R, Brushes.Black, 30, 440)
    '        e.Graphics.DrawString("Frente: " & sFrenTransp, Fonte8R, Brushes.Black, 30, 450)
    '        e.Graphics.DrawString("Cosechadora: " & sColhedora, Fonte8R, Brushes.Black, 180, 450)
    '        e.Graphics.DrawString("Cuartero: " & sTrator, Fonte8R, Brushes.Black, 320, 450)
    '        e.Graphics.DrawString("Tipo Caña:" & sTipoCana, Fonte8R, Brushes.Black, 440, 450)

    '        Dim wPen02 As New Pen(Color.Black, 2)
    '        e.Graphics.DrawRectangle(wPen02, 25, 265, 550, 212)

    '        'Terceira Via
    '        e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 510)
    '        e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 530)
    '        e.Graphics.DrawString("Nº: " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 510)
    '        e.Graphics.DrawString("Orden Corte: " & sOrdemCorte, Fonte8R, Brushes.Black, 400, 520)
    '        e.Graphics.DrawString("Finca: " & sNivel1, Fonte8R, Brushes.Black, 400, 530)
    '        e.Graphics.DrawString("Seccion: " & sNivel2, Fonte8R, Brushes.Black, 400, 540)
    '        e.Graphics.DrawString("Tablón: " & sNivel3, Fonte8R, Brushes.Black, 400, 550)
    '        e.Graphics.DrawString("Tarjeta: " & sNoCartao.PadLeft(5, "0"), Fonte8R, Brushes.Black, 400, 560)

    '        e.Graphics.DrawString("Origen:    " & sFazenda, Fonte8R, Brushes.Black, 30, 580)
    '        e.Graphics.DrawString("Orden de Carga: " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 580)
    '        e.Graphics.DrawString("Cañero:    " & sFornecedor, Fonte8R, Brushes.Black, 30, 590)
    '        e.Graphics.DrawString("Entrada:   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 600)
    '        e.Graphics.DrawString("Salida:    " & sHrSaida, Fonte8R, Brushes.Black, 250, 600)
    '        e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 620)
    '        e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 620)
    '        If sNoSequencia1 = 1 Then
    '            e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 630)
    '            e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 630)
    '        End If
    '        If sNoSequencia2 = 2 Then
    '            e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 640)
    '            e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 640)
    '        End If
    '        If sNoSequencia3 = 3 Then
    '            e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 650)
    '            e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 650)
    '        End If
    '        Dim sDescrTotal3 As String
    '        sDescrTotal3 = "Total".PadRight(39, " ")
    '        e.Graphics.DrawString(sDescrTotal3 & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 660)
    '        e.Graphics.DrawString("Camión: " & sEquipamento, Fonte8R, Brushes.Black, 29, 670)
    '        e.Graphics.DrawString("Transportista: " & sTransportCM, Fonte8R, Brushes.Black, 30, 680)
    '        e.Graphics.DrawString("Frente: " & sFrenTransp, Fonte8R, Brushes.Black, 30, 690)
    '        e.Graphics.DrawString("Cosechadora: " & sColhedora, Fonte8R, Brushes.Black, 180, 690)
    '        e.Graphics.DrawString("Cuartero: " & sTrator, Fonte8R, Brushes.Black, 320, 690)
    '        e.Graphics.DrawString("Tipo Caña:" & sTipoCana, Fonte8R, Brushes.Black, 440, 690)

    '        Dim wPen03 As New Pen(Color.Black, 2)
    '        e.Graphics.DrawRectangle(wPen03, 25, 505, 550, 212)

    '        'Quarta Via
    '        e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 750)
    '        e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 770)
    '        e.Graphics.DrawString("Nº: " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 750)
    '        e.Graphics.DrawString("Orden Corte: " & sOrdemCorte, Fonte8R, Brushes.Black, 400, 760)
    '        e.Graphics.DrawString("Finca: " & sNivel1, Fonte8R, Brushes.Black, 400, 770)
    '        e.Graphics.DrawString("Seccion: " & sNivel2, Fonte8R, Brushes.Black, 400, 780)
    '        e.Graphics.DrawString("Tablón: " & sNivel3, Fonte8R, Brushes.Black, 400, 790)
    '        e.Graphics.DrawString("Tarjeta: " & sNoCartao.PadLeft(5, "0"), Fonte8R, Brushes.Black, 400, 800)

    '        e.Graphics.DrawString("Origen:    " & sFazenda, Fonte8R, Brushes.Black, 30, 820)
    '        e.Graphics.DrawString("Orden de Carga: " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 820)
    '        e.Graphics.DrawString("Cañero:    " & sFornecedor, Fonte8R, Brushes.Black, 30, 830)
    '        e.Graphics.DrawString("Entrada:   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 840)
    '        e.Graphics.DrawString("Salida:    " & sHrSaida, Fonte8R, Brushes.Black, 250, 840)
    '        e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 860)
    '        e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 860)
    '        If sNoSequencia1 = 1 Then
    '            e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, Fonte8R, Brushes.Black, 30, 870)
    '            e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, Fonte8R, Brushes.Black, 300, 870)
    '        End If
    '        If sNoSequencia2 = 2 Then
    '            e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, Fonte8R, Brushes.Black, 30, 880)
    '            e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, Fonte8R, Brushes.Black, 300, 880)
    '        End If
    '        If sNoSequencia3 = 3 Then
    '            e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, Fonte8R, Brushes.Black, 30, 890)
    '            e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, Fonte8R, Brushes.Black, 300, 890)
    '        End If
    '        Dim sDescrTotal4 As String
    '        sDescrTotal4 = "Total".PadRight(39, " ")
    '        e.Graphics.DrawString(sDescrTotal4 & sTotalBruto & sTotalTara & sTotalLiqu, Fonte8B, Brushes.Black, 29, 900)
    '        e.Graphics.DrawString("Camión: " & sEquipamento, Fonte8R, Brushes.Black, 29, 910)
    '        e.Graphics.DrawString("Transportista: " & sTransportCM, Fonte8R, Brushes.Black, 30, 920)
    '        e.Graphics.DrawString("Frente: " & sFrenTransp, Fonte8R, Brushes.Black, 30, 930)
    '        e.Graphics.DrawString("Cosechadora: " & sColhedora, Fonte8R, Brushes.Black, 180, 930)
    '        e.Graphics.DrawString("Cuartero: " & sTrator, Fonte8R, Brushes.Black, 320, 930)
    '        e.Graphics.DrawString("Tipo Caña:" & sTipoCana, Fonte8R, Brushes.Black, 440, 930)

    '        Dim wPen04 As New Pen(Color.Black, 2)
    '        e.Graphics.DrawRectangle(wPen03, 25, 745, 550, 212)

    '        bImpressoOrdemCargas = True

    '    Catch ex As Exception
    '        vGlobal.vMensangem = ex.ToString
    '        frmMensagem.Show()
    '        vGlobal.vMensangem = Nothing
    '    End Try
    'End Sub

    Private Sub Impr_Balcanes()

        'Timer.Stop()

        'Impressão da Ordem de Carga
        If vGlobal.vImprOrdemCarga = "S" Then
            fGravaLog("Verificando Orden de Carga para impresión", "LOG_TIMER_IMPR")
            If fVerificaImprBalcanes("lt_despachos", "pfg_ordem_carga", " and fg_status = 'A'") = True Then
                If fValImprOrdemCarga() = True Then
                    PrintOrdemCarga_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintOrdemCarga_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintOrdemCarga_Balcanes.Print()
                    'PrintPreviewDialog1.Document = PrintOrdemCarga_Balcanes
                    'PrintPreviewDialog1.ShowDialog()
                    AtualizaDespachoOCImpresso(vGlobal.sValor01)
                End If
            End If
        End If

        'Impressão do Extrato de Saida
        If vGlobal.vImprExtrSaida = "S" Then
            If fVerificaImprBalcanes("apt_cargas", "pfg_impr_extr_saida", " and dt_saida is not null and rownum = 1") = True Then
                If vGlobal.vIMPRLIBSEPARADA = "N" Then
                    PrintExtratoSaida_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintExtratoSaida_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintExtratoSaida_Balcanes.Print()
                    'PrintPreviewDialog1.Document = PrintExtratoSaida_Balcanes
                    'PrintPreviewDialog1.ShowDialog()
                    AtualizaExtratoSaida_Balcanes(vGlobal.sValor02, "")
                ElseIf vGlobal.vIMPRLIBSEPARADA = "S" Then
                    vGlobal.sValor03 = fIdViagemImprSeparado()
                    For i As Integer = 1 To fCountLiberViagem(vGlobal.sValor03)
                        vGlobal.sValor04 = fLiberacaoImprSeparado(vGlobal.sValor03, i)
                        PrintExtratoSaida_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                        PrintExtratoSaida_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                        PrintExtratoSaida_Balcanes.Print()
                        'PrintPreviewDialog1.Document = PrintExtratoSaida_Balcanes
                        'PrintPreviewDialog1.ShowDialog()
                        AtualizaExtratoSaida_Balcanes(vGlobal.sValor02, vGlobal.sValor04)
                    Next

                End If
            End If
        End If

        'IMpressão Boletim de Analises
        If vGlobal.vImprBolAnl = "S" Then
            If fVerificBolAnl() = True Then
                PrintBolAnl_Balcanes.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                PrintBolAnl_Balcanes.PrinterSettings.Copies = vGlobal.vNumeroCopias
                PrintBolAnl_Balcanes.Print()
                'PrintPreviewDialog1.Document = PrintBolAnl_Balcanes
                'PrintPreviewDialog1.ShowDialog()
                AtualizaBolAnl_Balcanes(vGlobal.gn03)
            End If
        End If

        'Timer.Start()

    End Sub

    Public Function fVerificaImprBalcanes(sTabela, sColuna, swhere) As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader

            sql = "select instancia from " & sTabela & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and " & sColuna & " is null" & vbNewLine &
                  "" & swhere
            sMensagem = sql
            If fValidaConexao() = True Then
                rs = fCmd(sql).ExecuteReader
                If rs.HasRows = True Then
                    fCmd(sql).Dispose()
                    rs.Close()
                    Return True
                ElseIf rs.HasRows = False Then
                    fCmd(sql).Dispose()
                    rs.Close()
                    Return False
                End If
            End If
        Catch ex As Exception
            vGlobal.vMensangem += sMensagem
            vGlobal.vMensangem += ex.ToString
            'frmMensagem.Show()
            fGravaError("frmReimpressao", "fVerificaImprBalcanes", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Public Function fVerificBolAnl() As Boolean
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select id_viagem from apt_cargas" & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and pfg_impr_bolanl is null" & vbNewLine &
                  "  and qt_bruto is not null" & vbNewLine &
                  "  and qt_tara is null" & vbNewLine &
                  "  and fg_status != 'G'"
            vGlobal.vMensangem += sql & vbNewLine
            If fValidaConexao() = True Then
                rs = fCmd(sql).ExecuteReader
                While rs.Read
                    result = rs("id_viagem")
                End While
                fCmd(sql).Dispose()
                rs.Close()
            End If
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

    Private Function fEquiptoPesagem(noLiberacao, tpRecurso)
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select cd_equipto from apt_cargas_rec" & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and cd_tp_recurso = '" & tpRecurso & "'" & vbNewLine &
                  "  and no_liberacao = " & noLiberacao & ""
            vGlobal.vMensangem += sql & vbNewLine
            If fValidaConexao() = True Then
                rs = fCmd(sql).ExecuteReader
                While rs.Read
                    result = rs("cd_equipto")
                End While
                fCmd(sql).Dispose()
                rs.Close()
            End If
            If result <> Nothing Then
                Return result
            ElseIf result = Nothing Then
                Return 0
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fEquiptoPesagem", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function fUltimaViagem(nCaminhao, sStatusCarga)
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select max(C.no_liberacao) no_liberacao from apt_cargas c, apt_cargas_rec r" & vbNewLine &
                  "where c.instancia = r.instancia" & vbNewLine &
                  "  and c.no_liberacao = r.no_liberacao" & vbNewLine &
                  "  and r.cd_tp_recurso = 'CM'" & vbNewLine &
                  "  and r.cd_equipto = " & nCaminhao & vbNewLine &
                  "  and c.fg_status in ('" & sStatusCarga.ToString.Replace(",", "','") & "')"
            vGlobal.vMensangem += sql & vbNewLine
            If fValidaConexao() = True Then
                rs = fCmd(sql).ExecuteReader
                While rs.Read
                    result = rs("no_liberacao")
                End While
                fCmd(sql).Dispose()
                rs.Close()
            End If
            If result <> Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString & vbNewLine
            fGravaError("frmReimpressao", "fUltimaViagem", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function fIdViagem(nLiberacao)
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select distinct id_viagem from apt_cargas" & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and no_liberacao = " & nLiberacao & ""
            vGlobal.vMensangem += sql & vbNewLine
            If fValidaConexao() = True Then
                rs = fCmd(sql).ExecuteReader
                While rs.Read
                    result = rs("id_viagem")
                End While
                fCmd(sql).Dispose()
                rs.Close()
            End If

            If result <> Nothing Then
                Return result
            Else
                Return 0
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fIdViagem", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function fUltimoDespacho(nCaminhao)
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select max(id_despacho) id_despacho from lt_despachos" & vbNewLine &
                  "where cd_equipto = " & nCaminhao & "" & vbNewLine &
                  "  and instancia = '" & vInstLogin & "'"
            vGlobal.vMensangem += sql & vbNewLine
            If fValidaConexao() = True Then
                rs = fCmd(sql).ExecuteReader
                While rs.Read
                    result = rs("id_despacho")
                End While
                fCmd(sql).Dispose()
                rs.Close()
            End If
            If result <> Nothing Then
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
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select ac.instancia," & vbNewLine &
                  "       ac.id_viagem" & vbNewLine &
                  "  from apt_cargas     ac," & vbNewLine &
                  "       apt_cargas_rec acr," & vbNewLine &
                  "       equiptos       eq," & vbNewLine &
                  "       fornecs        fn," & vbNewLine &
                  "       upnivel1       u1," & vbNewLine &
                  "       transporta     t," & vbNewLine &
                  "       frentransp     f" & vbNewLine &
                  " WHERE f.cd_fren_tran = ac.cd_fren_tran" & vbNewLine &
                  "   And t.cd_transp = eq.cd_transp" & vbNewLine &
                  "   And u1.cd_upnivel1 = ac.cd_upnivel1" & vbNewLine &
                  "   And fn.cd_fornec = u1.cd_fornec" & vbNewLine &
                  "   And eq.cd_equipto = acr.cd_equipto" & vbNewLine &
                  "   And acr.instancia = ac.instancia" & vbNewLine &
                  "   And acr.no_liberacao = ac.no_liberacao" & vbNewLine &
                  "   And acr.cd_tp_recurso = 'CM'" & vbNewLine &
                  "   and ac.instancia = '" & vInstLogin & "'" & vbNewLine &
                  "   and ac.dt_saida is not null" & vbNewLine &
                  "   and ac.pfg_impr_extr_saida is null" & vbNewLine &
                  " order by 1,2"
            vGlobal.vMensangem += sql & vbNewLine
            If fValidaConexao() = True Then
                rs = fCmd(sql).ExecuteReader
                While rs.Read
                    result = rs("id_viagem")
                End While
                fCmd(sql).Dispose()
                rs.Close()
            End If

            If result <> Nothing Then
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
            If fValidaConexao() = True Then
                Using cmd As OracleCommand = New OracleCommand(sql, conn)
                    cmd.Parameters.Add(New OracleParameter("instancia", vInstLogin))
                    cmd.Parameters.Add(New OracleParameter("idViagem", idViagem))
                    Using rs As OracleDataReader = cmd.ExecuteReader()
                        If rs.Read() Then
                            result = rs.GetInt32(0)
                        End If
                    End Using
                End Using
            End If
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
            Dim sql As String = "select no_liberacao from apt_cargas" & vbNewLine &
                                "where instancia = :instancia" & vbNewLine &
                                "  and id_viagem = :idViagem" & vbNewLine &
                                "  and no_sequencia = :sequencia"
            If fValidaConexao() = True Then
                Using cmd As OracleCommand = New OracleCommand(sql, conn)
                    cmd.Parameters.Add(New OracleParameter("instancia", vInstLogin))
                    cmd.Parameters.Add(New OracleParameter("idViagem", idViagem))
                    cmd.Parameters.Add(New OracleParameter("sequencia", noSequencia))
                    Using rs As OracleDataReader = cmd.ExecuteReader
                        If rs.Read() Then
                            result = rs.GetInt32(0)
                        End If
                    End Using
                End Using
            End If
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
            Dim sNoQueima, sCartoes, sMensagem As String

            Dim sql As String
            Dim rs As OracleDataReader

            sql = "select ld.id_despacho," & vbNewLine &
                  "       to_char(ld.dthr_ret_planej,'dd/mm/yyyy') data," & vbNewLine &
                  "       to_char(ld.dthr_ret_planej,'hh24:mi:ss') hora," & vbNewLine &
                  "       fn.cd_fornec||'-'||fn.de_fornec fornecedor," & vbNewLine &
                  "       ft.cd_fren_tran||'-'||ft.de_fren_tran frente," & vbNewLine &
                  "       e.cd_equipto||'-'||e.no_placa veiculo," & vbNewLine &
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
                Else
                    sCaminhao = Nothing
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
            If Not IsDBNull(fDescrEmpresa(vInstLogin)) Then
                vGlobal.sNomeEmpresa = fDescrEmpresa(vInstLogin)
            Else
                vGlobal.sNomeEmpresa = Nothing
            End If
            If (sDtEmissao = Nothing Or sHoraEmissao = Nothing Or sNoOrdemCarga = Nothing Or
                sFazenda = Nothing Or sFornecedor = Nothing Or sFrenteColheita = Nothing Or
                sCaminhao = Nothing Or sPrestador = Nothing Or sNoQueima = Nothing Or sCartoes = Nothing Or
                vGlobal.sNomeEmpresa = Nothing) Then
                sMensagem = Nothing
                sMensagem += "*** INICIO ***"
                sMensagem += "NomeEmpresa = " & vGlobal.sNomeEmpresa & vbNewLine
                sMensagem += "OrdemCarga = " & sNoOrdemCarga & vbNewLine
                sMensagem += "Fecha = " & sDtEmissao & vbNewLine
                sMensagem += "Hora = " & sHoraEmissao & vbNewLine
                sMensagem += "OrdemCorte = " & sNoQueima & vbNewLine
                sMensagem += "Tarjetas = " & sCartoes & vbNewLine
                sMensagem += "Finca = " & sFazenda & vbNewLine
                sMensagem += "Cañero = " & sFornecedor & vbNewLine
                sMensagem += "Frente = " & sFrenteColheita & vbNewLine
                sMensagem += "Camión = " & sCaminhao & vbNewLine
                sMensagem += "Fletero = " & sPrestador & vbNewLine
                sMensagem += "*** FINAL ***"

                vGlobal.vMensangem += sMensagem & vbNewLine
                Return False
            Else
                Return True
            End If
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "fValImprOrdemCarga", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Sub lbVersao_DoubleClick(sender As Object, e As EventArgs) Handles lbVersao.DoubleClick
        FParametros.Show()
    End Sub

    Private Sub PrintOrdemCarga_Budeguer_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintOrdemCarga_Budeguer.PrintPage
        Try
            bImpressoOrdemCargas = False
            vGlobal.sValor01 = Nothing
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim TipoFonte As String = "Courier New"
            Dim Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
            Dim Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
            Dim Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
            Dim Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)
            Dim Fonte13R As New Font(TipoFonte, 13, FontStyle.Regular)
            Dim Fonte14R As New Font(TipoFonte, 14, FontStyle.Regular)

            Dim Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
            Dim Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
            Dim Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
            Dim Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)
            Dim Fonte13B As New Font(TipoFonte, 13, FontStyle.Bold)
            Dim Fonte14B As New Font(TipoFonte, 14, FontStyle.Bold)

            Dim sDtEmissao, sHoraEmissao, sNoOrdemCarga, sFazenda, sFornecedor, sFrenteColheita, sCaminhao, sPrestador As String
            Dim sNoQueima, sCartoes, sUsuario As String
            Dim sWhere As String

            If bBotaoImprimir = True Then
                sWhere = "  and ld.pfg_ordem_carga = 'S' and ld.id_despacho = " & fUltimoDespacho(tbBoletim.Text) & ""
            ElseIf bBotaoImprimir = False Then
                sWhere = "  and ld.fg_status = 'A' and ld.pfg_ordem_carga is null"
            End If

            Dim sql As String
            Dim rs As OracleDataReader

            sql = "select ld.id_despacho," & vbNewLine &
                  "       to_char(ld.dthr_ret_planej,'dd/mm/yyyy') data," & vbNewLine &
                  "       to_char(ld.dthr_ret_planej,'hh24:mi:ss') hora," & vbNewLine &
                  "       fn.cd_fornec||'-'||fn.de_fornec fornecedor," & vbNewLine &
                  "       ft.cd_fren_tran||'-'||ft.de_fren_tran frente," & vbNewLine &
                  "       e.cd_equipto||'-'||e.no_placa veiculo," & vbNewLine &
                  "       tt.cd_transp||'-'||tt.de_transp prestador," & vbNewLine &
                  "       trim(u1.cd_upnivel1)||'-'||u1.de_upnivel1 fazenda," & vbNewLine &
                  "       llc.no_queima,ld.usuario" & vbNewLine &
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
                  "" & sWhere & ""
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                sNoOrdemCarga = rs("id_despacho")
                vGlobal.sValor01 = sNoOrdemCarga
                sDtEmissao = rs("data")
                sHoraEmissao = rs("hora")
                sFazenda = rs("fazenda")
                sFornecedor = rs("fornecedor")
                sFrenteColheita = rs("frente")
                sCaminhao = rs("veiculo")
                sPrestador = rs("prestador")
                sNoQueima = rs("no_queima")
                sUsuario = rs("usuario")
                sCartoes = fListaCartoes(sNoQueima)
            End While
            fCmd(sql).Dispose()
            rs.Close()

            vGlobal.sNomeEmpresa = fDescrEmpresa(vInstLogin)

            'Inicio da Impressão

            'MARCA D'AGUA
            Dim Descr1 As String = vGlobal.gs01
            Dim Descr2 As String = vGlobal.gs02
            Dim font As New Font("Arial", 40, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
            Dim img As Image = Image.FromFile(Application.StartupPath & "\logotipo.jpg")

            '*** Primeira Via ***
            'Borda Externa
            Dim wPen01 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen01, 25, 25, 650, 360)

            'Borda Interna
            Dim sb As New SolidBrush(Color.LightGray)
            e.Graphics.FillRectangle(sb, 190, 55, 310, 30)
            Dim wPen001 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen001, 190, 55, 310, 30)

            'Linha Interna
            Dim blackPen2 As New Pen(Color.Black, 1)
            Dim Point001 As New Point(25, 110)
            Dim Point002 As New Point(675, 110)
            e.Graphics.DrawLine(blackPen2, Point001, Point002)

            'Marca De Agua
            e.Graphics.DrawString(Descr1, font, brush, 80, 155)
            e.Graphics.DrawString(Descr2, font, brush, 80, 205)

            'Logotipo
            'e.Graphics.DrawImage(img, 30, 30)

            'Dados a serem impressos
            e.Graphics.DrawString("ORDEN CARGA CAÑA DE AZUCAR", Fonte13B, Brushes.Black, 195, 60)
            e.Graphics.DrawString("Nº " & sNoOrdemCarga, Fonte13B, Brushes.Black, 520, 60)
            e.Graphics.DrawString("Frente de Cosecha:    " & sFrenteColheita, Fonte10R, Brushes.Black, 30, 140)
            e.Graphics.DrawString("Transportista:   " & sPrestador, Fonte10R, Brushes.Black, 30, 160)
            e.Graphics.DrawString("Vehículo:   " & sCaminhao, Fonte10R, Brushes.Black, 30, 180)
            e.Graphics.DrawString("Productor: " & sFornecedor, Fonte10R, Brushes.Black, 30, 200)
            e.Graphics.DrawString("Finca:  " & sFazenda, Fonte10R, Brushes.Black, 30, 220)
            e.Graphics.DrawString("Lote / Seccion:    " & sFazenda, Fonte10R, Brushes.Black, 30, 240)
            e.Graphics.DrawString("Nº de Remito: " & sNoQueima, Fonte10R, Brushes.Black, 30, 260)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, Fonte10B, Brushes.Black, 30, 280)

            e.Graphics.DrawString("Fecha: " & sDtEmissao & " " & sHoraEmissao, Fonte10B, Brushes.Black, 30, 340)
            e.Graphics.DrawString("Operador: " & sUsuario, Fonte10B, Brushes.Black, 30, 360)
            e.Graphics.DrawString("1ª Cañero", Fonte10B, Brushes.Black, 500, 360)

            'Código de Barras
            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 280, 140, 70)) '300

            '1ª Linha pontilhada
            Dim dashValues As Single() = {2, 2, 2, 2}
            Dim blackPen = New Pen(Color.Black, 1)
            Dim Point01 As New Point(10, 400)
            Dim Point02 As New Point(810, 400)
            blackPen.DashPattern = dashValues
            e.Graphics.DrawLine(blackPen, Point01, Point02)

            '*** Segunda Via ***
            'Borda Externa
            Dim wPen02 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen02, 25, 415, 650, 360)

            'Borda Interna
            Dim sb2 As New SolidBrush(Color.LightGray)
            e.Graphics.FillRectangle(sb2, 190, 445, 310, 30)
            Dim wPen002 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen002, 190, 445, 310, 30)

            'Linha Interna
            Dim blackPen3 As New Pen(Color.Black, 1)
            Dim Point003 As New Point(25, 500)
            Dim Point004 As New Point(675, 500)
            e.Graphics.DrawLine(blackPen3, Point003, Point004)

            'Marca De Agua
            e.Graphics.DrawString(Descr1, font, brush, 80, 545)
            e.Graphics.DrawString(Descr2, font, brush, 80, 595)

            'Logotipo
            'e.Graphics.DrawImage(img, 30, 480)

            'Dados a serem impressos
            e.Graphics.DrawString("ORDEN CARGA CAÑA DE AZUCAR", Fonte13B, Brushes.Black, 195, 450)
            e.Graphics.DrawString("Nº " & sNoOrdemCarga, Fonte13B, Brushes.Black, 520, 450)
            e.Graphics.DrawString("Frente de Cosecha:    " & sFrenteColheita, Fonte10R, Brushes.Black, 30, 530)
            e.Graphics.DrawString("Transportista:   " & sPrestador, Fonte10R, Brushes.Black, 30, 550)
            e.Graphics.DrawString("Vehículo:   " & sCaminhao, Fonte10R, Brushes.Black, 30, 570)
            e.Graphics.DrawString("Productor: " & sFornecedor, Fonte10R, Brushes.Black, 30, 590)
            e.Graphics.DrawString("Finca:  " & sFazenda, Fonte10R, Brushes.Black, 30, 610)
            e.Graphics.DrawString("Lote / Seccion:    " & sFazenda, Fonte10R, Brushes.Black, 30, 630)
            e.Graphics.DrawString("Nº de Remito: " & sNoQueima, Fonte10R, Brushes.Black, 30, 650)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, Fonte10B, Brushes.Black, 30, 670)

            e.Graphics.DrawString("Fecha: " & sDtEmissao & " " & sHoraEmissao, Fonte10B, Brushes.Black, 30, 730)
            e.Graphics.DrawString("Operador: " & sUsuario, Fonte10B, Brushes.Black, 30, 750)
            e.Graphics.DrawString("2ª Frente", Fonte10B, Brushes.Black, 500, 750)

            'Código de Barras
            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 670, 140, 70)) '690

            '2ª Linha pontilhada
            Dim dashValues2 As Single() = {2, 2, 2, 2}
            Dim blackPen5 = New Pen(Color.Black, 1)
            Dim Point03 As New Point(10, 790)
            Dim Point04 As New Point(810, 790)
            blackPen5.DashPattern = dashValues2
            e.Graphics.DrawLine(blackPen5, Point03, Point04)

            '*** Terceira Via ***
            'Borda Externa
            Dim wPen03 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen03, 25, 805, 650, 360)

            'Borda Interna
            Dim sb3 As New SolidBrush(Color.LightGray)
            e.Graphics.FillRectangle(sb3, 190, 835, 310, 30)
            Dim wPen003 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen003, 190, 835, 310, 30)

            'Linha Interna
            Dim blackPen4 As New Pen(Color.Black, 1)
            Dim Point005 As New Point(25, 890)
            Dim Point006 As New Point(675, 890)
            e.Graphics.DrawLine(blackPen4, Point005, Point006)

            'Marca De Agua
            e.Graphics.DrawString(Descr1, font, brush, 80, 940)
            e.Graphics.DrawString(Descr2, font, brush, 80, 990)

            'Logotipo
            'e.Graphics.DrawImage(img, 30, 480)

            'Dados a serem impressos
            e.Graphics.DrawString("ORDEN CARGA CAÑA DE AZUCAR", Fonte13B, Brushes.Black, 195, 840)
            e.Graphics.DrawString("Nº " & sNoOrdemCarga, Fonte13B, Brushes.Black, 520, 840)
            e.Graphics.DrawString("Frente de Cosecha:    " & sFrenteColheita, Fonte10R, Brushes.Black, 30, 920)
            e.Graphics.DrawString("Transportista:   " & sPrestador, Fonte10R, Brushes.Black, 30, 940)
            e.Graphics.DrawString("Vehículo:   " & sCaminhao, Fonte10R, Brushes.Black, 30, 960)
            e.Graphics.DrawString("Productor: " & sFornecedor, Fonte10R, Brushes.Black, 30, 980)
            e.Graphics.DrawString("Finca:  " & sFazenda, Fonte10R, Brushes.Black, 30, 1000)
            e.Graphics.DrawString("Lote / Seccion:    " & sFazenda, Fonte10R, Brushes.Black, 30, 1020)
            e.Graphics.DrawString("Nº de Remito: " & sNoQueima, Fonte10R, Brushes.Black, 30, 1040)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, Fonte10B, Brushes.Black, 30, 1060)

            e.Graphics.DrawString("Fecha: " & sDtEmissao & " " & sHoraEmissao, Fonte10B, Brushes.Black, 30, 1120)
            e.Graphics.DrawString("Operador: " & sUsuario, Fonte10B, Brushes.Black, 30, 1140)
            e.Graphics.DrawString("3ª Transporte", Fonte10B, Brushes.Black, 500, 1140)

            'Código de Barras
            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 1060, 140, 70)) '1080

            'Final da Impressão
            bImpressoOrdemCargas = True

        Catch ex As Exception
            fGravaError("frmReimpressao", "PrintOrdemCarga_Budeguer_PrintPage", ex.ToString, "LOG_OC_ERROR")
        End Try
    End Sub

    Private Sub PrintExtratoSaida_Budeguer_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Budeguer.PrintPage
        Try
            bImpressoOrdemCargas = False
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim TipoFonte As String = "Courier New"
            Dim Fonte8R As New Font(TipoFonte, 8, FontStyle.Regular)
            Dim Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
            Dim Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
            Dim Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
            Dim Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)
            Dim Fonte13R As New Font(TipoFonte, 13, FontStyle.Regular)
            Dim Fonte14R As New Font(TipoFonte, 14, FontStyle.Regular)

            Dim Fonte8B As New Font(TipoFonte, 8, FontStyle.Bold)
            Dim Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
            Dim Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
            Dim Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
            Dim Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)
            Dim Fonte13B As New Font(TipoFonte, 13, FontStyle.Bold)
            Dim Fonte14B As New Font(TipoFonte, 14, FontStyle.Bold)

            Dim sInstancia, sIdViagem, sNoLiberacao, sFazenda As String
            Dim sQtBruto, sQtTara, sQtLiquido As String
            Dim sFornecedor, sEquipamento, sDtEntrada, sHrEntrada, sOrdemCarga, sTipoCana As String
            Dim sNumeroRemito, sDeSistColh, sCuitFazenda, sCuitFornecedor As String
            Dim sDtSaida, sHrSaida, sDtMovimento, sNoPlaca, sCuitTransporta As String
            Dim sWhere, sTransporta, sFrenTransp, sColhedora, sTrator As String

            If bBotaoImprimir = True Then
                sNoLiberacao = fUltimaViagem(tbBoletim.Text, "P, C")
                If vGlobal.vIMPRLIBSEPARADA = "S" Then
                    sWhere = "  And ac.pfg_impr_extr_saida = 'S' and ac.id_viagem = " & fIdViagem(sNoLiberacao) & " and ac.no_liberacao = " & vGlobal.sValor04 & ""
                ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                    sWhere = "  and ac.pfg_impr_extr_saida = 'S' and ac.id_viagem = " & fIdViagem(sNoLiberacao) & ""
                End If

            ElseIf bBotaoImprimir = False Then
                If vGlobal.vIMPRLIBSEPARADA = "S" Then
                    sWhere = "  and ac.pfg_impr_extr_saida is null and ac.no_liberacao = " & vGlobal.sValor04 & ""
                ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                    sWhere = "  and ac.pfg_impr_extr_saida is null"
                End If
            End If

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select ac.instancia,ac.id_viagem,ac.no_liberacao," & vbNewLine &
                  "       nvl(ac.ordem_carga, 0) ordem_carga," & vbNewLine &
                  "       nvl(ac.numero_remito, 0) numero_remito," & vbNewLine &
                  "       fn.cd_fornec || '-' || fn.de_fornec fornecedor," & vbNewLine &
                  "       trim(u1.cd_upnivel1) ||'-'||trim(u1.de_upnivel1) fazenda," & vbNewLine &
                  "       u1.no_cgc_cpf cuit_fazenda,eq.cd_equipto equipto," & vbNewLine &
                  "       eq.no_placa placa,t.cd_transp || '-' || t.de_transp transporta," & vbNewLine &
                  "       t.no_cgc_cpf cuit_transporta,s.de_sist_colh," & vbNewLine &
                  "       ac.dt_saida,ac.hr_saida,ac.dt_movimento," & vbNewLine &
                  "       nvl(ac.qt_bruto, 0) qt_bruto,nvl(ac.qt_tara, 0) qt_tara,nvl(ac.qt_liquido, 0) qt_liquido" & vbNewLine &
                  "  from apt_cargas     ac," & vbNewLine &
                  "       apt_cargas_rec acr," & vbNewLine &
                  "       equiptos       eq," & vbNewLine &
                  "       fornecs        fn," & vbNewLine &
                  "       upnivel1       u1," & vbNewLine &
                  "       transporta     t" & vbNewLine &
                  "       sistcolh       s" & vbNewLine &
                  " WHERE s.cd_sist_colh = ac.cd_sist_colh" & vbNewLine &
                  "   And t.cd_transp = eq.cd_transp" & vbNewLine &
                  "   And u1.cd_upnivel1 = ac.cd_upnivel1" & vbNewLine &
                  "   And fn.cd_fornec = u1.cd_fornec" & vbNewLine &
                  "   And eq.cd_equipto = acr.cd_equipto" & vbNewLine &
                  "   And acr.instancia = ac.instancia" & vbNewLine &
                  "   And acr.no_liberacao = ac.no_liberacao" & vbNewLine &
                  "   And acr.cd_tp_recurso = 'CM'" & vbNewLine &
                  "   and ac.instancia = '001'" & vbNewLine &
                  "   and ac.dt_saida is not null" & vbNewLine &
                  "   and ac.pfg_impr_extr_saida = 'S'" & vbNewLine &
                  "" & sWhere & "" & vbNewLine &
                  " order by 2, 3"

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                sInstancia = rs("instancia")
                sIdViagem = rs("id_viagem")
                sNoLiberacao = rs("no_liberacao")
                sOrdemCarga = rs("ordem_carga")
                sNumeroRemito = rs("numero_remito")
                sFornecedor = rs("fornecedor")
                sFazenda = rs("fazenda")
                sCuitFazenda = rs("cuit_fazenda")
                sEquipamento = rs("equipto")
                sNoPlaca = rs("placa")
                sTransporta = rs("transporta")
                sCuitTransporta = rs("cuit_transporta")
                sDeSistColh = rs("de_sist_colh")
                sDtSaida = rs("dt_saida")
                sHrSaida = rs("hr_saida")
                sDtMovimento = rs("dt_movimento")
                sQtBruto = rs("qt_bruto")
                sQtTara = rs("qt_tara")
                sQtLiquido = rs("qt_liquido")
            End While
            fCmd(sql).Dispose()
            rs.Close()

            vGlobal.sNomeEmpresa = fDescrEmpresa(vInstLogin)

            vGlobal.sValor02 = sIdViagem

            'MARCA D'AGUA
            Dim Descr1 As String = vGlobal.gs01
            Dim Descr2 As String = vGlobal.gs02
            Dim font As New Font("Arial", 40, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
            Dim img As Image = Image.FromFile(Application.StartupPath & "\logotipo.jpg")
            'VIA 01
            e.Graphics.DrawString(Descr1, font, brush, 50, 105)
            e.Graphics.DrawString(Descr2, font, brush, 65, 155)
            e.Graphics.DrawImage(img, 30, 30)
            'VIA 02
            e.Graphics.DrawString(Descr1, font, brush, 50, 360)
            e.Graphics.DrawString(Descr2, font, brush, 65, 410)
            e.Graphics.DrawImage(img, 30, 285)
            'VIA 03
            e.Graphics.DrawString(Descr1, font, brush, 50, 615)
            e.Graphics.DrawString(Descr2, font, brush, 65, 665)
            e.Graphics.DrawImage(img, 30, 540)
            'VIA 04
            e.Graphics.DrawString(Descr1, font, brush, 50, 875)
            e.Graphics.DrawString(Descr2, font, brush, 65, 925)
            e.Graphics.DrawImage(img, 30, 800)

            'Linha pontilhada
            Dim dashValues As Single() = {2, 2, 2, 2}
            Dim blackPen = New Pen(Color.Black, 1)
            blackPen.DashPattern = dashValues
            'VIA01
            Dim PointV101 As New Point(10, 270)
            Dim PointV102 As New Point(810, 270)
            e.Graphics.DrawLine(blackPen, PointV101, PointV102)
            'VIA02
            Dim PointV201 As New Point(10, 525)
            Dim PointV202 As New Point(810, 525)
            e.Graphics.DrawLine(blackPen, PointV201, PointV202)
            'VIA03
            Dim PointV301 As New Point(10, 780)
            Dim PointV302 As New Point(810, 780)
            e.Graphics.DrawLine(blackPen, PointV301, PointV302)

            'Primeira Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", Fonte11R, Brushes.Black, 100, 30)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, Fonte11B, Brushes.Black, 100, 50)

            e.Graphics.DrawString("Nº: " & vInstLogin & "-" & vGlobal.gs03, Fonte8R, Brushes.Black, 400, 80)

            e.Graphics.DrawString("Origen:    " & sFazenda, Fonte8R, Brushes.Black, 30, 100)
            e.Graphics.DrawString("Orden de Carga: " & sOrdemCarga, Fonte8R, Brushes.Black, 400, 100)
            e.Graphics.DrawString("Cañero:    " & sFornecedor, Fonte8R, Brushes.Black, 30, 110)
            e.Graphics.DrawString("Entrada:   " & sHrEntrada, Fonte8R, Brushes.Black, 30, 120)
            e.Graphics.DrawString("Salida:    " & sHrSaida, Fonte8R, Brushes.Black, 250, 120)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", Fonte8B, Brushes.Black, 300, 140)
            e.Graphics.DrawString("Liberación    Remito        ", Fonte8B, Brushes.Black, 30, 140)

            e.Graphics.DrawString(sNoLiberacao & "" & sNumeroRemito, Fonte8R, Brushes.Black, 30, 150)
            e.Graphics.DrawString(sQtBruto & "" & sQtTara & "" & sQtLiquido, Fonte8R, Brushes.Black, 300, 150)

            Dim sDescrTotal As String
            sDescrTotal = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal & sQtBruto & sQtTara & sQtLiquido, Fonte8B, Brushes.Black, 29, 180)
            e.Graphics.DrawString("Camión: " & sEquipamento, Fonte8R, Brushes.Black, 29, 190)
            e.Graphics.DrawString("Transportista: " & sTransporta, Fonte8R, Brushes.Black, 30, 200)
            e.Graphics.DrawString("Frente: " & sFrenTransp, Fonte8R, Brushes.Black, 30, 210)
            e.Graphics.DrawString("Cosechadora: " & sColhedora, Fonte8R, Brushes.Black, 180, 210)
            e.Graphics.DrawString("Cuartero: " & sTrator, Fonte8R, Brushes.Black, 320, 210)
            e.Graphics.DrawString("Tipo Caña:" & sTipoCana, Fonte8R, Brushes.Black, 440, 210)

            Dim wPen01 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen01, 25, 25, 570, 235)

            bImpressoOrdemCargas = True

        Catch ex As Exception

        End Try
    End Sub

    Private Sub rbBolAnl_CheckedChanged(sender As Object, e As EventArgs) Handles rbBolAnl.CheckedChanged
        If vGlobal.vModeloImpressao = "02" Then
            If rbBolAnl.Checked = True Then
                lbTexto.Text = "Análisis:"
            End If
        End If
    End Sub

    Private Sub rbExtratoSaida_CheckedChanged(sender As Object, e As EventArgs) Handles rbExtratoSaida.CheckedChanged
        If vGlobal.vModeloImpressao = "02" Then
            If rbExtratoSaida.Checked = True Then
                lbTexto.Text = "Liberación:"
            End If
        End If
    End Sub

    Private Sub RbExtProducto_CheckedChanged(sender As Object, e As EventArgs) Handles RbExtProducto.CheckedChanged
        If vGlobal.vModeloImpressao = "02" Then
            If RbExtProducto.Checked = True Then
                lbTexto.Text = "Pesaje:"
            End If
        End If
    End Sub
End Class
