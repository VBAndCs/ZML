Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Vazor

<TestClass>
Public Class TestRazor

    <TestMethod>
    Sub TestImports()
        Dim x = <zml xmlns:z="zml">
                    <z:imports>Microsoft.eShopWeb.Web</z:imports>
                </zml>

        Dim y = x.ParseZml()
        Dim z = "@using Microsoft.eShopWeb.Web"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:using ns="Microsoft.eShopWeb.Web.ViewModels.Manage"/>
            </zml>

        y = x.ParseZml()
        z = "@using Microsoft.eShopWeb.Web.ViewModels.Manage"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:using Microsoft.eShopWeb.Web.Pages=""
                    Microsoft.AspNetCore.Identity=""
                    Microsoft.eShopWeb.Infrastructure.Identity=""/>
            </zml>

        y = x.ParseZml()
        z =
"@using Microsoft.eShopWeb.Web.Pages
@using Microsoft.AspNetCore.Identity
@using Microsoft.eShopWeb.Infrastructure.Identity"

        Assert.AreEqual(y, z)
    End Sub

    <TestMethod>
    Sub TestNamespace()
        Dim x = <zml xmlns:z="zml">
                    <z:namespace>Microsoft.eShopWeb.Web</z:namespace>
                </zml>

        Dim y = x.ParseZml()
        Dim z = $"@namespace Microsoft.eShopWeb.Web"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:namespace ns="Microsoft.eShopWeb.Web.ViewModels.Manage"/>
            </zml>

        y = x.ParseZml()
        z = $"@namespace Microsoft.eShopWeb.Web.ViewModels.Manage"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:namespace Microsoft.eShopWeb.Web.Pages=""
                    Microsoft.AspNetCore.Identity=""
                    Microsoft.eShopWeb.Infrastructure.Identity=""/>
            </zml>

        y = x.ParseZml()
        z =
"@namespace Microsoft.eShopWeb.Web.Pages
@namespace Microsoft.AspNetCore.Identity
@namespace Microsoft.eShopWeb.Infrastructure.Identity"

        Assert.AreEqual(y, z)

    End Sub

    <TestMethod>
    Sub TestAllImports()
        Dim x =
$"<z:imports>Microsoft.eShopWeb.Web</z:imports>
<z:imports ns={Qt}Microsoft.eShopWeb.Web.ViewModels{Qt} />
<z:using>Microsoft.eShopWeb.Web.ViewModels.Account</z:using>
<z:using ns={Qt}Microsoft.eShopWeb.Web.ViewModels.Manage{Qt} />
<z:using Microsoft.eShopWeb.Web.Pages
       Microsoft.AspNetCore.Identity
       Microsoft.eShopWeb.Infrastructure.Identity />
<z:namespace>Microsoft.eShopWeb.Web.Pages</z:namespace>
<z:helpers Microsoft.AspNetCore.Mvc.TagHelpers={Qt}*{Qt}/>"

        Dim y = x.ParseZml()
        Dim z =
"@using Microsoft.eShopWeb.Web
@using Microsoft.eShopWeb.Web.ViewModels
@using Microsoft.eShopWeb.Web.ViewModels.Account
@using Microsoft.eShopWeb.Web.ViewModels.Manage
@using Microsoft.eShopWeb.Web.Pages
@using Microsoft.AspNetCore.Identity
@using Microsoft.eShopWeb.Infrastructure.Identity
@namespace Microsoft.eShopWeb.Web.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers"

        Assert.AreEqual(y, z)
    End Sub

    <TestMethod>
    Sub TestHelperImports()
        Dim x = <zml xmlns:z="zml">
                    <z:helpers add="*" ns="Microsoft.AspNetCore.Mvc.TagHelpers"/>
                </zml>

        Dim y = x.ParseZml()

        Dim z = "@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers"
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:helpers ns="Microsoft.AspNetCore.Mvc.TagHelpers"/>
            </zml>

        y = x.ParseZml()
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:helpers add="*">Microsoft.AspNetCore.Mvc.TagHelpers</z:helpers>
            </zml>

        y = x.ParseZml()
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:helpers>Microsoft.AspNetCore.Mvc.TagHelpers</z:helpers>
            </zml>

        y = x.ParseZml()
        Assert.AreEqual(y, z)

        x = <zml xmlns:z="zml">
                <z:helpers Microsoft.AspNetCore.Mvc.TagHelpers="*"
                    MyHelpers="*"/>
            </zml>

        y = x.ParseZml()
        z =
"@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, MyHelpers"

        Assert.AreEqual(y, z)
    End Sub

End Class
