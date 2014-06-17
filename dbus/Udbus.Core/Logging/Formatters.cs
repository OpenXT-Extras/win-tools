//
// Copyright (c) 2012 Citrix Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Udbus.Core.Logging
{
    public class Formatter
    {
        internal class StringHolder
        {
            internal string data;
            internal StringHolder(string data) { this.data = data; }
            internal string GetData() { return this.data; }

        } // Ends class StringHolder

        public delegate string StashDelegate();
        private Dictionary<string, StashDelegate> stash = new Dictionary<string, StashDelegate>();
        private string formatString;
        private int cachedMaxIndex = 0;
        private LinkedList<StashDelegate> cachedValueDelegates = null;
        private bool gotCachedMessageField = false;
        private string cachedFormatString;

        public string FormatString
        {
            get
            {
                return this.formatString;
            }
            set
            {
                this.formatString = value;
                this.InitFormatString();
            }
        }

        private static void InitStash(Dictionary<string, StashDelegate> stash)
        {
            stash.Add(TraceFormatFieldDelegates.Fields.DateTime, TraceFormatFieldDelegates.DateTime);
            stash.Add(TraceFormatFieldDelegates.Fields.Timestamp, TraceFormatFieldDelegates.Timestamp);
            stash.Add(TraceFormatFieldDelegates.Fields.ProcessId, TraceFormatFieldDelegates.ProcessId);
            stash.Add(TraceFormatFieldDelegates.Fields.ThreadId, TraceFormatFieldDelegates.ThreadId);
            stash.Add(TraceFormatFieldDelegates.Fields.Callstack, TraceFormatFieldDelegates.Callstack);
        }

        private static IEnumerable<KeyValuePair<string, StashDelegate>> GenMessageStash(StringHolder messageHolder)
        {
            yield return new KeyValuePair<string, StashDelegate>(TraceFormatFieldDelegates.Fields.Message, messageHolder.GetData);
        }
        private static IEnumerable<KeyValuePair<string, StashDelegate>> StashWithMessage(IEnumerable<KeyValuePair<string, StashDelegate>> stash, StringHolder messageHolder)
        {
            return stash.Concat(GenMessageStash(messageHolder));
        }
        private static IEnumerable<StashDelegate> GenMessageDelegate(StringHolder messageHolder)
        {
            yield return messageHolder.GetData;
        }

        private static IEnumerable<object> ObjectsFromStrings(IEnumerable<string> e)
        {
            foreach (string s in e)
            {
                yield return s;
            }
        }

        static private bool TryNumerals(string parse, out string result)
        {
            int pos = 0;
            int parseResult;
            result = parse;
            bool canParse = int.TryParse(parse[pos].ToString(), out parseResult);
            if (canParse)
            {
                ++pos;
                while (pos < parse.Length && int.TryParse(parse[pos].ToString(), out parseResult))
                {
                    ++pos;
                }

                if (pos != parse.Length)
                {
                    result = parse.Substring(0, pos);
                }
            }

            return canParse;
        }

        static private bool AllNumerals(string parse)
        {
            string numerals;
            bool isNumerals = TryNumerals(parse, out numerals);
            return isNumerals && numerals.Length == parse.Length;
        }

        static private IEnumerable<int> GenFormatIndex(string format)
        {
            if (!string.IsNullOrEmpty(format)) // If got a format string
            {
                string[] formatTokens = format.Split('{');
                if (formatTokens.Length > 0) // If got format tokens
                {
                    bool bSkip = false;
                    IEnumerable<string> en = formatTokens;
                    IEnumerator<string> et = en.GetEnumerator();
                    bool move = et.MoveNext();
                    if (move)
                    {
                        // Skip the first entry leading up to the first brace.
                        move = et.MoveNext();
                    }

                    for (; move; move = et.MoveNext())
                    {
                        string formatToken = et.Current;
                        if (formatToken.Length == 0) // If just a {
                        {
                            bSkip = !bSkip;
                        }
                        else // Else not just a {
                        {
                            if (bSkip) // If previous said skip
                            {
                                bSkip = false;

                            }
                            else // Else previous didn't say skip
                            {
                                string numerals;
                                if (TryNumerals(formatToken, out numerals)) // If got numerals
                                {
                                    int index = int.Parse(numerals);
                                    yield return index;

                                } // Ends if got numerals
                            } // Ends else previous didn't say skip
                        } // Ends not just a {
                    } // Ends loop over format tokens
                } // Ends if got format tokens
            } // Ends if got a format string
        }

        static private bool GetMaxFormatIndex(string format, out int maxIndex)
        {
            maxIndex = 0;
            bool gotIndex = false;

            foreach (int index in GenFormatIndex(format))
            {
                gotIndex = true;
                if (index > maxIndex)
                {
                    maxIndex = index;
                }
            }
            return gotIndex;
        }

        static private bool GotFormatIndex(string format)
        {
            IEnumerable<int> indices = GenFormatIndex(format);
            return indices.GetEnumerator().MoveNext();
        }

        private static bool SkipDoubleBracePost(ref int posLookup, string format)
        {
            bool skip = posLookup + 1 != format.Length && format[posLookup + 1] == '{';
            if (skip) // If double brace
            {
                posLookup += 2;

            } // Ends if double brace
            return skip;
        }

        private static bool SkipDoubleBracePre(ref int posLookup, string format)
        {
            bool skip = posLookup > 0 && format.Length > posLookup - 1 && format[posLookup - 1] == '{';
            if (skip) // If double brace
            {
                posLookup += 1;

            } // Ends if double brace
            return skip;
        }

        private static bool GetFormatStringAndValues(IEnumerable<KeyValuePair<string, StashDelegate>> stash, bool gotStash, ref string format, out bool gotMaxIndex, ref int maxIndex, out LinkedList<StashDelegate> values)
        {
            gotMaxIndex = false;
            values = null;
            int posLookup = -1;

            if (format != null && gotStash) // If got a format string and a stash
            {
                posLookup = format.IndexOf('{');

            } // Ends if got a format string and a stash

            if (posLookup != -1) // If got brace
            {
                int nextFormatIndex = maxIndex;
                gotMaxIndex = GetMaxFormatIndex(format, out maxIndex);
                if (gotMaxIndex)
                {
                    nextFormatIndex = maxIndex + 1;
                }
                values = new LinkedList<StashDelegate>();
                bool skip = SkipDoubleBracePost(ref posLookup, format);
                while (posLookup != -1 && skip)
                {
                    skip = SkipDoubleBracePost(ref posLookup, format);

                } // Ends loop skipping double braces

                // Make a note of the non-bracey prefix.
                string firstPart = format.Substring(0, posLookup);
                format = format.Substring(posLookup);

                foreach (KeyValuePair<string, StashDelegate> stashEntry in stash)
                {
                    string lookup = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{{{0}}}", stashEntry.Key);
                    posLookup = format.IndexOf(lookup);
                    StringBuilder fragments = new StringBuilder();

                    bool bSeenLookup = false;
                    while (posLookup != -1)
                    {
                        if (!SkipDoubleBracePre(ref posLookup, format)) // If not double brace
                        {
                            // Add fragment replacing lookup format with index.
                            fragments.Append(format.Substring(0, posLookup + 1));
                            fragments.Append(nextFormatIndex.ToString());
                            bSeenLookup = true;

                            int posCloseBrace = format.IndexOf('}', posLookup + 1);
                            if (posCloseBrace != -1) // If got a close brace
                            {
                                fragments.Append('}');
                                format = format.Substring(posCloseBrace + 1);
                            }
                        } // Ends if not double brace
                        else // Else double brace
                        {
                            // Just put the string in and move on.
                            fragments.Append(format.Substring(0, posLookup + lookup.Length));
                            format = format.Substring(posLookup + lookup.Length);

                        } // Ends else double brace

                        posLookup = format.IndexOf(lookup);

                    } // Ends loop over double braces

                    if (bSeenLookup)
                    {
                        ++nextFormatIndex;
                        ++maxIndex;
                        values.AddLast(stashEntry.Value);
                        fragments.Append(format);

                    } // Ends if at least one entry in the format string

                    if (fragments.Length > 0) // If got fragments
                    {
                        format = fragments.ToString();

                    } // Ends if got fragments
                } // Ends loop over stash

                // The the non-bracey prefix back.
                format = firstPart + format;
            }

            return values != null && values.Count > 0;
        }

        public static string GetFormattedImpl(IEnumerable<KeyValuePair<string, StashDelegate>> stash, bool gotStash, int maxIndex, string format, params object[] args)
        {
            bool gotMaxIndex;
            LinkedList<StashDelegate> valueDelegates;
            //IEnumerable<KeyValuePair<string, StashDelegate>> eStash = stash;
            //StringHolder shFormat = new StringHolder(format);
            //eStash = eStash.Concat(new KeyValuePair<string, StashDelegate>[] { new KeyValuePair<string, StashDelegate>(TraceFormatFieldDelegates.Fields.Message, shFormat.GetData) });
            if (GetFormatStringAndValues(stash, gotStash, ref format, out gotMaxIndex, ref maxIndex, out valueDelegates)) // If adjusted format string
            {
                args = BuildArgs(args, gotMaxIndex, valueDelegates);

            } // Ends if adjusted format string

            string result;
            if (gotMaxIndex)
            {
                result = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, args);
            }
            else
            {
                if (format == null)
                {
                    result = null;
                }
                else
                {
                    result = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, args);
                }
                //result = format;
            }

            return result;
        }

        private static object[] BuildArgs(object[] args, bool gotMaxIndex, LinkedList<StashDelegate> valueDelegates)
        {
            if (valueDelegates.Count > 0) // If got values
            {
                object[] values = valueDelegates.Select(d => d()).ToArray();
                object[] newargs = new object[args.Length + values.Length];
                int oldArgsIndex, newArgsIndex;
                if (gotMaxIndex) // If there's a numeric index in format string
                {
                    // Old args, then new args.
                    oldArgsIndex = 0;
                    newArgsIndex = args.Length;

                } // Ends if there's a numeric index in format string
                else // Else no numeric index in format string
                {
                    // New args, then old args.
                    newArgsIndex = 0;
                    oldArgsIndex = values.Length;

                } // Ends else no numeric index in format string
                args.CopyTo(newargs, oldArgsIndex);

                foreach (string valuesIter in values)
                {
                    newargs[newArgsIndex] = valuesIter;
                    ++newArgsIndex;
                }
                args = newargs;

            } // Ends if got values
            return args;
        }

        private static bool InitFormatString(IEnumerable<KeyValuePair<string, StashDelegate>> stash, bool gotStash, ref string initFormatString, ref int maxIndex, out LinkedList<StashDelegate> values, out bool gotCachedMessageField)
        {
            string format = initFormatString;
            bool gotMaxIndex;
            bool adjusted = GetFormatStringAndValues(stash, gotStash, ref format, out gotMaxIndex, ref maxIndex, out values);
            LinkedList<StashDelegate> messageDelegateDummy;
            int cachedValueDelegatesCount = values != null ? values.Count : 0;

            int maxMessageIndex = maxIndex + cachedValueDelegatesCount;
            gotCachedMessageField = GetFormatStringAndValues(GenMessageStash(new StringHolder(string.Empty)), true, ref format, out gotMaxIndex, ref maxMessageIndex, out messageDelegateDummy);
            if (adjusted || gotCachedMessageField)
            {
                initFormatString = format;
            }

            return adjusted;
        }

        private void AddToStash(IEnumerable<KeyValuePair<string, StashDelegate>> e)
        {
            foreach (KeyValuePair<string, StashDelegate> pair in e)
            {
                this.AddPlaceholder(pair.Key, pair.Value);
            }
        }

        private void AddToStash(IEnumerable<KeyValuePair<string, string>> e)
        {
            foreach (KeyValuePair<string, string> pair in e)
            {
                this.AddPlaceholder(pair.Key, pair.Value);
            }
        }

        private void AddToStash<T>(IEnumerable<KeyValuePair<string, T>> e)
        {
            foreach (KeyValuePair<string, T> pair in e)
            {
                this.AddPlaceholder(pair.Key, pair.Value);
            }
        }

        private bool InitFormatString()
        {
            this.cachedFormatString = this.formatString;
            return InitFormatString(this.stash, this.stash.Count != 0, ref this.cachedFormatString, ref this.cachedMaxIndex, out this.cachedValueDelegates, out this.gotCachedMessageField);
        }

        public Formatter()
        {
            InitStash(this.stash);
        }

        private Formatter(string formatString, bool bInitFormatString)
            : this()
        {
            if (bInitFormatString)
            {
                this.FormatString = formatString;
            }
            else
            {
                this.formatString = formatString;
            }
        }

        public Formatter(string formatString)
            : this(formatString, true)
        {
        }

        public Formatter(string formatString, IEnumerable<KeyValuePair<string, StashDelegate>> e)
            : this(formatString, false)
        {
            AddToStash(e);
            InitFormatString();
        }

        public Formatter(IEnumerable<KeyValuePair<string, StashDelegate>> e)
            : this()
        {
            AddToStash(e);
        }

        public Formatter(string formatString, IEnumerable<KeyValuePair<string, string>> e)
            : this(formatString, false)
        {
            AddToStash(e);
            InitFormatString();
        }

        public Formatter(IEnumerable<KeyValuePair<string, string>> e)
            : this()
        {
            AddToStash(e);
        }

        public static Formatter Create<T>(string formatString, IEnumerable<KeyValuePair<string, T>> e)
        {
            Formatter f = new Formatter(formatString, false);
            f.AddToStash(e);
            f.InitFormatString();
            return f;
        }

        public static Formatter Create<T>(IEnumerable<KeyValuePair<string, T>> e)
        {
            Formatter f = new Formatter();
            f.AddToStash(e);
            return f;
        }

        public void AddPlaceholder(string name, StashDelegate value)
        {
            if (AllNumerals(name))
            {
                throw Exceptions.FormatterStashKeyException.CreateNumeric(name);
            }
            this.stash[name] = value;
        }

        public void AddPlaceholder(string name, string value)
        {
            this.AddPlaceholder(name, () => value);
        }

        public void AddPlaceholder<T>(string name, T value)
        {
            this.AddPlaceholder(name, () => value.ToString());
        }

        public void ClearStash()
        {
            this.stash.Clear();
            this.cachedValueDelegates.Clear();
            if (this.gotCachedMessageField)
            {
                this.cachedMaxIndex = 1;
            }
            else
            {
                this.cachedMaxIndex = 0;
            }
        }

        public virtual string GetFormattedString(string format, params object[] args)
        {
            // First format the format string.
            string formatted = GetFormattedImpl(this.stash, this.stash.Count != 0, 0, format, args);

            if (!string.IsNullOrEmpty(this.formatString)) // If there is a format string
            {
                if ((this.cachedValueDelegates != null && this.cachedValueDelegates.Count > 0) || this.gotCachedMessageField) // If got stashed fields
                {
                    string formattingString = this.cachedFormatString;
                    int maxFormattingIndex;
                    if (GetMaxFormatIndex(format, out maxFormattingIndex)) // If numeric indices in format
                    {
                        formattingString = this.formatString;  // Reset to original string.
                        ++maxFormattingIndex;
                        if (args.Length > maxFormattingIndex) // If there are more arguments than indices in the format string
                        {
                            // Just bump above number of args passed.
                            maxFormattingIndex = args.Length;

                        } // Ends if there are more arguments than indices in the format string
                        LinkedList<StashDelegate> values;
                        bool gotMessageField;
                        InitFormatString(this.stash, this.stash.Count != 0, ref formattingString, ref maxFormattingIndex, out values, out gotMessageField);

                        //formattingString = ;
                    } // Ends if numeric indices in format
                    // The internal format string is preparsed and the delegates are ready, apart from the formatted message.
                    StringHolder shFormat = new StringHolder(formatted);
                    IEnumerable<StashDelegate> cachedDelegates = this.cachedValueDelegates;
                    if (this.gotCachedMessageField)
                    {
                        // Chuck in the message.
                        cachedDelegates = cachedDelegates.Concat(GenMessageDelegate(shFormat));
                    }
                    else if (this.cachedMaxIndex > 0) // Else if adding cached fields
                    {
                        // TODO - handle passing args through to format string or internal format string.


                    } // Ends else if adding cached fields

                    // If we've got stashed values, going to have to put them at the end of the args.
                    IEnumerable<string> cachedStrings = cachedDelegates.Select(d => d());
                    IEnumerable<object> cached = ObjectsFromStrings(cachedStrings);
                    cached = args.Concat(cached);
                    object[] cachedValues = cached.ToArray();

                    formatted = string.Format(System.Globalization.CultureInfo.InvariantCulture, formattingString, cachedValues);

                } // Ends if got stashed fields
                else // Else no stashed fields
                {
                    IEnumerable<object> eArgs = args;
                    if (format != null) // If got a format string
                    {
                        // Not sure about this. Pass in the result of formatting the string as the first argument ?
                        eArgs = new object[] { formatted }.Concat(eArgs);
                    }

                    // Just use the internal format string. Sorry about that.
                    formatted = string.Format(System.Globalization.CultureInfo.InvariantCulture, this.FormatString, eArgs.ToArray());

                } // Ends else no stashed fields

            } // Ends if there is a format string

            return formatted;
        }

        public virtual string GetFormattedString(IEnumerable<KeyValuePair<string, StashDelegate>> extraStash, string format, params object[] args)
        {
            IEnumerable<KeyValuePair<string, StashDelegate>> formatStash = this.stash.Concat(extraStash);
            // First format the format string.
            string formatted = GetFormattedImpl(formatStash, true, 0, format, args);

            if (!string.IsNullOrEmpty(this.formatString)) // If there is a format string
            {
                // The internal format string is preparsed and the delegates are ready, apart from the formatted message.
                StringHolder shFormat = new StringHolder(formatted);
                IEnumerable<StashDelegate> cachedDelegates = this.cachedValueDelegates;
                if (this.gotCachedMessageField)
                {
                    // Chuck in the message.
                    cachedDelegates = cachedDelegates.Concat(GenMessageDelegate(shFormat));
                }
                else if (this.cachedMaxIndex > 0) // Else if adding cached fields
                {
                    // TODO - handle passing args through to format string or internal format string.

                } // Ends else if adding cached fields

                if (extraStash != null || (this.cachedValueDelegates != null && this.cachedValueDelegates.Count > 0) || this.gotCachedMessageField) // If got stashed fields
                {
                    string formattingString = this.cachedFormatString;
                    int maxFormattingIndex = 0;
                    bool bExtraFieldsToFormat = GetMaxFormatIndex(format, out maxFormattingIndex);
                    if (bExtraFieldsToFormat)
                    {
                        ++maxFormattingIndex;
                    }

                    if (bExtraFieldsToFormat || extraStash != null) // If extra fields to format or numeric indices in format
                    {
                        // Need to reparse the internal format string.
                        formattingString = this.formatString;  // Reset to original string.
                        if (args.Length > maxFormattingIndex) // If there are more arguments than indices in the format string
                        {
                            // Just bump above number of args passed.
                            maxFormattingIndex = args.Length;

                        } // Ends if there are more arguments than indices in the format string

                        LinkedList<StashDelegate> valueDelegates;
                        bool gotMessageField;
                        if (InitFormatString(formatStash, true, ref formattingString, ref maxFormattingIndex, out valueDelegates, out gotMessageField))
                        {
                            cachedDelegates = valueDelegates;
                            if (gotMessageField)
                            {
                                // Chuck in the message.
                                cachedDelegates = cachedDelegates.Concat(GenMessageDelegate(shFormat));
                            }
                        }

                    } // Ends if extra fields to format or numeric indices in format

                    object[] cachedValues = args;

                    if (cachedDelegates != null) // If got delegate fields
                    {
                        // If we've got stashed values, going to have to put them at the end of the args.
                        IEnumerable<string> cachedStrings = cachedDelegates.Select(d => d());
                        IEnumerable<object> cached = ObjectsFromStrings(cachedStrings);
                        cached = args.Concat(cached);
                        cachedValues = cached.ToArray();

                    } // Ends if got delegate fields

                    formatted = string.Format(System.Globalization.CultureInfo.InvariantCulture, formattingString, cachedValues);

                } // Ends if got stashed fields
                else // Else no stashed fields
                {
                    IEnumerable<object> eArgs = args;
                    if (format != null) // If got a format string
                    {
                        // Not sure about this. Pass in the result of formatting the string as the first argument ?
                        eArgs = new object[] { formatted }.Concat(eArgs);
                    }

                    // Just use the internal format string. Sorry about that.
                    formatted = string.Format(System.Globalization.CultureInfo.InvariantCulture, this.FormatString, eArgs.ToArray());

                } // Ends else no stashed fields

            } // Ends if there is a format string

            return formatted;
        }

        public string Format(IEnumerable<KeyValuePair<string, Formatter.StashDelegate>> stashEntries, string message, params object[] args)
        {
            return this.GetFormattedString(stashEntries, message, args);
        }

        public string Format(string format, params object[] args)
        {
            return this.GetFormattedString(format, args);
        }

        //public string Format(params object[] args)
        //{
        //    return this.GetFormattedString(this.FormatString, args);
        //}

    } // Ends class Formatter
}
