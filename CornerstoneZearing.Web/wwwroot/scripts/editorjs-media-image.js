/**
 * Editor.js tool that produces an <img> block whose image is chosen from the
 * admin media library (or uploaded on the spot), instead of the stock
 * @editorjs/image tool's local-file-only picker.
 */
(function () {
	'use strict';

	const STYLE_ID = 'cdx-media-image-styles';

	function injectStyles() {
		if (document.getElementById(STYLE_ID)) return;
		const style = document.createElement('style');
		style.id = STYLE_ID;
		style.textContent = `
			.cdx-media-image { border: 1px solid var(--border, #dee2e6); border-radius: .375rem; padding: .75rem; }
			.cdx-media-image__preview { border: 1px dashed var(--border, #ced4da); border-radius: .25rem; overflow: hidden; min-height: 3rem; display: flex; align-items: center; justify-content: center; }
			.cdx-media-image__preview img { max-width: 100%; max-height: 320px; display: block; }
			.cdx-media-image__placeholder { color: #6c757d; font-size: .85rem; padding: 1.5rem 1rem; text-align: center; }
			.cdx-media-image__actions { display: flex; gap: .4rem; margin-top: .4rem; }
			.cdx-media-image__btn { border: 1px solid #0d6efd; background: #fff; color: #0d6efd; border-radius: .25rem; font-size: .78rem; padding: .25rem .6rem; cursor: pointer; }
			.cdx-media-image__btn:hover { background: #e7f1ff; }
			.cdx-media-image__btn--danger { border-color: #dc3545; color: #dc3545; }
			.cdx-media-image__btn--danger:hover { background: #fbe7e9; }
			.cdx-media-image__caption { margin-top: .5rem; outline: none; text-align: center; font-size: .9rem; color: #495057; }
			.cdx-media-image__caption:empty:before { content: attr(data-placeholder); color: #adb5bd; }
			.cdx-media-image-modal-toolbar { display: flex; gap: .5rem; align-items: center; margin-bottom: .75rem; }
			.cdx-media-image-modal-toolbar .cdx-media-image-search { flex: 1 1 auto; }
			.cdx-media-image-modal-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(110px, 1fr)); gap: .5rem; }
			.cdx-media-image-modal-item { border: 1px solid #dee2e6; border-radius: .25rem; overflow: hidden; cursor: pointer; aspect-ratio: 1 / 1; }
			.cdx-media-image-modal-item:hover { border-color: #0d6efd; }
			.cdx-media-image-modal-item img { width: 100%; height: 100%; object-fit: cover; display: block; }
			.cdx-media-image-modal-loading { color: #6c757d; font-size: .85rem; }
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
						<div class="cdx-media-image-modal-toolbar">
							<input type="text" class="form-control form-control-sm cdx-media-image-search" placeholder="Search media…">
							<label class="btn btn-primary btn-sm mb-0">
								<i class="fas fa-upload"></i> Upload New
								<input type="file" accept=".jpg,.jpeg,.png,.gif,.svg" hidden>
							</label>
						</div>
						<div class="cdx-media-image-modal-grid"></div>
					</div>
				</div>
			</div>`;
		document.body.appendChild(root);

		const grid = root.querySelector('.cdx-media-image-modal-grid');
		const search = root.querySelector('.cdx-media-image-search');
		const fileInput = root.querySelector('input[type=file]');
		const bsModal = new bootstrap.Modal(root);

		async function loadImages(term) {
			grid.innerHTML = '<p class="cdx-media-image-modal-loading">Loading…</p>';
			try {
				const url = config.mediaListUrl + (term ? `?search=${encodeURIComponent(term)}` : '');
				const res = await fetch(url, { headers: { Accept: 'application/json' } });
				const items = await res.json();
				renderGrid(items);
			} catch {
				grid.innerHTML = '<p class="cdx-media-image-modal-loading">Failed to load media.</p>';
			}
		}

		function renderGrid(items) {
			if (!items.length) {
				grid.innerHTML = '<p class="cdx-media-image-modal-loading">No images found.</p>';
				return;
			}
			grid.innerHTML = '';
			items.forEach(item => {
				const el = document.createElement('div');
				el.className = 'cdx-media-image-modal-item';
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

	class MediaImageTool {
		static get toolbox() {
			return {
				title: 'Image',
				icon: '<svg width="17" height="17" viewBox="0 0 17 17" fill="none" xmlns="http://www.w3.org/2000/svg"><rect x="1.5" y="1.5" width="14" height="14" rx="1.5" stroke="currentColor" stroke-width="1.3"/><circle cx="5.5" cy="6" r="1.3" stroke="currentColor" stroke-width="1.1"/><path d="M2 12.5L6 8.5L9 11.5L11.5 9L15 12.5" stroke="currentColor" stroke-width="1.1"/></svg>'
			};
		}

		static get isReadOnlySupported() {
			return true;
		}

		static get sanitize() {
			return {
				url: false,
				alt: false,
				caption: false
			};
		}

		constructor({ data, config, readOnly }) {
			this.readOnly = readOnly;
			this.config = config || {};
			this.data = {
				url: (data && data.url) || '',
				alt: (data && data.alt) || '',
				caption: (data && data.caption) || ''
			};
		}

		render() {
			injectStyles();

			const wrapper = document.createElement('div');
			wrapper.classList.add('cdx-media-image');

			wrapper.appendChild(this._buildPreview());
			wrapper.appendChild(this._buildCaption());

			return wrapper;
		}

		save() {
			return { ...this.data };
		}

		validate(savedData) {
			return !!savedData.url;
		}

		_buildPreview() {
			const wrap = document.createElement('div');

			const preview = document.createElement('div');
			preview.className = 'cdx-media-image__preview';
			wrap.appendChild(preview);
			this._previewEl = preview;

			if (!this.readOnly) {
				const actions = document.createElement('div');
				actions.className = 'cdx-media-image__actions';

				const chooseBtn = document.createElement('button');
				chooseBtn.type = 'button';
				chooseBtn.className = 'cdx-media-image__btn';
				chooseBtn.textContent = 'Choose Image';
				chooseBtn.addEventListener('click', () => this._openImagePicker());

				const removeBtn = document.createElement('button');
				removeBtn.type = 'button';
				removeBtn.className = 'cdx-media-image__btn cdx-media-image__btn--danger';
				removeBtn.textContent = 'Remove';
				removeBtn.addEventListener('click', () => {
					this.data.url = '';
					this.data.alt = '';
					this._refreshPreview();
				});

				this._removeBtn = removeBtn;
				actions.append(chooseBtn, removeBtn);
				wrap.appendChild(actions);
			}

			this._refreshPreview();
			return wrap;
		}

		_buildCaption() {
			const caption = document.createElement('div');
			caption.className = 'cdx-media-image__caption';
			caption.contentEditable = this.readOnly ? 'false' : 'true';
			caption.dataset.placeholder = 'Caption (optional)';
			caption.textContent = this.data.caption || '';
			caption.addEventListener('input', () => this.data.caption = caption.textContent.trim());
			return caption;
		}

		_openImagePicker() {
			const modal = ensureModal(this.config);
			modal.onSelect = ({ url, alt }) => {
				this.data.url = url;
				this.data.alt = alt;
				this._refreshPreview();
			};
			modal.bsModal.show();
		}

		_refreshPreview() {
			if (!this._previewEl) return;

			this._previewEl.innerHTML = '';
			if (this.data.url) {
				const img = document.createElement('img');
				img.src = this.data.url;
				img.alt = this.data.alt || '';
				this._previewEl.appendChild(img);
			} else {
				const placeholder = document.createElement('div');
				placeholder.className = 'cdx-media-image__placeholder';
				placeholder.innerHTML = '<i class="fas fa-image"></i> No image selected';
				this._previewEl.appendChild(placeholder);
			}

			if (this._removeBtn) {
				this._removeBtn.style.display = this.data.url ? '' : 'none';
			}
		}
	}

	window.MediaImageTool = MediaImageTool;
})();
