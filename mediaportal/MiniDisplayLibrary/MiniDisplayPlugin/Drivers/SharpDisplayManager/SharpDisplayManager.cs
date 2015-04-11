using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using MediaPortal.GUI.Library;
using System.Windows.Forms;
using System.ServiceModel;
using System.Runtime.Serialization;
using SharpDisplay;

namespace SharpDisplay
{
    //That contract need to be in the same namespace than the original assembly
    //otherwise our parameter won't make it to the server.
    //See: http://stackoverflow.com/questions/14956377/passing-an-object-using-datacontract-in-wcf/25455292#25455292
    /// <summary>
    /// For client to specify a specific layout.
    /// </summary>
    [DataContract]
    public class TableLayout
    {
        public TableLayout()
        {
            Columns = new List<ColumnStyle>();
            Rows = new List<RowStyle>();
            Cells = new List<DataField>();
        }

        public TableLayout(int aColumnCount, int aRowCount)
        {
            Columns = new List<ColumnStyle>();
            Rows = new List<RowStyle>();

            for (int i = 0; i < aColumnCount; i++)
            {
                Columns.Add(new ColumnStyle(SizeType.Percent, 100 / aColumnCount));
            }

            for (int i = 0; i < aRowCount; i++)
            {
                Rows.Add(new RowStyle(SizeType.Percent, 100 / aRowCount));
            }
        }

        [DataMember]
        public List<DataField> Cells { get; set; }

        [DataMember]
        public List<ColumnStyle> Columns { get; set; }

