import React, { useState, useEffect } from "react";
import { utils, writeFileXLSX } from "xlsx";
import "@fortawesome/fontawesome-free/css/all.css";
import { saveSingleRecord } from "../api/service";
import { Tooltip as ReactTooltip } from "react-tooltip";
import "react-tooltip/dist/react-tooltip.css";

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
  const recordsPerPage = 10;

  useEffect(() => {
    console.log("records", records);
    setAllRecords(records);
    setOriginalRecords([...records]);
  }, [records]);

  const handleSearch = (e) => {
    setSearchTerm(e.target.value);
  };

  const filteredRecords = allRecords.filter((record) => {
    return Object.values(record).some(
      (value) =>
        value !== null &&
        value !== undefined &&
        String(value).toLowerCase().includes(searchTerm.toLowerCase())
    );
  });

  // Calculate current records based on pagination
  const indexOfFirstRecord = currentPage * recordsPerPage;
  const indexOfLastRecord = indexOfFirstRecord + recordsPerPage;
  const currentRecords = filteredRecords.slice(
    indexOfFirstRecord,
    indexOfLastRecord
  );

  console.log("Current Page:", currentPage);
  console.log("Records per Page:", recordsPerPage);
  console.log("Filtered Records Length:", filteredRecords.length);
  console.log("Current Records Length:", currentRecords.length);

  const handleDownloadExcel = () => {
    const ws = utils.json_to_sheet(allRecords);
    const wb = utils.book_new();
    utils.book_append_sheet(wb, ws, "Records");
    writeFileXLSX(wb, "records.xlsx");
  };

  const handleUpload = (e) => {
    // Handle file upload logic here
  };

  const handleToggleEdit = (index) => {
    const updatedRecords = [...allRecords];
    updatedRecords[index].isEditing = !updatedRecords[index].isEditing;
    setAllRecords(updatedRecords);
  };

  const handleSaveRecord = async (index) => {
    showLoader();
    const recordToSave = allRecords[index];

    try {
      const response = await saveSingleRecord(recordToSave, tableName);
      if (response.status === 204 || response.ok) {
        alert("Record updated successfully!");
        // Optionally, update the local state to reflect the saved changes
        const updatedRecords = [...allRecords];
        updatedRecords[index].isEditing = false; // Disable editing mode
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

  const handlePrevPage = () => {
    if (currentPage > 0) {
      setCurrentPage(currentPage - 1);
    }
  };

  const handleNextPage = () => {
    const total_pages = Math.ceil(filteredRecords.length / recordsPerPage);
    if (currentPage < total_pages - 1) {
      setCurrentPage(currentPage + 1);
    }
  };

  const clearRow = (index) => {
    const updatedRecords = [...allRecords];
    const record = updatedRecords[index];
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
    const fieldValue =
      e.target.type === "checkbox" ? e.target.checked : e.target.value;
    updatedRecords[index][field.name] = fieldValue;
    setAllRecords(updatedRecords);
  };

  const borrowerFields = [
    { name: "RowNo", type: "number", disabled: true },
    { name: "Date", type: "date", disabled: true },
    { name: "BankId", type: "number", disabled: true },
    { name: "Cin", type: "text" },
    { name: "BName", type: "text" },
    { name: "BDob", type: "date" },
    { name: "sbcitizenship", type: "text" },
    { name: "BPanNo", type: "text" },
    { name: "Aadhaar", type: "text" },
    { name: "IdType", type: "text" },
    { name: "IdNumber", type: "text" },
    { name: "BMonthlyIncome", type: "number" },
    { name: "BReligion", type: "text" },
    { name: "BCast", type: "text" },
    { name: "BGender", type: "text" },
    { name: "BOccupation", type: "text" },
    { name: "IsValidated", type: "checkbox" },
  ];

  const coBorrowerFields = [
    { name: "RowNo", type: "number", disabled: true },
    { name: "Date", type: "date", disabled: true },
    { name: "BankId", type: "number", disabled: true },
    { name: "Cin", type: "text" },
    { name: "IdType", type: "text" },
    { name: "IdNumber", type: "text" },
    { name: "CbCin", type: "text" },
    { name: "CbName", type: "text" },
    { name: "CbDob", type: "date" },
    { name: "CbCitizenship", type: "text" },
    { name: "CbPanNo", type: "text" },
    { name: "CbAadhaar", type: "text" },
    { name: "CbMonthlyIncome", type: "number" },
    { name: "CbReligion", type: "text" },
    { name: "CbCast", type: "text" },
    { name: "CbGender", type: "text" },
    { name: "CbOccupation", type: "text" },
    { name: "IsValidated", type: "checkbox" },
  ];

  const fields =
    tableName === "borrowerDetail" ? borrowerFields : coBorrowerFields;

  const renderTable = (records) => {
    return (
      <div className="overflow-x-auto bg-white rounded-xl shadow-sm mt-2 text-sm">
        <table className="w-full border-separate border-spacing-0">
          <thead>
            <tr>
              <th className="bg-blue-700 font-bold text-white text-left px-2 py-1 border border-gray-300 whitespace-nowrap">
                Edit
              </th>
              {fields.map((field) => (
                <th
                  key={field.name}
                  className="bg-blue-700 font-bold text-white text-left px-2 py-1 border border-gray-300 whitespace-nowrap"
                >
                  {field.name}
                </th>
              ))}
              <th className="bg-blue-700 font-bold text-white text-left px-2 py-1 border border-gray-300 whitespace-nowrap">
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
                <td className="text-left px-2 py-1 border border-gray-300 whitespace-nowrap">
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
                    className="text-left px-2 py-1 border border-gray-300 whitespace-nowrap"
                  >
                    {field.disabled ? (
                      <span>{record[field.name]}</span>
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
                        className="bg-white text-gray-800 w-fit py-1 px-4 border border-gray-400 rounded-lg outline-none transition-colors duration-200 max-w-[160px] disabled:bg-gray-200 disabled:text-gray-500 disabled:border-gray-300 disabled:cursor-not-allowed"
                        onChange={(e) => handleInputChange(index, field, e)}
                      />
                    )}
                  </td>
                ))}

                <td className="flex gap-2 px-2 py-1 border border-gray-300 whitespace-nowrap">
                  <button
                    data-tooltip-id="my-tooltip"
                    data-tooltip-content="Save"
                    className="bg-blue-700 hover:bg-blue-800 border-none text-sm text-white cursor-pointer rounded-md px-4 py-2"
                    onClick={() => handleSaveRecord(index)}
                  >
                    <i className="fas fa-save"></i>
                  </button>
                  <button
                    data-tooltip-id="my-tooltip"
                    data-tooltip-content="Clear"
                    className="bg-red-500 hover:bg-red-600 border-none text-sm text-white cursor-pointer rounded-md px-4 py-2"
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
        className="bg-white p-5 rounded-xl shadow-lg w-11/12 max-w-4xl text-left mx-auto max-h-[90vh] overflow-y-auto"
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
          <button
            onClick={handlePrevPage}
            disabled={currentPage === 0}
            className="bg-blue-500 text-white p-2 rounded-md"
          >
            Previous
          </button>
          <button className="bg-blue-500 text-white p-2 rounded-md" onclick="">
            Push to NHB Stagging
          </button>
          <button
            onClick={handleNextPage}
            disabled={
              currentPage >=
              Math.ceil(filteredRecords.length / recordsPerPage) - 1
            }
            className="bg-blue-500 text-white p-2 rounded-md"
          >
            Next
          </button>
        </div>
      </div>
      <ReactTooltip id="my-tooltip" />
    </div>
  );
};

export default ViewAllRecordsModal;
