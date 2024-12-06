import React, { useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faBars,
  faChartBar,
  faFileAlt,
  faCogs,
  faChevronDown,
  faChevronUp,
} from "@fortawesome/free-solid-svg-icons";
import logo from "../assets/logo.png";
import fullLogo from "../assets/fulllogo.png";

const Sidebar = ({ onComponentChange, activeComponent }) => {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const [isReportSubMenuOpen, setIsReportSubMenuOpen] = useState(false);

  const toggleSidebar = () => {
    setIsCollapsed(!isCollapsed);
  };

  const toggleReportSubMenu = () => {
    setIsReportSubMenuOpen(!isReportSubMenuOpen);
  };

  return (
    <div
      className={`bg-gradient-to-b from-gray-100 via-white to-gray-300 text-black min-h-screen md:flex flex-col transition-all duration-300 hidden  ${
        isCollapsed ? "w-20" : "w-56"
      } shadow-lg`}
    >
      <div className="flex justify-between items-center p-4">
        <div className={`${isCollapsed ? "hidden" : "block"}`}>
          <img src={fullLogo} alt="Large Logo" className="w-32" />
        </div>
        <div
          className={`${!isCollapsed ? "hidden" : "block cursor-pointer"}`}
          onClick={toggleSidebar}
        >
          <img src={logo} alt="Small Logo" className="w-8" />
        </div>
        {!isCollapsed && (
          <button onClick={toggleSidebar} className="text-xl">
            <FontAwesomeIcon icon={faBars} />
          </button>
        )}
      </div>
      <nav className="mt-8 flex-grow">
        <ul>
          <li
            className={`p-4 hover:bg-gray-200 hover:text-[#f26114] cursor-pointer transition-all duration-200 rounded-md mx-2 my-2 ${
              isCollapsed ? "text-center" : ""
            } ${
              activeComponent === "Dashboard"
                ? "bg-gray-200 text-[#f26114]"
                : ""
            }`}
            onClick={() => onComponentChange("Dashboard")}
          >
            <span className="flex items-center">
              <FontAwesomeIcon icon={faChartBar} className="mr-2" />
              {isCollapsed ? "" : "Dashboard"}
            </span>
          </li>
          <li
            className={`p-4 hover:bg-gray-200 hover:text-[#f26114] cursor-pointer transition-all duration-200 rounded-md mx-2 my-2 ${
              isCollapsed ? "text-center" : ""
            } ${
              activeComponent === "Compliance Reports" ||
              activeComponent === "MIS Reports" ||
              activeComponent === "Management Dashboard"
                ? "bg-gray-200 text-[#f26114]"
                : ""
            }`}
            onClick={toggleReportSubMenu}
          >
            <span className="flex items-center justify-between">
              <span className="flex items-center">
                <FontAwesomeIcon icon={faFileAlt} className="mr-2" />
                {isCollapsed ? "" : "Report"}
              </span>
              {!isCollapsed && (
                <FontAwesomeIcon
                  icon={isReportSubMenuOpen ? faChevronUp : faChevronDown}
                  className="ml-2"
                />
              )}
            </span>
          </li>
          {isReportSubMenuOpen && !isCollapsed && (
            <ul className="ml-8">
              <li
                className={`p-4 hover:bg-gray-200 hover:text-[#f26114] cursor-pointer transition-all duration-200 rounded-md mx-2 my-2 ${
                  activeComponent === "Compliance Reports"
                    ? "bg-gray-200 text-[#f26114]"
                    : ""
                }`}
                onClick={() => onComponentChange("Compliance Reports")}
              >
                <span className="flex items-center">Compliance Reports</span>
              </li>
              <li
                className={`p-4 hover:bg-gray-200 hover:text-[#f26114] cursor-pointer transition-all duration-200 rounded-md mx-2 my-2 ${
                  activeComponent === "MIS Reports"
                    ? "bg-gray-200 text-[#f26114]"
                    : ""
                }`}
                onClick={() => onComponentChange("MIS Reports")}
              >
                <span href="#" className="flex items-center">
                  MIS Reports
                </span>
              </li>
              <li
                className={`p-4 hover:bg-gray-200 hover:text-[#f26114] cursor-pointer transition-all duration-200 rounded-md mx-2 my-2 ${
                  activeComponent === "Management Dashboard"
                    ? "bg-gray-200 text-[#f26114]"
                    : ""
                }`}
                onClick={() => onComponentChange("Management Dashboard")}
              >
                <span className="flex items-center">Management Dashboard</span>
              </li>
            </ul>
          )}
          <li
            className={`p-4 hover:bg-gray-200 hover:text-[#f26114] cursor-pointer transition-all duration-200 rounded-md mx-2 my-2 ${
              isCollapsed ? "text-center" : ""
            } ${
              activeComponent === "Configuration"
                ? "bg-gray-200 text-[#f26114]"
                : ""
            }`}
            onClick={() => onComponentChange("Configuration")}
          >
            <span className="flex items-center">
              <FontAwesomeIcon icon={faCogs} className="mr-2" />
              {isCollapsed ? "" : "Configuration"}
            </span>
          </li>
        </ul>
      </nav>
    </div>
  );
};

export default Sidebar;
