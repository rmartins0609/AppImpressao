Imports System.Drawing.Printing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports AppImpressao.Impressoes.Laredo
Imports Oracle.ManagedDataAccess.Client
Imports System.Threading

Module Impressoes
    Dim sWhere As String
    <DllImport("winspool.Drv", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function WritePrinter(ByVal hPrinter As IntPtr, ByVal pBytes As IntPtr, ByVal dwCount As Integer, ByRef dwWritten As Integer) As Boolean
    End Function

    <DllImport("winspool.Drv", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function ClosePrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function

    <DllImport("winspool.Drv", CharSet:=CharSet.Auto, SetLastError:=True)>
    Public Function OpenPrinter(ByVal pPrinterName As String, ByRef phPrinter As IntPtr, ByVal pDefault As IntPtr) As Boolean
    End Function
    'Private Sub ClosePrinter(hPrinter As IntPtr)
    '    Throw New NotImplementedException()
    'End Sub

    'Private Sub WritePrinter(hPrinter As IntPtr, pBytes As IntPtr, length As Integer, dwWritten As Integer)
    '    Throw New NotImplementedException()
    'End Sub

    'Private Function OpenPrinter(printerName As String, hPrinter As IntPtr, zero As IntPtr) As Boolean
    '    Throw New NotImplementedException()
    'End Function

    Public PrintTela As New PrintPreviewDialog
    Private alturaLinha As Integer
    Private nLoopLinhas As Integer
    Public bImprimirBolAnl As Boolean

    Private CopiasRestantesEntrada As Integer
    Private CopiasRestantesBolAnl As Integer

    Private ReadOnly TravaBoletimPatio As New Object
    Private ReadOnly TravaSaida As New Object
    Private ReadOnly TravaProduto As New Object
    Private ReadOnly TravaBolAnalise As New Object

    Public Class BematechDLL
        ' Abre a porta de comunicação
        <DllImport("mp2032.dll")>
        Public Shared Function ConfiguraModeloImpressora(modelo As Integer) As Integer
        End Function

        <DllImport("mp2032.dll")>
        Public Shared Function IniciaPorta(porta As String) As Integer
        End Function

        <DllImport("mp2032.dll")>
        Public Shared Function BematechTX(comando As String) As Integer
        End Function

        <DllImport("mp2032.dll")>
        Public Shared Function FechaPorta() As Integer
        End Function

        <DllImport("mp2032.dll")>
        Public Shared Function AcionaGuilhotina(modo As Integer) As Integer
        End Function

        <DllImport("mp2032.dll")>
        Public Shared Function ImprimeCodigoBarrasCODABAR(codigo As String) As Integer
        End Function

        ' Para o padrão mais comum (EAN13 - Produtos)
        <DllImport("mp2032.dll")>
        Public Shared Function ImprimeCodigoBarrasEAN13(codigo As String) As Integer
        End Function

        ' Para o padrão CODE128 (Logística/Geral)
        <DllImport("mp2032.dll")>
        Public Shared Function ImprimeCodigoBarrasCODE128(codigo As String) As Integer
        End Function

        ' Para configurar altura e largura das barras
        <DllImport("mp2032.dll")>
        Public Shared Function ConfiguraCodigoBarras(altura As Integer, largura As Integer, posicao As Object, fonte As Integer, margem As Integer) As Integer
        End Function
    End Class

    'Public Class RawPrinterHelper
    '    <DllImport("winspool.Drv", EntryPoint:="OpenPrinterA", SetLastError:=True, CharSet:=CharSet.Ansi, ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    '    Public Shared Function OpenPrinter(szPrinter As String, ByRef hPrinter As IntPtr, pDefault As IntPtr) As Boolean
    '    End Function

    '    <DllImport("winspool.Drv", EntryPoint:="ClosePrinter", SetLastError:=True, ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    '    Public Shared Function ClosePrinter(hPrinter As IntPtr) As Boolean
    '    End Function

    '    <DllImport("winspool.Drv", EntryPoint:="StartDocPrinterA", SetLastError:=True, CharSet:=CharSet.Ansi, ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    '    Public Shared Function StartDocPrinter(hPrinter As IntPtr, level As Integer, ByRef di As DOCINFOA) As Boolean
    '    End Function

    '    <DllImport("winspool.Drv", EntryPoint:="EndDocPrinter", SetLastError:=True, ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    '    Public Shared Function EndDocPrinter(hPrinter As IntPtr) As Boolean
    '    End Function

    '    <DllImport("winspool.Drv", EntryPoint:="StartPagePrinter", SetLastError:=True, ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    '    Public Shared Function StartPagePrinter(hPrinter As IntPtr) As Boolean
    '    End Function

    '    <DllImport("winspool.Drv", EntryPoint:="EndPagePrinter", SetLastError:=True, ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    '    Public Shared Function EndPagePrinter(hPrinter As IntPtr) As Boolean
    '    End Function

    '    <DllImport("winspool.Drv", EntryPoint:="WritePrinter", SetLastError:=True, ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    '    Public Shared Function WritePrinter(hPrinter As IntPtr, pBytes As Byte(), dwCount As Integer, ByRef dwWritten As Integer) As Boolean
    '    End Function

    '    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi)>
    '    Public Structure DOCINFOA
    '        <MarshalAs(UnmanagedType.LPStr)> Public pDocName As String
    '        <MarshalAs(UnmanagedType.LPStr)> Public pOutputFile As String
    '        <MarshalAs(UnmanagedType.LPStr)> Public pDataType As String
    '    End Structure

    'Public Shared Sub SendBytesToPrinter(printerName As String, bytes As Byte(), count As Integer)
    '    Dim hPrinter As IntPtr = IntPtr.Zero
    '    Dim di As New DOCINFOA()
    '    di.pDocName = "Raw ESC/POS Document"
    '    di.pDataType = "RAW"

    '    If OpenPrinter(printerName, hPrinter, IntPtr.Zero) Then
    '        If StartDocPrinter(hPrinter, 1, di) Then
    '            If StartPagePrinter(hPrinter) Then
    '                Dim dwWritten As Integer = 0
    '                WritePrinter(hPrinter, bytes, count, dwWritten)
    '                EndPagePrinter(hPrinter)
    '            End If
    '            EndDocPrinter(hPrinter)
    '        End If
    '        ClosePrinter(hPrinter)
    '    End If
    'End Sub
    'End Class

    '[BUDEGUER]
    Public WithEvents PrintTicketRast_Budeguer As New PrintDocument()
    Private Sub PrintTicketRastreio_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintTicketRast_Budeguer.PrintPage
        Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
        Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr


        Dim barra As Image

        Dim barras As Image

        Dim Liberacao, Ticket, Caminhao, Caminhao2, Transporta, Hora_Guarita, Frente, FgAnlImp As String
        Dim dados = Budeguer.ImprTicketRastreio.Dados()
        For Each item In dados
            Liberacao = item.liberacao
            Ticket = item.ticket
            Caminhao = item.caminhao
            Caminhao2 = item.caminhao2
            Transporta = item.transporta
            Hora_Guarita = item.hr_guarita
            Frente = item.frente
            FgAnlImp = item.fg_anl_imp
        Next

        Transporta = If(Transporta.Length > 29, Transporta.Substring(1, 29), Transporta)

        Dim altura As Integer = 1000
        barras = codbarras.Draw(Ticket, altura)

        Dim sf As New StringFormat
        sf.Alignment = StringAlignment.Center
        Dim wPen As New Pen(Color.Black, 1)

        'Dim dashValues As Single() = {2, 2, 2, 2}
        'Dim blackPen = New Pen(Color.Black, 1)

        'Linha pontilhada horizontal
        'Dim Point01V1 As New Point(10, 370)
        'Dim Point02V1 As New Point(810, 370)
        'blackPen.DashPattern = dashValues
        'e.Graphics.DrawLine(blackPen, Point01V1, Point02V1)

        Budeguer.ImprTicketRastreio.LinhaPontilhada(e.Graphics, 10, 370, 810, 370)

        'Dim Point01V2 As New Point(10, 750)
        'Dim Point02V2 As New Point(810, 750)
        'blackPen.DashPattern = dashValues
        'e.Graphics.DrawLine(blackPen, Point01V2, Point02V2)
        Budeguer.ImprTicketRastreio.LinhaPontilhada(e.Graphics, 10, 750, 810, 750)

        'Linha pontilhada vertical
        'Dim Point01H As New Point(380, 30)
        'Dim Point02H As New Point(380, 750)
        'blackPen.DashPattern = dashValues
        'e.Graphics.DrawLine(blackPen, Point01H, Point02H)
        Budeguer.ImprTicketRastreio.LinhaPontilhada(e.Graphics, 380, 30, 380, 750)

        'Primeira Via
        e.Graphics.DrawRectangle(wPen, 10, 30, 340, 320)
        Dim rect1 As New System.Drawing.RectangleF(10, 40, 340, 300)
        e.Graphics.DrawString("TICKET RASTREO", FontesImpressao.Fonte12B, Brushes.Black, rect1, sf)
        e.Graphics.DrawString("Camión: " & Caminhao, FontesImpressao.Fonte10R, Brushes.Black, 20, 70)
        e.Graphics.DrawString("Fletero: " & Transporta, FontesImpressao.Fonte10R, Brushes.Black, 20, 90)
        e.Graphics.DrawString("Frente: " & Frente, FontesImpressao.Fonte10R, Brushes.Black, 20, 110)
        e.Graphics.DrawString("Recepción: " & Hora_Guarita, FontesImpressao.Fonte10R, Brushes.Black, 20, 130)
        e.Graphics.DrawString("Ticket: " & Ticket, FontesImpressao.Fonte10R, Brushes.Black, 20, 150)
        e.Graphics.DrawString("Liberación: " & Liberacao, FontesImpressao.Fonte10R, Brushes.Black, 20, 170)

        e.Graphics.DrawImage(barras, New Rectangle(100, 220, 140, 70))
        e.Graphics.DrawString(Ticket, FontesImpressao.Fonte10R, Brushes.Black, 100, 290)
        e.Graphics.DrawString("Balanza", FontesImpressao.Fonte10B, Brushes.Black, 280, 330)
        e.Graphics.DrawString("CM " & Caminhao2, FontesImpressao.Fonte18B, Brushes.Black, 100, 190)

        'Segunda Via
        e.Graphics.DrawRectangle(wPen, 410, 30, 340, 320)
        Dim rect2 As New System.Drawing.RectangleF(410, 40, 340, 300)
        e.Graphics.DrawString("TICKET RASTREO", FontesImpressao.Fonte12B, Brushes.Black, rect2, sf)
        e.Graphics.DrawString("Camión: " & Caminhao, FontesImpressao.Fonte10R, Brushes.Black, 420, 70)
        e.Graphics.DrawString("Fletero: " & Transporta, FontesImpressao.Fonte10R, Brushes.Black, 420, 90)
        e.Graphics.DrawString("Frente: " & Frente, FontesImpressao.Fonte10R, Brushes.Black, 420, 110)
        e.Graphics.DrawString("Recepción: " & Hora_Guarita, FontesImpressao.Fonte10R, Brushes.Black, 420, 130)
        e.Graphics.DrawString("Ticket: " & Ticket, FontesImpressao.Fonte10R, Brushes.Black, 420, 150)
        e.Graphics.DrawString("Liberación: " & Liberacao, FontesImpressao.Fonte10R, Brushes.Black, 420, 170)

        e.Graphics.DrawImage(barras, New Rectangle(500, 220, 140, 70))
        e.Graphics.DrawString(Ticket, FontesImpressao.Fonte10R, Brushes.Black, 500, 290)
        e.Graphics.DrawString("Lab Individual", FontesImpressao.Fonte10B, Brushes.Black, 620, 330)
        e.Graphics.DrawString("CM " & Caminhao2, FontesImpressao.Fonte18B, Brushes.Black, 500, 190)

        'Terceira Via
        e.Graphics.DrawRectangle(wPen, 10, 400, 340, 320)
        Dim rect3 As New System.Drawing.RectangleF(10, 410, 340, 300)
        e.Graphics.DrawString("TICKET RASTREO", FontesImpressao.Fonte12B, Brushes.Black, rect3, sf)
        e.Graphics.DrawString("Camión: " & Caminhao, FontesImpressao.Fonte10R, Brushes.Black, 20, 440)
        e.Graphics.DrawString("Fletero: " & Transporta, FontesImpressao.Fonte10R, Brushes.Black, 20, 460)
        e.Graphics.DrawString("Frente: " & Frente, FontesImpressao.Fonte10R, Brushes.Black, 20, 480)
        e.Graphics.DrawString("Recepción: " & Hora_Guarita, FontesImpressao.Fonte10R, Brushes.Black, 20, 500)
        e.Graphics.DrawString("Ticket: " & Ticket, FontesImpressao.Fonte10R, Brushes.Black, 20, 520)
        e.Graphics.DrawString("Liberación: " & Liberacao, FontesImpressao.Fonte10R, Brushes.Black, 20, 540)

        e.Graphics.DrawImage(barras, New Rectangle(100, 590, 140, 70))
        e.Graphics.DrawString(Ticket, FontesImpressao.Fonte10R, Brushes.Black, 100, 660)
        e.Graphics.DrawString("Lab Trash", FontesImpressao.Fonte10B, Brushes.Black, 260, 700)
        If FgAnlImp = "S" Then
            e.Graphics.DrawString("TRASH: SÍ", FontesImpressao.Fonte16B, Brushes.Black, 20, 690)
        ElseIf FgAnlImp = "N" Then
            e.Graphics.DrawString("TRASH: NO", FontesImpressao.Fonte16B, Brushes.Black, 20, 690)
        End If
        e.Graphics.DrawString("CM " & Caminhao2, FontesImpressao.Fonte18B, Brushes.Black, 100, 560)

        'Quarta Via
        e.Graphics.DrawRectangle(wPen, 410, 400, 340, 320)
        Dim rect4 As New System.Drawing.RectangleF(410, 410, 340, 300)
        e.Graphics.DrawString("TICKET RASTREO", FontesImpressao.Fonte12B, Brushes.Black, rect4, sf)
        e.Graphics.DrawString("Camión: " & Caminhao, FontesImpressao.Fonte10R, Brushes.Black, 420, 440)
        e.Graphics.DrawString("Fletero: " & Transporta, FontesImpressao.Fonte10R, Brushes.Black, 420, 460)
        e.Graphics.DrawString("Frente: " & Frente, FontesImpressao.Fonte10R, Brushes.Black, 420, 480)
        e.Graphics.DrawString("Recepción: " & Hora_Guarita, FontesImpressao.Fonte10R, Brushes.Black, 420, 500)
        e.Graphics.DrawString("Ticket: " & Ticket, FontesImpressao.Fonte10R, Brushes.Black, 420, 520)
        e.Graphics.DrawString("Liberación: " & Liberacao, FontesImpressao.Fonte10R, Brushes.Black, 420, 540)

        e.Graphics.DrawImage(barras, New Rectangle(500, 590, 140, 70))
        e.Graphics.DrawString(Ticket, FontesImpressao.Fonte10R, Brushes.Black, 500, 660)
        e.Graphics.DrawString("Marcador", FontesImpressao.Fonte10B, Brushes.Black, 670, 700)
        e.Graphics.DrawString("CM " & Caminhao2, FontesImpressao.Fonte18B, Brushes.Black, 500, 560)

    End Sub

    Public WithEvents PrintExtratoSaida_Budeguer As New PrintDocument()
    Private Sub PrintExtratoSaida_Budeguer_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Budeguer.PrintPage
        Try
            frmReimpressao.bImpressoOrdemCargas = False
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim sInstancia, sIdViagem, sNoLiberacao, sFazenda As String
            Dim sQtBruto, sQtTara, sQtLiquido As String
            Dim sFornecedor, sEquipamento, sOrdemCarga As String
            Dim sNumeroRemito, sDeSistColh, sCuitFazenda As String
            Dim sDtSaida, sHrSaida, sDtMovimento, sNoPlaca, sCuitTransporta As String
            Dim sTransporta As String

            Dim item = Budeguer.ImprExtratoSaida.Dados().FirstOrDefault
            sInstancia = item.Instancia
            sIdViagem = item.IdViagem
            sNoLiberacao = item.NoLiberacao
            sOrdemCarga = item.OrdemCarga
            sNumeroRemito = item.NumeroRemito
            sFornecedor = item.Fornecedor
            sFazenda = item.Fazenda
            sCuitFazenda = item.CuitFazenda
            sEquipamento = item.Equipamento
            sNoPlaca = item.NoPlaca
            sTransporta = item.Transporta
            sCuitTransporta = item.CuitTransporta
            sDeSistColh = item.DeSistColh
            sDtSaida = item.DtSaida
            sHrSaida = item.HrSaida
            sDtMovimento = item.DtMovimento
            sQtBruto = item.QtBruto
            sQtTara = item.QtTara
            sQtLiquido = item.QtLiquido

            vGlobal.sNomeEmpresa = cadempresas(cadparametros(("GERAIS", "CD_EMPRESA")))
            vGlobal.sValor02 = sIdViagem

            ' DEFINIÇÃO DE FONTES MAIORES (Criadas localmente para garantir a leitura)
            Dim FntNormal As New Font("Arial", 11, FontStyle.Regular)
            Dim FntNegrito12 As New Font("Arial", 12, FontStyle.Bold)
            Dim FntNegrito14 As New Font("Arial", 14, FontStyle.Bold)
            Dim FntGrandeX As New Font("Arial", 18, FontStyle.Bold)

            ' MARCA D'AGUA
            Dim Descr1 As String = vGlobal.gs01
            Dim Descr2 As String = vGlobal.gs02
            Dim font As New Font("Arial", 40, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
            Dim img As Image
            If vInstLogin = "LEA01" Or vInstLogin = "LEA02" Then
                img = Image.FromFile(Application.StartupPath & "\LogoLeales.png")
            ElseIf vInstLogin = "ILE01" Or vInstLogin = "ILE02" Then
                img = Image.FromFile(Application.StartupPath & "\LogoLaEsp.png")
            Else
                img = Image.FromFile(Application.StartupPath & "\LogoBudeguer.png")
            End If

            ' Linha pontilhada
            Dim dashValues As Single() = {2, 2, 2, 2}
            Dim blackPen = New Pen(Color.Black, 1)
            blackPen.DashPattern = dashValues

            Dim nEspessuraLine As Single = 1.5 ' Linha um pouquinho mais grossa para destacar os blocos
            Dim Pen = New Pen(Color.Black, nEspessuraLine)
            Dim wPen As New Pen(Color.Black, nEspessuraLine)

            ' =========================================================================
            ' [PRIMEIRA VIA] - Início no Y=25, Altura expandida para 340 pixels
            ' =========================================================================
            e.Graphics.DrawRectangle(wPen, 25, 25, 770, 340)

            ' Área Logotipo e Cabeçalho
            e.Graphics.DrawImage(img, 35, 35, 55, 55) ' Aumentado tamanho do logo
            e.Graphics.DrawString(sDESCR_EMPRESA, FntNormal, Brushes.Black, 100, 35)
            e.Graphics.DrawString(sDESCR_L2, FntNormal, Brushes.Black, 100, 52)
            e.Graphics.DrawString(sDESCR_L3, FntNormal, Brushes.Black, 100, 69)
            e.Graphics.DrawString(sDESCR_L4, FntNormal, Brushes.Black, 100, 86)

            e.Graphics.DrawString("COMPROBANTE RECEPCIÓN DE CAÑA", FntNegrito14, Brushes.Black, 410, 35)
            e.Graphics.DrawString("X", FntGrandeX, Brushes.Black, 415, 55)
            e.Graphics.DrawString("Comprobante No Válido como Factura", FntNormal, Brushes.Black, 340, 86)
            e.Graphics.DrawString("Nº " & sNoLiberacao.PadLeft(8, "0"), FntNegrito12, Brushes.Black, 670, 65)
            e.Graphics.DrawString("OC " & sOrdemCarga.PadLeft(9, "0"), FntNegrito12, Brushes.Black, 670, 86)

            e.Graphics.DrawLine(Pen, New Point(25, 115), New Point(795, 115))

            ' Área Cañero / Fornecedor
            e.Graphics.DrawString("Fecha/Hora: " & sHrSaida, FntNormal, Brushes.Black, 560, 122)
            e.Graphics.DrawString("Cañero: " & sFornecedor, FntNormal, Brushes.Black, 35, 145)
            e.Graphics.DrawString("Finca: " & sFazenda, FntNormal, Brushes.Black, 35, 165)
            e.Graphics.DrawString("Cosecha: " & sDeSistColh, FntNormal, Brushes.Black, 575, 165)
            e.Graphics.DrawString("Contrato: MAQUILA", FntNormal, Brushes.Black, 35, 185)
            e.Graphics.DrawString("Remito: " & sNumeroRemito, FntNormal, Brushes.Black, 575, 185)
            e.Graphics.DrawString("Nº CUIT: " & sCuitFazenda, FntNormal, Brushes.Black, 35, 205)

            e.Graphics.DrawLine(Pen, New Point(25, 230), New Point(795, 230))

            ' Área Prestador / Transporte
            e.Graphics.DrawString("Proveedor: " & sTransporta, FntNormal, Brushes.Black, 35, 240)
            e.Graphics.DrawString("Camión: " & sEquipamento & " - " & sNoPlaca, FntNormal, Brushes.Black, 35, 260)
            e.Graphics.DrawString("Nº de CUIT: " & sCuitTransporta, FntNormal, Brushes.Black, 35, 280)

            ' Área Valores dos Pesos
            'e.Graphics.DrawString("Bruto", FntNegrito12, Brushes.Black, 450, 240)
            'e.Graphics.DrawString("Tara", FntNegrito12, Brushes.Black, 570, 240)
            'e.Graphics.DrawString("Neto", FntNegrito12, Brushes.Black, 690, 240)
            e.Graphics.DrawString("Bruto", FntNegrito12, Brushes.Black, 35, 320)
            e.Graphics.DrawString("Tara", FntNegrito12, Brushes.Black, 155, 320)
            e.Graphics.DrawString("Neto", FntNegrito12, Brushes.Black, 275, 320)

            'e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FntNegrito12, Brushes.Black, 450, 265)
            'e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FntNegrito12, Brushes.Black, 570, 265)
            'e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FntNegrito12, Brushes.Black, 690, 265)
            e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FntNegrito12, Brushes.Black, 35, 340)
            e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FntNegrito12, Brushes.Black, 155, 340)
            e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FntNegrito12, Brushes.Black, 275, 340)

            e.Graphics.DrawString("1º Via", FntNegrito12, Brushes.Black, 740, 335)

            ' Linha divisória pontilhada 1
            e.Graphics.DrawLine(blackPen, New Point(10, 380), New Point(810, 380))


            ' =========================================================================
            ' [SEGUNDA VIA] - Início no Y=395, Altura expandida para 340 pixels
            ' =========================================================================
            e.Graphics.DrawRectangle(wPen, 25, 395, 770, 340)

            ' Área Logotipo e Cabeçalho
            e.Graphics.DrawImage(img, 35, 405, 55, 55)
            e.Graphics.DrawString(sDESCR_EMPRESA, FntNormal, Brushes.Black, 100, 405)
            e.Graphics.DrawString(sDESCR_L2, FntNormal, Brushes.Black, 100, 422)
            e.Graphics.DrawString(sDESCR_L3, FntNormal, Brushes.Black, 100, 439)
            e.Graphics.DrawString(sDESCR_L4, FntNormal, Brushes.Black, 100, 456)

            e.Graphics.DrawString("COMPROBANTE RECEPCIÓN DE CAÑA", FntNegrito14, Brushes.Black, 410, 405)
            e.Graphics.DrawString("X", FntGrandeX, Brushes.Black, 415, 425)
            e.Graphics.DrawString("Comprobante No Válido como Factura", FntNormal, Brushes.Black, 340, 456)
            e.Graphics.DrawString("Nº " & sNoLiberacao.PadLeft(8, "0"), FntNegrito12, Brushes.Black, 670, 435)
            e.Graphics.DrawString("OC " & sOrdemCarga.PadLeft(9, "0"), FntNegrito12, Brushes.Black, 670, 456)

            e.Graphics.DrawLine(Pen, New Point(25, 485), New Point(795, 485))

            ' Área Cañero / Fornecedor
            e.Graphics.DrawString("Fecha/Hora: " & sHrSaida, FntNormal, Brushes.Black, 560, 492)
            e.Graphics.DrawString("Cañero: " & sFornecedor, FntNormal, Brushes.Black, 35, 515)
            e.Graphics.DrawString("Finca: " & sFazenda, FntNormal, Brushes.Black, 35, 535)
            e.Graphics.DrawString("Cosecha: " & sDeSistColh, FntNormal, Brushes.Black, 575, 535)
            e.Graphics.DrawString("Contrato: MAQUILA", FntNormal, Brushes.Black, 35, 555)
            e.Graphics.DrawString("Remito: " & sNumeroRemito, FntNormal, Brushes.Black, 575, 555)
            e.Graphics.DrawString("Nº CUIT: " & sCuitFazenda, FntNormal, Brushes.Black, 35, 575)

            e.Graphics.DrawLine(Pen, New Point(25, 600), New Point(795, 600))

            ' Área Prestador / Transporte
            e.Graphics.DrawString("Proveedor: " & sTransporta, FntNormal, Brushes.Black, 35, 610)
            e.Graphics.DrawString("Camión: " & sEquipamento & " - " & sNoPlaca, FntNormal, Brushes.Black, 35, 630)
            e.Graphics.DrawString("Nº de CUIT: " & sCuitTransporta, FntNormal, Brushes.Black, 35, 650)

            ' Área Valores dos Pesos
            'e.Graphics.DrawString("Bruto", FntNegrito12, Brushes.Black, 450, 610)
            'e.Graphics.DrawString("Tara", FntNegrito12, Brushes.Black, 570, 610)
            'e.Graphics.DrawString("Neto", FntNegrito12, Brushes.Black, 690, 610)
            e.Graphics.DrawString("Bruto", FntNegrito12, Brushes.Black, 35, 690)
            e.Graphics.DrawString("Tara", FntNegrito12, Brushes.Black, 155, 690)
            e.Graphics.DrawString("Neto", FntNegrito12, Brushes.Black, 275, 690)

            'e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FntNegrito12, Brushes.Black, 450, 635)
            'e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FntNegrito12, Brushes.Black, 570, 635)
            'e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FntNegrito12, Brushes.Black, 690, 635)
            e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FntNegrito12, Brushes.Black, 35, 710)
            e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FntNegrito12, Brushes.Black, 155, 710)
            e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FntNegrito12, Brushes.Black, 275, 710)

            e.Graphics.DrawString("2º Via", FntNegrito12, Brushes.Black, 740, 705)

            ' Linha divisória pontilhada 2
            e.Graphics.DrawLine(blackPen, New Point(10, 750), New Point(810, 750))


            ' =========================================================================
            ' [TERCEIRA VIA] - Início no Y=765, Altura expandida para 340 pixels
            ' =========================================================================
            e.Graphics.DrawRectangle(wPen, 25, 765, 770, 340)

            ' Área Logotipo e Cabeçalho
            e.Graphics.DrawImage(img, 35, 775, 55, 55)
            e.Graphics.DrawString(sDESCR_EMPRESA, FntNormal, Brushes.Black, 100, 775)
            e.Graphics.DrawString(sDESCR_L2, FntNormal, Brushes.Black, 100, 792)
            e.Graphics.DrawString(sDESCR_L3, FntNormal, Brushes.Black, 100, 809)
            e.Graphics.DrawString(sDESCR_L4, FntNormal, Brushes.Black, 100, 826)

            e.Graphics.DrawString("COMPROBANTE RECEPCIÓN DE CAÑA", FntNegrito14, Brushes.Black, 410, 775)
            e.Graphics.DrawString("X", FntGrandeX, Brushes.Black, 415, 795)
            e.Graphics.DrawString("Comprobante No Válido como Factura", FntNormal, Brushes.Black, 340, 826)
            e.Graphics.DrawString("Nº " & sNoLiberacao.PadLeft(8, "0"), FntNegrito12, Brushes.Black, 670, 805)
            e.Graphics.DrawString("OC " & sOrdemCarga.PadLeft(9, "0"), FntNegrito12, Brushes.Black, 670, 826)

            e.Graphics.DrawLine(Pen, New Point(25, 855), New Point(795, 855))

            ' Área Cañero / Fornecedor
            e.Graphics.DrawString("Fecha/Hora: " & sHrSaida, FntNormal, Brushes.Black, 560, 862)
            e.Graphics.DrawString("Cañero: " & sFornecedor, FntNormal, Brushes.Black, 35, 885)
            e.Graphics.DrawString("Finca: " & sFazenda, FntNormal, Brushes.Black, 35, 905)
            e.Graphics.DrawString("Cosecha: " & sDeSistColh, FntNormal, Brushes.Black, 575, 905)
            e.Graphics.DrawString("Contrato: MAQUILA", FntNormal, Brushes.Black, 35, 925)
            e.Graphics.DrawString("Remito: " & sNumeroRemito, FntNormal, Brushes.Black, 575, 925)
            e.Graphics.DrawString("Nº CUIT: " & sCuitFazenda, FntNormal, Brushes.Black, 35, 945)

            e.Graphics.DrawLine(Pen, New Point(25, 970), New Point(795, 970))

            ' Área Prestador / Transporte
            e.Graphics.DrawString("Proveedor: " & sTransporta, FntNormal, Brushes.Black, 35, 980)
            e.Graphics.DrawString("Camión: " & sEquipamento & " - " & sNoPlaca, FntNormal, Brushes.Black, 35, 1000)
            e.Graphics.DrawString("Nº de CUIT: " & sCuitTransporta, FntNormal, Brushes.Black, 35, 1020)

            ' Área Valores dos Pesos
            'e.Graphics.DrawString("Bruto", FntNegrito12, Brushes.Black, 450, 980)
            'e.Graphics.DrawString("Tara", FntNegrito12, Brushes.Black, 570, 980)
            'e.Graphics.DrawString("Neto", FntNegrito12, Brushes.Black, 690, 980)
            e.Graphics.DrawString("Bruto", FntNegrito12, Brushes.Black, 35, 1060)
            e.Graphics.DrawString("Tara", FntNegrito12, Brushes.Black, 155, 1060)
            e.Graphics.DrawString("Neto", FntNegrito12, Brushes.Black, 275, 1060)

            'e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FntNegrito12, Brushes.Black, 450, 1005)
            'e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FntNegrito12, Brushes.Black, 570, 1005)
            'e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FntNegrito12, Brushes.Black, 690, 1005)
            e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FntNegrito12, Brushes.Black, 35, 1080)
            e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FntNegrito12, Brushes.Black, 155, 1080)
            e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FntNegrito12, Brushes.Black, 275, 1080)

            e.Graphics.DrawString("3º Via", FntNegrito12, Brushes.Black, 740, 1075)

            ' Linha final pontilhada
            e.Graphics.DrawLine(blackPen, New Point(10, 1120), New Point(810, 1120))


            frmReimpressao.bImpressoOrdemCargas = True

        Catch ex As Exception
            MsgBox("PrintExtratoSaida_Budeguer: " & ex.ToString, MsgBoxStyle.Critical, "Error")
        End Try
    End Sub
    'Private Sub PrintExtratoSaida_Budeguer_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Budeguer.PrintPage
    '    Try
    '        frmReimpressao.bImpressoOrdemCargas = False
    '        Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
    '        Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
    '        Dim altura As Integer = 1000

    '        Dim barra As Image

    '        Dim sInstancia, sIdViagem, sNoLiberacao, sFazenda As String
    '        Dim sQtBruto, sQtTara, sQtLiquido As String
    '        Dim sFornecedor, sEquipamento, sOrdemCarga As String
    '        Dim sNumeroRemito, sDeSistColh, sCuitFazenda As String
    '        Dim sDtSaida, sHrSaida, sDtMovimento, sNoPlaca, sCuitTransporta As String
    '        Dim sTransporta As String

    '        Dim item = Budeguer.ImprExtratoSaida.Dados().FirstOrDefault
    '        sInstancia = item.Instancia
    '        sIdViagem = item.IdViagem
    '        sNoLiberacao = item.NoLiberacao
    '        sOrdemCarga = item.OrdemCarga
    '        sNumeroRemito = item.NumeroRemito
    '        sFornecedor = item.Fornecedor
    '        sFazenda = item.Fazenda
    '        sCuitFazenda = item.CuitFazenda
    '        sEquipamento = item.Equipamento
    '        sNoPlaca = item.NoPlaca
    '        sTransporta = item.Transporta
    '        sCuitTransporta = item.CuitTransporta
    '        sDeSistColh = item.DeSistColh
    '        sDtSaida = item.DtSaida
    '        sHrSaida = item.HrSaida
    '        sDtMovimento = item.DtMovimento
    '        sQtBruto = item.QtBruto
    '        sQtTara = item.QtTara
    '        sQtLiquido = item.QtLiquido


    '        vGlobal.sNomeEmpresa = cadempresas(cadparametros(("GERAIS", "CD_EMPRESA")))

    '        vGlobal.sValor02 = sIdViagem

    '        'MARCA D'AGUA
    '        Dim Descr1 As String = vGlobal.gs01
    '        Dim Descr2 As String = vGlobal.gs02
    '        Dim font As New Font("Arial", 40, FontStyle.Bold)
    '        Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
    '        Dim img As Image
    '        If vInstLogin = "LEA01" Or vInstLogin = "LEA02" Then
    '            img = Image.FromFile(Application.StartupPath & "\LogoLeales.png")
    '        ElseIf vInstLogin = "ILE01" Or vInstLogin = "ILE02" Then
    '            img = Image.FromFile(Application.StartupPath & "\LogoLaEsp.png")
    '        Else
    '            img = Image.FromFile(Application.StartupPath & "\LogoBudeguer.png")
    '        End If

    '        'Linha pontilhada
    '        Dim dashValues As Single() = {2, 2, 2, 2}
    '        Dim blackPen = New Pen(Color.Black, 1)
    '        blackPen.DashPattern = dashValues

    '        Dim nEspessuraLine As Single = 1
    '        Dim Pen = New Pen(Color.Black, nEspessuraLine)

    '        '[Primeira Via - Inicio]
    '        '*** Borda Externa Inicio ***
    '        Dim wPen01 As New Pen(Color.Black, nEspessuraLine)
    '        e.Graphics.DrawRectangle(wPen01, 25, 25, 770, 235)
    '        '*** Borda Externa Fim ***

    '        '***Área logotipo Inicio ***
    '        e.Graphics.DrawImage(img, 30, 30, 40, 40)
    '        e.Graphics.DrawString(sDESCR_EMPRESA, FontesImpressao.Fonte8R, Brushes.Black, 80, 30)
    '        e.Graphics.DrawString(sDESCR_L2, FontesImpressao.Fonte8R, Brushes.Black, 80, 40)
    '        e.Graphics.DrawString(sDESCR_L3, FontesImpressao.Fonte8R, Brushes.Black, 80, 50)
    '        e.Graphics.DrawString(sDESCR_L4, FontesImpressao.Fonte8R, Brushes.Black, 80, 60)
    '        e.Graphics.DrawString("COMPROBANTE RECEPCIÓN DE CAÑA", FontesImpressao.Fonte11B, Brushes.Black, 500, 30)
    '        e.Graphics.DrawString("X", Fonte13B, Brushes.Black, 410, 45)
    '        e.Graphics.DrawString("Comprobante No Válido como Factura", FontesImpressao.Fonte8R, Brushes.Black, 300, 65)
    '        e.Graphics.DrawString("Nº " & sNoLiberacao.PadLeft(8, "0"), FontesImpressao.Fonte10B, Brushes.Black, 670, 50)

    '        Dim Point01 As New Point(25, 80)
    '        Dim Point11 As New Point(795, 80)
    '        e.Graphics.DrawLine(Pen, Point01, Point11)
    '        '***Área logotipo Fim ***

    '        '***Área Fornecedor Inicio ***
    '        e.Graphics.DrawString("Fecha/Hora: " & sHrSaida, FontesImpressao.Fonte8R, Brushes.Black, 575, 85)
    '        e.Graphics.DrawString("Cañero: " & sFornecedor, FontesImpressao.Fonte8R, Brushes.Black, 30, 105)
    '        e.Graphics.DrawString("Finca: " & sFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 115)
    '        e.Graphics.DrawString("Cosecha: " & sDeSistColh, FontesImpressao.Fonte8R, Brushes.Black, 600, 115)
    '        e.Graphics.DrawString("Remito: " & sNumeroRemito, FontesImpressao.Fonte8R, Brushes.Black, 600, 125)
    '        e.Graphics.DrawString("Contrato: MAQUILA", FontesImpressao.Fonte8R, Brushes.Black, 30, 125)
    '        e.Graphics.DrawString("Nº CUIT: " & sCuitFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 135)

    '        Dim Point02 As New Point(25, 150)
    '        Dim Point22 As New Point(795, 150)
    '        e.Graphics.DrawLine(Pen, Point02, Point22)
    '        '***Área Fornecedor Fim ***

    '        '***Área Prestador Inicio***
    '        e.Graphics.DrawString("Proveedor: " & sTransporta, FontesImpressao.Fonte8R, Brushes.Black, 30, 160)
    '        e.Graphics.DrawString("Camión: " & sEquipamento & " - " & sNoPlaca, FontesImpressao.Fonte8R, Brushes.Black, 30, 170)
    '        e.Graphics.DrawString("Nº de CUIT: " & sCuitTransporta, FontesImpressao.Fonte8R, Brushes.Black, 30, 180)
    '        '***Área Pretador Fim***

    '        '***Área Valores dos Pesos Inicio ***
    '        e.Graphics.DrawString("Bruto", FontesImpressao.Fonte10B, Brushes.Black, 30, 200)
    '        e.Graphics.DrawString("Tara", FontesImpressao.Fonte10B, Brushes.Black, 130, 200)
    '        e.Graphics.DrawString("Neto", FontesImpressao.Fonte10B, Brushes.Black, 230, 200)

    '        e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 30, 215)
    '        e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 130, 215)
    '        e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 230, 215)
    '        '***Área Valores dos Pesos Fim ***

    '        e.Graphics.DrawString("1º Via", FontesImpressao.Fonte8R, Brushes.Black, 750, 245)
    '        '[Primeira Via - Fim]

    '        '***Linha pontilhada após a primeira via***
    '        Dim PointV101 As New Point(10, 270)
    '        Dim PointV102 As New Point(810, 270)
    '        e.Graphics.DrawLine(blackPen, PointV101, PointV102)

    '        '[Segunda Via - Inicio]
    '        '*** Borda Externa Inicio ***
    '        Dim wPen02 As New Pen(Color.Black, nEspessuraLine)
    '        e.Graphics.DrawRectangle(wPen02, 25, 280, 770, 235)
    '        '*** Borda Externa Fim ***

    '        '***Área logotipo Inicio ***
    '        e.Graphics.DrawImage(img, 30, 285, 40, 40)
    '        e.Graphics.DrawString(sDESCR_EMPRESA, FontesImpressao.Fonte8R, Brushes.Black, 80, 285)
    '        e.Graphics.DrawString(sDESCR_L2, FontesImpressao.Fonte8R, Brushes.Black, 80, 295)
    '        e.Graphics.DrawString(sDESCR_L3, FontesImpressao.Fonte8R, Brushes.Black, 80, 305)
    '        e.Graphics.DrawString(sDESCR_L4, FontesImpressao.Fonte8R, Brushes.Black, 80, 315)
    '        e.Graphics.DrawString("COMPROBANTE RECEPCIÓN DE CAÑA", FontesImpressao.Fonte11B, Brushes.Black, 500, 285)
    '        e.Graphics.DrawString("X", Fonte13B, Brushes.Black, 410, 300)
    '        e.Graphics.DrawString("Comprobante No Válido como Factura", FontesImpressao.Fonte8R, Brushes.Black, 300, 320)
    '        e.Graphics.DrawString("Nº " & sNoLiberacao.PadLeft(8, "0"), FontesImpressao.Fonte10B, Brushes.Black, 670, 305)

    '        Dim Point03 As New Point(25, 335)
    '        Dim Point33 As New Point(795, 335)
    '        e.Graphics.DrawLine(Pen, Point03, Point33)
    '        '***Área logotipo Fim ***

    '        '***Área Fornecedor Inicio ***
    '        e.Graphics.DrawString("Fecha/Hora: " & sHrSaida, FontesImpressao.Fonte8R, Brushes.Black, 575, 340)
    '        e.Graphics.DrawString("Cañero: " & sFornecedor, FontesImpressao.Fonte8R, Brushes.Black, 30, 360) '+20
    '        e.Graphics.DrawString("Finca: " & sFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 370) '+10
    '        e.Graphics.DrawString("Cosecha: " & sDeSistColh, FontesImpressao.Fonte8R, Brushes.Black, 600, 370) '+10
    '        e.Graphics.DrawString("Remito: " & sNumeroRemito, FontesImpressao.Fonte8R, Brushes.Black, 600, 380) '+10
    '        e.Graphics.DrawString("Contrato: MAQUILA", FontesImpressao.Fonte8R, Brushes.Black, 30, 380) '+10
    '        e.Graphics.DrawString("Nº CUIT: " & sCuitFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 390) '+10

    '        Dim Point04 As New Point(25, 405)
    '        Dim Point44 As New Point(795, 405)
    '        e.Graphics.DrawLine(Pen, Point04, Point44)
    '        '***Área Fornecedor Fim ***

    '        '***Área Prestador Inicio***
    '        e.Graphics.DrawString("Proveedor: " & sTransporta, FontesImpressao.Fonte8R, Brushes.Black, 30, 415)
    '        e.Graphics.DrawString("Camión: " & sEquipamento & " - " & sNoPlaca, FontesImpressao.Fonte8R, Brushes.Black, 30, 425)
    '        e.Graphics.DrawString("Nº de CUIT: " & sCuitTransporta, FontesImpressao.Fonte8R, Brushes.Black, 30, 435)
    '        '***Área Pretador Fim***

    '        '***Área Valores dos Pesos Inicio ***
    '        e.Graphics.DrawString("Bruto", FontesImpressao.Fonte10B, Brushes.Black, 30, 455)
    '        e.Graphics.DrawString("Tara", FontesImpressao.Fonte10B, Brushes.Black, 130, 455)
    '        e.Graphics.DrawString("Neto", FontesImpressao.Fonte10B, Brushes.Black, 230, 455)

    '        e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 30, 470)
    '        e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 130, 470)
    '        e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 230, 470)
    '        '***Área Valores dos Pesos Fim ***

    '        e.Graphics.DrawString("2º Via", FontesImpressao.Fonte8R, Brushes.Black, 750, 500)
    '        '[Segunda Via - Fim]

    '        '***Linhas pontilhadas após a segunda via
    '        Dim PointV201 As New Point(10, 525)
    '        Dim PointV202 As New Point(810, 525)
    '        e.Graphics.DrawLine(blackPen, PointV201, PointV202)

    '        '[Terceira Via - Inicio]
    '        '*** Borda Externa Inicio ***
    '        Dim wPen03 As New Pen(Color.Black, nEspessuraLine)
    '        e.Graphics.DrawRectangle(wPen03, 25, 535, 770, 235)
    '        '*** Borda Externa Fim ***

    '        '***Área logotipo Inicio ***
    '        e.Graphics.DrawImage(img, 30, 540, 40, 40)
    '        e.Graphics.DrawString(sDESCR_EMPRESA, FontesImpressao.Fonte8R, Brushes.Black, 80, 540)
    '        e.Graphics.DrawString(sDESCR_L2, FontesImpressao.Fonte8R, Brushes.Black, 80, 550) '+10
    '        e.Graphics.DrawString(sDESCR_L3, FontesImpressao.Fonte8R, Brushes.Black, 80, 560) '+10
    '        e.Graphics.DrawString(sDESCR_L4, FontesImpressao.Fonte8R, Brushes.Black, 80, 570) '+10
    '        e.Graphics.DrawString("COMPROBANTE RECEPCIÓN DE CAÑA", FontesImpressao.Fonte11B, Brushes.Black, 500, 540)
    '        e.Graphics.DrawString("X", Fonte13B, Brushes.Black, 410, 555) '+15
    '        e.Graphics.DrawString("Comprobante No Válido como Factura", FontesImpressao.Fonte8R, Brushes.Black, 300, 575) '+20
    '        e.Graphics.DrawString("Nº " & sNoLiberacao.PadLeft(8, "0"), FontesImpressao.Fonte10B, Brushes.Black, 670, 560) '-15

    '        Dim Point05 As New Point(25, 590)
    '        Dim Point55 As New Point(795, 590)
    '        e.Graphics.DrawLine(Pen, Point05, Point55)
    '        '***Área logotipo Fim ***

    '        '***Área Fornecedor Inicio ***
    '        e.Graphics.DrawString("Fecha/Hora: " & sHrSaida, FontesImpressao.Fonte8R, Brushes.Black, 575, 595)
    '        e.Graphics.DrawString("Cañero: " & sFornecedor, FontesImpressao.Fonte8R, Brushes.Black, 30, 615) '+20
    '        e.Graphics.DrawString("Finca: " & sFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 625) '+10
    '        e.Graphics.DrawString("Cosecha: " & sDeSistColh, FontesImpressao.Fonte8R, Brushes.Black, 600, 625) '+10
    '        e.Graphics.DrawString("Remito: " & sNumeroRemito, FontesImpressao.Fonte8R, Brushes.Black, 600, 635) '+10
    '        e.Graphics.DrawString("Contrato: MAQUILA", FontesImpressao.Fonte8R, Brushes.Black, 30, 635) '+10
    '        e.Graphics.DrawString("Nº CUIT: " & sCuitFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 645) '+10

    '        Dim Point06 As New Point(25, 660)
    '        Dim Point66 As New Point(795, 660)
    '        e.Graphics.DrawLine(Pen, Point06, Point66)
    '        '***Área Fornecedor Fim ***

    '        '***Área Prestador Inicio***
    '        e.Graphics.DrawString("Proveedor: " & sTransporta, FontesImpressao.Fonte8R, Brushes.Black, 30, 670)
    '        e.Graphics.DrawString("Camión: " & sEquipamento & " - " & sNoPlaca, FontesImpressao.Fonte8R, Brushes.Black, 30, 680)
    '        e.Graphics.DrawString("Nº de CUIT: " & sCuitTransporta, FontesImpressao.Fonte8R, Brushes.Black, 30, 690)
    '        '***Área Pretador Fim***

    '        '***Área Valores dos Pesos Inicio ***
    '        e.Graphics.DrawString("Bruto", FontesImpressao.Fonte10B, Brushes.Black, 30, 710)
    '        e.Graphics.DrawString("Tara", FontesImpressao.Fonte10B, Brushes.Black, 130, 710)
    '        e.Graphics.DrawString("Neto", FontesImpressao.Fonte10B, Brushes.Black, 230, 710)

    '        e.Graphics.DrawString(Format(CInt(sQtBruto), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 30, 725)
    '        e.Graphics.DrawString(Format(CInt(sQtTara), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 130, 725)
    '        e.Graphics.DrawString(Format(CInt(sQtLiquido), "##,##0"), FontesImpressao.Fonte10B, Brushes.Black, 230, 725)
    '        '***Área Valores dos Pesos Fim ***

    '        e.Graphics.DrawString("3º Via", FontesImpressao.Fonte8R, Brushes.Black, 750, 755)
    '        '[Terceira Via - Fim]

    '        '***Linha pontilhada após a terceira via***
    '        Dim PointV301 As New Point(10, 780)
    '        Dim PointV302 As New Point(810, 780)
    '        e.Graphics.DrawLine(blackPen, PointV301, PointV302)

    '        frmReimpressao.bImpressoOrdemCargas = True

    '    Catch ex As Exception
    '        MsgBox("PrintExtratoSaida_Budeguer: " & ex.ToString, MsgBoxStyle.Critical, "Error")
    '    End Try
    'End Sub

    Public WithEvents PrintOrdemCarga_Budeguer As New PrintDocument()
    Private Sub PrintOrdemCarga_Budeguer_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintOrdemCarga_Budeguer.PrintPage
        Try
            frmReimpressao.bImpressoOrdemCargas = False
            vGlobal.sValor01 = Nothing
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim sDtEmissao, sHoraEmissao, sNoOrdemCarga, sFazenda, sFornecedor, sFrenteColheita, sCaminhao, sPrestador As String
            Dim sNoQueima, sCartoes, sUsuario As String
            Dim sWhere As String

            If frmReimpressao.bBotaoImprimir = True Then
                sWhere = "  and ld.pfg_ordem_carga = 'S' and ld.id_despacho = " & fUltimoDespacho(frmReimpressao.tbBoletim.Text) & ""
            ElseIf frmReimpressao.bBotaoImprimir = False Then
                sWhere = "  and ld.fg_status = 'A' and ld.pfg_ordem_carga is null"
            End If

            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select ld.id_despacho,")
            sql.AppendLine("       to_char(ld.dthr_ret_planej,'dd/mm/yyyy') data,")
            sql.AppendLine("       to_char(ld.dthr_ret_planej,'hh24:mi:ss') hora,")
            sql.AppendLine("       fn.cd_fornec||'-'||fn.de_fornec fornecedor,")
            sql.AppendLine("       ft.cd_fren_tran||'-'||ft.de_fren_tran frente,")
            sql.AppendLine("       e.cd_equipto||'-'||e.no_placa veiculo,")
            sql.AppendLine("       tt.cd_transp||'-'||tt.de_transp prestador,")
            sql.AppendLine("       trim(u1.cd_upnivel1)||'-'||u1.de_upnivel1 fazenda,")
            sql.AppendLine("       llc.no_queima,ld.usuario")
            sql.AppendLine("from lt_despachos ld, lt_liberacao_carreg llc,")
            sql.AppendLine("     pminstancias pi , equiptos e ,")
            sql.AppendLine("     queima_he qh, queima_de qd, frentransp ft, sistcolh sc,")
            sql.AppendLine("     upnivel1 u1, fornecs fn, transporta tt")
            sql.AppendLine("where tt.cd_transp = e.cd_transp")
            sql.AppendLine("  and fn.cd_fornec = u1.cd_fornec")
            sql.AppendLine("  and u1.cd_upnivel1 = qd.cd_upnivel1")
            sql.AppendLine("  and sc.cd_sist_colh = llc.cd_sist_colh")
            sql.AppendLine("  and ft.cd_fren_tran = llc.cd_fren_tran")
            sql.AppendLine("  and qh.no_queima = llc.no_queima")
            sql.AppendLine("  and qh.no_queima = qd.no_queima")
            sql.AppendLine("  and e.cd_equipto = ld.cd_equipto")
            sql.AppendLine("  and pi.instancia = ld.instancia")
            sql.AppendLine("  and ld.id_liberacao = llc.id_liberacao")
            sql.AppendLine("  and pi.instancia = :instancia")
            sql.AppendLine("" & sWhere & "")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                Using rs As OracleDataReader = cmd.ExecuteReader
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
                End Using
            End Using

            vGlobal.sNomeEmpresa = cadempresas(Convert.ToInt64(cadparametros(("GERAIS", "CD_EMPRESA"))))

            'Inicio da Impressão

            'MARCA D'AGUA
            Dim Descr1 As String = vGlobal.gs01
            Dim Descr2 As String = vGlobal.gs02
            Dim font As New Font("Arial", 40, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
            Dim imgBdg As Image = Image.FromFile(Application.StartupPath & "\GrupoBudeguer.png")
            Dim imgLea As Image = Image.FromFile(Application.StartupPath & "\IngLeales.png")
            Dim imgIle As Image = Image.FromFile(Application.StartupPath & "\IngLaEsp.png")
            Dim retImg01 As New Rectangle(25, 25, 136, 44)
            Dim retImg02 As New Rectangle(25, 415, 136, 44)
            Dim retImg03 As New Rectangle(25, 805, 136, 44)

            Dim retLEA01 As New Rectangle(25, 69, 111, 30)
            Dim retLEA02 As New Rectangle(25, 459, 111, 30)
            Dim retLEA03 As New Rectangle(25, 849, 111, 30)

            Dim retILE01 As New Rectangle(25, 69, 126, 35)
            Dim retILE02 As New Rectangle(25, 459, 126, 35)
            Dim retILE03 As New Rectangle(25, 849, 126, 35)

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
            e.Graphics.DrawImage(imgBdg, retImg01)
            If vInstLogin = "LEA01" Then
                e.Graphics.DrawImage(imgLea, retLEA01)
            ElseIf vInstLogin = "ILE01" Then
                e.Graphics.DrawImage(imgIle, retILE01)
            End If

            'Dados a serem impressos
            e.Graphics.DrawString("ORDEN CARGA CAÑA DE AZUCAR", Fonte13B, Brushes.Black, 195, 60)
            e.Graphics.DrawString("Nº " & sNoOrdemCarga, Fonte13B, Brushes.Black, 520, 60)
            e.Graphics.DrawString("Frente de Cosecha:    " & sFrenteColheita, FontesImpressao.Fonte10R, Brushes.Black, 30, 140)
            e.Graphics.DrawString("Transportista:   " & sPrestador, FontesImpressao.Fonte10R, Brushes.Black, 30, 160)
            e.Graphics.DrawString("Vehículo:   " & sCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 30, 180)
            e.Graphics.DrawString("Productor: " & sFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 30, 200)
            e.Graphics.DrawString("Finca:  " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 220)
            e.Graphics.DrawString("Lote / Seccion:    " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 240)
            e.Graphics.DrawString("Nº de Remito: " & sNoQueima, FontesImpressao.Fonte10R, Brushes.Black, 30, 260)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, FontesImpressao.Fonte10B, Brushes.Black, 30, 280)

            e.Graphics.DrawString("Fecha: " & sDtEmissao & " " & sHoraEmissao, FontesImpressao.Fonte10B, Brushes.Black, 30, 340)
            e.Graphics.DrawString("Operador: " & sUsuario, FontesImpressao.Fonte10B, Brushes.Black, 30, 360)
            e.Graphics.DrawString("1ª Cañero", FontesImpressao.Fonte10B, Brushes.Black, 500, 360)

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
            e.Graphics.DrawImage(imgBdg, retImg02)
            If vInstLogin = "LEA01" Then
                e.Graphics.DrawImage(imgLea, retLEA02)
            ElseIf vInstLogin = "ILE01" Then
                e.Graphics.DrawImage(imgIle, retILE02)
            End If

            'Dados a serem impressos
            e.Graphics.DrawString("ORDEN CARGA CAÑA DE AZUCAR", Fonte13B, Brushes.Black, 195, 450)
            e.Graphics.DrawString("Nº " & sNoOrdemCarga, Fonte13B, Brushes.Black, 520, 450)
            e.Graphics.DrawString("Frente de Cosecha:    " & sFrenteColheita, FontesImpressao.Fonte10R, Brushes.Black, 30, 530)
            e.Graphics.DrawString("Transportista:   " & sPrestador, FontesImpressao.Fonte10R, Brushes.Black, 30, 550)
            e.Graphics.DrawString("Vehículo:   " & sCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 30, 570)
            e.Graphics.DrawString("Productor: " & sFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 30, 590)
            e.Graphics.DrawString("Finca:  " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 610)
            e.Graphics.DrawString("Lote / Seccion:    " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 630)
            e.Graphics.DrawString("Nº de Remito: " & sNoQueima, FontesImpressao.Fonte10R, Brushes.Black, 30, 650)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, FontesImpressao.Fonte10B, Brushes.Black, 30, 670)

            e.Graphics.DrawString("Fecha: " & sDtEmissao & " " & sHoraEmissao, FontesImpressao.Fonte10B, Brushes.Black, 30, 730)
            e.Graphics.DrawString("Operador: " & sUsuario, FontesImpressao.Fonte10B, Brushes.Black, 30, 750)
            e.Graphics.DrawString("2ª Frente", FontesImpressao.Fonte10B, Brushes.Black, 500, 750)

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
            e.Graphics.DrawImage(imgBdg, retImg03)
            If vInstLogin = "LEA01" Then
                e.Graphics.DrawImage(imgLea, retLEA03)
            ElseIf vInstLogin = "ILE01" Then
                e.Graphics.DrawImage(imgIle, retILE03)
            End If

            'Dados a serem impressos
            e.Graphics.DrawString("ORDEN CARGA CAÑA DE AZUCAR", Fonte13B, Brushes.Black, 195, 840)
            e.Graphics.DrawString("Nº " & sNoOrdemCarga, Fonte13B, Brushes.Black, 520, 840)
            e.Graphics.DrawString("Frente de Cosecha:    " & sFrenteColheita, FontesImpressao.Fonte10R, Brushes.Black, 30, 920)
            e.Graphics.DrawString("Transportista:   " & sPrestador, FontesImpressao.Fonte10R, Brushes.Black, 30, 940)
            e.Graphics.DrawString("Vehículo:   " & sCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 30, 960)
            e.Graphics.DrawString("Productor: " & sFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 30, 980)
            e.Graphics.DrawString("Finca:  " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 1000)
            e.Graphics.DrawString("Lote / Seccion:    " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 1020)
            e.Graphics.DrawString("Nº de Remito: " & sNoQueima, FontesImpressao.Fonte10R, Brushes.Black, 30, 1040)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, FontesImpressao.Fonte10B, Brushes.Black, 30, 1060)

            e.Graphics.DrawString("Fecha: " & sDtEmissao & " " & sHoraEmissao, FontesImpressao.Fonte10B, Brushes.Black, 30, 1120)
            e.Graphics.DrawString("Operador: " & sUsuario, FontesImpressao.Fonte10B, Brushes.Black, 30, 1140)
            e.Graphics.DrawString("3ª Transporte", FontesImpressao.Fonte10B, Brushes.Black, 500, 1140)

            'Código de Barras
            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 1060, 140, 70)) '1080

            'Final da Impressão
            frmReimpressao.bImpressoOrdemCargas = True

        Catch ex As Exception
            fGravaError("frmReimpressao", "PrintOrdemCarga_Budeguer_PrintPage", ex.ToString, "LOG_OC_ERROR")
        End Try
    End Sub

    Public WithEvents PrintExtrProduto_Budeguer As New PrintDocument()
    Private Sub PrintExtrProduto_Budeguer_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtrProduto_Budeguer.PrintPage
        Try
            Dim sWhere As String
            Dim sWhere2 As String
            If frmReimpressao.bBotaoImprimir = True Then
                sWhere = "  and o.fg_impr_ordcar = 'S'"
                sWhere2 = " and no_pesagem = :pesagem"
            ElseIf frmReimpressao.bBotaoImprimir = False Then
                sWhere = "  and o.fg_impr_ordcar = 'N'"
                sWhere2 = ""
            End If

            Dim NoPesagem, NoOrdemCarga, HrEntrada, HrSaida, DeEmpresa, NoCuit, CdFornecedor, CdTpMov As String
            Dim CdTransportador, NoCaminhao, CdMotorista, HrImpressao, sObservacao As String
            Dim CdProduto, QtPeso1, QtPeso2, QtPesoLiq, QtBruto, QtTara As String

            Dim SqlOrdCargas As New StringBuilder
            SqlOrdCargas.AppendLine("select o.cd_ordem_car, o.cd_transport,")
            SqlOrdCargas.AppendLine("      o.fg_tp_pesag, o.cd_fornec, p.cd_produto, p.cd_origem,p.cd_destino")
            SqlOrdCargas.AppendLine("from ord_car_opes o, ord_car_opes_prd p")
            SqlOrdCargas.AppendLine("Where o.instancia = p.instancia")
            SqlOrdCargas.AppendLine("  and o.cd_ordem_car = p.cd_ordem_car")
            SqlOrdCargas.AppendLine("  and o.instancia = :instancia")
            SqlOrdCargas.AppendLine("  and o.fg_status = 'C'")
            SqlOrdCargas.AppendLine(sWhere)
            SqlOrdCargas.AppendLine("order by 1")
            Using cmd1 As OracleCommand = fCmd(SqlOrdCargas.ToString)
                cmd1.Parameters.Clear()
                cmd1.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                If frmReimpressao.bBotaoImprimir = True Then
                    cmd1.Parameters.Add("pesagem", OracleDbType.Int32).Value = CInt(frmReimpressao.tbBoletim.Text)
                End If
                Using rs1 As OracleDataReader = cmd1.ExecuteReader
                    While rs1.Read
                        Dim FgTpPesag = rs1("fg_tp_pesag").ToString
                        Select Case FgTpPesag
                            Case "A"
                                Dim SqlPesagPrd As New StringBuilder
                                SqlPesagPrd.Clear()
                                SqlPesagPrd.AppendLine("select * from outr_pesagens")
                                SqlPesagPrd.AppendLine("where no_ordem_car = :ordemcar")
                                SqlPesagPrd.AppendLine("  and instancia = :instancia")
                                SqlPesagPrd.AppendLine("  and fg_tp_pesag = 'A'")
                                SqlPesagPrd.AppendLine(sWhere2)
                                Using cmd2 As OracleCommand = fCmd(SqlPesagPrd.ToString)
                                    cmd2.Parameters.Clear()
                                    cmd2.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                                    cmd2.Parameters.Add("ordemcar", OracleDbType.Int32).Value = CInt(rs1("cd_ordem_car"))
                                    Using rs2 As OracleDataReader = cmd2.ExecuteReader
                                        While rs2.Read
                                            NoPesagem = rs2("no_pesagem")
                                            NoOrdemCarga = rs2("no_ordem_car")
                                            HrEntrada = rs2("hr_entrada")
                                            HrSaida = rs2("hr_saida")
                                            NoCaminhao = rs2("caminhao")
                                            If IsDBNull(rs2("motorista")) Then
                                                CdMotorista = " "
                                            Else
                                                CdMotorista = rs2("motorista")
                                            End If
                                            CdTransportador = cadtransportadora(Convert.ToInt64(rs1("cd_transport")))
                                            CdFornecedor = caddestinatario(Convert.ToInt64(rs1("cd_destino")))

                                            CdTpMov = cadtipomovimento(Convert.ToInt64(rs2("cd_tp_mov")))
                                            CdProduto = cadproduto(rs1("cd_produto"))
                                            QtPeso1 = rs2("qt_peso1")
                                            QtPeso2 = rs2("qt_peso2")
                                            QtPesoLiq = rs2("qt_pesoliq")
                                            DeEmpresa = cadempresadeposito(rs1("cd_origem")).de_empresa
                                            NoCuit = cadempresadeposito(rs1("cd_origem")).cnpj_cuit
                                            If IsDBNull(rs2("de_observacao")) Then
                                                sObservacao = " "
                                            Else
                                                sObservacao = rs2("de_observacao")
                                            End If
                                            HrImpressao = Now()
                                        End While
                                    End Using
                                End Using
                            Case "L"
                                Dim SqlPesagAlc As New StringBuilder
                                SqlPesagAlc.Clear()
                                SqlPesagAlc.AppendLine("select * from outr_pesagens")
                                SqlPesagAlc.AppendLine("where no_ordem_car = :ordemcar")
                                SqlPesagAlc.AppendLine("  and instancia = :instancia")
                                SqlPesagAlc.AppendLine("  and fg_tp_pesag = 'L'")
                                Using cmd3 As OracleCommand = fCmd(SqlPesagAlc.ToString)
                                    cmd3.Parameters.Clear()
                                    cmd3.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                                    cmd3.Parameters.Add("ordemcar", OracleDbType.Int32).Value = CInt(rs1("cd_ordem_car"))
                                    Using rs3 As OracleDataReader = cmd3.ExecuteReader
                                        While rs3.Read
                                            NoPesagem = rs3("no_pesagem")
                                            NoOrdemCarga = rs3("no_ordem_car")
                                            HrEntrada = rs3("hr_entrada")
                                            HrSaida = rs3("hr_saida")
                                            NoCaminhao = rs3("caminhao")
                                            CdMotorista = rs3("motorista")
                                            CdTransportador = cadtransportadora(rs1("cd_transport"))
                                            CdFornecedor = cadfornectransp(rs1("cd_fornec")).de_fornec
                                            CdTpMov = cadtipomovimento(rs3("cd_tp_mov"))
                                            CdProduto = cadproduto(rs1("cd_produto"))
                                            QtPeso1 = rs3("qt_peso1")
                                            QtPeso2 = rs3("qt_peso2")
                                            QtPesoLiq = rs3("qt_pesoliq")
                                            DeEmpresa = cadempresadeposito(rs1("cd_origem")).de_empresa
                                            NoCuit = cadempresadeposito(rs1("cd_origem")).cnpj_cuit
                                            If IsDBNull(rs3("de_observacao")) Then
                                                sObservacao = " "
                                            Else
                                                sObservacao = rs3("de_observacao")
                                            End If
                                            HrImpressao = Now()
                                        End While
                                    End Using
                                End Using
                        End Select
                    End While
                End Using
            End Using

            If QtPeso1 > QtPeso2 Then
                QtBruto = QtPeso1
                QtTara = QtPeso2
            ElseIf QtPeso1 < QtPeso2 Then
                QtBruto = QtPeso2
                QtTara = QtPeso1
            End If

            Dim nEspessuraLine As Single = 1
            Dim Pen = New Pen(Color.Black, nEspessuraLine)
            Dim img As Image
            Dim sf As New StringFormat
            sf.Alignment = StringAlignment.Center

            img = Image.FromFile(Application.StartupPath & "\LogoBudeguer.png")

            Dim wPen As New Pen(Color.Black, nEspessuraLine)

            Dim str As String = sObservacao
            Dim font As New Font("Arial", 10)
            Dim lines As New List(Of String)
            For i As Integer = 0 To str.Length - 1 Step 50
                If i + 50 < str.Length Then
                    lines.Add(str.Substring(i, 50))
                Else
                    lines.Add(str.Substring(i))
                End If
            Next

            '[Primeira Via - Inicio]
            '*** Borda Externa Inicio ***
            e.Graphics.DrawRectangle(wPen, 25, 25, 770, 235)

            e.Graphics.DrawRectangle(wPen, 412, 35, 15, 15)
            '*** Borda Externa Fim ***
            'vGlobal.EMPRESA_PRD
            '***Área logotipo Inicio ***
            e.Graphics.DrawImage(img, 30, 30, 40, 40)
            e.Graphics.DrawString("COMPROBANTE PESAJE DE PRODUCTOS", FontesImpressao.Fonte11B, Brushes.Black, 70, 30)
            e.Graphics.DrawString(vGlobal.EMPRESA_PRD, FontesImpressao.Fonte12B, Brushes.Black, 70, 40)
            e.Graphics.DrawString("Movimiento: " & CdTpMov, FontesImpressao.Fonte10R, Brushes.Black, 80, 80)
            Dim rect2 As New System.Drawing.RectangleF(35, 35, 770, 235)
            e.Graphics.DrawString("X", FontesImpressao.Fonte10B, Brushes.Black, rect2, sf)
            Dim rect3 As New System.Drawing.RectangleF(35, 50, 770, 235)
            e.Graphics.DrawString("DOCUMENTO", FontesImpressao.Fonte9R, Brushes.Black, rect3, sf)
            Dim rect4 As New System.Drawing.RectangleF(35, 60, 770, 235)
            e.Graphics.DrawString("NO VALIDO", FontesImpressao.Fonte9R, Brushes.Black, rect4, sf)
            Dim rect5 As New System.Drawing.RectangleF(35, 70, 770, 235)
            e.Graphics.DrawString("COMO FACTURA", FontesImpressao.Fonte9R, Brushes.Black, rect5, sf)
            e.Graphics.DrawString("Nº Pesaje: " & NoPesagem, FontesImpressao.Fonte10R, Brushes.Black, 580, 35)
            e.Graphics.DrawString("Ord Pesaje: " & NoOrdemCarga, FontesImpressao.Fonte10R, Brushes.Black, 580, 55)
            e.Graphics.DrawString(HrImpressao, FontesImpressao.Fonte10R, Brushes.Black, 580, 75)

            Dim Point01 As New Point(25, 100)
            Dim Point11 As New Point(795, 100)
            e.Graphics.DrawLine(Pen, Point01, Point11)
            '***Área logotipo Fim ***

            '***Área Fornecedor/Produto Inicio***
            e.Graphics.DrawString("Cliente: " & CdFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 35, 110)
            e.Graphics.DrawString("Producto: " & CdProduto, FontesImpressao.Fonte10R, Brushes.Black, 35, 130)
            e.Graphics.DrawString("Patente: " & NoCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 35, 150)

            Dim Point02 As New Point(25, 170)
            Dim Point22 As New Point(795, 170)
            e.Graphics.DrawLine(Pen, Point02, Point22)
            '***Área Fornecedor/Produto Fim***

            '***Dados da Pesagem Inicio ***
            e.Graphics.DrawString("Bruto", FontesImpressao.Fonte11B, Brushes.Black, 75, 190)
            e.Graphics.DrawString("Tara", FontesImpressao.Fonte11B, Brushes.Black, 190, 190)
            e.Graphics.DrawString("Neto", FontesImpressao.Fonte11B, Brushes.Black, 305, 190)

            e.Graphics.DrawString(QtBruto, FontesImpressao.Fonte11B, Brushes.Black, 75, 210)
            e.Graphics.DrawString(QtTara, FontesImpressao.Fonte11B, Brushes.Black, 190, 210)
            e.Graphics.DrawString(QtPesoLiq, FontesImpressao.Fonte11B, Brushes.Black, 305, 210)

            'FBuscaRemito
            'e.Graphics.DrawString(sObservacao, FontesImpressao.Fonte11B, Brushes.Black, 35, 230)
            Dim xPos As Single = 35
            Dim yPos As Single = 230
            For Each line As String In lines
                e.Graphics.DrawString(line, FontesImpressao.Fonte10R, Brushes.Black, xPos, yPos)
                yPos += FontesImpressao.Fonte10R.GetHeight(e.Graphics)
            Next
            e.Graphics.DrawString("Remito: " & FBuscaRemito(NoOrdemCarga), FontesImpressao.Fonte11B, Brushes.Black, 600, 230) '600 / 230
            '***Dados da Pesagem Fim ***

            '[Segunda Via - Inicio]
            '*** Borda Externa Inicio ***
            e.Graphics.DrawRectangle(wPen, 25, 300, 770, 235)

            e.Graphics.DrawRectangle(wPen, 412, 310, 15, 15)
            '*** Borda Externa Fim ***

            '***Área logotipo Inicio ***
            e.Graphics.DrawImage(img, 30, 305, 40, 40)
            e.Graphics.DrawString("COMPROBANTE PESAJE DE PRODUCTOS", FontesImpressao.Fonte11B, Brushes.Black, 70, 305)
            e.Graphics.DrawString(vGlobal.EMPRESA_PRD, FontesImpressao.Fonte12B, Brushes.Black, 70, 315)
            e.Graphics.DrawString("Movimiento: " & CdTpMov, FontesImpressao.Fonte10R, Brushes.Black, 80, 355)
            Dim rect6 As New System.Drawing.RectangleF(35, 310, 770, 235)
            e.Graphics.DrawString("X", FontesImpressao.Fonte10B, Brushes.Black, rect6, sf)
            Dim rect7 As New System.Drawing.RectangleF(35, 325, 770, 235)
            e.Graphics.DrawString("DOCUMENTO", FontesImpressao.Fonte9R, Brushes.Black, rect7, sf)
            Dim rect8 As New System.Drawing.RectangleF(35, 335, 770, 235)
            e.Graphics.DrawString("NO VALIDO", FontesImpressao.Fonte9R, Brushes.Black, rect8, sf)
            Dim rect9 As New System.Drawing.RectangleF(35, 345, 770, 235)
            e.Graphics.DrawString("COMO FACTURA", FontesImpressao.Fonte9R, Brushes.Black, rect9, sf)
            e.Graphics.DrawString("Nº Pesaje: " & NoPesagem, FontesImpressao.Fonte10R, Brushes.Black, 580, 310)
            e.Graphics.DrawString("Ord Pesaje: " & NoOrdemCarga, FontesImpressao.Fonte10R, Brushes.Black, 580, 330)
            e.Graphics.DrawString(HrImpressao, FontesImpressao.Fonte10R, Brushes.Black, 580, 350)

            Dim Point03 As New Point(25, 375)
            Dim Point33 As New Point(795, 375)
            e.Graphics.DrawLine(Pen, Point03, Point33)
            '***Área logotipo Fim ***

            '***Área Fornecedor/Produto Inicio***
            e.Graphics.DrawString("Cliente: " & CdFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 35, 385)
            e.Graphics.DrawString("Producto: " & CdProduto, FontesImpressao.Fonte10R, Brushes.Black, 35, 405)
            e.Graphics.DrawString("Patente: " & NoCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 35, 435)
            Dim Point04 As New Point(25, 445)
            Dim Point44 As New Point(795, 445)
            e.Graphics.DrawLine(Pen, Point04, Point44)
            '***Área Fornecedor/Produto Fim***

            '***Dados da Pesagem Inicio ***
            e.Graphics.DrawString("Bruto", FontesImpressao.Fonte11B, Brushes.Black, 75, 465)
            e.Graphics.DrawString("Tara", FontesImpressao.Fonte11B, Brushes.Black, 190, 465)
            e.Graphics.DrawString("Neto", FontesImpressao.Fonte11B, Brushes.Black, 305, 465)

            e.Graphics.DrawString(QtBruto, FontesImpressao.Fonte11B, Brushes.Black, 75, 485)
            e.Graphics.DrawString(QtTara, FontesImpressao.Fonte11B, Brushes.Black, 190, 485)
            e.Graphics.DrawString(QtPesoLiq, FontesImpressao.Fonte11B, Brushes.Black, 305, 485)
            e.Graphics.DrawString(sObservacao, FontesImpressao.Fonte11B, Brushes.Black, 35, 485)
            e.Graphics.DrawString("Remito: " & FBuscaRemito(NoOrdemCarga), FontesImpressao.Fonte11B, Brushes.Black, 600, 505)
            '***Dados da Pesagem Fim ***
            If NoPesagem <> Nothing Then
                MudaStatusOrdemProduto(NoOrdemCarga, NoPesagem)
            End If

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
    End Sub

    '[LAREDO]
    Public WithEvents PrintBolEntr_Laredo As New PrintDocument()
    Private Sub PrintBolEntr_Laredo_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintBolEntr_Laredo.PrintPage
        Try
            Dim sEmpresa, sCaminhao, sCarreta, sPlaca, sPlacaCarreta, sCdFunc, sMotorista, sEntrada, sLiberacao, sViagem, sPesoBruto, sAnalise As String
            Dim sCdNivel1, sDeNivel1 As String
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000
            Dim barra As Image

            Dim lista = Laredo.listaDados
            For Each item In lista
                sLiberacao = item.noliberacao
                sViagem = item.idviagem
                sCaminhao = item.cdequiptocm
                sCarreta = If(item.cdequiptoct = 0, Nothing, item.cdequiptoct)
                sCdFunc = item.cdoperador
                If cadequiptos.ContainsKey(sCarreta) Then
                    sPlacaCarreta = cadequiptos(sCarreta).no_placa
                End If
                If cadequiptos.ContainsKey(sCaminhao) Then
                    sPlaca = cadequiptos(sCaminhao).no_placa
                End If
                If cadfuncionarios.ContainsKey(sCdFunc) Then
                    sMotorista = sCdFunc & "-" & cadfuncionarios(sCdFunc)
                End If
                sEntrada = item.hrentrada
                sPesoBruto = item.qtbruto
                sCdNivel1 = item.cdupnivel1
                sAnalise = item.noaanlise
            Next

            sEmpresa = cadparametros(("GERAIS", "DA_EMPRESA"))
            sDeNivel1 = cadparametros(("NIVEIS", "UPNIVEL1"))

            Dim sf As New StringFormat
            sf.Alignment = StringAlignment.Center

            'Linha 01
            Dim Point01 As New Point(2, 30)
            Dim Point11 As New Point(270, 30)
            Dim blackPen1 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen1, Point01, Point11)

            'Linha 02
            Dim Point02 As New Point(2, 45)
            Dim Point22 As New Point(270, 45)
            Dim blackPen2 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen2, Point02, Point22)

            'Linha 03
            Dim Point03 As New Point(2, 155)
            Dim Point33 As New Point(270, 155)
            Dim blackPen3 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen3, Point03, Point33)

            Dim rect1 As New System.Drawing.RectangleF(2, 5, 270, 20)
            Dim rect2 As New System.Drawing.RectangleF(2, 30, 270, 20)

            Dim texto As String
            e.Graphics.DrawString("*** " & sEmpresa & " ***", FontesImpressao.Fonte10R, Brushes.Black, rect1, sf)
            e.Graphics.DrawString("Boleto de Patio", FontesImpressao.Fonte10R, Brushes.Black, rect2, sf)
            texto += sDeNivel1 & " " & sCdNivel1 & vbCrLf
            texto += "Camión: " & sCaminhao & "    Placa: " & sPlaca & "" & vbCrLf
            texto += "Chofer: " & sMotorista & "" & vbCrLf
            texto += "F/H Entr: " & sEntrada & "" & vbCrLf
            texto += "Liberación: " & sLiberacao & "     Viaje: " & sViagem.PadLeft(6, "0"c) & vbCrLf & vbCrLf
            If sCarreta <> " " Then
                texto += "Remolque: " & sCarreta & "     Placa: " & sCarreta & vbCrLf & vbCrLf
            End If
            texto += "Peso Bruto: " & sPesoBruto & vbCrLf & vbCrLf & vbCrLf
            texto += ". "
            e.Graphics.DrawString(texto, FontesImpressao.Fonte10R, Brushes.Black, 2, 60)

        Catch ex As Exception
            vMensangem = ex.ToString & vbNewLine
            frmMensagem.Show()
            vMensangem = Nothing
        End Try
    End Sub

    Public Sub ImprimirBolEntr_Laredo()
        Try
            fGravaLog("9. **** Inicio da montagem de impressão", "LOG_TESTE")
            Dim sEmpresa, sCaminhao, sCarreta, sPlaca, sPlacaCarreta, sCdFunc, sMotorista, sEntrada, sLiberacao, sViagem, sPesoBruto, sAnalise As String
            Dim sCdNivel1, sDeNivel1 As String

            Dim lista = Laredo.listaDados
            For Each item In lista
                sLiberacao = item.noliberacao
                sViagem = item.idviagem
                sCaminhao = item.cdequiptocm
                sCarreta = If(item.cdequiptoct = 0, Nothing, item.cdequiptoct)
                sCdFunc = item.cdoperador
                If cadequiptos.ContainsKey(sCarreta) Then sPlacaCarreta = FBuscaPlaca(sCarreta, sLiberacao)
                If cadequiptos.ContainsKey(sCaminhao) Then sPlaca = FBuscaPlaca(sCaminhao, sLiberacao)
                If cadfuncionarios.ContainsKey(sCdFunc) Then sMotorista = sCdFunc & "-" & cadfuncionarios(sCdFunc)
                sEntrada = item.hrentrada
                sPesoBruto = item.qtbruto
                sCdNivel1 = item.cdupnivel1
                sAnalise = item.noaanlise
            Next

            sEmpresa = cadparametros(("GERAIS", "DA_EMPRESA"))
            sDeNivel1 = cadparametros(("NIVEIS", "UPNIVEL1"))

            Dim enc850 As System.Text.Encoding = System.Text.Encoding.GetEncoding(850)
            Dim buffer As New List(Of Byte)

            buffer.AddRange(New Byte() {&H1B, &H40})

            For i As Integer = 1 To vGlobal.vNumCopiasBoletimentr
                ' Cabeçalho centralizado e negrito
                buffer.AddRange(New Byte() {&H1B, &H61, 1}) ' Centralizar
                buffer.AddRange(New Byte() {&H1B, &H45, 1}) ' Negrito on
                buffer.AddRange(enc850.GetBytes($"*** {sEmpresa} ***{vbCrLf}"))
                buffer.AddRange(enc850.GetBytes($"Boleto de Patio{vbCrLf}"))
                buffer.AddRange(New Byte() {&H1B, &H45, 0}) ' Negrito off
                buffer.AddRange(New Byte() {&H1B, &H61, 0}) ' Alinhar esquerda
                buffer.AddRange(enc850.GetBytes("------------------------------------------" & vbCrLf))

                ' Corpo
                Dim texto As New StringBuilder
                texto.AppendLine($"{sDeNivel1} {sCdNivel1}")
                texto.AppendLine($"Camión: {sCaminhao}    Placa: {sPlaca}")
                texto.AppendLine($"Chofer: {sMotorista}")
                texto.AppendLine($"F/H Entr: {sEntrada}")
                texto.AppendLine($"Liberación: {sLiberacao}      Viaje: {sViagem.PadLeft(6, "0"c)}")
                texto.AppendLine()
                If Not String.IsNullOrEmpty(sCarreta) Then
                    texto.AppendLine($"Remolque: {sCarreta}     Placa: {sPlacaCarreta}")
                    texto.AppendLine()
                End If
                texto.AppendLine($"Peso Bruto: {sPesoBruto}")
                texto.AppendLine()

                buffer.AddRange(enc850.GetBytes(texto.ToString))

                ' Espaço antes do código de barras
                buffer.AddRange(enc850.GetBytes(vbCrLf & vbCrLf))

                ' Espaço extra e corte
                buffer.AddRange(enc850.GetBytes(vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf))
                buffer.AddRange(New Byte() {&H1B, &H69, 0}) ' Corte total '''  &H1D, &H56
            Next
            ' Enviar para a impressora
            fGravaLog("10. **** Inicio do envio da impressora", "LOG_TESTE")
            RawPrinterHelper.SendBytesToPrinter(vGlobal.vNomeImpressora, buffer.ToArray(), "Boleto_Patio")
            fGravaLog("11. **** Final do envio da impressora", "LOG_TESTE")

        Catch ex As Exception
            vMensangem = "Erro na impressão: " & ex.Message
            frmMensagem.Show()
        End Try
    End Sub

    Public Sub ImprimirBolEntr_Laredo_Old()
        Try
            Dim sEmpresa, sCaminhao, sCarreta, sPlaca, sPlacaCarreta, sCdFunc, sMotorista, sEntrada, sLiberacao, sViagem, sPesoBruto, sAnalise As String
            Dim sCdNivel1, sDeNivel1 As String

            Dim lista = Laredo.listaDados
            For Each item In lista
                sLiberacao = item.noliberacao
                sViagem = item.idviagem
                sCaminhao = item.cdequiptocm
                sCarreta = If(item.cdequiptoct = 0, Nothing, item.cdequiptoct)
                sCdFunc = item.cdoperador
                If cadequiptos.ContainsKey(sCarreta) Then sPlacaCarreta = cadequiptos(sCarreta).no_placa
                If cadequiptos.ContainsKey(sCaminhao) Then sPlaca = cadequiptos(sCaminhao).no_placa
                If cadfuncionarios.ContainsKey(sCdFunc) Then sMotorista = sCdFunc & "-" & cadfuncionarios(sCdFunc)
                sEntrada = item.hrentrada
                sPesoBruto = item.qtbruto
                sCdNivel1 = item.cdupnivel1
                sAnalise = item.noaanlise
            Next

            sEmpresa = cadparametros(("GERAIS", "DA_EMPRESA"))
            sDeNivel1 = cadparametros(("NIVEIS", "UPNIVEL1"))

            Dim buffer As New List(Of Byte)
            For i As Integer = 1 To vGlobal.vNumCopiasBoletimentr
                ' Cabeçalho centralizado e negrito
                buffer.AddRange(New Byte() {&H1B, &H61, 1}) ' Centralizar
                buffer.AddRange(New Byte() {&H1B, &H45, 1}) ' Negrito on
                buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes("*** " & sEmpresa & " ***" & vbCrLf))
                buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes("Boleto de Patio" & vbCrLf))
                buffer.AddRange(New Byte() {&H1B, &H45, 0}) ' Negrito off
                buffer.AddRange(New Byte() {&H1B, &H61, 0}) ' Alinhar esquerda
                buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes("------------------------------------------" & vbCrLf))

                ' Corpo
                Dim texto As String = ""
                texto &= sDeNivel1 & " " & sCdNivel1 & vbCrLf
                texto &= "Camión: " & sCaminhao & "    Placa: " & sPlaca & vbCrLf
                texto &= "Chofer: " & sMotorista & vbCrLf
                texto &= "F/H Entr: " & sEntrada & vbCrLf
                texto &= "Liberación: " & sLiberacao & "     Viaje: " & sViagem.PadLeft(6, "0"c) & vbCrLf & vbCrLf
                If Not String.IsNullOrEmpty(sCarreta) Then
                    texto &= "Remolque: " & sCarreta & "     Placa: " & sPlacaCarreta & vbCrLf & vbCrLf
                End If
                texto &= "Peso Bruto: " & sPesoBruto & vbCrLf & vbCrLf

                buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes(texto))

                ' Espaço antes do código de barras
                buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes(vbCrLf & vbCrLf))

                Select Case ModeloImprTermica
                    Case "MP-4200HS" : buffer.AddRange(MontaCodigoBarrasCODE128_MP4200HS(sLiberacao))
                    Case "LR2000" : buffer.AddRange(MontaCodigoBarrasCODE128_LR2000(sLiberacao))
                End Select

                ' Espaço extra e corte
                buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes(vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf))
                buffer.AddRange(New Byte() {&H1D, &H56, 0}) ' Corte total
            Next

            If vGlobal.vImprBolAnl = "S" And frmReimpressao.bBotaoImprimir = False Then
                For i As Integer = 1 To vGlobal.vNumCopiasLaboratCana
                    ' Cabeçalho centralizado e negrito
                    buffer.AddRange(New Byte() {&H1B, &H61, 1}) ' ESC a 1 = centralizar
                    buffer.AddRange(New Byte() {&H1B, &H45, 1}) ' ESC E = negrito on
                    buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes("*** " & sEmpresa & " ***" & vbCrLf))
                    buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes("Ticket de Laboratorio" & vbCrLf))
                    buffer.AddRange(New Byte() {&H1B, &H45, 0}) ' ESC E = negrito off
                    buffer.AddRange(New Byte() {&H1B, &H61, 0}) ' ESC a 0 = alinhar esquerda

                    buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes("------------------------------------------" & vbCrLf))

                    ' Corpo
                    Dim corpo As String = ""
                    corpo &= sDeNivel1 & ": " & sCdNivel1 & vbCrLf
                    corpo &= "Camion: " & sCaminhao & "  Placa: " & sPlaca & vbCrLf
                    corpo &= "Chofer: " & sMotorista & vbCrLf
                    corpo &= "Fecha/Hora Entrada: " & sEntrada & vbCrLf
                    corpo &= "Liberacion: " & sLiberacao & "  N. Viaje: " & sViagem & vbCrLf
                    If Not String.IsNullOrEmpty(sCarreta) Then
                        corpo &= "Remolque: " & sCarreta & "  Placa: " & sPlacaCarreta & vbCrLf
                    End If
                    corpo &= "Peso Bruto: " & sPesoBruto & vbCrLf
                    corpo &= "------------------------------------------" & vbCrLf & vbCrLf & vbCrLf & vbCrLf
                    corpo &= "__________             __________" & vbCrLf
                    corpo &= "Patio                  Laboratorio" & vbCrLf & vbCrLf

                    buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes(corpo))

                    ' Espaço extra antes do código de barras
                    buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes(vbCrLf & vbCrLf))

                    Select Case ModeloImprTermica
                        Case "MP-4200HS" : buffer.AddRange(MontaCodigoBarrasCODE128_MP4200HS(sAnalise))
                        Case "LR2000" : buffer.AddRange(MontaCodigoBarrasCODE128_LR2000(sAnalise))
                    End Select

                    ' Espaço extra e corte
                    buffer.AddRange(System.Text.Encoding.GetEncoding(850).GetBytes(vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf))
                    buffer.AddRange(New Byte() {&H1B, &H69, 0}) ' Corte total
                Next
            End If
            ' Enviar para a impressora
            RawPrinterHelper.SendBytesToPrinter(vGlobal.vNomeImpressora, buffer.ToArray())

        Catch ex As Exception
            vMensangem = "Erro na impressão: " & ex.Message
            frmMensagem.Show()
        End Try
    End Sub

    Public WithEvents PrintPesagProdutos_Laredo As New PrintDocument
    Private Sub PrintPesagProdutos_Laredo_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintPesagProdutos_Laredo.PrintPage
        Try
            Dim noPesagem, _noPesage, noOrdemCarga, cdTransacao, cdTipoPesagem, cdTipoPesagemCod, cdProduto, cdOrigem, cdDestino, cdTransportadora, cdProvedor, cdCaminhao As String
            Dim cdCondutor, cdReboque1, cdReboque2, cdReboque3, qtPesoLiquido, qtProduto, qtPesoEmbalagem, qtPesoLiquidoSemEmbalagem As String
            Dim dtHoraEntrada, dtHoraSaida, cdUsuario, deObservacao, pesagem, fgPesagEs, strDescricao, noDocumento As String
            Dim cdNivel1_O, cdNivel2_O, cdNivel3_O, cdNivel1_D, cdNivel2_D, cdNivel3_D, DescNivel1, DescNivel2, DescNivel3 As String
            Dim cdRecursoCG, cdRecursoTR, cdOperCG, cdOperTR, cdVariedade, FgCadVeiculos As String
            Dim noPlacaCM, noPlacaRB1, noPlacaRB2, noPlacaRB3 As String
            Dim qtPeso1, qtPeso2, qtPesoTara, qtPesoBruto As Integer

            DescNivel1 = cadparametros(("NIVEIS", "UPNIVEL1"))
            DescNivel2 = cadparametros(("NIVEIS", "UPNIVEL2"))
            DescNivel3 = cadparametros(("NIVEIS", "UPNIVEL3"))
            FgCadVeiculos = cadparametros(("SPA_OUTR", "FG_CAD_VEICULO"))

            Select Case True
                Case vGlobal.bBotaoImprimir
                    DicRecursosPrd = FDicRecursosPrd(vGlobal.nRegistroImpressao)
                    DicNiveisPrd = FDicNiveisPrd(vGlobal.nRegistroImpressao)
                Case Else
                    DicRecursosPrd = FDicRecursosPrd(vGlobal.gn20)
                    DicNiveisPrd = FDicNiveisPrd(vGlobal.gn20)
            End Select

            Dim sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center

            Dim sql As New StringBuilder
            sql.Clear()
            sql.AppendLine("select o.instancia,lpad(o.no_pesagem,10,0) no_pesagem, o.no_pesagem pesagem,")
            sql.AppendLine("       o.hr_entrada,o.hr_saida,o.fg_pesag_es,nvl(op.cd_varied,0) cd_varied,")
            sql.AppendLine("       tm.cd_tp_mov || '-' || tm.de_tp_mov tipo_transacao,o.fg_tp_pesag fg_tp_pesag_cod,")
            sql.AppendLine("       case")
            sql.AppendLine("         when o.fg_tp_pesag = 'P' then")
            sql.AppendLine("          'P - Prod.Acabados'")
            sql.AppendLine("         when o.fg_tp_pesag = 'A' then")
            sql.AppendLine("          'A - Açúcar'")
            sql.AppendLine("         when o.fg_tp_pesag = 'T' then")
            sql.AppendLine("          'T - Insumos-Resíduos'")
            sql.AppendLine("         when o.fg_tp_pesag = 'M' then")
            sql.AppendLine("          'M - Mudas de Cana'")
            sql.AppendLine("         when o.fg_tp_pesag = 'O' then")
            sql.AppendLine("          'O - Outros'")
            sql.AppendLine("       end fg_tp_pesagem,")
            sql.AppendLine("       o.no_documento,")
            sql.AppendLine("       nvl(op.cd_origem,0) cd_origem,")
            sql.AppendLine("       nvl(op.cd_destino,0) cd_destino,")
            sql.AppendLine("       nvl(o.cd_fornec,0) cd_fornec,")
            sql.AppendLine("       nvl(o.cd_transport,0) cd_transport,")
            sql.AppendLine("       o.caminhao,o.motorista,")
            sql.AppendLine("       nvl(o.reboque,0) reboque_1,")
            sql.AppendLine("       nvl(o.reboque_2,0) reboque_2,")
            sql.AppendLine("       nvl(o.reboque_3,0) reboque_3,")
            sql.AppendLine("       o.cd_produto || '-' || p.de_produto produto,")
            sql.AppendLine("       nvl(op.qt_produto,0) qt_produto,o.qt_peso1,o.qt_peso2,o.qt_pesoliq,")
            sql.AppendLine("       o.qt_peso_emb,nvl(o.no_ordem_car,' ') no_ordem_car,nvl(o.de_observacao,' ') observacao,o.cd_usuario")
            sql.AppendLine("  from outr_pesagens o")
            sql.AppendLine(" inner join outr_pesagens_prd op")
            sql.AppendLine("    on o.instancia = op.instancia")
            sql.AppendLine("   and o.no_pesagem = op.no_pesagem")
            sql.AppendLine(" inner join cad_produto p")
            sql.AppendLine("    on p.cd_produto = op.cd_produto")
            sql.AppendLine(" inner join tipo_movimento tm")
            sql.AppendLine("    on tm.cd_tp_mov = o.cd_tp_mov")
            sql.AppendLine(" where o.instancia = :instancia")
            sql.AppendLine("   and o.fg_status = :fg_status")
            sql.AppendLine("  and o.no_pesagem = :no_pesagem")
            sql.AppendLine("   and rownum = 1")

            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("fg_status", OracleDbType.Varchar2).Value = "C"
                If vGlobal.bBotaoImprimir Then
                    cmd.Parameters.Add("no_pesagem", OracleDbType.Int64).Value = vGlobal.nRegistroImpressao
                Else
                    cmd.Parameters.Add("no_pesagem", OracleDbType.Int64).Value = vGlobal.gn20
                End If

                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        noPesagem = vGlobal.gn20
                        _noPesage = rs("no_pesagem")
                        If Not IsDBNull(rs("no_ordem_car")) Then
                            noOrdemCarga = rs("no_ordem_car")
                        Else
                            noOrdemCarga = " "
                        End If
                        If Not IsDBNull(rs("tipo_transacao")) Then
                            cdTransacao = rs("tipo_transacao")
                        Else
                            cdTransacao = " "
                        End If
                        If Not IsDBNull(rs("fg_tp_pesagem")) Then
                            cdTipoPesagem = rs("fg_tp_pesagem")
                        Else
                            cdTipoPesagem = " "
                        End If
                        If Not IsDBNull(rs("fg_tp_pesag_cod")) Then
                            cdTipoPesagemCod = rs("fg_tp_pesag_cod")
                        Else
                            cdTipoPesagemCod = " "
                        End If
                        If Not IsDBNull(rs("produto")) Then
                            cdProduto = rs("produto")
                        Else
                            cdProduto = " "
                        End If
                        If Not IsDBNull(rs("qt_produto")) Then
                            qtProduto = rs("qt_produto")
                        Else
                            qtProduto = "0"
                        End If
                        If Not IsDBNull(rs("no_documento")) Then
                            noDocumento = rs("no_documento")
                        Else
                            noDocumento = " "
                        End If
                        If Not IsDBNull(rs("cd_origem")) Then
                            If caddepositoprod.ContainsKey(rs("cd_origem")) Then
                                cdOrigem = rs("cd_origem") & " - " & caddepositoprod(rs("cd_origem")).de_deposito
                            End If
                        Else
                            cdOrigem = " "
                        End If
                        If Not IsDBNull(rs("cd_destino")) Then
                            If caddestinatario.ContainsKey(rs("cd_destino")) Then
                                cdDestino = rs("cd_destino") & " - " & caddestinatario(rs("cd_destino"))
                            End If
                        Else
                            cdDestino = " "
                        End If
                        If Not IsDBNull(rs("cd_transport")) Then
                            If cadtransportadora.ContainsKey(rs("cd_transport")) Then
                                cdTransportadora = rs("cd_transport") & " - " & cadtransportadora(rs("cd_transport"))
                            End If
                        Else
                            cdTransportadora = " "
                        End If
                        If Not IsDBNull(rs("cd_fornec")) Then
                            If cadfornectransp.ContainsKey(rs("cd_fornec")) Then
                                cdProvedor = rs("cd_fornec") & " - " & cadfornectransp(rs("cd_fornec")).de_fornec
                            End If
                        Else
                            cdProvedor = " "
                        End If
                        If Not IsDBNull(rs("caminhao")) Then
                            cdCaminhao = rs("caminhao")
                        Else
                            cdCaminhao = " "
                        End If
                        If Not IsDBNull(rs("motorista")) Then
                            cdCondutor = rs("motorista")
                        Else
                            cdCondutor = " "
                        End If
                        If Not IsDBNull(rs("reboque_1")) Then
                            cdReboque1 = rs("reboque_1")
                        Else
                            cdReboque1 = " "
                        End If
                        If Not IsDBNull(rs("reboque_2")) Then
                            cdReboque2 = rs("reboque_2")
                        Else
                            cdReboque2 = " "
                        End If
                        If Not IsDBNull(rs("reboque_3")) Then
                            cdReboque3 = rs("reboque_3")
                        Else
                            cdReboque3 = " "
                        End If
                        If Not IsDBNull(rs("qt_peso1")) Then
                            qtPeso1 = rs("qt_peso1")
                        Else
                            qtPeso1 = "0"
                        End If
                        If Not IsDBNull(rs("qt_peso2")) Then
                            qtPeso2 = rs("qt_peso2")
                        Else
                            qtPeso2 = "0"
                        End If
                        If Not IsDBNull(rs("qt_pesoliq")) Then
                            qtPesoLiquido = rs("qt_pesoliq")
                        Else
                            qtPesoLiquido = "0"
                        End If
                        If Not IsDBNull(rs("qt_peso_emb")) Then
                            qtPesoEmbalagem = rs("qt_peso_emb")
                        Else
                            qtPesoEmbalagem = "0"
                        End If

                        qtPesoLiquidoSemEmbalagem = (qtPesoLiquido - qtPesoEmbalagem)

                        If Not IsDBNull(rs("hr_entrada")) Then
                            dtHoraEntrada = rs("hr_entrada")
                        Else
                            dtHoraEntrada = " "
                        End If
                        If Not IsDBNull(rs("hr_saida")) Then
                            dtHoraSaida = rs("hr_saida")
                        Else
                            dtHoraSaida = " "
                        End If
                        If Not IsDBNull(rs("cd_usuario")) Then
                            cdUsuario = rs("cd_usuario")
                        Else
                            cdUsuario = " "
                        End If
                        If Not IsDBNull(rs("observacao")) Then
                            deObservacao = rs("observacao")
                        Else
                            deObservacao = " "
                        End If
                        If Not IsDBNull(rs("pesagem")) Then
                            pesagem = rs("pesagem")
                        Else
                            pesagem = " "
                        End If
                        If Not IsDBNull(rs("fg_pesag_es")) Then
                            fgPesagEs = rs("fg_pesag_es")
                        Else
                            fgPesagEs = " "
                        End If

                        If cdTipoPesagemCod = "M" Then
                            If DicNiveisPrd.Any Then
                                cdNivel1_O = DicNiveisPrd((vInstLogin, _noPesage)).nivel1_ori
                                cdNivel2_O = DicNiveisPrd((vInstLogin, _noPesage)).nivel2_ori
                                cdNivel3_O = DicNiveisPrd((vInstLogin, _noPesage)).nivel3_ori
                                cdNivel1_D = DicNiveisPrd((vInstLogin, _noPesage)).nivel1_des
                                cdNivel2_D = DicNiveisPrd((vInstLogin, _noPesage)).nivel2_des
                                cdNivel3_D = DicNiveisPrd((vInstLogin, _noPesage)).nivel3_des
                            End If
                            If DicRecursosPrd.Any Then
                                cdRecursoCG = DicRecursosPrd.Where(Function(x) x.Key.Item2 = _noPesage AndAlso
                                                         x.Value.cdtprecurso = "CG").
                                                         Select(Function(y) y.Value.cdequipto).FirstOrDefault()
                                cdRecursoTR = DicRecursosPrd.Where(Function(x) x.Key.Item2 = _noPesage AndAlso
                                                             x.Value.cdtprecurso = "TR").
                                                             Select(Function(y) y.Value.cdequipto).FirstOrDefault()
                                cdOperCG = DicRecursosPrd.Where(Function(x) x.Key.Item2 = _noPesage AndAlso
                                                             x.Value.cdtprecurso = "CG").
                                                             Select(Function(y) y.Value.cdoperador).FirstOrDefault()
                                cdOperTR = DicRecursosPrd.Where(Function(x) x.Key.Item2 = _noPesage AndAlso
                                                             x.Value.cdtprecurso = "TR").
                                                             Select(Function(y) y.Value.cdoperador).FirstOrDefault()
                                cdCaminhao = DicRecursosPrd.Where(Function(x) x.Key.Item2 = _noPesage AndAlso
                                                             x.Value.cdtprecurso = "CM").
                                                             Select(Function(y) y.Value.cdequipto).FirstOrDefault()
                                cdCondutor = DicRecursosPrd.Where(Function(x) x.Key.Item2 = _noPesage AndAlso
                                                             x.Value.cdtprecurso = "CM").
                                                             Select(Function(y) y.Value.cdoperador).FirstOrDefault()
                            End If

                            cdVariedade = rs("cd_varied")
                        End If
                    End While
                End Using
            End Using

            nLoopLinhas = 0
            vGlobal.sNomeEmpresa = cadparametros(("GERAIS", "DA_EMPRESA"))

            If FgCadVeiculos = "S" Then
                If Not IsDBNull(cdCaminhao) And cdCaminhao <> " " Then
                    If cadveiculos.ContainsKey(cdCaminhao) Then
                        noPlacaCM = cadveiculos(cdCaminhao).no_placa
                    End If
                End If
                If Not IsDBNull(cdReboque1) And cdReboque1 <> " " Then
                    If cadveiculos.ContainsKey(cdReboque1) Then
                        noPlacaRB1 = cadveiculos(cdReboque1).no_placa
                    End If
                End If
                If Not IsDBNull(cdReboque2) And cdReboque2 <> " " Then
                    If cadveiculos.ContainsKey(cdReboque2) Then
                        noPlacaRB2 = cadveiculos(cdReboque2).no_placa
                    End If
                End If
                If Not IsDBNull(cdReboque3) And cdReboque3 <> " " Then
                    If cadveiculos.ContainsKey(cdReboque3) Then
                        noPlacaRB3 = cadveiculos(cdReboque3).no_placa
                    End If
                End If
            ElseIf FgCadVeiculos = "N" Then
                Dim sTabelaPesquisa As String = Nothing
                Dim sColunaPesquisa As String = Nothing
                If Not IsDBNull(cdCaminhao) And cdCaminhao <> " " Then
                    If cdTipoPesagemCod = "M" Then
                        If cadequiptos.ContainsKey(cdCaminhao) Then
                            noPlacaCM = cadequiptos(cdCaminhao).no_placa
                        End If
                    Else
                        If cadplacaequiptos.ContainsKey(cdCaminhao) Then
                            noPlacaCM = cadplacaequiptos(cdCaminhao).no_placa
                            cdCaminhao = cadplacaequiptos(cdCaminhao).cd_equipto
                        End If
                    End If
                End If
                If Not IsDBNull(cdReboque1) And cdReboque1 <> " " Then
                    If IsNumeric(cdReboque1) = True Then
                        If cadequiptos.ContainsKey(cdReboque1) Then
                            noPlacaRB1 = cadequiptos(cdReboque1).no_placa
                        End If
                    Else
                        If cadplacaequiptos.ContainsKey(cdReboque1) Then
                            noPlacaRB1 = cadplacaequiptos(cdReboque1).no_placa
                        End If
                    End If
                End If
                If Not IsDBNull(cdReboque2) And cdReboque2 <> " " Then
                    If IsNumeric(cdReboque2) = True Then
                        If cadequiptos.ContainsKey(cdReboque2) Then
                            noPlacaRB2 = cadequiptos(cdReboque2).no_placa
                        End If
                    Else
                        If cadplacaequiptos.ContainsKey(cdReboque2) Then
                            noPlacaRB2 = cadplacaequiptos(cdReboque2).no_placa
                        End If
                    End If
                End If
                If Not IsDBNull(cdReboque3) And cdReboque3 <> " " Then
                    If IsNumeric(cdReboque3) = True Then
                        If cadequiptos.ContainsKey(cdReboque3) Then
                            noPlacaRB3 = cadequiptos(cdReboque3).no_placa
                        End If
                    Else
                        If cadplacaequiptos.ContainsKey(cdReboque3) Then
                            noPlacaRB3 = cadplacaequiptos(cdReboque3).no_placa
                        End If
                    End If
                End If
            End If

            Dim texto As String = Nothing
            alturaLinha = 10
            Dim largura1 As Single = 270.0F
            Dim altura1 As Single = 30.0F
            Dim rect1 As New RectangleF(10, alturaLinha, largura1, altura1)
            e.Graphics.DrawString("*** " & vGlobal.sNomeEmpresa & " ***", FontesImpressao.Fonte10R, Brushes.Black, rect1, sf)

            alturaLinha += 30

            Dim largura2 As Single = 270.0F
            Dim altura2 As Single = 30.0F
            Dim rect2 As New RectangleF(2, alturaLinha, largura2, altura2)
            e.Graphics.DrawString("Extracto de Pesaje de Productos", FontesImpressao.Fonte10R, Brushes.Black, rect2, sf)
            alturaLinha += 30
            texto += "Numero Pesaje:   " & _noPesage & "" & vbCrLf
            texto += "Numero Orden Carga:  " & noOrdemCarga & "" & vbCrLf
            texto += "Tipo Movimentación:  " & cdTransacao & "" & vbCrLf
            texto += "Documento:  " & noDocumento & "" & vbCrLf
            texto += "Tipo de Pesaje:  " & cdTipoPesagem & "" & vbCrLf
            texto += "Producto:"
            texto += String.Concat(Enumerable.Repeat(" ", 13)) & "Cantidad: " & String.Format("{0:N2}", qtProduto) & vbCrLf
            texto += "" & FQuebraLinhasImpressao(cdProduto) & vbCrLf
            If cdTipoPesagemCod <> "M" Then
                texto += "Origen:  " & cdOrigem & vbCrLf
                texto += "Destino:  " & cdDestino & vbCrLf
            ElseIf cdTipoPesagemCod = "M" Then
                texto += "Variedad: " & cdVariedade & vbCrLf
                texto += "O.: " & DescNivel1 & cdNivel1_O
                texto += String.Concat(Enumerable.Repeat(" ", 3)) & "O.: " & DescNivel2 & cdNivel2_O
                texto += String.Concat(Enumerable.Repeat(" ", 3)) & "O.: " & DescNivel3 & cdNivel3_O & vbCrLf
                texto += "D.: " & DescNivel1 & cdNivel1_D
                texto += String.Concat(Enumerable.Repeat(" ", 3)) & "D.: " & DescNivel2 & cdNivel2_D
                texto += String.Concat(Enumerable.Repeat(" ", 3)) & "D.: " & DescNivel3 & cdNivel3_D & vbCrLf
                texto += "Carg.:" & cdRecursoCG
                texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Oper.:" & cdOperCG & vbCrLf
                texto += "Tract.:" & cdRecursoTR
                texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Oper.:" & cdOperTR & vbCrLf
            End If
            Dim DescTransportadora As String = "Transportadora: " & cdTransportadora & ""
            texto += FQuebraLinhasImpressao(DescTransportadora) & vbCrLf
            Dim DescProveedor As String = "Proveedor: " & cdProvedor & ""
            texto += FQuebraLinhasImpressao(DescProveedor) & vbCrLf
            texto += "Camion:  " & cdCaminhao
            texto += String.Concat(Enumerable.Repeat(" ", 10)) & "Placa:  " & noPlacaCM & vbCrLf
            Dim DescCondutor As String = $"Conductor:  {cdCondutor}"
            texto += FQuebraLinhasImpressao(DescCondutor) & vbCrLf

            Dim alturaLinha2 As Integer
            If cdTipoPesagemCod <> "M" Then
                alturaLinha2 += -50
                If cdReboque1 <> "0" Then
                    texto += "1.Reboque:  " & cdReboque1
                    texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa: " & noPlacaRB1 & vbCrLf
                    alturaLinha2 += 20
                End If
                If cdReboque2 <> "0" Then
                    texto += "2.Reboque:  " & cdReboque2
                    texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa:  " & noPlacaRB2 & vbCrLf
                    alturaLinha2 += 10
                End If
                If cdReboque3 <> "0" Then
                    texto += "3.Reboque:  " & cdReboque3
                    texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa: " & noPlacaRB3 & vbCrLf
                    alturaLinha2 += 10
                End If
            ElseIf cdTipoPesagemCod = "M" Then
                alturaLinha2 += -10
                If cdReboque1 <> "0" Then
                    texto += "1.Reboque:  " & cdReboque1
                    texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa: " & noPlacaRB1 & vbCrLf
                    alturaLinha2 += 20
                End If
                If cdReboque2 <> "0" Then
                    texto += "2.Reboque:  " & cdReboque2
                    texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa:  " & noPlacaRB2 & vbCrLf
                    alturaLinha2 += 10
                End If
                If cdReboque3 <> "0" Then
                    texto += "3.Reboque:  " & cdReboque3
                    texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa: " & noPlacaRB3 & vbCrLf
                    alturaLinha2 += 10
                End If
            End If

            e.Graphics.DrawString(texto, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)
            alturaLinha += 280 + alturaLinha2

            If fgPesagEs = "E" Then
                qtPesoTara = qtPeso2
                qtPesoBruto = qtPeso1
                strDescricao = "Entrada de Producto"
            ElseIf fgPesagEs = "S" Then
                qtPesoTara = qtPeso1
                qtPesoBruto = qtPeso2
                strDescricao = "Salida de Producto"
            End If

            Dim largura3 As Single = 270.0F
            Dim altura3 As Single = 30.0F
            Dim rect3 As New RectangleF(2, alturaLinha, largura3, altura3)
            e.Graphics.DrawString("*** " & strDescricao & " ***", FontesImpressao.Fonte10R, Brushes.Black, rect3, sf)
            alturaLinha += 30

            Dim texto2 As String = Nothing
            texto2 += "Peso Tara: " & qtPesoTara & vbCrLf
            texto2 += "Peso Bruto: " & qtPesoBruto & vbCrLf
            texto2 += "Peso Neto: " & qtPesoLiquido & vbCrLf
            texto2 += "Peso Emb: " & qtPesoEmbalagem & vbCrLf
            texto2 += "Peso Neto Sem Emb: " & qtPesoLiquidoSemEmbalagem & vbCrLf
            texto2 += "Fec/Hr Entrada: " & dtHoraEntrada & vbCrLf
            texto2 += "Fec/Hr Salida: " & dtHoraSaida & vbCrLf
            texto2 += "Usuario: " & cdUsuario & vbCrLf
            texto2 += "Observacion:" & vbCrLf
            texto2 += FQuebraLinhasImpressao(deObservacao) & vbCrLf & vbCrLf & vbCrLf
            texto2 += "."
            e.Graphics.DrawString(texto2, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)

        Catch ex As Exception
            Dim st As New StackTrace(ex, True)
            Dim frame As StackFrame = st.GetFrame(st.FrameCount - 1)
            Dim line As Integer = frame.GetFileLineNumber()
        End Try
    End Sub

    Public WithEvents PrintBolAnl_Laredo As New PrintDocument
    Private Sub PrintBolAnl_Laredo_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintBolAnl_Laredo.PrintPage
        Try
            Dim sEmpresa, sCaminhao, sCarreta, sPlaca, sPlacaCarreta, sMotorista, sEntrada, sLiberacao, sViagem, sPesoBruto, sAnalise As String
            Dim sCdNivel1, sDeNivel1 As String
            Dim alturaLinha As Integer
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000
            Dim alturaPatioLab As Integer = 260
            Dim alturaCodigoBarras As Integer = 325
            Dim barra As Image

            Dim lista = Laredo.listaDados
            For Each item In lista
                sLiberacao = item.noliberacao
                sViagem = item.idviagem
                sCaminhao = item.cdequiptocm
                If cadequiptos.ContainsKey(sCaminhao) Then
                    sPlaca = cadequiptos(sCaminhao).no_placa
                End If
                sCarreta = If(item.cdequiptoct = 0, Nothing, item.cdequiptoct)
                If cadequiptos.ContainsKey(sCarreta) Then
                    sPlacaCarreta = cadequiptos(sCarreta).no_placa
                End If
                If cadfuncionarios.ContainsKey(item.cdoperador) Then
                    sMotorista = cadfuncionarios(item.cdoperador)
                End If
                sEntrada = item.hrentrada
                sPesoBruto = item.qtbruto
                sAnalise = item.noaanlise
                sCdNivel1 = item.cdupnivel1
            Next
            sEmpresa = cadparametros(("GERAIS", "DA_EMPRESA"))
            sDeNivel1 = cadparametros(("NIVEIS", "UPNIVEL1"))

            'Linha 01
            Dim Point01 As New Point(2, 50)
            Dim Point11 As New Point(270, 50)
            Dim blackPen1 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen1, Point01, Point11)

            'Linha 02
            Dim Point02 As New Point(2, 75)
            Dim Point22 As New Point(270, 75)
            Dim blackPen2 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen2, Point02, Point22)

            'Linha 03
            Dim Point03 As New Point(2, 155)
            Dim Point33 As New Point(270, 155)
            Dim blackPen3 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen3, Point03, Point33)

            Dim sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center

            alturaLinha = 10
            Dim larguraRet1 As Single = 270.0F
            Dim alturaRet1 As Single = 30.0F
            Dim rect1 As New RectangleF(10, alturaLinha, larguraRet1, alturaRet1)

            alturaLinha += 40
            Dim larguraRet2 As Single = 270.0F
            Dim alturaRet2 As Single = 30.0F
            Dim rect2 As New RectangleF(10, alturaLinha, larguraRet2, alturaRet2)


            e.Graphics.DrawString("*** " & sEmpresa & " ***", FontesImpressao.Fonte10R, Brushes.Black, rect1, sf)

            e.Graphics.DrawString("Ticket de Laboratorio", FontesImpressao.Fonte10R, Brushes.Black, rect2, sf)

            Dim texto As String
            texto += sDeNivel1 & " " & sCdNivel1 & vbCrLf
            texto += "Camión:  " & sCaminhao & ""
            texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa:  " & sPlaca & vbCrLf
            texto += "Chofer:  " & sMotorista & vbCrLf
            texto += "Fecha/Hora Entrada:  " & sEntrada & vbCrLf
            texto += "Liberación: " & sLiberacao.PadLeft(8, "0"c)
            texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Nº Viaje: " & sViagem.PadLeft(8, "0"c) & vbCrLf & vbCrLf
            If sCarreta <> " " Then
                texto += "Remolque: " & sCarreta
                texto += String.Concat(Enumerable.Repeat(" ", 3)) & "Placa: " & sCarreta & vbCrLf & vbCrLf
                alturaPatioLab += 30
                alturaCodigoBarras += 30
            End If
            texto += "Peso Bruto: " & sPesoBruto & String.Concat(Enumerable.Repeat(vbCrLf, 7))
            texto += "Patio"
            texto += String.Concat(Enumerable.Repeat(" ", 45)) & "Laboratorio" & String.Concat(Enumerable.Repeat(vbCrLf, 9))
            texto += String.Concat(Enumerable.Repeat(" ", 20)) & sAnalise.PadLeft(6, "0"c) & vbCrLf
            texto += "."
            e.Graphics.DrawString(texto, FontesImpressao.Fonte9R, Brushes.Black, 2, 80)

            'Linha Patio
            Dim Point04 As New Point(2, alturaPatioLab)
            Dim Point44 As New Point(100, alturaPatioLab)
            Dim blackPen4 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen4, Point04, Point44)

            'Linha laboratorio
            Dim Point05 As New Point(180, alturaPatioLab)
            Dim Point55 As New Point(270, alturaPatioLab)
            Dim blackPen5 = New Pen(Color.Black, 1)
            e.Graphics.DrawLine(blackPen5, Point05, Point55)

            barra = codbarras.Draw(sAnalise, altura)
            e.Graphics.DrawImage(barra, New Rectangle(70, alturaCodigoBarras, 140, 70))

        Catch ex As Exception
            vMensangem = ex.ToString & vbNewLine
            frmMensagem.Show()
            vMensangem = Nothing
        End Try
    End Sub

    Public Sub ImprimirTicketLaboratorio_Laredo()
        Try
            ' 1. Coleta de Dados (mantém sua lógica)
            Dim sEmpresa, sCaminhao, sPlaca, sMotorista, sEntrada, sLiberacao, sViagem, sPesoBruto, sAnalise As String
            Dim sCdNivel1, sDeNivel1, sCarreta, sPlacaCarreta As String

            Dim lista = Laredo.listaDados
            For Each item In lista
                sLiberacao = item.noliberacao.ToString().PadLeft(8, "0"c)
                sViagem = item.idviagem.ToString().PadLeft(8, "0"c)
                sCaminhao = item.cdequiptocm
                If cadequiptos.ContainsKey(sCaminhao) Then sPlaca = FBuscaPlaca(sCaminhao, sLiberacao)
                sCarreta = item.cdequiptoct
                If cadequiptos.ContainsKey(sCarreta) Then sPlacaCarreta = FBuscaPlaca(sCarreta, sLiberacao)
                If cadfuncionarios.ContainsKey(item.cdoperador) Then sMotorista = cadfuncionarios(item.cdoperador)
                sEntrada = item.hrentrada
                sPesoBruto = item.qtbruto
                sAnalise = item.noaanlise
                sCdNivel1 = item.cdupnivel1
            Next

            sEmpresa = cadparametros(("GERAIS", "DA_EMPRESA"))
            sDeNivel1 = cadparametros(("NIVEIS", "UPNIVEL1"))

            Dim ec850 As System.Text.Encoding = System.Text.Encoding.GetEncoding(850)
            Dim buffer As New List(Of Byte)
            buffer.AddRange(New Byte() {&H1B, &H40})

            ' 2. Montagem do buffer ESC/POS
            For i As Integer = 1 To vGlobal.vNumCopiasLaboratCana
                ' Cabeçalho centralizado e negrito
                buffer.AddRange(New Byte() {&H1B, &H61, 1}) ' ESC a 1 = centralizar
                buffer.AddRange(New Byte() {&H1B, &H45, 1}) ' ESC E = negrito on
                buffer.AddRange(ec850.GetBytes($"*** {sEmpresa} ***{vbCrLf}"))
                buffer.AddRange(ec850.GetBytes($"Ticket de Laboratorio{vbCrLf}"))
                buffer.AddRange(New Byte() {&H1B, &H45, 0}) ' ESC E = negrito off
                buffer.AddRange(New Byte() {&H1B, &H61, 0}) ' ESC a 0 = alinhar esquerda

                buffer.AddRange(ec850.GetBytes($"------------------------------------------{vbCrLf}"))

                ' Corpo
                Dim corpo As New StringBuilder
                corpo.AppendLine($"{sDeNivel1}: {sCdNivel1}")
                corpo.AppendLine($"Camion: {sCaminhao}  Placa: {sPlaca}")
                corpo.AppendLine($"Chofer: {sMotorista}")
                corpo.AppendLine($"Fecha/Hora Entrada: {sEntrada}")
                corpo.AppendLine($"Liberacion: {sLiberacao}  N. Viaje: {sViagem}")
                If Not String.IsNullOrEmpty(sCarreta) Then
                    corpo.AppendLine($"Remolque: {sCarreta}  Placa: {sPlacaCarreta}")
                End If
                corpo.AppendLine($"Peso Bruto: {sPesoBruto}")
                corpo.AppendLine($"------------------------------------------{vbCrLf}{vbCrLf}{vbCrLf}")
                corpo.AppendLine($"__________             __________")
                corpo.AppendLine($"Patio                  Laboratorio{vbCrLf}")

                buffer.AddRange(ec850.GetBytes(corpo.ToString))

                ' Espaço extra antes do código de barras
                buffer.AddRange(ec850.GetBytes(vbCrLf & vbCrLf))

                Select Case ModeloImprTermica
                    Case "MP-4200HS" : buffer.AddRange(MontaCodigoBarrasCODE128_MP4200HS(sAnalise))
                    Case "LR2000" : buffer.AddRange(MontaCodigoBarrasCODE128_LR2000(sAnalise))
                End Select

                ' Espaço extra e corte
                buffer.AddRange(ec850.GetBytes(vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf))
                buffer.AddRange(New Byte() {&H1B, &H69, 0}) ' Corte total

            Next
            ' Enviar para a impressora pelo nome
            RawPrinterHelper.SendBytesToPrinter(vGlobal.vNomeImpressora, buffer.ToArray(), "Ticket_Laboratorio")
        Catch ex As Exception
            vMensangem = "Erro na Impressão Bematech: " & ex.Message
            frmMensagem.Show()
        End Try
    End Sub

    Public WithEvents PrintExtratoSaida_Laredo As New PrintDocument
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

            Dim textImpr As String

            Dim alturaLinha As Integer
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000
            Dim barra As Image
            Dim hrFechamento As String = cadparametros(("RCMP", "HR_FECHAMENTO")) & ":00"
            Dim DtHrIniPeriodo, DtHrFinalPeriodo As String

            vMensangem = Nothing
            vMensangem += "**PrintExtratoSaida_Laredo**"

            Dim sWhere As String
            Dim sFiltro As String
            If frmReimpressao.bBotaoImprimir = True Then
                sFiltro = frmReimpressao.tbBoletim.Text.Replace(";", ",")
                sWhere = " and c.hr_entrada = to_date('" & FBuscaHrEntrada(sFiltro) & "','dd/mm/yyyy hh24:mi:ss')"
                vMensangem += sWhere & vbNewLine
            ElseIf frmReimpressao.bBotaoImprimir = False Then
                sWhere = " and c.hr_entrada = to_date('" & hrEntradaCanaLaredo & "','dd/mm/yyyy hh24:mi:ss')"
                vMensangem += sWhere & vbNewLine
            End If

            Dim sql As String
            Dim rs As OracleDataReader
            sql += "select * from" & vbNewLine
            sql += "(select nvl(c.no_liberacao,0) no_liberacao," & vbNewLine
            sql += "       nvl(c.id_viagem,0) id_viagem," & vbNewLine
            sql += "       nvl(c.no_sequencia,0) no_sequencia," & vbNewLine
            sql += "       c.hr_entrada," & vbNewLine
            sql += "       c.hr_saida," & vbNewLine
            sql += "       nvl(t.cd_fornec,0) cd_fornec," & vbNewLine
            sql += "       nvl(t.de_fornec,0) de_fornec," & vbNewLine
            sql += "       nvl(c.cd_upnivel1,0) cd_upnivel1," & vbNewLine
            sql += "       nvl(u.de_upnivel1,0) de_upnivel1," & vbNewLine
            sql += "       nvl(r.cd_equipto,0) cd_equipto," & vbNewLine
            sql += "       nvl(r.cd_tp_recurso,0) cd_tp_recurso," & vbNewLine
            sql += "       nvl(c.qt_distancia,0) qt_distancia," & vbNewLine
            sql += "       nvl(c.qt_bruto,0) qt_bruto," & vbNewLine
            sql += "       nvl(c.qt_tara,0) qt_tara," & vbNewLine
            sql += "       nvl(c.qt_liquido,0) qt_liquido" & vbNewLine
            sql += "from apt_cargas c" & vbNewLine
            sql += "inner join apt_cargas_rec r on c.instancia = r.instancia and c.no_liberacao = r.no_liberacao" & vbNewLine
            sql += "inner join equiptos e on e.cd_equipto = r.cd_equipto" & vbNewLine
            sql += "inner join transporta p on p.cd_transp = e.cd_transp" & vbNewLine
            sql += "inner join upnivel1 u on u.cd_upnivel1 = c.cd_upnivel1" & vbNewLine
            sql += "inner join fornecs t on t.cd_fornec = u.cd_fornec" & vbNewLine
            sql += "where  c.instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine
            sql += "" & sWhere & vbNewLine
            sql += "  and  c.fg_status in ('C','P')" & vbNewLine
            sql += "  and  r.cd_tp_recurso in ('CM','CT'))" & vbNewLine
            sql += "pivot (" & vbNewLine
            sql += "      max(cd_equipto)" & vbNewLine
            sql += "  for cd_tp_recurso in ('CM' as CM,'CT' as CT))" & vbNewLine
            sql += "order by no_sequencia"

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                sNoSequencia = rs("no_sequencia")

                If sNoSequencia = 1 Then
                    sNoSequencia1 = rs("no_sequencia")
                    sLiberacao1 = rs("no_liberacao")
                    If Not IsDBNull(rs("id_viagem")) Then
                        sViagem = rs("id_viagem")
                    Else
                        sViagem = " "
                    End If
                    sCaminhao = rs("CM")
                    'sPlaca = FBuscaPlaca(sCaminhao, sLiberacao1)
                    'sPlaca = cadequiptos(sCaminhao).no_placa
                    sCdFunc = FBuscaMotorista(sCaminhao, sLiberacao1, "D")
                    nTamMotorista = sCdFunc.Length
                    If nTamMotorista > 28 Then
                        sCdFunc = sCdFunc.Substring(0, 28)
                    End If
                    sMotorista = FBuscaMotorista(sCaminhao, sLiberacao1, "C")
                    sEntrada = rs("hr_entrada")
                    HrInicial = Convert.ToDateTime(sEntrada)
                    DtHrIniPeriodo = HrInicial.ToString("dd/MM/yyyy") & " " & hrFechamento
                    HrFinal = Convert.ToDateTime(DtHrIniPeriodo)
                    HrFinal = HrFinal.AddHours(24).AddSeconds(-1)
                    DtHrFinalPeriodo = HrFinal.ToString("dd/MM/yyyy HH:mm:ss")
                    sSaida = rs("hr_saida")
                    sPesoBruto1 = rs("qt_bruto")
                    sPesoTara1 = rs("qt_tara")
                    sPesoNeto1 = rs("qt_liquido")
                    sCdNivel1_1 = rs("cd_upnivel1") & " " & cadupnivel1(rs("cd_upnivel1")).de_upnivel1
                    nTamNivel1_1 = sCdNivel1_1.Length
                    If nTamNivel1_1 > 33 Then
                        sCdNivel1_1 = sCdNivel1_1.Substring(0, 33)
                    End If
                    sCdFornec1 = cadupnivel1(rs("cd_upnivel1")).de_fornec
                    nTamForn1_1 = sCdFornec1.Length
                    If nTamForn1_1 > 28 Then
                        sCdFornec1 = sCdFornec1.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 2 Then
                    sNoSequencia2 = rs("no_sequencia")
                    sLiberacao2 = rs("no_liberacao")
                    If IsDBNull(rs("CT")) Then
                        sCarreta2 = FBuscaCarretaLiberacao(sLiberacao2)
                    Else
                        sCarreta2 = rs("CT")
                    End If
                    sPesoBruto2 = rs("qt_bruto")
                    sPesoTara2 = rs("qt_tara")
                    sPesoNeto2 = rs("qt_liquido")
                    sCdNivel1_2 = rs("cd_upnivel1") & " " & cadupnivel1(rs("cd_upnivel1")).de_upnivel1
                    nTamNivel1_2 = sCdNivel1_2.Length
                    If nTamNivel1_2 > 33 Then
                        sCdNivel1_2 = sCdNivel1_2.Substring(0, 33)
                    End If
                    sCdFornec2 = cadupnivel1(rs("cd_upnivel1")).de_fornec
                    nTamForn1_2 = sCdFornec2.Length
                    If nTamForn1_2 > 28 Then
                        sCdFornec2 = sCdFornec2.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 3 Then
                    sNoSequencia3 = rs("no_sequencia")
                    sLiberacao3 = rs("no_liberacao")
                    If IsDBNull(rs("CT")) Then
                        sCarreta3 = FBuscaCarretaLiberacao(sLiberacao3)
                    Else
                        sCarreta3 = rs("CT")
                    End If
                    sPesoBruto3 = rs("qt_bruto")
                    sPesoTara3 = rs("qt_tara")
                    sPesoNeto3 = rs("qt_liquido")
                    sCdNivel1_3 = rs("cd_upnivel1") & " " & cadupnivel1(rs("cd_upnivel1")).de_upnivel1
                    nTamNivel1_3 = sCdNivel1_3.Length
                    If nTamNivel1_3 > 33 Then
                        sCdNivel1_3 = sCdNivel1_3.Substring(0, 33)
                    End If
                    sCdFornec3 = cadupnivel1(rs("cd_upnivel1")).de_fornec
                    nTamForn1_3 = sCdFornec3.Length
                    If nTamForn1_3 > 28 Then
                        sCdFornec3 = sCdFornec3.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 4 Then
                    sNoSequencia4 = rs("no_sequencia")
                    sLiberacao4 = rs("no_liberacao")
                    If IsDBNull(rs("CT")) Then
                        sCarreta4 = FBuscaCarretaLiberacao(sLiberacao4)
                    Else
                        sCarreta4 = rs("CT")
                    End If
                    sPesoBruto4 = rs("qt_bruto")
                    sPesoTara4 = rs("qt_tara")
                    sPesoNeto4 = rs("qt_liquido")
                    sCdNivel1_4 = rs("cd_upnivel1") & " " & cadupnivel1(rs("cd_upnivel1")).de_upnivel1
                    nTamNivel1_4 = sCdNivel1_4.Length
                    If nTamNivel1_4 > 33 Then
                        sCdNivel1_4 = sCdNivel1_4.Substring(0, 33)
                    End If
                    sCdFornec4 = cadupnivel1(rs("cd_upnivel1")).de_fornec
                    nTamForn1_4 = sCdFornec4.Length
                    If nTamForn1_4 > 28 Then
                        sCdFornec4 = sCdFornec4.Substring(0, 28)
                    End If
                End If
            End While
            fCmd(sql).Dispose()
            rs.Close()

            For i = 1 To 4
                If i = 1 And sNoSequencia1 = 1 Then
                    sEmpresa = cadparametros(("GERAIS", "DA_EMPRESA"))
                    sDeNivel1 = cadparametros(("NIVEIS", "UPNIVEL1"))

                    Dim sf As New StringFormat()
                    sf.Alignment = StringAlignment.Center
                    sf.LineAlignment = StringAlignment.Center

                    Dim largura1 As Single = 270.0F
                    Dim altura1 As Single = 30.0F
                    Dim rect1 As New RectangleF(2.0F, 2.0F, largura1, altura1)
                    e.Graphics.DrawString("*** " & sEmpresa & " ***", FontesImpressao.Fonte10R, Brushes.Black, rect1, sf)

                    alturaLinha = 50

                    'Linha 01
                    Dim Point01 As New Point(2, alturaLinha)
                    Dim Point11 As New Point(270, alturaLinha)
                    Dim blackPen1 = New Pen(Color.Black, 1)
                    e.Graphics.DrawLine(blackPen1, Point01, Point11)

                    Dim largura2 As Single = 270.0F
                    Dim altura2 As Single = 20.0F
                    Dim rect2 As New RectangleF(2.0F, 50.0F, largura2, altura2)
                    e.Graphics.DrawString("Extracto de Pesaje de Caña", FontesImpressao.Fonte10R, Brushes.Black, rect2, sf)

                    'Linha 02
                    Dim Point02 As New Point(2, alturaLinha + 16)
                    Dim Point22 As New Point(270, alturaLinha + 16)
                    Dim blackPen2 = New Pen(Color.Black, 1)
                    e.Graphics.DrawLine(blackPen2, Point02, Point22)

                    alturaLinha += 20


                    'Linha 03
                    Dim Point03 As New Point(2, alturaLinha + 85)
                    Dim Point33 As New Point(270, alturaLinha + 85)
                    Dim blackPen3 = New Pen(Color.Black, 1)
                    e.Graphics.DrawLine(blackPen3, Point03, Point33)

                    textImpr += "Nombre: " & sCdFunc & vbCrLf
                    textImpr += "Camión: " & sCaminhao.PadLeft(8, " ") & "Chofer: " & sMotorista.PadLeft(10, " ") & vbCrLf
                    textImpr += "Placa: " & sPlaca & vbCrLf
                    textImpr += "Entrada: " & sEntrada & vbCrLf
                    textImpr += "Salida: " & sSaida & vbCrLf
                    textImpr += "Nº Viaje: " & sViagem.PadLeft(6, "0"c) & vbCrLf & vbCrLf
                    textImpr += "Proveedor: " & sCdFornec1 & vbCrLf
                    textImpr += sDeNivel1 & " " & sCdNivel1_1 & vbCrLf
                    textImpr += "Liberación Equipo     Bruto      Tara       Neto" & vbCrLf
                    textImpr += sLiberacao1.PadLeft(8, "0") & "   " & sCaminhao.PadLeft(8, " ") & " " & sPesoBruto1.PadLeft(6, " ") & " " & sPesoTara1.PadLeft(6, " ") & " " & sPesoNeto1.PadLeft(6, " ") & vbCrLf & vbCrLf

                ElseIf i = 2 And sNoSequencia2 = 2 Then
                    textImpr += "Proveedor:  " & sCdFornec2 & vbCrLf
                    textImpr += sDeNivel1 & " " & sCdNivel1_2 & vbCrLf
                    textImpr += "Liberación Equipo     Bruto      Tara       Neto" & vbCrLf
                    textImpr += sLiberacao2.PadLeft(8, "0") & " " & sCarreta2.PadLeft(10, " ") & " " & sPesoBruto2.PadLeft(6, " ") & " " & sPesoTara2.PadLeft(6, " ") & " " & sPesoNeto2.PadLeft(6, " ") & vbCrLf & vbCrLf
                ElseIf i = 3 And sNoSequencia3 = 3 Then
                    textImpr += "Proveedor:  " & sCdFornec3 & vbCrLf
                    textImpr += sDeNivel1 & " " & sCdNivel1_3 & vbCrLf
                    textImpr += "Liberación Equipo     Bruto      Tara       Neto" & vbCrLf
                    textImpr += sLiberacao3.PadLeft(8, "0") & " " & sCarreta3.PadLeft(10, " ") & " " & sPesoBruto3.PadLeft(6, " ") & " " & sPesoTara3.PadLeft(6, " ") & " " & sPesoNeto3.PadLeft(6, " ") & vbCrLf & vbCrLf
                ElseIf i = 4 And sNoSequencia4 = 4 Then
                    textImpr += "Proveedor:  " & sCdFornec4 & vbCrLf
                    textImpr += sDeNivel1 & " " & sCdNivel1_4 & vbCrLf
                    textImpr += "Liberación Equipo     Bruto      Tara       Neto" & vbCrLf
                    textImpr += sLiberacao4.PadLeft(8, "0") & " " & sCarreta4.PadLeft(10, " ") & " " & sPesoBruto4.PadLeft(6, " ") & " " & sPesoTara4.PadLeft(6, " ") & " " & sPesoNeto4.PadLeft(6, " ") & vbCrLf & vbCrLf
                End If
            Next

            textImpr += "Peso Bruto: " & CDbl(sPesoBruto1) + CDbl(sPesoBruto2) + CDbl(sPesoBruto3) + CDbl(sPesoBruto4) & vbCrLf
            textImpr += "Peso Tara.: " & CDbl(sPesoTara1) + CDbl(sPesoTara2) + CDbl(sPesoTara3) + CDbl(sPesoTara4) & vbCrLf
            textImpr += "Peso Neto.: " & CDbl(sPesoNeto1) + CDbl(sPesoNeto2) + CDbl(sPesoNeto3) + CDbl(sPesoNeto4) & vbCrLf & vbCrLf
            textImpr += "Total Transp: " & FTotalEquiptoExtSaida(sMotorista, DtHrIniPeriodo, DtHrFinalPeriodo) & vbCrLf
            textImpr += "."

            e.Graphics.DrawString(textImpr, FontesImpressao.Fonte9R, Brushes.Black, 2, alturaLinha)

            'Linha 04
            Dim Point04 As New Point(2, alturaLinha + 215)
            Dim Point44 As New Point(270, alturaLinha + 215)
            Dim blackPen4 = New Pen(Color.Black, 1)
            'e.Graphics.DrawLine(blackPen4, Point04, Point44)

            'Linha 05
            Dim Point05 As New Point(2, alturaLinha + 230)
            Dim Point55 As New Point(270, alturaLinha + 230)
            Dim blackPen5 = New Pen(Color.Black, 1)
            'e.Graphics.DrawLine(blackPen5, Point05, Point55)

        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            frmMensagem.Show()
            vMensangem = Nothing
        End Try
    End Sub

    'Novo metodo de impressão da saida Laredo
    Public Sub ImprimirSaidaLaredo(list As List(Of ImprBolSaida))
        Try
            Dim sViagem, sNoSequencia, sMotorista, sCdFunc As String

            Dim HrInicial, HrFinal As DateTime

            Dim nTamMotorista As Integer
            Dim nTamNivel1_1, nTamNivel1_2, nTamNivel1_3, nTamNivel1_4 As Integer
            Dim nTamForn1_1, nTamForn1_2, nTamForn1_3, nTamForn1_4 As Integer

            'String primeira linha
            Dim sNoSequencia1, sCaminhao, sPlaca, sEntrada, sSaida, sCdFornec1, sCdNivel1_1, sLiberacao1, sPesoBruto1, sPesoTara1, sPesoNeto1 As String

            'String segunda linha
            Dim sNoSequencia2, sCarreta2, sCdFornec2, sCdNivel1_2, sLiberacao2, sPesoBruto2, sPesoTara2, sPesoNeto2 As String

            'String terceira linha
            Dim sNoSequencia3, sCarreta3, sCdFornec3, sCdNivel1_3, sLiberacao3, sPesoBruto3, sPesoTara3, sPesoNeto3 As String

            'String quarta linha
            Dim sNoSequencia4, sCarreta4, sCdFornec4, sCdNivel1_4, sLiberacao4, sPesoBruto4, sPesoTara4, sPesoNeto4 As String

            Dim sCdOperador As Integer

            Dim textImpr As String

            Dim alturaLinha As Integer
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000
            Dim barra As Image
            Dim hrFechamento As String = cadparametros(("RCMP", "HR_FECHAMENTO")) & ":00"
            Dim DtHrIniPeriodo, DtHrFinalPeriodo As String
            For Each item In list
                sNoSequencia = item.no_sequencia

                If sNoSequencia = 1 Then
                    sNoSequencia1 = item.no_sequencia
                    sLiberacao1 = item.no_liberacao
                    sViagem = If(IsDBNull(item.id_viagem), " ", item.id_viagem.ToString())

                    sCaminhao = item.cd_equipto_cm
                    sPlaca = FBuscaPlaca(sCaminhao, sLiberacao1)
                    'sPlaca = cadequiptos(sCaminhao).no_placa
                    sCdOperador = item.cd_operador
                    If cadfuncionarios.ContainsKey(sCdOperador) Then
                        sCdFunc = $"{sCdOperador} {cadfuncionarios(sCdOperador)}"
                    Else
                        sCdFunc = Nothing
                    End If
                    'sCdFunc = FBuscaMotorista(sCaminhao, sLiberacao1, "D")
                    nTamMotorista = If(sCdFunc?.Length, 0)
                    If nTamMotorista > 28 Then
                        sCdFunc = sCdFunc.Substring(0, 28)
                    End If

                    'sMotorista = FBuscaMotorista(sCaminhao, sLiberacao1, "C")
                    sMotorista = sCdOperador
                    sEntrada = item.hr_entrada
                    HrInicial = Convert.ToDateTime(sEntrada)
                    DtHrIniPeriodo = HrInicial.ToString("dd/MM/yyyy") & " " & hrFechamento
                    HrFinal = Convert.ToDateTime(DtHrIniPeriodo)
                    HrFinal = HrFinal.AddHours(24).AddSeconds(-1)
                    DtHrFinalPeriodo = HrFinal.ToString("dd/MM/yyyy HH:mm:ss")
                    sSaida = item.hr_saida
                    sPesoBruto1 = item.qt_bruto
                    sPesoTara1 = item.qt_tara
                    sPesoNeto1 = item.qt_liquido
                    sCdNivel1_1 = item.cd_upnivel1 & " " & cadupnivel1(item.cd_upnivel1).de_upnivel1
                    nTamNivel1_1 = If(sCdNivel1_1?.Length, 0)
                    If nTamNivel1_1 > 33 Then
                        sCdNivel1_1 = sCdNivel1_1.Substring(0, 33)
                    End If
                    sCdFornec1 = cadupnivel1(item.cd_upnivel1).de_fornec
                    nTamForn1_1 = If(sCdFornec1?.Length, 0)
                    If nTamForn1_1 > 28 Then
                        sCdFornec1 = sCdFornec1.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 2 Then
                    sNoSequencia2 = item.no_sequencia
                    sLiberacao2 = item.no_liberacao
                    If IsDBNull(item.cd_equipto_ct) Then
                        sCarreta2 = FBuscaCarretaLiberacao(sLiberacao2)
                    Else
                        sCarreta2 = item.cd_equipto_ct
                    End If
                    sPesoBruto2 = item.qt_bruto
                    sPesoTara2 = item.qt_tara
                    sPesoNeto2 = item.qt_liquido
                    sCdNivel1_2 = item.cd_upnivel1 & " " & cadupnivel1(item.cd_upnivel1).de_upnivel1
                    nTamNivel1_2 = If(sCdNivel1_2?.Length, 0)
                    If nTamNivel1_2 > 33 Then
                        sCdNivel1_2 = sCdNivel1_2.Substring(0, 33)
                    End If
                    sCdFornec2 = cadupnivel1(item.cd_upnivel1).de_fornec
                    nTamForn1_2 = If(sCdFornec2?.Length, 0)
                    If nTamForn1_2 > 28 Then
                        sCdFornec2 = sCdFornec2.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 3 Then
                    sNoSequencia3 = item.no_sequencia
                    sLiberacao3 = item.no_liberacao
                    If IsDBNull(item.cd_equipto_ct) Then
                        sCarreta3 = FBuscaCarretaLiberacao(sLiberacao3)
                    Else
                        sCarreta3 = item.cd_equipto_ct
                    End If
                    sPesoBruto3 = item.qt_bruto
                    sPesoTara3 = item.qt_tara
                    sPesoNeto3 = item.qt_liquido
                    sCdNivel1_3 = item.cd_upnivel1 & " " & cadupnivel1(item.cd_upnivel1).de_upnivel1
                    nTamNivel1_3 = If(sCdNivel1_3?.Length, 0)
                    If nTamNivel1_3 > 33 Then
                        sCdNivel1_3 = sCdNivel1_3.Substring(0, 33)
                    End If
                    sCdFornec3 = cadupnivel1(item.cd_upnivel1).de_fornec
                    nTamForn1_3 = If(sCdFornec3?.Length, 0)
                    If nTamForn1_3 > 28 Then
                        sCdFornec3 = sCdFornec3.Substring(0, 28)
                    End If
                ElseIf sNoSequencia = 4 Then
                    sNoSequencia4 = item.no_sequencia
                    sLiberacao4 = item.no_liberacao
                    If IsDBNull(item.cd_equipto_ct) Then
                        sCarreta4 = FBuscaCarretaLiberacao(sLiberacao4)
                    Else
                        sCarreta4 = item.cd_equipto_ct
                    End If
                    sPesoBruto4 = item.qt_bruto
                    sPesoTara4 = item.qt_tara
                    sPesoNeto4 = item.qt_liquido
                    sCdNivel1_4 = item.cd_upnivel1 & " " & cadupnivel1(item.cd_upnivel1).de_upnivel1
                    nTamNivel1_4 = If(sCdNivel1_4?.Length, 0)
                    If nTamNivel1_4 > 33 Then
                        sCdNivel1_4 = sCdNivel1_4.Substring(0, 33)
                    End If
                    sCdFornec4 = cadupnivel1(item.cd_upnivel1).de_fornec
                    nTamForn1_4 = If(sCdFornec4?.Length, 0)
                    If nTamForn1_4 > 28 Then
                        sCdFornec4 = sCdFornec4.Substring(0, 28)
                    End If
                End If
            Next
            Dim sEmpresa As String = cadparametros(("GERAIS", "DA_EMPRESA"))
            Dim sDeNivel1 As String = cadparametros(("NIVEIS", "UPNIVEL1"))

            Dim ec850 As System.Text.Encoding = System.Text.Encoding.GetEncoding(850)
            Dim buffer As New List(Of Byte)
            buffer.AddRange(New Byte() {&H1B, &H40})

            ' --- Montagem do buffer ESC/POS ---
            For i As Integer = 1 To vGlobal.vNumCopiasExtrSaidaCana
                ' Cabeçalho centralizado e negrito
                buffer.AddRange(New Byte() {&H1B, &H61, 1}) ' ESC a 1 = centralizar
                buffer.AddRange(New Byte() {&H1B, &H45, 1}) ' ESC E = negrito on
                buffer.AddRange(ec850.GetBytes($"*** {sEmpresa} ***{vbCrLf}"))
                buffer.AddRange(ec850.GetBytes($"Extracto de Pesaje de Caña{vbCrLf}"))
                buffer.AddRange(New Byte() {&H1B, &H45, 0}) ' ESC E = negrito off
                buffer.AddRange(New Byte() {&H1B, &H61, 0}) ' ESC a 0 = alinhar esquerda

                buffer.AddRange(ec850.GetBytes($"------------------------------------------{vbCrLf}"))

                ' Corpo principal
                Dim corpo As New StringBuilder()

                If sNoSequencia1 = 1 Then
                    corpo.AppendLine($"Nombre: {sCdFunc}")
                    corpo.AppendLine($"Camión: {sCaminhao.PadLeft(8)} Chofer: {sMotorista.PadLeft(10)}")
                    corpo.AppendLine($"Placa: {sPlaca}")
                    corpo.AppendLine($"Entrada: {sEntrada}")
                    corpo.AppendLine($"Salida: {sSaida}")
                    corpo.AppendLine($"Nº Viaje: {sViagem.PadLeft(6, "0"c)}")
                    corpo.AppendLine()
                    corpo.Append(BlocoImpressaoRaw(sCdFornec1, sDeNivel1, sCdNivel1_1, sLiberacao1, sCaminhao, sPesoBruto1, sPesoTara1, sPesoNeto1))
                    corpo.AppendLine()
                End If

                If sNoSequencia2 = 2 Then corpo.AppendLine(BlocoImpressaoRaw(sCdFornec2, sDeNivel1, sCdNivel1_2, sLiberacao2, sCarreta2, sPesoBruto2, sPesoTara2, sPesoNeto2))
                corpo.AppendLine()
                If sNoSequencia3 = 3 Then corpo.AppendLine(BlocoImpressaoRaw(sCdFornec3, sDeNivel1, sCdNivel1_3, sLiberacao3, sCarreta3, sPesoBruto3, sPesoTara3, sPesoNeto3))
                corpo.AppendLine()
                If sNoSequencia4 = 4 Then corpo.AppendLine(BlocoImpressaoRaw(sCdFornec4, sDeNivel1, sCdNivel1_4, sLiberacao4, sCarreta4, sPesoBruto4, sPesoTara4, sPesoNeto4))

                ' 1. Monta a linha dos cabeçalhos (com espaços para separar)
                'corpo.AppendLine("BRUTO            TARA             NETO")

                ' 2. Calcula as somas
                Dim totalBruto As Double = CDbl(sPesoBruto1) + CDbl(sPesoBruto2) + CDbl(sPesoBruto3) + CDbl(sPesoBruto4)
                Dim totalTara As Double = CDbl(sPesoTara1) + CDbl(sPesoTara2) + CDbl(sPesoTara3) + CDbl(sPesoTara4)
                Dim totalNeto As Double = CDbl(sPesoNeto1) + CDbl(sPesoNeto2) + CDbl(sPesoNeto3) + CDbl(sPesoNeto4)

                ' 3. Monta a linha dos valores logo abaixo
                'corpo.AppendLine(totalBruto & "            " & totalTara & "            " & totalNeto)

                ' 4 Novo modelo de impressão dos pesos
                corpo.AppendLine($"Peso Bruto: {totalBruto}")
                corpo.AppendLine($"Peso Tara: {totalTara}")
                corpo.AppendLine($"Peso Neto: {totalNeto}")

                corpo.AppendLine()
                corpo.AppendLine($"Total Transp: {FTotalEquiptoExtSaida(sMotorista, DtHrIniPeriodo, DtHrFinalPeriodo)}")
                corpo.AppendLine(".")
                corpo.AppendLine("------------------------------------------")
                corpo.AppendLine()

                buffer.AddRange(ec850.GetBytes(corpo.ToString()))

                ' Espaço extra e corte
                buffer.AddRange(ec850.GetBytes(vbCrLf & vbCrLf & vbCrLf & vbCrLf))
                buffer.AddRange(New Byte() {&H1B, &H69, 0}) ' Corte total
            Next
            ' Enviar para a impressora
            RawPrinterHelper.SendBytesToPrinter(vGlobal.vNomeImpressora, buffer.ToArray(), "Extrao_Pesaje_Caña")
        Catch ex As Exception
            FMensagem("Impressoes.ImprimirSaidaLaredo: " & ex.ToString, "C")
        End Try
    End Sub

    Private Function BlocoImpressaoRaw(fornec As String, nivel As String, cdNivel As String,
                                   liberacao As String, veiculo As String,
                                   bruto As String, tara As String, neto As String) As String
        Dim sb As New StringBuilder()

        sb.AppendLine("Proveedor: " & fornec)
        sb.AppendLine(nivel & " " & cdNivel)

        ' Cabeçalhos
        '[12/ 12/ 9/ 9/ 9]
        Dim headerLiberacao As String = "Liberación".PadRight(12)
        Dim headerEquipo As String = "Equipo".PadRight(9)
        Dim headerBruto As String = "Bruto".PadRight(7)
        Dim headerTara As String = "Tara".PadRight(7)
        Dim headerNeto As String = "Neto".PadRight(7)

        ' Valores
        Dim valLiberacao As String = liberacao.PadRight(12)
        Dim valEquipo As String = veiculo.PadRight(9)
        Dim valBruto As String = bruto.PadRight(7)
        Dim valTara As String = tara.PadRight(7)
        Dim valNeto As String = neto.PadRight(7)

        ' Cabeçalho e valores juntos, sem linha extra
        sb.AppendLine(headerLiberacao & headerEquipo & headerBruto & headerTara & headerNeto)
        sb.AppendLine(valLiberacao & valEquipo & valBruto & valTara & valNeto)

        Return sb.ToString()
    End Function

    Public WithEvents PrintImpurezaMineralMD01 As New PrintDocument
    Private Sub PrintImpurezaMineralMD01_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintImpurezaMineralMD01.PrintPage
        Try
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

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

            vGlobal.sNomeEmpresa = cadempresas(cadparametros(("GERAIS", "CD_EMPRESA")))

            e.Graphics.DrawString("**            " & vGlobal.sNomeEmpresa & "            **", FontesImpressao.Fonte11R, Brushes.Black, 10, 10)
            e.Graphics.DrawString("      Boletín de Análisis de Trash", FontesImpressao.Fonte11R, Brushes.Black, 10, 50)
            e.Graphics.DrawString("Camión: " & cdCaminhao & "   Chofer: " & cdMotorista & "", FontesImpressao.Fonte10R, Brushes.Black, 10, 90)
            e.Graphics.DrawString("Remolques: " & cdCarretas & "", FontesImpressao.Fonte10R, Brushes.Black, 10, 110)
            e.Graphics.DrawString("Entrada: " & hrEntrada & "", FontesImpressao.Fonte10R, Brushes.Black, 10, 130)
            e.Graphics.DrawString("Frente de Cosecha: " & cdFrenTran & "", FontesImpressao.Fonte10R, Brushes.Black, 10, 150)

            barra = codbarras.Draw(noAnalise, altura)
            e.Graphics.DrawImage(barra, New Rectangle(35, 200, 140, 70))
            e.Graphics.DrawString(noAnalise, FontesImpressao.Fonte9R, Brushes.Black, 85, 280)

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

    Public WithEvents PrintZebra As New PrintDocument
    Private Sub PrintZebra_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintZebra.PrintPage
        Try
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

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

            e.Graphics.DrawString("Materia Extraña", FontesImpressao.Fonte8R, Brushes.Black, 5, 3)
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
        'RawPrinterHelper.SendFileToPrinter(vGlobal.vNomeImpressora, sSourceFile)

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
        'RawPrinterHelper.SendFileToPrinter(vGlobal.vNomeImpressora, sFileName2)

    End Sub

    '[BALCANES]
    Public WithEvents PrintBolAnl_Balcanes As New PrintDocument
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

            Dim tblaptcargas As String = "apt_cargas"
            Dim tblaptcargasrec As String = "apt_cargas_rec"
            Dim valor As String = vGlobal.nRegistroImpressao

            Dim swhere As New StringBuilder
            If vGlobal.bBotaoImprimir = True Then
                Dim partes() As String = Nothing
                If Not String.IsNullOrWhiteSpace(valor) Then
                    partes = valor.Split("*"c)
                End If

                Dim liberacao As String = Nothing
                Dim ano As String = Nothing
                If partes.Count >= 2 Then
                    liberacao = partes(0)
                    ano = partes(1)

                    tblaptcargas = $"{tblaptcargas}_{ano}"
                    tblaptcargasrec = $"{tblaptcargasrec}_{ano}"
                Else
                    liberacao = partes(0)
                End If

                IdViagem = FIdViagem(liberacao, tblaptcargas)
                swhere.AppendLine(" and c.pfg_impr_bolanl = :fg_impr_bolanl")
                swhere.AppendLine(" and c.id_viagem = :id_viagem")
            ElseIf vGlobal.bBotaoImprimir = False Then
                swhere.AppendLine("  and c.id_viagem = :id_viagem")
                swhere.AppendLine("  and c.pfg_impr_bolanl is null")
            End If

            Dim sql As New StringBuilder
            sql.AppendLine("select c.no_sequencia,c.no_liberacao,c.no_analise,")
            sql.AppendLine("e.cd_equipto||' '||e.no_placa equipo,r.cd_operador,")
            sql.AppendLine("t.cd_transp||'-'||t.de_transp transp,")
            sql.AppendLine("c.hr_guarita,u.cd_upnivel1||'-'||u.de_upnivel1 upnivel1,")
            sql.AppendLine("f.cd_fornec||'-'||f.de_fornec fornec")
            sql.AppendLine($"from {tblaptcargas} c")
            sql.AppendLine($"inner join {tblaptcargasrec} r on r.instancia = c.instancia and r.no_liberacao = c.no_liberacao")
            sql.AppendLine("inner join equiptos e on e.cd_equipto = r.cd_equipto")
            sql.AppendLine("inner join transporta t on t.cd_transp = e.cd_transp")
            sql.AppendLine("inner join upnivel1 u on u.cd_upnivel1 = c.cd_upnivel1")
            sql.AppendLine("inner join fornecs f on f.cd_fornec = u.cd_fornec")
            sql.AppendLine("where c.instancia = :instancia")
            sql.AppendLine("  and r.cd_tp_recurso = :cd_tp_recurso")
            sql.AppendLine("  and c.fg_analise = :fg_analise")
            sql.AppendLine($"{swhere.ToString}")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("cd_tp_recurso", OracleDbType.Varchar2).Value = "CM"
                cmd.Parameters.Add("fg_analise", OracleDbType.Varchar2).Value = "S"
                cmd.Parameters.Add("id_viagem", OracleDbType.Int32).Value = IdViagem

                If vGlobal.bBotaoImprimir = True Then
                    cmd.Parameters.Add("fg_impr_bolanl", OracleDbType.Varchar2).Value = "S"
                End If

                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        noSeq = rs("no_sequencia")
                        vMensangem += "noSeq: " & rs("no_sequencia") & vbNewLine
                        noLiberacao = rs("no_liberacao")
                        vMensangem += "noLiberacao: " & rs("no_liberacao") & vbNewLine
                        cdCaminhao = rs("equipo")
                        vMensangem += "cdCaminhao: " & rs("equipo") & vbNewLine
                        If IsDBNull(rs("cd_operador")) Then
                            cdMotorista = "0"
                        Else
                            cdMotorista = rs("cd_operador")
                        End If

                        hrEntrada = rs("hr_guarita")
                        cdTransporta = rs("transp").ToString.Replace(",", " ")
                        cdFazenda = rs("upnivel1").ToString.Replace(",", " ")
                        cdFornecedor = rs("fornec").ToString.Replace(",", " ")
                        noAnalise = rs("no_analise")
                        qtBruto = fPesoBrutoBolAnl(noLiberacao, tblaptcargas)

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
                End Using
            End Using

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
            fGravaError("frmReimpressao", "PrintBolAnl_Balcanes_PrintPage", ex.ToString, "LOG_BOLA_ERROR")
        End Try
    End Sub

    Public WithEvents PrintOrdemCarga_Balcanes As New PrintDocument
    Private Sub PrintOrdemCarga_Balcanes_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintOrdemCarga_Balcanes.PrintPage
        Try
            frmReimpressao.bImpressoOrdemCargas = False
            vGlobal.sValor01 = Nothing
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

            Dim sDtEmissao, sHoraEmissao, sNoOrdemCarga, sFazenda, sFornecedor, sFrenteColheita, sCaminhao, sPrestador As String
            Dim sNoQueima, sCartoes, sQtConsumo, sCdEquipto, sCdUnidInd, sDeUnidInd As String
            Dim sWhere As String
            Dim bFgAbatec As Boolean
            Dim txt As New StringBuilder

            '------------------------------------------------------------------
            '--- INICIO -------------------------------------------------------
            '--- Ticket #4818 -------------------------------------------------
            '--- Impresión de Orden de Carga y Extracto de Pesaje de otras zafras
            '--- Data: 17/11/2026----------------------------------------------
            Dim registro As String = Nothing
            registro = vGlobal.nRegistroImpressao

            Dim tblqueimaHe As String = "queima_he"
            Dim tblqueimaDe As String = "queima_de"

            Dim boletim As String = Nothing
            Dim ano As String = Nothing

            Dim partes() As String

            If Not String.IsNullOrWhiteSpace(registro) Then
                partes = Split(registro, "*"c)
            End If

            If vGlobal.bBotaoImprimir = True Then
                If partes.Count >= 2 Then
                    boletim = partes(0)
                    ano = partes(1)

                    tblqueimaHe = $"{tblqueimaHe}_{ano}"
                    tblqueimaDe = $"{tblqueimaDe}_{ano}"

                Else
                    boletim = partes(0)
                End If
                '--- FINAL --------------------------------------------------------
                sWhere = "and ld.pfg_ordem_carga = 'S' and ld.id_despacho = :id_despacho"
            ElseIf vGlobal.bBotaoImprimir = False Then
                sWhere = "  and ld.fg_status = 'A' and ld.pfg_ordem_carga is null"
            End If

            Dim sql As New StringBuilder
            sql.AppendLine("select ld.id_despacho,")
            sql.AppendLine("to_char(ld.dthr_inicio,'dd/mm/yyyy') data,")
            sql.AppendLine("to_char(ld.dthr_inicio,'hh24:mi:ss') hora,")
            sql.AppendLine("fn.cd_fornec||'-'||fn.de_fornec fornecedor,")
            sql.AppendLine("ft.cd_fren_tran||'-'||ft.de_fren_tran frente,")
            sql.AppendLine("e.cd_equipto||'-'||e.no_placa veiculo,e.cd_equipto,")
            sql.AppendLine("tt.cd_transp||'-'||tt.de_transp prestador,")
            sql.AppendLine("trim(u1.cd_upnivel1)||'-'||u1.de_upnivel1 fazenda,")
            sql.AppendLine("llc.no_queima_ant no_queima,nvl(round(ld.qt_consumo,0),0) qt_consumo,")
            sql.AppendLine("ld.cd_unid_ind")
            sql.AppendLine("from lt_despachos ld")
            sql.AppendLine("inner join lt_liberacao_carreg llc on ld.id_liberacao = llc.id_liberacao")
            sql.AppendLine("inner join frentransp ft on ft.cd_fren_tran = llc.cd_fren_tran")
            sql.AppendLine("inner join sistcolh sc on sc.cd_sist_colh = llc.cd_sist_colh")
            sql.AppendLine("inner join equiptos e on e.cd_equipto = ld.cd_equipto")
            sql.AppendLine("inner join transporta tt on tt.cd_transp = e.cd_transp")
            sql.AppendLine($"left join {tblqueimaHe} qh on qh.no_queima = llc.no_queima_ant")
            sql.AppendLine($"left join {tblqueimaDe} qd on qd.no_queima = qh.no_queima")
            sql.AppendLine("left join upnivel1 u1 on u1.cd_upnivel1 = qd.cd_upnivel1")
            sql.AppendLine("left join fornecs fn on fn.cd_fornec = u1.cd_fornec")
            sql.AppendLine("where ld.instancia = :instancia")
            sql.AppendLine("and ld.id_despacho = :id_despacho")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("id_despacho", OracleDbType.Int32).Value = CInt(boletim)
                Using rs As OracleDataReader = cmd.ExecuteReader
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
                        sQtConsumo = rs("qt_consumo")
                        sCdEquipto = rs("cd_equipto")
                        sCdUnidInd = rs("cd_unid_ind")
                        sCartoes = fListaCartoes(sNoQueima)
                    End While
                End Using
            End Using
            bFgAbatec = FEquipAbastec(sCdEquipto)

            vGlobal.sNomeEmpresa = cadempresas(cadparametros(("GERAIS", "CD_EMPRESA")))
            sDeUnidInd = cadunidind(sCdUnidInd)

            'MARCA D'AGUA
            Dim wPen As New Pen(Color.Black, 1)
            Dim blackPen = New Pen(Color.Black, 1)
            Dim dashValues As Single() = {2, 2, 2, 2}
            Dim Descr1 As String = vGlobal.gs01
            Dim Descr2 As String = vGlobal.gs02
            Dim font As New Font("Arial", 40, FontStyle.Bold)
            Dim brush As New SolidBrush(Color.FromArgb(80, Color.Gray))
            Dim img As Image = Image.FromFile(Application.StartupPath & "\logotipo.jpg")
            Dim sf As New StringFormat
            sf.Alignment = StringAlignment.Center
            'VIA 01
            Dim rect1 As New System.Drawing.RectangleF(25, 120, 640, 235)
            e.Graphics.DrawString(Descr1, font, brush, rect1, sf)
            Dim rect2 As New System.Drawing.RectangleF(25, 170, 640, 235)
            e.Graphics.DrawString(Descr2, font, brush, rect2, sf)
            e.Graphics.DrawImage(img, 30, 30)
            'VIA 02
            Dim rect3 As New System.Drawing.RectangleF(25, 450, 640, 235)
            e.Graphics.DrawString(Descr1, font, brush, rect3, sf)
            Dim rect4 As New System.Drawing.RectangleF(25, 500, 640, 235)
            e.Graphics.DrawString(Descr2, font, brush, rect4, sf)
            e.Graphics.DrawImage(img, 30, 355)
            'VIA 03
            If bFgAbatec = True Then
                Dim rect5 As New System.Drawing.RectangleF(25, 805, 640, 235)
                e.Graphics.DrawString(Descr1, font, brush, rect5, sf)
                Dim rect6 As New System.Drawing.RectangleF(25, 855, 640, 235)
                e.Graphics.DrawString(Descr2, font, brush, rect6, sf)
                e.Graphics.DrawImage(img, 30, 685)
            End If

            Dim textoLitros As String
            Dim sAbastec As String
            If sQtConsumo > 0 Then
                sAbastec = "SÍ"
                textoLitros = Nothing
                textoLitros = sQtConsumo.Replace(",", ".") & " lts"
            Else
                sAbastec = "NO"
                textoLitros = Nothing
                textoLitros = "0 lts"
            End If

            If vGlobal.sValor05 = "N" Then
                textoLitros = Nothing
            End If

            'Primeira Via
            e.Graphics.DrawRectangle(wPen, 25, 25, 640, 300)

            Dim rect7 As New System.Drawing.RectangleF(25, 30, 640, 235)
            e.Graphics.DrawString("ORDEN DE CARGA", Fonte13B, Brushes.Black, rect7, sf)
            Dim rect8 As New System.Drawing.RectangleF(25, 70, 640, 235)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte12B, Brushes.Black, rect8, sf)
            e.Graphics.DrawString("Fecha:    " & sDtEmissao & "     Hora: " & sHoraEmissao & "     Número: " & sNoOrdemCarga, FontesImpressao.Fonte10R, Brushes.Black, 30, 110)
            e.Graphics.DrawString("Cañero:   " & sFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 30, 130)
            e.Graphics.DrawString("Frente:   " & sFrenteColheita, FontesImpressao.Fonte10R, Brushes.Black, 30, 150)
            e.Graphics.DrawString("Vehículo: " & sCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 30, 170)
            e.Graphics.DrawString("Fletero:  " & sPrestador, FontesImpressao.Fonte10R, Brushes.Black, 30, 190)
            e.Graphics.DrawString("Finca:    " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 210)
            e.Graphics.DrawString("Orden de Cosecha: " & sNoQueima, FontesImpressao.Fonte10R, Brushes.Black, 30, 230)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, FontesImpressao.Fonte10B, Brushes.Black, 30, 250)
            'If bFgAbatec = True Then
            e.Graphics.DrawString("Abastecimiento: " & sAbastec & "   " & textoLitros, FontesImpressao.Fonte10B, Brushes.Black, 30, 270)
            'End If
            e.Graphics.DrawString("Descargar en: ", FontesImpressao.Fonte10R, Brushes.Black, 30, 300)
            e.Graphics.DrawString(sDeUnidInd, FontesImpressao.Fonte12B, Brushes.Black, 165, 300)

            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 240, 140, 70))

            'Linha pontilhada 1
            Dim Point01 As New Point(25, 340)
            Dim Point02 As New Point(650, 340)
            blackPen.DashPattern = dashValues
            e.Graphics.DrawLine(blackPen, Point01, Point02)

            'Segunda Via
            e.Graphics.DrawRectangle(wPen, 25, 355, 640, 300)

            Dim rect9 As New System.Drawing.RectangleF(25, 360, 640, 235)
            e.Graphics.DrawString("ORDEN DE CARGA", Fonte13B, Brushes.Black, rect9, sf)
            Dim rect10 As New System.Drawing.RectangleF(25, 400, 640, 235)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte12B, Brushes.Black, rect10, sf)
            e.Graphics.DrawString("Fecha:    " & sDtEmissao & "     Hora: " & sHoraEmissao & "     Número: " & sNoOrdemCarga, FontesImpressao.Fonte10R, Brushes.Black, 30, 440)
            e.Graphics.DrawString("Cañero:   " & sFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 30, 460)
            e.Graphics.DrawString("Frente:   " & sFrenteColheita, FontesImpressao.Fonte10R, Brushes.Black, 30, 480)
            e.Graphics.DrawString("Vehículo: " & sCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 30, 500)
            e.Graphics.DrawString("Fletero:  " & sPrestador, FontesImpressao.Fonte10R, Brushes.Black, 30, 520)
            e.Graphics.DrawString("Finca:    " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 540)
            e.Graphics.DrawString("Orden de Cosecha: " & sNoQueima, FontesImpressao.Fonte10R, Brushes.Black, 30, 560)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, FontesImpressao.Fonte10B, Brushes.Black, 30, 580)
            'If bFgAbatec = True Then
            e.Graphics.DrawString("Abastecimiento: " & sAbastec & "   " & textoLitros, FontesImpressao.Fonte10B, Brushes.Black, 30, 600)
            'End If
            e.Graphics.DrawString("Descargar en: ", FontesImpressao.Fonte10R, Brushes.Black, 30, 630)
            e.Graphics.DrawString(sDeUnidInd, FontesImpressao.Fonte12B, Brushes.Black, 165, 630)

            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 570, 140, 70))

            'Linha pontilhada 2
            Dim Point03 As New Point(25, 670)
            Dim Point04 As New Point(650, 670)
            blackPen.DashPattern = dashValues
            e.Graphics.DrawLine(blackPen, Point03, Point04)

            'Terceira Via
            e.Graphics.DrawRectangle(wPen, 25, 685, 640, 300)

            Dim rect11 As New System.Drawing.RectangleF(25, 690, 640, 235)
            e.Graphics.DrawString("ORDEN DE CARGA", Fonte13B, Brushes.Black, rect11, sf)
            Dim rect12 As New System.Drawing.RectangleF(25, 730, 640, 235)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte12B, Brushes.Black, rect12, sf)
            e.Graphics.DrawString("Fecha:    " & sDtEmissao & "     Hora: " & sHoraEmissao & "     Número: " & sNoOrdemCarga, FontesImpressao.Fonte10R, Brushes.Black, 30, 770)
            e.Graphics.DrawString("Cañero:   " & sFornecedor, FontesImpressao.Fonte10R, Brushes.Black, 30, 790)
            e.Graphics.DrawString("Frente:   " & sFrenteColheita, FontesImpressao.Fonte10R, Brushes.Black, 30, 810)
            e.Graphics.DrawString("Vehículo: " & sCaminhao, FontesImpressao.Fonte10R, Brushes.Black, 30, 830)
            e.Graphics.DrawString("Fletero:  " & sPrestador, FontesImpressao.Fonte10R, Brushes.Black, 30, 850)
            e.Graphics.DrawString("Finca:    " & sFazenda, FontesImpressao.Fonte10R, Brushes.Black, 30, 870)
            e.Graphics.DrawString("Orden de Cosecha: " & sNoQueima, FontesImpressao.Fonte10R, Brushes.Black, 30, 890)
            e.Graphics.DrawString("Tarjetas: " & sCartoes, FontesImpressao.Fonte10B, Brushes.Black, 30, 910)
            'If bFgAbatec = True Then
            e.Graphics.DrawString("Abastecimiento: " & sAbastec & "   " & textoLitros, FontesImpressao.Fonte10B, Brushes.Black, 30, 930)
            'End If
            e.Graphics.DrawString("Descargar en: ", FontesImpressao.Fonte10R, Brushes.Black, 30, 960)
            e.Graphics.DrawString(sDeUnidInd, FontesImpressao.Fonte12B, Brushes.Black, 165, 960)

            barra = codbarras.Draw(sNoOrdemCarga, altura)
            e.Graphics.DrawImage(barra, New Rectangle(500, 900, 140, 70))
            'e.Graphics.DrawString("3ª Via Abastecimiento", FontesImpressao.Fonte10R, Brushes.Black, 470, 970)

            frmReimpressao.bImpressoOrdemCargas = True

            AtualizaDespachoOCImpresso(vGlobal.sValor01)
        Catch ex As Exception
            fGravaError("frmReimpressao", "PrintOrdemCarga_Balcanes_PrintPage", ex.ToString, "LOG_OC_ERROR")
        End Try
    End Sub

    Private Sub TextLogDebug(texto As String)
        ' Caminho do arquivo de log
        Dim caminhoArquivo As String = Application.StartupPath & "\logs\LogDebug.txt"

        ' Usando StreamWriter para gravar
        Using sw As New StreamWriter(caminhoArquivo, False) ' False = sobrescreve, True = append
            ' Cabeçalho
            sw.WriteLine("=== LOG DE DADOS ===")
            sw.WriteLine("Data/Hora: " & DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            sw.WriteLine("--------------------------------------------------")

            sw.WriteLine(texto)

            sw.WriteLine("--------------------------------------------------")
            sw.WriteLine()
        End Using

        Console.WriteLine("Arquivo gravado em: " & caminhoArquivo)
    End Sub

    Public WithEvents PrintExtratoSaida_Balcanes As New PrintDocument
    Private Sub PrintExtratoSaida_Balcanes_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintExtratoSaida_Balcanes.PrintPage
        Try
            frmReimpressao.bImpressoOrdemCargas = False
            vGlobal.sValor02 = Nothing
            Dim codbarras As Zen.Barcode.Code128BarcodeDraw = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum
            Dim codQrCode As Zen.Barcode.CodeQrBarcodeDraw = Zen.Barcode.BarcodeDrawFactory.CodeQr
            Dim altura As Integer = 1000

            Dim barra As Image

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

            Dim tblAptCargas As String = "apt_cargas"
            Dim tblAptCargasRec As String = "apt_cargas_rec"

            Dim partes() As String
            Dim valorespartes As String = vGlobal.nRegistroImpressao


            If vGlobal.bBotaoImprimir = True Then
                If String.IsNullOrWhiteSpace(valorespartes) Then
                    MessageBox.Show("No se ha seleccionado ninguna liberación para imprimir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Exit Sub
                End If
                partes = valorespartes.Split("*"c)

                Dim liberacao As String = Nothing
                Dim ano As String = Nothing

                If partes.Length >= 2 Then
                    liberacao = partes(0)
                    ano = partes(1)

                    tblAptCargas = $"{tblAptCargas}_{ano}"
                    tblAptCargasRec = $"{tblAptCargasRec}_{ano}"
                Else
                    liberacao = partes(0)
                End If

                sLiberacoes = liberacao

                If vGlobal.vIMPRLIBSEPARADA = "S" Then
                    sWhere = $"  and ac.pfg_impr_extr_saida = 'S' and ac.id_viagem = {FIdViagem(sLiberacoes, tblAptCargas)}" '    and ac.no_liberacao = { vGlobal.sValor04 }   
                ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                    sWhere = $"  and ac.pfg_impr_extr_saida = 'S' and ac.id_viagem = {FIdViagem(sLiberacoes, tblAptCargas)}" '  and ac.no_liberacao = { vGlobal.sValor04 }
                End If

            ElseIf vGlobal.bBotaoImprimir = False Then
                If vGlobal.vIMPRLIBSEPARADA = "S" Then
                    sWhere = $"  and ac.pfg_impr_extr_saida is null and ac.no_liberacao = { vGlobal.sValor04 }"
                ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                    sWhere = "  and ac.pfg_impr_extr_saida is null"
                End If
            End If

            Dim sql As New StringBuilder
            sql.AppendLine("select ac.instancia,ac.id_viagem,ac.no_sequencia,")
            sql.AppendLine("       ac.no_liberacao,nvl(ac.ordem_carga,0) ordem_carga,nvl(ac.numero_remito,0) numero_remito,")
            sql.AppendLine("       fn.cd_fornec || '-' || fn.de_fornec fornecedor,")
            sql.AppendLine("       eq.cd_equipto || '-' || eq.no_placa equipamento,")
            sql.AppendLine("       t.cd_transp||'-'||t.de_transp transporta,decode(ac.fg_cana_crua,'S','Cruda','Quemada') tipo_cana,")
            sql.AppendLine("       trim(u1.cd_upnivel1) || '-' || u1.de_upnivel1 fazenda,")
            sql.AppendLine("       f.cd_fren_tran||'-'||f.de_fren_tran frente,")
            sql.AppendLine("       ac.dt_entrada,ac.hr_entrada,")
            sql.AppendLine("       ac.dt_saida,ac.hr_saida,")
            sql.AppendLine("       ac.dt_movimento,nvl(ac.qt_bruto,0) qt_bruto,")
            sql.AppendLine("       nvl(ac.qt_tara,0) qt_tara,nvl(ac.qt_liquido,0) qt_liquido,")
            sql.AppendLine("       ac.fg_cana_crua,ac.no_cartao,ac.no_queima,")
            sql.AppendLine("       trim(ac.cd_upnivel1) cd_upnivel1,trim(ac.cd_upnivel2) cd_upnivel2,trim(ac.cd_upnivel3) cd_upnivel3")
            sql.AppendLine($"  from {tblAptCargas} ac,")
            sql.AppendLine($"       {tblAptCargasRec} acr,")
            sql.AppendLine("       equiptos       eq,")
            sql.AppendLine("       fornecs        fn,")
            sql.AppendLine("       upnivel1       u1,")
            sql.AppendLine("       transporta     t,")
            sql.AppendLine("       frentransp     f")
            sql.AppendLine(" where f.cd_fren_tran = ac.cd_fren_tran")
            sql.AppendLine("   and t.cd_transp = eq.cd_transp")
            sql.AppendLine("   and u1.cd_upnivel1 = ac.cd_upnivel1")
            sql.AppendLine("   and fn.cd_fornec = u1.cd_fornec")
            sql.AppendLine("   and eq.cd_equipto = acr.cd_equipto")
            sql.AppendLine("   and acr.instancia = ac.instancia")
            sql.AppendLine("   and acr.no_liberacao = ac.no_liberacao")
            sql.AppendLine("   and acr.cd_tp_recurso = :cd_tp_recurso")
            sql.AppendLine("   and ac.instancia = :instancia")
            sql.AppendLine("   and ac.dt_saida is not null")
            sql.AppendLine($"{sWhere}")
            sql.AppendLine(" order by 2, 3")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("cd_tp_recurso", OracleDbType.Varchar2).Value = "CM"

                Using rs As OracleDataReader = cmd.ExecuteReader
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
                End Using
            End Using

            vGlobal.sNomeEmpresa = cadempresas(cadparametros(("GERAIS", "CD_EMPRESA")))

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
            e.Graphics.DrawString("EXTRACTO DE PESAJE", FontesImpressao.Fonte11R, Brushes.Black, 100, 30)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte11B, Brushes.Black, 100, 50)

            e.Graphics.DrawString("Nº: " & vInstLogin & "-" & vGlobal.gs03, FontesImpressao.Fonte8R, Brushes.Black, 400, 80)

            e.Graphics.DrawString("Origen:    " & sFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 100)
            e.Graphics.DrawString("Orden de Carga: " & sOrdemCarga, FontesImpressao.Fonte8R, Brushes.Black, 400, 100)
            e.Graphics.DrawString("Cañero:    " & sFornecedor, FontesImpressao.Fonte8R, Brushes.Black, 30, 110)
            e.Graphics.DrawString("Entrada:   " & sHrEntrada, FontesImpressao.Fonte8R, Brushes.Black, 30, 120)
            e.Graphics.DrawString("Salida:    " & sHrSaida, FontesImpressao.Fonte8R, Brushes.Black, 250, 120)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", FontesImpressao.Fonte8B, Brushes.Black, 300, 140)
            e.Graphics.DrawString("Liberación    Remito        ", FontesImpressao.Fonte8B, Brushes.Black, 30, 140)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, FontesImpressao.Fonte8R, Brushes.Black, 30, 150)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, FontesImpressao.Fonte8R, Brushes.Black, 300, 150)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, FontesImpressao.Fonte8R, Brushes.Black, 30, 160)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, FontesImpressao.Fonte8R, Brushes.Black, 300, 160)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, FontesImpressao.Fonte8R, Brushes.Black, 30, 170)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, FontesImpressao.Fonte8R, Brushes.Black, 300, 170)
            End If
            Dim sDescrTotal As String
            sDescrTotal = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal & sTotalBruto & sTotalTara & sTotalLiqu, FontesImpressao.Fonte8B, Brushes.Black, 29, 180)
            e.Graphics.DrawString("Camión: " & sEquipamento, FontesImpressao.Fonte8R, Brushes.Black, 29, 190)
            e.Graphics.DrawString("Transportista: " & sTransportCM, FontesImpressao.Fonte8R, Brushes.Black, 30, 200)
            e.Graphics.DrawString("Frente: " & sFrenTransp, FontesImpressao.Fonte8R, Brushes.Black, 30, 210)
            e.Graphics.DrawString("Cosechadora: " & sColhedora, FontesImpressao.Fonte8R, Brushes.Black, 180, 210)
            e.Graphics.DrawString("Cuartero: " & sTrator, FontesImpressao.Fonte8R, Brushes.Black, 320, 210)
            e.Graphics.DrawString("Tipo Caña:" & sTipoCana, FontesImpressao.Fonte8R, Brushes.Black, 440, 210)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, FontesImpressao.Fonte8R, Brushes.Black, 29, 220)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, FontesImpressao.Fonte8R, Brushes.Black, 29, 230)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, FontesImpressao.Fonte8R, Brushes.Black, 29, 240)
            End If


            Dim wPen01 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen01, 25, 25, 570, 235)

            'Segunda Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", FontesImpressao.Fonte11R, Brushes.Black, 100, 285)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte11B, Brushes.Black, 100, 305)
            e.Graphics.DrawString("Nº " & vInstLogin & "-" & vGlobal.gs03, FontesImpressao.Fonte8R, Brushes.Black, 400, 335)

            e.Graphics.DrawString("Origen    " & sFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 355)
            e.Graphics.DrawString("Orden de Carga " & sOrdemCarga, FontesImpressao.Fonte8R, Brushes.Black, 400, 355)
            e.Graphics.DrawString("Cañero    " & sFornecedor, FontesImpressao.Fonte8R, Brushes.Black, 30, 365)
            e.Graphics.DrawString("Entrada   " & sHrEntrada, FontesImpressao.Fonte8R, Brushes.Black, 30, 375)
            e.Graphics.DrawString("Salida    " & sHrSaida, FontesImpressao.Fonte8R, Brushes.Black, 250, 375)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", FontesImpressao.Fonte8B, Brushes.Black, 300, 395)
            e.Graphics.DrawString("Liberación    Remito        ", FontesImpressao.Fonte8B, Brushes.Black, 30, 395)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, FontesImpressao.Fonte8R, Brushes.Black, 30, 405)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, FontesImpressao.Fonte8R, Brushes.Black, 300, 405)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, FontesImpressao.Fonte8R, Brushes.Black, 30, 415)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, FontesImpressao.Fonte8R, Brushes.Black, 300, 415)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, FontesImpressao.Fonte8R, Brushes.Black, 30, 425)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, FontesImpressao.Fonte8R, Brushes.Black, 300, 425)
            End If
            Dim sDescrTotal2 As String
            sDescrTotal2 = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal2 & sTotalBruto & sTotalTara & sTotalLiqu, FontesImpressao.Fonte8B, Brushes.Black, 29, 435)
            e.Graphics.DrawString("Camión " & sEquipamento, FontesImpressao.Fonte8R, Brushes.Black, 29, 445)
            e.Graphics.DrawString("Transportista " & sTransportCM, FontesImpressao.Fonte8R, Brushes.Black, 30, 455)
            e.Graphics.DrawString("Frente " & sFrenTransp, FontesImpressao.Fonte8R, Brushes.Black, 30, 465)
            e.Graphics.DrawString("Cosechadora " & sColhedora, FontesImpressao.Fonte8R, Brushes.Black, 180, 465)
            e.Graphics.DrawString("Cuartero " & sTrator, FontesImpressao.Fonte8R, Brushes.Black, 320, 465)
            e.Graphics.DrawString("Tipo Caña" & sTipoCana, FontesImpressao.Fonte8R, Brushes.Black, 440, 465)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, FontesImpressao.Fonte8R, Brushes.Black, 29, 475)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, FontesImpressao.Fonte8R, Brushes.Black, 29, 485)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, FontesImpressao.Fonte8R, Brushes.Black, 29, 495)
            End If

            Dim wPen02 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen02, 25, 280, 570, 235)

            'Terceira Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", FontesImpressao.Fonte11R, Brushes.Black, 100, 540)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte11B, Brushes.Black, 100, 560)
            e.Graphics.DrawString("Nº " & vInstLogin & "-" & vGlobal.gs03, FontesImpressao.Fonte8R, Brushes.Black, 400, 590)

            e.Graphics.DrawString("Origen    " & sFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 610)
            e.Graphics.DrawString("Orden de Carga " & sOrdemCarga, FontesImpressao.Fonte8R, Brushes.Black, 400, 610)
            e.Graphics.DrawString("Cañero    " & sFornecedor, FontesImpressao.Fonte8R, Brushes.Black, 30, 620)
            e.Graphics.DrawString("Entrada   " & sHrEntrada, FontesImpressao.Fonte8R, Brushes.Black, 30, 630)
            e.Graphics.DrawString("Salida    " & sHrSaida, FontesImpressao.Fonte8R, Brushes.Black, 250, 630)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", FontesImpressao.Fonte8B, Brushes.Black, 300, 650)
            e.Graphics.DrawString("Liberación    Remito        ", FontesImpressao.Fonte8B, Brushes.Black, 30, 650)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, FontesImpressao.Fonte8R, Brushes.Black, 30, 660)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, FontesImpressao.Fonte8R, Brushes.Black, 300, 660)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, FontesImpressao.Fonte8R, Brushes.Black, 30, 670)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, FontesImpressao.Fonte8R, Brushes.Black, 300, 670)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, FontesImpressao.Fonte8R, Brushes.Black, 30, 680)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, FontesImpressao.Fonte8R, Brushes.Black, 300, 680)
            End If
            Dim sDescrTotal3 As String
            sDescrTotal3 = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal3 & sTotalBruto & sTotalTara & sTotalLiqu, FontesImpressao.Fonte8B, Brushes.Black, 29, 690)
            e.Graphics.DrawString("Camión " & sEquipamento, FontesImpressao.Fonte8R, Brushes.Black, 29, 700)
            e.Graphics.DrawString("Transportista " & sTransportCM, FontesImpressao.Fonte8R, Brushes.Black, 30, 710)
            e.Graphics.DrawString("Frente " & sFrenTransp, FontesImpressao.Fonte8R, Brushes.Black, 30, 720)
            e.Graphics.DrawString("Cosechadora " & sColhedora, FontesImpressao.Fonte8R, Brushes.Black, 180, 720)
            e.Graphics.DrawString("Cuartero " & sTrator, FontesImpressao.Fonte8R, Brushes.Black, 320, 720)
            e.Graphics.DrawString("Tipo Caña" & sTipoCana, FontesImpressao.Fonte8R, Brushes.Black, 440, 720)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, FontesImpressao.Fonte8R, Brushes.Black, 29, 730)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, FontesImpressao.Fonte8R, Brushes.Black, 29, 740)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, FontesImpressao.Fonte8R, Brushes.Black, 29, 750)
            End If

            Dim wPen03 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen03, 25, 535, 570, 235)

            'Quarta Via
            e.Graphics.DrawString("EXTRACTO DE PESAJE", FontesImpressao.Fonte11R, Brushes.Black, 100, 800)
            e.Graphics.DrawString(vGlobal.sNomeEmpresa, FontesImpressao.Fonte11B, Brushes.Black, 100, 820)
            e.Graphics.DrawString("Nº " & vInstLogin & "-" & vGlobal.gs03, FontesImpressao.Fonte8R, Brushes.Black, 400, 850)

            e.Graphics.DrawString("Origen    " & sFazenda, FontesImpressao.Fonte8R, Brushes.Black, 30, 870)
            e.Graphics.DrawString("Orden de Carga " & sOrdemCarga, FontesImpressao.Fonte8R, Brushes.Black, 400, 870)
            e.Graphics.DrawString("Cañero    " & sFornecedor, FontesImpressao.Fonte8R, Brushes.Black, 30, 880)
            e.Graphics.DrawString("Entrada   " & sHrEntrada, FontesImpressao.Fonte8R, Brushes.Black, 30, 890)
            e.Graphics.DrawString("Salida    " & sHrSaida, FontesImpressao.Fonte8R, Brushes.Black, 250, 890)
            e.Graphics.DrawString("Bruto         Tara          Neto          ", FontesImpressao.Fonte8B, Brushes.Black, 300, 910)
            e.Graphics.DrawString("Liberación    Remito        ", FontesImpressao.Fonte8B, Brushes.Black, 30, 910)
            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString(sNoLiberacao1 & "" & sNumeroRemito1, FontesImpressao.Fonte8R, Brushes.Black, 30, 920)
                e.Graphics.DrawString(sQtBruto1 & "" & sQtTara1 & "" & sQtLiquido1, FontesImpressao.Fonte8R, Brushes.Black, 300, 920)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString(sNoLiberacao2 & "" & sNumeroRemito2, FontesImpressao.Fonte8R, Brushes.Black, 30, 930)
                e.Graphics.DrawString(sQtBruto2 & "" & sQtTara2 & "" & sQtLiquido2, FontesImpressao.Fonte8R, Brushes.Black, 300, 930)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString(sNoLiberacao3 & "" & sNumeroRemito3, FontesImpressao.Fonte8R, Brushes.Black, 30, 940)
                e.Graphics.DrawString(sQtBruto3 & "" & sQtTara3 & "" & sQtLiquido3, FontesImpressao.Fonte8R, Brushes.Black, 300, 940)
            End If
            Dim sDescrTotal4 As String
            sDescrTotal4 = "Total".PadRight(39, " ")
            e.Graphics.DrawString(sDescrTotal4 & sTotalBruto & sTotalTara & sTotalLiqu, FontesImpressao.Fonte8B, Brushes.Black, 29, 950)
            e.Graphics.DrawString("Camión " & sEquipamento, FontesImpressao.Fonte8R, Brushes.Black, 29, 960)
            e.Graphics.DrawString("Transportista " & sTransportCM, FontesImpressao.Fonte8R, Brushes.Black, 30, 970)
            e.Graphics.DrawString("Frente " & sFrenTransp, FontesImpressao.Fonte8R, Brushes.Black, 30, 980)
            e.Graphics.DrawString("Cosechadora " & sColhedora, FontesImpressao.Fonte8R, Brushes.Black, 180, 980)
            e.Graphics.DrawString("Cuartero " & sTrator, FontesImpressao.Fonte8R, Brushes.Black, 320, 980)
            e.Graphics.DrawString("Tipo Caña" & sTipoCana, FontesImpressao.Fonte8R, Brushes.Black, 440, 980)

            If sNoSequencia1 = 1 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao1_ & " Orden Corte: " & sOrdemCorte1 & " Finca: " & sNivel11 &
                                      " Seccion: " & sNivel21 & " Tablón: " & sNivel31 & " Tarjeta: " & sNoCartao1, FontesImpressao.Fonte8R, Brushes.Black, 29, 990)
            End If
            If sNoSequencia2 = 2 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao2_ & " Orden Corte: " & sOrdemCorte2 & " Finca: " & sNivel12 &
                                      " Seccion: " & sNivel22 & " Tablón: " & sNivel32 & " Tarjeta: " & sNoCartao2, FontesImpressao.Fonte8R, Brushes.Black, 29, 1000)
            End If
            If sNoSequencia3 = 3 Then
                e.Graphics.DrawString("Liberación: " & sNoLiberacao3_ & " Orden Corte: " & sOrdemCorte3 & " Finca: " & sNivel13 &
                                      " Seccion: " & sNivel23 & " Tablón: " & sNivel33 & " Tarjeta: " & sNoCartao3, FontesImpressao.Fonte8R, Brushes.Black, 29, 1010)
            End If

            Dim wPen04 As New Pen(Color.Black, 2)
            e.Graphics.DrawRectangle(wPen03, 25, 795, 570, 235)

            frmReimpressao.bImpressoOrdemCargas = True

        Catch ex As Exception
            'vGlobal.vMensangem = ex.ToString
            'frmMensagem.Show()
            'vGlobal.vMensangem = Nothing
            fGravaError("frmReimpressao", "PrintExtratoSaida_Balcanes_PrintPage", ex.ToString, "LOG_EXS_ERROR")
        End Try
    End Sub

    '[TESTE]
    Public WithEvents PrintTestes As New PrintDocument()
    Private Sub PrintTestes_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintTestes.PrintPage
        Dim texto As String

        texto += "Inicio do Teste" & vbCrLf
        texto += " " & vbCrLf & vbCrLf & vbCrLf
        texto += "Final do Teste" & vbCrLf
        e.Graphics.DrawString(texto, FontesImpressao.Fonte10R, Brushes.Black, 2, 10)


    End Sub

    Public Sub ImprTela(documento As PrintDocument)
        Impressoes.PrintTela.Document = documento
        Impressoes.PrintTela.ShowDialog()
    End Sub

    Public Function FUltimaViagem(nCaminhao As Integer, sStatusCarga As String, tblcargas As String, tblrecursos As String)
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = $"select max(C.no_liberacao) no_liberacao from {tblcargas} c, {tblrecursos} r" & vbNewLine
            sql += "where c.instancia = r.instancia" & vbNewLine
            sql += "  and c.no_liberacao = r.no_liberacao" & vbNewLine
            sql += "  and r.cd_tp_recurso = 'CM'" & vbNewLine
            sql += "  and r.cd_equipto = " & nCaminhao & vbNewLine
            sql += "  and c.instancia = '" & vInstLogin & "'" & vbNewLine
            sql += "  and c.fg_status in ('" & sStatusCarga.ToString.Replace(",", "','") & "')"
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("no_liberacao")
            End While
            fCmd(sql).Dispose()
            rs.Close()
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

    Public Function FIdViagem(nLiberacao As Integer, tblcargas As String) As Integer?
        Try
            Dim sql As New StringBuilder
            sql.AppendLine($"select distinct id_viagem from {tblcargas}")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and no_liberacao = :no_liberacao")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = nLiberacao
                Using rs As OracleDataReader = cmd.ExecuteReader
                    If rs.Read AndAlso Not rs.IsDBNull(rs.GetOrdinal("id_viagem")) Then
                        Return Convert.ToInt32(rs("id_viagem"))
                    End If
                End Using
            End Using
            Return Nothing
        Catch ex As Exception
            fGravaError("frmReimpressao", "fIdViagem", ex.ToString, "LOG_ERROR")
            Return Nothing
        End Try
    End Function

    Public Function RetratoPaisagemBematech(Impressao, Tipo) As PrinterSettings
        If Impressao = "BOLETIM_ENTRADA" Then
            PrintBolEntr_Laredo.DocumentName = "BoletínPatio"
            If Tipo = "R" Then
                PrintBolEntr_Laredo.DefaultPageSettings.Landscape = False
            ElseIf Tipo = "P" Then
                PrintBolEntr_Laredo.DefaultPageSettings.Landscape = True
            End If
        ElseIf Impressao = "BOLETIM_SAIDA" Then
            PrintExtratoSaida_Laredo.DocumentName = "ExtractoSalida"
            If Tipo = "R" Then
                PrintExtratoSaida_Laredo.DefaultPageSettings.Landscape = False
            ElseIf Tipo = "P" Then
                PrintExtratoSaida_Laredo.DefaultPageSettings.Landscape = True
            End If
        ElseIf Impressao = "BOLETIM_LABORATORIO" Then
            PrintBolAnl_Laredo.DocumentName = "BoletínLaboratório"
            If Tipo = "R" Then
                PrintBolAnl_Laredo.DefaultPageSettings.Landscape = False
            ElseIf Tipo = "P" Then
                PrintBolAnl_Laredo.DefaultPageSettings.Landscape = True
            End If
        ElseIf Impressao = "EXTRATO_PRODUTO" Then
            PrintPesagProdutos_Laredo.DocumentName = "ExtractoPesajeProducto"
            If Tipo = "R" Then
                PrintPesagProdutos_Laredo.DefaultPageSettings.Landscape = False
            ElseIf Tipo = "P" Then
                PrintPesagProdutos_Laredo.DefaultPageSettings.Landscape = True
            End If
        ElseIf Impressao = "TESTE" Then
            'PrintTestes
            PrintTestes.DocumentName = "ImpressaoDeTestes"
            If Tipo = "R" Then
                PrintTestes.DefaultPageSettings.Landscape = False
            ElseIf Tipo = "P" Then
                PrintTestes.DefaultPageSettings.Landscape = True
            End If
        End If
    End Function

    Public Function FVerificaImpressora(impressora As String) As Boolean
        Try
            For Each printer As String In PrinterSettings.InstalledPrinters
                If printer.Equals(impressora, StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next
        Catch ex As Exception
            vMensangem = Nothing
            vMensangem = ex.ToString
            frmMensagem.Show()
        End Try
    End Function

    Public Function FAvancaImprTermica(NumeroAvanco As Integer) As String
        Dim Avanco As String
        For i = 1 To NumeroAvanco
            Avanco += " " & vbCrLf
        Next
        Return Avanco
    End Function

    Public Sub SendPrinterCommand(printerName As String, command As Byte())
        Dim hPrinter As IntPtr = IntPtr.Zero
        If OpenPrinter(printerName, hPrinter, IntPtr.Zero) Then
            Dim pBytes As IntPtr = Marshal.AllocHGlobal(command.Length)
            Marshal.Copy(command, 0, pBytes, command.Length)
            Dim dwWritten As Integer
            WritePrinter(hPrinter, pBytes, command.Length, dwWritten)
            Marshal.FreeHGlobal(pBytes)
            ClosePrinter(hPrinter)
        End If
    End Sub

    Private Sub AvancarEExecutarCorte(printerName As String, distanciaMM As Integer)
        Dim hPrinter As IntPtr = IntPtr.Zero
        If OpenPrinter(printerName, hPrinter, IntPtr.Zero) Then
            ' Converter milímetros para pontos de impressão
            Dim avancarPontos As Integer = distanciaMM * 8 ' Aproximadamente 8 pontos por mm

            ' Criar o buffer de comandos: avanço e corte de papel
            Dim comandos As Byte() = {&H1B, &H64, CByte(avancarPontos), &H1B, &H69} ' ESC d n para avanço e ESC i para corte

            ' Alocar memória e copiar os comandos
            Dim pBytes As IntPtr = Marshal.AllocHGlobal(comandos.Length)
            Marshal.Copy(comandos, 0, pBytes, comandos.Length)
            Dim dwWritten As Integer

            ' Enviar os comandos para a impressora
            WritePrinter(hPrinter, pBytes, comandos.Length, dwWritten)
            Marshal.FreeHGlobal(pBytes)
            ClosePrinter(hPrinter)
        End If
    End Sub

    Public Sub SendCustomBytes(printerName As String, customBytes As Byte())
        Dim hPrinter As IntPtr = IntPtr.Zero
        If OpenPrinter(printerName, hPrinter, IntPtr.Zero) Then
            Dim pBytes As IntPtr = Marshal.AllocHGlobal(customBytes.Length)
            Marshal.Copy(customBytes, 0, pBytes, customBytes.Length)
            Dim dwWritten As Integer
            WritePrinter(hPrinter, pBytes, customBytes.Length, dwWritten)
            Marshal.FreeHGlobal(pBytes)
            ClosePrinter(hPrinter)
        End If
    End Sub

    Public Sub SendCutCommand(printerName As String)
        Dim hPrinter As IntPtr = IntPtr.Zero
        If OpenPrinter(printerName, hPrinter, IntPtr.Zero) Then
            ' Comandos de inicialização e corte da impressora
            Dim initCommand As Byte() = {CByte(29), CByte(86), CByte(48)}
            Dim cutCommand As Byte() = {CByte(27), CByte(119)} ' ESC i - Cortar papel

            Dim commands As Byte() = initCommand.Concat(cutCommand).ToArray()

            Dim pBytes As IntPtr = Marshal.AllocHGlobal(commands.Length)
            Marshal.Copy(commands, 0, pBytes, commands.Length)
            Dim dwWritten As Integer
            WritePrinter(hPrinter, pBytes, commands.Length, dwWritten)
            Marshal.FreeHGlobal(pBytes)
            ClosePrinter(hPrinter)
        End If
    End Sub

    Private Sub EnviarComando(hPrinter As IntPtr, command As Byte())
        Dim pBytes As IntPtr = Marshal.AllocHGlobal(command.Length)
        Marshal.Copy(command, 0, pBytes, command.Length)
        Dim dwWritten As Integer
        WritePrinter(hPrinter, pBytes, command.Length, dwWritten)
        Marshal.FreeHGlobal(pBytes)
    End Sub

    Private Sub EnviarComandoCorte(printerName As String)
        Dim hPrinter As IntPtr = IntPtr.Zero
        If OpenPrinter(printerName, hPrinter, IntPtr.Zero) Then
            ' Comandos de inicialização e corte da impressora
            Dim initCommand As Byte() = {&H1B, &H40} ' ESC @ - Inicializa a impressora
            Dim cutCommand As Byte() = {&H1B, &H69} ' ESC i - Comando de corte

            ' Enviar comando de inicialização
            EnviarComando(hPrinter, initCommand)
            ' Enviar comando de corte
            EnviarComando(hPrinter, cutCommand)

            ClosePrinter(hPrinter)
        Else
            MessageBox.Show("Erro ao abrir a impressora.")
        End If
    End Sub


    '[Dictionary para busca de locais da ordem de pesagem]
    Private DicNiveisPrd As New Dictionary(Of (String, Integer), (nivel1_ori As String, nivel2_ori As String, nivel3_ori As String,
                                                                  nivel1_des As String, nivel2_des As String, nivel3_des As String))

    Private Function FDicNiveisPrd(nopesagem As Integer) As Dictionary(Of (String, Integer), (String, String, String, String, String, String))
        Try
            Dim result As New Dictionary(Of (String, Integer), (String, String, String, String, String, String))
            Dim sql As New StringBuilder
            sql.AppendLine("select instancia,no_pesagem,")
            sql.AppendLine("nvl(trim(cd_upnivel1_o),'0') cd_upnivel1_o,nvl(trim(cd_upnivel2_o),'0') cd_upnivel2_o,nvl(trim(cd_upnivel3_o),'0') cd_upnivel3_o,")
            sql.AppendLine("nvl(trim(cd_upnivel1_d),'0') cd_upnivel1_d,nvl(trim(cd_upnivel2_d),'0') cd_upnivel2_d,nvl(trim(cd_upnivel3_d),'0') cd_upnivel3_d")
            sql.AppendLine("from outr_pesagens_prd")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("and no_pesagem = :no_pesagem")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_pesagem", OracleDbType.Int32).Value = nopesagem
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        Dim vinstancia As String = rs("instancia")
                        Dim vnopesagem As Integer = rs("no_pesagem")
                        Dim nivel1ori As String = rs("cd_upnivel1_o")
                        Dim nivel2ori As String = rs("cd_upnivel2_o")
                        Dim nivel3ori As String = rs("cd_upnivel3_o")
                        Dim nivel1des As String = rs("cd_upnivel1_d")
                        Dim nivel2des As String = rs("cd_upnivel2_d")
                        Dim nivel3des As String = rs("cd_upnivel3_d")
                        If Not result.ContainsKey((vinstancia, vnopesagem)) Then
                            result.Add((vinstancia, vnopesagem), (nivel1ori, nivel2ori, nivel3ori, nivel1des, nivel2des, nivel3des))
                        End If
                    End While
                End Using
            End Using
            Return result
        Catch ex As OracleException
            FMensagem($"OracleErr>> Impressoes.FDicNiveisPrd: {ex.ToString}", "C")
        Catch ex As Exception
            FMensagem($"Impressoes.FDicNiveisPrd: {ex.ToString}", "C")
        End Try
    End Function

    Private Function FBuscaNiveisPrd(Pesagem, Nivel, Tipo) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            Dim swhere As String = Nothing

            If Nivel = "1" And Tipo = "O" Then
                swhere = " nvl(trim(cd_upnivel1_o),'0') "
            ElseIf Nivel = "1" And Tipo = "D" Then
                swhere = " nvl(trim(cd_upnivel1_d),'0') "
            ElseIf Nivel = "2" And Tipo = "O" Then
                swhere = " nvl(trim(cd_upnivel2_o),'0') "
            ElseIf Nivel = "2" And Tipo = "D" Then
                swhere = " nvl(trim(cd_upnivel2_d),'0') "
            ElseIf Nivel = "3" And Tipo = "O" Then
                swhere = " nvl(trim(cd_upnivel3_o),'0') "
            ElseIf Nivel = "3" And Tipo = "D" Then
                swhere = " nvl(trim(cd_upnivel3_d),'0') "
            End If


            sql = "select" & swhere & "resultado from outr_pesagens_prd" & vbNewLine &
                  "where instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine &
                  "  and no_pesagem = " & Pesagem & ""

            fGravaLogDebug("***[Inicio - FBuscaNiveisPrd]***", "LOG_DEBUG")
            fGravaLogDebug(sql, "LOG_DEBUG")
            fGravaLogDebug("***[Fim - FBuscaNiveisPrd]***", "LOG_DEBUG")


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

        End Try
    End Function

    '[Dictionary para busca de recursos da ordem de pesagem]
    Private DicRecursosPrd As New Dictionary(Of (String, Integer), (cdequipto As Integer, cdoperador As Integer, cdtprecurso As String))
    Private Function FDicRecursosPrd(pesagem As Integer) As Dictionary(Of (String, Integer), (Integer, Integer, String))
        Try
            Dim result As New Dictionary(Of (String, Integer), (Integer, Integer, String))
            Dim sql As New StringBuilder
            sql.AppendLine("select instancia,no_pesagem, nvl(cd_equipto,0) cd_equipto,")
            sql.AppendLine("nvl(cd_operador,0) cd_operador, cd_tp_recurso from outr_pesagens_rec")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("and no_pesagem = :no_pesagem")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_pesagem", OracleDbType.Int32).Value = pesagem
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        Dim vinstancia As String = rs("instancia")
                        Dim vnopesagem As Integer = rs("no_pesagem")
                        Dim vcdequipto As Integer = rs("cd_equipto")
                        Dim vcdoperador As Integer = rs("cd_operador")
                        Dim vcdtprecurso As String = rs("cd_tp_recurso")
                        If Not result.ContainsKey((vinstancia, vnopesagem)) Then
                            result.Add((vinstancia, vnopesagem), (vcdequipto, vcdoperador, vcdtprecurso))
                        End If
                    End While
                End Using
            End Using
            Return result
        Catch ex As OracleException
            FMensagem($"OracleErr>> Impressoes.FDicRecursosPrd: {ex.ToString}", "C")
        Catch ex As Exception
            FMensagem($"Impressoes.FDicRecursosPrd: {ex.ToString}", "C")
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
                        newTexto = texto.Substring(0, tamObservacao)
                    Else
                        newTexto = texto.Substring(0, tamStr) & vbNewLine
                    End If
                ElseIf i >= 1 And i < nLinhas Then
                    newTexto += texto.Substring(i * tamStr, If(tamObservacao - (i * tamStr) < tamStr, tamObservacao - (i * tamStr), tamStr)) & vbNewLine
                    alturaLinha += 0
                Else
                    If tamObservacao - (i * tamStr) >= 1 Then
                        newTexto += texto.Substring(i * tamStr, tamObservacao - (i * tamStr))
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

    Public Function FBuscaPlaca(equipamento, liberacao) As String
        Try
            Dim sql As New StringBuilder
            sql.AppendLine("select ")
            sql.AppendLine("case when r.no_compl is not null then r.no_compl")
            sql.AppendLine("when e.no_placa is not null then e.no_placa")
            sql.AppendLine("else '0'")
            sql.AppendLine("end no_placa")
            sql.AppendLine("from apt_cargas_rec r")
            sql.AppendLine("inner join equiptos e on e.cd_equipto = r.cd_equipto")
            sql.AppendLine("where e.cd_equipto = :cd_equipto")
            sql.AppendLine("and r.no_liberacao = :no_liberacao")
            sql.AppendLine("and r.instancia = :instancia")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("cd_equipto", OracleDbType.Int32).Value = equipamento
                cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = liberacao
                Dim rs As Object = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                    Return rs
                Else
                    Return 0
                End If
            End Using
        Catch ex As Exception
            vMensangem += ex.ToString & vbNewLine
            frmMensagem.Show()
        End Try
    End Function

    Public Function FBuscaMotorista(equipamento As Integer, liberacao As Integer, TipoDado As String) As String
        Try
            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.AppendLine("select nvl(f.cd_func,0) cd_func,nvl(f.de_func,0) de_func")
            sql.AppendLine("from apt_cargas_rec r")
            sql.AppendLine("inner join funcionars f on f.cd_func = r.cd_operador")
            sql.AppendLine("where r.cd_tp_recurso = 'CM'")
            sql.AppendLine("  and r.cd_equipto = :cd_equipto")
            sql.AppendLine("  and r.no_liberacao = :no_liberacao")
            sql.AppendLine("  and r.instancia = :instancia")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("cd_equipto", OracleDbType.Int32).Value = equipamento
                cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = liberacao
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        Select Case TipoDado
                            Case "D" : result = rs("cd_func") & " " & rs("de_func")
                            Case "C" : result = rs("cd_func")
                            Case Else : result = Nothing
                        End Select
                    End While
                End Using
            End Using
            Return result
        Catch ex As Exception
            vMensangem += ex.ToString & vbNewLine
            frmMensagem.Show()
        End Try
    End Function

    Private Function FBuscaHrEntrada(Liberacao As Integer) As String
        Try
            fGravaLogDebug($"{Now()} 3.Inicio da busca da hora de entrada", "LOG_TESTE")
            Dim result As String = Nothing
            Dim sql As New StringBuilder
            sql.AppendLine("select to_char(hr_entrada,'dd/mm/yyyy hh24:mi:ss') hr_entrada from apt_cargas")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("and no_liberacao in (:no_liberacao)")
            sql.AppendLine("and rownum = 1")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = Liberacao
                Dim rs As Object = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                    fGravaLogDebug($"{Now()} 3(1).Hora de entrada {rs}", "LOG_TESTE")
                    Return rs
                Else
                    Return 0
                End If
            End Using
            fGravaLogDebug($"{Now()} 4.Final da busca da hora de entrada", "LOG_TESTE")
        Catch ex As Exception
            vGlobal.vMensangem += ex.ToString
            fGravaError("frmReimpressao", "FBuscaHrEntrada", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
        End Try
    End Function

    Private Function FTotalEquiptoExtSaida(operador, hrInicial, hrFinal) As String
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql += "select sum(qt_liquido) - (sum(desc_impur) + sum(desc_sacar)) qt_liquido" & vbNewLine
            sql += "  from (select r.qt_liquido," & vbNewLine
            sql += "               nvl(c.pe_desc_impur, 0) / 100 pe_impur," & vbNewLine
            sql += "               nvl(c.pe_desc_sacar, 0) / 100 pe_sacar," & vbNewLine
            sql += "               r.qt_liquido * (nvl(c.pe_desc_impur, 0) / 100) desc_impur," & vbNewLine
            sql += "               r.qt_liquido * (nvl(c.pe_desc_sacar, 0) / 100) desc_sacar" & vbNewLine
            sql += "from apt_cargas c" & vbNewLine
            sql += "inner join apt_cargas_rec r on c.instancia = r.instancia and c.no_liberacao = r.no_liberacao" & vbNewLine
            sql += " where r.cd_operador = " & operador & vbNewLine
            sql += "   and r.instancia = '" & vGlobal.vInstLogin & "'" & vbNewLine
            sql += "   and c.hr_entrada between" & vbNewLine
            sql += "   to_date('" & hrInicial & "', 'dd/mm/yyyy hh24:mi:ss') and" & vbNewLine
            sql += "   to_date('" & hrFinal & "', 'dd/mm/yyyy hh24:mi:ss')" & vbNewLine
            sql += "   and r.cd_tp_recurso = 'CM')"

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

    Public Function FValidaStatuLiberacao(noliberacao As Integer) As Boolean
        Try
            Dim sql As New StringBuilder
            sql.AppendLine("select no_liberacao from apt_cargas where instancia = :instancia and no_liberacao = :noliberacao")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("noliberacao", OracleDbType.Int32).Value = noliberacao
                Using rs As OracleDataReader = cmd.ExecuteReader
                    If rs.Read Then
                        Return True
                    Else
                        FMensagem($"Liberacín {noliberacao} inválido.", "I")
                        Return False
                    End If
                End Using
            End Using
        Catch ex As Exception
            FMensagem($"Error{vbCrLf}FValidaStatuLiberacao: {ex.ToString}.", "C")
        End Try
    End Function

    Public Function FVerificaSaidaLaredo() As Boolean
        Try

            hrEntradaCanaLaredo = Nothing
            Dim bImprimir As Boolean = False
            Dim sql As New StringBuilder
            sql.AppendLine("with")
            sql.AppendLine("param as (select instancia, valor from parametros")
            sql.AppendLine("where secao = :secao")
            sql.AppendLine("and entrada = :entrada)")
            sql.AppendLine("select to_char(c.hr_entrada,'dd/mm/yyyy hh24:mi:ss') hr_entrada,")
            sql.AppendLine("       to_char(c.hr_saida,'dd/mm/yyyy hh24:mi:ss') hr_saida")
            sql.AppendLine("from apt_cargas c")
            sql.AppendLine("inner join param p on p.instancia = c.instancia")
            sql.AppendLine("where c.instancia = :instancia")
            sql.AppendLine("and c.fg_status in ('C','P')")
            'sql.AppendLine("and c.pfg_impr_extr_saida = :pfg_impr_extr_saida")
            sql.AppendLine("and c.dt_movimento > p.valor")
            sql.AppendLine("and c.hr_saida > :hr_saida")
            'sql.AppendLine("and c.no_liberacao > :no_liberacao")
            'sql.AppendLine("and rownum = 1")
            'sql.AppendLine("order by 1 desc")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.AddRange({
                New OracleParameter("instancia", OracleDbType.Varchar2) With {.Value = vInstLogin},
                New OracleParameter("secao", OracleDbType.Varchar2) With {.Value = "RCMP"},
                New OracleParameter("entrada", OracleDbType.Varchar2) With {.Value = "DT_PROCESSO"},
                New OracleParameter("hr_saida", OracleDbType.Date) With {.Value = vGlobal.DtHrSaida}
                })
                'New OracleParameter("pfg_impr_extr_saida", OracleDbType.Varchar2) With {.Value = "N"}

                'Dim txt As String = GetDebugSql(cmd)
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        hrEntradaCanaLaredo = rs("hr_entrada")
                        vGlobal.DtHrSaida = Convert.ToDateTime(rs("hr_saida"))
                        bImprimir = True
                    End While
                End Using
            End Using
            Return bImprimir
        Catch ex As OracleException
            FMensagem("OracleErr>> Impressoes.FVerificaSaidaLaredo: " & ex.ToString, "C")
        Catch ex As Exception
            FMensagem("Impressoes.FVerificaSaidaLaredo: " & ex.ToString, "C")
        End Try
    End Function

    Private Function FBuscaCarretaLiberacao(NoLiberacao) As String
        Try
            Dim result As String
            Dim sql As String
            Dim rs As OracleDataReader
            sql += "select cd_equipto from apt_cargas_rec" & vbNewLine
            sql += "where instancia = '" & vInstLogin & "'" & vbNewLine
            sql += "  and no_liberacao = " & NoLiberacao & vbNewLine
            sql += "  and cd_tp_recurso = 'CT'"

            rs = fCmd(sql).ExecuteReader
            While rs.Read
                result = rs("cd_equipto")
            End While
            fCmd(sql).Dispose()
            rs.Close()
            If result = Nothing Then
                result = 0
            End If
            Return result
        Catch ex As Exception

        End Try
    End Function

    Private Function fListaCartoes(noQueima) As String
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
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        result = rs("id_despacho")
                    End While
                End Using
            End Using
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

    Private Sub AtualizaTablacargas(noLiberacao)
        Dim sql As String
        Dim rs As OracleDataReader
        sql = "update apt_cargas set fg_impressao_impureza = 'S' where no_liberacao = " & noLiberacao & ""
        rs = fCmd(sql).ExecuteReader
        fCmd(sql).Dispose()
        rs.Close()
    End Sub

    Private Function fPesoBrutoBolAnl(noLiberacao As Integer, tblcargas As String) As Integer?
        Try
            Dim sql As New StringBuilder
            sql.AppendLine($"select sum(nvl(qt_bruto,0)) qt_bruto from {tblcargas}")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("  and no_liberacao = :no_liberacao")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = noLiberacao
                Using rs As OracleDataReader = cmd.ExecuteReader
                    If rs.Read AndAlso Not IsDBNull(rs("qt_bruto")) Then
                        Return Convert.ToInt32(rs("qt_bruto"))
                    Else
                        Return Nothing
                    End If
                End Using
            End Using
        Catch ex As Exception
            MsgBox("fPesoBrutoBolAnl: " & ex.ToString)
            Return Nothing
        End Try
    End Function

    Private Function FEquipAbastec(CdEquipto) As Boolean
        Try
            Dim sql As String
            Dim rs As OracleDataReader
            sql += "select * from equiptos" & vbNewLine
            sql += "where cd_equipto = " & CdEquipto & ""
            sql += "  and fg_abastec = 'S'"
            rs = fCmd(sql).ExecuteReader
            If rs.HasRows = True Then
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

    Private Function fEquiptoPesagem(noLiberacao, tpRecurso)
        Try
            Dim result As String = Nothing
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select cd_equipto from apt_cargas_rec" & vbNewLine &
                  "where instancia = '" & vInstLogin & "'" & vbNewLine &
                  "  and cd_tp_recurso = '" & tpRecurso & "'" & vbNewLine &
                  "  and no_liberacao = " & noLiberacao & ""
            rs = fCmd(sql).ExecuteReader
            While rs.Read
                    result = rs("cd_equipto")
                End While
                fCmd(sql).Dispose()
                rs.Close()
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

    Private Sub MudaStatusOrdemProduto(OrdemCarga, NoPesagem)
        Using trans As OracleTransaction = conn.BeginTransaction
            Try
                Dim upd1 As New StringBuilder
                upd1.Clear()
                upd1.AppendLine("update ord_car_opes")
                upd1.AppendLine("set fg_impr_ordcar = 'S'")
                upd1.AppendLine("where instancia = :instancia")
                upd1.AppendLine("  and cd_ordem_car = :ordemcar")
                Using cmd As OracleCommand = fCmd(upd1.ToString)
                    cmd.Transaction = trans
                    cmd.Parameters.Clear()
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("ordemcar", OracleDbType.Int32).Value = CInt(OrdemCarga)
                    cmd.ExecuteNonQuery()
                End Using

                Dim upd2 As New StringBuilder
                upd2.Clear()
                upd2.AppendLine("update outr_pesagens")
                upd2.AppendLine("set pfg_impr_extrato = 'S'")
                upd2.AppendLine("where instancia = :instancia")
                upd2.AppendLine("  and no_pesagem = :pesagem")
                Using cmd As OracleCommand = fCmd(upd2.ToString)
                    cmd.Transaction = trans
                    cmd.Parameters.Clear()
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("pesagem", OracleDbType.Int32).Value = CInt(NoPesagem)
                    cmd.ExecuteNonQuery()
                End Using

                trans.Commit()
            Catch ex As Exception
                trans.Rollback()
                Throw
                MsgBox("MudaStatusOrdemProduto>>" & ex.ToString, MsgBoxStyle.Critical, "Error")
            End Try
        End Using
    End Sub

    Private Function MontaCodigoBarrasCODE128_LR2000(texto As String) As List(Of Byte)
        Dim buffer As New List(Of Byte)

        ' Configurações visuais (ESC/POS)
        buffer.AddRange(New Byte() {&H1B, &H61, 1})   ' Centraliza
        buffer.AddRange(New Byte() {&H1D, &H68, 130}) ' Altura (100 dots)
        buffer.AddRange(New Byte() {&H1D, &H77, 3})   ' Largura 2 (Recomendado: 3 pode estourar a margem na LR2000)
        buffer.AddRange(New Byte() {&H1D, &H48, 2})   ' HRI abaixo do código

        ' Prefixo {B força o Code 128 - Subset B (Texto e Números)
        ' Funciona nativamente tanto na MP-4200 quanto na LR2000
        Dim conteudoComPrefixo As String = "{B" & texto
        Dim dadosBytes As Byte() = System.Text.Encoding.ASCII.GetBytes(conteudoComPrefixo)

        ' GS k m=73 (CODE128) + n (tamanho dos dados)
        buffer.AddRange(New Byte() {&H1D, &H6B, 73, CByte(dadosBytes.Length)})
        buffer.AddRange(dadosBytes)

        Return buffer
    End Function

    Private Function MontaCodigoBarrasCODE128_MP4200HS(texto As String) As List(Of Byte)
        Dim buffer As New List(Of Byte)

        ' Configurações visuais
        buffer.AddRange(New Byte() {&H1B, &H61, 1}) ' Centraliza
        buffer.AddRange(New Byte() {&H1D, &H68, 100}) ' Altura
        buffer.AddRange(New Byte() {&H1D, &H77, 3})   ' Largura
        buffer.AddRange(New Byte() {&H1D, &H48, 2})   ' HRI abaixo

        ' GS k m (m=73 = CODE128)
        buffer.AddRange(New Byte() {&H1D, &H6B, 73})

        ' Dados ASCII direto (MP-4200HS aceita)
        Dim dadosBytes = System.Text.Encoding.ASCII.GetBytes(texto)
        buffer.Add(CByte(dadosBytes.Length))   ' tamanho dos dados
        buffer.AddRange(dadosBytes)            ' dados do código

        Return buffer
    End Function

    Public Function fValidaImprOrdemCarga() As Boolean
        Try
            Dim result As Object = Nothing
            Dim sql As New StringBuilder
            sql.AppendLine("select id_despacho from lt_despachos")
            sql.AppendLine("where instancia = :instancia")
            sql.AppendLine("and cd_unid_ind = :cd_unid_ind")
            sql.AppendLine("and fg_status = :fg_status")
            sql.AppendLine("and pfg_ordem_carga is null")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                cmd.Parameters.Add("cd_unid_ind", OracleDbType.Int32).Value = vGlobal.sUnidInd
                cmd.Parameters.Add("fg_status", OracleDbType.Varchar2).Value = "A"
                Dim rs As Object = cmd.ExecuteScalar
                If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                    vGlobal.nValor01 = rs
                    Return True
                Else
                    vGlobal.sValor01 = 0
                    Return False
                End If
            End Using
        Catch ex As OracleException
            FMensagem("OracleErr>> Impressoes.fValidaImprOrdemCarga: " & vbCrLf &
                      ex.ToString, "C")
        Catch ex As Exception
            FMensagem("Impressoes.fValidaImprOrdemCarga: " & vbCrLf &
                      ex.ToString, "C")
        End Try
    End Function

    Public Sub AtualizaDespachoOCImpresso(IdDespacho As Integer)
        Using t As OracleTransaction = conn.BeginTransaction
            Try
                Dim sql As New StringBuilder
                sql.AppendLine("update lt_despachos")
                sql.AppendLine("set pfg_ordem_carga = 'S'")
                sql.AppendLine("where instancia = :instancia")
                sql.AppendLine("  and id_despacho = :id_despacho")
                sql.AppendLine("  and pfg_ordem_carga is null")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Transaction = t
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("id_despacho", OracleDbType.Int32).Value = IdDespacho
                    cmd.ExecuteNonQuery()
                End Using
                t.Commit()
            Catch ex As OracleException
                t.Rollback()
                vGlobal.vMensangem += ex.ToString
                fGravaError("OracleErr>> frmReimpressao", "AtualizaDespachoOCImpresso", vGlobal.vMensangem, "LOG_ERROR")
                vGlobal.vMensangem = Nothing
            Catch ex As Exception
                t.Rollback()
                vGlobal.vMensangem += ex.ToString
                fGravaError("frmReimpressao", "AtualizaDespachoOCImpresso", vGlobal.vMensangem, "LOG_ERROR")
                vGlobal.vMensangem = Nothing
            End Try
        End Using
    End Sub

    Public Class Processos
        Public Shared Sub ImprimeBolAnl()
            Select Case vGlobal.sCliente
                Case "BALCANES" : Balcanes.ImpBalcanesBoletimAnalises()
            End Select
        End Sub

        Public Shared Sub ImprimeSaida()
            Select Case vGlobal.sCliente
                Case "LAREDO" : Laredo.ImprBolSaida.ImpLaredoExtratoSaida()
                Case "BALCANES" : Balcanes.ImpBalcanesExtratoSaida()
                Case "BUDEGUER" : Budeguer.ImpBudeguerExtratoSaida()
            End Select
        End Sub

        Public Shared Sub ImprimeExtrProdutos()
            Select Case vGlobal.sCliente
                Case "LAREDO" : Laredo.ImpExtrProduto_Laredo()
                Case "BUDEGUER" : Budeguer.ImpExtrProduto_Budeguer()
            End Select
        End Sub

        Public Shared Sub ImprimeOrdemCarga()
            Select Case vGlobal.sCliente
                Case "BUDEGUER" : Budeguer.ImpBudeguerOrdemCarga()
                Case "BALCANES" : Balcanes.ImpBalcanesOrdemCarga()
            End Select
        End Sub
    End Class

    Public Class Budeguer
        Public Class ImprTicketRastreio
            Public Property liberacao As Integer
            Public Property ticket As String
            Public Property caminhao As String
            Public Property caminhao2 As Integer
            Public Property fg_anl_imp As String
            Public Property transporta As String
            Public Property frente As String
            Public Property hr_guarita As Date

            Public Shared Function Dados() As List(Of ImprTicketRastreio)
                Try
                    Dim resultado As New List(Of ImprTicketRastreio)
                    Dim sWhere, sWhere2 As String
                    If frmReimpressao.bBotaoImprimir = False Then
                        sWhere += "  and c.fg_status  = :fg_status"
                        sWhere += "  and c.no_liberacao = :no_liberacao"
                        sWhere += "  and c.pfg_impr_ticket_rast = :pfg_impr_ticket_rast"
                        sWhere2 = Nothing
                    Else
                        sWhere += "  and c.pfg_impr_ticket_rast = :pfg_impr_ticket_rast"
                        sWhere += "  and c.id_despacho = :id_despacho"
                    End If

                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select c.no_liberacao liberacao,c.no_ticket_rastreio ticket,")
                    sql.AppendLine("e.cd_equipto||'-'||e.no_placa caminhao,e.cd_equipto caminhao2,fg_anl_imp,")
                    sql.AppendLine("t.cd_transp||'-'||t.de_transp transporta,")
                    sql.AppendLine("f.cd_fren_tran||'-'||f.de_fren_tran frente,c.hr_guarita")
                    sql.AppendLine("from apt_cargas c, apt_cargas_rec r, transporta t, frentransp f, equiptos e")
                    sql.AppendLine("Where e.cd_equipto = r.cd_equipto")
                    sql.AppendLine("  And f.cd_fren_tran = c.cd_fren_tran")
                    sql.AppendLine("  And t.cd_transp = e.cd_transp")
                    sql.AppendLine("  And r.instancia = c.instancia")
                    sql.AppendLine("  And r.no_liberacao = c.no_liberacao")
                    sql.AppendLine("  And c.instancia = :instancia")
                    sql.AppendLine("  and r.cd_tp_recurso = 'CM'")
                    sql.AppendLine("" & sWhere)
                    sql.AppendLine("" & sWhere2)
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        If frmReimpressao.bBotaoImprimir = False Then
                            cmd.Parameters.Add("fg_status", OracleDbType.Varchar2).Value = "G"
                            cmd.Parameters.Add("no_liberacao", OracleDbType.Int64).Value = Convert.ToInt64(vGlobal.gn20)
                            cmd.Parameters.Add("pfg_impr_ticket_rast", OracleDbType.Varchar2).Value = "N"
                        Else
                            cmd.Parameters.Add("pfg_impr_ticket_rast", OracleDbType.Varchar2).Value = "S"
                            cmd.Parameters.Add("id_despacho", OracleDbType.Int64).Value = Convert.ToInt64(frmReimpressao.tbBoletim.Text)
                        End If

                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                resultado.Add(New ImprTicketRastreio With {
                                              .liberacao = If(IsDBNull(rs("liberacao")), Nothing, rs("liberacao")),
                                              .ticket = If(IsDBNull(rs("ticket")), Nothing, rs("ticket")),
                                              .caminhao = If(IsDBNull(rs("caminhao")), Nothing, rs("caminhao")),
                                              .caminhao2 = If(IsDBNull(rs("caminhao2")), 0, rs("caminhao2")),
                                              .fg_anl_imp = If(IsDBNull(rs("fg_anl_imp")), "N", rs("fg_anl_imp")),
                                              .transporta = If(IsDBNull(rs("transporta")), Nothing, rs("transporta")),
                                              .frente = If(IsDBNull(rs("frente")), Nothing, rs("frente")),
                                              .hr_guarita = If(IsDBNull(rs("hr_guarita")), Nothing, rs("hr_guarita"))})
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Budeguer.ImprTicketRastreio.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Budeguer.ImprTicketRastreio.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

            Public Shared Sub LinhaPontilhada(g As Graphics, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)

                Dim dashValues As Single() = {2, 2}
                Using pen As New Pen(Color.Black, 1)
                    pen.DashPattern = dashValues
                    g.DrawLine(pen, x1, y1, x2, y2)
                End Using

            End Sub
        End Class

        Public Class ImprExtratoSaida
            Public Property Instancia As String
            Public Property IdViagem As Integer
            Public Property NoLiberacao As Integer
            Public Property OrdemCarga As Integer
            Public Property NumeroRemito As String
            Public Property Fornecedor As String
            Public Property Fazenda As String
            Public Property CuitFazenda As String
            Public Property Equipamento As Integer
            Public Property NoPlaca As String
            Public Property Transporta As String
            Public Property CuitTransporta As String
            Public Property DeSistColh As String
            Public Property DtSaida As Date
            Public Property HrSaida As Date
            Public Property DtMovimento As Date
            Public Property QtBruto As Double
            Public Property QtTara As Double
            Public Property QtLiquido As Double
            Public Shared Function Dados() As List(Of ImprExtratoSaida)
                Try
                    Dim resultado As New List(Of ImprExtratoSaida)

                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''
                    ''' INICIO
                    ''' Ticket #4848
                    ''' Data: 18/11/2026 
                    ''' Impresión de Orden de Carga y Extracto de Pesaje de otras zafras
                    ''''''''''''''''''''''''''''''''''''''''''''''''''''''
                    Dim tblaptcargas As String = "apt_cargas"
                    Dim tblaptcargasrec As String = "apt_cargas_rec"
                    Dim tblltdespachos As String = "lt_despachos"

                    ''' FINAL



                    Dim sWhere As String = Nothing
                    Dim sNoLiberacao As String = Nothing
                    Dim ano As String = Nothing
                    Dim valor As String = vGlobal.nRegistroImpressao
                    Dim partes() As String = Nothing
                    If vGlobal.bBotaoImprimir = True Then
                        ''''''''''''''''''''''''''''''''''''''''''''''''''''''
                        ''' INICIO
                        ''' Ticket #4848
                        ''' Data: 18/11/2026 
                        ''' Impresión de Orden de Carga y Extracto de Pesaje de otras zafras
                        ''''''''''''''''''''''''''''''''''''''''''''''''''''''
                        partes = valor.Split("*"c)
                        If partes.Count >= 2 Then
                            sNoLiberacao = partes(0)
                            ano = partes(1)

                            tblaptcargas = $"{tblaptcargas}_{ano}"
                            tblaptcargasrec = $"{tblaptcargasrec}_{ano}"
                            tblltdespachos = $"{tblltdespachos}_{ano}"
                        Else
                            sNoLiberacao = partes(0)
                        End If
                        ''' FINAL

                        If vGlobal.vIMPRLIBSEPARADA = "S" Then
                            sWhere = "  and ac.pfg_impr_extr_saida = :pfg_impr_extr_saida and ac.id_viagem = :id_viagem and ac.no_liberacao = :no_liberacao"
                        ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                            sWhere = "  and ac.pfg_impr_extr_saida = :pfg_impr_extr_saida and ac.id_viagem = :id_viagem"
                        End If

                    ElseIf vglobal.bBotaoImprimir = False Then
                        If vGlobal.vIMPRLIBSEPARADA = "S" Then
                            sWhere = "  and ac.pfg_impr_extr_saida = :pfg_impr_extr_saida and ac.no_liberacao = :no_liberacao"
                        ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                            sWhere = "  and ac.pfg_impr_extr_saida = :pfg_impr_extr_saida"
                        End If
                    End If

                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select ac.instancia,ac.id_viagem,ac.no_liberacao,")
                    sql.AppendLine("nvl(d.id_despacho, 0) ordem_carga,")
                    sql.AppendLine("nvl(d.no_remito, 0) numero_remito,")
                    sql.AppendLine("fn.cd_fornec || '-' || fn.de_fornec fornecedor,")
                    sql.AppendLine("trim(u1.cd_upnivel1) ||'-'||trim(u1.de_upnivel1) fazenda,")
                    sql.AppendLine("u1.no_cgc_cpf cuit_fazenda,eq.cd_equipto equipto,")
                    sql.AppendLine("eq.no_placa placa,t.cd_transp || '-' || t.de_transp transporta,")
                    sql.AppendLine("t.no_cgc_cpf cuit_transporta,s.de_sist_colh,")
                    sql.AppendLine("ac.dt_saida,ac.hr_saida,ac.dt_movimento,")
                    sql.AppendLine("nvl(ac.qt_bruto, 0) qt_bruto,nvl(ac.qt_tara, 0) qt_tara,")
                    sql.AppendLine("nvl(ac.qt_liquido, 0) qt_liquido")
                    sql.AppendLine($"from {tblaptcargas} ac")
                    sql.AppendLine($"inner join {tblaptcargasrec} acr on acr.instancia = ac.instancia and acr.no_liberacao = ac.no_liberacao")
                    sql.AppendLine("inner join equiptos eq on eq.cd_equipto = acr.cd_equipto")
                    sql.AppendLine("inner join upnivel1 u1 on u1.cd_upnivel1 = ac.cd_upnivel1")
                    sql.AppendLine("inner join fornecs  fn on fn.cd_fornec = u1.cd_fornec")
                    sql.AppendLine("inner join transporta t on t.cd_transp = eq.cd_transp")
                    sql.AppendLine("inner join sistcolh s on s.cd_sist_colh = ac.cd_sist_colh")
                    sql.AppendLine($"left join {tblltdespachos} d on d.instancia = ac.instancia and d.id_despacho = ac.id_despacho ")
                    sql.AppendLine("where acr.cd_tp_recurso = :cd_tp_recurso")
                    sql.AppendLine("and ac.instancia = :instancia")
                    sql.AppendLine("and ac.dt_saida is not null")
                    sql.AppendLine(sWhere)
                    sql.AppendLine("order by 2, 3")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        cmd.Parameters.Add("cd_tp_recurso", OracleDbType.Varchar2).Value = "CM"
                        If vGlobal.bBotaoImprimir = True Then
                            sNoLiberacao = frmReimpressao.tbBoletim.Text
                            If vGlobal.vIMPRLIBSEPARADA = "S" Then
                                cmd.Parameters.Add("pfg_impr_extr_saida", OracleDbType.Varchar2).Value = "S"
                                cmd.Parameters.Add("id_viagem", OracleDbType.Int64).Value = Convert.ToInt64(FIdViagem(sNoLiberacao, tblaptcargas))
                                cmd.Parameters.Add("no_liberacao", OracleDbType.Int64).Value = Convert.ToInt64(vGlobal.sValor04)
                            ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                                cmd.Parameters.Add("pfg_impr_extr_saida", OracleDbType.Varchar2).Value = "S"
                                cmd.Parameters.Add("id_viagem", OracleDbType.Int64).Value = Convert.ToInt64(FIdViagem(sNoLiberacao, tblaptcargas))
                            End If

                        ElseIf vglobal.bBotaoImprimir = False Then
                            If vGlobal.vIMPRLIBSEPARADA = "S" Then
                                cmd.Parameters.Add("pfg_impr_extr_saida", OracleDbType.Varchar2).Value = "N"
                                cmd.Parameters.Add("no_liberacao", OracleDbType.Int64).Value = Convert.ToInt64(vGlobal.sValor04)
                            ElseIf vGlobal.vIMPRLIBSEPARADA = "N" Then
                                cmd.Parameters.Add("pfg_impr_extr_saida", OracleDbType.Varchar2).Value = "N"
                            End If
                        End If

                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                resultado.Add(New ImprExtratoSaida With {
                                        .Instancia = If(IsDBNull(rs("instancia")), Nothing, rs("instancia")),
                                        .IdViagem = If(IsDBNull(rs("id_viagem")), Nothing, rs("id_viagem")),
                                        .NoLiberacao = If(IsDBNull(rs("no_liberacao")), Nothing, rs("no_liberacao")),
                                        .OrdemCarga = If(IsDBNull(rs("ordem_carga")), Nothing, rs("ordem_carga")),
                                        .NumeroRemito = If(IsDBNull(rs("numero_remito")), Nothing, rs("numero_remito")),
                                        .Fornecedor = If(IsDBNull(rs("fornecedor")), Nothing, rs("fornecedor")),
                                        .Fazenda = If(IsDBNull(rs("fazenda")), Nothing, rs("fazenda")),
                                        .CuitFazenda = If(IsDBNull(rs("cuit_fazenda")), Nothing, rs("cuit_fazenda")),
                                        .Equipamento = If(IsDBNull(rs("equipto")), Nothing, rs("equipto")),
                                        .NoPlaca = If(IsDBNull(rs("placa")), Nothing, rs("placa")),
                                        .Transporta = If(IsDBNull(rs("transporta")), Nothing, rs("transporta")),
                                        .CuitTransporta = If(IsDBNull(rs("cuit_transporta")), Nothing, rs("cuit_transporta")),
                                        .DeSistColh = If(IsDBNull(rs("de_sist_colh")), Nothing, rs("de_sist_colh")),
                                        .DtSaida = If(IsDBNull(rs("dt_saida")), Nothing, rs("dt_saida")),
                                        .HrSaida = If(IsDBNull(rs("hr_saida")), Nothing, rs("hr_saida")),
                                        .DtMovimento = If(IsDBNull(rs("dt_movimento")), Nothing, rs("dt_movimento")),
                                        .QtBruto = If(IsDBNull(rs("qt_bruto")), Nothing, rs("qt_bruto")),
                                        .QtTara = If(IsDBNull(rs("qt_tara")), Nothing, rs("qt_tara")),
                                        .QtLiquido = If(IsDBNull(rs("qt_liquido")), Nothing, rs("qt_liquido"))})
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Budeguer.ImprExtratoSaida.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Budeguer.ImprExtratoSaida.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

            Public Shared Sub AtualizaSaida(noliberacao As Integer)
                Using t As OracleTransaction = conn.BeginTransaction
                    Try
                        Dim sql As New StringBuilder
                        sql.AppendLine("update apt_cargas")
                        sql.AppendLine("set pfg_impr_extr_saida = 'S'")
                        sql.AppendLine("where instancia = :instancia")
                        sql.AppendLine("and no_liberacao = :no_liberacao")
                        sql.AppendLine("and pfg_impr_extr_saida = :pfg_impr_extr_saida")
                        Using cmd As OracleCommand = fCmd(sql.ToString)
                            cmd.Transaction = t
                            cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                            cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = noliberacao
                            cmd.Parameters.Add("pfg_impr_extr_saida", OracleDbType.Varchar2).Value = "N"
                            cmd.ExecuteNonQuery()
                        End Using
                        t.Commit()
                    Catch ex As Exception
                        t.Rollback()
                    End Try
                End Using
            End Sub
        End Class

        Public Shared Sub ImpBudeguerExtratoSaida()
            If frmReimpressao.FVerifPesagCana() = True Then
                frmReimpressao.bBotaoImprimir = False
                If vGlobal.vImprTela = "N" Then
                    PrintExtratoSaida_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                    PrintExtratoSaida_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                    PrintExtratoSaida_Budeguer.Print()
                ElseIf vGlobal.vImprTela = "S" Then
                    Impressoes.ImprTela(frmReimpressao.PrintExtratoSaida_Budeguer)
                End If
            End If
        End Sub

        Public Shared Sub ImpExtrProduto_Budeguer()
            If Not TestarConexaoSilencioso() Then
                MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
                Return
            End If
            If frmReimpressao.FVerifPesagProd() = True Then
                If FBuscaRemito(vGlobal.gn20) > 0 Then
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

        Public Shared Sub ImpBudeguerOrdemCarga()
            If Not TestarConexaoSilencioso() Then
                MsgBox("No hay conexión con el servidor de base de datos. Verifique la red o aguarde la reconexión automática.", MsgBoxStyle.Critical, "Error de Conexión")
                Return
            End If

            fGravaLog("Inicio impresión Orden de Carga", "LOG_TIMER_IMPR")
            If frmReimpressao.fVerificaImprBalcanes("lt_despachos", "pfg_ordem_carga", " and fg_status = 'A'") = True Then
                If frmReimpressao.fValImprOrdemCarga() = True Then
                    If vGlobal.vImprTela = "N" Then
                        frmReimpressao.PrintOrdemCarga_Budeguer.PrinterSettings.PrinterName = vGlobal.vNomeImpressora
                        frmReimpressao.PrintOrdemCarga_Budeguer.PrinterSettings.Copies = vGlobal.vNumeroCopias
                        frmReimpressao.PrintOrdemCarga_Budeguer.Print()
                    ElseIf vGlobal.vImprTela = "S" Then
                        Impressoes.ImprTela(frmReimpressao.PrintOrdemCarga_Budeguer)
                    End If
                    AtualizaDespachoOCImpresso(vGlobal.sValor01)
                End If
            End If
            fGravaLog("Final impresión Orden de Carga", "LOG_TIMER_IMPR")
        End Sub


        '********************************************************************************* 
        '**Valida ID Despacho valido para impressão***************************************
        '*********************************************************************************
        Public Shared Function FValidaIdDespacho(IdDespacho As Integer) As Boolean
            If Not FBuscaIdDespacho(IdDespacho) Then
                FMensagem("Orden de Carga " & IdDespacho & " inexistente", "E")
                Return False
            End If
            If Not FBuscaIdDespachoCarga(IdDespacho) Then
                FMensagem("Orden de Carga " & IdDespacho & " no vinculada a ninguna carga", "E")
                Return False
            End If
            Return True
        End Function

        Public Shared Function FBuscaIdDespacho(IdDespacho As Integer) As Boolean
            Try
                Dim sql As New StringBuilder
                sql.AppendLine("select id_despacho from lt_despachos")
                sql.AppendLine("where instancia = :instancia")
                sql.AppendLine("and id_despacho = :id_despacho")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("id_despacho", OracleDbType.Int32).Value = IdDespacho
                    Dim rs As Object = cmd.ExecuteScalar
                    If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                        Return True
                    Else
                        Return False
                    End If
                End Using
            Catch ex As OracleException
                FMensagem("OracleErr>> Impressoes.BudeguerFBuscaIdDespacho: " & ex.ToString, "C")
            Catch ex As Exception
                FMensagem("Impressoes.BudeguerFBuscaIdDespacho: " & ex.ToString, "C")
            End Try
        End Function

        Public Shared Function FBuscaIdDespachoCarga(IdDespacho As Integer) As Boolean
            Try
                Dim sql As New StringBuilder
                sql.AppendLine("select id_despacho from apt_cargas")
                sql.AppendLine("where instancia = :instancia")
                sql.AppendLine("and id_despacho = :id_despacho")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("id_despacho", OracleDbType.Int32).Value = IdDespacho
                    Dim rs As Object = cmd.ExecuteScalar
                    If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                        Return True
                    Else
                        Return False
                    End If
                End Using
            Catch ex As OracleException
                FMensagem("OracleErr>> Impressoes.FBuscaIdDespachoCarga: " & ex.ToString, "C")
            Catch ex As Exception
                FMensagem("Impressoes.FBuscaIdDespachoCarga: " & ex.ToString, "C")
            End Try
        End Function
        '*********************************************************************************

        '********************************************************************************* 
        '**Valida liberação para realizar a impressão*************************************
        '*********************************************************************************

        Public Shared Function FValidaLiberacao(NoLiberacao As Integer) As Boolean
            If Not FBuscaLiberacao(NoLiberacao) Then
                FMensagem("La liberación informada no existe", "E")
                Return False
            End If
            If Not FBuscaLiberacaoConcluida(NoLiberacao) Then
                FMensagem("La liberación informada no tiene el peso neto", "E")
                Return False
            End If
            Return True
        End Function

        Public Shared Function FBuscaLiberacao(NoLiberacao As Integer) As Boolean
            Try
                Dim sql As New StringBuilder
                sql.AppendLine("select no_liberacao from apt_cargas")
                sql.AppendLine("where instancia = :instancia")
                sql.AppendLine("and no_liberacao = :no_liberacao")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = NoLiberacao
                    Dim rs As Object = cmd.ExecuteScalar
                    If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                        Return True
                    Else
                        Return False
                    End If
                End Using
            Catch ex As OracleException
                FMensagem("OracleErr>> Impressoes.FBuscaLiberacao: " & ex.ToString, "C")
            Catch ex As Exception
                FMensagem("Impressoes.FBuscaLiberacao: " & ex.ToString, "C")
            End Try
        End Function

        Public Shared Function FBuscaLiberacaoConcluida(NoLiberacao As Integer) As Boolean
            Try
                Dim sql As New StringBuilder
                sql.AppendLine("select no_liberacao from apt_cargas")
                sql.AppendLine("where instancia = :instancia")
                sql.AppendLine("and no_liberacao = :no_liberacao")
                sql.AppendLine("and fg_status = :fg_status")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = NoLiberacao
                    cmd.Parameters.Add("fg_status", OracleDbType.Varchar2).Value = "C"
                    Dim rs As Object = cmd.ExecuteScalar
                    If rs IsNot Nothing AndAlso Not IsDBNull(rs) Then
                        Return True
                    Else
                        Return False
                    End If
                End Using
            Catch ex As OracleException
                FMensagem("OracleErr>> Impressoes.FBuscaLiberacaoConcluida: " & ex.ToString, "C")
            Catch ex As Exception
                FMensagem("Impressoes.FBuscaLiberacaoConcluida: " & ex.ToString, "C")
            End Try
        End Function
        '*********************************************************************************
    End Class

    Public Class Laredo
        Public Shared listaDados As New List(Of ImprBolEntrada)
        Public Shared listaDadosSaida As New List(Of ImprBolSaida)
        'Public Shared listaDadosExtrProduto As New List(Of ImprPesagemProdutos)
        Public Class ImprBolEntrada
            Public Property noliberacao As Integer
            Public Property hrentrada As Date
            Public Property cdtprecurso As String
            Public Property cdequiptocm As Integer
            Public Property cdequiptoct As Integer
            Public Property cdoperador As Integer
            Public Property fgimprbolentr As String
            Public Property cdupnivel1 As String
            Public Property idviagem As Integer
            Public Property fganalise As String
            Public Property noaanlise As Integer
            Public Property qtbruto As Decimal

            Public Shared Function Dados(Optional noliberacao As Integer = 0) As List(Of ImprBolEntrada)
                Try
                    fGravaLog("7. Inicio de lista de dados para impresion", "LOG_TESTE")
                    Dim where As String = Nothing
                    Dim whereData As String = Nothing
                    Dim botaoimprimir As Boolean = frmReimpressao.bBotaoImprimir
                    Dim vLiberacao As Boolean = frmReimpressao.cbLiberacao.Checked
                    Dim vNoAnalise As Boolean = frmReimpressao.cbAnalise.Checked
                    Dim vNoBoletim As Integer = 0
                    If vGlobal.bBotaoImprimir Then
                        Select Case vGlobal.FgImprLiberAnalise
                            Case "L" : where = "and c.no_liberacao = :no_liberacao"
                            Case "A" : where = "and c.no_analise = :no_analise"
                        End Select

                        Select Case True
                            Case vGlobal.FgImprBoletiEntrLaredo
                                where = "and c.no_liberacao = :no_liberacao"
                        End Select

                    Else
                        where = "and c.no_liberacao = :no_liberacao"
                        whereData = "  and c.dt_entrada > p.valor"
                    End If
                    Dim result As New List(Of ImprBolEntrada)
                    Dim sql As New StringBuilder
                    sql.AppendLine("SELECT *")
                    sql.AppendLine("FROM (")
                    sql.AppendLine("  WITH param AS (")
                    sql.AppendLine("    SELECT instancia, valor ")
                    sql.AppendLine("    FROM parametros")
                    sql.AppendLine("    WHERE secao = 'RCMP'")
                    sql.AppendLine("      AND entrada = 'DT_PROCESSO'")
                    sql.AppendLine("  )")
                    sql.AppendLine("  SELECT c.no_liberacao,")
                    sql.AppendLine("         c.hr_entrada,")
                    sql.AppendLine("         r.cd_tp_recurso,")
                    sql.AppendLine("         r.cd_equipto,")
                    sql.AppendLine("         -- Pega o maior cd_operador que seja diferente de '0' / 0")
                    sql.AppendLine("         MAX(CASE WHEN r.cd_operador <> '0' THEN r.cd_operador END) OVER (PARTITION BY c.no_liberacao) AS cd_operador,")
                    sql.AppendLine("         c.fg_impr_bol_entr,")
                    sql.AppendLine("         c.fg_analise,")
                    sql.AppendLine("         c.no_analise,")
                    sql.AppendLine("         TRUNC(u.cd_upnivel1) || ' ' || u.de_upnivel1 AS cd_upnivel1,")
                    sql.AppendLine("         NVL(c.id_viagem, 0) AS id_viagem,")
                    sql.AppendLine("         NVL(c.qt_bruto, 0) AS qt_bruto")
                    sql.AppendLine("  FROM apt_cargas c")
                    sql.AppendLine("  INNER JOIN apt_cargas_rec r ON r.instancia = c.instancia AND r.no_liberacao = c.no_liberacao")
                    sql.AppendLine("  INNER JOIN upnivel1 u ON u.cd_upnivel1 = c.cd_upnivel1")
                    sql.AppendLine("  INNER JOIN equiptos e ON e.cd_equipto = r.cd_equipto")
                    sql.AppendLine("  INNER JOIN param p ON p.instancia = c.instancia")
                    sql.AppendLine("  WHERE r.cd_tp_recurso IN ('CM', 'CT')")
                    sql.AppendLine(where)
                    sql.AppendLine(whereData)
                    'If vGlobal.bBotaoImprimir Then
                    '    sql.AppendLine("  and c.fg_impr_bol_entr = :fg_impr_bol_entr")
                    'End If
                    sql.AppendLine("  and c.instancia = :instancia)")
                    sql.AppendLine("pivot (")
                    sql.AppendLine("  max(cd_equipto)")
                    sql.AppendLine("  for cd_tp_recurso in ('CM' AS CM, 'CT' AS CT))")
                    sql.AppendLine("order by no_liberacao")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Dim param As New List(Of OracleParameter) From {
                        New OracleParameter("instancia", OracleDbType.Varchar2) With {.Value = vGlobal.vInstLogin},
                        New OracleParameter("secao", OracleDbType.Varchar2) With {.Value = "RCMP"},
                        New OracleParameter("entrada", OracleDbType.Varchar2) With {.Value = "DT_PROCESSO"}
                        }
                        cmd.Parameters.AddRange(param.ToArray())
                        If vGlobal.bBotaoImprimir Then
                            If vGlobal.FgImprLiberAnalise = "L" Then
                                cmd.Parameters.Add(New OracleParameter("no_liberacao", OracleDbType.Int32) With {.Value = vGlobal.nRegistroImpressao})
                            ElseIf vGlobal.FgImprLiberAnalise = "A" Then
                                cmd.Parameters.Add(New OracleParameter("no_analise", OracleDbType.Int32) With {.Value = vGlobal.nRegistroImpressao})
                            End If

                            Select Case True
                                Case vGlobal.FgImprBoletiEntrLaredo
                                    cmd.Parameters.Add(New OracleParameter("no_liberacao", OracleDbType.Int32) With {.Value = vGlobal.nRegistroImpressao})
                            End Select
                            'cmd.Parameters.Add(New OracleParameter("fg_impr_bol_entr", OracleDbType.Varchar2) With {.Value = "S"})
                        Else
                            'cmd.Parameters.Add(New OracleParameter("no_liberacao", OracleDbType.Int32) With {.Value = noLiberacaoBolEntr})
                            cmd.Parameters.Add(New OracleParameter("no_liberacao", OracleDbType.Int32) With {.Value = noliberacao})
                            ''''''cmd.Parameters.Add(New OracleParameter("fg_impr_bol_entr", OracleDbType.Varchar2) With {.Value = "N"}) '''''''"Y"
                        End If
                        'Dim texto As String = GetDebugSql(cmd)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                result.Add(New ImprBolEntrada With {
                                .noliberacao = rs("no_liberacao"),
                                .cdequiptocm = If(IsDBNull(rs("cm")), 0, rs("cm")),
                                .cdequiptoct = If(IsDBNull(rs("ct")), 0, rs("ct")),
                                .cdoperador = If(IsDBNull(rs("cd_operador")), 0, rs("cd_operador")),
                                .cdupnivel1 = rs("cd_upnivel1"),
                                .fgimprbolentr = If(IsDBNull(rs("fg_impr_bol_entr")), "N", rs("fg_impr_bol_entr")),
                                .hrentrada = rs("hr_entrada"),
                                .idviagem = If(IsDBNull(rs("id_viagem")), 0, rs("id_viagem")),
                                .fganalise = If(IsDBNull(rs("fg_analise")), Nothing, rs("fg_analise")),
                                .noaanlise = If(IsDBNull(rs("no_analise")), 0, rs("no_analise")),
                                .qtbruto = If(IsDBNull(rs("qt_bruto")), 0, rs("qt_bruto"))
                                })
                            End While
                        End Using
                    End Using
                    fGravaLog("8. Final de lista de dados para impresion", "LOG_TESTE")
                    Return result
                Catch ex As OracleException
                    FMensagem($"OracleErr>> Impressoes.ImprBolEntrada.Dados: {ex.ToString()}", "C")
                Catch ex As Exception
                    FMensagem($"Impressoes.ImprBolEntrada.Dados: {ex.ToString()}", "C")
                End Try
            End Function

            Public Shared Sub ImprimeEntrada()
                'If fValidaConexao() Then
                Try
                    If Not Monitor.TryEnter(TravaBoletimPatio) Then
                        Exit Sub
                    End If
                    'A impressão da entrada, já está realizando a impressão do ticket e boletim de analises.
                    listaDados = Dados()
                    If listaDados.Count > 0 Then
                        Threads.bImprimindoBolEntr = True
                        For Each item In listaDados
                            If frmReimpressao.bBotaoImprimir = False Then
                                AtualizaImprEntrada(item.noliberacao, item.fganalise) 'Muda o status da pesagem antes da impressão
                            End If

                            Select Case vGlobal.vImprTela
                                Case "N"
                                    If vGlobal.vImprExtrEntr = "S" Then
                                        frmReimpressao.bBotaoImprimir = False
                                        'ImprimirBolEntr_Laredo()
                                        For i As Integer = 1 To vGlobal.vNumCopiasBoletimentr
                                            Impressoes.ImprimirBolEntr_Laredo()
                                            'Realiza a impressão do boletim de análise no mesmo momento em que é realiado a pesagem
                                            'If i = vGlobal.vNumCopiasBoletimentr Then
                                            '    If vGlobal.vImprBolAnl = "S" Then
                                            '        For b As Integer = 1 To vGlobal.vNumCopiasLaboratCana
                                            '            Impressoes.ImprimirTicketLaboratorio_Laredo()
                                            '        Next
                                            '    End If
                                            'End If
                                        Next
                                    End If
                                Case "S"
                                    Impressoes.ImprTela(Impressoes.PrintBolEntr_Laredo)
                            End Select
                        Next
                        Threads.bImprimindoBolEntr = False
                    End If
                Catch ex As OracleException
                    FMensagem("OracleErr>> IMpressoes.ImprimeEntrada: " & ex.ToString, "C")
                Catch ex As Exception
                    FMensagem("Impressoes.ImprimeEntrada: " & ex.ToString, "C")
                End Try
                'End If
            End Sub

            'Public Shared Sub ImprimeEntrada()

            '    If Not Monitor.TryEnter(TravaBoletimPatio) Then
            '        Exit Sub
            '    End If

            '    Dim listaDados = Dados()

            '    ' Guard Clause: Se não há dados, sai imediatamente (evita aninhar o código todo num If)
            '    If listaDados Is Nothing OrElse listaDados.Count = 0 Then Exit Sub

            '    Try
            '        Threads.bImprimindoBolEntr = True

            '        For Each item In listaDados
            '            ProcessarImpressaoItem(item)
            '        Next

            '    Catch ex As OracleException
            '        FMensagem("OracleErr>> Impressoes.ImprimeEntrada: " & ex.ToString(), "C")
            '    Catch ex As Exception
            '        FMensagem("Impressoes.ImprimeEntrada: " & ex.ToString(), "C")
            '    Finally
            '        ' O Finally GARANTE que a flag é resetada, mesmo se estourar um erro no meio da impressão
            '        Threads.bImprimindoBolEntr = False
            '    End Try
            'End Sub

            'Private Shared Sub ProcessarImpressaoItem(item As Object)
            '    ' Atualiza status do registro antes da impressão, se necessário
            '    If Not frmReimpressao.bBotaoImprimir Then
            '        AtualizaImprEntrada(item.noliberacao, item.fganalise)
            '    End If

            '    Select Case vGlobal.vImprTela
            '        Case "S"
            '            Impressoes.ImprTela(Impressoes.PrintBolEntr_Laredo)
            '        Case "N"
            '            If vGlobal.vImprExtrEntr = "S" Then
            '                ExecutarImpressaoFisica()
            '            End If
            '    End Select
            'End Sub

            'Private Shared Sub ExecutarImpressaoFisica()
            '    frmReimpressao.bBotaoImprimir = False

            '    ' 1. Imprime as cópias do Boletim de Entrada
            '    EnviarParaImpressora(PrintBolEntr_Laredo, vGlobal.vNumCopiasBoletimentr)

            '    ' 2. Imprime as cópias do Boletim de Análise (Laboratório)
            '    If vGlobal.vImprBolAnl = "S" Then
            '        EnviarParaImpressora(PrintBolAnl_Laredo, vGlobal.vNumCopiasLaboratCana)
            '    End If
            'End Sub

            'Private Shared Sub EnviarParaImpressora(doc As PrintDocument, numCopias As Integer)
            '    If doc Is Nothing OrElse numCopias <= 0 Then Exit Sub

            '    doc.PrinterSettings.PrinterName = vNomeImpressora
            '    For i As Integer = 1 To numCopias
            '        doc.Print()
            '    Next
            'End Sub

            Private Shared Sub AtualizaImprEntrada(noliberacao As Integer, fganalise As String)
                Using c As New OracleConnection(FStringConnOracle())
                    c.Open()

                    Using t As OracleTransaction = c.BeginTransaction
                        Try
                            Dim sSet As String = Nothing
                            If fganalise = "S" Then
                                sSet = ", fg_impr_bolanl = 'S'"
                            End If
                            Dim sql As New StringBuilder
                            sql.AppendLine("update apt_cargas")
                            sql.AppendLine("set fg_impr_bol_entr = 'S'" & sSet)
                            sql.AppendLine("where instancia = :instancia")
                            sql.AppendLine("and no_liberacao = :no_liberacao")
                            Using cmd As OracleCommand = New OracleCommand(sql.ToString, c)
                                cmd.Transaction = t
                                cmd.Parameters.Add(New OracleParameter("instancia", OracleDbType.Varchar2) With {.Value = vGlobal.vInstLogin})
                                cmd.Parameters.Add(New OracleParameter("no_liberacao", OracleDbType.Int32) With {.Value = noliberacao})
                                Dim txt As String = GetDebugSql(cmd)
                                Console.WriteLine(txt)
                                cmd.ExecuteNonQuery()
                                t.Commit()
                            End Using
                        Catch ex As OracleException
                            t.Rollback()
                            FMensagem("OracleErr>> Impressoes.AtualizaImprEntrada: " & ex.ToString, "C")
                        Catch ex As Exception
                            t.Rollback()
                            FMensagem("Impressoes.AtualizaImprEntrada: " & ex.ToString, "C")
                        End Try
                    End Using
                End Using
            End Sub
        End Class

        Public Class ImprBolSaida
            Public Property no_liberacao As Integer
            Public Property cd_equipto_cm As Integer
            Public Property cd_equipto_ct As Integer
            Public Property cd_operador As Integer
            Public Property id_viagem As Integer
            Public Property no_sequencia As Integer
            Public Property hr_entrada As Date
            Public Property hr_saida As Date
            Public Property cd_fornec As Integer
            Public Property de_fornec As String
            Public Property cd_upnivel1 As String
            Public Property de_upnivel1 As String
            Public Property qt_distancia As Integer
            Public Property qt_bruto As Integer
            Public Property qt_tara As Integer
            Public Property qt_liquido As Integer
            Public Shared Function Dados() As List(Of ImprBolSaida)
                Try
                    Dim sFiltro As String
                    If Not String.IsNullOrWhiteSpace(vGlobal.nRegistroImpressao) Then
                        sFiltro = vGlobal.nRegistroImpressao.Replace(";", ",")
                    End If

                    Dim hrentrada = Nothing
                    If vGlobal.bBotaoImprimir Then
                        hrentrada = FBuscaHrEntrada(sFiltro)
                    Else
                        hrentrada = hrEntradaCanaLaredo
                    End If

                    Dim resultado As New List(Of ImprBolSaida)
                    Dim sql As New StringBuilder
                    sql.AppendLine("select ")
                    sql.AppendLine("       nvl(c.no_liberacao, 0) no_liberacao,")
                    sql.AppendLine("       nvl(c.id_viagem, 0) id_viagem,")
                    sql.AppendLine("       nvl(c.no_sequencia, 0) no_sequencia,")
                    sql.AppendLine("       c.hr_entrada,")
                    sql.AppendLine("       c.hr_saida,")
                    sql.AppendLine("       nvl(t.cd_fornec, 0) cd_fornec,")
                    sql.AppendLine("       nvl(t.de_fornec, 0) de_fornec,")
                    sql.AppendLine("       nvl(c.cd_upnivel1, 0) cd_upnivel1,")
                    sql.AppendLine("       nvl(u.de_upnivel1, 0) de_upnivel1,")
                    sql.AppendLine("       max(case when r.cd_tp_recurso = 'CM' then r.cd_equipto end) as cm,")
                    sql.AppendLine("       max(case when r.cd_tp_recurso = 'CT' then r.cd_equipto end) as ct,")
                    sql.AppendLine("       max(case when r.cd_tp_recurso = 'CM' then nvl(r.cd_operador, 0) else 0 end) cd_operador,")
                    sql.AppendLine("       nvl(c.qt_distancia, 0) qt_distancia,")
                    sql.AppendLine("       nvl(c.qt_bruto, 0) qt_bruto,")
                    sql.AppendLine("       nvl(c.qt_tara, 0) qt_tara,")
                    sql.AppendLine("       nvl(c.qt_liquido, 0) qt_liquido")
                    sql.AppendLine("from apt_cargas c")
                    sql.AppendLine("inner join apt_cargas_rec r on c.instancia = r.instancia and c.no_liberacao = r.no_liberacao")
                    sql.AppendLine("inner join equiptos e on e.cd_equipto = r.cd_equipto")
                    sql.AppendLine("inner join transporta p on p.cd_transp = e.cd_transp")
                    sql.AppendLine("inner join upnivel1 u on u.cd_upnivel1 = c.cd_upnivel1")
                    sql.AppendLine("inner join fornecs t on t.cd_fornec = u.cd_fornec")
                    sql.AppendLine("where c.instancia = :instancia")
                    sql.AppendLine("and c.hr_entrada = :hr_entrada")
                    sql.AppendLine("and c.fg_status in ('C', 'P')")
                    sql.AppendLine("and r.cd_tp_recurso in ('CM', 'CT')")
                    sql.AppendLine("group by c.no_liberacao,")
                    sql.AppendLine("         c.id_viagem,")
                    sql.AppendLine("         c.no_sequencia,")
                    sql.AppendLine("         c.hr_entrada,")
                    sql.AppendLine("         c.hr_saida,")
                    sql.AppendLine("         t.cd_fornec,")
                    sql.AppendLine("         t.de_fornec,")
                    sql.AppendLine("         c.cd_upnivel1,")
                    sql.AppendLine("         u.de_upnivel1,")
                    sql.AppendLine("         c.qt_distancia,")
                    sql.AppendLine("         c.qt_bruto,")
                    sql.AppendLine("         c.qt_tara,")
                    sql.AppendLine("         c.qt_liquido")
                    sql.AppendLine("order by no_sequencia")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.AddRange({
                        New OracleParameter("instancia", OracleDbType.Varchar2) With {.Value = vInstLogin},
                        New OracleParameter("hr_entrada", OracleDbType.Date) With {.Value = Convert.ToDateTime(hrentrada)}
                        })

                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                resultado.Add(New ImprBolSaida With {
                                .cd_fornec = rs("cd_fornec"),
                                .cd_upnivel1 = rs("cd_upnivel1"),
                                .de_fornec = rs("de_fornec"),
                                .de_upnivel1 = rs("de_upnivel1"),
                                .hr_entrada = rs("hr_entrada"),
                                .hr_saida = rs("hr_saida"),
                                .id_viagem = rs("id_viagem"),
                                .no_liberacao = rs("no_liberacao"),
                                .no_sequencia = rs("no_sequencia"),
                                .qt_bruto = rs("qt_bruto"),
                                .qt_distancia = rs("qt_distancia"),
                                .qt_liquido = rs("qt_liquido"),
                                .qt_tara = rs("qt_tara"),
                                .cd_equipto_cm = rs("cm"),
                                .cd_operador = rs("cd_operador"),
                                .cd_equipto_ct = If(IsDBNull(rs("ct")), 0, rs("ct"))
                                })
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    FMensagem("OracleErr>> Impressoes.Laredo.ImprBolSaida.Dados: " & ex.ToString, "C")
                Catch ex As Exception
                    FMensagem("Impressoes.Laredo.ImprBolSaida.Dados: " & ex.ToString, "C")
                End Try
            End Function

            Public Shared Sub ImpLaredoExtratoSaida()
                'If Not Monitor.TryEnter(TravaSaida) Then
                '    Exit Sub
                'End If

                'Threads.ControlaThreadImpressao.Reset()
                Try
                    If FVerificaSaidaLaredo() Then
                        listaDadosSaida = Laredo.ImprBolSaida.Dados()
                        If listaDadosSaida IsNot Nothing AndAlso listaDadosSaida.Any Then
                            ImprimirSaidaLaredo(listaDadosSaida)
                            'AtualizaExtratoSaidaLaredo(listaDadosSaida)
                        End If
                    End If
                Finally
                    'Threads.ControlaThreadImpressao.Set()
                End Try
            End Sub

            Public Shared Sub AtualizaExtratoSaidaLaredo(list As List(Of ImprBolSaida))
                Using c As New OracleConnection(Conexao.FStringConnOracle())
                    c.Open()
                    Using t As OracleTransaction = c.BeginTransaction
                        Try
                            Dim sql As New StringBuilder
                            sql.AppendLine("update apt_cargas")
                            sql.AppendLine("set pfg_impr_extr_saida = 'S'")
                            sql.AppendLine("where pfg_impr_extr_saida = 'N'")
                            sql.AppendLine("  and instancia = :instancia")
                            sql.AppendLine("  and no_liberacao = :no_liberacao")
                            Using cmd As OracleCommand = New OracleCommand(sql.ToString, c)
                                cmd.Transaction = t
                                cmd.BindByName = True
                                cmd.ArrayBindCount = list.Count
                                Dim arrInst As String() = Enumerable.Repeat(vInstLogin, list.Count).ToArray()
                                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = arrInst
                                cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = list.Select(Function(x) x.no_liberacao).ToArray()
                                cmd.ExecuteNonQuery()
                            End Using
                            t.Commit()
                        Catch ex As OracleException
                            t.Rollback()
                            FMensagem("OracleErr>> Impressoes.AtualizaExtratoSaidaLaredo: " & ex.ToString, "C")
                        Catch ex As Exception
                            t.Rollback()
                            FMensagem("Impressoes.AtualizaExtratoSaidaLaredo: " & ex.ToString, "C")
                        End Try
                    End Using
                End Using
            End Sub
        End Class

        Public Shared Sub ImpExtrProduto_Laredo()
            If Not Monitor.TryEnter(TravaProduto) Then
                Exit Sub
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

        Public Class ImpBolAnalise
            Public Shared Sub Imprimir()
                If Not Monitor.TryEnter(TravaBolAnalise) Then
                    Exit Sub
                End If

                If vGlobal.vImprBolAnl <> "S" Then Exit Sub

                Threads.bImprimindoBolEntr = True
                Laredo.listaDados = Laredo.ImprBolEntrada.Dados()
                Impressoes.ImprimirTicketLaboratorio_Laredo()

                Threads.bImprimindoBolEntr = False
            End Sub
        End Class

        'Public Class ImprPesagemProdutos
        '    Public Property NoPesagem As String = ""
        '    Public Property NoPesagemFormatado As String = ""
        '    Public Property NoOrdemCarga As String = " "
        '    Public Property CdTransacao As String = " "
        '    Public Property CdTipoPesagem As String = " "
        '    Public Property CdTipoPesagemCod As String = " "
        '    Public Property CdProduto As String = " "
        '    Public Property QtProduto As String = "0"
        '    Public Property NoDocumento As String = " "
        '    Public Property CdOrigem As String = " "
        '    Public Property CdDestino As String = " "
        '    Public Property CdTransportadora As String = " "
        '    Public Property CdProvedor As String = " "
        '    Public Property CdCaminhao As String = " "
        '    Public Property CdCondutor As String = " "
        '    Public Property CdReboque1 As String = " "
        '    Public Property CdReboque2 As String = " "
        '    Public Property CdReboque3 As String = " "
        '    Public Property QtPeso1 As Integer = 0
        '    Public Property QtPeso2 As Integer = 0
        '    Public Property QtPesoLiquido As String = "0"
        '    Public Property QtPesoEmbalagem As String = "0"
        '    Public Property QtPesoLiquidoSemEmbalagem As String = "0"
        '    Public Property DtHoraEntrada As String = " "
        '    Public Property DtHoraSaida As String = " "
        '    Public Property CdUsuario As String = " "
        '    Public Property DeObservacao As String = " "
        '    Public Property Pesagem As String = " "
        '    Public Property FgPesagEs As String = " "
        '    Public Property CdNivel1_O As String = ""
        '    Public Property CdNivel2_O As String = ""
        '    Public Property CdNivel3_O As String = ""
        '    Public Property CdNivel1_D As String = ""
        '    Public Property CdNivel2_D As String = ""
        '    Public Property CdNivel3_D As String = ""
        '    Public Property CdRecursoCG As String = ""
        '    Public Property CdRecursoTR As String = ""
        '    Public Property CdOperCG As String = ""
        '    Public Property CdOperTR As String = ""
        '    Public Property CdVariedade As String = ""
        '    Public Property NoPlacaCM As String = ""
        '    Public Property NoPlacaRB1 As String = ""
        '    Public Property NoPlacaRB2 As String = ""
        '    Public Property NoPlacaRB3 As String = ""
        '    Public Property DescNivel1 As String = ""
        '    Public Property DescNivel2 As String = ""
        '    Public Property DescNivel3 As String = ""
        '    Public Property FgCadVeculos As String = ""
        '    Public Property NomeEmpresa As String = ""

        '    Public Shared Function DadosImpressao() As List(Of ImprPesagemProdutos)
        '        Dim lstRetorno As New List(Of ImprPesagemProdutos)()

        '        Dim descNivel1 As String = cadparametros(("NIVEIS", "UPNIVEL1"))
        '        Dim descNivel2 As String = cadparametros(("NIVEIS", "UPNIVEL2"))
        '        Dim descNivel3 As String = cadparametros(("NIVEIS", "UPNIVEL3"))
        '        Dim fgCadVeculos As String = cadparametros(("SPA_OUTR", "FG_CAD_VEICULO"))
        '        Dim nomeEmpresa As String = cadparametros(("GERAIS", "DA_EMPRESA"))
        '        vGlobal.sNomeEmpresa = nomeEmpresa

        '        Select Case True
        '            Case vGlobal.bBotaoImprimir
        '                DicRecursosPrd = FDicRecursosPrd(vGlobal.nRegistroImpressao)
        '                DicNiveisPrd = FDicNiveisPrd(vGlobal.nRegistroImpressao)
        '            Case Else
        '                DicRecursosPrd = FDicRecursosPrd(vGlobal.gn20)
        '                DicNiveisPrd = FDicNiveisPrd(vGlobal.gn20)
        '        End Select

        '        Dim sql As New StringBuilder()
        '        sql.AppendLine("select o.instancia, lpad(o.no_pesagem,10,0) no_pesagem, o.no_pesagem pesagem,")
        '        sql.AppendLine("       o.hr_entrada, o.hr_saida, o.fg_pesag_es, nvl(op.cd_varied,0) cd_varied,")
        '        sql.AppendLine("       tm.cd_tp_mov || '-' || tm.de_tp_mov tipo_transacao, o.fg_tp_pesag fg_tp_pesag_cod,")
        '        sql.AppendLine("       case")
        '        sql.AppendLine("         when o.fg_tp_pesag = 'P' then 'P - Prod.Acabados'")
        '        sql.AppendLine("         when o.fg_tp_pesag = 'A' then 'A - Açúcar'")
        '        sql.AppendLine("         when o.fg_tp_pesag = 'T' then 'T - Insumos-Resíduos'")
        '        sql.AppendLine("         when o.fg_tp_pesag = 'M' then 'M - Mudas de Cana'")
        '        sql.AppendLine("         when o.fg_tp_pesag = 'O' then 'O - Outros'")
        '        sql.AppendLine("       end fg_tp_pesagem,")
        '        sql.AppendLine("       o.no_documento,")
        '        sql.AppendLine("       nvl(op.cd_origem,0) cd_origem,")
        '        sql.AppendLine("       nvl(op.cd_destino,0) cd_destino,")
        '        sql.AppendLine("       nvl(o.cd_fornec,0) cd_fornec,")
        '        sql.AppendLine("       nvl(o.cd_transport,0) cd_transport,")
        '        sql.AppendLine("       o.caminhao, o.motorista,")
        '        sql.AppendLine("       nvl(o.reboque,0) reboque_1,")
        '        sql.AppendLine("       nvl(o.reboque_2,0) reboque_2,")
        '        sql.AppendLine("       nvl(o.reboque_3,0) reboque_3,")
        '        sql.AppendLine("       o.cd_produto || '-' || p.de_produto produto,")
        '        sql.AppendLine("       nvl(op.qt_produto,0) qt_produto, o.qt_peso1, o.qt_peso2, o.qt_pesoliq,")
        '        sql.AppendLine("       o.qt_peso_emb, nvl(o.no_ordem_car,' ') no_ordem_car, nvl(o.de_observacao,' ') observacao, o.cd_usuario")
        '        sql.AppendLine("  from outr_pesagens o")
        '        sql.AppendLine(" inner join outr_pesagens_prd op")
        '        sql.AppendLine("    on o.instancia = op.instancia")
        '        sql.AppendLine("   and o.no_pesagem = op.no_pesagem")
        '        sql.AppendLine(" inner join cad_produto p")
        '        sql.AppendLine("    on p.cd_produto = op.cd_produto")
        '        sql.AppendLine(" inner join tipo_movimento tm")
        '        sql.AppendLine("    on tm.cd_tp_mov = o.cd_tp_mov")
        '        sql.AppendLine(" where o.instancia = :instancia")
        '        sql.AppendLine("   and o.fg_status = :fg_status")
        '        sql.AppendLine("   and o.no_pesagem = :no_pesagem")
        '        sql.AppendLine("   and rownum = 1")

        '        Using cmd As OracleCommand = fCmd(sql.ToString())
        '            cmd.Parameters.Clear()
        '            cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
        '            cmd.Parameters.Add("fg_status", OracleDbType.Varchar2).Value = "C"

        '            If vGlobal.bBotaoImprimir Then
        '                cmd.Parameters.Add("no_pesagem", OracleDbType.Int64).Value = vGlobal.nRegistroImpressao
        '            Else
        '                cmd.Parameters.Add("no_pesagem", OracleDbType.Int64).Value = vGlobal.gn20
        '            End If

        '            Using rs As OracleDataReader = cmd.ExecuteReader()
        '                While rs.Read()
        '                    Dim item As New ImprPesagemProdutos()

        '                    item.DescNivel1 = descNivel1
        '                    item.DescNivel2 = descNivel2
        '                    item.DescNivel3 = descNivel3
        '                    item.FgCadVeculos = fgCadVeculos
        '                    item.NomeEmpresa = nomeEmpresa

        '                    item.NoPesagem = vGlobal.gn20
        '                    item.NoPesagemFormatado = Convert.ToString(rs("no_pesagem"))
        '                    item.NoOrdemCarga = If(Not IsDBNull(rs("no_ordem_car")), rs("no_ordem_car").ToString(), " ")
        '                    item.CdTransacao = If(Not IsDBNull(rs("tipo_transacao")), rs("tipo_transacao").ToString(), " ")
        '                    item.CdTipoPesagem = If(Not IsDBNull(rs("fg_tp_pesagem")), rs("fg_tp_pesagem").ToString(), " ")
        '                    item.CdTipoPesagemCod = If(Not IsDBNull(rs("fg_tp_pesag_cod")), rs("fg_tp_pesag_cod").ToString(), " ")
        '                    item.CdProduto = If(Not IsDBNull(rs("produto")), rs("produto").ToString(), " ")
        '                    item.QtProduto = If(Not IsDBNull(rs("qt_produto")), rs("qt_produto").ToString(), "0")
        '                    item.NoDocumento = If(Not IsDBNull(rs("no_documento")), rs("no_documento").ToString(), " ")

        '                    If Not IsDBNull(rs("cd_origem")) AndAlso caddepositoprod.ContainsKey(rs("cd_origem")) Then
        '                        item.CdOrigem = rs("cd_origem").ToString() & " - " & caddepositoprod(rs("cd_origem")).de_deposito
        '                    End If

        '                    If Not IsDBNull(rs("cd_destino")) AndAlso caddestinatario.ContainsKey(rs("cd_destino")) Then
        '                        item.CdDestino = rs("cd_destino").ToString() & " - " & caddestinatario(rs("cd_destino"))
        '                    End If

        '                    If Not IsDBNull(rs("cd_transport")) AndAlso cadtransportadora.ContainsKey(rs("cd_transport")) Then
        '                        item.CdTransportadora = rs("cd_transport").ToString() & " - " & cadtransportadora(rs("cd_transport"))
        '                    End If

        '                    If Not IsDBNull(rs("cd_fornec")) AndAlso cadfornectransp.ContainsKey(rs("cd_fornec")) Then
        '                        item.CdProvedor = rs("cd_fornec").ToString() & " - " & cadfornectransp(rs("cd_fornec")).de_fornec
        '                    End If

        '                    item.CdCaminhao = If(Not IsDBNull(rs("caminhao")), rs("caminhao").ToString(), " ")
        '                    item.CdCondutor = If(Not IsDBNull(rs("motorista")), rs("motorista").ToString(), " ")
        '                    item.CdReboque1 = If(Not IsDBNull(rs("reboque_1")), rs("reboque_1").ToString(), " ")
        '                    item.CdReboque2 = If(Not IsDBNull(rs("reboque_2")), rs("reboque_2").ToString(), " ")
        '                    item.CdReboque3 = If(Not IsDBNull(rs("reboque_3")), rs("reboque_3").ToString(), " ")

        '                    item.QtPeso1 = If(Not IsDBNull(rs("qt_peso1")), Convert.ToInt32(rs("qt_peso1")), 0)
        '                    item.QtPeso2 = If(Not IsDBNull(rs("qt_peso2")), Convert.ToInt32(rs("qt_peso2")), 0)
        '                    item.QtPesoLiquido = If(Not IsDBNull(rs("qt_pesoliq")), rs("qt_pesoliq").ToString(), "0")
        '                    item.QtPesoEmbalagem = If(Not IsDBNull(rs("qt_peso_emb")), rs("qt_peso_emb").ToString(), "0")

        '                    ' Cálculo de Peso Liquido sem Embalagem
        '                    Dim pLiq As Decimal = 0
        '                    Dim pEmb As Decimal = 0
        '                    Decimal.TryParse(item.QtPesoLiquido, pLiq)
        '                    Decimal.TryParse(item.QtPesoEmbalagem, pEmb)
        '                    item.QtPesoLiquidoSemEmbalagem = (pLiq - pEmb).ToString()

        '                    item.DtHoraEntrada = If(Not IsDBNull(rs("hr_entrada")), rs("hr_entrada").ToString(), " ")
        '                    item.DtHoraSaida = If(Not IsDBNull(rs("hr_saida")), rs("hr_saida").ToString(), " ")
        '                    item.CdUsuario = If(Not IsDBNull(rs("cd_usuario")), rs("cd_usuario").ToString(), " ")
        '                    item.DeObservacao = If(Not IsDBNull(rs("observacao")), rs("observacao").ToString(), " ")
        '                    item.Pesagem = If(Not IsDBNull(rs("pesagem")), rs("pesagem").ToString(), " ")
        '                    item.FgPesagEs = If(Not IsDBNull(rs("fg_pesag_es")), rs("fg_pesag_es").ToString(), " ")

        '                    ' Buscas adicionais para Mudas de Cana (M)
        '                    If item.CdTipoPesagemCod = "M" Then
        '                        If DicNiveisPrd.Any Then
        '                            item.CdNivel1_O = DicNiveisPrd((vInstLogin, item.NoPesagemFormatado)).nivel1_ori
        '                            item.CdNivel2_O = DicNiveisPrd((vInstLogin, item.NoPesagemFormatado)).nivel2_ori
        '                            item.CdNivel3_O = DicNiveisPrd((vInstLogin, item.NoPesagemFormatado)).nivel3_ori
        '                            item.CdNivel1_D = DicNiveisPrd((vInstLogin, item.NoPesagemFormatado)).nivel1_des
        '                            item.CdNivel2_D = DicNiveisPrd((vInstLogin, item.NoPesagemFormatado)).nivel2_des
        '                            item.CdNivel3_D = DicNiveisPrd((vInstLogin, item.NoPesagemFormatado)).nivel3_des
        '                        End If

        '                        If DicRecursosPrd.Any Then
        '                            item.CdRecursoCG = DicRecursosPrd.Where(Function(x) x.Key.Item2 = item.NoPesagemFormatado AndAlso x.Value.cdtprecurso = "CG").Select(Function(y) y.Value.cdequipto).FirstOrDefault()
        '                            item.CdRecursoTR = DicRecursosPrd.Where(Function(x) x.Key.Item2 = item.NoPesagemFormatado AndAlso x.Value.cdtprecurso = "TR").Select(Function(y) y.Value.cdequipto).FirstOrDefault()
        '                            item.CdOperCG = DicRecursosPrd.Where(Function(x) x.Key.Item2 = item.NoPesagemFormatado AndAlso x.Value.cdtprecurso = "CG").Select(Function(y) y.Value.cdoperador).FirstOrDefault()
        '                            item.CdOperTR = DicRecursosPrd.Where(Function(x) x.Key.Item2 = item.NoPesagemFormatado AndAlso x.Value.cdtprecurso = "TR").Select(Function(y) y.Value.cdoperador).FirstOrDefault()
        '                            item.CdCaminhao = DicRecursosPrd.Where(Function(x) x.Key.Item2 = item.NoPesagemFormatado AndAlso x.Value.cdtprecurso = "CM").Select(Function(y) y.Value.cdequipto).FirstOrDefault()
        '                            item.CdCondutor = DicRecursosPrd.Where(Function(x) x.Key.Item2 = item.NoPesagemFormatado AndAlso x.Value.cdtprecurso = "CM").Select(Function(y) y.Value.cdoperador).FirstOrDefault()
        '                        End If

        '                        item.CdVariedade = rs("cd_varied").ToString()
        '                        fGravaLogDebug($"{Now()} 17.Final de buscas de recursos da pesagem", "LOG_TESTE")
        '                    End If

        '                    ' Busca de Placas
        '                    If item.FgCadVeculos = "S" Then
        '                        If Not String.IsNullOrWhiteSpace(item.CdCaminhao) AndAlso cadveiculos.ContainsKey(item.CdCaminhao) Then
        '                            item.NoPlacaCM = cadveiculos(item.CdCaminhao).no_placa
        '                        End If
        '                        If Not String.IsNullOrWhiteSpace(item.CdReboque1) AndAlso cadveiculos.ContainsKey(item.CdReboque1) Then
        '                            item.NoPlacaRB1 = cadveiculos(item.CdReboque1).no_placa
        '                        End If
        '                        If Not String.IsNullOrWhiteSpace(item.CdReboque2) AndAlso cadveiculos.ContainsKey(item.CdReboque2) Then
        '                            item.NoPlacaRB2 = cadveiculos(item.CdReboque2).no_placa
        '                        End If
        '                        If Not String.IsNullOrWhiteSpace(item.CdReboque3) AndAlso cadveiculos.ContainsKey(item.CdReboque3) Then
        '                            item.NoPlacaRB3 = cadveiculos(item.CdReboque3).no_placa
        '                        End If
        '                    ElseIf item.FgCadVeculos = "N" Then
        '                        If Not String.IsNullOrWhiteSpace(item.CdCaminhao) Then
        '                            If item.CdTipoPesagemCod = "M" Then
        '                                If cadequiptos.ContainsKey(item.CdCaminhao) Then
        '                                    item.NoPlacaCM = cadequiptos(item.CdCaminhao).no_placa
        '                                End If
        '                            Else
        '                                If cadplacaequiptos.ContainsKey(item.CdCaminhao) Then
        '                                    item.CdCaminhao = cadplacaequiptos(item.CdCaminhao).cd_equipto
        '                                End If
        '                            End If
        '                        End If

        '                        If Not String.IsNullOrWhiteSpace(item.CdReboque1) Then
        '                            If IsNumeric(item.CdReboque1) Then
        '                                If cadequiptos.ContainsKey(item.CdReboque1) Then item.NoPlacaRB1 = cadequiptos(item.CdReboque1).no_placa
        '                            Else
        '                                If cadplacaequiptos.ContainsKey(item.CdReboque1) Then item.NoPlacaRB1 = cadplacaequiptos(item.CdReboque1).no_placa
        '                            End If
        '                        End If

        '                        If Not String.IsNullOrWhiteSpace(item.CdReboque2) Then
        '                            If IsNumeric(item.CdReboque2) Then
        '                                If cadequiptos.ContainsKey(item.CdReboque2) Then item.NoPlacaRB2 = cadequiptos(item.CdReboque2).no_placa
        '                            Else
        '                                If cadplacaequiptos.ContainsKey(item.CdReboque2) Then item.NoPlacaRB2 = cadplacaequiptos(item.CdReboque2).no_placa
        '                            End If
        '                        End If

        '                        If Not String.IsNullOrWhiteSpace(item.CdReboque3) Then
        '                            If IsNumeric(item.CdReboque3) Then
        '                                If cadequiptos.ContainsKey(item.CdReboque3) Then item.NoPlacaRB3 = cadequiptos(item.CdReboque3).no_placa
        '                            Else
        '                                If cadplacaequiptos.ContainsKey(item.CdReboque3) Then item.NoPlacaRB3 = cadplacaequiptos(item.CdReboque3).no_placa
        '                            End If
        '                        End If
        '                    End If

        '                    lstRetorno.Add(item)
        '                End While
        '            End Using
        '        End Using

        '        Return lstRetorno
        '    End Function

        '    Public Shared Sub ImprimirPesagemProdutos()
        '        If Not Monitor.TryEnter(TravaProduto) Then
        '            Exit Sub
        '        End If
        '        If vGlobal.vImprExtrProduto <> "S" Then Exit Sub

        '        'If FVerificaSaidaLaredo() Then
        '        '    listaDadosSaida = Laredo.ImprBolSaida.Dados()
        '        '    If listaDadosSaida IsNot Nothing AndAlso listaDadosSaida.Any Then
        '        '        ImprimirSaidaLaredo(listaDadosSaida)
        '        '        AtualizaExtratoSaidaLaredo(listaDadosSaida)
        '        '    End If
        '        'End If


        '        If FVerifPesagProd() Then
        '            listaDadosExtrProduto = Laredo.ImprPesagemProdutos.DadosImpressao()
        '            If listaDadosExtrProduto IsNot Nothing AndAlso listaDadosExtrProduto.Any Then
        '                ImprimirExtratoProduto(listaDadosExtrProduto)
        '            End If
        '        End If
        '    End Sub

        '    Private Shared Function FVerifPesagProd() As Boolean
        '        Try
        '            Dim result As String = Nothing
        '            Dim sql As New StringBuilder
        '            sql.AppendLine("select no_pesagem from outr_pesagens")
        '            sql.AppendLine("where instancia = :instancia")
        '            sql.AppendLine("  and pfg_impr_extrato = :pfg_impr_extrato")
        '            sql.AppendLine("  and fg_status = :fg_status")
        '            Using cmd As OracleCommand = fCmd(sql.ToString)
        '                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vGlobal.vInstLogin
        '                cmd.Parameters.Add("pfg_impr_extrato", OracleDbType.Varchar2).Value = "N"
        '                cmd.Parameters.Add("fg_status", OracleDbType.Varchar2).Value = "C"
        '                Using rs As OracleDataReader = cmd.ExecuteReader
        '                    If rs.Read Then
        '                        vGlobal.gn20 = rs("no_pesagem")
        '                        AtualizaOutrPesagens(vGlobal.gn20)
        '                        Return True
        '                    End If
        '                End Using
        '            End Using
        '            Return False
        '        Catch ex As Exception
        '        End Try
        '    End Function

        '    Private Shared Sub AtualizaOutrPesagens(noPesagem As Int64)
        '        Using t As OracleTransaction = conn.BeginTransaction
        '            Try
        '                Dim sql As New StringBuilder
        '                sql.AppendLine("update outr_pesagens")
        '                sql.AppendLine("set pfg_impr_extrato = :fg_s")
        '                sql.AppendLine("where instancia = :instancia")
        '                sql.AppendLine("  and no_pesagem = :nopesagem")
        '                sql.AppendLine("  and pfg_impr_extrato = :fg_n")
        '                Using cmd As OracleCommand = fCmd(sql.ToString)
        '                    cmd.Transaction = t
        '                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
        '                    cmd.Parameters.Add("nopesagem", OracleDbType.Int64).Value = noPesagem
        '                    cmd.Parameters.Add("fg_s", OracleDbType.Varchar2).Value = "S"
        '                    cmd.Parameters.Add("fg_n", OracleDbType.Varchar2).Value = "N"
        '                    cmd.ExecuteNonQuery()
        '                    t.Commit()
        '                End Using
        '            Catch ex As Exception
        '                t.Rollback()
        '            End Try
        '        End Using
        '    End Sub

        '    Private Shared Sub ImprimirExtratoProduto(lista As List(Of ImprPesagemProdutos))
        '        Dim dados As New VariaviesImpr
        '        For Each item In listaDadosExtrProduto
        '            dados._NoPesagem = item.NoPesagem
        '            dados._NoOrdemCarga = item.NoOrdemCarga
        '            dados._CdTransacao = item.CdTransacao
        '            dados._NoDocumento = item.NoDocumento
        '            dados._CdTipoPesagem = item.CdTipoPesagem
        '            dados._QtProduto = item.QtProduto
        '            dados._CdProduto = item.CdProduto
        '            dados._CdTipoPesagemCod = item.CdTipoPesagemCod
        '            dados._CdOrigem = item.CdOrigem
        '            dados._CdDestino = item.CdDestino
        '            dados._CdVariedade = item.CdVariedade
        '            dados._CdNivel1_O = item.CdNivel1_O
        '            dados._CdNivel1_D = item.CdNivel1_D
        '            dados._CdNivel2_O = item.CdNivel2_O
        '            dados._CdNivel2_D = item.CdNivel2_D
        '            dados._CdNivel3_O = item.CdNivel3_O
        '            dados._CdNivel3_D = item.CdNivel3_D
        '            dados._DescNivel1 = item.DescNivel1
        '            dados._DescNivel2 = item.DescNivel2
        '            dados._DescNivel3 = item.DescNivel3
        '        Next

        '        ' Inicializa o StringBuilder para montar o conteúdo RAW
        '        Dim sb As New StringBuilder()

        '        ' --- COMANDOS ESC/POS BÁSICOS ---
        '        Dim ESC As String = Chr(27)
        '        Dim GS As String = Chr(29)
        '        Dim ResetPrint As String = $"{ESC}@"            ' Restaura padrões
        '        Dim Centralizar As String = $"{ESC}a{Chr(1)}"  ' Alinha ao centro
        '        Dim Esquerda As String = $"{ESC}a{Chr(0)}"     ' Alinha à esquerda
        '        Dim NegritoOn As String = $"{ESC}E{Chr(1)}"    ' Liga Negrito
        '        Dim NegritoOff As String = $"{ESC}E{Chr(0)}"   ' Desliga Negrito

        '        ' Reset da impressora
        '        sb.Append(ResetPrint)

        '        ' --- CABEÇALHO ---
        '        sb.Append(Centralizar)
        '        sb.Append(NegritoOn)
        '        sb.AppendLine("Extracto de Pesaje de Productos")
        '        sb.Append(NegritoOff)
        '        sb.Append(Esquerda)

        '        ' --- DADOS DO PESAJE ---
        '        sb.AppendLine($"Numero Pesaje:   {dados._NoPesagem}")
        '        sb.AppendLine($"Numero Orden Carga:  {dados._NoOrdemCarga}")
        '        sb.AppendLine($"Tipo Movimentación:  {dados._CdTransacao}")
        '        sb.AppendLine($"Documento:  {dados._NoDocumento}")
        '        sb.AppendLine($"Tipo de Pesaje:  {dados._CdTipoPesagem}")

        '        Dim quantidadeFormatada As String = String.Format("{0:N2}", dados._QtProduto)
        '        sb.AppendLine($"Producto:{Space(13)}Cantidad: {quantidadeFormatada}")
        '        sb.AppendLine(FQuebraLinhasImpressao(dados._CdProduto))

        '        If dados._CdTipoPesagemCod <> "M" Then
        '            sb.AppendLine($"Origen:  {dados._CdOrigem}")
        '            sb.AppendLine($"Destino:  {dados._CdDestino}")
        '        ElseIf dados._CdTipoPesagemCod = "M" Then
        '            sb.AppendLine($"Variedad: {dados._CdVariedade}")
        '            sb.AppendLine($"O.: {dados._DescNivel1}{dados._CdNivel1_O}{Space(3)}O.: {dados._DescNivel2}{dados._CdNivel2_O}{Space(3)}O.: {dados._DescNivel3}{dados._CdNivel3_O}")
        '            sb.AppendLine($"D.: {dados._DescNivel1}{dados._CdNivel1_D}{Space(3)}D.: {dados._DescNivel2}{dados._CdNivel2_D}{Space(3)}D.: {dados._DescNivel3}{dados._CdNivel3_D}")
        '            sb.AppendLine($"Carg.:{dados._CdRecursoCG}{Space(3)}Oper.:{dados._CdOperCG}")
        '            sb.AppendLine($"Tract.:{dados._CdRecursoTR}{Space(3)}Oper.:{dados._CdOperTR}")
        '        End If

        '        sb.AppendLine(FQuebraLinhasImpressao($"Transportadora: {dados._CdTransportadora}"))
        '        sb.AppendLine(FQuebraLinhasImpressao($"Proveedor: {dados._CdProvedor}"))
        '        sb.AppendLine($"Camion:  {dados._CdCaminhao}{Space(10)}Placa:  {dados._NoPlacaCM}")
        '        sb.AppendLine(FQuebraLinhasImpressao($"Conductor:  {dados._CdCondutor}"))

        '        ' --- REBOQUES ---
        '        If dados._CdReboque1 <> "0" Then
        '            sb.AppendLine($"1.Reboque:  {dados._CdReboque1}{Space(3)}Placa: {dados._NoPlacaRB1}")
        '        End If

        '        If dados._CdReboque2 <> "0" Then
        '            sb.AppendLine($"2.Reboque:  {dados._CdReboque2}{Space(3)}Placa:  {dados._NoPlacaRB2}")
        '        End If

        '        If CdReboque3 <> "0" Then
        '            sb.AppendLine($"3.Reboque:  {CdReboque3}{Space(3)}Placa: {NoPlacaRB3}")
        '        End If

        '        ' --- TARA E PESAGEM ---
        '        If FgPesagEs = "E" Then
        '            qtPesoTara = QtPeso2
        '            qtPesoBruto = QtPeso1
        '            strDescricao = "Entrada de Producto"
        '        ElseIf FgPesagEs = "S" Then
        '            qtPesoTara = QtPeso1
        '            qtPesoBruto = QtPeso2
        '            strDescricao = "Salida de Producto"
        '        End If

        '        sb.AppendLine()
        '        sb.Append(Centralizar)
        '        sb.Append(NegritoOn)
        '        sb.AppendLine($"*** {strDescricao} ***")
        '        sb.Append(NegritoOff)
        '        sb.Append(Esquerda)

        '        sb.AppendLine($"Peso Tara: {qtPesoTara}")
        '        sb.AppendLine($"Peso Bruto: {qtPesoBruto}")
        '        sb.AppendLine($"Peso Neto: {QtPesoLiquido}")
        '        sb.AppendLine($"Peso Emb: {QtPesoEmbalagem}")
        '        sb.AppendLine($"Peso Neto Sem Emb: {QtPesoLiquidoSemEmbalagem}")
        '        sb.AppendLine($"Fec/Hr Entrada: {DtHoraEntrada}")
        '        sb.AppendLine($"Fec/Hr Salida: {DtHoraSaida}")
        '        sb.AppendLine($"Usuario: {CdUsuario}")
        '        sb.AppendLine("Observacion:")
        '        sb.AppendLine(FQuebraLinhasImpressao(DeObservacao))

        '        ' Linhas de avanço/corte de papel
        '        sb.AppendLine()
        '        sb.AppendLine()
        '        sb.AppendLine(".")
        '        sb.AppendLine($"{ESC}d{Chr(4)}") ' Avança 4 linhas antes de cortar
        '        sb.AppendLine($"{GS}V{Chr(66)}{Chr(0)}") ' Descomente para acionar a guilhotina

        '        ' String final formatada em RAW pronta para envio
        '        Dim textoRAW As String = sb.ToString()
        '    End Sub

        '    Public Class VariaviesImpr
        '        Public Property _NoPesagem As String
        '        Public Property _NoPesagemFormatado As String
        '        Public Property _NoOrdemCarga As String
        '        Public Property _CdTransacao As String
        '        Public Property _CdTipoPesagem As String
        '        Public Property _CdTipoPesagemCod As String
        '        Public Property _CdProduto As String
        '        Public Property _QtProduto As String
        '        Public Property _NoDocumento As String
        '        Public Property _CdOrigem As String
        '        Public Property _CdDestino As String
        '        Public Property _CdTransportadora As String
        '        Public Property _CdProvedor As String
        '        Public Property _CdCaminhao As String
        '        Public Property _CdCondutor As String
        '        Public Property _CdReboque1 As String
        '        Public Property _CdReboque2 As String
        '        Public Property _CdReboque3 As String
        '        Public Property _QtPeso1 As Integer
        '        Public Property _QtPeso2 As Integer
        '        Public Property _QtPesoLiquido As String
        '        Public Property _QtPesoEmbalagem As String
        '        Public Property _QtPesoLiquidoSemEmbalagem As String
        '        Public Property _DtHoraEntrada As String
        '        Public Property _DtHoraSaida As String
        '        Public Property _CdUsuario As String
        '        Public Property _DeObservacao As String
        '        Public Property _Pesagem As String
        '        Public Property _FgPesagEs As String
        '        Public Property _CdNivel1_O As String
        '        Public Property _CdNivel2_O As String
        '        Public Property _CdNivel3_O As String
        '        Public Property _CdNivel1_D As String
        '        Public Property _CdNivel2_D As String
        '        Public Property _CdNivel3_D As String
        '        Public Property _CdRecursoCG As String
        '        Public Property _CdRecursoTR As String
        '        Public Property _CdOperCG As String
        '        Public Property _CdOperTR As String
        '        Public Property _CdVariedade As String
        '        Public Property _NoPlacaCM As String
        '        Public Property _NoPlacaRB1 As String
        '        Public Property _NoPlacaRB2 As String
        '        Public Property _NoPlacaRB3 As String
        '        Public Property _DescNivel1 As String
        '        Public Property _DescNivel2 As String
        '        Public Property _DescNivel3 As String
        '        Public Property _FgCadVeculos As String
        '        Public Property _NomeEmpresa As String
        '    End Class

        'End Class
    End Class

    Public Class Balcanes
        Public Shared Sub ImpBalcanesBoletimAnalises()
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

        Public Shared Sub ImpBalcanesExtratoSaida()
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

        Public Shared Sub ImpBalcanesOrdemCarga()
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

    End Class

    Public Class Cadastros
        Public Class Empresas
            Public Shared Function CadEmpresas() As Dictionary(Of Long, String)
                Try
                    Dim resultado As New Dictionary(Of Long, String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_empresa,de_empresa from empresas")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd_empresa As Long = If(IsDBNull(rs("cd_empresa")), Nothing, rs("cd_empresa"))
                                Dim de_empresa As String = If(IsDBNull(rs("de_empresa")), Nothing, rs("de_empresa"))
                                If Not resultado.ContainsKey(cd_empresa) Then
                                    resultado.Add(cd_empresa, de_empresa)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>>Cadastros.Empresas.CadEmpresa" & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Cadastros.Empresas.CadEmpresa" & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class Parametros
            Public Shared Function DictParam() As Dictionary(Of (String, String), String)
                Try
                    Dim resultado As New Dictionary(Of (String, String), String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select secao,entrada,valor from parametros")
                    sql.AppendLine("where instancia = :instancia")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim secao As String = If(IsDBNull(rs("secao")), Nothing, rs("secao"))
                                Dim entrada As String = If(IsDBNull(rs("entrada")), Nothing, rs("entrada"))
                                Dim valor As String = If(IsDBNull(rs("valor")), Nothing, rs("valor"))
                                If Not resultado.ContainsKey((secao, entrada)) Then
                                    resultado.Add((secao, entrada), valor)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Parametros.DictParam: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Parametros.DictParam: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class Equiptos
            Public Shared Function Dados() As Dictionary(Of Integer, (String, String))
                Try
                    Dim resultado As New Dictionary(Of Integer, (String, String))
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select e.cd_equipto, e.no_placa, m.de_modelo from equiptos e")
                    sql.AppendLine("inner join modelos m on m.cd_modelo = e.cd_modelo")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd_equipto As Integer = If(IsDBNull(rs("cd_equipto")), Nothing, rs("cd_equipto"))
                                Dim no_placa As String = If(IsDBNull(rs("no_placa")), Nothing, rs("no_placa"))
                                Dim de_modelo As String = If(IsDBNull(rs("de_modelo")), Nothing, rs("de_modelo"))
                                If Not resultado.ContainsKey(cd_equipto) Then
                                    resultado.Add(cd_equipto, (no_placa, de_modelo))
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Equiptos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Equiptos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

            Public Class Placa
                Public Shared Function Dados() As Dictionary(Of String, (Integer, String))
                    Try
                        Dim resultado As New Dictionary(Of String, (Integer, String))
                        Dim sql As New StringBuilder
                        sql.Clear()
                        sql.AppendLine("select e.cd_equipto, e.no_placa, m.de_modelo from equiptos e")
                        sql.AppendLine("inner join modelos m on m.cd_modelo = e.cd_modelo")
                        Using cmd As OracleCommand = fCmd(sql.ToString)
                            Using rs As OracleDataReader = cmd.ExecuteReader
                                While rs.Read
                                    Dim cd_equipto As Integer = If(IsDBNull(rs("cd_equipto")), Nothing, rs("cd_equipto"))
                                    Dim no_placa As String = If(IsDBNull(rs("no_placa")), "P" & cd_equipto, rs("no_placa"))
                                    Dim de_modelo As String = If(IsDBNull(rs("de_modelo")), Nothing, rs("de_modelo"))
                                    If Not resultado.ContainsKey(no_placa) Then
                                        resultado.Add(no_placa, (cd_equipto, no_placa))
                                    End If
                                End While
                            End Using
                        End Using
                        Return resultado
                    Catch ex As OracleException
                        MsgBox("OracleErr>> Equiptos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                    Catch ex As Exception
                        MsgBox("Equiptos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                    End Try
                End Function
            End Class
        End Class

        Public Class DepositoProdutos
            Public Shared Function Dados() As Dictionary(Of Integer, (String, String))
                Try
                    Dim resultado As New Dictionary(Of Integer, (String, String))
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_deposito,da_deposito,de_deposito from deposito")
                    sql.AppendLine("where instancia = :instancia")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd_deposito As Integer = If(IsDBNull(rs("cd_deposito")), Nothing, rs("cd_deposito"))
                                Dim da_deposito As String = If(IsDBNull(rs("da_deposito")), Nothing, rs("da_deposito"))
                                Dim de_deposito As String = If(IsDBNull(rs("de_deposito")), Nothing, rs("de_deposito"))
                                If Not resultado.ContainsKey(cd_deposito) Then
                                    resultado.Add(cd_deposito, (da_deposito, de_deposito))
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> DepositoProdutos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("DepositoProdutos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class Transportadora
            Public Shared Function Dados() As Dictionary(Of Integer, String)
                Try
                    Dim resultado As New Dictionary(Of Integer, String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_transport, de_transport from transportadora")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd As Integer = If(IsDBNull(rs("cd_transport")), Nothing, rs("cd_transport"))
                                Dim de As String = If(IsDBNull(rs("cd_transport")), Nothing, rs("cd_transport"))
                                If Not resultado.ContainsKey(cd) Then
                                    resultado.Add(cd, de)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Transportadora.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Transportadora.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class Destinatario

            Public Shared Function Dados() As Dictionary(Of Integer, String)
                Try
                    Dim resultado As New Dictionary(Of Integer, String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_destinat, de_destinat from destinatario")
                    sql.AppendLine("where instancia = :instancia")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd As Integer = If(IsDBNull(rs("cd_destinat")), Nothing, rs("cd_destinat"))
                                Dim de As String = If(IsDBNull(rs("de_destinat")), Nothing, rs("de_destinat"))

                                If Not resultado.ContainsKey(cd) Then
                                    resultado.Add(cd, de)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Destinatario.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Destinatario.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

        End Class

        Public Class TipoMovimento

            Public Shared Function Dados() As Dictionary(Of Integer, String)
                Try
                    Dim resultado As New Dictionary(Of Integer, String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_tp_mov, de_tp_mov from tipo_movimento")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd As Integer = If(IsDBNull(rs("cd_tp_mov")), Nothing, rs("cd_tp_mov"))
                                Dim de As String = If(IsDBNull(rs("de_tp_mov")), Nothing, rs("de_tp_mov"))
                                If Not resultado.ContainsKey(cd) Then
                                    resultado.Add(cd, de)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> TipoMovimento.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("TipoMovimento.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

        End Class

        Public Class Produtos

            Public Shared Function Dados() As Dictionary(Of Long, String)
                Try
                    Dim resultado As New Dictionary(Of Long, String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_produto, de_produto from cad_produto")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd As Long = If(IsDBNull(rs("cd_produto")), Nothing, rs("cd_produto"))
                                Dim de As String = If(IsDBNull(rs("de_produto")), Nothing, rs("de_produto"))
                                If Not resultado.ContainsKey(cd) Then
                                    resultado.Add(cd, de)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Produtos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Produtos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class EmpresaDeposito

            Public Shared Function Dados() As Dictionary(Of Integer, (cd_empresa As String, de_empresa As String, cnpj_cuit As String))
                Try
                    Dim resultado As New Dictionary(Of Integer, (String, String, String))
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select d.cd_deposito, e.cd_empresa, e.de_empresa, e.no_cgc_cpf from empresas e")
                    sql.AppendLine("inner join deposito d on d.int_empresa_erp = e.cd_empresa")
                    sql.AppendLine("where d.instancia = :instancia")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd_deposito As Integer = If(IsDBNull(rs("cd_deposito")), Nothing, rs("cd_deposito"))
                                Dim cd_empresa As String = If(IsDBNull(rs("cd_empresa")), Nothing, rs("cd_empresa"))
                                Dim de_empresa As String = If(IsDBNull(rs("de_empresa")), Nothing, rs("de_empresa"))
                                Dim cnpj_cuit As String = If(IsDBNull(rs("no_cgc_cpf")), Nothing, rs("no_cgc_cpf"))
                                If Not resultado.ContainsKey(cd_deposito) Then
                                    resultado.Add(cd_deposito, (cd_empresa, de_empresa, cnpj_cuit))
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> EmpresaDeposito.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("EmpresaDeposito.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class CadFornTransp
            Public Shared Function Dados() As Dictionary(Of Integer, (String, String))
                Try
                    Dim resultado As New Dictionary(Of Integer, (String, String))
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_fornec,de_fornec,no_cgc_cpf from cad_fornec")
                    sql.AppendLine("where instancia = :instancia")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd_fornec As Integer = If(IsDBNull(rs("cd_fornec")), Nothing, rs("cd_fornec"))
                                Dim de_fornec As String = If(IsDBNull(rs("de_fornec")), Nothing, rs("de_fornec"))
                                Dim no_cgc_cpf As String = If(IsDBNull(rs("no_cgc_cpf")), Nothing, rs("no_cgc_cpf"))
                                If Not resultado.ContainsKey(cd_fornec) Then
                                    resultado.Add(cd_fornec, (de_fornec, no_cgc_cpf))
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> CadFornTransp.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("CadFornTransp.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

        End Class

        Public Class Upnivel1
            Public Shared Function Dados() As Dictionary(Of String, (String, Integer, String))
                Try
                    Dim resultado As New Dictionary(Of String, (String, Integer, String))
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select u.cd_upnivel1, u.de_upnivel1, f.cd_fornec, f.cd_fornec||'-'||f.de_fornec de_fornec from upnivel1 u")
                    sql.AppendLine("inner join fornecs f on f.cd_fornec = u.cd_fornec")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd_upnivel1 As String = If(IsDBNull(rs("cd_upnivel1")), Nothing, rs("cd_upnivel1"))
                                Dim de_upnivel1 As String = If(IsDBNull(rs("de_upnivel1")), Nothing, rs("de_upnivel1"))
                                Dim cd_fornec As Integer = If(IsDBNull(rs("cd_fornec")), Nothing, rs("cd_fornec"))
                                Dim de_fornec As String = If(IsDBNull(rs("de_fornec")), Nothing, rs("de_fornec"))
                                If Not resultado.ContainsKey(cd_upnivel1) Then
                                    resultado.Add(cd_upnivel1, (de_upnivel1, cd_fornec, de_fornec))
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Upnivel1.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Upnivel1.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

        End Class

        Public Class CadVeiculos
            Public Shared Function Dados() As Dictionary(Of String, (String, String))
                Try
                    Dim resultado As New Dictionary(Of String, (String, String))
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select no_placa,no_placa_1 from cad_veiculos")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim no_placa As String = If(IsDBNull(rs("no_placa")), Nothing, rs("no_placa"))
                                Dim no_placa1 As String = If(IsDBNull(rs("no_placa1")), Nothing, rs("no_placa1"))
                                If Not resultado.ContainsKey(no_placa) Then
                                    resultado.Add(no_placa, (no_placa, no_placa1))
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> CadVeiculos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("CadVeiculos.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function

        End Class

        Public Class UnidadeIndustrial
            Public Shared Function Dados() As Dictionary(Of Integer, String)
                Try
                    Dim resultado As New Dictionary(Of Integer, String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_unid_ind, de_unid_ind from unidind")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd As Integer = If(IsDBNull(rs("cd_unid_ind")), Nothing, rs("cd_unid_ind"))
                                Dim de As String = If(IsDBNull(rs("de_unid_ind")), Nothing, rs("de_unid_ind"))
                                If Not resultado.ContainsKey(cd) Then
                                    resultado.Add(cd, de)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> UnidadeIndustrial.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("UnidadeIndustrial.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class Funcionarios
            Public Shared Function Dados() As Dictionary(Of Long, String)
                Try
                    Dim resultado As New Dictionary(Of Long, String)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select cd_func, de_func from funcionars")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim cd_func As Long = If(IsDBNull(rs("cd_func")), Nothing, rs("cd_func"))
                                Dim de_func As String = If(IsDBNull(rs("de_func")), Nothing, rs("de_func"))
                                If Not resultado.ContainsKey(cd_func) Then
                                    resultado.Add(cd_func, de_func)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> Funcionarios.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("Funcionarios.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

        Public Class TblBalanca
            Public Shared Function Dados() As Dictionary(Of String, Integer)
                Try
                    Dim resultado As New Dictionary(Of String, Integer)
                    Dim sql As New StringBuilder
                    sql.Clear()
                    sql.AppendLine("select nm_computador,cd_balanca from tbl_balanca")
                    sql.AppendLine("where instancia = :instancia")
                    sql.AppendLine("order by cd_balanca")
                    Using cmd As OracleCommand = fCmd(sql.ToString)
                        cmd.Parameters.Clear()
                        cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                        Using rs As OracleDataReader = cmd.ExecuteReader
                            While rs.Read
                                Dim nm_computador As String = If(IsDBNull(rs("nm_computador")), Nothing, rs("nm_computador"))
                                Dim cd_balanca As Integer = If(IsDBNull(rs("cd_balanca")), Nothing, rs("cd_balanca"))
                                If Not resultado.ContainsKey(nm_computador) Then
                                    resultado.Add(nm_computador, cd_balanca)
                                End If
                            End While
                        End Using
                    End Using
                    Return resultado
                Catch ex As OracleException
                    MsgBox("OracleErr>> TblBalanca.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                Catch ex As Exception
                    MsgBox("TblBalanca.Dados: " & ex.ToString, MsgBoxStyle.Critical, "Error")
                End Try
            End Function
        End Class

    End Class

    Public Class AptCargas
        Public Shared Function Cargas() As Dictionary(Of (instancia As String, noliberacao As Integer, noseq As Integer),
                                                         (dtentrada As Date, hrentrada As Date, noanalise As Integer,
                                                         qtbruto As Decimal))
            Try
                Dim database As Date = DateAdd(DateInterval.Day, -30, Today)
                Dim result As New Dictionary(Of (String, Integer, Integer), (Date, Date, Integer, Decimal))
                Dim sql As New StringBuilder
                sql.AppendLine("select * from apt_cargas c")
                sql.AppendLine("where c.instancia = :instancia")
                sql.AppendLine("and c.dt_entrada >= :dt_entrada")
                sql.AppendLine("and c.fg_status in ('A','P')")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Parameters.AddRange({
                    New OracleParameter("instancia", OracleDbType.Varchar2) With {.Value = vInstLogin},
                    New OracleParameter("dt_entrada", OracleDbType.Date) With {.Value = database}
                    })
                    Using rs As OracleDataReader = cmd.ExecuteReader
                        While rs.Read
                            Dim instancia As String = rs("instancia")
                            Dim noliberacao As Integer = rs("no_liberacao")
                            Dim noseq As Integer = rs("no_seq")
                            Dim dtentrada As Date = rs("dt_entrada")
                            Dim hrentrada As Date = rs("hr_entrada")
                            Dim noanalise As Integer = If(IsDBNull(rs("no_analise")), 0, rs("no_analise"))
                            Dim qtbruto As Decimal = rs("qt_bruto")
                            If Not result.ContainsKey((instancia, noliberacao, noseq)) Then
                                result.Add((instancia, noliberacao, noseq), (dtentrada, hrentrada, noanalise, qtbruto))
                            End If
                        End While
                    End Using
                End Using
                Return result
            Catch ex As OracleException
                FMensagem("OracleErr>> Impressoes.AptCargas.Cargas: " & ex.ToString, "C")
            Catch ex As Exception
                FMensagem("Impressoes.AptCargas.Cargas: " & ex.ToString, "C")
            End Try
        End Function
    End Class

End Module