using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Xml.Serialization;



namespace MediaLibrary
{
    #region public class MLHashItem : IMLHashItem
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    [Serializable]
    public class MLHashItem : IMLHashItem
    {
        #region Members

        private ArrayList unsync_items;
        private ArrayList _items;

        #endregion

        #region Properties

        #region Serializable Properties

        #region public ArrayList items
        /// <summary>
        /// Get/Sets the items of the MLHashItem
        /// this is public because we want to serialize it
        /// </summary>
        /// <value></value>
        [XmlElement(ElementName = "Entry", Type = typeof(MLHashItemEntry))]
        public ArrayList items
        {
            get { return _items; }
            set { _items = value; }
        }
        #endregion

        #endregion

        #region Non-Serializable Properties

        #region public object this[int index]
        /// <summary>
        /// Gets the <see cref="Object"/> item identified by the given arguments of the MLHashItem
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public object this[int index]
        {
            get
            {
                if (index < items.Count)
                {
                    return ((MLHashItemEntry)items[index]).Value;
                }
                return null;
            }
        }
        #endregion

        #region public object this[string key]
        /// <summary>
        /// Get/Sets the <see cref="Object"/> item identified by the given arguments of the MLHashItem
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public object this[string key]
        {
            get
            {
                object reto = null;

                lock (items.SyncRoot)
                {
                    foreach (MLHashItemEntry o in items)
                    {
                        if (string.Compare(o.Key, key, true) == 0)
                        {
                            reto = o.Value;
                            break;
                        }
                    }
                }
                return reto;
            }

            set
            {
                bool newitem = true;



                lock (items.SyncRoot)
                {
                    foreach (MLHashItemEntry o in items)
                    {
                        if (string.Compare(o.Key, key, true) == 0)
                        {
                            o.Value = value;
                            newitem = false;
                            break;
                        }
                    }

                    if (newitem)
                    {
                        MLHashItemEntry e = new MLHashItemEntry();
                        e.Key = key;
                        e.Value = value;
                        items.Add(e);
                    }
                }
            }
        }

        #endregion

        #region public int Count
        /// <summary>
        /// Gets the Count of the MLHashItem
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public int Count
        {
            get
            {
                return items.Count;
            }
        }
        #endregion

        #region public ICollection Keys
        /// <summary>
        /// Gets the Keys of the MLHashItem
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public ICollection Keys
        {
            get
            {
                ArrayList sc = new ArrayList();
                lock (items.SyncRoot)
                {
                    foreach (MLHashItemEntry o in items)
                    {
                        sc.Add(o.Key);
                    }
                }
                return sc;
            }
        }
        #endregion

        #region public ICollection Values
        /// <summary>
        /// Gets the Values of the MLHashItem
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public ICollection Values
        {
            get
            {
                ArrayList sc = new ArrayList();
                lock (items.SyncRoot)
                {
                    foreach (MLHashItemEntry o in items)
                    {
                        sc.Add(o.Value);
                    }
                }
                return sc;
            }
        }
        #endregion

        #endregion

        #endregion

        #region Constructors

        #region public MLHashItem()
        /// <summary>
        /// Initializes a new instance of the <b>MLHashItem</b> class.
        /// </summary>
        public MLHashItem()
        {
            unsync_items = new ArrayList();
            items = ArrayList.Synchronized(unsync_items);
        }
        #endregion

        #endregion

        #region Methods

        #region Public Methods

        #region public void Clear()
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }
        #endregion

        #region public bool Contains(string Key)
        /// <summary>
        /// Returns a value indicating whether the specified <see cref="String"/>
        ///  is contained in the <see cref="MediaLibrary.MLHashItem"/>.
        /// </summary>
        /// <param name="Key">The <see cref="String"/> to locate in the 
        /// <see cref="MediaLibrary.MLHashItem"/>.</param>
        /// <returns><b>true</b> if the <i>String</i> parameter is a member 
        /// of the <see cref="MediaLibrary.MLHashItem"/>; otherwise, <b>false</b>.</returns>
        public bool Contains(string Key)
        {
            bool retb = false;

            lock (items.SyncRoot)
            {
                foreach (MLHashItemEntry o in items)
                {
                    if (string.Compare(o.Key, Key, true) == 0)
                    {
                        retb = true;
                        break;
                    }
                }
            }
            return retb;
        }
        #endregion

