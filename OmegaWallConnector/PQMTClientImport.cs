using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PQ_SDK_MultiTouch
{
    public class PQMTClientImport
    {
        //static int PQ_MT_CLIENT_VERSION = (0x0102);
        public enum EnumPQErrorType
        {
	        PQMTE_SUCESS = 0,
	        PQMTE_RCV_INVALIDATE_DATA	=	(0x31000001),    // the data received is invalidate,
													        //	may be the client receive the data from other application but pqmtserver;
            PQMTE_SERVER_VERSION_OLD	=	(0x31000002)    // the pqmtserver is too old for this version of client.
        }

        /// <summary>
        ///  point_event of TouchPoint
        /// </summary>
        public enum EnumPQTouchPointType
        {
	        TP_DOWN		= 0,
	        TP_MOVE		= 1,
	        TP_UP		= 2
        }

        /// <summary>
        /// type of TouchGesture
        /// </summary>
        public enum EnumPQTouchGestureType
        {
         // single point
         TG_TOUCH_START		    =0x0000,
         TG_DOWN				=0x0001,

         TG_MOVE				=0x0006,
         TG_UP				    =0x0007,
         TG_CLICK			    =0x0008,
         TG_DB_CLICK			=0x0009,
         // single big point
         TG_BIG_DOWN			=0x000a,
         TG_BIG_MOVE			=0x000b,
         TG_BIG_UP			    =0x000c,

         TG_MOVE_RIGHT		    =0x0011,
         TG_MOVE_UP			    =0x0012,
         TG_MOVE_LEFT		    =0x0013,
         TG_MOVE_DOWN		    =0x0014,

         // second point
         TG_SECOND_DOWN		    =0x0019,
         TG_SECOND_UP		    =0x001a,
         TG_SECOND_CLICK        =0x001b,
         TG_SECOND_DB_CLICK     =0x001c,

         // split
         TG_SPLIT_START		    =0x0020,
         TG_SPLIT_APART		    =0x0021,
         TG_SPLIT_CLOSE		    =0x0022,
         TG_SPLIT_END		    =0x0023,

         // rotate
         TG_ROTATE_START		=0x0024,
         TG_ROTATE_ANTICLOCK	=0x0025,
         TG_ROTATE_CLOCK		=0x0026,
         TG_ROTATE_END		    =0x0027,

         // near parrel
         TG_NEAR_PARREL_DOWN	=0x0028,
         TG_NEAR_PARREL_MOVE	=0x002d,
         TG_NEAR_PARREL_UP		=0x002e,
         TG_NEAR_PARREL_CLICK   =0x002f,
         TG_NEAR_PARREL_DB_CLICK = 0x0030,

         TG_NEAR_PARREL_MOVE_RIGHT	=0x0031,
         TG_NEAR_PARREL_MOVE_UP		=0x0032,
         TG_NEAR_PARREL_MOVE_LEFT	=0x0033,
         TG_NEAR_PARREL_MOVE_DOWN	=0x0034,

         // multi points
         TG_MULTI_DOWN		        =0x0035,
         TG_MULTI_MOVE		        =0x003a,
         TG_MULTI_UP			    =0x003b,

         TG_MULTI_MOVE_RIGHT		=0x003c,
         TG_MULTI_MOVE_UP			=0x003d,
         TG_MULTI_MOVE_LEFT			=0x003e,
         TG_MULTI_MOVE_DOWN			=0x003f,

         // 
         TG_TOUCH_END		        =0x0080,
         TG_NO_ACTION		        =0xffff
        }

        // type 0f TouchClientRequest
        public enum EnumTouchClientRequestType
        {
	        RQST_RAWDATA_INSIDE_ONLY = 0x00,
	        //
	        RQST_RAWDATA_INSIDE		=0x01,
	        RQST_RAWDATA_ALL		=0x02, 
	        RQST_GESTURE_INSIDE		=0x04, 
	        RQST_GESTURE_ALL		=0x08,
	        RQST_TRANSLATOR_CONFIG	    =0x10 
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct TouchPoint
        {
            public ushort   point_event;
            public ushort   id;
            public int      x;
            public int      y;
            public ushort   dx;
            public ushort   dy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TouchClientRequest
        {
            // request type
            public ushort type;
            // an sole id of your application, use TRIAL_APP_ID for trial
            public Guid app_id;
            // param for RQST_TRANSLATOR_CONFIG, it is the name of gesture translator which will be queried from the server;
            public string param;
        }

        const int MAX_TG_PARAM_SIZE = 6;

        [StructLayout(LayoutKind.Sequential)]
        public struct TouchGesture
        {
	        // type
	        public ushort	type;
	        // param size
	        public  ushort	param_size;
	        // params
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_TG_PARAM_SIZE)]
            public double [] param_s;
        }

        /// <summary>
        /// the action you want to take when receive the touch point frame;
        /// the touch points unmoving won't be sent from the server for the sake of efficency;
        /// The new touch point with its pointevent being TP_DOWN
        //	and the leaving touch point with its pointevent being TP_UP will be always sent from server;
        /// </summary>
        /// <param name="frame_id">the unique flag for the frame</param>
        /// <param name="time_stamp">the start time flag of this frame, in milli-seconds</param>
        /// <param name="point_count">the count of the moving or new/leaving points in this frame</param>
        /// <param name="point_array">the moving or new/leaving points data in this frame</param>
        /// <param name="call_back_object"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFuncOnReceivePointFrame([In]int frame_id, [In]int time_stamp, [In] int moving_point_count, [In] IntPtr moving_point_array, [In] IntPtr call_back_object);
        
        /// <summary>
        /// the action you want to take when receive the TouchGesture
        /// </summary>
        /// <param name="gesture"></param>
        /// <param name="call_back_object"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFuncOnReceiveGesture([In]ref TouchGesture gesture, [In]IntPtr call_back_object);
        
        /// <summary>
        /// the action you want to take when server interrupt the connection.
        /// </summary>
        /// <param name="param">reserved</param>
        /// <param name="call_back_object"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFuncOnServerBreak([In]IntPtr param, [In] IntPtr call_back_object);

        /// <summary>
        /// there may occur some network error while inter-communicate with the server, handle them here.
        /// </summary>
        /// <param name="error_code">this code is always the windows socket error_code except "PQMTE_RCV_INVALIDATE_DATA",
        /// PQMTE_RCV_INVALIDATE_DATA means that the received data from server is uncompleted,
        /// try to check for the network;
        /// </param>
        /// <param name="call_back_object"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFuncOnReceiveError([In]int error_code, [In] IntPtr call_back_object);

        /// <summary>
        /// usage: when the client and server is in different machine,you may need the display resolution of server;
        /// to get the display resolution of the server system, attention: not the resolution of touch screen.
        /// </summary>
        /// <param name="max_x"></param>
        /// <param name="max_y"></param>
        /// <param name="call_back_object"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PFuncOnGetServerResolution([In]int max_x, [In] int max_y, [In] IntPtr call_back_object);

        /// <summary>
        /// Connect to the multi-touch server.
        /// </summary>
        /// <param name="ip">the ip of server;</param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention=CallingConvention.Cdecl)]
        static extern public int ConnectServer([In]string ip);
        
        /// <summary>
        ///	After connect the multi-touch server successfully, send your request to the server.
        ///	The request tell the server which service you'd like to enjoy.
        ///		request: Request information to send to the server.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public int SendRequest([In] ref TouchClientRequest request);
 
        /// <summary>
        /// server won't send all the touch data for the default sensitivity maybe too highest
        /// to general client;
        /// for some special clients you may need the high sensitivity, you can send threshold by this function;
        /// </summary>
        /// <param name="move_threshold">
        /// for the request type of RQST_RAWDATA_INSIDE or RQST_GESTURE_INSIDE
        ///	it is the move threshold that will filter some points not move(the move_dis < threshold);
        ///	it is in pixel(the pixel in the coordinate of server);
        /// 0 for highest sensitivity in server;
        /// </param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public int SendThreshold([In]int move_threshold);

        /// <summary>
        /// to get the display resolution of the server system, attention: not the resolution of touch screen.
        /// </summary>
        /// <param name="pFnCallback"></param>
        /// <param name="call_back_object"></param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public int GetServerResolution([In]PFuncOnGetServerResolution pFnCallback, [In] IntPtr call_back_object);

        /// <summary>
        /// Disconnect from the multi-touch server.
        /// </summary>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public int DisconnectServer();

        /// <summary>
        /// Set the function that you want to execute while receiving the touch points in a frame.
        /// </summary>
        /// <param name="pf_on_rcv_frame">The function pointer you want to execute while receiving the touch frame.</param>
        /// <param name="call_back_object"></param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public PFuncOnReceivePointFrame SetOnReceivePointFrame([In]PFuncOnReceivePointFrame pf_on_rcv_frame, [In] IntPtr call_back_object);

        /// <summary>
        /// Set the function that you want to execute while receiving the touch gesture.
        /// </summary>
        /// <param name="pf_on_rcv_gesture">The function pointer you want to execute while receiving the touch gesture.</param>
        /// <param name="call_back_object"></param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public PFuncOnReceiveGesture SetOnReceiveGesture([In]PFuncOnReceiveGesture pf_on_rcv_gesture, [In] IntPtr call_back_object);

        /// <summary>
        ///  Set the function that you want to execute while receive the message that the server interrupt the connection.
        /// </summary>
        /// <param name="pf_on_svr_break"></param>
        /// <param name="call_back_object"></param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public PFuncOnServerBreak SetOnServerBreak([In]PFuncOnServerBreak pf_on_svr_break, [In] IntPtr call_back_object);

        /// <summary>
        /// Set the function that you want to execute while some errors occur during the receive process.
        /// </summary>
        /// <param name="pf_on_rcv_error"></param>
        /// <param name="call_back_object"></param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public PFuncOnReceiveError SetOnReceiveError([In]PFuncOnReceiveError pf_on_rcv_error,[In] IntPtr call_back_object);
        
        /// <summary>
        /// Get the touch gesture name of the touch gesture.
        /// </summary>
        /// <param name="tg"></param>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public IntPtr GetGestureName([In]ref TouchGesture tg);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport("PQMTClient.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern public Guid GetTrialAppID();
    };
}