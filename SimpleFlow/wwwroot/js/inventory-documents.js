document.addEventListener("DOMContentLoaded", () => {
    const table = document.querySelector("#documentLines tbody");
    const addButton = document.querySelector("#addLine");
    const template = document.querySelector("#documentLineTemplate");

    if (!table || !addButton || !template) {
        return;
    }

    addButton.addEventListener("click", () => {
        const token = `${Date.now()}${Math.floor(Math.random() * 1000)}`;
        const html = template.innerHTML.replaceAll("__token__", token);

        table.insertAdjacentHTML("beforeend", html);
    });

    table.addEventListener("click", event => {
        const removeButton = event.target.closest(".remove-line");

        if (!removeButton) {
            return;
        }

        const rows = table.querySelectorAll(".document-line");

        if (rows.length === 1) {
            return;
        }

        removeButton.closest(".document-line")?.remove();
    });

    const form = table.closest("form");

    form?.addEventListener("submit", () => {
        const rows = table.querySelectorAll(".document-line");

        rows.forEach((row, index) => {
            const indexInput = row.querySelector(
                'input[name="Lines.Index"]');

            if (!indexInput) {
                return;
            }

            const oldToken = indexInput.value;
            indexInput.value = index;

            row.querySelectorAll("[name]").forEach(element => {
                if (element === indexInput) {
                    return;
                }

                element.name = element.name.replace(
                    `Lines[${oldToken}]`,
                    `Lines[${index}]`);
            });

            row.querySelectorAll("[data-valmsg-for]")
                .forEach(element => {
                    const value = element.getAttribute(
                        "data-valmsg-for");

                    if (!value) {
                        return;
                    }

                    element.setAttribute(
                        "data-valmsg-for",
                        value.replace(
                            `Lines[${oldToken}]`,
                            `Lines[${index}]`));
                });
        });
    });
});