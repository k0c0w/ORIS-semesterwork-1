namespace Server
{
    public  class HttpPostAttribute : ApiControllerMethodAttribute
    {
        public HttpPostAttribute(string methodURI) : base(methodURI) { }
        public HttpPostAttribute() : base() { }
    }
}