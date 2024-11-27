import React, { useState } from "react";
import stateData from "../utils/stateData";
import { Tooltip as ReactTooltip } from "react-tooltip";
import "react-tooltip/dist/react-tooltip.css";
import { read, writeFile } from "xlsx";
import "@fortawesome/fontawesome-free/css/all.css";

const InteractiveMap = () => {
  const [filter, setFilter] = useState("disbursement");

  const handleFilterChange = (e) => {
    setFilter(e.target.value);
  };

  const downloadReport = async () => {
    try {
      // Fetch the existing Excel file from the public folder
      const response = await fetch("loanAndAdvances.xlsx");
      const arrayBuffer = await response.arrayBuffer();

      // Convert the ArrayBuffer to a workbook
      const workbook = read(new Uint8Array(arrayBuffer), {
        type: "array",
      });
      const worksheet = workbook.Sheets["Sheet1"];

      // Update the worksheet with the desired data
      // Example: Update cell D9 with a new value
      worksheet["D9"] = { v: 100, t: "n" };

      // Generate a new Excel file
      writeFile(workbook, "loanAndAdvances_updated.xlsx");
      console.log("Excel file created successfully!");
    } catch (error) {
      console.error("Error downloading report:", error);
    }
  };

  return (
    <div className="font-sans h-full p-5">
      <div className="flex h-full justify-between items-start m-0">
        <div className="w-full max-w-full mx-auto">
          <div className="flex justify-between items-center">
            <div className="mb-4">
              <label htmlFor="filter" className="mr-2">
                Filter by:
              </label>
              <select
                id="filter"
                value={filter}
                onChange={handleFilterChange}
                className="p-2 border border-gray-300 rounded-lg"
              >
                <option value="disbursement">Disbursement</option>
                <option value="sanction">Sanction</option>
                <option value="outstanding">Outstanding</option>
                <option value="npa">NPA</option>
              </select>
            </div>
            <button
              id="downloadBtn"
              data-tooltip-id="my-tooltip"
              data-tooltip-content="Download"
              onClick={downloadReport}
              className="bg-green-500 border-none text-sm text-white cursor-pointer rounded-md px-4 py-2"
            >
              <i className="fas fa-download"></i>
            </button>
          </div>
          <div id="india-map-container" className="h-full max-w-full mx-auto">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              xmlnsXlink="http://www.w3.org/1999/xlink"
              preserveAspectRatio="xMidYMid meet"
              viewBox="0 0 2500 2843"
              version="1.1"
              className="h-full max-h-full block"
            >
              <g id="surface1">
                {Object.keys(stateData).map((stateName) => {
                  const data = stateData[stateName];
                  return (
                    <path
                      data-tooltip-id="my-tooltip"
                      data-tooltip-content={`${filter}: ${data[filter]}`}
                      key={stateName}
                      className="state cursor-pointer transition-colors duration-300"
                      id={stateName}
                      d={data.path}
                      style={{
                        fill: data.color,
                      }}
                    />
                  );
                })}
              </g>
            </svg>
          </div>
        </div>
        <div
          id="stateData"
          className="ml-5 p-4 border border-gray-300 rounded-lg bg-gray-100 shadow-md w-fit"
        >
          <h3 className="text-xl font-bold">Map Index</h3>
          <div id="stateDetails" className="mt-4 text-base text-gray-700">
            {Object.keys(stateData).map((stateName) => {
              const data = stateData[stateName];
              return (
                <div key={stateName} className="flex items-center mb-2">
                  <div
                    className="w-4 h-4 mr-2"
                    style={{ backgroundColor: data.color }}
                  ></div>
                  <span>
                    {stateName}: {data[filter]}
                  </span>
                </div>
              );
            })}
          </div>
        </div>
      </div>
      <ReactTooltip id="my-tooltip" />
    </div>
  );
};

export default InteractiveMap;
