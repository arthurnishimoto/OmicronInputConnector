/**
 * ---------------------------------------------
 * KinectManager.cs
 * Description: Manages Kinect data using the Kinect SDK 
 * 
 * Class: 
 * System: Windows 7 Professional x64
 * Copyright 2012, Electronic Visualization Laboratory, University of Illinois at Chicago.
 * Author(s): Arthur Nishimoto
 * Version: 0.1
 * Version Notes:
 * 1/23/12      - Initial version
 * ---------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Collections; // Hashtable
using System.Linq;
using System.Text;

using System.IO;

using System.Windows.Threading; // For DispatcherTimer

using System.Windows.Media.Imaging; //
using Microsoft.Kinect;

// Needed to grab dll Reference from:
// C:\Windows\assembly\GAC_MSIL\Microsoft.Speech\11.0.0.0__31bf3856ad364e35\Microsoft.Speech.dll
// I assume this was loaded with the Kinect 1.0 SDK
using Microsoft.Speech;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

using OmicronSDKServer;
using System.Timers;
using omicron;
using Renci.SshNet;

namespace OmegaWallConnector
{
    class KinectManager
    {
        public enum SkeletonMode { TooMany, Off, Default, Seated };
        public enum SkeletonChooser { Default, Closest1Player, Closest2Player, Sticky1Player, Sticky2Player, MostActive1Player, MostActive2Player };

        static SkeletonMode skeletonMode = SkeletonMode.Default;

        static Hashtable kinectTable; // List of all KinectSensor objects
        private bool disabled = false;

        // ---- Elevation control ----
        public static Timer elevationTimer; // Timer used to limit sensor motor movement
        static int elevationTimeLimit = 1500; // Response time of motor in milliseconds (should be no less than 1 second)
        static int timerIncrement = 100; // in millis
        static int timeSinceLastElevationChange;
        static Boolean updateElevation = false;
        static int newElevation;
        static ArrayList newElevationKinectSensorList;

        static GUI gui;

        // ---- Skeleton Tracking ----
        // List of all skeletons (users) in current Kinect frame
        private int enabledSkeletonStreams = 0;
        private Skeleton[] skeletonData;

        // ---- Image Displays ----
        //private KinectColorViewer kinectColorViewer;
        private KinectDepthViewer kinectDepthViewer;
        //private KinectSkeletonViewer KinectSkeletonViewerOnDepth;

        // ---- Audio Processing ----
        private KinectSensor speechSensor; // At the moment, only the last connected kinect will do speech recognition
        private SpeechRecognitionEngine speechRecognizer;
        private Boolean voiceRecognitionEnabled = false;
        private Boolean voiceConsoleText = false;
        private Choices commandControlChoices;
        public ArrayList speechChoiceList = new ArrayList(); // String representation of grammar list
        private Choices speechChoices; // List speech grammar will be generated from (loaded from arraylist)
        private SpeechRecognitionEngine recognitionEngine;
        
        // ---- Output Streaming ----
        OmicronServer server;

        // ---- Timer ----
        private DispatcherTimer readyTimer;

        public KinectManager(GUI p, OmicronServer o)
        {
            try
            {
                kinectTable = new Hashtable();
                newElevationKinectSensorList = new ArrayList();

                //kinectColorViewer = new KinectColorViewer(p);
                kinectDepthViewer = new KinectDepthViewer(p);

                //KinectSkeletonViewerOnDepth = new KinectSkeletonViewer(p);
                //KinectSkeletonViewerOnDepth.ShowBones = true;
                //KinectSkeletonViewerOnDepth.ShowJoints = true;
                //KinectSkeletonViewerOnDepth.ShowCenter = true;
                //KinectSkeletonViewerOnDepth.ImageType = ImageType.Depth;

                //ShowBones="true" ShowJoints="true" ShowCenter="true" ImageType="Depth"
                server = o;
                gui = p;
                KinectStart();

                // Create a timer with a ten second interval.
                elevationTimer = new Timer(10000);

                // Hook up the Elapsed event for the timer.
                elevationTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

                elevationTimer.Interval = timerIncrement;
                elevationTimer.Enabled = true;
            }
            catch (Exception e)
            {
                //Console.WriteLine("KinectManager: Failed to find Kinect for Windows Runtime v1.6!");
                Console.WriteLine("KinectManager: Initialization exception: {0}", e.Message);
                Console.WriteLine("KinectManager: Disabling Kinect");
                disabled = true;
            }

        }// CTOR

        public bool IsDisabled()
        {
            return disabled;
        }

        // ------- Kinect Initializations ---------------------------------------------------------
        public void KinectStart()
        {
            // listen to any status change for Kinects.
            KinectSensor.KinectSensors.StatusChanged += this.KinectsStatusChanged;

            // show status for each sensor that is found now.
            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                this.ShowStatus(kinect, kinect.Status);
            }
        }

        public void KinectStop()
        {
            // listen to any status change for Kinects.
            KinectSensor.KinectSensors.StatusChanged += this.KinectsStatusChanged;

            // show status for each sensor that is found now.
            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                this.UninitializeKinectServices(kinect);
            }
        }

        // Callback function for Kinect status changes
        private void KinectsStatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.ShowStatus(e.Sensor, e.Status);
        }

        private void ShowStatus(KinectSensor kinectSensor, KinectStatus kinectStatus)
        {
            Console.WriteLine( "Kinect Sensor: " + kinectSensor.DeviceConnectionId + " " + kinectStatus + "\n" );
            
            if (kinectStatus == KinectStatus.Connected)
            {
                if( !kinectTable.ContainsKey(kinectSensor.DeviceConnectionId) )
                {
                    InitializeKinectServices(kinectSensor);

                    kinectTable.Add(kinectSensor.DeviceConnectionId, kinectSensor);
                    gui.SetKinectEnabled(true);
                    gui.addKinectSensorToList(kinectSensor.DeviceConnectionId);
                    speechSensor = kinectSensor;
                }
            }
            else if (kinectStatus == KinectStatus.Disconnected)
            {
                if (kinectTable.ContainsKey(kinectSensor.DeviceConnectionId))
                {
                    kinectTable.Remove(kinectSensor.DeviceConnectionId);
                    gui.removeKinectSensorToList(kinectSensor.DeviceConnectionId);
                }
                if (kinectTable.Count == 0)
                    gui.SetKinectEnabled(false);
            }
        }


        // Kinect enabled apps should customize which Kinect services it initializes here.
        private KinectSensor InitializeKinectServices(KinectSensor sensor)
        {
            // Application should enable all streams first.

            // Centralized control of the formats for Color/Depth
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

            switch(skeletonMode)
            {
                case(SkeletonMode.Off):
                    break;
                case(SkeletonMode.Seated): 
                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    break;
                default:
                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                    break;
            }

            // Inform the viewers of the Kinect KinectSensor.
            //kinectColorViewer.Kinect = sensor;
            kinectDepthViewer.Kinect = sensor;
            //KinectSkeletonViewerOnDepth.Kinect = sensor;

            // ---- Skeleton Service ----
            sensor.SkeletonFrameReady += this.SkeletonsReady;

            if (kinectTable.Count < 1)
            {
                enabledSkeletonStreams++;
                sensor.SkeletonStream.Enable(new TransformSmoothParameters()
                {
                    Smoothing = 0.5f,
                    Correction = 0.5f,
                    Prediction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f
                });
            }
            // ---- Audio Services ----
            speechRecognizer = this.CreateSpeechRecognizer();

            try
            {
                sensor.Start();
            }
            catch (IOException)
            {
                //SensorChooser.AppConflictOccurred();
                return null;
            }
            
            // Initialize AudioSource (KinectSensor Start() must be called before this)
            var audioSource = sensor.AudioSource;
            audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            var kinectStream = audioSource.Start();
            speechRecognizer.SetInputToAudioStream(
                kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            this.speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine("Initializing Kinect Audio Stream....");
            readyTimer = new DispatcherTimer();
            readyTimer.Tick += this.AudioReadyTick;
            readyTimer.Interval = new TimeSpan(0, 0, 4);
            readyTimer.Start();

            DisableVoiceInterface();

            Console.WriteLine("Kinect Elevation Range: "+sensor.MinElevationAngle + " " + sensor.MaxElevationAngle);
            Console.WriteLine("Kinect Current Elevation: "+sensor.ElevationAngle);

            return sensor;
        }

        void AudioReadyTick(object sender, EventArgs e)
        {
            Console.WriteLine("Kinect Voice Recognition Ready!");
            readyTimer.Stop();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            timeSinceLastElevationChange += timerIncrement;

            if (updateElevation && timeSinceLastElevationChange >= elevationTimeLimit)
            {
                Console.WriteLine("Kinect elevation changed to {0}", newElevation);
                updateElevation = false;

                foreach (String kinectSensorID in newElevationKinectSensorList )
                {
                    KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
                    if( kinect != null )
                        kinect.ElevationAngle = newElevation;
                }
                newElevationKinectSensorList.Clear();
            }

        }

        public int GetKinectElevation(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];

            if (kinect != null)
            {
                return kinect.ElevationAngle;
            }
            return 0;
        }

        public void SetKinectElevation(String kinectSensorID, int angle)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];

            if (kinect != null )
            {
                timeSinceLastElevationChange = 0;
                updateElevation = true;
                newElevation = angle;

                if( !newElevationKinectSensorList.Contains(kinectSensorID) )
                    newElevationKinectSensorList.Add(kinectSensorID);
            }
        }

        public void SetSkeletonModeDefault(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];

            if (kinect != null)
            {
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                if (!kinect.SkeletonStream.IsEnabled && enabledSkeletonStreams == 0)
                {
                    kinect.SkeletonStream.Enable();
                    enabledSkeletonStreams++;
                }
            }
        }

        public void SetSkeletonModeSeated(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
            if (kinect != null && enabledSkeletonStreams == 0 )
            {
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                if (!kinect.SkeletonStream.IsEnabled)
                {
                    kinect.SkeletonStream.Enable();
                    enabledSkeletonStreams++;
                }
            }
        }

        public void SetSkeletonModeOff(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
            if (kinect != null)
            {
                kinect.SkeletonStream.Disable();
                enabledSkeletonStreams--;
            }
        }

        public SkeletonMode GetSkeletonMode(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
            if (kinect != null)
            {
                if (!kinect.SkeletonStream.IsEnabled && enabledSkeletonStreams > 0)
                    return SkeletonMode.TooMany;

                if (kinect.SkeletonStream.IsEnabled && kinect.SkeletonStream.TrackingMode == SkeletonTrackingMode.Seated)
                    return SkeletonMode.Seated;
                else if (kinect.SkeletonStream.IsEnabled && kinect.SkeletonStream.TrackingMode == SkeletonTrackingMode.Default)
                    return SkeletonMode.Default;
            }
            return SkeletonMode.Off;
        }

        public void SetNearMode(String kinectSensorID, Boolean value)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
            if (kinect != null)
            {
                kinect.SkeletonStream.EnableTrackingInNearRange = value;

                try
                {
                    if (value)
                        kinect.DepthStream.Range = DepthRange.Near;
                    else
                        kinect.DepthStream.Range = DepthRange.Default;
                }
                catch (InvalidOperationException)
                {
                    kinect.DepthStream.Range = DepthRange.Default;
                }
            }
        }

        public bool GetNearMode(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
            if (kinect != null)
            {
                return kinect.SkeletonStream.EnableTrackingInNearRange;
            }
            return false;
        }

        public bool HasNearModeSupport(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
            try
            {
                kinect.DepthStream.Range = DepthRange.Near;
                kinect.DepthStream.Range = DepthRange.Default;
                return true;
            }
            catch (InvalidOperationException)
            {
                kinect.DepthStream.Range = DepthRange.Default;
            }
            return false;
        }

        public void SetDepthView(String kinectSensorID)
        {
            KinectSensor kinect = (KinectSensor)kinectTable[kinectSensorID];
            if (kinect != null)
            {
                kinectDepthViewer.Kinect = kinect;
            }
           
        }

        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            sensor.Stop();

            sensor.SkeletonFrameReady -= this.SkeletonsReady;
            kinectTable.Remove(sensor.DeviceConnectionId);
            gui.removeKinectSensorToList(sensor.DeviceConnectionId);

            kinectDepthViewer.Kinect = null;
            //KinectSkeletonViewerOnDepth.Kinect = null;

            if (speechRecognizer != null)
            {
                speechRecognizer.RecognizeAsyncCancel();
                speechRecognizer.RecognizeAsyncStop();
            }
            
            //enableAec.Visibility = Visibility.Collapsed;
        }

        // ---- Skeleton Tracking Functions -----------------------------------------------------------------------
        private void SkeletonsReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    int skeletonSlot = 0;

                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    // Loop through all skeletons (users) in the current frame
                    // Skeleton contains:
                    //  ClippedEdges
                    //  Joints
                    //  Position
                    //  TrackingId
                    //  TrackingState
                    foreach (Skeleton skeleton in this.skeletonData)
                    {
                        // Check if the skeleton is currently tracked
                        if (SkeletonTrackingState.Tracked == skeleton.TrackingState)
                        {
                            DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
                            long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

                            // Omicron (binary) data
                            MemoryStream ms = new MemoryStream();
                            BinaryWriter writer = new BinaryWriter(ms);
                            writer.Write((UInt32)timeStamp);     // Timestamp
                            writer.Write((UInt32)skeleton.TrackingId);    // sourceID
                            writer.Write((UInt32)0);    // serviceID
                            writer.Write((UInt32)EventBase.ServiceType.ServiceTypeMocap);    // serviceType
                            writer.Write((UInt32)EventBase.Type.Update);    // type
                            writer.Write((UInt32)0);    // flags

                            // Head pos
                            writer.Write((Single)skeleton.Joints[JointType.Head].Position.X);    // posx
                            writer.Write((Single)skeleton.Joints[JointType.Head].Position.Y);    // posy
                            writer.Write((Single)skeleton.Joints[JointType.Head].Position.Z);    // posz
                            writer.Write((Single)0);    // orx
                            writer.Write((Single)0);    // ory
                            writer.Write((Single)0);    // orz
                            writer.Write((Single)0);    // orw

                            writer.Write((UInt32)EventBase.ExtraDataType.ExtraDataVector3Array);    // extraDataType

                            int extraDataItems = 27;
                            writer.Write((UInt32)extraDataItems);    // extraDataItems


                            int myExtraDataValidMask= 0;

                            myExtraDataValidMask |= (1 << 0);
                            myExtraDataValidMask |= (1 << 1);
                            myExtraDataValidMask |= (1 << 6);
                            myExtraDataValidMask |= (1 << 7);
                            myExtraDataValidMask |= (1 << 8);
                            myExtraDataValidMask |= (1 << 9);
                            myExtraDataValidMask |= (1 << 11);
                            myExtraDataValidMask |= (1 << 12);
                            myExtraDataValidMask |= (1 << 13);
                            myExtraDataValidMask |= (1 << 14);
                            myExtraDataValidMask |= (1 << 16);
                            myExtraDataValidMask |= (1 << 17);
                            myExtraDataValidMask |= (1 << 18);
                            myExtraDataValidMask |= (1 << 19);
                            myExtraDataValidMask |= (1 << 21);
                            myExtraDataValidMask |= (1 << 22);
                            myExtraDataValidMask |= (1 << 23);
                            myExtraDataValidMask |= (1 << 24);
                            myExtraDataValidMask |= (1 << 25);
                            myExtraDataValidMask |= (1 << 26);

                            writer.Write((UInt32)myExtraDataValidMask);    // extraDataMask

                            // Extra data index 0
                            writer.Write((Single)skeleton.Joints[JointType.HipCenter].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.HipCenter].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.HipCenter].Position.Z);

                            // Extra data index 1
                            writer.Write((Single)skeleton.Joints[JointType.Head].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.Head].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.Head].Position.Z);

                            // Extra data index 2: Neck (OpenNI only)
                            writer.Write((Single)0);
                            writer.Write((Single)0);
                            writer.Write((Single)0);

                            // Extra data index 3: Torso (OpenNI only)
                            writer.Write((Single)0);
                            writer.Write((Single)0);
                            writer.Write((Single)0);

                            // Extra data index 4: Waist (OpenNI only)
                            writer.Write((Single)0);
                            writer.Write((Single)0);
                            writer.Write((Single)0);

                            // Extra data index 5: Left Collar (OpenNI only)
                            writer.Write((Single)0);
                            writer.Write((Single)0);
                            writer.Write((Single)0);

                            // Extra data index 6
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderLeft].Position.Z);

                            // Extra data index 7
                            writer.Write((Single)skeleton.Joints[JointType.ElbowLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.ElbowLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.ElbowLeft].Position.Z);

                            // Extra data index 8
                            writer.Write((Single)skeleton.Joints[JointType.WristLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.WristLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.WristLeft].Position.Z);

                            // Extra data index 9
                            writer.Write((Single)skeleton.Joints[JointType.HandLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.HandLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.HandLeft].Position.Z);

                            // Extra data index 10: Left Fingertip (OpenNI only)
                            writer.Write((Single)0);
                            writer.Write((Single)0);
                            writer.Write((Single)0);

                            // Extra data index 11
                            writer.Write((Single)skeleton.Joints[JointType.HipLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.HipLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.HipLeft].Position.Z);

                            // Extra data index 12
                            writer.Write((Single)skeleton.Joints[JointType.KneeLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.KneeLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.KneeLeft].Position.Z);

                            // Extra data index 13
                            writer.Write((Single)skeleton.Joints[JointType.AnkleLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.AnkleLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.AnkleLeft].Position.Z);

                            // Extra data index 14
                            writer.Write((Single)skeleton.Joints[JointType.FootLeft].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.FootLeft].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.FootLeft].Position.Z);

                            // Extra data index 15: Right Collar (OpenNI only)
                            writer.Write((Single)0);
                            writer.Write((Single)0);
                            writer.Write((Single)0);

                            // Extra data index 16
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderRight].Position.Z);

                            // Extra data index 17
                            writer.Write((Single)skeleton.Joints[JointType.ElbowRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.ElbowRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.ElbowRight].Position.Z);

                            // Extra data index 18
                            writer.Write((Single)skeleton.Joints[JointType.WristRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.WristRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.WristRight].Position.Z);

                            // Extra data index 19
                            writer.Write((Single)skeleton.Joints[JointType.HandRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.HandRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.HandRight].Position.Z);

                            // Extra data index 20: Right Fingertip (OpenNI only)
                            writer.Write((Single)0);
                            writer.Write((Single)0);
                            writer.Write((Single)0);

                            // Extra data index 21
                            writer.Write((Single)skeleton.Joints[JointType.HipRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.HipRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.HipRight].Position.Z);

                            // Extra data index 22
                            writer.Write((Single)skeleton.Joints[JointType.KneeRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.KneeRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.KneeRight].Position.Z);

                            // Extra data index 23
                            writer.Write((Single)skeleton.Joints[JointType.AnkleRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.AnkleRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.AnkleRight].Position.Z);

                            // Extra data index 24
                            writer.Write((Single)skeleton.Joints[JointType.FootRight].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.FootRight].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.FootRight].Position.Z);

                            // Extra data index 25
                            writer.Write((Single)skeleton.Joints[JointType.Spine].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.Spine].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.Spine].Position.Z);

                            // Extra data index 26
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderCenter].Position.X);
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderCenter].Position.Y);
                            writer.Write((Single)skeleton.Joints[JointType.ShoulderCenter].Position.Z);

                            

                            Byte[] omicronData = ms.GetBuffer();
                            server.SendOmicronEvent(omicronData);
                        }

                        skeletonSlot++;
                    }
                }
            }
        }

        // ---- General Kinect Audio Setup  -----------------------------------------------------------------------

        // ---- Audio Recognition Functions -----------------------------------------------------------------------
        public void EnableVoiceInterface(){
            voiceRecognitionEnabled = true;
            Console.WriteLine("\nVoice Interface Enabled");
            gui.SetVoiceInterfaceCheckbox(true);
        }

        public void DisableVoiceInterface()
        {
            voiceRecognitionEnabled = false;
            Console.WriteLine("\nVoice Interface Disabled");
            gui.SetVoiceInterfaceCheckbox(false);
        }

        public Boolean IsVoiceInterfaceEnabled()
        {
            return voiceRecognitionEnabled;
        }

        public void EnableVoiceConsoleText()
        {
            voiceConsoleText = true;
        }

        public void DisableVoiceConsoleText()
        {
            voiceConsoleText = false;
        }

        public Boolean IsVoiceConsoleTextEnabled()
        {
            return voiceConsoleText;
        }

        public enum Command
        {
            None = 0,
            Reset,
            Enable,
            Disable,
            Quit,
            Left,
            Right,
            Center
        }

        public enum Number
        {
            Zero = 0,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Mark
        }

        public enum SystemType
        {
            None = 0,
            SAGE,
            SAGENext,
            Touch,
            Voice,
            Screen,
            Mocap,
            Computer,
            Debug
        }

        struct WhatSaid
        {
            public Command command;
            public SystemType system;
        }

        static Dictionary<string, WhatSaid> SystemPhrases = new Dictionary<string, WhatSaid>()
        {
            {"SAGE", new WhatSaid()      {system = SystemType.SAGE}},
            {"SAGE Next", new WhatSaid()      {system = SystemType.SAGENext}},
            {"Touch", new WhatSaid()      {system = SystemType.Touch}},
            {"Wall", new WhatSaid()      {system = SystemType.Touch}},
            {"Screen", new WhatSaid()      {system = SystemType.Screen}},
            {"Voice Interface", new WhatSaid()      {system = SystemType.Voice}},
            {"Motion Tracking", new WhatSaid()      {system = SystemType.Mocap}},
            {"Computer", new WhatSaid()      {system = SystemType.Computer}},
            {"Debug Mode", new WhatSaid()      {system = SystemType.Debug}},
        };

        static Dictionary<string, WhatSaid> CommandPhrases = new Dictionary<string, WhatSaid>()
        {
            {"Enable", new WhatSaid()      {command = Command.Enable}},
            {"On", new WhatSaid()      {command = Command.Enable}},
            {"Activate", new WhatSaid()      {command = Command.Enable}},
            {"Disable", new WhatSaid()      {command = Command.Disable}},
            {"Off", new WhatSaid()      {command = Command.Disable}},
            {"Deactivate", new WhatSaid()      {command = Command.Disable}},
            {"Left", new WhatSaid()      {command = Command.Left}},
            {"Right", new WhatSaid()      {command = Command.Right}},
            {"Center", new WhatSaid()      {command = Command.Center}},
            {"End Program", new WhatSaid()      {command = Command.Quit}},
        };

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private void BuildCommandGrammar()
        {
            // Here we create the recognized speech library
            // Add all systems and command choices
            var systems = new Choices();
            foreach (var phrase in SystemPhrases)
                systems.Add(phrase.Key);

            var commands = new Choices();
            foreach (var phrase in CommandPhrases)
                commands.Add(phrase.Key);

            // Grammer for 'system command' format
            var systemCommandGrammer = new GrammarBuilder();
            systemCommandGrammer.Append(systems);
            systemCommandGrammer.Append(commands);

            // Grammer for 'command system' format
            var commandSystemGrammer = new GrammarBuilder();
            commandSystemGrammer.Append(commands);
            commandSystemGrammer.Append(systems);

            // Add command choices to class list
            commandControlChoices = new Choices();
            commandControlChoices.Add(systemCommandGrammer);
            commandControlChoices.Add(commandSystemGrammer);
        }

        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {
            RecognizerInfo ri = GetKinectRecognizer();
            recognitionEngine = new SpeechRecognitionEngine(ri.Id);
                       
            // Other commands - Recognition testing only
            // Real commands should be added to the dictionaries
            var otherChoices = new Choices();
            /*otherChoices.Add("red");
            otherChoices.Add("green");
            otherChoices.Add("blue");

            otherChoices.Add("Cyber-Commons");
            otherChoices.Add("XBox");
            otherChoices.Add("Kinect");

            otherChoices.Add("Star Wars");
            otherChoices.Add("Jedi");

            otherChoices.Add("USS Enterprise");
            otherChoices.Add("USS Reliant");

            otherChoices.Add("test");
            otherChoices.Add("Condition Green");
            otherChoices.Add("Yellow Alert");
            otherChoices.Add("Red Alert");
            otherChoices.Add("Battle Stations");
            otherChoices.Add("Fire");
            otherChoices.Add("Shields Up");
            otherChoices.Add("Raise Shields");
            otherChoices.Add("Shields Down");
            otherChoices.Add("Lower Shields");
            otherChoices.Add("Four Seven Alpha Tango");

            //otherChoices.Add("Destruct Sequence 1, code 1 1 A");
            //otherChoices.Add("Destruct Sequence 2, code 1 1 A 2 B");
            //otherChoices.Add("Destruct Sequence 3, code 1 B 2 B 3");
            otherChoices.Add("Begin auto destruct sequence");

            otherChoices.Add("Hello");
            otherChoices.Add("Doctor");
            otherChoices.Add("Name");
            otherChoices.Add("Continue");
            otherChoices.Add("Yesterday");
            otherChoices.Add("Tomorrow");

            // Greek Letters
            otherChoices.Add("alpha");
            otherChoices.Add("beta");
            otherChoices.Add("gamma");
            otherChoices.Add("delta");
            otherChoices.Add("epsilon");
            otherChoices.Add("zeta");
            otherChoices.Add("eta");
            otherChoices.Add("iota");
            otherChoices.Add("kappa");
            otherChoices.Add("lambda");
            // ...
            otherChoices.Add("omicron");
            // ...
            otherChoices.Add("sigma");
            // ...
            otherChoices.Add("omega");
            */
            
            // Loads choices added by config file
            foreach (String str in speechChoiceList)
            {
                speechChoices.Add(str);
            }

            BuildCommandGrammar();

            var allCommands = new Choices();
            allCommands.Add(commandControlChoices); // Add default control commands

            if (speechChoiceList.Count > 0)
                allCommands.Add(speechChoices);
            //allCommands.Add(otherChoices);

            

            // Final grammer builder for all commands
            var gb = new GrammarBuilder();

            //Specify the culture to match the recognizer in case we are running in a different culture.                                 
            gb.Culture = ri.Culture;
            gb.Append(allCommands);
            

            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            recognitionEngine.LoadGrammar(g);
            recognitionEngine.SpeechRecognized += this.SreSpeechRecognized;
            recognitionEngine.SpeechHypothesized += this.SreSpeechHypothesized;
            recognitionEngine.SpeechRecognitionRejected += this.SreSpeechRecognitionRejected;

            


            return recognitionEngine;
        }

        public void AddSpeechGrammarChoice(String choice)
        {
            if( !speechChoiceList.Contains(choice) )
                speechChoiceList.Add(choice);
        }

        public void RemoveSpeechGrammarChoice(String choice)
        {
            speechChoiceList.Remove(choice);
        }

        public void GenerateNewSpeechGrammar()
        {
            RecognizerInfo ri = GetKinectRecognizer();

            speechChoices = new Choices();

            foreach (String str in speechChoiceList)
            {
                speechChoices.Add(str);
            }
            BuildCommandGrammar();

            var allCommands = new Choices();
            allCommands.Add(commandControlChoices); // Add default control commands

            if( speechChoiceList.Count > 0 )
                allCommands.Add(speechChoices);

            // Final grammer builder for all commands
            var gb = new GrammarBuilder();

            //Specify the culture to match the recognizer in case we are running in a different culture.                                 
            gb.Culture = ri.Culture;
            gb.Append(allCommands);

            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            // Create a grammar from grammar definition XML file.
            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(OmicronInputConnector.Properties.Resources.SpeechGrammar)))
            {
                g = new Grammar(memoryStream);
            }

            // Update the grammar
            if (recognitionEngine != null)
            {
                recognitionEngine.RequestRecognizerUpdate();
                recognitionEngine.UnloadAllGrammars();

                // Load new grammar
                recognitionEngine.LoadGrammarAsync(g);
            }
        }

        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            RecognitionResult result = e.Result;
            string status = "Rejected: " + (result == null ? string.Empty : result.Text + " " + result.Confidence);
            if(voiceConsoleText)
                Console.WriteLine(status);
        }

        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            if (voiceConsoleText)
                Console.WriteLine("Hypothesized: " + e.Result.Text + " " + e.Result.Confidence);
        }

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string status = "Recognized: " + e.Result.Text + " " + e.Result.Confidence;
            
            double sourceAngle = speechSensor.AudioSource.SoundSourceAngle;
            double sourceAngleConfidence = speechSensor.AudioSource.SoundSourceAngleConfidence;

            status += " Source Angle " + sourceAngle + " " + sourceAngleConfidence;
            if (voiceConsoleText)
            {
                Console.WriteLine(status);
            }
            
            // Look for a match in the order of the lists below, first match wins.
            List<Dictionary<string, WhatSaid>> allDicts = new List<Dictionary<string, WhatSaid>>() { SystemPhrases, CommandPhrases };

            var said = new WhatSaid();

            SendKinectSpeech(e.Result.Text, e.Result.Confidence, (float)sourceAngle, (float)sourceAngleConfidence);

            float minConfidence = 0.70f;
            bool foundSystem = false;
            bool foundCommand = false;

            if (e.Result.Confidence >= minConfidence)
            {
                // Get the system phrase
                foreach (var phrase in SystemPhrases)
                {
                    if (e.Result.Text.Contains(phrase.Key))
                    {
                        said.system = phrase.Value.system;
                        //Console.WriteLine("\nRunning System: \t{0} Command:\t{1}", said.system, said.command);
                        foundSystem = true;
                    }
                }

                // Get the command phrase
                foreach (var phrase in CommandPhrases)
                {
                    if (e.Result.Text.Contains(phrase.Key))
                    {
                        said.command = phrase.Value.command;
                        foundCommand = true;
                    }
                }

                // ---- Enable/Disable Voice Recognition ----
                if (foundSystem && foundCommand && voiceRecognitionEnabled)
                {
                    if (said.system == SystemType.Voice && said.command == Command.Disable)
                    {
                        DisableVoiceInterface();
                    }
                    //else
                    //{
                        //Console.WriteLine("\nRunning Command \t'{1}' on \t{0}", said.system, said.command);
                        //server.SendKinectSpeech(0, (int)said.command, (int)said.system);
                    //}

                    if (said.system == SystemType.Debug && said.command == Command.Enable)
                    {
                        EnableVoiceConsoleText();
                    }
                    else if (said.system == SystemType.Debug && said.command == Command.Disable)
                    {
                        DisableVoiceConsoleText();
                    }
                }
                else if (foundSystem && foundCommand)
                {
                    if (said.system == SystemType.Voice && said.command == Command.Enable)
                    {
                        EnableVoiceInterface();
                    }
                }

                if (said.system != SystemType.None && said.command != Command.None)
                {
                    if (voiceConsoleText)
                    {
                        Console.WriteLine("\nRunning Command \t'{1}' on \t{0}", said.system, said.command);
                    }
                }
            }// if minConfidence
        }// SreSpeechRecognized

        private void SendKinectSpeech(String speechSaid, float speechAccuracy, float angle, float angleAccuracy)
        {
            DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
            long timeStamp = (DateTime.UtcNow - baseTime).Ticks / 10000;

            // Omicron (binary) data
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write((UInt32)timeStamp);     // Timestamp
            writer.Write((UInt32)0);    // sourceID
            writer.Write((UInt32)0);    // serviceID
            writer.Write((UInt32)EventBase.ServiceType.ServiceTypeSpeech);    // serviceType
            writer.Write((UInt32)EventBase.Type.Update);    // type
            writer.Write((UInt32)0);    // flags

            // Position/Rotation
            writer.Write((Single)speechAccuracy);    // posx
            writer.Write((Single)angle);    // posy
            writer.Write((Single)angleAccuracy);    // posz
            writer.Write((Single)0);    // orx
            writer.Write((Single)0);    // ory
            writer.Write((Single)0);    // orz
            writer.Write((Single)0);    // orw

            writer.Write((UInt32)EventBase.ExtraDataType.ExtraDataString);    // extraDataType

            // Extra data
            speechSaid += "\0";
            int extraDataItems = speechSaid.Length * sizeof(char);

            writer.Write((UInt32)extraDataItems);    // extraDataItems

            int myExtraDataValidMask = 0;
            //for (int i = 0; i < extraDataItems; i++)
            //    myExtraDataValidMask |= (1 << i);
            writer.Write((UInt32)myExtraDataValidMask);    // extraDataMask

            //byte[] strBuffer = new byte[omicronConnector.EventData.ExtraDataSize];
            //System.Buffer.BlockCopy(speechSaid.ToCharArray(), 0, strBuffer, 0, omicronConnector.EventData.ExtraDataSize);

            byte[] strBuffer = System.Text.Encoding.UTF8.GetBytes(speechSaid);

            writer.Write(strBuffer);

            Byte[] omicronData = ms.GetBuffer();
            server.SendOmicronEvent(omicronData);
        }// SendKinectSpeech
    }
}
