Partial Public Class Zml

    Function ParseZml(zml As XElement) As String
        BlockStart = AddToCsList("{")
        BlockEnd = AddToCsList("}")

        ' <z:displayfor var="modelItem" return="item.OrderDate" />

        Xml = New XElement(zml)
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
                   (ChngQt, ""), (SnglQt + SnglQt, Qt),
                   (doctypeTag, doctypeStr)
                 ).Trim(" ", vbCr, vbLf)

        Dim lines = x.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim sb As New Text.StringBuilder()

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
        Return sb.ToString().Trim(" ", vbCr, vbLf)
    End Function

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

    Private Sub ParseSetters()

        Dim setter = (From elm In Xml.Descendants()
                      Where elm.Name = setTag).FirstOrDefault

        If setter IsNot Nothing Then
            Dim cs As XElement
            Dim obj = setter.Attribute(objectAttr)
            If obj Is Nothing Then
                ' Set multiple values

                If setter.Attributes.Count = 1 Then
                    Dim o = setter.Attributes()(0)
                    cs = AddToCsList("@{ " & $"{At(o.Name.ToString())} = {Quote(o.Value)};" & " }", setter)
                Else
                    Dim settings = <zmlbody/>
                    For Each o In setter.Attributes
                        settings.Add(AddToCsList($"{At(o.Name.ToString())} = {Quote(o.Value)};"))
                    Next
                    cs = CombineXml(
                            AddToCsList("@{", setter),
                                 settings,
                            AddToCsList("}", setter))
                End If
            Else ' Set single value 
                Dim key = setter.Attribute(keyAttr)

                Dim value = ""
                If setter.Nodes.Count > 0 AndAlso TypeOf setter.Nodes(0) Is XElement Then
                    value = ParseNestedInvoke(setter.Nodes(0))
                Else
                    value = Quote(If(setter.Attribute(valueAttr)?.Value, setter.Value))
                End If

                If key Is Nothing Then
                    ' Set single value without key
                    cs = AddToCsList("@{ " + $"{At(obj.Value)} = {value};" + " }", setter)

                Else ' Set single value with key
                    cs = AddToCsList("@{ " + $"{At(obj.Value)}[{Quote(key.Value)}] = {value};" + " }", setter)
                End If
            End If

            setter.ReplaceWith(cs)
            ParseSetters()
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
            Dim cs = $"@Html.{methodName}({var} => {_return})"
            helper.ReplaceWith(AddToCsList(cs, helper))
            ParseHtmlHelper(tag, methodName)
        End If
    End Sub

    Private Sub ParseDeclarations()

        Dim _declare = (From elm In Xml.Descendants()
                        Where elm.Name = declareTag).FirstOrDefault

        If _declare IsNot Nothing Then
            Dim cs As XElement
            Dim var = _declare.Attribute(varAttr)

            If var Is Nothing Then
                ' Set multiple values

                If _declare.Attributes.Count = 1 Then
                    Dim o = _declare.Attributes()(0)
                    cs = AddToCsList("@{ " & $"var {At(o.Name.ToString())} = {Quote(o.Value)};" & " }", _declare)
                Else
                    Dim settings = <zmlbody/>
                    For Each o In _declare.Attributes
                        settings.Add(AddToCsList($"var {At(o.Name.ToString())} = {Quote(o.Value)};"))
                    Next
                    cs = CombineXml(
                            AddToCsList("@{", _declare),
                                 settings,
                            AddToCsList("}", _declare))
                End If

            Else ' Set single value 
                Dim type = If(convVars(_declare.Attribute(typeAttr)?.Value), varAttr)
                Dim value = ""
                If _declare.Nodes.Count > 0 AndAlso TypeOf _declare.Nodes(0) Is XElement Then
                    value = ParseNestedInvoke(_declare.Nodes(0))
                Else
                    value = Quote(If(_declare.Attribute(valueAttr)?.Value, _declare.Value))
                End If
                Dim key = _declare.Attribute(keyAttr)

                If key Is Nothing Then
                    ' Set var value without key
                    cs = AddToCsList("@{ " + $"{type} {At(var.Value)} = {value};" + " }", _declare)
                Else ' Set single value with key
                    cs = AddToCsList("@{ " + $"{type} {At(var.Value)} = {At(value)}[{Quote(key.Value)}];" + " }", _declare)
                End If
            End If

            _declare.ReplaceWith(cs)
            ParseDeclarations()
        End If

    End Sub

    Private Sub ParseGetters()

        Dim getter = (From elm In Xml.Descendants()
                      Where elm.Name = getTag).FirstOrDefault

        If getter IsNot Nothing Then
            Dim key = getter.Attribute(keyAttr)
            Dim obj = At(If(getter.Attribute(objectAttr)?.Value, getter.Value))
            Dim x = ""

            If key Is Nothing Then
                x = $"@{obj}" + Ln
            Else
                x = $"@{obj}[{Quote(key.Value)}]" + Ln
            End If

            getter.ReplaceWith(AddToCsList(x, getter))

            ParseGetters()
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

    Private Sub ParseForLoops()
        Dim _for = (From elm In Xml.Descendants()
                    Where elm.Name = forTag)?.FirstOrDefault

        If _for IsNot Nothing Then
            Dim label, var, _to, _step, _while, _let, type As XAttribute

            For Each attr In _for.Attributes
                Select Case attr.Name.ToString().ToLower()
                    Case labelAttr
                        label = attr
                    Case toAttr
                        _to = attr
                    Case stepAttr
                        _step = attr
                    Case whileattr
                        _while = attr
                    Case letAttr
                        _let = attr
                    Case typeAttr
                        type = attr
                    Case Else
                        var = attr
                End Select
            Next

            Dim varName = At(var.Name.ToString())
            Dim cond = ""
            Dim inc = ""

            If _to IsNot Nothing Then
                cond = _to.Value.Trim().Replace(atModel + ".", ModelKeyword + ".")
                Dim n As Integer
                Dim L = cond.Length - 1
                Dim reverseLoop = False

                If _step Is Nothing Then
                    inc = varName + "++"
                Else
                    Dim value = Quote(_step.Value)
                    If Integer.TryParse(value, n) Then
                        If n > 0 Then
                            inc = varName & " += " & n
                        ElseIf n = -1 Then
                            inc = varName & "--"
                            reverseLoop = True
                        Else
                            inc = varName & " -= " & Math.Abs(n)
                            reverseLoop = True
                        End If
                    Else
                        inc = varName & " += " & value
                    End If

                End If

                cond = varName + If(reverseLoop, " > " & cond & " - 1", " < " & cond & " + 1")
                If cond.EndsWithAny(" - 1 + 1", " + 1 - 1") Then
                    cond = cond.Substring(0, cond.Length - 8)
                ElseIf cond.EndsWith(" > 0 - 1") Then
                    cond = cond.Substring(0, cond.Length - 5) & "-1"
                End If
            Else
                cond = ConvCond(_while?.Value.Replace(atModel + ".", ModelKeyword + "."))
                inc = If(_let?.Value, varName & "++").Replace(atModel + ".", ModelKeyword + ".")
            End If

            Dim typeName = If(convVars(type?.Value), varAttr)

            Dim cs = $"@for ({typeName} {varName} = {Quote(var.Value)}; {cond}; {inc})"

            Dim x = AddLable(GetCsHtml(cs, _for), label?.Value)
            _for.ReplaceWith(x)
            ParseForLoops()
        End If
    End Sub

    Private Function AddLable(x As XElement, label As String) As XElement
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

    Private Sub ParseForEachLoops()
        Dim foreach = (From elm In Xml.Descendants()
                       Where elm.Name = foreachTag)?.FirstOrDefault

        If foreach IsNot Nothing Then
            Dim type = At(If(convVars(foreach.Attribute(typeAttr)?.Value), "var"))
            Dim _var = At(foreach.Attribute(varAttr)?.Value)
            Dim _in = foreach.Attribute(inAttr).Value.Replace(atModel + ".", ModelKeyword + ".")
            Dim label = foreach.Attribute(labelAttr)?.Value

            If _var = "" Then
                _var = (From attr In foreach.Attributes
                        Where attr.Value = "").FirstOrDefault.Name.ToString()
            End If

            Dim cs = $"@foreach ({type} {_var} in {_in})"
            Dim x = AddLable(GetCsHtml(cs, foreach), label)
            foreach.ReplaceWith(x)

            ParseForEachLoops()
        End If
    End Sub

    Private Sub ParseWhileLoops()
        Dim _while = (From elm In Xml.Descendants()
                      Where elm.Name = whileTag)?.FirstOrDefault

        If _while IsNot Nothing Then
            Dim cond = ConvCond(_while.Attribute(conditionAttr)?.Value)
            Dim label = _while.Attribute(labelAttr)?.Value

            Dim cs = $"@while ({cond})"
            Dim x = AddLable(GetCsHtml(cs, _while), label)
            _while.ReplaceWith(x)

            ParseForEachLoops()
        End If
    End Sub

    Sub ParseBreaks()
        ParseBreak(exitTag, "break")
        ParseBreak(breakTag, "break")
        ParseBreak(continueTag, "continue")
    End Sub

    Private Sub ParseBreak(tagName As String, keyword As String)
        Dim tag = (From elm In Xml.Descendants()
                   Where elm.Name = tagName)?.FirstOrDefault

        If tag IsNot Nothing Then
            Dim label = tag.Attribute(labelAttr)?.Value
            Dim cs = ""
            If label = "" Then
                cs = keyword & ";"
            Else
                cs = $"{gotoKeyword} {keyword}_{label};"
            End If

            tag.ReplaceWith(AddToCsList(cs))
            ParseBreak(tagName, keyword)
        End If
    End Sub

    Private Sub ParseChecks()
        Dim check = (From elm In Xml.Descendants()
                     Where elm.Name = checkTag)?.FirstOrDefault

        If check IsNot Nothing Then
            Dim cond = check.Attribute(conditionAttr).Value
            Dim ifNull = Quote(check.Attribute(ifnullAttr)?.Value)
            Dim cs = ""
            If ifNull IsNot Nothing Then
                cs = $"@{cond} ?? {ifNull}"
            Else
                Dim IfTrue = Quote(check.Attribute(iftrueAttr).Value)
                Dim ifFalse = Quote(check.Attribute(iffalseAttr).Value)
                cs = $"@{cond} ? {IfTrue} : {ifFalse}"
            End If

            check.ReplaceWith(AddToCsList(cs, check))
            ParseChecks()
        End If
    End Sub

    Private Sub ParseIfStatements()
        Dim _if = (From elm In Xml.Descendants()
                   Where elm.Name = ifTag)?.FirstOrDefault

        If _if IsNot Nothing Then

            If _if.Nodes.Count > 0 Then
                Dim children = _if.Nodes
                Dim firstChild = TryCast(children(0), XElement)
                If firstChild IsNot Nothing AndAlso firstChild.Name = thenTag Then
                    Dim cs = "@if (" + ConvCond(_if.Attribute(conditionAttr).Value) + ")"
                    Dim _then = GetCsHtml(cs, firstChild, True)

                    Dim _elseifs = ParseElseIfs(_if)

                    Dim _else As XElement
                    Dim lastChild As XElement = children(children.Count - 1)
                    If lastChild.Name = elseTag Then
                        _else = GetCsHtml("else", lastChild, True)
                    End If

                    If _elseifs.Count = 0 Then
                        _if.ReplaceWith(CombineXml(_then, _else))
                    Else
                        _if.ReplaceWith(CombineXml(_then, _elseifs, _else))
                    End If
                Else
                    Dim cs = "@if (" + ConvCond(_if.Attribute("condition").Value) + ")"
                    _if.ReplaceWith(GetCsHtml(cs, _if))
                End If
            Else
                Dim cs = "@if (" + ConvCond(_if.Attribute("condition").Value) + ")"
                _if.ReplaceWith(GetCsHtml(cs, _if))
            End If

            ParseIfStatements()
        End If

    End Sub

    Private Function ParseElseIfs(ifXml As XElement) As List(Of XElement)
        Dim _elseifs = (From elm In ifXml.Descendants()
                        Where elm.Name = elseifTag)

        Dim x As New List(Of XElement)
        Dim cs = ""

        For Each _elseif In _elseifs
            cs = "else if (" + ConvCond(_elseif.Attribute(conditionAttr).Value) + ")"
            x.Add(GetCsHtml(cs, _elseif, True))
        Next

        Return x
    End Function

    Private Sub ParseDots()
        Dim dot = (From elm In Xml.Descendants()
                   Where elm.Name = dotTag)?.FirstOrDefault

        If dot IsNot Nothing Then
            Dim sb As New Text.StringBuilder()
            For Each item As XElement In dot.Nodes
                Dim x = ParseNestedInvoke(item)
                If x.StartsWith(awaitKeyword) Then x = "(" + x + ")"
                sb.Append(x)
                sb.Append(".")
            Next
            Dim cs = "@" & If(sb.Length = 0, "", sb.Remove(sb.Length - 1, 1).ToString())
            dot.ReplaceWith(AddToCsList(cs, dot))
            ParseDots()
        End If
    End Sub

    Private Sub ParseInvokes()
        Dim invoke = (From elm In Xml.Descendants()
                      Where elm.Name = invokeTag OrElse
                          elm.Name = awaitTag)?.FirstOrDefault

        If invoke IsNot Nothing Then
            Dim cs = ""
            For Each attr In invoke.Attributes
                Select Case attr.Name.ToString
                    Case propertyAttr
                        cs = At($"@{attr.Value}")
                    Case indexerAttr
                        Dim indexer = attr.Value
                        Dim sb As New Text.StringBuilder()

                        For Each key As XElement In invoke.Nodes
                            Dim namedArg = key.Attribute(nameAttr)?.Value
                            If namedArg <> "" Then sb.Append(namedArg + ": ")
                            If key.Nodes.Count > 0 AndAlso TypeOf key.Nodes(0) Is XElement Then
                                sb.Append(ParseNestedInvoke(key.Nodes(0)))
                            Else
                                sb.Append(Quote(If(key.Value, key.Attribute(valueAttr).Value)))
                            End If

                            sb.Append(", ")
                        Next

                        Dim args = If(sb.Length = 0, "", sb.Remove(sb.Length - 2, 2).ToString())

                        If invoke.Name.ToString = awaitTag Then
                            cs = "@{ " & awaitKeyword & $" {indexer}[{args}]" + "; }"
                        Else
                            cs = $"@{indexer}([args])"
                        End If
                    Case Else
                        Dim method = convVars(At(If(invoke.Attribute(methodAttr)?.Value, invoke.Attributes()(0).Name.ToString())))
                        Dim sbArgs As New Text.StringBuilder()
                        Dim sbTypeParams As New Text.StringBuilder()
                        For Each arg As XElement In invoke.Nodes
                            Select Case arg.Name.ToString()
                                Case typeparamTag
                                    sbTypeParams.Append(convVars(At(If(arg.Value, arg.Attribute(typeAttr).Value))))
                                    sbTypeParams.Append(", ")
                                Case argTag
                                    Dim namedArg = arg.Attribute(nameAttr)?.Value
                                    If namedArg <> "" Then sbArgs.Append(namedArg + ": ")
                                    If arg.Nodes.Count > 0 AndAlso TypeOf arg.Nodes(0) Is XElement Then
                                        sbArgs.Append(ParseNestedInvoke(arg.Nodes(0)))
                                    Else
                                        sbArgs.Append(Quote(If(arg.Value, arg.Attribute(valueAttr).Value)))
                                    End If
                                    sbArgs.Append(", ")
                                Case Else
                                    sbArgs.Append(ParseNestedInvoke(arg))
                                    sbArgs.Append(", ")
                            End Select
                        Next

                        Dim args = If(sbArgs.Length = 0, "", sbArgs.Remove(sbArgs.Length - 2, 2).ToString())
                        Dim typeParams = If(sbTypeParams.Length = 0, "", "<" & sbTypeParams.Remove(sbTypeParams.Length - 2, 2).ToString() & ">")

                        If invoke.Name.ToString = awaitTag Then
                            cs = "@{ " & awaitKeyword & $" {method}{typeParams}({args})" + "; }"
                        Else
                            cs = $"@{method}{typeParams}({args})"
                        End If
                End Select
            Next

            invoke.ReplaceWith(AddToCsList(cs, invoke))

            ParseInvokes()
        End If
    End Sub

    Private Sub ParseLambdas()
        Dim lambda = (From elm In Xml.Descendants()
                      Where elm.Name = lambdaTag)?.FirstOrDefault

        If lambda IsNot Nothing Then
            Dim args = ""
            Dim sb As New Text.StringBuilder()
            For Each arg In lambda.Attributes
                Dim name = arg.Name.ToString()
                If name <> returnAttr Then
                    Dim var = convVars(arg.Value)
                    If name.EndsWith("." & typeAttr) Then name = name.Substring(0, name.Length - 5)
                    If var = "var" OrElse var = "" Then
                        sb.Append($"{name}, ")
                    Else
                        sb.Append($"{var} {name}, ")
                    End If
                End If
            Next
            args = sb.Remove(sb.Length - 2, 2).ToString()

            Dim _return = ""
            If lambda.Nodes.Count > 0 AndAlso TypeOf lambda.Nodes(0) Is XElement Then
                _return = ParseNestedInvoke(lambda.Nodes(0))
            Else
                _return = At(If(lambda.Attribute(returnAttr)?.Value, lambda.Value))
            End If

            Dim cs = ""
            If args.IndexOfAny({","c, " "c}) > -1 Then
                cs = $"({args}) => {_return}"
            Else
                cs = $"{args} => {_return}"
            End If

            lambda.ReplaceWith(AddToCsList(cs))
            ParseLambdas()
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

    Function ParseNestedInvoke(x As XElement) As String
        Dim CsBlock() As Char = {"@"c, "{"c, "}"c, ";", " "c, CChar(vbCr), CChar(vbLf)}
        Dim z = <zml/>
        z.Add(x)
        Dim result = New Zml().ParseZml(z).Trim(CsBlock)
        If result.StartsWith("<zmlitem") Then
            Dim n = CInt(result.Substring(8, result.Length - 11))
            CsCode(n) = CsCode(n).Trim(CsBlock)
        End If
        Return result
    End Function

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
