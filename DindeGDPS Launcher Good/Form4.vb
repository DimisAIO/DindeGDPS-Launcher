Imports System.IO
Imports Ionic.Zip
Imports System.Threading.Tasks
Imports System.Net
Imports System.Configuration
Imports System.Windows.Interop
Public Class Form4
    Private IsUsingConsole As Boolean = False
    Private Async Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If Directory.Exists(Path.Combine(RootFS, "gd19", "Proxy")) Then
            Dim req = MsgBox("Old 1.9 GDPS version detected. Due to incompatibility problems with the launcher, it has to be deleted!" & nl & "Are you sure you want to continue?" & nl & "It is recommended to delete it.", vbYesNo + vbExclamation, "1.9 GDPS")
            If req = vbYes Then
                Directory.Delete(Path.Combine(RootFS, "gd19"), True)
            End If
        End If

        RadioButton1.Checked = True
    End Sub
    Public Async Function PSInst(Optional ConsoleMode As Boolean = False) As Task
        Dim ver = VersionParser.ReadableToReal(ComboBox1.Text)
        If Not VersionParser.official.Contains(ver) Then Return
        IsUsingConsole = ConsoleMode
        For Each hold As Control In Me.Controls
            If TypeOf hold Is Windows.Forms.Button Then hold.Visible = False
        Next
        Label7.Visible = True
        Dim chk = False
        If Directory.Exists(Path.Combine(RootFS, ver)) Then
            Backup(ver)
            chk = True
        End If
        Dim DLS = "https://cdn-dinde.141412.xyz/"
        If Not {"gd22", "gd21", "gd20", "gd19"}.Contains(ver) Then
            DLS = "https://archive-dinde.141412.xyz/"
        End If
        If IsUsingConsole Then
            Form6.TextBox1.Text += "Downloading..." + nl
        Else
            Label8.Text = "Downloading..."
        End If
        ' tous les memes
        Dim verzip = ver + ".zip"
        Await DownloadAsync(DLS + verzip, verzip)
        Directory.CreateDirectory(ver)
        If IsUsingConsole Then
            Form6.TextBox1.Text &= "Installing..." & nl
        End If
        Dim extract = Task.Run(Sub()
                                   Using zip As ZipFile = ZipFile.Read(verzip)
                                       For Each entry As ZipEntry In zip
                                           entry.Extract(ver, ExtractExistingFileAction.OverwriteSilently)
                                           If Not IsUsingConsole Then
                                               Me.Invoke(Sub()
                                                             Me.Label8.Text = "Extracting " + entry.FileName
                                                         End Sub)
                                           End If
                                       Next
                                   End Using
                                   File.Delete(verzip)
                               End Sub)
        Await extract
        If chk Then
            Restore(ver)
        End If
        If Not File.Exists(Path.Combine(RootFS, ver, "logo.png")) Then File.Copy(Path.Combine(RootFS, "default.png"), Path.Combine(RootFS, ver, "logo.png"))

        If IsUsingConsole Then
            Form6.TextBox1.Text += "Finished installing " + ver + nl
        Else
            Label8.Text = "Done!"
        End If
        Button8.Text = "Uninstall"
        If String.IsNullOrEmpty(My.Settings.DfPS) Or My.Settings.DfPS.Equals("none") Then My.Settings.DfPS = ver
        For Each hold As Control In Me.Controls
            If TypeOf hold Is Windows.Forms.Button Then hold.Visible = True
        Next
        Label7.Visible = False
        If My.Settings.Simple Then
            Simple.ComboBox1.Items.Clear()
            Simple.ComboBox1.Items.AddRange(RefreshGDPS())
            Simple.ComboBox1.Text = My.Settings.DfPS
        Else
            Form1.ProbablyRefresh()
        End If
        ' MsgBox("Installed successfully! DindeGDPS will restart...", vbOKOnly + vbInformation, "Instance Manager")
        My.Computer.Audio.Play(My.Resources.gg, AudioPlayMode.Background)
        ' Application.Restart()
        IsUsingConsole = False
    End Function
    Dim LastChange As String = ""
    Private Sub DownloadProgressCallback(sender As Object, e As DownloadProgressChangedEventArgs)
        ' Calculate the progress percentage
        Dim progressPercentage As Integer = CInt((CDbl(e.BytesReceived) / e.TotalBytesToReceive) * 100)

        Dim UpdateString = "Downloading (" & progressPercentage & "%)"
        ' Print or use the progress information as needed
        If Not IsUsingConsole Then
            Label8.Text = UpdateString
        ElseIf UpdateString <> LastChange Then
            Form6.Invoke(Sub()
                             Form6.TextBox1.Text += UpdateString + nl
                         End Sub)
            LastChange = UpdateString
        End If
    End Sub
    Private Async Function DownloadFileAsync(wc As WebClient, uri As Uri, destinationPath As String) As Task
        Await wc.DownloadFileTaskAsync(uri, destinationPath)
    End Function
    Private Async Function DownloadAsync(aurl As String, dest As String) As Threading.Tasks.Task
        ServicePointManager.SecurityProtocol = DirectCast(3072, System.Net.SecurityProtocolType)
        Dim rn = RootFS
        Dim wc As New WebClient
        AddHandler wc.DownloadProgressChanged, AddressOf DownloadProgressCallback

        Try
            Await DownloadFileAsync(wc, New Uri(aurl), Path.Combine(rn, dest))
            Console.WriteLine("Download completed successfully.")
        Catch ex As Exception
            MsgBox($"Error downloading file: {ex.Message}")
        End Try
        ' Await wc.DownloadFileTaskAsync(url, Path.Combine(rn, dest))
        Return
    End Function

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Process.Start(RootFS)
    End Sub

    Private Sub Uninstall()
        Dim Version = VersionParser.ReadableToReal(ComboBox1.Text)
        If Directory.Exists(Path.Combine(RootFS, Version, "Resources")) Then
            Directory.Delete(Path.Combine(RootFS, Version), True)
        End If
        Form1.ProbablyRefresh()
        Button8.Text = "Install"
    End Sub

    Private Sub LinkLabel3_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel3.LinkClicked
        MsgBox("Antivirus solutions can sometimes have false-positives against DindeGDPS." + nl + "To avoid that, add %appdata%\DimisAIO\DindeGDPS\[gd19, gd20, gd21 and gd22] to your antivirus' exceptions!")
    End Sub

    Private Sub DeleteMods()
        Dim Version = ComboBox1.Text
        Dim x = MsgBox("Are you sure you want to delete mods?" + nl + "You will have to reinstall this GD version if you want mods again!", vbYesNo + vbExclamation, "Uninstalling Mods")
        If x <> vbYes Then
            Return
        End If
        If Not Directory.Exists(Path.Combine(RootFS, Version)) Then
            Return
        End If
        ' GD 1.9 => XInput9_1_0.dll & adaf-dll
        ' GD 2.0 => XInput9_1_0.dll & adaf-dll
        ' GD 2.1 => XInput9_1_0.dll & geode
        ' GD 2.2 => XInput1_4.dll & geode
        Dim Dir1 = Path.Combine(RootFS, Version, "adaf-dll")
        Dim File1 = Path.Combine(RootFS, Version, "XInput9_1_0.dll")
        Dim Dir2 = Path.Combine(RootFS, Version, "geode")
        Dim File2 = Path.Combine(RootFS, Version, "XInput1_4.dll")

        Dim ModDir = If(Directory.Exists(Dir1), Dir1, Dir2)
        Dim ModFile = If(File.Exists(File1), File1, File2)

        Try
            Directory.Delete(ModDir, True)
            File.Delete(ModFile)
            MsgBox("Deletion Done!", vbOKOnly + vbInformation, "Mod Deletion")
        Catch ex As Exception
            MsgBox("An error has occured! Is the built-in modloader even present?")
        End Try

        If Version = "gd21" Then
            Dim y = MsgBox("Did you want to download Megahack instead?" + nl + "Click Yes to get a full guide!", vbYesNo + vbQuestion, "Megahack?")
            If y <> vbYes Then
                Return
            Else
                Process.Start("https://cdn-dinde.141412.xyz/mh%20dinde%202.1.pdf")
            End If
        End If
    End Sub
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        ' Delete Mods
        DeleteMods()
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        ' Install 
        If Button8.Text = "Install" Then
            PSInst()
        Else
            Uninstall()
        End If
    End Sub

    Private Async Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        ' Update Check?
        Dim Version = VersionParser.ReadableToReal(ComboBox1.Text)
        If Not VersionParser.official.Contains(Version) Then Return
        If Button7.Text = "Update" Then PSInst(Version)
        If Await GDPSVerCheck(Version, True) = False Then Button7.Text = "Update"
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If Not RadioButton1.Checked Then Return
        ComboBox1.Items.Clear()
        ComboBox1.Text = My.Settings.DfPS
        For Each item As String In VersionParser.official
            ComboBox1.Items.Add(VersionParser.RealToReadable(item))
        Next
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        If Not RadioButton2.Checked Then Return
        ComboBox1.Items.Clear()
        For Each gdps As String In VersionParser.unofficial
            ' MsgBox(gdps) ty fridge for pointing this out
            ComboBox1.Items.Add(gdps)
        Next
    End Sub

    Private Async Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim Version = VersionParser.ReadableToReal(ComboBox1.Text)
        ' todo: cache this
        ' reality: im robtop and ill push the worst code ever
        Dim UpdateCheck As Boolean = Await GDPSVerCheck(Version, True)
        Button8.Text = If(Directory.Exists(Path.Combine(RootFS, Version)), "Uninstall", "Install")
        Button7.Text = If(UpdateCheck, "Check", "Update")
    End Sub
End Class