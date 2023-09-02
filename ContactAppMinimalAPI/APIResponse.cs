using System.Net;

namespace ContactAppMinimalAPI
{
    public class APIResponse
    {
        public Boolean IsSuccess { get; set; }
        public Object Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public String ErrorMessage { get; set; }
    }
}
