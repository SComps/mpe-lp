Imports System
Imports System.Net.Http
Imports System.Threading

Module Program
    ' *** MPE-LP: 2024, ScottJ, westdalefarmer@gmail.com
    ' *** This code is public domain with no warranties whatsoever.
    ' *** Entry module.

    Public cfgFile As String = "mpe-lp.xml"
    Public cfg As Config        ' Does NOT create the class object yet. Placeholder
    Public WithEvents lp As New LPDev("init", 0, False)
    Sub Main(args As String())
        Dim newCfg As String = ""
        Console.WriteLine("MPE-LP: 2024 ScottJ and others, westdalefarmer@gmail.com")
        Console.WriteLine("This code is given to the public domain.  There are no promises")
        Console.WriteLine("and no warranties for anything whatsoever.  The only thing guaranteed")
        Console.WriteLine("is that it will consume space on your storage device." & vbCrLf)

        If args.Count <> 0 Then
            newCfg = args(0)
            Console.WriteLine("*** Using " & newCfg & " for configuration data.")
            Console.WriteLine()
            cfgFile = newCfg
        End If
        cfg = New Config(cfgFile)       'Creates the Config class object
        If Not cfg.Exists Then
            Console.WriteLine("[CONFIG] Creating new configuration file " & cfgFile)
            Console.WriteLine("         Edit this file and re-run this program.")
            cfg.Save()
            End
        Else
            cfg.Load()
            Console.WriteLine("[REMOTE] " & cfg.Hostname & ":" & cfg.Port & " writing to " & cfg.OutputDirectory)
            If cfg.AutoConnect Then
                Console.WriteLine("         *** Automatic connection")
            Else
                Console.WriteLine("[NOTE] Automatic connection is False, but is being ignored because this application is in console mode.")
            End If
            If cfg.SilentLog Then Console.WriteLine("         *** No log output after startup")
        End If

        lp = New LPDev(cfg.Hostname, cfg.Port, cfg.AutoConnect)
        AddHandler lp.ConnectionError, AddressOf BadConnect
        AddHandler lp.DataIn, AddressOf DataIn
        Console.WriteLine("----- Attempting to connect to the remote host.")
        lp.Connect()
        Console.WriteLine("Connected.")
        Do
            Thread.Sleep(1)
            Dim ch As Char = Console.ReadKey.KeyChar
            If ch = Chr(27) Then
                lp = Nothing
                cfg = Nothing
                End
            End If
        Loop
    End Sub

    Public Sub DataIn(txt As String)
        txt = txt.Replace(vbCr, "<CR>")
        txt = txt.Replace(vbLf, "<LF>")
        txt = txt.Replace(vbFormFeed, "<FF>")
        Console.WriteLine("[RECV] " & txt)
    End Sub
    Public Sub BadConnect(ex As Exception)
        Console.WriteLine("[ERROR] " & ex.Message)
        End
    End Sub
End Module
