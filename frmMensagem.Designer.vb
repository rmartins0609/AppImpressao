<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMensagem
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
        Me.tbMensagem = New System.Windows.Forms.RichTextBox()
        Me.SuspendLayout()
        '
        'tbMensagem
        '
        Me.tbMensagem.Font = New System.Drawing.Font("Arial Narrow", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbMensagem.Location = New System.Drawing.Point(0, 2)
        Me.tbMensagem.Name = "tbMensagem"
        Me.tbMensagem.Size = New System.Drawing.Size(449, 185)
        Me.tbMensagem.TabIndex = 0
        Me.tbMensagem.Text = ""
        '
        'frmMensagem
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(453, 192)
        Me.Controls.Add(Me.tbMensagem)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmMensagem"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "frmMensagem"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents tbMensagem As RichTextBox
End Class
