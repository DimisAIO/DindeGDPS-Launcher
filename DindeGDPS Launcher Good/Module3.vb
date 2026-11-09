Imports System.Text.RegularExpressions

Module VersionParser

    Public official As String() = {
       "gd22", "gd21", "gd20", "gd19", "gd2208", "gd2207", "gd2206", "gd22-old", "gd21-og", "gdbeta"
    }
    Public unofficial As New List(Of String)
    Public Function ReadableToReal(readable As String) As String
        Dim parts = readable.Split(" "c)
        If parts.Length < 2 Then Return readable

        Dim versionPart = parts(1)
        Dim isOG = readable.Contains("(OG)")

        Dim attempt = versionPart.Replace(".", "")

        If isOG Then Return "gd21-og"
        If attempt = "2204" Then Return "gd22-old"
        If readable = "DindeGDPS Beta Release" Then Return "gdbeta"

        If official.Contains("gd" & attempt) Then
            Return "gd" & attempt
        End If

        Return readable
    End Function

    Public Function RealToReadable(real As String) As String
        If real = "gdbeta" Then Return "DindeGDPS Beta Release"

        If Not Regex.IsMatch(real, "^gd\d+(-old|-og)?$") Then Return real

        If Not official.Contains(real) Then Return real

        Dim core = real.Substring(2)

        If core.Contains("old") Then
            Return "DindeGDPS 2.204"
        End If

        If core.Contains("og") Then
            Return "DindeGDPS 2.1 (OG)"
        End If

        Return "DindeGDPS " & core.Insert(1, ".")
    End Function

End Module