/**
 * Initializes product filtering functionality.
 * Allows users to filter products based on keywords.
 */
function productFiltering() {
    const btnFilter = document.getElementById('btnFilter');
    const keywordFilter = document.getElementById('keywordFilter');
    const productList = document.getElementById('product-list');
    
    // Filtering
    // Add a click event listener to the filter button.
    btnFilter.addEventListener('click', () => {
        const keyword = keywordFilter.value.toLowerCase();
        
        const rows = productList.querySelectorAll('tr');
        rows.forEach(function(row) {
            // Get the product details from the data attribute, or an empty string if it doesn't exist.
            const detail = row.getAttribute('data-product-detail') ? row.getAttribute('data-product-detail').toLowerCase() : '';

            // Check if the product detail contains the keyword.
            const matchKeyword = detail.indexOf(keyword) !== -1;

            // Show or hide the row based on the filter match.
            if (matchKeyword) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    });
}

/**
 * Initializes the clear filtering functionality.
 * Resets the keyword input and shows all products.
 */
function clearFiltering() {
    const btnClear = document.getElementById('btnClear');
    const keywordFilter = document.getElementById('keywordFilter');
    const productList = document.getElementById('product-list');

    // Add a click event listener to the clear button.
    btnClear.addEventListener('click', () => {
        // Clear the keyword input.
        keywordFilter.value = '';

        // Get all table rows.
        const rows = productList.querySelectorAll('tr');
        rows.forEach(function(row) {
            row.style.display = '';
        });
    });
}

/**
 * Initializes product sorting functionality.
 * Allows users to sort products by clicking on table headers.
 */
function productSorting() {
    const productList = document.getElementById('product-list');
    const tableHeaders = document.querySelectorAll('th');
    
    let currentSortBy = '';
    let currentSortDir = 'asc';

    // Iterate over each table header and add a click event listener.
    tableHeaders.forEach((header, index) => {
        header.addEventListener('click', () => {
            // Toggle sort direction if the same header is clicked again.
            if (currentSortBy === index) {
                currentSortDir = currentSortDir === 'asc' ? 'desc' : 'asc';
            } else {
                currentSortBy = index;
                currentSortDir = 'asc';
            }

            // Convert the NodeList of rows to an Array so we can use the sort() method.
            const rows = Array.from(productList.querySelectorAll('tr.product-item'));

            // Sort the rows based on the clicked column.
            rows.sort((a, b) => {
                const valueA = a.cells[index].textContent.trim().toLowerCase();
                const valueB = b.cells[index].textContent.trim().toLowerCase();
                
                // Release Date
                if (index === 1) {
                    const dateA = new Date(valueA);
                    const dateB = new Date(valueB);
                    if (!isNaN(dateA) && !isNaN(dateB)) {
                        return currentSortDir === 'asc' ? dateA - dateB : dateB - dateA;
                    } 
                }
                
                // Count
                const intA = parseInt(valueA, 10);
                const intB = parseInt(valueB, 10);
                if (!isNaN(intA) && !isNaN(intB)) {
                    return currentSortDir === 'asc' ? intA - intB : intB - intA;
                }
                
                // Default
                return currentSortDir === 'asc' ? valueA.localeCompare(valueB) : valueB.localeCompare(valueA);
            });
            
            // Re-append the sorted rows to the table.
            rows.forEach(row => productList.appendChild(row));

            // Update the sort icons in the table headers.
            updateSortIcons(currentSortBy, currentSortDir);
        });
    });
}

/**
 * Updates the sort icons in the table headers to indicate the current sort column and direction.
 *
 * @param {number} currentSortBy The index of the currently sorted column.
 * @param {string} currentSortDir The current sort direction ('asc' or 'desc').
 */
function updateSortIcons(currentSortBy, currentSortDir) {
    const tableHeaders = document.querySelectorAll('th');
    
    tableHeaders.forEach((header, index) => {
        // Remove any existing sort icons.
        header.innerHTML = header.innerHTML.replace(/<i class="bi.*"><\/i>/, '');

        // Add the appropriate sort icon to the currently sorted column.
        if (currentSortBy === index) {
            const icon = document.createElement('i');
            icon.className = currentSortDir === 'asc' ? 'bi bi-arrow-up' : 'bi bi-arrow-down';
            header.appendChild(icon);
        }
    });
}

/**
* Sets up click event listeners for product rows to navigate to product details page.
*/
function setProductRowClick() {
    const rows = document.querySelectorAll('tr[data-product-url]');
    rows.forEach(row => {
        row.addEventListener('click', () => {
            // Get the URL from the data attribute.
            const url = row.getAttribute('data-product-url');
            // Navigate to the product details page.
            window.location.href = url;
        });
    });
}

// Add a DOMContentLoaded event listener to execute the initialization functions after the DOM is fully loaded.
document.addEventListener('DOMContentLoaded', () => {
    productFiltering();
    productSorting();
    clearFiltering();
    setProductRowClick();
})