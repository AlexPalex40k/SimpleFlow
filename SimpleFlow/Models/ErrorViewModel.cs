namespace SimpleFlow.Models
{
    /// <summary>
    /// Данные страницы отображения ошибки.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Идентификатор HTTP-запроса.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Признак необходимости отображения идентификатора запроса.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
