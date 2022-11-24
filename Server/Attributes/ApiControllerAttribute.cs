namespace Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiControllerAttribute : Attribute
    {
        public readonly string ControllerRoute;

        public ApiControllerAttribute(string controllerRoute)
        {
            ControllerRoute = controllerRoute;
        }

        public ApiControllerAttribute() : this("") { }
    }
}