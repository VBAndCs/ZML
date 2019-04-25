' ZML Parser: Converts ZML tags to C# Razor statements.
' Copyright (c) Mohammad Hamdy Ghanem 2019


Imports System.Runtime.CompilerServices

Public Module ZMLExtentions

    Public Const SnglQt = "'"
    Public Const Qt = """"
    Public Const Ln = vbCrLf

    <Extension>
    Function ContainsAny(s As String, ParamArray findStr() As String) As Boolean
        For Each x In findStr
            If s.Contains(x) Then Return True
        Next

        Return False
    End Function

    <Extension>
    Function EndsWithAny(s As String, ParamArray ends() As String) As Boolean
        For Each e In ends
            If s.EndsWith(e) Then Return True
        Next
        Return False
    End Function

    <Extension>
    Function Replace(s As String, ParamArray repPairs() As (repStr As String, repWithStr As String)) As String
        If s = "" Then Return ""

        For Each x In repPairs
            s = s.Replace(x.repStr, x.repWithStr)
        Next
        Return s
    End Function

    <Extension>
    Private Function ToXml(x As String) As XElement
        x = Zml.FixAttr(x)
        Dim xml = XElement.Parse(
                Zml.TempRoot +
                 x +
                 Zml.TempTagEnd)
        'LoadOptions.PreserveWhitespace)

        Return xml
    End Function

    <Extension>
    Public Function GetInnerXML(el As XElement) As String
        Return InnerXml(el).ToString().
                   Replace((Zml.TempRoot, ""), (Zml.TempTagStart, ""), (Zml.TempTagEnd, ""),
                   ("<zmlbody>", ""), ("</zmlbody>", "")).
                   Trim(" ", vbCr, vbLf)
    End Function

    <Extension>
    Friend Function InnerXml(el As XElement) As XElement
        Dim x = <zmlbody/>
        x.Add(el.Nodes)
        Return x
    End Function

    <Extension>
    Friend Function OuterXml(el As XElement) As XElement
        Dim x = <zmlbody/>
        x.Add(el)
        Return x
    End Function

    <Extension>
    Function ParseZml(zml As XElement, Optional addComment As Boolean = False) As String
        Return New Zml().ParseZml(zml)
    End Function

    <Extension>
    Public Function ParseZml(zml As String) As String
        Return ToXml(zml).ParseZml(True)
    End Function
End Module
