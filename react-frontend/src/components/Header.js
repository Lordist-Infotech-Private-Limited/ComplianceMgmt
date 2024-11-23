import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUser } from "@fortawesome/free-solid-svg-icons";
import logo from "../assets/logo.png";

const Header = ({ componentName, user, onLogout }) => {
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const navigate = useNavigate();

  const toggleDropdown = () => {
    setIsDropdownOpen(!isDropdownOpen);
  };

  const handleLogout = () => {
    onLogout();
    navigate("/");
  };

  return (
    <header className="bg-white shadow-md p-4 flex justify-between items-center md:flex-row">
      <h1 className="text-xl font-semibold">{componentName}</h1>
      <div className="flex gap-4 relative">
        <img src={logo} alt="Small Logo" className="w-8" />
        <button
          onClick={toggleDropdown}
          className="flex items-center space-x-2 bg-blue-500 px-4 rounded-md"
        >
          <FontAwesomeIcon icon={faUser} className="text-white" />
          <div>
            <span className="text-white">
              {user ? user.UserName : "Profile"}
            </span>
          </div>
        </button>
        {isDropdownOpen && (
          <div className="absolute right-0 mt-2 w-48 bg-white border border-gray-200 shadow-lg rounded-md">
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
