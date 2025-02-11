/**
 * Checks the status of the database by making an AJAX request to the server.
 * This function polls the server at regular intervals until the database is ready.
 */
async function checkDatabase() {
    try {
        // Call API to check availability of Database
        let response = await fetch("/Home/CheckDatabaseStatus");
        let data = await response.json();

        // Check if Database is ready
        if (data.isDatabaseReady) {
            // Hide loading section
            document.getElementById("loading").style.display = "none";
            document.getElementById("login-btn").disabled = false;
            document.getElementById("signup-btn").disabled = false;
            
            // Check if user is loggin already
            if (data.isAuthenticated)
            {
                // Redirect to Tickets Index
                window.location.href = "/Tickets/Index";
            }
        } else {
            setTimeout(checkDatabase, 5000); // Check again in 5 seconds
        }
    } catch (error) {
        console.error("Error checking database status:", error);
        setTimeout(checkDatabase, 5000);
    }
}

// Add event listener for DOMContentLoaded to ensure that the script runs after the DOM is fully loaded.
document.addEventListener("DOMContentLoaded", () => {
    checkDatabase();
});