﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2016 by Serg V. Zhdanovskih (aka Alchemist, aka Norseman).
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
using System.Collections.Generic;
using GKCommon;
using GKCommon.GEDCOM;
using GKCore.Interfaces;
using GKCore.Types;

namespace GKTests.Mocks
{
    internal class WorkWindowMock : IWorkWindow
    {
        public string GetStatusString() { return ""; }
        public void UpdateView() {}
        public bool NavCanBackward() { return false; }
        public bool NavCanForward() { return false; }
        public void NavNext() {}
        public void NavPrev() {}
        public bool AllowQuickFind() { return false; }
        public IList<ISearchResult> FindAll(string searchPattern) { return new List<ISearchResult>(); }
        public void QuickFind() {}
        public void SelectByRec(GEDCOMIndividualRecord iRec) {}
        public bool AllowFilter() { return false; }
        public void SetFilter() {}
    }

    internal class ProgressMock : IProgressController
    {
        public void ProgressInit(string title, int max) {}
        public void ProgressDone() {}
        public void ProgressStep() {}
        public void ProgressStep(int value) {}
    }

    internal class BaseWindowMock : WorkWindowMock, IBaseWindow
    {
        private GEDCOMTree fTree;

        public BaseWindowMock(GEDCOMTree tree)
        {
            this.fTree = tree;
        }

        public void ProgressInit(string title, int max) { }
        public void ProgressDone() { }
        public void ProgressStep() { }
        public void ProgressStep(int value) { }

        public void SetLang() {}


        public IHost Host { get { return null; } }
        public IBaseContext Context { get { return null; } }

        public bool Modified { get { return false; } set {} }
        public ShieldState ShieldState { get { return ShieldState.None; } set {} }
        public GEDCOMTree Tree { get { return this.fTree; } }
        public ValuesCollection ValuesCollection { get { return null; } }

        public void Activate() { }
        public void ApplyFilter() { }
        public void ApplyFilter(GEDCOMRecordType recType) { }
        public void ChangeRecord(GEDCOMRecord record) { }
        public void Close() { }

        public string DefinePatronymic(string name, GEDCOMSex sex, bool confirm) { return null; }
        public GEDCOMSex DefineSex(string iName, string iPatr) { return GEDCOMSex.svNone; }
        public void CheckPersonSex(GEDCOMIndividualRecord iRec) { }
        public void CollectEventValues(GEDCOMCustomEvent evt) { }

        public bool IsUnknown() { return false; }
        public void FileNew() { }
        public void FileLoad(string fileName) { }
        public void FileSave(string fileName) { }

        public GEDCOMIndividualRecord AddChildForParent(GEDCOMIndividualRecord parent, GEDCOMSex needSex) { return null; }
        public GEDCOMFamilyRecord AddFamilyForSpouse(GEDCOMIndividualRecord spouse) { return null; }
        public GEDCOMFamilyRecord GetChildFamily(GEDCOMIndividualRecord iChild, bool canCreate, GEDCOMIndividualRecord newParent) { return null; }
        public List<GEDCOMRecord> GetContentList(GEDCOMRecordType recType) { return null; }
        public StringList GetRecordContent(GEDCOMRecord record) { return null; }
        public IListManager GetRecordsListManByType(GEDCOMRecordType recType) { return null; }
        public GEDCOMIndividualRecord GetSelectedPerson() { return null; }
        public GEDCOMRecordType GetSelectedRecordType() { return GEDCOMRecordType.rtIndividual; }
        public void RefreshLists(bool titles) { }

        public GEDCOMIndividualRecord CreatePersonDialog(GEDCOMIndividualRecord target, TargetMode targetMode, GEDCOMSex needSex) { return null; }

        public bool ModifyMedia(ref GEDCOMMultimediaRecord mediaRec) { mediaRec = null; return false; }
        public bool ModifyNote(ref GEDCOMNoteRecord noteRec) { noteRec = null; return false; }
        public bool ModifySource(ref GEDCOMSourceRecord sourceRec) { sourceRec = null; return false; }
        public bool ModifyRepository(ref GEDCOMRepositoryRecord repRec) { repRec = null; return false; }
        public bool ModifyGroup(ref GEDCOMGroupRecord groupRec) { groupRec = null; return false; }
        public bool ModifyResearch(ref GEDCOMResearchRecord researchRec) { researchRec = null; return false; }
        public bool ModifyTask(ref GEDCOMTaskRecord taskRec) { taskRec = null; return false; }
        public bool ModifyCommunication(ref GEDCOMCommunicationRecord commRec) { commRec = null; return false; }
        public bool ModifyLocation(ref GEDCOMLocationRecord locRec) { locRec = null; return false; }
        public bool ModifyPerson(ref GEDCOMIndividualRecord indivRec) { indivRec = null; return false; }
        public bool ModifyFamily(ref GEDCOMFamilyRecord familyRec, FamilyTarget target, GEDCOMIndividualRecord person) { familyRec = null; return false; }
        public bool ModifyAddress(GEDCOMAddress address) { return false; }
        public bool ModifySourceCitation(IGEDCOMStructWithLists _struct, ref GEDCOMSourceCitation cit) { cit = null; return false; }

        public void RecordAdd() { }
        public void RecordDelete() { }
        public bool RecordDelete(GEDCOMRecord record, bool confirm) { return false; }
        public void RecordEdit(object sender, EventArgs e) { }
        public bool RecordIsFiltered(GEDCOMRecord record) { return false; }

        public GEDCOMIndividualRecord SelectSpouseFor(GEDCOMIndividualRecord iRec) { return null; }
        public GEDCOMFamilyRecord SelectFamily(GEDCOMIndividualRecord target) { return null; }
        public GEDCOMIndividualRecord SelectPerson(GEDCOMIndividualRecord target, TargetMode targetMode, GEDCOMSex needSex) { return null; }
        public GEDCOMRecord SelectRecord(GEDCOMRecordType mode, params object[] args) { return null; }
        public void SelectRecordByXRef(string xref) { }
        public void Show() { }
        public void ShowMedia(GEDCOMMultimediaRecord mediaRec, bool modal) { }
    }
}