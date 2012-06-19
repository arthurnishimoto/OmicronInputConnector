/**
 * ---------------------------------------------
 * GUI.Designer.cs
 * Description: TouchAPI Connector GUI class
 * 
 * Class: 
 * System: Windows 7 Professional x64
 * Copyright 2010, Electronic Visualization Laboratory, University of Illinois at Chicago.
 * Author(s): Arthur Nishimoto
 * Version: 0.1
 * Version Notes:
 * ---------------------------------------------
 */


namespace OmegaWallConnector
{
    partial class GUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.connectServerButton = new System.Windows.Forms.Button();
            this.touchServerList = new System.Windows.Forms.ComboBox();
            this.touchGroupBox = new System.Windows.Forms.GroupBox();
            this.touchDebugTextCheckBox = new System.Windows.Forms.CheckBox();
            this.touchEnableBox = new System.Windows.Forms.CheckBox();
            this.logCheckBox = new System.Windows.Forms.CheckBox();
            this.maxBlobSizeNumLabel = new System.Windows.Forms.Label();
            this.maxBlobSizeBar = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.thresholdNumLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.thresholdBar = new System.Windows.Forms.TrackBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pqGestureBox = new System.Windows.Forms.CheckBox();
            this.touchPointBox = new System.Windows.Forms.CheckBox();
            this.debugTextButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.clientGroupBox = new System.Windows.Forms.GroupBox();
            this.streamTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.oinputserverRadioButton = new System.Windows.Forms.RadioButton();
            this.oinputLegacyRadioButton = new System.Windows.Forms.RadioButton();
            this.tacTileRadioButton = new System.Windows.Forms.RadioButton();
            this.removeClientButton = new System.Windows.Forms.Button();
            this.touchClientList = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.clientListBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.messagePortBox = new System.Windows.Forms.ComboBox();
            this.kinectGroupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.kinectSkeletonOffButton = new System.Windows.Forms.RadioButton();
            this.kinectSkeletonSeatedButton = new System.Windows.Forms.RadioButton();
            this.kinectSkeletonDefaultButton = new System.Windows.Forms.RadioButton();
            this.kinectTiltGroupBox = new System.Windows.Forms.GroupBox();
            this.elevationLabel = new System.Windows.Forms.Label();
            this.kinectElevationBar = new System.Windows.Forms.TrackBar();
            this.kinectEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.depthImage = new System.Windows.Forms.PictureBox();
            this.kinectSkeletonTextCheckBox = new System.Windows.Forms.CheckBox();
            this.kinectGroupBox2 = new System.Windows.Forms.GroupBox();
            this.kinectAudioDebugTextCheckBox = new System.Windows.Forms.CheckBox();
            this.voiceRecogCheckBox = new System.Windows.Forms.CheckBox();
            this.touchGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxBlobSizeBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thresholdBar)).BeginInit();
            this.clientGroupBox.SuspendLayout();
            this.streamTypeGroupBox.SuspendLayout();
            this.kinectGroupBox1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.kinectTiltGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kinectElevationBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.depthImage)).BeginInit();
            this.kinectGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // connectServerButton
            // 
            this.connectServerButton.Location = new System.Drawing.Point(163, 12);
            this.connectServerButton.Name = "connectServerButton";
            this.connectServerButton.Size = new System.Drawing.Size(236, 28);
            this.connectServerButton.TabIndex = 0;
            this.connectServerButton.Text = "Start Connector";
            this.connectServerButton.UseVisualStyleBackColor = true;
            this.connectServerButton.Click += new System.EventHandler(this.connectServerButton_Click);
            // 
            // touchServerList
            // 
            this.touchServerList.FormattingEnabled = true;
            this.touchServerList.Location = new System.Drawing.Point(167, 47);
            this.touchServerList.Name = "touchServerList";
            this.touchServerList.Size = new System.Drawing.Size(121, 21);
            this.touchServerList.TabIndex = 1;
            this.touchServerList.TextChanged += new System.EventHandler(this.touchServerList_TextChanged);
            // 
            // touchGroupBox
            // 
            this.touchGroupBox.Controls.Add(this.touchDebugTextCheckBox);
            this.touchGroupBox.Controls.Add(this.touchEnableBox);
            this.touchGroupBox.Controls.Add(this.logCheckBox);
            this.touchGroupBox.Controls.Add(this.maxBlobSizeNumLabel);
            this.touchGroupBox.Controls.Add(this.maxBlobSizeBar);
            this.touchGroupBox.Controls.Add(this.label4);
            this.touchGroupBox.Controls.Add(this.thresholdNumLabel);
            this.touchGroupBox.Controls.Add(this.label3);
            this.touchGroupBox.Controls.Add(this.thresholdBar);
            this.touchGroupBox.Controls.Add(this.textBox1);
            this.touchGroupBox.Controls.Add(this.pqGestureBox);
            this.touchGroupBox.Controls.Add(this.touchPointBox);
            this.touchGroupBox.Controls.Add(this.debugTextButton);
            this.touchGroupBox.Controls.Add(this.label1);
            this.touchGroupBox.Controls.Add(this.touchServerList);
            this.touchGroupBox.Location = new System.Drawing.Point(12, 224);
            this.touchGroupBox.Name = "touchGroupBox";
            this.touchGroupBox.Size = new System.Drawing.Size(407, 240);
            this.touchGroupBox.TabIndex = 2;
            this.touchGroupBox.TabStop = false;
            this.touchGroupBox.Text = "Touch Server Setup";
            // 
            // touchDebugTextCheckBox
            // 
            this.touchDebugTextCheckBox.AutoSize = true;
            this.touchDebugTextCheckBox.Location = new System.Drawing.Point(281, 19);
            this.touchDebugTextCheckBox.Name = "touchDebugTextCheckBox";
            this.touchDebugTextCheckBox.Size = new System.Drawing.Size(118, 17);
            this.touchDebugTextCheckBox.TabIndex = 2;
            this.touchDebugTextCheckBox.Text = "Show Console Text";
            this.touchDebugTextCheckBox.UseVisualStyleBackColor = true;
            // 
            // touchEnableBox
            // 
            this.touchEnableBox.AutoSize = true;
            this.touchEnableBox.Location = new System.Drawing.Point(11, 19);
            this.touchEnableBox.Name = "touchEnableBox";
            this.touchEnableBox.Size = new System.Drawing.Size(99, 17);
            this.touchEnableBox.TabIndex = 2;
            this.touchEnableBox.Text = "Touch Enabled";
            this.touchEnableBox.UseVisualStyleBackColor = true;
            this.touchEnableBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.touchEnableBox_MouseClick);
            // 
            // logCheckBox
            // 
            this.logCheckBox.AutoSize = true;
            this.logCheckBox.Location = new System.Drawing.Point(6, 177);
            this.logCheckBox.Name = "logCheckBox";
            this.logCheckBox.Size = new System.Drawing.Size(125, 17);
            this.logCheckBox.TabIndex = 14;
            this.logCheckBox.TabStop = false;
            this.logCheckBox.Text = "Generate Touch Log";
            this.logCheckBox.UseVisualStyleBackColor = true;
            this.logCheckBox.CheckedChanged += new System.EventHandler(this.logCheckBox_CheckedChanged);
            // 
            // maxBlobSizeNumLabel
            // 
            this.maxBlobSizeNumLabel.AutoSize = true;
            this.maxBlobSizeNumLabel.Location = new System.Drawing.Point(252, 138);
            this.maxBlobSizeNumLabel.Name = "maxBlobSizeNumLabel";
            this.maxBlobSizeNumLabel.Size = new System.Drawing.Size(13, 13);
            this.maxBlobSizeNumLabel.TabIndex = 12;
            this.maxBlobSizeNumLabel.Text = "0";
            // 
            // maxBlobSizeBar
            // 
            this.maxBlobSizeBar.Location = new System.Drawing.Point(88, 138);
            this.maxBlobSizeBar.Maximum = 4000;
            this.maxBlobSizeBar.Name = "maxBlobSizeBar";
            this.maxBlobSizeBar.Size = new System.Drawing.Size(168, 45);
            this.maxBlobSizeBar.TabIndex = 11;
            this.maxBlobSizeBar.Tag = "";
            this.maxBlobSizeBar.TickFrequency = 0;
            this.maxBlobSizeBar.Scroll += new System.EventHandler(this.maxBlobSizeBar_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 138);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Max Blob Size";
            // 
            // thresholdNumLabel
            // 
            this.thresholdNumLabel.AutoSize = true;
            this.thresholdNumLabel.Enabled = false;
            this.thresholdNumLabel.Location = new System.Drawing.Point(252, 90);
            this.thresholdNumLabel.Name = "thresholdNumLabel";
            this.thresholdNumLabel.Size = new System.Drawing.Size(13, 13);
            this.thresholdNumLabel.TabIndex = 9;
            this.thresholdNumLabel.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Enabled = false;
            this.label3.Location = new System.Drawing.Point(6, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Move Threshold";
            // 
            // thresholdBar
            // 
            this.thresholdBar.Enabled = false;
            this.thresholdBar.Location = new System.Drawing.Point(88, 90);
            this.thresholdBar.Maximum = 100;
            this.thresholdBar.Name = "thresholdBar";
            this.thresholdBar.Size = new System.Drawing.Size(168, 45);
            this.thresholdBar.TabIndex = 7;
            this.thresholdBar.Tag = "";
            this.thresholdBar.TickFrequency = 0;
            this.thresholdBar.Scroll += new System.EventHandler(this.sensitivityBar_Scroll);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 200);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(395, 34);
            this.textBox1.TabIndex = 6;
            // 
            // pqGestureBox
            // 
            this.pqGestureBox.AutoSize = true;
            this.pqGestureBox.Enabled = false;
            this.pqGestureBox.Location = new System.Drawing.Point(271, 109);
            this.pqGestureBox.Name = "pqGestureBox";
            this.pqGestureBox.Size = new System.Drawing.Size(131, 17);
            this.pqGestureBox.TabIndex = 5;
            this.pqGestureBox.Text = "Use PQLabs Gestures";
            this.pqGestureBox.UseVisualStyleBackColor = true;
            this.pqGestureBox.CheckedChanged += new System.EventHandler(this.pqGestureBox_CheckedChanged);
            // 
            // touchPointBox
            // 
            this.touchPointBox.AutoSize = true;
            this.touchPointBox.Location = new System.Drawing.Point(271, 86);
            this.touchPointBox.Name = "touchPointBox";
            this.touchPointBox.Size = new System.Drawing.Size(111, 17);
            this.touchPointBox.TabIndex = 4;
            this.touchPointBox.Text = "Use Touch Points";
            this.touchPointBox.UseVisualStyleBackColor = true;
            this.touchPointBox.CheckedChanged += new System.EventHandler(this.touchPointBox_CheckedChanged);
            // 
            // debugTextButton
            // 
            this.debugTextButton.Location = new System.Drawing.Point(292, 42);
            this.debugTextButton.Name = "debugTextButton";
            this.debugTextButton.Size = new System.Drawing.Size(107, 28);
            this.debugTextButton.TabIndex = 3;
            this.debugTextButton.Text = "Show Touch Text";
            this.debugTextButton.UseVisualStyleBackColor = true;
            this.debugTextButton.Click += new System.EventHandler(this.debugTextButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "PQLabs TouchServer Address";
            // 
            // clientGroupBox
            // 
            this.clientGroupBox.Controls.Add(this.streamTypeGroupBox);
            this.clientGroupBox.Controls.Add(this.removeClientButton);
            this.clientGroupBox.Controls.Add(this.touchClientList);
            this.clientGroupBox.Controls.Add(this.button1);
            this.clientGroupBox.Controls.Add(this.label6);
            this.clientGroupBox.Controls.Add(this.label5);
            this.clientGroupBox.Controls.Add(this.clientListBox);
            this.clientGroupBox.Controls.Add(this.label2);
            this.clientGroupBox.Controls.Add(this.messagePortBox);
            this.clientGroupBox.Controls.Add(this.connectServerButton);
            this.clientGroupBox.Location = new System.Drawing.Point(12, 12);
            this.clientGroupBox.Name = "clientGroupBox";
            this.clientGroupBox.Size = new System.Drawing.Size(405, 206);
            this.clientGroupBox.TabIndex = 3;
            this.clientGroupBox.TabStop = false;
            this.clientGroupBox.Text = "Connector Setup";
            // 
            // streamTypeGroupBox
            // 
            this.streamTypeGroupBox.Controls.Add(this.oinputserverRadioButton);
            this.streamTypeGroupBox.Controls.Add(this.oinputLegacyRadioButton);
            this.streamTypeGroupBox.Controls.Add(this.tacTileRadioButton);
            this.streamTypeGroupBox.Location = new System.Drawing.Point(163, 100);
            this.streamTypeGroupBox.Name = "streamTypeGroupBox";
            this.streamTypeGroupBox.Size = new System.Drawing.Size(236, 100);
            this.streamTypeGroupBox.TabIndex = 13;
            this.streamTypeGroupBox.TabStop = false;
            this.streamTypeGroupBox.Text = "Default Output Stream Format";
            // 
            // oinputserverRadioButton
            // 
            this.oinputserverRadioButton.AutoSize = true;
            this.oinputserverRadioButton.Location = new System.Drawing.Point(6, 65);
            this.oinputserverRadioButton.Name = "oinputserverRadioButton";
            this.oinputserverRadioButton.Size = new System.Drawing.Size(73, 17);
            this.oinputserverRadioButton.TabIndex = 13;
            this.oinputserverRadioButton.TabStop = true;
            this.oinputserverRadioButton.Text = "OmegaLib";
            this.oinputserverRadioButton.UseVisualStyleBackColor = true;
            // 
            // oinputLegacyRadioButton
            // 
            this.oinputLegacyRadioButton.AutoSize = true;
            this.oinputLegacyRadioButton.Location = new System.Drawing.Point(6, 42);
            this.oinputLegacyRadioButton.Name = "oinputLegacyRadioButton";
            this.oinputLegacyRadioButton.Size = new System.Drawing.Size(111, 17);
            this.oinputLegacyRadioButton.TabIndex = 12;
            this.oinputLegacyRadioButton.TabStop = true;
            this.oinputLegacyRadioButton.Text = "Legacy OmegaLib";
            this.oinputLegacyRadioButton.UseVisualStyleBackColor = true;
            // 
            // tacTileRadioButton
            // 
            this.tacTileRadioButton.AutoSize = true;
            this.tacTileRadioButton.Location = new System.Drawing.Point(6, 19);
            this.tacTileRadioButton.Name = "tacTileRadioButton";
            this.tacTileRadioButton.Size = new System.Drawing.Size(118, 17);
            this.tacTileRadioButton.TabIndex = 11;
            this.tacTileRadioButton.TabStop = true;
            this.tacTileRadioButton.Text = "TouchAPI (TacTile)";
            this.tacTileRadioButton.UseVisualStyleBackColor = true;
            // 
            // removeClientButton
            // 
            this.removeClientButton.Location = new System.Drawing.Point(13, 167);
            this.removeClientButton.Name = "removeClientButton";
            this.removeClientButton.Size = new System.Drawing.Size(144, 20);
            this.removeClientButton.TabIndex = 10;
            this.removeClientButton.Text = "Remove Client";
            this.removeClientButton.UseVisualStyleBackColor = true;
            this.removeClientButton.Click += new System.EventHandler(this.removeClientButton_Click);
            // 
            // touchClientList
            // 
            this.touchClientList.FormattingEnabled = true;
            this.touchClientList.Location = new System.Drawing.Point(163, 67);
            this.touchClientList.Name = "touchClientList";
            this.touchClientList.Size = new System.Drawing.Size(131, 21);
            this.touchClientList.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(300, 66);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 20);
            this.button1.TabIndex = 8;
            this.button1.Text = "Add Client";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(160, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(193, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Add Client (Format \'IPaddress:dataPort\')";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Client List";
            // 
            // clientListBox
            // 
            this.clientListBox.FormattingEnabled = true;
            this.clientListBox.Location = new System.Drawing.Point(13, 66);
            this.clientListBox.Name = "clientListBox";
            this.clientListBox.Size = new System.Drawing.Size(144, 82);
            this.clientListBox.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Message Port";
            // 
            // messagePortBox
            // 
            this.messagePortBox.FormattingEnabled = true;
            this.messagePortBox.Items.AddRange(new object[] {
            "7340"});
            this.messagePortBox.Location = new System.Drawing.Point(84, 19);
            this.messagePortBox.Name = "messagePortBox";
            this.messagePortBox.Size = new System.Drawing.Size(73, 21);
            this.messagePortBox.TabIndex = 1;
            this.messagePortBox.TextChanged += new System.EventHandler(this.messagePortBox_TextChanged);
            // 
            // kinectGroupBox1
            // 
            this.kinectGroupBox1.Controls.Add(this.groupBox1);
            this.kinectGroupBox1.Controls.Add(this.kinectTiltGroupBox);
            this.kinectGroupBox1.Controls.Add(this.kinectEnabledCheckBox);
            this.kinectGroupBox1.Controls.Add(this.depthImage);
            this.kinectGroupBox1.Controls.Add(this.kinectSkeletonTextCheckBox);
            this.kinectGroupBox1.Location = new System.Drawing.Point(425, 12);
            this.kinectGroupBox1.Name = "kinectGroupBox1";
            this.kinectGroupBox1.Size = new System.Drawing.Size(429, 384);
            this.kinectGroupBox1.TabIndex = 5;
            this.kinectGroupBox1.TabStop = false;
            this.kinectGroupBox1.Text = "Kinect Skeleton Tracking";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.kinectSkeletonOffButton);
            this.groupBox1.Controls.Add(this.kinectSkeletonSeatedButton);
            this.groupBox1.Controls.Add(this.kinectSkeletonDefaultButton);
            this.groupBox1.Location = new System.Drawing.Point(7, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(140, 95);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Skeleton Tracking";
            // 
            // kinectSkeletonOffButton
            // 
            this.kinectSkeletonOffButton.AutoSize = true;
            this.kinectSkeletonOffButton.Location = new System.Drawing.Point(6, 64);
            this.kinectSkeletonOffButton.Name = "kinectSkeletonOffButton";
            this.kinectSkeletonOffButton.Size = new System.Drawing.Size(66, 17);
            this.kinectSkeletonOffButton.TabIndex = 13;
            this.kinectSkeletonOffButton.TabStop = true;
            this.kinectSkeletonOffButton.Text = "Disabled";
            this.kinectSkeletonOffButton.UseVisualStyleBackColor = true;
            this.kinectSkeletonOffButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.kinectSkeletonOffButton_MouseClick);
            // 
            // kinectSkeletonSeatedButton
            // 
            this.kinectSkeletonSeatedButton.AutoSize = true;
            this.kinectSkeletonSeatedButton.Location = new System.Drawing.Point(6, 41);
            this.kinectSkeletonSeatedButton.Name = "kinectSkeletonSeatedButton";
            this.kinectSkeletonSeatedButton.Size = new System.Drawing.Size(59, 17);
            this.kinectSkeletonSeatedButton.TabIndex = 12;
            this.kinectSkeletonSeatedButton.TabStop = true;
            this.kinectSkeletonSeatedButton.Text = "Seated";
            this.kinectSkeletonSeatedButton.UseVisualStyleBackColor = true;
            this.kinectSkeletonSeatedButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.kinectSkeletonSeatedButton_MouseClick);
            // 
            // kinectSkeletonDefaultButton
            // 
            this.kinectSkeletonDefaultButton.AutoSize = true;
            this.kinectSkeletonDefaultButton.Location = new System.Drawing.Point(6, 18);
            this.kinectSkeletonDefaultButton.Name = "kinectSkeletonDefaultButton";
            this.kinectSkeletonDefaultButton.Size = new System.Drawing.Size(59, 17);
            this.kinectSkeletonDefaultButton.TabIndex = 11;
            this.kinectSkeletonDefaultButton.TabStop = true;
            this.kinectSkeletonDefaultButton.Text = "Default";
            this.kinectSkeletonDefaultButton.UseVisualStyleBackColor = true;
            this.kinectSkeletonDefaultButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.kinectSkeletonDefaultButton_MouseClick);
            // 
            // kinectTiltGroupBox
            // 
            this.kinectTiltGroupBox.Controls.Add(this.elevationLabel);
            this.kinectTiltGroupBox.Controls.Add(this.kinectElevationBar);
            this.kinectTiltGroupBox.Location = new System.Drawing.Point(354, 142);
            this.kinectTiltGroupBox.Name = "kinectTiltGroupBox";
            this.kinectTiltGroupBox.Size = new System.Drawing.Size(69, 233);
            this.kinectTiltGroupBox.TabIndex = 2;
            this.kinectTiltGroupBox.TabStop = false;
            this.kinectTiltGroupBox.Text = "Tilt Angle";
            // 
            // elevationLabel
            // 
            this.elevationLabel.AutoSize = true;
            this.elevationLabel.Location = new System.Drawing.Point(41, 122);
            this.elevationLabel.Name = "elevationLabel";
            this.elevationLabel.Size = new System.Drawing.Size(13, 13);
            this.elevationLabel.TabIndex = 6;
            this.elevationLabel.Text = "0";
            // 
            // kinectElevationBar
            // 
            this.kinectElevationBar.Location = new System.Drawing.Point(6, 19);
            this.kinectElevationBar.Maximum = 27;
            this.kinectElevationBar.Minimum = -27;
            this.kinectElevationBar.Name = "kinectElevationBar";
            this.kinectElevationBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.kinectElevationBar.Size = new System.Drawing.Size(45, 215);
            this.kinectElevationBar.TabIndex = 5;
            this.kinectElevationBar.ValueChanged += new System.EventHandler(this.kinectElevationBar_ValueChanged);
            // 
            // kinectEnabledCheckBox
            // 
            this.kinectEnabledCheckBox.AutoSize = true;
            this.kinectEnabledCheckBox.Location = new System.Drawing.Point(7, 18);
            this.kinectEnabledCheckBox.Name = "kinectEnabledCheckBox";
            this.kinectEnabledCheckBox.Size = new System.Drawing.Size(145, 17);
            this.kinectEnabledCheckBox.TabIndex = 4;
            this.kinectEnabledCheckBox.Text = "Kinect Sensor(s) Enabled";
            this.kinectEnabledCheckBox.UseVisualStyleBackColor = true;
            this.kinectEnabledCheckBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.kinectEnabledCheckBox_MouseClick);
            // 
            // depthImage
            // 
            this.depthImage.Location = new System.Drawing.Point(7, 142);
            this.depthImage.Name = "depthImage";
            this.depthImage.Size = new System.Drawing.Size(341, 233);
            this.depthImage.TabIndex = 3;
            this.depthImage.TabStop = false;
            // 
            // kinectSkeletonTextCheckBox
            // 
            this.kinectSkeletonTextCheckBox.AutoSize = true;
            this.kinectSkeletonTextCheckBox.Enabled = false;
            this.kinectSkeletonTextCheckBox.Location = new System.Drawing.Point(305, 18);
            this.kinectSkeletonTextCheckBox.Name = "kinectSkeletonTextCheckBox";
            this.kinectSkeletonTextCheckBox.Size = new System.Drawing.Size(118, 17);
            this.kinectSkeletonTextCheckBox.TabIndex = 2;
            this.kinectSkeletonTextCheckBox.Text = "Show Console Text";
            this.kinectSkeletonTextCheckBox.UseVisualStyleBackColor = true;
            // 
            // kinectGroupBox2
            // 
            this.kinectGroupBox2.Controls.Add(this.kinectAudioDebugTextCheckBox);
            this.kinectGroupBox2.Controls.Add(this.voiceRecogCheckBox);
            this.kinectGroupBox2.Location = new System.Drawing.Point(425, 402);
            this.kinectGroupBox2.Name = "kinectGroupBox2";
            this.kinectGroupBox2.Size = new System.Drawing.Size(429, 62);
            this.kinectGroupBox2.TabIndex = 6;
            this.kinectGroupBox2.TabStop = false;
            this.kinectGroupBox2.Text = "Kinect Voice Recognition";
            // 
            // kinectAudioDebugTextCheckBox
            // 
            this.kinectAudioDebugTextCheckBox.AutoSize = true;
            this.kinectAudioDebugTextCheckBox.Location = new System.Drawing.Point(305, 17);
            this.kinectAudioDebugTextCheckBox.Name = "kinectAudioDebugTextCheckBox";
            this.kinectAudioDebugTextCheckBox.Size = new System.Drawing.Size(118, 17);
            this.kinectAudioDebugTextCheckBox.TabIndex = 1;
            this.kinectAudioDebugTextCheckBox.Text = "Show Console Text";
            this.kinectAudioDebugTextCheckBox.UseVisualStyleBackColor = true;
            this.kinectAudioDebugTextCheckBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.kinectAudioDebugTextCheckBox_MouseClick);
            // 
            // voiceRecogCheckBox
            // 
            this.voiceRecogCheckBox.AutoSize = true;
            this.voiceRecogCheckBox.Location = new System.Drawing.Point(7, 17);
            this.voiceRecogCheckBox.Name = "voiceRecogCheckBox";
            this.voiceRecogCheckBox.Size = new System.Drawing.Size(140, 17);
            this.voiceRecogCheckBox.TabIndex = 0;
            this.voiceRecogCheckBox.Text = "Voice Interface Enabled";
            this.voiceRecogCheckBox.UseVisualStyleBackColor = true;
            this.voiceRecogCheckBox.Click += new System.EventHandler(this.voiceRecogCheckBox_Click);
            // 
            // GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 476);
            this.Controls.Add(this.kinectGroupBox2);
            this.Controls.Add(this.clientGroupBox);
            this.Controls.Add(this.touchGroupBox);
            this.Controls.Add(this.kinectGroupBox1);
            this.Name = "GUI";
            this.Text = "OmegaWallConnector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GUI_FormClosing);
            this.touchGroupBox.ResumeLayout(false);
            this.touchGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxBlobSizeBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thresholdBar)).EndInit();
            this.clientGroupBox.ResumeLayout(false);
            this.clientGroupBox.PerformLayout();
            this.streamTypeGroupBox.ResumeLayout(false);
            this.streamTypeGroupBox.PerformLayout();
            this.kinectGroupBox1.ResumeLayout(false);
            this.kinectGroupBox1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.kinectTiltGroupBox.ResumeLayout(false);
            this.kinectTiltGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kinectElevationBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.depthImage)).EndInit();
            this.kinectGroupBox2.ResumeLayout(false);
            this.kinectGroupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button connectServerButton;
        private System.Windows.Forms.ComboBox touchServerList;
        private System.Windows.Forms.GroupBox touchGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox clientGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox messagePortBox;
        private System.Windows.Forms.Button debugTextButton;
        private System.Windows.Forms.CheckBox pqGestureBox;
        private System.Windows.Forms.CheckBox touchPointBox;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TrackBar thresholdBar;
        private System.Windows.Forms.Label thresholdNumLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label maxBlobSizeNumLabel;
        private System.Windows.Forms.TrackBar maxBlobSizeBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox clientListBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox touchClientList;
        private System.Windows.Forms.CheckBox logCheckBox;
        private System.Windows.Forms.Button removeClientButton;
        private System.Windows.Forms.GroupBox kinectGroupBox1;
        private System.Windows.Forms.GroupBox streamTypeGroupBox;
        private System.Windows.Forms.RadioButton oinputserverRadioButton;
        private System.Windows.Forms.RadioButton oinputLegacyRadioButton;
        private System.Windows.Forms.RadioButton tacTileRadioButton;
        private System.Windows.Forms.GroupBox kinectGroupBox2;
        private System.Windows.Forms.CheckBox voiceRecogCheckBox;
        private System.Windows.Forms.CheckBox kinectSkeletonTextCheckBox;
        private System.Windows.Forms.CheckBox kinectAudioDebugTextCheckBox;
        public System.Windows.Forms.PictureBox depthImage;
        private System.Windows.Forms.CheckBox touchDebugTextCheckBox;
        private System.Windows.Forms.CheckBox touchEnableBox;
        private System.Windows.Forms.CheckBox kinectEnabledCheckBox;
        private System.Windows.Forms.TrackBar kinectElevationBar;
        private System.Windows.Forms.GroupBox kinectTiltGroupBox;
        private System.Windows.Forms.Label elevationLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton kinectSkeletonOffButton;
        private System.Windows.Forms.RadioButton kinectSkeletonSeatedButton;
        private System.Windows.Forms.RadioButton kinectSkeletonDefaultButton;
    }
}