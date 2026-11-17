Imports System.IO

Module Inicializar
    Public Sub Main()
        ' 1. A primeiríssima coisa: joga as DLLs na pasta. 
        ' Como esse módulo não usa nada do Oracle, o .NET não vai quebrar ao abrir!
        'CriaArquivosDLL.GaranteDllsNoDiretorio()

        ' 2. Cria o arquivo de configuração padrão
        If IO.File.Exists(Application.StartupPath & "\configuracao.ini") Then
            dadosarquivoini = GeraArquivoINI.Dados(Application.StartupPath & "\configuracao.ini")
        End If
        AtualizaVariaveis()
        ListaDicionarioParametros()
        GeraArquivoINI.CriaArquivoConfic()

        ' 3. Agora que as DLLs já foram "cuspidas" na pasta com sucesso, 
        ' é 100% seguro abrir o seu formulário principal!
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New frmReimpressao())
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
End Module
