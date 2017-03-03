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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GKCommon.GEDCOM
{
    public sealed class GEDCOMEnumHelper<T> where T : struct, IComparable, IFormattable, IConvertible
    {
        private readonly string[] fStrValues;
        private readonly Dictionary<string, int> fValues;
        private readonly T fDefaultValue;
        private readonly bool fCaseSensitive;

        public GEDCOMEnumHelper(string[] strValues, T defaultValue, bool caseSensitive = false)
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException(string.Format("{0} is not of type Enum", enumType.Name));

            T[] enums = (T[]) Enum.GetValues(enumType);

            if (enums.Length != strValues.Length) {
                throw new ArgumentException("Arguments are not compatible");
            }

            fStrValues = strValues;
            fDefaultValue = defaultValue;
            fCaseSensitive = caseSensitive;

            fValues = new Dictionary<string, int>(enums.Length);
            for (int i = 0; i < enums.Length; i++)
            {
                fValues.Add(strValues[i], i);
            }
        }

        public string GetStrValue(T enumVal)
        {
            int idx = (int)((IConvertible)enumVal);

            if (idx < 0 || idx >= fStrValues.Length) {
                return string.Empty;
            } else {
                return fStrValues[idx];
            }
        }

        public T GetEnumValue(string key)
        {
            if (!fCaseSensitive) {
                key = key.Trim().ToLowerInvariant();
            }

            int result;
            if (fValues.TryGetValue(key, out result)) {
                return (T)((IConvertible)result);
            } else {
                return fDefaultValue;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class GEDCOMUtils
    {
        #region Tag properties
        
        public sealed class TagProperties
        {
            public readonly string Name;
            public readonly bool EmptySkip;
            public readonly bool GKExtend;

            public TagProperties(string name, bool emptySkip, bool extend)
            {
                Name = name;
                EmptySkip = emptySkip;
                GKExtend = extend;
            }
        }

        private static readonly Dictionary<string, TagProperties> TAGS_BASE;


        static GEDCOMUtils()
        {
            TAGS_BASE = new Dictionary<string, TagProperties>();
            TAGS_BASE.Add("ADDR", new TagProperties("ADDR", true, false));
            TAGS_BASE.Add("AGNC", new TagProperties("AGNC", true, false));
            TAGS_BASE.Add("AUTH", new TagProperties("AUTH", true, false));
            TAGS_BASE.Add("CAUS", new TagProperties("CAUS", true, false));
            TAGS_BASE.Add("CHAN", new TagProperties("CHAN", true, false));
            TAGS_BASE.Add("CITY", new TagProperties("CITY", true, false));
            TAGS_BASE.Add("CTRY", new TagProperties("CTRY", true, false));
            TAGS_BASE.Add("DATE", new TagProperties("DATE", true, false));
            TAGS_BASE.Add("PAGE", new TagProperties("PAGE", true, false));
            TAGS_BASE.Add("PLAC", new TagProperties("PLAC", true, false));
            TAGS_BASE.Add("POST", new TagProperties("POST", true, false));
            TAGS_BASE.Add("PUBL", new TagProperties("PUBL", true, false));
            TAGS_BASE.Add("RESN", new TagProperties("RESN", true, false));
            TAGS_BASE.Add("STAE", new TagProperties("STAE", true, false));
            TAGS_BASE.Add("TEXT", new TagProperties("TEXT", true, false));
            TAGS_BASE.Add("TIME", new TagProperties("TIME", true, false));
            TAGS_BASE.Add("TYPE", new TagProperties("TYPE", true, false));
            TAGS_BASE.Add("SUBM", new TagProperties("SUBM", true, false));
            TAGS_BASE.Add("VERS", new TagProperties("VERS", true, false));
            TAGS_BASE.Add("LANG", new TagProperties("LANG", true, false));

            TAGS_BASE.Add("NPFX", new TagProperties("NPFX", true, false));
            TAGS_BASE.Add("GIVN", new TagProperties("GIVN", true, false));
            TAGS_BASE.Add("NICK", new TagProperties("NICK", true, false));
            TAGS_BASE.Add("SPFX", new TagProperties("SPFX", true, false));
            TAGS_BASE.Add("SURN", new TagProperties("SURN", true, false));
            TAGS_BASE.Add("NSFX", new TagProperties("NSFX", true, false));

            TAGS_BASE.Add("_PATN", new TagProperties("_PATN", true, true));
            TAGS_BASE.Add("_MARN", new TagProperties("_MARN", true, true));
            TAGS_BASE.Add("_RELN", new TagProperties("_RELN", true, true));
            TAGS_BASE.Add("_CENN", new TagProperties("_CENN", true, true));

            TAGS_BASE.Add("_LOC", new TagProperties("_LOC", true,  true));
            TAGS_BASE.Add("_POSITION", new TagProperties("_POSITION", true,  true));
            TAGS_BASE.Add("ALIA", new TagProperties("ALIA", true, false));

            //new TagProperties("HUSB", true, false));
            //new TagProperties("WIFE", true, false));

            TAGS_BASE.Add("_BGRO", new TagProperties("_BGRO", true,  true));
            TAGS_BASE.Add("_HAIR", new TagProperties("_HAIR", true,  true));
            TAGS_BASE.Add("_EYES", new TagProperties("_EYES", true,  true));
            TAGS_BASE.Add("_MDNA", new TagProperties("_MDNA", true,  true));
            TAGS_BASE.Add("_YDNA", new TagProperties("_YDNA", true,  true));
        }

        public static TagProperties GetTagProps(string tagName)
        {
            TagProperties result;
            TAGS_BASE.TryGetValue(tagName, out result);
            return result;
        }

        #endregion

        #region Parse functions

        public static string ExtractDelimiter(string str, int max)
        {
            string result = str;

            if (result != null)
            {
                while (result.Length > 0 && result[0] == ' ')
                {
                    result = result.Remove(0, 1);
                    if (max > 0)
                    {
                        max--;
                        if (max == 0) break;
                    }
                }
            }

            return result;
        }

        public static string ExtractDotDelimiter(string str, int max)
        {
            string result = str;

            if (result != null)
            {
                while (result.Length > 0 && result[0] == '.')
                {
                    result = result.Remove(0, 1);
                    if (max > 0)
                    {
                        max--;
                        if (max == 0) break;
                    }
                }
            }

            return result;
        }

        public static string ExtractString(string str, out string value, string defValue)
        {
            string result = str;

            if (!string.IsNullOrEmpty(result)) {
                int I = 0;
                while (I < result.Length && result[I] != ' ') {
                    I++;
                }

                if (I > 0) {
                    value = result.Substring(0, I);
                    result = result.Remove(0, I);
                } else {
                    value = defValue;
                }
            } else {
                value = defValue;
            }

            return result;
        }

        public static string ExtractXRef(string str, out string aXRef, bool noException, string defValue)
        {
            string result = str;

            if (!string.IsNullOrEmpty(result) && result[0] == '@') {
                int p = result.IndexOf('@', 1);
                if (p > 0) {
                    aXRef = result.Substring(1, p - 1);
                    result = result.Remove(0, p + 1);
                } else {
                    if (!noException) {
                        throw new EGEDCOMException(string.Format("The string {0} contains an unterminated XRef pointer", str));
                    }
                    aXRef = defValue;
                }
            } else {
                if (!noException) {
                    throw new EGEDCOMException(string.Format("The string {0} is expected to start with an XRef pointer", str));
                }
                aXRef = defValue;
            }

            return result;
        }

        public static string CleanXRef(string xref)
        {
            string result = xref;

            if (!string.IsNullOrEmpty(result)) {
                if (result[0] == '@') {
                    result = result.Remove(0, 1);
                }

                if (result.Length > 0 && result[result.Length - 1] == '@') {
                    result = result.Remove(result.Length - 1, 1);
                }
            }

            return result;
        }

        public static string EncloseXRef(string xref)
        {
            if (!string.IsNullOrEmpty(xref)) {
                if (xref[0] != '@') {
                    xref = "@" + xref;
                }

                if (xref[xref.Length - 1] != '@') {
                    xref += "@";
                }
            }
            return xref;
        }

        public static string ExtractNumber(string str, out int value, bool noException, int defValue)
        {
            string result = str;

            if (!string.IsNullOrEmpty(result)) {
                int I = 0;
                while (I < result.Length && IsDigit(result[I])) {
                    I++;
                }

                if (I > 0) {
                    value = int.Parse(result.Substring(0, I));
                    result = result.Remove(0, I);
                } else {
                    if (!noException) {
                        throw new EGEDCOMException(string.Format("The string {0} doesn't start with a valid number", str));
                    }
                    value = defValue;
                }
            } else {
                value = defValue;
            }

            return result;
        }

        public static bool IsDigit(char chr)
        {
            return chr >= '0' && chr <= '9';
        }

        public static bool IsDigits(string str)
        {
            bool res = false;

            if (!string.IsNullOrEmpty(str))
            {
                int I;
                for (I = 1; I <= str.Length; I++)
                {
                    char c = str[I - 1];
                    if (c < '0' || c >= ':')
                    {
                        break;
                    }
                }
                res = (I > str.Length);
            }

            return res;
        }

        #endregion

        #region GEDCOM Enums processing
        
        public static Encoding GetEncodingByCharacterSet(GEDCOMCharacterSet cs)
        {
            Encoding res = Encoding.Default;

            switch (cs) {
                case GEDCOMCharacterSet.csANSEL:
                    // [16/03/2016] not supported
                    //res = new AnselEncoding();
                    //break;

                case GEDCOMCharacterSet.csASCII:
                    res = Encoding.GetEncoding(1251);
                    break;

                case GEDCOMCharacterSet.csUNICODE:
                    res = Encoding.Unicode;
                    break;

                case GEDCOMCharacterSet.csUTF8:
                    res = Encoding.UTF8;
                    break;
            }

            return res;
        }

        public static GEDCOMRestriction GetRestrictionVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMRestriction.rnNone;

            GEDCOMRestriction res;
            str = str.Trim().ToLowerInvariant();
            
            if (str == "confidential")
            {
                res = GEDCOMRestriction.rnConfidential;
            }
            else if (str == "locked")
            {
                res = GEDCOMRestriction.rnLocked;
            }
            else if (str == "privacy")
            {
                res = GEDCOMRestriction.rnPrivacy;
            }
            else
            {
                res = GEDCOMRestriction.rnNone;
            }
            return res;
        }

        public static string GetRestrictionStr(GEDCOMRestriction value)
        {
            string s;

            switch (value) {
                case GEDCOMRestriction.rnConfidential:
                    s = "confidential";
                    break;

                case GEDCOMRestriction.rnLocked:
                    s = "locked";
                    break;

                case GEDCOMRestriction.rnPrivacy:
                    s = "privacy";
                    break;

                default:
                    s = "";
                    break;
            }

            return s;
        }

        public static GEDCOMPedigreeLinkageType GetPedigreeLinkageTypeVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMPedigreeLinkageType.plNone;

            GEDCOMPedigreeLinkageType result;
            str = str.Trim().ToLowerInvariant();
            
            if (str == "adopted")
            {
                result = GEDCOMPedigreeLinkageType.plAdopted;
            }
            else if (str == "birth")
            {
                result = GEDCOMPedigreeLinkageType.plBirth;
            }
            else if (str == "foster")
            {
                result = GEDCOMPedigreeLinkageType.plFoster;
            }
            else if (str == "sealing")
            {
                result = GEDCOMPedigreeLinkageType.plSealing;
            }
            else
            {
                result = GEDCOMPedigreeLinkageType.plNone;
            }
            return result;
        }

        public static string GetPedigreeLinkageTypeStr(GEDCOMPedigreeLinkageType value)
        {
            string s;
            switch (value) {
                case GEDCOMPedigreeLinkageType.plAdopted:
                    s = "adopted";
                    break;
                case GEDCOMPedigreeLinkageType.plBirth:
                    s = "birth";
                    break;
                case GEDCOMPedigreeLinkageType.plFoster:
                    s = "foster";
                    break;
                case GEDCOMPedigreeLinkageType.plSealing:
                    s = "sealing";
                    break;
                default:
                    s = "";
                    break;
            }
            return s;
        }

        public static GEDCOMChildLinkageStatus GetChildLinkageStatusVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMChildLinkageStatus.clNone;

            GEDCOMChildLinkageStatus result;
            str = str.Trim().ToLowerInvariant();
            
            if (str == "challenged")
            {
                result = GEDCOMChildLinkageStatus.clChallenged;
            }
            else if (str == "disproven")
            {
                result = GEDCOMChildLinkageStatus.clDisproven;
            }
            else if (str == "proven")
            {
                result = GEDCOMChildLinkageStatus.clProven;
            }
            else
            {
                result = GEDCOMChildLinkageStatus.clNone;
            }
            return result;
        }

        public static string GetChildLinkageStatusStr(GEDCOMChildLinkageStatus value)
        {
            string s;
            switch (value) {
                case GEDCOMChildLinkageStatus.clChallenged:
                    s = "challenged";
                    break;
                case GEDCOMChildLinkageStatus.clDisproven:
                    s = "disproven";
                    break;
                case GEDCOMChildLinkageStatus.clProven:
                    s = "proven";
                    break;
                default:
                    s = "";
                    break;
            }
            return s;
        }

        public static GKCommunicationType GetCommunicationTypeVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GKCommunicationType.ctVisit;

            GKCommunicationType result;
            str = str.Trim().ToLowerInvariant();
            
            if (str == "call")
            {
                result = GKCommunicationType.ctCall;
            }
            else if (str == "email")
            {
                result = GKCommunicationType.ctEMail;
            }
            else if (str == "fax")
            {
                result = GKCommunicationType.ctFax;
            }
            else if (str == "letter")
            {
                result = GKCommunicationType.ctLetter;
            }
            else if (str == "tape")
            {
                result = GKCommunicationType.ctTape;
            }
            else if (str == "visit")
            {
                result = GKCommunicationType.ctVisit;
            }
            else
            {
                result = GKCommunicationType.ctVisit;
            }
            return result;
        }

        public static string GetCommunicationTypeStr(GKCommunicationType value)
        {
            string s = "";
            switch (value) {
                case GKCommunicationType.ctCall:
                    s = "call";
                    break;
                case GKCommunicationType.ctEMail:
                    s = "email";
                    break;
                case GKCommunicationType.ctFax:
                    s = "fax";
                    break;
                case GKCommunicationType.ctLetter:
                    s = "letter";
                    break;
                case GKCommunicationType.ctTape:
                    s = "tape";
                    break;
                case GKCommunicationType.ctVisit:
                    s = "visit";
                    break;
            }
            return s;
        }

        public static GEDCOMMultimediaFormat GetMultimediaFormatVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMMultimediaFormat.mfNone;

            GEDCOMMultimediaFormat result;
            str = str.Trim().ToLowerInvariant();
            
            if (str == "bmp")
            {
                result = GEDCOMMultimediaFormat.mfBMP;
            }
            else if (str == "gif")
            {
                result = GEDCOMMultimediaFormat.mfGIF;
            }
            else if (str == "jpg" || str == "jpeg")
            {
                result = GEDCOMMultimediaFormat.mfJPG;
            }
            else if (str == "ole")
            {
                result = GEDCOMMultimediaFormat.mfOLE;
            }
            else if (str == "pcx")
            {
                result = GEDCOMMultimediaFormat.mfPCX;
            }
            else if (str == "tif" || str == "tiff")
            {
                result = GEDCOMMultimediaFormat.mfTIF;
            }
            else if (str == "wav")
            {
                result = GEDCOMMultimediaFormat.mfWAV;
            }
            else if (str == "txt")
            {
                result = GEDCOMMultimediaFormat.mfTXT;
            }
            else if (str == "rtf")
            {
                result = GEDCOMMultimediaFormat.mfRTF;
            }
            else if (str == "avi")
            {
                result = GEDCOMMultimediaFormat.mfAVI;
            }
            else if (str == "tga")
            {
                result = GEDCOMMultimediaFormat.mfTGA;
            }
            else if (str == "png")
            {
                result = GEDCOMMultimediaFormat.mfPNG;
            }
            else if (str == "mpg" || str == "mpeg")
            {
                result = GEDCOMMultimediaFormat.mfMPG;
            }
            else if (str == "htm" || str == "html")
            {
                result = GEDCOMMultimediaFormat.mfHTM;
            }
            else if (str == "raw")
            {
                result = GEDCOMMultimediaFormat.mfRAW;
            }
            else if (str == "mp3")
            {
                result = GEDCOMMultimediaFormat.mfMP3;
            }
            else if (str == "wma")
            {
                result = GEDCOMMultimediaFormat.mfWMA;
            }
            else if (str == "psd")
            {
                result = GEDCOMMultimediaFormat.mfPSD;
            }
            else if (str == "pdf")
            {
                result = GEDCOMMultimediaFormat.mfPDF;
            }
            else if (str == "mp4")
            {
                result = GEDCOMMultimediaFormat.mfMP4;
            }
            else if (str == "ogv")
            {
                result = GEDCOMMultimediaFormat.mfOGV;
            }
            else if (str == "mka")
            {
                result = GEDCOMMultimediaFormat.mfMKA;
            }
            else if (str == "wmv")
            {
                result = GEDCOMMultimediaFormat.mfWMV;
            }
            else if (str == "mkv")
            {
                result = GEDCOMMultimediaFormat.mfMKV;
            }
            else if (str == "mov")
            {
                result = GEDCOMMultimediaFormat.mfMOV;
            }
            else
            {
                result = GEDCOMMultimediaFormat.mfUnknown;
            }
            return result;
        }

        public static string GetMultimediaFormatStr(GEDCOMMultimediaFormat value)
        {
            string s;
            switch (value) {
                case GEDCOMMultimediaFormat.mfBMP:
                    s = "bmp";
                    break;
                case GEDCOMMultimediaFormat.mfGIF:
                    s = "gif";
                    break;
                case GEDCOMMultimediaFormat.mfJPG:
                    s = "jpg";
                    break;
                case GEDCOMMultimediaFormat.mfOLE:
                    s = "ole";
                    break;
                case GEDCOMMultimediaFormat.mfPCX:
                    s = "pcx";
                    break;
                case GEDCOMMultimediaFormat.mfTIF:
                    s = "tif";
                    break;
                case GEDCOMMultimediaFormat.mfWAV:
                    s = "wav";
                    break;
                case GEDCOMMultimediaFormat.mfTXT:
                    s = "txt";
                    break;
                case GEDCOMMultimediaFormat.mfRTF:
                    s = "rtf";
                    break;
                case GEDCOMMultimediaFormat.mfAVI:
                    s = "avi";
                    break;
                case GEDCOMMultimediaFormat.mfTGA:
                    s = "tga";
                    break;
                case GEDCOMMultimediaFormat.mfPNG:
                    s = "png";
                    break;
                case GEDCOMMultimediaFormat.mfMPG:
                    s = "mpg";
                    break;
                case GEDCOMMultimediaFormat.mfHTM:
                    s = "htm";
                    break;
                case GEDCOMMultimediaFormat.mfRAW:
                    s = "raw";
                    break;
                case GEDCOMMultimediaFormat.mfMP3:
                    s = "mp3";
                    break;
                case GEDCOMMultimediaFormat.mfWMA:
                    s = "wma";
                    break;
                case GEDCOMMultimediaFormat.mfPSD:
                    s = "psd";
                    break;
                case GEDCOMMultimediaFormat.mfPDF:
                    s = "pdf";
                    break;
                case GEDCOMMultimediaFormat.mfMP4:
                    s = "mp4";
                    break;
                case GEDCOMMultimediaFormat.mfOGV:
                    s = "ogv";
                    break;
                case GEDCOMMultimediaFormat.mfMKA:
                    s = "mka";
                    break;
                case GEDCOMMultimediaFormat.mfWMV:
                    s = "wmv";
                    break;
                case GEDCOMMultimediaFormat.mfMKV:
                    s = "mkv";
                    break;
                case GEDCOMMultimediaFormat.mfMOV:
                    s = "mov";
                    break;
                default:
                    s = "";
                    break;
            }
            return s;
        }

        public static GEDCOMMediaType GetMediaTypeVal(string str)
        {
            GEDCOMMediaType result = GEDCOMMediaType.mtUnknown;
            if (string.IsNullOrEmpty(str)) return result;

            str = str.Trim().ToLowerInvariant();

            if (str == "audio")
            {
                result = GEDCOMMediaType.mtAudio;
            }
            else if (str == "book")
            {
                result = GEDCOMMediaType.mtBook;
            }
            else if (str == "card")
            {
                result = GEDCOMMediaType.mtCard;
            }
            else if (str == "electronic")
            {
                result = GEDCOMMediaType.mtElectronic;
            }
            else if (str == "fiche")
            {
                result = GEDCOMMediaType.mtFiche;
            }
            else if (str == "film")
            {
                result = GEDCOMMediaType.mtFilm;
            }
            else if (str == "magazine")
            {
                result = GEDCOMMediaType.mtMagazine;
            }
            else if (str == "manuscript")
            {
                result = GEDCOMMediaType.mtManuscript;
            }
            else if (str == "map")
            {
                result = GEDCOMMediaType.mtMap;
            }
            else if (str == "newspaper")
            {
                result = GEDCOMMediaType.mtNewspaper;
            }
            else if (str == "photo")
            {
                result = GEDCOMMediaType.mtPhoto;
            }
            else if (str == "tombstone")
            {
                result = GEDCOMMediaType.mtTombstone;
            }
            else if (str == "video")
            {
                result = GEDCOMMediaType.mtVideo;
            }

            return result;
        }

        public static string GetMediaTypeStr(GEDCOMMediaType value)
        {
            string s;
            switch (value) {
                case GEDCOMMediaType.mtAudio:
                    s = "audio";
                    break;
                case GEDCOMMediaType.mtBook:
                    s = "book";
                    break;
                case GEDCOMMediaType.mtCard:
                    s = "card";
                    break;
                case GEDCOMMediaType.mtElectronic:
                    s = "electronic";
                    break;
                case GEDCOMMediaType.mtFiche:
                    s = "fiche";
                    break;
                case GEDCOMMediaType.mtFilm:
                    s = "film";
                    break;
                case GEDCOMMediaType.mtMagazine:
                    s = "magazine";
                    break;
                case GEDCOMMediaType.mtManuscript:
                    s = "manuscript";
                    break;
                case GEDCOMMediaType.mtMap:
                    s = "map";
                    break;
                case GEDCOMMediaType.mtNewspaper:
                    s = "newspaper";
                    break;
                case GEDCOMMediaType.mtPhoto:
                    s = "photo";
                    break;
                case GEDCOMMediaType.mtTombstone:
                    s = "tombstone";
                    break;
                case GEDCOMMediaType.mtVideo:
                    s = "video";
                    break;
                default:
                    s = "";
                    break;
            }
            return s;
        }

        public static GEDCOMNameType GetNameTypeVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMNameType.ntNone;

            GEDCOMNameType result;
            str = str.Trim().ToLowerInvariant();
            
            if (str == "aka")
            {
                result = GEDCOMNameType.ntAka;
            }
            else if (str == "birth")
            {
                result = GEDCOMNameType.ntBirth;
            }
            else if (str == "immigrant")
            {
                result = GEDCOMNameType.ntImmigrant;
            }
            else if (str == "maiden")
            {
                result = GEDCOMNameType.ntMaiden;
            }
            else if (str == "married")
            {
                result = GEDCOMNameType.ntMarried;
            }
            else
            {
                result = GEDCOMNameType.ntNone;
            }
            return result;
        }

        public static string GetNameTypeStr(GEDCOMNameType value)
        {
            string s = "";
            switch (value) {
                case GEDCOMNameType.ntNone:
                    s = "";
                    break;
                case GEDCOMNameType.ntAka:
                    s = "aka";
                    break;
                case GEDCOMNameType.ntBirth:
                    s = "birth";
                    break;
                case GEDCOMNameType.ntImmigrant:
                    s = "immigrant";
                    break;
                case GEDCOMNameType.ntMaiden:
                    s = "maiden";
                    break;
                case GEDCOMNameType.ntMarried:
                    s = "married";
                    break;
            }
            return s;
        }

        public static GKResearchStatus GetStatusVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GKResearchStatus.rsDefined;

            GKResearchStatus result;
            str = str.Trim().ToLowerInvariant();
            
            if (str == "inprogress")
            {
                result = GKResearchStatus.rsInProgress;
            }
            else if (str == "onhold")
            {
                result = GKResearchStatus.rsOnHold;
            }
            else if (str == "problems")
            {
                result = GKResearchStatus.rsProblems;
            }
            else if (str == "completed")
            {
                result = GKResearchStatus.rsCompleted;
            }
            else if (str == "withdrawn")
            {
                result = GKResearchStatus.rsWithdrawn;
            }
            else
            {
                result = GKResearchStatus.rsDefined;
            }
            return result;
        }

        public static string GetStatusStr(GKResearchStatus value)
        {
            string s = "";
            switch (value) {
                case GKResearchStatus.rsDefined:
                    s = "defined";
                    break;
                case GKResearchStatus.rsInProgress:
                    s = "inprogress";
                    break;
                case GKResearchStatus.rsOnHold:
                    s = "onhold";
                    break;
                case GKResearchStatus.rsProblems:
                    s = "problems";
                    break;
                case GKResearchStatus.rsCompleted:
                    s = "completed";
                    break;
                case GKResearchStatus.rsWithdrawn:
                    s = "withdrawn";
                    break;
            }
            return s;
        }

        public static GEDCOMSpouseSealingDateStatus GetSpouseSealingDateStatusVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMSpouseSealingDateStatus.sdsNone;

            GEDCOMSpouseSealingDateStatus result;
            str = str.Trim().ToUpperInvariant();
            
            if (str == "CANCELED")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsCanceled;
            }
            else if (str == "COMPLETED")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsCompleted;
            }
            else if (str == "EXCLUDED")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsExcluded;
            }
            else if (str == "DNS")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsDNS;
            }
            else if (str == "DNS/CAN")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsDNSCAN;
            }
            else if (str == "PRE-1970")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsPre1970;
            }
            else if (str == "SUBMITTED")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsSubmitted;
            }
            else if (str == "UNCLEARED")
            {
                result = GEDCOMSpouseSealingDateStatus.sdsUncleared;
            }
            else
            {
                result = GEDCOMSpouseSealingDateStatus.sdsNone;
            }
            return result;
        }

        public static string GetSpouseSealingDateStatusStr(GEDCOMSpouseSealingDateStatus value)
        {
            string str;
            switch (value) {
                case GEDCOMSpouseSealingDateStatus.sdsCanceled:
                    str = "CANCELED";
                    break;
                case GEDCOMSpouseSealingDateStatus.sdsCompleted:
                    str = "COMPLETED";
                    break;
                case GEDCOMSpouseSealingDateStatus.sdsExcluded:
                    str = "EXCLUDED";
                    break;
                case GEDCOMSpouseSealingDateStatus.sdsDNS:
                    str = "DNS";
                    break;
                case GEDCOMSpouseSealingDateStatus.sdsDNSCAN:
                    str = "DNS/CAN";
                    break;
                case GEDCOMSpouseSealingDateStatus.sdsPre1970:
                    str = "PRE-1970";
                    break;
                case GEDCOMSpouseSealingDateStatus.sdsSubmitted:
                    str = "SUBMITTED";
                    break;
                case GEDCOMSpouseSealingDateStatus.sdsUncleared:
                    str = "UNCLEARED";
                    break;
                default:
                    str = "";
                    break;
            }
            return str;
        }

        public static GEDCOMOrdinanceProcessFlag GetOrdinanceProcessFlagVal(string su)
        {
            if (string.IsNullOrEmpty(su)) return GEDCOMOrdinanceProcessFlag.opNone;

            GEDCOMOrdinanceProcessFlag result;
            su = su.Trim().ToUpperInvariant(); // FIXME
            
            if (su == "YES")
            {
                result = GEDCOMOrdinanceProcessFlag.opYes;
            }
            else if (su == "NO")
            {
                result = GEDCOMOrdinanceProcessFlag.opNo;
            }
            else
            {
                result = GEDCOMOrdinanceProcessFlag.opNone;
            }
            return result;
        }

        public static string GetOrdinanceProcessFlagStr(GEDCOMOrdinanceProcessFlag value)
        {
            string str = "";
            switch (value) {
                case GEDCOMOrdinanceProcessFlag.opNone:
                    str = "";
                    break;
                case GEDCOMOrdinanceProcessFlag.opYes:
                    str = "yes";
                    break;
                case GEDCOMOrdinanceProcessFlag.opNo:
                    str = "no";
                    break;
            }
            return str;
        }

        public static string GetPriorityStr(GKResearchPriority value)
        {
            string str = "";
            switch (value) {
                case GKResearchPriority.rpNone:
                    str = "";
                    break;
                case GKResearchPriority.rpLow:
                    str = "low";
                    break;
                case GKResearchPriority.rpNormal:
                    str = "normal";
                    break;
                case GKResearchPriority.rpHigh:
                    str = "high";
                    break;
                case GKResearchPriority.rpTop:
                    str = "top";
                    break;
            }
            return str;
        }

        public static GKResearchPriority GetPriorityVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GKResearchPriority.rpNone;

            string su = str.Trim().ToLowerInvariant();
            GKResearchPriority result;

            if (su == "low")
            {
                result = GKResearchPriority.rpLow;
            }
            else if (su == "normal")
            {
                result = GKResearchPriority.rpNormal;
            }
            else if (su == "high")
            {
                result = GKResearchPriority.rpHigh;
            }
            else if (su == "top")
            {
                result = GKResearchPriority.rpTop;
            }
            else
            {
                result = GKResearchPriority.rpNone;
            }
            
            return result;
        }

        public static string GetCharacterSetStr(GEDCOMCharacterSet value)
        {
            string str = "";
            switch (value) {
                case GEDCOMCharacterSet.csASCII:
                    str = "ASCII";
                    break;
                case GEDCOMCharacterSet.csANSEL:
                    str = "ANSEL";
                    break;
                case GEDCOMCharacterSet.csUNICODE:
                    str = "UNICODE";
                    break;
                case GEDCOMCharacterSet.csUTF8:
                    str = "UTF-8";
                    break;
            }
            return str;
        }

        public static GEDCOMCharacterSet GetCharacterSetVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMCharacterSet.csASCII;

            string su = str.ToUpperInvariant();
            GEDCOMCharacterSet result;

            if (su == "ASCII" || su == "ANSI" || su == "IBMPC")
            {
                result = GEDCOMCharacterSet.csASCII;
            }
            else if (su == "ANSEL")
            {
                result = GEDCOMCharacterSet.csANSEL;
            }
            else if (su == "UNICODE")
            {
                result = GEDCOMCharacterSet.csUNICODE;
            }
            else if (su == "UTF8" || su == "UTF-8")
            {
                result = GEDCOMCharacterSet.csUTF8;
            }
            else
            {
                result = GEDCOMCharacterSet.csASCII;
            }
            
            return result;
        }

        public static GEDCOMSex GetSexVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMSex.svNone;

            string su = str.Trim().ToUpperInvariant();
            GEDCOMSex result;

            switch (su) {
                case "M":
                    result = GEDCOMSex.svMale;
                    break;
                case "F":
                    result = GEDCOMSex.svFemale;
                    break;
                case "U":
                    result = GEDCOMSex.svUndetermined;
                    break;
                default:
                    result = GEDCOMSex.svNone;
                    break;
            }
            
            return result;
        }

        public static string GetSexStr(GEDCOMSex value)
        {
            string str;
            
            switch (value) {
                case GEDCOMSex.svMale:
                    str = "M";
                    break;
                case GEDCOMSex.svFemale:
                    str = "F";
                    break;
                case GEDCOMSex.svUndetermined:
                    str = "U";
                    break;
                default:
                    str = "";
                    break;
            }
            
            return str;
        }


        public static GEDCOMBaptismDateStatus GetBaptismDateStatusVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMBaptismDateStatus.bdsNone;

            string su = str.Trim().ToUpperInvariant();
            GEDCOMBaptismDateStatus result;

            if (su == "CHILD")
            {
                result = GEDCOMBaptismDateStatus.bdsChild;
            }
            else if (su == "COMPLETED")
            {
                result = GEDCOMBaptismDateStatus.bdsCompleted;
            }
            else if (su == "EXCLUDED")
            {
                result = GEDCOMBaptismDateStatus.bdsExcluded;
            }
            else if (su == "PRE-1970")
            {
                result = GEDCOMBaptismDateStatus.bdsPre1970;
            }
            else if (su == "STILLBORN")
            {
                result = GEDCOMBaptismDateStatus.bdsStillborn;
            }
            else if (su == "SUBMITTED")
            {
                result = GEDCOMBaptismDateStatus.bdsSubmitted;
            }
            else if (su == "UNCLEARED")
            {
                result = GEDCOMBaptismDateStatus.bdsUncleared;
            }
            else
            {
                result = GEDCOMBaptismDateStatus.bdsNone;
            }
            return result;
        }

        public static string GetBaptismDateStatusStr(GEDCOMBaptismDateStatus value)
        {
            string str = "";
            switch (value)
            {
                case GEDCOMBaptismDateStatus.bdsChild:
                    str = "CHILD";
                    break;

                case GEDCOMBaptismDateStatus.bdsCompleted:
                    str = "COMPLETED";
                    break;

                case GEDCOMBaptismDateStatus.bdsExcluded:
                    str = "EXCLUDED";
                    break;

                case GEDCOMBaptismDateStatus.bdsPre1970:
                    str = "PRE-1970";
                    break;

                case GEDCOMBaptismDateStatus.bdsStillborn:
                    str = "STILLBORN";
                    break;

                case GEDCOMBaptismDateStatus.bdsSubmitted:
                    str = "SUBMITTED";
                    break;

                case GEDCOMBaptismDateStatus.bdsUncleared:
                    str = "UNCLEARED";
                    break;
            }

            return str;
        }

        public static GEDCOMEndowmentDateStatus GetEndowmentDateStatusVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMEndowmentDateStatus.edsNone;

            string su = str.Trim().ToUpperInvariant();
            GEDCOMEndowmentDateStatus result;

            if (su == "CHILD")
            {
                result = GEDCOMEndowmentDateStatus.edsChild;
            }
            else if (su == "COMPLETED")
            {
                result = GEDCOMEndowmentDateStatus.edsCompleted;
            }
            else if (su == "EXCLUDED")
            {
                result = GEDCOMEndowmentDateStatus.edsExcluded;
            }
            else if (su == "INFANT")
            {
                result = GEDCOMEndowmentDateStatus.edsInfant;
            }
            else if (su == "PRE-1970")
            {
                result = GEDCOMEndowmentDateStatus.edsPre1970;
            }
            else if (su == "STILLBORN")
            {
                result = GEDCOMEndowmentDateStatus.edsStillborn;
            }
            else if (su == "SUBMITTED")
            {
                result = GEDCOMEndowmentDateStatus.edsSubmitted;
            }
            else if (su == "UNCLEARED")
            {
                result = GEDCOMEndowmentDateStatus.edsUncleared;
            }
            else
            {
                result = GEDCOMEndowmentDateStatus.edsNone;
            }
            return result;
        }

        public static string GetEndowmentDateStatusStr(GEDCOMEndowmentDateStatus value)
        {
            string str = "";
            
            switch (value)
            {
                case GEDCOMEndowmentDateStatus.edsChild:
                    str = "CHILD";
                    break;

                case GEDCOMEndowmentDateStatus.edsCompleted:
                    str = "COMPLETED";
                    break;

                case GEDCOMEndowmentDateStatus.edsExcluded:
                    str = "EXCLUDED";
                    break;

                case GEDCOMEndowmentDateStatus.edsInfant:
                    str = "INFANT";
                    break;

                case GEDCOMEndowmentDateStatus.edsPre1970:
                    str = "PRE-1970";
                    break;

                case GEDCOMEndowmentDateStatus.edsStillborn:
                    str = "STILLBORN";
                    break;

                case GEDCOMEndowmentDateStatus.edsSubmitted:
                    str = "SUBMITTED";
                    break;

                case GEDCOMEndowmentDateStatus.edsUncleared:
                    str = "UNCLEARED";
                    break;
            }

            return str;
        }

        public static GEDCOMChildSealingDateStatus GetChildSealingDateStatusVal(string str)
        {
            if (string.IsNullOrEmpty(str)) return GEDCOMChildSealingDateStatus.cdsNone;

            string su = str.Trim().ToUpperInvariant();
            GEDCOMChildSealingDateStatus result;

            if (su == "BIC")
            {
                result = GEDCOMChildSealingDateStatus.cdsBIC;
            }
            else if (su == "EXCLUDED")
            {
                result = GEDCOMChildSealingDateStatus.cdsExcluded;
            }
            else if (su == "PRE-1970")
            {
                result = GEDCOMChildSealingDateStatus.cdsPre1970;
            }
            else if (su == "STILLBORN")
            {
                result = GEDCOMChildSealingDateStatus.cdsStillborn;
            }
            else if (su == "SUBMITTED")
            {
                result = GEDCOMChildSealingDateStatus.cdsSubmitted;
            }
            else if (su == "UNCLEARED")
            {
                result = GEDCOMChildSealingDateStatus.cdsUncleared;
            }
            else
            {
                result = GEDCOMChildSealingDateStatus.cdsNone;
            }

            return result;
        }

        public static string GetChildSealingDateStatusStr(GEDCOMChildSealingDateStatus value)
        {
            string str = "";

            switch (value)
            {
                case GEDCOMChildSealingDateStatus.cdsBIC:
                    str = "BIC";
                    break;

                case GEDCOMChildSealingDateStatus.cdsExcluded:
                    str = "EXCLUDED";
                    break;

                case GEDCOMChildSealingDateStatus.cdsPre1970:
                    str = "PRE-1970";
                    break;

                case GEDCOMChildSealingDateStatus.cdsStillborn:
                    str = "STILLBORN";
                    break;

                case GEDCOMChildSealingDateStatus.cdsSubmitted:
                    str = "SUBMITTED";
                    break;

                case GEDCOMChildSealingDateStatus.cdsUncleared:
                    str = "UNCLEARED";
                    break;
            }

            return str;
        }
        
        #endregion

        #region Other

        public static StreamReader OpenStreamReader(Stream src, Encoding defaultEncoding)
        {
            Encoding encodingSource = defaultEncoding;
            bool detectEncoding = false;

            if (src.CanSeek)
            {
                byte[] bPreamble = new byte[4];
                int iReaded = src.Read(bPreamble, 0, 4);

                if (iReaded >= 3 && bPreamble[0] == 0xEF && bPreamble[1] == 0xBB && bPreamble[2] == 0xBF) // utf-8
                    encodingSource = Encoding.UTF8;
                else if (iReaded == 4 && bPreamble[0] == 0x00 && bPreamble[1] == 0x00 && bPreamble[2] == 0xFE && bPreamble[3] == 0xFF) // utf-32 EB
                {
                    encodingSource = Encoding.GetEncoding("utf-32"); // is a EL codepage, but the StreamReader should switch to EB
                    detectEncoding = true;
                }
                else if (iReaded == 4 && bPreamble[0] == 0xFF && bPreamble[1] == 0xFE && bPreamble[2] == 0x00 && bPreamble[3] == 0x00) // utf-32 EL
                    encodingSource = Encoding.GetEncoding("utf-32");
                else if (iReaded >= 2 && bPreamble[0] == 0xFE && bPreamble[1] == 0xFF) // utf-16 EB
                    encodingSource = Encoding.BigEndianUnicode;
                else if (iReaded >= 2 && bPreamble[0] == 0xFF && bPreamble[1] == 0xFE) // utf-16 EL
                    encodingSource = Encoding.Unicode;

                src.Seek(-iReaded, SeekOrigin.Current);
            }
            else
                detectEncoding = true;

            return new StreamReader(src, encodingSource, detectEncoding);
        }

        public static bool IsUnicodeEncoding(Encoding encoding)
        {
            return (encoding == Encoding.Unicode || encoding == Encoding.UTF7 || encoding == Encoding.UTF8 || encoding == Encoding.UTF32);
        }

        /// <summary>
        /// Fix of errors that are in the dates of FamilyTreeBuilder.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FixFTB(string str)
        {
            string result = str;
            string su = result.Substring(0, 3).ToUpperInvariant();

            if (su == GEDCOMCustomDate.GEDCOMDateRangeArray[0] ||
                su == GEDCOMCustomDate.GEDCOMDateRangeArray[1] ||
                su == GEDCOMCustomDate.GEDCOMDateApproximatedArray[1] ||
                su == GEDCOMCustomDate.GEDCOMDateApproximatedArray[2] ||
                su == GEDCOMCustomDate.GEDCOMDateApproximatedArray[3])
            {
                result = result.Remove(0, 4);
            }
            return result;
        }

        /// <summary>
        /// Fix of line errors that are in the files of FamilyTreeBuilder.
        /// </summary>
        public static void FixFTBLine(GEDCOMCustomRecord curRecord, GEDCOMTag curTag, int lineNum, string str)
        {
            try
            {
                if (curTag is GEDCOMNotes) {
                    curTag.AddTag("CONT", str, null);
                } else {
                    if (curRecord != null) {
                        curRecord.AddTag("NOTE", str, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("GKUtils.FixFTBLine(): Line " + lineNum.ToString() + " failed correct: " + ex.Message);
            }
        }

        // FIXME: there is the bug - use GEDCOMMonthArray without depend to Calendar (hebrew and islamic month's names)
        public static string StrToGEDCOMDate(string strDate, bool aException)
        {
            if (string.IsNullOrEmpty(strDate)) return "";

            if (strDate.IndexOf("/") >= 0) strDate = strDate.Replace("/", ".");
            if (strDate.IndexOf("_") >= 0) strDate = strDate.Replace("_", " ");

            string[] dtParts = strDate.Split('.');
            if (dtParts.Length < 3)
            {
                if (aException) {
                    throw new GEDCOMDateException(string.Format("GKUtils.StrToGEDCOMDate(): date format is invalid {0}", strDate));
                }

                return "";
            }

            string result = "";

            string pd = dtParts[0].Trim();
            string pm = dtParts[1].Trim();
            string py = dtParts[2].Trim();

            if (pd != "") result = result + pd + " ";
            if (pm != "") result = result + GEDCOMCustomDate.GEDCOMMonthArray[SysUtils.ParseInt(pm, 1) - 1] + " ";
            if (py != "") result += py;

            return result;
        }

        public static string TrimLeft(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            int len = str.Length;
            int i = 1;
            while (i <= len && str[i - 1] <= ' ') i++;

            string result;
            if (i > len) {
                result = "";
            } else {
                result = ((i != 1) ? str.Substring(i - 1) : str);
            }
            return result;
        }

        public static string TrimRight(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            int len = str.Length;
            int i = len;
            while (i > 0 && str[i - 1] <= ' ') i--;

            string result = ((i != len) ? str.Substring(0, i) : str);
            return result;
        }

        public static string EncodeUID(byte[] binaryKey)
        {
            StringBuilder result = new StringBuilder(36);
            byte checkA = 0;
            byte checkB = 0;

            int num = binaryKey.Length;
            for (int i = 0; i < num; i++)
            {
                byte val = binaryKey[i];
                checkA = unchecked((byte)(checkA + (uint)val));
                checkB = unchecked((byte)(checkB + (uint)checkA));
                result.Append(val.ToString("X2"));
            }

            result.Append(checkA.ToString("X2"));
            result.Append(checkB.ToString("X2"));

            return result.ToString();
        }

        public static string GetRectUID(int x1, int y1, int x2, int y2)
        {
            byte[] bx1 = BitConverter.GetBytes((ushort)x1);
            byte[] by1 = BitConverter.GetBytes((ushort)y1);
            byte[] bx2 = BitConverter.GetBytes((ushort)x2);
            byte[] by2 = BitConverter.GetBytes((ushort)y2);

            byte[] buffer = new byte[8];
            Buffer.BlockCopy(bx1, 0, buffer, 0, 2);
            Buffer.BlockCopy(by1, 0, buffer, 2, 2);
            Buffer.BlockCopy(bx2, 0, buffer, 4, 2);
            Buffer.BlockCopy(by2, 0, buffer, 6, 2);

            return EncodeUID(buffer);
        }

        public static string GetMultimediaLinkUID(GEDCOMMultimediaLink mmLink)
        {
            string result = null;
            try
            {
                if (mmLink != null && mmLink.Value != null)
                {
                    ExtRect cutoutArea;
                    if (mmLink.IsPrimaryCutout) {
                        cutoutArea = mmLink.CutoutPosition.Value;
                    } else {
                        cutoutArea = ExtRect.CreateEmpty();
                    }

                    GEDCOMMultimediaRecord mmRec = (GEDCOMMultimediaRecord)mmLink.Value;
                    result = mmRec.UID + "-" + GEDCOMUtils.GetRectUID(cutoutArea.Left, cutoutArea.Top, cutoutArea.Right, cutoutArea.Bottom);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("GEDCOMUtils.GetMultimediaLinkUID(): " + ex.Message);
                result = null;
            }
            return result;
        }

        #endregion

        #region RelativeYear utils

        public static int GetRelativeYear(GEDCOMRecordWithEvents evsRec, string evSign)
        {
            int result;

            if (evsRec == null) {
                result = 0;
            } else {
                GEDCOMCustomEvent evt = evsRec.FindEvent(evSign);
                result = GetRelativeYear(evt);
            }

            return result;
        }

        /// <summary>
        /// In the historical chronology of the year 0 does not exist.
        /// Therefore, the digit 0 in the year value can be used as a sign of lack or error.
        /// RelativeYear - introduced for the purposes of uniform chronology years in the Gregorian calendar.
        /// Is estimated from -4714 BC to 3268 AD.
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public static int GetRelativeYear(GEDCOMCustomEvent evt)
        {
            return (evt == null) ? 0 : GetRelativeYear(evt.Detail.Date);
        }

        public static int GetRelativeYear(GEDCOMDateValue dateVal)
        {
            return (dateVal.Value == null) ? 0 : GetRelativeYear(dateVal.Value);
        }

        // TODO: all of years lead to the Gregorian calendar!
        public static int GetRelativeYear(GEDCOMCustomDate customDate)
        {
            if (customDate == null) {
                return 0;
            } else {
                GEDCOMDate date = customDate as GEDCOMDate;

                if (date == null) {
                    return 0;
                } else {
                    int year = date.Year;
                    if (year <= 0) {
                        return 0;
                    } else {
                        if (date.YearBC) year = -year;
                        // TODO: calendars and other!
                    }

                    return year;
                }
            }
        }

        #endregion

        #region UDN utils

        public static UDN GetUDN(GEDCOMCustomEvent evt)
        {
            return (evt == null) ? UDN.CreateEmpty() : evt.Detail.Date.GetUDN();
        }

        public static UDN GetUDN(GEDCOMRecordWithEvents evsRec, string evSign)
        {
            UDN result;

            if (evsRec == null) {
                result = UDN.CreateEmpty();
            } else {
                GEDCOMCustomEvent evt = evsRec.FindEvent(evSign);
                result = GetUDN(evt);
            }

            return result;
        }

        public static UDN GetUDN(string dateStr)
        {
            try
            {
                dateStr = StrToGEDCOMDate(dateStr, false);

                GEDCOMDateExact dtx = (GEDCOMDateExact)GEDCOMDateExact.Create(null, null, "", "");
                dtx.ParseString(dateStr);
                return dtx.GetUDN();
            }
            catch
            {
                return UDN.CreateEmpty();
            }
        }

        #endregion

        #region Clean utils

        public static void CleanFamily(GEDCOMFamilyRecord famRec)
        {
            if (famRec == null) return;

            int num = famRec.Children.Count;
            for (int i = 0; i < num; i++)
            {
                GEDCOMIndividualRecord child = (GEDCOMIndividualRecord)famRec.Children[i].Value;
                child.DeleteChildToFamilyLink(famRec);
            }

            GEDCOMIndividualRecord spouse;

            spouse = famRec.GetHusband();
            famRec.RemoveSpouse(spouse);

            spouse = famRec.GetWife();
            famRec.RemoveSpouse(spouse);
        }

        public static void CleanIndividual(GEDCOMIndividualRecord indRec)
        {
            if (indRec == null) return;
            
            for (int i = indRec.ChildToFamilyLinks.Count - 1; i >= 0; i--)
            {
                GEDCOMFamilyRecord family = indRec.ChildToFamilyLinks[i].Family;
                family.DeleteChild(indRec);
            }

            for (int i = indRec.SpouseToFamilyLinks.Count - 1; i >= 0; i--)
            {
                GEDCOMFamilyRecord family = indRec.SpouseToFamilyLinks[i].Family;
                family.RemoveSpouse(indRec);
            }

            for (int i = indRec.Groups.Count - 1; i >= 0; i--)
            {
                GEDCOMGroupRecord group = (GEDCOMGroupRecord)indRec.Groups[i].Value;
                group.RemoveMember(indRec);
            }
        }

        #endregion
    }
}
