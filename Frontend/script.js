document.getElementById('loginForm').addEventListener('submit', function(event) {
    event.preventDefault();

    const mailId = document.getElementById('mailId').value;
    const password = document.getElementById('password').value;
    const errorMessage = document.getElementById('error-message');
    
    // Replace with your API endpoint
    const apiUrl = 'https://localhost:7275/Auth/Login';

    fetch(apiUrl, {
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
        if (data.UserID) {
            // Redirect to a dashboard or home page
            window.location.href = 'dashboard.html';
        } else {
            document.getElementById('error-message').textContent = data.message || 'Login failed';
        }
    })
    .catch(error => {
        console.error('Error:', error);
        document.getElementById('error-message').textContent = 'An error occurred during login';
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
