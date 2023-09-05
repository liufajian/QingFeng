using NPOI.XWPF.UserModel;
using System;

namespace QingFeng.NpoiUtils.Parser
{
    class InnerRunNode
    {
        public InnerRunNode(XWPFRun run, int runIndex, int matchPos)
        {
            Run = run ?? throw new ArgumentNullException(nameof(run));
            RunIndex = runIndex;
            MatchPos = matchPos;
        }

        public XWPFRun Run { get; }

        public int RunIndex { get; }

        public int MatchPos { get; }

        public InnerRunNode Prev { get; set; }

        public InnerRunNode Sibling { get; set; }
    }
}
