using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Kcow.Application.Import;

public sealed record LegacyChildImportResult(
    IReadOnlyList<LegacyChildRecord> Records,
    IReadOnlyList<LegacyXmlValidationError> ValidationErrors);

public sealed record LegacyChildRecord(
    string Reference,
    string? ChildName,
    string? ChildSurname,
    string? ChildBirthdate,
    string? Sex,
    string? Language,
    string? AccountPersonName,
    string? AccountPersonSurname,
    string? AccountPersonIdnumber,
    string? AccountPersonCellphone,
    string? AccountPersonOffice,
    string? AccountPersonHome,
    string? AccountPersonEmail,
    string? Relation,
    string? MotherName,
    string? MotherSurname,
    string? MotherOffice,
    string? MotherCell,
    string? MotherHome,
    string? MotherEmail,
    string? FatherName,
    string? FatherSurname,
    string? FatherOffice,
    string? FatherCell,
    string? FatherHome,
    string? FatherEmail,
    string? Address1,
    string? Address2,
    string? Code,
    string? SchoolName,
    string? ClassGroup,
    string? Grade,
    string? Teacher,
    string? AttendingKcowAt,
    string? Aftercare,
    string? Extra,
    string? HomeTime,
    string? StartClasses,
    string? Terms,
    string? Seat,
    string? Truck,
    string? Family,
    string? Sequence,
    string? FinancialCode,
    string? Charge,
    string? Deposit,
    string? PayDate,
    string? TshirtCode,
    string? TshirtMoney1,
    string? TshirtMoneyDate1,
    string? TshirtReceived1,
    string? TshirtRecDate1,
    string? ReceiveNote1,
    string? TshirtSize1,
    string? TshirtColor1,
    string? TshirtDesign1,
    string? TshirtSize2,
    string? TshirtMoney2,
    string? TshirtMoneyDate2,
    string? TshirtReceived2,
    string? TshirtRecDate2,
    string? ReceiveNote2,
    string? TshirtColor2,
    string? TshirtDesign2,
    string? Indicator1,
    string? Indicator2,
    string? GeneralNote,
    string? PrintIdCard,
    string? AcceptTermsCond,
    string? Status,
    string? SmsOrEmail,
    string? SchoolClose,
    string? Cnt,
    string? OnlineEntry,
    string? Created,
    string? Submitted,
    string? Updated,
    string? BookEmail,
    string? Report1GivenOut,
    string? AccountGivenOut,
    string? CertificatePrinted,
    string? Report2GivenOut,
    string? Social,
    string? ActivityReportGivenOut,
    string? Photo,
    string? PhotoUpdated);

