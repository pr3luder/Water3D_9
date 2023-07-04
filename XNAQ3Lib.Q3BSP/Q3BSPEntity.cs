///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Aanand Narayanan
// Copyright (c) 2006-2009 All rights reserved
///////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XNAQ3Lib.Q3BSP
{
    public class Q3BSPEntity
    {
        Hashtable entries;
        string className;

        public Q3BSPEntity()
        {
            entries = new Hashtable();
            className = "none";
        }

        public string this[string key]
        {
            get { return (string)entries[key]; }
            set { entries[key] = value; }
        }

        public Hashtable Entries
        {
            get { return entries; }
            set { entries = value; }
        }

        public void ParseString(string inputString)
        {
            string[] lines = inputString.Split(new char[] { '\n' });
            Regex rx = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);

            foreach (string oneLine in lines)
            {
                MatchCollection matches;

                string str = oneLine.Trim();
                matches = rx.Matches(str);
                if (1 < matches.Count)
                {
                    if ("classname" == matches[0].Groups[1].Value)
                    {
                        className = matches[1].Groups[1].Value;
                    }
                    else
                    {
                        entries[matches[0].Groups[1].Value] = matches[1].Groups[1].Value;
                    }
                }
            }
        }

        public override string ToString()
        {
            return "classname=" + className;
        }
    }

    public class Q3BSPEntityManager
    {
        Q3BSPEntity[] entities;

        public bool LoadEntities(string entityString)
        {
            Regex rx = new Regex("{([^}]*)}", RegexOptions.Compiled | RegexOptions.Multiline);
            MatchCollection matches = rx.Matches(entityString);

            if (0 < matches.Count)
            {
                entities = new Q3BSPEntity[matches.Count];
                for(int i=0;i<matches.Count; i++)
                {
                    entities[i] = new Q3BSPEntity();
                    entities[i].ParseString(matches[i].Groups[1].Value);
                }
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            if (null == entities)
            {
                return "null";
            }

            string str = "Entity Count: " + entities.Length.ToString();
            foreach (Q3BSPEntity e in entities)
            {
                str += "\r\n" + e.ToString();
            }

            return str;
        }

    }
}
