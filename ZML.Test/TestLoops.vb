Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Vazor

<TestClass>
Public Class TestLoops

    <TestMethod>
    Sub TestForLoop()
        Dim lp =
                <zml xmlns:z="zml">
                    <z:for i="0" to="10">
                        <p>@i</p>
                    </z:for>
                </zml>

        Dim y = lp.ParseZml()
        Dim z =
"@for (var i = 0; i < 10 + 1; i++)
{
  <p>@i</p>
}"
        Assert.AreEqual(y, z)


        lp = <zml xmlns:z="zml">
                 <z:for i="0" to="@Model.Count - 1">
                     <p>@i</p>
                 </z:for>
             </zml>

        y = lp.ParseZml()
        z =
"@for (var i = 0; i < Model.Count; i++)
{
  <p>@i</p>
}"

        Assert.AreEqual(y, z)

        lp = <zml xmlns:z="zml">
                 <z:for i="0" while=<%= "i < Model.Count" %>>
                     <p>@i</p>
                 </z:for>
             </zml>

        y = lp.ParseZml()
        z =
"@for (var i = 0; i < Model.Count; i++)
{
  <p>@i</p>
}"

        Assert.AreEqual(y, z)

        lp = <zml xmlns:z="zml">
                 <z:for i="0" while=<%= "i > Model.Count" %> let="i -= 2">
                     <p>@i</p>
                 </z:for>
             </zml>

        y = lp.ParseZml()
        z =
"@for (var i = 0; i > Model.Count; i -= 2)
{
  <p>@i</p>
}"

        Assert.AreEqual(y, z)
    End Sub

    <TestMethod>
    Sub TestForLoopSteps()
        Dim lp = <zml xmlns:z="zml">
                     <z:for i="0" to="10" step="2">
                         <p>@i</p>
                     </z:for>
                 </zml>

        Dim y = lp.ParseZml()
        Dim z =
"@for (var i = 0; i <10 + 1; i += 2)
{
  <p>@i</p>
}"

        lp = <zml xmlns:z="zml">
                 <z:for type="Integer" i="@Model.Count - 1" to="0" step="-1">
                     <p>@i</p>
                 </z:for>
             </zml>

        y = lp.ParseZml()
        z =
"@for (int i = Model.Count - 1; i > -1; i--)
{
  <p>@i</p>
}"

        Assert.AreEqual(y, z)

        lp = <zml xmlns:z="zml">
                 <z:for type="Byte" i="@Model.Count - 1" to="0" step="-2">
                     <p>@i</p>
                 </z:for>
             </zml>

        y = lp.ParseZml()
        z =
"@for (byte i = Model.Count - 1; i > -1; i -= 2)
{
  <p>@i</p>
}"

        Assert.AreEqual(y, z)
    End Sub


    <TestMethod>
    Sub TestForEachLoop()
        Dim lp = <zml xmlns:z="zml">
                     <z:foreach var="i" in='"abcd"'>
                         <p>@i</p>
                     </z:foreach>
                 </zml>


        Dim y = lp.ParseZml()
        Dim z =
$"@foreach (var i in {Qt}abcd{Qt})
{{
  <p>@i</p>
}}"
        Assert.AreEqual(y, z)

        lp = <zml xmlns:z="zml">
                 <z:foreach type="Integer" var="i" in='"abcd"'>
                     <p>@i</p>
                 </z:foreach>
             </zml>


        y = lp.ParseZml()
        z =
$"@foreach (int i in {Qt}abcd{Qt})
{{
  <p>@i</p>
}}"
        Assert.AreEqual(y, z)

        lp = <zml xmlns:z="zml">
                 <z:foreach i="" in="''abcd''">
                     <p>@i</p>
                 </z:foreach>
             </zml>


        y = lp.ParseZml()
        z =
$"@foreach (var i in {Qt}abcd{Qt})
{{
  <p>@i</p>
}}"
        Assert.AreEqual(y, z)
    End Sub

    <TestMethod>
    Sub TestNestedForEachLoops()

        Dim lp = <zml xmlns:z="zml">
                     <z:foreach var="country" in="Model.Countries">
                         <h1>Country: @country</h1>
                         <z:foreach var="city" in="country.Cities">
                             <p>City: @city</p>
                         </z:foreach>
                     </z:foreach>
                 </zml>


        Dim y = lp.ParseZml()
        Dim z =
"@foreach (var country in Model.Countries)
{
  <h1>Country: @country</h1>
  foreach (var city in country.Cities)
  {
    <p>City: @city</p>
  }
}"

        Assert.AreEqual(y, z)
    End Sub


    <TestMethod>
    Sub TestlabeledLoops()

        Dim lp = <zml xmlns:z="zml">
                     <z:foreach var="country" in="Model.Countries" label="loop1">
                         <h1>Country: @country</h1>
                         <z:foreach var="city" in="country.Cities">
                             <z:if condition="city.StartsWith(''_'')">
                                 <z:then>
                                     <z:exit label="loop1"/>
                                 </z:then>
                                 <z:else>
                                     <z:exit/>
                                 </z:else>
                             </z:if>
                             <p>City: @city</p>
                         </z:foreach>
                     </z:foreach>
                 </zml>


        Dim y = lp.ParseZml()
        Dim z =
"@foreach (var country in Model.Countries)
{
  <h1>Country: @country</h1>
  foreach (var city in country.Cities)
  {
    if (city.StartsWith('_'))
    {
      goto break_loop1;
    }
    else
    {
      break;
    }
    <p>City: @city</p>
  }
  continue_loop1:
}
break_loop1:".Replace(SnglQt, Qt)

        Assert.AreEqual(y, z)
    End Sub

    <TestMethod>
    Sub TestWhileLoops()

        Dim x = <zml xmlns:z="zml">
                    <z:declare i="0"/>
                    <z:while>
                        <p>i</p>
                        <z:set i="@i+1"/>
                        <z:if condition="i>10">
                            <z:break/>
                        </z:if>
                    </z:while>
                </zml>

        Dim y = x.ParseZml()
        Dim z =
"@{ var i = 0; }
@while ()
{
  <p>i</p>
  i = i+1;
  if (i>10)
  {
    break;
  }
}"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:declare i="0"/>
                <z:while condition=<%= "i<=10" %>>
                    <p>i</p>
                    <z:set i="@i+1"/>
                </z:while>
            </zml>

        y = x.ParseZml()
        z =
"@{ var i = 0; }
@while (i<=10)
{
  <p>i</p>
  i = i+1;
}"
        Assert.AreEqual(y, z)


        x = <zml xmlns:z="zml">
                <z:for c="0" to="10" label="for1">
                    <z:declare i="0"/>
                    <z:while condition=<%= "i<=10" %>>
                        <p>i</p>
                        <z:set i="@i+1"/>
                        <z:if condition="i = c">
                            <z:continue label="for1"/>
                        </z:if>
                    </z:while>
                </z:for>
            </zml>

        y = x.ParseZml()
        z =
"@for (var c = 0; c < 10 + 1; c++)
{
  var i = 0;
  while (i<=10)
  {
    <p>i</p>
    i = i+1;
    if (i == c)
    {
      goto continue_for1;
    }
  }
  continue_for1:
}
break_for1:"

        Assert.AreEqual(y, z)
    End Sub
End Class
