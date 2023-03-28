using System.Web;
using System.Web.Mvc;

namespace UAndes.ICC5103._202301
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
