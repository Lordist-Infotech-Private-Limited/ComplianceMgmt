document.getElementById('loginForm').addEventListener('submit', function(event) {
    event.preventDefault();

    const mailId = document.getElementById('mailId').value;
    const password = document.getElementById('password').value;
    const errorMessage = document.getElementById('error-message');
    const loaderContainer = document.getElementById('loaderContainer');

    // Show loader
    loaderContainer.style.display = 'flex';

    // Check if the app is running locally (development) or on a live server (production)
    const isLocal = window.location.hostname === "localhost" || window.location.hostname === "https://adfapi.lordist.in";
        
    // Set the base URL for the API based on the environment
    const apiBaseUrl = isLocal ? "https://localhost:7275/Auth/Login" : "https://adfapi.lordist.in/Auth/Login";

    fetch(apiBaseUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            mailId: mailId,
            password: password,
        }),
    })
    .then(response => response.json())
    .then(data => {
        // Hide loader
        loaderContainer.style.display = 'none';

        if (data.UserID) {
            // Redirect to a dashboard or home page
            window.location.href = 'dashboard.html';
        } else {
            errorMessage.textContent = data.message || 'Login failed';
        }
    })
    .catch(error => {
        // Hide loader
        loaderContainer.style.display = 'none';

        console.error('Error:', error);
        errorMessage.textContent = 'An error occurred during login';
    });
});



//dashboard.html

function showTable(tableType) {
    // Show the table section when a counter is clicked
    document.getElementById('tableSection').style.display = 'block';
    // Hide the details section when changing table views
    document.getElementById('detailsSection').style.display = 'none';

    // Add logic here to fetch data based on `tableType` if needed
    console.log('Displaying table for:', tableType);
}

function showDetails() {
    // Show the details section when a "+" button is clicked
    document.getElementById('detailsSection').style.display = 'block';
}

function closeDetails() {
    // Hide the details section when the Close button is clicked
    document.getElementById('detailsSection').style.display = 'none';
}
