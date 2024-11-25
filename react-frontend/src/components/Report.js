import React, { useState } from "react";
import stateData from "../utils/stateData";

const InteractiveMap = () => {
  const [selectedState, setSelectedState] = useState(null);

  const handleStateClick = (stateName) => {
    setSelectedState(stateName);
  };

  const displayStateData = (stateName) => {
    const data = stateData[stateName];
    if (data) {
      return (
        <div>
          <h4 className="text-lg font-bold">{stateName}</h4>
          <p>
            <strong>Population:</strong> {data.population}
          </p>
          <p>
            <strong>Area:</strong> {data.area}
          </p>
          <p>
            <strong>Capital:</strong> {data.capital}
          </p>
        </div>
      );
    } else {
      return <p>No data available for this state.</p>;
    }
  };

  return (
    <div className="font-sans h-full p-5">
      <div className="flex h-full justify-between items-start m-0">
        <div
          id="india-map-container"
          className="flex-1 h-full max-w-full mx-auto"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            xmlnsXlink="http://www.w3.org/1999/xlink"
            preserveAspectRatio="xMidYMid meet"
            viewBox="0 0 2500 2843"
            version="1.1"
            className="h-full max-h-full max-w-[60%] m-auto block"
          >
            <g id="surface1">
              {Object.keys(stateData).map((stateName) => (
                <path
                  key={stateName}
                  className="state cursor-pointer fill-gray-300 transition-colors duration-300"
                  id={stateName}
                  d={stateData[stateName].path}
                  onClick={() => handleStateClick(stateName)}
                  style={{
                    fill: selectedState === stateName ? "#FF5722" : "#B0BEC5",
                  }}
                />
              ))}
            </g>
          </svg>
        </div>
        <div
          id="stateData"
          className=" ml-5 p-4 border border-gray-300 rounded-lg bg-gray-100 shadow-md w-fit absolute right-2"
        >
          <h3 className="text-xl font-bold">State Data</h3>
          <div id="stateDetails" className="mt-4 text-base text-gray-700">
            {selectedState ? (
              displayStateData(selectedState)
            ) : (
              <p>Click on a state to view details.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default InteractiveMap;
