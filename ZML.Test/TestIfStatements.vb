Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Vazor

<TestClass>
Public Class TestIfStatements

    <TestMethod>
    Sub TestIf()

        Dim x =
                    <zml xmlns:z="zml">
                        <z:if condition=<%= "a>3 and y<5" %>>
                            <p>a = 4</p>
                        </z:if>
                    </zml>

        Dim y = x.ParseZml()
        Dim z =
"@if (a>3 & y<5)
{
  <p>a = 4</p>
}"

        Assert.AreEqual(y, z)

    End Sub

    <TestMethod>
    Sub TestIfElse()
        Dim x =
                <zml xmlns:z="zml">
                    <z:if condition=<%= "a <> 3 andalso b == 5" %>>
                        <z:then>
                            <p>test 1</p>
                        </z:then>
                        <z:else>
                            <p>test 2</p>
                        </z:else>
                    </z:if>
                </zml>

        Dim y = x.ParseZml()
        Dim z =
"@if (a != 3 && b == 5)
{
  <p>test 1</p>
}
else
{
  <p>test 2</p>
}"

        Assert.AreEqual(y, z)

    End Sub

    <TestMethod>
    Sub TestElseIfs()
        Dim x =
                <zml xmlns:z="zml">
                    <z:if condition=<%= "grade < 30" %>>
                        <z:then>
                            <p>Very weak</p>
                        </z:then>
                        <z:elseif condition=<%= "grade < 50" %>>
                            <p>Weak 2</p>
                        </z:elseif>
                        <z:elseif condition=<%= "grade < 65" %>>
                            <p>Accepted</p>
                        </z:elseif>
                        <z:elseif condition=<%= "grade < 75" %>>
                            <p>Good</p>
                        </z:elseif>
                        <z:elseif condition=<%= "grade < 85" %>>
                            <p>Very Good</p>
                        </z:elseif>
                        <z:else>
                            <p>Excellent</p>
                        </z:else>
                    </z:if>
                </zml>

        Dim y = x.ParseZml()
        Dim z =
"@if (grade < 30)
{
  <p>Very weak</p>
}
else if (grade < 50)
{
  <p>Weak 2</p>
}
else if (grade < 65)
{
  <p>Accepted</p>
}
else if (grade < 75)
{
  <p>Good</p>
}
else if (grade < 85)
{
  <p>Very Good</p>
}
else
{
  <p>Excellent</p>
}"

        Assert.AreEqual(y, z)

    End Sub

    <TestMethod>
    Sub TestNestedIfs()
        Dim x = <zml xmlns:z="zml">
                    <z:if condition="@Model.Count = 0">
                        <z:then>
                            <z:if condition="Model.Test">
                                <z:then>
                                    <p>Test</p>
                                </z:then>
                                <z:else>
                                    <p>Not Test</p>
                                </z:else>
                            </z:if>
                        </z:then>
                        <z:else>
                            <h1>Show Items</h1>
                            <z:foreach var="item" in="Model">
                                <z:if condition="item.Id mod 2 = 0">
                                    <z:then>
                                        <p class="EvenItems">item.Name</p>
                                    </z:then>
                                    <z:else>
                                        <p class="OddItems">item.Name</p>
                                    </z:else>
                                </z:if>
                            </z:foreach>
                            <p>Done</p>
                        </z:else>
                    </z:if>
                </zml>

        Dim y = x.ParseZml()
        Dim z =
"@if (Model.Count == 0)
{
  if (Model.Test)
  {
    <p>Test</p>
  }
  else
  {
    <p>Not Test</p>
  }
}
else
{
  <h1>Show Items</h1>
  foreach (var item in Model)
  {
    if (item.Id % 2 == 0)
    {
      <p class=" + Qt + "EvenItems" + Qt + ">item.Name</p>
    }
    else
    {
      <p class=" + Qt + "OddItems" + Qt + ">item.Name</p>
    }
  }
  <p>Done</p>
}"

        Assert.AreEqual(y, z)

    End Sub

    <TestMethod>
    Sub TestChecks()
        Dim x = <zml xmlns:z="zml">
                    <p>
                        <z:check condition="Model.StartsWith(''Error'')"
                            iftrue="danger" iffalse="success"/>
                    </p>
                </zml>
        Dim y = x.ParseZml()
        Dim z =
$"<p>
  @Model.StartsWith({Qt}Error{Qt}) ? {Qt}danger{Qt} : {Qt}success{Qt}
</p>"

        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:declare var="x">
                    <z:check condition="@Error"
                        ifnull="success"/>
                </z:declare>
            </zml>

        y = x.ParseZml()
        z = $"@{{ var x = Error ?? {Qt}success{Qt}; }}"
        Assert.AreEqual(y, z)

    End Sub

    <TestMethod>
    Sub TestConditions()
        Dim x =
$"<zml xmlns:z={Qt}zml{Qt}>
       <z:if condition={Qt}a>0 And b <10 And c>=10 And d<= 3{Qt}>
            <z:for i={Qt}0{Qt} while={Qt}i<count{Qt} let={Qt}i++{Qt}>

            </z:for>
       </z:if>
</zml>"

        Dim y = x.ParseZml()
        Dim z =
"@if (a>0 & b <10 & c>=10 & d<= 3)
{
  for (var i = 0; i<count; i++)
  {
  }
}"
        Assert.AreEqual(y, z)
    End Sub

End Class
