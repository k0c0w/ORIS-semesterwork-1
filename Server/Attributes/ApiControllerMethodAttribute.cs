namespace Server
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiControllerMethodAttribute : Attribute
    {
        public readonly string MethodURI;

        public ApiControllerMethodAttribute(string methodURI)
        {
            MethodURI = methodURI;
        }

        public ApiControllerMethodAttribute() : this("") { }
    }
}