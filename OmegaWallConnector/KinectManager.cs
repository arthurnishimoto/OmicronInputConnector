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
using System.Linq;
using System.Text;

using System.IO;

using System.Windows.Threading; // For DispatcherTimer

using System.Windows.Media.Imaging; //
using Microsoft.Kinect;

// Needed to grab dll Reference from:
// C:\Windows\assembly\GAC_MSIL\Microsoft.Speech\11.0.0.0__31bf3856ad364e35\Microsoft.Speech.dll
// I assume this was loaded with the Kinect 1.0 SDK
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

using TouchAPI_PQServer;
using System.Timers;

namespace OmegaWallConnector
{
    class KinectManager
    {
        public enum SkeletonMode { Off, Default, Seated };
        public enum SkeletonChooser { Default, Closest1Player, Closest2Player, Sticky1Player, Sticky2Player, MostActive1Player, MostActive2Player };

        static SkeletonMode skeletonMode = SkeletonMode.Default;

        // Kinect Sensor Interface (single Kinect use only - any use of this may break on multi-Kinect use)
        static KinectSensor kinect; // Currently used for audio and elevation control

        // ---- Elevation control ----
        public static Timer elevationTimer; // Timer used to limit sensor motor movement
        static int elevationTimeLimit = 1500; // Response time of motor in milliseconds (should be no less than 1 second)
        static int timerIncrement = 100; // in millis
        static int timeSinceLastElevationChange;
        static Boolean updateElevation = false;
        static int newElevation;

        int connectedKinects = 0;

        GUI gui;

        // ---- Skeleton Tracking ----
        // List of all skeletons (users) in current Kinect frame
        private Skeleton[] skeletonData;

        // ---- Image Displays ----
        //private KinectColorViewer kinectColorViewer;
        private KinectDepthViewer kinectDepthViewer;
        //private KinectSkeletonViewer KinectSkeletonViewerOnDepth;

        // ---- Audio Processing ----
        private SpeechRecognitionEngine speechRecognizer;
        private Boolean voiceRecognitionEnabled = false;
        private Boolean voiceConsoleText = false;

        // ---- Output Streaming ----
        TouchAPI_Server server;

        // ---- Timer ----
        private DispatcherTimer readyTimer;

        public KinectManager(GUI p, TouchAPI_Server o)
        {
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

        }// CTOR

