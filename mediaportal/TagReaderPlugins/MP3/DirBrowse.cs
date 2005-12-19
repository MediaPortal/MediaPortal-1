/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

// Copyright(C) 2002 Hugo Rumayor Montemayor, All rights reserved.
namespace id3
{
    using System;
	using System.IO;
	using System.Xml;

    public class DirBrowse
    {
		XmlTextWriter _writer;

        public void Browse(string[] args)
        {
			Stream stream = File.Create(@"c:\music.xml");
			_writer = new XmlTextWriter(stream,System.Text.Encoding.UTF8);
			
            DirectoryInfo dir;
			DateTime start = System.DateTime.Now;
			try
			{
				if (args.Length == 0)
					dir = new DirectoryInfo(Directory.GetCurrentDirectory());
				else
				{
					string directory = args[0];
					if (directory[directory.Length - 1] == '"')
						directory = directory.Substring(0, directory.Length - 1);

					dir = new DirectoryInfo(directory);				
				}
				_writer.WriteRaw("<?xml version=\"1.0\" encoding=\"uft-8\"?>\r\n");
				_writer.Indentation = 3;
				_writer.WriteStartElement("Catalog");
				_writer.WriteAttributeString("xmlns", "", null, "urn:1");
				IterateFiles(dir);
				_writer.WriteEndElement();
				_writer.Flush();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				if (args.Length == 0)
					Console.WriteLine("Error reading directory.");
				else
					Console.WriteLine("Error reading directory \"{0}\"", args[0]);
			}

			DateTime end = System.DateTime.Now;

			Console.WriteLine();
			Console.WriteLine("Elapsed time: {0}s", (end - start));
        }

		protected void IterateFiles(DirectoryInfo dir)
		{
			foreach (FileSystemInfo fileSystemInfo in dir.GetFileSystemInfos())
			{
				if (fileSystemInfo is FileInfo)
				{
					FileInfo fileInfo = (FileInfo)fileSystemInfo;
					if (fileInfo.Extension.ToLower() == ".mp3")
					{
						FileStream stream = File.OpenRead(fileInfo.FullName);
						Console.WriteLine("Parsing: {0}.",fileInfo.Name);
						Tags tag = new Tags();
						try
						{
							tag.Deserialize(stream);
						}
						catch(Exception e)
						{
							Console.WriteLine("Error: {0}",e.Message);
							ID3v1 id3v1 = new ID3v1();
							try
							{
								id3v1.Deserialize(stream);
								tag = id3v1.Tags;
							}
							catch
							{
								Console.WriteLine("ID3v1 tag error");
							}
						}
						try
						{
							Frame frame = new Frame(tag.Header);
							_writer.WriteStartElement("Tag","urn:1");
							foreach(RawFrame rawFrame in tag)
							{
								try
								{
									frame.Parse(rawFrame);
									_writer.WriteElementString(rawFrame.Tag,"urn:1",frame.ToString());
								}
								catch(Exception e)
								{
									Console.WriteLine("Unknown Tag {0}, in File: {1}.",rawFrame.Tag,fileInfo.Name);
									Console.WriteLine("Error: {0}",e.Message);
								}
							}
							_writer.WriteEndElement();
							_writer.WriteRaw("\r\n"); // Write  enter/EOL
						}
						catch(Exception e)
						{
							Console.WriteLine("Error: "+e.ToString());
						}
					}
				}
				else if (fileSystemInfo is DirectoryInfo)
				{
					IterateFiles((DirectoryInfo)fileSystemInfo);
				}
			}
		}
	}
}
