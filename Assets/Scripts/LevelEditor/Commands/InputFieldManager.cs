using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using System.Linq;
using System;

namespace HeavenStudio.Editor
{
    public class InputFieldManager
    {
        private TMP_InputField inputField;
        private MonoBehaviour runner; //for implementing ctrlz later
        private string logicalText;
        private int selectionStart;
        private int selectionEnd;
        private static readonly Regex regexRemoveTagPattern = new Regex(@"<[^>]*>");
        private static readonly Regex regexTagPattern = new Regex(@"<(/?)([^=\s>]+)([^>]*)>");
        private bool isRichTextEnabled = true;

        private List<TagInterval> intervalList = new List<TagInterval>();
        private List<Tag> preexistingUnmanagedTags = new List<Tag>();
        private Dictionary<int, int> microPosCountersDict = new Dictionary<int, int>(); //(K: log Pos, V: microindex)
        private Dictionary<string, int> managedTagMicroPosDict = new Dictionary<string, int>();
        private HashSet<string> tagTypeBasedStrictTags = new HashSet<string> { "font", "outlineFont" }; //position and tagType strict
        private HashSet<string> contentBasedStrictTags = new HashSet<string> { "size", "color" }; //position and tagType and exact content strict
        private HashSet<string> managedTags = new HashSet<string> { "b", "i", "u", "font", "size", "color" };
        private HashSet<string> strictMatchingSet = new HashSet<string> { "font", "outlineFont", "size", "color" }; //HashSet for O(1) lookup
        private Dictionary<string, string> endTagDict = new Dictionary<string, string>//for autoclosing managed-tag variations
        {
            {"outlineFont", "</font>"}
        };
        private Dictionary<string, HashSet<string>> incompatibilityDict = new Dictionary<string, HashSet<string>>
        {
            {"font", new HashSet<string> {"font", "outlineFont"}},
            {"outlineFont", new HashSet<string> {"font", "outlineFont"}},
            {"size", new HashSet<string> {"size"}},
            {"color", new HashSet<string> {"color"}}
        };

        public InputFieldManager(TMP_InputField field, MonoBehaviour runner)
        {
            inputField = field;
            this.runner = runner;
            
            Init();
        }
        
        private void Init()
        {
            inputField.richText = isRichTextEnabled;
            ParseUserTextToIntervals();
        }

        #region Initial Parsing

        public void ParseUserTextToIntervals()
        {   
            intervalList.Clear();
            preexistingUnmanagedTags.Clear();
            microPosCountersDict.Clear();

            string textWithoutUnmanaged = ExtractUnmanagedTags(inputField.text);
            logicalText = regexRemoveTagPattern.Replace(textWithoutUnmanaged, "");

            var managedMatches = regexTagPattern.Matches(textWithoutUnmanaged);
            CreateIntervals(managedMatches);
        }

        private void CreateIntervals(MatchCollection matches)
        {
            var openTags = new Dictionary<string, Stack<(int position, string openTag, int microPos)>>();         
            int lastTagLogicalIndex = 0;
            int lastMatchEnd = 0;

            foreach (Match match in matches)
            {
                lastTagLogicalIndex += (match.Index - lastMatchEnd); //base logical index off of chars before it
                
                bool isClosingTag = match.Groups[1].Value == "/"; // (/?)
                string tagName = match.Groups[2].Value.ToLower(); // ([^=\s>]+)
                string tagAttributes = match.Groups[3].Value; // ([^>]*)
                
                // Handle font tag variations
                if (tagName == "font")
                {
                    if (isClosingTag) { tagName = DetermineClosingFontTag(openTags); }
                    else { tagName = tagAttributes.Contains("material=") ? "outlineFont" : "font"; }
                }

                int microPos = managedTagMicroPosDict.ContainsKey(match.Value) ? managedTagMicroPosDict[match.Value] : GetNextMicroPosition(lastTagLogicalIndex);
                
                //basically that one match-parenthesis stack problem
                if (isClosingTag)
                {
                    if (StackElementExists(openTags, tagName))
                    {
                        var (openTagPos, fullOpenTag, openMicroPos) = openTags[tagName].Pop();
                        intervalList.Add(new TagInterval(
                            openTagPos, 
                            lastTagLogicalIndex, 
                            tagName, 
                            fullOpenTag, 
                            match.Value,
                            true, //its parsing; everything rn is preexisting
                            openMicroPos,
                            microPos
                        ));
                    }
                }
                else
                {
                    if (!openTags.ContainsKey(tagName))
                    {
                        openTags[tagName] = new Stack<(int, string, int)>();
                    }
                    openTags[tagName].Push((lastTagLogicalIndex, match.Value, microPos));
                }
                lastMatchEnd = match.Index + match.Length;
            }

            AddMissingClosingTags(openTags);
        }

