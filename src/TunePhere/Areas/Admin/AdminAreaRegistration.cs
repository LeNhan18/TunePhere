using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace TunePhere.Areas.Admin
{
    public class AdminAreaRegistration : IApplicationModelConvention
    {
        private readonly string _areaName;

        public AdminAreaRegistration()
        {
            _areaName = "Admin";
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.Attributes.OfType<AreaAttribute>().Any(a => a.RouteValue == _areaName))
                {
                    foreach (var action in controller.Actions)
                    {
                        action.RouteValues["area"] = _areaName;
                    }
                }
            }
        }
    }
} 