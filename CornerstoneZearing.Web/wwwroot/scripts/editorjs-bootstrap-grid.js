/**
 * Editor.js tool that produces a Bootstrap 5 grid (row of columns, each with
 * independent sm/md/lg widths) containing simple rich content — paragraphs,
 * headings, links, and buttons.
 */
(function () {
	'use strict';

	const STYLE_ID = 'cdx-bs-grid-styles';
	const BREAKPOINTS = ['sm', 'md', 'lg'];
	const BUTTON_STYLES = ['primary', 'secondary', 'success', 'danger', 'warning', 'info', 'light', 'dark', 'outline-primary', 'outline-secondary'];

	function injectStyles() {
		if (document.getElementById(STYLE_ID)) return;
		const style = document.createElement('style');
		style.id = STYLE_ID;
		style.textContent = `
			.cdx-bs-grid { border: 1px solid var(--border, #dee2e6); border-radius: .375rem; padding: .75rem; }
			.cdx-bs-grid__columns { display: flex; flex-wrap: wrap; gap: .6rem; }
			.cdx-bs-grid__col { flex: 1 1 220px; min-width: 200px; border: 1px dashed var(--border, #ced4da); border-radius: .25rem; padding: .5rem; }
			.cdx-bs-grid__col-header { display: flex; align-items: center; gap: .3rem; flex-wrap: wrap; margin-bottom: .4rem; }
			.cdx-bs-grid__col-label { font-size: .75rem; font-weight: 600; color: #6c757d; margin-right: auto; }
			.cdx-bs-grid__width { display: flex; align-items: center; gap: .15rem; }
			.cdx-bs-grid__width label { font-size: .68rem; color: #6c757d; }
			.cdx-bs-grid__select { border: 1px solid #ced4da; border-radius: .2rem; font-size: .75rem; padding: .1rem .25rem; }
			.cdx-bs-grid__icon-btn { border: 1px solid #ced4da; background: #fff; color: #495057; border-radius: .2rem; font-size: .72rem; padding: .1rem .35rem; cursor: pointer; line-height: 1.3; }
			.cdx-bs-grid__icon-btn:hover { background: #e9ecef; }
			.cdx-bs-grid__icon-btn--danger { border-color: #dc3545; color: #dc3545; }
			.cdx-bs-grid__icon-btn--danger:hover { background: #fbe7e9; }
			.cdx-bs-grid__blocks { display: flex; flex-direction: column; gap: .35rem; margin-bottom: .5rem; }
			.cdx-bs-grid__block { position: relative; border: 1px solid #eee; border-radius: .2rem; padding: .35rem .5rem .35rem .5rem; }
			.cdx-bs-grid__block-remove { position: absolute; top: .2rem; right: .2rem; border: none; background: none; color: #adb5bd; cursor: pointer; font-size: .8rem; line-height: 1; padding: .1rem; }
			.cdx-bs-grid__block-remove:hover { color: #dc3545; }
			.cdx-bs-grid__editable { outline: none; min-height: 1.3em; padding-right: 1.2rem; }
			.cdx-bs-grid__editable[data-role="heading"] { font-weight: 600; font-size: 1.05rem; }
			[contenteditable].cdx-bs-grid__editable:empty:before { content: attr(data-placeholder); color: #adb5bd; }
			.cdx-bs-grid__row2 { display: flex; gap: .4rem; margin-top: .25rem; flex-wrap: wrap; }
			.cdx-bs-grid__input { flex: 1 1 45%; min-width: 8rem; border: 1px solid #ced4da; border-radius: .2rem; padding: .2rem .4rem; font-size: .8rem; }
			.cdx-bs-grid__add-row { display: flex; gap: .3rem; flex-wrap: wrap; }
			.cdx-bs-grid__add-btn { border: 1px solid #0d6efd; background: #fff; color: #0d6efd; border-radius: .2rem; font-size: .72rem; padding: .2rem .5rem; cursor: pointer; }
			.cdx-bs-grid__add-btn:hover { background: #e7f1ff; }
			.cdx-bs-grid__footer { margin-top: .6rem; }
			.cdx-bs-grid__add-col-btn { border: 1px solid #198754; background: #fff; color: #198754; border-radius: .25rem; font-size: .8rem; padding: .3rem .7rem; cursor: pointer; }
			.cdx-bs-grid__add-col-btn:hover { background: #e9f7ef; }
		`;
		document.head.appendChild(style);
	}

	function makeWidthSelect(bp, value, onChange, disabled) {
		const wrap = document.createElement('div');
		wrap.className = 'cdx-bs-grid__width';

		const label = document.createElement('label');
		label.textContent = bp;
		wrap.appendChild(label);

		const select = document.createElement('select');
		select.className = 'cdx-bs-grid__select';
		select.disabled = !!disabled;

		const noneOpt = document.createElement('option');
		noneOpt.value = '';
		noneOpt.textContent = 'auto';
		select.appendChild(noneOpt);

		for (let i = 1; i <= 12; i++) {
			const opt = document.createElement('option');
			opt.value = String(i);
			opt.textContent = String(i);
			select.appendChild(opt);
		}

		select.value = value || '';
		select.addEventListener('change', () => onChange(select.value));
		wrap.appendChild(select);
		return wrap;
	}

	class BootstrapGridTool {
		static get toolbox() {
			return {
				title: 'Grid',
				icon: '<svg width="17" height="17" viewBox="0 0 17 17" fill="none" xmlns="http://www.w3.org/2000/svg"><rect x="1.5" y="1.5" width="14" height="14" rx="1.5" stroke="currentColor" stroke-width="1.3"/><path d="M6.3 1.5V15.5" stroke="currentColor" stroke-width="1.3"/><path d="M11 1.5V15.5" stroke="currentColor" stroke-width="1.3"/></svg>'
			};
		}

		static get isReadOnlySupported() {
			return true;
		}

		static get sanitize() {
			return {
				columns: false
			};
		}

		constructor({ data, readOnly }) {
			this.readOnly = readOnly;
			this.data = {
				columns: (data && Array.isArray(data.columns) && data.columns.length)
					? data.columns.map(c => this._normalizeColumn(c))
					: [this._newColumn()]
			};
		}

		_newColumn() {
			return { sm: '', md: '', lg: '', blocks: [] };
		}

		_normalizeColumn(c) {
			return {
				sm: (c && c.sm) || '',
				md: (c && c.md) || '',
				lg: (c && c.lg) || '',
				blocks: (c && Array.isArray(c.blocks)) ? c.blocks.map(b => ({ ...b })) : []
			};
		}

		render() {
			injectStyles();

			this.wrapper = document.createElement('div');
			this.wrapper.classList.add('cdx-bs-grid');

			this.columnsEl = document.createElement('div');
			this.columnsEl.className = 'cdx-bs-grid__columns';
			this.wrapper.appendChild(this.columnsEl);

			if (!this.readOnly) {
				const footer = document.createElement('div');
				footer.className = 'cdx-bs-grid__footer';
				const addColBtn = document.createElement('button');
				addColBtn.type = 'button';
				addColBtn.className = 'cdx-bs-grid__add-col-btn';
				addColBtn.textContent = '+ Add Column';
				addColBtn.addEventListener('click', () => {
					this.data.columns.push(this._newColumn());
					this._renderColumns();
				});
				footer.appendChild(addColBtn);
				this.wrapper.appendChild(footer);
			}

			this._renderColumns();

			return this.wrapper;
		}

		save() {
			return { columns: this.data.columns.map(c => this._normalizeColumn(c)) };
		}

		validate(savedData) {
			return !!(savedData.columns && savedData.columns.length);
		}

		_renderColumns() {
			this.columnsEl.innerHTML = '';
			this.data.columns.forEach((col, index) => {
				this.columnsEl.appendChild(this._buildColumn(col, index));
			});
		}

		_buildColumn(col, colIndex) {
			const el = document.createElement('div');
			el.className = 'cdx-bs-grid__col';

			const header = document.createElement('div');
			header.className = 'cdx-bs-grid__col-header';

			const label = document.createElement('span');
			label.className = 'cdx-bs-grid__col-label';
			label.textContent = `Column ${colIndex + 1}`;
			header.appendChild(label);

			if (!this.readOnly) {
				BREAKPOINTS.forEach(bp => {
					header.appendChild(makeWidthSelect(bp, col[bp], value => { col[bp] = value; }));
				});

				if (this.data.columns.length > 1) {
					if (colIndex > 0) {
						const leftBtn = document.createElement('button');
						leftBtn.type = 'button';
						leftBtn.className = 'cdx-bs-grid__icon-btn';
						leftBtn.title = 'Move left';
						leftBtn.textContent = '←';
						leftBtn.addEventListener('click', () => {
							[this.data.columns[colIndex - 1], this.data.columns[colIndex]] =
								[this.data.columns[colIndex], this.data.columns[colIndex - 1]];
							this._renderColumns();
						});
						header.appendChild(leftBtn);
					}
					if (colIndex < this.data.columns.length - 1) {
						const rightBtn = document.createElement('button');
						rightBtn.type = 'button';
						rightBtn.className = 'cdx-bs-grid__icon-btn';
						rightBtn.title = 'Move right';
						rightBtn.textContent = '→';
						rightBtn.addEventListener('click', () => {
							[this.data.columns[colIndex], this.data.columns[colIndex + 1]] =
								[this.data.columns[colIndex + 1], this.data.columns[colIndex]];
							this._renderColumns();
						});
						header.appendChild(rightBtn);
					}
				}

				const removeBtn = document.createElement('button');
				removeBtn.type = 'button';
				removeBtn.className = 'cdx-bs-grid__icon-btn cdx-bs-grid__icon-btn--danger';
				removeBtn.title = 'Remove column';
				removeBtn.textContent = '✕';
				removeBtn.addEventListener('click', () => {
					if (this.data.columns.length <= 1) return;
					this.data.columns.splice(colIndex, 1);
					this._renderColumns();
				});
				header.appendChild(removeBtn);
			}

			el.appendChild(header);

			const blocksEl = document.createElement('div');
			blocksEl.className = 'cdx-bs-grid__blocks';
			col.blocks.forEach((block, blockIndex) => {
				blocksEl.appendChild(this._buildBlock(col, block, blockIndex));
			});
			el.appendChild(blocksEl);

			if (!this.readOnly) {
				const addRow = document.createElement('div');
				addRow.className = 'cdx-bs-grid__add-row';

				const addBlockBtn = (label, factory) => {
					const btn = document.createElement('button');
					btn.type = 'button';
					btn.className = 'cdx-bs-grid__add-btn';
					btn.textContent = '+ ' + label;
					btn.addEventListener('click', () => {
						col.blocks.push(factory());
						this._renderColumns();
					});
					addRow.appendChild(btn);
				};

				addBlockBtn('Paragraph', () => ({ type: 'paragraph', text: '' }));
				addBlockBtn('Heading', () => ({ type: 'heading', level: 2, text: '' }));
				addBlockBtn('Link', () => ({ type: 'link', text: '', url: '' }));
				addBlockBtn('Button', () => ({ type: 'button', text: '', url: '', style: 'primary' }));

				el.appendChild(addRow);
			}

			return el;
		}

		_buildBlock(col, block, blockIndex) {
			const wrap = document.createElement('div');
			wrap.className = 'cdx-bs-grid__block';

			if (!this.readOnly) {
				const removeBtn = document.createElement('button');
				removeBtn.type = 'button';
				removeBtn.className = 'cdx-bs-grid__block-remove';
				removeBtn.title = 'Remove';
				removeBtn.textContent = '✕';
				removeBtn.addEventListener('click', () => {
					col.blocks.splice(blockIndex, 1);
					this._renderColumns();
				});
				wrap.appendChild(removeBtn);
			}

			if (block.type === 'paragraph') {
				wrap.appendChild(this._buildEditable('paragraph', 'Paragraph text…', block.text, v => block.text = v));
			} else if (block.type === 'heading') {
				const editable = this._buildEditable('heading', 'Heading text…', block.text, v => block.text = v);
				wrap.appendChild(editable);

				if (!this.readOnly) {
					const levelRow = document.createElement('div');
					levelRow.className = 'cdx-bs-grid__row2';
					const levelSelect = document.createElement('select');
					levelSelect.className = 'cdx-bs-grid__select';
					[2, 3, 4, 5].forEach(l => {
						const opt = document.createElement('option');
						opt.value = String(l);
						opt.textContent = `H${l}`;
						levelSelect.appendChild(opt);
					});
					levelSelect.value = String(block.level || 2);
					levelSelect.addEventListener('change', () => block.level = parseInt(levelSelect.value, 10));
					levelRow.appendChild(levelSelect);
					wrap.appendChild(levelRow);
				}
			} else if (block.type === 'link' || block.type === 'button') {
				const editable = this._buildEditable(block.type, block.type === 'button' ? 'Button text…' : 'Link text…', block.text, v => block.text = v);
				wrap.appendChild(editable);

				const row = document.createElement('div');
				row.className = 'cdx-bs-grid__row2';

				const urlInput = document.createElement('input');
				urlInput.type = 'url';
				urlInput.className = 'cdx-bs-grid__input';
				urlInput.placeholder = 'URL';
				urlInput.value = block.url || '';
				urlInput.disabled = !!this.readOnly;
				urlInput.addEventListener('input', () => block.url = urlInput.value.trim());
				row.appendChild(urlInput);

				if (block.type === 'button' && !this.readOnly) {
					const styleSelect = document.createElement('select');
					styleSelect.className = 'cdx-bs-grid__select';
					BUTTON_STYLES.forEach(s => {
						const opt = document.createElement('option');
						opt.value = s;
						opt.textContent = s;
						styleSelect.appendChild(opt);
					});
					styleSelect.value = block.style || 'primary';
					styleSelect.addEventListener('change', () => block.style = styleSelect.value);
					row.appendChild(styleSelect);
				}

				wrap.appendChild(row);
			}

			return wrap;
		}

		_buildEditable(role, placeholder, value, onInput) {
			const el = document.createElement('div');
			el.className = 'cdx-bs-grid__editable';
			el.dataset.role = role;
			el.contentEditable = this.readOnly ? 'false' : 'true';
			el.dataset.placeholder = placeholder;
			el.textContent = value || '';
			el.addEventListener('input', () => onInput(el.textContent.trim()));
			return el;
		}
	}

	window.BootstrapGridTool = BootstrapGridTool;
})();