        private int GetNextMicroPosition(int logicalPos)
        {
            if (!microPosCountersDict.ContainsKey(logicalPos))
            {
                microPosCountersDict[logicalPos] = 0;
            }
            return microPosCountersDict[logicalPos]++;
        }

        private void AddMissingClosingTags(Dictionary<string, Stack<(int position, string openTag, int microPos)>> openTags)
        {
            foreach (var openTag in openTags)
            {
                while (openTag.Value.Count > 0)
                {
                    var (openTagPos, fullOpenTag, openMicroPos) = openTag.Value.Pop();
                    string newCloseTag = $"</{openTag.Key}>";
                    
                    if (endTagDict.ContainsKey(openTag.Key))
                    {
                        newCloseTag = endTagDict[openTag.Key];
                    }
                    
                    intervalList.Add(new TagInterval(
                        openTagPos, 
                        logicalText.Length, 
                        openTag.Key, 
                        fullOpenTag, 
                        newCloseTag,
                        true,
                        openMicroPos,
                        GetNextMicroPosition(logicalText.Length)
                    ));
                    //Debug.Log($"Added unclosed tag: {fullOpenTag} at position {openTagPos} to {logicalText.Length}");
                }
            }
        }

        //since this one actually works with the raw text since its parsing and not text selection
        private string ExtractUnmanagedTags(string inputText)
        {
            var allTagMatches = regexTagPattern.Matches(inputField.text);
            var result = inputText;
            int rawOffset = 0;
            
            foreach (Match match in allTagMatches)
            {
                string tagName = match.Groups[2].Value.ToLower(); //([^=\s>]+)
                string textBeforeTag = inputText.Substring(0, match.Index);
                int logicalPos = regexRemoveTagPattern.Replace(textBeforeTag, "").Length;
                
                if (!managedTags.Contains(tagName))
                {
                    int microPosition = GetNextMicroPosition(logicalPos);
                    
                    preexistingUnmanagedTags.Add(new Tag(
                        logicalPos, 
                        !match.Groups[1].Value.Equals("/"), //isOpeningTag
                        match.Value, 
                        "unmanaged",
                        true, //isPreexisting
                        microPosition
                    ));
                    
                    //remove from text
                    int adjustedIndex = match.Index - rawOffset;
                    result = result.Substring(0, adjustedIndex) + result.Substring(adjustedIndex + match.Length);
                    rawOffset += match.Length;
                }
                else //Keep managed tags for the interval comparisons and new tag insertions l8r
                {
                    int reservedMicroPos = GetNextMicroPosition(logicalPos);
                    GetNextMicroPosition(logicalPos);
                    managedTagMicroPosDict[match.Value] = reservedMicroPos;
                }
            }
            return result;//removed unmanaged tags
        }

        private bool StackElementExists(Dictionary<string, Stack<(int, string, int)>> openTags, string tagName)
        {
            return openTags.ContainsKey(tagName) && openTags[tagName].Count > 0;
        }

