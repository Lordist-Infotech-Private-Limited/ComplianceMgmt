import React from "react";

const ComplianceReports = () => {
  const data = [
    {
      srNo: 1,
      particulars: "Housing Loans",
      fromAprilToPreviousMonth: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      duringTheMonthUnderReport: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      cumulativeSanctions: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      itemCode: "LS101",
      validation: ">=0, & = LS102+LS106+LS110",
    },
    {
      srNo: 1.1,
      particulars: "Individuals",
      fromAprilToPreviousMonth: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      duringTheMonthUnderReport: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      cumulativeSanctions: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      itemCode: "LS102",
      validation: ">=0, & = LS103+LS104+LS105",
    },
    {
      srNo: "(i)",
      particulars: "for construction/purchase of new units",
      itemCode: "LS103",
      validation: ">=0",
    },
    {
      srNo: "(a)",
      particulars:
        "Out of the 1.1(i) above, loans granted for purchase of units from builders (under construction + completed)",
      itemCode: "LS103A",
      validation: ">=0",
    },
    {
      srNo: "(b)",
      particulars:
        "Out of the 1.1(i) above, loans granted for Plot + Construction",
      itemCode: "LS103B",
      validation: ">=0",
    },
    {
      srNo: "(ii)",
      particulars: "for purchasing old units (Resale)",
      itemCode: "LS104",
      validation: ">=0",
    },
    {
      srNo: "(iii)",
      particulars: "for repair & renovation of existing units",
      itemCode: "LS105",
      validation: ">=0",
    },
    {
      srNo: 1.2,
      particulars: "Builders and Corporates",
      fromAprilToPreviousMonth: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      duringTheMonthUnderReport: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      cumulativeSanctions: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      itemCode: "LS106",
      validation: ">=0 & = LS107",
    },
    {
      srNo: "(i)",
      particulars: "Residential Projects",
      itemCode: "LS107",
      validation: ">=0",
    },
    {
      srNo: "(a)",
      particulars:
        "Out of the 1.2(i) above, Loans for Slum Rehabilitation Authority (SRA)",
      itemCode: "LS107A",
      validation: ">=0",
    },
    {
      srNo: 1.3,
      particulars: "Housing Loans to others (excluding 1.1 and 1.2 above)",
      itemCode: "LS110",
      validation: ">=0",
    },
    {
      srNo: 2,
      particulars: "Non-Housing Loans",
      fromAprilToPreviousMonth: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      duringTheMonthUnderReport: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      cumulativeSanctions: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      itemCode: "LS109",
      validation: ">=0, & = LS110+LS114+LS120",
    },
    {
      srNo: 2.1,
      particulars: "Individuals",
      fromAprilToPreviousMonth: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      duringTheMonthUnderReport: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      cumulativeSanctions: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      itemCode: "LS110",
      validation: ">=0, & = LS111+LS112+LS113",
    },
    {
      srNo: "(i)",
      particulars: "for mortgage/property/home equity loans/LAP",
      itemCode: "LS111",
      validation: ">=0",
    },
    {
      srNo: "(ii)",
      particulars: "Lease Rental Discounting",
      itemCode: "LS112",
      validation: ">=0",
    },
    {
      srNo: "(iii)",
      particulars: "Others",
      itemCode: "LS113",
      validation: ">=0",
    },
    {
      srNo: "(a)",
      particulars:
        "Of (iii) above loan against the security of shares/ debentures / bonds",
      itemCode: "LS113A",
      validation: ">=0",
    },
    {
      srNo: "(b)",
      particulars: "Of (iii) above loans against security of gold jewellery",
      itemCode: "LS113B",
      validation: "",
    },
    {
      srNo: 2.2,
      particulars: "Builders and Corporates",
      fromAprilToPreviousMonth: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      duringTheMonthUnderReport: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      cumulativeSanctions: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      itemCode: "LS114",
      validation: ">=0, & = LS115+LS116+LS117",
    },
    {
      srNo: "(i)",
      particulars: "Non-residential projects",
      itemCode: "LS115",
      validation: ">=0",
    },
    {
      srNo: "(ii)",
      particulars: "Lease Rental Discounting",
      itemCode: "LS116",
      validation: ">=0",
    },
    {
      srNo: "(iii)",
      particulars: "Others",
      itemCode: "LS117",
      validation: ">=0",
    },
    {
      srNo: "(a)",
      particulars:
        "Of (iii) above loan against the security of shares/ debentures / bonds",
      itemCode: "LS117A",
      validation: ">=0",
    },
    {
      srNo: 2.3,
      particulars: "Non-housing Loans to others (excluding 2.1 and 2.2 above)",
      itemCode: "LS120",
      validation: ">=0",
    },
    {
      srNo: 3,
      particulars: "Total",
      fromAprilToPreviousMonth: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      duringTheMonthUnderReport: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      cumulativeSanctions: { noOfAcs: 0, grossCarryingAmount: "0.00" },
      itemCode: "LS100",
      validation: ">=0, & = LS101+LS109",
    },
  ];

  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold mb-4">Compliance Reports</h2>
      <table className="min-w-full bg-white border border-gray-300">
        <thead>
          <tr>
            <th className="py-2 px-4 border-b" rowSpan="2">
              Sr. No.
            </th>
            <th className="py-2 px-4 border-b" rowSpan="2">
              Particulars
            </th>
            <th className="py-2 px-4 border-b" colSpan="2">
              From April to previous month
            </th>
            <th className="py-2 px-4 border-b" colSpan="2">
              During the month under report
            </th>
            <th className="py-2 px-4 border-b" colSpan="2">
              Cumulative sanctions from April to reporting date
            </th>
            <th className="py-2 px-4 border-b" rowSpan="2">
              Item Code
            </th>
            <th className="py-2 px-4 border-b" rowSpan="2">
              Validation
            </th>
          </tr>
          <tr>
            <th className="py-2 px-4 border-b">No. of A/cs</th>
            <th className="py-2 px-4 border-b">
              Gross Carrying Amount (Rs. In Lakhs)
            </th>
            <th className="py-2 px-4 border-b">No. of A/cs</th>
            <th className="py-2 px-4 border-b">
              Gross Carrying Amount (Rs. In Lakhs)
            </th>
            <th className="py-2 px-4 border-b">No. of A/cs</th>
            <th className="py-2 px-4 border-b">
              Gross Carrying Amount (Rs. In Lakhs)
            </th>
          </tr>
        </thead>
        <tbody>
          {data.map((row, index) => (
            <tr key={index}>
              <td className="py-2 px-4 border-b">{row.srNo}</td>
              <td className="py-2 px-4 border-b">{row.particulars}</td>
              <td className="py-2 px-4 border-b">
                {row.fromAprilToPreviousMonth
                  ? row.fromAprilToPreviousMonth.noOfAcs
                  : ""}
              </td>
              <td className="py-2 px-4 border-b">
                {row.fromAprilToPreviousMonth
                  ? row.fromAprilToPreviousMonth.grossCarryingAmount
                  : ""}
              </td>
              <td className="py-2 px-4 border-b">
                {row.duringTheMonthUnderReport
                  ? row.duringTheMonthUnderReport.noOfAcs
                  : ""}
              </td>
              <td className="py-2 px-4 border-b">
                {row.duringTheMonthUnderReport
                  ? row.duringTheMonthUnderReport.grossCarryingAmount
                  : ""}
              </td>
              <td className="py-2 px-4 border-b">
                {row.cumulativeSanctions ? row.cumulativeSanctions.noOfAcs : ""}
              </td>
              <td className="py-2 px-4 border-b">
                {row.cumulativeSanctions
                  ? row.cumulativeSanctions.grossCarryingAmount
                  : ""}
              </td>
              <td className="py-2 px-4 border-b">{row.itemCode}</td>
              <td className="py-2 px-4 border-b">{row.validation}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ComplianceReports;
