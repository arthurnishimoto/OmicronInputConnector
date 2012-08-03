/**
 * ---------------------------------------------
 * Client.cs
 * Description: UDP client class.
 * 
 * Class: 
 * System: Windows 7 Professional x64
 * Copyright 2010, Electronic Visualization Laboratory, University of Illinois at Chicago.
 * Author(s): Arthur Nishimoto
 * Version: 0.1
 * Version Notes:
 * 8/23/10      - Initial version
 * ---------------------------------------------
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace TouchAPI_PQServer
{
    class Client
    {
        private static int clientsCreated = 0;
        private int clientID;
        private bool active = false;
        private Socket handler;
        private string clientAddress;
        private int dataPort;
        private UdpClient udpClient;

        private TouchAPI_Server.OutputType dataFormat = TouchAPI_Server.OutputType.Omicron_Legacy;

        // Incoming data from the client.
        private static string data = null;

        // Data buffer for incoming data.
        private static byte[] bytes = new Byte[1024];

        public Client(Socket acceptedListener)
        {
            clientID = clientsCreated;
            clientsCreated++;

            udpClient = new UdpClient();
            handler = acceptedListener;
            
            clientAddress = handler.RemoteEndPoint.ToString();

            // Parses message from TouchAPI for ready signal
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            data = data.Trim();

            // Checks if TouchAPI client is ready to receive data
            if (data.Contains("data_on"))
            {
                // Parse out the dataPort from the data string
                int beginPort = data.IndexOf(',') + 1;
                dataPort = Convert.ToInt32(data.Substring(beginPort, data.Length - beginPort));

                //Console.WriteLine("Client requests data to be sent on port " + dataPort + ".");

                // Parse client address and remove the port, leaving the IPAddress of client
                clientAddress = clientAddress.Substring(0, clientAddress.IndexOf(':'));
                //Console.WriteLine(clientAddress);
                IPAddress clientIP = IPAddress.Parse(clientAddress);
                

                // Connect to the client's dataport
                udpClient.Connect(clientIP, dataPort);
                active = true;

                if (data.Contains("omicron_legacy_data_on"))
                {
                    dataFormat = TouchAPI_Server.OutputType.Omicron_Legacy;
                    //Console.WriteLine("   " + clientAddress + " requested Omicron legacy data sent on port " + dataPort);
                }
                else
                {
                    dataFormat = TouchAPI_Server.OutputType.TacTile;
                    //Console.WriteLine("   " + clientAddress + " requested TouchAPI data sent on port " + dataPort);
                }
                data = "";
            }
        }// CTOR

        public Client(String clientAddr, int dataPrt, TouchAPI_Server.OutputType outputType)
        {
            clientID = clientsCreated;
            clientsCreated++;

            udpClient = new UdpClient();

            // Parse client address and remove the port, leaving the IPAddress of client
            String parsedIP = clientAddr;
            clientAddress = clientAddr;
            dataPort = dataPrt;

            IPAddress clientIP = IPAddress.Parse(parsedIP);
            data = "";

            // Connect to the client's dataport
            udpClient.Connect(clientIP, dataPort);
            active = true;

            switch (outputType)
            {
                case (TouchAPI_Server.OutputType.TacTile):
                    dataFormat = TouchAPI_Server.OutputType.TacTile;
                    break;
                case (TouchAPI_Server.OutputType.Omicron_Legacy):
                    dataFormat = TouchAPI_Server.OutputType.Omicron_Legacy;
                    break;
            }
        }// CTOR

        public void Update(Client c)
        {
            if (c.getAddress() == clientAddress && c.getDataPort() == dataPort)
            {
                if (dataFormat != c.getDataFormat())
                {
                    dataFormat = c.getDataFormat();
                    //Console.WriteLine("Existing client "+clientAddress+":"+dataPort+" data format updated:");
                    switch (dataFormat)
                    {
                        case (TouchAPI_Server.OutputType.TacTile):
                            Console.WriteLine("    Requested data using TouchAPI (TacTile) format");
                            break;
                        case (TouchAPI_Server.OutputType.Omicron_Legacy):
                            Console.WriteLine("    Requested data using Omicron Legacy format");
                            break;
                        case (TouchAPI_Server.OutputType.Omicron):
                            Console.WriteLine("    Requested data using Omicron format");
                            break;
                    }
                }
            }
        }

        public void process()
        {
            // Check if client has sent any data back
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            data = data.Trim();
            
            //Console.WriteLine(data);
        }// process

        public void sendData(String touchAPI_dataString, String omegaLegacy_dataString)
        {
            String dataString;
            switch (dataFormat)
            {
                case(TouchAPI_Server.OutputType.TacTile):
                    dataString = touchAPI_dataString;
                    break;
                case (TouchAPI_Server.OutputType.Omicron_Legacy):
                    dataString = omegaLegacy_dataString;
                    break;
                default:
                    dataString = omegaLegacy_dataString;
                    break;
            }
            try
            {
                // Sends data string to TouchAPI data port
                //Console.WriteLine("Sending message '{0}' to {1}", dataString, clientAddress);
                byte[] msg = Encoding.ASCII.GetBytes(dataString);
                udpClient.Send(msg, msg.Length);
                //Console.WriteLine(dataString);
                if (!handler.Connected)
                {
                    Console.WriteLine(clientAddress + " disconnected.");
                    active = false;
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(clientAddress + " disconnected.");
                active = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }// listen

        public bool isActive()
        {
            return active;
        }// isActive

        public String getAddress()
        {
            return clientAddress;
        }// getAddress

        public int getDataPort()
        {
            return dataPort;
        }// getDataPort

        public TouchAPI_Server.OutputType getDataFormat()
        {
            return dataFormat;
        }// getDataFormat

    }// class
}// namespace