        private string DetermineClosingFontTag(Dictionary<string, Stack<(int, string, int)>> openTags)
        {
            int? regFontTagPos = null;
            int? outlineFontTagPos = null;
            
            if (StackElementExists(openTags, "font")) 
            { 
                regFontTagPos = openTags["font"].Peek().Item1;
            }
            if (StackElementExists(openTags, "outlineFont")) 
            {
                outlineFontTagPos = openTags["outlineFont"].Peek().Item1; 
            }
            
            if (regFontTagPos.HasValue && outlineFontTagPos.HasValue)
            {
                return (regFontTagPos > outlineFontTagPos) ? "font" : "outlineFont"; //whichever came first
            }
            else if (regFontTagPos.HasValue) return "font";
            else if (outlineFontTagPos.HasValue) return "outlineFont";
            
            return "font"; //fallback
        }

        #endregion

        #region Main

        //todo?: bite the bullet on redundancy and make font/standard text operations completely separate?
        public void DetermineTextOperation(string tagType, string openingTag, string closingTag)
        {
            ParseUserTextToIntervals();

            var logicalSelectionRange = GetLogicalTextSelectionRange();
            selectionStart = logicalSelectionRange.start;
            selectionEnd = logicalSelectionRange.end;

            if (selectionStart == selectionEnd)
            {
                // Nothing selected, so everything selected. Make sense, bub?
                selectionStart = 0;
                selectionEnd = logicalText.Length;
            }

            bool isStrictMatchingTag = strictMatchingSet.Contains(tagType);

            if (isStrictMatchingTag) { HandleStrictOperation(tagType, openingTag, closingTag); } //one-or-the-other tags
            else { HandleStandardOperation(tagType, openingTag, closingTag); } //can coexist tags

            RebuildRawText();
        }

        private void HandleStrictOperation(string tagType, string openingTag, string closingTag)
        {
            //Debug.Log($"Handling font operation: {tagType}");
            var selectionInterval = new TagInterval(
                selectionStart, 
                selectionEnd, 
                tagType, 
                openingTag, 
                closingTag,
                false,
                GetNextMicroPosition(selectionStart),
                GetNextMicroPosition(selectionEnd)
            );
            
            var incompatibleTagTypes = incompatibilityDict.ContainsKey(tagType) ? incompatibilityDict[tagType] : new HashSet<string> { tagType };
            
            var conflictingIntervals = intervalList.Where(interval => 
                incompatibleTagTypes.Contains(interval.tagType) && 
                interval.DoesOverlap(selectionInterval)
            ).ToList();

            var exactMatch = conflictingIntervals.FirstOrDefault(interval => 
                interval.tagType == tagType &&
                interval.intervalStart == selectionStart && 
                interval.intervalEnd == selectionEnd &&
                (tagTypeBasedStrictTags.Contains(tagType) || (contentBasedStrictTags.Contains(tagType) && interval.openTag == openingTag && interval.closeTag == closingTag))
            );
            
            if (exactMatch != null) //selection is same as other tagged interval
            {
                RemoveExactMatch(exactMatch);
            }
            else if (conflictingIntervals.Any())
            {
                foreach (var interval in conflictingIntervals)
                {
                    RemoveFromSelectionInterval(interval);//kil on any overlapping
                }
                AddNewInterval(selectionInterval, true);
            }
            else
            {
                AddNewInterval(selectionInterval, true);
            }
        }

