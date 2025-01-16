namespace APIWMS.Data.Enums
{
    public enum DocumentType
    {
        // Dokumenty magazynowe
        WM = 1601,
        PM = 1089,
        MP = 1602,
        AWD = 1093,
        ZWM = 1605,

        // Dokumenty handlowe
        FSK = 2041,
        FS = 2033,
        WZ = 2001,
        RW = 1616,
        PW = 1617,
        MMW = 1603,
        PA = 2034,
        FSE = 2037,
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
