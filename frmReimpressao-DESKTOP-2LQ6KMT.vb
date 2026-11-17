Imports System.Data.OracleClient
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar

Public Class frmReimpressao
    Dim nContador As Integer = 0
    Dim nContadorTimer As Integer
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

            Dim cdCaminhao, cdMotorista, hrEntrada, cdCarretas, cdFrenTran, noLiberacao As String

            cdCarretas = " "

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select c.no_liberacao,r.cd_tp_recurso,r.cd_func," & vbNewLine &
                  "r.cd_equipto, c.hr_entrada,f.cd_fren_tran || f.de_fren_tran frente" & vbNewLine &
                  "from apt_cargas c, apt_cargas_rec r, frentransp f" & vbNewLine &
                  "where r.no_liberacao = c.no_liberacao" & vbNewLine &
                  "f.cd_fren_tran = c.cd_fren_tran" & vbNewLine &
                  "  And r.cd_tp_recurso in ('CM','CT')" & vbNewLine &
                  "  and c.fg_impresso_impureza is null"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                noLiberacao = rs("no_liberacao")
                hrEntrada = rs("hr_entrada")
                cdFrenTran = rs("frente")
                If rs("cd_tp_recurso") = "CM" Then
                    cdCaminhao = rs("cd_equipto")
                    cdMotorista = rs("cd_func")
                ElseIf rs("cd_tp_recurso") = "CT" Then
                    cdCarretas = rs("cd_equipto")
                End If
            End While
            fCmd(sql).Dispose()
            rs.Close()

            AtualizaTablacargas(noLiberacao)
            'vGlobal.sNomeEmpresa = "Ingenio Laredo"
            'cdCaminhao = "14345678"
            'cdMotorista = "87654321"
            'cdCarretas = "123654"
            'hrEntrada = "16/09/2022 12:10:21"
            'cdFrenTran = "1-Frente 1"
            'noLiberacao = "45"

            e.Graphics.DrawString("**            " & vGlobal.sNomeEmpresa & "            **", Fonte11R, Brushes.Black, 10, 10)
            e.Graphics.DrawString("      Boletín de Análisis de Trash", Fonte11R, Brushes.Black, 10, 50)
            e.Graphics.DrawString("Camión: " & cdCaminhao & "   Chofer: " & cdMotorista & "", Fonte10R, Brushes.Black, 10, 90)
            e.Graphics.DrawString("Remolques: " & cdCarretas & "", Fonte10R, Brushes.Black, 10, 110)
            e.Graphics.DrawString("Entrada: " & hrEntrada & "", Fonte10R, Brushes.Black, 10, 130)
            e.Graphics.DrawString("Frente de Cosecha: " & cdFrenTran & "", Fonte10R, Brushes.Black, 10, 150)

            barra = codbarras.Draw(noLiberacao, altura)
            e.Graphics.DrawImage(barra, New Rectangle(35, 200, 140, 70))
            e.Graphics.DrawString(noLiberacao, Fonte9R, Brushes.Black, 85, 280)

            cdCaminhao = Nothing
            hrEntrada = Nothing
            cdCarretas = Nothing
            cdFrenTran = Nothing
            noLiberacao = Nothing
        Catch ex As Exception
            vGlobal.vMensangem = ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Sub btImprimir_Click(sender As Object, e As EventArgs) Handles btImprimir.Click
        If vGlobal.vModeloImpressao = "01" Then
            PrintImpurezaMineralMD01.Print()
        ElseIf vGlobal.vModeloImpressao = "02" Then
            PrintPesagProdutos.Print()
        End If

    End Sub

    Private Sub AtualizaTablacargas(noLiberacao)
        Dim sql As String
        Dim rs As OracleDataReader
        sql = "update apt_cargas set fg_impresso_impureza = 'S' where no_liberacao = " & noLiberacao & ""
        rs = fCmd(sql).ExecuteReader
        fCmd(sql).Dispose()
        rs.Close()
    End Sub

    Private Sub PrintPesagProdutos_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintPesagProdutos.PrintPage
        Try
            Dim noPesagem, noOrdemCarga, cdTransacao, cdTipoPesagem, cdProdutoCant, cdOrigem, cdDestino, cdTransportadora, cdProvedor, cdCaminhao As String
            Dim cdCondutor, cdReboque1, cdReboque2, cdReboque3, qtPesoLiquido, qtPesoEmbalagem, qtPesoLiquidoSemEmbalagem As String
            Dim dtHoraEntrada, dtHoraSaida, cdUsuario, deObservacao, pesagem, fgPesagEs, strDescricao, strTextoProduto As String
            Dim qtPeso1, qtPeso2, qtPesoTara, qtPesoBruto As Integer
            Dim alturaLinha As Integer

            Dim nStringProduto As Integer

            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select o.instancia,lpad(o.no_pesagem,10,0) no_pesagem, o.no_pesagem pesagem," & vbNewLine &
                  "       o.hr_entrada,o.hr_saida,o.fg_pesag_es," & vbNewLine &
                  "       tm.cd_tp_mov || '-' || tm.de_tp_mov tipo_transacao," & vbNewLine &
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
                  "       op.cd_origem || ' - ' || dp.de_deposito deposito," & vbNewLine &
                  "       op.cd_destino || ' - ' || dt.de_destinat destino," & vbNewLine &
                  "       o.cd_fornec || '-' || cf.de_fornec fornec," & vbNewLine &
                  "       o.cd_transport || '-' || tp.de_transport transportadora," & vbNewLine &
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
                  " inner join deposito dp" & vbNewLine &
                  "    on dp.cd_deposito = op.cd_origem" & vbNewLine &
                  " inner join destinatario dt" & vbNewLine &
                  "    on dt.cd_destinat = op.cd_destino" & vbNewLine &
                  "   and dt.instancia = op.instancia" & vbNewLine &
                  " inner join tipo_movimento tm" & vbNewLine &
                  "    on tm.cd_tp_mov = o.cd_tp_mov" & vbNewLine &
                  " inner join cad_fornec cf" & vbNewLine &
                  "    on cf.cd_fornec = o.cd_fornec" & vbNewLine &
                  "   and cf.instancia = o.instancia" & vbNewLine &
                  " inner join transportadora tp" & vbNewLine &
                  "    on tp.instancia = o.instancia" & vbNewLine &
                  "   and tp.cd_transport = o.cd_transport" & vbNewLine &
                  "   and o.instancia = '" & vInstLogin & "'" & vbNewLine &
                  "   and o.zfg_impr_extrato is null" & vbNewLine &
                  "   and o.fg_status = 'C'" & vbNewLine &
                  "   and rownum = 1"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                noPesagem = rs("no_pesagem")
                noOrdemCarga = rs("no_ordem_car")
                cdTransacao = rs("tipo_transacao")
                cdTipoPesagem = rs("fg_tp_pesagem")
                cdProdutoCant = rs("produto")
                cdOrigem = rs("deposito")
                cdDestino = rs("destino")
                cdTransportadora = rs("transportadora")
                cdProvedor = rs("fornec")
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
            End While

            nStringProduto = Len(cdProdutoCant)
            strTextoProduto = cdProdutoCant

            vGlobal.sNomeEmpresa = fDescricao("empresas", "de_empresa", "cd_empresa", fGetParam("GERAIS", "CD_EMPRESA"))

            alturaLinha = 10
            e.Graphics.DrawString("******        " & vGlobal.sNomeEmpresa & "        ******", Fonte12R, Brushes.Black, 50, alturaLinha)
            alturaLinha += 40
            e.Graphics.DrawString("    Extracto de Pesaje de Productos    ", Fonte11R, Brushes.Black, 10, alturaLinha)

            alturaLinha += 30
            e.Graphics.DrawString("Numero Pesaje:  " & noPesagem & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Numero Orden Carga:  " & noOrdemCarga & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Tipo Transaccion:  " & cdTransacao & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Tipo de Pesaje:  " & cdTipoPesagem & "", Fonte9R, Brushes.Black, 10, alturaLinha)

            alturaLinha += 30
            e.Graphics.DrawString("Producto Cant", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20

            If nStringProduto <= 10 Then
                e.Graphics.DrawString(strTextoProduto, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
            ElseIf nStringProduto >= 11 And nStringProduto <= 20 Then
                strTextoProduto = strTextoProduto.Substring(10)
                e.Graphics.DrawString(strTextoProduto, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString(strTextoProduto, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
            ElseIf nStringProduto >= 21 Then
                e.Graphics.DrawString(strTextoProduto, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString(strTextoProduto, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
                e.Graphics.DrawString(strTextoProduto, Fonte9R, Brushes.Black, 10, alturaLinha)
                alturaLinha += 20
            End If

            e.Graphics.DrawString("Origen:  " & cdOrigem & "", Fonte9R, Brushes.Black, 10, alturaLinha)
            alturaLinha += 20
            e.Graphics.DrawString("Destino:  " & cdDestino & "", Fonte9R, Brushes.Black, 10, alturaLinha)

            alturaLinha += 30
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
            e.Graphics.DrawString("*** " & strDescricao & " ***", Fonte11R, Brushes.Black, 10, alturaLinha)
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

            AtualizaOutrPesagens(pesagem)

        Catch ex As Exception
            vGlobal.vMensangem = ex.ToString
            frmMensagem.Show()
            vGlobal.vMensangem = Nothing
        End Try
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick
        If nContadorTimer = vTimerImpressao Then
            nContadorTimer = 0
            If vGlobal.vModeloImpressao = "01" Then
                If fVerificaImpressao() = True Then
                    PrintPesagProdutos.Print()
                End If
            ElseIf vGlobal.vModeloImpressao = "02" Then
                PrintImpurezaMineralMD01.Print()
            End If

        ElseIf nContadorTimer < vTimerImpressao Then
            nContadorTimer += 1
        End If
    End Sub

    Private Sub frmReimpressao_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AtualizaVariaveis()
        Conexao.conn = Conexao.fConexao()

        Timer.Start()

        If vGlobal.vModeloImpressao = "01" Then
            lbTexto.Text = "Extrato:"
        ElseIf vGlobal.vModeloImpressao = "02" Then
            lbTexto.Text = "Boletim:"
        End If

    End Sub

    Private Sub AtualizaOutrPesagens(noPesagem)
        Try
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "update outr_pesagens" & vbNewLine &
                  "set zfg_impr_extrato = 'S'" & vbNewLine &
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
                  "where o.zfg_impr_extrato = 'S'" & vbNewLine &
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
End Class
