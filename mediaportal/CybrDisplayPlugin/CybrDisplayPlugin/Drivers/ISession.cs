namespace CybrDisplayPlugin.Drivers
{
    using System;

    public interface ISession : IDisposable
    {
        void Process();
    }
}

