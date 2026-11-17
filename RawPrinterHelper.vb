Imports System.IO
Imports System.Runtime.InteropServices
Public Class RawPrinterHelper

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Structure DOCINFOW
        <MarshalAs(UnmanagedType.LPWStr)> Public pDocName As String
        <MarshalAs(UnmanagedType.LPWStr)> Public pOutputFile As String
        <MarshalAs(UnmanagedType.LPWStr)> Public pDataType As String
    End Structure

    <DllImport("winspool.Drv", EntryPoint:="OpenPrinterW",
           SetLastError:=True, CharSet:=CharSet.Unicode,
           ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function OpenPrinter(ByVal src As String, ByRef hPrinter As IntPtr, ByVal pd As IntPtr) As Boolean
    End Function

    <DllImport("winspool.Drv", EntryPoint:="ClosePrinter",
           SetLastError:=True, CharSet:=CharSet.Unicode,
           ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function ClosePrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function

    <DllImport("winspool.Drv", EntryPoint:="StartDocPrinterW",
           SetLastError:=True, CharSet:=CharSet.Unicode,
           ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function StartDocPrinter(ByVal hPrinter As IntPtr, ByVal level As Int32, ByRef pDI As DOCINFOW) As Boolean
    End Function

    <DllImport("winspool.Drv", EntryPoint:="EndDocPrinter",
           SetLastError:=True, CharSet:=CharSet.Unicode,
           ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function EndDocPrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function

    <DllImport("winspool.Drv", EntryPoint:="StartPagePrinter",
           SetLastError:=True, CharSet:=CharSet.Unicode,
           ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function StartPagePrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function

    <DllImport("winspool.Drv", EntryPoint:="EndPagePrinter",
           SetLastError:=True, CharSet:=CharSet.Unicode,
           ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function EndPagePrinter(ByVal hPrinter As IntPtr) As Boolean
    End Function

    <DllImport("winspool.Drv", EntryPoint:="WritePrinter",
           SetLastError:=True, CharSet:=CharSet.Unicode,
           ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Public Shared Function WritePrinter(ByVal hPrinter As IntPtr, ByVal pBytes As IntPtr, ByVal dwCount As Int32, ByRef dwWritten As Int32) As Boolean
    End Function

    ' Envio via Ponteiro (Memória não gerenciada)
    Public Shared Function SendBytesToPrinter(ByVal szPrinterName As String, ByVal pBytes As IntPtr, ByVal dwCount As Int32, Optional szDocName As String = "ImpressaoRaw") As Boolean
        Dim hPrinter As IntPtr
        Dim dwError As Int32
        Dim di As DOCINFOW
        Dim dwWritten As Int32
        Dim bSuccess As Boolean = False

        With di
            .pDocName = szDocName 'Nome da impressão que está sendo realizado
            .pDataType = "RAW"
        End With

        If OpenPrinter(szPrinterName, hPrinter, IntPtr.Zero) Then
            If StartDocPrinter(hPrinter, 1, di) Then
                If StartPagePrinter(hPrinter) Then
                    bSuccess = WritePrinter(hPrinter, pBytes, dwCount, dwWritten)
                    EndPagePrinter(hPrinter)
                End If
                EndDocPrinter(hPrinter)
            End If
            ClosePrinter(hPrinter)
        End If

        If Not bSuccess Then
            dwError = Marshal.GetLastWin32Error()
        End If

        Return bSuccess
    End Function

    ' Sobrecarga para envio direto de Byte()
    Public Shared Function SendBytesToPrinter(ByVal szPrinterName As String, ByVal bytes() As Byte, Optional szDocName As String = "ImpressaoRaw") As Boolean
        Dim pUnmanagedBytes As IntPtr = Marshal.AllocCoTaskMem(bytes.Length)
        Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length)

        Dim bSuccess As Boolean = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, bytes.Length)

        Marshal.FreeCoTaskMem(pUnmanagedBytes)
        Return bSuccess
    End Function

    Public Shared Function SendFileToPrinter(ByVal szPrinterName As String, ByVal szFileName As String) As Boolean
        Using fs As New FileStream(szFileName, FileMode.Open, FileAccess.Read)
            Using br As New BinaryReader(fs)
                Dim bytes() As Byte = br.ReadBytes(CInt(fs.Length))
                Return SendBytesToPrinter(szPrinterName, bytes)
            End Using
        End Using
    End Function

    Public Shared Function SendStringToPrinter(ByVal szPrinterName As String, ByVal szString As String) As Boolean
        Dim pBytes As IntPtr = Marshal.StringToCoTaskMemAnsi(szString)
        Dim bSuccess As Boolean = SendBytesToPrinter(szPrinterName, pBytes, szString.Length)
        Marshal.FreeCoTaskMem(pBytes)
        Return bSuccess
    End Function

End Class
