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

  function fitPage(args) {
    var reportviewerObj = $("#reportviewer-container").data("boldReportViewer");
    reportviewerObj.fitToPage(); // To fit the report page.
  }
  var parameters = [{ name: "StateId", values: [1], nullable: false }];
  return (
    <div className="p-4 flex flex-col flex-1">
      <h2 className="text-2xl font-bold mb-4">Compliance Reports</h2>
      <BoldReportViewerComponent
        id="reportviewer-container"
        reportServiceUrl={"https://adfapi.lordist.in/ReportViewer"}
        reportPath={"StateReport.rdl"}
        viewReportClick={viewReportClick}
        renderingComplete={fitPage}
        parameters={parameters}
      ></BoldReportViewerComponent>
    </div>
  );
};

export default ComplianceReports;
