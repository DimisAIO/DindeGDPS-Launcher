Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.VisualBasic.Logging
Imports Microsoft.Web.WebView2.Core
Imports Newtonsoft.Json

Public Class Loader

    Dim PremierCheck = My.Settings.Premier

    Private Sub NotifyIcon1_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles NotifyIcon1.MouseDown
        If e.Button = MouseButtons.Left Then
            If My.Settings.Simple Then
                Simple.Visible = True
                If Simple.WindowState = FormWindowState.Minimized Then
                    Simple.WindowState = FormWindowState.Normal
                End If
            Else
                Form1.Visible = True
                If Form1.WindowState = FormWindowState.Minimized Then
                    Form1.WindowState = FormWindowState.Normal
                End If
            End If
        End If
    End Sub

    Private Sub NotifyIcon1_BalloonTipClicked(sender As Object, e As EventArgs) Handles NotifyIcon1.BalloonTipClicked
        BringToFront() ' Else, it goes back and we don't want that
        If NotifyState = "IM" Then
            Form4.Show()
        End If
    End Sub
    Public SayHi As Boolean = False
    Private Sub MenuItem_Click(sender As Object, e As EventArgs)
        ' Get the clicked ToolStripMenuItem
        Dim clickedItem As ToolStripMenuItem = CType(sender, ToolStripMenuItem)

        ' Get the name of the clicked item
        Dim itemName As String = clickedItem.Text

        If itemName = "Exit" Then
            Form1.Close()
            Simple.Close()
            Close()
            Return
        Else
            itemName = itemName.Replace("Play ", "")
        End If

        Play(itemName, ModifierKeys)
    End Sub

    Public Async Function Hello(Optional RestartBrowser As Boolean = False) As Threading.Tasks.Task
        If String.IsNullOrEmpty(My.Settings.token) Then Return

        Using client As New Net.Http.HttpClient
            Dim reqparm As New Dictionary(Of String, String) From {{"token", My.Settings.token}}
            Dim content1 = New Net.Http.FormUrlEncodedContent(reqparm)
            Dim content2 = New Net.Http.FormUrlEncodedContent(reqparm)
            Dim content3 = New Net.Http.FormUrlEncodedContent(reqparm)

            Dim s = "https://gdps.dimisaio.be/api/checkUsername.php"
            Dim s2 = "https://gdps.dimisaio.be/api/checkRates.php"
            Dim s3 = "https://gdps.dimisaio.be/api/getSettings.php"

            Try
                Dim t1 = client.PostAsync(s, content1)
                Dim t2 = client.PostAsync(s2, content2)
                Dim t3 = client.PostAsync(s3, content3)

                Await Task.WhenAll(t1, t2, t3)

                Dim res = Await t1.Result.Content.ReadAsStringAsync()
                Dim res2 = Await t2.Result.Content.ReadAsStringAsync()
                Dim res3 = Await t3.Result.Content.ReadAsStringAsync()

                If res.ToLower() <> My.Settings.Player.ToLower() Then
                    My.Settings.Player = res
                    Form1.WebView21.CoreWebView2.Settings.UserAgent = $"UneTesla-{Origine}-{res}"
                    If RestartBrowser Then Form1.WebView21.CoreWebView2.Reload()
                End If

                SayHi = (res2 = "1")

                If Not String.IsNullOrEmpty(res3) AndAlso My.Settings.Sync Then
                    Dim settings As Settings = JsonConvert.DeserializeObject(Of Settings)(res3)
                    My.Settings.Color = settings.color
                    My.Settings.Language = settings.lang
                    My.Settings.CloseLauncher = Convert.ToBoolean(settings.closeOP)
                    My.Settings.ComboPos = settings.barPos
                    ApplyColor(settings.color)
                    Form1.Form1_Resize(Nothing, Nothing)
                    My.Settings.Save()
                End If
            Catch ex As Exception
                Console.WriteLine("ALERTE: LA TESLA EST HORS LIGNE")
            End Try
        End Using
    End Function

    Private Sub Loader_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If File.Exists(Path.Combine(RootFS, "setup.exe")) Then
            File.Delete(Path.Combine(RootFS, "setup.exe"))
        End If

        If Environment.GetCommandLineArgs.Length > 1 Then
            Hide()
            If Form5.Fork(Environment.GetCommandLineArgs()(1)) Then
                Form5.ShowDialog()
            End If
            If IsOnlyInstance() Then
                Process.Start(Application.ExecutablePath)
                Environment.Exit(0)
            Else
                If Form5.Success Then
                    Process.GetProcessById(Path.Combine(RootFS, ".lock")).Close()
                    File.Delete(Path.Combine(RootFS, ".lock"))
                    Application.Restart()
                End If
            End If
        End If

        If IsOnlyInstance() = False Then
            Hide()
            Dim s = MsgBox("DindeGDPS is already running. Stop current process and start a new one?", vbYesNo + vbQuestion, "DindeGDPS is already running")
            If s = vbNo Then
                Application.Exit() ' bye :3
                Return
            Else
                Process.GetProcessById(File.ReadAllText(Path.Combine(RootFS, ".lock"))).CloseMainWindow()
            End If
        End If
        File.WriteAllText(Path.Combine(RootFS, ".lock"), Process.GetCurrentProcess.Id)
    End Sub
    Private Async Sub Loader_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If My.Settings.persistorperish <> False Then
            My.Settings.Upgrade()
            My.Settings.persistorperish = False
            My.Settings.Save()
            ' Thanks to the huge rework, this shouldn't be needed to be changed
        End If

        Dim MyFile = Path.Combine(RootFS, "patchnote.txt")
        If (File.Exists(MyFile)) Then
            Dim Patch = File.ReadAllText(MyFile)
            Patches.Label1.Text = "DindeGDPS Updated!" + nl + "Changes for " + Application.ProductVersion
            Patches.TextBox1.Text = Patch
            Patches.ShowDialog()
        End If

        Dim player = My.Settings.Player

        If My.Settings.Premier = True Then
            If player <> "" Then
                Dim qu = MsgBox($"Continue as {player}?", vbYesNo + vbQuestion, "DindeGDPS")
                If qu = vbNo Then
                    OpenWait(Setup1)
                End If
            Else
                OpenWait(Setup1)
            End If
            AgeInformation.ShowDialog()
            OpenWait(Setup2)
            PremierCheck = True
        End If

        ' another slop to avoid 1st time bug
        If My.Settings.Player = "Guest" Then
            player = "Guest"
        End If


        If (String.IsNullOrEmpty(My.Settings.token) AndAlso My.Settings.Player <> "Guest") Then
            MsgBox("We are now using a new, more sophisticated login system on the launcher with many improvements!" + nl + "Please, log in again!", vbOKOnly + vbInformation, "DindeGDPS Auth")
            OpenWait(Setup1)
        End If

        If My.Settings.Simple = False Then
            Dim webviewcheck = True
            Try
                Dim versionString As String = CoreWebView2Environment.GetAvailableBrowserVersionString()
                If String.IsNullOrEmpty(versionString) Then
                    webviewcheck = False
                End If
            Catch ex As FileNotFoundException
                webviewcheck = False
            Catch ex As Exception
                webviewcheck = False
            End Try
            If Not webviewcheck Then
                Dim errbox = MsgBox($"DindeGDPS requires the Webview 2 Runtime (or an erorr occured!). You can do the following:{nl}Yes: Close DindeGDPS and install WebView2{nl}No: Close DindeGDPS{nl}Cancel: Open simple mode, removing that requirement.", vbYesNoCancel + vbCritical, "WebView2 Error")
                Select Case errbox
                    Case vbYes
                        Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703")
                        Environment.Exit(0)
                    Case vbNo
                        Environment.Exit(0)
                    Case vbCancel
                        My.Settings.Simple = True
                        My.Settings.Save()
                        Application.Restart()
                End Select
                Return
            Else
                Await Hello()

                Dim lang As String = SafeGuardLang(My.Settings.Language)

                Form1.WebView21.CreationProperties =
    New Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties With {
        .Language = lang
    }

                Form1.WebView22.CreationProperties =
    New Microsoft.Web.WebView2.WinForms.CoreWebView2CreationProperties With {
        .Language = lang
    }

                Await Form1.WebView21.EnsureCoreWebView2Async()
                Await Form1.WebView22.EnsureCoreWebView2Async()

                AddHandler Form1.WebView21.CoreWebView2.WebResourceRequested, AddressOf WebResourceRequested

                Form1.WebView21.CoreWebView2.AddWebResourceRequestedFilter(
    "https://dindegdps.localhost/*",
    CoreWebView2WebResourceContext.All)

                Form1.WebView21.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "dindegdps.141412.xyz",
                    Path.Combine(RootFS, "web"),
                    CoreWebView2HostResourceAccessKind.Allow)

                Form1.WebView21.CoreWebView2.Settings.UserAgent = "UneTesla-" + Origine + "-" + player

                Form1.WebView21.CoreWebView2.Navigate("https://dindegdps.141412.xyz/index.html")
                Form1.WebView22.CoreWebView2.Navigate("file:///" & RootFS & "/web/sidebar.html")

            End If
        End If

        ApplyColor(My.Settings.Color)
        CheckForUpdates()

        ' Get the directory names from RefreshGDPS
        Dim directoryArray As String() = RefreshGDPS()

        ' Add each directory name as a ToolStripMenuItem to the ContextMenuStrip
        For Each directoryName As String In directoryArray
            Dim Zemel = VersionParser.RealToReadable(directoryName)
            Dim IsOfficial As Boolean = VersionParser.official.Contains(directoryName)
            If Not IsOfficial Then VersionParser.unofficial.Add(directoryName)
            Dim menuItem As New ToolStripMenuItem("Play " + Zemel)
            AddHandler menuItem.Click, AddressOf MenuItem_Click
            ContextMenuStrip1.Items.Add(menuItem)
            If My.Settings.Simple Then
                Simple.ComboBox1.Items.Add(Zemel)
            End If
        Next

        Dim exitItem As New ToolStripMenuItem("Exit")
        AddHandler exitItem.Click, AddressOf MenuItem_Click
        ContextMenuStrip1.Items.Add(exitItem)
        Hide()

        If My.Settings.Simple = True Then
            ' Already done few lines above Simple.ComboBox1.Items.AddRange(directoryArray)
            Simple.ComboBox1.Text = My.Settings.DfPS
            Simple.Show()
        Else
            Task.Run(Sub() MiniRefreshGDPS())

            Form4.ComboBox1.Text = My.Settings.DfPS

            Form1.Form1_Resize(sender, e)

            If My.Settings.XPoint <> 0 Then
                Form1.Width = My.Settings.XPoint
            End If

            If My.Settings.YPoint <> 0 Then
                Form1.Height = My.Settings.YPoint
            End If

            ' Greeting has been implemented on index.html instead
            Form1.Show()
        End If

        If PremierCheck = False Then
            dogcheck() ' hall of dog
        End If

    End Sub

    Private Function StreamString(MyString As String) As MemoryStream
        Return New MemoryStream(Encoding.UTF8.GetBytes(MyString))
    End Function
    Private Sub WebResourceRequested(
    sender As Object,
    e As CoreWebView2WebResourceRequestedEventArgs)
        Debug.WriteLine("CAUGHT: " & e.Request.Uri)

        If e.Request.Uri.StartsWith("https://dindegdps.localhost/get/") Then
            Dim headers =
                "Content-Type: text/plain" & vbCrLf &
                "Access-Control-Allow-Origin: https://dindegdps.141412.xyz" & vbCrLf &
                "Access-Control-Allow-Methods: GET, POST, OPTIONS" & vbCrLf &
                "Access-Control-Allow-Headers: Content-Type"

            Dim setting = e.Request.Uri.Substring(32)

            Dim stream As MemoryStream

            Dim serializer As New JsonSerializer()

            Dim sw As New StringWriter()

            Select Case True
                Case setting.Equals("gdps")
                    Dim items As New List(Of String)
                    For Each GDPS In RefreshGDPS()
                        items.Add(RealToReadable(GDPS))
                    Next

                    Using writer As New JsonTextWriter(sw)
                        writer.Formatting = Formatting.Indented

                        serializer.Serialize(writer, items)
                    End Using

                    stream = StreamString(sw.ToString())
                Case setting.StartsWith("img")
                    Dim ipath = ReadableToReal(WebUtility.UrlDecode(setting.Substring(4)))
                    headers =
                         "Content-Type: image/png" & vbCrLf &
                         "Access-Control-Allow-Origin: https://dindegdps.141412.xyz" & vbCrLf &
                         "Access-Control-Allow-Methods: GET, POST, OPTIONS" & vbCrLf &
                         "Access-Control-Allow-Headers: Content-Type"
                    stream = New MemoryStream(File.ReadAllBytes(Path.Combine(RootFS, ipath, "logo.png")))
                Case setting.Equals("color")
                    stream = StreamString(My.Settings.Color)
                Case setting.Equals("settings")
                    Dim settings As New Dictionary(Of String, Object) From {
                        {"cupdates", Not My.Settings.DisableUpd},
                        {"clauncher", My.Settings.CloseLauncher},
                        {"uchannel", My.Settings.Channel},
                        {"lang", Origine},
                        {"sync", My.Settings.Sync},
                        {"color", My.Settings.Color},
                        {"barPos", My.Settings.ComboPos}
                    }

                    Dim json As String = JsonConvert.SerializeObject(settings)
                    stream = StreamString(json)
                Case Else
                    stream = StreamString(setting)
            End Select

            e.Response =
                Form1.WebView21.CoreWebView2.Environment.CreateWebResourceResponse(
                    stream,
                    200,
                    "OK",
                    headers)

        End If

    End Sub

End Class