/* ---------------- */
/* Admin JavaScript */
/* ---------------- */

(function () {
	'use strict';

	var STORAGE_KEY = 'admin-theme';
	var media = window.matchMedia('(prefers-color-scheme: dark)');

	function getStored() {
		return localStorage.getItem(STORAGE_KEY) || 'auto';
	}

	function effectiveTheme(stored) {
		return stored === 'auto' ? (media.matches ? 'dark' : 'light') : stored;
	}

	function applyTheme(stored) {
		document.documentElement.setAttribute('data-bs-theme', effectiveTheme(stored));
		updateToggleUI(stored);
	}

	function updateToggleUI(stored) {
		var icon = document.getElementById('themeToggleIcon');
		if (icon) {
			icon.className = stored === 'light' ? 'fas fa-sun'
				: stored === 'dark' ? 'fas fa-moon'
				: 'fas fa-circle-half-stroke';
		}
		document.querySelectorAll('.theme-option').forEach(function (btn) {
			btn.classList.toggle('active', btn.dataset.themeValue === stored);
		});
	}

	document.querySelectorAll('.theme-option').forEach(function (btn) {
		btn.addEventListener('click', function () {
			var value = btn.dataset.themeValue;
			localStorage.setItem(STORAGE_KEY, value);
			applyTheme(value);
		});
	});

	media.addEventListener('change', function () {
		if (getStored() === 'auto') {
			applyTheme('auto');
		}
	});

	applyTheme(getStored());
})();
