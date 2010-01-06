#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public class EncryptDecrypt
{
  public string Encrypt(string toEncrypt)
  {
    byte[] keyArray;
    byte[] toEncryptArray = UTF8Encoding.BigEndianUnicode.GetBytes(toEncrypt);
    bool useHashing = false;

    //System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
    // Get the key from config file

    //string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));
    string key = "MPcrypto";
    //System.Windows.Forms.MessageBox.Show(key);
    //If hashing use get hashcode regards to your key
    if (useHashing)
    {
      MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
      keyArray = hashmd5.ComputeHash(UTF8Encoding.BigEndianUnicode.GetBytes(key));
      //Always release the resources and flush data
      // of the Cryptographic service provide. Best Practice

      hashmd5.Clear();
    }
    else
      keyArray = UTF8Encoding.BigEndianUnicode.GetBytes(key);

    TripleDESCryptoServiceProvider tdes =
      new TripleDESCryptoServiceProvider();
    //set the secret key for the tripleDES algorithm
    tdes.Key = keyArray;
    //mode of operation. there are other 4 modes.
    //We choose ECB(Electronic code Book)
    tdes.Mode = CipherMode.ECB;
    //padding mode(if any extra byte added)

    tdes.Padding = PaddingMode.PKCS7;

    ICryptoTransform cTransform = tdes.CreateEncryptor();
    //transform the specified region of bytes array to resultArray
    byte[] resultArray =
      cTransform.TransformFinalBlock(toEncryptArray, 0,
                                     toEncryptArray.Length);
    //Release resources held by TripleDes Encryptor
    tdes.Clear();
    //Return the encrypted data into unreadable string format
    return Convert.ToBase64String(resultArray, 0, resultArray.Length);
  }

  public string Decrypt(string cipherString)
  {
    byte[] keyArray;
    //get the byte code of the string
    bool useHashing = false;

    byte[] toEncryptArray = Convert.FromBase64String(cipherString);

    //System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
    //Get your key from config file to open the lock!
    //string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));
    string key = "MPcrypto";

    if (useHashing)
    {
      //if hashing was used get the hash code with regards to your key
      MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
      keyArray = hashmd5.ComputeHash(UTF8Encoding.BigEndianUnicode.GetBytes(key));
      //release any resource held by the MD5CryptoServiceProvider

      hashmd5.Clear();
    }
    else
    {
      //if hashing was not implemented get the byte code of the key
      keyArray = UTF8Encoding.BigEndianUnicode.GetBytes(key);
    }

    TripleDESCryptoServiceProvider tdes =
      new TripleDESCryptoServiceProvider();
    //set the secret key for the tripleDES algorithm
    tdes.Key = keyArray;
    //mode of operation. there are other 4 modes. 
    //We choose ECB(Electronic code Book)

    tdes.Mode = CipherMode.ECB;
    //padding mode(if any extra byte added)
    tdes.Padding = PaddingMode.PKCS7;

    ICryptoTransform cTransform = tdes.CreateDecryptor();
    byte[] resultArray = cTransform.TransformFinalBlock(
      toEncryptArray, 0, toEncryptArray.Length);
    //Release resources held by TripleDes Encryptor                
    tdes.Clear();
    //return the Clear decrypted TEXT
    return UTF8Encoding.BigEndianUnicode.GetString(resultArray);
  }
}