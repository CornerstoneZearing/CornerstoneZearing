/* ---------------- */
/* Theme JavaScript */
/* ---------------- */

const header = document.querySelector("header");

const onScroll = () => {
    header.classList.toggle("scrolled", window.scrollY > 0);
};

window.addEventListener("scroll", onScroll, { passive: true });
onScroll();