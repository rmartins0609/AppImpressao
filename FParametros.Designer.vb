<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FParametros
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
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.BtSalvar = New System.Windows.Forms.Button()
        Me.DgDados = New System.Windows.Forms.DataGridView()
        Me.Status = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.SECAO = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.CHAVE = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.VALOR = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.DgDados, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'BtSalvar
        '
        Me.BtSalvar.Location = New System.Drawing.Point(12, 12)
        Me.BtSalvar.Name = "BtSalvar"
        Me.BtSalvar.Size = New System.Drawing.Size(75, 23)
        Me.BtSalvar.TabIndex = 0
        Me.BtSalvar.Text = "Guadar"
        Me.BtSalvar.UseVisualStyleBackColor = True
        '
        'DgDados
        '
        Me.DgDados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DgDados.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Status, Me.SECAO, Me.CHAVE, Me.VALOR})
        Me.DgDados.Location = New System.Drawing.Point(12, 50)
        Me.DgDados.Name = "DgDados"
        Me.DgDados.RowHeadersVisible = False
        Me.DgDados.Size = New System.Drawing.Size(581, 212)
        Me.DgDados.TabIndex = 1
        '
        'Status
        '
        DataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.Status.DefaultCellStyle = DataGridViewCellStyle2
        Me.Status.HeaderText = " "
        Me.Status.Name = "Status"
        Me.Status.Width = 25
        '
        'SECAO
        '
        Me.SECAO.HeaderText = "Sección"
        Me.SECAO.Name = "SECAO"
        Me.SECAO.Width = 150
        '
        'CHAVE
        '
        Me.CHAVE.HeaderText = "Clave"
        Me.CHAVE.Name = "CHAVE"
        Me.CHAVE.Width = 150
        '
        'VALOR
        '
        Me.VALOR.HeaderText = "Valor"
        Me.VALOR.Name = "VALOR"
        Me.VALOR.Width = 200
        '
        'FParametros
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(609, 274)
        Me.Controls.Add(Me.DgDados)
        Me.Controls.Add(Me.BtSalvar)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "FParametros"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Parametros"
        CType(Me.DgDados, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents BtSalvar As Button
    Friend WithEvents DgDados As DataGridView
    Friend WithEvents Status As DataGridViewTextBoxColumn
    Friend WithEvents SECAO As DataGridViewTextBoxColumn
    Friend WithEvents CHAVE As DataGridViewTextBoxColumn
    Friend WithEvents VALOR As DataGridViewTextBoxColumn
End Class
