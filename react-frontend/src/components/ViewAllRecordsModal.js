import React, { useState, useEffect } from "react";
import { utils, writeFileXLSX } from "xlsx";
import "@fortawesome/fontawesome-free/css/all.css";
import { saveSingleRecord } from "../utils/service";
import { Tooltip as ReactTooltip } from "react-tooltip";
import "react-tooltip/dist/react-tooltip.css";
import ReactPaginate from "react-paginate";
import { borrowerFields, coBorrowerFields } from "../utils/tables";

const ViewAllRecordsModal = ({
  records,
  tableName,
  onClose,
  showLoader,
  hideLoader,
}) => {
  const [allRecords, setAllRecords] = useState([]);
  const [currentPage, setCurrentPage] = useState(0);
  const [searchTerm, setSearchTerm] = useState("");
  const [originalRecords, setOriginalRecords] = useState([]);
  const [recordsPerPage, setRecordsPerPage] = useState(10); // Default page size

  useEffect(() => {
    console.log("records", records);
    const recordsWithEditingState = records.map((record) => ({
      ...record,
      isEditing: false, // Initialize isEditing state for each record
    }));
    setAllRecords(recordsWithEditingState);
    setOriginalRecords([...recordsWithEditingState]);
  }, [records]);

  const handleSearch = (e) => {
    setSearchTerm(e.target.value);
  };

  const filteredRecords = allRecords.filter((record) => {
    const trimmedSearchTerm = searchTerm.trim().toLowerCase();
    return Object.values(record).some(
      (value) =>
        value !== null &&
        value !== undefined &&
        String(value).toLowerCase().includes(trimmedSearchTerm)
    );
  });

  // Calculate current records based on pagination
  const indexOfFirstRecord = currentPage * recordsPerPage;
  const indexOfLastRecord = indexOfFirstRecord + recordsPerPage;
  const currentRecords = filteredRecords.slice(
    indexOfFirstRecord,
    indexOfLastRecord
  );

  const totalPages = Math.ceil(filteredRecords.length / recordsPerPage);

  const handleDownloadExcel = () => {
    // Assuming allRecords is your array of objects
    const updatedRecords = allRecords.map((record) => ({
      ...record,
      ValidatedDate: record.Date,
      RejectedReason: "",
    }));

    const ws = utils.json_to_sheet(updatedRecords);
    const wb = utils.book_new();
    utils.book_append_sheet(wb, ws, "Records");
    writeFileXLSX(wb, "records.xlsx");
  };

  const handleUpload = (e) => {
    // Handle file upload logic here
  };

  const handleToggleEdit = (index) => {
    const updatedRecords = [...allRecords];
    const globalIndex = indexOfFirstRecord + index; // Calculate the global index
    updatedRecords[globalIndex].isEditing =
      !updatedRecords[globalIndex].isEditing;
    setAllRecords(updatedRecords);
  };

  const handleSaveRecord = async (index) => {
    showLoader();
    const globalIndex = indexOfFirstRecord + index; // Calculate the global index
    const recordToSave = allRecords[globalIndex];

    try {
      const response = await saveSingleRecord(recordToSave, tableName);
      if (response.status === 204 || response.ok) {
        alert("Record updated successfully!");
        // Optionally, update the local state to reflect the saved changes
        const updatedRecords = [...allRecords];
        updatedRecords[globalIndex].isEditing = false; // Disable editing mode
        setAllRecords(updatedRecords);
      } else {
        throw new Error(`Failed to update record. Status: ${response.status}`);
      }
    } catch (error) {
      console.error("Error updating record:", error);
      alert("Failed to update record.");
    }
    hideLoader();
  };

  const handlePageChange = ({ selected }) => {
    setCurrentPage(selected);
  };

  const clearRow = (index) => {
    const updatedRecords = [...allRecords];
    const globalIndex = indexOfFirstRecord + index; // Calculate the global index
    const record = updatedRecords[globalIndex];
    for (let key in record) {
      if (
        key === "Cin" ||
        key === "RowNo" ||
        key === "Date" ||
        key === "BankId" ||
        key === "CbCin"
      ) {
        continue;
      }
      if (record.hasOwnProperty(key)) {
        if (typeof record[key] === "string") {
          record[key] = "";
        } else if (typeof record[key] === "number") {
          record[key] = 0;
        } else if (typeof record[key] === "boolean") {
          record[key] = false;
        }
      }
    }
    setAllRecords(updatedRecords);
  };

  const handleInputChange = (index, field, e) => {
    const updatedRecords = [...allRecords];
    const globalIndex = indexOfFirstRecord + index; // Calculate the global index
    const fieldValue =
      e.target.type === "checkbox" ? e.target.checked : e.target.value;
    updatedRecords[globalIndex][field.name] = fieldValue;
    setAllRecords(updatedRecords);
  };

  const fields =
    tableName === "borrowerDetail" ? borrowerFields : coBorrowerFields;

  const renderTable = (records) => {
    return (
      <div className="overflow-x-auto bg-white rounded-xl shadow-sm mt-2 text-sm">
        <table className="w-full border-separate border-spacing-0">
          <thead>
            <tr>
              <th className="bg-blue-700 font-bold text-white text-left px-2 py-[1px] border border-gray-300 whitespace-nowrap">
                Edit
              </th>
              {fields.map((field) => (
                <th
                  key={field.name}
                  className="bg-blue-700 font-bold text-white text-left px-2 py-[1px] border border-gray-300 whitespace-nowrap"
                >
                  {field.name}
                </th>
              ))}
              <th className="bg-blue-700 font-bold text-white text-left px-2 py-[1px] border border-gray-300 whitespace-nowrap">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {records.map((record, index) => (
              <tr
                key={record.id || index}
                className={`hover:bg-gray-200 ${
                  index % 2 === 0 ? "bg-white" : "bg-[#F2F2F2]"
                }`}
              >
                <td className="text-left px-2 py-[1px] border border-gray-300 whitespace-nowrap">
                  <input
                    type="checkbox"
                    className="edit-checkbox"
                    checked={record.isEditing}
                    onChange={() => handleToggleEdit(index)}
                  />
                </td>
                {fields.map((field) => (
                  <td
                    key={field.name}
                    className="text-left px-2 py-[1px] border border-gray-300 whitespace-nowrap"
                  >
                    {field.disabled ? (
                      <span>
                        {field.name === "Date"
                          ? new Date(record[field.name])
                              .toISOString()
                              .split("T")[0]
                          : record[field.name]}
                      </span>
                    ) : (
                      <input
                        type={field.type}
                        value={
                          field.type === "date"
                            ? record[field.name].split("T")[0]
                            : record[field.name]
                        }
                        checked={
                          field.type === "checkbox"
                            ? record[field.name]
                            : undefined
                        }
                        disabled={!record.isEditing}
                        className="bg-white text-gray-800 w-fit py-[2px] px-4 border border-gray-400 rounded-lg outline-none transition-colors duration-200 max-w-[160px] disabled:bg-gray-200 disabled:text-gray-500 disabled:border-gray-300 disabled:cursor-not-allowed"
                        onChange={(e) => handleInputChange(index, field, e)}
                      />
                    )}
                  </td>
                ))}

                <td className="flex gap-2 px-2 py-1 border border-gray-300 whitespace-nowrap">
                  <button
                    data-tooltip-id="my-tooltip"
                    data-tooltip-content="Save"
                    className="bg-blue-700 hover:bg-blue-800 border-none text-sm text-white cursor-pointer rounded-md px-4 py-[1px]"
                    onClick={() => handleSaveRecord(index)}
                  >
                    <i className="fas fa-save"></i>
                  </button>
                  <button
                    data-tooltip-id="my-tooltip"
                    data-tooltip-content="Clear"
                    className="bg-red-500 hover:bg-red-600 border-none text-sm text-white cursor-pointer rounded-md px-4 py-[1px]"
                    id="clearBtn"
                    onClick={() => clearRow(index)}
                  >
                    <i className="fas fa-trash"></i>
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  };

  return (
    <div
      className="fixed top-0 left-0 w-full h-full bg-black bg-opacity-50 flex justify-center items-center overflow-y-auto backdrop-blur-md"
      onClick={(e) => e.stopPropagation()}
    >
      <div
        className="bg-white p-3 rounded-xl shadow-lg w-11/12 text-left mx-auto max-h-[90vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Modal content */}
        <div className="flex justify-between items-center mb-2">
          <h2 className="text-xl font-bold">All Records</h2>
          <input
            type="text"
            id="searchInput"
            placeholder="Search..."
            value={searchTerm}
            onChange={handleSearch}
            className="p-2 px-4 border border-gray-200 rounded-lg outline-none transition-all duration-200 text-base w-full max-w-[300px] box-border"
          />
          <div>
            <label htmlFor="pageSize" className="mr-2">
              Page Size:
            </label>
            <select
              id="pageSize"
              value={recordsPerPage}
              onChange={(e) => {
                setRecordsPerPage(Number(e.target.value));
                setCurrentPage(0); // Reset to the first page when changing page size
              }}
              className="p-2 border border-gray-200 rounded-lg outline-none transition-all duration-200 text-base"
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
          </div>
          <div className="flex space-x-2">
            <button
              id="downloadBtn"
              data-tooltip-id="my-tooltip"
              data-tooltip-content="Download"
              onClick={handleDownloadExcel}
              className="bg-green-500 border-none text-sm text-white cursor-pointer rounded-md px-4 py-2"
            >
              <i className="fas fa-download"></i>
            </button>
            <button
              id="uploadBtn"
              data-tooltip-id="my-tooltip"
              data-tooltip-content="Upload"
              onClick={() => document.getElementById("uploadInput").click()}
              className="bg-red-500 border-none text-sm text-white cursor-pointer rounded-md px-4 py-2"
            >
              <i className="fas fa-upload"></i>
            </button>
            <input
              type="file"
              id="uploadInput"
              style={{ display: "none" }}
              onChange={handleUpload}
            />
          </div>
          <button
            className="border-none text-sm text-white cursor-pointer rounded-md px-4 py-2 bg-blue-700 font-medium hover:bg-blue-800"
            onClick={onClose}
          >
            X
          </button>
        </div>
        <div className="overflow-x-auto">{renderTable(currentRecords)}</div>
        <div className="flex justify-between items-center mt-4">
          <ReactPaginate
            previousLabel={<i className="fas fa-chevron-left"></i>}
            nextLabel={<i className="fas fa-chevron-right"></i>}
            breakLabel={"..."}
            breakClassName={"break-me px-2 py-1"}
            pageCount={totalPages}
            marginPagesDisplayed={2}
            pageRangeDisplayed={5}
            onPageChange={handlePageChange}
            containerClassName={"flex gap-1"}
            activeClassName={"bg-blue-500 text-white rounded-md px-2 py-1"}
            previousClassName={"pagination-previous  px-2 py-1"}
            nextClassName={"pagination-next px-2 py-1"}
            pageClassName={"pagination-page px-2 py-1"}
            disabledClassName={"pagination-disabled"}
            forcePage={currentPage}
          />
          <button className="bg-blue-500 text-white p-2 rounded-md" onclick="">
            Push to NHB Stagging
          </button>
        </div>
      </div>
      <ReactTooltip id="my-tooltip" />
    </div>
  );
};

export default ViewAllRecordsModal;
