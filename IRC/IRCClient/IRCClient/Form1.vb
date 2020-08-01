Imports System
Imports System.IO
Imports System.Net.Sockets

Public Class Form1

    Dim Client As TcpClient
    Dim Rx As StreamReader
    Dim Tx As StreamWriter

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Client = New TcpClient("localhost", 4305)
            If Client.GetStream.CanRead = True Then
                Rx = New StreamReader(Client.GetStream)
                Tx = New StreamWriter(Client.GetStream)
                Threading.ThreadPool.QueueUserWorkItem(AddressOf Connected)
            End If
        Catch ex As Exception
            RichTextBox1.Text += "Failed to connect, E: " + ex.Message + vbNewLine
        End Try
    End Sub

    Function Connected()
        If Rx.BaseStream.CanRead = True Then
            Try
                While Rx.BaseStream.CanRead = True
                    Dim RawData As String = Rx.ReadLine
                    If RawData.ToUpper = "/MSG" Then
                        Threading.ThreadPool.QueueUserWorkItem(AddressOf MSG1, "Hello World.")
                    Else
                        Dim strHostName As String
                        Dim strIPAddress As String
                        strHostName = System.Net.Dns.GetHostName()
                        strIPAddress = System.Net.Dns.GetHostByName(strHostName).AddressList(0).ToString()
                        RichTextBox1.Text += strIPAddress + " >>> " + RawData + vbNewLine
                    End If
                End While
            Catch ex As Exception
                Client.Close()
            End Try
        End If
        Return True
    End Function

    Function MSG1(ByVal data As String)
        MsgBox(data)
        Return True
    End Function

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If TextBox1.Text.Length > 0 Then
                SendToServer(TextBox1.Text)
                TextBox1.Clear()
            End If
        End If
    End Sub

    Function SendToServer(ByVal data As String)
        Try
            Tx.WriteLine(data)
            Tx.Flush()
        Catch ex As Exception
            '
        End Try
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
    End Sub

End Class
