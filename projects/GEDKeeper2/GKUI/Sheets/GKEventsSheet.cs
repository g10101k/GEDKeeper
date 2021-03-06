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
using System.Windows.Forms;

using GKCommon;
using GKCommon.Controls;
using GKCommon.GEDCOM;
using GKCore;
using GKCore.Interfaces;
using GKCore.Lists;
using GKCore.Operations;
using GKCore.Types;
using GKUI.Controls;
using GKUI.Dialogs;

namespace GKUI.Sheets
{
    public sealed class GKEventsSheet : GKCustomSheet
    {
        private readonly bool fPersonsMode;

        public GKEventsSheet(IBaseEditor baseEditor, Control owner, bool personsMode, ChangeTracker undoman) : base(baseEditor, owner, undoman)
        {
            fPersonsMode = personsMode;

            Columns_BeginUpdate();
            AddColumn("№", 25, false);
            AddColumn(LangMan.LS(LSID.LSID_Event), 90, false);
            AddColumn(LangMan.LS(LSID.LSID_Date), 80, false);
            if (!fPersonsMode) {
                AddColumn(LangMan.LS(LSID.LSID_Place), 200, false);
            } else {
                AddColumn(LangMan.LS(LSID.LSID_PlaceAndAttribute), 200, false);
            }
            AddColumn(LangMan.LS(LSID.LSID_Cause), 130, false);
            Columns_EndUpdate();

            Buttons = EnumSet<SheetButton>.Create(SheetButton.lbAdd, SheetButton.lbEdit, SheetButton.lbDelete,
                                                  SheetButton.lbMoveUp, SheetButton.lbMoveDown);

            OnModify += ListModify;
        }

        public override void UpdateSheet()
        {
            if (DataList == null) return;

            try
            {
                ClearItems();

                int idx = 0;
                DataList.Reset();
                while (DataList.MoveNext()) {
                    GEDCOMCustomEvent evt = DataList.Current as GEDCOMCustomEvent;
                    if (evt == null) continue;

                    idx += 1;
                    
                    GKListItem item = AddItem(idx, evt);
                    item.AddSubItem(GKUtils.GetEventName(evt));
                    item.AddSubItem(new GEDCOMDateItem(evt.Detail.Date.Value));

                    if (fPersonsMode) {
                        string st = evt.Detail.Place.StringValue;
                        if (evt.StringValue != "") {
                            st = st + " [" + evt.StringValue + "]";
                        }
                        item.AddSubItem(st);
                    } else {
                        item.AddSubItem(evt.Detail.Place.StringValue);
                    }

                    item.AddSubItem(GKUtils.GetEventCause(evt));
                }

                ResizeColumn(1);
                ResizeColumn(2);
                ResizeColumn(3);
            }
            catch (Exception ex)
            {
                Logger.LogWrite("GKEventsSheet.UpdateSheet(): " + ex.Message);
            }
        }

        private void ListModify(object sender, ModifyEventArgs eArgs)
        {
            if (DataList == null) return;

            IBaseWindow baseWin = Editor.Base;
            if (baseWin == null) return;

            GEDCOMCustomEvent evt = eArgs.ItemData as GEDCOMCustomEvent;

            bool result = ModifyRecEvent(baseWin, DataList.Owner as GEDCOMRecordWithEvents, ref evt, eArgs.Action);

            if (result && eArgs.Action == RecordAction.raAdd) eArgs.ItemData = evt;

            if (result) {
                baseWin.Modified = true;
                UpdateSheet();
            }
        }

        private bool ModifyRecEvent(IBaseWindow baseWin, GEDCOMRecordWithEvents record, ref GEDCOMCustomEvent aEvent, RecordAction action)
        {
            bool result = false;

            try
            {
                switch (action)
                {
                    case RecordAction.raAdd:
                    case RecordAction.raEdit:
                        using (EventEditDlg dlgEventEdit = new EventEditDlg(baseWin))
                        {
                            bool exists = (aEvent != null);

                            GEDCOMCustomEvent newEvent;
                            if (aEvent != null) {
                                newEvent = aEvent;
                            } else {
                                if (record is GEDCOMIndividualRecord) {
                                    newEvent = new GEDCOMIndividualEvent(baseWin.Tree, record, "", "");
                                } else {
                                    newEvent = new GEDCOMFamilyEvent(baseWin.Tree, record, "", "");
                                }
                            }

                            dlgEventEdit.Event = newEvent;
                            result = (MainWin.Instance.ShowModalEx(dlgEventEdit, true) == DialogResult.OK);

                            if (!result) {
                                if (!exists) {
                                    newEvent.Dispose();
                                }
                            } else {
                                newEvent = dlgEventEdit.Event;

                                if (!exists) {
                                    //record.AddEvent(newEvent);
                                    result = fUndoman.DoOrdinaryOperation(OperationType.otRecordEventAdd, record, newEvent);
                                } else {
                                    if (record is GEDCOMIndividualRecord && newEvent != aEvent) {
                                        //record.Events.Delete(aEvent);
                                        //record.AddEvent(newEvent);
                                        fUndoman.DoOrdinaryOperation(OperationType.otRecordEventRemove, record, aEvent);
                                        result = fUndoman.DoOrdinaryOperation(OperationType.otRecordEventAdd, record, newEvent);
                                    }
                                }

                                aEvent = newEvent;
                                baseWin.Context.CollectEventValues(aEvent);
                            }
                        }
                        break;

                    case RecordAction.raDelete:
                        if (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_RemoveEventQuery)) != DialogResult.No) {
                            //record.Events.Delete(aEvent);
                            //result = true;
                            result = fUndoman.DoOrdinaryOperation(OperationType.otRecordEventRemove, record, aEvent);
                            aEvent = null;
                        }
                        break;

                    case RecordAction.raMoveUp:
                    case RecordAction.raMoveDown:
                        {
                            int idx = record.Events.IndexOf(aEvent);
                            switch (action)
                            {
                                case RecordAction.raMoveUp:
                                    record.Events.Exchange(idx - 1, idx);
                                    break;

                                case RecordAction.raMoveDown:
                                    record.Events.Exchange(idx, idx + 1);
                                    break;
                            }
                            result = true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("GKEventsSheet.ModifyRecEvent(): " + ex.Message);
                return false;
            }

            return result;
        }
    }
}
