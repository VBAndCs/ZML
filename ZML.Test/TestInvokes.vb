Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ZML


<TestClass>
    Public Class TestInvokes

    <TestMethod>
    Sub TestInvokes()
        Dim x = <zml xmlns:z="zml">
                    <z:invoke method="Foo">
                        <z:typeparam>Integer</z:typeparam>
                        <z:arg>3</z:arg>
                        <z:arg>'a'</z:arg>
                        <z:arg>Ali</z:arg>
                        <z:lambda m.type="var" return="m.Name"/>
                        <z:lambda n.type="Integer" return="n + 1"/>
                        <z:lambda x.type="int" y.type="int" return="x + y"/>
                        <z:lambda a="Double" b="Single" return="a + b"/>
                    </z:invoke>
                </zml>

        Dim y = x.ParseZml()
        Dim z = $"@Foo<int>(3, 'a', {Qt}Ali{Qt}, m => m.Name, (int n) => n + 1, (int x, int y) => x + y, (double a, float b) => a + b)"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:invoke method="RenderSection">
                    <z:arg>Scripts</z:arg>
                    <z:arg name="required">false</z:arg>
                </z:invoke>
            </zml>

        y = x.ParseZml()
        z = "@RenderSection('Scripts', required: false)".Replace(SnglQt, Qt)
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:await method="Foo">
                    <z:typeparam>Integer</z:typeparam>
                    <z:typeparam>UInteger</z:typeparam>
                    <z:arg>3</z:arg>
                    <z:arg>'a'</z:arg>
                    <z:arg>Ali</z:arg>
                    <z:lambda m="" return="m.Name"/>
                    <z:lambda n.type="Integer" return="n + 1"/>
                    <z:lambda x.type="int" y.type="int" return="x + y"/>
                    <z:lambda a="Double" b="Single" return="a + b"/>
                </z:await>
            </zml>

        y = x.ParseZml()
        z = "@{ await " & $"Foo<int, uint>(3, 'a', {Qt}Ali{Qt}, m => m.Name, (int n) => n + 1, (int x, int y) => x + y, (double a, float b) => a + b);" & " }"
        Assert.AreEqual(y, z)
    End Sub

    <TestMethod>
        Sub TestNestedInvokes()
            Dim x = <zml xmlns:z="zml">
                        <z:invoke method="Foo">
                            <z:arg>3</z:arg>
                            <z:arg>
                                <z:invoke method="RenderSection">
                                    <z:arg>Scripts</z:arg>
                                    <z:arg name="required">false</z:arg>
                                </z:invoke>
                            </z:arg>
                            <z:arg>Ali</z:arg>
                            <z:lambda m="var">
                                <z:invoke property="m.Name"/>
                            </z:lambda>
                            <z:lambda x.type="int" y.type="int">
                                <z:invoke method="Test">
                                    <z:arg>@x</z:arg>
                                    <z:arg>@y</z:arg>
                                    <z:lambda a="Double" b="Single" return="a + b"/>
                                </z:invoke>
                            </z:lambda>
                            <z:await method="Foo2">
                                <z:arg>false</z:arg>
                                <z:arg>Ali</z:arg>
                            </z:await>
                        </z:invoke>
                    </zml>

            Dim y = x.ParseZml()
            Dim z =
"@Foo(
3, 
RenderSection('Scripts', required: false), 
'Ali', 
m => m.Name, 
(int x, int y) => Test(x, y, (double a, float b) => a + b), 
await Foo2(false, 'Ali'))".Replace((SnglQt, Qt), (vbCrLf, ""))

            Assert.AreEqual(y, z)

        End Sub

        <TestMethod>
        Sub TestDots()
            Dim x = <zml xmlns:z="zml">
                        <z:set object="hasExternalLogins">
                            <z:dot>
                                <z:await method="SignInManager.GetExternalAuthenticationSchemesAsync"/>
                                <z:invoke method="Any"/>
                            </z:dot>
                        </z:set>
                    </zml>

            Dim y = x.ParseZml()
            Dim z = "@{ hasExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).Any(); }"

            Assert.AreEqual(y, z)
        End Sub

    <TestMethod>
    Sub TestRawLambdas()

        Dim x = <zml xmlns:z="zml">
                    <z:invoke method="Foo">
                        <z:arg>Fn(m) => m.Name</z:arg>
                        <z:arg>Fn(n as integer)=> n+1</z:arg>
                        <z:arg>fn(x, y)=>x+y</z:arg>
                        <z:arg>fn(a as string, b as string) =>a + b</z:arg>
                        <z:arg>fn(x, y)=>myfn(x, Fn(t) => t + y)</z:arg>
                    </z:invoke>
                </zml>

        Dim y = x.ParseZml()
        Dim z = $"@Foo(m => m.Name, (int n) => n+1, (x, y) => x+y, (string a, string b) => a + b, (x, y) => myfn(x, t => t + y))"
        Assert.AreEqual(y, z)


    End Sub

    <TestMethod>
    Sub TestRawLambdaRawText()
        Dim x =
<zml xmlns:z="zml">
    @Foo(Fn(m as List(Of integer, Dictionary(of String, Single))) => m.ToString())
    <p>Foo(Fn(n as integer)=> n+1)</p>
    <z:if condition='foo(fn(x, y)=>x+y, 10)'>
    </z:if>
    <z:invoke method='Foo(3, fn(a As String, b As String) =>a + b)'/>
    <z:declare f="fn(x, y)=>myfn(x, Fn(t) => t + y)"/>
    <div>@{ f2= x => x.Name; }</div>
    <z:declare f3="''Fn(int x) => x.Name''"/>
    <code>@{ f3= Fn(int x, int y) => x + y; }</code>
</zml>

        Dim y = x.ParseZml()
        Dim z =
"@Foo((List<int, Dictionary<string, float>> m) => m.ToString())
  <p>Foo((int n) => n+1)</p>
@if (foo((x, y) => x+y, 10))
{
}
@Foo(3, (string a, string b) => a + b)()
@{ var f = (x, y) => myfn(x, t => t + y); }
<div>@{ f2= x => x.Name; }</div>
@{ var f3 = 'Fn(int x) => x.Name'; }
<code>@{ f3= (int x, int y) => x + y; }</code>".Replace(SnglQt, Qt)
        Assert.AreEqual(y, z)
    End Sub

    <TestMethod>
    Sub TextInvokeLambda()
        Dim s = "<z:invoke method='Html.DisplayNameFor'>
                    <z:lambda m return='m.Name'/>
                </z:invoke>".Replace(SnglQt, Qt)

        Dim y = s.ParseZml()
        Dim z = "@Html.DisplayNameFor(m => m.Name)"
        Assert.AreEqual(y, z)

    End Sub
End Class
