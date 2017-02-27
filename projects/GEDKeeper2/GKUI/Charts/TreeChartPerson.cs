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
using System.Drawing;

using GKCommon;
using GKCommon.GEDCOM;
using GKCommon.SmartGraph;
using GKCore;
using GKCore.Types;

namespace GKUI.Charts
{
    public enum PersonFlag
    {
        pfDivorced, pfIsDead, pfSelected, pfIsDup,
        pfDescByFather, pfDescByMother, // descending flags
        pfAncWalk, pfDescWalk, // walk flags
        pfHasInvAnc, pfHasInvDesc // invisible flags
    }

    /// <summary>
    /// 
    /// </summary>
    public class TreeChartPerson : BaseObject
    {
        protected TreeChartBox fChart;

        // strictly private
        private string fBirthDate;
        private string fDeathDate;
        private EnumSet<PersonFlag> fFlags;
        private string fName;
        private string fNick;
        private string fPatronymic;
        private string fSurname;
        private GEDCOMIndividualRecord fRec;
        private EnumSet<SpecialUserRef> fSigns;
        private GEDCOMSex fSex;
        private PersonList fChilds;
        private PersonList fSpouses;
        private Image fPortrait;
        private int fPortraitWidth;

        // for rendering
        private int fHeight;
        private int fPtX;
        private int fPtY;
        private int fWidth;

        // without property controlling
        public GEDCOMFamilyRecord BaseFamily;
        public TreeChartPerson BaseSpouse;
        public float CertaintyAssessment;
        public TreeChartPerson Father;
        public TreeChartPerson Mother;
        public TreeChartPerson Parent;
        public bool CanExpand;
        public int Generation;
        public bool OutsideKin;
        public string Kinship;
        public string[] Lines;
        public IVertex Node;
        public string PathDebug;
        public ExtRect PortraitArea;


        public Image Portrait
        {
            get { return fPortrait; }
        }

        public int PortraitWidth
        {
            get { return fPortraitWidth; }
        }

        public bool Divorced
        {
            get {
                return fFlags.Contains(PersonFlag.pfDivorced);
            }
            set {
                if (value) {
                    fFlags.Include(PersonFlag.pfDivorced);
                } else {
                    fFlags.Exclude(PersonFlag.pfDivorced);
                }
            }
        }

        public bool IsDup
        {
            get {
                return fFlags.Contains(PersonFlag.pfIsDup);
            }
            set {
                if (value) {
                    fFlags.Include(PersonFlag.pfIsDup);
                } else {
                    fFlags.Exclude(PersonFlag.pfIsDup);
                }
            }
        }

        public int Height
        {
            get { return fHeight; }
        }

        public bool IsDead
        {
            get {
                return fFlags.Contains(PersonFlag.pfIsDead);
            }
            set {
                if (value) {
                    fFlags.Include(PersonFlag.pfIsDead);
                } else {
                    fFlags.Exclude(PersonFlag.pfIsDead);
                }
            }
        }

        public int PtX
        {
            get { return fPtX; }
            set { fPtX = value; }
        }

        public int PtY
        {
            get { return fPtY; }
            set { fPtY = value; }
        }

        public GEDCOMIndividualRecord Rec
        {
            get { return fRec; }
        }

        public ExtRect Rect
        {
            get {
                ExtRect result;
                result.Left = fPtX - fWidth / 2;
                result.Right = result.Left + fWidth - 1;
                result.Top = fPtY;
                result.Bottom = result.Top + fHeight - 1;
                return result;
            }
        }

        public bool Selected
        {
            get {
                return fFlags.Contains(PersonFlag.pfSelected);
            }
            set {
                if (value) {
                    fFlags.Include(PersonFlag.pfSelected);
                } else {
                    fFlags.Exclude(PersonFlag.pfSelected);
                }
            }
        }

        public GEDCOMSex Sex
        {
            get { return fSex; }
            set { fSex = value; }
        }

        public EnumSet<SpecialUserRef> Signs
        {
            get { return fSigns; }
        }

        public int Width
        {
            get { return fWidth; }
        }

        public TreeChartPerson(TreeChartBox chart)
        {
            fChart = chart;

            fFlags = EnumSet<PersonFlag>.Create();
            fPortrait = null;
            fSpouses = null;
            fChilds = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                // don't dispose portrait - he's from cache!
                //if (fPortrait != null) fPortrait.Dispose();

                if (fChilds != null) fChilds.Dispose();
                if (fSpouses != null) fSpouses.Dispose();
            }
            base.Dispose(disposing);
        }

        public bool HasFlag(PersonFlag flag)
        {
            return fFlags.Contains(flag);
        }