        [DataMember]
        public List<RowStyle> Rows { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    [DataContract]
    public class DataField
    {
        public DataField()
        {
            Index = 0;
            ColumnSpan = 1;
            RowSpan = 1;
            //Text
            Text = "";
            Alignment = ContentAlignment.MiddleLeft;
            //Bitmap
            Bitmap = null;
        }

        //Text constructor
        public DataField(int aIndex, string aText = "", ContentAlignment aAlignment = ContentAlignment.MiddleLeft)
        {
            ColumnSpan = 1;
            RowSpan = 1;
            Index = aIndex;
            Text = aText;
            Alignment = aAlignment;
            //
            Bitmap = null;
        }

        //Bitmap constructor
        public DataField(int aIndex, Bitmap aBitmap)
        {
            ColumnSpan = 1;
            RowSpan = 1;
            Index = aIndex;
            Bitmap = aBitmap;
            //Text
            Text = "";
            Alignment = ContentAlignment.MiddleLeft;
        }


        //Generic layout properties
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public int Column { get; set; }

        [DataMember]
        public int Row { get; set; }

        [DataMember]
        public int ColumnSpan { get; set; }

        [DataMember]
        public int RowSpan { get; set; }

        //Text properties
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public ContentAlignment Alignment { get; set; }

        //Bitmap properties
        [DataMember]
        public Bitmap Bitmap { get; set; }

        //
        public bool IsBitmap { get { return Bitmap != null; } }
        //
        public bool IsText { get { return Bitmap == null; } }
        //
        public bool IsSameLayout(DataField aField)
        {
            return (aField.ColumnSpan == ColumnSpan && aField.RowSpan == RowSpan);
        }
    }

    /// <summary>
    /// Define our SharpDisplay service.
    /// Clients and servers must implement it to communicate with one another.
    /// Through this service clients can send requests to a server.
    /// Through this service a server session can receive requests from a client.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ICallback), SessionMode = SessionMode.Required)]
    public interface IService
    {
        /// <summary>
        /// Set the name of this client.
        /// Name is a convenient way to recognize your client.
        /// Naming you client is not mandatory.
        /// In the absence of a name the session ID is often used instead.
        /// </summary>
        /// <param name="aClientName"></param>
        [OperationContract(IsOneWay = true)]
        void SetName(string aClientName);

        /// <summary>
        /// </summary>
        /// <param name="aLayout"></param>
        [OperationContract(IsOneWay = true)]
        void SetLayout(TableLayout aLayout);

        /// <summary>
        /// Set the given field on your display.
        /// Fields are often just lines of text or bitmaps.
        /// </summary>
        /// <param name="aTextFieldIndex"></param>
        [OperationContract(IsOneWay = true)]
        void SetField(DataField aField);

        /// <summary>
        /// Allows a client to set multiple fields at once.
        /// </summary>
        /// <param name="aFields"></param>
        [OperationContract(IsOneWay = true)]
        void SetFields(System.Collections.Generic.IList<DataField> aFields);

        /// <summary>
        /// Provides the number of clients currently connected
        /// </summary>
        /// <returns></returns>
        [OperationContract()]
        int ClientCount();

    }

    /// <summary>
    /// SharDisplay callback provides a means for a server to notify its clients.
    /// </summary>
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void OnConnected();

        /// <summary>
        /// Tell our client to close its connection.
        /// Notably sent when the server is shutting down.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnCloseOrder();
    }
}

//////////////////////////////////////////////////////////////////////////

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers.SharpDisplayManager
{

    /// <summary>
    ///
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Client : DuplexClientBase<IService>
    {
        public string Name { get; set; }
        public string SessionId { get { return InnerChannel.SessionId; } }

        public Client(InstanceContext callbackInstance)
            : base(callbackInstance, new NetTcpBinding(SecurityMode.None, true), new EndpointAddress("net.tcp://localhost:8001/DisplayService"))
        { }

        public void SetName(string aClientName)
        {
            Name = aClientName;
            Channel.SetName(aClientName);
        }

        public void SetLayout(TableLayout aLayout)
        {
            Channel.SetLayout(aLayout);
        }

        public void SetField(DataField aField)
        {
            Channel.SetField(aField);
        }

        public void SetFields(System.Collections.Generic.IList<DataField> aFields)
        {
            Channel.SetFields(aFields);
        }

        public int ClientCount()
        {
            return Channel.ClientCount();
        }
    }


    public class Callback : ICallback, IDisposable
    {
        Display iDisplay;

        public Callback(Display aDisplay)
        {
            iDisplay = aDisplay;
        }

        public void OnConnected()
        {
            //Debug.Assert(Thread.CurrentThread.IsThreadPoolThread);
            //Trace.WriteLine("Callback thread = " + Thread.CurrentThread.ManagedThreadId);

            //MessageBox.Show("OnConnected()", "Client");
        }


        public void OnCloseOrder()
        {
            iDisplay.CloseConnection();
            //Debug.Assert(Thread.CurrentThread.IsThreadPoolThread);
            //Trace.WriteLine("Callback thread = " + Thread.CurrentThread.ManagedThreadId);

            //MessageBox.Show("OnServerClosing()", "Client");
        }

        //From IDisposable
        public void Dispose()
        {

        }
    }


    /// <summary>
    /// SoundGraph iMON MiniDisplay implementation.
    /// Provides access to iMON Display API.
    /// </summary>
    public class Display : BaseDisplay
    {
        Client iClient;
        Callback iCallback;
        DataField iBitmapField;
        DataField iTextFieldTop;
        DataField iTextFieldBottom;
        DataField[] iFields;
        bool iNeedUpdate;

        public Display()
        {
            Initialized = false;
            iNeedUpdate = true;

            if (SupportsGraphics)
            {
                iBitmapField = new DataField(0);
                iBitmapField.RowSpan = 2;
                iTextFieldTop = new DataField(1);
                iTextFieldBottom = new DataField(2);

                iFields = new DataField[] { iBitmapField, iTextFieldTop, iTextFieldBottom };
            }
            else
            {
                iTextFieldTop = new DataField(0);
                iTextFieldBottom = new DataField(1);
                iFields = new DataField[] { iTextFieldTop, iTextFieldBottom };
            }
        }

        //From IDisplay
        public override bool SupportsGraphics { get { return false; } }

        //From IDisplay
        public override bool SupportsText { get { return true; } }

        /// <summary>
        /// Tell whether or not our SoundGraph Display plug-in is initialized
        /// </summary>
        protected bool Initialized { get; set; }

        //From IDisplay
        public override string Description { get { return "Sharp Display Manager"; } }

        //From IDisplay
        //Notably used when testing to put on the screen
        public override string Name
        {
            get
            {
                return "Sharp Display Manager";
            }
        }

        //
        private bool CheckDisplay()
        {
            if (iClient == null || iClient.State==CommunicationState.Faulted)
            {
                //Attempt to recover
                //LogDebug("SoundGraphDisplay.CheckDisplay(): Trying to recover");
                CleanUp();
                Initialize();
                return false;
            }

            return true;
        }

        //From IDisplay
        public override void Update()
        {
            if (CheckDisplay() && iNeedUpdate)
            {
                //Display connection should be good
                iClient.SetFields(iFields);
                iNeedUpdate = false;
            }
        }

        //
        protected string ClassErrorName { get; set; }

        protected string UnsupportedDeviceErrorMessage { get; set; }


        public override string ErrorMessage
        {
            get
            {
                if (IsDisabled)
                {
                    return "ERROR: SharpDisplayManager";
                }
                return string.Empty;
            }
        }

        //From IDisplay

        public override bool IsDisabled
        {
            get
            {
                //To check if our display is enabled we need to initialize it.
                Initialize();
                bool res = !Initialized;
                CleanUp();
                return res;
            }
        }

        //From IDisplay
        public override void Dispose()
        {
            CleanUp();
        }

        //From IDisplay
        public override void Initialize()
        {
            try
            {
                iCallback = new Callback(this);
                InstanceContext instanceContext = new InstanceContext(iCallback);
                iClient = new Client(instanceContext);
                iClient.SetName("MediaPortal");
                Initialized = true;
                if (SupportsGraphics)
                {
                    SetLayoutWithBitmap();
                }
                else
                {
                    SetLayoutWithoutBitmap();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(
                "SharpDisplayManager.Display.Initialize(): CAUGHT EXCEPTION {0}\n\n{1}\n\n", ex.Message,
                new object[] { ex.StackTrace });

                //Rollback
                iClient = null;
                iCallback = null;
                Initialized = false;
            }
        }

        //From IDisplay
        public override void CleanUp()
        {
            if (iClient != null)
            {
                try
                {
                    iTextFieldTop.Text="Bye Bye!";
                    iTextFieldBottom.Text="See you next time!";
                    iClient.SetFields(iFields);
                    iClient.Close();
                }
                catch (System.Exception ex)
                {
                    Log.Error(
                    "SharpDisplayManager.Display.CleanUp(): CAUGHT EXCEPTION {0}\n\n{1}\n\n", ex.Message,
                    new object[] { ex.StackTrace });
                }

                iClient = null;
                iCallback = null;
                Initialized = false;
            }
        }

        //From IDisplay
        public override void SetLine(int line, string message, ContentAlignment aAlignment)
        {
            CheckDisplay();

            if (!Initialized)
            {
                return;
            }

            //
            if (line == 0 && (iTextFieldTop.Text != message || iTextFieldTop.Alignment != aAlignment))
            {
                iNeedUpdate = true;
                iTextFieldTop.Text = message;
                iTextFieldTop.Alignment = aAlignment;
                //iClient.SetField(iTextFieldTop);
            }
            else if (line == 1 && (iTextFieldBottom.Text != message || iTextFieldBottom.Alignment != aAlignment))
            {
                iNeedUpdate = true;
                iTextFieldBottom.Text = message;
                iTextFieldBottom.Alignment = aAlignment;
                //iClient.SetField(iTextFieldBottom);
            }

        }

        //From IDisplay
        public override void Configure()
        {
            //We need to have an initialized display to be able to configure it
            Initialize();

            CleanUp();
        }

        //From IDisplay
        public override void DrawImage(Bitmap aBitmap)
        {           
            //Set a bitmap for our first field            
            int x1 = 0;
            int y1 = 0;
            int x2 = 64;
            int y2 = 64;

            Bitmap bitmap = new Bitmap(x2, y2);
            Pen blackPen = new Pen(Color.Black, 3);

            // Draw line to screen.
            
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(aBitmap, new Rectangle(x1, y1, x2, y2));
                //graphics.DrawLine(blackPen, x1, y1, x2, y2);
                //graphics.DrawLine(blackPen, x1, y2, x2, y1);
            }

            //aBitmap.Save("D:\\mp.png");
            
            //Not sure why passing aBitmap directly causes an exception in SharpDisplayManager WFC layers
            iBitmapField.Bitmap = bitmap;

        }

        //From IDisplay
        public override void SetCustomCharacters(int[][] customCharacters)
        {
            // Not supported
        }

        //From IDisplay
        public override void Setup(string port,
            int lines, int cols, int delay,
            int linesG, int colsG, int timeG,
            bool backLight, int backLightLevel,
            bool contrast, int contrastLevel,
            bool BlankOnExit)
        {
            // iMON VFD/LCD cannot be setup
        }

        public void CloseConnection()
        {
            if (iClient != null)
            {
                try
                {
                    iClient.Close();
                }
                catch (System.Exception ex)
                {
                    Log.Error(
                    "SharpDisplayManager.Display.CloseConnection(): CAUGHT EXCEPTION {0}\n\n{1}\n\n", ex.Message,
                    new object[] { ex.StackTrace });
                }

                iClient = null;
                iCallback = null;
                Initialized = false;
            }
        }

        /// <summary>
        /// Define a layout with a bitmap field on the left and two lines of text on the right.
        /// </summary>
        private void SetLayoutWithBitmap()
        {
            //Define a 2 by 2 layout
            TableLayout layout = new TableLayout(2, 2);
            //First column only takes 25%
            layout.Columns[0].Width = 25F;
            //Second column takes up 75% 
            layout.Columns[1].Width = 75F;
            //Send layout to server
            iClient.SetLayout(layout);
        }

        /// <summary>
        /// </summary>
        private void SetLayoutWithoutBitmap()
        {
            //Define a 1 column by 2 rows layout
            TableLayout layout = new TableLayout(1, 2);
            //First column only takes 25%
            layout.Columns[0].Width = 100F;
            //Send layout to server
            iClient.SetLayout(layout);
        }


    }
}

