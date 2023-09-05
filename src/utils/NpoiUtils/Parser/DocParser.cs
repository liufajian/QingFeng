using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace QingFeng.NpoiUtils.Parser
{
    internal class DocParser
    {
        readonly StringBuilder _sb;

        public DocParser()
        {
            _sb = new StringBuilder(100);
        }

        private void ParseDocument(XWPFDocument doc)
        {
            var nodeList = new List<ParsedRunNode>();

            for (var i = 0; i < doc.HeaderList.Count; i++)
            {
                for (var j = 0; j < doc.HeaderList[i].BodyElements.Count; j++)
                {
                    if (doc.HeaderList[i].BodyElements[j] is XWPFParagraph paragraph)
                    {
                        ParseParagraph(paragraph, j, nodeList);
                    }
                }
            }

            //解析成树状结构再进行处理
            for (var i = 0; i < doc.BodyElements.Count; i++)
            {
                if (doc.BodyElements[i] is XWPFParagraph paragraph)
                {
                    ParseParagraph(paragraph, i, nodeList);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ParseParagraph(XWPFParagraph paragraph, int paragraphIndex, List<ParsedRunNode> list)
        {
            int runIndex = -1;
            XWPFRun startRun = null;
            int searchStart, searchEnd;
            InnerRunNode lastNode = null, lastSibling = null;

            foreach (var run in paragraph.Runs)
            {
                runIndex++;
                searchStart = -1;

                for (var i = 0; i < 100; i++)
                {
                    if (startRun == null)
                    {
                        searchStart = run.Text.IndexOf("{{", searchStart + 1);

                        if (searchStart < 0)
                        {
                            break;
                        }
                        else
                        {
                            startRun = run;
                            lastSibling = lastNode = new InnerRunNode(run, runIndex, searchStart) { Prev = lastNode };
                        }
                    }
                    else
                    {
                        //must be searchStart + 1 to prevent current run is not start run
                        searchEnd = run.Text.IndexOf("}}", searchStart + 1);

                        if (searchEnd < 0)
                        {
                            //search like:{{aaa{{bbb}}
                            searchStart = run.Text.LastIndexOf("{{", searchStart + 1);

                            if (searchStart < 0)
                            {
                                lastSibling = lastSibling.Sibling = new InnerRunNode(run, runIndex, -1) { Prev = lastSibling };
                            }
                            else //reset start run
                            {
                                startRun = run;
                                lastSibling = lastNode = new InnerRunNode(run, runIndex, searchEnd) { Prev = lastNode };
                                break;
                            }
                        }
                        else
                        {
                            searchStart = searchEnd + 2;
                            lastSibling.Sibling = new InnerRunNode(run, runIndex, searchEnd) { Prev = lastSibling };

                            startRun = null;
                            lastSibling = null;
                        }
                    }
                }
            }

            //lastSibling is not null meaning the last node is not closed
            if (lastSibling != null)
            {
                lastNode = lastNode.Prev;
            }

            if (lastNode == null)
            {
                return;
            }

            while (lastNode != null)
            {
                if (lastNode.Run == lastNode.Sibling.Run)
                {
                    ParseSingleRun(paragraph, paragraphIndex, lastNode);
                }
                else
                {
                    ParseMultiRun(paragraph, paragraphIndex, lastNode);
                }
                lastNode = lastNode.Prev;
            }
        }

        private ParsedRunNode ParseSingleRun(XWPFParagraph paragraph, int paragraphIndex, InnerRunNode node)
        {
            var startPos1 = node.MatchPos;
            var startPos2 = node.Sibling.MatchPos + 2;

            var runTextSpan = node.Run.Text.AsSpan();
            int innerLen = startPos2 - startPos1 - 4;
            var innerMatch = runTextSpan.Slice(startPos1 + 2, innerLen).Trim();

            if (innerMatch.Length == innerLen && innerMatch.Length > 0)
            {
                return new ParsedRunNode
                {
                    Run = node.Run,
                    RunIndex = node.RunIndex,
                    InnerText = innerMatch.ToString(),
                    Paragraph = paragraph,
                    ParagraphIndex = paragraphIndex
                };
            }

            _sb.Clear();

            if (startPos1 > 0)
            {
                _sb.Append(runTextSpan.Slice(0, startPos1).ToArray());
            }

            ParsedRunNode parsedNode = null;

            if (innerMatch.Length > 0)
            {
                parsedNode = new ParsedRunNode
                {
                    Run = node.Run,
                    RunIndex = node.RunIndex,
                    InnerText = innerMatch.ToString(),
                    Paragraph = paragraph,
                    ParagraphIndex = paragraphIndex
                };

                _sb.Append("{{").Append(parsedNode.InnerText).Append("}}");
            }

            if (runTextSpan.Length > startPos2)
            {
                _sb.Append(runTextSpan.Slice(startPos2).ToArray());
            }

            if (_sb.Length > 0)
            {
                node.Run.SetText(_sb.ToString());
            }
            else
            {
                paragraph.RemoveRun(node.RunIndex);
            }

            return parsedNode;
        }

        private ParsedRunNode ParseMultiRun(XWPFParagraph paragraph, int paragraphIndex, InnerRunNode node)
        {
            _sb.Clear();

            var runTextSpan = node.Run.Text.AsSpan();

            var parsedNode = new ParsedRunNode { Paragraph = paragraph, ParagraphIndex = paragraphIndex };

            //parse start

            var removed = 0;
            var innerMatch = runTextSpan.Slice(node.MatchPos + 2).Trim();

            if (innerMatch.Length > 0)
            {
                parsedNode.Run = node.Run;
                parsedNode.RunIndex = node.RunIndex;
                _sb.Append(innerMatch.ToArray());
            }
            else
            {
                if (node.MatchPos > 0)
                {
                    node.Run.SetText(runTextSpan.Slice(0, node.MatchPos).ToString());
                }
                else
                {
                    removed++;
                    paragraph.RemoveRun(node.RunIndex);
                }
            }

            var lastSibling = node.Sibling;

            while (lastSibling != null)
            {
                if (lastSibling.MatchPos < 0) //mid
                {
                    var text = lastSibling.Run.Text.Trim();

                    if (text.Length > 0)
                    {
                        _sb.Append(text);

                        if (parsedNode.Run == null)
                        {
                            parsedNode.Run = lastSibling.Run;
                            parsedNode.RunIndex = lastSibling.RunIndex - removed;
                        }
                    }

                    if (parsedNode.Run != lastSibling.Run)
                    {
                        paragraph.RemoveRun(lastSibling.RunIndex - removed);
                        removed++;
                    }
                }
                else //end
                {
                    runTextSpan = lastSibling.Run.Text.AsSpan();
                    innerMatch = runTextSpan.Slice(0, lastSibling.MatchPos).Trim();

                    if (innerMatch.Length > 0)
                    {
                        _sb.Append(innerMatch.ToArray());
                    }

                    if (parsedNode.Run == null && _sb.Length > 2)
                    {
                        parsedNode.Run = lastSibling.Run;
                        parsedNode.RunIndex = lastSibling.RunIndex - removed;
                        parsedNode.InnerText = _sb.ToString();
                        _sb.Insert(0, "{{").Append("}}")
                            .Append(runTextSpan.Slice(lastSibling.MatchPos + 2).ToArray());
                        lastSibling.Run.SetText(_sb.ToString());
                    }
                    else
                    {
                        if (runTextSpan.Length > lastSibling.MatchPos + 2)
                        {
                            lastSibling.Run.SetText(runTextSpan.Slice(lastSibling.MatchPos + 2).ToString());
                        }
                        else
                        {
                            paragraph.RemoveRun(lastSibling.RunIndex - removed);
                        }
                    }
                }

                lastSibling = lastSibling.Sibling;
            }

            //set run
            if (parsedNode.Run != null && parsedNode.InnerText == null)
            {
                parsedNode.InnerText = _sb.ToString();

                _sb.Insert(0, "{{").Append("}}");

                if (node.Run == parsedNode.Run && node.MatchPos > 0)
                {
                    node.Run.SetText(node.Run.Text.Substring(0, node.MatchPos) + _sb.ToString());
                }
                else
                {
                    node.Run.SetText(_sb.ToString());
                }
            }

            return parsedNode;
        }
    }
}
