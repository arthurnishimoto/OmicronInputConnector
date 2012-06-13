/**
 * ---------------------------------------------
 * TouchManager.cs
 * Description: Manages incoming PQLabs touch data
 * 
 * Class: 
 * System: Windows 7 Professional x64
 * Copyright 2012, Electronic Visualization Laboratory, University of Illinois at Chicago.
 * Author(s): Arthur Nishimoto
 * Version: 0.1
 * Version Notes:
 * 1/23/12      - Moved from GUI.cs
 * ---------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Diagnostics;
using System.Runtime.InteropServices;

using PQ_SDK_MultiTouch;
using TouchAPI_PQServer;

using EPQT_Error = PQ_SDK_MultiTouch.PQMTClientImport.EnumPQErrorType;
using EPQT_TGesture = PQ_SDK_MultiTouch.PQMTClientImport.EnumPQTouchGestureType;
using EPQT_TPoint = PQ_SDK_MultiTouch.PQMTClientImport.EnumPQTouchPointType;
using EPQT_TRequest = PQ_SDK_MultiTouch.PQMTClientImport.EnumTouchClientRequestType;


namespace OmegaWallConnector
{
    class TouchManager
    {
        delegate void PFuncOnTouchGesture(ref PQMTClientImport.TouchGesture tg);

        static PFuncOnTouchGesture[] g_pf_on_tges = new PFuncOnTouchGesture[(int)EPQT_TGesture.TG_TOUCH_END + 1];

        // use static delegate to prevent the GC collect the CallBack functors;
        static PQMTClientImport.PFuncOnReceivePointFrame cur_rf_func = new PQMTClientImport.PFuncOnReceivePointFrame(OnReceivePointFrame);
        static PQMTClientImport.PFuncOnReceiveGesture cur_rg_func = new PQMTClientImport.PFuncOnReceiveGesture(OnReceiveGesture);
        static PQMTClientImport.PFuncOnServerBreak cur_svr_break = new PQMTClientImport.PFuncOnServerBreak(OnServerBreak);
        static PQMTClientImport.PFuncOnReceiveError cur_rcv_err_func = new PQMTClientImport.PFuncOnReceiveError(OnReceiveError);
        static PQMTClientImport.PFuncOnGetServerResolution cur_get_resolution = new PQMTClientImport.PFuncOnGetServerResolution(OnGetServerResolution);

        static int screenWidth = -1;
        static int screenHeight = -1;

        static string touchServer_ip = "131.193.77.116"; // OmegaTable "131.193.77.102" Tera5 "131.193.77.116"
        static Boolean serverConnected = false;

        static Boolean useTouchPoints = true;
        static Boolean usePQGestures = false;
        static Boolean useHoldGesture = false;

        static Boolean touchText = false;
        static Boolean generateLog = false;

        static int[] touchID;
        static ArrayList IDsHeld;
        static float[] ID_x, ID_y, ID_xW, ID_yW;
        static int maxTouches = 300; // Maximum touches of hardware
        static int nextID = 0;
        static int downSent = 0;
        static int upSent = 0;
        static int moveSent = 0;

        // Sets touch sensitivity
        static int move_threshold = 10; // in pixels
        static int max_blob_size = 60; // max blob width or height in pixels
        static int min_blob_size = 0;
        static DateTime baseTime;

        static TouchAPI_Server omegaDesk;
        static StreamWriter textOut;

        public TouchManager(GUI g, TouchAPI_Server o)
        {
            touchID = new int[maxTouches];
            IDsHeld = new ArrayList();
            ID_x = new float[maxTouches];
            ID_y = new float[maxTouches];
            ID_xW = new float[maxTouches];
            ID_yW = new float[maxTouches];
            omegaDesk = o;

            for (int i = 0; i < maxTouches; i++)
            {
                touchID[i] = 0;
                ID_x[i] = 0;
                ID_y[i] = 0;
            }
        }
        public bool InitAndConnectServer()
        {
            return InitAndConnectServer(touchServer_ip);
        }

        public bool InitAndConnectServer(string server_ip)
        {
            touchServer_ip = server_ip;
            int err_code = (int)EPQT_Error.PQMTE_SUCESS;
            try
            {
                // initialize
                InitFuncOnTG();

                // set the functions on receive
                SetFuncsOnReceiveProc();

                baseTime = DateTime.UtcNow;

                // connect server
                Console.WriteLine(" connecting to PQLabs server " + touchServer_ip);
                if ((err_code = PQMTClientImport.ConnectServer(touchServer_ip)) != (int)EPQT_Error.PQMTE_SUCESS)
                {
                    Console.WriteLine(" connect server fail, socket errror code:{0}", err_code);
                    return false;
                }

                // send request to server
                Console.WriteLine(" connect success, send request.");
                PQMTClientImport.TouchClientRequest tcq = new PQMTClientImport.TouchClientRequest();
                tcq.type = (int)EPQT_TRequest.RQST_RAWDATA_ALL |
                    (int)EPQT_TRequest.RQST_GESTURE_ALL;
                tcq.app_id = PQMTClientImport.GetTrialAppID();

                if ((err_code = PQMTClientImport.SendRequest(ref tcq)) != (int)EPQT_Error.PQMTE_SUCESS)
                {
                    Console.WriteLine(" send request fail, error code:{0}", err_code);
                    return false;
                }
                ////////// you can set the move_threshold when the tcq.type is RQST_RAWDATA_INSIDE;
                /*
                //int move_threshold = 1; // 1 pixel
                err_code = PQMTClientImport.SendThreshold(move_threshold);
                if (err_code != (int)EPQT_Error.PQMTE_SUCESS)
                {
                    Console.WriteLine(" send threshold fail, error code:{0}", err_code);
                    return err_code;
                }
                */
                //////////////////
                if ((err_code = PQMTClientImport.GetServerResolution(cur_get_resolution, IntPtr.Zero)) != (int)EPQT_Error.PQMTE_SUCESS)
                {
                    Console.WriteLine(" get server resolution fail,error code:{0}", err_code);
                    return false;
                };

                // start receiving
                Console.WriteLine(" send request success, start recv.");
                serverConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" exception: {0}", ex.Message);
            }
            return false;
        }
        static void InitFuncOnTG()
        {
            // initialize the call back functions of touch gestures;
            /*
            g_pf_on_tges[(int)EPQT_TGesture.TG_TOUCH_START] = (PFuncOnTouchGesture)OnTG_TouchStart;
            g_pf_on_tges[(int)EPQT_TGesture.TG_DOWN] = (PFuncOnTouchGesture)OnTG_Down;
            g_pf_on_tges[(int)EPQT_TGesture.TG_MOVE] = (PFuncOnTouchGesture)OnTG_Move;
            g_pf_on_tges[(int)EPQT_TGesture.TG_UP] = (PFuncOnTouchGesture)OnTG_Up;

            g_pf_on_tges[(int)EPQT_TGesture.TG_SECOND_DOWN] = (PFuncOnTouchGesture)OnTG_SecondDown;
            g_pf_on_tges[(int)EPQT_TGesture.TG_SECOND_UP] = (PFuncOnTouchGesture)OnTG_SecondUp;

            g_pf_on_tges[(int)EPQT_TGesture.TG_SPLIT_START] = (PFuncOnTouchGesture)OnTG_SplitStart;
            g_pf_on_tges[(int)EPQT_TGesture.TG_SPLIT_APART] = (PFuncOnTouchGesture)OnTG_SplitApart;
            g_pf_on_tges[(int)EPQT_TGesture.TG_SPLIT_CLOSE] = (PFuncOnTouchGesture)OnTG_SplitClose;
            g_pf_on_tges[(int)EPQT_TGesture.TG_SPLIT_END] = (PFuncOnTouchGesture)OnTG_SplitEnd;

            g_pf_on_tges[(int)EPQT_TGesture.TG_TOUCH_END] = (PFuncOnTouchGesture)OnTG_TouchEnd;
             */
        }

        static void SetFuncsOnReceiveProc()
        {
            PQMTClientImport.SetOnReceivePointFrame(cur_rf_func, IntPtr.Zero);
            PQMTClientImport.SetOnReceiveGesture(cur_rg_func, IntPtr.Zero);
            PQMTClientImport.SetOnServerBreak(cur_svr_break, IntPtr.Zero);
            PQMTClientImport.SetOnReceiveError(cur_rcv_err_func, IntPtr.Zero);
        }

        static void OnReceivePointFrame(int frame_id, int time_stamp, int moving_point_count, IntPtr moving_point_array, IntPtr call_back_object)
        {
            string[] tp_event = new string[3]
            {
                "down",
                "move",
                "up"
            };
            //if (touchText)
            //    Console.WriteLine("frame_id:{0},time_stamp:{1} ms,moving point count:{2}", frame_id, time_stamp, moving_point_count);
            for (int i = 0; i < moving_point_count; ++i)
            {
                IntPtr p_tp = (IntPtr)(moving_point_array.ToInt64() + i * Marshal.SizeOf(typeof(PQMTClientImport.TouchPoint)));
                PQMTClientImport.TouchPoint tp = (PQMTClientImport.TouchPoint)Marshal.PtrToStructure(p_tp, typeof(PQMTClientImport.TouchPoint));

                OnTouchPoint(ref tp);
            }
        }
        static void OnReceiveGesture(ref PQMTClientImport.TouchGesture gesture, IntPtr call_back_object)
        {
            OnTouchGesture(ref gesture);
        }

        static void OnServerBreak(IntPtr param, IntPtr call_back_object)
        {
            // when the server break, disconenct server;
            Console.WriteLine("server break, disconnect here");
            PQMTClientImport.DisconnectServer();
        }
        static void OnGetServerResolution(int x, int y, IntPtr call_back_object)
        {
            Console.WriteLine("server resolution:{0},{1}", x, y);
            screenWidth = x;
            screenHeight = y;
        }
        static void OnReceiveError(int err_code, IntPtr call_back_object)
        {
            switch (err_code)
            {
                case (int)EPQT_Error.PQMTE_RCV_INVALIDATE_DATA:
                    Console.WriteLine(" error: receive invalidate data.");
                    break;
                case (int)EPQT_Error.PQMTE_SERVER_VERSION_OLD:
                    Console.WriteLine(" error: the multi-touch server is old for this client, please update the multi-touch server.");
                    break;
                default:
                    Console.WriteLine(" socket error, socket error code:{0}", err_code);
                    break;
            }
        }

        // here, just record the position of point,
        //	you can do mouse map like "OnTG_Down" etc;
        static void OnTouchPoint(ref PQMTClientImport.TouchPoint tp)
        {
            switch ((EPQT_TPoint)tp.point_event)
            {
                case EPQT_TPoint.TP_DOWN:
                    touchID[tp.id] = nextID;
                    IDsHeld.Add(tp.id);
                    if (nextID < maxTouches - 100)
                    {
                        nextID++;
                    }
                    else
                    {
                        nextID = 0;
                    }
                    break;
                case EPQT_TPoint.TP_MOVE:
                    if (!IDsHeld.Contains(tp.id))
                        IDsHeld.Add(tp.id);
                    break;
                case EPQT_TPoint.TP_UP:
                    if (IDsHeld.Contains(tp.id))
                        IDsHeld.Remove(tp.id);
                    break;
            }
            // Format data into TacTile TouchAPI format
            float xPos_ratio = ((float)tp.x / (float)screenWidth);
            float yPos_ratio = ((float)screenHeight - (float)tp.y) / (float)screenHeight;
            float intensity = (float)0.5;
            int finger = (int)touchID[tp.id];
            float xWidth_ratio = ((float)tp.dx / (float)screenWidth);
            float yWidth_ratio = ((float)tp.dy / (float)screenHeight);


            ID_x[tp.id] = xPos_ratio;
            ID_y[tp.id] = yPos_ratio;
            ID_xW[tp.id] = xWidth_ratio;
            ID_yW[tp.id] = yWidth_ratio;
            if (useHoldGesture)
                omegaDesk.updateHoldData(touchID, IDsHeld, ID_x, ID_y, ID_xW, ID_yW);
            //Console.WriteLine("{0}, {1}", xWidth_ratio, yWidth_ratio);

            // Check for valid blob width and/or height
            if ((tp.dx <= max_blob_size && tp.dy <= max_blob_size))
            {
                // Send data to TouchAPI_Connector (timestamp is set there)
                omegaDesk.SendTouchData(finger, xPos_ratio, yPos_ratio, intensity, xWidth_ratio, yWidth_ratio, tp.point_event);
                omegaDesk.SendTouchData(finger, xPos_ratio, yPos_ratio, intensity);

                //DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
                long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

                //String touchAPI_output = timeStamp + ":q:" + tp.id + "," + xPos_ratio + "," + yPos_ratio + "," + 0.5;
                //Console.WriteLine("<{0}:q:{1},{2},{3},{4},{5},{6}>",timeStamp,tp.id,xPos_ratio,yPos_ratio,xWidth_ratio,yWidth_ratio,tp.point_event,intensity);
                if (generateLog)
                {
                    //.Console.WriteLine("{0} {1} ({2}) {3} {4} {5} {6} {7}", timeStamp, tp.id, finger, tp.point_event, xPos_ratio, yPos_ratio, xWidth_ratio, yWidth_ratio);
                    textOut.WriteLine("{0} {1} ({2}) {3} {4} {5} {6} {7}", timeStamp, tp.id, finger, tp.point_event, xPos_ratio, yPos_ratio, xWidth_ratio, yWidth_ratio);
                }

                switch ((EPQT_TPoint)tp.point_event)
                {
                    case EPQT_TPoint.TP_DOWN:
                        downSent++;
                        if (touchText && useTouchPoints)
                            Console.WriteLine("  point {0} come at ({1},{2}) width:{3} height:{4}", finger, tp.x, tp.y, tp.dx, tp.dy);
                        break;
                    case EPQT_TPoint.TP_MOVE:
                        moveSent++;
                        if (touchText && useTouchPoints)
                            Console.WriteLine("  point {0} move at ({1},{2}) width:{3} height:{4}", finger, tp.x, tp.y, tp.dx, tp.dy);
                        break;
                    case EPQT_TPoint.TP_UP:
                        upSent++;
                        if (touchText && useTouchPoints)
                            Console.WriteLine("  point {0} leave at ({1},{2}) width:{3} height:{4}", finger, tp.x, tp.y, tp.dx, tp.dy);
                        break;
                }

            }
            else
            {
                // Current touch is too large or existing touch has grown too large
                // Send up signal to clear touch
                omegaDesk.SendTouchData(finger, -xPos_ratio, -yPos_ratio, 0, 0, 0, 3);
                upSent++;
            }
            //Console.WriteLine("Sent: Down {0}, Move {1}, Up {2}, Total {3}", downSent, moveSent, upSent, downSent + moveSent + upSent);

        }

        // TouchGestures -----------------------------------------------

        static void OnTouchGesture(ref PQMTClientImport.TouchGesture tg)
        {
            if ((int)EPQT_TGesture.TG_NO_ACTION == tg.type)
                return;
            DefaultOnTG(ref tg);

            PFuncOnTouchGesture pf = (PFuncOnTouchGesture)g_pf_on_tges[(int)tg.type];
            if (null != pf)
            {
                pf(ref tg);
            }
        }

        static void DefaultOnTG(ref PQMTClientImport.TouchGesture tg) // just show the gesture
        {
            string name = Marshal.PtrToStringAnsi(PQMTClientImport.GetGestureName(ref tg));
            string dataString = "";
            if (touchText && usePQGestures)
                Console.Write("gesture:{0},type:{1},param size:{2} ", name, tg.type, tg.param_size);
            for (int i = 0; i < tg.param_size; ++i)
            {
                //double tmp = 0;
                //tg.param_s.GetValue(i,out tmp);
                if (touchText && usePQGestures)
                    Console.Write("{0} ", tg.param_s[i]);

                dataString += tg.param_s[i] + ",";
            }
            dataString += tg.type;
            if (usePQGestures)
                omegaDesk.SendGestureString(dataString);
            if (touchText && usePQGestures)
                Console.WriteLine("");
        }

        public void disconectServer()
        {
            Console.WriteLine("disconnect from PQLabs server...");
            PQMTClientImport.DisconnectServer();
            serverConnected = false;
            if (textOut != null)
                textOut.Close();
        }

        // Getters/Setters ---------------------------------------------
        public void SetServerIP(String ip)
        {
            touchServer_ip = ip;
        }

        public string GetServerIP()
        {
            return touchServer_ip;
        }

        public bool IsServerConnected()
        {
            return serverConnected;
        }

        public void SetTouchPoints(bool b)
        {
            useTouchPoints = b;
        }

        public bool IsTouchPointsEnabled()
        {
            return useTouchPoints;
        }

        public void SetPQGestures(bool b)
        {
            usePQGestures = b;
        }

        public bool IsPQGesturesEnabled()
        {
            return usePQGestures;
        }

        public void SetMoveThreshold(int value)
        {
            move_threshold = value;

            int err_code = PQMTClientImport.SendThreshold(move_threshold);
            if (err_code != (int)EPQT_Error.PQMTE_SUCESS)
            {
                Console.WriteLine(" send threshold fail, error code:{0}", err_code);
            }
        }

        public int GetMoveThreshold()
        {
            return move_threshold;
        }

        public void SetMaxBlobSize(int value)
        {
            max_blob_size = value;
        }

        public int GetMaxBlobSize()
        {
            return max_blob_size;
        }

        public void SetTouchText(bool b)
        {
            touchText = b;
        }

        public void SetGenerateLog(bool b)
        {
            generateLog = b;
        }
    }
}