        public void SetFlag(PersonFlag flag)
        {
            fFlags.Include(flag);
        }

        public TreeChartPerson GetChild(int index)
        {
            TreeChartPerson result = ((fChilds == null) ? null : fChilds[index]);
            return result;
        }

        public int GetChildsCount()
        {
            int result = ((fChilds == null) ? 0 : fChilds.Count);
            return result;
        }

        public TreeChartPerson GetSpouse(int index)
        {
            TreeChartPerson result = ((fSpouses == null) ? null : fSpouses[index]);
            return result;
        }

        public int GetSpousesCount()
        {
            int result = ((fSpouses == null) ? 0 : fSpouses.Count);
            return result;
        }

        public void AddChild(TreeChartPerson child)
        {
            if (child == null) return;

            if (fChilds == null) fChilds = new PersonList(false);

            fChilds.Add(child);
        }

        public void AddSpouse(TreeChartPerson spouse)
        {
            if (spouse == null) return;

            if (fSpouses == null) fSpouses = new PersonList(false);

            fSpouses.Add(spouse);
        }

        public void BuildBy(GEDCOMIndividualRecord iRec, ref bool hasMediaFail)
        {
            try
            {
                fRec = iRec;

                if (iRec != null) {
                    if (fChart.fPreparedIndividuals.IndexOf(iRec.XRef) < 0) {
                        fChart.fPreparedIndividuals.Add(iRec.XRef);
                    }

                    string fam, nam, pat;
                    GKUtils.GetNameParts(iRec, out fam, out nam, out pat);
                    fSurname = fam;
                    fName = nam;
                    fPatronymic = pat;
                    fNick = GKUtils.GetNickString(iRec);
                    fSex = iRec.Sex;

                    GEDCOMCustomEvent birthEvent, deathEvent;
                    iRec.GetLifeDates(out birthEvent, out deathEvent);
                    DateFormat dateFormat = (fChart.Options.OnlyYears) ? DateFormat.dfYYYY : DateFormat.dfDD_MM_YYYY;

                    IsDead = (deathEvent != null);
                    fBirthDate = GKUtils.GEDCOMEventToDateStr(birthEvent, dateFormat, false);
                    fDeathDate = GKUtils.GEDCOMEventToDateStr(deathEvent, dateFormat, false);

                    if (fChart.Options.SignsVisible) {
                        EnumSet<SpecialUserRef> signs = EnumSet<SpecialUserRef>.Create();

                        int num = fRec.UserReferences.Count;
                        for (int i = 0; i < num; i++)
                        {
                            string rs = fRec.UserReferences[i].StringValue;
                            for (var cps = SpecialUserRef.urRI_StGeorgeCross; cps <= SpecialUserRef.urLast; cps++)
                            {
                                if (rs == GKData.SpecialUserRefs[(int)cps]) signs.Include(cps);
                            }
                        }

                        fSigns = signs;
                    } else {
                        fSigns = EnumSet<SpecialUserRef>.Create();
                    }

                    if (fChart.Options.PortraitsVisible) {
                        try
                        {
                            //fPortrait = fChart.Base.Context.GetPrimaryBitmap(iRec, -1, -1, true);
                            fPortrait = PortraitsCache.Instance.GetImage(fChart.Base.Context, iRec);

                            if (fPortrait == null && fChart.Options.DefaultPortraits) {
                                fPortrait = (fSex == GEDCOMSex.svFemale) ? GKResources.piFemale140 : GKResources.piMale140;
                            }
                        }
                        catch (MediaFileNotFoundException)
                        {
                            if (!hasMediaFail) {
                                GKUtils.ShowError(LangMan.LS(LSID.LSID_ArcNotFound));
                                hasMediaFail = true;
                            }
                        }
                    }

                    CertaintyAssessment = iRec.GetCertaintyAssessment();
                } else {
                    fSurname = "";
                    fName = "< ? >";
                    fPatronymic = "";
                    fNick = "";
                    fBirthDate = "";
                    fDeathDate = "";
                    IsDead = false;
                    fSex = GEDCOMSex.svNone;
                    fSigns = EnumSet<SpecialUserRef>.Create();

                    CertaintyAssessment = 0.0f;
                }
            }
            catch (Exception ex)
            {
                fChart.Base.Host.LogWrite("TreeChartPerson.BuildBy(): " + ex.Message);
                throw;
            }
        }

