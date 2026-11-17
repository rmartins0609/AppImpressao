Imports System.Data.OracleClient
Module vGlobal
    Public sformulario As String
    Public vInstLogin As String
    Public sValor01, sValor02, sValor03, sValor04, sValor05, sValor06, sValor07, sValor08, sValor09, sValor10 As String
    Public nValor01, nValor02, nValor03, nValor04, nValor05, nValor06, nValor07, nValor08, nValor09, nValor10 As Integer
    Public dDate01, dDate02, dDate03, dDate04, dDate05, dDate06, dDate07, dDate08, dDate09, dDate10 As Date
    Public gn01, gn02, gn03, gn04, gn05, gn06, gn07, gn08, gn09, gn10, gn11, gn12, gn13, gn14, gn15, gn16, gn17, gn18, gn19, gn20 As Integer
    Public gs01, gs02, gs03, gs04, gs05, gs06, gs07, gs08, gs09, gs10, gs11, gs12, gs13, gs14, gs15, gs16, gs17, gs18, gs19, gs20 As String

    Public Arquivo As String
    Public Reduzido, CdRed1, CdRed2, QtTamUpn1, QtTamUpn2, QtTamUpn3, nBocaDes, vMetodoBaixa As String

    Public vVersion As String = String.Format("{1} {2}",
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
    System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToShortDateString(),
    System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToShortTimeString())

    Public sServiceName As String
    Public sUser As String
    Public sPassword As String
    Public sSensitiveLogon As String
    Public sUpperCase As String

    Public vMensangem As String

    Public sNomeEmpresa As String
    Public sCliente As String


    Public vModeloImpressao As String
    Public vBalancaCod As String
    Public vTimerImpressao As String
    Public vNomeImpressora As String
    Public vNumeroCopias As String
    Public vImprExtrSaida As String
    Public vImprOrdemCarga As String
    Public vImprBolAnl As String
    Public vImprExtrProduto As String
    Public vIMPRLIBSEPARADA As String

    Public nContadorFila As Integer
    Public nContadorListaFila As Integer

    Public TipoFonte As String = "Arial"
    Public Fonte9R As New Font(TipoFonte, 9, FontStyle.Regular)
    Public Fonte10R As New Font(TipoFonte, 10, FontStyle.Regular)
    Public Fonte11R As New Font(TipoFonte, 11, FontStyle.Regular)
    Public Fonte12R As New Font(TipoFonte, 12, FontStyle.Regular)

    Public Fonte9B As New Font(TipoFonte, 9, FontStyle.Bold)
    Public Fonte10B As New Font(TipoFonte, 10, FontStyle.Bold)
    Public Fonte11B As New Font(TipoFonte, 11, FontStyle.Bold)
    Public Fonte12B As New Font(TipoFonte, 12, FontStyle.Bold)

    'Timer Threads
    Public VTimerOrdemCarga, VTimerExtratoSaida, VTimerBoletimAnalises, VTimerExtrProduto As Integer


End Module
