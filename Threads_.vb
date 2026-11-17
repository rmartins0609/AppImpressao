Imports System.Threading
Imports AppImpressao.Cadastros
Public Class Threads_
    Private Shared TimerThreads As Integer = 180000 'Tempo fixo de 3min
    Private Shared workersMap As New Dictionary(Of String, IWorkTask)
    Private Shared workers As New List(Of IWorkTask)

    Public Interface IWorkTask
        Sub Start()
        Sub [Stop]()
    End Interface

    'Classes de threads
    Private Class GenericWorkerTask
        Implements IWorkTask

        Private _cts As CancellationTokenSource
        Private ReadOnly _interval As Integer
        Private ReadOnly _metodo As String

        Public Sub New(intervalo As Integer, metodo As String)
            _interval = intervalo
            _metodo = metodo
        End Sub

        Public Sub Start() Implements IWorkTask.Start
            _cts = New CancellationTokenSource()
            Task.Run(Async Function()
                         While Not _cts.Token.IsCancellationRequested

                             AtualizaProcessos()
                             If AtualizadoresListas.ContainsKey(_metodo) Then
                                 AtualizaListas(_metodo)
                             End If
                             Await Task.Delay(_interval, _cts.Token)
                         End While
                     End Function, _cts.Token)
        End Sub

        Public Sub [Stop]() Implements IWorkTask.Stop
            _cts?.Cancel()
        End Sub
    End Class

    'Inicia e finaliza as threads
    Public Shared Sub IniciaThreads()
        Dim timer As Integer = TimerThreads
        Dim incremento As Integer = 5000

        ' Definições dos cadastros que precisam de worker
        Dim workerDefinitions As New List(Of String) From {
        "ORDEMCARGA",
        "BOLANALISE",
        "EXTRATOSAIDA",
        "EXTRATOPRODUTO",
        "TICKETRAST",
        "BOLETIMENTR",
        "UNIDIND",
        "PARAMETROS",
        "EQUIPTOS",
        "EMPRESAS",
        "DEPOSPRODU",
        "TRANSPORTADORA",
        "UPNIVEL1",
        "DESTINATARIO",
        "TIPOMOVIMENTO",
        "PRODUTOS",
        "EMPRESADEPOS",
        "FORNECTRANSP",
        "VEICULOS",
        "FUNCIONARIOS",
        "TBLBALANCA",
        "CARGAS"
         }

        Dim workerDictProcessos As New Dictionary(Of String, Integer) From {
        {"ORDEMCARGA", Convert.ToInt32(dadosarquivoini(("PARAMETROS", "TIMER_IMPR_ORDEMCARGA")))},
        {"BOLANALISE", Convert.ToInt32(dadosarquivoini(("PARAMETROS", "TIMER_IMPR_BOLANL")))},
        {"EXTRATOSAIDA", Convert.ToInt32(dadosarquivoini(("PARAMETROS", "TIMER_IMPR_EXTRSAIDA")))},
        {"EXTRATOPRODUTO", Convert.ToInt32(dadosarquivoini(("PARAMETROS", "TIMER_IMPR_EXTRPROD")))},
        {"TICKETRAST", Convert.ToInt32(dadosarquivoini(("PARAMETROS", "TIMER_IMPR_TICKETRAST")))},
        {"BOLETIMENTR", Convert.ToInt32(dadosarquivoini(("PARAMETROS", "TIMER_IMPR_BOL_ENTR")))}
        }

        Dim workerDictHabProc As New Dictionary(Of String, String) From {
        {"ORDEMCARGA", dadosarquivoini(("PARAMETROS", "IMPRORDEMCARGA"))},
        {"EXTRATOSAIDA", dadosarquivoini(("PARAMETROS", "IMPREXTRSAIDA"))},
        {"BOLETIMENTR", dadosarquivoini(("PARAMETROS", "IMPRBOLENTR"))},
        {"TICKETSORT", dadosarquivoini(("PARAMETROS", "TICKETENTRSORT"))},
        {"BOLANALISE", dadosarquivoini(("PARAMETROS", "IMPRBOLANL"))},
        {"TICKERRAST", dadosarquivoini(("PARAMETROS", "IMPRETICKETRAST"))},
        {"EXTRPRODUTO", dadosarquivoini(("PARAMETROS", "IMPREXTRPRODUTO"))}
        }

        ' Cria e inicia cada worker usando GenericWorkerTask
        For Each nome In workerDefinitions

            Dim timerNew As Integer = 0
            If workerDictProcessos.ContainsKey(nome) Then
                timerNew = workerDictProcessos(nome)
            Else
                timerNew = timer
            End If

            Dim worker As New GenericWorkerTask(timerNew, nome)
            Dim workerinicia As Boolean = False

            If workerDictHabProc.ContainsKey(nome) Then
                If workerDictHabProc(nome) = "S" Then
                    workerinicia = True
                Else
                    workerinicia = False
                End If
            Else
                workerinicia = True
            End If

            If workerinicia = True Then
                worker.Start()
                workers.Add(worker)
                workersMap.Add(nome, worker)
                If Not workerDictProcessos.ContainsKey(nome) Then
                    timer += incremento
                End If
            End If
        Next
    End Sub

    Public Shared Sub FinalizaThreads()
        For Each w In workers
            w.Stop()
        Next
        workers.Clear()
    End Sub


    'Metodos de criação das listas

    Private Shared ReadOnly AtualizadoresListas As New Dictionary(Of String, Action) From {
        {"TICKETRAST", Sub() cadequiptos = Cadastros.Equiptos.Dados()},
        {"UNIDIND", Sub() cadunidind = Cadastros.UnidadeIndustrial.Dados()},
        {"PARAMETROS", Sub() cadparametros = Cadastros.Parametros.DictParam()},
        {"EQUIPTOS", Sub() cadequiptos = Cadastros.Equiptos.Dados()},
        {"DEPOSPRODU", Sub() caddepositoprod = Cadastros.DepositoProdutos.Dados()},
        {"TRANSPORTADORA", Sub() cadtransportadora = Cadastros.Transportadora.Dados()},
        {"UPNIVEL1", Sub() cadupnivel1 = Cadastros.Upnivel1.Dados()},
        {"DESTINATARIO", Sub() caddestinatario = Cadastros.Destinatario.Dados()},
        {"TIPOMOVIMENTO", Sub() cadtpmovimento = Cadastros.TipoMovimento.Dados()},
        {"PRODUTOS", Sub() cadproduto = Cadastros.Produtos.Dados()},
        {"EMPRESADEPOS", Sub() cadempresadeposito = Cadastros.EmpresaDeposito.Dados()},
        {"FORNECTRANSP", Sub() cadfornectransp = Cadastros.CadFornTransp.Dados},
        {"VEICULOS", Sub() cadveiculos = Cadastros.CadVeiculos.Dados()},
        {"FUNCIONARIOS", Sub() cadfuncionarios = Cadastros.Funcionarios.Dados()},
        {"TBLBALANCA", Sub() cadtblbalanca = Cadastros.TblBalanca.Dados()},
        {"CARGAS", Sub() cargascana = AptCargas.Cargas()}
         }

    Private Shared ReadOnly AtualizadoresProcessos As New Dictionary(Of String, Action) From {
        {"ORDEMCARGA", AddressOf Impressoes.Processos.ImprimeOrdemCarga},
        {"BOLETIMENTR", AddressOf Impressoes.Laredo.ImprBolEntrada.ImprimeEntrada},
        {"BOLANALISE", AddressOf Impressoes.Processos.ImprimeBolAnl},
        {"EXTRATOSAIDA", AddressOf Impressoes.Processos.ImprimeSaida},
        {"EXTRATOPRODUTO", AddressOf Impressoes.Processos.ImprimeExtrProdutos}
         }

    Private Shared Sub AtualizaListas(metodo As String)
        If AtualizadoresListas.ContainsKey(metodo) Then
            AtualizadoresListas(metodo).Invoke()
        End If
    End Sub

    Private Shared Sub AtualizaProcessos()
        Dim workerDictHabProc As New Dictionary(Of String, String) From {
        {"ORDEMCARGA", dadosarquivoini(("PARAMETROS", "IMPRORDEMCARGA"))},
        {"EXTRATOSAIDA", dadosarquivoini(("PARAMETROS", "IMPREXTRSAIDA"))},
        {"BOLETIMENTR", dadosarquivoini(("PARAMETROS", "IMPRBOLENTR"))},
        {"TICKETSORT", dadosarquivoini(("PARAMETROS", "TICKETENTRSORT"))},
        {"BOLANALISE", dadosarquivoini(("PARAMETROS", "IMPRBOLANL"))},
        {"TICKERRAST", dadosarquivoini(("PARAMETROS", "IMPRETICKETRAST"))},
        {"EXTRPRODUTO", dadosarquivoini(("PARAMETROS", "IMPREXTRPRODUTO"))}
        }

        For Each kvp In AtualizadoresProcessos
            If workerDictHabProc(kvp.Key) = "S" Then
                kvp.Value.Invoke
            End If
        Next
    End Sub
End Class