        #region public void CopyFrom(System.IntPtr Source)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Source"></param>
        public void CopyFrom(System.IntPtr Source)
        {
            if (Source != null)
            {
                CopyFrom(Marshal.GetObjectForIUnknown(Source) as IMLHashItem);
                Marshal.Release(Source);
            }
        }
        #endregion

        #region public void CopyFrom(IMLHashItem Source)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Source"></param>
        public void CopyFrom(IMLHashItem Source)
        {
            if (Source != null)
            {
                MLHashItem _source = Source as MLHashItem;

                lock (_source.items.SyncRoot)
                {
                    foreach (MLHashItemEntry o in _source.items)
                    {
                        this[o.Key] = o.Value;
                    }
                }
            }
        }
        #endregion

        #region public object Get(string Key, object Default)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public object Get(string Key, object Default)
        {
            object value = this[Key];

            if (value == null)
                return Default;
            else
                return value;
        }
        #endregion

        #region public MLHashItemEntry GetEntry(string Key)
        /// <summary>
        /// Finds or creates an entry with Key.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public MLHashItemEntry GetEntry(string Key)
        {
            MLHashItemEntry retmie = null;

            lock (items.SyncRoot)
            {
                foreach (MLHashItemEntry o in items)
                {
                    if (string.Compare(o.Key, Key, true) == 0)
                    {
                        retmie = o;
                        break;
                    }
                }

                if (retmie == null)
                {
                    retmie = new MLHashItemEntry();
                    retmie.Key = Key;
                    retmie.Value = null;
                    items.Add(retmie);
                }
            }

            return retmie;
        }

        #endregion

        #endregion

        #region Internal Methods

        #region public void SaveAsIni(string FileName)
        /// <summary>
        /// saves items to a text file. each line is "key=value" e.g. "volume=100". this function is meant for SaveState and SaveLanguageFile
        /// </summary>
        /// <param name="FileName"></param>
        public void SaveAsIni(string FileName)
        {
            StreamWriter unsync_w;
            TextWriter sync_w;

            unsync_w = new StreamWriter(FileName);
            sync_w = TextWriter.Synchronized(unsync_w);

            lock (items.SyncRoot)
            {
                foreach (MLHashItemEntry myDE in items)
                {
                    sync_w.WriteLine(myDE.Key + "=" + myDE.Value as string);

                    //if (myDE.Value != null)
                    //    sync_w.WriteLine(myDE.Key + "=" + myDE.Value.ToString());
                    //else
                    //    sync_w.WriteLine(myDE.Key + "="); // this gives users the possibility to manually add to translation tables
                }
            }

            sync_w.Close();
            unsync_w.Close();
        }
        #endregion

        #region public void LoadFromIni(string FileName)
        /// <summary>
        /// loads items from a text file. each line is "key=value" e.g. "volume=100". this function is meant for LoadState and LoadLanguageFile
        /// </summary>
        /// <param name="FileName"></param>
        public void LoadFromIni(string FileName)
        {
            StreamReader unsync_r;
            TextReader sync_r;
            string rawline;
            char[] delimiter = { '=' };
            string[] splitline;


            lock (items.SyncRoot)
            {
                items.Clear();


                // TODO: TextReader.ReadLine does not read special characters!

                if (File.Exists(FileName))
                {
                    unsync_r = new StreamReader(FileName);
                    sync_r = TextReader.Synchronized(unsync_r);

                    do
                    {
                        rawline = sync_r.ReadLine();
                        if (rawline != null)
                        {
                            splitline = rawline.Split(delimiter, 2);
                            if (splitline.Length == 2)
                                this[splitline[0]] = splitline[1];
                        }
                    }
                    while (rawline != null);

                    sync_r.Close();
                    unsync_r.Close();
                }
            }
        }
        #endregion

        #region public static MLHashItem Deserialize(string filename)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static MLHashItem Deserialize(string filename)
        {
            if (!System.IO.File.Exists(filename)) return null;
            MLHashItem item;

            XmlSerializer deserializer = new XmlSerializer(typeof(MLHashItem));

            TextReader reader = new StreamReader(filename);
            item = (MLHashItem)deserializer.Deserialize(reader);
            reader.Close();
            return item;
        }
        #endregion

        #region public void Serialize( string filename)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        public void Serialize(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MLHashItem));

            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, this);
            writer.Close();
        }
        #endregion

        #endregion

        #endregion
    } 
    #endregion
}
