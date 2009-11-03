namespace MpeCore.Classes.Events
{
    public class UnInstallEventArgs
    {
        public UnInstallEventArgs()
        {
            Message = string.Empty;
            UnInstallItem = new UnInstallItem();
        }

        public UnInstallEventArgs(string message, UnInstallItem item)
        {
            Message = message;
            UnInstallItem = item;
        }

        public string Message { get; set; }
        public UnInstallItem UnInstallItem { get; set; }
    }
}
