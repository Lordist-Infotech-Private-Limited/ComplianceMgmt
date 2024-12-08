export const borrowerFields = [
  { name: "RowNo", type: "number", disabled: true },
  { name: "Date", type: "date", disabled: true },
  { name: "BankId", type: "number", disabled: true },
  { name: "Cin", type: "text" },
  { name: "BName", type: "text" },
  { name: "BDob", type: "date" },
  { name: "sbcitizenship", type: "text" },
  { name: "BPanNo", type: "text" },
  { name: "Aadhaar", type: "text" },
  { name: "IdType", type: "text" },
  { name: "IdNumber", type: "text" },
  { name: "BMonthlyIncome", type: "number" },
  { name: "BReligion", type: "text" },
  { name: "BCast", type: "text" },
  { name: "BGender", type: "text" },
  { name: "BOccupation", type: "text" },
  { name: "IsValidated", type: "checkbox" },
];

export const borrowerLoanFields = [
  { name: "RowNo", type: "number", disabled: true },
  { name: "Date", type: "date", disabled: true },
  { name: "BankId", type: "number", disabled: true },
  { name: "Cin", type: "text", disabled: true },
  { name: "BLoanNo", type: "text", disabled: true },
  { name: "LoanType", type: "text" },
  { name: "LoanPurpose", type: "text" },
  { name: "SanctAmount", type: "number" },
  { name: "SanctDate", type: "date" },
  { name: "DwellingUnit", type: "text" },
  { name: "MoratoriumPeriod", type: "number" },
  { name: "LoanTenCont", type: "number" },
  { name: "LoanTenResidual", type: "number" },
  { name: "Roi", type: "number" },
  { name: "IntType", type: "text" },
  { name: "Emi", type: "number" },
  { name: "PreEmi", type: "number" },
  { name: "FirstDisbDate", type: "date" },
  { name: "EmiStartDate", type: "date" },
  { name: "PreEmiStartDate", type: "date" },
  { name: "LoanDisbDuringMonth", type: "number" },
  { name: "CummuLoanDisb", type: "number" },
  { name: "LoanStatus", type: "text" },
  { name: "AmtUnderCons", type: "number" },
  { name: "PartyName", type: "text" },
  { name: "MortGuarantee", type: "text" },
  { name: "AmoutUnderGuar", type: "number" },
  { name: "TotalLoanOut", type: "number" },
  { name: "POut", type: "number" },
  { name: "IOut", type: "number" },
  { name: "OtherDueOut", type: "number" },
  { name: "LoanDsbp", type: "number" },
  { name: "LoanRepayDurMth", type: "number" },
  { name: "TotalLoanOverDue", type: "number" },
  { name: "POverDue", type: "number" },
  { name: "IOverDue", type: "number" },
  { name: "OtherOverDue", type: "number" },
  { name: "AccntClosedDurMth", type: "text" },
  { name: "AssetCat", type: "text" },
  { name: "Ecl", type: "text" },
  { name: "ClassDate", type: "date" },
  { name: "Pd", type: "number" },
  { name: "Lgd", type: "number" },
  { name: "ProvAmt", type: "number" },
  { name: "Rw", type: "text" },
  { name: "RefFromNhb", type: "text" },
  { name: "UnderPmayClss", type: "text" },
  { name: "SerfaseiAct", type: "text" },
  { name: "AmtClaimUnderNotice", type: "number" },
  { name: "AmtRecovered", type: "number" },
  { name: "StayGranted", type: "text" },
  { name: "IsValidated", type: "checkbox" },
  { name: "RejectedReason", type: "text" },
  { name: "ValidatedDate", type: "datetime" },
];

export const borrowerMortgageFields = [
  { name: "RowNo", type: "number", disabled: true },
  { name: "Date", type: "date", disabled: true },
  { name: "BankId", type: "number", disabled: true },
  { name: "Cin", type: "text", disabled: true },
  { name: "BLoanNo", type: "text", disabled: true },
  { name: "PropType", type: "text", disabled: true },
  { name: "PropAdd", type: "text" },
  { name: "LandArea", type: "number" },
  { name: "BuildingArea", type: "number" },
  { name: "TownName", type: "text" },
  { name: "District", type: "text" },
  { name: "State", type: "text" },
  { name: "Pin", type: "number" },
  { name: "RuralUrban", type: "text" },
  { name: "PropValAtSanct", type: "number" },
  { name: "PresentValue", type: "number" },
  { name: "Insurance", type: "text" },
  { name: "IsValidated", type: "checkbox" },
  { name: "RejectedReason", type: "text" },
  { name: "ValidatedDate", type: "datetime" },
];

export const borrowerMortgageOtherFields = [
  { name: "RowNo", type: "number", disabled: true },
  { name: "Date", type: "date", disabled: true },
  { name: "BankId", type: "number", disabled: true },
  { name: "Cin", type: "text", disabled: true },
  { name: "BLoanNo", type: "text", disabled: true },
  { name: "CollType", type: "text", disabled: true },
  { name: "ValueAtSanct", type: "number" },
  { name: "PresentValue", type: "number" },
  { name: "IsValidated", type: "checkbox" },
  { name: "RejectedReason", type: "text" },
  { name: "ValidatedDate", type: "datetime" },
];

export const coBorrowerFields = [
  { name: "RowNo", type: "number", disabled: true },
  { name: "Date", type: "date", disabled: true },
  { name: "BankId", type: "number", disabled: true },
  { name: "Cin", type: "text" },
  { name: "IdType", type: "text" },
  { name: "IdNumber", type: "text" },
  { name: "CbCin", type: "text" },
  { name: "CbName", type: "text" },
  { name: "CbDob", type: "date" },
  { name: "CbCitizenship", type: "text" },
  { name: "CbPanNo", type: "text" },
  { name: "CbAadhaar", type: "text" },
  { name: "CbMonthlyIncome", type: "number" },
  { name: "CbReligion", type: "text" },
  { name: "CbCast", type: "text" },
  { name: "CbGender", type: "text" },
  { name: "CbOccupation", type: "text" },
  { name: "IsValidated", type: "checkbox" },
];
