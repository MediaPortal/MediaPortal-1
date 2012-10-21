using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Common.Utils
{
    public abstract class LogProvider
    {
        private ILogManager _log;
        protected virtual ILogManager Log
        {
            get
            {
                if (_log == null)
                {
                    _log = new LogManager(GetType());
                }
                return _log;
            }
        }
    }
}
