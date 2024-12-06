/* eslint-disable */

import React from "react";
import Header from "./Header";

const ComplianceReports = ({ user, onLogout }) => {
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
    <>
      <Header
        componentName={"Compliance Reports"}
        user={user}
        onLogout={onLogout}
      />
      <div className="p-4 flex flex-col flex-1">
        <BoldReportViewerComponent
          id="reportviewer-container"
          reportServiceUrl={"https://adfapi.lordist.in/ReportViewer"}
          reportPath={"StateReport.rdl"}
          viewReportClick={viewReportClick}
          renderingComplete={fitPage}
          parameters={parameters}
        ></BoldReportViewerComponent>
      </div>
    </>
  );
};

export default ComplianceReports;
