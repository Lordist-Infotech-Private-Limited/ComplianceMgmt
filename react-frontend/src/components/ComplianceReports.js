/* eslint-disable */

import React from "react";

const ComplianceReports = () => {
  return (
    <div className="p-4 flex flex-col flex-1">
      <h2 className="text-2xl font-bold mb-4">Compliance Reports</h2>
      <BoldReportViewerComponent
        id="reportviewer-container"
        reportServiceUrl={"/ReportViewer"}
        reportPath={"StateReport.rdlc"}
      ></BoldReportViewerComponent>
    </div>
  );
};

export default ComplianceReports;