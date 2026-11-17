Imports System.Runtime.InteropServices
Imports System.Text

Public Class FParametros
    Private Shared Function GetPrivateProfileString(ByVal section As String, ByVal key As String, ByVal def As String, ByVal retVal As StringBuilder, ByVal size As Integer, ByVal filePath As String) As Integer
    End Function

    Public Function GetIniValue(section As String, key As String, filename As String, Optional defaultValue As String = "") As String
        Dim sb As New StringBuilder(500)
        If GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, filename) > 0 Then
            Return sb.ToString
        Else
            Return defaultValue
        End If
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)> Private Shared Function GetPrivateProfileSectionNames(ByVal lpszReturnBuffer As StringBuilder, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    End Function
    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)> Private Shared Function GetPrivateProfileSection(ByVal lpAppName As String, ByVal lpReturnedString As StringBuilder, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
    End Function
    Private Sub BtSalvar_Click(sender As Object, e As EventArgs) Handles BtSalvar.Click
        If MsgBox("Confirma la actualización de los parámetros", MsgBoxStyle.Question + MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            Dim Arquivo = Application.StartupPath & "\configuracao.ini"
            For Each x As DataGridViewRow In DgDados.Rows
                Dim Secao = x.Cells("SECAO").Value
                Dim Chave = x.Cells("CHAVE").Value
                Dim Valor = x.Cells("VALOR").Value
                If Secao = "BANCODADOS" And Chave = "PASSWORD" Then
                    Valor = fCrypto(Valor)
                End If
                If x.Cells("STATUS").Value = ">>" Then
                    fSetParamINI(Secao, Chave, Valor, Arquivo)
                End If

            Next
            ListaArquivo()
            MsgBox("Parámetros actualizados", MsgBoxStyle.Information, "Aviso")
        End If
    End Sub

    Private Sub FParametros_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ListaArquivo()
    End Sub

    Private Sub ListaArquivo()
        Dim secao As String

        Dim iniPath As String = Application.StartupPath & "\configuracao.ini"

        ' Lê todas as linhas do arquivo INI
        Dim lines = IO.File.ReadAllLines(iniPath)

        ' Analisa cada linha do arquivo INI
        DgDados.Rows.Clear()

        For Each line In lines
            ' Verifica se a linha contém uma chave e um valor
            If line.Contains("=") Then
                ' Extrai a chave e o valor da linha
                Dim parts = line.Split("="c)
                Dim key = parts(0).Trim()
                Dim value = parts(1).Trim()

                ' Adiciona o par chave-valor à tabela de dados
                DgDados.Rows.Add("", secao, key, value)
            Else
                secao = line.ToString.Replace("[", "").Replace("]", "")
            End If
        Next
    End Sub

    Private Sub DgDados_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles DgDados.CellValueChanged
        Dim columnindex As Integer = e.ColumnIndex
        Dim rowindex As Integer = e.RowIndex
        If columnindex >= 1 Then
            If rowindex >= 0 Then
                DgDados.Rows(rowindex).Cells(0).Value = ">>"
            End If
        End If
    End Sub
End Class