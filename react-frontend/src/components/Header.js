import React, { useState } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUser } from '@fortawesome/free-solid-svg-icons';

const Header = ({ componentName }) => {
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  const toggleDropdown = () => {
    setIsDropdownOpen(!isDropdownOpen);
  };

  return (
    <header className="bg-white shadow-md p-4 flex justify-between items-center md:flex-row">
      <h1 className="text-xl font-semibold">{componentName}</h1>
      <div className="relative">
        <button onClick={toggleDropdown} className="flex items-center space-x-2">
          <FontAwesomeIcon icon={faUser} className="text-blue-600" />
          <span className="text-blue-600">Profile</span>
        </button>
        {isDropdownOpen && (
          <div className="absolute right-0 mt-2 w-48 bg-white border border-gray-200 shadow-lg rounded-md">
            <ul>
              <li className="p-2 hover:bg-gray-100">
                <a href="/profile">Profile</a>
              </li>
              <li className="p-2 hover:bg-gray-100">
                <a href="/settings">Settings</a>
              </li>
              <li className="p-2 hover:bg-gray-100">
                <a href="/logout">Logout</a>
              </li>
            </ul>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;