/**
 * ---------------------------------------------
 * TouchAPI_Server.cs
 * Description: Creates a TCP/UDP server for TouchAPI connections.
 * 
 * Class: 
 * System: Windows 7 Professional x64
 * Copyright 2010, Electronic Visualization Laboratory, University of Illinois at Chicago.
 * Author(s): Arthur Nishimoto
 * Version: 0.1
 * Version Notes:
 * 8/23/10      - Initial version
 * 9/13/10      - Added Touch Hold
 * ---------------------------------------------
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using OmegaWallConnector;

// Binary packet
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TouchAPI_PQServer
{
    public class Event
    {
        public int sourceID;
        public int eventID;
        public int serviceID;
        public int serviceType;

        public float[] dataArray;
        int dataArraySize = 0;
        public float[] position;
        public float[] rotation;
        public float timestamp;

        Event()
        {

        }

        public float getXPos()
        {
            if (position != null)
                return position[0];
            else
                return -1;
        }

        public float getYPos()
        {
            if (position != null)
                return position[1];
            else
                return -1;
        }

        public float getZPos()
        {
            if (position != null)
                return position[2];
            else
                return -1;
        }

        public int getServiceType()
        {
            return serviceType;
        }

        public int getSourceID()
        {
            return sourceID;
        }

        public void setTimeStamp(float ts)
        {
            timestamp = ts;
        }

        public long getTimeStamp()
        {
            return (long)timestamp;
        }

        public float getFloatData(int index)
        {
            if (dataArraySize > index)
                return (float)dataArray[index];
            else
                return -1;
        }

        public int getIntData(int index)
        {
            if (dataArraySize > index)
                return (int)dataArray[index];
            else
                return -1;
        }
    }

    public class TouchAPI_Server
    {
        private static Boolean ENABLE_HOLD = false;

        public enum OutputType { TacTile, OmegaLib_Legacy, OmegaLib, OSC };

        private OutputType outputMode = OutputType.OmegaLib_Legacy;

        // UdpClient port - TouchAPI message port
        private static int msgPort = 7340;
        IPEndPoint localEndPoint;

        // Server Thread which handles incoming connections
        private static Thread serverThread;
        private static Thread listenerThread;
        private static Thread holdThread;

        // Socket handling client input
        private static Socket listener;

        // UdpClient handling output to client
        private static UdpClient udpClient;

        private List<Client> clients;
        private static Semaphore clientLock;
        private List<Client> doneClients;

        private bool serverRunning = false;
        private static bool hasData = false; // If server has new touch data
        private static String touchAPI_dataString;
        private static String omegaLegacy_dataString;

        // Hold gesture implementation
        private static int[] ID;
        private static ArrayList ID_held;
        private static float[] ID_x, ID_y, ID_xW, ID_yW;
        private static Semaphore holdLock;

        private static GUI parent;

        public TouchAPI_Server(GUI p)
        {
            parent = p;
            clientLock = new Semaphore(1, 1);
            holdLock = new Semaphore(1, 1);

            clients = new List<Client>();
            doneClients = new List<Client>();
        }// CTOR

        public OutputType getOutputType()
        {
            return outputMode;
        }

        public void StartServer(int messagePort)
        {
            msgPort = messagePort;

            serverThread = new Thread(StartConnector);
            serverThread.Start();            
        }// startServer

        public void StopServer()
        {
            serverRunning = false;
            clients.Clear();
            listenerThread.Abort();
            serverThread.Abort();
            udpClient.Close();
            listener.Close();
        }// StopServer

        private void StartConnector()
        {
            // Header
            //Console.WriteLine("TouchAPI_Server 1.0");
            //Console.WriteLine("Copyright (C) 2010 Electronic Visualization Laboratory\nUniversity of Illinois at Chicago");
            //Console.WriteLine("======================================================");
            //Console.WriteLine("");

            parent.updateClientList(getClientList());

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            localEndPoint = new IPEndPoint(ipAddress, msgPort);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Prepare socket to be set when client connect
            udpClient = new UdpClient(msgPort);

            listener.Bind(localEndPoint);
            listenerThread = new Thread(Listen);
            listenerThread.Start();

            setHold(ENABLE_HOLD);

            serverRunning = true;
        }// StartConnector

        private void Listen()
        {
            Console.WriteLine("Listening for TouchAPI client on " + localEndPoint + " (MsgPort: " + msgPort + ")");
                
            while(serverRunning){
                listener.Listen(10);
                
                // Program is suspended while waiting for an incoming connection.
                Socket newSocket = listener.Accept();
                Client newClient = new Client(newSocket);
                //Console.WriteLine("Client {0} connected.", newClient.getAddress() );
                //Console.WriteLine("   Sending data to client on port {0}", newClient.getDataPort().ToString());
                addClient(newClient);
                //Console.WriteLine("   Clients now connected: {0}", clients.Count);
            }
        }// StartListening

        private void processHolds()
        {
            while (true)
            {
                holdLock.WaitOne();
                if (ID_held != null)
                {
                    try
                    {
                        foreach (ushort i in ID_held)
                        {
                            // 3 = GESTURE_HOLD
                            SendTouchData(ID[i], ID_x[i], ID_y[i], (float)0.5, ID_xW[i], ID_yW[i], 3);
                            SendTouchData(ID[i], ID_x[i], ID_y[i], (float)0.5);
                        }
                    }
                    catch (InvalidOperationException e) { }
                }
                holdLock.Release();
            }
        }// processHolds

        private void addClient(Client c)
        {
            bool newClient = true;
            clientLock.WaitOne();
            
            foreach (Client client in clients)
            {
                if (client.getDataPort() == c.getDataPort() && client.getAddress() == c.getAddress())
                {
                    newClient = false;

                    //Update client
                    Console.WriteLine("Existing client {0}:{1} updated.", c.getAddress(), c.getDataPort() );
                    client.Update(c);
                }
            }// foreach

            if (newClient)
            {
                clients.Add(c);
                Console.WriteLine("Client {0}:{1} added.", c.getAddress(), c.getDataPort());
                switch (c.getDataFormat())
                {
                    case (OutputType.TacTile):
                        Console.WriteLine("    Sending data using TouchAPI (TacTile) format");
                        break;
                    case (OutputType.OmegaLib_Legacy):
                        Console.WriteLine("    Sending data using OmegaLib Legacy format");
                        break;
                    case (OutputType.OmegaLib):
                        Console.WriteLine("    Sending data using OmegaLib format");
                        break;
                }
            }
            clientLock.Release();
            parent.updateClientList(getClientList());
        }// addClient

        public void addClient(String IPaddress, int dataPort)
        {
            Client c = new Client(IPaddress, dataPort, outputMode);
            bool newClient = true;
            clientLock.WaitOne();
            foreach (Client client in clients)
            {
                if (client.getDataPort() == c.getDataPort() && client.getAddress() == c.getAddress())
                {
                    newClient = false;
                }
            }// foreach
            if (newClient)
            {
                clients.Add(c);
                Console.WriteLine("Manually opened client " + c.getAddress() + ":" + dataPort);
                switch (outputMode)
                {
                    case (OutputType.TacTile):
                        Console.WriteLine("    Sending data using TouchAPI (TacTile) format");
                        break;
                    case (OutputType.OmegaLib_Legacy):
                        Console.WriteLine("    Sending data using OmegaLib Legacy format");
                        break;
                    case (OutputType.OmegaLib):
                        Console.WriteLine("    Sending data using OmegaLib format");
                        break;
                }

            }
            clientLock.Release();
            parent.updateClientList(getClientList());
        }// addClient

        public void removeClient(String IPaddress, int dataPort)
        {
            Client clientToRemove = null;
            Boolean removeClient = false;
            clientLock.WaitOne();
            foreach (Client client in clients)
            {
                if (client.getDataPort() == dataPort && client.getAddress() == IPaddress)
                {
                    clientToRemove = client;
                    removeClient = true;
                }
            }// foreach
            if (removeClient)
            {
                clients.Remove(clientToRemove);
                Console.WriteLine("Manually removed client " + IPaddress + ":" + dataPort);
            }
            clientLock.Release();
            parent.updateClientList(getClientList());
        }// removeClient

        private void sendToClients(String touchAPI_data, String omegaLibLegacy_data)
        {
            clientLock.WaitOne();
            doneClients.Clear();
            foreach (Client client in clients)
            {
                if (client.isActive())
                {
                    client.sendData(touchAPI_data, omegaLibLegacy_data);
                }
                else if (!client.isActive()) // Check for inactive clients
                {
                    doneClients.Add(client);
                }

            }

            // Remove inactive clients
            foreach (Client client in doneClients)
            {
                clients.Remove(client);
            }
            clientLock.Release();
        }// sendToClients

        /**
         * Converts touch information into TouchAPI format and indicates that there is new data
         * 
         **/
        public void SendTouchData(int touchID, float xPosRatio, float yPosRatio, float intensity)
        {
            //timeStamp + ":d:" + tp.id + "," + xPos_ratio + "," + yPos_ratio + "," + 1.0 + " ";
            DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

            touchAPI_dataString = timeStamp + ":d:" + touchID + "," + xPosRatio + "," + yPosRatio + "," + intensity + " ";
            omegaLegacy_dataString = "0:-1," + touchID + "," + xPosRatio + "," + yPosRatio + "," + intensity + "," + intensity + " ";

            hasData = true;
            sendToClients(touchAPI_dataString, omegaLegacy_dataString);
        }

        public void SendTouchData(int touchID, float xPosRatio, float yPosRatio, float intensity, float xWidth, float yWidth, int gesture)
        {
            //timeStamp + ":d:" + tp.id + "," + xPos_ratio + "," + yPos_ratio + "," + 1.0 + " ";
            DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

            touchAPI_dataString = timeStamp + ":q:" + touchID + "," + xPosRatio + "," + yPosRatio + "," + xWidth + "," + yWidth + "," + gesture + "," + intensity + " ";

            // Map PQLabs gesture IDs to OmegaLib
            switch (gesture)
            {
                case (1): gesture = 4; break; // move
                case (0): gesture = 5; break; // down
                case (2): gesture = 6; break; // up
            }
            // Flip y position for OmegaLib format (Flips y by default for TouchAPI)
            yPosRatio = 1.0f - yPosRatio;

            omegaLegacy_dataString = "0:" + gesture + "," + touchID + "," + xPosRatio + "," + yPosRatio + "," + xWidth + "," + yWidth + " ";

            hasData = true;
            sendToClients(touchAPI_dataString, omegaLegacy_dataString);
        }

        public void SendMocapData(int userID, int jointID, float xPos, float yPos, float zPos, float xQuat, float yQuat, float zQuat, float wQuat)
        {
            //DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            //long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

            // OmegaLib/Omicron typically only has one ID paraemeter followed by 3 position and 4 orientation.
            // This is modified to handle an additional ID parameter to distinguish user and joint IDs.
            //dataString = "8:" + userID + "," + jointID + "," + xPos + "," + yPos + "," + zPos + "," + xQuat + "," + yQuat + "," + zQuat + "," + wQuat + " ";

            touchAPI_dataString = " ";
            omegaLegacy_dataString = "8:" + userID + "," + jointID + "," + xPos + "," + yPos + "," + zPos + "," + xQuat + "," + yQuat + "," + zQuat + "," + wQuat + " ";

            hasData = true;
            sendToClients(touchAPI_dataString, omegaLegacy_dataString);
        }

        public void SendKinectSpeech(int sourceID, int commandID, int systemID, double sourceAngle, double sourceAngleConfidence)
        {
            //DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            //long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

            touchAPI_dataString = " ";
            omegaLegacy_dataString = "7:" + sourceID + "," + commandID + "," + systemID + "," + sourceAngle + " ";

            hasData = true;
            sendToClients(touchAPI_dataString, omegaLegacy_dataString);
        }

        public void SendKinectSpeech(int sourceID, String speechText, double sourceAngle, double sourceAngleConfidence)
        {
            //DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            //long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

            touchAPI_dataString = " ";
            omegaLegacy_dataString = "7:" + sourceID + "," + speechText + "," + sourceAngle + " ";

            hasData = true;
            sendToClients(touchAPI_dataString, omegaLegacy_dataString);
        }

        public void SendGestureString(string gestureData)
        {
            //timeStamp + ":d:" + tp.id + "," + xPos_ratio + "," + yPos_ratio + "," + 1.0 + " ";
            //DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            //long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

            //dataString = timeStamp + ":d:" + touchID + "," + xPosRatio + "," + yPosRatio + "," + intensity + " ";
            //dataString = timeStamp + ":l:" + gestureData + " ";
            //Console.WriteLine(dataString);
            //hasData = true;
            //sendToClients(dataString);
        }

        public void updateHoldData(int[] ids, ArrayList held, float[] x, float[] y, float[] xW, float[] yW)
        {
            holdLock.WaitOne();
            ID = ids;
            ID_held = held;
            ID_x = x;
            ID_y = y;
            ID_xW = xW;
            ID_yW = yW;
            holdLock.Release();
        }

        public void clearHolds()
        {
            holdLock.WaitOne();
            ID = null;
            ID_held = null;
            ID_x = null;
            ID_y = null;
            ID_xW = null;
            ID_yW = null;
            holdLock.Release();
        }
        public List<String> getClientList()
        {
            clientLock.WaitOne();
            List<String> clientList = new List<string>();
            foreach (Client client in clients)
            {
                String addressStr = client.getAddress() + ":" + client.getDataPort();
                clientList.Add(addressStr);
            }
            clientLock.Release();

            return clientList;
        }// getClientList

        public void setHold(Boolean value){
            ENABLE_HOLD = value;
            Console.WriteLine("Hold Gesture: "+ENABLE_HOLD);

            if (ENABLE_HOLD)
            {
                holdThread = new Thread(processHolds);
                holdThread.Start();
            }
            else if (!ENABLE_HOLD && holdThread != null && holdThread.IsAlive)
            {
                clearHolds();
                holdThread.Abort();
            }
        }

        public bool IsServerRunning()
        {
            return serverRunning;
        }

        public void SetMsgPort(int port)
        {
            msgPort = port;
        }

        public int GetMsgPort()
        {
            return msgPort;
        }
    }// class
}// namespace
