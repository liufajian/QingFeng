using NPOI.XWPF.UserModel;

namespace QingFeng.NpoiUtils.Parser
{
    public class ParsedRunNode
    {
        public XWPFParagraph Paragraph { get; set; }

        public int ParagraphIndex { get; set; }

        public XWPFRun Run { get; set; }

        public int RunIndex { get; set; }

        public string InnerText { get; set; }
    }
}
