﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2017 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;

using nVLC;
using nVLC.Events;
using nVLC.Media;
using nVLC.Players;

namespace GKMediaPlayer
{
    public partial class MediaPlayer : UserControl
    {
        #if !__MonoCS__
        private const bool FIND_LIBVLC = true;
        #else
        private const bool FIND_LIBVLC = false;
        #endif

        private readonly IMediaPlayerFactory fFactory;
        private readonly IDiskPlayer fPlayer;
        private IMedia fMedia;
        private string fMediaFile;

        public string MediaFile
        {
            get { return fMediaFile; }
            set { fMediaFile = value; }
        }

        public MediaPlayer()
        {
            InitializeComponent();

            fFactory = new MediaPlayerFactory(FIND_LIBVLC);
            fPlayer = fFactory.CreatePlayer<IDiskPlayer>();

            fPlayer.Events.PlayerPositionChanged += Events_PlayerPositionChanged;
            fPlayer.Events.TimeChanged += Events_TimeChanged;
            fPlayer.Events.MediaEnded += Events_MediaEnded;
            fPlayer.Events.PlayerStopped += Events_PlayerStopped;

            fPlayer.WindowHandle = pnlVideo.Handle;

            trkVolume.Value = Math.Max(0, fPlayer.Volume);
            trkVolume_Scroll(null, null);

            fMedia = null;

            UISync.Init(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                fPlayer.Stop();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitControls()
        {
            trkPosition.Value = 0;
            lblTime.Text = @"00:00:00";
            lblDuration.Text = @"00:00:00";
        }

        #region Event handlers

        private void Events_PlayerStopped(object sender, EventArgs e)
        {
            UISync.Execute(InitControls);
        }

        private void Events_MediaEnded(object sender, EventArgs e)
        {
            UISync.Execute(InitControls);
        }

        private void Events_TimeChanged(object sender, MediaPlayerTimeChanged e)
        {
            UISync.Execute(() => lblTime.Text = TimeSpan.FromMilliseconds(e.NewTime).ToString().Substring(0, 8));
        }

        private void Events_PlayerPositionChanged(object sender, MediaPlayerPositionChanged e)
        {
            UISync.Execute(() => {
                               int newPos = (int)(e.NewPosition * 100);
                               if (newPos > trkPosition.Maximum) return;
                               trkPosition.Value = newPos;
                           });
        }

        private void Events_StateChanged(object sender, MediaStateChange e)
        {
            UISync.Execute(() => label1.Text = e.NewState.ToString());
        }

        private void Events_DurationChanged(object sender, MediaDurationChange e)
        {
            UISync.Execute(() => lblDuration.Text = TimeSpan.FromMilliseconds(e.NewDuration).ToString().Substring(0, 8));
        }

        private void Events_ParsedChanged(object sender, MediaParseChange e)
        {
            //Console.WriteLine(e.Parsed);
        }

        #endregion

        #region Controls handlers

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (fMedia == null) {
                if (!string.IsNullOrEmpty(fMediaFile)) {
                    fMedia = fFactory.CreateMedia<IMedia>(fMediaFile);
                    fMedia.Events.DurationChanged += Events_DurationChanged;
                    fMedia.Events.StateChanged += Events_StateChanged;
                    fMedia.Events.ParsedChanged += Events_ParsedChanged;

                    fPlayer.Open(fMedia);
                    fMedia.Parse(true);
                } else {
                    MessageBox.Show("Please select media path first", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            fPlayer.Play();
        }

        private void trkVolume_Scroll(object sender, EventArgs e)
        {
            fPlayer.Mute = false;
            fPlayer.Volume = trkVolume.Value;

            if (100 >= fPlayer.Volume && fPlayer.Volume > 50) {
                btnMute.BackgroundImage = GKMediaPlayerResources.btnVolumeMax;
            }
            if (50 >= fPlayer.Volume && fPlayer.Volume > 5) {
                btnMute.BackgroundImage = GKMediaPlayerResources.btnVolumeMiddle;
            }
            if (5 >= fPlayer.Volume && fPlayer.Volume > 0) {
                btnMute.BackgroundImage = GKMediaPlayerResources.btnVolumeMin;
            }
            if (fPlayer.Volume == 0) {
                btnMute.BackgroundImage = GKMediaPlayerResources.btnVolumeMute;
            }
        }

        private void trkPosition_Scroll(object sender, EventArgs e)
        {
            fPlayer.Position = trkPosition.Value / 100.0f;
        }

        public void btnStop_Click(object sender, EventArgs e)
        {
            fPlayer.Stop();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            fPlayer.Pause();
        }

        private void btnMute_Click(object sender, EventArgs e)
        {
            fPlayer.ToggleMute();

            if (fPlayer.Mute) {
                btnMute.BackgroundImage = GKMediaPlayerResources.btnVolumeMute;
            } else {
                trkVolume_Scroll(sender, e);
            }
        }

        #endregion

        #region UI synchronization

        private static class UISync
        {
            private static ISynchronizeInvoke fSync;

            public static void Init(ISynchronizeInvoke sync)
            {
                fSync = sync;
            }

            public static void Execute(Action action)
            {
                // TODO: to rework this part, because there is a critical error
                // when closing window during playback
                fSync.BeginInvoke(action, null);
            }
        }

        #endregion
    }
}
