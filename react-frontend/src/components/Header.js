import React, { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUser } from "@fortawesome/free-solid-svg-icons";
import logo from "../assets/placeholder.png";
import Actions from "./Actions";

const Header = ({
  componentName,
  user,
  onLogout,
  onFetchData,
  selectedDate,
  setSelectedDate,
  filter,
  handleFilterChange,
  downloadReport,
}) => {
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const navigate = useNavigate();
  const dropdownRef = useRef(null);

  const toggleDropdown = () => {
    setIsDropdownOpen(!isDropdownOpen);
  };

  const handleLogout = () => {
    onLogout();
    navigate("/");
  };

  const handleClickOutside = (event) => {
    if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
      setIsDropdownOpen(false);
    }
  };

  useEffect(() => {
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <header className="hidden bg-white shadow-md p-4 justify-between items-center md:flex-row md:flex">
      <h1 className="text-xl font-semibold">{componentName}</h1>

      {componentName === "Dashboard" && (
        <Actions
          onFetchData={onFetchData}
          selectedDate={selectedDate}
          setSelectedDate={setSelectedDate}
        />
      )}
      {componentName === "MIS Reports" && (
        <div className="flex items-center gap-4">
          <div>
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
            className="bg-[#f26114] hover:bg-orange-500 border-none text-sm text-white cursor-pointer rounded-md px-4 py-2"
          >
            <i className="fas fa-download"></i>
          </button>
        </div>
      )}
      <div className="flex gap-4 relative" ref={dropdownRef}>
        <img src={logo} alt="Small Logo" className="w-8" />
        <button
          onClick={toggleDropdown}
          className="px-4 py-2 font-medium bg-[#f26114] hover:bg-orange-500 text-white border-none rounded-md cursor-pointer text-sm w-full md:w-fit flex gap-2 items-center"
        >
          <FontAwesomeIcon icon={faUser} className="text-white" />
          <div>
            <span className="text-white">
              {user ? user.UserName : "Profile"}
            </span>
          </div>
        </button>

        {isDropdownOpen && (
          <div className="absolute right-0 mt-2 w-48 bg-white border border-gray-200 shadow-lg rounded-md z-10">
            <ul>
              <li className="p-2 hover:bg-gray-100 cursor-pointer">
                <button>Profile</button>
              </li>
              <li className="p-2 hover:bg-gray-100 cursor-pointer">
                <button>Settings</button>
              </li>
              <li
                className="p-2 hover:bg-gray-100 cursor-pointer"
                onClick={handleLogout}
              >
                <button>Logout</button>
              </li>
            </ul>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;
