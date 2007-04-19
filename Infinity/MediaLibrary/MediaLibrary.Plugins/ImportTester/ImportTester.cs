using System;
using MediaLibrary;
using System.Collections.Generic;
using System.Text;

namespace ImportTester
{
    public class ImportTester : IMLImportPlugin
    {
        public bool GetProperties(IMLPluginProperties Properties)
        {
            IMLPluginProperty Prop = Properties.AddNew("TestProp1");
            {
                Prop.Caption = "This is only a test";
                Prop.DataType = "string";
                Prop.DefaultValue = string.Empty;
                Prop.HelpText = "";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("TestProp2");
            {
                Prop.Caption = "This is only a test2";
                Prop.DataType = "string";
                Prop.DefaultValue = "Default";
                Prop.HelpText = "";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("TestProp3");
            {
                Prop.Caption = "This is only a test3";
                Prop.DataType = "string";
                Prop.DefaultValue = string.Empty;
                Prop.HelpText = "";
                Prop.IsMandatory = false;
            }
            return true;
        }

        public bool SetProperties(IMLHashItem Properties, out string ErrorText)
        {
            ErrorText = "";
            return true;
        }

        public bool ValidateProperties(IMLPluginProperties Properties, IMLHashItem PropertyValues)
        {
            return true;
        }

        public bool EditCustomProperty(IntPtr Window, string PropertyName, ref string Value)
        {
            return true;
        }

        public bool Import(IMLSection Section, IMLImportProgress Progress)
        {
            Section.BeginUpdate();
            int i = 0;
            for (int genreCount = 0; genreCount < 10; genreCount++)
            {
                for (int artistCount = 0; artistCount < 10; artistCount++)
                {
                    for (int albumCount = 0; albumCount < 5; albumCount++)
                    {
                        for (int trackCount = 0; trackCount < 10; trackCount++)
                        {
                            //Find the percent done to show progress
                            int num = Convert.ToInt32((Convert.ToDouble(i++) / 50000) * 100);

                            //Create a new item
                            IMLItem Item = Section.AddNewItem("item-" + trackCount, "blah");
                            Item.Tags["Genre"] = "Genre_" + genreCount;
                            Item.Tags["Artist"] = "Artist_" + artistCount;
                            Item.Tags["Album"] = "Album_" + albumCount;
                            Item.Tags["Track"] = "Track_" + trackCount;

                            //Now we fill extra unique data to take up space, 
                            //to see if we can make the library take longer
                            for(int j = 1; j <= 20; j++)
                                Item.Tags["TAG" + j] = Guid.NewGuid().ToString();

                            //Save our item to the database
                            Item.SaveTags();
                            if (!Progress.Progress(num, Item.Name))
                                return false;
                        }
                    }
                }
            }
            Section.EndUpdate();
            return true;
        }
    }
}
