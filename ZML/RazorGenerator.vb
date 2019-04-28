Partial Public Class Zml

    Private Sub FixSelfClosing()
        Dim tagNamess = {"span", "label"}
        Dim tags = (From elm In Xml.Descendants()
                    Where tagNamess.Contains(elm.Name.ToString()))

        ' add a temp node to ensure using a closing tag
        For Each tag In tags
            If tag.Nodes.Count = 0 Then
                tag.Add(tempText)
            End If
        Next
    End Sub

    Private Sub FixTagHelpers()
        Dim tageHelpers = From elm In Xml.Descendants()
                          From attr In elm.Attributes
                          Let name = attr.Name.ToString()
                          Where name = aspItems Or name = aspFor
                          Select attr

        For Each tageHelper In tageHelpers
            Dim value = tageHelper.Value
            If Not (value.StartsWith("@") Or value.StartsWith(AtSymbole)) Then
                If value.StartsWith(ModelKeyword) Then
                    tageHelper.Value = "@" + value
                Else
                    tageHelper.Value = atModel + "." + value
                End If
            End If
        Next

    End Sub

    Sub FixAttrExpressions()
        Dim attrs = From elm In Xml.Descendants()
                    From attr In elm.Attributes
                    Select attr

        For Each attr In attrs
            Dim value = attr.Value
            If value.Contains(SnglQt & SnglQt) Then
                attr.Value = ChngQt & value & ChngQt
            End If
            If ContainsLambda(value) Then
                attr.Value = ParseRawLmbda(attr.Value)
            End If
        Next

    End Sub

    Private Sub ParseTitle()
        Dim viewTitle = (From elm In Xml.Descendants()
                         Where elm.Name = viewtitleTag)?.FirstOrDefault

        If viewTitle IsNot Nothing Then
            Dim cs = ""
            Dim value = If(viewTitle.Attribute(valueAttr)?.Value, viewTitle.Value)
            If value = "" Then 'Read Title
                cs = $"@ViewData[{Qt }Title{Qt }]"
            Else ' Set Title
                cs = "@{ " & $"ViewData[{Qt}Title{Qt}] = {Quote(value)};" & " }"
            End If

            viewTitle.ReplaceWith(AddToCsList(cs, viewTitle))

            ParseTitle()
        End If

    End Sub

    Private Sub ParseText()
        Dim text = (From elm In Xml.Descendants()
                    Where elm.Name = textTag)?.FirstOrDefault

        If text IsNot Nothing Then

            Dim value = If(text.Attribute(valueAttr)?.Value, text.Value)
            text.ReplaceWith(AddToCsList("@: " + value))

            ParseText()
        End If

    End Sub

    Private Sub ParseComments()
        Dim comment = (From elm In Xml.Descendants()
                       Where elm.Name = commentTag).FirstOrDefault

        If comment IsNot Nothing Then
            Dim x = CombineXml(
                      AddToCsList("@*"),
                      comment.InnerXml,
                     AddToCsList("*@"))

            comment.ReplaceWith(x)
            ParseComments()
        End If
    End Sub

    Private Sub ParseHtmlHelpers()
        ParseHtmlHelper(displayforTag, "DisplayFor")
        ParseHtmlHelper(displaynameforTag, "DisplayNameFor")
    End Sub

    Sub ParseHtmlHelper(tag As String, methodName As String)
        Dim helper = (From elm In Xml.Descendants()
                      Where elm.Name = tag).FirstOrDefault

        If helper IsNot Nothing Then
            Dim var = At(helper.Attribute(varAttr).Value)
            Dim _return = ""
            If helper.Nodes.Count > 0 AndAlso TypeOf helper.Nodes(0) Is XElement Then
                _return = ParseNestedInvoke(helper.Nodes(0))
            Else
                _return = At(If(helper.Attribute(returnAttr)?.Value, helper.Value))
            End If
            Dim cs = $"@Html.{methodName}({var} ={GreaterThan} {_return})"
            helper.ReplaceWith(AddToCsList(cs, helper))
            ParseHtmlHelper(tag, methodName)
        End If
    End Sub

    Private Sub ParseViewData()
        Dim viewdata = (From elm In Xml.Descendants()
                        Where elm.Name = viewdataTag)?.FirstOrDefault

        If viewdata IsNot Nothing Then
            Dim _keyAttr = viewdata.Attribute(keyAttr)
            Dim value = If(viewdata.Attribute(valueAttr)?.Value, viewdata.Value)

            If _keyAttr Is Nothing Then
                ' Write miltiple values to ViewData

                Dim sb As New Text.StringBuilder(Ln + "@{" + Ln)
                For Each key In viewdata.Attributes
                    sb.AppendLine($"ViewData[{Quote(key.Name.ToString())}] = {Quote(key.Value)};")
                Next
                sb.AppendLine("}" + Ln)

                viewdata.ReplaceWith(AddToCsList(sb.ToString(), viewdata))

            ElseIf value IsNot Nothing Then
                ' Write one value to ViewData

                Dim cs = $"ViewData[{Quote(_keyAttr.Value)}] = {Quote(value)};"
                viewdata.ReplaceWith(AddToCsList(cs, viewdata))

            Else ' Read from ViewData
                Dim cs = $"@ViewData[{Quote(_keyAttr.Value)}]"
                viewdata.ReplaceWith(AddToCsList(cs, viewdata))
            End If

            ParseViewData()
        End If

    End Sub

    Private Sub ParseImports()
        Dim tag = (From elm In Xml.Descendants()
                   Where elm.Name = importsTag OrElse
                            elm.Name = usingTag OrElse
                            elm.Name = namespaceTag)?.FirstOrDefault

        If tag IsNot Nothing Then
            Dim ns = If(tag.Attribute(nsAttr)?.Value, tag.Value)
            Dim _using = ""
            Dim x = ""

            If tag.Name.ToString = namespaceTag Then
                _using = AtNamespace & " "
            Else
                _using = AtUsing & " "
            End If

            If ns = "" Then
                Dim sb As New Text.StringBuilder
                For Each attr In tag.Attributes
                    sb.AppendLine(_using + attr.Name.ToString())
                Next
                x = sb.ToString()
            Else
                x = _using + ns
            End If

            tag.ReplaceWith(AddToCsList(x))
            ParseImports()
        End If
    End Sub

    Private Sub ParseHelperImports()
        Dim helper = (From elm In Xml.Descendants()
                      Where elm.Name = helpersTag)?.FirstOrDefault

        If helper IsNot Nothing Then
            Dim ns = If(helper.Attribute(nsAttr)?.Value, helper.Value)
            Dim x = ""
            If ns = "" Then
                Dim sb As New Text.StringBuilder
                For Each attr In helper.Attributes
                    sb.AppendLine($"@addTagHelper {attr.Value}, {attr.Name}")
                Next
                x = sb.ToString()
            Else
                Dim add = If(helper.Attribute(addAttr)?.Value, "*")
                x = $"@addTagHelper {add}, {ns}"
            End If

            helper.ReplaceWith(AddToCsList(x))
            ParseHelperImports()
        End If
    End Sub

    Private Sub ParsePage()
        Dim page = (From elm In Xml.Descendants()
                    Where elm.Name = pageTag)?.FirstOrDefault

        If page IsNot Nothing Then
            Dim route = If(page.Attribute(routeAttr)?.Value, page.Value)
            Dim x = AtPage & " "
            If route <> "" Then x += Qt + route + Qt

            page.ReplaceWith(AddToCsList(x))
            ParsePage()
        End If
    End Sub

    Private Sub ParseLayout()
        Dim layout = (From elm In Xml.Descendants()
                      Where elm.Name = layoutTag)?.FirstOrDefault

        If layout IsNot Nothing Then
            Dim page = If(layout.Attribute(pageAttr)?.Value, layout.Value)
            Dim x = "@{" + Ln + $"      Layout = {Qt}{page}{Qt};" + Ln + "}"

            layout.ReplaceWith(AddToCsList(x))
        End If
    End Sub

    Private Sub ParseModel()
        Dim model = (From elm In Xml.Descendants()
                     Where elm.Name = modelTag)?.FirstOrDefault

        If model IsNot Nothing Then
            Dim type = If(model.Attribute(typeAttr)?.Value, model.Value)
            Dim x = "@model " + convVars(type)

            model.ReplaceWith(AddToCsList(x))
        End If
    End Sub

    Private Sub ParseInjects()
        Dim inject = (From elm In Xml.Descendants()
                      Where elm.Name = injectTag)?.FirstOrDefault

        If inject IsNot Nothing Then
            Dim sb As New Text.StringBuilder()
            For Each arg In inject.Attributes
                Dim name = arg.Name.ToString()
                Dim type = convVars(arg.Value)
                If name.EndsWith("." & typeAttr) Then name = name.Substring(0, name.Length - 5)
                sb.AppendLine($"@inject {type} {name}")
            Next

            inject.ReplaceWith(AddToCsList(sb.ToString()))
            ParseInjects()
        End If
    End Sub

    Private Sub ParseSections()
        Dim section = (From elm In Xml.Descendants()
                       Where elm.Name = sectionTag)?.FirstOrDefault

        If section IsNot Nothing Then
            Dim name = At(section.Attribute(nameAttr).Value)
            Dim cs = $"@section {name}"
            section.ReplaceWith(GetCsHtml(cs, section, False, True))

            ParseSections()
        End If
    End Sub

End Class
