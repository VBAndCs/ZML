Public Class ZmlPages

    Public Shared Sub Compile(Optional encoding As Text.Encoding = Nothing)
        Dim path = IO.Directory.GetCurrentDirectory()
        CompileDir(path, encoding)
    End Sub

    Shared Sub CompileDir(path As String, encoding As Text.Encoding)
        For Each d In IO.Directory.GetDirectories(path)
            For Each f In IO.Directory.GetFiles(d, "*.zml")
                CompilePage(f, encoding)
            Next

            CompileDir(d, encoding)
        Next
    End Sub

    Private Shared Sub CompilePage(zmlFile As String, Optional encoding As Text.Encoding = Nothing)
        Dim cshtmlPath = IO.Path.GetDirectoryName(zmlFile)
        Dim cshtmlName = IO.Path.GetFileNameWithoutExtension(zmlFile)
        Dim cshtmlFile = IO.Path.Combine(cshtmlPath, cshtmlName & ".cshtml")

        If IO.File.Exists(cshtmlFile) Then
            Dim cshtmlLastModified = IO.File.GetLastWriteTime(cshtmlFile)
            Dim zmlLastModified = IO.File.GetLastWriteTime(zmlFile)
            If cshtmlLastModified < zmlLastModified Then SaveCshtml(zmlFile, cshtmlFile, encoding)
        Else
            SaveCshtml(zmlFile, cshtmlFile, encoding)
        End If

    End Sub


    Shared Sub SaveCshtml(zmlFile As String, cshtmlFile As String, encoding As Text.Encoding)

        Dim zml = ""
        If encoding Is Nothing Then
            zml = IO.File.OpenText(zmlFile).ReadToEnd()
            IO.File.WriteAllText(cshtmlFile, ParseZml(zml))
        Else
            zml = New IO.StreamReader(zmlFile, encoding).ReadToEnd()
            Dim sw As New IO.StreamWriter(cshtmlFile, False, encoding)
            sw.Write(zml)
        End If

    End Sub

    Shared Function ReadCshtml(cshtmllFile As String, encoding As Text.Encoding) As String

        Dim cshtml = ""
        If encoding Is Nothing Then
            cshtml = IO.File.OpenText(cshtmllFile).ReadToEnd()
        Else
            cshtml = New IO.StreamReader(cshtmllFile, encoding).ReadToEnd()
        End If

        Return cshtml
    End Function

End Class
