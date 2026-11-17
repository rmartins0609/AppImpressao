<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmReimpressao
    Inherits System.Windows.Forms.Form

    'Descartar substituições de formulário para limpar a lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Exigido pelo Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'OBSERVAÇÃO: o procedimento a seguir é exigido pelo Windows Form Designer
    'Pode ser modificado usando o Windows Form Designer.  
    'Não o modifique usando o editor de códigos.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmReimpressao))
        Me.lbTexto = New System.Windows.Forms.Label()
        Me.tbBoletim = New System.Windows.Forms.TextBox()
        Me.btImprimir = New System.Windows.Forms.Button()
        Me.PrintImpurezaMineralMD01 = New System.Drawing.Printing.PrintDocument()
        Me.PrintPesagProdutos_Laredo = New System.Drawing.Printing.PrintDocument()
        Me.Timer = New System.Windows.Forms.Timer(Me.components)
        Me.NotifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.PrintZebra = New System.Drawing.Printing.PrintDocument()
        Me.PrintDialog1 = New System.Windows.Forms.PrintDialog()
        Me.PrintPreviewDialog1 = New System.Windows.Forms.PrintPreviewDialog()
        Me.rbOrdemCarga = New System.Windows.Forms.RadioButton()
        Me.rbExtratoSaida = New System.Windows.Forms.RadioButton()
        Me.PrintOrdemCarga_Balcanes = New System.Drawing.Printing.PrintDocument()
        Me.PrintExtratoSaida_Balcanes = New System.Drawing.Printing.PrintDocument()
        Me.rbBolAnl = New System.Windows.Forms.RadioButton()
        Me.PrintBolAnl_Balcanes = New System.Drawing.Printing.PrintDocument()
        Me.lbVersao = New System.Windows.Forms.Label()
        Me.PbStatusConexao = New System.Windows.Forms.PictureBox()
        Me.BtStatus = New System.Windows.Forms.Button()
        Me.PrintOrdemCarga_Budeguer = New System.Drawing.Printing.PrintDocument()
        Me.PrintExtratoSaida_Budeguer = New System.Drawing.Printing.PrintDocument()
        Me.PrintBolAnl_Laredo = New System.Drawing.Printing.PrintDocument()
        Me.PrintExtratoSaida_Laredo = New System.Drawing.Printing.PrintDocument()
        Me.RbExtProducto = New System.Windows.Forms.RadioButton()
        CType(Me.PbStatusConexao, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lbTexto
        '
        Me.lbTexto.AutoSize = True
        Me.lbTexto.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbTexto.Location = New System.Drawing.Point(16, 18)
        Me.lbTexto.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lbTexto.Name = "lbTexto"
        Me.lbTexto.Size = New System.Drawing.Size(52, 16)
        Me.lbTexto.TabIndex = 0
        Me.lbTexto.Text = "lbTexto"
        '
        'tbBoletim
        '
        Me.tbBoletim.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbBoletim.Location = New System.Drawing.Point(99, 15)
        Me.tbBoletim.Margin = New System.Windows.Forms.Padding(4)
        Me.tbBoletim.Name = "tbBoletim"
        Me.tbBoletim.Size = New System.Drawing.Size(303, 22)
        Me.tbBoletim.TabIndex = 1
        '
        'btImprimir
        '
        Me.btImprimir.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btImprimir.Location = New System.Drawing.Point(411, 15)
        Me.btImprimir.Margin = New System.Windows.Forms.Padding(4)
        Me.btImprimir.Name = "btImprimir"
        Me.btImprimir.Size = New System.Drawing.Size(100, 28)
        Me.btImprimir.TabIndex = 2
        Me.btImprimir.Text = "Imprimir"
        Me.btImprimir.UseVisualStyleBackColor = True
        '
        'PrintImpurezaMineralMD01
        '
        Me.PrintImpurezaMineralMD01.DocumentName = "Impureza Mineral MD01"
        '
        'PrintPesagProdutos_Laredo
        '
        Me.PrintPesagProdutos_Laredo.DocumentName = "Extrato Pesagem Produtos"
        '
        'Timer
        '
        Me.Timer.Interval = 1000
        '
        'NotifyIcon1
        '
        Me.NotifyIcon1.Icon = CType(resources.GetObject("NotifyIcon1.Icon"), System.Drawing.Icon)
        Me.NotifyIcon1.Text = "LuxArvum - Impresión de Extractos de Pesaje"
        Me.NotifyIcon1.Visible = True
        '
        'PrintZebra
        '
        Me.PrintZebra.DocumentName = "BoletinMateriaExtraña"
        '
        'PrintDialog1
        '
        Me.PrintDialog1.UseEXDialog = True
        '
        'PrintPreviewDialog1
        '
        Me.PrintPreviewDialog1.AutoScrollMargin = New System.Drawing.Size(0, 0)
        Me.PrintPreviewDialog1.AutoScrollMinSize = New System.Drawing.Size(0, 0)
        Me.PrintPreviewDialog1.ClientSize = New System.Drawing.Size(400, 300)
        Me.PrintPreviewDialog1.Enabled = True
        Me.PrintPreviewDialog1.Icon = CType(resources.GetObject("PrintPreviewDialog1.Icon"), System.Drawing.Icon)
        Me.PrintPreviewDialog1.Name = "PrintPreviewDialog1"
        Me.PrintPreviewDialog1.Visible = False
        '
        'rbOrdemCarga
        '
        Me.rbOrdemCarga.AutoSize = True
        Me.rbOrdemCarga.Location = New System.Drawing.Point(99, 58)
        Me.rbOrdemCarga.Name = "rbOrdemCarga"
        Me.rbOrdemCarga.Size = New System.Drawing.Size(121, 20)
        Me.rbOrdemCarga.TabIndex = 3
        Me.rbOrdemCarga.TabStop = True
        Me.rbOrdemCarga.Text = "Orden de Carga"
        Me.rbOrdemCarga.UseVisualStyleBackColor = True
        '
        'rbExtratoSaida
        '
        Me.rbExtratoSaida.AutoSize = True
        Me.rbExtratoSaida.Location = New System.Drawing.Point(229, 58)
        Me.rbExtratoSaida.Name = "rbExtratoSaida"
        Me.rbExtratoSaida.Size = New System.Drawing.Size(134, 20)
        Me.rbExtratoSaida.TabIndex = 4
        Me.rbExtratoSaida.TabStop = True
        Me.rbExtratoSaida.Text = "Extracto de Salida"
        Me.rbExtratoSaida.UseVisualStyleBackColor = True
        '
        'PrintOrdemCarga_Balcanes
        '
        Me.PrintOrdemCarga_Balcanes.DocumentName = "Orden de Carga"
        '
        'PrintExtratoSaida_Balcanes
        '
        Me.PrintExtratoSaida_Balcanes.DocumentName = "Extrato de Saida"
        '
        'rbBolAnl
        '
        Me.rbBolAnl.AutoSize = True
        Me.rbBolAnl.Location = New System.Drawing.Point(370, 58)
        Me.rbBolAnl.Name = "rbBolAnl"
        Me.rbBolAnl.Size = New System.Drawing.Size(163, 20)
        Me.rbBolAnl.TabIndex = 5
        Me.rbBolAnl.TabStop = True
        Me.rbBolAnl.Text = "Talón para Laboratorio"
        Me.rbBolAnl.UseVisualStyleBackColor = True
        '
        'PrintBolAnl_Balcanes
        '
        Me.PrintBolAnl_Balcanes.DocumentName = "BoletinAnalisis"
        '
        'lbVersao
        '
        Me.lbVersao.AutoSize = True
        Me.lbVersao.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbVersao.Location = New System.Drawing.Point(12, 90)
        Me.lbVersao.Name = "lbVersao"
        Me.lbVersao.Size = New System.Drawing.Size(32, 12)
        Me.lbVersao.TabIndex = 6
        Me.lbVersao.Text = "Label1"
        '
        'PbStatusConexao
        '
        Me.PbStatusConexao.Location = New System.Drawing.Point(534, 90)
        Me.PbStatusConexao.Name = "PbStatusConexao"
        Me.PbStatusConexao.Size = New System.Drawing.Size(26, 20)
        Me.PbStatusConexao.TabIndex = 7
        Me.PbStatusConexao.TabStop = False
        '
        'BtStatus
        '
        Me.BtStatus.Enabled = False
        Me.BtStatus.Location = New System.Drawing.Point(504, 90)
        Me.BtStatus.Name = "BtStatus"
        Me.BtStatus.Size = New System.Drawing.Size(24, 23)
        Me.BtStatus.TabIndex = 8
        Me.BtStatus.UseVisualStyleBackColor = True
        '
        'PrintOrdemCarga_Budeguer
        '
        '
        'PrintExtratoSaida_Budeguer
        '
        '
        'PrintBolAnl_Laredo
        '
        Me.PrintBolAnl_Laredo.DocumentName = "Boletim de Analise"
        '
        'PrintExtratoSaida_Laredo
        '
        Me.PrintExtratoSaida_Laredo.DocumentName = "Extracto_Salida"
        '
        'RbExtProducto
        '
        Me.RbExtProducto.AutoSize = True
        Me.RbExtProducto.Location = New System.Drawing.Point(99, 83)
        Me.RbExtProducto.Name = "RbExtProducto"
        Me.RbExtProducto.Size = New System.Drawing.Size(100, 20)
        Me.RbExtProducto.TabIndex = 9
        Me.RbExtProducto.TabStop = True
        Me.RbExtProducto.Text = "Ext Producto"
        Me.RbExtProducto.UseVisualStyleBackColor = True
        '
        'frmReimpressao
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(572, 115)
        Me.Controls.Add(Me.RbExtProducto)
        Me.Controls.Add(Me.BtStatus)
        Me.Controls.Add(Me.PbStatusConexao)
        Me.Controls.Add(Me.lbVersao)
        Me.Controls.Add(Me.rbBolAnl)
        Me.Controls.Add(Me.rbExtratoSaida)
        Me.Controls.Add(Me.rbOrdemCarga)
        Me.Controls.Add(Me.btImprimir)
        Me.Controls.Add(Me.tbBoletim)
        Me.Controls.Add(Me.lbTexto)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.Name = "frmReimpressao"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "App Impressões"
        CType(Me.PbStatusConexao, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lbTexto As Label
    Friend WithEvents tbBoletim As TextBox
    Friend WithEvents btImprimir As Button
    Friend WithEvents PrintImpurezaMineralMD01 As Printing.PrintDocument
    Friend WithEvents PrintPesagProdutos_Laredo As Printing.PrintDocument
    Friend WithEvents Timer As Timer
    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents PrintZebra As Printing.PrintDocument
    Friend WithEvents PrintDialog1 As PrintDialog
    Friend WithEvents PrintPreviewDialog1 As PrintPreviewDialog
    Friend WithEvents rbOrdemCarga As RadioButton
    Friend WithEvents rbExtratoSaida As RadioButton
    Friend WithEvents PrintOrdemCarga_Balcanes As Printing.PrintDocument
    Friend WithEvents PrintExtratoSaida_Balcanes As Printing.PrintDocument
    Friend WithEvents rbBolAnl As RadioButton
    Friend WithEvents PrintBolAnl_Balcanes As Printing.PrintDocument
    Friend WithEvents lbVersao As Label
    Friend WithEvents PbStatusConexao As PictureBox
    Friend WithEvents BtStatus As Button
    Friend WithEvents PrintOrdemCarga_Budeguer As Printing.PrintDocument
    Friend WithEvents PrintExtratoSaida_Budeguer As Printing.PrintDocument
    Friend WithEvents PrintBolAnl_Laredo As Printing.PrintDocument
    Friend WithEvents PrintExtratoSaida_Laredo As Printing.PrintDocument
    Friend WithEvents RbExtProducto As RadioButton
End Class
