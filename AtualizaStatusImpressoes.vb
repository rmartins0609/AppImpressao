Imports Oracle.ManagedDataAccess.Client
Imports System.Text
Public Class AtualizaStatusImpressoes
    Public Shared Sub AtualizaBolEntr_Laredo(noLiberacao)
        Using t As OracleTransaction = conn.BeginTransaction
            Try
                fGravaLog("3. Inicio da atualizado de status do boletim de entrada", "LOG_TESTE")
                Dim sql As New StringBuilder
                sql.AppendLine("update apt_cargas")
                sql.AppendLine("set fg_impr_bol_entr = :fg_impr_bol_entr_s")
                sql.AppendLine("where fg_impr_bol_entr = :fg_impr_bol_entr_n")
                sql.AppendLine("and instancia = :instancia")
                sql.AppendLine("and no_liberacao = :no_liberacao")
                Using cmd As OracleCommand = fCmd(sql.ToString)
                    cmd.Transaction = t
                    cmd.Parameters.Add("instancia", OracleDbType.Varchar2).Value = vInstLogin
                    cmd.Parameters.Add("no_liberacao", OracleDbType.Int32).Value = noLiberacao
                    cmd.Parameters.Add("fg_impr_bol_entr_s", OracleDbType.Varchar2).Value = "S"
                    cmd.Parameters.Add("fg_impr_bol_entr_n", OracleDbType.Varchar2).Value = "N" '''' N
                    cmd.ExecuteNonQuery()
                End Using
                t.Commit()
                fGravaLog("4. Final da atualizado de status do boletim de entrada", "LOG_TESTE")
            Catch ex As Exception
                t.Rollback()
            End Try
        End Using
    End Sub
End Class