        private void InitInfo(int lines)
        {
            Lines = new string[lines];

            try
            {
                // prepare
                string nameLine = fName;
                if (fChart.Options.NickVisible && !string.IsNullOrEmpty(fNick)) {
                    nameLine += " \"" + fNick + "\"";
                }

                // create lines
                int idx = 0;

                if (fChart.Options.FamilyVisible) {
                    Lines[idx] = fSurname;
                    idx++;
                }

                if (!fChart.Options.DiffLines) {
                    Lines[idx] = nameLine + " " + fPatronymic; // attention: "Name" is combined property
                    idx++;
                } else {
                    Lines[idx] = nameLine;
                    idx++;

                    Lines[idx] = fPatronymic;
                    idx++;
                }

                if (!fChart.Options.OnlyYears) {
                    if (fChart.Options.BirthDateVisible) {
                        Lines[idx] = fBirthDate;
                        idx++;
                    }

                    if (fChart.Options.DeathDateVisible) {
                        Lines[idx] = fDeathDate;
                        idx++;
                    }
                } else {
                    string lifeYears = "[ ";
                    lifeYears += (fBirthDate == "") ? "?" : fBirthDate;
                    if (IsDead) {
                        lifeYears += (fDeathDate == "") ? " - ?" : " - " + fDeathDate;
                    }
                    lifeYears += " ]";

                    Lines[idx] = lifeYears;
                    idx++;
                }

                if (fChart.Options.Kinship) {
                    Lines[idx] = Kinship;
                    idx++;
                }

                if (fChart.PathDebug) {
                    Lines[idx] = PathDebug;
                    //idx++;
                }
            }
            catch (Exception ex)
            {
                fChart.Base.Host.LogWrite("TreeChartPerson.InitInfo(): " + ex.Message);
            }
        }

        private void DefineExpands()
        {
            if (fFlags.Contains(PersonFlag.pfAncWalk) && fFlags.Contains(PersonFlag.pfDescWalk)
                && fFlags.Contains(PersonFlag.pfHasInvDesc))
            {
                // it's hack
                fFlags.Exclude(PersonFlag.pfHasInvDesc);
            }

            if (fFlags.Contains(PersonFlag.pfHasInvAnc)) {
                CanExpand = true;
            }

            if (fFlags.Contains(PersonFlag.pfHasInvDesc)) {
                CanExpand = true;
            }
        }

        public void CalcBounds(int lines, TreeChartRenderer renderer)
        {
            try
            {
                InitInfo(lines);
                DefineExpands();

                int maxwid = 0;
                for (int k = 0; k < lines; k++) {
                    int wt = renderer.GetTextWidth(Lines[k], fChart.DrawFont);
                    if (maxwid < wt) maxwid = wt;
                }

                int pad2side = (fChart.NodePadding * 2);

                fWidth = pad2side + maxwid;
                fHeight = pad2side + renderer.GetTextHeight(fChart.DrawFont) * lines;

                if (fPortrait != null) {
                    ExtRect portRt = ExtRect.Create(0, 0, fHeight - 1, fHeight - 1);
                    portRt.Inflate(3, 3);

                    int rtW = portRt.GetWidth();
                    int rtH = portRt.GetHeight();
                    int imgW = fPortrait.Width;
                    int imgH = fPortrait.Height;
                    float ratio = SysUtils.ZoomToFit(imgW, imgH, rtW, rtH);
                    imgW = (int)Math.Round(imgW * ratio);
                    imgH = (int)Math.Round(imgH * ratio);

                    PortraitArea = ExtRect.CreateBounds(portRt.Left, portRt.Top, imgW, imgH);
                    fPortraitWidth = imgW;

                    fWidth += imgW;
                }
            }
            catch (Exception ex)
            {
                fChart.Base.Host.LogWrite("TreeChartPerson.CalcBounds(): " + ex.Message);
            }
        }

        public Color GetSelectedColor()
        {
            Color result;

            switch (fSex) {
                case GEDCOMSex.svMale:
                    result = Color.Blue;
                    break;

                case GEDCOMSex.svFemale:
                    result = Color.Red;
                    break;

                default:
                    result = Color.Black;
                    break;
            }

            return result;
        }

        public Color GetFillColor(bool dead)
        {
            Color result;

            if (dead) {
                result = Color.Black;
            } else {
                if (IsDup) {
                    result = Color.FromArgb(192, 192, 192);
                } else {
                    switch (fSex) {
                        case GEDCOMSex.svMale:
                            result = Divorced ? fChart.Options.UnHusbandColor : fChart.Options.MaleColor;
                            break;

                        case GEDCOMSex.svFemale:
                            result = Divorced ? fChart.Options.UnWifeColor : fChart.Options.FemaleColor;
                            break;

                        default:
                            result = fChart.Options.UnkSexColor;
                            break;
                    }
                }
            }

            return result;
        }
    }
}
