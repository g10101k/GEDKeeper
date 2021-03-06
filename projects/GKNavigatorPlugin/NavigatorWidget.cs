﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2017 by Sergey V. Zhdanovskih.
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
using System.IO;
using System.Windows.Forms;

using GKCommon.GEDCOM;
using GKCore.Interfaces;

namespace GKNavigatorPlugin
{
    public partial class NavigatorWidget : Form, ILocalization
    {
        private readonly Plugin fPlugin;

        private IBaseWindow fBase;
        private string fDatabaseName;

        private TreeNode tnRoot;
        private TreeNode tnRecAct;
        private TreeNode tnJumpHist;
        private TreeNode tnProblems;
        private TreeNode tnFilters;
        private TreeNode tnBookmarks;
        private TreeNode tnRecords;
        private TreeNode tnRecsIndividual;
        private TreeNode tnRecsFamily;
        private TreeNode tnRecsNote;
        private TreeNode tnRecsMultimedia;
        private TreeNode tnRecsSource;
        private TreeNode tnRecsRepository;
        private TreeNode tnRecsGroup;
        private TreeNode tnRecsResearch;
        private TreeNode tnRecsTask;
        private TreeNode tnRecsCommunication;
        private TreeNode tnRecsLocation;

        public NavigatorWidget(Plugin plugin)
        {
            InitializeComponent();

            fPlugin = plugin;

            InitControls();
            SetLang();
        }

        private void InitControls()
        {
            treeView1.Nodes.Clear();

            tnRoot = treeView1.Nodes.Add("root");

            tnRecAct = tnRoot.Nodes.Add("Recent Activity");
            tnJumpHist = tnRecAct.Nodes.Add("Jump history");
            tnProblems = tnRecAct.Nodes.Add("Potencial problems");
            tnFilters = tnRecAct.Nodes.Add("Filters");

            tnBookmarks = tnRoot.Nodes.Add("Bookmarks");

            tnRecords = tnRoot.Nodes.Add("Records");
            tnRecsIndividual = CreateRecordTypeNode("Individuals", GEDCOMRecordType.rtIndividual);
            tnRecsFamily = CreateRecordTypeNode("Families", GEDCOMRecordType.rtFamily);
            tnRecsNote = CreateRecordTypeNode("Notes", GEDCOMRecordType.rtNote);
            tnRecsMultimedia = CreateRecordTypeNode("Multimedia", GEDCOMRecordType.rtMultimedia);
            tnRecsSource = CreateRecordTypeNode("Sources", GEDCOMRecordType.rtSource);
            tnRecsRepository = CreateRecordTypeNode("Repositories", GEDCOMRecordType.rtRepository);
            tnRecsGroup = CreateRecordTypeNode("Groups", GEDCOMRecordType.rtGroup);
            tnRecsResearch = CreateRecordTypeNode("Researches", GEDCOMRecordType.rtResearch);
            tnRecsTask = CreateRecordTypeNode("Tasks", GEDCOMRecordType.rtTask);
            tnRecsCommunication = CreateRecordTypeNode("Communications", GEDCOMRecordType.rtCommunication);
            tnRecsLocation = CreateRecordTypeNode("Locations", GEDCOMRecordType.rtLocation);
        }

        #region ILocalization support

        public void SetLang()
        {
            Text = fPlugin.LangMan.LS(PLS.LSID_Navigator);

            // FIXME: translations
            tnRecAct.Text = "Recent Activity";
            tnJumpHist.Text = "Jump history";
            tnProblems.Text = "Potencial problems";
            tnFilters.Text = "Filters";
            tnBookmarks.Text = "Bookmarks";
        }

        #endregion

        private void NavigatorWidget_Load(object sender, EventArgs e)
        {
            fPlugin.Host.WidgetShow(fPlugin);
            BaseChanged(fPlugin.Host.GetCurrentFile());
        }

        private void NavigatorWidget_Closed(object sender, EventArgs e)
        {
            BaseChanged(null);
            fPlugin.Host.WidgetClose(fPlugin);
        }

        public void BaseChanged(IBaseWindow baseWin)
        {
            if (fBase != baseWin && fBase != null)
            {
            }

            fBase = baseWin;

            if (fBase != null) {
                fDatabaseName = Path.GetFileName(fBase.Tree.FileName);
            } else {
                fDatabaseName = "";
            }

            UpdateControls();
        }

        public void BaseClosed(IBaseWindow baseWin)
        {
            fPlugin.Data.CloseBase(baseWin.Context.Tree.FileName);
            fDatabaseName = "";
            fBase = null;
            UpdateControls(true);
        }

        private static string FmtTitle(string title, int count)
        {
            return string.Format("{0} ({1})", title, count);
        }

        private void UpdateControls(bool afterClose = false)
        {
            try {
                string dbName;
                int[] stats;

                if (fBase == null) {
                    dbName = "";
                    stats = new int[((int)GEDCOMRecordType.rtLast)];
                } else {
                    dbName = fDatabaseName;
                    stats = fBase.Tree.GetRecordStats();
                }

                try {
                    treeView1.BeginUpdate();

                    tnRoot.Text = dbName;
                    tnRecsIndividual.Text = FmtTitle("Individuals", stats[(int)GEDCOMRecordType.rtIndividual]);
                    tnRecsFamily.Text = FmtTitle("Families", stats[(int)GEDCOMRecordType.rtFamily]);
                    tnRecsNote.Text = FmtTitle("Notes", stats[(int)GEDCOMRecordType.rtNote]);
                    tnRecsMultimedia.Text = FmtTitle("Multimedia", stats[(int)GEDCOMRecordType.rtMultimedia]);
                    tnRecsSource.Text = FmtTitle("Sources", stats[(int)GEDCOMRecordType.rtSource]);
                    tnRecsRepository.Text = FmtTitle("Repositories", stats[(int)GEDCOMRecordType.rtRepository]);
                    tnRecsGroup.Text = FmtTitle("Groups", stats[(int)GEDCOMRecordType.rtGroup]);
                    tnRecsResearch.Text = FmtTitle("Researches", stats[(int)GEDCOMRecordType.rtResearch]);
                    tnRecsTask.Text = FmtTitle("Tasks", stats[(int)GEDCOMRecordType.rtTask]);
                    tnRecsCommunication.Text = FmtTitle("Communications", stats[(int)GEDCOMRecordType.rtCommunication]);
                    tnRecsLocation.Text = FmtTitle("Locations", stats[(int)GEDCOMRecordType.rtLocation]);

                    treeView1.ExpandAll();
                } finally {
                    treeView1.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                fPlugin.Host.LogWrite("GKNavigatorPlugin.UpdateControls(): " + ex.Message);
            }
        }

        public enum NodeType
        {
            ntRoot,
            ntRecentActivity, ntJumpHistory, ntPotencialProblems, ntFilters,
            ntBookmarks,
            ntRecords
        }

        private TreeNode CreateTreeNode(string title, NodeType nodeType, GEDCOMRecordType recType)
        {
            return null;
        }

        private TreeNode CreateRecordTypeNode(string title, GEDCOMRecordType recType)
        {
            TreeNode result = tnRecords.Nodes.Add(title);
            result.Tag = recType;
            return result;
        }

        private void CollectData()
        {
        }

        private void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
        {
            object tag = e.Node.Tag;
            if (tag != null && tag.GetType() == typeof(GEDCOMRecordType)) {
                fBase.ShowRecordsTab((GEDCOMRecordType)tag);
            }
        }
    }
}
