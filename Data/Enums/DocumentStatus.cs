namespace APIWMS.Data.Enums
{
    public enum DocumentStatus
    {
        Cancel = -2,
        Delete = -1,
        Confirm = 0,
        Bufor = 1,

        // Dla AWD i ZWM

        AWD_Bufor = 100,
        AWD_Realize = 101,
        AWD_Close = 104,
    }
}
