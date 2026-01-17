namespace fuszerkomat_api.VM
{
    public class MessageVM
    {
        public string EncryptedPayload { get; set; } = string.Empty;
        public string KeyForRecipient { get; set; } = string.Empty;
        public string KeyForSender { get; set; } = string.Empty;
        public string Iv { get; set; } = string.Empty;
    }
}
