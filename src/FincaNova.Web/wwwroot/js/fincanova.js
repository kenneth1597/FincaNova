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
