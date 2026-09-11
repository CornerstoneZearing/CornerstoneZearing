using CornerstoneZearing.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CornerstoneZearing.Web.ViewComponents;

public class NavigationViewComponent : ViewComponent
{
    private readonly NavigationService _navigation;

    public NavigationViewComponent(NavigationService navigation) => _navigation = navigation;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var nodes = await _navigation.GetNavigationAsync();
        return View(nodes);
    }
}
