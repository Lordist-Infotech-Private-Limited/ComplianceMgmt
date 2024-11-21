document.addEventListener('DOMContentLoaded', function () {
    const stateData = {
        "Maharashtra": { population: "112 million", area: "307,713 km²", capital: "Mumbai" },
        "Karnataka": { population: "67 million", area: "191,791 km²", capital: "Bangalore" },
        // Add data for each state
    };

    // Select all states
    const states = document.querySelectorAll('.state');

    states.forEach(state => {
        state.addEventListener('click', function () {
            const stateName = state.id; // Get the ID of the clicked state
            displayStateData(stateName);
            highlightState(state);
        });
    });

    // Display state data
    function displayStateData(stateName) {
        const stateDetailsDiv = document.getElementById('stateDetails');
        const data = stateData[stateName];

        if (data) {
            stateDetailsDiv.innerHTML = `
                <h4>${stateName}</h4>
                <p><strong>Population:</strong> ${data.population}</p>
                <p><strong>Area:</strong> ${data.area}</p>
                <p><strong>Capital:</strong> ${data.capital}</p>
            `;
            stateDetailsDiv.style.display = 'block';
        } else {
            stateDetailsDiv.innerHTML = "<p>No data available for this state.</p>";
            stateDetailsDiv.style.display = 'block';
        }
    }

    // Highlight state on click
    function highlightState(selectedState) {
        states.forEach(state => state.style.fill = '#B0BEC5'); // Reset all states to default color
        selectedState.style.fill = '#FF5722'; // Highlight the selected state
    }
});
