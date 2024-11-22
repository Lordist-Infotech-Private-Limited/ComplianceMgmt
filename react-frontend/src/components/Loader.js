import React from "react";

const Loader = () => {
  return (
    <div className="absolute top-0 left-0 w-full h-full bg-gray-500 bg-opacity-80 flex justify-center items-center z-10">
      <div className="border-4 border-gray-200 border-t-blue-500 rounded-full w-10 h-10 animate-spin"></div>
    </div>
  );
};

export default Loader;
