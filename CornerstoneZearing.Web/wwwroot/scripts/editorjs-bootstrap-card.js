/**
 * Editor.js tool that produces a Bootstrap 5 card block (image, title, text, link)
 * with image selection backed by the admin media library.
 */
(function () {
	'use strict';

	const STYLE_ID = 'cdx-bs-card-styles';

	function injectStyles() {
		if (document.getElementById(STYLE_ID)) return;
		const style = document.createElement('style');
		style.id = STYLE_ID;
		style.textContent = `
			.cdx-bs-card { border: 1px solid var(--border, #dee2e6); border-radius: .375rem; padding: .75rem; }
			.cdx-bs-card__image-wrap { margin-bottom: .5rem; }
			.cdx-bs-card__image-preview { border: 1px dashed var(--border, #ced4da); border-radius: .25rem; overflow: hidden; min-height: 3rem; display: flex; align-items: center; justify-content: center; }
			.cdx-bs-card__image-preview img { max-width: 100%; max-height: 220px; display: block; }
			.cdx-bs-card__image-placeholder { color: #6c757d; font-size: .85rem; padding: 1.5rem 1rem; text-align: center; }
			.cdx-bs-card__image-actions { display: flex; gap: .4rem; margin-top: .4rem; }
			.cdx-bs-card__btn { border: 1px solid #0d6efd; background: #fff; color: #0d6efd; border-radius: .25rem; font-size: .78rem; padding: .25rem .6rem; cursor: pointer; }
			.cdx-bs-card__btn:hover { background: #e7f1ff; }
			.cdx-bs-card__btn--danger { border-color: #dc3545; color: #dc3545; }
			.cdx-bs-card__btn--danger:hover { background: #fbe7e9; }
			.cdx-bs-card__title { font-size: 1.15rem; font-weight: 600; margin-bottom: .35rem; outline: none; }
			.cdx-bs-card__text { margin-bottom: .5rem; outline: none; min-height: 1.5em; }
			[contenteditable]:empty:before { content: attr(data-placeholder); color: #adb5bd; }
			.cdx-bs-card__link-row { display: flex; gap: .5rem; flex-wrap: wrap; }
			.cdx-bs-card__input { flex: 1 1 45%; min-width: 10rem; border: 1px solid #ced4da; border-radius: .25rem; padding: .3rem .5rem; font-size: .85rem; }
			.cdx-bs-card-modal-toolbar { display: flex; gap: .5rem; align-items: center; margin-bottom: .75rem; }
			.cdx-bs-card-modal-toolbar .cdx-bs-card-search { flex: 1 1 auto; }
			.cdx-bs-card-modal-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(110px, 1fr)); gap: .5rem; }
			.cdx-bs-card-modal-item { border: 1px solid #dee2e6; border-radius: .25rem; overflow: hidden; cursor: pointer; aspect-ratio: 1 / 1; }
			.cdx-bs-card-modal-item:hover { border-color: #0d6efd; }
			.cdx-bs-card-modal-item img { width: 100%; height: 100%; object-fit: cover; display: block; }
			.cdx-bs-card-modal-loading { color: #6c757d; font-size: .85rem; }
		`;
		document.head.appendChild(style);
	}

	let sharedModal = null;

	function ensureModal(config) {
		if (sharedModal) return sharedModal;

		const root = document.createElement('div');
		root.className = 'modal fade';
		root.tabIndex = -1;
		root.innerHTML = `
			<div class="modal-dialog modal-lg modal-dialog-scrollable">
				<div class="modal-content">
					<div class="modal-header">
						<h5 class="modal-title">Choose an Image</h5>
						<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
					</div>
					<div class="modal-body">
						<div class="cdx-bs-card-modal-toolbar">
							<input type="text" class="form-control form-control-sm cdx-bs-card-search" placeholder="Search media…">
							<label class="btn btn-primary btn-sm mb-0">
								<i class="fas fa-upload"></i> Upload New
								<input type="file" accept=".jpg,.jpeg,.png,.gif,.svg" hidden>
							</label>
						</div>
						<div class="cdx-bs-card-modal-grid"></div>
					</div>
				</div>
			</div>`;
		document.body.appendChild(root);

		const grid = root.querySelector('.cdx-bs-card-modal-grid');
		const search = root.querySelector('.cdx-bs-card-search');
		const fileInput = root.querySelector('input[type=file]');
		const bsModal = new bootstrap.Modal(root);

		async function loadImages(term) {
			grid.innerHTML = '<p class="cdx-bs-card-modal-loading">Loading…</p>';
			try {
				const url = config.mediaListUrl + (term ? `?search=${encodeURIComponent(term)}` : '');
				const res = await fetch(url, { headers: { Accept: 'application/json' } });
				const items = await res.json();
				renderGrid(items);
			} catch {
				grid.innerHTML = '<p class="cdx-bs-card-modal-loading">Failed to load media.</p>';
			}
		}

		function renderGrid(items) {
			if (!items.length) {
				grid.innerHTML = '<p class="cdx-bs-card-modal-loading">No images found.</p>';
				return;
			}
			grid.innerHTML = '';
			items.forEach(item => {
				const el = document.createElement('div');
				el.className = 'cdx-bs-card-modal-item';
				el.title = item.name;
				const img = document.createElement('img');
				img.src = item.url;
				img.alt = item.alt || '';
				img.loading = 'lazy';
				el.appendChild(img);
				el.addEventListener('click', () => {
					if (sharedModal.onSelect) sharedModal.onSelect({ url: item.url, alt: item.alt || '' });
					bsModal.hide();
				});
				grid.appendChild(el);
			});
		}

		let searchTimer;
		search.addEventListener('input', () => {
			clearTimeout(searchTimer);
			searchTimer = setTimeout(() => loadImages(search.value.trim()), 300);
		});

		fileInput.addEventListener('change', async () => {
			const file = fileInput.files[0];
			if (!file) return;

			const formData = new FormData();
			formData.append('file', file);
			formData.append('__RequestVerificationToken', config.antiForgeryToken);

			try {
				const res = await fetch(config.uploadUrl, {
					method: 'POST',
					headers: { Accept: 'application/json' },
					body: formData
				});
				const data = await res.json();
				if (data.success) {
					if (sharedModal.onSelect) sharedModal.onSelect({ url: data.url, alt: '' });
					bsModal.hide();
				} else {
					alert(data.error || 'Upload failed.');
				}
			} catch {
				alert('Upload failed.');
			} finally {
				fileInput.value = '';
			}
		});

		root.addEventListener('show.bs.modal', () => loadImages(search.value.trim()));

		sharedModal = { bsModal, onSelect: null };
		return sharedModal;
	}

	class BootstrapCardTool {
		static get toolbox() {
			return {
				title: 'Card',
				icon: '<svg width="17" height="17" viewBox="0 0 17 17" fill="none" xmlns="http://www.w3.org/2000/svg"><rect x="1.5" y="1.5" width="14" height="14" rx="1.5" stroke="currentColor" stroke-width="1.3"/><path d="M1.5 6.5H15.5" stroke="currentColor" stroke-width="1.3"/><path d="M4 9.5H13" stroke="currentColor" stroke-width="1.1"/><path d="M4 12H9.5" stroke="currentColor" stroke-width="1.1"/></svg>'
			};
		}

		static get isReadOnlySupported() {
			return true;
		}

		static get sanitize() {
			return {
				imageUrl: false,
				imageAlt: false,
				title: false,
				text: false,
				linkUrl: false,
				linkText: false
			};
		}

		constructor({ data, config, readOnly }) {
			this.readOnly = readOnly;
			this.config = config || {};
			this.data = {
				imageUrl: (data && data.imageUrl) || '',
				imageAlt: (data && data.imageAlt) || '',
				title: (data && data.title) || '',
				text: (data && data.text) || '',
				linkUrl: (data && data.linkUrl) || '',
				linkText: (data && data.linkText) || ''
			};
		}

		render() {
			injectStyles();

			const wrapper = document.createElement('div');
			wrapper.classList.add('cdx-bs-card');

			wrapper.appendChild(this._buildImagePicker());
			wrapper.appendChild(this._buildEditable('cdx-bs-card__title', 'Card title', this.data.title, v => this.data.title = v));
			wrapper.appendChild(this._buildEditable('cdx-bs-card__text', 'Card text', this.data.text, v => this.data.text = v));
			wrapper.appendChild(this._buildLinkRow());

			return wrapper;
		}

		save() {
			return { ...this.data };
		}

		validate(savedData) {
			return !!(savedData.title || savedData.text || savedData.imageUrl);
		}

		_buildImagePicker() {
			const wrap = document.createElement('div');
			wrap.className = 'cdx-bs-card__image-wrap';

			const preview = document.createElement('div');
			preview.className = 'cdx-bs-card__image-preview';
			wrap.appendChild(preview);
			this._imagePreviewEl = preview;

			if (!this.readOnly) {
				const actions = document.createElement('div');
				actions.className = 'cdx-bs-card__image-actions';

				const chooseBtn = document.createElement('button');
				chooseBtn.type = 'button';
				chooseBtn.className = 'cdx-bs-card__btn';
				chooseBtn.textContent = 'Choose Image';
				chooseBtn.addEventListener('click', () => this._openImagePicker());

				const removeBtn = document.createElement('button');
				removeBtn.type = 'button';
				removeBtn.className = 'cdx-bs-card__btn cdx-bs-card__btn--danger';
				removeBtn.textContent = 'Remove';
				removeBtn.addEventListener('click', () => {
					this.data.imageUrl = '';
					this.data.imageAlt = '';
					this._refreshImagePreview();
				});

				this._removeBtn = removeBtn;
				actions.append(chooseBtn, removeBtn);
				wrap.appendChild(actions);
			}

			this._refreshImagePreview();
			return wrap;
		}

		_openImagePicker() {
			const modal = ensureModal(this.config);
			modal.onSelect = ({ url, alt }) => {
				this.data.imageUrl = url;
				this.data.imageAlt = alt;
				this._refreshImagePreview();
			};
			modal.bsModal.show();
		}

		_refreshImagePreview() {
			if (!this._imagePreviewEl) return;

			this._imagePreviewEl.innerHTML = '';
			if (this.data.imageUrl) {
				const img = document.createElement('img');
				img.src = this.data.imageUrl;
				img.alt = this.data.imageAlt || '';
				this._imagePreviewEl.appendChild(img);
			} else {
				const placeholder = document.createElement('div');
				placeholder.className = 'cdx-bs-card__image-placeholder';
				placeholder.innerHTML = '<i class="fas fa-image"></i> No image selected';
				this._imagePreviewEl.appendChild(placeholder);
			}

			if (this._removeBtn) {
				this._removeBtn.style.display = this.data.imageUrl ? '' : 'none';
			}
		}

		_buildEditable(className, placeholder, value, onInput) {
			const el = document.createElement('div');
			el.className = className;
			el.contentEditable = this.readOnly ? 'false' : 'true';
			el.dataset.placeholder = placeholder;
			el.textContent = value || '';
			el.addEventListener('input', () => onInput(el.textContent.trim()));
			return el;
		}

		_buildLinkRow() {
			const row = document.createElement('div');
			row.className = 'cdx-bs-card__link-row';

			const urlInput = document.createElement('input');
			urlInput.type = 'url';
			urlInput.className = 'cdx-bs-card__input';
			urlInput.placeholder = 'Link URL (optional)';
			urlInput.value = this.data.linkUrl || '';
			urlInput.disabled = !!this.readOnly;
			urlInput.addEventListener('input', () => this.data.linkUrl = urlInput.value.trim());

			const textInput = document.createElement('input');
			textInput.type = 'text';
			textInput.className = 'cdx-bs-card__input';
			textInput.placeholder = 'Button text (e.g. Learn More)';
			textInput.value = this.data.linkText || '';
			textInput.disabled = !!this.readOnly;
			textInput.addEventListener('input', () => this.data.linkText = textInput.value.trim());

			row.append(urlInput, textInput);
			return row;
		}
	}

	window.BootstrapCardTool = BootstrapCardTool;
})();
