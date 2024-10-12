Imports System
Imports System.Net.Http
Imports System.Runtime
Imports System.Threading
Imports PdfSharp.Charting
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

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
        AddHandler lp.ReceivedJob, AddressOf JobReady
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

    Public Sub JobReady(sender As Object, job As List(Of String))
        CreatePDF("Test Job", job, "test.pdf")
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

    Public Function CreatePDF(title As String, outList As List(Of String), filename As String) As String
        Dim JobNumber As String = ""
        Dim JobName As String = ""
        Dim doc As New PdfSharp.Pdf.PdfDocument
        doc.Info.Title = title
        Dim page As PdfPage = doc.AddPage()
        page.Orientation = PdfSharp.PageOrientation.Landscape
        ' Get an XGraphics object for drawing
        Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
        ' Create a font

        Dim font As New XFont("Courier", 8, XFontStyleEx.Regular)
        Dim bkgrd As XImage = XImage.FromFile("dummy.jpg")
        gfx.DrawImage(bkgrd, 0, 0)
        ' Set initial coordinates for text
        Dim x As Double = 30
        Dim y As Double = 0
        Dim newHeight As Double = page.Height.Point / 66
        Dim lineHeight As Double = (newHeight - 0.55)
        ' Calculate the maximum number of lines that can fit on a page
        Dim maxLinesPerPage As Integer = CInt((page.Height.Point - y) / lineHeight)

        ' Loop through the list of strings and draw each on a new line
        Dim currentLine As Integer = 0

        For Each line As String In outList

            If (line(0) = vbFormFeed) Then
                ' Add a new page
                page = doc.AddPage()
                page.Orientation = PdfSharp.PageOrientation.Landscape
                gfx = XGraphics.FromPdfPage(page)
                gfx.DrawImage(bkgrd, 0, 0)
                y = 0 ' Reset the y-coordinate
                currentLine = 0
                ' For MPE we'll allow a half inch top margin and let MPE handle
                ' the bottom.
            End If
            line = line.Replace(vbFormFeed, "") 'We've already dealt with the FormFeeds
            line = line.Replace(vbCr, "") 'Get rid of CR
            line = line.Replace(vbLf, "") 'Get rid of LF (we may deal with them later)
            If line = "" Then line = " "  ' Make sure the line contains at least *something*
            ' If the current line exceeds maxLinesPerPage, create a new page
            If currentLine > 0 AndAlso currentLine Mod maxLinesPerPage = 0 Then
                ' Add a new page
                page = doc.AddPage()
                page.Orientation = PdfSharp.PageOrientation.Landscape
                gfx = XGraphics.FromPdfPage(page)
                y = 0 ' Reset the y-coordinate
                currentLine = 0
                For i = 1 To 5
                    gfx.DrawString(" ", font, XBrushes.Black, New XRect(x, y, page.Width.Point, page.Height.Point), XStringFormats.TopLeft)
                    y += lineHeight ' Move to the next line
                    currentLine += 1
                Next
            End If

            ' Draw the current line
            gfx.DrawString(line, font, XBrushes.Black, New XRect(x, y, page.Width.Point, page.Height.Point), XStringFormats.TopLeft)
            y += lineHeight ' Move to the next line
            currentLine += 1
        Next
        Dim outputFile As String = filename
        doc.Save(outputFile)
        doc.Close()
        Return outputFile
    End Function
End Module
