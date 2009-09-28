
namespace MpeCore.Classes
{
    public class ValidationResponse
    {
        public ValidationResponse()
        {
            Valid = true;
            Message = string.Empty;
        }
        
        public bool Valid { get; set; }
        public string Message { get; set; }
    }
}