        private void HandleStandardOperation(string tagType, string openingTag, string closingTag)
        {
            //Debug.Log($"Handling standard operation: {tagType}");
            var relevantIntervals = intervalList.Where(interval => interval.tagType == tagType).ToList();

            //base intervals
            var selectionInterval = new TagInterval(
                selectionStart, 
                selectionEnd, 
                tagType, 
                openingTag, 
                closingTag,
                false, // isPreexistingInterval - this is a new user operation  
                GetNextMicroPosition(selectionStart), // openMicroPos
                GetNextMicroPosition(selectionEnd) // closeMicroPos
            );
            var overlappingIntervals = relevantIntervals.Where(interval => interval.DoesOverlap(selectionInterval)).ToList();

            //check intervals
            var exactMatch = overlappingIntervals.FirstOrDefault(interval => selectionInterval == interval);
            var intervalContains = overlappingIntervals.FirstOrDefault(interval => interval.DoesContain(selectionInterval));

            if (!overlappingIntervals.Any()) //add interval to mega list; tag is applied to unaffected text
            {
                //tag merging happens here too
                AddNewInterval(selectionInterval, false);
            }
            else if (exactMatch != null) //selection is same as other tagged interval
            {
                RemoveExactMatch(exactMatch);
            }
            else if (intervalContains != null) // selection is within another interval(s)
            {
                RemoveFromSelectionInterval(intervalContains);
            }
            else //must be partial overlaps; extend/merge
            {
                ExtendAndMergeIntervals(overlappingIntervals, selectionInterval);
            }
        }

        #endregion

