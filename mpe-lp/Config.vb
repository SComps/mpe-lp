' *** MPE-LP: 2024, ScottJ, westdalefarmer@gmail.com
' *** This code is public domain with no warranties whatsoever.
' *** Configuration class.
Imports System.Net.Http.Headers
Imports System.Xml.Serialization

Public Class Config
    Dim myFile As String = ""
    Dim myHost As String = "127.0.0.1"
    Dim myPort As Integer = 1043
    Dim myOutputDir As String = "pdf"
    Dim myAutoconnect As Boolean = False
    Dim mySilentLog As Boolean = True

    Public Property ConfigFile As String
        Get
            Return myFile
        End Get
        Set(value As String)
            myFile = value
        End Set
    End Property

    Public Property Hostname As String
        Get
            Return myHost
        End Get
        Set(value As String)
            myHost = value
        End Set
    End Property

    Public Property Port As String
        Get
            Return myPort
        End Get
        Set(value As String)
            myPort = value
        End Set
    End Property
    Public Property OutputDirectory As String
        Get
            Return myOutputDir
        End Get
        Set(value As String)
            myOutputDir = value
        End Set
    End Property

    Public Property AutoConnect As Boolean
        Get
            Return myAutoconnect
        End Get
        Set(value As Boolean)
            myAutoconnect = value
        End Set
    End Property
    Public Property SilentLog As Boolean
        Get
            Return mySilentLog
        End Get
        Set(value As Boolean)
            mySilentLog = value
        End Set
    End Property

    Public Sub New()
        ' creates the configuration object without defining the configuration
        ' file.  Be sure to set the file *BEFORE* attempting to use the save
        ' subroutine.
    End Sub

    Public Sub New(cfgFile As String)
        myFile = cfgFile
    End Sub

    Public Function Exists() As Boolean
        If IO.File.Exists(myFile) Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Sub Load()
        Dim serializer As New XmlSerializer(GetType(Config), New XmlRootAttribute("Configuration"))
        Dim deserialized As Config = Nothing
        Using file = System.IO.File.OpenRead(myFile)
            deserialized = DirectCast(serializer.Deserialize(file), Config)
        End Using
        Me.Hostname = deserialized.Hostname
        Me.Port = deserialized.Port
        Me.AutoConnect = deserialized.AutoConnect
        Me.SilentLog = deserialized.SilentLog
        Me.myFile = deserialized.myFile ' Shouldn't be necessary, but why not?
        deserialized = Nothing
    End Sub
    Public Sub Save()
        Dim serializer As New XmlSerializer(GetType(Config), New XmlRootAttribute("Configuration"))
        Using file As System.IO.FileStream = System.IO.File.Open(myFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write)
            serializer.Serialize(file, Me)
        End Using
    End Sub
End Class
