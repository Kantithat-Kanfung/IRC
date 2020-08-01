Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets

Public Class Form1

    Dim Server As TcpListener
    Dim Clients As New List(Of TcpClient)
    Dim ServerStatus As Boolean = False
    Dim ServerTrying As Boolean = False

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        StartServer()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        StopServer()
    End Sub

    Function StartServer()
        If ServerStatus = False Then
            ServerTrying = True
            Try
                Server = New TcpListener(IPAddress.Any, 4305)
                Server.Start()
                ServerStatus = True
                Threading.ThreadPool.QueueUserWorkItem(AddressOf Handler_Client)
            Catch ex As Exception
                ServerStatus = False
            End Try
            ServerTrying = False
        End If
        Return True
    End Function

    Function StopServer()
        If ServerStatus = True Then
            ServerTrying = True
            Try
                For Each Client As TcpClient In Clients
                    Client.Close()
                Next
                Server.Stop()
                ServerStatus = False
            Catch ex As Exception
                StopServer()
            End Try
            ServerTrying = False
        End If
        Return True
    End Function

    Function Handler_Client(ByVal stat As Object)
        Dim TempClient As TcpClient
        Try
            Using Client As TcpClient = Server.AcceptTcpClient
                If ServerTrying = False Then
                    Threading.ThreadPool.QueueUserWorkItem(AddressOf Handler_Client)
                End If
                Clients.Add(Client)
                TempClient = Client
                Dim Tx As New StreamWriter(Client.GetStream)
                Dim Rx As New StreamReader(Client.GetStream)
                If Rx.BaseStream.CanRead = True Then
                    While Rx.BaseStream.CanRead = True
                        Dim RawData As String = Rx.ReadLine
                        RichTextBox1.Text += Client.Client.RemoteEndPoint.ToString + " " + RawData + " " + vbNewLine
                    End While
                End If
                If Rx.BaseStream.CanRead = False Then
                    Client.Close()
                    Clients.Remove(Client)
                End If
            End Using
        Catch ex As Exception
            If TempClient.GetStream.CanRead = False Then
                TempClient.Close()
                Clients.Remove(TempClient)
            End If
        End Try
        Return True
    End Function

    Function SendToClient(ByVal data As String)
        If ServerStatus = True Then
            If Clients.Count > 0 Then
                Try
                    For Each Client As TcpClient In Clients
                        Dim Tx As New StreamWriter(Client.GetStream)
                        Tx.WriteLine(data)
                        Tx.Flush()
                    Next
                Catch ex As Exception
                    SendToClient(data)
                End Try
            End If
        End If
        Return True
    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Label1.Text = Clients.Count.ToString()
    End Sub

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If TextBox1.Text.Length > 0 Then
                Threading.ThreadPool.QueueUserWorkItem(AddressOf SendToClient, TextBox1.Text)
            End If
        End If
    End Sub

End Class
