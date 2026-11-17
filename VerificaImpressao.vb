Imports System.Text
Imports AppImpressao.frmReimpressao
Imports Oracle.ManagedDataAccess.Client
Public Class VerificaImpressao
    Public Shared Function FVerBolEntrLaredo() As (sucesso As Boolean, lista As List(Of LiberImprEntrada))
        Try
            fGravaLog("2. Mirando numero boletin", "LOG_TESTE")
            'Dim swhere2 As String = Nothing
            'Select Case vGlobal.vTICKETENTRSORT
            '    Case "N" : swhere2 = " and c.fg_analise = 'S'"
            '    Case "S" : swhere2 = " and (c.fg_analise in ('S','N') or c.fg_analise is null)"
            'End Select
            Dim lista As New List(Of LiberImprEntrada)

            Dim sql As New StringBuilder
            sql.AppendLine("select c.no_liberacao, c.hr_entrada from apt_cargas c")
            sql.AppendLine("inner join parametros p on p.instancia = c.instancia")
            sql.AppendLine("where c.instancia = :instancia")
            'sql.AppendLine("  and c.fg_impr_bol_entr = :fg_impr_bol_entr")
            sql.AppendLine("  and p.secao = :secao")
            sql.AppendLine("  and p.entrada = :entrada")
            sql.AppendLine("  and c.dt_entrada > p.valor")
            'sql.AppendLine(swhere2)
            'sql.AppendLine("  and rownum = 1")
            sql.AppendLine("  and c.hr_entrada > :hr_entrada")
            'sql.AppendLine("  and c.no_liberacao > :no_liberacao")
            Using cmd As OracleCommand = fCmd(sql.ToString)
                cmd.Parameters.Clear()
                cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vGlobal.vInstLogin
                cmd.Parameters.Add("fg_impr_bol_entr", OracleDbType.Varchar2).Value = "N"
                cmd.Parameters.Add("secao", OracleDbType.Varchar2).Value = "RCMP"
                cmd.Parameters.Add("entrada", OracleDbType.Varchar2).Value = "DT_PROCESSO"
                cmd.Parameters.Add("hr_entrada", OracleDbType.Date).Value = vGlobal.DtHrInicial
                'cmd.Parameters.Add("no_liberacao", OracleDbType.Varchar2).Value = vGlobal.NoLiberacaoEntrada
                Dim textsql As String = GetDebugSql(cmd)
                Console.WriteLine($"{Now()}{vbCrLf} SQL: {textsql}")
                Using rs As OracleDataReader = cmd.ExecuteReader
                    While rs.Read
                        lista.Add(New LiberImprEntrada With {
                    .liberacao = If(rs("no_liberacao") IsNot DBNull.Value, Convert.ToInt32(rs("no_liberacao")), 0),
                    .hrentrada = If(rs("hr_entrada") IsNot DBNull.Value, Convert.ToDateTime(rs("hr_entrada")), DateTime.MinValue)
                      })
                    End While
                    'vGlobal.DtHrInicial = Convert.ToDateTime(rs("hr_entrada"))
                    'Dim temvalor As Boolean = rs.Read()
                    'If temvalor Then
                    'noLiberacaoBolEntr = rs("no_liberacao")
                    'vGlobal.NoLiberacaoEntrada = noLiberacaoBolEntr
                    'vGlobal.DtHrInicial = Convert.ToDateTime(rs("hr_entrada"))
                    'fGravaLog("5. Finalizando Mirando numero boletin", "LOG_TESTE")
                    'Return True
                    'End If

                    'While rs.Read
                    '    noLiberacaoBolEntr = rs("no_liberacao")
                    '    'AtualizaBolEntr_Laredo(noLiberacaoBolEntr)
                    '    fGravaLog("5. Finalizando Mirando numero boletin", "LOG_TESTE")
                    '    Return True
                    'End While
                End Using

                fGravaLog("6. Finalizando Mirando numero boletin", "LOG_TESTE")

                Dim temvalor As Boolean = lista.Any()
                If lista.Any Then
                    vGlobal.DtHrInicial = lista.Select(Function(x) x.hrentrada).FirstOrDefault()
                End If
                Return (temvalor, lista)
            End Using
        Catch ex As Exception

        End Try
    End Function

    Public Shared Function FVerBolLabLaredo() As Boolean
        Try
            Dim swhere2 As String = Nothing
            Select Case vGlobal.vTICKETENTRSORT
                Case "N" : swhere2 = " and c.fg_analise = 'S'"
                Case "S" : swhere2 = " and (c.fg_analise in ('S','N') or c.fg_analise is null)"
            End Select

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
                cmd.Parameters.Add("fg_impr_bol_entr", OracleDbType.Varchar2).Value = "N"
                cmd.Parameters.Add("secao", OracleDbType.Varchar2).Value = "RCMP"
                cmd.Parameters.Add("entrada", OracleDbType.Varchar2).Value = "DT_PROCESSO"
                Using rs As OracleDataReader = cmd.ExecuteReader
                    Dim temvalor As Boolean = rs.Read()
                    If temvalor Then
                        noLiberacaoBolEntr = rs("no_liberacao")
                        fGravaLog("5. Finalizando Mirando numero boletin", "LOG_TESTE")
                        Return True
                    End If
                End Using
                fGravaLog("6. Finalizando Mirando numero boletin", "LOG_TESTE")
                Return False
            End Using
        Catch ex As Exception

        End Try
    End Function

    Public Class LiberImprEntrada
        Public Property liberacao As Integer
        Public Property hrentrada As Date


    End Class
End Class
