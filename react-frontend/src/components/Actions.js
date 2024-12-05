import React from "react";

const Actions = ({ onFetchData, selectedDate, setSelectedDate }) => {
  const handleDateChange = (e) => {
    console.log("Updating the date: ", e.target.value);
    setSelectedDate(e.target.value);
  };

  return (
    <div className="flex gap-4 items-center justify-between flex-wrap p-2 bg-white rounded-xl shadow-sm">
      <input
        type="date"
        id="referenceDate"
        className="p-2 px-4 border border-gray-200 rounded-lg outline-none transition-colors duration-200 w-full md:w-fit"
        value={selectedDate}
        onChange={handleDateChange}
      />
      <button
        onClick={() => onFetchData(selectedDate)}
        className="px-4 py-2 bg-[#f26114] text-white border-none rounded-md cursor-pointer text-sm w-full md:w-fit"
      >
        Fetch Data
      </button>
    </div>
  );
};

export default Actions;
