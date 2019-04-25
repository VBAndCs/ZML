# ZML: The XML Razor
ZML is a tagged programming language designed to create Razor Views/Pages that do not have to contain any C#, VB.NET or F# code, so the Razor entirely looks like a normal XML file, that is valid to use in any ASP.NET app regardless of the programming language it uses.

# Install ZML NuGet:
Use this command line in Pachage Manager
```
Install-Package ZML.1.0 -Version 1.0.0 
```

**How does ZML work**:
you can use ZML in two ways:
1. add .zml files to your project (and choose to open them with the HTML editor), then call the `zml.ZmlPages.Compile()` method in the `Startup.Configure()` method. This will search for all .zml files in your project, and generate the cshtml files from them. This process will only include .zml files that have been modified since last compilation. Using auto generated cshtml files grantees that your app works as a standard Razor app, giving you all control over razor options, including using pre compiled cshtml files.

2. You can use zml with a virtual file provider, to deliver the cshtml content to the Razor Engine in runtime, so there will be no cshtml files in the project. To get the compiled CS string, Use the `ParseZML` method, which is an extension method to both String and XElement classes.
In fact, I first crated ZML to be used within vbxml code in [Vazor project](https://github.com/VBAndCs/Vazor-DotNetCore2) (which uses a virtual file provider), to solve some difficulties in using For Each loops in vbxml in some cases.

Note that ZML is built on top of Razor engine. This means:
1. You don't have to worry about future versions of Razor, because ZML will implicitly use them.

2. Your project can contain .zml files beside .cshtml file (other than the auto generated ones), such as cshtml files that do not contain any C# code.

3. You can still use C# code inside .zml file. This is useful if you miss a ZML tag to generate that C# code, or if the ZML tag expression is much longer than the C# code!

4. In Vazor apps, you can use a vbxml code that contains one or more of these pieces:
a. Embedded VB.NET code.
b. ZML tags.
c. C# razor code!

# Some Basic ZML Syntax Rules:
I need to establish some ground rules here before listing ZML tags in the next sections:
 
1. ZML is an XML not HTML, so, it will not accept any broken HTML structure. Make sure you closed all html tags.

2. You can use >, < and & symbols directly in ZML code, as it handles them internally. Don't encode these symbols unless you intend to view them in the browser. But note that using these symbols in vbxml literals is not valid, unless you embed the string containing them in ` <%=  %> quotes` to force VB to encode them for you.

3. Always use double quotes `"abc"` around ZML attribute values, and if those values text contain double quotes, use double single quotes to represent them (an ' followed by ') such as `" X=''abc'' "`. ZML will compile this to `' X="abc" '` in the cshtml file. But don't use the last form yourself, because the XML parser will convert the single quotes to double quotes and you will end with `" X="abc" "` which will cause errors if you opened the cshtml file with HTML editor!

4. Write numerics, chars and booleans directly as attribute values such as `x="3"`, `c="'a'"` and `flag="false"`. This will generate `x = 3`, `c = 'a'` and `flag = false` in C#. 

5. if you want to write numeric and Boolean values as strings, you must enclose then with double single quotes such as `x="''3''"` and `flag="''3''"`. This will generate `x = "3"` and `flag = "false"` in C#.  

6. Enclose date and time values with `##` notation such as `d="#1/1/2019#"`. This will generate `d = DateTime.Parse("1/1/2019", new System.Globalization.CultureInfo("en-US"))` in C#. It is clear that you must use the English-United stated culture when writing the date format inside the `##`.

7. Use @ before variable names in attribute value, such as `s="@Name"` which will generate `s = Name` in the C# file.

8. Any value except the above cases will be considered a string value such as `s="Name"`, which will generate `s = "Name"` in the C# file.

9. I tried to make ZML syntax familiar to both VB.NET and C# programmers, so:
a. You can use the basic type names of either VB or C#. For example, the attribute `type="Integer"` is an alternative equivalent for `type="int"`,

b. You can use either VB or C# comparison expressions. For example the attribute `condition="a <> b andalso c = 3"` is an alternative equivalent for `condition="a != b && c = 3"`.

c. You can use either VB or C# generic notation. For example the attribute `type="Enumerable(Of Integer)"` is an alternative equivalent for `type="Enumerable<int>"`, but note that the last expression can be used in .zml files but not in vbxml literals, where you need to use this workaround: `type=<%= "Enumerable<int>" %>`.
 
But on the other hand, you have to use the C# indexer square brackets `[ ]` instead of VB brackets `( )` when you deal with collection items.

# ZML Tags:
ZML contains the most needed tags to represent basic C# statements, Razor statements and some commonly uses tag helpers. 
All ZML tags belongs to the `z:` namespace (such as `<z:text>`) to avoid any confliction with HTML tags (such as `<text>`). You don't have to define the `z:` namespace in .zml files, because ZML does that internally, but you have to define it in vbxml code like this:
```xml
<zml xmlns:z="zml">
  <!--add your ZML tages inside this block-->
</zml>
```
I will list ZML tags in brief, and you can look at the eShopOnWeb_zml and the ZML.Test projects, to see examples on how to uses them. I will provide a full documentation later, but this needs time.

**`<z:using x />` / `<z:imports x />`**
Generates the `@using x` statement.

**`<z:namespace x />`**
Generates the `@namespace x` statement.

**`<z:helpers x="*" />`**
Generates the `@addTagHelper *, x` statement.

**`<z:page [route="x"] />`**
Generates the `@page ["x"]` statement.

**`<z:model type="Integer" />`**
Generates the `@model int` statement.

**`<z:layout page="x" />`**
Generates the `@{Layout = "x";}` statement.

**`<z:inject x.type="int" />`**
Generates the `@inject int x` statement.

**`<z:title>test</z:title>`**
Generates the `@{ ViewData["Title"] = "test"; }` statement.

**`<z:title />`**
Generates the `@ViewData["Title"]` expression to read Title key.

**`<z:viewdata x="y"/>`**
Generates the `@{ ViewData["x"] = "y"; }` expression to read Title key.

**`<z:text>any text</z:text>`**
Generates the `@: any text` expression.

**`<z:comment>any block</z:comment>`**
Generates the `@* any block *@` razor comment.

**`<z:comment>block</z:comment>`**
Generates the `@* block *@` razor comment.

**`<z:section name="x">block</z:section>`**
Generates the `@section x { block }` razor block.

**`<z:displayfor var="x" return="x.Name" />`**
Generates the `@Html.DisplayFor(x => x.Name)` helper method.

**`<z:displaynamefor var="x" return="x.Name" />`**
Generates the `@Html.DisplayNameFor(x => x.Name)` helper method.

**`<z:declare x="3"/> `**
Generates the `@{ var x=3; }` expression.

**`<z:set x="3"/> `**
Generates the `@{ x=3; }` expression.

**`<z:get object="x"/> `**
Generates the `@x` expression, which you can use directly instead of using the get tag! but the get tag maybe useful when you read an indexed value, for example: `<z:get object="x" key="0"/> ` generates: `@x[0]`.
Note: You can use the key attribute also with viewdata, declare and set attributes.

**`<z:check condition="x" ifnull="y" />`**
Generates the `x ?? y` expression.

**`<z:if></z:if >`**
Generates if statements.. In the simplest form:
```xml
<z:if condition="cond">
   <!-- when cond is true -->
</z:if >
```

And you can also have `else if` and `else` statements, in the form:
```xml
<z:if condition="cond1">
   <z:then>
      <!-- when cond1 is true -->
   </z:then>

   <z:elseif condition="cond2">
      <!-- when cond2 is true -->
   </z:elseif>

   <z:elseif condition="cond3">
      <!-- when cond3 is true -->
   </z:elseif>

   <z:else>
      <!-- when no condition is true -->
   </z:else>

</z:if >
```

**`<z:foreach></z: foreach>`**
Generates `foreach` loop statement.. It has the following form:
```xml
<z:foreach type="int" var="x" in="obj">
      <!-- add code or html tags here -->
</z:foreach>
```

You can omit the `type` attribute so `var` will be used to infer the variable type. 

**`<z:for></z: for>`**
Generates `for` loop statement.. It has two different forms:
1. VB.NET alike form:
```xml
<z:for type="Integer" i="0" to="10" step="2">
      <!-- add code or html tags here -->
</z:for>
```

You can omit the `type` attribute so `var` will be used to infer the variable type. You can also omit the step attribute so step="1" is assumed.

2. C# alike form:
```xml
<z:for type="int" i="0" while="i < 9" let="i++">
     <!-- add code or html tags here -->
</z:for>
```

You can omit the `type` attribute so `var` will be used to infer the variable type. You can also omit the let attribute so `let="i++"` is assumed in the last example.

Note:
The while form of `<z:for>` tag can do the job of `do` and `while` loops in both VB and C#. Just use `let="i += 0"`. Ex:
```xml
<z:for i="0" while="i < 9" let="i += 0">
     <z:set i = "GetNext(i)" />
</z:for>
```

but if `i` is defined outside the loop, you'd better use the `<z:while>` tag.

**`<z:while></z:while>`**:
It has the form:
```xml
<z:while [condition="cond"]>
     <!-- add code or html tags here -->   
</z:while>
```

**`<z:exit />`, `<z:break />` and `<z:continue />`**:
Use thes tags inside loops.
Both `<z:exit />`, `<z:break />` generate the `break` statement.
`<z:continue />` generates the `continue` statement.
Note that you can control which loop to exit or continue. To do that, add the label attribute in the loop tag with a unique name, and use this name in the label attribute in the exit, break or continue tags. Ex:
```xml
<z:for c="0" to="10" label="for1">
    <z:declare i="0"/>
    <z:while condition="i<=10">
        <p>i</p>
        <z:set i="@i+1"/>
        <z:if condition="i = c">
            <z:continue label="for1"/>
        </z:if>
    </z:while>
</z:for>
```

The trick here is that the `label="for1"` attribute will generate two labels in C# code:
1. `continue_for1:` label, added as the last line in the loop body, so jumping to this label will escape the current iteration and continues the next one.
2. `break_for1:` label, added at the first line after loop closing bracket, so jumping to this label will break the loop.

Now, when you use the this label in break or exit tags, this generates a `goto break_for1` statement, and if you use it in continue tag, it generates a `goto continue_for1` statement. This is the generated code from last sample:
```C#
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
break_for1:
```

**`<z:invoke> </z:invoke>`**
generates expressions for getting properties, indeesrs and method call values. In simple cases like `@x.Trim()`, you don't need to use this tag like this: `<z:invoke method="x.Trim" />`! But invoke tag can be useful when the method have complex params like lambda expressions that you don't like to write them with C# syntax! For example:
```xml
<z:invoke method="Foo">
   <z:typeparam>Integer</z:typeparam>
      <z:arg>3</z:arg>
      <z:arg>Adam</z:arg>
      <z:lambda x.type="Integer" y.type="Integer" return="x + y"/>
</z:invoke>
```

which will generate: `@Foo(3, "Adam", (int x, int y) => x + y) `
It is obvious that using the invoke tag is longer than using the c# syntax directly, so, you can choose to use the later directly.
Note that if you need to use another method return value as a param for the invoked method, you can use nested invokes directly, or inside arg tags.
You can also have named args, by using the name attribute of the arg tag, such as:
`<z:arg name="required">false</z:arg>`
which will generate the argument: `required: false`

**`<z:lambda m>m.Name</z: lambda>`**
Generates the lambda expression `m => m.Name` that can be used inside invoke, declare or set tags. You can use the return attribute to set the labda return value, or you can set it as the content of the lambda tag so that you can use another invokes or lambdas.
