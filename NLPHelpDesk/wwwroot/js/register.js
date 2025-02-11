/**
 * Handles the change event of the role selection dropdown.
 * This function shows or hides the help desk category selection
 * based on the selected role.
 */
function handleChangeRole() {
    var selectedRole = document.getElementById('input-select-role');
    var divCategory = document.getElementById('input-hd-category');

    // Check if the selected role is "EndUser".
    if (selectedRole.value === 'EndUser') {
        // Hide the help desk category div.
        divCategory.style.display = 'none';
    } else {
        // Show the help desk category div.
        divCategory.style.display = 'block';
    }
}