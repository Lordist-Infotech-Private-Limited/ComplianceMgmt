import React, { useState } from 'react';
import Sidebar from './Sidebar';
import Header from './Header';
import Dashboard from './Dashboard';
import Report from './Report';

function Home() {
  const [selectedComponent, setSelectedComponent] = useState('Dashboard');

  const handleComponentChange = (component) => {
    setSelectedComponent(component);
  };

  return (
    <div className="flex flex-col md:flex-row">
      <Sidebar onComponentChange={handleComponentChange} activeComponent={selectedComponent} />
      <div className="flex-1">
        <Header componentName={selectedComponent} />
        {selectedComponent === 'Dashboard' ? <Dashboard /> : <Report />}
      </div>
    </div>
  );
}

export default Home;