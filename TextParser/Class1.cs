using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextParser
{
    public class ListMerge
    {
        List<string[]> _OriginalList;
        List<string[]> _TranslatedList;
        List<string[]> _MergedList = new List<string[]>();
        public List<string[]> MergendList
        {
            get { return _MergedList; }
        }
        string[] keys;
        Option _Option;

        public enum Option { Update, Diff };

        public ListMerge(List<string[]> OriginalList, List<string[]> TranslatedList, Option option)
        {
            _OriginalList = OriginalList;
            _TranslatedList = TranslatedList;
            _Option = option;
            switch (option)
            {
                case Option.Update:
                    MergeUpdate();
                    break;
                case Option.Diff:
                    MergeDiff();
                    break;
            }
        }

        private void MergeUpdate()
        {
            for (int i = 0; i < _OriginalList.Count; i++)
            {
                int j = 0;
                for (j = 0; j < _TranslatedList.Count; j++)
                {
                    if (_OriginalList[i][0] == _TranslatedList[j][0]) break;
                }
                if (j == _TranslatedList.Count)
                {
                    _MergedList.Add(new string[] { _OriginalList[i][0], _OriginalList[i][1], "", "" });
                }
                else
                {
                    if (_TranslatedList[j].Length == 3)
                    {
                        _MergedList.Add(new string[] { _OriginalList[i][0], _OriginalList[i][1], _TranslatedList[j][2], "" });
                    }
                    else
                    {
                        _MergedList.Add(new string[] { _OriginalList[i][0], _OriginalList[i][1], _TranslatedList[j][2], _TranslatedList[j][3] });
                    }
                }
            }
        }

        private void MergeDiff()
        {
            List<string[]> tempList = new List<string[]>();
            for (int i = 0; i < _OriginalList.Count; i++)
            {
                int j = 0;
                for (j = 0; j < _TranslatedList.Count; j++)
                {
                    if (_OriginalList[i][0] == _TranslatedList[j][0]) break;
                }
                if (j == _TranslatedList.Count || _TranslatedList[j][2] == "")
                {
                    tempList.Add(new string[] { _OriginalList[i][0], _OriginalList[i][1], "", "" });
                }
                else
                {
                    if (_TranslatedList[j].Length == 3)
                    {
                        _MergedList.Add(new string[] { _OriginalList[i][0], _OriginalList[i][1], _TranslatedList[j][2], "" });
                    }
                    else
                    {
                        _MergedList.Add(new string[] { _OriginalList[i][0], _OriginalList[i][1], _TranslatedList[j][2], _TranslatedList[j][3] });
                    }
                }
            }
            _MergedList.AddRange(tempList);
        }

        public void RemoveSharp()
        {
            List<string[]> tempList = new List<string[]>();
            for (int i = 0; i < _MergedList.Count; i++)
            {
                if (_MergedList[i][0][0] != '#')
                {
                    tempList.Add(_MergedList[i]);
                }
            }
            _MergedList = tempList;
        }

    }

    public class TXTParser
    {
        List<string[]> _ListContext = new List<string[]>();
        public List<string[]> ListContext
        {
            get
            {
                return _ListContext;
            }
        }
        public TXTParser()
        {

        }
        public TXTParser(List<string[]> listContext)
        {
            _ListContext = listContext;
        }

        public int Length()
        {
            return _ListContext.Count;
        }
        public void Append(string str)
        {
            string[] AppendedStr = str.Split('=');
            int tempIter = AppendedStr[0].LastIndexOf('\n');
            string[] tempStrs = { AppendedStr[0].Substring(0, tempIter), AppendedStr[0].Substring(tempIter + 1) };
            if (tempStrs.Length == 2)
            {
                _ListContext.Add(new string[] { RemoveCRLFSpace(tempStrs[0]), "" });
            }
            for (int i = 0; i < AppendedStr.Length - 1; i++)
            {
                int LFIter1 = AppendedStr[i].LastIndexOf('\n');
                string[] SplitedStrs1 = { AppendedStr[i].Substring(0, LFIter1), AppendedStr[i].Substring(LFIter1 + 1) };
                string key = SplitedStrs1[1];
                int DetectSharpIter = AppendedStr[i + 1].IndexOf('#');
                if (DetectSharpIter == -1)
                {
                    int LFIter2 = AppendedStr[i + 1].LastIndexOf('\n');
                    if (LFIter2 != -1)
                    {
                        string[] SplitedStrs2 = { AppendedStr[i + 1].Substring(0, LFIter2), AppendedStr[i + 1].Substring(LFIter2 + 1) };
                        string value = SplitedStrs2[0];
                        _ListContext.Add(new string[] { RemoveCRLFSpace(key), RemoveCRLFSpace(value) });
                    }
                    else
                    {
                        _ListContext.Add(new string[] { RemoveCRLFSpace(key), RemoveCRLFSpace(AppendedStr[i + 1]) });
                    }
                }
                else
                {
                    string[] SplitedStrs2 = { AppendedStr[i + 1].Substring(0, DetectSharpIter), AppendedStr[i + 1].Substring(DetectSharpIter) };
                    _ListContext.Add(new string[] { RemoveCRLFSpace(key), RemoveCRLFSpace(SplitedStrs2[0]) });

                    int LFIter2 = SplitedStrs2[1].LastIndexOf('\n');
                    string SharpString = SplitedStrs2[1].Substring(0, LFIter2);
                    _ListContext.Add(new string[] { SharpString, "" });
                }



                /*int LFIter2 = AppendedStr[i + 1].LastIndexOf('\n');
                if (LFIter2 != -1)
                {
                    string[] SplitedStrs2 = { AppendedStr[i + 1].Substring(0, LFIter2), AppendedStr[i + 1].Substring(LFIter2 + 1) };
                    int DetectSharpIter = SplitedStrs2[1].IndexOf('#');
                    if (DetectSharpIter != -1)
                    {
                        string[] SharpSplitedStr = { SplitedStrs2[0].Substring(0, DetectSharpIter), SplitedStrs2[0].Substring(DetectSharpIter) };
                        string value = SharpSplitedStr[0];
                        _ListContext.Add(new string[] { RemoveCRLFSpace(key), RemoveCRLFSpace(value) });
                        _ListContext.Add(new string[] { RemoveCRLFSpace(SharpSplitedStr[1]), "" });
                    }
                    else
                    {
                        string value = SplitedStrs2[0];
                        _ListContext.Add(new string[] { RemoveCRLFSpace(key), RemoveCRLFSpace(value) });
                    }
                } else
                {
                    _ListContext.Add(new string[] { RemoveCRLFSpace(key), RemoveCRLFSpace(AppendedStr[i + 1]) });
                }*/
            }
        }

        public string ReadItem(int iter)
        {
            if (iter >= _ListContext.Count)
            {
                throw new IndexOutOfRangeException("_ListContext.Count : " + _ListContext.Count.ToString() + ", iter : " + iter.ToString());
            }
            string[] tempStr = _ListContext[iter];
            if (tempStr[1] == "" && tempStr[0].IndexOf('#') != -1)
            {
                return tempStr[0] + "\r\n";
            }
            else if (tempStr[1] == "")
            {
                return tempStr[0] + " = \r\n";
            }
            else
            {
                string buffer = "";
                for (int i = 0; i < tempStr.Length; i++)
                {
                    buffer += tempStr[i] + " = ";
                }
                if (tempStr.Length == 1 && tempStr[0].IndexOf('#') == -1)
                {
                    buffer += " = ";
                }
                buffer = buffer.Substring(0, buffer.Length - 3) + "\r\n";
                return buffer;
            }
        }

        public string ReadFromTo(int offset, int count)
        {
            string buffer = "";
            if ((offset + count) > _ListContext.Count)
            {
                throw new IndexOutOfRangeException("offset : " + offset.ToString() + ", count : " + count.ToString() + ", _ListContext.Count : " + _ListContext.Count.ToString());
            }
            for (int i = offset; i < (offset + count); i++)
            {
                buffer += ReadItem(i);
            }
            return buffer;
        }

        public string ReadAll()
        {
            return ReadFromTo(0, _ListContext.Count);
        }

        static string RemoveCRLFSpace(string str)
        {
            while (str.Length != 0 && (str[0] == ' ' || str[0] == '\n' || str[0] == '\r'))
            {
                str = str.Substring(1);
            }
            while (str.Length != 0 && (str[str.Length - 1] == ' ' || str[str.Length - 1] == '\n' || str[str.Length - 1] == '\r'))
            {
                str = str.Substring(0, str.Length - 1);
            }
            return str;
        }
    }

    public class CSVParser
    {
        public List<string[]> ListContext
        {
            get
            {
                return _ListContext;
            }
        }
        char _Delimiter;
        List<string[]> _ListContext = new List<string[]>();
        public CSVParser()
        {
            _Delimiter = ',';
        }
        public CSVParser(char Delimiter)
        {
            _Delimiter = Delimiter;
        }
        public CSVParser(List<string[]> ListContext)
        {
            _ListContext = ListContext;
            _Delimiter = ',';
        }
        public CSVParser(List<string[]> ListContext, char Delimiter)
        {
            _ListContext = ListContext;
            _Delimiter = Delimiter;
        }
        public int Length()
        {
            return _ListContext.Count;
        }
        public void Append(string str)
        {
            string[] AppendedStr = str.Split('\n');
            for (int i = 0; i < AppendedStr.Length; i++)
            {
                string temp = AppendedStr[i].Replace("\r", "");
                int index = -1;
                int CountOfQMark = 0;
                do
                {
                    CountOfQMark = 0;
                    do
                    {
                        index = temp.IndexOf('\"', index + 1);
                        if (index != -1)
                        {
                            CountOfQMark++;
                        }
                    } while (index != -1);
                    if (CountOfQMark % 2 == 1)
                    {
                        temp += "\r\n" + AppendedStr[++i].Replace("\r", "");
                    }
                } while (CountOfQMark % 2 == 1);

                if (temp != "")
                {
                    _ListContext.Add(Parser(temp));
                }
                //printItem(ListContext.Count - 1);
            }
        }
        public void printItem(int iter)
        {
            Console.Write(ReadItem(iter, _Delimiter));
        }
        public void printItem(int iter, char Delimiter)
        {
            Console.Write(ReadItem(iter, Delimiter));
        }
        public string ReadItem(int iter)
        {
            return ReadItem(iter, _Delimiter, _ListContext[0]);
        }

        public string ReadItem(int iter, char Delimiter)
        {
            return ReadItem(iter, Delimiter, _ListContext[0]);
        }

        public string ReadItem(int iter, char Delimiter, string keys)
        {
            return ReadItem(iter, Delimiter, Parser(keys));
        }

        public string ReadItem(int iter, char Delimiter, string[] keys)
        {
            string[] temp = _ListContext[iter];
            int[] key_pos = new int[keys.Length];
            string[] origin_key = _ListContext[0];
            for (int i = 0; i < key_pos.Length; i++)
            {
                int j = 0;
                for (j = 0; j < origin_key.Length; j++)
                {
                    if (origin_key[j] == keys[i]) break;
                }
                if (j >= origin_key.Length)
                {
                    key_pos[i] = -1;
                }
                else
                {
                    key_pos[i] = j;
                }
            }
            string buffer = "";
            for (int i = 0; i < keys.Length; i++)
            {
                if (key_pos[i] == -1)
                {
                    buffer += Delimiter;
                }
                else
                {
                    buffer += '"' + temp[key_pos[i]] + "\"" + Delimiter;
                }

            }
            buffer = buffer.Remove(buffer.Length - 1, 1) + "\r\n";
            return buffer;
        }
        public string ReadAll()
        {
            return ReadAll(_Delimiter);
        }

        public string ReadAll(char Delimiter)
        {
            return ReadAll(Delimiter, _ListContext[0]);
        }

        public string ReadAll(char Delimiter, string keys)
        {
            return ReadAll(Delimiter, Parser(keys));
        }
        public string ReadAll(char Delimiter, string[] keys)
        {
            string buffer = "";
            for (int i = 0; i < _ListContext.Count; i++)
            {
                buffer += ReadItem(i, Delimiter, keys);
            }
            return buffer;
        }

        public string ReadAll(char Delimiter, string[] keys, string[] keysName)
        {
            string buffer = "";
            for (int i = 0; i < keysName.Length; i++)
            {
                buffer += '"' + keysName[i] + "\"" + Delimiter;
            }
            for (int i = 1; i < _ListContext.Count; i++)
            {
                buffer += ReadItem(i, Delimiter, keys);
            }
            return buffer;
        }

        public string ReadFromTo(char Delimiter, string keys, int offset, int count)
        {
            return ReadFromTo(Delimiter, Parser(keys), offset, count);
        }

        public string ReadFromTo(char Delimiter, string[] keys, int offset, int count)
        {
            string buffer = "";
            if ((offset + count) > _ListContext.Count)
            {
                throw new Exception("(offset+count)>ListContext.Count");
            }
            for (int i = offset; i < (offset + count); i++)
            {
                buffer += ReadItem(i, Delimiter, keys);
            }
            return buffer;
        }

        public string ReadFromTo(char Delimiter, string[] keys, string[] keysName, int offset, int count)
        {
            string buffer = "";
            if ((offset + count) > _ListContext.Count)
            {
                throw new Exception("(offset+count)>ListContext.Count");
            }
            if (offset > 0)
            {
                for (int i = 0; i < keysName.Length; i++)
                {
                    buffer += '"' + keysName[i] + "\"" + Delimiter;
                }
            }
            for (int i = offset; i < (offset + count); i++)
            {
                buffer += ReadItem(i, Delimiter, keys);
            }
            return buffer;
        }

        public string[] Parser(string str)
        {
            return Parser(str, _Delimiter);
        }
        static public string[] Parser(string str, char Delimiter)
        {
            bool inItem = false;
            List<string> listStr = new List<string>();
            string buffer = "";
            for (int i = 0; i < str.Length; i++)
            {

                if (inItem)
                {
                    if (str[i] == '"')
                    {
                        inItem = false;
                    }
                    buffer += str[i];
                }
                else
                {
                    if (str[i] == '"')
                    {
                        inItem = true;
                    }
                    if (str[i] == Delimiter)
                    {
                        buffer = removeQutaMark(buffer);
                        listStr.Add(buffer);
                        buffer = "";
                    }
                    else
                    {
                        buffer += str[i];
                    }
                }
            }
            buffer = removeQutaMark(buffer);
            listStr.Add(buffer);

            return listStr.ToArray();
        }
        static private string removeQutaMark(string buffer)
        {
            if (buffer.Length > 0)
            {
                if (buffer[0] == '"')
                {
                    buffer = buffer.Remove(0, 1);
                }
                if (buffer[buffer.Length - 1] == '"')
                {
                    buffer = buffer.Remove(buffer.Length - 1, 1);
                }
            }
            return buffer;
        }
    }
}
