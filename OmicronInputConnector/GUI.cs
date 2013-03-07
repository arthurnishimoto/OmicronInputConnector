/**
 * ---------------------------------------------
 * GUI.cs
 * Description: TouchAPI Connector GUI class
 * 
 * Class: 
 * System: Windows 7 Professional x64
 * Copyright 2010, Electronic Visualization Laboratory, University of Illinois at Chicago.
 * Author(s): Arthur Nishimoto
 * Version: 0.1
 * Version Notes:
 * 6/1/10      - Initial version
 * 6/3/10      - TouchAPI_Server support
 * 6/4/10      - TouchIDs now recycle
 * 6/9/10      - Added max blob size slider
 * 6/23/10     - Touch hold added
 * 8/23/10     - Added client config option, added remove client button
 * ---------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;

using OmicronSDKServer;

namespace OmegaWallConnector
{
    public partial class GUI : Form
    {
        static OmicronServer omegaDesk;
        static TouchManager touchManager;
        static KinectManager kinectManager;

        static StreamWriter textOut;

        static Boolean touchText = false;
        static Boolean generateLog = false;

        delegate void SetListBoxCallback(List<String> text);

        public GUI()
        {
            InitializeComponent();
            SetKinectEnabled(false);

            omegaDesk = new OmicronServer(this); // Handles client connections
            touchManager = new TouchManager(this, omegaDesk); // PQLabs touch
            kinectManager = new KinectManager(this, omegaDesk); // Kinect via Kinect for Windows SDK

            // Default GUI values
            touchServerList.Text = touchManager.GetServerIP();
            messagePortBox.Text = omegaDesk.GetMsgPort().ToString();

            pqGestureBox.Checked = touchManager.IsPQGesturesEnabled();
            touchPointBox.Checked = touchManager.IsTouchPointsEnabled();

            voiceRecogCheckBox.Checked = kinectManager.IsVoiceInterfaceEnabled();
            kinectAudioDebugTextCheckBox.Checked = kinectManager.IsVoiceConsoleTextEnabled();

            if( kinectSensorListBox.Items.Count > 0 )
                kinectSensorListBox.SelectedIndex = 0;

            //if (kinectManager.GetKinectElevation() <= 27 && kinectManager.GetKinectElevation() >= -27)
            //{
            //    kinectElevationBar.Value = kinectManager.GetKinectElevation();
            //    elevationLabel.Text = kinectManager.GetKinectElevation().ToString();
            //}

            thresholdBar.Value = touchManager.GetMoveThreshold();
            thresholdNumLabel.Text = touchManager.GetMoveThreshold().ToString();

            maxBlobSizeBar.Value = touchManager.GetMaxBlobSize();
            maxBlobSizeNumLabel.Text = touchManager.GetMaxBlobSize().ToString();

            bool serverOnStartup = false; // Config state to start server on startup?


            // Check for libraries
            if (touchManager.IsDisabled())
                touchGroupBox.Enabled = false;
            if (kinectManager.IsDisabled())
            {
                kinectTabControl.Enabled = false;
            }

            // Read the config file
            try
            {
                String line;
                System.IO.StreamReader file = new System.IO.StreamReader("config.cfg");
                bool readingServerList = false;
                bool readingClientList = false;
                bool readingKinectSpeechList = false;

                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains('\t')) // Remove leading tabs
                        line = line.Substring(line.IndexOf('\t') + 1, line.Length - 1);

                    if (!line.StartsWith("//")) // Ignore comment lines
                    {
                        if (line.Contains("//")) // Remove trailing comments
                            line = line.Substring(0, line.IndexOf('/') - 1);

                        if (line.Contains("ServerUpOnStartup") && line.Contains("true") )
                        {
                            serverOnStartup = true;
                            continue;
                        }

                        if (line.Contains("EnableTouch") && line.Contains("true"))
                        {
                            touchEnableBox.Checked = true;
                        }

                        if (line.Contains("UseTouchPoints") && line.Contains("true"))
                        {
                            touchManager.SetTouchPoints(true);
                            continue;
                        }

                        if (line.Contains("UseTouchGestures") && line.Contains("true"))
                        {
                            touchManager.SetPQGestures(true);
                            continue;
                        }

                        if (line.Contains("MaxBlobSize") )
                        {
                            int lineLength = line.Length - line.IndexOf('=') - 3;
                            line = line.Substring(line.IndexOf('=') + 2, lineLength);
                            setMaxBlobSize(Convert.ToInt16(line, 10));
                            continue;
                        }

                        if (line.Contains("MessagePort"))
                        {
                            int lineLength = line.Length - line.IndexOf('=') - 3;
                            line = line.Substring(line.IndexOf('=') + 2, lineLength);
                            
                            int msgPort = (Convert.ToInt16(line, 10));
                            messagePortBox.Items.Add(msgPort);
                            messagePortBox.Text = msgPort.ToString();
                            omegaDesk.SetMsgPort(msgPort);
                            continue;
                        }

                        if (line.Contains("SERVER"))
                        {
                            readingServerList = true;
                            continue;
                        }
                        else if (line.Contains("CLIENT"))
                        {
                            readingClientList = true;
                            continue;
                        }
                        else if (line.Contains("SPEECH_CHOICES"))
                        {
                            readingKinectSpeechList = true;
                            continue;
                        }
                        else if (line.Contains("}"))
                        {
                            readingServerList = false;
                            readingClientList = false;
                            readingKinectSpeechList = false;
                        }
                        if (readingServerList && !line.Contains("{") && !line.Contains("}"))
                        {
                            touchServerList.Items.Add(line);
                            touchServerList.Text = line;
                        }
                        if (readingClientList && !line.Contains("{") && !line.Contains("}"))
                        {
                            touchClientList.Items.Add(line);
                            touchClientList.Text = line;
                        }
                        if (readingKinectSpeechList && !line.Contains("{") && !line.Contains("}"))
                        {
                            kinectManager.AddSpeechGrammarChoice(line);
                        }
                    }
                }

            }
            catch (FileNotFoundException e)
            {
            }

            // Set startup config settings
            if (serverOnStartup)
            {
                connectServerButton_Click(null, null);
            }
            pqGestureBox.Checked = touchManager.IsPQGesturesEnabled();
            touchPointBox.Checked = touchManager.IsTouchPointsEnabled();

            if (touchEnableBox.Checked)
            {
                touchManager.InitAndConnectServer();
            }

            switch (omegaDesk.getOutputType())
            {
                case (OmicronServer.OutputType.TacTile):
                    tacTileRadioButton.Checked = true;
                    break;
                case (OmicronServer.OutputType.Omicron):
                    oinputserverRadioButton.Checked = true;
                    break;
                case (OmicronServer.OutputType.Omicron_Legacy):
                    oinputLegacyRadioButton.Checked = true;
                    break;
            }


            // Update speech grammar and refresh GUI list
            kinectManager.GenerateNewSpeechGrammar();
            speechGrammarListBox.Items.Clear();
            foreach( String s in kinectManager.speechChoiceList )
                speechGrammarListBox.Items.Add(s);

            //switch (kinectManager.GetSkeletonMode())
            //{
            //    case (KinectManager.SkeletonMode.Off):
            //        kinectSkeletonOffButton.Checked = true;
            //        break;
            //    case (KinectManager.SkeletonMode.Default):
            //        kinectSkeletonDefaultButton.Checked = true;
            //        break;
            //    case (KinectManager.SkeletonMode.Seated):
            //        kinectSkeletonSeatedButton.Checked = true;
            //        break;
            //}
        }

        private void GUI_FormClosed(Object sender, FormClosedEventArgs e)
        {

            System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "CloseReason", e.CloseReason);
            messageBoxCS.AppendLine();
            MessageBox.Show(messageBoxCS.ToString(), "FormClosed Event");
            Console.WriteLine("FormClosed Event");
        }

        // GUI -----------------------------------------------------------

        private void connectServerButton_Click(object sender, EventArgs e)
        {
            if (!omegaDesk.IsServerRunning())
            {
                //touchManager.InitAndConnectServer();
                //if (touchManager.IsServerConnected())
                //{
                omegaDesk.StartServer(omegaDesk.GetMsgPort());
                    //omegaDesk.setHold(useHoldGesture);
                connectServerButton.Text = "Stop Server";
                //}
            }
            else
            {
                connectServerButton.Text = "Start Server";
                //touchManager.disconectServer();
                Console.WriteLine("shutting down TouchAPI server...");
                omegaDesk.StopServer();
            }
        }

        private void debugTextButton_Click(object sender, EventArgs e)
        {
            if (!touchText)
            {
                debugTextButton.Text = "Hide Touch Text";
                touchText = true;
            }
            else
            {
                debugTextButton.Text = "Show Touch Text";
                touchText = false;
            }
            touchManager.SetTouchText(touchText);
        }

        // Update client message port on combo box change
        private void messagePortBox_TextChanged(object sender, EventArgs e)
        {
            omegaDesk.SetMsgPort( Convert.ToInt32(messagePortBox.Text) );
        }

        // Update server ip when combo box changed
        private void touchServerList_TextChanged(object sender, EventArgs e)
        {
            touchManager.SetServerIP( touchServerList.Text );
        }


        // Update what data is sent / displayed on console
        private void touchPointBox_CheckedChanged(object sender, EventArgs e)
        {
            touchManager.SetTouchPoints( touchPointBox.Checked );
        }

        // Update what data is sent / displayed on console
        private void pqGestureBox_CheckedChanged(object sender, EventArgs e)
        {
            touchManager.SetPQGestures( pqGestureBox.Checked );
        }

        private void sensitivityBar_Scroll(object sender, EventArgs e)
        {
            thresholdNumLabel.Text = thresholdBar.Value.ToString();
            touchManager.SetMoveThreshold( thresholdBar.Value );
        }

        private void GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if( generateLog )
                textOut.Close();

            // Close managers
            kinectManager.KinectStop();

            Environment.Exit(0);
        }

        private void maxBlobSizeBar_Scroll(object sender, EventArgs e)
        {
            maxBlobSizeNumLabel.Text = maxBlobSizeBar.Value.ToString();
            touchManager.SetMaxBlobSize( maxBlobSizeBar.Value );
        }

        private void setMaxBlobSize(int size)
        {
            maxBlobSizeNumLabel.Text = size.ToString();
            maxBlobSizeBar.Value = size;
            touchManager.SetMaxBlobSize( size );
        }

        // Thread-safe method for updating clientList
        public void updateClientList(List<String> clientList)
        {
            if (this.clientListBox.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                SetListBoxCallback d = new SetListBoxCallback(AddClientList);
                this.Invoke
                    (d, new object[] { clientList });
            }
            else
            {
                // It's on the same thread, no need for Invoke
                this.clientListBox.DataSource = clientList;
            } 
        }

        private void AddClientList(List<String> clientList)
        {
            this.clientListBox.DataSource = clientList;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            String clientInfo = touchClientList.Text;
            //try
            //{
                if (clientInfo.IndexOf(':') > 0)
                {
                    String address = clientInfo.Substring(0, clientInfo.IndexOf(':'));
                    int dataPort = Convert.ToInt16(clientInfo.Substring(clientInfo.IndexOf(':') + 1, clientInfo.Length - address.Length - 1));
                    omegaDesk.addClient(address, dataPort);
                }
            //}
            //catch (Exception e1)
            //{
            //    Console.WriteLine("Invalid client data");
            //}
        }

        private void logCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            generateLog = logCheckBox.Checked;
            touchManager.SetGenerateLog(generateLog);
            if (generateLog)
            {
                String fileName = DateTime.UtcNow.Year + "-" + DateTime.UtcNow.Month + "-" + DateTime.UtcNow.Day + "-" + DateTime.UtcNow.Hour + "-" + DateTime.UtcNow.Minute + "-" + DateTime.UtcNow.Second + "-" + DateTime.UtcNow.Millisecond + ".txt";
                FileInfo t = new FileInfo(fileName);
                textOut = t.CreateText();
            }
            else if( textOut != null )
            {
                textOut.Close();
            }

        }

        private void removeClientButton_Click(object sender, EventArgs e)
        {
            String clientInfo = (String)clientListBox.SelectedItem; // Should be in IP_Address:dataPort format i.e. 127.0.0.1:7000
            if (clientInfo != null)
            {
                String clientAddress = clientInfo.Substring(0, clientInfo.IndexOf(':'));
                int clientPort = Convert.ToInt16(clientInfo.Substring(clientInfo.IndexOf(':') + 1, (clientInfo.Length - 1 - clientAddress.Length)));
                omegaDesk.removeClient(clientAddress, clientPort);
            }
        }

        private void voiceRecogCheckBox_Click(object sender, EventArgs e)
        {
            if (kinectManager.IsVoiceInterfaceEnabled())
            {
                kinectManager.DisableVoiceInterface();
            }
            else
            {
                kinectManager.EnableVoiceInterface();
            }
        }

        public void SetVoiceInterfaceCheckbox(bool value)
        {
            voiceRecogCheckBox.Checked = value;
        }

        public void SetKinectEnabled(bool value)
        {
            kinectGroupBox1.Enabled = value;
            kinectGroupBox2.Enabled = value;
            kinectEnabledCheckBox.Checked = value;
        }

        private void kinectEnabledCheckBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (kinectEnabledCheckBox.Checked)
            {
                kinectManager.KinectStart();
            }
            else
            {
                kinectManager.KinectStop();
            }
        }

        private void touchEnableBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (touchManager.IsServerConnected())
            {
                touchManager.disconectServer();
                touchEnableBox.Checked = false;
            }
            else
            {
                touchEnableBox.Checked = touchManager.InitAndConnectServer();
            }
        }

        private void kinectAudioDebugTextCheckBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (kinectManager.IsVoiceConsoleTextEnabled())
            {
                Console.WriteLine("Kinect Voice Console Text Disabled");
                kinectManager.DisableVoiceConsoleText();
                kinectAudioDebugTextCheckBox.Checked = false;
            }
            else
            {
                Console.WriteLine("Kinect Voice Console Text Enabled");
                kinectManager.EnableVoiceConsoleText();
                kinectAudioDebugTextCheckBox.Checked = true;
            }
        }

        private void kinectElevationBar_ValueChanged(object sender, EventArgs e)
        {
            foreach (object itemChecked in kinectSensorListBox.SelectedItems)
            {
                kinectManager.SetKinectElevation(itemChecked.ToString(),kinectElevationBar.Value);
            }
           
            elevationLabel.Text = kinectElevationBar.Value.ToString();
        }

        private void kinectSkeletonDefaultButton_MouseClick(object sender, MouseEventArgs e)
        {
            
            if (kinectSkeletonDefaultButton.Checked)
            {
                Console.WriteLine("Kinect Skeleton Mode: Default");
                foreach (object itemChecked in kinectSensorListBox.SelectedItems)
                {
                    kinectManager.SetSkeletonModeDefault(itemChecked.ToString());
                }
            }
        }

        private void kinectSkeletonSeatedButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (kinectSkeletonSeatedButton.Checked)
            {
                Console.WriteLine("Kinect Skeleton Mode: Seated");
                foreach (object itemChecked in kinectSensorListBox.SelectedItems)
                {
                    kinectManager.SetSkeletonModeSeated(itemChecked.ToString());
                }
            }
        }

        private void kinectSkeletonOffButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (kinectSkeletonOffButton.Checked)
            {
                Console.WriteLine("Kinect Skeleton Mode: Off");
                foreach (object itemChecked in kinectSensorListBox.SelectedItems)
                {
                    kinectManager.SetSkeletonModeOff(itemChecked.ToString());
                }
            }
        }

        public void updateKinectAngle(int angle)
        {
            kinectCurrentAngleLabel.Text = angle.ToString();
        }

        public void addKinectSensorToList(string kinectSensorID)
        {
            kinectSensorListBox.Items.Add(kinectSensorID);

            if ( kinectManager != null && kinectSensorListBox.Items.Count == 1)
                kinectSensorListBox.SelectedIndex = 0;
        }

        public void removeKinectSensorToList(string kinectSensorID)
        {
            kinectSensorListBox.Items.Remove(kinectSensorID);
        }

        private void kinectSensorListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Console.WriteLine("kinectSensorListBox_SelectedIndexChanged");

            foreach (object itemChecked in kinectSensorListBox.SelectedItems)
            {
                // Get the selected sensor's current elevation
                kinectCurrentAngleLabel.Text = kinectManager.GetKinectElevation(itemChecked.ToString()).ToString();

                // Get the selected sensor's depth image
                kinectManager.SetDepthView(itemChecked.ToString());

                //  Get the selected sensor's near mode
                if (kinectManager.HasNearModeSupport(itemChecked.ToString()))
                {
                    kinectNearModeCheckBox.Enabled = true;
                    kinectNearModeCheckBox.Checked = kinectManager.GetNearMode(itemChecked.ToString());
                }
                else
                {
                    kinectNearModeCheckBox.Enabled = false;
                }

                // Get the selected sensor's tracking mode
                switch (kinectManager.GetSkeletonMode(itemChecked.ToString()))
                {
                    case (KinectManager.SkeletonMode.Off):
                        kinectSkeletonDefaultButton.Enabled = true;
                        kinectSkeletonSeatedButton.Enabled = true;
                        kinectSkeletonOffButton.Checked = true;
                        break;
                    case (KinectManager.SkeletonMode.Default):
                        kinectSkeletonDefaultButton.Enabled = true;
                        kinectSkeletonSeatedButton.Enabled = true;
                        kinectSkeletonDefaultButton.Checked = true;
                        break;
                    case (KinectManager.SkeletonMode.Seated):
                        kinectSkeletonDefaultButton.Enabled = true;
                        kinectSkeletonSeatedButton.Enabled = true;
                        kinectSkeletonSeatedButton.Checked = true;
                        break;
                    default:
                        kinectSkeletonOffButton.Checked = true;
                        kinectSkeletonDefaultButton.Enabled = false;
                        kinectSkeletonSeatedButton.Enabled = false;
                        break;
                }
            }
        }

        private void kinectNearModeCheckBox_Click(object sender, EventArgs e)
        {
            foreach (object itemChecked in kinectSensorListBox.SelectedItems)
            {
                if (kinectNearModeCheckBox.Checked)
                {

                    kinectManager.SetNearMode(itemChecked.ToString(), true);
                }
                else
                {
                    kinectManager.SetNearMode(itemChecked.ToString(), false);
                }
            }

        }

        private void updateGrammarButton_Click(object sender, EventArgs e)
        {
            if (newSpeechChoiceTextBox.TextLength > 0)
            {
                kinectManager.AddSpeechGrammarChoice(newSpeechChoiceTextBox.Text);
                newSpeechChoiceTextBox.Text = "";
            }

            // Update grammar
            kinectManager.GenerateNewSpeechGrammar();

            // Refresh GUI list
            speechGrammarListBox.Items.Clear();
            foreach( String s in kinectManager.speechChoiceList )
                speechGrammarListBox.Items.Add(s);
        }

        private void removeGrammarChoiceButton_Click(object sender, EventArgs e)
        {
            foreach (String s in speechGrammarListBox.SelectedItems)
            {
                kinectManager.RemoveSpeechGrammarChoice(s);
            }

            // Update grammar
            kinectManager.GenerateNewSpeechGrammar();

            // Refresh GUI list
            speechGrammarListBox.Items.Clear();
            foreach (String s in kinectManager.speechChoiceList)
                speechGrammarListBox.Items.Add(s);
        }
    }
    
}