public sealed class LegacyChildXmlParser
{
    public LegacyChildImportResult Parse(string xmlPath, string xsdPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath))
        {
            throw new ArgumentException("XML path is required.", nameof(xmlPath));
        }

        if (string.IsNullOrWhiteSpace(xsdPath))
        {
            throw new ArgumentException("XSD path is required.", nameof(xsdPath));
        }

        var errors = new List<LegacyXmlValidationError>();
        var settings = BuildValidationSettings(xsdPath, errors);

        XDocument document;
        try
        {
            using var stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = XmlReader.Create(stream, settings);
            document = XDocument.Load(reader);
        }
        catch (XmlException ex)
        {
            errors.Add(new LegacyXmlValidationError(ex.Message, ex.LineNumber, ex.LinePosition));
            return new LegacyChildImportResult(Array.Empty<LegacyChildRecord>(), errors);
        }
        catch (Exception ex)
        {
            errors.Add(new LegacyXmlValidationError(ex.Message, null, null));
            return new LegacyChildImportResult(Array.Empty<LegacyChildRecord>(), errors);
        }

        var records = new List<LegacyChildRecord>();
        foreach (var childElement in document.Root?.Elements("Children") ?? Enumerable.Empty<XElement>())
        {
            var record = new LegacyChildRecord(
                Reference: NormalizeString(GetValue(childElement, "Reference")) ?? string.Empty,
                ChildName: NormalizeString(GetValue(childElement, "Child_x0020_Name")),
                ChildSurname: NormalizeString(GetValue(childElement, "Child_x0020_Surname")),
                ChildBirthdate: NormalizeString(GetValue(childElement, "Child_x0020_birthdate")),
                Sex: NormalizeString(GetValue(childElement, "Sex")),
                Language: NormalizeString(GetValue(childElement, "Language")),
                AccountPersonName: NormalizeString(GetValue(childElement, "Account_x0020_Person_x0020_Name")),
                AccountPersonSurname: NormalizeString(GetValue(childElement, "Account_x0020_Person_x0020_Surname")),
                AccountPersonIdnumber: NormalizeString(GetValue(childElement, "Account_x0020_Person_x0020_Idnumber")),
                AccountPersonCellphone: NormalizeString(GetValue(childElement, "Account_x0020_Person_x0020_Cellphone")),
                AccountPersonOffice: NormalizeString(GetValue(childElement, "Account_x0020_Person_x0020_Office")),
                AccountPersonHome: NormalizeString(GetValue(childElement, "Account_x0020_Person_x0020_Home")),
                AccountPersonEmail: NormalizeString(GetValue(childElement, "Account_x0020_Person_x0020_Email")),
                Relation: NormalizeString(GetValue(childElement, "Relation")),
                MotherName: NormalizeString(GetValue(childElement, "Mother_x0020_Name")),
                MotherSurname: NormalizeString(GetValue(childElement, "Mother_x0020_Surname")),
                MotherOffice: NormalizeString(GetValue(childElement, "Mother_x0020_Office")),
                MotherCell: NormalizeString(GetValue(childElement, "Mother_x0020_Cell")),
                MotherHome: NormalizeString(GetValue(childElement, "Mother_x0020_Home")),
                MotherEmail: NormalizeString(GetValue(childElement, "Mother_x0020_Email")),
                FatherName: NormalizeString(GetValue(childElement, "Father_x0020_Name")),
                FatherSurname: NormalizeString(GetValue(childElement, "Father_x0020_Surname")),
                FatherOffice: NormalizeString(GetValue(childElement, "Father_x0020_Office")),
                FatherCell: NormalizeString(GetValue(childElement, "Father_x0020_Cell")),
                FatherHome: NormalizeString(GetValue(childElement, "Father_x0020_Home")),
                FatherEmail: NormalizeString(GetValue(childElement, "Father_x0020_Email")),
                Address1: NormalizeString(GetValue(childElement, "Address1")),
                Address2: NormalizeString(GetValue(childElement, "Address2")),
                Code: NormalizeString(GetValue(childElement, "Code")),
                SchoolName: NormalizeString(GetValue(childElement, "School_x0020_Name")),
                ClassGroup: NormalizeString(GetValue(childElement, "Class_x0020_Group")),
                Grade: NormalizeString(GetValue(childElement, "Grade")),
                Teacher: NormalizeString(GetValue(childElement, "Teacher")),
                AttendingKcowAt: NormalizeString(GetValue(childElement, "Attending_x0020_KCOW_x0020_at")),
                Aftercare: NormalizeString(GetValue(childElement, "Aftercare")),
                Extra: NormalizeString(GetValue(childElement, "Extra")),
                HomeTime: NormalizeString(GetValue(childElement, "Home_x0020_Time")),
                StartClasses: NormalizeString(GetValue(childElement, "Start_x0020_Classes")),
                Terms: NormalizeString(GetValue(childElement, "Terms")),
                Seat: NormalizeString(GetValue(childElement, "Seat")),
                Truck: NormalizeString(GetValue(childElement, "Truck")),
                Family: NormalizeString(GetValue(childElement, "Family")),
                Sequence: NormalizeString(GetValue(childElement, "Sequence")),
                FinancialCode: NormalizeString(GetValue(childElement, "Financial_x0020_Code")),
                Charge: NormalizeString(GetValue(childElement, "Charge")),
                Deposit: NormalizeString(GetValue(childElement, "Deposit")),
                PayDate: NormalizeString(GetValue(childElement, "PayDate")),
                TshirtCode: NormalizeString(GetValue(childElement, "Tshirt_x0020_Code")),
                TshirtMoney1: NormalizeString(GetValue(childElement, "Tshirt_x0020_Money_x0020_1")),
                TshirtMoneyDate1: NormalizeString(GetValue(childElement, "Tshirt_x0020_MoneyDate_x0020_1")),
                TshirtReceived1: NormalizeString(GetValue(childElement, "Tshirt_x0020_Received_x0020_1")),
                TshirtRecDate1: NormalizeString(GetValue(childElement, "Tshirt_x0020_RecDate_x0020_1")),
                ReceiveNote1: NormalizeString(GetValue(childElement, "Receive_x0020_Note_x0020_1")),
                TshirtSize1: NormalizeString(GetValue(childElement, "TshirtSize1")),
                TshirtColor1: NormalizeString(GetValue(childElement, "TshirtColor1")),
                TshirtDesign1: NormalizeString(GetValue(childElement, "TshirtDesign1")),
                TshirtSize2: NormalizeString(GetValue(childElement, "TshirtSize2")),
                TshirtMoney2: NormalizeString(GetValue(childElement, "Tshirt_x0020_Money_x0020_2")),
                TshirtMoneyDate2: NormalizeString(GetValue(childElement, "Tshirt_x0020_MoneyDate_x0020_2")),
                TshirtReceived2: NormalizeString(GetValue(childElement, "Tshirt_x0020_Received_x0020_2")),
                TshirtRecDate2: NormalizeString(GetValue(childElement, "Tshirt_x0020_RecDate_x0020_2")),
                ReceiveNote2: NormalizeString(GetValue(childElement, "Receive_x0020_Note_x0020_2")),
                TshirtColor2: NormalizeString(GetValue(childElement, "TshirtColor2")),
                TshirtDesign2: NormalizeString(GetValue(childElement, "TshirtDesign2")),
                Indicator1: NormalizeString(GetValue(childElement, "Indicator_x0020_1")),
                Indicator2: NormalizeString(GetValue(childElement, "Indicator_x0020_2")),
                GeneralNote: NormalizeString(GetValue(childElement, "General_x0020_Note")),
                PrintIdCard: NormalizeString(GetValue(childElement, "Print_x0020_Id_x0020_Card")),
                AcceptTermsCond: NormalizeString(GetValue(childElement, "AcceptTermsCond")),
                Status: NormalizeString(GetValue(childElement, "Status")),
                SmsOrEmail: NormalizeString(GetValue(childElement, "SmsOrEmail")),
                SchoolClose: NormalizeString(GetValue(childElement, "SchoolClose")),
                Cnt: NormalizeString(GetValue(childElement, "Cnt")),
                OnlineEntry: NormalizeString(GetValue(childElement, "OnlineEntry")),
                Created: NormalizeString(GetValue(childElement, "Created")),
                Submitted: NormalizeString(GetValue(childElement, "Submitted")),
                Updated: NormalizeString(GetValue(childElement, "Updated")),
                BookEmail: NormalizeString(GetValue(childElement, "BookEmail")),
                Report1GivenOut: NormalizeString(GetValue(childElement, "Report1GivenOut")),
                AccountGivenOut: NormalizeString(GetValue(childElement, "AccountGivenOut")),
                CertificatePrinted: NormalizeString(GetValue(childElement, "CertificatePrinted")),
                Report2GivenOut: NormalizeString(GetValue(childElement, "Report2GivenOut")),
                Social: NormalizeString(GetValue(childElement, "Social")),
                ActivityReportGivenOut: NormalizeString(GetValue(childElement, "ActivityReportGivenOut")),
                Photo: NormalizeString(GetValue(childElement, "Photo")),
                PhotoUpdated: NormalizeString(GetValue(childElement, "PhotoUpdated")));

            records.Add(record);
        }

        return new LegacyChildImportResult(records, errors);
    }

    private static XmlReaderSettings BuildValidationSettings(string xsdPath, ICollection<LegacyXmlValidationError> errors)
    {
        var schemas = new XmlSchemaSet();
        schemas.Add(null, xsdPath);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        settings.ValidationEventHandler += (_, args) =>
        {
            var exception = args.Exception;
            errors.Add(new LegacyXmlValidationError(
                args.Message,
                exception?.LineNumber,
                exception?.LinePosition));
        };

        return settings;
    }

    private static string? GetValue(XElement parent, string elementName)
    {
        return parent.Element(elementName)?.Value;
    }

    private static string? NormalizeString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
