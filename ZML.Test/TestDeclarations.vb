Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Vazor

<TestClass>
Public Class TestDeclarations

    <TestMethod>
    Sub TestDeclarations()
        Dim x = <zml xmlns:z="zml">
                    <z:declare
                        d="4/1/2019"
                        d2="#4/1/2019#"
                        n="3"
                        s='"3"'
                        y="@arr[3]"
                        z='@dict["key"]'
                        myChar="'a'"
                        name="student"
                        obj="@Student"/>
                </zml>

        Dim y = x.ParseZml()
        Dim z =
$"@{{
  var d = {Qt}4/1/2019{Qt};
  var d2 = DateTime.Parse({Qt}4/1/2019{Qt}, new System.Globalization.CultureInfo({Qt}en-US{Qt}));
  var n = 3;
  var s = {Qt}3{Qt};
  var y = arr[3];
  var z = dict[{Qt}key{Qt}];
  var myChar = 'a';
  var name = {Qt}student{Qt};
  var obj = Student;
}}"

        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:declare var="arr" value="new String(){}"/>
            </zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ var arr = {Qt}new String(){{}}{Qt}; }}")

        x = <zml xmlns:z="zml">
                <z:declare var="arr" value="@new String[]{}"/>
            </zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ var arr = new String[]{{}}; }}")

        x = <zml xmlns:z="zml">
                <z:declare var="Name" key="Adam">dict</z:declare>
            </zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ var Name = dict[{Qt}Adam{Qt}]; }}")

        x = <zml xmlns:z="zml">
                <z:declare var="Name" value="dict" key="Adam"/>
            </zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ var Name = dict[{Qt}Adam{Qt}]; }}")

        x = <zml xmlns:z="zml">
                <z:declare var="Name" key="@Adam">@dict</z:declare>
            </zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ var Name = dict[Adam]; }}")

        x = <zml xmlns:z="zml">
                <z:declare var="Sum">
                    <z:lambda a.type="int" b.type="integer" return="a + b"/>
                </z:declare>
            </zml>
        y = x.ParseZml()

        Assert.AreEqual(y, $"@{{ var Sum = (int a, int b) => a + b; }}")


        ' Test Types
        ' ------------------------
        x = <zml xmlns:z="zml"><z:declare var="arr" type="int" value="new String(){}"/></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ int arr = {Qt}new String(){{}}{Qt}; }}")

        x = <zml xmlns:z="zml"><z:declare var="arr" type="Int32" value="@new String[]{}"/></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ Int32 arr = new String[]{{}}; }}")

        x = <zml xmlns:z="zml"><z:declare var="Name" type="Integer" key="Adam">dict</z:declare></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ int Name = dict[{Qt}Adam{Qt}]; }}")

        x = <zml xmlns:z="zml"><z:declare var="Name" type="Long" value="dict" key="Adam"/></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ long Name = dict[{Qt}Adam{Qt}]; }}")

        x = <zml xmlns:z="zml"><z:declare var="Name" type="List(Of Single, UInteger)" key="@Adam">@dict</z:declare></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ List<float, uint> Name = dict[Adam]; }}")

    End Sub

    <TestMethod>
    Sub TestSetters()
        Dim x = <zml xmlns:z="zml">
                    <z:set
                        d="4/1/2019"
                        d2="#4/1/2019#"
                        n="3"
                        s='"3"'
                        y="@arr[3]"
                        z='@dict["key"]'
                        myChar="'a'"
                        name="student"
                        obj="@Student"/>
                </zml>

        Dim y = x.ParseZml()
        Dim z =
$"@{{
  d = {Qt}4/1/2019{Qt};
  d2 = DateTime.Parse({Qt}4/1/2019{Qt}, new System.Globalization.CultureInfo({Qt}en-US{Qt}));
  n = 3;
  s = {Qt}3{Qt};
  y = arr[3];
  z = dict[{Qt}key{Qt}];
  myChar = 'a';
  name = {Qt}student{Qt};
  obj = Student;
}}"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:set object="arr" value="new String(){}"/>
            </zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ arr = {Qt}new String(){{}}{Qt}; }}")

        x = <zml xmlns:z="zml"><z:set object="arr" value="@new String[]{}"/></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ arr = new String[]{{}}; }}")

        x = <zml xmlns:z="zml"><z:set object="dict" key="Name">Adam</z:set></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ dict[{Qt}Name{Qt}] = {Qt}Adam{Qt}; }}")

        x = <zml xmlns:z="zml"><z:set object="dict" key="Name" value="Adam"/></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ dict[{Qt}Name{Qt}] = {Qt}Adam{Qt}; }}")

        x = <zml xmlns:z="zml">
                <z:set object="dict" key="@Name">@Adam</z:set>
            </zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@{{ dict[Name] = Adam; }}")

        x = <zml xmlns:z="zml">
                <z:set object="Sum">
                    <z:lambda a.type="int" b.type="integer" return="a + b"/>
                </z:set>
            </zml>
        y = x.ParseZml()

        Assert.AreEqual(y, $"@{{ Sum = (int a, int b) => a + b; }}")

    End Sub

    <TestMethod>
    Sub TestGetters()
        Dim x = <zml xmlns:z="zml"><z:get>X</z:get></zml>
        Dim y = x.ParseZml()
        Assert.AreEqual(y, "@X")

        x = <zml xmlns:z="zml">
                <z:get object="X"/>
            </zml>

        y = x.ParseZml()
        Assert.AreEqual(y, "@X")

        x = <zml xmlns:z="zml"><z:get object="X" key="@i"/></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, "@X[i]")

        x = <zml xmlns:z="zml"><z:get object="X" key="name"/></zml>
        y = x.ParseZml()
        Assert.AreEqual(y, $"@X[{Qt}name{Qt}]")

    End Sub

    <TestMethod>
    Sub TestQuotesInAttr()
        Dim x = <zml xmlns:z="zml">
                    <input type="hidden" name="Items[''@i''].Key" value="@item.Id"/>
                    <input type="number" class="esh-basket-input" min="1" name="Items[''@i''].Value" value="@item.Quantity"/>
                </zml>

        Dim y = x.ParseZml()
        Dim z =
$"<input type={Qt}hidden{Qt} name='Items[{Qt}@i{Qt}].Key' value={Qt}@item.Id{Qt} />
<input type={Qt}number{Qt} class={Qt}esh-basket-input{Qt} min={Qt}1{Qt} name='Items[{Qt}@i{Qt}].Value' value={Qt}@item.Quantity{Qt} />"

        Assert.AreEqual(y, z)
    End Sub

End Class
