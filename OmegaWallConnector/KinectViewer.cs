//------------------------------------------------------------------------------
// <copyright file="ImageViewer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace OmegaWallConnector
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.Kinect;

    public abstract class KinectViewer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(KinectViewer), new UIPropertyMetadata(Stretch.Uniform));

        public static readonly DependencyProperty KinectProperty =
            DependencyProperty.Register("Kinect", typeof(KinectSensor), typeof(KinectViewer), new UIPropertyMetadata(null, new PropertyChangedCallback(KinectChanged)));

        private bool flipHorizontally;
        private ScaleTransform horizontalScaleTransform;
        private int frameRate = -1;
        private bool collectFrameRate;
        private DateTime lastTime = DateTime.MaxValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public KinectSensor Kinect
        {
            get { return (KinectSensor)GetValue(KinectProperty); }

            set { SetValue(KinectProperty, value); }
        }

        public bool FlipHorizontally
        {
            get
            {
                return this.flipHorizontally;
            }

            set
            {
                if (this.flipHorizontally != value)
                {
                    this.flipHorizontally = value;
                    this.NotifyPropertyChanged("FlipHorizontally");
                    this.horizontalScaleTransform = new ScaleTransform { ScaleX = this.flipHorizontally ? -1 : 1 };
                    this.NotifyPropertyChanged("HorizontalScaleTransform");
                }
            }
        }

        public ScaleTransform HorizontalScaleTransform
        {
            get
            {
                return this.horizontalScaleTransform;
            }
        }

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public bool CollectFrameRate
        {
            get
            {
                return this.collectFrameRate;
            }

            set
            {
                if (value != this.collectFrameRate)
                {
                    this.collectFrameRate = value;
                    this.NotifyPropertyChanged("CollectFrameRate");
                }
            }
        }

        public int FrameRate
        {
            get
            {
                return this.frameRate;
            }

            private set
            {
                if (this.frameRate != value)
                {
                    this.frameRate = value;
                    this.NotifyPropertyChanged("FrameRate");
                }
            }
        }

        protected int TotalFrames { get; set; }

        protected int LastFrames { get; set; }

        protected abstract void OnKinectChanged(KinectSensor oldKinectSensor, KinectSensor newKinectSensor);

        protected void ResetFrameRateCounters()
        {
            if (this.CollectFrameRate)
            {
                this.lastTime = DateTime.MaxValue;
                this.TotalFrames = 0;
                this.LastFrames = 0;
            }
        }

        protected void UpdateFrameRate()
        {
            if (this.CollectFrameRate)
            {
                ++this.TotalFrames;

                DateTime cur = DateTime.Now;
                var span = cur.Subtract(this.lastTime);
                if (this.lastTime == DateTime.MaxValue || span >= TimeSpan.FromSeconds(1))
                {
                    // A straight cast will truncate the value, leading to chronic under-reporting of framerate.
                    // rounding yields a more balanced result
                    this.FrameRate = (int)Math.Round((this.TotalFrames - this.LastFrames) / span.TotalSeconds);
                    this.LastFrames = this.TotalFrames;
                    this.lastTime = cur;
                }
            }
        }

        protected void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private static void KinectChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            KinectViewer imageViewer = (KinectViewer)d;
            imageViewer.OnKinectChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue);
        }
    }
}
