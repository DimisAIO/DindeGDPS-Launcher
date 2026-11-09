Imports System.ComponentModel

Public Class Setup2
    Dim ok As Boolean
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        My.Settings.DisableUpd = Not CheckBox1.Checked
        My.Settings.Save()
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        If CheckBox2.Checked = True Then
            Dim a = MsgBox("By enabling this, the dgdps:// protocol will be enabled. DimisAIO will not take any responsibility if you enable this option!" + Environment.NewLine + "Do you want to enable it?", vbYesNo + vbExclamation, "DindeGDPS - Custom GDPSes")
            If a = vbNo Then
                CheckBox2.Checked = False
                Return
            End If
            RegisterProtocol()
        Else
            RegisterProtocol()
        End If
    End Sub

    Private Sub Setup2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckBox1.Checked = Not My.Settings.DisableUpd
        CheckBox2.Checked = My.Settings.CustomGDPS
        For Each item As Object In Form4.ComboBox1.Items
            ComboBox2.Items.Add(item)
        Next
    End Sub

    Private Sub Setup2_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If ok = True Then
            If ComboBox1.Text = "Simple" Then
                My.Settings.Simple = True
                My.Settings.Save()
            End If
            Form4.Show()
        Else
            Dim a = MsgBox("Are you sure you do not want to install a GDPS yet?" + nl + "You will be notified again on next launch", vbYesNo + vbExclamation, "DindeGDPS")
            If a = vbNo Then
                e.Cancel = True
            End If
            ' slop
            If ComboBox1.Text = "Simple" Then
                My.Settings.Simple = True
                My.Settings.Save()
            End If
        End If
        My.Settings.Premier = False
        My.Settings.Save()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Form4.ComboBox1.Text = ComboBox2.Text
        My.Settings.Save()
        Form4.PSInst()
        Form4.Button8.Text = "Uninstall"
        ok = True
        Close()
    End Sub
End Class