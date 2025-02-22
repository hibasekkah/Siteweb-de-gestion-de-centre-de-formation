const listOfEdit = document.querySelectorAll(".editBtn");

listOfEdit.forEach((btn) => {
    btn.addEventListener("click", (event) => {
        event.preventDefault();
        const row = btn.closest("tr"); // Get the parent row of the button

        // Save the original row content if needed
        const originalContent = row.innerHTML;

        // Start building the form
        let formHTML = `<form asp-controller="Trainers" asp-action="UpdateTrainer" method="post" enctype="multipart/form-data">`;

        // Get all non-action cells
        const cells = row.querySelectorAll("td:not(:last-child)"); // Exclude the last cell with buttons

        cells.forEach((cell, index) => {
            const previousValue = cell.innerText.trim();
            const dataLabel = cell.getAttribute("data-label");

            let inputField = `
                <input name="${dataLabel}" class="form-control" value="${previousValue}" type="text" placeholder="${dataLabel}" />
            `;

            // Handle file input for "Profile image"
            if (dataLabel === "Profile Image") {
                inputField = `
                    <input type="file" name="${dataLabel}" class="form-control" accept="image/*" />
                `;
            }

            formHTML += `
                <td>
                    ${inputField}
                </td>
            `;
        });
        formHTML += `
        
        `

        // Add actions for Save and Cancel
        formHTML += `
            <td>
                <button type="submit" class="btn btn-primary btn-sm">
                    <i class="fa fa-save"></i> Save
                </button>
                <button type="button" class="btn btn-secondary btn-sm cancelBtn">
                    <i class="fa fa-times"></i> Cancel
                </button>
            </td>
        </form>`;

        // Replace row content with the form
        row.innerHTML = formHTML;

        // Add Cancel button functionality
        const cancelBtn = row.querySelector(".cancelBtn");
        cancelBtn.addEventListener("click", () => {
            row.innerHTML = originalContent; // Restore original content
        });
    });
});
