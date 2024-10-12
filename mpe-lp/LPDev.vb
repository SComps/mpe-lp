Imports System.Net.Sockets
Imports System.Text
Imports System.Timers

Public Class LPDev

    ' Class to connect to the remote host and receive print jobs

    Public Event ReceivedJob(sender As Object, job As List(Of String))  ' Raised when job is completed.
    Public Event ConnectionError(errorArg As Exception) ' Raised when connection is NOT established.
    ' Note, for some reason we're not told if the connection disconnects, so that's
    ' just wierd, so this event is raised ONLY if it can't initially connect.
    Public Event DataIn(lineIn As String) ' Raised when a line of data is received, in case the main
    ' program would like to handle it.

    Private BannerLinesIn As Integer = 0 ' SIMH Sends junk and a banner.  Total 4 lines.
    Private OkToReceive As Boolean = False

    Private myHost As String = ""
    Private myport As Integer = 1043
    Private myauto As Boolean = False

    Private client As New TcpClient
    Private WithEvents clientTimer As New Timer(2000)

    Private CurrentJobFlag As String = ""
    Private CurrentFlagLines As Integer = 0
    Public Sub New(host As String, port As Integer, auto As Boolean)
        If host = "init" Then Exit Sub
        myHost = host
        myport = port
        myauto = auto

    End Sub

    Public Sub Connect()
        Try
            client.Connect(myHost, myport)
            clientTimer.Enabled = True
        Catch ex As Exception
            RaiseEvent ConnectionError(ex)
        End Try
    End Sub

    Private Function GetOutput() As List(Of String)
        Dim thisOutput As New List(Of String)
        Dim myStream As NetworkStream = Nothing
        If client.Connected Then
            myStream = client.GetStream()
            Do
                Dim newLine As String = ReadLine(myStream)
                If (newLine Is Nothing) Then
                    Exit Do
                End If
                thisOutput.Add(newLine)
            Loop
        End If
        Return thisOutput
    End Function

    Private Function ReadLine(thisStream As NetworkStream) As String
        'Reads one line from the network stream.  What's the point of taking
        'ReadLine out of the Network Stream?  Somebody at Microsoft thought
        'that was a good idea.  Probably the same guy that came up with CoPilot.
        'because nobody ever reads whole lines of text from a network stream.  
        Dim MyString As String = ""
        If thisStream.DataAvailable Then
            While thisStream.DataAvailable
                Dim thisByte(1) As Byte
                thisStream.Read(thisByte, 0, 1)
                Dim thisChar As String = Encoding.ASCII.GetString(thisByte, 0, 1)
                MyString = MyString & thisChar
                If thisChar = vbLf Then
                    RaiseEvent DataIn(MyString)
                    If Not OkToReceive Then
                        BannerLinesIn = BannerLinesIn + 1
                        If BannerLinesIn >= 4 Then
                            OkToReceive = True
                            RaiseEvent DataIn("***** RECEIVING JOBS *****")
                        End If
                    End If
                    Return MyString
                End If
            End While
        Else
            Return Nothing
        End If
        RaiseEvent DataIn(MyString)
        Return MyString
    End Function

    Public Sub Timer_Tick() Handles clientTimer.Elapsed
        ' Once connected to the remote host, this time is enabled and checked based on interval.
        ' If a job exists, it reads it into the object, and raises the JobReceived Event.  
        ' then it re-enables the Timer for the next pass.  The interval can be extended
        ' if the checks happen to rapidly.
        clientTimer.Enabled = False
        Dim myJob As List(Of String) = GetOutput()
        If myJob.Count > 0 Then
            ' A job has been printed.  It might be ours, it might not.
            RaiseEvent ReceivedJob(Me, myJob)
        End If
        clientTimer.Enabled = True

    End Sub

End Class
