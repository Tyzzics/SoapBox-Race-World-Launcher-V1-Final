Imports System.IO
Imports System.IO.Compression
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Xml.Linq
Imports Microsoft.VisualBasic
Public Class Form1
    Dim token As Integer
    Dim arguments As String
    Dim userid As Integer
    Dim gamepath As String
    Dim drag As Boolean
    Dim mousex As Integer
    Dim mousey As Integer
    Dim path As String = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    Dim filepath As String = path & "\SoapBoxLauncher\email.txt"
    Dim ipremember As String = path & "\SoapBoxLauncher\ip.txt"
    Dim filepathgame As String = path & "\SoapBoxLauncher\gamepath.txt"
    Dim normalpassword As String
    Dim criptedpassword As String
    Dim fullxml As String
    Dim UserId_value As Integer
    Dim Token_value As Integer
    'making the window movible from anywhere
    Private Sub Form1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        drag = True
        mousex = Windows.Forms.Cursor.Position.X - Me.Left
        mousey = Windows.Forms.Cursor.Position.Y - Me.Top
    End Sub
    Private Sub Form1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If drag Then
            Me.Top = Windows.Forms.Cursor.Position.Y - mousey
            Me.Left = Windows.Forms.Cursor.Position.X - mousex
        End If
    End Sub
    Private Sub Form1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        drag = False
    End Sub
    'magic beggins
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'creates the soapbox folder
        My.Computer.FileSystem.CreateDirectory(path & "\SoapBoxLauncher")
        'email remember
        If Not System.IO.File.Exists(filepath) Then
            System.IO.File.Create(filepath).Dispose()
        End If
        emailtxt.Text = System.IO.File.ReadAllText(filepath)
        'gamepath remember
        If Not System.IO.File.Exists(filepathgame) Then
            System.IO.File.Create(filepathgame).Dispose()
        End If
        gamepath = System.IO.File.ReadAllText(filepathgame)
        'ipv4 remember
        If Not System.IO.File.Exists(ipremember) Then
            System.IO.File.Create(ipremember).Dispose()
        End If
        ipv4server.Text = System.IO.File.ReadAllText(ipremember)
    End Sub
    'X button
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        End
    End Sub
    'pass encription
    Public Function passwordEncryptSHA(ByVal normalpassword As String) As String
        Dim sha As New SHA1CryptoServiceProvider
        Dim bytesToHash() As Byte
        bytesToHash = System.Text.Encoding.ASCII.GetBytes(normalpassword)
        bytesToHash = sha.ComputeHash(bytesToHash)
        Dim encPassword As String = ""
        For Each b As Byte In bytesToHash
            encPassword += b.ToString("x2")
        Next
        Return encPassword
    End Function
    Public Function GetUrl(ByVal PostData As String) As String
        Try
            Dim Http2 As HttpWebRequest = WebRequest.Create("Http://" & ipv4server.Text & "/soapbox/Engine.svc/user/authenticateUser?email=" & emailtxt.Text & "&password=" & criptedpassword)
            Http2.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate")
            If Not String.IsNullOrEmpty(PostData) Then
                Http2.Method = "POST"
                Dim lbPostBuffer As Byte() = Encoding.Default.GetBytes(PostData)
                Http2.ContentLength = lbPostBuffer.Length
                Using PostStream As Stream = Http2.GetRequestStream()
                    PostStream.Write(lbPostBuffer, 0, lbPostBuffer.Length)
                End Using
            End If
            Using WebResponse As HttpWebResponse = Http2.GetResponse()
                Dim responseStream As Stream = WebResponse.GetResponseStream()
                If (WebResponse.ContentEncoding.ToLower().Contains("gzip")) Then
                    responseStream = New GZipStream(responseStream, CompressionMode.Decompress)
                ElseIf (WebResponse.ContentEncoding.ToLower().Contains("deflate")) Then
                    responseStream = New DeflateStream(responseStream, CompressionMode.Decompress)
                End If
                Dim reader As StreamReader = New StreamReader(responseStream, Encoding.Default)
                Dim html As String = reader.ReadToEnd()
                responseStream.Close()
                Return html
            End Using
        Catch
        End Try
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles btn_login.Click
        txt_logingood.Text = ""
        If gamepath = "" Then
            txt_logingood.Text = "Select the game folder first."
        Else
            'this checks if the textbox has an "@" and a "." like a normal Email does.
            If emailtxt.Text.Contains("@") And emailtxt.Text.Contains(".") And passtxt.TextLength <> 0 And ipv4server.TextLength <> 0 Then
                If CheckBox1.Checked = True Then
                    'CREATES A FILE THAT WILL SAVE THE EMAIL
                    If Not System.IO.File.Exists(filepath) Then
                        System.IO.File.Create(filepath).Dispose()
                        System.IO.File.WriteAllText(filepath, emailtxt.Text)
                    Else

                        System.IO.File.Delete(filepath)
                        System.IO.File.Create(filepath).Dispose()
                        System.IO.File.WriteAllText(filepath, emailtxt.Text)
                    End If
                End If
                normalpassword = passtxt.Text
                Call passwordEncryptSHA(normalpassword)
                criptedpassword = passwordEncryptSHA(normalpassword)
                fullxml = GetUrl(String.Empty)
                If Not fullxml = "" Then
                    Dim element As XDocument = XDocument.Parse(fullxml)
                    Dim UserId = From d In element.Descendants("LoginData")
                                 Select d.Element("UserId").Value
                    Dim UserId_value As Integer = UserId.First()
                    Dim Token = From d In element.Descendants("LoginData")
                                Select d.Element("LoginToken").Value
                    Dim Token_value As String = Token.First()
                    arguments = " US " & "http://" & ipv4server.Text & "/soapbox/Engine.svc " & Token_value & " " & UserId_value
                    Try
                        Process.Start(gamepath & "\nfsw.exe", arguments)
                        txt_logingood.Text = "Loged in successfully, Launching the game."
                        If Not System.IO.File.Exists(ipremember) Then
                            System.IO.File.Create(ipremember).Dispose()
                            System.IO.File.WriteAllText(ipremember, ipv4server.Text)
                        Else
                            System.IO.File.Delete(ipremember)
                            System.IO.File.Create(ipremember).Dispose()
                            System.IO.File.WriteAllText(ipremember, ipv4server.Text)
                        End If
                        GoTo godgamepath
                    Catch ex As System.ComponentModel.Win32Exception
                        GoTo badgamepath
                    End Try
                Else
                    txt_logingood.Text = "Ip, email or password its not correctly whriten."
                    GoTo godgamepath
                End If
            Else
                txt_logingood.Text = "Email or password its not correctly whriten."
                GoTo godgamepath
            End If
badgamepath:
            txt_logingood.Text = "Game Path is not correct, Ex: C:\Program Files (x86)\NFS World" & ControlChars.NewLine & "The folder should contain the nfsw.exe"
        End If
godgamepath:
    End Sub
    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles Button2.Click
        txt_logingood.Text = ""
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            gamepath = FolderBrowserDialog1.SelectedPath
            If Not System.IO.File.Exists(filepathgame) Then
                System.IO.File.Create(filepathgame).Dispose()
                System.IO.File.WriteAllText(filepathgame, gamepath)
            Else
                System.IO.File.Delete(filepathgame)
                System.IO.File.Create(filepathgame).Dispose()
                System.IO.File.WriteAllText(filepathgame, gamepath)
            End If
        End If
    End Sub
End Class