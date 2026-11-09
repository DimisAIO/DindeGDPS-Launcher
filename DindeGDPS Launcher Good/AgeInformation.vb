Public Class AgeInformation
    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        If Label3.Text.Contains("teenager") Then
            Label3.Text = "If you're an adult:"
            LinkLabel1.Text = "I'm a teenager"
        Else
            Label3.Text = "If you're a teenager:"
            LinkLabel1.Text = "I'm an adult"
        End If
        Label4.Visible = Not Label4.Visible
        Label5.Visible = Not Label5.Visible
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Close()
    End Sub
End Class