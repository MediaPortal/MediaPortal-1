using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace MpeCore.Classes.ZipProvider
{
    public class ZipProviderClass
    {
        private List<string> _tempFileList = new List<string>();
        private ZipFile _zipPackageFile = null;

        public PackageClass Load(string zipfile)
        {
            try
            {
                PackageClass pak = new PackageClass();
                _zipPackageFile = ZipFile.Read(zipfile);
                string tempPackageFile = Path.GetTempFileName();
                FileStream fs = new FileStream(tempPackageFile, FileMode.Create);
                _zipPackageFile["MediaPortalExtension.xml"].Extract(fs);
                fs.Close();
                pak.Load(tempPackageFile);
                _tempFileList.Add(tempPackageFile);
                foreach (FileItem fileItem in pak.UniqueFileList.Items)
                {
                    if(fileItem.SystemFile)
                    {
                        string tempfil = Path.GetTempFileName();
                        Extract(fileItem, tempfil);
                        fileItem.TempFileLocation = tempfil;
                        fileItem.LocalFileName = tempfil;
                        _tempFileList.Add(tempfil);
                    }
                }
                pak.ZipProvider = this;
                return pak;

            }
            catch (Exception)
            {
                if(_zipPackageFile!=null)
                    _zipPackageFile.Dispose();
                return null;
            }
        }

        public DateTime FileDate(FileItem item)
        {
            return _zipPackageFile[item.ZipFileName].LastModified;
        }

        public bool Extract(FileItem item, string extractLocation)
        {
            if (File.Exists(item.TempFileLocation))
                File.Copy(item.TempFileLocation, extractLocation);
            FileStream fs = new FileStream(extractLocation, FileMode.Create);
            _zipPackageFile[item.ZipFileName].Extract(fs);
            fs.Close();
            File.SetCreationTime(extractLocation,FileDate(item));
            File.SetLastAccessTime(extractLocation, FileDate(item));
            File.SetLastWriteTime(extractLocation, FileDate(item));
            item.TempFileLocation = extractLocation;
            return true;
        }

        public bool Save(PackageClass pak, string filename)
        {
            string temfile = Path.GetTempFileName();
            pak.Save(temfile);
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(temfile).FileName = "MediaPortalExtension.xml";

                foreach (FileItem fileItem in pak.UniqueFileList.Items )
                {
                    zip.AddFile(fileItem.LocalFileName).FileName = fileItem.ZipFileName;
                }

                zip.Save(filename);
            }
            File.Delete(temfile);
            return true;
        }

        private void Clear()
        {
            if (_zipPackageFile != null)
                _zipPackageFile.Dispose();
            foreach (string s in _tempFileList)
            {
                try
                {
                    File.Delete(s);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
