// FincaNova — comportamiento base de la interfaz.
(function () {
    "use strict";

    // Menú lateral responsivo (HU-51): abrir/cerrar en pantallas pequeñas.
    var sidebar = document.getElementById("fnSidebar");
    var toggle = document.getElementById("fnSidebarToggle");
    var backdrop = document.getElementById("fnSidebarBackdrop");

    function closeSidebar() {
        if (!sidebar) return;
        sidebar.classList.remove("is-open");
        if (backdrop) backdrop.classList.remove("is-open");
    }

    if (toggle && sidebar) {
        toggle.addEventListener("click", function () {
            sidebar.classList.toggle("is-open");
            if (backdrop) backdrop.classList.toggle("is-open");
        });
    }
    if (backdrop) backdrop.addEventListener("click", closeSidebar);

    document.querySelectorAll(".fn-sidebar__link").forEach(function (link) {
        link.addEventListener("click", function () {
            if (window.innerWidth < 768) closeSidebar();
        });
    });
})();

// ---------------------------------------------------------------------------
// Restricción de entrada por tipo de campo (complementa la validación del
// servidor). Impide escribir caracteres no permitidos y limpia lo pegado.
//   - input[type=number]      -> solo números y separador decimal
//   - [data-fn="digits"]      -> solo números, guion y espacio (cédulas/teléfonos)
//   - [data-fn="nombre"]      -> solo letras, espacios y . ' -
// ---------------------------------------------------------------------------
(function () {
    "use strict";

    var CONTROL = ["Backspace", "Tab", "Enter", "Escape", "Delete", "ArrowLeft",
        "ArrowRight", "ArrowUp", "ArrowDown", "Home", "End"];

    var patrones = {
        numero: /[0-9]/,
        digitos: /[0-9\- ]/,
        nombre: /[A-Za-zÁÉÍÓÚÜÑáéíóúüñ .'\-]/
    };

    function esControl(e) {
        return e.ctrlKey || e.metaKey || e.altKey || CONTROL.indexOf(e.key) >= 0;
    }

    function proteger(input, patron, permiteDecimal) {
        input.addEventListener("keydown", function (e) {
            if (esControl(e) || e.key.length !== 1) return;
            var ok = patron.test(e.key);
            if (!ok && permiteDecimal && (e.key === "." || e.key === ",")) {
                ok = input.value.indexOf(".") < 0 && input.value.indexOf(",") < 0;
            }
            if (!ok && permiteDecimal && e.key === "-") {
                ok = input.selectionStart === 0 && input.value.indexOf("-") < 0;
            }
            if (!ok) e.preventDefault();
        });

        input.addEventListener("paste", function (e) {
            var texto = (e.clipboardData || window.clipboardData).getData("text") || "";
            var limpio = "";
            var vistoSep = false;
            for (var i = 0; i < texto.length; i++) {
                var c = texto[i];
                if (patron.test(c)) limpio += c;
                else if (permiteDecimal && (c === "." || c === ",") && !vistoSep) { limpio += "."; vistoSep = true; }
            }
            if (limpio !== texto) {
                e.preventDefault();
                try {
                    var s = input.selectionStart, en = input.selectionEnd;
                    input.value = input.value.slice(0, s) + limpio + input.value.slice(en);
                    input.setSelectionRange(s + limpio.length, s + limpio.length);
                } catch (err) {
                    input.value = limpio;
                }
                input.dispatchEvent(new Event("input", { bubbles: true }));
            }
        });
    }

    document.querySelectorAll('input[type="number"], [data-fn="decimal"]').forEach(function (i) {
        proteger(i, patrones.numero, true);
        if (!i.getAttribute("inputmode")) i.setAttribute("inputmode", "decimal");
    });
    document.querySelectorAll('[data-fn="digits"]').forEach(function (i) {
        proteger(i, patrones.digitos, false);
        if (!i.getAttribute("inputmode")) i.setAttribute("inputmode", "numeric");
    });
    document.querySelectorAll('[data-fn="nombre"]').forEach(function (i) {
        proteger(i, patrones.nombre, false);
    });
})();
