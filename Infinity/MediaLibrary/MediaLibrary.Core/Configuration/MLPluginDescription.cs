using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MediaLibrary.Configuration
{

    #region public class MLPluginDescription
    /// <summary>
    /// MLPluginDescription holds all the data in the .mlpd file.
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    [XmlRoot(ElementName = "PluginDescription")]
    public class MLPluginDescription
    {
        #region Members

        private PluginDescription_information _information;
        private PluginDescription_documentation _documentation;
        private PluginDescription_author _author;
        private PluginDescription_installation _installation;

        #endregion

        #region Properties

        #region Serializable Properties

        #region public PluginDescription_information information
        /// <summary>
        /// Get/Sets the information of the MLPluginDescription
        /// </summary>
        /// <value></value>
        public PluginDescription_information information
        {
            get { return _information; }
            set { _information = value; }
        }
        #endregion

        #region public PluginDescription_documentation documentation
        /// <summary>
        /// Get/Sets the documentation of the MLPluginDescription
        /// </summary>
        /// <value></value>
        public PluginDescription_documentation documentation
        {
            get { return _documentation; }
            set { _documentation = value; }
        }
        #endregion

        #region public PluginDescription_author author
        /// <summary>
        /// Get/Sets the author of the MLPluginDescription
        /// </summary>
        /// <value></value>
        public PluginDescription_author author
        {
            get { return _author; }
            set { _author = value; }
        }
        #endregion

        #region public PluginDescription_installation installation
        /// <summary>
        /// Get/Sets the installation of the MLPluginDescription
        /// </summary>
        /// <value></value>
        public PluginDescription_installation installation
        {
            get { return _installation; }
            set { _installation = value; }
        }
        #endregion

        #endregion

        #endregion

        #region Constructors

        #region public MLPluginDescription()
        /// <summary>
        /// Initializes a new instance of the <b>MLPluginDescription</b> class.
        /// </summary>
        public MLPluginDescription()
        {
            information = new PluginDescription_information();
            documentation = new PluginDescription_documentation();
            author = new PluginDescription_author();
            installation = new PluginDescription_installation();
        }
        #endregion

        #endregion

        #region Methods

        #region Static Methods

        public static MLPluginDescription Deserialize(string filename)
        {
            MLPluginDescription desc;
            XmlSerializer deserializer = new XmlSerializer(typeof(MLPluginDescription));
            TextReader reader = new StreamReader(filename);
            desc = (MLPluginDescription)deserializer.Deserialize(reader);
            reader.Close();
            return desc;
        }

        public static void Serialize(MLPluginDescription desc, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MLPluginDescription));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, desc);
            writer.Close();
        }

        #endregion

        #endregion
    } 
    #endregion

    #region public class PluginDescription_install_file
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public class PluginDescription_install_file
    {
        #region Members

        private string _action;
        private string _file;

        #endregion

        #region Properties

        #region Serializable Properties

        #region public string action
        /// <summary>
        /// Get/Sets the action of the PluginDescription_install_file
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string action
        {
            get { return _action; }
            set { _action = value; }
        }
        #endregion

        #region public string file
        /// <summary>
        /// Get/Sets the file of the PluginDescription_install_file
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string file
        {
            get { return _file; }
            set { _file = value; }
        }
        #endregion

        #endregion

        #endregion
    } 
    #endregion

    #region public class PluginDescription_information
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public class PluginDescription_information
    {
        #region Members

        private string _plugin_name;
        private Guid _plugin_id;
        private string _plugin_version;
        private string _plugin_state;
        private string _plugin_type;
        private string _short_description;
        private string _default_image;
        private string _default_sound;
        private bool _is_COM;

        #endregion

        #region Properties

        #region Serializable Properties
        #region public string plugin_name
        /// <summary>
        /// Get/Sets the plugin_name of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string plugin_name
        {
            get { return _plugin_name; }
            set { _plugin_name = value; }
        }
        #endregion

        #region public Guid plugin_id
        /// <summary>
        /// Get/Sets the plugin_id of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public Guid plugin_id
        {
            get { return _plugin_id; }
            set { _plugin_id = value; }
        }
        #endregion

        #region public string plugin_version
        /// <summary>
        /// Get/Sets the plugin_version of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string plugin_version
        {
            get { return _plugin_version; }
            set { _plugin_version = value; }
        }
        #endregion

        #region public string plugin_state
        /// <summary>
        /// Get/Sets the plugin_state of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string plugin_state
        {
            get { return _plugin_state; }
            set { _plugin_state = value; }
        }
        #endregion

        #region public string plugin_type
        /// <summary>
        /// Get/Sets the plugin_type of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string plugin_type
        {
            get { return _plugin_type; }
            set { _plugin_type = value; }
        }
        #endregion

        #region public string short_description
        /// <summary>
        /// Get/Sets the short_description of the PluginDescription_information
        /// </summary>
        /// <value></value>
        public string short_description
        {
            get { return _short_description; }
            set { _short_description = value; }
        }
        #endregion

        #region public string default_image
        /// <summary>
        /// Get/Sets the default_image of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string default_image
        {
            get { return _default_image; }
            set { _default_image = value; }
        }
        #endregion

        #region public string default_sound
        /// <summary>
        /// Get/Sets the default_sound of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string default_sound
        {
            get { return _default_sound; }
            set { _default_sound = value; }
        }
        #endregion

        #region public bool is_COM
        /// <summary>
        /// Get/Sets the is_COM of the PluginDescription_information
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public bool is_COM
        {
            get { return _is_COM; }
            set { _is_COM = value; }
        }
        #endregion
        #endregion

        #endregion

    } 
    #endregion

    #region public class PluginDescription_documentation
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public class PluginDescription_documentation
    {
        #region Members

        private string _document_text;
        private string _document_file;

        #endregion

        #region Properties

        #region Serializable Properties

        #region public string document_text
        /// <summary>
        /// Get/Sets the document_text of the PluginDescription_documentation
        /// </summary>
        /// <value></value>
        public string document_text
        {
            get { return _document_text; }
            set { _document_text = value; }
        }
        #endregion

        #region public string document_file
        /// <summary>
        /// Get/Sets the document_file of the PluginDescription_documentation
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string document_file
        {
            get { return _document_file; }
            set { _document_file = value; }
        }
        #endregion

        #endregion

        #endregion

    } 
    #endregion

    #region public class PluginDescription_author
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public class PluginDescription_author
    {
        #region Members

        private string _author_name;
        private string _author_email;
        private string _author_site;
        private string _license_type;
        private string _copyright;

        #endregion

        #region Properties

        #region Serializable Properties

        #region public string author_name
        /// <summary>
        /// Get/Sets the author_name of the PluginDescription_author
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string author_name
        {
            get { return _author_name; }
            set { _author_name = value; }
        }
        #endregion

        #region public string author_email
        /// <summary>
        /// Get/Sets the author_email of the PluginDescription_author
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string author_email
        {
            get { return _author_email; }
            set { _author_email = value; }
        }
        #endregion

        #region public string author_site
        /// <summary>
        /// Get/Sets the author_site of the PluginDescription_author
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string author_site
        {
            get { return _author_site; }
            set { _author_site = value; }
        }
        #endregion

        #region public string license_type
        /// <summary>
        /// Get/Sets the license_type of the PluginDescription_author
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string license_type
        {
            get { return _license_type; }
            set { _license_type = value; }
        }
        #endregion

        #region public string copyright
        /// <summary>
        /// Get/Sets the copyright of the PluginDescription_author
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string copyright
        {
            get { return _copyright; }
            set { _copyright = value; }
        }
        #endregion

        #endregion

        #endregion
    } 
    #endregion

    #region public class PluginDescription_installation
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public class PluginDescription_installation
    {
        #region Members

        private bool _is_multi_package;
        private string _destination_folder;
        private string _main_file;
        private string _min_required_version;
        private string _max_required_version;
        private ArrayList _install_file;

        #endregion

        #region Properties

        #region Serializable Properties

        #region public bool is_multi_package
        /// <summary>
        /// Get/Sets the is_multi_package of the PluginDescription_installation
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public bool is_multi_package
        {
            get { return _is_multi_package; }
            set { _is_multi_package = value; }
        }
        #endregion

        #region public string destination_folder
        /// <summary>
        /// Get/Sets the destination_folder of the PluginDescription_installation
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string destination_folder
        {
            get { return _destination_folder; }
            set { _destination_folder = value; }
        }
        #endregion

        #region public string main_file
        /// <summary>
        /// Get/Sets the main_file of the PluginDescription_installation
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string main_file
        {
            get { return _main_file; }
            set { _main_file = value; }
        }
        #endregion

        #region public string min_required_version
        /// <summary>
        /// Get/Sets the min_required_version of the PluginDescription_installation
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string min_required_version
        {
            get { return _min_required_version; }
            set { _min_required_version = value; }
        }
        #endregion

        #region public string max_required_version
        /// <summary>
        /// Get/Sets the max_required_version of the PluginDescription_installation
        /// </summary>
        /// <value></value>
        [XmlAttribute()]
        public string max_required_version
        {
            get { return _max_required_version; }
            set { _max_required_version = value; }
        }
        #endregion

        #region public ArrayList install_file
        /// <summary>
        /// Get/Sets the install_file of the PluginDescription_installation
        /// </summary>
        /// <value></value>
        [XmlElement(Type = typeof(PluginDescription_install_file))]
        public ArrayList install_file
        {
            get { return _install_file; }
            set { _install_file = value; }
        }
        #endregion

        #endregion

        #endregion

        #region Constructors

        #region public PluginDescription_installation()
        /// <summary>
        /// Initializes a new instance of the <b>PluginDescription_installation</b> class.
        /// </summary>
        public PluginDescription_installation()
        {
            install_file = new ArrayList();
        }
        #endregion

        #endregion
    } 
    #endregion


}
