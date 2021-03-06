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


namespace GKCommon.GEDCOM
{
    public class GEDCOMIndividualAttribute : GEDCOMCustomEvent
    {
        public StringList PhysicalDescription
        {
            get { return GetTagStrings(this); }
            set { SetTagStrings(this, value); }
        }

        public override GEDCOMTag AddTag(string tagName, string tagValue, TagConstructor tagConstructor)
        {
            GEDCOMTag result;

            if (tagName == "CONC" || tagName == "CONT")
            {
                result = base.AddTag(tagName, tagValue, tagConstructor);
            }
            else
            {
                result = Detail.AddTag(tagName, tagValue, tagConstructor);
            }

            return result;
        }

        public GEDCOMIndividualAttribute(GEDCOMTree owner, GEDCOMObject parent, string tagName, string tagValue) : base(owner, parent, tagName, tagValue)
        {
        }

        public new static GEDCOMTag Create(GEDCOMTree owner, GEDCOMObject parent, string tagName, string tagValue)
        {
            return new GEDCOMIndividualAttribute(owner, parent, tagName, tagValue);
        }
    }
}