        // ------- Kinect Initializations ---------------------------------------------------------
        public void KinectStart()
        {
            // listen to any status change for Kinects.
            KinectSensor.KinectSensors.StatusChanged += this.KinectsStatusChanged;

            // show status for each sensor that is found now.
            connectedKinects = 0;
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
                if (kinect == null)
                    kinect = kinectSensor;
                InitializeKinectServices(kinectSensor);
                connectedKinects++;

                gui.SetKinectEnabled(true);
            }
            else if (kinectStatus == KinectStatus.Disconnected)
            {
                connectedKinects--;

                if( connectedKinects < 1 )
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
            sensor.SkeletonStream.Enable(new TransformSmoothParameters()
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });

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
                kinect.ElevationAngle = newElevation;
            }
            
        }

        public int GetKinectElevation()
        {
            if (kinect != null)
                return kinect.ElevationAngle;
            else
                return 0;
        }

        public void SetKinectElevation(int angle)
        {
            if (kinect != null)
            {
                timeSinceLastElevationChange = 0;
                updateElevation = true;
                newElevation = angle;
            }
        }

        public void SetSkeletonModeDefault()
        {
            if (kinect != null)
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
        }

        public void SetSkeletonModeSeated()
        {
            if (kinect != null)
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
        }

        public SkeletonMode GetSkeletonMode()
        {
            return skeletonMode;
        }

        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            sensor.Stop();

            sensor.SkeletonFrameReady -= this.SkeletonsReady;

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
                            /*
                             * JointType Enum
                            HipCenter = 0,
                            Spine = 1,
                            ShoulderCenter = 2,
                            Head = 3,
                            ShoulderLeft = 4,
                            ElbowLeft = 5,
                            WristLeft = 6,
                            HandLeft = 7,
                            ShoulderRight = 8,
                            ElbowRight = 9,
                            WristRight = 10,
                            HandRight = 11,
                            HipLeft = 12,
                            KneeLeft = 13,
                            AnkleLeft = 14,
                            FootLeft = 15,
                            HipRight = 16,
                            KneeRight = 17,
                            AnkleRight = 18,
                            FootRight = 19,
                             * */
                            SendKinectData(skeleton.TrackingId, JointType.Head, skeleton.Joints[JointType.Head].Position);
                            //SendKinectDataLegacyOIS(JointType.ShoulderCenter, skeleton.Joints[JointType.ShoulderCenter].Position);
                            
                            //SendKinectDataLegacyOIS(JointType.ShoulderLeft, skeleton.Joints[JointType.ShoulderLeft].Position);
                            SendKinectData(skeleton.TrackingId, JointType.ElbowLeft, skeleton.Joints[JointType.ElbowLeft].Position);
                            //SendKinectDataLegacyOIS(JointType.WristLeft, skeleton.Joints[JointType.WristLeft].Position);
                            SendKinectData(skeleton.TrackingId, JointType.HandLeft, skeleton.Joints[JointType.HandLeft].Position);

                            //SendKinectDataLegacyOIS(JointType.ShoulderRight, skeleton.Joints[JointType.ShoulderRight].Position);
                            SendKinectData(skeleton.TrackingId, JointType.ElbowRight, skeleton.Joints[JointType.ElbowRight].Position);
                            //SendKinectDataLegacyOIS(JointType.WristRight, skeleton.Joints[JointType.WristRight].Position);
                            SendKinectData(skeleton.TrackingId, JointType.HandRight, skeleton.Joints[JointType.HandRight].Position);

                            SendKinectData(skeleton.TrackingId, JointType.HipCenter, skeleton.Joints[JointType.HipCenter].Position);

                            //SendKinectDataLegacyOIS(JointType.HipLeft, skeleton.Joints[JointType.HipLeft].Position);
                            SendKinectData(skeleton.TrackingId, JointType.KneeLeft, skeleton.Joints[JointType.KneeLeft].Position);
                            //SendKinectDataLegacyOIS(JointType.AnkleLeft, skeleton.Joints[JointType.AnkleLeft].Position);
                            SendKinectData(skeleton.TrackingId, JointType.FootLeft, skeleton.Joints[JointType.FootLeft].Position);

                            //SendKinectDataLegacyOIS(JointType.HipRight, skeleton.Joints[JointType.HipRight].Position);
                            SendKinectData(skeleton.TrackingId, JointType.KneeRight, skeleton.Joints[JointType.KneeRight].Position);
                            //SendKinectDataLegacyOIS(JointType.AnkleRight, skeleton.Joints[JointType.AnkleRight].Position);
                            SendKinectData(skeleton.TrackingId, JointType.FootRight, skeleton.Joints[JointType.FootRight].Position);

                        }

                        skeletonSlot++;
                    }
                }
            }
        }

        void SendKinectData(int userID, JointType jointType, SkeletonPoint skeletonPoint)
        {
            server.SendMocapData(userID, (int)jointType, skeletonPoint.X, skeletonPoint.Y, skeletonPoint.Z, 0, 0, 0, 0);
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

        public enum System
        {
            None = 0,
            SAGE,
            SAGENext,
            Touch,
            Voice,
            Screen,
            Mocap,
            Computer
        }

        struct WhatSaid
        {
            public Command command;
            public System system;
        }

        static Dictionary<string, WhatSaid> SystemPhrases = new Dictionary<string, WhatSaid>()
        {
            {"SAGE", new WhatSaid()      {system = System.SAGE}},
            {"SAGE Next", new WhatSaid()      {system = System.SAGENext}},
            {"Touch", new WhatSaid()      {system = System.Touch}},
            {"Wall", new WhatSaid()      {system = System.Touch}},
            {"Screen", new WhatSaid()      {system = System.Screen}},
            {"Voice Interface", new WhatSaid()      {system = System.Voice}},
            {"Motion Tracking", new WhatSaid()      {system = System.Mocap}},
            {"Computer", new WhatSaid()      {system = System.Computer}},
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

        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {
            RecognizerInfo ri = GetKinectRecognizer();
            SpeechRecognitionEngine sre = new SpeechRecognitionEngine(ri.Id);

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

            // Other commands - Recognition testing only
            // Real commands should be added to the dictionaries
            var otherChoices = new Choices();
            otherChoices.Add("red");
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

            // Numbers
            otherChoices.Add("One");
            otherChoices.Add("Two");
            otherChoices.Add("Three");
            otherChoices.Add("Four");
            otherChoices.Add("Five");
            otherChoices.Add("Six");
            otherChoices.Add("Seven");
            otherChoices.Add("Eight");
            otherChoices.Add("Nine");

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

            var allCommands = new Choices();
            allCommands.Add(systemCommandGrammer);
            allCommands.Add(commandSystemGrammer);
            allCommands.Add(otherChoices);

            // Final grammer builder for all commands
            var gb = new GrammarBuilder();

            //Specify the culture to match the recognizer in case we are running in a different culture.                                 
            gb.Culture = ri.Culture;
            gb.Append(allCommands);

            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            sre.LoadGrammar(g);
            sre.SpeechRecognized += this.SreSpeechRecognized;
            sre.SpeechHypothesized += this.SreSpeechHypothesized;
            sre.SpeechRecognitionRejected += this.SreSpeechRecognitionRejected;

            return sre;
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

            double sourceAngle = kinect.AudioSource.SoundSourceAngle;
            double sourceAngleConfidence = kinect.AudioSource.SoundSourceAngleConfidence;

            server.SendKinectSpeech(-1, e.Result.Text, sourceAngle, sourceAngleConfidence);

            status += " Source Angle " + sourceAngle + " " + sourceAngleConfidence;
            if (voiceConsoleText)
                Console.WriteLine(status);

            // Look for a match in the order of the lists below, first match wins.
            List<Dictionary<string, WhatSaid>> allDicts = new List<Dictionary<string, WhatSaid>>() { SystemPhrases, CommandPhrases };

            var said = new WhatSaid();

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
                    if (said.system == System.Voice && said.command == Command.Disable)
                    {
                        DisableVoiceInterface();
                    }
                    //else
                    //{
                        //Console.WriteLine("\nRunning Command \t'{1}' on \t{0}", said.system, said.command);
                        //server.SendKinectSpeech(0, (int)said.command, (int)said.system);
                    //}
                }
                else if (foundSystem && foundCommand)
                {
                    if (said.system == System.Voice && said.command == Command.Enable)
                    {
                        EnableVoiceInterface();
                    }
                }

                if (said.system != System.None && said.command != Command.None)
                {
                    if (voiceConsoleText)
                        Console.WriteLine("\nRunning Command \t'{1}' on \t{0}", said.system, said.command);
                    server.SendKinectSpeech(0, (int)said.command, (int)said.system, sourceAngle, sourceAngleConfidence);
                }
            }// if minConfidence
        }// SreSpeechRecognized


    }
}