        //For anyone who wants to mess with this file, know that apparently there is a logic disjoint between what the user sees and what Unity sees
        //In a bolded "Hello", Unity sees "<b>Hello</b> World!", the user sees "Hello World!" where Hello is bolded
        //Highlighting "Hello" (5 positions) from the user standpoint means Unity sees "<b>He" (5 positions)
        //But its still getting the correct logical range anyway so we just roll with it and clamp just in case (task failed successfully lol)
        private (int start, int end) GetLogicalTextSelectionRange()
        {
            int logicalStart = Mathf.Min(inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
            int logicalEnd = Mathf.Max(inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
            
            logicalStart = Mathf.Clamp(logicalStart, 0, logicalText.Length);
            logicalEnd = Mathf.Clamp(logicalEnd, 0, logicalText.Length);
            
            return (logicalStart, logicalEnd);
        }

        #region Text Operations

        private void AddNewInterval(TagInterval selectionInterval, bool strictMatchingOn)//also handles merging tags if same type tags are next to each other
        {
            //Debug.Log($"Adding Interval: {selectionInterval.ToString()}");
            List<TagInterval> sameTypeIntervals;
            var toMerge = new List<TagInterval>();

            if (strictMatchingOn && (strictMatchingSet.Contains(selectionInterval.tagType)))
            {
                sameTypeIntervals = intervalList.Where(interval => 
                    interval.tagType == selectionInterval.tagType &&
                    interval.openTag == selectionInterval.openTag &&
                    interval.closeTag == selectionInterval.closeTag
                ).ToList(); //same tagType AND tags
            }
            else
            {
                sameTypeIntervals = intervalList.Where(interval => interval.tagType == selectionInterval.tagType).ToList();//just same tagtype
            }
            
            foreach (var interval in sameTypeIntervals)
            {
                if (AreIntervalsAdjacent(selectionInterval, interval) || selectionInterval.DoesOverlap(interval))//overlapping for when users edit the rich text themselves
                {
                    toMerge.Add(interval);
                }
            }
            
            if (toMerge.Any()) //merge if found adjacent/overlapping
            {
                foreach (var interval in toMerge)
                {
                    intervalList.Remove(interval);
                }
                
                //new single interval with merged tags
                int mergedStart = Math.Min(selectionInterval.intervalStart, toMerge.Min(i => i.intervalStart));
                int mergedEnd = Math.Max(selectionInterval.intervalEnd, toMerge.Max(i => i.intervalEnd));
                
                var mergedInterval = new TagInterval(
                    mergedStart,
                    mergedEnd,
                    selectionInterval.tagType,
                    selectionInterval.openTag,
                    selectionInterval.closeTag,
                    false,
                    GetNextMicroPosition(mergedStart),
                    GetNextMicroPosition(mergedEnd)
                );
                
                //Debug.Log($"Merged interval: {mergedInterval.ToString()}");
                intervalList.Add(mergedInterval);
            }
            else//or just add it as is
            {
                intervalList.Add(selectionInterval);
            }
        }

        private void RemoveExactMatch(TagInterval exactMatch)
        {
            //Debug.Log($"Removing exact match: {exactMatch.ToString()}");
            intervalList.RemoveAll(interval => interval == exactMatch);
        }

        private void RemoveFromSelectionInterval(TagInterval intervalContains)
        {
           // Debug.Log($"Removing from containing area: {intervalContains.ToString()}");
            intervalList.Remove(intervalContains);

            if (intervalContains.intervalStart < selectionStart)
            {
                intervalList.Add(new TagInterval(
                    intervalContains.intervalStart, 
                    selectionStart, 
                    intervalContains.tagType, 
                    intervalContains.openTag, 
                    intervalContains.closeTag,
                    false,
                    GetNextMicroPosition(intervalContains.intervalStart),
                    GetNextMicroPosition(selectionStart)
                ));
            }
            
            if (selectionEnd < intervalContains.intervalEnd)
            {
                intervalList.Add(new TagInterval(
                    selectionEnd, 
                    intervalContains.intervalEnd, 
                    intervalContains.tagType, 
                    intervalContains.openTag, 
                    intervalContains.closeTag,
                    false,
                    GetNextMicroPosition(intervalContains.intervalStart),
                    GetNextMicroPosition(selectionStart)
                ));
            }
        }

        private void ExtendAndMergeIntervals(List<TagInterval> overlappingIntervals, TagInterval selectionInterval)
        {
            //Debug.Log($"Extending/merging intervals: {selectionInterval.ToString()}");
            foreach (var interval in overlappingIntervals)
            {
                intervalList.Remove(interval);
            }

            //new extended/merged interval
            int newStart = Math.Min(
                selectionInterval.intervalStart, overlappingIntervals.Any() ? 
                overlappingIntervals.Min(i => i.intervalStart) : selectionInterval.intervalStart
            );

            int newEnd = Math.Max(
                selectionInterval.intervalEnd, overlappingIntervals.Any() ? 
                overlappingIntervals.Max(i => i.intervalEnd) : selectionInterval.intervalEnd
            );
            
            intervalList.Add(new TagInterval(
                newStart, 
                newEnd, 
                selectionInterval.tagType, 
                selectionInterval.openTag, 
                selectionInterval.closeTag,
                false,
                GetNextMicroPosition(newStart),
                GetNextMicroPosition(newEnd)
            ));
        }

        private bool AreIntervalsAdjacent(TagInterval a, TagInterval b)
        {
            return a.intervalEnd == b.intervalStart || b.intervalEnd == a.intervalStart;
        }

        #endregion

        #region Logical-to-Raw Conversion

        private void RebuildRawText() // what will be shown to the user
        {
            if (intervalList.Count == 0 && preexistingUnmanagedTags.Count == 0) //defensive but not really
            {
                inputField.text = logicalText;
                return;
            }

            var tagList = new List<Tag>();
            
            foreach (var interval in intervalList)
            {
                tagList.Add(new Tag(
                    interval.intervalStart, 
                    true, 
                    interval.openTag, 
                    interval.tagType, 
                    interval.isPreexistingInterval, 
                    interval.openMicroPos
                ));

                tagList.Add(new Tag(
                    interval.intervalEnd, 
                    false, 
                    interval.closeTag, 
                    interval.tagType, 
                    interval.isPreexistingInterval, 
                    interval.closeMicroPos
                ));
            }

           tagList.AddRange(preexistingUnmanagedTags);

            // sort conflicting tags in case they occupy the same place
            tagList.Sort((a, b) => 
            {
                //Le sort priority order
                if (a.logicalPos != b.logicalPos) return a.logicalPos.CompareTo(b.logicalPos); // 1. earlier -> late index tags
                if (a.microPos != b.microPos) return a.microPos.CompareTo(b.microPos); //2. microPos order
                if (a.isOpeningTag != b.isOpeningTag) return a.isOpeningTag.CompareTo(b.isOpeningTag); // 3. closing tags -> open tags
                return 0; // 4. fallback; tags are equal
            });

            var result = new System.Text.StringBuilder();
            int currRawTextPos = 0;

            foreach (var tag in tagList)
            {
                if (tag.logicalPos > currRawTextPos)
                {
                    result.Append(logicalText.Substring(currRawTextPos, tag.logicalPos - currRawTextPos));//add logical (non-tag) text til this point
                    currRawTextPos = tag.logicalPos;
                }
                result.Append(tag.tag);//add the tag now
            }

            //add remaining logical text
            if (currRawTextPos < logicalText.Length)
            {
                result.Append(logicalText.Substring(currRawTextPos));
            }

            string newUserText = result.ToString();
            //Debug.Log($"Generated rich text: {newUserText}");
            
            inputField.text = newUserText; //tags will be abstracted (inputField.richText = true)
        }

        public void ToggleRichText()
        {
            isRichTextEnabled = !isRichTextEnabled;
            inputField.richText = isRichTextEnabled;
            
            if (isRichTextEnabled)
            {
                ParseUserTextToIntervals();
                RebuildRawText();
            }
        }

        #endregion

        #region Tag

        private class Tag
        {
            public int logicalPos;
            public bool isOpeningTag;
            public string tag; // "<b>", "<font="..." material="..." Outline>"
            public string tagType;// "b", "fontOutline"
            public bool isPreexistingTag;
            public int microPos;

            public Tag(int logicalPos, bool isOpeningTag, string tag, string tagType, bool isPreexistingTag, int microPos)
            {
                this.logicalPos = logicalPos;
                this.isOpeningTag = isOpeningTag;
                this.tag = tag;
                this.tagType = tagType;
                this.isPreexistingTag = isPreexistingTag;
                this.microPos = microPos;
            }

            public override string ToString() //for debugging
            {
                return $"{tagType}[{tag},{logicalPos})";
            }
        }
        #endregion

        #region TagInterval

        private class TagInterval //Based off of logical index
        {
            public int intervalStart; //first char that the tag affects relative to logicalText index/open tag pos
            public int intervalEnd; //first char AFTER the last char tag affects/close tag pos
            public string tagType;
            public string openTag;
            public string closeTag;
            public bool isPreexistingInterval;
            public int openMicroPos; 
            public int closeMicroPos;

            public TagInterval(int intervalStart, int intervalEnd, string tagType, string openTag, string closeTag, bool isPreexistingInterval, int openMicroPos, int closeMicroPos)
            {
                this.intervalStart = intervalStart;
                this.intervalEnd = intervalEnd;
                this.tagType = tagType;
                this.openTag = openTag;
                this.closeTag = closeTag;
                this.isPreexistingInterval = isPreexistingInterval;
                this.openMicroPos = openMicroPos;
                this.closeMicroPos = closeMicroPos;
            }

            //todo: adapt/modify more operator oveloads for the text manipulation operations
            public static bool operator ==(TagInterval a, TagInterval b)
            {
                if (ReferenceEquals(a, b)) return true;
                if (a is null || b is null) return false;

                return (a.intervalStart == b.intervalStart) && (a.intervalEnd == b.intervalEnd) && (a.tagType == b.tagType);
            }

            public static bool operator !=(TagInterval a, TagInterval b)
            {
                return !(a == b);
            }

            public override bool Equals(object obj)
            {
                return this == obj as TagInterval;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(intervalStart, intervalEnd, tagType);
            }

            public override string ToString() //for debugging
            {
                return $"{tagType}[{intervalStart},{intervalEnd})";
            }

            //=======aktual class methods==========

            public bool DoesOverlap(TagInterval b)
            {
                if (b == null) return false;
                return intervalStart < b.intervalEnd && intervalEnd > b.intervalStart;
            }

            public bool DoesContain(TagInterval b)
            {
                if (b == null) return false;
                return intervalStart <= b.intervalStart && intervalEnd >= b.intervalEnd;
            }
        }
        #endregion
    }
}