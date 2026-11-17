Imports System.IO
Imports System.Reflection
Public Class CriaArquivosDLL
    Public Shared Sub GaranteDllsNoDiretorio()
        ' O namespace padrão do seu projeto (ajuste para o nome real do seu projeto)
        Dim nomeProjeto As String = "ApplImpressao"

        ' O DICIONÁRIO QUE VOCÊ PENSOU: 
        ' Chave = Nome do arquivo físico que o sistema precisa na pasta
        ' Valor = Caminho do recurso embutido (Formato: NomeDoProjeto.NomeDaDll.dll)
        Dim dllsNecessarias As New Dictionary(Of String, String) From {
            {"Oracle.ManagedDataAccess.dll", $"{nomeProjeto}.Oracle.ManagedDataAccess.dll"},
            {"Microsoft.ReportViewer.WinForms.dll", $"{nomeProjeto}.Microsoft.ReportViewer.WinForms.dll"}
        }

        ' Percorre o seu dicionário
        For Each kvp In dllsNecessarias
            Dim nomeArquivoFisico As String = kvp.Key
            Dim caminhoRecursoInterno As String = kvp.Value

            ' Define o caminho onde a DLL deve existir (pasta do .exe)
            Dim caminhoDestino As String = Path.Combine(Application.StartupPath, nomeArquivoFisico)

            ' SE NÃO ENCONTRAR A DLL NO DIRETÓRIO...
            If Not File.Exists(caminhoDestino) Then
                Try
                    ' Pega o assembly atual do sistema
                    Dim assem As Assembly = Assembly.GetExecutingAssembly()

                    ' Abre o fluxo de leitura dos bytes escondidos dentro do .exe
                    Using streamInterno As Stream = assem.GetManifestResourceStream(caminhoRecursoInterno)

                        ' Se encontrou o recurso embutido com sucesso
                        If streamInterno IsNot Nothing Then

                            ' Cria o arquivo físico na pasta
                            Using streamDestino As New FileStream(caminhoDestino, FileMode.Create, FileAccess.Write)
                                ' O .NET copia todos os bytes de dentro do seu .exe direto para o HD do cliente
                                streamInterno.CopyTo(streamDestino)
                            End Using

                        End If
                    End Using
                Catch ex As Exception
                    ' Grava no log caso dê algum chabu de permissão de pasta do Windows
                    fGravaError("Inicializacao", "GaranteDlls", "Erro ao extrair DLL: " & ex.Message, "LOG_ERROR")
                End Try
            End If
        Next
    End Sub
End Class
