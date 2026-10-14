document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("form[data-confirm]").forEach(function (form) {
        form.addEventListener("submit", function (event) {
            var message = form.getAttribute("data-confirm");
            if (message && !window.confirm(message)) {
                event.preventDefault();
            }
        });
    });

    document.querySelectorAll("[data-image-input]").forEach(function (input) {
        input.addEventListener("change", function () {
            var file = input.files && input.files[0];
            var selector = input.getAttribute("data-preview-target");
            var preview = selector ? document.querySelector(selector) : null;
            if (!file || !preview || !file.type.startsWith("image/")) {
                return;
            }

            var imageUrl = URL.createObjectURL(file);
            preview.replaceChildren();
            var image = document.createElement("img");
            image.src = imageUrl;
            image.alt = "Selected product image preview";
            image.onload = function () { URL.revokeObjectURL(imageUrl); };
            preview.appendChild(image);
        });
    });

    document.querySelectorAll("[data-order-form]").forEach(function (form) {
        var input = form.querySelector("[data-quantity-input]");
        var decrease = form.querySelector("[data-quantity-decrease]");
        var increase = form.querySelector("[data-quantity-increase]");
        var total = form.querySelector("[data-order-total]");
        var unitPrice = Number(form.getAttribute("data-unit-price"));

        if (!input || !Number.isFinite(unitPrice)) {
            return;
        }

        function updateQuantity(value) {
            var minimum = Number(input.min) || 1;
            var maximum = Number(input.max) || minimum;
            var quantity = Math.min(maximum, Math.max(minimum, Number(value) || minimum));
            input.value = String(quantity);
            if (total) {
                total.textContent = "Rs. " + (unitPrice * quantity).toLocaleString("en-PK", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            }
        }

        decrease?.addEventListener("click", function () { updateQuantity(Number(input.value) - 1); });
        increase?.addEventListener("click", function () { updateQuantity(Number(input.value) + 1); });
        input.addEventListener("input", function () { updateQuantity(input.value); });
        updateQuantity(input.value);
    });

    document.querySelectorAll("[data-character-count]").forEach(function (field) {
        var form = field.closest("form");
        var output = form ? form.querySelector("[data-character-output]") : null;
        var maximum = Number(field.maxLength);
        if (!output || maximum <= 0) {
            return;
        }

        function updateCount() {
            output.textContent = field.value.length + " / " + maximum;
        }

        field.addEventListener("input", updateCount);
        updateCount();
    });
});
