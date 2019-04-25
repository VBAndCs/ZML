Imports Microsoft.AspNetCore.Mvc.RazorPages
Imports Microsoft.eShopWeb.Web.Services
Imports Microsoft.eShopWeb.Web.ViewModels
Imports ZML

Namespace Pages

    Public Class IndexModel : Inherits PageModel

        ' This property is used in the Index.cshtml,
        ' to inject our vbxml code in the page as a pratial view

        Private ReadOnly _catalogViewModelService As ICatalogViewModelService

        Public Sub New(ByVal catalogViewModelService As ICatalogViewModelService)
            _catalogViewModelService = catalogViewModelService
        End Sub

        Public Property CatalogModel As CatalogIndexViewModel = New CatalogIndexViewModel()

        Public Async Function OnGet(ByVal catalogModel As CatalogIndexViewModel, ByVal pageId As Integer?) As Task
            Me.CatalogModel = Await _catalogViewModelService.GetCatalogItems(If(pageId, 0), Constants.ITEMS_PER_PAGE, catalogModel.BrandFilterApplied, catalogModel.TypesFilterApplied)
        End Function

    End Class
End Namespace