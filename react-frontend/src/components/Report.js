import React, { useState } from "react";
import stateData from "../utils/stateData";
import { Tooltip as ReactTooltip } from "react-tooltip";
import "react-tooltip/dist/react-tooltip.css";

const InteractiveMap = () => {
  const [filter, setFilter] = useState("disbursement");

  const handleFilterChange = (e) => {
    setFilter(e.target.value);
  };

  return (
    <div className="font-sans h-full p-5">
      <div className="flex h-full justify-between items-start m-0">
        <div className="w-full max-w-full mx-auto">
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
