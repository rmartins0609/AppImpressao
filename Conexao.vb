Imports System.Data.OracleClient
Module Conexao
    <Obsolete>
    Public conn As OracleConnection
    Public sStateConexao As Boolean = False

    <Obsolete>
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

            Dim cn As New OracleClient.OracleConnection("Password=" & vGlobal.sPassword & ";User ID=" & vGlobal.sUser & ";Data Source=" & vGlobal.sServiceName & "")

            fStrConexao = cn
            Return cn
        Catch ex As Exception
            Exit Function
        End Try
    End Function

    <Obsolete>
    Public Function fConexao()
        Try
            sStateConexao = False
            Dim conn As OracleConnection = fStrConexao()
            conn.Open()

            fConexao = conn

            If conn.State = ConnectionState.Open Then
                frmReimpressao.PbStatusConexao.BackColor = Color.Green
                sStateConexao = True
            ElseIf conn.State = ConnectionState.Closed Then
                frmReimpressao.PbStatusConexao.BackColor = Color.Red
                sStateConexao = False
            End If

        Catch ex As Exception
            frmReimpressao.PbStatusConexao.BackColor = Color.Red
            sStateConexao = False
            vGlobal.vMensangem += ex.ToString
            fGravaError("Conexao", "fConexao", vGlobal.vMensangem, "LOG_ERROR")
            vGlobal.vMensangem = Nothing
            Exit Function
        End Try
    End Function

    <Obsolete>
    Public Function fCmd(ByVal sql As String) As OracleCommand
        Try

            If conn.State = ConnectionState.Closed Then
                Conexao.conn = Conexao.fConexao()
            End If

            Dim cmd As OracleCommand = New OracleCommand(sql, conn)
            frmReimpressao.PbStatusConexao.BackColor = Color.YellowGreen
            frmReimpressao.BtStatus.BackColor = Color.YellowGreen
            Return cmd
        Catch ex As Exception
            frmReimpressao.PbStatusConexao.BackColor = Color.Red
            frmReimpressao.BtStatus.BackColor = Color.Red
        End Try
    End Function

    Public Function fValidaConexao() As Boolean
        Try
            Dim sql As String
            Dim rs As OracleDataReader
            sql = "select '1' from dual"
            rs = fCmd(sql).ExecuteReader
            If rs.HasRows = True Then
                frmReimpressao.PbStatusConexao.BackColor = Color.YellowGreen
                frmReimpressao.BtStatus.BackColor = Color.YellowGreen
                Return True
            Else
                frmReimpressao.PbStatusConexao.BackColor = Color.Red
                frmReimpressao.BtStatus.BackColor = Color.Red
                Return False
            End If
        Catch ex As Exception
            frmReimpressao.PbStatusConexao.BackColor = Color.Red
            frmReimpressao.BtStatus.BackColor = Color.Red
            Return False
        End Try
    End Function
End Module
