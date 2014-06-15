using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.BDA;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.SAT_IP
{
    /// <summary>
    /// A class for handling conditional access and DiSEqC for Turbosight tuners. Note that some Turbosight drivers
    /// seem to still support the original Conexant, NXP and Cyprus interfaces/structures. However, it is
    /// simpler and definitely more future-proof to stick with the information in the published SDK.
    /// </summary>
    public class SAT_IP : BaseCustomDevice, IDirectShowAddOnDevice
    {

        #region COM interface imports

        [ComImport, Guid("9D9FBAFE-3E8C-4104-A279-D9EEEC072BA2")]
        private class SatIpIFilter { };

        [ComVisible(true), ComImport,
          Guid("22D98D0E-6956-4EA0-9D18-4F55DEA8F5EC"),
          InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITsMuxer
        {
            /// <summary>
            /// Some summary
            /// </summary>
            [PreserveSig]
            //int FilterCreateNamedPipe(string devicePath);
            int FilterCreateNamedPipe([MarshalAs(UnmanagedType.LPStr)] string devicePath);
        }

        #endregion

        #region structs

        private class FilterSlot
        {
            public IBaseFilter Filter;
            public IChannel CurrentChannel;
        }

        #endregion

        #region variables

        private bool _isSATIP_Plugin = false;
        private IFilterGraph2 _graph = null;
        private IBaseFilter _infTee = null;
        private FilterSlot _slot = null;
        private string _tunerExternalIdentifier;

        #endregion

        #region BaseCustomDevice members

        /// <summary>
        /// The loading priority for this extension.
        /// </summary>
        public override byte Priority
        {
            get
            {
                return 100;
            }
        }

        /// <summary>
        /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
        /// even a model name and/or number.
        /// </summary>
        public override string Name
        {
            get
            {
                return "MediaPortal SAT>IP Server Plugin";
            }
        }

        /// <summary>
        /// Attempt to initialise the extension-specific interfaces used by the class. If
        /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
        /// immediately.
        /// </summary>
        /// <param name="tunerExternalIdentifier">The external identifier for the tuner.</param>
        /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
        /// <param name="context">Context required to initialise the interface.</param>
        /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
        public override bool Initialise(string tunerExternalIdentifier, CardType tunerType, object context)
        {
            this.LogDebug("SAT>IP: initialising - %s", tunerExternalIdentifier);

            IBaseFilter tunerFilter = context as IBaseFilter;
            if (tunerFilter == null)
            {
                this.LogDebug("SAT>IP: tuner filter is null");
                return false;
            }
            if (string.IsNullOrEmpty(tunerExternalIdentifier))
            {
                this.LogDebug("SAT>IP: tuner external identifier is not set");
                return false;
            }
            if (_isSATIP_Plugin)
            {
                this.LogWarn("SAT>IP: extension already initialised");
                return true;
            }

            _tunerExternalIdentifier = tunerExternalIdentifier;



            this.LogInfo("SAT>IP: plugin is enabled");
            _isSATIP_Plugin = true;

            return true;
        }

        #endregion

        #region IDirectShowAddOnDevice member

        /// <summary>
        /// Insert and connect additional filter(s) into the graph.
        /// </summary>
        /// <param name="graph">The tuner filter graph.</param>
        /// <param name="lastFilter">The source filter (usually either a capture/receiver or
        ///   multiplexer filter) to connect the [first] additional filter to.</param>
        /// <returns><c>true</c> if one or more additional filters were successfully added to the graph, otherwise <c>false</c></returns>
        public bool AddToGraph(IFilterGraph2 graph, ref IBaseFilter lastFilter)
        {
            this.LogDebug("SAT>IP: add filter to graph");

            if (!_isSATIP_Plugin)
            {
                this.LogWarn("SAT>IP: not initialised or interface not supported");
                return false;
            }
            if (graph == null)
            {
                this.LogError("SAT>IP: graph is null");
                return false;
            }
            if (lastFilter == null)
            {
                this.LogError("SAT>IP: last filter is null");
                return false;
            }

            // Add an infinite tee after the tuner/capture filter.
            this.LogDebug("SAT>IP: adding infinite tee");
            _graph = graph;
            _infTee = (IBaseFilter)new InfTee();
            int hr = _graph.AddFilter(_infTee, "SAT>IP Plugin Infinite Tee");
            if (hr != (int)HResult.Severity.Success)
            {
                this.LogError("SAT>IP: failed to add the inf tee to the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                return false;
            }
            IPin outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
            IPin inputPin = DsFindPin.ByDirection(_infTee, PinDirection.Input, 0);
            hr = _graph.ConnectDirect(outputPin, inputPin, null);
            Release.ComObject("SAT>IP plugin upstream filter output pin", ref outputPin);
            Release.ComObject("SAT>IP plugin infinite tee input pin", ref inputPin);
            if (hr != (int)HResult.Severity.Success)
            {
                this.LogError("SAT>IP: failed to connect the inf tee into the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                return false;
            }
            lastFilter = _infTee;

            // Add filter.
            this.LogDebug("SAT>IP: adding SAT>IP filter");
            _slot = new FilterSlot();
            _slot.Filter = (IBaseFilter)new SatIpIFilter();
            _slot.CurrentChannel = null;

            // Add the filter to the graph.
            hr = _graph.AddFilter(_slot.Filter, "SAT>IP Filter");
            if (hr != (int)HResult.Severity.Success)
            {
                this.LogError("SAT>IP: failed to add SAT>IP plugin filter to the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                return false;
            }

            // Connect the filter into the graph.
            outputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
            inputPin = DsFindPin.ByDirection(_slot.Filter, PinDirection.Input, 0);
            hr = _graph.ConnectDirect(outputPin, inputPin, null);
            Release.ComObject("SAT>IP plugin source filter output pin", ref outputPin);
            Release.ComObject("SAT>IP plugin MDAPI filter input pin", ref inputPin);
            if (hr != (int)HResult.Severity.Success)
            {
                this.LogError("SAT>IP: failed to connect SAT>IP plugin filter into the graph, hr = 0x{0:x} ({1})", hr, HResult.GetDXErrorString(hr));
                return false;
            }

            // configure the filter
            try
            {
                ITsMuxer conf = _slot.Filter as ITsMuxer;
                if (conf != null)
                {
                    this.LogInfo("SAT>IP: configure filter, named pipe name: {0}", _tunerExternalIdentifier);
                    //conf.FilterCreateNamedPipe(_tunerExternalIdentifier);
                }
                else
                {
                    this.LogError("SAT>IP: failed to configure channel (conf == null)");
                }
                    
            }
            catch (Exception ex)
            {
                this.LogError(ex, "SAT>IP: failed to configure filter");
                return false;
            }
            

            // Note all cleanup is done in Dispose(), which should be called immediately if we return false.
            return true;
        }

        #endregion

        #region IDisposable member

        /// <summary>
        /// Release and dispose all resources.
        /// </summary>
        public override void Dispose()
        {
            if (_graph != null)
            {
                _graph.RemoveFilter(_infTee);

                if (_slot != null)
                {
                    _graph.RemoveFilter(_slot.Filter);
                }

                Release.ComObject("SAT>IP plugin graph", ref _graph);
            }

            Release.ComObject("SAT>IP plugin infinite tee", ref _infTee);
            if (_slot != null)
            {
                Release.ComObject("SAT>IP plugin MDAPI filter", ref _slot.Filter);
            }
            _slot = null;

            _isSATIP_Plugin = false;
        }

        #endregion
    }
}
