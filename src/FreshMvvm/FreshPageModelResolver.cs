using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace FreshMvvm
{
    public static class FreshPageModelResolver
    {
        public static Page ResolvePageModel<T> () where T : FreshBasePageModel
        {
            return ResolvePageModel<T> (null);
        }

        public static Page ResolvePageModel<T> (object initData) where T : FreshBasePageModel
        {
            var pageModel = FreshIOC.Container.Resolve<T> ();

            return ResolvePageModel<T> (initData, pageModel);
        }

        public static Page ResolvePageModel<T> (object data, T pageModel) where T : FreshBasePageModel
        {
            var type = pageModel.GetType ();
            return ResolvePageModel (type, data, pageModel);
        }

        public static Page ResolvePageModel (Type type, object data) 
        {
            var pageModel = FreshIOC.Container.Resolve (type) as FreshBasePageModel;
            return ResolvePageModel (type, data, pageModel);
        }

        public static Page ResolvePageModel (Type type, object data, FreshBasePageModel pageModel)
        {
            var pageType = GetPageType(type);
            if (pageType == null)
                throw new Exception ("Page for " + type.FullName + " not found");

            var page = (Page)FreshIOC.Container.Resolve (pageType);

            pageModel.WireEvents (page);
            pageModel.CurrentPage = page;
            pageModel.CoreMethods = new PageModelCoreMethods (page, pageModel);
            pageModel.Init (data);
            page.BindingContext = pageModel;

            return page;
        }

        private static Type GetPageType (Type viewType)
        {
            var viewTypeName = viewType.FullName;
            if (viewTypeName.EndsWith ("PageModel", StringComparison.Ordinal))
                viewTypeName = viewTypeName.Replace ("PageModel", string.Empty);
            else if (viewTypeName.EndsWith ("ViewModel", StringComparison.Ordinal))
                viewTypeName = viewTypeName.Replace ("ViewModel", string.Empty);
            else
                return null;

            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var pageFullName = string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", 
                                             viewTypeName, 
                                             "Page", 
                                             viewAssemblyName);
            return Type.GetType(pageFullName);
        }
    }
}

