﻿Partial Public Class Zml
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
                Dim st = type.Length - 1
                Do
                    st = type.LastIndexOf("(of ", st, StringComparison.InvariantCultureIgnoreCase)
                    If st = -1 Then Return type
                    Dim en = type.IndexOf(")", st)
                    Dim subTypes = type.Substring(st + 4, en - st - 4).Split({", "}, StringSplitOptions.RemoveEmptyEntries)
                    Dim sb As New Text.StringBuilder()
                    For Each subtype In subTypes
                        sb.Append(convVars(subtype))
                        sb.Append(tempComma)
                        sb.Append(" ")
                    Next
                    If sb.Length > 0 Then sb.Remove(sb.Length - tempComma.Length - 1, tempComma.Length + 1)
                    type = type.Substring(0, st) + LessThan + sb.ToString + GreaterThan + type.Substring(en + 1)
                    st = st - 1
                Loop

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
