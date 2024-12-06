import React, { useState, useRef, useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faBars,
  faChartBar,
  faFileAlt,
  faCogs,
  faChevronDown,
  faChevronUp,
  faSignOutAlt,
  faUser,
  faCog,
} from "@fortawesome/free-solid-svg-icons";
import logo from "../assets/logo.png";

const MobileSidebar = ({
  onComponentChange,
  activeComponent,
  onLogout,
  user,
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [isReportSubMenuOpen, setIsReportSubMenuOpen] = useState(false);
  const [isProfileDropdownOpen, setIsProfileDropdownOpen] = useState(false);
  const profileDropdownRef = useRef(null);

  const toggleSidebar = () => {
    setIsOpen(!isOpen);
  };

  const toggleReportSubMenu = () => {
    setIsReportSubMenuOpen(!isReportSubMenuOpen);
  };

  const toggleProfileDropdown = () => {
    setIsProfileDropdownOpen(!isProfileDropdownOpen);
  };

  const handleComponentChange = (component) => {
    onComponentChange(component);
    setIsOpen(false);
  };

  const handleClickOutside = (event) => {
    if (
      profileDropdownRef.current &&
      !profileDropdownRef.current.contains(event.target)
    ) {
      setIsProfileDropdownOpen(false);
    }
  };

  useEffect(() => {
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  const handleLogout = () => {
    onLogout();
    setIsOpen(false);
    setIsProfileDropdownOpen(false);
  };

  return (
    <>
      {/* Mobile Hamburger Button */}
      <button
        onClick={toggleSidebar}
        className="md:hidden fixed top-4 left-4 z-[9999] text-2xl"
      >
        <FontAwesomeIcon icon={faBars} />
      </button>

      {/* Overlay */}
      <div
        className={`
          fixed 
          inset-0 
          bg-black 
          bg-opacity-50 
          z-[9998] 
          transition-opacity 
          duration-300 
          ease-in-out
          ${isOpen ? "opacity-100 visible" : "opacity-0 invisible"}
        `}
        onClick={toggleSidebar}
      />

      {/* Sidebar */}
      <div
        className={`
          fixed 
          top-0 
          left-0 
          w-64 
          h-full 
          bg-white 
          shadow-lg 
          z-[10000] 
          transform 
          transition-transform 
          duration-300 
          ease-in-out
          ${isOpen ? "translate-x-0" : "-translate-x-full"}
        `}
      >
        {/* Sidebar Header */}
        <div className="flex justify-between items-center p-4 border-b">
          <img src={logo} alt="Logo" className="w-8" />
          <button onClick={toggleSidebar} className="text-2xl">
            <FontAwesomeIcon icon={faBars} />
          </button>
        </div>

        {/* Sidebar Navigation */}
        <nav className="p-4">
          <ul>
            {/* Dashboard */}
            <li
              className={`
                py-3 px-4 
                hover:bg-gray-100 
                rounded 
                transition-colors
                my-2
                ${
                  activeComponent === "Dashboard"
                    ? "bg-gray-200 text-[#f26114]"
                    : ""
                }
              `}
              onClick={() => handleComponentChange("Dashboard")}
            >
              <div className="flex items-center">
                <FontAwesomeIcon icon={faChartBar} className="mr-3" />
                Dashboard
              </div>
            </li>

            {/* Reports */}
            <li
              className={`
                py-3 px-4 
                hover:bg-gray-100 
                rounded 
                transition-colors
                my-2
                ${
                  activeComponent === "Report"
                    ? "bg-gray-200 text-[#f26114]"
                    : ""
                }
              `}
              onClick={toggleReportSubMenu}
            >
              <div className="flex justify-between items-center">
                <div className="flex items-center">
                  <FontAwesomeIcon icon={faFileAlt} className="mr-3" />
                  Reports
                </div>
                <FontAwesomeIcon
                  icon={isReportSubMenuOpen ? faChevronUp : faChevronDown}
                />
              </div>
            </li>

            {/* Report Submenu */}
            {isReportSubMenuOpen && (
              <div className="pl-8">
                <li
                  className={`
                    py-2 px-4 
                    hover:bg-gray-100 
                    rounded 
                    transition-colors
                    ${
                      activeComponent === "Compliance Reports"
                        ? "bg-gray-200 text-[#f26114]"
                        : ""
                    }
                  `}
                  onClick={() => handleComponentChange("Compliance Reports")}
                >
                  Compliance Reports
                </li>
                <li
                  className={`
                    py-2 px-4 
                    hover:bg-gray-100 
                    rounded 
                    transition-colors
                    ${
                      activeComponent === "MIS Reports"
                        ? "bg-gray-200 text-[#f26114]"
                        : ""
                    }
                  `}
                  onClick={() => handleComponentChange("MIS Reports")}
                >
                  MIS Reports
                </li>
                <li
                  className={`
                    py-2 px-4 
                    hover:bg-gray-100 
                    rounded 
                    transition-colors
                    ${
                      activeComponent === "Management Dashboard"
                        ? "bg-gray-200 text-[#f26114]"
                        : ""
                    }
                  `}
                  onClick={() => handleComponentChange("Management Dashboard")}
                >
                  Management Dashboard
                </li>
              </div>
            )}

            {/* Configuration */}
            <li
              className={`
                py-3 px-4 
                hover:bg-gray-100 
                rounded 
                transition-colors
                my-2
                ${
                  activeComponent === "Configuration"
                    ? "bg-gray-200 text-[#f26114]"
                    : ""
                }
              `}
              onClick={() => handleComponentChange("Configuration")}
            >
              <div className="flex items-center">
                <FontAwesomeIcon icon={faCogs} className="mr-3" />
                Configuration
              </div>
            </li>
            {/* Profile Section */}
            <li
              ref={profileDropdownRef}
              className={`
                py-3 px-4 
                hover:bg-gray-100 
                rounded 
                transition-colors 
                relative
                my-2
                ${isProfileDropdownOpen ? "bg-gray-200" : ""}
              `}
              onClick={toggleProfileDropdown}
            >
              <div className="flex justify-between items-center">
                <div className="flex items-center">
                  <FontAwesomeIcon icon={faUser} className="mr-3" />
                  {user ? user.UserName : "Profile"}
                </div>
                <FontAwesomeIcon
                  icon={isProfileDropdownOpen ? faChevronUp : faChevronDown}
                />
              </div>

              {/* Profile Dropdown */}
              {isProfileDropdownOpen && (
                <div className="absolute left-0 right-0 top-full bg-white shadow-lg rounded mt-1">
                  <ul>
                    <li
                      className="py-2 px-4 hover:bg-gray-100"
                      onClick={() => {
                        handleComponentChange("Profile");
                        setIsProfileDropdownOpen(false);
                      }}
                    >
                      <FontAwesomeIcon icon={faUser} className="mr-3" />
                      Profile
                    </li>
                    <li
                      className="py-2 px-4 hover:bg-gray-100"
                      onClick={() => {
                        handleComponentChange("Settings");
                        setIsProfileDropdownOpen(false);
                      }}
                    >
                      <FontAwesomeIcon icon={faCog} className="mr-3" />
                      Settings
                    </li>
                    <li
                      className="py-2 px-4 hover:bg-gray-100 text-red-600"
                      onClick={handleLogout}
                    >
                      <FontAwesomeIcon icon={faSignOutAlt} className="mr-3" />
                      Logout
                    </li>
                  </ul>
                </div>
              )}
            </li>
          </ul>
        </nav>
      </div>
    </>
  );
};

export default MobileSidebar;
