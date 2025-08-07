using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

public static class RazorEngineHelper
{
    public static string RenderPartial(string viewName, object model = null, ViewDataDictionary viewData = null)
    {
        // Initialize view engines if needed
        EnsureViewEnginesInitialized();

        // Create minimal context without controller
        var context = CreateMinimalContext();
        var viewData2 = viewData ?? new ViewDataDictionary();

        if (model != null)
        {
            viewData2.Model = model;
        }

        using (var stringWriter = new StringWriter())
        {
            // Find the view directly through view engines
            var viewResult = FindView(viewName);

            if (viewResult.View == null)
            {
                var locations = viewResult.SearchedLocations != null
                    ? string.Join(", ", viewResult.SearchedLocations)
                    : "No locations searched";
                throw new FileNotFoundException($"View '{viewName}' not found. Searched: {locations}");
            }

            // Create view context without controller
            var viewContext = new ViewContext
            {
                HttpContext = context,
                ViewData = viewData2,
                TempData = new TempDataDictionary(),
                Writer = stringWriter,
                View = viewResult.View,
                RouteData = new RouteData()
            };

            // Render the view
            viewResult.View.Render(viewContext, stringWriter);
            viewResult.ViewEngine.ReleaseView(null, viewResult.View);

            return stringWriter.ToString();
        }
    }

    public static string RenderPartialFromPath(string viewPath, object model = null, ViewDataDictionary viewData = null)
    {
        EnsureViewEnginesInitialized();

        var context = CreateMinimalContext();
        var viewData2 = viewData ?? new ViewDataDictionary();

        if (model != null)
        {
            viewData2.Model = model;
        }

        using (var stringWriter = new StringWriter())
        {
            // Create view from file path directly
            var view = CreateViewFromPath(viewPath);

            if (view == null)
            {
                throw new FileNotFoundException($"View file not found at path: {viewPath}");
            }

            var viewContext = new ViewContext
            {
                HttpContext = context,
                ViewData = viewData2,
                TempData = new TempDataDictionary(),
                Writer = stringWriter,
                View = view,
                RouteData = new RouteData()
            };

            view.Render(viewContext, stringWriter);

            return stringWriter.ToString();
        }
    }

    private static void EnsureViewEnginesInitialized()
    {
        if (ViewEngines.Engines.Count == 0)
        {
            ViewEngines.Engines.Add(new RazorViewEngine());
        }
    }

    private static HttpContextBase CreateMinimalContext()
    {
        if (HttpContext.Current != null)
        {
            return new HttpContextWrapper(HttpContext.Current);
        }

        // Create minimal offline context
        var request = new HttpRequest("", "http://localhost/", "");
        var response = new HttpResponse(new StringWriter());
        var context = new HttpContext(request, response);

        return new HttpContextWrapper(context);
    }

    private static ViewEngineResult FindView(string viewName)
    {
        // Create a fake controller context for view location
        var httpContext = CreateMinimalContext();
        var routeData = new RouteData();
        routeData.Values["controller"] = "Fake";

        var fakeControllerContext = new ControllerContext(
            new RequestContext(httpContext, routeData),
            new FakeController());

        return ViewEngines.Engines.FindPartialView(fakeControllerContext, viewName);
    }

    private static IView CreateViewFromPath(string viewPath)
    {
        // Create a Razor view directly from file
        return new RazorView(new ControllerContext(), viewPath, null, false, null);
    }

    // Minimal controller just for view location context
    private class FakeController : Controller
    {
        // Empty implementation
    }
}

// Standalone Razor renderer that doesn't depend on MVC infrastructure
public static class StandaloneRazorRenderer
{
    private static readonly Dictionary<string, string> _viewCache = new Dictionary<string, string>();
    private static readonly object _cacheLock = new object();

    public static string Render(string viewName, object model = null, Dictionary<string, object> viewBag = null, bool useCache = false)
    {
        var cacheKey = useCache ? $"{viewName}_{model?.GetHashCode()}" : null;

        if (useCache && cacheKey != null)
        {
            lock (_cacheLock)
            {
                if (_viewCache.ContainsKey(cacheKey))
                    return _viewCache[cacheKey];
            }
        }

        var result = RenderInternal(viewName, model, viewBag);

        if (useCache && cacheKey != null)
        {
            lock (_cacheLock)
            {
                _viewCache[cacheKey] = result;
            }
        }

        return result;
    }

    private static string RenderInternal(string viewName, object model, Dictionary<string, object> viewBag)
    {
        var viewData = new ViewDataDictionary();

        if (model != null)
            viewData.Model = model;

        if (viewBag != null)
        {
            foreach (var kvp in viewBag)
                viewData[kvp.Key] = kvp.Value;
        }

        return RazorEngineHelper.RenderPartial(viewName, model, viewData);
    }

    public static void ClearCache()
    {
        lock (_cacheLock)
        {
            _viewCache.Clear();
        }
    }
}

// File-based Razor renderer for direct file access
public static class FileBasedRazorRenderer
{
    private static readonly Dictionary<string, IView> _compiledViews = new Dictionary<string, IView>();
    private static readonly object _viewLock = new object();

    public static string RenderFromFile(string filePath, object model = null, Dictionary<string, object> viewData = null)
    {
        IView view;

        lock (_viewLock)
        {
            if (!_compiledViews.ContainsKey(filePath))
            {
                view = CompileView(filePath);
                _compiledViews[filePath] = view;
            }
            else
            {
                view = _compiledViews[filePath];
            }
        }

        if (view == null)
            throw new InvalidOperationException($"Could not compile view from file: {filePath}");

        return ExecuteView(view, model, viewData);
    }

    private static IView CompileView(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"View file not found: {filePath}");
        }

        try
        {
            // Create a RazorView directly from the file
            var httpContext = CreateHttpContext();
            var routeData = new RouteData();
            var controllerContext = new ControllerContext(new RequestContext(httpContext, routeData), new EmptyController());

            return new RazorView(controllerContext, filePath, null, false, null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to compile view from {filePath}: {ex.Message}", ex);
        }
    }

    private static string ExecuteView(IView view, object model, Dictionary<string, object> viewData)
    {
        using (var stringWriter = new StringWriter())
        {
            var viewDataDict = new ViewDataDictionary();

            if (model != null)
                viewDataDict.Model = model;

            if (viewData != null)
            {
                foreach (var kvp in viewData)
                    viewDataDict[kvp.Key] = kvp.Value;
            }

            var viewContext = new ViewContext
            {
                HttpContext = CreateHttpContext(),
                ViewData = viewDataDict,
                TempData = new TempDataDictionary(),
                Writer = stringWriter,
                View = view,
                RouteData = new RouteData()
            };

            view.Render(viewContext, stringWriter);
            return stringWriter.ToString();
        }
    }

    private static HttpContextBase CreateHttpContext()
    {
        if (HttpContext.Current != null)
            return new HttpContextWrapper(HttpContext.Current);

        var request = new HttpRequest("", "http://localhost/", "");
        var response = new HttpResponse(new StringWriter());
        return new HttpContextWrapper(new HttpContext(request, response));
    }

    private class EmptyController : Controller { }

    public static void ClearCompiledViews()
    {
        lock (_viewLock)
        {
            _compiledViews.Clear();
        }
    }
}