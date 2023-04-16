using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MPx86Proxy
{
    public class RequestHandler : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GlobalGetAtomName(ushort nAtom, System.Text.StringBuilder lpBuffer, int nSize);

        private class Client
        {
            public MemoryMappedFile File = null;
            public MemoryMappedViewAccessor FileAccesor = null;
            public byte[] Buffer = new byte[256];
            public List<object> Args = new List<object>();
        }

        private Dictionary<int, Client> _Clients = new Dictionary<int, Client>();

        private StringBuilder _Sb = new StringBuilder(256);


        public int HandleRequest(int iClientId)
        {
            int iResut = -1;
            Client client;

            //Get existing client
            if (!this._Clients.TryGetValue(iClientId, out client))
            {
                //Not found

                //Try get client's file name
                if (GlobalGetAtomName((ushort)iClientId, this._Sb, 256) > 0)
                {
                    //Got it
                    try
                    {
                        //Create file accessor
                        string strFile = this._Sb.ToString();
                        MemoryMappedFile f = MemoryMappedFile.OpenExisting(strFile);
                        MemoryMappedViewAccessor a = f.CreateViewAccessor();
                        client = new Client()
                        {
                            File = f,
                            FileAccesor = a
                        };
                        this._Clients.Add(iClientId, client);
                    }
                    catch (Exception ex)
                    {
                        Log.Log.Error("[HandleRequest] Error: " + ex.Message);
                        return -1;
                    }
                }
            }

            //Data size
            int iSize = client.FileAccesor.ReadInt32(0);

            byte[] data = client.Buffer;

            //Read data to client buffer
            client.FileAccesor.ReadArray<byte>(0, data, 0, iSize);

            int iIdx = 4;

            //Get driver name
            this._Sb.Clear();
            while (iIdx < iSize)
            {
                char c = (char)data[iIdx++];
                if (c == '\0')
                    break;

                this._Sb.Append(c);
            }

            if (this._Sb.Length == 0)
                goto resp;

            string strDriver = this._Sb.ToString();

            //Get method name
            this._Sb.Clear();
            while (iIdx < iSize)
            {
                char c = (char)data[iIdx++];
                if (c == '\0')
                    break;

                this._Sb.Append(c);
            }

            if (this._Sb.Length == 0)
                goto resp;

            string strMethod = this._Sb.ToString();

            //Get driver
            Type tDriver = Type.GetType("MPx86Proxy.Drivers." + strDriver);

            if (tDriver != null)
            {
                //Get method
                MethodInfo mi = tDriver.GetMethod(strMethod, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (mi != null)
                {
                    client.Args.Clear();

                    while ((iIdx + 1) < iSize)
                    {
                        ObjectTypeEnum code = (ObjectTypeEnum)data[iIdx++];

                        switch (code)
                        {
                            case ObjectTypeEnum.Byte:
                                if ((iIdx + 1) <= iSize)
                                {
                                    client.Args.Add((byte)data[iIdx]);
                                    iIdx++;
                                }
                                else
                                    goto resp;
                                break;

                            case ObjectTypeEnum.Short:
                                if ((iIdx + 2) <= iSize)
                                {
                                    client.Args.Add(BitConverter.ToInt16(data, iIdx));
                                    iIdx += 2;
                                }
                                else
                                    goto resp;
                                break;

                            case ObjectTypeEnum.Integer:
                                if ((iIdx + 4) <= iSize)
                                {
                                    client.Args.Add(BitConverter.ToInt32(data, iIdx));
                                    iIdx += 4;
                                }
                                else
                                    goto resp;
                                break;

                            case ObjectTypeEnum.Long:
                                if ((iIdx + 8) <= iSize)
                                {
                                    client.Args.Add(BitConverter.ToInt64(data, iIdx));
                                    iIdx += 8;
                                }
                                else
                                    goto resp;
                                break;

                            case ObjectTypeEnum.ULong:
                                if ((iIdx + 8) <= iSize)
                                {
                                    client.Args.Add(BitConverter.ToUInt64(data, iIdx));
                                    iIdx += 8;
                                }
                                else
                                    goto resp;
                                break;

                            case ObjectTypeEnum.String:
                                if ((iIdx + 4) <= iSize)
                                {
                                    int iLng = BitConverter.ToInt32(data, iIdx);
                                    iIdx += 4;

                                    if ((iIdx + iLng) <= iSize)
                                    {
                                        client.Args.Add(Encoding.UTF8.GetString(data, iIdx, iLng));
                                        iIdx += iLng;
                                    }
                                    else
                                        goto resp;
                                }
                                else
                                    goto resp;
                                break;


                            case ObjectTypeEnum.Array:
                                if ((iIdx + 5) <= iSize)
                                {
                                    ObjectTypeEnum t = (ObjectTypeEnum)data[iIdx++];

                                    int iLng = BitConverter.ToInt32(data, iIdx);
                                    iIdx += 4;

                                    object arg = null;

                                    switch (t)
                                    {
                                        case ObjectTypeEnum.Byte:
                                            if ((iIdx + iLng) <= iSize)
                                            {
                                                byte[] d = new byte[iLng];
                                                Buffer.BlockCopy(data, iIdx, d, 0, iLng);
                                                arg = d;
                                            }
                                            else
                                                goto resp;

                                            break;

                                        case ObjectTypeEnum.Integer:
                                            if ((iIdx + (iLng * 4)) <= iSize)
                                            {
                                                int[] d = new int[iLng];
                                                for (int i = 0; i < iLng; i++)
                                                {
                                                    d[i] = BitConverter.ToInt32(data, iIdx);
                                                    iIdx += 4;
                                                }
                                                arg = d;
                                            }
                                            else
                                                goto resp;

                                            break;
                                    }

                                    client.Args.Add(arg);

                                }
                                else
                                    goto resp;
                                break;

                            default:
                                goto resp;
                        }


                    }

                    //Invoke the native method
                    if (mi.GetParameters().Length == client.Args.Count)
                    {
                        try
                        {
                            object r = mi.Invoke(null, client.Args.ToArray());
                            if (r == null)
                                iResut = 1;
                            else if (r is bool)
                                iResut = (bool)r ? 1 : 0;
                            else
                                iResut = (int)r;
                        }
                        catch (Exception ex)
                        {
                            Log.Log.Error("[HandleRequest] Error: " + ex.Message);
                            iResut = -1;
                        }
                    }
                }
            }

        resp:
            client.FileAccesor.Write(0, iResut);
            return iResut;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<int, Client> pair in this._Clients)
            {
                pair.Value.FileAccesor.Dispose();
                pair.Value.File.Dispose();
            }

            this._Clients.Clear();
            this._Clients = null;
        }
    }
}
