﻿
Public Class Zml

    Dim CsCode As New List(Of String)
    Dim BlockStart, BlockEnd As XElement
    Dim Xml As XElement
    Const TempBody = "<zmlbody />"
    Const TempBodyStart = "<zmlbody>"
    Const TempBodyEnd = "</zmlbody>"
    Const RawTextStart = "<zmlrawtext>"
    Const RawTextEnd = "</zmlrawtext>"
    Const ChngQt = "__chngqt__;"

    Dim autoGeneratedComment As String

    Public Sub New(Optional fileName As String = "")
        If fileName <> "" Then
            autoGeneratedComment =
$"@* <!-- This cshtml file is auto-generated from the {fileName} file. 
You must not do any changes here, because it can be overwritten 
when the {fileName} file changes.
Make all the changes you need to the {fileName} file.
But even this is an auto-generated, DONT DELETE this file 
from the soluton explorer, becuase there is a cshtml.vb file attached to, 
and will also be deleted with all the code you wrote in it!  --> *@
"
        End If
    End Sub

    Function CompileZml(zml As XElement) As String

        BlockStart = AddToCsList("{")
        BlockEnd = AddToCsList("}")

        ' <z:displayfor var="modelItem" return="item.OrderDate" />

        Xml = New XElement(zml)
        PreserveLines(Xml)
        FixSelfClosing()
        ParseImports()
        ParseHelperImports()
        ParseLayout()
        ParsePage()
        ParseInjects()
        ParseModel()
        ParseTitle()
        ParseText()
        FixTagHelpers()
        FixAttrExpressions()
        ParseChecks()
        ParseIfStatements()
        ParseForEachLoops()
        ParseForLoops()
        ParseWhileLoops()
        ParseBreaks()
        ParseComments()
        ParseViewData()
        ParseHtmlHelpers()
        ParseDots()
        ParseLambdas()
        ParseInvokes()
        ParseGetters()
        ParseSetters()
        ParseSections()
        ParseDeclarations()

        Dim x = Xml.ToString()
        For n = CsCode.Count - 1 To 0 Step -1
            x = x.Replace($"<zmlitem{n} />", CsCode(n))
        Next

        x = x.Replace(
                   (LessThan, "<"), (GreaterThan, ">"),
                   (Ampersand, "&"), (tempText, ""), (AtSymbole, "@"),
                   (Qt + ChngQt, SnglQt),
                   (ChngQt + Qt, SnglQt),
                   (tempComma, ","),
                   (ChngQt, ""), (SnglQt + SnglQt, Qt),
                   (doctypeTag, doctypeStr)
                 ).Trim(" ", vbCr, vbLf)

        x = RestoreRawText(x)
        Dim lines = x.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim sb As New Text.StringBuilder(autoGeneratedComment)
        Dim offset = 0

        For Each line In lines
            Dim absLine = line.Trim()
            If absLine = TempRoot Or absLine = TempTagStart Then
                offset += 2
            ElseIf absLine = TempTagEnd Then
                offset -= 2
            ElseIf absLine <> "" AndAlso absLine <> TempBodyStart AndAlso absLine <> TempBodyEnd AndAlso absLine <> TempBody Then
                line = line.Replace((TempBodyStart, ""), (TempBodyEnd, ""))
                If line.Length > offset AndAlso line.StartsWith(New String(" ", offset)) Then
                    sb.AppendLine(line.Substring(offset))
                Else
                    sb.AppendLine(line)
                End If
            End If
        Next
        Return sb.ToString().
            Replace(
            (TempTagStart, ""), (TempTagEnd, ""),
            (TempBody, "")).Trim(" ", vbCr, vbLf)
    End Function

    Private Function RestoreRawText(x As String) As String
        Dim st = 0
        Dim en = 0
        Do
            en = x.IndexOf(RawTextStart)
            If en = -1 Then Return x
            st = x.LastIndexOf(Ln, en)
            If st = -1 Then st = 0
            x = x.Remove(st, en + RawTextStart.Length - st)

            st = x.IndexOf(RawTextEnd, st)
            en = x.IndexOf(Ln, st) + 1
            If en = 0 Then
                en = x.Length - 1
            Else
                While x(en + 1) = " "
                    en += 1
                End While
            End If
            x = x.Remove(st, en + 1 - st)
        Loop

    End Function

    Private Function AddToCsList(item As String, Optional node As XElement = Nothing) As XElement
        If node IsNot Nothing Then
            If IsInsideCsBlock(node) Then
                item = item.TrimStart("@"c, "{"c).TrimEnd("}"c).Trim()
            End If
        End If

        CsCode.Add(item)
        Dim zmlKey = "zmlitem" & CsCode.Count - 1
        Return <<%= zmlKey %>/>
    End Function

    Friend Shared Function FixAttr(x As String) As String

        Dim lines = x.Replace(
                  ("&", Ampersand), ("@", AtSymbole),
                  (doctypeStr, doctypeTag)
               ).Split({CChar(vbCr), CChar(vbLf)}, StringSplitOptions.RemoveEmptyEntries)
        Dim sb As New Text.StringBuilder

        For Each line In lines
            Dim absLine = line.TrimStart()
            Dim spaces = New String(" ", line.Length - absLine.Length)
            Do
                Dim L = absLine.Length
                absLine = absLine.Replace(("< ", "<"), (" >", ">"))
                If L = absLine.Length Then Exit Do
            Loop
            sb.Append(spaces)
            sb.AppendLine(absLine)
        Next

        x = sb.ToString()

        Dim tags = {lambdaTag, usingTag, importsTag, namespaceTag, helpersTag}

        For Each tag In tags
            Dim lambdaCase = (tag = lambdaTag)
            tag = tag.Replace(zns, "z:")
            Dim pos = 0
            Dim offset = 0
            Dim endPos = 0
            Do
                pos = x.IndexOf("<" & tag + " ", endPos)
                If pos = -1 Then Exit Do
                offset = pos + tag.Length + 2
                endPos = x.IndexOf("/>", offset)
                If endPos > -1 Then
                    Dim s = x.Substring(offset, endPos - offset)
                    Dim t = s
                    If lambdaCase Then t = t.Replace("return=", "")

                    If Not t.Contains("=") Then
                        Dim attrs = s.Split(" "c, CChar(vbCr), CChar(vbLf))
                        sb = New Text.StringBuilder
                        For Each attr In attrs
                            If attr <> "" Then
                                If attr.Contains("=") Then
                                    sb.Append(attr)
                                Else
                                    sb.Append(attr + "=" + Qt + Qt)
                                End If
                                sb.Append(" ")
                            End If
                        Next
                        x = x.Substring(0, offset) + sb.ToString().TrimEnd() + x.Substring(endPos)
                        endPos += 2
                    End If
                End If
            Loop
        Next


        x = FixConditions(x)

        Return x
    End Function

    Private Shared Function FixConditions(x As String) As String
        Dim pos = 0
        Dim endPos = 0
        Dim sb As New Text.StringBuilder()

        Do
            pos = x.IndexOf(Qt, endPos)
            If pos = -1 Then Exit Do
            sb.Append(x.Substring(endPos, pos - endPos))
            endPos = x.IndexOf(Qt, pos + 1) + 1
            If endPos = 0 Then Exit Do
            Dim s = x.Substring(pos, endPos - pos).
                    Replace(("<", LessThan), (">", GreaterThan))
            sb.Append(s)
        Loop

        If endPos = 0 Then Return x

        sb.Append(x.Substring(endPos, x.Length - endPos))
        Return sb.ToString()
    End Function

    ' Qute string values, except objects (starting with @) and chars (quted by ' ')
    Private Function Quote(value As String) As String
        If value Is Nothing Then Return Nothing

        If value.StartsWith(AtSymbole) Then
            Return value.Substring(AtSymbole.Length) ' value is object
        ElseIf value.StartsWith("@") Then ' This is for test app
            Return value.Substring(1) ' value is object 
        ElseIf value.StartsWith(SnglQt) AndAlso value.EndsWith(SnglQt) Then
            Return value ' value is char
        ElseIf value.StartsWith("#") AndAlso value.EndsWith("#") Then
            Return $"DateTime.Parse({Qt}{value.Trim("#")}{Qt}, new System.Globalization.CultureInfo({Qt}en-US{Qt}))"
        ElseIf value = trueKeyword OrElse value = falseKeyword Then
            Return value ' value is boolean  
        ElseIf Double.TryParse(value, New Double()) Then
            Return value ' value is numeric
        ElseIf ContainsLambda(value) Then
            Return value ' value is lambda expr.
        Else
            Return Qt + value.Trim(Qt) + Qt ' value is string
        End If
    End Function

    ' vars are always objects. Erase @ and qoutes
    Private Function At(value As String) As String
        If value Is Nothing Then Return Nothing

        If value.StartsWith(AtSymbole) Then
            Return value.Substring(AtSymbole.Length)
        ElseIf value.StartsWith("@") Then ' This is for test app
            Return value.Substring(1) ' value is object 
        Else
            Return value.Trim(Qt).Trim(SnglQt)
        End If
    End Function

    Private Function CombineXml(ParamArray xml() As XElement) As XElement
        Dim x = <zml/>
        x.Add(xml)
        Return x
    End Function

    Private Function CombineXml(header As XElement, blocks As List(Of XElement), footer As XElement) As XElement
        Dim x = <zml/>
        x.Add(header)
        x.Add(blocks)
        x.Add(footer)
        Return x
    End Function

    Private Function GetCsHtml(cs As String, html As XElement, Optional CheckParentBlock As Boolean = False, Optional AtRequired As Boolean = False) As XElement
        Dim x = <zml/>
        Dim csTag As XElement
        If AtRequired Then
            csTag = AddToCsList(cs & " {", Nothing)
            x.Add(
                 csTag,
                  html.InnerXml,
              BlockEnd
          )
        Else
            csTag = AddToCsList(cs, If(CheckParentBlock, html.Parent, html))
            x.Add(
                csTag,
                BlockStart,
                    html.InnerXml,
                BlockEnd
          )
        End If

        Return x
    End Function

    Private Function IsInsideCsBlock(item As XElement) As Boolean
        If item Is Nothing Then Return False
        Dim pn As XElement = item.Parent
        If pn Is Nothing Then Return False
        Dim parentName = pn.Name.ToString
        If parentName.StartsWith(zns) AndAlso parentName <> sectionTag Then Return True

        If parentName = "zmlbody" OrElse (parentName = "zml" AndAlso pn.Nodes.Count = 1) Then pn = pn.Parent
        If pn Is Nothing Then Return False
        parentName = pn.Name.ToString()
        If parentName.StartsWith(zns) AndAlso parentName <> sectionTag Then Return True

        Dim blkSt = BlockStart.Name.ToString()

        For Each node In pn.Nodes
            Dim x = TryCast(node, XElement)
            If x IsNot Nothing AndAlso x.Name.ToString() = blkSt Then Return True
        Next

        Return False
    End Function

    Private Sub PreserveLines(x As XElement)
        If x Is Nothing Then Return
        If x.Nodes.Count = 1 AndAlso TypeOf x.Nodes(0) Is XText Then
            If x.Name.LocalName <> "zml" Then
                Dim n = x.Nodes(0).ToString()
                If ContainsLambda(n) Then
                    x.Nodes(0).ReplaceWith(ParseRawLmbda(n))
                End If
            End If
            Return
        End If

        For Each node In x.Nodes
            If TypeOf node Is XText Then
                Dim s = RawTextStart + ParseRawLmbda(node.ToString()) + RawTextEnd
                node.ReplaceWith(XElement.Parse(s))
                ' Loop is broken. Reapeat from start
                PreserveLines(x)
                Exit For
            ElseIf TypeOf node Is XElement Then
                PreserveLines(node)
            End If
        Next
    End Sub

    Private Function ContainsLambda(x As String) As Boolean
        Return x.Contains("=>") OrElse
            x.Contains("=&gt;") OrElse
            x.Contains("=" & GreaterThan)
    End Function

    Private Function AddLoopLables(x As XElement, label As String) As XElement
        If label <> "" Then

            Dim body As XElement = (From n In x.Nodes
                                    Where TryCast(n, XElement)?.Name.ToString() = "zmlbody").First

            body.Add(AddToCsList("continue_" & label & ":"))

            Dim be = BlockEnd.Name.ToString()
            Dim BlkEnd As XElement = (From n In x.Nodes()
                                      Where TryCast(n, XElement)?.Name.ToString() = be).First

            BlkEnd.AddAfterSelf(AddToCsList("break_" & label & ":"))
        End If
        Return x
    End Function

End Class
