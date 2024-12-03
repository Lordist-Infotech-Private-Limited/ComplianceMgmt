/* eslint-disable */

import React from "react";

const ComplianceReports = () => {
  function viewReportClick(event) {
    let reportParams = [];
    reportParams.push({
      name: "ReportParameter1",
      lables: ["SO50756"],
      value: ["SO550756"],
    });
    event.model.parameters = reportParams;
  }
  return (
    <div className="p-4 flex flex-col flex-1">
      <h2 className="text-2xl font-bold mb-4">Compliance Reports</h2>
      <BoldReportViewerComponent
        id="reportviewer-container"
        reportServiceUrl={"/ReportViewer"}
        reportPath={"StateReport.rdlc"}
        viewReportClick={viewReportClick}
      ></BoldReportViewerComponent>
    </div>
  );
};

export default ComplianceReports;
