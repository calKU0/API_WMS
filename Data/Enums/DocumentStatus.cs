using System.Runtime.Serialization;

namespace APIWMS.Data.Enums
{
    public enum DocumentStatus
    {
        [EnumMember(Value = "-2 - Cancel")]
        Cancel = -2,

        [EnumMember(Value = "-1 - Delete")]
        Delete = -1,

        [EnumMember(Value = "0 - Confirm")]
        Confirm = 0,

        [EnumMember(Value = "1 - Bufor")]
        Bufor = 1
    }
}
