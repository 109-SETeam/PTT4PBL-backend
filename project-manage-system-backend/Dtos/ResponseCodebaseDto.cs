namespace project_manage_system_backend.Dtos
{
    public class ResponseCodebaseDto
    {
        /// <summary>
        /// date - format: 05/10，每週第一天
        /// </summary>
        public string date { get; set; }
        /// <summary>
        /// 當週程式碼增加的行數
        /// </summary>
        public int numberOfRowsAdded { get; set; }
        /// <summary>
        /// 當週程式碼刪減的行數
        /// </summary>
        public int numberOfRowsDeleted { get; set; }
        /// <summary>
        /// 截至目前，程式碼行數的總量，numberOfRows += numberOfRowsAdded + (-numberOfRowsDeleted)
        /// </summary>
        public int numberOfRows { get; set; }
    }
}
