namespace QingFeng.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct TableHeader
    {
        public Dictionary<string, int> ColumnMap { get; }

        public TableHeader()
        {
            ColumnMap = new Dictionary<string, int>();
        }

        public TableHeader(Dictionary<string, int> columnMap)
        {
            ColumnMap = columnMap ?? throw new ArgumentNullException(nameof(columnMap));
        }

        public TableHeader(IEnumerable<string> columnNames)
        {
            ColumnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (columnNames != null)
            {
                var index = 0;

                foreach (var col in columnNames)
                {
                    if (!ColumnMap.ContainsKey(col ?? ""))
                    {
                        ColumnMap.Add(col ?? "", index);
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public int GetIndex(string columnName)
        {
            return ColumnMap.TryGetValue(columnName ?? "", out var index) ? index : -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetIndex(string columnName, out int index)
        {
            return ColumnMap.TryGetValue(columnName ?? "", out index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TableHeader DeepClone()
        {
            return new TableHeader(new Dictionary<string, int>(ColumnMap));
        }
    }
}
