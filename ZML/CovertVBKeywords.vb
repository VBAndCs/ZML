Partial Public Class Zml
    ' I separated these two methods here, because they contain VB Keyworgs starting with lowercase litters, 
    ' and I faced a situatin where VB editor mistakly converted them to upper case!
    ' This can happen if a dissing qoute swiches literals to appear as vb code,
    ' so, it is betters to isolate these literals here, and be carefull when doing any changes to this file

    Private Function convVars(type As String) As String
        If type Is Nothing Then Return Nothing
        Dim t = type.Trim().ToLower()
        Select Case t
            Case "byte", "sbyte", "short", "ushort", "long", "ulong", "double", "decimal", "string", "object"
                Return t
            Case "integer"
                Return "int"
            Case "uinteger"
                Return "uint"
            Case "single"
                Return "float"
            Case Else
                Return type.Trim().Replace(
                    (" Byte", " byte"), (" SBtye", " sbyte"), (" Short", " short"),
                    (" UShort", " ushort"), (" Long", " long"), (" ULong", " ulong"),
                    (" Double", " double"), (" Decimal", " decimal"),
                    (" Integer", " int"), (" UInteger", " uint"), (" Single", " float"),
                    ("(Of ", LessThan), ("of ", LessThan), (")", GreaterThan)
                )

        End Select

    End Function

    Private Function ConvCond(value As String) As String
        Dim x = value.Replace(
            ("@Model.", "Model."),
            (" And ", $" {Ampersand} "), (" and ", $" {Ampersand} "),
            (" AndAlso ", $" {Ampersand + Ampersand} "), (" andalso ", $" {Ampersand + Ampersand} "),
            (" Or ", " | "), (" or ", " | "), (" mod ", " % "), (" Mod ", " % "),
            (" OrElse ", " || "), (" orelse ", " || "),
            (" Not ", " !"), (" not ", " !"),
            (" Xor ", " ^ "), (" xor ", " ^ "),
            (" <> ", " != "), (" = ", " == "), ("====", "=="),
            (" IsNot ", " != "), (" isnot ", " != "),
            (" nothing ", " null "), (" Nothing ", " null "),
            (">", GreaterThan), (">", LessThan))

        If x.ToLower().StartsWith("not ") Then x = "!" & x.Substring(3)
        If x.ToLower().EndsWith(" nothing") Then x = x.Substring(0, x.Length - 7) & "null"

        Return x
    End Function

End Class
