using System.Runtime.Serialization;

namespace APIWMS.Data.Enums
{
    public enum DocumentType
    {
        [EnumMember(Value = "1601 - WM")]
        WM = 1601,

        [EnumMember(Value = "1089 - PM")]
        PM = 1089,

        [EnumMember(Value = "1602 - MP")]
        MP = 1602,

        [EnumMember(Value = "1093 - AWD")]
        AWD = 1093,

        [EnumMember(Value = "1605 - ZWM")]
        ZWM = 1605,

        [EnumMember(Value = "2041 - FSK")]
        FSK = 2041,

        [EnumMember(Value = "2033 - FS")]
        FS = 2033,

        [EnumMember(Value = "2001 - WZ")]
        WZ = 2001,

        [EnumMember(Value = "1616 - RW")]
        RW = 1616,

        [EnumMember(Value = "1617 - PW")]
        PW = 1617,

        [EnumMember(Value = "1603 - MMW")]
        MMW = 1603,

        [EnumMember(Value = "2034 - PA")]
        PA = 2034,

        [EnumMember(Value = "2037 - FSE")]
        FSE = 2037,

        [EnumMember(Value = "1604 - MMP")]
        MMP = 1604
    }



    // Groups
    public static class DocumentTypeGroups
    {
        public static readonly HashSet<DocumentType> DokHandlowy = new()
        {
            DocumentType.FS,
            DocumentType.WZ,
            DocumentType.RW,
            DocumentType.PW,
            DocumentType.MMW,
            DocumentType.PA,
            DocumentType.FSE,
            DocumentType.MMP,
            DocumentType.FSK
        };

        public static readonly HashSet<DocumentType> DokMagazynowe = new()
        {
            DocumentType.WM,
            DocumentType.PM,
            DocumentType.MP,
            DocumentType.AWD,
            DocumentType.ZWM
        };
    }
}
