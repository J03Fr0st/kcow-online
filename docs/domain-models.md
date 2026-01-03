# KCOW Domain Models

> Domain entity reference derived from legacy Microsoft Access schemas
> Generated: 2025-12-27 | Updated: 2026-01-03 (Bilingual field names - English and Afrikaans)

## Overview

KCOW manages mobile computer literacy trucks (Trok) that visit schools to provide educational computer sessions to children. The domain model reflects this core business:

```
School (1) ←——→ (N) Class_Group (sessions at a school)
Class_Group (1) ←——→ (N) Children (students in group)
Activity (1) ←——→ (N) Class_Group (programs delivered)
```

---

## School

**Description**: Schools visited by the mobile computer trucks. (Afrikaans: "Skool")

### Fields (Complete from XSD - 30 fields)

| Field (Proposed) | Legacy Name (XSD) | English Display | Afrikaans Display | Type | Max Length | Required | Description |
|------------------|-------------------|-----------------|-------------------|------|------------|----------|-------------|
| `schoolId` | `School_x0020_Id` | School Id | Skool Id | int | - | Yes (PK) | Unique identifier (auto-increment) |
| `shortName` | `Short_x0020_School` | Short School | Kort Skool | nvarchar | 50 | No | Short name/code for the school |
| `description` | `School_x0020_Description` | School Description | Skool Beskrywing | nvarchar | 50 | No | Full school name |
| `truckId` | `Trok` | Truck | Trok | tinyint | - | No | Assigned truck number (default: 0) |
| `price` | `Price` | Price | Prys | money | - | No | Service fee charged (format: R #,##0.00) |
| `feeDescription` | `F_x0020_Descr` | Fee Description | Fooi Beskrywing | nvarchar | 255 | No | Fee description text |
| `formula` | `Formula` | Formula | Formule | float | - | No | Calculation formula value (default: 0) |
| `visitDay` | `Day` | Day | Dag | nvarchar | 50 | No | Visit day of week |
| `visitSequence` | `Sequence` | Sequence | Volgorde | nvarchar | 50 | No | Visit order for the day |
| `contactPerson` | `ContactPerson` | Contact Person | Kontak Persoon | nvarchar | 50 | No | Primary contact name |
| `contactCell` | `ContactCell` | Contact Cell | Kontak Selfoon | nvarchar | 50 | No | Contact mobile phone |
| `contactEmail` | `E-mail_x0020_adress` | E-mail Address | E-pos Adres | nvarchar | 50 | No | Contact email address |
| `telephone` | `Telephone` | Telephone | Telefoon | nvarchar | 50 | No | School telephone number |
| `fax` | `Fax` | Fax | Faks | nvarchar | 50 | No | School fax number |
| `address1` | `Address1` | Address 1 | Adres 1 | nvarchar | 50 | No | Street address line 1 |
| `address2` | `Address2` | Address 2 | Adres 2 | nvarchar | 50 | No | Street address line 2 |
| `headmaster` | `Headmaster` | Headmaster | Hoof | nvarchar | 50 | No | Principal/headmaster name |
| `headmasterCell` | `HeadmasterCell` | Headmaster Cell | Hoof Selfoon | nvarchar | 50 | No | Headmaster mobile phone |
| `moneyMessage` | `MoneyMessage` | Money Message | Geld Boodskap | ntext | memo | No | Payment-related message/notes |
| `printInvoice` | `Print` | Print | Druk | bit | - | Yes | Include in print batch (Yes/No) |
| `language` | `Taal` | Language | Taal | nvarchar | 50 | No | Language preference (Afr/Eng, default: Afr) |
| `importFlag` | `Import` | Import | Invoer | bit | - | Yes | Import marker flag (default: False) |
| `webPage` | `web_x0020_page` | Web Page | Web Bladsy | hyperlink | memo | No | School website URL |
| `afterschool1Name` | `Naskool1_x0020_Name` | Afterschool 1 Name | Naskool 1 Naam | nvarchar | 255 | No | Afterschool program 1 name |
| `afterschool1Contact` | `Naskool1_x0020_Contact` | Afterschool 1 Contact | Naskool 1 Kontak | nvarchar | 255 | No | Afterschool program 1 contact |
| `afterschool2Name` | `Naskool2_x0020_Name` | Afterschool 2 Name | Naskool 2 Naam | nvarchar | 255 | No | Afterschool program 2 name |
| `afterschool2Contact` | `Naskool2_x0020_Contact` | Afterschool 2 Contact | Naskool 2 Kontak | nvarchar | 255 | No | Afterschool program 2 contact |
| `safeNotes` | `Kluis` | Safe | Kluis | ntext | memo | No | Storage/safe access notes |
| `circularsEmail` | `omsendbriewe` | Circulars Email | Omsendbriewe | nvarchar | 255 | No | Email for circular distribution |
| `kcowWebPageLink` | `KcowWebPageLink` | KCOW Web Page Link | KCOW Web Bladsy Skakel | hyperlink | memo | No | KCOW-specific web page link |

### TypeScript Interface (Proposed)

```typescript
interface School {
  id: number;
  shortName: string;
  description: string;
  truckId: number;
  price: number;
  feeDescription?: string;
  formula?: number;
  visitDay: string;
  visitSequence: string;
  contact: {
    name?: string;
    cell?: string;
    email?: string;
  };
  telephone?: string;
  fax?: string;
  address: {
    line1?: string;
    line2?: string;
  };
  headmaster?: string;
  headmasterCell?: string;
  moneyMessage?: string;
  printInvoice: boolean;
  language: 'Afrikaans' | 'English';
  importFlag: boolean;
  webPage?: string;
  afterschool1?: {
    name?: string;
    contact?: string;
  };
  afterschool2?: {
    name?: string;
    contact?: string;
  };
  safeNotes?: string;
  circularsEmail?: string;
  kcowWebPageLink?: string;
}
```

### Business Rules

- Each school is assigned to one truck (`truckId`)
- Schools are visited on specific days in a defined sequence
- Bilingual support: Afrikaans or English instruction (default: Afrikaans)
- `printInvoice` controls invoice batch printing
- Afterschool programs can be associated for extended care coordination

---

## Activity

**Description**: Educational programs/activities offered on the trucks. (Afrikaans: "Aktiwiteit")

### Fields (Complete from XSD - 7 fields)

| Field (Proposed) | Legacy Name (XSD) | English Display | Afrikaans Display | Type | Max Length | Required | Description |
|------------------|-------------------|-----------------|-------------------|------|------------|----------|-------------|
| `activityId` | `ActivityID` | Activity ID | Aktiwiteit ID | int | - | Yes (PK) | Unique identifier |
| `programCode` | `Program` | Program | Program | nvarchar | 255 | No | Program code/identifier |
| `programName` | `ProgramName` | Program Name | Program Naam | nvarchar | 255 | No | Display name of program |
| `educationalFocus` | `Educational_Focus` | Educational Focus | Opvoedkundige Fokus | ntext | memo | No | Learning objectives description |
| `folder` | `Folder` | Folder | Vouer | nvarchar | 255 | No | Resource folder path |
| `grade` | `Grade` | Grade | Graad | nvarchar | 255 | No | Target grade level |
| `icon` | `Icon` | Icon | Ikoon | image | blob | No | Visual representation (OLE object) |

### TypeScript Interface (Proposed)

```typescript
interface Activity {
  id: number;
  programCode?: string;
  programName?: string;
  educationalFocus?: string;
  folder?: string;
  grade?: string;
  iconUrl?: string;
}
```

### Business Rules

- Activities are educational software programs
- Each activity targets specific grade levels
- Resources are stored in designated folders
- Icon is stored as OLE object (needs conversion for web)

---

## Class Group

**Description**: Scheduled class sessions at schools. (Afrikaans: "Klas Groep")

### Fields (Complete from XSD - 15 fields)

| Field (Proposed) | Legacy Name (XSD) | English Display | Afrikaans Display | Type | Max Length | Required | Description |
|------------------|-------------------|-----------------|-------------------|------|------------|----------|-------------|
| `classGroup` | `Class_x0020_Group` | Class Group | Klas Groep | nvarchar | 10 | No | Group identifier/code |
| `dayTruck` | `DayTruck` | Day Truck | Dag Trok | nvarchar | 6 | No | Day-truck combination key |
| `description` | `Description` | Description | Beskrywing | nvarchar | 35 | No | Group description |
| `startTime` | `Start_x0020_Time` | Start Time | Begin Tyd | nvarchar | 5 | No | Session start (HH:MM format) |
| `endTime` | `End_x0020_Time` | End Time | Eind Tyd | nvarchar | 5 | No | Session end (HH:MM format) |
| `schoolId` | `School_x0020_Id` | School Id | Skool Id | smallint | - | No | Associated school (FK) |
| `dayId` | `DayId` | Day Id | Dag Id | nvarchar | 1 | No | Day code (M/T/W/H/F) |
| `sequence` | `Sequence` | Sequence | Volgorde | nvarchar | 50 | No | Display order |
| `evaluate` | `Evaluate` | Evaluate | Evalueer | bit | - | Yes | Requires evaluation (default: No) |
| `note` | `Note` | Note | Nota | nvarchar | 255 | No | Additional notes |
| `importFlag` | `Import` | Import | Invoer | bit | - | Yes | Import marker (default: False) |
| `groupMessage` | `GroupMessage` | Group Message | Groep Boodskap | nvarchar | 255 | No | Message for this group |
| `sendCertificates` | `Send_x0020_Certificates` | Send Certificates | Stuur Sertifikate | nvarchar | 255 | No | Certificate sending status/notes |
| `moneyMessage` | `Money_x0020_Message` | Money Message | Geld Boodskap | nvarchar | 50 | No | Payment-related message |
| `ixl` | `IXL` | IXL | IXL | nvarchar | 3 | No | IXL program code |

### TypeScript Interface (Proposed)

```typescript
interface ClassGroup {
  code: string;
  dayTruck: string;
  description?: string;
  schedule: {
    startTime: string; // "HH:MM"
    endTime: string;   // "HH:MM"
    dayOfWeek: 'M' | 'T' | 'W' | 'H' | 'F';
  };
  schoolId: number;
  sequence?: string;
  evaluate: boolean;
  note?: string;
  importFlag: boolean;
  groupMessage?: string;
  sendCertificates?: string;
  moneyMessage?: string;
  ixlCode?: string;
}
```

### Business Rules

- Class groups are time slots at specific schools
- `dayTruck` creates unique scheduling key (e.g., "M1" = Monday, Truck 1)
- Day codes: M=Monday, T=Tuesday, W=Wednesday, H=Thursday, F=Friday
- `evaluate` flag indicates assessment requirements
- `ixl` code links to IXL educational platform

---

## Children (Students)

**Description**: Student enrollment records. Comprehensive schema with personal, family, enrollment, and tracking data.

### Legacy UI Reference

The legacy Microsoft Access application provides a comprehensive student management interface with the following tabs:

| Tab | Screenshot | Purpose |
|-----|------------|---------|
| **Child Information** | [View](./legacy/4_Children/1_Child_Information.png) | Main student form - personal details, family contacts, class assignment, T-shirt orders |
| **Child Financial** | [View](./legacy/4_Children/2_Child_Financial.png) | Invoice/Receipt tracking, balance calculation, certificate & statement printing |
| **Class Groups** | [View](./legacy/4_Children/3_Class_Group.png) | Grid view of all students in a class group with basic info and balance |
| **Class Groups Attendance** | [View](./legacy/4_Children/4_Class_Group_Attendance.png) | Attendance tracking grid with value, grade, planned date, seat, activity |
| **Child Evaluation** | [View](./legacy/4_Children/5_Child_Evaluation.png) | Individual student evaluation matrix (14 activity scores × 2 sets), speed/accuracy metrics, progress reports |
| **Class Groups Evaluation** | [View](./legacy/4_Children/6_Class_Groups_Evaluation.png) | Consolidated evaluation grid view for all students in a class group |

### Fields (Complete from XSD - 92 fields)

#### Identity Fields

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `childrenId` | `ChildrenID` | int | - | Yes (PK) | Unique identifier (auto-increment) |
| `reference` | `Reference` | nvarchar | 10 | Yes | Reference code |
| `childName` | `Child_Name` | nvarchar | 50 | No | Child's first name |
| `childSurname` | `Child_Surname` | nvarchar | 50 | No | Child's surname |
| `childBirthdate` | `Child_birthdate` | datetime | - | No | Date of birth |
| `sex` | `Sex` | nvarchar | 3 | No | Gender (M/F) |
| `language` | `Language` | nvarchar | 3 | No | Language preference (Afr/Eng) |

#### Account Person Fields (Responsible Adult)

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `accountPersonName` | `Account_Person_Name` | nvarchar | 50 | No | Responsible person first name |
| `accountPersonSurname` | `Account_Person_Surname` | nvarchar | 50 | No | Responsible person surname |
| `accountPersonIdNumber` | `Account_Person_Idnumber` | nvarchar | 20 | No | ID number |
| `accountPersonCellphone` | `Account_Person_Cellphone` | nvarchar | 20 | No | Mobile phone |
| `accountPersonOffice` | `Account_Person_Office` | nvarchar | 20 | No | Office phone |
| `accountPersonHome` | `Account_Person_Home` | nvarchar | 20 | No | Home phone |
| `accountPersonEmail` | `Account_Person_Email` | nvarchar | 100 | No | Email address |
| `relation` | `Relation` | nvarchar | 20 | No | Relationship to child |

#### Mother's Details

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `motherName` | `Mother_Name` | nvarchar | 50 | No | Mother's first name |
| `motherSurname` | `Mother_Surname` | nvarchar | 50 | No | Mother's surname |
| `motherOffice` | `Mother_Office` | nvarchar | 20 | No | Office phone |
| `motherCell` | `Mother_Cell` | nvarchar | 20 | No | Mobile phone |
| `motherHome` | `Mother_Home` | nvarchar | 20 | No | Home phone |
| `motherEmail` | `Mother_Email` | nvarchar | 100 | No | Email address |

#### Father's Details

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `fatherName` | `Father_Name` | nvarchar | 50 | No | Father's first name |
| `fatherSurname` | `Father_Surname` | nvarchar | 50 | No | Father's surname |
| `fatherOffice` | `Father_Office` | nvarchar | 20 | No | Office phone |
| `fatherCell` | `Father_Cell` | nvarchar | 20 | No | Mobile phone |
| `fatherHome` | `Father_Home` | nvarchar | 20 | No | Home phone |
| `fatherEmail` | `Father_Email` | nvarchar | 100 | No | Email address |

#### Address Fields

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `address1` | `Address1` | nvarchar | 50 | No | Street address line 1 |
| `address2` | `Address2` | nvarchar | 50 | No | Street address line 2 |
| `code` | `Code` | nvarchar | 10 | No | Postal code |

#### Enrollment Fields

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `schoolName` | `School_Name` | nvarchar | 50 | No | School name |
| `attendingKcowAt` | `Attending_KCOW_at` | nvarchar | 50 | No | KCOW location attending |
| `aftercare` | `Aftercare` | nvarchar | 50 | No | Aftercare program name |
| `extra` | `Extra` | nvarchar | 50 | No | Extra activities |
| `classGroup` | `Class_Group` | nvarchar | 10 | No | Assigned class group code |
| `grade` | `Grade` | nvarchar | 5 | No | Current grade |
| `teacher` | `Teacher` | nvarchar | 50 | No | Teacher name |
| `homeTime` | `Home_Time` | datetime | - | No | Home time |
| `startClasses` | `Start_Classes` | datetime | - | No | Class start date |
| `terms` | `Terms` | nvarchar | 10 | No | Terms enrolled |
| `seat` | `Seat` | nvarchar | 5 | No | Assigned seat |
| `truck` | `Truck` | nvarchar | 3 | No | Assigned truck |
| `family` | `Family` | nvarchar | 50 | No | Family grouping code |
| `sequence` | `Sequence` | nvarchar | 50 | No | Display sequence |

#### Financial Fields

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `financialCode` | `Financial_Code` | nvarchar | 10 | No | Financial status code |
| `charge` | `Charge` | money | - | No | Amount charged |
| `deposit` | `Deposit` | nvarchar | 50 | No | Deposit info |
| `payDate` | `PayDate` | nvarchar | 50 | No | Payment date |

#### T-Shirt Orders (Set 1)

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `tshirtCode` | `Tshirt_Code` | nvarchar | 5 | Yes | T-shirt order code |
| `tshirtMoney1` | `Tshirt_Money_1` | nvarchar | 50 | No | Payment amount |
| `tshirtMoneyDate1` | `Tshirt_MoneyDate_1` | datetime | - | No | Payment date |
| `tshirtReceived1` | `Tshirt_Received_1` | nvarchar | 50 | No | Received status |
| `tshirtRecDate1` | `Tshirt_RecDate_1` | datetime | - | No | Receive date |
| `receiveNote1` | `Receive_Note_1` | nvarchar | 50 | No | Receive notes |
| `tshirtColor1` | `TshirtColor1` | nvarchar | 20 | No | Color choice |
| `tshirtDesign1` | `TshirtDesign1` | nvarchar | 20 | No | Design choice |
| `tshirtSize1` | `TshirtSize1` | nvarchar | 10 | No | Size choice |

#### T-Shirt Orders (Set 2)

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `tshirtMoney2` | `Tshirt_Money_2` | nvarchar | 50 | No | Payment amount |
| `tshirtMoneyDate2` | `Tshirt_MoneyDate_2` | datetime | - | No | Payment date |
| `tshirtReceived2` | `Tshirt_Received_2` | nvarchar | 50 | No | Received status |
| `tshirtRecDate2` | `Tshirt_RecDate_2` | datetime | - | No | Receive date |
| `receiveNote2` | `Receive_Note_2` | nvarchar | 50 | No | Receive notes |
| `tshirtColor2` | `TshirtColor2` | nvarchar | 20 | No | Color choice |
| `tshirtDesign2` | `TshirtDesign2` | nvarchar | 20 | No | Design choice |
| `tshirtSize2` | `TshirtSize2` | nvarchar | 10 | No | Size choice |

#### Status & Tracking Fields

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `indicator1` | `Indicator_1` | nvarchar | 3 | No | Status indicator 1 |
| `indicator2` | `Indicator_2` | nvarchar | 3 | No | Status indicator 2 |
| `generalNote` | `General_Note` | nvarchar | 255 | No | General notes |
| `printIdCard` | `Print_Id_Card` | bit | - | Yes | Print ID card flag |
| `acceptTermsCond` | `AcceptTermsCond` | nvarchar | 50 | No | Terms acceptance status |
| `schoolClose` | `SchoolClose` | datetime | - | No | School closure date |
| `cnt` | `Cnt` | float | - | No | Counter/count value |
| `status` | `Status` | nvarchar | 20 | No | Current status |
| `created` | `Created` | datetime | - | No | Record creation date |
| `submitted` | `Submitted` | datetime | - | No | Submission date |
| `updated` | `Updated` | datetime | - | No | Last update date |
| `onlineEntry` | `OnlineEntry` | int | - | No | Online entry ID |
| `smsOrEmail` | `SmsOrEmail` | nvarchar | 10 | No | Contact preference |
| `bookEmail` | `BookEmail` | nvarchar | 100 | No | Booking email |
| `social` | `Social` | nvarchar | 50 | No | Social media info |

#### Reports & Certificates

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `report1GivenOut` | `Report1GivenOut` | nvarchar | 50 | No | Report 1 distribution status |
| `report2GivenOut` | `Report2GivenOut` | nvarchar | 50 | No | Report 2 distribution status |
| `accountGivenOut` | `AccountGivenOut` | nvarchar | 50 | No | Account given out status |
| `certificatePrinted` | `CertificatePrinted` | nvarchar | 50 | No | Certificate print status |
| `activityReportGivenOut` | `ActivityReportGivenOut` | nvarchar | 50 | No | Activity report status |

#### Photo Attachment

| Field | Legacy Name | Type | Max Length | Required | Description |
|-------|-------------|------|------------|----------|-------------|
| `photo` | `Photo` | attachment | - | No | Photo attachment (complex type) |
| `photoUpdated` | `PhotoUpdated` | datetime | - | No | Photo update date |

### TypeScript Interface (Proposed - Core Fields)

```typescript
interface Child {
  id: number;
  reference: string;
  
  // Identity
  firstName?: string;
  lastName?: string;
  dateOfBirth?: Date;
  gender?: 'M' | 'F';
  language?: 'Afrikaans' | 'English';
  
  // Account Person (Responsible Adult)
  accountPerson?: {
    firstName?: string;
    lastName?: string;
    idNumber?: string;
    cellphone?: string;
    officePhone?: string;
    homePhone?: string;
    email?: string;
    relationship?: string;
  };
  
  // Parents
  mother?: {
    firstName?: string;
    lastName?: string;
    officePhone?: string;
    cellphone?: string;
    homePhone?: string;
    email?: string;
  };
  
  father?: {
    firstName?: string;
    lastName?: string;
    officePhone?: string;
    cellphone?: string;
    homePhone?: string;
    email?: string;
  };
  
  // Address
  address?: {
    line1?: string;
    line2?: string;
    postalCode?: string;
  };
  
  // Enrollment
  enrollment?: {
    schoolName?: string;
    attendingKcowAt?: string;
    aftercare?: string;
    classGroupCode?: string;
    grade?: string;
    teacher?: string;
    startDate?: Date;
    terms?: string;
    seat?: string;
    truck?: string;
    family?: string;
  };
  
  // Financial
  financial?: {
    code?: string;
    charge?: number;
    deposit?: string;
    payDate?: string;
  };
  
  // Status
  status?: string;
  createdAt?: Date;
  submittedAt?: Date;
  updatedAt?: Date;
  
  // Flags
  printIdCard: boolean;
  acceptedTerms?: string;
  contactPreference?: 'SMS' | 'Email';
}
```

### Business Rules

- A child is assigned to exactly **one** class group (`classGroup` field)
- Contains sensitive personal data - requires POPIA compliance
- Bilingual support for Afrikaans/English
- T-shirt ordering system is built-in (may be separated in v2)

### Notes

- Full schema in `docs/legacy/4_Children/Children.xsd` (92 fields)
- Photo stored as Access attachment (complex type with multiple sub-fields)

---

## Entity Relationships

### ER Diagram (Conceptual)

```
┌─────────────────┐           ┌──────────────────┐
│     School      │ 1       N │    Class_Group   │
│─────────────────│───────────│──────────────────│
│ schoolId (PK)   │           │ classGroup       │
│ shortName       │           │ schoolId (FK)    │
│ description     │           │ dayTruck         │
│ truckId         │           │ startTime        │
│ visitDay        │           │ endTime          │
│ price           │           │ evaluate         │
│ ...             │           │ ...              │
└─────────────────┘           └────────┬─────────┘
                                       │
                                       │ 1
                                       ▼
┌─────────────────┐           ┌──────────────────┐
│    Activity     │ 1       N │     Children     │
│─────────────────│◄──────────│──────────────────│
│ activityId (PK) │ (implicit)│ childrenId (PK)  │
│ programCode     │           │ reference        │
│ programName     │           │ childName        │
│ grade           │           │ classGroup (FK)  │
│ ...             │           │ ...              │
└─────────────────┘           └──────────────────┘
```

### Relationship Rules

1. **School → Class_Group**: One-to-Many
   - A school hosts multiple class sessions
   - Each class group belongs to exactly one school

2. **Class_Group → Children**: One-to-Many
   - A child is assigned to exactly one class group
   - Each class group has multiple students

3. **Activity → Class_Group**: One-to-Many (implicit)
   - Activities are delivered during class sessions
   - Tracked via scheduling, not explicit FK

---

## Afrikaans to English Field Name Translations

| Afrikaans | English | Context |
|-----------|---------|---------|
| Trok | Truck | Mobile computer literacy truck |
| Taal | Language | Language preference (Afr/Eng) |
| Kluis | Safe/Vault | Storage access notes |
| Naskool | Afterschool | Afterschool care program |
| omsendbriewe | Circulars | Circular letters/emails |
| Klas Groep | Class Group | Scheduled session |
| Kinders | Children | Students |
| Seun | Boy | Male gender |
| Dogter | Girl | Female gender |
| Graad | Grade | School grade level |
| Onderwyser | Teacher | Teacher |
| Skool | School | School |
| Datum | Date | Date |
| Tyd | Time | Time |

---

## Migration Considerations

### From Access to SQLite/SQL Server

| Access Type | SQLite Type | SQL Server Type | Notes |
|-------------|-------------|-----------------|-------|
| `nvarchar(n)` | `TEXT` | `NVARCHAR(n)` | |
| `int` | `INTEGER` | `INT` | |
| `smallint` | `INTEGER` | `SMALLINT` | |
| `tinyint` | `INTEGER` | `TINYINT` | |
| `money` | `REAL` | `MONEY` | |
| `bit` | `INTEGER` (0/1) | `BIT` | |
| `ntext` | `TEXT` | `NVARCHAR(MAX)` | |
| `image` | `BLOB` | `VARBINARY(MAX)` | |
| `hyperlink` | `TEXT` | `NVARCHAR(MAX)` | Parse for URL |
| `attachment` | Custom | Custom | Complex type |
| `datetime` | `TEXT` (ISO) | `DATETIME2` | |

### Naming Convention Updates

| Legacy Field | Proposed Name | Entity |
|--------------|---------------|--------|
| `School_Id` | `schoolId` | School |
| `Short_School` | `shortName` | School |
| `E-mail_adress` | `email` | School |
| `Trok` | `truckId` | School |
| `Taal` | `language` | School |
| `Naskool1_Name` | `afterschool1Name` | School |
| `Kluis` | `safeNotes` | School |
| `omsendbriewe` | `circularsEmail` | School |
| `DayTruck` | `dayTruck` | ClassGroup |
| `Money_Message` | `moneyMessage` | ClassGroup |
| `Educational_Focus` | `educationalFocus` | Activity |
| `Child_Name` | `firstName` | Children |
| `Account_Person_*` | `accountPerson*` | Children |

---

## API Endpoints (Planned)

Based on domain model, expected REST endpoints:

```
Schools
  GET    /api/v1/schools           List all schools
  GET    /api/v1/schools/:id       Get school by ID
  POST   /api/v1/schools           Create school
  PUT    /api/v1/schools/:id       Update school
  DELETE /api/v1/schools/:id       Delete school

Class Groups
  GET    /api/v1/class-groups                   List all groups
  GET    /api/v1/schools/:id/class-groups       Groups by school
  POST   /api/v1/class-groups                   Create group
  PUT    /api/v1/class-groups/:id               Update group

Activities
  GET    /api/v1/activities        List all activities
  GET    /api/v1/activities/:id    Get activity
  POST   /api/v1/activities        Create activity

Students (Children)
  GET    /api/v1/students                       List students
  GET    /api/v1/students/:id                   Get student
  GET    /api/v1/class-groups/:id/students      Students in group
  POST   /api/v1/students                       Enroll student
  PUT    /api/v1/students/:id                   Update student
```

---

## Legacy Files Reference

| File | Description |
|------|-------------|
| `docs/legacy/1_School/School.xsd` | School schema definition (30 fields) |
| `docs/legacy/1_School/School.xml` | School sample data |
| `docs/legacy/3_Activity/Activity.xsd` | Activity schema definition (7 fields) |
| `docs/legacy/3_Activity/Activity.xml` | Activity sample data |
| `docs/legacy/2_Class_Group/Class Group.xsd` | Class group schema (15 fields) |
| `docs/legacy/2_Class_Group/Class Group.xml` | Class group sample data |
| `docs/legacy/4_Children/Children.xsd` | Student schema (92 fields) |
| `docs/legacy/4_Children/Children.xml` | Student sample data |
| `docs/legacy/4_Children/1_Child_Information.png` | Legacy UI - Child info tab |
| `docs/legacy/4_Children/2_Child_Financial.png` | Legacy UI - Financial tab |
| `docs/legacy/4_Children/3_Class_Group.png` | Legacy UI - Class group tab |
| `docs/legacy/4_Children/4_Class_Group_Attendance.png` | Legacy UI - Attendance tab |
| `docs/legacy/4_Children/5_Child_Evaluation.png` | Legacy UI - Evaluation tab |
| `docs/legacy/4_Children/6_Class_Groups_Evaluation.png` | Legacy UI - Group evaluation tab |
| `docs/legacy/kcow_logo.png` | KCOW logo (PNG) |
| `docs/legacy/kcow_logo.svg` | KCOW logo (SVG) |

---

## Revision History

| Date | Change |
|------|--------|
| 2025-12-27 | Initial creation with partial field mapping |
| 2025-12-27 | Complete XSD field mapping - all 144 fields documented |
| 2025-12-27 | Added Afrikaans to English translations |
| 2025-12-27 | Added Children schema complete field listing (92 fields) |
| 2026-01-03 | Corrected Class_Group↔Children relationship: one-to-many (child belongs to one class group) |
| 2026-01-03 | Updated legacy file paths to reflect reorganized folder structure |
| 2026-01-03 | Added Legacy UI Reference section with tab screenshots for Children entity |
| 2026-01-03 | Updated School entity fields table with bilingual field names (English and Afrikaans display names) |
