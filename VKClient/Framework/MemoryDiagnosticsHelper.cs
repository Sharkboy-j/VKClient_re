using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.System;

namespace VKClient.Framework
{
    public static class MemoryDiagnosticsHelper
    {
        private static int lastSafetyBand = -1;
        private static bool alreadyFailedPeak = false;
        private static Popup popup;
        private static TextBlock currentMemoryBlock;
        private static TextBlock peakMemoryBlock;
        private static DispatcherTimer timer;
        private static bool forceGc;
        private const long MAX_MEMORY = 209715200;
        private const long MAX_CHECKPOINTS = 10;
        private static Queue<MemoryCheckpoint> recentCheckpoints;

        public static IEnumerable<MemoryCheckpoint> RecentCheckpoints
        {
            get
            {
                if (MemoryDiagnosticsHelper.recentCheckpoints != null)
                {
                    foreach (MemoryCheckpoint recentCheckpoint in MemoryDiagnosticsHelper.recentCheckpoints)
                        yield return recentCheckpoint;
                    //Queue<MemoryCheckpoint>.Enumerator enumerator = new Queue<MemoryCheckpoint>.Enumerator();
                }
            }
        }

        public static void Start(TimeSpan timespan, bool forceGc)
        {
            if (MemoryDiagnosticsHelper.timer != null)
                throw new InvalidOperationException("Diagnostics already running");
            MemoryDiagnosticsHelper.forceGc = forceGc;
            MemoryDiagnosticsHelper.recentCheckpoints = new Queue<MemoryCheckpoint>();
            MemoryDiagnosticsHelper.StartTimer(timespan);
            MemoryDiagnosticsHelper.ShowPopup();
        }

        public static void Stop()
        {
            MemoryDiagnosticsHelper.HidePopup();
            MemoryDiagnosticsHelper.StopTimer();
            MemoryDiagnosticsHelper.recentCheckpoints = (Queue<MemoryCheckpoint>)null;
        }

        public static void Checkpoint(string text)
        {
            if (MemoryDiagnosticsHelper.recentCheckpoints == null)
                return;
            if ((long)MemoryDiagnosticsHelper.recentCheckpoints.Count >= 9L)
                MemoryDiagnosticsHelper.recentCheckpoints.Dequeue();
            MemoryDiagnosticsHelper.recentCheckpoints.Enqueue(new MemoryCheckpoint(text, MemoryDiagnosticsHelper.GetCurrentMemoryUsage()));
        }

        public static long GetCurrentMemoryUsage()
        {
            return (long)MemoryManager.AppMemoryUsage;
        }

        public static long GetPeakMemoryUsage()
        {
            return 0;
        }

        private static void ShowPopup()
        {
            MemoryDiagnosticsHelper.popup = new Popup();
            double num = (double)Application.Current.Resources["PhoneFontSizeSmall"] - 2.0;
            Brush brush1 = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
            StackPanel stackPanel1 = new StackPanel();
            stackPanel1.Orientation = Orientation.Horizontal;
            Brush brush2 = (Brush)Application.Current.Resources["PhoneSemitransparentBrush"];
            stackPanel1.Background = brush2;
            StackPanel stackPanel2 = stackPanel1;
            MemoryDiagnosticsHelper.currentMemoryBlock = new TextBlock()
            {
                Text = "---",
                FontSize = num,
                Foreground = brush1
            };
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "";
            textBlock.FontSize = num;
            textBlock.Foreground = brush1;
            Thickness thickness = new Thickness(5.0, 0.0, 0.0, 0.0);
            textBlock.Margin = thickness;
            MemoryDiagnosticsHelper.peakMemoryBlock = textBlock;
            stackPanel2.Children.Add((UIElement)MemoryDiagnosticsHelper.currentMemoryBlock);
            stackPanel2.Children.Add((UIElement)new TextBlock()
            {
                Text = " kb",
                FontSize = num,
                Foreground = brush1
            });
            stackPanel2.Children.Add((UIElement)MemoryDiagnosticsHelper.peakMemoryBlock);
            stackPanel2.RenderTransform = (Transform)new CompositeTransform()
            {
                Rotation = 90.0,
                TranslateX = 480.0,
                TranslateY = 425.0,
                CenterX = 0.0,
                CenterY = 0.0
            };
            MemoryDiagnosticsHelper.popup.Child = (UIElement)stackPanel2;
            MemoryDiagnosticsHelper.popup.IsOpen = true;
        }

        private static void StartTimer(TimeSpan timespan)
        {
            MemoryDiagnosticsHelper.timer = new DispatcherTimer();
            MemoryDiagnosticsHelper.timer.Interval = timespan;
            MemoryDiagnosticsHelper.timer.Tick += new EventHandler(MemoryDiagnosticsHelper.timer_Tick);
            MemoryDiagnosticsHelper.timer.Start();
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            if (MemoryDiagnosticsHelper.forceGc)
                GC.Collect();
            MemoryDiagnosticsHelper.UpdateCurrentMemoryUsage();
            MemoryDiagnosticsHelper.UpdatePeakMemoryUsage();
        }

        private static void UpdatePeakMemoryUsage()
        {
            if (MemoryDiagnosticsHelper.alreadyFailedPeak || MemoryDiagnosticsHelper.GetPeakMemoryUsage() < 209715200L)
                return;
            MemoryDiagnosticsHelper.alreadyFailedPeak = true;
            MemoryDiagnosticsHelper.Checkpoint("*MEMORY USAGE FAIL*");
            MemoryDiagnosticsHelper.peakMemoryBlock.Text = "FAIL!";
            MemoryDiagnosticsHelper.peakMemoryBlock.Foreground = (Brush)new SolidColorBrush(Colors.Red);
            int num = Debugger.IsAttached ? 1 : 0;
        }

        private static void UpdateCurrentMemoryUsage()
        {
            long currentMemoryUsage = MemoryDiagnosticsHelper.GetCurrentMemoryUsage();
            MemoryDiagnosticsHelper.currentMemoryBlock.Text = string.Format("{0:N}", (object)(currentMemoryUsage / 1024L));
            int safetyBand = MemoryDiagnosticsHelper.GetSafetyBand(currentMemoryUsage);
            if (safetyBand == MemoryDiagnosticsHelper.lastSafetyBand)
                return;
            MemoryDiagnosticsHelper.currentMemoryBlock.Foreground = MemoryDiagnosticsHelper.GetBrushForSafetyBand(safetyBand);
            MemoryDiagnosticsHelper.lastSafetyBand = safetyBand;
        }

        private static Brush GetBrushForSafetyBand(int safetyBand)
        {
            if (safetyBand == 0)
                return (Brush)new SolidColorBrush(Colors.Green);
            if (safetyBand == 1)
                return (Brush)new SolidColorBrush(Colors.Orange);
            return (Brush)new SolidColorBrush(Colors.Red);
        }

        private static int GetSafetyBand(long mem)
        {
            double num = (double)mem / 209715200.0;
            if (num <= 0.75)
                return 0;
            return num <= 0.9 ? 1 : 2;
        }

        private static void StopTimer()
        {
            MemoryDiagnosticsHelper.timer.Stop();
            MemoryDiagnosticsHelper.timer = null;
        }

        private static void HidePopup()
        {
            MemoryDiagnosticsHelper.popup.IsOpen = false;
            MemoryDiagnosticsHelper.popup = null;
        }
    }
}
