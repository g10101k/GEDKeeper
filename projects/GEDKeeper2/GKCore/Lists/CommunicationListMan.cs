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

using GKCommon.GEDCOM;
using GKCore.Interfaces;
using GKCore.Types;

namespace GKCore.Lists
{
    /// <summary>
    /// 
    /// </summary>
    public enum CommunicationColumnType
    {
        cctCommName,
        cctCorresponder,
        cctCommType,
        cctDate,
        cctChangeDate
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class CommunicationListColumns : ListColumns
    {
        protected override void InitColumnStatics()
        {
            AddStatic(LSID.LSID_Theme, DataType.dtString, 300, true);
            AddStatic(LSID.LSID_Corresponder, DataType.dtString, 200, true);
            AddStatic(LSID.LSID_Type, DataType.dtString, 90, true);
            AddStatic(LSID.LSID_Date, DataType.dtString, 90, true);
            AddStatic(LSID.LSID_Changed, DataType.dtDateTime, 150, true);
        }

        public CommunicationListColumns()
        {
            InitData(typeof(CommunicationColumnType));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class CommunicationListMan : ListManager
    {
        private GEDCOMCommunicationRecord fRec;

        public override bool CheckFilter(ShieldState shieldState)
        {
            bool res = (QuickFilter == "*" || IsMatchesMask(fRec.CommName, QuickFilter));

            res = res && CheckCommonFilter();

            return res;
        }

        public override void Fetch(GEDCOMRecord aRec)
        {
            fRec = (aRec as GEDCOMCommunicationRecord);
        }

        protected override object GetColumnValueEx(int colType, int colSubtype, bool isVisible)
        {
            object result = null;
            switch (colType) {
                case 0:
                    result = fRec.CommName;
                    break;
                case 1:
                    result = GKUtils.GetCorresponderStr(fTree, fRec, false);
                    break;
                case 2:
                    result = LangMan.LS(GKData.CommunicationNames[(int)fRec.CommunicationType]);
                    break;
                case 3:
                    result = GetDateValue(fRec.Date, isVisible);
                    break;
                case 4:
                    result = fRec.ChangeDate.ChangeDateTime;
                    break;
            }
            return result;
        }

        public CommunicationListMan(GEDCOMTree tree) : base(tree, new CommunicationListColumns())
        {
        }
    }
}
