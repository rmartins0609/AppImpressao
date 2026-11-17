Imports Oracle.ManagedDataAccess.Client
Module Conexao
    Public conn As OracleConnection
    Public sStateConexao As Boolean = False

    Private WithEvents tmrVerificador As New Timer()
    Private _verificando As Boolean = False

    Public Function FStringConnOracle() As String
        If vGlobal.sSensitiveLogon = "S" Then
            If vGlobal.sUpperCase = "S" Then
                vGlobal.sPassword = UCase(vGlobal.sPassword)
                vGlobal.sUser = UCase(vGlobal.sUser)
            ElseIf vGlobal.sUpperCase = "N" Then
                vGlobal.sPassword = LCase(vGlobal.sPassword)
                vGlobal.sUser = LCase(vGlobal.sUser)
            End If
        End If

        Return $"Password={vGlobal.sPassword};User ID={vGlobal.sUser};Data Source={vGlobal.sServiceName}"

    End Function

    Private Function fStrConexao()
        Try
            If vGlobal.sSensitiveLogon = "S" Then
                If vGlobal.sUpperCase = "S" Then
                    vGlobal.sPassword = UCase(vGlobal.sPassword)
                    vGlobal.sUser = UCase(vGlobal.sUser)
                ElseIf vGlobal.sUpperCase = "N" Then
                    vGlobal.sPassword = LCase(vGlobal.sPassword)
                    vGlobal.sUser = LCase(vGlobal.sUser)
                End If
            End If

            Dim cn As New OracleConnection($"Password={vGlobal.sPassword};User ID={vGlobal.sUser};Data Source={vGlobal.sServiceName}")
            fStrConexao = cn
            Return cn
        Catch ex As Exception
            Exit Function
        End Try
    End Function

    Public Function fConexao() As OracleConnection
        Try
            sStateConexao = False
            conn = fStrConexao()
            conn.Open()

            If conn.State = ConnectionState.Open Then
                frmReimpressao.PbStatusConexao.BackColor = Color.YellowGreen
                sStateConexao = True
            ElseIf conn.State = ConnectionState.Closed Then
                frmReimpressao.PbStatusConexao.BackColor = Color.Red
                sStateConexao = False
            End If

            Return conn
        Catch ex As Exception
            frmReimpressao.PbStatusConexao.BackColor = Color.Red
            sStateConexao = False
            vGlobal.vMensangem += ex.ToString
            fGravaError("Conexao", "fConexao", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
            Exit Function
        End Try
    End Function

    Public Function fCmd(ByVal sql As String) As OracleCommand
        Try
            Dim cmd As OracleCommand = New OracleCommand(sql, conn)
            cmd.BindByName = True
            Return cmd
        Catch ex As Exception
        End Try
    End Function


    Dim conexaoOk As New Threading.ManualResetEvent(False)

    ' Separamos a função: ela só testa o banco e não sabe que a tela existe
    Public Async Function fValidaConexaoAsync() As Task(Of Boolean)
        Using c As New OracleConnection(FStringConnOracle())
            Try
                ' OpenAsync abre a conexão em segundo plano, liberando a interface do usuário
                c.Open()
                Return True
            Catch ex As Exception
                ' Você pode logar o erro "ex.Message" aqui se quiser saber o motivo exato da falha
                Return False
            End Try
        End Using
    End Function


    ' ==========================================
    ' VERIFICAÇÃO AUTOMÁTICA EM BACKGROUND (TIMER)
    ' ==========================================
    Public Sub IniciarVerificadorConexao()
        tmrVerificador.Interval = 10000 ' 10 segundos
        tmrVerificador.Start()
    End Sub

    Private Sub tmrVerificador_Tick(sender As Object, e As EventArgs) Handles tmrVerificador.Tick
        If _verificando Then Return
        _verificando = True

        Try
            If conn Is Nothing OrElse conn.State = ConnectionState.Closed OrElse conn.State = ConnectionState.Broken Then
                Dim novaConn As OracleConnection = fStrConexao()
                If novaConn IsNot Nothing Then
                    novaConn.Open()
                    conn = novaConn
                    AtualizarStatusInterface(True)
                End If
            Else
                If Not TestarConexaoSilencioso() Then
                    AtualizarStatusInterface(False)
                End If
            End If
        Catch ex As Exception
            AtualizarStatusInterface(False)
        Finally
            _verificando = False
        End Try
    End Sub

    ' ==========================================
    ' TESTE SILENCIOSO (ATUALIZA A UI DIRETAMENTE)
    ' ==========================================
    Public Function TestarConexaoSilencioso() As Boolean
        Try
            If conn Is Nothing OrElse conn.State <> ConnectionState.Open Then
                conn = fStrConexao()
                If conn IsNot Nothing Then conn.Open()
            End If

            Using cmd As New OracleCommand("select '1' from dual", conn)
                cmd.ExecuteScalar()
            End Using

            AtualizarStatusInterface(True)
            Return True
        Catch ex As Exception
            AtualizarStatusInterface(False)
            Return False
        End Try
    End Function

    ' ==========================================
    ' ATUALIZAÇÃO SEGURA DA INTERFACE (UI)
    ' ==========================================
    Private Sub AtualizarStatusInterface(ByVal conectado As Boolean)
        sStateConexao = conectado

        ' Procura pelo formulário ativo de forma genérica ou específica
        Dim frm = Application.OpenForms("frmReimpressao") ' Ou "frmReimpressao" dependendo da tela atual
        If frm IsNot Nothing Then
            ' Exemplo usando o controle PbStatusConexao que você já utilizava
            Dim pb = CType(frm.Controls("PbStatusConexao"), PictureBox) ' Ou o nome exato do componente de status
            If pb IsNot Nothing Then
                pb.BackColor = If(conectado, Color.YellowGreen, Color.Red)
            End If
        End If
    End Sub

End Module
