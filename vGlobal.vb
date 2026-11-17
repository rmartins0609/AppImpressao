Module vGlobal
    Public sformulario As String
    Public vInstLogin As String
    Public sValor01, sValor02, sValor03, sValor04, sValor05, sValor06, sValor07, sValor08, sValor09, sValor10 As String
    Public nValor01, nValor02, nValor03, nValor04, nValor05, nValor06, nValor07, nValor08, nValor09, nValor10 As Integer
    Public dDate01, dDate02, dDate03, dDate04, dDate05, dDate06, dDate07, dDate08, dDate09, dDate10 As Date
    Public gn01, gn02, gn03, gn04, gn05, gn06, gn07, gn08, gn09, gn10, gn11, gn12, gn13, gn14, gn15, gn16, gn17, gn18, gn19, gn20 As Integer
    Public gs01, gs02, gs03, gs04, gs05, gs06, gs07, gs08, gs09, gs10, gs11, gs12, gs13, gs14, gs15, gs16, gs17, gs18, gs19, gs20 As String

    Public ModeloImprTermica As String

    Public bBotaoImprimir As Boolean
    Public nRegistroImpressao As String

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
    Public sUnidInd As Integer


    Public vModeloImpressao As String
    Public vBalancaCod As String
    Public vTimerImpressao As String
    Public vNomeImpressora As String
    Public vNumeroCopias As String
    Public vNumCopiasExtrProduto, vNumCopiasTicketPatio, vNumCopiasExtrSaidaCana As String
    Public vNumCopiasLaboratCana, vNumCopiasBoletimentr As String
    Public vRetratoPaisagem As String
    Public vImprExtrSaida As String
    Public vImprExtrEntr As String
    Public vImprOrdemCarga As String
    Public vImprBolAnl As String
    Public vImprTicketRast As String
    Public vImprExtrProduto As String
    Public vIMPRLIBSEPARADA As String
    Public vNUMERO_CARACTER_LINHA_OBS As String
    Public vTICKETENTRSORT As String
    Public vImprTela As String
    Public vNomePc As String

    Public nContadorFila As Integer
    Public nContadorListaFila As Integer

    Public vTipoFonte As String
    Public Fonte9R As New Font(vTipoFonte, 9, FontStyle.Regular)
    Public Fonte10R As New Font(vTipoFonte, 10, FontStyle.Regular)
    Public Fonte11R As New Font(vTipoFonte, 11, FontStyle.Regular)
    Public Fonte12R As New Font(vTipoFonte, 12, FontStyle.Regular)

    Public Fonte9B As New Font(vTipoFonte, 9, FontStyle.Bold)
    Public Fonte10B As New Font(vTipoFonte, 10, FontStyle.Bold)
    Public Fonte11B As New Font(vTipoFonte, 11, FontStyle.Bold)
    Public Fonte12B As New Font(vTipoFonte, 12, FontStyle.Bold)

    'Impressão de Boletim de Analises
    Public FgImprLiberAnalise As String
    Public FgImprBoletiEntrLaredo As Boolean

    'Timer Threads
    Public VTimerOrdemCarga, VTimerExtratoSaida, VTimerBolEntr, VTimerBoletimAnalises, VTimerExtrProduto As Integer

    'Budeguer
    'Impressão de dados da Empresa
    Public sDESCR_EMPRESA, sDESCR_L1, sDESCR_L2, sDESCR_L3, sDESCR_L4, EMPRESA_PRD As String

    Public currentCopy As Integer
    Public TipoCortePapel As String

    Public noLiberacao, noLiberacaoBolEntr, noLiberacaoBolAnl As String
    Public hrEntradaCanaLaredo As String

    ''' <summary>
    ''' '''''''''''''''''
    'Parametros para impressão de boletim de entrada
    Public DtHrInicial As DateTime
    Public NoLiberacaoEntrada As Integer
    '''''''''''''''''''
    ''' <summary>

    ''' <summary>
    ''' '''''''''''''''''
    'Parametros para impressão extrato de pesagem
    Public DtHrSaida As DateTime
    Public NoLiberacaoSaida As Integer
    '''''''''''''''''''
    ''' <summary>
    ''' 

    ''' <summary>
    '''''''''''''''''''''
    ''' Parametros para impressão de extrato de produtos
    Public DtHrSaidaProduto As DateTime
    ''' <summary>




    'Dictionarys - Dicionarios de cadastros carregado ao abrir a aplicação
    Public dadosarquivoini As Dictionary(Of (String, String), String)
    Public cadparametros As Dictionary(Of (secao As String, entrada As String), String)
    Public cadempresas As Dictionary(Of Long, String)
    Public cadtransportadora As Dictionary(Of Integer, String)
    Public caddestinatario As Dictionary(Of Integer, String)
    Public cadtipomovimento As Dictionary(Of Integer, String)
    Public cadproduto As Dictionary(Of Long, String)
    Public cadempresadeposito As Dictionary(Of Integer, (cd_empresa As String, de_empresa As String, cnpj_cuit As String))
    Public cadfornectransp As Dictionary(Of Integer, (de_fornec As String, cnpj_cuit As String))
    Public cadupnivel1 As Dictionary(Of String, (de_upnivel1 As String, cd_fornec As Integer, de_fornec As String))
    Public caddepositoprod As Dictionary(Of Integer, (da_deposito As String, de_deposito As String))
    Public cadveiculos As Dictionary(Of String, (no_placa As String, no_placa1 As String))
    Public cadequiptos As Dictionary(Of Integer, (no_placa As String, de_modelo As String))
    Public cadplacaequiptos As Dictionary(Of String, (cd_equipto As Integer, no_placa As String))
    Public cadunidind As Dictionary(Of Integer, String)
    Public cadfuncionarios As Dictionary(Of Long, String)
    Public cadtblbalanca As Dictionary(Of String, Integer)
    Public cadtpmovimento As Dictionary(Of Integer, String)
    Public cargascana As Dictionary(Of (String, Integer, Integer), (Date, Date, Integer, Decimal))

End Module
