Imports System.IO
Imports System.ComponentModel
Public Class Patches
    Private Sub Patches_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If File.Exists(Path.Combine(RootFS, "patchnote.txt")) Then
            File.Delete(Path.Combine(RootFS, "patchnote.txt"))
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Close()
    End Sub
End Class