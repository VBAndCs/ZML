Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Vazor


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

    End Class
